using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;

namespace SqlServer_Files
{
    class Srx400Parser : IParser
    {
        private readonly Byte[] _bytes;

        public Srx400Parser(Byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                throw new ArgumentNullException(null,"File is empty");
            _bytes = bytes;
        }

        public bool IsValidData
        {
            get
            {
                var header = GetLines(_bytes).Skip(1).FirstOrDefault();
                return header != null && header.Trim() == "Environment history:";
            }
        }

        enum ParseState
        {
            Start,
            ChangeSet,
            AntennaGroup,
            AntennaDetails,
            HydrophoneGroup,
            HydrophoneDetails,
            FilterDetails,
            GpsMode,
            ScanTable,
            DataTable,
            End
        }

        [Flags]
        enum DataLineDetails
        {
            HeaderHasLatLong   = 1,
            HeaderHasStopDate  = 2,
            HasDataAttribute   = 4,
            HasSensorAttribute = 8,
            GpsModeIsOn        = 16,
        }

        private struct Location
        {
            internal float? Lat;
            internal float? Long;
        }

        public void ParseFileIntoDatabase(SqlInt32 fileId, SqlConnection database)
        {
            ClearDatabase(database, fileId);
            var state = ParseState.Start;
            var changeDateTime = new DateTime();
            int changeLineNumber = 0;
            bool haveScanTable = false;
            DataLineDetails lineType = 0;
            var environment = new Dictionary<string, string>();
            int lineNumber = 2;
            foreach (var line in GetLines(_bytes).Skip(lineNumber))
            {
                lineNumber++;
                switch (state)
                {
                    case ParseState.Start:
                        if (line.StartsWith("changed: "))
                        {
                            changeDateTime = ParseSrx400DateTime(line.Substring(9), lineNumber);
                            changeLineNumber = lineNumber;
                            state = ParseState.ChangeSet;
                        }
                        else
                        {
                            throw new InvalidDataException("File does not begin with a change set date");
                        }
                        break;

                    case ParseState.ChangeSet:
                        if (line == String.Empty)
                            continue;
                        if (line.Trim() == "ANTENNA GROUP:")
                        {
                            state = ParseState.AntennaGroup;
                            break;
                        }
                        if (line.Trim() == "HYDROPHONE GROUP:")
                        {
                            state = ParseState.HydrophoneGroup;
                            break;
                        }
                        if (line.Trim() == "Active scan_table:")
                        {
                            state = ParseState.ScanTable;
                            break;
                        }
                        if (line.StartsWith("Filter:"))
                        {
                            ParseKeyValue(line, environment);
                            if (line.StartsWith("Filter: Channel & Code"))
                            {
                                state = ParseState.FilterDetails;
                            }
                            break;
                        }
                        if (line.StartsWith("GPS data mode ="))
                        {
                            state = ParseState.GpsMode;
                            ParseKeyValue(line, environment);
                            break;
                        }
                        if (line.StartsWith("changed: "))
                        {
                            WriteEnvironmentToDatabase(database, fileId, changeLineNumber, changeDateTime, environment);
                            environment.Clear();
                            changeDateTime = ParseSrx400DateTime(line.Substring(9), lineNumber);
                            changeLineNumber = lineNumber;
                            break;
                        }
                        if (line.Trim() == "Code_log data:")
                        {
                            lineType = 0;
                            WriteEnvironmentToDatabase(database, fileId, changeLineNumber, changeDateTime, environment);
                            if (!haveScanTable)
                                throw new FormatException("No scan table found before the data table");
                            state = ParseState.DataTable;
                            break;
                        }
                        if (!ParseKeyValue(line, environment))
                            throw new FormatException("Unrecognized environment parameter in file at line " + lineNumber);
                        break;

                    case ParseState.AntennaGroup:
                        if (line == String.Empty)
                        {
                            state = ParseState.AntennaDetails;
                            break;
                        }
                        if (line.StartsWith("Antenna      Gain"))
                            continue;
                        AddLineToAntennas(database, fileId, lineNumber, changeDateTime, "Antenna",
                            line.Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
                        break;

                    case ParseState.AntennaDetails:
                        if (line == String.Empty)
                        {
                            state = ParseState.ChangeSet;
                            break;
                        }
                        if (!ParseAntennaKeyValue(line, "Antenna", environment))
                            throw new FormatException("Unrecognized antenna parameter in file at line " + lineNumber);
                        break;

                    case ParseState.HydrophoneGroup:
                        if (line == String.Empty)
                        {
                            state = ParseState.HydrophoneDetails;
                            break;
                        }
                        if (line.StartsWith("Hydrophone   Gain"))
                            continue;
                        AddLineToAntennas(database, fileId, lineNumber, changeDateTime, "Hydrophone",
                            line.Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
                        break;

                    case ParseState.HydrophoneDetails:
                        if (line == String.Empty)
                        {
                            state = ParseState.ChangeSet;
                            break;
                        }
                        if (!ParseAntennaKeyValue(line, "Hydrophone", environment))
                            throw new FormatException("Unrecognized hydrophone parameter in file at line " + lineNumber);
                        break;

                    case ParseState.FilterDetails:
                        if (line == String.Empty)
                        {
                            state = ParseState.ChangeSet;
                            break;
                        }
                        AddLineToFilters(database, fileId, lineNumber, changeDateTime,
                            line.Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
                        break;

                    case ParseState.GpsMode:
                        if (line == String.Empty)
                        {
                            state = ParseState.ChangeSet;
                            break;
                        }
                        if (line == "Reference position" || line == "  Latitude        Longitude")
                            continue;
                        AddLineToLocations(database, fileId, lineNumber, changeDateTime, line);
                        break;

                    case ParseState.ScanTable:
                        if (line == String.Empty)
                        {
                            state = ParseState.ChangeSet;
                            break;
                        }
                        if (line.Trim() == "CHANNEL  FREQUENCY")
                            continue;
                        AddLineToChannels(database, fileId, lineNumber, changeDateTime,
                            line.Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
                        haveScanTable = true;
                        break;

                    case ParseState.DataTable:
                        if (line == String.Empty)
                        {
                            state = ParseState.End;
                            break;
                        }
                        var tokens = line.Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                        if (tokens.Length == 2 && tokens[0] == "Start")
                        {
                            lineType = lineType | DataLineDetails.HeaderHasStopDate;
                            continue;
                        }
                        if (tokens.Length == 2 && tokens[0] == "Latitude")
                        {
                            lineType = lineType | DataLineDetails.HeaderHasLatLong;
                            continue;
                        }
                        if (tokens[0] == "Date")
                        {
                            if (tokens[6] == "Data")
                            {
                                lineType = lineType | DataLineDetails.HasDataAttribute;
                            }
                            if (tokens[6] == "Sensor" || tokens[7] == "Sensor")
                            {
                                lineType = lineType | DataLineDetails.HasSensorAttribute;
                            }
                            if (tokens[tokens.Length-1] == "Longitude")
                            {
                                lineType = lineType | DataLineDetails.HeaderHasLatLong;
                            }
                            break;
                        }
                        if (tokens.Length > 2 && tokens[2] == "Battery")
                        {
                            AddLineToBatteryStatus(database, fileId, lineNumber,
                                ParseSrx400DateTime(tokens[0] + " " + tokens[1], lineNumber), tokens[3]);
                            break;
                        }
                        if (tokens[0] == "GPS")
                        {
                            if (tokens[3] == "OFF")
                            {
                                if ((lineType & DataLineDetails.GpsModeIsOn) == DataLineDetails.GpsModeIsOn)
                                    lineType = lineType ^ DataLineDetails.GpsModeIsOn;
                                continue;
                            }
                            if (tokens[3] == "ON")
                            {
                                AddLineToLocations(database, fileId, lineNumber, null,
                                    String.Join(" ", tokens.Skip(6).ToArray()));
                                lineType = lineType | DataLineDetails.GpsModeIsOn;
                            }
                            else
                                throw new FormatException("Unexpected GPS data mode in data table at line " + lineNumber);
                            break;
                        }
                        AddLineToTracking(database, fileId, lineNumber, lineType, tokens);
                        break;

                    case ParseState.End:
                        if (line != String.Empty)
                            throw new InvalidDataException("Unexpected text after the data table at line " + lineNumber);
                        break;

                    default:
                        throw new InvalidOperationException("Unhandled ParseState: " + state);
                }
            }
            if (state != ParseState.End && state != ParseState.DataTable)
                throw new InvalidDataException("Data file ended prematurely");
        }

        private static bool ParseAntennaKeyValue(string line, string antennaType, Dictionary<String, String> attributes)
        {
            var sentinals = new Dictionary<String, String>
            {
                {"Master=", "Master"},
                {"Gain=", "Gain"},
                {"Priority scan", "PriorityScanState"},
            };
            foreach (KeyValuePair<string, string> sentinal in sentinals)
            {
                if (line.StartsWith(sentinal.Key))
                {
                    attributes[antennaType+sentinal.Value] = line.Substring(sentinal.Key.Length).Trim();
                    return true;
                }
            }
            return false;
        }

        private static bool ParseKeyValue(string line, Dictionary<String, String> attributes)
        {
            var sentinals = new Dictionary<String, String>
            {
                {"Site number", "Site"},
                {"Available memory", "Memory"},
                {"Codeset =", "CodeSet"},
                {"AGC", "AGC"},
                {"Scan time =", "ScanTime"},
                {"Scan delay =", "ScanDelay"},
                {"Active partition =", "ActivePartition"},
                {"Continuous record time-out =", "ContinuousRecordTimeout"},
                {"Noise blank level =", "NoiseBlankLevel"},
                {"Upconverter base frequency =", "UpconverterBaseFrequency"},
                {"Filter:", "Filter"},
                {"Echo delay =", "EchoDelay"},
                {"GPS data mode =", "GpsMode"}
            };
            foreach (KeyValuePair<string, string> sentinal in sentinals)
            {
                if (line.StartsWith(sentinal.Key))
                {
                    attributes[sentinal.Value] = line.Substring(sentinal.Key.Length).Trim();
                    return true;
                }
            }
            return false;
        }

        private static DateTime ParseSrx400DateTime(string text, int lineNumber)
        {
            try
            {
                // Date format appears to be [ws]ddddd hh:mm:ss[ws]; where ws = whitespace, hh = 00-23, mm = 00-59; ss = 00-59
                // ddddd appears to be the number of days since 1900-Jan-0 (Excel date format)
                // assume the erroneously include 1 for the date 1900-Feb-29, per the excel "feature". See http://www.cpearson.com/excel/datetime.htm
                // so we subtract 2 days from Jan 1, 1900, one to get back to Jan 0, and one to remove Feb 29.
                // TimeSpan.Parse() expects time interval format of [ws][-]{ d | [d.]hh:mm[:ss[.ff]] }[ws]
                var timeSpan = TimeSpan.Parse(text.Trim().Replace(' ', '.'));
                var date = new DateTime(1900, 1, 1) + timeSpan - new TimeSpan(2, 0, 0, 0);
                return date;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException || ex is FormatException ||
                    ex is ArgumentNullException || ex is OverflowException)
                    throw new FormatException(String.Format("Date/Time format is unrecognized at line {0}", lineNumber), ex);
                throw;
            }
        }

        private static Location ParseSrx400LatLong(string[] tokens, int lineNumber)
        {
            // input in the following example forms: 
            //    "60 12 29.8 N 154 17 21.8 W"
            //    "   N.A.         N.A.      "
            //    " 0 00 00.0 N 0 00 00.0 E"
            try
            {

                if (tokens[0] == "N.A." && tokens[1] == "N.A.")
                    return new Location {Lat = null, Long = null};
                if (tokens[0] == "N.A.")
                    return new Location { Lat = null, Long = BuildLatLong(tokens[1],tokens[2],tokens[3],tokens[4]) };
                if (tokens[4] == "N.A.")
                    return new Location { Lat = BuildLatLong(tokens[0], tokens[1], tokens[2], tokens[3]), Long = null };
                return new Location { Lat  = BuildLatLong(tokens[0], tokens[1], tokens[2], tokens[3]), 
                                      Long = BuildLatLong(tokens[4], tokens[5], tokens[6], tokens[7]) };
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException || ex is FormatException ||
                    ex is ArgumentNullException || ex is OverflowException)
                    throw new FormatException(String.Format("Coordinate format is unrecognized at line {0}", lineNumber), ex);
                throw;
            }
        }

        private static float BuildLatLong(string degrees, string minutes, string seconds, string direction)
        {
            int dir = (direction == "N" || direction == "E") ? 1 : ((direction == "W" || direction == "S") ? -1 : 0);
            if (dir == 0)
                throw new FormatException("Direction of Lat/Long is not one of N,S,E,W");
            int d = Int32.Parse(degrees);
            if ((direction == "N" || direction == "S") && (d < 0 || 90 < d))
                throw new FormatException("Magnitude of Latitude is invalid. Must be (0..90)");
            if ((direction == "E" || direction == "W") && (d < 0 || 180 < d))
                throw new FormatException("Magnitude of Longitude is invalid. Must be (0..180)");
            int m = Int32.Parse(minutes);
            if (m < 0 || 60 <= m)
                throw new FormatException("Magnitude of minutes is invalid. Must be (0..90)");
            float s = Single.Parse(seconds);
            if (s < 0 || 60 <= s)
                throw new FormatException("Magnitude of seconds is invalid. Must be (0..90)");
            return dir * (d + m / 60.0f + s / 3600f);
        }

        private static IEnumerable<string> GetLines(Byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, 0, bytes.Length))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                while (!reader.EndOfStream)
                    yield return reader.ReadLine();
        }

