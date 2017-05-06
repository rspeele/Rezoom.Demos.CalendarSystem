namespace Utilities
open System

/// Represents an exception that should never be thrown.
/// If it's thrown, it means somebody screwed up and the program needs to be fixed.
type BugException(msg : string) =
    // If I had my way, this class would be in the framework already, and some exceptions
    // like NullReferenceException and IndexOutOfRangeException would inherit from it.
    // Others, like SecurityException and FileNotFoundException are *not* necessarily indicative of bugs
    // and are in fact expected to occur now and then in a correctly running system, so they would not inherit it.
    inherit Exception(msg)

[<AutoOpen>]
module BugUtilities =
    let inline bug msg = raise <| BugException(msg)