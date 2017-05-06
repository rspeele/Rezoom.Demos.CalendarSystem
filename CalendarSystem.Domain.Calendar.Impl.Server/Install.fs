module CalendarSystem.Domain.Calendar.Impl.Server.Install
open CalendarSystem.Domain.Calendar

let install () =
    { new ICalendarDomain with
        member __.CalendarEvents = CalendarEventService.service
    } |> Setup.useCalendar

