module Tickets.Data.Journals

open Tickets
let extractor doc = 
  Extractor.dataFromXml doc "sys_journal_field"