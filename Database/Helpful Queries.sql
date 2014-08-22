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

-- Adding Location Type Codes to Raw Data Files
UPDATE RawDataFiles 
   SET LocationTypeCode = 'Air' 
 WHERE LocationTypeCode IS NULL AND [FileName] LIKE '%air%'

-- Create Locations from ATS Data
INSERT INTO Locations (FileId, LineNumber, Location, [TimeStamp], Frequency, TagCode, SignalStrength) 
SELECT D.FileId, D.LineNumber, geography::Point(D.Latitude, D.Longitude, 4326) AS Location,
       dbo.DateTimeFromAtsWithSeconds(D.[Year], D.[Day], D.[Hour], D.[Minute], D.[Second]) AS [TimeStamp],
       D.Frequency, D.TagNumberAndMortality, D.SignalStrength
  FROM TelemetryDataATSTracking AS D
 WHERE 55 < D.Latitude AND D.Latitude < 70 AND -165 < D.Longitude AND  D.Longitude < -135
       AND D.Frequency IS NOT NULL AND D.TagNumberAndMortality IS NOT NULL

INSERT INTO Locations (FileId, LineNumber, Location, [TimeStamp], Frequency, TagCode, SignalStrength, DuplicateCount) 
SELECT D.FileId, D.LineNumber, geography::Point(A.Latitude, A.Longitude, 4326) AS Location,
       dbo.DateTimeFromAts(D.[Year], D.[Day], D.[Hour], D.[Minute]) AS [TimeStamp],
       D.Frequency, D.TagNumberAndMortality, D.SignalStrength, D.DuplicateCount
  FROM TelemetryDataATSStationary AS D
  JOIN AntennaLocations AS A ON D.Antenna = A.AntennaId
       AND (A.StartDate IS NULL OR A.StartDate < dbo.DateTimeFromAts(D.[Year], D.[Day], D.[Hour], D.[Minute]))
       AND (A.EndDate IS NULL OR A.EndDate > dbo.DateTimeFromAts(D.[Year], D.[Day], D.[Hour], D.[Minute]))
 WHERE D.Frequency IS NOT NULL AND D.TagNumberAndMortality IS NOT NULL


-- Create locations from SRX400 Data
-- FIXME - some files have multiple reference locations (TelemetryDataSRX400Locations table)
-- I need to join with the reference location with the closest smaller line number
-- FIXME - do not delete a duplicate reference location if there is an interceeding different location (see file 91)
INSERT INTO Locations (FileId, LineNumber, Location, [TimeStamp], Frequency, TagCode, SignalStrength, DuplicateCount) 
 SELECT
--		 L.LineNumber,
         D.FileId, D.LineNumber, Location = 
	     CASE WHEN D.Latitude IS NULL THEN
	        geography::Point(Max(L.Latitude), Min(L.Longitude), 4326)
--	        geography::Point(L.Latitude, L.Longitude, 4326)
         ELSE
	        geography::Point(D.Latitude, D.Longitude, 4326)
	     END,
--         L.Latitude, L.Longitude, D.Latitude, D.Longitude, 
--         Max(L.Latitude), Min(L.Longitude), D.Latitude, D.Longitude, 
         D.[Date] AS [TimeStamp], C.Frequency, D.Code AS TagCode, D.[Power] AS SignalStrength, D.[Events] AS DuplicateCount
    FROM TelemetryDataSRX400TrackingData AS D
    JOIN (
              SELECT [FileId]
                    ,min([LineNumber]) as LineNumber
                    ,[Channel]
                    ,[Frequency]
                FROM TelemetryDataSRX400Channels
            GROUP BY Fileid,Channel,Frequency
         ) AS C
      ON D.FileId = C.FileId AND C.Channel = D.Channel AND C.LineNumber < D.LineNumber
    JOIN (
              SELECT [FileId]
                    ,min([LineNumber]) as LineNumber
                    ,[Latitude]
                    ,[Longitude]
                FROM TelemetryDataSRX400Locations
            GROUP BY Fileid,Latitude,Longitude
              HAVING 55 < Latitude AND Latitude < 70 AND -165 < Longitude AND  Longitude < -135
         ) AS L
      ON D.FileId = L.FileId AND L.LineNumber < D.LineNumber
GROUP BY D.FileId, D.LineNumber, D.[Date], C.Frequency, D.Code, D.[Power], D.[Events], D.Latitude, D.Longitude
--WHERE D.FileId = 91 and D.LineNumber = 97
--order by D.LineNumber, L.LineNumber



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

DELETE FROM Locations

CREATE SPATIAL INDEX [SIndex_Locations_Location] ON [dbo].[Locations] 
(
	[Location]
)USING  GEOGRAPHY_GRID 
WITH (
GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM), 
CELLS_PER_OBJECT = 16, PAD_INDEX  = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
