USE [SSK]
GO

/****** Object:  Table [dbo].[Membership_UserDevices]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Membership_UserDevices](
	[Device_ID] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NULL,
	[User_Device_ID] [nvarchar](192) NOT NULL,
	[Terminal] [varchar](50) NULL,
	[Active] [bit] NOT NULL,
	[Access_token] [nvarchar](700) NULL,
 CONSTRAINT [PK__Restaura__1907C64E07428A87] PRIMARY KEY CLUSTERED 
(
	[Device_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

/****** Object:  Table [dbo].[Membership_UserLogins]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Membership_UserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[IdentityUser_Id] [nvarchar](128) NULL,
 CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Membership_Roles]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Membership_Roles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Membership_UserClaims]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Membership_UserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
	[IdentityUser_Id] [nvarchar](128) NULL,
 CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Membership_UserRoles]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Membership_UserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
	[IdentityUser_Id] [nvarchar](128) NULL,
 CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[QueueNumbers]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[QueueNumbers](
	[ID] [uniqueidentifier] NOT NULL,
	[QueuedNumber] [varchar](10) NOT NULL,
	[Appointment_ID] [uniqueidentifier] NOT NULL,
	[CreatedTime] [datetime] NOT NULL,
	[Status] [varchar](20) NOT NULL,
 CONSTRAINT [PK_QueueNumber] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

/****** Object:  Table [dbo].[User_Profiles]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[User_Profiles](
	[UserId] [nvarchar](128) NOT NULL,
	[Primary_Phone] [varchar](50) NULL,
	[Secondary_Phone] [varchar](50) NULL,
	[Primary_Email] [nvarchar](255) NULL,
	[Secondary_Email] [nvarchar](255) NULL,
	[DOB] [date] NULL,
	[Nationality] [nchar](10) NULL,
	[Maritial_Status] [nvarchar](50) NULL,
	[Residential_Addess_ID] [int] NULL,
	[Other_Address_ID] [int] NULL,
	[NextOfKin_Name] [nvarchar](255) NULL,
	[NextOfKin_Contact_Number] [varchar](50) NULL,
	[NextOfKin_Relationship] [nvarchar](255) NULL,
	[NextOfKin_BlkHouse_Number] [varchar](50) NULL,
	[NextOfKin_FlrUnit_Number] [varchar](50) NULL,
	[NextOfKin_Street_Name] [nvarchar](255) NULL,
	[NextOfKin_Country] [nvarchar](50) NULL,
	[NextOfKin_PostalCode] [varchar](30) NULL,
	[Employment_Name] [nvarchar](255) NULL,
	[Employment_Contact_Number] [nvarchar](50) NULL,
	[Employment_Company_Name] [nvarchar](255) NULL,
	[Employment_Job_Title] [nvarchar](50) NULL,
	[Employment_Start_Date] [date] NULL,
	[Employment_End_Date] [date] NULL,
	[Employment_Remarks] [nvarchar](255) NULL,
 CONSTRAINT [PK_User_Profile] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

/****** Object:  Table [dbo].[Membership_Users]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Membership_Users](
	[UserId] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
	[Discriminator] [nvarchar](128) NULL,
	[NRIC] [nvarchar](20) NULL,
	[Name] [nvarchar](128) NULL,
	[SmartCardId] [nvarchar](50) NULL,
	[Fingerprint] [varbinary](max) NULL,
	[Status] [varchar](20) NULL,
 CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

/****** Object:  Table [dbo].[Notifications]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Notifications](
	[ID] [uniqueidentifier] NOT NULL,
	[Date] [datetime] NOT NULL,
	[FromUserId] [nvarchar](128) NULL,
	[ToUserId] [nvarchar](128) NULL,
	[Subject] [nvarchar](100) NULL,
	[Content] [nvarchar](500) NOT NULL,
	[IsRead] [bit] NOT NULL,
	[IsFromSupervisee] [bit] NULL,
 CONSTRAINT [PK_Notifications_1] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Users_deleted]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Users_deleted](
	[UserId] [nvarchar](128) NOT NULL,
	[NRIC] [nvarchar](20) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[LastLoginTime] [datetime] NULL,
	[SmartCardId] [nvarchar](50) NOT NULL,
	[EnrolledDate] [datetime] NULL,
	[Role] [smallint] NOT NULL,
	[Fingerprint] [varbinary](max) NULL,
	[Status] [varchar](20) NULL,
 CONSTRAINT [PK_SSK_Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

/****** Object:  Table [dbo].[Addresses]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Addresses](
	[Address_ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NULL,
	[BlkHouse_Number] [varchar](50) NULL,
	[FlrUnit_Number] [varchar](50) NULL,
	[Street_Name] [varchar](255) NULL,
	[Country] [nvarchar](50) NULL,
	[Postal_Code] [varchar](30) NULL,
 CONSTRAINT [PK_Addresses] PRIMARY KEY CLUSTERED 
(
	[Address_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

/****** Object:  Table [dbo].[ApplicationDevice_Status]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[ApplicationDevice_Status](
	[ID] [uniqueidentifier] NOT NULL,
	[ApplicationType] [varchar](10) NOT NULL,
	[DeviceID] [int] NULL,
	[StatusCode] [int] NOT NULL,
	[StatusMessage] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_ApplicationDevice_Status_1] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

/****** Object:  Table [dbo].[Membership_RoleClaims]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Membership_RoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[AbsenceReporting]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[AbsenceReporting](
	[ID] [uniqueidentifier] NOT NULL,
	[ReportingDate] [datetime] NOT NULL,
	[AbsenceReason] [smallint] NOT NULL,
	[ReasonDetails] [nvarchar](500) NULL,
	[ScannedDocument] [varbinary](max) NULL,
 CONSTRAINT [PK_AbsenceReporting] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

/****** Object:  Table [dbo].[Environment]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Environment](
	[DateOfWeek] [int] NOT NULL,
	[Frequency] [varchar](50) NOT NULL,
	[StartTime] [time](4) NOT NULL,
	[EndTime] [time](4) NOT NULL,
	[Duration] [int] NOT NULL,
 CONSTRAINT [PK_Environment] PRIMARY KEY CLUSTERED 
(
	[DateOfWeek] ASC,
	[Frequency] ASC,
	[StartTime] ASC,
	[EndTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

/****** Object:  Table [dbo].[Holidays]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Holidays](
	[Holiday] [date] NOT NULL,
	[IsSingHoliday] [bit] NOT NULL,
	[IsMalayHoliday] [bit] NULL,
 CONSTRAINT [PK_Holidays] PRIMARY KEY CLUSTERED 
(
	[Holiday] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Appointments]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Appointments](
	[ID] [uniqueidentifier] NOT NULL,
	[AbsenceReporting_ID] [uniqueidentifier] NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[Date] [date] NOT NULL,
	[FromTime] [time](4) NULL,
	[ToTime] [time](4) NULL,
	[ChangedCount] [smallint] NOT NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_Appointments_1] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Devices]    Script Date: 12/27/2017 5:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Devices](
	[DeviceID] [int] NOT NULL,
	[DeviceName] [nvarchar](50) NULL,
	[Details] [nvarchar](255) NULL,
 CONSTRAINT [PK_Devices] PRIMARY KEY CLUSTERED 
(
	[DeviceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Membership_UserDevices]  WITH CHECK ADD  CONSTRAINT [FK__Restauran__UserI__1CFC3D38] FOREIGN KEY([UserId])
REFERENCES [dbo].[Membership_Users] ([UserId])
GO

ALTER TABLE [dbo].[Membership_UserDevices] CHECK CONSTRAINT [FK__Restauran__UserI__1CFC3D38]
GO

ALTER TABLE [dbo].[Membership_UserLogins]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Membership_Users] ([UserId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Membership_UserLogins] CHECK CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]
GO

ALTER TABLE [dbo].[Membership_UserClaims]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Membership_Users] ([UserId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Membership_UserClaims] CHECK CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]
GO

ALTER TABLE [dbo].[Membership_UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Membership_Roles] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Membership_UserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]
GO

ALTER TABLE [dbo].[Membership_UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Membership_Users] ([UserId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Membership_UserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]
GO

ALTER TABLE [dbo].[QueueNumbers]  WITH CHECK ADD  CONSTRAINT [FK_QueueNumbers_Appointments1] FOREIGN KEY([Appointment_ID])
REFERENCES [dbo].[Appointments] ([ID])
GO

ALTER TABLE [dbo].[QueueNumbers] CHECK CONSTRAINT [FK_QueueNumbers_Appointments1]
GO

ALTER TABLE [dbo].[User_Profiles]  WITH CHECK ADD  CONSTRAINT [FK_User_Profiles_Addresses] FOREIGN KEY([Residential_Addess_ID])
REFERENCES [dbo].[Addresses] ([Address_ID])
GO

ALTER TABLE [dbo].[User_Profiles] CHECK CONSTRAINT [FK_User_Profiles_Addresses]
GO

ALTER TABLE [dbo].[User_Profiles]  WITH CHECK ADD  CONSTRAINT [FK_User_Profiles_Addresses1] FOREIGN KEY([Other_Address_ID])
REFERENCES [dbo].[Addresses] ([Address_ID])
GO

ALTER TABLE [dbo].[User_Profiles] CHECK CONSTRAINT [FK_User_Profiles_Addresses1]
GO

ALTER TABLE [dbo].[User_Profiles]  WITH CHECK ADD  CONSTRAINT [FK_User_Profiles_Membership_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Membership_Users] ([UserId])
GO

ALTER TABLE [dbo].[User_Profiles] CHECK CONSTRAINT [FK_User_Profiles_Membership_Users]
GO

ALTER TABLE [dbo].[User_Profiles]  WITH CHECK ADD  CONSTRAINT [FK_User_Profiles_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users_deleted] ([UserId])
GO

ALTER TABLE [dbo].[User_Profiles] CHECK CONSTRAINT [FK_User_Profiles_Users]
GO

ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [FK_Notifications_Membership_Users] FOREIGN KEY([ToUserId])
REFERENCES [dbo].[Membership_Users] ([UserId])
GO

ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_Membership_Users]
GO

ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [FK_Notifications_Membership_Users1] FOREIGN KEY([FromUserId])
REFERENCES [dbo].[Membership_Users] ([UserId])
GO

ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_Membership_Users1]
GO

ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [FK_Notifications_Users] FOREIGN KEY([FromUserId])
REFERENCES [dbo].[Users_deleted] ([UserId])
GO

ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_Users]
GO

ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [FK_Notifications_Users1] FOREIGN KEY([FromUserId])
REFERENCES [dbo].[Users_deleted] ([UserId])
GO

ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_Users1]
GO

ALTER TABLE [dbo].[Membership_RoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_Membership_RoleClaims_Membership_Roles] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Membership_Roles] ([Id])
GO

ALTER TABLE [dbo].[Membership_RoleClaims] CHECK CONSTRAINT [FK_Membership_RoleClaims_Membership_Roles]
GO

ALTER TABLE [dbo].[Appointments]  WITH CHECK ADD  CONSTRAINT [FK_Appointments_AbsenceReporting] FOREIGN KEY([AbsenceReporting_ID])
REFERENCES [dbo].[AbsenceReporting] ([ID])
GO

ALTER TABLE [dbo].[Appointments] CHECK CONSTRAINT [FK_Appointments_AbsenceReporting]
GO

ALTER TABLE [dbo].[Appointments]  WITH CHECK ADD  CONSTRAINT [FK_Appointments_Membership_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Membership_Users] ([UserId])
GO

ALTER TABLE [dbo].[Appointments] CHECK CONSTRAINT [FK_Appointments_Membership_Users]
GO

ALTER TABLE [dbo].[Appointments]  WITH CHECK ADD  CONSTRAINT [FK_Appointments_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users_deleted] ([UserId])
GO

ALTER TABLE [dbo].[Appointments] CHECK CONSTRAINT [FK_Appointments_Users]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID of the Restaurant which the user is currently logged-in' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Membership_Users', @level2type=N'COLUMN',@level2name=N'NRIC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0: Duty Officer
1: Supervisee' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Users_deleted', @level2type=N'COLUMN',@level2name=N'Role'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SSK: SSK Application
SSA: SSA Application
ODA: Officer''s Desktop Application
ES: Enrollment Station' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationDevice_Status', @level2type=N'COLUMN',@level2name=N'ApplicationType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0:Medical Certificate (MC)
1:Work Commitment
2: Family Matters
3:Other Reasons
4:No Valid Reason
5:No Supporting Document' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AbsenceReporting', @level2type=N'COLUMN',@level2name=N'AbsenceReason'
GO


