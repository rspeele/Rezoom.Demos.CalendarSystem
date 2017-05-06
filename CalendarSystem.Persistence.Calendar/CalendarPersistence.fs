[<AutoOpen>]
module CalendarSystem.Persistence.Calendar.Implementation

let private di =
    LazyDependencyInjection(fun i ->
        { new ICalendarPersistence with
            member __.CalendarEvents = i.Value.CalendarEvents
        })

module Setup =
    let useCalendarPersistence i = di.UseImplementation(i)

let CalendarPersistence = di.Instance


