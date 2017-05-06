module CalendarSystem.Domain.Test.TestAuthentication
open Rezoom
open NUnit.Framework
open FsUnit
open CalendarSystem.Model.Membership
open CalendarSystem.Domain.Test
open CalendarSystem.Domain.Membership

[<Test>]
let ``Can log in as the initial root user`` () =
    plan {
        let! claim = loginAsRoot
        match claim with
        | ClaimingSuperUser _ ->
            return ()
        | _ ->
            return bug "Should be super-user"
    } |> Testing.runPlan

[<Test>]
let ``Can't log in with bogus credentials for root user`` () =
    plan {
        let password = InputPassword.OfString("garblewarbleflorp") |> assumeResultOk
        let! loginResult =
            Membership.Authentication.Login(Testing.rootEmail, password)
        match loginResult with
        | Some _ -> bug "We should not get a claim from this"
        | None -> ()
    } |> Testing.runPlan


[<Test>]
let ``Result of log in as root is a valid claim`` () =
    plan {
        let! claim = loginAsRoot
        let! _, user = Membership.Authentication.Authenticate(claim)
        match user.Role with
        | SuperUser -> ()
        | _ -> bug "Root should be a super user"
    } |> Testing.runPlan
