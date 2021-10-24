using System;
using System.Collections.Generic;
using System.Linq;
using app.Lib.Model;
using Dapper;
using Npgsql;
using PostgreSQLCopyHelper;
using app.Lib.Configuration;

namespace app.Lib.QueryRepositories
{
    public class LoginEventsQueryRepository
    {
        private readonly string connectionString;
        private readonly PostgreSQLCopyHelper<StoredEvent> copyHelper;

        public LoginEventsQueryRepository(PostgreSQLConnectionStringProvider connectionStringProvider)
        {
            connectionString = connectionStringProvider.GetNpgsqlConnectionString();

            copyHelper = new PostgreSQLCopyHelper<StoredEvent>("public", "loginevents")
                .MapTimeStamp("time", x => x.Time)
                .MapVarchar("connectiontype", x => x.ConnectionType)
                .MapVarchar("level", x => x.Level)
                .MapJsonb("extraprops", x => x.ExtraProps)
                ;
        }

        public void SaveChunk(IEnumerable<StoredEvent> loginEvents)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                copyHelper.SaveAll(conn, loginEvents);
                conn.Close();
            }
        }

        public class OriginModel
        {
            public DateTime Time;
            public string Account;
            public string IPV4;
            public string IPV6;
            public string Country;
        }

        public OriginModel[] ReadEventsForAnalysis(string account, DateTime from, DateTime to)
        {
            const string query = @"
                SELECT 
                    time,
                    extraprops->>'Email' AS Account, 
                    extraprops->>'IPV4' AS IPV4 , 
                    extraprops->>'IPV6' AS IPV6 , 
                    extraprops->>'Country' AS Country
                FROM loginevents
                WHERE 
                    time BETWEEN @from AND @to AND
                    extraprops->>'Email' ILIKE @account
            ";

            var queryParameters = new { account, from,to };

            using (var conn = new NpgsqlConnection(connectionString))
            {
                return conn.Query<OriginModel>(query, queryParameters).ToArray();
            }

        }
        
    }
}