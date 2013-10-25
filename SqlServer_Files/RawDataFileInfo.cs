using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

namespace SqlServer_Files
{
    public class RawDataFileInfo
    {
        [SqlProcedure]
        public static void ProcessRawDataFile(SqlInt32 fileId)
        {
            Byte[] bytes = null;
            using (var connection = new SqlConnection("context connection=true"))
            {
                connection.Open();
                string sql = "SELECT [Contents] FROM [dbo].[RawDataFiles] WHERE [FileId] = @fileId";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add(new SqlParameter("@fileId", SqlDbType.Int) { Value = fileId });
                    using (SqlDataReader results = command.ExecuteReader())
                    {
                        while (results.Read())
                        {
                            bytes = results.GetSqlBytes(0).Buffer;
                        }
                    }
                }
                if (bytes == null)
                    throw new InvalidOperationException("File not found: " + fileId);
                try
                {
                    IParser parser = selectParser(bytes);
                    if (parser == null) throw new InvalidOperationException("file contents is not a known format");
                    ClearErrors(connection, fileId);
                    parser.ParseFileIntoDatabase(fileId, connection);
                }
                catch (Exception ex)
                {
                    LogError(connection, fileId, ex.Message);
                }
                finally
                {
                    sql = "UPDATE [dbo].[RawDataFiles] SET [ProcessingDone] = 1 WHERE FileId = @FileId";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        [SqlProcedure]
        public static void AddBlankLineToFile(SqlInt32 fileId)
        {
            Byte[] bytes = null;
            using (var connection = new SqlConnection("context connection=true"))
            {
                connection.Open();
                string sql = "SELECT [Contents] FROM [dbo].[RawDataFiles] WHERE [FileId] = @fileId";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add(new SqlParameter("@fileId", SqlDbType.Int) { Value = fileId });
                    using (SqlDataReader results = command.ExecuteReader())
                    {
                        while (results.Read())
                        {
                            bytes = results.GetSqlBytes(0).Buffer;
                        }
                    }
                }
                if (bytes == null)
                    throw new InvalidOperationException("File not found: " + fileId);
                var contents = new byte[bytes.Length+2];
                contents[0] = 0x0d;
                contents[1] = 0x0a;
                bytes.CopyTo(contents,2);
                sql = "UPDATE [dbo].[RawDataFiles] SET [Contents] = @Contents WHERE FileId = @FileId";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                    command.Parameters.Add(new SqlParameter("@Contents", SqlDbType.VarBinary) { Value = contents, Size = -1 });
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void ClearErrors(SqlConnection database, SqlInt32 fileId)
        {
            const string sql = "UPDATE [dbo].[RawDataFiles] SET [ProcessingErrors] = NULL WHERE FileId = @FileId";
            using (var command = new SqlCommand(sql, database))
            {
                command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                command.ExecuteNonQuery();
            }
        }

        private static void LogError(SqlConnection database, SqlInt32 fileId, string error)
        {
            const string sql = "UPDATE [dbo].[RawDataFiles] SET [ProcessingErrors] = @Errors WHERE FileId = @FileId";
            using (var command = new SqlCommand(sql, database))
            {
                command.Parameters.Add(new SqlParameter("@FileId", SqlDbType.Int) { Value = fileId });
                command.Parameters.Add(new SqlParameter("@Errors", SqlDbType.NVarChar) { Value = error });
                command.ExecuteNonQuery();
            }
        }

        private static IParser selectParser(byte[] bytes)
        {
            //FIXME - use header sentinals to select the correct parser;
            //TODO - make a list of known parsers, have each parser
            //TODO - implement two static methods that optionally return a static sentinal or regex for
            //TODO - some text in the first 500 bytes.
            //TODO - If that fails, then instantiate a new parser for each type and return the first one that
            //TODO - says it is valid for the data.
            IParser parser = new Srx400Parser(bytes);
            if (parser.IsValidData)
                return parser;
            parser = new ATSWinRec(bytes);
            return parser.IsValidData ? parser : null;
        }


        /*
        [SqlFunction(IsDeterministic = true, IsPrecise = true, DataAccess = DataAccessKind.Read)]
        public static char FileFormat(SqlBytes data)
        {
            //get the first line of the file
            var fileHeader = ReadHeader(data.Buffer, Encoding.UTF8, 500).Trim().Normalize();  //database for header is only 450 char
            char code = '?';
            using (var connection = new SqlConnection("context connection=true"))
            {
                connection.Open();
                const string sql = "SELECT [Header], [FileFormat], [Regex] FROM [dbo].[LookupCollarFileHeaders]";
                using (var command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader results = command.ExecuteReader())
                    {
                        while (results.Read())
                        {
                            var header = results.GetString(0).Normalize();
                            var format = results.GetString(1)[0]; //.GetChar() is not implemented
                            var regex = results.IsDBNull(2) ? null : results.GetString(2);
                            if (fileHeader.StartsWith(header, StringComparison.OrdinalIgnoreCase) ||
                                (regex != null && new Regex(regex).IsMatch(fileHeader)))
                            {
                                code = format;
                                break;
                            }
                        }
                    }
                }
            }
            if (code == '?' && (new ATSWinRec(data.Buffer)).GetPrograms().Any())
                // We already checked for ArgosAwsFile with the header
                code = 'E';
            return code;
        }

        private static string ReadHeader(Byte[] bytes, Encoding enc, int maxLength)
        {
            var length = Math.Min(bytes.Length, maxLength);
            using (var stream = new MemoryStream(bytes, 0, length))
            using (var reader = new StreamReader(stream, enc))
                return reader.ReadLine();
        }

        [SqlProcedure]
        public static void Summerize(SqlInt32 fileId)
        {
            Byte[] bytes = null;
            char format = '?';
            using (var connection = new SqlConnection("context connection=true"))
            {
                connection.Open();
                const string sql = "SELECT [Contents], [Format] FROM [dbo].[CollarFiles] WHERE [FileId] = @fileId";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add(new SqlParameter("@fileId", SqlDbType.Int) { Value = fileId });
                    using (SqlDataReader results = command.ExecuteReader())
                    {
                        while (results.Read())
                        {
                            bytes = results.GetSqlBytes(0).Buffer;
                            format = results.GetString(1)[0]; //.GetChar() is not implemented
                        }
                    }
                }
            }
            if (bytes == null)
                throw new InvalidOperationException("File not found: " + fileId);
            SummerizeFile(fileId, bytes, format);
        }





        private static void SummerizeFile(SqlInt32 fileId, Byte[] contents, char format)
        {
            TelemetryDataFile argos;
            switch (format)
            {
                case 'E':
                    argos = new ATSWinRec(contents);
                    break;
                case 'F':
                    argos = new ArgosAwsFile(contents);
                    break;
                case 'G':
                    argos = new DebevekFile(contents);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported File Format: " + format);
            }
            SummarizeArgosFile(fileId, argos);
        }

        private static void SummarizeArgosFile(SqlInt32 fileId, TelemetryDataFile file)
        {
            using (var connection = new SqlConnection("context connection=true"))
            {
                connection.Open();

                foreach (var program in file.GetPrograms())
                {
                    foreach (var platform in file.GetPlatforms(program))
                    {
                        var minDate = file.FirstTransmission(platform);
                        var maxDate = file.LastTransmission(platform);
                        const string sql = "INSERT INTO [dbo].[ArgosFilePlatformDates] (FileId, PlatformId, ProgramId, FirstTransmission, LastTransmission)" +
                                           " VALUES (@FileId, @PlatformId, @ProgramId, @FirstTransmission, @LastTransmission)";
                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.Add(new SqlParameter("@fileId", SqlDbType.Int) {Value = fileId});
                            command.Parameters.Add(new SqlParameter("@PlatformId", SqlDbType.NVarChar) {Value = platform});
                            command.Parameters.Add(new SqlParameter("@ProgramId", SqlDbType.NVarChar) {Value = program});
                            command.Parameters.Add(new SqlParameter("@FirstTransmission", SqlDbType.DateTime2) {Value = minDate});
                            command.Parameters.Add(new SqlParameter("@LastTransmission", SqlDbType.DateTime2) {Value = maxDate});
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
         * */
    }
}
