module CalendarSystem.Domain.Membership.Impl.Server.Install
open CalendarSystem.Domain.Membership

let install () =
    { new IMembershipDomain with
        member __.Authentication = AuthenticationService.service
        member __.Users = UserService.service
    } |> Setup.useMembership

