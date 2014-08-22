using System;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using Microsoft.SqlServer.Server;

namespace SqlServer_Functions
{
    public class SimpleFunctions
    {
        [SqlFunction]
        public static SqlDateTime LocalTime(SqlDateTime utcDateTime)
        {
            if (utcDateTime.IsNull)
                return SqlDateTime.Null;
            return new SqlDateTime(utcDateTime.Value.ToLocalTime());
        }

        [SqlFunction]
        public static SqlDateTime UtcTime(SqlDateTime localDateTime)
        {
            if (localDateTime.IsNull)
                return SqlDateTime.Null;
            return new SqlDateTime(localDateTime.Value.ToUniversalTime());
        }

        [SqlFunction(IsDeterministic = true, IsPrecise = true)]
        public static SqlDateTime DateTimeFromAts(int year, int days, int hours, int minutes)
        {
            var datetime = new DateTime(2000 + year, 1, 1) + new TimeSpan(days-1, hours, minutes, 0);
            return new SqlDateTime(datetime);
        }

        [SqlFunction(IsDeterministic = true, IsPrecise = true)]
        public static SqlDateTime DateTimeFromAtsWithSeconds(int year, int days, int hours, int minutes, int seconds)
        {
            var datetime = new DateTime(2000 + year, 1, 1) + new TimeSpan(days-1, hours, minutes, seconds);
            return new SqlDateTime(datetime);
        }

        [SqlFunction(IsDeterministic = true, IsPrecise = true)]
        public static SqlBinary Sha1Hash(SqlBytes data)
        {
            return (new SHA1CryptoServiceProvider()).ComputeHash(data.Stream);
        }
    }
}
