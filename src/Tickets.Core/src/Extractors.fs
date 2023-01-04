module Tickets.Extractor

open System
open System.IO
open System.Xml
open HtmlAgilityPack

let private now () = System.DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss")

let xmlDocument (path:string) = 
  let doc = new XmlDocument()
  doc.PreserveWhitespace <- false
  doc.Load(path)
  printfn $"{now()} | Load file {Path.GetFileName path}"
  doc

let xmlDocuments path =
  Directory.EnumerateFiles(path)
  |> Seq.filter( fun path -> Path.GetExtension(path) = ".xml")
  |> Seq.map xmlDocument

// Select with xpath syntax.
// see more use cases https://www.w3schools.com/xml/xpath_syntax.asp

let dataFromXml (doc:XmlDocument) (nodeName:string) = 
  doc.SelectNodes($"//unload/{nodeName}")
  |> Seq.cast<XmlNode>
  |> Seq.map ( fun node -> 
    node.ChildNodes
    |> Seq.cast<XmlNode>
    |> Seq.map (fun item -> 
      let value = 
        let displayValue = item.Attributes["display_value"] |> Option.ofObj
        let sysId =  item.Attributes["sys_id"] |> Option.ofObj
        match sysId with 
        | Some x -> x.Value
        | None -> match displayValue with
                  | Some x -> x.Value
                  | None -> item.InnerText
      item.Name, value
    )
    |> Map.ofSeq
  )

let extractAllData extractor docs =
  docs
  |> Seq.map extractor
  |> Seq.concat

let textFromHtml (html:string) =
  let doc = new HtmlDocument()
  doc.LoadHtml(html)

  let nodes = doc.DocumentNode.SelectNodes("//p")
  nodes
  |> Option.ofObj
  |> Option.map (
      Seq.map ( fun row -> 
          row.InnerText.Replace("&nbsp;","") 
          )
  >> Seq.filter ( String.IsNullOrWhiteSpace >> not )
  >> String.concat Environment.NewLine
  )
  |> Option.defaultValue ""