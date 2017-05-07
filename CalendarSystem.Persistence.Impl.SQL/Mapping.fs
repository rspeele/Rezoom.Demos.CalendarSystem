module CalendarSystem.Persistence.Impl.SQL.Mapping
open System.IO
open CalendarSystem.Model
open CalendarSystem.Model.Membership

let dbOfRole role =
    match role with
    | SuperUser -> "SUPER", None
    | AdminUser -> "ADMIN", None
    | ConsultantUser -> "CONSULT", None
    | ClientUser (Id id) -> "CLIENT", Some id

let dbToRole str clientId =
    match str, clientId with
    | "SUPER", None -> SuperUser
    | "ADMIN", None -> AdminUser
    | "CONSULT", None -> ConsultantUser
    | "CLIENT", Some id -> ClientUser (Id id)
    | _ -> raise <| InvalidDataException("Invalid role storage")

let occurence sessionId dt =
    {   Who = Id sessionId
        When = dt
    }

