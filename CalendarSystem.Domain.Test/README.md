
# CalendarSystem.Domain.Test

This assembly defines tests for the domain layer of our calendar system.

It depends on CalendarSystem.Domain.Test.Standup to provide the implmeentations of interfaces.

It also depends on CalendarSystem.Persistence.SystemTasks.Processing in order to be able to test
some things about system tasks that must be scheduled as a result of the domain layer.

Other than that one detour into the persistence layer, you can look at the tests in this assembly as stuff
that an application could do (i.e. calling into the domain layer).