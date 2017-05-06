namespace CalendarSystem.Domain.Membership
open Rezoom
open CalendarSystem.Model
open CalendarSystem.Model.Membership

type IUserService =
    abstract member GetUserById
        : session : AuthSession
        * user : User Id
        -> User Plan
    
    abstract member CreateUser
        : session : AdminAuthSession
        * email : EmailAddress
        * name : string
        * role : Role
        -> User Id Plan