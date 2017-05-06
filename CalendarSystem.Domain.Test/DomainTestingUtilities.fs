[<AutoOpen>]
module CalendarSystem.Domain.Test.DomainTestingUtilities
open Rezoom
open CalendarSystem.Domain.Test
open CalendarSystem.Domain.Membership

let loginAsRoot =
    plan {
        let password = InputPassword.OfString(Testing.rootPass) |> assumeResultOk
        let! claim = Membership.Authentication.Login(Testing.rootEmail, password)
        return
            match claim with
            | Some (claim, _) -> claim
            | None -> bug "Root credentials should always work in testing"
    }