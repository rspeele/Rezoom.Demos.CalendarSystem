namespace CalendarSystem.Domain
open System
open System.Security
open Rezoom
open CalendarSystem.Common
open CalendarSystem.Persistence.Membership
open CalendarSystem.Persistence.Calendar

type CalendarEventService
    ( users : IUserPersistence
    , calendarEvents : ICalendarEventPersistence
    ) =
    member __.CreateEvent(AdminSession session, clientId, consultantId, name, duration) =
        plan {
            let! consultant = users.GetUserById(consultantId)
            match consultant.Role with
            | ConsultantUser ->
                let! existing =
                    calendarEvents.GetCalendarEvents
                        (filterToClient = None, filterToConsultant = Some consultantId, touchesDuration = duration)
                if existing.Count > 0 then
                    return Error [InvalidArgument("duration", "A consultant cannot have overlapping events")]
                else
                    let doIt = calendarEvents.CreateCalendarEvent(session.SessionId, clientId, consultantId, name, duration)
                    return Ok doIt
            | _ ->
                return Error [InvalidArgument("consultantId", "Only a consultant can have calendar events.")]
        }