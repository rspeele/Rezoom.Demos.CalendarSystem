namespace CalendarSystem.Model.Membership
open CalendarSystem.Model

type CommonSession =
    internal
        {   userId : User Id
            sessionId : Session Id
        }
    member this.UserId = this.userId
    member this.SessionId = this.sessionId

type AdminAuthSession =
    internal
    | AdminAuthSession of CommonSession
    member this.CommonSession = let (AdminAuthSession c) = this in c

type ConsultantAuthSession =
    internal
    | ConsultantAuthSession of CommonSession
    member this.CommonSession = let (ConsultantAuthSession c) = this in c

type ClientAuthSession =
    internal
    | ClientAuthSession of CommonSession * Client Id
    member this.CommonSession = let (ClientAuthSession (common, _)) = this in common
    member this.ClientId = let (ClientAuthSession (_, id)) = this in id

type AuthSession =
    | AuthAdmin of AdminAuthSession
    | AuthConsultant of ConsultantAuthSession
    | AuthClient of ClientAuthSession
    member this.CommonSession =
        match this with
        | AuthAdmin (AdminAuthSession common) -> common
        | AuthConsultant (ConsultantAuthSession common) -> common
        | AuthClient (ClientAuthSession (common, _)) -> common