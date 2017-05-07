module CalendarSystem.Persistence.Impl.SQL.CalendarEventPersistence
open System
open System.Collections.Generic
open CalendarSystem.Persistence.Membership
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open CalendarSystem.Model.Calendar
open CalendarSystem.Persistence.Impl.SQL.Mapping
open CalendarSystem.Persistence.Calendar
open Rezoom
open Rezoom.SQL
open Rezoom.SQL.Plans

type private CreateCalendarEventSQL = SQL<"""
    insert into CalendarEvents row
        ClientId = @clientId
        , Created = @created
        , CreatedBy = @createdBy
        , Deleted = null
        , DeletedBy = null
        ;
    select scope_identity() as id;
    insert into CalendarEventVersions row
        CalendarEventId = scope_identity()
        , Name = @name
        , DurationFirst = @durationFirst
        , DurationLast = @durationLast
        , Created = @created
        , CreatedBy = @createdBy
        , ConsultantId = @consultant
        , IsCurrentVersion = true
        ;
""">

let private createCalendarEvent (Id sessionId) (Id clientId) (Id consultantId) name (duration : _ InclusiveRange) =
    plan {
        let cmd =
            CreateCalendarEventSQL.Command
                ( clientId = clientId
                , consultant = consultantId
                , created = DateTimeOffset.UtcNow
                , createdBy = sessionId
                , durationFirst = duration.First
                , durationLast = duration.Last
                , name = name
                )
        let! calendarEventId = cmd.Scalar()
        return Id calendarEventId
    }

type private GetCalendarEventsSQL = SQL<"""
    select c.*, one CurrentVersion(v.*)
    from CalendarEvents c
    join CalendarEventVersions v on v.CalendarEventId = c.Id and v.IsCurrentVersion
    where c.DeletedBy is null
    and (@clientId is null or @clientId = c.ClientId)
    and (@consultantId is null or @consultantId = v.ConsultantId)
    and (v.DurationFirst >= @durationFirst and v.DurationFirst <= @durationLast
        or v.DurationLast >= @durationFirst and v.DurationLast <= @durationLast)
""">

let private getCalendarEvents filterToClient filterToConsultant (touchesDuration : _ InclusiveRange) =
    plan {
        let cmd =
            GetCalendarEventsSQL.Command
                ( clientId = (filterToClient |> Option.map (fun (Id i) -> i))
                , consultantId = (filterToConsultant |> Option.map (fun (Id i) -> i))
                , durationFirst = touchesDuration.First
                , durationLast = touchesDuration.Last
                )
        let! rows = cmd.Plan()
        return
            [| for row in rows ->
                let version =
                    let v = row.CurrentVersion
                    {   Id = Id v.Id
                        CalendarEventId = Id v.CalendarEventId
                        Name = v.Name
                        Duration = InclusiveRange.Of(v.DurationFirst, v.DurationLast) |> assumeResultOk
                        ConsultantId = Id v.ConsultantId
                    }
                {   Id = Id row.Id
                    ClientId = Id row.ClientId
                    Created = occurence row.CreatedBy row.Created
                    CurrentVersion = version
                }
            |] :> _ IReadOnlyList
    }

type private CreateCalendarEventVersionSQL = SQL<"""
    update CalendarEventVersions set
        IsCurrentVersion = false
    where IsCurrentVersion and CalendarEventId = @eventId;
    insert into CalendarEventVersions row
        CalendarEventId = @eventId
        , Name = @name
        , DurationFirst = @durationFirst
        , DurationLast = @durationLast
        , Created = @created
        , CreatedBy = @createdBy
        , ConsultantId = @consultant
        , IsCurrentVersion = true
        ;
    select scope_identity() as id;
""">

let private createCalendarEventVersion
    (Id createdBy) (Id calendarEvent) (Id consultant) name (duration : _ InclusiveRange) =
    plan {
        let cmd =
            CreateCalendarEventVersionSQL.Command
                ( consultant = consultant
                , created = DateTimeOffset.UtcNow
                , createdBy = createdBy
                , eventId = calendarEvent
                , name = name
                , durationFirst = duration.First
                , durationLast = duration.Last
                )
        let! id = cmd.Scalar()
        return Id id
    }

type private DeleteCalendarEventSQL = SQL<"""
    update CalendarEvents set
        DeletedBy = @deletedBy
        , Deleted = @deleted
    where Id = @id and DeletedBy is null
""">

let deleteCalendarEvent (Id deletedBy : Session Id) (Id calendarEvent : CalendarEvent Id) =
    let cmd =
        DeleteCalendarEventSQL.Command
            (deleted = Some DateTimeOffset.UtcNow, deletedBy = Some deletedBy, id = calendarEvent)
    cmd.Plan()

let calendarEventPersistence =
    { new ICalendarEventPersistence with
        member __.CreateCalendarEvent(createdBy, client, consultant, name, duration) =
            createCalendarEvent createdBy client consultant name duration
        member __.GetCalendarEvents(filterToClient, filterToConsultant, touchesDuration) =
            getCalendarEvents filterToClient filterToConsultant touchesDuration
        member __.CreateCalendarEventVersion(createdBy, calendarEvent, consultant, name, duration) =
            createCalendarEventVersion createdBy calendarEvent consultant name duration
        member __.DeleteCalendarEvent(deletedBy, calendarEvent) =
            deleteCalendarEvent deletedBy calendarEvent
    }