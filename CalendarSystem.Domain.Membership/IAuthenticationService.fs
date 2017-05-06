namespace CalendarSystem.Domain.Membership
open Rezoom
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open System

type IAuthenticationService =
    abstract member Login
        : email : EmailAddress
        * password : InputPassword
        -> (Claim * DateTimeOffset) option Plan

    abstract member Authenticate
        : claim : Claim
        -> (Session Id * User) Plan