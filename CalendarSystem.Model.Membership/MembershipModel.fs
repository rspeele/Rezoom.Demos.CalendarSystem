namespace rec CalendarSystem.Model.Membership
open System
open CalendarSystem.Model

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
    /// SuperUser -- a sr. developer or product owner who can go stomping around doing whatever they want.
    | SuperUser
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
        /// The user the session authenticate.
        UserId : User Id
        /// If present, the super-user who created this session via impersonation.
        Impersonated : Occurence option
        Token : SessionToken
        Created : DateTimeOffset
        ValidTo : DateTimeOffset
    }