## Persistence layer

This layer has its interfaces defined in assemblies named CalendarSystem.Persistence.XXX,
and its implementation defined in CalendarSystem.Persistence.Impl.SQL and CalendarSystem.Persistence.Impl.Memory.

The job of the persistence layer is to store and retrieve data in some way that outlives the running process.

This API is *not* a safe way to interact with the system, because it does not enforce almost any business rules.
It's just a way to stuff data into some unknown place and read it back out in the future.

For this reason it should be directly referenced *only* by the implementation of the domain layer.

This may include interfaces and DTO-style bag-of-data types that get passed into and out of those interfaces.

This does NOT include any implementation of the interfaces.
We'll actually have two implementations:

1. CalandarSystem.Persistence.Impl.SQL that stores data to a SQL database
2. CalendarSystem.Persistence.Impl.Memory that fakes it by manipulating objects in memory (not really persisted!)

The first one is what we'll use in reality, while the second one makes it possible to test our higher-level
domain logic without standing up a test database and restoring a backup prior to each test run.

Perhaps in the future we would want to use some other storage technology like a NoSQL database,
in which case we could create another implementation for that. I've never seen this happen to a real system though.

## A note about enforcing business rules

Sometimes the power of the data storage technology we're using is too good to pass up.

Rules like "two users cannot have the same email address" are best enforced using a unique index in a SQL database,
for example. If you try to enforce that up in the domain layer, you'll have a hell of a time dealing with race conditions
when two users set their email to "a@example.com" at the same time.

Row level security could also be useful to set some rough fail-safe visibility boundaries, especially in a multi-tenant
application.

So, don't religiously *avoid* enforcing business rules in the persistence layer, because sometimes it's the place
most capable of doing that enforcement. But don't do it when it _could_ be done equally well in the domain layer.

Why not?

Well, if you have multiple implementations of the persistence layer (we plan on having at least 2), each one needs
to enforce the same set of business rules. If you don't keep that set of rules small, you'll almost certainly
fail to make that enforcement consistent across all your persistence implementations. Then your unit tests become
close to worthless.