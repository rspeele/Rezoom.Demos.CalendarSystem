module CalendarSystem.Persistence.Impl.Memory.SeedInfo
open CalendarSystem.Model

let rootEmail =  EmailAddress.OfString("root@example.com") |> assumeResultOk
let rootPass = "Root user password for testing purposes"
