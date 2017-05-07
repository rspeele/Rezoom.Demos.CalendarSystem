namespace CalendarSystem.Persistence.SystemTasks.Processing
open System
open Rezoom
open CalendarSystem.Model
open CalendarSystem.Model.SystemTasks

/// This interface is used by our background task processing code.
type ISystemTaskPersistenceProcessing =
    abstract member DequeueTask
        : forProcessingBy : Guid
        * filterToType : SystemTaskType option
        -> (SystemTask Id * SystemTask) option Plan

    abstract member CompleteTask
        : taskId : SystemTask Id
        * completion : SystemTaskCompletion
        -> unit Plan
