USE [LACL_Fish]
GO

ALTER TABLE [dbo].[Escapements] DROP CONSTRAINT [CK__Escapement__Hour__607251E5]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Escapements]') AND type in (N'U'))
DROP TABLE [dbo].[Escapements]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Escapements](
	[Location] [nvarchar](200) NOT NULL,
	[DateStamp] [date] NOT NULL,
	[Hour] [tinyint] NOT NULL,
	[LBank_Count] [int] NULL,
	[RBank_Count] [int] NULL,
	[Comments] [nvarchar](4000) NULL,
 CONSTRAINT [PK_Escapements] PRIMARY KEY CLUSTERED 
(
	[Location] ASC,
	[DateStamp] ASC,
	[Hour] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Escapements]  WITH CHECK ADD CHECK  (([Hour]>=(0) AND [Hour]<=(23)))
GO

