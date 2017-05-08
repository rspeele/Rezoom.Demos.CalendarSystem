# Calendar system demo app

This is intended to illustrate how I'd architect a web application using my library,
[Rezoom](https://github.com/rspeele/Rezoom), if I was going for maximum separation of concerns.

[Architecture.md](Architecture.md) provided an overview of the architecture.

Additionally, each project folder contains its own README describing the purpose of that project.

If your initial reaction is:

> Whoa, that is 19 projects to do almost nothing!

Then I empathize with you. It could of course be reduced down to one project.
More reasonably, if you ditch the separation between "vertical" concerns like
membership vs calendar vs system tasks, it could be as simple as:

* CalendarSystem.Product.MVCWebApplication
* CalendarSystem.Domain
* CalendarSystem.Persistence
* CalendarSystem.Model
* CalendarSystem.Test

I figured you can always take stuff from multiple assemblies and mush it into one.
So, for this demo, my goal is to show how much you *can* break it out if you want.

Read the README within each project's folder to find out what purpose that project serves.