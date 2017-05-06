module CalendarSystem.Domain.Calendar.Impl.Server.Install
open CalendarSystem.Domain.Calendar

/// Tell the domain layer to use this assembly as its implementation.
let install () =
    { new ICalendarDomain with
        member __.CalendarEvents = CalendarEventService.service
    } |> Setup.useCalendar

