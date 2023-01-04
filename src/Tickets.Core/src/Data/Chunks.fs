module Tickets.Data.Chunks

open Tickets
open Tickets.Model

let extractor doc = 
  Extractor.dataFromXml doc "sys_attachment_doc"
  |> Seq.map ( fun map -> 
    let find x = Map.find x map
    {
      Position = int <| find "position" 
      AttachmentId = find "sys_attachment"
      Data = find "data"
    }
  )

let findContent attachmentId chunks =  
    chunks
    |> Seq.filter (fun chunk -> chunk.AttachmentId = attachmentId)
    |> Seq.sortBy (fun chunk -> chunk.Position)
    |> Seq.map (fun chunk -> System.Convert.FromBase64String(chunk.Data) )
    |> Array.concat