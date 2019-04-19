
DROP TABLE SonarCountFileSummaries
DROP TABLE SonarCountFileCounts
DROP TABLE SonarCountFiles
GO

CREATE TABLE SonarCountFiles (
	Sonar_Count_File_ID  int IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	Processing_File_Name nvarchar(250) NOT NULL,
	Count_File_Name varchar(250),
	Source_File_Name varchar(250),
	Contents varchar(max) NOT NULL,
    CONSTRAINT PK_SonarCountFiles PRIMARY KEY CLUSTERED (Sonar_Count_File_ID)
)

CREATE TABLE SonarCountFileSummaries (
	Sonar_Count_File_ID int NOT NULL,
	Project varchar(250),
	Alpha varchar(250),
	Average_N_Beams varchar(250),
	Background_Subtraction varchar(250),
	Cluster_Area  varchar(250),
	Convolve_Beams varchar(250),
	Convolve_Samps varchar(250),
	Detect_Motion varchar(250),
	Downstream varchar(250),
	Echo_Detect_Angle varchar(250),
	Editor_ID varchar(250),
	Factor_A varchar(250),
	Factor_C varchar(250),
	Intensity varchar(250),
	Log_Multiplier varchar(250),
	Max_Count_Angle varchar(250),
	Max_Count_Range varchar(250),
	Max_Process_Frame varchar(250),
	Min_Count_Angle varchar(250),
	Min_Count_Range varchar(250),
	Min_Process_Frame varchar(250),
	Min_Threshold varchar(250),
	Min_Track_Size varchar(250),
	Sidestream varchar(250),
	Smooth_Foreground varchar(250),
	Source_File_Date varchar(250),
	Source_File_End varchar(250),
	Source_File_Start varchar(250),
	TL_Correction varchar(250),
	Threshold varchar(250),
	Total_Fish varchar(250),
	Upstream varchar(250),
	Upstream_Motion varchar(20),
	Window_End varchar(250),
	Window_Start varchar(250),
    CONSTRAINT PK_SonarCountFileSummaries PRIMARY KEY CLUSTERED (Sonar_Count_File_ID)
)

CREATE TABLE SonarCountFileCounts (
	Sonar_Count_File_ID int NOT NULL,
	File_Code varchar(250),
	Total varchar(250),
	Mark varchar(250),
	Frame# varchar(250),
	Dir varchar(250),
	R_m varchar(250),
	Theta varchar(250),
	L_cm varchar(250),
	L_frm varchar(250),
	T_cm varchar(250),
	dR_cm varchar(250),
	L_over_T varchar(250),
	L_over_dR varchar(250),
	Aspect varchar(250),
	Cluster varchar(250),
	Track varchar(250),
	Time_iso varchar(250),
	Date_iso varchar(250),
	Latitude varchar(20),
	Longitude varchar(20),
	Pan varchar(250),
	Tilt varchar(250),
	Roll varchar(250),
	Species varchar(20),
	Motion varchar(20),
	Quality varchar(20),
	Repeat_Count varchar(250),
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
