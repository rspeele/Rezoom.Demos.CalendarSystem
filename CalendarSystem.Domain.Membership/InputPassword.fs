namespace CalendarSystem.Domain.Membership

/// Represents a plain-text password we input.
/// We don't keep these around for long -- we certainly don't store them.
/// Using a wrapper keeps the password rules consistent.
type InputPassword =
    private
    | InputPassword of string
    static member OfString(str : string) =
        if str.Length < 8 then
            Error "Password must be at least 8 characters long"
        else
            Ok (InputPassword str)
    member this.Text =
        let (InputPassword str) = this in str