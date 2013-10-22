using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace SqlServer_Files
{
    interface IParser
    {
        bool IsValidData { get; }

        void ParseFileIntoDatabase(SqlInt32 fileId, SqlConnection database);
    }
}
