[<AutoOpen>]
module CalendarSystem.Persistence.Membership.Implementation

let private di =
    LazyDependencyInjection(fun i ->
        { new IMembershipPersistence with
            member __.Users = i.Value.Users
            member __.Sessions = i.Value.Sessions
        })

module Setup =
    let useMembershipPersistence i = di.UseImplementation(i)

let MembershipPersistence = di.Instance