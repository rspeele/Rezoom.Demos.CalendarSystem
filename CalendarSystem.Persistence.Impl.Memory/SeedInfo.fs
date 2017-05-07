module CalendarSystem.Persistence.Impl.Memory.SeedInfo
open CalendarSystem.Model
open CalendarSystem.Model.Membership

let rootEmail =  EmailAddress.OfString("root@example.com") |> assumeResultOk
let rootPass = InputPassword.OfString("Root user password for testing purposes") |> assumeResultOk
