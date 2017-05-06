module CalendarSystem.Domain.Membership.Impl.Server.Install
open CalendarSystem.Domain.Membership

/// Tell the domain layer to use this assembly as its implementation.
let install () =
    { new IMembershipDomain with
        member __.Authentication = AuthenticationService.service
        member __.Users = UserService.service
    } |> Setup.useMembership

