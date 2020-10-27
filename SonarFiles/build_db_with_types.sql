
--DROP TABLE SonarCountFiles
--DROP TABLE SonarCountFileSummaries
--DROP TABLE SonarCountFileCounts

CREATE TABLE SonarCountFiles (
	Sonar_Count_File_ID  int IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	Processing_File_Name nvarchar(250) NOT NULL,
	Count_File_Name nvarchar(250),
	Source_File_Name nvarchar(250),
	Contents varbinary(max) NOT NULL,
    CONSTRAINT PK_SonarCountFiles PRIMARY KEY CLUSTERED (Sonar_Count_File_ID)
)

CREATE TABLE SonarCountFileSummaries (
	Sonar_Count_File_ID int NOT NULL,
	Project varchar(100),
	Alpha real,
	Average_N_Beams int,
	Background_Subtraction varchar(10),
	Cluster_Area  varchar(10),
	Convolve_Beams int,
	Convolve_Samps int,
	Detect_Motion varchar(10),
	Downstream int,
	Echo_Detect_Angle varchar(10),
	Editor_ID varchar(10),
	Factor_A real,
	Factor_C real,
	Intensity varchar(10),
	Log_Multiplier real,
	Max_Count_Angle varchar(10),
	Max_Count_Range varchar(10),
	Max_Process_Frame int,
	Min_Count_Angle varchar(10),
	Min_Count_Range varchar(10),
	Min_Process_Frame int,
	Min_Threshold varchar(10),
	Min_Track_Size int,
	Sidestream int,
	Smooth_Foreground varchar(10),
	Source_File_Date varchar(10),
	Source_File_End varchar(10),
	Source_File_Start varchar(10),
	TL_Correction varchar(10),
	Threshold varchar(10),
	Total_Fish int,
	Upstream int,
	Upstream_Motion varchar(20),
	Window_End varchar(10),
	Window_Start varchar(10),
    CONSTRAINT PK_SonarCountFileSummaries PRIMARY KEY CLUSTERED (Sonar_Count_File_ID)
)

CREATE TABLE SonarCountFileCounts (
	Sonar_Count_File_ID int NOT NULL,
	File_Code int,
	Total int,
	Mark int,
	Frame# int,
	Dir varchar(10),
	R_m real,
	Theta real,
	L_cm real,
	L_frm real,
	T_cm real,
	dR_cm real,
	L_over_T real,
	L_over_dR real,
	Aspect real,
	Cluster int,
	Track int,
	Time_iso varchar(10),
	Date_iso varchar(10),
	Latitude varchar(20),
	Longitude varchar(20),
	Pan real,
	Tilt real,
	Roll real,
	Species varchar(20),
	Motion varchar(20),
	Quality varchar(20),
	Repeat_Count int,
	Comment varchar(250),
)
GO

ALTER TABLE SonarCountFileSummaries  WITH CHECK ADD CONSTRAINT FK_SonarCountFileSummaries_Sonar_Count_File_ID FOREIGN KEY(Sonar_Count_File_ID)
REFERENCES SonarCountFiles (Sonar_Count_File_ID)
ON DELETE CASCADE
GO

ALTER TABLE SonarCountFileSummaries CHECK CONSTRAINT FK_SonarCountFileSummaries_Sonar_Count_File_ID
GO

ALTER TABLE SonarCountFileCounts  WITH CHECK ADD CONSTRAINT FK_SonarCountFileCounts_Sonar_Count_File_ID FOREIGN KEY(Sonar_Count_File_ID)
REFERENCES SonarCountFiles (Sonar_Count_File_ID)
ON DELETE CASCADE
GO

ALTER TABLE SonarCountFileCounts CHECK CONSTRAINT FK_SonarCountFileCounts_Sonar_Count_File_ID
GO
