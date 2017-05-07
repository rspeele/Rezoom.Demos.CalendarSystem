module private CalendarSystem.Domain.Membership.Impl.Server.UserService
open System
open Rezoom
open CalendarSystem.Persistence.Membership
open CalendarSystem.Domain.Membership
open CalendarSystem.Model
open CalendarSystem.Model.SystemTasks
open CalendarSystem.Model.Membership
open CalendarSystem.Persistence.SystemTasks.Implementation
open System.Security

let service =
    { new IUserService with
        member this.CreateUser(claim, email, name, role) =
            plan {
                let! sessionId, me = Membership.Authentication.Authenticate(claim)
                let create =
                    plan {
                        let setupToken = UserSetupToken.Generate()
                        let! createdUserId =
                            MembershipPersistence.Users.CreateUser
                                ( sessionId
                                , email
                                , setupToken
                                , name
                                , role
                                )
                        let! _ =
                            SystemTaskPersistence.EnqueueTask
                                (DateTimeOffset.UtcNow, QueueSystemTask.userSetupToken setupToken)
                        return createdUserId
                    }
                return
                    match me.Role, role with
                    | SuperUser, _ -> Ok create
                    | AdminUser, SuperUser -> Error [Invalid "Only super-users can create other super-users"]
                    | AdminUser, _ -> Ok create
                    | ConsultantUser, _
                    | ClientUser _, _ -> Error [Invalid "You must be an admin or super-user to create other users"]
            }
        member this.GetUserById(claim, userId) =
            plan {
                let! _ = Membership.Authentication.Authenticate(claim)
                return! MembershipPersistence.Users.GetUserById(userId)
            }
        member this.SetupUser(setupToken, password) =
            plan {
                let! user = MembershipPersistence.Users.GetUserBySetupToken(setupToken)
                match user with
                | None -> raise <| SecurityException("Invalid setup token")
                | Some (userId, created) ->
                    if created <= DateTimeOffset.UtcNow + TimeSpan.OfDays(-14.0<days>) then
                        raise <| SecurityException("Setup token expired")
                    else
                        // get a temp session just for the setup
                        let sessionToken = SessionToken.Generate()
                        let! sessionId =
                            MembershipPersistence.Sessions.CreateSession
                                ( sessionToken
                                , None
                                , userId
                                , DateTimeOffset.UtcNow.AddMinutes(30.0)
                                )
                        return!
                            MembershipPersistence.Users.UpdatePassword
                                ( updatedBy = sessionId
                                , updateUser = userId
                                , password = password
                                )
            }
    }