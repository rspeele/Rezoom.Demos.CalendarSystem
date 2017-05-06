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
                        let claim =
                            match user.Role with
                            | AdminUser -> ClaimingAdmin (AdminClaim token)
                            | ConsultantUser -> ClaimingConsultant (ConsultantClaim token)
                            | ClientUser _ -> ClaimingClient (ClientClaim token)
                        return Some (claim, expiration)
                    else
                        return None
            }
        member __.Authenticate(claim) =
            plan {
                let token = claim.SessionToken
                let! session = MembershipPersistence.Sessions.GetValidSessionByToken(token)
                match session with
                | Some (session, user) when session.ValidTo > DateTimeOffset.UtcNow ->
                    return
                        match user.Role, claim with
                        | AdminUser, ClaimingAdmin _
                        | ConsultantUser, ClaimingConsultant _
                        | ClientUser _, ClaimingClient _ -> session.Id, user
                        | _ ->
                            raise <| SecurityException("Invalid claim")
                | _ ->
                    return raise <| SecurityException("Invalid or expired session token")
            }
    }
