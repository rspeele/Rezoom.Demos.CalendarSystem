module CalendarSystem.Persistence.Impl.SQL.Migrations
open Rezoom
open Rezoom.SQL
open Rezoom.SQL.Synchronous
open CalendarSystem.Persistence.Membership
open CalendarSystem.Persistence.Calendar
open System
open CalendarSystem.Model.Membership

type private Model = SQLModel<".">

type SeedCheckSQL = SQL<"select exists(select null as it from Users where Role = 'SUPER') as ThingsExist">
type SeedCreateSQL = SQL<"""
    insert into Users row
        Name = @name
        , Email = @email
        , PasswordHash = @hash
        , Role = 'SUPER'
        , Created = @now
        , Updated = @now
        ;
    insert into Sessions row
        UserId = scope_identity()
        , Token = @token
        , Created = @now
        , ValidTo = @now
        ;
""">

let private seed () =
    use context = new ConnectionContext()
    let thingsExist = SeedCheckSQL.Command().ExecuteScalar(context)
    if not thingsExist then
        let hash = PasswordHash.generate (InputPassword.OfString("changeMeFirstThingAfterLoggingIn") |> assumeResultOk)
        let sessionToken = SessionToken.Generate()
        let seedCmd =
            SeedCreateSQL.Command
                ( name = "root"
                , email = "root@example.com"
                , now = DateTimeOffset.UtcNow
                , token = string sessionToken
                , hash = Some hash
                )
        seedCmd.Execute(context)

let migrate () =
    let config =
        {   Migrations.MigrationConfig.AllowRetroactiveMigrations = false
            Migrations.MigrationConfig.LogMigrationRan = fun m -> printfn "Ran V%d: %s" m.MajorVersion m.MigrationName
        }
    Model.Migrate(config)
    seed ()


    