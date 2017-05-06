namespace CalendarSystem.Domain.Membership

type IMembershipDomain =
    abstract member Users : IUserService
    abstract member Authentication : IAuthenticationService
