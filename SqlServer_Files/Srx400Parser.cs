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
    class Srx400Parser
    {
        private readonly Byte[] _bytes;

        public Srx400Parser(Byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                throw new ArgumentNullException("bytes", "byte array must not be null or empty");
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
            GpsMode,
            ScanTable,
            DataTable,
            End
        }

        public void ParseFileIntoDatabase(SqlInt32 fileId, SqlConnection database)
        {
            ClearDatabase(database, fileId);
            var state = ParseState.Start;
            var changeDateTime = new DateTime();
            int changeLineNumber = 0;
            bool haveScanTable = false;
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
                        if (line.StartsWith("changed: "))
                        {
                            WriteEnvironmentToDatabase(database, fileId, changeLineNumber, changeDateTime, environment);
                            environment.Clear();
                            changeDateTime = ParseSrx400DateTime(line.Substring(9), lineNumber);
                        }
                        if (line.Trim() == "Code_log data:")
                        {
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
                        AddLineToAntennas(database, fileId, lineNumber, changeDateTime, "Antenna", line.Trim().Split());
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
                            state = ParseState.AntennaDetails;
                            break;
                        }
                        if (line.StartsWith("Hydrophone   Gain"))
                            continue;
                        AddLineToAntennas(database, fileId, lineNumber, changeDateTime, "Hydrophone", line.Trim().Split());
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

                    case ParseState.GpsMode:
                        if (line == String.Empty)
                        {
                            state = ParseState.ChangeSet;
                            break;
                        }
                        //TODO skip headers,
                        //TODO read lat/long lines, add to Location table
                        break;

                    case ParseState.ScanTable:
                        if (line == String.Empty)
                        {
                            state = ParseState.ChangeSet;
                            break;
                        }
                        if (line.Trim() == "CHANNEL  FREQUENCY")
                            continue;
                        AddLineToChannels(database, fileId, lineNumber, changeDateTime, line.Trim().Split());
                        haveScanTable = true;
                        break;

                    case ParseState.DataTable:
                        if (line == String.Empty)
                        {
                            state = ParseState.End;
                            break;
                        }
                        if (line.StartsWith(" "))
                            continue;
                        //FIXME - I need to check for "   GPS data mode ON/OFF"
                        //FIXME GPS mode on sets the reference position, needs to be written to the GPS table
                        //cases:
                        // date battery ...
                        // GPS data mode ... date unknown...
                        // date ... date
                        // date ... lat/lon
                        AddLineToTracking(database, fileId, lineNumber, line.Trim());
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

        private bool ParseAntennaKeyValue(string line, string antennaType, Dictionary<String, String> attributes)
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

        private bool ParseKeyValue(string line, Dictionary<String, String> attributes)
        {
            var sentinals = new Dictionary<String, String>
            {
                {"Site number", "Site"},
                {"Available memory", "Memory"},
                {"Codeset", "CodeSet"},
                {"AGC", "AGC"},
                {"Scan time =", "ScanTime"},
                {"Scan delay =", "ScanDelay"},
                {"Active partition =", "ActivePartition"},
                {"Continuous record time-out =", "ContinuousRecordTimeout"},
                {"Noise blank level =", "NoiseBlankLevel"},
                {"Upconverter base frequency =", "UpconverterBaseFrequency"},
                {"Filter:", "Filter"},
                {"Echo delay =", "EchoDelay"}
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

        private DateTime ParseSrx400DateTime(string text, int lineNumber)
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

        private float? ParseSrx400LatLong(string text, int lineNumber)
        {
            try
            {
                if (text == "N.A.")
                    return null;
                throw new NotImplementedException();
                return 0.0f;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException || ex is FormatException ||
                    ex is ArgumentNullException || ex is OverflowException)
                    throw new FormatException(String.Format("Coordinate format is unrecognized at line {0}", lineNumber), ex);
                throw;
            }
        }

        private IEnumerable<string> GetLines(Byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, 0, bytes.Length))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                while (!reader.EndOfStream)
                    yield return reader.ReadLine();
        }

        private void ClearDatabase(SqlConnection database, SqlInt32 fileId)
        {
            foreach (string table in new[] { "TelemetryDataSRX400Channels", "TelemetryDataSRX400Antennas", "TelemetryDataSRX400Tracking", "TelemetryDataSRX400Environment"})
            {
                var sql = "DELETE [dbo].[" + table + "] WHERE FileId = @FileId";
                using (var command = new SqlCommand(sql, database))
                {
                    command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddLineToChannels(SqlConnection database, SqlInt32 fileId, int lineNumber, DateTime dateTime, string[] tokens)
        {
            const string sql = "INSERT INTO [dbo].[TelemetryDataSRX400Channels] (FileId, LineNumber, ChangeDate, Channel, Frequency)" +
                               " VALUES (@FileId, @LineNumber, @ChangeDate, @Channel, @Frequency)";
            using (var command = new SqlCommand(sql, database))
            {
                command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                command.Parameters.Add(new SqlParameter("@LineNumber", SqlDbType.Int) { Value = lineNumber });
                command.Parameters.Add(new SqlParameter("@ChangeDate", SqlDbType.DateTime2) { Value = dateTime });
                command.Parameters.Add(new SqlParameter("@Channel", SqlDbType.Int) { Value = Int32.Parse(tokens[0]) });
                command.Parameters.Add(new SqlParameter("@Frequency", SqlDbType.Real) { Value = Single.Parse(tokens[1]) });
                command.ExecuteNonQuery();
            }
        }

        private void AddLineToAntennas(SqlConnection database, SqlInt32 fileId, int lineNumber, DateTime dateTime, string deviceType, string[] tokens)
        {
            const string sql = "INSERT INTO [dbo].[TelemetryDataSRX400Antennas] (FileId, LineNumber, ChangeDate, DeviceType, DeviceId, Gain)" +
                               " VALUES (@FileId, @LineNumber, @ChangeDate, @DeviceType, @DeviceId, @Gain)";
            using (var command = new SqlCommand(sql, database))
            {
                command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                command.Parameters.Add(new SqlParameter("@LineNumber", SqlDbType.Int) { Value = lineNumber });
                command.Parameters.Add(new SqlParameter("@ChangeDate", SqlDbType.DateTime2) { Value = dateTime });
                command.Parameters.Add(new SqlParameter("@DeviceType", SqlDbType.NVarChar) { Value = deviceType });
                command.Parameters.Add(new SqlParameter("@DeviceId", SqlDbType.Int) { Value = Int32.Parse(tokens[0]) });
                command.Parameters.Add(new SqlParameter("@Gain", SqlDbType.Int) { Value = Int32.Parse(tokens[1]) });
                command.ExecuteNonQuery();
            }
        }

        private void AddLineToTracking(SqlConnection database, SqlInt32 fileId, int lineNumber, string line)
        {
            //FIXME - different line types need different processing
            //FIXME - lat long need special parsing
            //FIXME - first two tokens need to be combined as one token for date parser
            //FIXME - build database table
            string[] tokens = line.Split();
            const string sql = "INSERT INTO [dbo].[TelemetryDataSRX400Tracking] (FileId, LineNumber, Date, Channel, Code, Antenna, Power, Data, Events, Battery, Latitude, Longitude, StopDate)" +
                               " VALUES (@FileId, @LineNumber, @Date, @Channel, @Code, @Antenna, @Power, @Data, @Events, @Battery, @Latitude, @Longitude, @StopDate)";
            using (var command = new SqlCommand(sql, database))
            {
                command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                command.Parameters.Add(new SqlParameter("@LineNumber", SqlDbType.Int) { Value = lineNumber });
                command.Parameters.Add(new SqlParameter("@Date", SqlDbType.DateTime) { Value = Int32.Parse(tokens[0]) });
                command.Parameters.Add(new SqlParameter("@Channel", SqlDbType.Int) { Value = Int32.Parse(tokens[1]) });
                command.Parameters.Add(new SqlParameter("@Code", SqlDbType.Int) { Value = Int32.Parse(tokens[2]) });
                command.Parameters.Add(new SqlParameter("@Antenna", SqlDbType.NVarChar) { Value = Int32.Parse(tokens[3]) });
                command.Parameters.Add(new SqlParameter("@Power", SqlDbType.Int) { Value = Int32.Parse(tokens[4]) });
                command.Parameters.Add(new SqlParameter("@Data", SqlDbType.Int) { IsNullable = true, Value = tokens[5] == "NA" ? null : (int?)Int32.Parse(tokens[5]) });
                command.Parameters.Add(new SqlParameter("@Events", SqlDbType.Int) { IsNullable = true, Value = tokens[6] == "N.A." ? null : (int?)Int32.Parse(tokens[6]) });
                command.Parameters.Add(new SqlParameter("@Battery", SqlDbType.NVarChar) { Value = Int32.Parse(tokens[10]) });
                command.Parameters.Add(new SqlParameter("@Latitude", SqlDbType.Real) { Value = Single.Parse(tokens[8]) });
                command.Parameters.Add(new SqlParameter("@Longitude", SqlDbType.Real) { Value = Single.Parse(tokens[9]) });
                command.Parameters.Add(new SqlParameter("@StopDate", SqlDbType.DateTime) { Value = Int32.Parse(tokens[10]) });
                command.ExecuteNonQuery();
            }
        }

        private void WriteEnvironmentToDatabase(SqlConnection database, SqlInt32 fileId, int lineNumber, DateTime dateTime, Dictionary<String, String> attributes)
        {
            if (attributes.Count == 0)
                throw new InvalidDataException("There are environment settings for the change at line " + lineNumber);

            var sql = "INSERT INTO [dbo].[TelemetryDataSRX400Environment] (FileId, LineNumber, ChangeDate, " +
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
