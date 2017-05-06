module private CalendarSystem.Domain.Membership.Impl.Server.UserService
open System
open System.Security
open Utilities
open Rezoom
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open CalendarSystem.Persistence.Membership
open CalendarSystem.Domain.Membership

let service =
    { new IUserService with
        member this.CreateUser(claim, email, name, role) =
            plan {
                let! sessionId, _ = Membership.Authentication.Authenticate(ClaimingAdmin claim)
                let setupToken = UserSetupToken.Generate()
                return!
                    MembershipPersistence.Users.CreateUser
                        ( sessionId
                        , email
                        , setupToken
                        , name
                        , role
                        )
            }
        member this.GetUserById(claim, userId) =
            plan {
                let! _ = Membership.Authentication.Authenticate(claim)
                return! MembershipPersistence.Users.GetUserById(userId)
            }
    }