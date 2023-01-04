module Tickets.Stat

open Tickets
open Tickets.Data.Requests.Deprecate
open System.Xml

let stat (doc:XmlDocument) nodeName = 
  doc.SelectNodes($"//{nodeName}/*")
  |> Seq.cast<XmlNode>
  |> Seq.groupBy (fun x -> x.Name)
  |> Seq.map (fun (name,xmlNodes)-> 
    let columns = 
      xmlNodes 
      |> Seq.head 
      |> ( fun x -> x.ChildNodes )
      |> Seq.cast<XmlNode>
      |> Seq.map (fun x -> x.Name)
      |> Seq.sort
    let colLen = Seq.length columns
    let maxLen = 10
    let ellipsis = if colLen > maxLen then "..." else ""
    let colNames = 
      columns 
      |> Seq.take (min colLen maxLen) 
      |> String.concat ", " 
      |> fun text -> $"{text} {ellipsis}"
    {|
      Name = name
      Len = Seq.length xmlNodes
      ``Column Names`` = colNames 
      ``Columns Count`` = columns |> Seq.length
    |}
  )
  |> Seq.sortByDescending (fun x -> x.Len)

let reqestStat (tableByCols: Map< string,Column>) = 
  let counter xs p =
    xs
    |> Map.filter p
    |> Map.count

  let cols xs p = 
    xs
  |> Map.filter p
  |> Map.keys
  |> String.concat ", "

  let isEmpty = counter tableByCols ( fun _ item -> item.IsEmpty )
  let isOne = counter tableByCols ( fun _ item -> item.IsOne )
  let isBoolean = counter tableByCols ( fun _ item -> item.IsBoolean )
  let isDummy = counter tableByCols ( fun _ item -> item.IsDummy )
  let allColumn = counter tableByCols (fun _ _ -> true)
  let bigColumns = cols tableByCols (fun _ item -> item.IsBig)
  let identityColumns = cols tableByCols (fun _ item -> item.IsIdentity)

  [
    {| Description = "Columns without values"; Value = string <| isEmpty |}
    {| Description = "Columns with a single value"; Value = string <| isOne |}
    {| Description = "Boolean column"; Value = string <| isBoolean |}
    {| Description = "Columns can be deleted"; Value = string <| isDummy |}
    {| Description = "Total columns"; Value = string <| allColumn |}
    {| Description = "Columns with values"; Value = string <| allColumn - isDummy |}
    {| Description = "Large text columns"; Value = bigColumns |}
    {| Description = "ID columns"; Value = identityColumns |}
  ]