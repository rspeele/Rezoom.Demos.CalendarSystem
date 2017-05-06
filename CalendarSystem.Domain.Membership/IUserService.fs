namespace CalendarSystem.Domain.Membership
open Rezoom
open CalendarSystem.Model
open CalendarSystem.Model.Membership

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