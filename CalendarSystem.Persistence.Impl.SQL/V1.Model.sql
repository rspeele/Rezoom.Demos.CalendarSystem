-- Membership

create table Clients
    ( Id int primary key autoincrement
    , Name string(128)
    );

create table Users
    ( Id int primary key autoincrement
    , Name string(128)
    , Email string(254) unique
    , SetupToken string(8) null
    , PasswordHash string(60) null
    , Role string(8) -- 'CLIENT', 'SUPER', 'CONSULT', 'ADMIN'
    , ClientId int null references Clients(Id)
    , Created datetimeoffset
    , Updated datetimeoffset
    );

create table Sessions
    ( Id int primary key autoincrement
    , UserId int references Users(Id)
    , Token string(12) unique
    , ImpersonatedBy int null references Sessions(Id)
    , Created datetimeoffset
    , ValidTo datetimeoffset
    );

create index IX_Sessions_ValidTo on Sessions(ValidTo);

alter table Users add column CreatedBy int references Sessions(Id);
alter table Users add column UpdatedBy int references Sessions(Id);

-- Calendar

create table CalendarEvents
    ( Id int primary key autoincrement
    , ClientId int references Clients(Id)
    , Created datetimeoffset
    , CreatedBy int references Sessions(Id)
    , Deleted datetimeoffset null
    , DeletedBy int null references Sessions(Id)
    );

create table CalendarEventVersions
    ( Id int primary key autoincrement
    , CalendarEventId int references CalendarEvents(Id)
    , Name string(128)
    , DurationFirst datetimeoffset
    , DurationLast datetimeoffset
    , Created datetimeoffset
    , CreatedBy int references Sessions(Id)
    , ConsultantId int references Users(Id)
    );