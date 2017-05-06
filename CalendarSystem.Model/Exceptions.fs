namespace CalendarSystem.Model
open System

type DomainException(msg : string) =
    inherit Exception(msg)

[<AutoOpen>]
module ExceptionUtilities =
    let inline raiseDomain msg = raise (DomainException(msg))