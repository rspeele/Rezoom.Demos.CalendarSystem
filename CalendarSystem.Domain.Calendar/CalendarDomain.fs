[<AutoOpen>]
module CalendarSystem.Domain.Calendar.Implementation

let private di =
    LazyDependencyInjection(fun i ->
        { new ICalendarDomain with
            member __.CalendarEvents = i.Value.CalendarEvents
        })

module Setup =
    let useCalendar i = di.UseImplementation(i)

let Calendar = di.Instance