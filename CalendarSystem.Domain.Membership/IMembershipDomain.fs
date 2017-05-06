namespace CalendarSystem.Domain.Membership
open Rezoom
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open System

type IUserService =
    abstract member GetUserById
        : claim : Claim
        * user : User Id
        -> User Plan
    
    abstract member CreateUser
        : claim : AdminClaim
        * email : EmailAddress
        * name : string
        * role : Role
        -> User Id Plan

type IAuthenticationService =
    /// If you supply a username and password, you may be able get a valid claim and an estimate of
    /// when that claim will expire.
    abstract member Login
        : email : EmailAddress
        * password : InputPassword
        -> (Claim * DateTimeOffset) option Plan

    /// If you have a claim, you can get out the session ID and user that *you* are.

    // Note that with an autoincrementing integer Session Id,
    // this leaks information about how many other sessions have occured between each of your sessions.
    // If this is a problem (for example you let random people register accounts, and you don't want
    // your competitors to register and use this to see how much business you're doing) then
    // you could change the system to use Guids or something for session IDs. No matter what though, sessions *do* need
    // an ID separate from the token, because we want to expose that ID as part of `Occurence` without leaking session
    // tokens to anybody.

    abstract member Authenticate
        : claim : Claim
        -> (Session Id * User) Plan

    /// Super-users can impersonate other others, getting a claim that
    /// looks just like a claim for the user (but we track the fact that their session
    /// was impersonated).
    abstract member Impersonate
        : claim : SuperUserClaim
        * targetUserId : User Id
        -> (Claim * DateTimeOffset) Plan

type IMembershipDomain =
    abstract member Users : IUserService
    abstract member Authentication : IAuthenticationService
