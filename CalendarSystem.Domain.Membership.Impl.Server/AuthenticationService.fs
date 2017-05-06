module private CalendarSystem.Domain.Membership.Impl.Server.AuthenticationService
open System
open System.Security
open Utilities
open Rezoom
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open CalendarSystem.Persistence.Membership
open CalendarSystem.Domain.Membership

let private normalSessionLength = 7.0<days>
let private impersonationSessionLength = 1.0<hours>

let private authenticate (claim : Claim) =
    plan {
        let token = claim.SessionToken
        let! session = MembershipPersistence.Sessions.GetValidSessionByToken(token)
        match session with
        | Some (session, user) when session.ValidTo > DateTimeOffset.UtcNow ->
            return
                match user.Role, claim with
                | SuperUser, _ // super users can make any claim they please
                | AdminUser, ClaimingAdmin _
                | ConsultantUser, ClaimingConsultant _
                | ClientUser _, ClaimingClient _ -> session.Id, user
                | _ ->
                    raise <| SecurityException("Invalid claim")
        | _ ->
            return raise <| SecurityException("Invalid or expired session token")
    }

let private claimForUser (user : User) token =
    match user.Role with
    | SuperUser -> ClaimingSuperUser (SuperUserClaim token)
    | AdminUser -> ClaimingAdmin (AdminClaim token)
    | ConsultantUser -> ClaimingConsultant (ConsultantClaim token)
    | ClientUser _ -> ClaimingClient (ClientClaim token)

let service =
    { new IAuthenticationService with
        member __.Login(email, password) =
            plan {
                let! user = MembershipPersistence.Users.GetUserByEmail(email)
                match user with
                | None -> return None
                | Some (_, None) -> return None // they haven't set up their password yet
                | Some (user, Some hash) ->
                    if hash.Verify(password.Text) then
                        let token = SessionToken.Generate()
                        let expiration = DateTimeOffset.UtcNow + TimeSpan.OfDays(normalSessionLength)
                        let! _ = MembershipPersistence.Sessions.CreateSession(token, None, user.Id, expiration)
                        return Some (claimForUser user token, expiration)
                    else
                        return None
            }
        member __.Authenticate(claim) = authenticate claim
        member __.Impersonate(superUserClaim, userId) =
            plan {
                let! superSessionId, _ = authenticate (ClaimingSuperUser superUserClaim)
                let! targetUser = MembershipPersistence.Users.GetUserById(userId)
                let token = SessionToken.Generate()
                let expiration = DateTimeOffset.UtcNow + TimeSpan.OfHours(impersonationSessionLength)
                let! _ = MembershipPersistence.Sessions.CreateSession(token, Some superSessionId, userId, expiration)
                return claimForUser targetUser token, expiration
            }
    }
