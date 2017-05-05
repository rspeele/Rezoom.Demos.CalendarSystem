namespace CalendarSystem.Persistence.Membership
open System
open Rezoom
open CalendarSystem.Common
    
/// Defines the API for persisting sessions.
type ISessionPersistence =
    /// Store a session.
    abstract member CreateSession
        : sessionToken : SessionToken
        * sessionUserId : User Id
        * sessionValidTo : DateTimeOffset
        -> Session Id Plan

    /// Get a session record that is still valid by its token.
    /// Useful when implementing domain logic to check session validity/associated user.
    abstract member GetValidSessionByToken : token : SessionToken -> (Session * User) option Plan