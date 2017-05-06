module private CalendarSystem.Domain.Calendar.Impl.Server.CalendarEventService
open Rezoom
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open CalendarSystem.Model.Calendar
open CalendarSystem.Persistence.Calendar
open CalendarSystem.Domain.Membership
open CalendarSystem.Domain.Calendar

let service =
    { new ICalendarEventService with
        member __.CreateEvent(NonNull session, clientId, consultantId, name, duration) =
            plan {
                let! consultant = Membership.Users.GetUserById(AuthAdmin session, consultantId)
                match consultant.Role with
                | ConsultantUser ->
                    let! existing =
                        CalendarPersistence.CalendarEvents.GetCalendarEvents
                            (filterToClient = None, filterToConsultant = Some consultantId, touchesDuration = duration)
                    if existing.Count > 0 then
                        return Error [InvalidArgument("duration", "A consultant cannot have overlapping events")]
                    else
                        let doIt =
                            CalendarPersistence.CalendarEvents.CreateCalendarEvent
                                (session.CommonSession.SessionId, clientId, consultantId, name, duration)
                        return Ok doIt
                | _ ->
                    return Error [InvalidArgument("consultantId", "Only a consultant can have calendar events.")]
            }
        member __.GetEvents(session, duration) =
            plan {
                return failwith ""
            }
    }