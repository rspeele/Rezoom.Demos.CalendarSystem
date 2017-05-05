// For validation, we'll use a pattern where domain layer methods that *do stuff* in the system
// (e.g. create calendar event) return a Result<'a Plan, ValidationErrors> Plan.
// You run the first plan to find out whether you *can* do it, and if you can, you get the second plan
// that you can (but don't have to) run to *actually* do it.
namespace CalendarSystem.Common

type ValidationError =
    /// Something in general is wrong.
    | Invalid of msg : string
    /// A specific argument is wrong.
    | InvalidArgument of arg : string * msg : string

type ValidationErrors = ValidationError list

type ValidationResult<'a> = Result<'a, ValidationErrors>