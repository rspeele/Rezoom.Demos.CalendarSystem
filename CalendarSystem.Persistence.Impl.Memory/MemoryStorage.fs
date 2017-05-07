/// Implements the functionality required by the persistence layer using in-memory storage (dictionaries).
module private CalendarSystem.Persistence.Impl.Memory.Storage
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open CalendarSystem.Model.Calendar
open System
open System.Collections.Generic

module IdGeneration =
    let mutable counter = 0
    let newId () =
        counter <- counter + 1
        Id counter
    let resetIds () =
        counter <- 0
open IdGeneration
open CalendarSystem.Persistence.Impl.Memory.SeedInfo
open CalendarSystem.Model.SystemTasks

let private store (dict : Dictionary<_ Id, _>) makeThing =
    let id = newId()
    let thing = makeThing id
    dict.[id] <- thing
    thing

let private occurence sessionId =
    {   Who = sessionId
        When = DateTimeOffset.UtcNow
    }

type private StorageUser =
    {   User : User
        UserAuthInfo : Choice<UserPasswordHash, UserSetupToken>
    }

type private StorageCalendarEvent =
    {   CalendarEvent : CalendarEvent
        Deleted : Occurence option
    }

type private StorageSystemTask =
    {   Id : SystemTask Id
        Task : SystemTask
        State : SystemTaskState
    }

let private users = Dictionary()
let private sessions = Dictionary()
let private calendarEvents = Dictionary()
let private calendarEventVersions = Dictionary()
let private systemTasks = Dictionary()

let nuke() =
    users.Clear()
    sessions.Clear()
    calendarEvents.Clear()
    calendarEventVersions.Clear()
    systemTasks.Clear()
    resetIds()

let seed() =
    let initialUserId = newId()
    let initialSession =
        store sessions <| fun id ->
            {   Id = id
                UserId = initialUserId
                ImpersonatedBy = None
                Token = SessionToken.Generate()
                Created = DateTimeOffset.UtcNow
                ValidTo = DateTimeOffset.UtcNow.AddDays(1.0)
            }
    let initialUser =
        {   User =
                {   Id = initialUserId
                    Name = "Root"
                    Email = rootEmail
                    Role = SuperUser
                    Created = occurence initialSession.Id
                    Updated = occurence initialSession.Id
                }
            UserAuthInfo = Choice1Of2 ((=) rootPass)
        }
    users.[initialUserId] <- initialUser

module Users =
    let createUser createdBy email setupToken name role =
        let user =
            store users <| fun id ->
                let user =
                    {   Id = id
                        Name = name
                        Email = email
                        Role = role
                        Created = occurence createdBy
                        Updated = occurence createdBy
                    }
                { User = user; UserAuthInfo = Choice2Of2 setupToken }
        user.User.Id

    let getUserByEmail email =
        let user = users.Values |> Seq.tryFind (fun u -> u.User.Email = email)
        match user with
        | None -> None
        | Some user -> Some (user.User, match user.UserAuthInfo with | Choice1Of2 hash -> Some hash | _ -> None)

    let getUserById id = users.[id].User

    let getUserBySetupToken token =
        let user =
            users.Values
            |> Seq.tryFind (fun u ->
                match u.UserAuthInfo with
                | Choice2Of2 setupToken -> setupToken = token
                | Choice1Of2 _ -> false)
        match user with
        | None -> None
        | Some user -> Some (user.User.Id, user.User.Created.When)

    let updateUser updatedBy updateUser email name =
        let user = users.[updateUser]
        users.[updateUser] <- { user with User = { user.User with Email = email; Name = name } }

    let updateUserPassword by userId password =
        let user = users.[userId]
        users.[userId] <-
            { user with
                UserAuthInfo = Choice1Of2 ((=) password); User = { user.User with Updated = occurence by }
            }

