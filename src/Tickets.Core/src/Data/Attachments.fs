module Tickets.Data.Attachments

open System
open System.IO
open Tickets
open Tickets.Model

let extractor doc = 
  Extractor.dataFromXml doc "sys_attachment"
  |> Seq.map ( fun map -> 
    let find x = Map.find x map
    {
      Id = find "sys_id"
      RequestId = find "table_sys_id"
      FileName = find "file_name"
    }
  )
  |> Seq.filter ( fun item -> 
    not <| String.IsNullOrEmpty(item.FileName) 
  )
