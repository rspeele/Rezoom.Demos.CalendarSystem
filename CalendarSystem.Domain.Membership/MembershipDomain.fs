[<AutoOpen>]
module CalendarSystem.Domain.Membership.Implementation

let private di =
    LazyDependencyInjection(fun i ->
        { new IMembershipDomain with
            member __.Users = i.Value.Users
            member __.Authentication = i.Value.Authentication
        })

module Setup =
    let useMembership i = di.UseImplementation(i)

let Membership = di.Instance