module Sessions =
    let createSession sessionToken impersonator sessionUserId sessionValidTo =
        let session =
            {   Id = newId()
                UserId = sessionUserId
                ImpersonatedBy = impersonator
                Token = sessionToken
                Created = DateTimeOffset.UtcNow
                ValidTo = sessionValidTo
            }
        sessions.[session.Id] <- session
        session.Id

    let getValidSessionByToken token =
        let found = sessions.Values |> Seq.tryFind (fun s -> s.Token = token && s.ValidTo >= DateTimeOffset.UtcNow)
        found |> Option.map (fun s -> s, users.[s.UserId].User)

module CalendarEvents =
    let createCalendarEvent createdBy client consultant name duration =
        let eventId = newId()
        let version = store calendarEventVersions <| fun id ->
            {   Id = id
                CalendarEventId = eventId
                Name = name
                Duration = duration
                ConsultantId = consultant
            }
        let event =
            {   Id = eventId
                ClientId = client
                Created = occurence createdBy
                CurrentVersion = version
            }
        calendarEvents.[event.Id] <- { CalendarEvent = event; Deleted = None }
        event.Id

    let createCalendarEventVersion createdBy calendarEvent consultant name duration =
        let version = store calendarEventVersions <| fun id ->
            {   Id = id
                CalendarEventId = calendarEvent
                Name = name
                Duration = duration
                ConsultantId = consultant
            }
        let event = calendarEvents.[calendarEvent]
        calendarEvents.[calendarEvent] <-
            { event with
                CalendarEvent =
                    { event.CalendarEvent with CurrentVersion = version }
            }
        version.Id

    let deleteCalendarEvent deletedBy calendarEvent =
        let event = calendarEvents.[calendarEvent]
        calendarEvents.[calendarEvent] <-
            { event with
                Deleted = Some (occurence deletedBy)
            }

    let getCalendarEvents filterToClient filterToConsultant touchesDuration =
        let events =
            calendarEvents.Values
            |> Seq.filter (fun c -> Option.isNone c.Deleted)
            |> Seq.map (fun c -> c.CalendarEvent)
        let events =
            match filterToClient with
            | None -> events
            | Some clientId -> events |> Seq.filter (fun e -> e.ClientId = clientId)
        let events =
            match filterToConsultant with
            | None -> events
            | Some consultantId -> events |> Seq.filter (fun e -> e.CurrentVersion.ConsultantId = consultantId)
        events
        |> Seq.filter (fun e -> e.CurrentVersion.Duration.Overlaps(touchesDuration))
        |> Seq.sortBy (fun e -> e.CurrentVersion.Duration.First)
        |> ResizeArray
        :> _ IReadOnlyList

module SystemTasks =
    let enqueueTask (scheduledFor : DateTimeOffset) (task : SystemTask) =
        let stored =
            store systemTasks <| fun id ->
                {   Id = id
                    Task = task
                    State = SystemTaskScheduledFor scheduledFor
                }
        stored.Id

    let dequeueTask processingBy (filterToType : SystemTaskType option) =
        let now = DateTimeOffset.UtcNow
        let found =
            systemTasks.Values
            |> Seq.filter (fun t ->
                match t.State with
                | SystemTaskScheduledFor time
                | SystemTaskCompleted (SystemTaskFailed { RetryAfter = Some time }) ->
                    time <= now
                | _ -> false)
            |> (match filterToType with
                | None -> id
                | Some ty -> Seq.filter (fun t -> t.Task.TaskType = ty))
            |> Seq.tryHead
        match found with
        | None -> None
        | Some found ->
            let newState =
                { found with
                    State = SystemTaskInProcess { ProcessingBy = processingBy; ProcessingStarted = now }
                }
            systemTasks.[newState.Id] <- newState
            Some (newState.Id, newState.Task)

    let completeTask taskId completion =
        let existing = systemTasks.[taskId]
        systemTasks.[taskId] <-
            { existing with
                State = SystemTaskCompleted completion
            }
               