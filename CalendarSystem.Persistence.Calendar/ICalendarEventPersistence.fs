namespace CalendarSystem.Persistence.Calendar
open System
open System.Collections.Generic
open Rezoom
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open CalendarSystem.Model.Calendar

/// Defines the API for persisting calendar events.
type ICalendarEventPersistence =
    /// Store a calendar event.
    abstract member CreateCalendarEvent
        : createdBy : Session Id
        * client : Client Id
        * consultant : User Id
        * name : string
        * duration : DateTimeOffset InclusiveRange
        -> CalendarEvent Id Plan

    /// Get the calendar events overlapping a given range (such as a month), with
    /// some other optional filters. The returned list will be ordered by start time.
    abstract member GetCalendarEvents
        : filterToClient : Client Id option
        * filterToConsultant : User Id option
        * touchesDuration : DateTimeOffset InclusiveRange
        -> CalendarEvent IReadOnlyList Plan

    /// Create a new version of a calendar event (effectively updating it).
    /// Note that it is not possible to change the client it's associated with.
    /// For that, you should probably just delete the event and make a new one.
    abstract member CreateCalendarEventVersion
        : createdBy : Session Id
        * calendarEvent : CalendarEvent Id
        * consultant : User Id
        * name : string
        * duration : DateTimeOffset InclusiveRange
        -> CalendarEventVersion Id

    /// Mark a calendar event as deleted.
    abstract member DeleteCalendarEvent
        : deletedBy : Session Id
        * calendarEvent : CalendarEvent Id
        -> unit Plan

    