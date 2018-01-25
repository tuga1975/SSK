USE [UHPSimulatorII]
GO

/****** Object:  Table [dbo].[DrugResults]    Script Date: 16/12/2017 10:49:10 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DrugResults](
	[DrugResultsID] [nvarchar](50) NOT NULL,
	[NRIC] [nvarchar](50) NULL,
	[timestamp] [datetime] NULL,
	[markingnumber] [nvarchar](50) NULL,
	[AMPH] [bit] NULL,
	[BENZ] [bit] NULL,
	[OPI] [bit] NULL,
	[THC] [bit] NULL,
	[COCA] [bit] NULL,
	[BARB] [bit] NULL,
	[LSD] [bit] NULL,
	[METH] [bit] NULL,
	[MTQL] [bit] NULL,
	[PCP] [bit] NULL,
	[KET] [bit] NULL,
	[BUPRE] [bit] NULL,
	[CAT] [bit] NULL,
	[PPZ] [bit] NULL,
	[NPS] [bit] NULL,
 CONSTRAINT [PK_DrugResults] PRIMARY KEY CLUSTERED 
(
	[DrugResultsID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


