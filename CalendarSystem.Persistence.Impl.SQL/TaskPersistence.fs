module CalendarSystem.Persistence.Impl.SQL.TaskPersistence
open System
open System.Collections.Generic
open CalendarSystem.Persistence.Membership
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open CalendarSystem.Model.Calendar
open CalendarSystem.Model.SystemTasks
open CalendarSystem.Persistence.Impl.SQL.Mapping
open CalendarSystem.Persistence.Calendar
open Rezoom
open Rezoom.SQL
open Rezoom.SQL.Plans
open Newtonsoft.Json

let private taskTypeStringMapping =
    [|  SendUserSetupTokenTaskType, "USER_SETUP_TOKEN"
        SendCalendarEventNotificationTaskType, "CALENDAR_EVENT_NOTIFICATION"
    |]

let private taskTypeToString (taskType : SystemTaskType) =
    taskTypeStringMapping |> Array.find (fst >> (=) taskType) |> snd

let private stringToTaskType (str : string) =
    taskTypeStringMapping |> Array.find (snd >> (=) str) |> fst

type private TaskStateEnum =
    | Scheduled = 1
    | InProcessing = 2
    | CompletedFail = 3
    | CompletedSuccess = 4

type private EnqueueSystemTaskSQL = SQL<"""
    insert into SystemTasks row
        Scheduled = @scheduled
        , ScheduledFor = @scheduledFor
        , TaskType = @taskType
        , TaskJson = @taskJson
        , TaskState = @taskState
        ;
    select scope_identity() as id;
""">

let enqueueSystemTask (scheduledFor : DateTimeOffset) (task : SystemTask) =
    plan {
        let taskJson = JsonConvert.SerializeObject(task)
        let cmd =
            EnqueueSystemTaskSQL.Command
                ( scheduled = DateTimeOffset.UtcNow
                , scheduledFor = scheduledFor
                , taskJson = taskJson
                , taskState = int TaskStateEnum.Scheduled
                , taskType = taskTypeToString task.TaskType
                )
        let! taskId = cmd.Scalar()
        return Id taskId
    }

type private DequeueSystemTaskSQL = SQL<"""
    update SystemTasks set
        TaskState = @takenTaskState
        , ProcessingBy = @processingBy
        , ProcessingStarted = @now
    where ((TaskState = @scheduledTaskState and ScheduledFor <= @now)
            or (TaskState = @failedTaskState and RetryAfter <= @now))
    and (@taskType is null or @taskType = TaskType)
    limit 1;

    select * from SystemTasks
    where TaskState = @takenTaskState
    and ScheduledFor <= @now
    and ProcessingBy = @processingBy
    and (@taskType is null or @taskType = TaskType)
    limit 1;
""">

let dequeueSystemTask (forProcessingBy : Guid) (filterToType : SystemTaskType option) =
    plan {
        let cmd =
            DequeueSystemTaskSQL.Command
                ( scheduledTaskState = int TaskStateEnum.Scheduled
                , failedTaskState = int TaskStateEnum.CompletedFail
                , takenTaskState = int TaskStateEnum.InProcessing
                , processingBy = Some (forProcessingBy.ToByteArray())
                , now = (Some DateTimeOffset.UtcNow)
                , taskType = (filterToType |> Option.map taskTypeToString)
                )
        let! row = cmd.TryExactlyOne()
        match row with
        | None -> return None
        | Some row ->
            return Some (Id row.Id, JsonConvert.DeserializeObject<SystemTask>(row.TaskJson))
    }

type CompleteTaskSQL = SQL<"""
    update SystemTasks set
        TaskState = @completedTaskState
        , Completed = @now
        , FailureMessage = @failureMessage
        , FailureStackTrace = @failureStackTrace
        , RetryAfter = @retryAfter
    where Id = @id
""">

let completeTask (Id taskId : SystemTask Id) completion =
    plan {
        let cmd =
            match completion with
            | SystemTaskFailed info ->
                CompleteTaskSQL.Command
                    ( id = taskId
                    , completedTaskState = int TaskStateEnum.CompletedFail
                    , now = Some DateTimeOffset.UtcNow
                    , failureMessage = Some info.Message
                    , failureStackTrace = Some info.StackTrace
                    , retryAfter = info.RetryAfter
                    )
            | SystemTaskSucceeded ->
                CompleteTaskSQL.Command
                    ( id = taskId
                    , completedTaskState = int TaskStateEnum.CompletedSuccess
                    , now = Some DateTimeOffset.UtcNow
                    , failureMessage = None
                    , failureStackTrace = None
                    , retryAfter = None
                    )
        return! cmd.Plan()
    }