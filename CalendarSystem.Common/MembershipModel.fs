// Here's where we define the representation of entities in the system
// related to membership (being a user, being associated with a client, that sort of thing).

namespace rec CalendarSystem.Common
open System

type Occurence = // e.g. at owl creek bridge
    {   Who : Session Id
        When : DateTimeOffset
    }

type Client =
    {   Id : Client Id
        Name : string
        Updated : Occurence
        Created : Occurence
    }

/// A user can be one of these three things.
/// Some systems are fancier and let you be multiple roles at once but that doesn't really make sense here.
type Role =
    /// Admin -- handles setting up new clients, invoicing, etc.
    | AdminUser
    /// Consultant -- logs on to view calendar, goes to customer sites to do work.
    | ConsultantUser
    /// Client -- logs on to view calendar events related to their own company.
    | ClientUser of Client Id

type User =
    {   Id : User Id
        Name : string
        Email : EmailAddress
        Role : Role
        Created : Occurence
        Updated : Occurence
    }

/// Helps us upgrade the password hash format later, since we'll know which passwords
/// are stored in which format.
type UserPasswordHashVersion =
    | PBKDF2With5000Rounds

type UserPasswordHash =
    {   Version : UserPasswordHashVersion
        Hash : byte array
        Salt : byte array
    }

type Session =
    {   Id : Session Id
        UserId : User Id
        Token : SessionToken
        Created : DateTimeOffset
        ValidTo : DateTimeOffset
    }

type CurrentUserId = internal CurrentUserId of User Id