USE [Fish_Tagging]
GO
CREATE USER [NPS\Domain Users] FOR LOGIN [NPS\Domain Users]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LookupRadioManufacturers](
	[RadioManufacturer] [varchar](16) NOT NULL,
	[Name] [nvarchar](200) NULL,
	[Website] [nvarchar](200) NULL,
	[Description] [nvarchar](2000) NULL,
 CONSTRAINT [PK_LookupRadioManufacturers] PRIMARY KEY CLUSTERED 
(
	[RadioManufacturer] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
CREATE PROCEDURE [dbo].[ProcessRawDataFile]
	@fileId [int]
AS
EXTERNAL NAME [SqlServer_Files].[SqlServer_Files.RawDataFileInfo].[ProcessRawDataFile]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LookupFileFormats](
	[Code] [char](1) NOT NULL,
	[RadioManufacturer] [varchar](16) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[Header] [nvarchar](450) NOT NULL,
	[Regex] [nvarchar](450) NULL,
 CONSTRAINT [PK_LookupFileFormats] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
CREATE FUNCTION [dbo].[LocalTime](@utcDateTime [datetime])
RETURNS [datetime] WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SqlServer_Functions].[SqlServer_Functions.SimpleFunctions].[LocalTime]
GO
CREATE FUNCTION [dbo].[DateTimeFromAtsWithSeconds](@year [int], @days [int], @hours [int], @minutes [int], @seconds [int])
RETURNS [datetime] WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SqlServer_Functions].[SqlServer_Functions.SimpleFunctions].[DateTimeFromAtsWithSeconds]
GO
CREATE FUNCTION [dbo].[DateTimeFromAts](@year [int], @days [int], @hours [int], @minutes [int])
RETURNS [datetime] WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SqlServer_Functions].[SqlServer_Functions.SimpleFunctions].[DateTimeFromAts]
GO
CREATE FUNCTION [dbo].[UtcTime](@localDateTime [datetime])
RETURNS [datetime] WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SqlServer_Functions].[SqlServer_Functions.SimpleFunctions].[UtcTime]
GO
CREATE FUNCTION [dbo].[Sha1Hash](@data [varbinary](max))
RETURNS [varbinary](8000) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SqlServer_Functions].[SqlServer_Functions.SimpleFunctions].[Sha1Hash]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RawDataFiles](
	[FileId] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[FileName] [nvarchar](255) NOT NULL,
	[FolderName] [nvarchar](1000) NULL,
	[UploadDate] [datetime2](7) NOT NULL,
	[UploaderLogin] [sysname] NOT NULL,
	[Contents] [varbinary](max) NOT NULL,
	[ProcessingErrors] [varchar](8000) NULL,
	[ProcessingDone] [bit] NOT NULL,
	[Sha1Hash]  AS ([dbo].[Sha1Hash]([Contents])) PERSISTED,
 CONSTRAINT [PK_RawDataFiles] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TelemetryDataSRX400TrackingData](
	[FileId] [int] NOT NULL,
	[LineNumber] [int] NOT NULL,
	[Date] [datetime2](7) NOT NULL,
	[Channel] [int] NOT NULL,
	[Code] [int] NOT NULL,
	[Antenna] [nvarchar](50) NOT NULL,
	[Power] [int] NOT NULL,
	[Data] [nvarchar](50) NULL,
	[Sensor] [nvarchar](50) NULL,
	[Events] [int] NOT NULL,
	[Latitude] [real] NULL,
	[Longitude] [real] NULL,
	[StopDate] [datetime2](7) NULL,
 CONSTRAINT [PK_TelemetryDataSRX400TrackingData] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC,
	[LineNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TelemetryDataSRX400Locations](
	[FileId] [int] NOT NULL,
	[LineNumber] [int] NOT NULL,
	[ChangeDate] [datetime2](7) NULL,
	[Latitude] [real] NULL,
	[Longitude] [real] NULL,
 CONSTRAINT [PK_TelemetryDataSRX400Locations] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC,
	[LineNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TelemetryDataSRX400Filters](
	[FileId] [int] NOT NULL,
	[LineNumber] [int] NOT NULL,
	[ChangeDate] [datetime2](7) NOT NULL,
	[Channel] [int] NOT NULL,
	[Code] [int] NOT NULL,
 CONSTRAINT [PK_TelemetryDataSRX400Filters] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC,
	[LineNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TelemetryDataSRX400Environments](
	[FileId] [int] NOT NULL,
	[LineNumber] [int] NOT NULL,
	[ChangeDate] [datetime2](7) NOT NULL,
	[Site] [nvarchar](50) NULL,
	[Memory] [nvarchar](50) NULL,
	[CodeSet] [nvarchar](50) NULL,
	[AntennaMaster] [nvarchar](50) NULL,
	[AntennaGain] [nvarchar](50) NULL,
	[AntennaPriorityScanState] [nvarchar](50) NULL,
	[HydrophoneMaster] [nvarchar](50) NULL,
	[HydrophoneGain] [nvarchar](50) NULL,
	[HydrophonePriorityScanState] [nvarchar](50) NULL,
	[AGC] [nvarchar](50) NULL,
	[ScanTime] [nvarchar](50) NULL,
	[ScanDelay] [nvarchar](50) NULL,
	[ActivePartition] [nvarchar](50) NULL,
	[ContinuousRecordTimeout] [nvarchar](50) NULL,
	[NoiseBlankLevel] [nvarchar](50) NULL,
	[UpconverterBaseFrequency] [nvarchar](50) NULL,
	[Filter] [nvarchar](50) NULL,
	[EchoDelay] [nvarchar](50) NULL,
	[GpsMode] [nvarchar](50) NULL,
 CONSTRAINT [PK_TelemetryDataSRX400Environments] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC,
	[LineNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TelemetryDataSRX400Channels](
	[FileId] [int] NOT NULL,
	[LineNumber] [int] NOT NULL,
	[ChangeDate] [datetime2](7) NOT NULL,
	[Channel] [int] NOT NULL,
	[Frequency] [real] NOT NULL,
 CONSTRAINT [PK_TelemetryDataSRX400Channels] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC,
	[LineNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TelemetryDataSRX400BatteryStatus](
	[FileId] [int] NOT NULL,
	[LineNumber] [int] NOT NULL,
	[timeStamp] [datetime2](7) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_TelemetryDataSRX400BatteryStatus] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC,
	[LineNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TelemetryDataSRX400Antennas](
	[FileId] [int] NOT NULL,
	[LineNumber] [int] NOT NULL,
	[ChangeDate] [datetime2](7) NOT NULL,
	[DeviceType] [nvarchar](50) NOT NULL,
	[DeviceId] [int] NOT NULL,
	[Gain] [int] NOT NULL,
 CONSTRAINT [PK_TelemetryDataSRX400Antennas] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC,
	[LineNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TelemetryDataATSTracking](
	[FileId] [int] NOT NULL,
	[LineNumber] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[Day] [int] NOT NULL,
	[Hour] [int] NOT NULL,
	[Minute] [int] NOT NULL,
	[Second] [int] NOT NULL,
	[Frequency] [int] NULL,
	[TagNumberAndMortality] [int] NULL,
	[SignalStrength] [int] NULL,
	[Latitude] [real] NULL,
	[Longitude] [real] NULL,
	[UtmX] [real] NULL,
	[UtmY] [real] NULL,
	[UtmZone] [varchar](50) NULL,
	[GpsAge] [int] NOT NULL,
 CONSTRAINT [PK_TelemetryDataATSTracking] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC,
	[LineNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TelemetryDataATSStationary](
	[FileId] [int] NOT NULL,
	[LineNumber] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[Day] [int] NOT NULL,
	[Hour] [int] NOT NULL,
	[Minute] [int] NOT NULL,
	[Antenna] [int] NOT NULL,
	[Frequency] [int] NOT NULL,
	[TagNumberAndMortality] [int] NOT NULL,
	[SignalStrength] [int] NOT NULL,
	[DuplicateCount] [int] NOT NULL,
 CONSTRAINT [PK_TelemetryDataATSStationary] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC,
	[LineNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Regan Sarwas
-- Create date: August 21, 2013
-- Description:	Adds a new raw data file to the database.
-- =============================================
CREATE PROCEDURE [dbo].[RawDataFile_Insert] 
    @FileName NVARCHAR(255),
    @FolderName NVARCHAR(1000),
    @Contents VARBINARY(max),
    @FileId INT OUTPUT,
    --@Format CHAR OUTPUT,
    @UploadDate DATETIME2(7) OUTPUT,
    @UploaderLogin NVARCHAR(128) OUTPUT
    --@Sha1Hash VARBINARY(8000) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get the name of the caller
    DECLARE @Caller SYSNAME = ORIGINAL_LOGIN();

    --Fix Text Inputs
    SET @FileName           = NULLIF(@FileName,'')
    SET @FolderName         = NULLIF(@FolderName,'')

    -- Fix the defaults

    -- Validate permission for this operation

    -- Get the format of the file, and determine the processing required

    -- Do the update, check the trigger for integrity checks
    INSERT INTO dbo.RawDataFiles ([FileName], [FolderName], [Contents])
                          VALUES (@FileName,  @FolderName,  @Contents)
    SET @FileId = SCOPE_IDENTITY();
    --SELECT @UploadDate = [UploadDate], @UploaderLogin = [UploaderLogin], @Sha1Hash = [Sha1Hash], @Format = [Format] FROM dbo.RawDataFiles WHERE FileId = @FileId
    SELECT @UploadDate = [UploadDate], @UploaderLogin = [UploaderLogin] FROM dbo.RawDataFiles WHERE FileId = @FileId
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Regan Sarwas
-- Create date: August 21, 2013
-- Description: Deletes a RawDataFile
-- =============================================
CREATE PROCEDURE [dbo].[RawDataFile_Delete] 
    @FileId int
AS
BEGIN
    SET NOCOUNT ON;

    -- Get the name of the caller
    DECLARE @Caller SYSNAME = ORIGINAL_LOGIN();

    -- First check that this is a valid file, if not exit quietly (we are done)
    IF NOT EXISTS (SELECT 1 FROM [dbo].[RawDataFiles] WHERE [FileId] = @FileId)
    BEGIN
        RETURN 0
    END

    -- Validate permission for this operation
    -- Do not check the uploader. i.e. Do not allow someone who may have lost their privileges to remove a file.

    -- Delete this file
    DELETE FROM [dbo].[RawDataFiles] WHERE [FileId] = @FileId;

END
GO
ALTER TABLE [dbo].[RawDataFiles] ADD  CONSTRAINT [DF_RawDataFiles_UploadDate]  DEFAULT (getdate()) FOR [UploadDate]
GO
ALTER TABLE [dbo].[RawDataFiles] ADD  CONSTRAINT [DF_RawDataFiles_UploaderLogin]  DEFAULT (original_login()) FOR [UploaderLogin]
GO
ALTER TABLE [dbo].[RawDataFiles] ADD  CONSTRAINT [DF_RawDataFiles_ProcessingDone]  DEFAULT ((0)) FOR [ProcessingDone]
GO
ALTER TABLE [dbo].[LookupFileFormats]  WITH CHECK ADD  CONSTRAINT [FK_LookupFileFormats_LookupRadioManufacturer] FOREIGN KEY([RadioManufacturer])
REFERENCES [dbo].[LookupRadioManufacturers] ([RadioManufacturer])
GO
ALTER TABLE [dbo].[LookupFileFormats] CHECK CONSTRAINT [FK_LookupFileFormats_LookupRadioManufacturer]
GO
ALTER TABLE [dbo].[TelemetryDataATSStationary]  WITH CHECK ADD  CONSTRAINT [FK_TelemetryDataATSStationary_RawDataFiles] FOREIGN KEY([FileId])
REFERENCES [dbo].[RawDataFiles] ([FileId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TelemetryDataATSStationary] CHECK CONSTRAINT [FK_TelemetryDataATSStationary_RawDataFiles]
GO
ALTER TABLE [dbo].[TelemetryDataATSTracking]  WITH CHECK ADD  CONSTRAINT [FK_TelemetryDataATSTracking_RawDataFiles] FOREIGN KEY([FileId])
REFERENCES [dbo].[RawDataFiles] ([FileId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TelemetryDataATSTracking] CHECK CONSTRAINT [FK_TelemetryDataATSTracking_RawDataFiles]
GO
ALTER TABLE [dbo].[TelemetryDataSRX400Antennas]  WITH CHECK ADD  CONSTRAINT [FK_TelemetryDataSRX400Antennas_RawDataFiles] FOREIGN KEY([FileId])
REFERENCES [dbo].[RawDataFiles] ([FileId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TelemetryDataSRX400Antennas] CHECK CONSTRAINT [FK_TelemetryDataSRX400Antennas_RawDataFiles]
GO
ALTER TABLE [dbo].[TelemetryDataSRX400BatteryStatus]  WITH CHECK ADD  CONSTRAINT [FK_TelemetryDataSRX400BatteryStatus_RawDataFiles] FOREIGN KEY([FileId])
REFERENCES [dbo].[RawDataFiles] ([FileId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TelemetryDataSRX400BatteryStatus] CHECK CONSTRAINT [FK_TelemetryDataSRX400BatteryStatus_RawDataFiles]
GO
ALTER TABLE [dbo].[TelemetryDataSRX400Channels]  WITH CHECK ADD  CONSTRAINT [FK_TelemetryDataSRX400Channels_RawDataFiles] FOREIGN KEY([FileId])
REFERENCES [dbo].[RawDataFiles] ([FileId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TelemetryDataSRX400Channels] CHECK CONSTRAINT [FK_TelemetryDataSRX400Channels_RawDataFiles]
GO
ALTER TABLE [dbo].[TelemetryDataSRX400Environments]  WITH CHECK ADD  CONSTRAINT [FK_TelemetryDataSRX400Environments_RawDataFiles] FOREIGN KEY([FileId])
REFERENCES [dbo].[RawDataFiles] ([FileId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TelemetryDataSRX400Environments] CHECK CONSTRAINT [FK_TelemetryDataSRX400Environments_RawDataFiles]
GO
ALTER TABLE [dbo].[TelemetryDataSRX400Filters]  WITH CHECK ADD  CONSTRAINT [FK_TelemetryDataSRX400Filters_RawDataFiles] FOREIGN KEY([FileId])
REFERENCES [dbo].[RawDataFiles] ([FileId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TelemetryDataSRX400Filters] CHECK CONSTRAINT [FK_TelemetryDataSRX400Filters_RawDataFiles]
GO
ALTER TABLE [dbo].[TelemetryDataSRX400Locations]  WITH CHECK ADD  CONSTRAINT [FK_TelemetryDataSRX400Locations_RawDataFiles] FOREIGN KEY([FileId])
REFERENCES [dbo].[RawDataFiles] ([FileId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TelemetryDataSRX400Locations] CHECK CONSTRAINT [FK_TelemetryDataSRX400Locations_RawDataFiles]
GO
ALTER TABLE [dbo].[TelemetryDataSRX400TrackingData]  WITH CHECK ADD  CONSTRAINT [FK_TelemetryDataSRX400TrackingData_RawDataFiles] FOREIGN KEY([FileId])
REFERENCES [dbo].[RawDataFiles] ([FileId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TelemetryDataSRX400TrackingData] CHECK CONSTRAINT [FK_TelemetryDataSRX400TrackingData_RawDataFiles]
GO
GRANT EXECUTE ON [dbo].[RawDataFile_Delete] TO [public] AS [dbo]
GO
GRANT EXECUTE ON [dbo].[RawDataFile_Insert] TO [public] AS [dbo]
GO