        private static void ClearDatabase(SqlConnection database, SqlInt32 fileId)
        {
            foreach (string table in new[]
            {
                "TelemetryDataSRX400Antennas",  "TelemetryDataSRX400BatteryStatus",
                "TelemetryDataSRX400Channels",  "TelemetryDataSRX400Environments",
                "TelemetryDataSRX400Filters",   "TelemetryDataSRX400Locations",
                "TelemetryDataSRX400TrackingData"
            })
            {
                var sql = "DELETE [dbo].[" + table + "] WHERE FileId = @FileId";
                using (var command = new SqlCommand(sql, database))
                {
                    command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void AddLineToChannels(SqlConnection database, SqlInt32 fileId, int lineNumber, DateTime dateTime, string[] tokens)
        {
            const string sql = "INSERT INTO [dbo].[TelemetryDataSRX400Channels] (FileId, LineNumber, ChangeDate, Channel, Frequency)" +
                               " VALUES (@FileId, @LineNumber, @ChangeDate, @Channel, @Frequency)";
            using (var command = new SqlCommand(sql, database))
            {
                command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                command.Parameters.Add(new SqlParameter("@LineNumber", SqlDbType.Int) { Value = lineNumber });
                command.Parameters.Add(new SqlParameter("@ChangeDate", SqlDbType.DateTime2) { Value = dateTime });
                try
                {
                    command.Parameters.Add(new SqlParameter("@Channel", SqlDbType.Int) { Value = Int32.Parse(tokens[0]) });
                    command.Parameters.Add(new SqlParameter("@Frequency", SqlDbType.Real) { Value = Single.Parse(tokens[1]) });
                }
                catch (Exception ex)
                {
                    if (ex is FormatException || ex is ArgumentNullException || ex is OverflowException)
                        throw new FormatException("Unable to parse channel data at line " + lineNumber, ex);
                    throw;
                }
                command.ExecuteNonQuery();
            }
        }

        private static void AddLineToAntennas(SqlConnection database, SqlInt32 fileId, int lineNumber, DateTime dateTime, string deviceType, string[] tokens)
        {
            const string sql = "INSERT INTO [dbo].[TelemetryDataSRX400Antennas] (FileId, LineNumber, ChangeDate, DeviceType, DeviceId, Gain)" +
                               " VALUES (@FileId, @LineNumber, @ChangeDate, @DeviceType, @DeviceId, @Gain)";
            using (var command = new SqlCommand(sql, database))
            {
                command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                command.Parameters.Add(new SqlParameter("@LineNumber", SqlDbType.Int) { Value = lineNumber });
                command.Parameters.Add(new SqlParameter("@ChangeDate", SqlDbType.DateTime2) { Value = dateTime });
                command.Parameters.Add(new SqlParameter("@DeviceType", SqlDbType.NVarChar) { Value = deviceType });
                try
                {
                    command.Parameters.Add(new SqlParameter("@DeviceId", SqlDbType.Int) { Value = Int32.Parse(tokens[0]) });
                    command.Parameters.Add(new SqlParameter("@Gain", SqlDbType.Int) { Value = Int32.Parse(tokens[1]) });
                }
                catch (Exception ex)
                {
                    if (ex is FormatException || ex is ArgumentNullException || ex is OverflowException)
                        throw new FormatException("Unable to parse antenna data at line " + lineNumber, ex);
                    throw;
                }
                command.ExecuteNonQuery();
            }
        }

        private void AddLineToFilters(SqlConnection database, SqlInt32 fileId, int lineNumber, DateTime dateTime, string[] tokens)
        {
            const string sql = "INSERT INTO [dbo].[TelemetryDataSRX400Filters] (FileId, LineNumber, ChangeDate, Channel, Code)" +
                               " VALUES (@FileId, @LineNumber, @ChangeDate, @Channel, @Code)";
            using (var command = new SqlCommand(sql, database))
            {
                command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                command.Parameters.Add(new SqlParameter("@LineNumber", SqlDbType.Int) { Value = lineNumber });
                command.Parameters.Add(new SqlParameter("@ChangeDate", SqlDbType.DateTime2) { Value = dateTime });
                try
                {
                    command.Parameters.Add(new SqlParameter("@Channel", SqlDbType.Int) { Value = Int32.Parse(tokens[0]) });
                    command.Parameters.Add(new SqlParameter("@Code", SqlDbType.Int) { Value = Int32.Parse(tokens[1]) });
                }
                catch (Exception ex)
                {
                    if (ex is FormatException || ex is ArgumentNullException || ex is OverflowException)
                        throw new FormatException("Unable to parse filter data at line " + lineNumber, ex);
                    throw;
                }
                command.ExecuteNonQuery();
            }
        }

        private static void AddLineToLocations(SqlConnection database, SqlInt32 fileId, int lineNumber, DateTime? changeDateTime, string line)
        {
            var location = ParseSrx400LatLong(line.Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries),
                lineNumber);
            const string sql = "INSERT INTO [dbo].[TelemetryDataSRX400Locations] (FileId, LineNumber, ChangeDate, Latitude, Longitude)" +
                               " VALUES (@FileId, @LineNumber, @ChangeDate, @Latitude, @Longitude)";
            using (var command = new SqlCommand(sql, database))
            {
                command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                command.Parameters.Add(new SqlParameter("@LineNumber", SqlDbType.Int) { Value = lineNumber });
                command.Parameters.Add(new SqlParameter("@ChangeDate", SqlDbType.DateTime2) { IsNullable = true, Value = changeDateTime });
                command.Parameters.Add(new SqlParameter("@Latitude", SqlDbType.Real) { IsNullable = true, Value = location.Lat });
                command.Parameters.Add(new SqlParameter("@Longitude", SqlDbType.Real) { IsNullable = true, Value = location.Long });
                command.ExecuteNonQuery();
            }
        }

        private static void AddLineToBatteryStatus(SqlConnection database, SqlInt32 fileId, int lineNumber, DateTime dateTime, string status)
        {
            const string sql = "INSERT INTO [dbo].[TelemetryDataSRX400BatteryStatus] (FileId, LineNumber, TimeStamp, Status)" +
                               " VALUES (@FileId, @LineNumber, @TimeStamp, @Status)";
            using (var command = new SqlCommand(sql, database))
            {
                command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                command.Parameters.Add(new SqlParameter("@LineNumber", SqlDbType.Int) { Value = lineNumber });
                command.Parameters.Add(new SqlParameter("@TimeStamp", SqlDbType.DateTime2) { Value = dateTime });
                command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });
                command.ExecuteNonQuery();
            }
        }

