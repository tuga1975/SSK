USE [UHPSimulatorII]
GO

/****** Object:  Table [dbo].[Notifications]    Script Date: 16/12/2017 10:49:50 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Notifications](
	[NotificationID] [nvarchar](50) NOT NULL,
	[Source] [nvarchar](50) NULL,
	[Type] [nvarchar](50) NULL,
	[Content] [text] NULL,
	[Datetime] [datetime] NULL,
	[notification_code] [varchar](50) NULL,
 CONSTRAINT [PK_NotificationID] PRIMARY KEY CLUSTERED 
(
	[NotificationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


