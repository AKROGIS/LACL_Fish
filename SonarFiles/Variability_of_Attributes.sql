-- Variability of attributes in SonarCountFileSummaries
-- Highly variable
--   Sonar_Count_File_ID, Project, Downstream, Editor_ID, Max_Process_Frame,
--   Sidestream, Source_File_Date, Source_File_End, Source_File_Start, Total_Fish, Upstream

-- Somewhat variable (7 values) 
  SELECT Project, COUNT(*)
    FROM SonarCountFileSummaries
GROUP BY Project
-- Somewhat variable (23 values) 
  SELECT Editor_ID, COUNT(*)
    FROM SonarCountFileSummaries
GROUP BY Editor_ID

-- Somewhat variable (3-5 values) 
  SELECT Max_Count_Range, Min_Count_Range, Window_End, Window_Start, COUNT(*)
    FROM SonarCountFileSummaries
GROUP BY Max_Count_Range, Min_Count_Range, Window_End, Window_Start

  SELECT Alpha, Echo_Detect_Angle, Factor_A, Intensity, Threshold, COUNT(*)
    FROM SonarCountFileSummaries
GROUP BY Alpha, Echo_Detect_Angle, Factor_A, Intensity, Threshold

-- Slightly variable (2 values)
  SELECT Average_N_Beams, Background_Subtraction, Cluster_Area, Convolve_Beams, Convolve_Samps, Detect_Motion, COUNT(*)
    FROM SonarCountFileSummaries
GROUP BY Average_N_Beams, Background_Subtraction, Cluster_Area, Convolve_Beams, Convolve_Samps, Detect_Motion

  SELECT Factor_C, Log_Multiplier, Max_Count_Angle, Min_Count_Angle, COUNT(*)
    FROM SonarCountFileSummaries
GROUP BY Factor_A, Factor_C, Log_Multiplier, Max_Count_Angle, Min_Count_Angle

  SELECT Min_Process_Frame, Min_Threshold, Min_Track_Size, Smooth_Foreground, TL_Correction, Upstream_Motion, COUNT(*)
    FROM SonarCountFileSummaries
GROUP BY Min_Process_Frame, Min_Threshold, Min_Track_Size, Smooth_Foreground, TL_Correction, Upstream_Motion

-- NOT Variable
--  None
 

-- Variability of attributes in SonarCountFileCounts
-- Highly variable
--   Sonar_Count_File_ID, Total, Mark, Frame#, R_m, Time_iso, Date_iso

-- Somewhat variable 
  SELECT Theta, L_cm, Aspect, Track, COUNT(*)
    FROM SonarCountFileCounts
GROUP BY Theta, L_cm, Aspect, Track

-- Slightly variable (3-4 Values)
  SELECT L_frm, dR_cm, L_over_T, L_over_dR, COUNT(*)
    FROM SonarCountFileCounts
GROUP BY L_frm, dR_cm, L_over_T, L_over_dR

-- Slightly variable (3 Values)
  SELECT Dir, COUNT(*)
    FROM SonarCountFileCounts
GROUP BY Dir

-- Slightly variable (2 Values)
  SELECT T_cm, Cluster, COUNT(*)
    FROM SonarCountFileCounts
GROUP BY T_cm, Cluster

-- NOT Variable
  SELECT File_Code, Latitude, longitude, Pan, Tilt, Roll, Species, Motion, Quality, Repeat_Count, Comment, COUNT(*)
    FROM SonarCountFileCounts
GROUP BY File_Code, Latitude, longitude, Pan, Tilt, Roll, Species, Motion, Quality, Repeat_Count, Comment