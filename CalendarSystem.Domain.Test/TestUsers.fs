module CalendarSystem.Domain.Test.TestUsers
open Rezoom
open NUnit.Framework
open FsUnit
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open CalendarSystem.Domain.Test
open CalendarSystem.Domain.Membership
open System.Security

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