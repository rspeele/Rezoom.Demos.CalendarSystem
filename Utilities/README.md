# Utilities

Defines utilities that are not specific to any particular domain.

That means the code in here could be useful to just about any F# project.
The logic I use for deciding whether to put something in Utilities is as simple as:

    1. Do I think it could've belonged in the standard library for .NET or F#?
        yes -> utilities
        no -> somewhere else