using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace OpenNetTools.Logger
{
    public class DbLog : ILog
    {
        private readonly LogConfig logConfig;

        public bool IsEnabled
        {
            get
            {
                return logConfig.DatabaseLogSource.Supress == 0;
            }
        }

        public void Initialize()
        {
            Cleanup();
        }

        public void WriteLog(
            LogState state,
            List<string> stackList,
            string topic,
            string description,
            string category,
            string user,
            string custom)
        {
            const string queryFormat = @"
            INSERT INTO errors 
                ([datetime], logname, logstate, stackinfo, topic, description, username, custom)
            VALUES
                (GETDATE(), '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')
            ";
            string query = string.Format(
                                         queryFormat,
                                         category,
                                         state,
                                         string.Join(@"\n", stackList.ToArray()),
                                         topic,
                                         description,
                                         user,
                                         custom);
            ExecuteQuery(query);
        }

        public void Cleanup()
        {
            int purgeDays = logConfig.PurgeDays;
            const string queryFormat = @"
            DELETE FROM errors 
            WHERE [datetime] < '{0}'
            ";
            string query = string.Format(
                                         queryFormat,
                                         DateTime.Now.AddDays(-1*purgeDays).Date.ToString("yyyy-MM-dd"));
            ExecuteQuery(query);
        }

        private void ExecuteQuery(string query)
        {
            string connectionString = logConfig.DatabaseLogSource.ConnectionString;
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(
                    query,
                    sqlConnection))
                {
                    sqlConnection.Open();
                    command.ExecuteNonQuery();
                    sqlConnection.Close();
                }
            }         
        }

        internal DbLog(LogConfig _logConfig)
        {
            logConfig = _logConfig;
        }


    }
}