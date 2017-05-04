namespace CalendarSystem.Common
open System

type DomainException(msg : string) =
    inherit Exception(msg)
