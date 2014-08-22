-- Neither of these two methods will delete the columns, causing failure when replacing the function
-- Curiously, they both work when called from SSMS.
-- more curiouser, the second method will fail (column not found), if the column has already been deleted
/*
USE [Fish_Tagging]
GO

IF COL_LENGTH('RawDataFiles','Sha1Hash') IS NOT NULL --safely check if column exists
BEGIN
    alter table [Fish_Tagging].[dbo].[RawDataFiles] drop column [Sha1Hash]
END
*/
/*
alter table [Fish_Tagging].[dbo].[collarfiles] drop column [Sha1Hash]
*/
