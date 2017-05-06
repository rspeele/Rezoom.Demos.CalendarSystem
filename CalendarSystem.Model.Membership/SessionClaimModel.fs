namespace CalendarSystem.Model.Membership
open System

module private SessionTokenGeneration =
    // Characters to use in session tokens.
    let legal =
        [|  yield! seq { 'a' .. 'z' }
            yield! seq { 'A' .. 'Z' }
            yield! seq { '0' .. '9' }
        |] |> String

/// Secure session token (random string generated for each session).
type SessionToken =
    private
    | SessionToken of string
    // A 12-character alphanumeric (case sensitive) string representing the session token.
    member this.TokenString = let (SessionToken str) = this in str
    // 12 is a good length: 62^12 is on the order of (1 trillion * 1 billion) possibilities,
    // so nobody's going to be guessing a valid session token.
    static member Generate() =
        SessionToken (SecureRandomString.ofLength 12 SessionTokenGeneration.legal)

type SuperUserClaim = SuperUserClaim of SessionToken
type AdminClaim = AdminClaim of SessionToken
type ConsultantClaim = ConsultantClaim of SessionToken
type ClientClaim = ClientClaim of SessionToken

/// A claim. This is what we get back from authenticating.
/// All claims are still checked by the domain layer code (which is why we let anybody construct them)
/// but they provide a layer of type safety as well, not for security but for self-documenting code,
/// that you can't try to call AdminService.DeleteEverything without passing it a ClaimAdmin.
type Claim =
    | ClaimingSuperUser of SuperUserClaim
    | ClaimingAdmin of AdminClaim
    | ClaimingConsultant of ConsultantClaim
    | ClaimingClient of ClientClaim
    member this.SessionToken =
        // might be better to compose and have a { Token : SessionToken; ClaimCase : ClaimCase }
        // but this way doesn't hurt too bad with just 4 cases
        match this with
        | ClaimingSuperUser (SuperUserClaim t)
        | ClaimingAdmin (AdminClaim t)
        | ClaimingConsultant (ConsultantClaim t)
        | ClaimingClient (ClientClaim t) -> t

