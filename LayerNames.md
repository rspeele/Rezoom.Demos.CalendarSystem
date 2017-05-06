# The names I use and what I mean by them

# Layers

Layers refer to the top-to-bottom organization of the code, which goes something like this:

```
    Note: "a --> b" means "a references b"

         +--> Product Utility -->--+
         |                         |
         |           +-<--<--<--<--+
         |           |
         |           v
        Product --> Domain --> Persistence --> Common
             |          |               |      / / /
             |          |               +-->--+ / /
             |          |                      / /
             |          +-->-->-->-->-->-->-->+ /
             |                                 /
             +-->-->-->-->-->-->-->-->-->-->--+
```

## Common layer

Every other layer references this, because it defines the vocabulary of the system.

These are immutable data types. They don't know how to do things, they just *are* things.

F# makes these immutable data types easy to write with records and discriminated unions.

## Persistence layer

Exposes the interface for storing and retrieving data.

The *implementation* (not interface) of the domain layer references this, but nobody else should use it,
because it doesn't know about the rules of the system and therefore might let you store bogus data or
retrieve things you aren't supposed to know about.

## Domain layer

Exposes a "safe" interface to the system. In concept, an end user could write code against this interface
and not be able to do anything they're not supposed to (ignoring reflection tricks).

This means that the implementation of this layer enforces all the rules of the system, such as authentication,
authorization, and business rules like "can't Foozle the Bar on a Tuesday".

## Product layer

References the domain layer and uses it to provide a runnable program like a website, console app, or desktop app.

## Product utility layer

If you have multiple product layer implementations (for example, you support both a mobile app and a desktop app),
and you're repeating yourself composing domain layer operations in both apps, (for example, create an invoice then
load its data and the data of the associated client), then maybe you should move those helpers into this intermediate
layer so you can have both products reference it.

Obviously, a lot of solutions don't really need this layer at all, because they have only one product.

# Concerns

While layers describe the horizontal slices of the system, concerns refer to the vertical slices that shouldn't be
too tightly coupled.

For example, if I were writing a system for a car dealership, the part of the system that manages their inventory
of vehicles would be a separate concern from the part that manages the details of making a sale (tax, commission, etc).

The *implementation* of making a sale would need to reference the inventory concern's *interface* in order to tell it
that you no longer have the sold vehicle, but the interfaces don't need to reference one another, and the
implementations *definitely* shouldn't reference one another directly.

I think this vertical slicing usually either disappears or gets a lot less strict at the top and bottom of the system.

```

    Horizontal slice               | Vertical slices   |
    -------------------------------|-------------------|
    Product                        |     ...mix...     |
    Product utilities              |     ...mix...     |
    Domain interfaces              | Inventory | Sales |
    Domain implementation          | Inventory | Sales |
    Persistence interfaces         | Inventory | Sales |
    Persistence implementation     |     ...mix...     |

```

At the top, this is because the product's UI is driven by the customer's requirements the the customer might well
ask for a screen or button that cuts across what you consider to be different vertical slices at the domain layer.

At the bottom, it's because you usually need to tell your dtabase about the whole model to enforce relational integrity.

# Assembly naming conventions

Anything not related to the calendar system at all, that's just generally useful F# code, either
goes in a NuGet package we reference or in our own Utilities project.

For stuff coded for the calendar system, the general naming convention depends on whether the assembly
just contains interfaces/data types or interface implementations.

Assemblies that just contain interfaces and data types are named:

    CalendarSystem.<Layer>.<Concern>

    So for example:

    CalendarSystem.Common.Membership
        -- vocabulary of data types related to membership

    CalendarSystem.Domain.Membership
        -- interfaces for the part of the domain layer handling the membership concern

    CalendarSystem.Persistence.Membership
        -- interfaces for the part of the persistence layer handling the membership concern

Assemblies that contain implementation are named:

    CalendarSystem.<Layer>.<Concern>.Impl.HowItsDone

    So for example:

    CalendarSystem.Persistence.Membership.Impl.SQL
        -- implements the persistence API defined in CalendarSystem.Persistence.Membership using SQL

    CalendarSystem.Persistence.Membership.Impl.Files
        -- implements the persistence API defined in CalendarSystem.Persistence.Membership using flat files


If a single implementation assembly contains the implementation for a whole layer, it's named accordingly:

    CalendarSystem.Persistence.Impl.SQL
        -- implements the entire persistence layer API using a SQL

    This would make sense for SQL since we'd want to have foreign keys pointing across concerns.









