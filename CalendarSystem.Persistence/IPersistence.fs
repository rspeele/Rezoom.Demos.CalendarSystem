namespace CalendarSystem.Persistence
open CalendarSystem.Persistence.Calendar
open CalendarSystem.Persistence.Membership

type IPersistence =
    abstract member Users : IUserPersistence
    abstract member Sessions : ISessionPersistence
    abstract member CalendarEvents : ICalendarEventPersistence

[<AutoOpen>]
module Implementation =
    let mutable private instance = None

    module Setup =
        let usePersistence persistence =
            match instance with
            | None -> instance <- persistence
            | Some _ -> bug "Persistence implementation should be set only once, at program initialization"

    let persistence =
        let i : IPersistence Lazy =
            lazy
                match instance with
                | Some i -> i
                | None -> bug "Persistence implementation was not set (call usePersistence at program initialization)"
        { new IPersistence with
            member __.Users = i.Value.Users
            member __.Sessions = i.Value.Sessions
            member __.CalendarEvents = i.Value.CalendarEvents
        }