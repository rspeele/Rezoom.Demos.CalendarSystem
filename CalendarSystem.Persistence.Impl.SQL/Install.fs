module CalendarSystem.Persistence.Impl.SQL.Install
open Rezoom
open Rezoom.SQL
open CalendarSystem.Persistence.Membership
open CalendarSystem.Persistence.Calendar

let install () =
    { new ICalendarPersistence with
        member __.CalendarEvents = CalendarEventPersistence.calendarEventPersistence
    } |> Setup.useCalendarPersistence
    { new IMembershipPersistence with
        member __.Users = UserPersistence.userPersistence
        member __.Sessions = SessionPersistence.sessionPersistence
    } |> Setup.useMembershipPersistence