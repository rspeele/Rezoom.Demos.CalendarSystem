namespace CalendarSystem.Persistence.Membership

type IMembershipPersistence =
    abstract member Users : IUserPersistence
    abstract member Sessions : ISessionPersistence