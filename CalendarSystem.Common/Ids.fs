namespace CalendarSystem.Common

/// Represents an integer ID with the generic type of entity it relates to.
type Id<'a> = internal Id of int
    
/// Secure session token (random string generated for each session)
type SessionToken = internal SessionToken of string

