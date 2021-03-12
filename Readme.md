# LACL Fish Monitoring

This project contains code in support of the Lake Clark NPP Fish Monitoring
database `LACL_Fish`
hosted internally on the Alaska Region's SQL Server.

Contacts:

* Dan Young (LACL)
* Joel Reynolds (NR)
* James Kramer (LACL)

This project has had 3 phases, starting in 2014, as of 2020, only phases 2 and 3
appear to be active.

## Phase 1 (Tagging)

This was the original 2014 project. It loaded and processed different data files
from several RFID sensors that collected data on RFID tagged fish. The raw
files are stored in the database as well as data tables and location data
resulting from the processing of the files.  This project was modeled on the
[Animal Movements Database](https://github.com/AKROGIS/AnimalMovement).

The C# solution and most of the files/folders are part of this phase.
It also created the majority of the
[database objects](Database/Database%20Schema.sql).
This phase provides a simple WinForms GUI app
([FileUploader project](FileUploader))
for uploading files to the database and reviewing the files
(and their contents). It requires one library project
[DataModel](DataModel).

There are also two SQL CLR projects (to load custom functions into the SQL
Server database).

* [SqlServer_Files](SqlServer_Files)

  This project creates an SQL Assembly for a store procedure called
  `dbo.ProcessRawDataFile` which takes a `FileId` from the `dbo.RawDataFiles`
  and process the file contents in the blob column into tabular data in the
  `dbo.TelemetryData*` tables depending on the type/contents of file.
  This assembly has been loaded into the server. If it needs to be modified,
  see the Animal Movements repo for instructions on loading the compiled
  assembly into the database and creating the stored procedure that calls it.

* [SqlServer_Functions](SqlServer_Functions)

  This project creates an SQL Assembly for several scalar valued functions
  (`dbo.DateTimeFromAts`, `dbo.DateTimeFromAtsWithSeconds`, `dbo.LocalTime`,
  `dbo.Sha1Hash` and `dbo.UtcTime`) that can be called by other database objects
  Currently only `dbo.Sha1hash` is being used in the definition of the
  `dbo.RawDataFiles` table. If these functions need to be modified,
  see the Animal Movements repo for instructions on loading the compiled
  assembly into the database and creating the functions that call it.

Data files provided by Dan Young were loaded into the database, and a number of
[queries](Database/Helpful%20Queries.sql)
were provided to analyze the data and visualize it in ArcMap.  It has not been
updated since 2014.

**NOTE:** This phase was never fully automated.  Some of the processing was done
manually with the queries found in
[Database/Helpful Queries.sql](Database/Helpful%20Queries.sql).

### Phase 1 Database objects

#### Tables (Phase 1)

* `AntennaLocations` - Derived from `TelemetryData*` tables
* `Locations` - Derived from `TelemetryData*` tables
* `LookupFileFormats` - Manually created/maintained
* `LookupLocationType` - Manually created/maintained
* `LookupRadioManufacturers` - Manually created/maintained
* `RawDataFiles` - Maintained by user using `FileUploader` app.
* `TelemetryDataATSStationary` - Derived from `RawDataFiles`
* `TelemetryDataATSTracking` - Derived from `RawDataFiles`
* `TelemetryDataSRX400Antennas` - Derived from `RawDataFiles`
* `TelemetryDataSRX400BatteryStatus` - Derived from `RawDataFiles`
* `TelemetryDataSRX400Channels` - Derived from `RawDataFiles`
* `TelemetryDataSRX400Environments` - Derived from `RawDataFiles`
* `TelemetryDataSRX400Locations` - Derived from `RawDataFiles`
* `TelemetryDataSRX400TrackingData` - Derived from `RawDataFiles`

#### Stored Procedures

* `ProcessRawDataFile` - wrapper for CLR (C#) Assembly
  parse the file contents blob if a record in `RawDataFiles`
  and populates the `TelemetryData*` tables
* `RawDataFile_Delete` - deletes a record from `RawDataFiles`. Users cannot
  directly edit the table and must use this procedure which validates
  the request and applies required business rules.
* `RawDataFile_Insert` - Adds a record to `RawDataFiles`. See discussion above.

#### Scalar Value Functions

* `DateTimeFromAts` - Returns a Datetime from the text in an ATS data file.
* `DateTimeFromAtsWithSeconds` - Returns a Datetime from the text in an ATS
  data file.
* `LocalTime` - Returns a local DateTime from a UTC DataTime.
* `Sha1Hash` - Provides a computed column in the `RawDataFiles` table. Hashes
  the file contents column (blob) to allow detection of duplicate files.
* `UtcTime` - Returns a UTC DateTime from a local DataTime.

#### Views

* `LocationsWithFileError` - Location data suitable for ArcGIS

Also see [Database/Helpful Queries.sql](Database/Helpful%20Queries.sql).

## Phase 2 (Sonar)

This phase added sonar data files to the LACL Fish database.  These files are
text output from a manual review process of raw sonar data by a technician.
The text files are processed by a Python script and the data is loaded into
the three `SonarCount*` tables.  All of the files for this phase are in the
[SonarFiles](SonarFiles) folder.

I recall this phase began in 2018 with an initial upload of data from 2016-2018.
Updates were done with data collected in 2019 and then again in 2020.

Every year, a folder of files is provided by LACL (James Kramer in 2020).
The `parse.py` script `Config` object is edited to first test the input files
(an entire folder of files can be processed at once). If there are no
processing errors in the test, then edit the `Config` to do an actual database
update and then re-run the script.  In the past the processing has failed due to
a corrupt (usually empty) sonar file. These can be removed from the processing
folder and returned to LACL for remedial action. The remainder of the files can
then be processed.  It is also possible that a new (unexpected) file format is
submitted which may require edits to the script.  Hopefully small tweaks to the
global configuration parameters will be sufficient.

Once the data is loaded the LACL staff access the data directly using
*Azure Data Studio* and SQL queries.

### Phase 2 Project Files

* `build_db_with_types.sql` - An SQL script to build the initial schema.  This
  schema was not used (see `build_db.sql`), as there were too many data loading
  errors importing text to real database types like `int`/`float`/`datetime`,
  etc. instead the schema uses all `varchar` columns, that are converted in
  queries as needed for analysis.
* `build_db.sql` - An SQL script initially used to create the initial
  empty tables. This is no longer needed, unless the database is recreated
  from scratch.
* `parse.py` - a Python 2/3 script to read a folder of sonar data files and
  check for potential problems (without loading data), or load the data into the
  database.
  This script needs to be edited before being run on a new collection of data
  files.  See the comments at the head of this file for additional information
  on the correct usage.
* `project_assignment.sql` - Correct the project names after the initial load.
  The `parse.py` script has been corrected since then and this script is no
  longer needed.
* `Sample_Queries.sql` - Examples of queries the users might use to start
  analyzing the data.
* `Variability_of_Attributes.sql` - Analysis of the variability of the
  values in `SonarCountFileSummaries`.  This can be used to normalized or
  aggregate the data.  Currently is only for information.

### Phase 2 Database objects

#### Tables (Phase2)

* `SonarCountFileCounts` - In addition to summary metadata, each file usually
  includes a list of records for each fish count in the file.  All these
  records are captured in this table.
* `SonarCountFiles` - One record for each file uploaded.  The record includes
  a `Contents` field which contains the entire contents of the source file.
* `SonarCountFileSummaries` - Each file contains a summary (metadata) about the
  counts in the file.  This data is extracted to tabular form in this table
  (one record per file).

## Phase 3 (Escapement)

This project is derived from a spreadsheet created each year that captures
the manual escapement counts from observers in the Newhalen fish counting tower.
The data has been binned into time intervals for most days during the summer.

The initial spreadsheet was converted to CSV and upload into the `Escapements`
table.  Each year a new spreadsheet is provided and it is appended to the
table with the same process.  It may be necessary to adjust the layout and
formatting of the excel file before exporting to CSV, so that the CSV file
creates the same schema. I suggest loading into a test table to ensure a
compatible schema before appending the test table into the `Escapements` table.
*Azure Data Studio* and *Sql Server Management Studio* have tools to load a CSV
file into a new table in SQL server.  You can check for Primary Key violations
before appending with something like

```SQL
Select Location, DateStamp, Hour, count(*) as Duplicates
from Escape2020 group by Location, DateStamp, Hour having count(*) > 1
```

Then use a command like

```SQL
INSERT INTO Escapements SELECT * FROM Escape2020
```

Once the data is loaded the LACL staff access the data directly
using *Azure Data Studio* and SQL queries.

### Phase 3 Project Files

All project Files are in the [Escapements](Escapements) folder.

* `build_db.sql` - An SQL script to recreate an empty `Escapements` table.
  This is no longer needed, unless the database is recreated from scratch.
* `LACL_Tower_vs_Sonar_Counts.sql` - A request query that compares the
  sonar data (phase 2) with the escapement data.  See the comments in the file
  for details.

### Phase 3 Database objects

#### Tables (Phase3)

* `Escapements` - See [build_db.sql](Escapements/build_db.sql)
  for the table schema.
