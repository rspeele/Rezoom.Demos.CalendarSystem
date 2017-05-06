namespace CalendarSystem.Domain.Calendar

type ICalendarDomain =
    abstract member CalendarEvents : ICalendarEventService

