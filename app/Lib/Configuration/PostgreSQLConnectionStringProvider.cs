using Npgsql;
using System;

namespace app.Lib.Configuration
{
    public class PostgreSQLConnectionStringProvider
    {
        public string HostName { get; set; } = "127.0.0.1";
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DatabaseName { get; set; } = "whoru";
        public uint Port { get; set; } = 5432;
        public string ModelName { get; set; } = "whoruPostgreSQLModel";

        public string GetNpgsqlConnectionString()
        {
            var sqlBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = HostName,
                Database = DatabaseName,
                PersistSecurityInfo = true,
                Username = UserName,
                Password = Password,
                Port = (int)Port
            };

            return sqlBuilder.ToString();
        }
    }
}