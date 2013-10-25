-- Show the tags for the ATS data
SELECT COUNT(*),[TagNumberAndMortality]
  FROM [Fish_Tagging].[dbo].[TelemetryDataATSTracking]
  group by [Frequency],[TagNumberAndMortality]
  order by [Frequency],[TagNumberAndMortality]

-- Good Locations in SRX400 data with Frequency/Code
SELECT t.[Date], T.Latitude, T.Longitude, T.Channel, C.Frequency, T.Code
  FROM TelemetryDataSRX400TrackingData AS T
  LEFT JOIN TelemetryDataSRX400Channels AS C
  ON T.FileId = C.FileId
  where 59 <= Latitude and Latitude <= 61 and -156 <= Longitude and Longitude <= -153

-- Unique SXR400 Antennas
SELECT [DeviceType],[DeviceId],[Gain]
  FROM [Fish_Tagging].[dbo].[TelemetryDataSRX400Antennas]
  group by [DeviceType],[DeviceId],[Gain]

-- Unique SXR400 Channels
SELECT [Channel],[Frequency]
  FROM [Fish_Tagging].[dbo].[TelemetryDataSRX400Channels]
  group by [Channel],[Frequency]
  order by [Channel]

-- Unique SXR400 Filters
SELECT [Channel],[Code]
  FROM [Fish_Tagging].[dbo].[TelemetryDataSRX400Filters]
  group by [Channel],[Code]
  order by [Channel]

-- Unique SXR400 BatteryStatus
SELECT [Status]
  FROM [Fish_Tagging].[dbo].[TelemetryDataSRX400BatteryStatus]
  group by [Status]

-- Bad Reference locations
SELECT *
  FROM [Fish_Tagging].[dbo].[TelemetryDataSRX400Locations]
  where Latitude < 59 or 61 < Latitude or Longitude < -156 or -153 < Longitude
  order by Longitude

-- Good Reference locations
SELECT *
  FROM [Fish_Tagging].[dbo].[TelemetryDataSRX400Locations]
  where 59 <= Latitude and Latitude <= 61 and -156 <= Longitude and Longitude <= -153
  order by Longitude

-- Sites in SXR400 environments
SELECT [Site]
  FROM [Fish_Tagging].[dbo].[TelemetryDataSRX400Environments] group by [Site]

--Show all records for an SRX400 file
declare @id int = 98
select [FolderName],[FileName],ProcessingErrors, ProcessingDone from RawDataFiles where FileId = @id
SELECT * FROM TelemetryDataSRX400Antennas where fileid = @id
SELECT * FROM TelemetryDataSRX400BatteryStatus where fileid = @id
SELECT * FROM TelemetryDataSRX400Channels where fileid = @id
SELECT * FROM TelemetryDataSRX400Environments where fileid = @id
SELECT * FROM TelemetryDataSRX400Filters where fileid = @id
SELECT * FROM TelemetryDataSRX400Locations where fileid = @id
SELECT * FROM TelemetryDataSRX400TrackingData where fileid = @id

-- Clear all tables
/*
DELETE TelemetryDataSRX400Antennas
DELETE TelemetryDataSRX400BatteryStatus
DELETE TelemetryDataSRX400Channels
DELETE TelemetryDataSRX400Environments
DELETE TelemetryDataSRX400Filters
DELETE TelemetryDataSRX400Locations
DELETE TelemetryDataSRX400TrackingData
*/

-- Processing SXR400 files

SELECT [FileId]
      ,[FolderName]
      ,[FileName]
      ,[ProcessingErrors]
      ,[ProcessingDone]
  FROM [Fish_Tagging].[dbo].[RawDataFiles]
  where processingErrors is not null
  order by [FolderName], [FileName]
GO

select COUNT(*) from [RawDataFiles]


-- Processing files

-- execute [dbo].[ProcessRawDataFile] 37

/*
declare @id int = 1
while @id < 294
if @id in (11,248,268,269,270,271)
begin
   set @id = @id + 1
end
else
begin
  execute [dbo].[ProcessRawDataFile] @id
  set @id = @id + 1
end
set @id = 343
while @id < 535
begin
  execute [dbo].[ProcessRawDataFile] @id
  set @id = @id + 1
end
*/