namespace Utilities

type LazyDependencyInjection<'iface>(wrapper : 'iface Lazy -> 'iface) =
    let mutable instance = None
    let lazyInstance : 'iface Lazy =
        lazy
            match instance with 
            | Some i -> i
            | None -> bug "Implementation was not set (call UseImplementation at program initialization)"
    member __.UseImplementation(impl) =
        match instance with
        | None -> instance <- Some impl
        | Some _ -> bug "Implementation should be set only once, at program initialization"

    member __.Instance =
        wrapper lazyInstance
    