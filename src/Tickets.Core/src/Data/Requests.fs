module Tickets.Data.Requests

open System
open Tickets
open Tickets.Model

let extractor doc = 
  Extractor.dataFromXml doc "sc_req_item"
  |> Seq.map ( fun map -> 
    let find x = Map.find x map
    {
      Id     = find "sys_id"
      Number = find "number" 
    }
  )

let findRequestNumber id requests = 
  requests
  |> Seq.find (fun item -> item.Id = id )
  |> fun item -> item.Number

module Deprecate = 

  type Column = {
    IsBig: bool; 
    IsBoolean: bool; 
    IsDummy: bool; 
    IsEmpty: bool; 
    IsIdentity: bool; 
    IsOne: bool; 
    MaxLength: int; 
    Values: string [] 
  }

  let private extractDataByRow doc = 
    Extractor.dataFromXml doc "sc_req_item"
    |> Seq.toArray

  let private transposeRowToColumn (rows:array<Map<string,string>>) = 
    // Extract the column names from the original row-by-row table
    let columnNames = 
      rows
      |> Array.head
      |> Map.keys
      |> Seq.toArray

    // Transpose rows into columns
    let baseTable = 
      columnNames
      |> Array.map ( fun columnName -> 
        let values = 
          rows
          |> Array.map (fun map ->
              let value = Map.tryFind columnName map
              match value with
              | Some x -> x
              | None -> ""
          )
        columnName,values
        )
      |> Map.ofArray

    // For the basic transposed table, let's calculate the statistics
    baseTable
    |> Map.map ( fun columnName columnValues -> 
      let uniques = columnValues |> Array.distinct
      let isEmpty = uniques |> Array.forall String.IsNullOrWhiteSpace
      let avgLength = uniques |> Array.averageBy ( fun text -> text.Length |> double ) |> round
      let maxLength = uniques |> Array.maxBy ( fun text -> text.Length ) |> String.length

      // -----------------------------------
      // Algorithm for calculating the threshold value (thresholdLenght)
      // -----------------------------------
      // Calculated the average length of the values in the column
      // Then sorted all columns in descending order
      // Then we saw that by a large margin there are columns with a value greater than 50
      // For another table, this value will be different. The solution is not universal.      // -----------------------------------
      // tableByCols
      // |> Map.map (fun _ item -> item.AvgLength)
      // |> Map.toSeq
      // |> Seq.sortByDescending snd
      // =================================
      let thresholdLenght = 50
      let isBig = avgLength > thresholdLenght
      let isOne = uniques.Length = 1 && not isEmpty
      let isTwo = uniques.Length = 2 && not isEmpty
      let isIdentity = uniques.Length = columnValues.Length
      let firstValue = uniques |> Array.tryHead
      let likeBoolean = 
        match firstValue with
        | Some value -> value = "true" || value = "false" 
        | None -> false
      let isBoolean = ( isOne || isTwo ) && likeBoolean
      let isDummy = isEmpty || ( isOne && isBoolean )
      let columnValues = 
        if columnName = "u_problem_description"
        then 
          columnValues
          |> Array.map ( fun html -> 
            if String.IsNullOrWhiteSpace html 
            then html 
            else Extractor.textFromHtml html
          )
        else columnValues
      { 
        Values = columnValues
        IsIdentity = isIdentity
        IsBig = isBig
        IsEmpty = isEmpty 
        IsOne = isOne
        IsBoolean = isBoolean
        IsDummy = isDummy
        MaxLength = maxLength
      }
  )

  let extractData doc = 
    extractDataByRow doc
    |> transposeRowToColumn

  let extractAllData docs = 
    docs 
    |> Seq.map extractDataByRow
    |> Seq.concat
    |> Array.ofSeq
    |> transposeRowToColumn