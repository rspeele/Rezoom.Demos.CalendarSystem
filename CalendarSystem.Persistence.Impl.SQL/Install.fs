module CalendarSystem.Persistence.Impl.SQL.Install
open Rezoom
open Rezoom.SQL
open CalendarSystem.Persistence.Membership
open CalendarSystem.Persistence.Calendar
open CalendarSystem.Persistence.SystemTasks
open CalendarSystem.Persistence.SystemTasks.Processing

let install () =
    { new ICalendarPersistence with
        member __.CalendarEvents = CalendarEventPersistence.calendarEventPersistence
    } |> Setup.useCalendarPersistence
    { new IMembershipPersistence with
        member __.Users = UserPersistence.userPersistence
        member __.Sessions = SessionPersistence.sessionPersistence
    } |> Setup.useMembershipPersistence
    { new ISystemTaskPersistence with
        member __.EnqueueTask(scheduledFor, task) =
            TaskPersistence.enqueueSystemTask scheduledFor task
    } |> Setup.useSystemTaskPersistence
    { new ISystemTaskPersistenceProcessing with
        member __.DequeueTask(forProcessingBy, filterToType) =
            TaskPersistence.dequeueSystemTask forProcessingBy filterToType
        member __.CompleteTask(taskId, completion) =
            TaskPersistence.completeTask taskId completion
    } |> Setup.useSystemTaskPersistenceProcessing