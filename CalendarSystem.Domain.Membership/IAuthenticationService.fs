namespace CalendarSystem.Domain.Membership
open Rezoom
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open System

type IAuthenticationService =
    abstract member Login
        : email : EmailAddress
        * password : InputPassword
        -> (SessionToken * DateTimeOffset) option Plan

    abstract member Authenticate
        : token : SessionToken
        -> AuthSession Plan