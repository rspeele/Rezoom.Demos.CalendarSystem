[<AutoOpen>]
module CalendarSystem.Persistence.SystemTasks.Implementation

let private di =
    LazyDependencyInjection(fun i ->
        { new ISystemTaskPersistence with
            member __.EnqueueTask(scheduleFor, task) = i.Value.EnqueueTask(scheduleFor, task)
        })

module Setup =
    let useSystemTaskPersistence i = di.UseImplementation(i)

let SystemTaskPersistence = di.Instance