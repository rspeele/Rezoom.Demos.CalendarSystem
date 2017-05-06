module CalendarSystem.Persistence.Impl.Memory.Install
open Rezoom
open CalendarSystem.Persistence.Membership
open CalendarSystem.Persistence.Calendar

let private userPersistence =
    { new IUserPersistence with
        member __.CreateUser(createdBy, email, setupToken, name, role) =
            plan {
                return Storage.Users.createUser createdBy email setupToken name role
            }
        member __.GetUserByEmail(email) =
            plan {
                return Storage.Users.getUserByEmail email
            }
        member __.GetUserById(id) =
            plan {
                return Storage.Users.getUserById id
            }
        member __.Update(updatedBy, updateUser, email, name) =
            plan {
                return Storage.Users.updateUser updatedBy updateUser email name
            }
        member __.UpdatePasswordHash(updatedBy, updateUser, hash) =
            plan {
                return Storage.Users.updateUserPassword updatedBy updateUser hash
            }
        
    }

let private sessionPersistence =
    { new ISessionPersistence with
        member __.CreateSession(sessionToken, impersonator, sessionUserId, sessionValidTo) =
            plan {
                return Storage.Sessions.createSession sessionToken impersonator sessionUserId sessionValidTo
            }
        member __.GetValidSessionByToken(token) =
            plan {
                return Storage.Sessions.getValidSessionByToken token
            }
    }

let private calendarEventPersistence =
    { new ICalendarEventPersistence with
        member __.CreateCalendarEvent(createdBy, client, consultant, name, duration) =
            plan {
                return Storage.CalendarEvents.createCalendarEvent createdBy client consultant name duration
            }
        member __.CreateCalendarEventVersion(createdBy, calendarEvent, consultant, name, duration) =
            plan {
                return Storage.CalendarEvents.createCalendarEventVersion createdBy calendarEvent consultant name duration
            }
        member __.DeleteCalendarEvent(deletedBy, calendarEvent) =
            plan {
                return Storage.CalendarEvents.deleteCalendarEvent deletedBy calendarEvent
            }
        member __.GetCalendarEvents(filterToClient, filterToConsultant, touchesDuration) =
            plan {
                return Storage.CalendarEvents.getCalendarEvents filterToClient filterToConsultant touchesDuration
            }
    }
    
let install () =
    { new ICalendarPersistence with
        member __.CalendarEvents = calendarEventPersistence
    } |> Setup.useCalendarPersistence
    { new IMembershipPersistence with
        member __.Users = userPersistence
        member __.Sessions = sessionPersistence
    } |> Setup.useMembershipPersistence

