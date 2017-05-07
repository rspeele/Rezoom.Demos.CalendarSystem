module CalendarSystem.Persistence.Impl.SQL.SessionPersistence
open CalendarSystem.Persistence.Membership
open CalendarSystem.Model
open CalendarSystem.Model.Membership
open Rezoom
open Rezoom.SQL
open Rezoom.SQL.Plans
open System
open CalendarSystem.Persistence.Impl.SQL.Mapping

type private CreateSessionSQL = SQL<"""
    insert into Sessions row
        Token = @token
        , UserId = @userId
        , ImpersonatedBy = @impersonatedById
        , Created = @created
        , ValidTo = @validTo
        ;
    select scope_identity() as id;
""">

let private createSession (SessionToken token) impersonator (Id userId) sessionValidTo =
    plan {
        let cmd =
            CreateSessionSQL.Command
                ( created = DateTimeOffset.UtcNow
                , impersonatedById =
                    match impersonator with
                    | Some (Id id) -> Some id
                    | None -> None
                , userId = userId
                , validTo = sessionValidTo
                , token = token
                )
        let! id = cmd.Scalar()
        return Id id
    }

type private GetValidSessionByTokenSQL = SQL<"""
    select *, one User(u.*)
    from Sessions s
    join Users u on u.Id = s.UserId
    where s.Token = @token and s.ValidTo >= @now
""">

let private getValidSessionByToken (SessionToken token) =
    plan {
        let! row = GetValidSessionByTokenSQL.Command(token = token, now = DateTimeOffset.UtcNow).TryExactlyOne()
        return row |> Option.map (fun row ->
            let session =
                {   Id = Id row.Id
                    UserId = Id row.UserId
                    ImpersonatedBy = row.ImpersonatedBy |> Option.map Id
                    Token = SessionToken row.Token
                    Created = row.Created
                    ValidTo = row.ValidTo
                }
            let user =
                let user = row.User
                {   Id = Id user.Id
                    Name = user.Name
                    Email = EmailAddress.OfString(user.Email) |> assumeResultOk
                    Created = occurence (Option.get user.CreatedBy) user.Created
                    Updated = occurence (Option.get user.UpdatedBy) user.Updated
                    Role = dbToRole user.Role user.ClientId
                }
            (session, user))
    }

let sessionPersistence =
    { new ISessionPersistence with
        member this.CreateSession(sessionToken, impersonator, sessionUserId, sessionValidTo) =
            createSession sessionToken impersonator sessionUserId sessionValidTo
        member this.GetValidSessionByToken(token) =
            getValidSessionByToken token
    }