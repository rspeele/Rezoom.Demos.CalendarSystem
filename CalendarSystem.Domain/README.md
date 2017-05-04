# CalendarSystem.Domain

Defines the API to the domain layer. This is what applications are supposed to use.

That is, you could write a web API on top of this, or an MVC app, or a desktop app, or whatever.

Every business rule is enforced by this code or further down the dependency line, so the public functions
exposed here are like Reese's cups: there's no wrong way to consume them.

I mean, obviously you could use this API to write an app that does something different from what the end user
*intended*, such as creating an event a week after the date range they requested, but you can't write one that does
something they *aren't supposed to be able* to do, such as a consultant deleting another consultant's data.

## Not interfaces

We're never going to make multiple implementations of these APIs, so they are classes, not interfaces.

There's no good reason to have multiple implementations of this code because the whole point of it is to enforce business
rules that *do not vary* regardless of your persistence layer.
