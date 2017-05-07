module CalendarSystem.Domain.Test.TestUsers
open System
open System.Security
open Rezoom
open NUnit.Framework
open FsUnit
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open CalendarSystem.Model.SystemTasks
open CalendarSystem.Domain.Test
open CalendarSystem.Domain.Membership
open CalendarSystem.Persistence.SystemTasks.Processing.Implementation

[<Test>]
let ``Root can create admins who can create other admins`` () =
    plan {
        let! claim = loginAsRoot
        let email = EmailAddress.OfString("person@example.com") |> assumeResultOk
        let! create =
            Membership.Users.CreateUser(claim, email, "McPerson", AdminUser)
        match create with
        | Ok create ->
            let! createdId = create
            let! adminClaim, _ = Membership.Authentication.Impersonate(SuperUserClaim claim.SessionToken, createdId)
            let email2 = EmailAddress.OfString("human@example.com") |> assumeResultOk
            let! canCreateAdmin, canCreateSuper, canCreateConsultant =
                Membership.Users.CreateUser(adminClaim, email2, "Admin", AdminUser)
                , Membership.Users.CreateUser(adminClaim, email2, "Super", SuperUser)
                , Membership.Users.CreateUser(adminClaim, email2, "Consultant", ConsultantUser)
            match canCreateAdmin, canCreateSuper, canCreateConsultant with
            | Ok _, Error _, Ok _ ->
                ()
            | _ -> bug "Permissions for user creation not working right"
        | Error validationErrors ->
            bug <| Validation.showErrors validationErrors
    } |> Testing.runPlan

[<Test>]
let ``Root can create users who can use their setup token to obtain a working account`` () =
    plan {
        let! claim = loginAsRoot
        let email = EmailAddress.OfString("person@example.com") |> assumeResultOk
        let! create =
            Membership.Users.CreateUser(claim, email, "McPerson", AdminUser)
        match create with
        | Ok create ->
            let! createdId = create
            let! task = SystemTaskPersistenceProcessing.DequeueTask(Guid.NewGuid(), Some SendUserSetupTokenTaskType)
            match task with
            | Some (taskId, SendUserSetupTokenTask token) ->
                let pass = InputPassword.OfString("jumblebumblewump") |> assumeResultOk
                do! Membership.Users.SetupUser(token, pass)
                let! claim = Membership.Authentication.Login(email, pass)
                match claim with
                | Some _ -> ()
                | None -> bug "Couldn't obtain a claim from logging in after setup"
            | _ ->
                bug "Should've send setup token to user"
        | Error validationErrors ->
            bug <| Validation.showErrors validationErrors
    } |> Testing.runPlan