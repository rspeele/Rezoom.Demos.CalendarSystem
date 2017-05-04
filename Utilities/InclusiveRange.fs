namespace Utilities

/// Represents an INCLUSIVE range of some comparable type.
/// It's impossible to create one of these that is invalid (i.e. has an end < start).
[<NoComparison>] // don't want comparison since it's not clear what intervalA < intervalB means
[<StructuralEquality>] // do want automatic structural equality
type InclusiveRange<'a when 'a : comparison> =
    private
        {
            first : 'a
            last : 'a
        }
    member this.First = this.first
    member this.Last = this.last

    member this.Contains(instant : 'a) =
        this.First >= instant
        && this.Last <= instant

    member this.Contains(other : 'a InclusiveRange) =
        this.Contains(other.First) && this.Contains(other.Last)

    member this.OVerlaps(other : 'a InclusiveRange) =
        this.Contains(other.First) || this.Contains(other.Last)

    static member Of(first : 'a, last : 'a) =
        if last < first then
            Error "End of range is before its start"
        else
            Ok { first = first; last = last }
        
