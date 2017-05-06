namespace CalendarSystem.Model
open System.Text.RegularExpressions

type EmailAddress =
    private
    | EmailAddress of string
    static member OfString(str : string) =
        let str = str.Trim() // we'll do this for them silently
        let maxLength = 254
        if str.Length > maxLength then
            Error "Email address is too long"
        // just enforce that we have an @ sign and "stuff" to the left and right of it
        elif Regex.IsMatch(str, @"^[-\w.]+@[-\w.]+") then
            Ok (EmailAddress str)
        else
            Error "Email address contains invalid characters or is missing '@' sign"
    override this.ToString() =
        let (EmailAddress str) = this in str
