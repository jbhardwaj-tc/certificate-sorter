using SqlBulkTools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Transactions;

namespace CertificateSorter
{
    /// <summary>
    /// Define helper method to bulk update records in the database
    /// </summary>
    public static class SqlHelper
    {
        /// <summary>
        /// Updates the database with the records provided
        /// </summary>
        /// <param name="records">Records to update</param>
        public static void BulkUpdate(IEnumerable<TextValues> records)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Speednic"].ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("No connection string defined");

            var bulkManager = new BulkOperations();

            using (var scope = new TransactionScope())
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    bulkManager.Setup<TextValues>()
                        .ForCollection(records)
                        .WithTable("TextValues")
                        .AddColumn(x => x.ObjectId)
                        .AddColumn(x => x.Value)
                        .BulkUpdate()
                        .MatchTargetOn(x => x.ObjectId)
                        .Commit(connection);

                }
                scope.Complete();
            }
        }
    }
}
