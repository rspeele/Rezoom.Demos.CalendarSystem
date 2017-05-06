module private CalendarSystem.Persistence.Impl.Memory.ResetPerPlan
open Rezoom

type private Reset() =
    do
        // reset storage when constructed (once per execution)
        Storage.nuke()
        Storage.seed()

type private RequireResetAtExecutionStartErrand() =
    inherit SynchronousErrand<unit>()
    let identity = box typeof<RequireResetAtExecutionStartErrand>
    let category = box typeof<RequireResetAtExecutionStartErrand>
    let cacheInfo =
        { new CacheInfo() with
            override __.Cacheable = false
            override __.Category = category
            override __.Identity = identity
        }
    override __.CacheInfo = cacheInfo
    override __.Prepare(serviceContext) =
        ignore <| serviceContext.GetService<ExecutionLocal<Reset>, _>()
        fun () -> ()

let requireReset = Plan.ofErrand (RequireResetAtExecutionStartErrand())