namespace CalendarSystem.Persistence.SystemTasks
open System
open Rezoom
open CalendarSystem.Model
open CalendarSystem.Model.SystemTasks

/// This interface is referenced by the domain layer implementations to enqueue tasks.
type ISystemTaskPersistence =
    abstract member EnqueueTask : scheduleFor : DateTimeOffset * task : SystemTask -> SystemTask Id Plan
