-- Total number of data files
SELECT COUNT(*)
  FROM SonarCountFileSummaries

-- Total number of data files with no Fish
SELECT COUNT(*)
  FROM SonarCountFileSummaries
 WHERE Total_Fish = 0

-- File Count by Project/Year
  SELECT Project, LEFT(Source_File_Date,4) AS [Year], 
         RIGHT(MIN(Source_File_Date), 5) AS Start_Date, RIGHT(MAX(Source_File_Date), 5) AS End_Date,
		 COUNT(*) AS Num_Files
    FROM SonarCountFileSummaries
GROUP BY Project, LEFT(Source_File_Date,4)
ORDER BY Project

-- Total fish up/down by project with begin/end
  SELECT Project, LEFT(Source_File_Date,4) AS [Year],
         RIGHT(MIN(Source_File_Date), 5) AS Start_Date, RIGHT(MAX(Source_File_Date), 5) AS End_Date,
		 SUM(CAST(Upstream AS int)) as Up, SUM(CAST(Downstream AS int)) AS Down,
		 SUM(CAST(Upstream AS int)) - SUM(CAST(Downstream AS int)) AS NetUp,
		 SUM(CAST(Sidestream AS int)) AS Unk, SUM(CAST(Total_Fish AS int)) AS Total_Fish
    FROM SonarCountFileSummaries
GROUP BY Project, LEFT(Source_File_Date,4)
ORDER BY Project

-- Fish count per day
  SELECT Project, Source_File_Date AS Date,
		 SUM(CAST(Upstream AS int)) as Up, SUM(CAST(Downstream AS int)) AS Down,
		 SUM(CAST(Upstream AS int)) - SUM(CAST(Downstream AS int)) AS NetUp,
		 SUM(CAST(Sidestream AS int)) AS Unk, SUM(CAST(Total_Fish AS int)) AS Total_Fish
    FROM SonarCountFileSummaries
GROUP BY Project, Source_File_Date
ORDER BY Project, Source_File_Date

-- Fish count per hour
  SELECT S.Project, C.Date_iso AS Date, LEFT(C.Time_iso, 2) AS Hour,
         C.Dir AS Direction, COUNT(*) AS Fish
    FROM SonarCountFileCounts AS C
	JOIN SonarCountFileSummaries AS S
	  ON C.Sonar_Count_File_ID = S.Sonar_Count_File_ID
GROUP BY S.Project, C.Date_iso, LEFT(C.Time_iso, 2), C.Dir
ORDER BY S.Project, C.Date_iso, LEFT(C.Time_iso, 2), C.Dir
