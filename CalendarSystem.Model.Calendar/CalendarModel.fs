namespace rec CalendarSystem.Model.Calendar
open System
open CalendarSystem.Model
open CalendarSystem.Model.Membership

/// Represents an event on the calendar.
/// Most of the details are tied to the event version.
/// The only change you can directly make to a CalendarEvent after creating it
/// is to delete it.
type CalendarEvent =
    {   Id : CalendarEvent Id
        ClientId : Client Id
        Created : Occurence
        Deleted : Occurence option
        CurrentVersion : CalendarEventVersion
    }

/// A calendar event can have many versions.
/// Most edits to the calendar event are accomplished just by adding a new version.
/// We *NEVER* edit an existing version.
type CalendarEventVersion =
    {   Id : CalendarEventVersion Id
        CalendarEventId : CalendarEvent Id
        Name : string
        Duration : DateTimeOffset InclusiveRange
        ConsultantId : User Id
    }
