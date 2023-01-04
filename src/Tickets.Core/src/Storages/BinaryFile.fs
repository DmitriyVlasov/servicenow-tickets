module Tickets.Storages.BinaryFile

open System
open System.IO
open System.IO.Compression

let private createZipFile fileName buffer = 
  File.WriteAllBytes( fileName, buffer )

// Example of unpacking an archive:
// https://docs.microsoft.com/ru-ru/dotnet/api/system.io.compression.gzipstream.-ctor
let private unzipFile archivePath targetPath =
  use gzFile = File.Open( archivePath,FileMode.Open )
  use targetFileStream = File.Create( targetPath )
  use decompressor =  new GZipStream( gzFile, CompressionMode.Decompress )
  decompressor.CopyTo( targetFileStream )

let private deleteFile fileName = 
  File.Delete( fileName )

let saveContent content path =
  let tempFileName = Path.GetTempFileName()
  createZipFile tempFileName content
  unzipFile tempFileName path
  deleteFile tempFileName