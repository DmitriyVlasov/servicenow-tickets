open Argu
open System
open System.IO
open ClosedXML.Excel

open Tickets
open Tickets.Storages

type CliArguments = 
  | [<AltCommandLine("-s")>] Source_Directory of path:string
  | [<AltCommandLine("-t")>] Target_Directroy of path:string
  interface IArgParserTemplate with
    member s.Usage =
      match s with 
      | Source_Directory _ -> "Specify a source directory, what contains some XML files."
      | Target_Directroy _ -> "Specify a target directory for excel file and attachments."

let parser = ArgumentParser.Create<CliArguments>(programName="servicenow-tickets.exe")

let now () = System.DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss")

[<EntryPoint>]
let main args =

  if Array.length args < 2 
  then 
    printfn "%s" <| parser.PrintUsage()

  let args = parser.Parse args
  let sourcePath = args.GetResult(Source_Directory)
  let targetPath = args.GetResult(Target_Directroy)
  let targetFilePath = Path.Combine(targetPath,"result.xlsx")
  
  printfn $"{now()} | Start..."

  let docs = Extractor.xmlDocuments sourcePath
  printfn $"{now()} | Extract all xml data"

  let journalData = Extractor.extractAllData Data.Journals.extractor docs
  printfn $"{now()} | Extract all journal table"

  let requestData = 
    Data.Requests.Deprecate.extractAllData docs
    |> Map.filter (fun _ item -> not item.IsDummy)
  printfn $"{now()} | Extract all Request table"

  let wb = new XLWorkbook()
  
  Excel.fillWorksheetByRow wb "sys_journal_field" journalData
  printfn $"{now()} | Save journal Table"

  Excel.fillWorksheetByCol wb "sc_req_item" requestData
  printfn $"{now()} | Save Request Table"

  wb.SaveAs(targetFilePath)
  printfn $"{now()} | Save Excel file"
  printfn $"{now()} | Finish..."

  // Return 0. This indicates success.
  0