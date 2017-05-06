namespace CalendarSystem.Domain.Calendar
open System
open System.Collections.Generic
open Rezoom
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open CalendarSystem.Model.Calendar

type ICalendarEventService =
    abstract member CreateEvent
        : claim : AdminClaim
        * clientId : Client Id
        * consultantId : User Id
        * name : string
        * duration : DateTimeOffset InclusiveRange
        -> CalendarEvent Id Plan ValidationResult Plan

    abstract member GetEvents
        : claim : Claim
        * duration : DateTimeOffset InclusiveRange
        -> CalendarEvent IReadOnlyList Plan