module CalendarSystem.Persistence.Impl.SQL.PasswordHash
open BCrypt.Net
open CalendarSystem.Model.Membership

let verify hash (pass : InputPassword) =
    BCrypt.Verify(pass.Text, hash)

let generate (pass : InputPassword) =
    BCrypt.HashPassword(pass.Text)
