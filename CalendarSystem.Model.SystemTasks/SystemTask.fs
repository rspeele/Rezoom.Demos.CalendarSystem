namespace CalendarSystem.Model.SystemTasks
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open CalendarSystem.Model.Calendar
open System.Diagnostics
open System

type SystemTaskType =
    | SendUserSetupTokenTaskType
    | SendCalendarEventNotificationTaskType
    // expand this when adding new types of system tasks

type SystemTask =
    // Internal constructor so people who reference SystemTask don't have to reference
    // all the model assemblies (i.e. you don't get forced into referencing CalendarSystem.Model.Calendar
    // when all you need is to create a SendUserSetupTokenTask). Queuers construct tasks by the QueueSystemTask module,
    // and the task processor discriminates with InternalsVisibleTo access.
    internal
    | SendUserSetupTokenTask of UserSetupToken
    | SendCalendarEventNotificationTask of CalendarEvent Id
    // expand this when adding new types of system tasks
    member this.TaskType =
        match this with
        | SendUserSetupTokenTask _ -> SendUserSetupTokenTaskType
        | SendCalendarEventNotificationTask _ -> SendCalendarEventNotificationTaskType

module QueueSystemTask =
    let userSetupToken token = SendUserSetupTokenTask token
    let sendCalendarEventNotification id = SendCalendarEventNotificationTask id

type SystemTaskError =
    {   Message : string
        StackTrace : string
        RetryAfter : DateTimeOffset option
    }

type SystemTaskCompletion =
    | SystemTaskFailed of SystemTaskError
    | SystemTaskSucceeded

type SystemTaskInProcess =
    {   /// Identifies the server or process that picked up the task from the queue.
        ProcessingBy : Guid
        ProcessingStarted : DateTimeOffset
    }

type SystemTaskState =
    | SystemTaskScheduledFor of DateTimeOffset
    | SystemTaskInProcess of SystemTaskInProcess
    | SystemTaskCompleted of SystemTaskCompletion