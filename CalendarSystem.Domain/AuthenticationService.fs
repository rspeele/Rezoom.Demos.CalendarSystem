namespace CalendarSystem.Domain
open System
open System.Security
open Rezoom
open CalendarSystem.Common
open CalendarSystem.Persistence.Membership

type CommonSessionInfo =
    internal
        {   SessionId : Session Id
            UserId : User Id
        }

type AdminSession = internal AdminSession of CommonSessionInfo
type ConsultantSession = internal ConsultantSession of CommonSessionInfo
type ClientSession = internal ClientSession of CommonSessionInfo * Client Id

type Session =
    | AsAdmin of AdminSession
    | AsConsultant of ConsultantSession
    | AsClient of ClientSession
    member this.CommonInfo =
        match this with
        | AsAdmin (AdminSession id)
        | AsConsultant (ConsultantSession id)
        | AsClient (ClientSession (id, _)) -> id

type AuthenticationService
    ( users : IUserPersistence
    , sessions : ISessionPersistence
    ) =
    // Issue tokens valid for up to a week. Maybe a shorter duration would be suitable if security requirements
    // are high.
    static let sessionLength = TimeSpan.FromDays(7.0)

    /// Attempt to log in with an email and password.
    /// If successful, you get both a session token, and a DateTimeOffset telling you when it'll expire.
    member __.Login(email : EmailAddress, password : string) =
        plan {
            let! user = users.GetUserByEmail(email)
            match user with
            | None -> return None
            | Some (user, hash) ->
                if hash.Verify(password) then
                    let token = SessionToken.Generate()
                    let expiration = DateTimeOffset.UtcNow + sessionLength
                    let! _ = sessions.CreateSession(token, user.Id, expiration)
                    return Some (token, expiration)
                else
                    return None
        }
    /// Attempt to authenticate using a session token.
    /// If successful, you'll get a Session, otherwise a SecurityException is thrown.
    member __.Authenticate(token : SessionToken) =
        plan {
            let! session = sessions.GetValidSessionByToken(token)
            match session with
            | Some (session, user) when session.ValidTo > DateTimeOffset.UtcNow ->
                let common = { SessionId = session.Id; UserId = user.Id }
                return
                    match user.Role with
                    | AdminUser -> AsAdmin (AdminSession common)
                    | ConsultantUser -> AsConsultant (ConsultantSession common)
                    | ClientUser clientId -> AsClient (ClientSession (common, clientId))
            | _ ->
                return raise <| SecurityException("Invalid or expired session token")
        }
