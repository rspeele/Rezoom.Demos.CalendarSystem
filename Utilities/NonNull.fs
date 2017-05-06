[<AutoOpen>]
module Utilities.NonNull
open System

// Although the compiler tries to help prevent us from creating null values of types defined in F#,
// unfortunately they're still .NET types at their core and you can always create a null with Unchecked.defaultof<_>.
// Most of the time we just consider that a harmless bug -- if you pass Unchecked.defaultof<_>, you are misusing
// the system and you'll probably get the NullReferenceException you deserve.

// However, sometimes we use types for security, like if you have an AdminAuthSession that means you were able
// to get it from privileged code, so we believe that you're an admin. In this case we have to null check it early
// because we might not actually be using the value, just trying to use your having the thing to prove something about
// you.

// This active pattern makes it slightly easier to write that null check in the method signature.
// Instead of:
//     member this.DoSomething(session, arg) =
//         if isNull arg then bug "blah blah" else
//         ...
// You write:
//     member this.DoSomething(NonNull session, arg) =
//         ...

/// Passes its argument through untouched, but asserting that it's non-null.
let (|NonNull|) (x : 'a) =
    match box x with
    | null ->
        raise <| ArgumentNullException(sprintf "An argument of type %s is expected to never be null" typeof<'a>.Name)
    | _ -> x

