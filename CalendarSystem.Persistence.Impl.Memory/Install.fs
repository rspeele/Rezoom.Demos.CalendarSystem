module CalendarSystem.Persistence.Impl.Memory.Install
open Rezoom
open CalendarSystem.Persistence.Membership
open CalendarSystem.Persistence.Calendar
open CalendarSystem.Persistence.SystemTasks
open CalendarSystem.Persistence.SystemTasks.Processing

let private wrap x =
    plan {
        do! ResetPerPlan.requireReset
        return x()
    }

let private userPersistence =
    { new IUserPersistence with
        member __.CreateUser(createdBy, email, setupToken, name, role) =
            wrap <| fun () -> Storage.Users.createUser createdBy email setupToken name role
        member __.GetUserByEmail(email) =
            wrap <| fun () -> Storage.Users.getUserByEmail email
        member __.GetUserById(id) =
            wrap <| fun () -> Storage.Users.getUserById id
        member __.GetUserBySetupToken(token) =
            wrap <| fun () -> Storage.Users.getUserBySetupToken token
        member __.Update(updatedBy, updateUser, email, name) =
            wrap <| fun () -> Storage.Users.updateUser updatedBy updateUser email name
        member __.UpdatePassword(updatedBy, updateUser, password) =
            wrap <| fun () -> Storage.Users.updateUserPassword updatedBy updateUser password
    }

let private sessionPersistence =
    { new ISessionPersistence with
        member __.CreateSession(sessionToken, impersonator, sessionUserId, sessionValidTo) =
            wrap <| fun () -> Storage.Sessions.createSession sessionToken impersonator sessionUserId sessionValidTo
        member __.GetValidSessionByToken(token) =
            wrap <| fun () -> Storage.Sessions.getValidSessionByToken token
    }

let private calendarEventPersistence =
    { new ICalendarEventPersistence with
        member __.CreateCalendarEvent(createdBy, client, consultant, name, duration) =
            wrap <| fun () -> Storage.CalendarEvents.createCalendarEvent createdBy client consultant name duration
        member __.CreateCalendarEventVersion(createdBy, calendarEvent, consultant, name, duration) =
            wrap <| fun () -> Storage.CalendarEvents.createCalendarEventVersion createdBy calendarEvent consultant name duration
        member __.DeleteCalendarEvent(deletedBy, calendarEvent) =
            wrap <| fun () -> Storage.CalendarEvents.deleteCalendarEvent deletedBy calendarEvent
        member __.GetCalendarEvents(filterToClient, filterToConsultant, touchesDuration) =
            wrap <| fun () -> Storage.CalendarEvents.getCalendarEvents filterToClient filterToConsultant touchesDuration
    }

let private systemTasksPersistence =
    { new ISystemTaskPersistence with
        member __.EnqueueTask(scheduledFor, task) =
            wrap <| fun () -> Storage.SystemTasks.enqueueTask scheduledFor task
    }

let private systemTasksPersistenceProcessing =
    { new ISystemTaskPersistenceProcessing with
        member __.DequeueTask(processingBy, filtertoType) =
            wrap <| fun () -> Storage.SystemTasks.dequeueTask processingBy filtertoType
        member __.CompleteTask(taskId, completion) =
            wrap <| fun () -> Storage.SystemTasks.completeTask taskId completion
    }
    
let install () =
    { new ICalendarPersistence with
        member __.CalendarEvents = calendarEventPersistence
    } |> Setup.useCalendarPersistence
    { new IMembershipPersistence with
        member __.Users = userPersistence
        member __.Sessions = sessionPersistence
    } |> Setup.useMembershipPersistence
    Setup.useSystemTaskPersistence systemTasksPersistence
    Setup.useSystemTaskPersistenceProcessing systemTasksPersistenceProcessing

