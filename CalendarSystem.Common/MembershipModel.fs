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

type UserPasswordHash =
    | BCryptDotNet of string
    member this.Verify(password : string) =
        match this with
        | BCryptDotNet hash ->
            BCrypt.Net.BCrypt.Verify(password, hash)
    static member Generate(password : string) =
        let salt = BCrypt.Net.BCrypt.GenerateSalt()
        let hashed = BCrypt.Net.BCrypt.HashPassword(password, salt)
        BCryptDotNet hashed

type Session =
    {   Id : Session Id
        UserId : User Id
        Token : SessionToken
        Created : DateTimeOffset
        ValidTo : DateTimeOffset
    }