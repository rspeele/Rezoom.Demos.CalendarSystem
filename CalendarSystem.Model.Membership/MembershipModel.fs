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
    member this.MemberOfClient =
        match this with
        | SuperUser
        | AdminUser
        | ConsultantUser -> None
        | ClientUser clientId -> Some clientId

type User =
    {   Id : User Id
        Name : string
        Email : EmailAddress
        Role : Role
        Created : Occurence
        Updated : Occurence
    }

/// Represents the randomly generated setup token for a new user.
/// When a user is first created, they don't have a password to log in yet, but we send them this token.
/// Using it, they can set up their password.
type UserSetupToken =
    | UserSetupToken of string
    static member Generate() =
        /// Almost 3 trillion possibilities -- nobody should be able to guess one during the time it's valid
        UserSetupToken (SecureRandomString.ofLength 8 "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" )

/// Represents the stored form of a password, which should be impossible to reverse to a plain-text password
/// without brute forcing it, and slow to brute force.
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