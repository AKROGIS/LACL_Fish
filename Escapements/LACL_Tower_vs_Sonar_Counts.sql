-- Compare Tower counts (Escapement table) with Sonar counts (SonarCountFileCounts table) wherever there is overlap
-- There is sonar data for Newhalen and Chulitna, but only tower data for New Halen; I will ignore Chulitna sonar data
-- The Sonar device is on the left bank upstream of the tower and it takes fish about 2min 45sec to go from tower to sonar
-- All hours in the Tower data and the timestamps in the sonar data are both local time.
-- There are right and left bank tower counts, but only left bank sonar data; I will ignore all right bank counts
-- Left bank tower counts are done for 10 minutes before every odd hour (but they reported for the preceding even hour)
-- The tower only counts upstream fish, sonar counts upstream and downstream fish; I will subtract downstream fish from the upstream count 
-- Therefore I will compare the tower count for:
--    hour 4 with the sonar data from 4:52:45 to 5:02:45
--    hour 6 with the sonar data from 6:52:45 to 7:02:45
--    repeat for all even hours

-- Lets look at some of the data
SELECT * FROM Escapements where datestamp = '2019-07-27'
SELECT top 10 * FROM SonarCountFileCounts where date_iso = '2019-07-27'
select dir, count(dir) from SonarCountFileCounts where date_iso = '2019-07-27' group by dir
SELECT Source_File_Date, Source_File_Start, Source_File_End, Total_Fish, Upstream, Downstream, Sidestream FROM SonarCountFileSummaries where Source_File_Date = '2019-07-27' order by Source_File_Start
select date_iso, time_iso, dir from SonarCountFileCounts where date_iso = '2019-07-27' and time_iso > '06:52:45' and time_iso < '07:02:45' order by time_iso

-- Left Bank Tower counts
SELECT DateStamp, Hour, LBank_Count AS TowerCount FROM Escapements where hour % 2 = 0 and LBank_Count is not null
  and datestamp = '2019-07-27'

-- Tower counts with overlapping sonar data
Select t.DateStamp, t.Hour, t.LBank_Count
, right('0' + cast(t.Hour as varchar(2)), 2) + ':52:45' as SonarStart
, right('0' + cast((t.Hour + 1) as varchar(2)), 2) + ':02:45' as SonarEnd
  , s1.Source_File_Start, s1.Source_File_End, s2.Source_File_Start, s2.Source_File_End
from Escapements as t left join SonarCountFileSummaries as s1
on t.DateStamp = s1.Source_File_Date AND right('0' + cast(t.Hour as varchar(2)), 2) + ':50:00' = s1.Source_File_Start 
left join SonarCountFileSummaries as s2
on t.DateStamp = s2.Source_File_Date AND right('0' + cast((t.Hour + 1) as varchar(2)), 2) + ':00:00' = s2.Source_File_Start 
where t.hour % 2 = 0 and t.LBank_Count is not null and s1.Source_File_Start is not null and s2.Source_File_Start is not null
  and t.DateStamp = '2019-07-27' order by t.hour, s1.Source_File_Start

-- Individual Sonar counts for a date and start/end time
select date_iso, time_iso, dir from SonarCountFileCounts where date_iso = '2019-07-27' and time_iso >= '06:52:45' and time_iso < '07:02:45' order by time_iso

-- Summarize Individual Sonar counts for a date and start/end time
select dir, count(dir) as count from SonarCountFileCounts
where date_iso = '2019-07-27' and time_iso >= '06:52:45' and time_iso < '07:02:45'
group by dir

-- Join tower counts with sonar counts
select t2.DateStamp, t2.hour, t2.LBank_Count, s.dir, count(*) as count
from (
    Select t.DateStamp, t.Hour, t.LBank_Count
    from Escapements as t left join SonarCountFileSummaries as s1
    on t.DateStamp = s1.Source_File_Date AND right('0' + cast(t.Hour as varchar(2)), 2) + ':50:00' = s1.Source_File_Start 
    left join SonarCountFileSummaries as s2
    on t.DateStamp = s2.Source_File_Date AND right('0' + cast((t.Hour + 1) as varchar(2)), 2) + ':00:00' = s2.Source_File_Start 
    where t.hour % 2 = 0 and t.LBank_Count is not null and s1.Source_File_Start is not null and s2.Source_File_Start is not null
) as t2 join SonarCountFileCounts as s
on s.date_iso = t2.DateStamp
and s.time_iso >= right('0' + cast(t2.Hour as varchar(2)), 2) + ':52:45'
and s.time_iso < right('0' + cast((t2.Hour + 1) as varchar(2)), 2) + ':02:45'
where (s.dir = 'Up' or s.dir = 'Dn') and t2.DateStamp = '2019-07-27'
group by t2.DateStamp, t2.hour, t2.LBank_Count, s.dir
order by t2.DateStamp, t2.hour, t2.LBank_Count, s.dir

