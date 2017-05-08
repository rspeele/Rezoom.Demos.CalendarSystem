
# CalendarSystem.Product.MVCWebApplication.Standup

This assemblies exists to choose what implementations of the domain and persistence layer APIs
will be used to support the MVC web app. The point of splitting it out is to make sure that the MVC web app
doesn't reference the raw implementations, just the interfaces, and this assembly can swap in a different
implementation.