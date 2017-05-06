[<AutoOpen>]
module Utilities.Measures
open System

/// Convert from a decimal with a unit of measure to a float with the same unit of measure.
let toFloat (value: decimal<'a>) =
    LanguagePrimitives.FloatWithMeasure<'a>(float (decimal value))

/// Convert from a float with a unit of measure to a decimal with the same unit of measure.
let toDecimal (value : float<'a>) =
    LanguagePrimitives.DecimalWithMeasure<'a>(decimal (float value))

[<Measure>]
type dollars

[<Measure>]
type days =
    static member OfTimeSpan(ts : TimeSpan) =
        ts.TotalDays * 1.0<days>

[<Measure>]
type hours =
    static member OfTimeSpan(ts : TimeSpan) =
        ts.TotalHours * 1.0<hours>

[<Measure>]
type minutes =
    static member OfTimeSpan(ts : TimeSpan) =
        ts.TotalMinutes * 1.0<minutes>

[<Measure>]
type seconds =
    static member OfTimeSpan(ts : TimeSpan) =
        ts.TotalSeconds * 1.0<seconds>

[<Measure>]
type milliseconds =
    static member OfTimeSpan(ts : TimeSpan) =
        ts.TotalMilliseconds * 1.0<milliseconds>
    static member ToTimeSpan(ms : float<milliseconds>) =
        TimeSpan.FromMilliseconds(float ms)

let hoursPerDay = 24.0<hours> / 1.0<days>
let minutesPerHour = 60.0<minutes> / 1.0<hours>
let secondsPerMinute = 60.0<seconds> / 1.0<minutes>
let millisPerSecond = 1000.0<milliseconds> / 1.0<seconds>