-- Join tower counts with sonar counts - Pivot direction counts
WITH counts (DateStamp, hour, TowerCnt, SonarDir, SonarCnt)  
AS (  
    select t2.DateStamp, t2.hour, t2.LBank_Count, s.dir, count(*) as count
    from (
        Select t.DateStamp, t.Hour, t.LBank_Count
        from Escapements as t left join SonarCountFileSummaries as s1
        on t.DateStamp = s1.Source_File_Date AND right('0' + cast(t.Hour as varchar(2)), 2) + ':50:00' = s1.Source_File_Start 
        left join SonarCountFileSummaries as s2
        on t.DateStamp = s2.Source_File_Date AND right('0' + cast((t.Hour + 1) as varchar(2)), 2) + ':00:00' = s2.Source_File_Start 
        where t.hour % 2 = 0 and t.LBank_Count is not null and s1.Source_File_Start is not null and s2.Source_File_Start is not null
    ) as t2 join SonarCountFileCounts as s
    on s.date_iso = t2.DateStamp
    and s.time_iso >= right('0' + cast(t2.Hour as varchar(2)), 2) + ':52:45'
    and s.time_iso < right('0' + cast((t2.Hour + 1) as varchar(2)), 2) + ':02:45'
    where (s.dir = 'Up' or s.dir = 'Dn') and t2.DateStamp = '2019-07-27'
    group by t2.DateStamp, t2.hour, t2.LBank_Count, s.dir
)
Select c1.DateStamp, c1.hour, c1.TowerCnt, c1.SonarCnt as SonarUp, c2.SonarCnt as SonarDn 
from (select * from counts where sonarDir = 'Up') as c1
left join (select * from counts where sonarDir = 'Dn') as c2
on c1.DateStamp = c2.DateStamp and c1.hour = c2.hour

-- FINAL QUERY

-- Join tower counts vs sonar counts - All dates with summary stats
WITH counts (DateStamp, hour, TowerCnt, SonarDir, SonarCnt)  
AS (  
    select t2.DateStamp, t2.hour, t2.LBank_Count, s.dir, count(*) as count
    from (
        Select t.DateStamp, t.Hour, t.LBank_Count
        from Escapements as t left join SonarCountFileSummaries as s1
        on t.DateStamp = s1.Source_File_Date AND right('0' + cast(t.Hour as varchar(2)), 2) + ':50:00' = s1.Source_File_Start 
        left join SonarCountFileSummaries as s2
        on t.DateStamp = s2.Source_File_Date AND right('0' + cast((t.Hour + 1) as varchar(2)), 2) + ':00:00' = s2.Source_File_Start 
        where t.hour % 2 = 0 and t.LBank_Count is not null and s1.Source_File_Start is not null and s2.Source_File_Start is not null
    ) as t2 join SonarCountFileCounts as s
    on s.date_iso = t2.DateStamp
    and s.time_iso >= right('0' + cast(t2.Hour as varchar(2)), 2) + ':52:45'
    and s.time_iso < right('0' + cast((t2.Hour + 1) as varchar(2)), 2) + ':02:45'
    where (s.dir = 'Up' or s.dir = 'Dn')
    -- and t2.DateStamp = '2019-07-27'
    group by t2.DateStamp, t2.hour, t2.LBank_Count, s.dir
)
Select c1.DateStamp, c1.hour, c1.TowerCnt, c1.SonarCnt as SonarUp, c2.SonarCnt as SonarDn,  
c1.SonarCnt - coalesce(c2.SonarCnt, 0) as SonarCnt,
c1.SonarCnt - coalesce(c2.SonarCnt, 0) - c1.TowerCnt as delta,
case when c1.TowerCnt = 0 then null else CAST(round((c1.SonarCnt - coalesce(c2.SonarCnt, 0)) / (1.0 * c1.TowerCnt), 2) AS DECIMAL(8,2)) end as pct
from (select * from counts where sonarDir = 'Up') as c1
left join (select * from counts where sonarDir = 'Dn') as c2
on c1.DateStamp = c2.DateStamp and c1.hour = c2.hour
order by c1.DateStamp, c1.hour