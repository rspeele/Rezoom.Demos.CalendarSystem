module private CalendarSystem.Domain.Membership.Impl.Server.CalendarEventService
open System
open System.Security
open Utilities
open Rezoom
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open CalendarSystem.Persistence.Membership
open CalendarSystem.Domain.Membership

let private sessionLength = 7.0<days>

let service =
    { new IAuthenticationService with
        member __.Login(email, password) =
            plan {
                let! user = MembershipPersistence.Users.GetUserByEmail(email)
                match user with
                | None -> return None
                | Some (user, hash) ->
                    if hash.Verify(password.Text) then
                        let token = SessionToken.Generate()
                        let expiration = DateTimeOffset.UtcNow + TimeSpan.OfDays(sessionLength)
                        let! _ = MembershipPersistence.Sessions.CreateSession(token, user.Id, expiration)
                        return Some (token, expiration)
                    else
                        return None
            }
        member __.Authenticate(token) =
            plan {
                let! session = MembershipPersistence.Sessions.GetValidSessionByToken(token)
                match session with
                | Some (session, user) when session.ValidTo > DateTimeOffset.UtcNow ->
                    let common = { sessionId = session.Id; userId = user.Id }
                    return
                        match user.Role with
                        | AdminUser -> AuthAdmin (AdminAuthSession common)
                        | ConsultantUser -> AuthConsultant (ConsultantAuthSession common)
                        | ClientUser clientId -> AuthClient (ClientAuthSession (common, clientId))
                | _ ->
                    return raise <| SecurityException("Invalid or expired session token")
            }
    }
