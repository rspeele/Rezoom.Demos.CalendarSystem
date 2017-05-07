module CalendarSystem.Persistence.Impl.SQL.UserPersistence
open CalendarSystem.Persistence.Membership
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open Rezoom
open Rezoom.SQL
open Rezoom.SQL.Plans
open System.IO
open System

let dbOfRole role =
    match role with
    | SuperUser -> "SUPER", None
    | AdminUser -> "ADMIN", None
    | ConsultantUser -> "CONSULT", None
    | ClientUser (Id id) -> "CLIENT", Some id

let dbToRole str clientId =
    match str, clientId with
    | "SUPER", None -> SuperUser
    | "ADMIN", None -> AdminUser
    | "CONSULT", None -> ConsultantUser
    | "CLIENT", Some id -> ClientUser (Id id)
    | _ -> raise <| InvalidDataException("Invalid role storage")

let occurence sessionId dt =
    {   Who = Id sessionId
        When = dt
    }

type private CreateUserSQL = SQL<"""
    insert into Users row
        CreatedBy = @createdBy
        , Created = @created
        , UpdatedBy = @createdBy
        , Updated = @created
        , Email = @email
        , Name = @name
        , ClientId = @clientId
        , Role = @role
        , SetupToken = @setupToken
        ;
    select scope_identity() as id;
""">

let createUser (Id createdBy) (email : EmailAddress) (UserSetupToken setupToken) name role =
    plan {
        let roleString, clientId = dbOfRole role
        let cmd =
            CreateUserSQL.Command
                ( clientId = clientId
                , role = roleString
                , created = DateTimeOffset.UtcNow
                , createdBy = createdBy
                , email = string email
                , name = name
                , setupToken = Some setupToken
                )
        let! inserted = cmd.Scalar()
        return Id inserted
    }

type private GetUserByEmailSQL = SQL<"select * from Users where Email = @email">

let getUserByEmail (email : EmailAddress) =
    plan {
        let! found = GetUserByEmailSQL.Command(string email).TryExactlyOne()
        return
            found |> Option.map (fun row ->
                {   Id = Id row.Id
                    Name = row.Name
                    Email = EmailAddress.OfString(row.Email) |> assumeResultOk
                    Created = occurence row.CreatedBy row.Created
                    Updated = occurence row.UpdatedBy row.Updated
                    Role = dbToRole row.Role row.ClientId
                }, row.PasswordHash |> Option.map PasswordHash.verify)
    }

type private GetUserByIdSQL = SQL<"""
    select
        Id, Name, Email, Role, ClientId, CreatedBy, Created, UpdatedBy, Updated
    from Users
    where Id = @id
""">

let getUserById (Id id) =
    plan {
        let! row = GetUserByIdSQL.Command(id).ExactlyOne()
        return
            {   Id = Id row.Id
                Name = row.Name
                Email = EmailAddress.OfString(row.Email) |> assumeResultOk
                Created = occurence row.CreatedBy row.Created
                Updated = occurence row.UpdatedBy row.Updated
                Role = dbToRole row.Role row.ClientId
            }
    }

type private UpdateUserSQL = SQL<"""
    update Users set
        UpdatedBy = @updatedBy
        , Updated = @updated
        , Email = @email
        , Name = @name
    where Id = @id
""">

let updateUser (Id updatedBy) (Id updateUser) (email : EmailAddress) name =
    let cmd =
        UpdateUserSQL.Command
            ( id = updateUser
            , updatedBy = updatedBy
            , updated = DateTimeOffset.UtcNow
            , email = string email
            , name = name
            )
    cmd.Plan()

type private UpdatePasswordSQL = SQL<"""
    update Users set
        UpdatedBy = @updatedBy
        , Updated = @updated
        , PasswordHash = @hash
        , SetupToken = null
    where Id = @id
""">

let updatePassword (Id updatedBy) (Id updateUser) password =
    let hash = PasswordHash.generate password
    let cmd =
        UpdatePasswordSQL.Command
            (hash = Some hash, updated = DateTimeOffset.UtcNow, updatedBy = updatedBy, id = updateUser)
    cmd.Plan()

let userPersistence =
    { new IUserPersistence with
        member this.CreateUser(createdBy, email, setupToken, name, role) =
            createUser createdBy email setupToken name role
        member this.GetUserByEmail(email) =
            getUserByEmail email
        member this.GetUserById(id) =
            getUserById id
        member this.Update(updatedBy, updateUserId, email, name) =
            updateUser updatedBy updateUserId email name
        member this.UpdatePassword(updatedBy, updateUser, password) =
            updatePassword updatedBy updateUser password
    }

