// Here's where we define the representation of entities in the system
// related to calendar events.

namespace rec CalendarSystem.Common
open System

/// A client can have many rates over the life of the system.
/// Only the most recent one is used, but historical data will still point
/// to the old rates. Therefore we NEVER edit a ClientRate, we only insert a newer one.
type ClientRate =
    {   Id : ClientRate Id
        ClientId : Client Id
        Rate : decimal<dollars/days>
        Created : Occurence
    }

// TODO: balance sheets and invoices