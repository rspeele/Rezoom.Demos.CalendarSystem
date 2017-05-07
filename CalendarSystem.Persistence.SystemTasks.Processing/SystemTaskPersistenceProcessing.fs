[<AutoOpen>]
module CalendarSystem.Persistence.SystemTasks.Processing.Implementation

let private di =
    LazyDependencyInjection(fun i ->
        { new ISystemTaskPersistenceProcessing with
            member __.DequeueTask(forProcessingBy, filterToType) = i.Value.DequeueTask(forProcessingBy, filterToType)
            member __.CompleteTask(taskId, completion) = i.Value.CompleteTask(taskId, completion)
        })

module Setup =
    let useSystemTaskPersistenceProcessing i = di.UseImplementation(i)

let SystemTaskPersistenceProcessing = di.Instance