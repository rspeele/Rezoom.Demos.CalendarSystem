namespace CalendarSystem.Domain
open System
open System.Security
open Rezoom
open CalendarSystem.Common
open CalendarSystem.Persistence.Membership

type SessionService
    ( users : IUserPersistence
    , sessions : ISessionPersistence
    ) =
    static let sessionLength = TimeSpan.FromDays(1.0)
    let computeHash version (password : string) (salt : byte array) =
        match version with
        | PBKDF2With5000Rounds ->
            failwith "not implemented" : byte array
    let newSessionToken () =
        failwith "not implemented (random string)" : SessionToken
    member __.Login(email : EmailAddress, password : string) =
        plan {
            let! user = users.GetUserByEmail(email)
            match user with
            | None -> return None
            | Some (user, hash) ->
                let computedHash = computeHash hash.Version password hash.Salt
                if computedHash = hash.Hash then
                    let token = newSessionToken ()
                    let! _ = sessions.CreateSession(token, user.Id, DateTimeOffset.UtcNow + sessionLength)
                    return Some token
                else
                    return None
        }
    member __.Authenticate(token : SessionToken) =
        plan {
            let! session = sessions.GetValidSessionByToken(token)
            match session with
            | Some session when session.ValidTo > DateTimeOffset.UtcNow ->
                return CurrentUserId session.UserId
            | _ ->
                return raise <| SecurityException("Invalid or expired session token")
        }
