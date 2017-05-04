// Units of measure applicable to our domain.

namespace CalendarSystem.Common
open System

[<Measure>]
type dollars

[<Measure>]
type days =
    static member OfTimeSpan(ts : TimeSpan) =
        decimal ts.TotalDays * 1m<days>