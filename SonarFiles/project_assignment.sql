--Different projects in initial load
SELECT DISTINCT REPLACE(RIGHT(LEFT(Processing_File_Name, 31),13),'\','_') FROM SonarCountFiles

-- Special case project assignment based on folder of processed file in initial load
UPDATE S
   SET project = REPLACE(RIGHT(LEFT(Processing_File_Name, 31),13),'\','_')
  FROM SonarCountFileSummaries AS S
  JOIN SonarCountFiles AS F
    ON S.Sonar_Count_File_ID = F.Sonar_Count_File_ID
 WHERE S.project IS NULL

-- Check for files where year folder does not match year of data
    -- Everything in the initial data load is OK.
SELECT Project, Source_File_Date
  FROM SonarCountFileSummaries
 WHERE LEFT(Source_File_Date,4) <> RIGHT(Project, 4)


-- Example for a general project name based on project folder of processed file
    -- Query to see which records would be changed
SELECT S.project, 'Newhalen_2018', F.Sonar_Count_File_ID, F.Processing_File_Name
  FROM SonarCountFileSummaries AS S
  JOIN SonarCountFiles AS F
    ON S.Sonar_Count_File_ID = F.Sonar_Count_File_ID
 WHERE F.Processing_File_Name LIKE '%\Newhalen\2018\%'
   AND S.project IS NULL

    -- Query to make the assignment
UPDATE S
   SET project = 'Newhalen_2018'
  FROM SonarCountFileSummaries AS S
  JOIN SonarCountFiles AS F
    ON S.Sonar_Count_File_ID = F.Sonar_Count_File_ID
 WHERE F.Processing_File_Name LIKE '%\Newhalen\2018\%'
   AND S.project IS NULL
