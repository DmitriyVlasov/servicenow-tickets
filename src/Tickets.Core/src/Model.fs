module Tickets.Model 

type Attachment =
  {
    Id            : string
    RequestId     : string
    FileName      : string
  }

type Chunk =
  {
    Position     : int
    AttachmentId : string
    Data         : string
  }

type Request = 
  {
    Id     : string
    Number : string
  }