        private static void AddLineToTracking(SqlConnection database, SqlInt32 fileId, int lineNumber, DataLineDetails lineType, string[] tokens)
        {
            try
            {
                int inc1 = 0; //additional increment for lines that have a data attribute
                int inc2 = 0; //additional increment for lines that have a sensor attribute
                int inc3 = 0; //additional increment for lines that have a stop date
                var startDate = ParseSrx400DateTime(tokens[0] + " " + tokens[1], lineNumber);
                string data = null;
                if ((lineType & DataLineDetails.HasDataAttribute) == DataLineDetails.HasDataAttribute)
                {
                    data = tokens[6];
                    inc1 = 1;
                }
                if ((lineType & DataLineDetails.HasSensorAttribute) == DataLineDetails.HasSensorAttribute)
                {
                    data = tokens[6+inc1];
                    inc2 = 1;
                }
                DateTime? stopDate = null;
                if ((lineType & DataLineDetails.HeaderHasStopDate) == DataLineDetails.HeaderHasStopDate ||
                    (lineType & DataLineDetails.GpsModeIsOn) == 0)
                {
                    stopDate = ParseSrx400DateTime(tokens[7 + inc1 + inc2] + " " + tokens[8 + inc1 + inc2], lineNumber);
                    inc3 = 2;
                }
                var latLong = new Location { Lat = null, Long = null };
                if ((lineType & DataLineDetails.HeaderHasLatLong) == DataLineDetails.HeaderHasLatLong &&
                    (lineType & DataLineDetails.GpsModeIsOn) == DataLineDetails.GpsModeIsOn)
                {
                    latLong = ParseSrx400LatLong(tokens.Skip(7 + inc1 + inc2 + inc3).ToArray(), lineNumber);
                }
                const string sql = "INSERT INTO [dbo].[TelemetryDataSRX400TrackingData] (FileId, LineNumber, Date, Channel, Code, Antenna, Power, Data, Sensor, Events, Latitude, Longitude, StopDate)" +
                                   " VALUES (@FileId, @LineNumber, @Date, @Channel, @Code, @Antenna, @Power, @Data, @Sensor, @Events, @Latitude, @Longitude, @StopDate)";
                using (var command = new SqlCommand(sql, database))
                {
                    command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                    command.Parameters.Add(new SqlParameter("@LineNumber", SqlDbType.Int) { Value = lineNumber });
                    command.Parameters.Add(new SqlParameter("@Date", SqlDbType.DateTime2) { Value = startDate });
                    command.Parameters.Add(new SqlParameter("@Channel", SqlDbType.Int) { Value = Int32.Parse(tokens[2]) });
                    command.Parameters.Add(new SqlParameter("@Code", SqlDbType.Int) { Value = Int32.Parse(tokens[3]) });
                    command.Parameters.Add(new SqlParameter("@Antenna", SqlDbType.NVarChar) { Value = tokens[4] });
                    command.Parameters.Add(new SqlParameter("@Power", SqlDbType.Int) { Value = Int32.Parse(tokens[5]) });
                    command.Parameters.Add(new SqlParameter("@Data", SqlDbType.NVarChar) { IsNullable = true, Value = data });
                    command.Parameters.Add(new SqlParameter("@Sensor", SqlDbType.NVarChar) { IsNullable = true, Value = data });
                    command.Parameters.Add(new SqlParameter("@Events", SqlDbType.Int) { Value = Int32.Parse(tokens[6 + inc1 + inc2]) });
                    command.Parameters.Add(new SqlParameter("@Latitude", SqlDbType.Real) { IsNullable = true, Value = latLong.Lat });
                    command.Parameters.Add(new SqlParameter("@Longitude", SqlDbType.Real) { IsNullable = true, Value = latLong.Long });
                    command.Parameters.Add(new SqlParameter("@StopDate", SqlDbType.DateTime) { IsNullable = true, Value = stopDate });
                    command.ExecuteNonQuery();
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new FormatException("Unexpected token count ("+tokens.Length+") in line "+lineNumber +". Flags = "+ lineType);
            }

        }

        private static void WriteEnvironmentToDatabase(SqlConnection database, SqlInt32 fileId, int lineNumber, DateTime dateTime, Dictionary<String, String> attributes)
        {
            if (attributes.Count == 0)
                throw new InvalidDataException("There are environment settings for the change at line " + lineNumber);

            var sql = "INSERT INTO [dbo].[TelemetryDataSRX400Environments] (FileId, LineNumber, ChangeDate, " +
                      String.Join(", ", attributes.Keys.ToArray()) + ")" +
                      " VALUES (@FileId, @LineNumber, @ChangeDate, @" +
                      String.Join(", @", attributes.Keys.ToArray()) + ")";
            using (var command = new SqlCommand(sql, database))
            {
                command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                command.Parameters.Add(new SqlParameter("@LineNumber", SqlDbType.Int) { Value = lineNumber });
                command.Parameters.Add(new SqlParameter("@ChangeDate", SqlDbType.DateTime2) { Value = dateTime });
                foreach (KeyValuePair<string, string> keyValuePair in attributes)
                {
                    command.Parameters.Add(new SqlParameter("@"+keyValuePair.Key, SqlDbType.NVarChar) { Value = keyValuePair.Value });
                }
                command.ExecuteNonQuery();
            }
        }


    }
}
