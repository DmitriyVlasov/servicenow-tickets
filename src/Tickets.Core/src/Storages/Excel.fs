module Tickets.Storages.Excel
open Tickets.Data.Requests.Deprecate

open ClosedXML.Excel

let fillWorksheetByCol (wb:XLWorkbook) (sheetName:string) (tableByCols: Map< string,Column>) = 
  let ws = wb.Worksheets.Add(sheetName)
  
  let tableHeader = 
    tableByCols
    |> Map.keys
    |> Seq.mapi (fun i key -> key, i+1)
    |> Map.ofSeq
  
  let getColumnId columnName = 
    tableHeader
    |> Map.find columnName

  // insert header
  for column in tableHeader do
    let rowId = 1
    let colId = column.Value
    let columnName = column.Key
    ws.Cell(rowId,colId).Value <- columnName

  // insert rows
  for column in tableByCols do
    let colName = column.Key
    let columnValues = column.Value.Values
    let colId = getColumnId colName
    let mutable rowId = 1
    for fullValue in columnValues do
      rowId <- rowId + 1
      let maxLen = min fullValue.Length 32767
      let cropValue = fullValue.Substring(0,maxLen)
      ws.Cell(rowId,colId).Value <- cropValue  

let fillWorksheetByRow (wb:XLWorkbook) (sheetName:string) (tableByRows:seq<Map<string,string>>) = 
  let ws = wb.Worksheets.Add(sheetName)

  let tableHeader = 
    tableByRows
    |> Seq.head
    |> Map.keys
    |> Seq.mapi (fun i key -> key, i+1)
    |> Map.ofSeq

  let getColumnId columnName = 
    tableHeader
    |> Map.find columnName

  // insert header
  for col in tableHeader do
    ws.Cell(1,col.Value).Value <- col.Key

  // insert rows
  let mutable line = 1
  for row in tableByRows do
    line <- line + 1
    for cell in row do
      let col = getColumnId cell.Key
      let fullValue = cell.Value
      let maxLen = min fullValue.Length 32767
      let cropValue = fullValue.Substring(0, maxLen )
      ws.Cell(line, col).Value <- cropValue