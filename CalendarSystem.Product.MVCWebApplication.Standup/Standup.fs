module CalendarSystem.Product.MVCWebApplication.Standup.Standup
open System.Threading.Tasks
open Rezoom

let private installed =
    lazy
        CalendarSystem.Persistence.Impl.SQL.Install.install()
        CalendarSystem.Domain.Calendar.Impl.Server.Install.install()
        CalendarSystem.Domain.Membership.Impl.Server.Install.install()

[<CompiledName("Migrate")>]
let migrate () =
    installed.Force()
    CalendarSystem.Persistence.Impl.SQL.Migrations.migrate()

[<CompiledName("RunPlan")>]
let runPlan (plan : 'a Plan) =
    installed.Force()
    Execution.execute Execution.ExecutionConfig.Default plan