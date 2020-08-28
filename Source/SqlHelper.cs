using SqlBulkTools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Transactions;
using SimpleLogger;

namespace CertificateSorter
{
    /// <summary>
    /// Define helper method to bulk update records in the database
    /// </summary>
    public static class SqlHelper
    {
        private static string AscioOrderConnectionString = ConfigurationManager.ConnectionStrings["AscioOrder"].ConnectionString;
        private static string SpeedNicConnectionString = ConfigurationManager.ConnectionStrings["Speednic"].ConnectionString;
        private static string ScriptFilePath = ConfigurationManager.AppSettings["ScriptFilePath"];

        /// <summary>
        /// Updates the database with the records provided
        /// </summary>
        /// <param name="records">Records to update</param>
        public static void BulkUpdate(IEnumerable<TextValues> records)
        {
            Logger.Log(Logger.Level.Info, "Starting generating database update script...");

            if (File.Exists(ScriptFilePath))
            {
                using (var writer = File.AppendText(ScriptFilePath))
                {
                    foreach (var record in records)
                    {
                        var certificate = record.Certificate.ExportCertToPem().Replace("\r", string.Empty);
                        var script = $"UPDATE TextValues SET Value='{certificate}' WHERE Id={record.Id} AND ObjectId={record.ObjectId}";

                        writer.WriteLine(script);
                    }
                }
            }

            Logger.Log(Logger.Level.Info, "Finished generating database update script...");
        }

        public static string GetHandle(string fileName)
        {
            var orderId = fileName?.Replace("A", "");
            using (var connection = new SqlConnection(AscioOrderConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(Properties.Resources.GetHandleQuery, connection))
                {
                    command.Parameters.AddWithValue("@orderid", orderId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetFieldValue<string>(0);
                        }
                    }
                }
            }

            throw new Exception($"Unable to get handle for file: {fileName}");
        }

        public static TextValues ExtractTextValueData(string fileName)
        {
            var handle = GetHandle(fileName);
            using (var connection = new SqlConnection(SpeedNicConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(Properties.Resources.GetTextValueRecordQuery, connection))
                {
                    command.Parameters.AddWithValue("@handle", handle);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var id = reader.GetFieldValue<int>(0);
                            var objectId = reader.GetFieldValue<int>(1);
                            var value = reader.GetFieldValue<string>(2);
                            var objectHandle = reader.GetFieldValue<string>(3);
                            return new TextValues(id, objectId, value, objectHandle);
                        }
                    }
                }
            }

            throw new Exception($"Unable to extract text value record for file: {fileName}");
        }
    }
}
