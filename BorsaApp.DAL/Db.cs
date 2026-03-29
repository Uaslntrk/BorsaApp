
using Microsoft.Data.SqlClient;
using System.Data;

namespace WpfLibrary1
{
    public class Db
    {
        private static string? _connectionString;

        public static string ConnectionString => _connectionString ?? string.Empty;

        public static void Init(string connectionString)
            => _connectionString = connectionString;

        public static IDbConnection Create()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("Db.Init(connectionString) �a�r�lmad�.");

            return new SqlConnection(_connectionString);
        }
    }

}
