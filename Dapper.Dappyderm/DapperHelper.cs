using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Logging;
using CsvHelper;
using Npgsql;

namespace Dapper.Dappyderm
{
    public static class DapperHelper
    {
        /// <summary>
        ///     The Log (Common.Logging)
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Determines whether the specified query results is empty.
        /// </summary>
        /// <param name="queryResults">The query results.</param>
        /// <returns>
        ///     <c>true</c> if the specified query results is empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmpty(IEnumerable<dynamic> queryResults)
        {
            return queryResults == null || !queryResults.GetEnumerator().MoveNext();
        }

        /// <summary>
        ///     Dappers the query to CSV string.
        /// </summary>
        /// <param name="dapperQueryResults">The dapper query results.</param>
        /// <returns></returns>
        public static string DapperQueryToCsvString(IEnumerable<dynamic> dapperQueryResults)
        {
            var queryResults = dapperQueryResults.ToList();

            if (IsEmpty(queryResults)) return string.Empty;

            // results not empty

            var sw = new StringWriter(); // defaults to System.Text.UnicodeEncoding

            DapperQueryToTextWriter(queryResults, sw);
            return sw.ToString();
        }

        /// <summary>
        ///     Dappers the query to text writer.
        /// </summary>
        /// <param name="dapperQueryResults">The dapper query results.</param>
        /// <param name="tw">The tw.</param>
        private static void DapperQueryToTextWriter(IEnumerable<dynamic> dapperQueryResults, TextWriter tw)
        {
            var csv = new CsvWriter(tw);

            csv.Configuration.QuoteAllFields = true;

            var dapperRows = dapperQueryResults.Cast<IDictionary<string, object>>().ToList();
            var headerWritten = false;
            foreach (var row in dapperRows)
            {
                if (!headerWritten)
                {
                    foreach (var item in row) csv.WriteField(item.Key);
                    csv.NextRecord();
                    headerWritten = true;
                }

                foreach (var item in row)
                    csv.WriteField(item.Value);

                csv.NextRecord();
            }

            csv.Flush();
        }

        /// <summary>
        /// Selects all data from a database view and dumps to CSV.  This function is intended to be used against the flattened
        /// database views.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="deleteFileIfExists">if set to <c>true</c> [delete file if exists].</param>
        /// <exception cref="System.ApplicationException"></exception>
        public static void WriteViewToCsv(string connectionString, string viewName, string filePath,
            int? limit,
            bool deleteFileIfExists = false
            )
        {
            if (File.Exists(filePath))
                if (deleteFileIfExists)
                    File.Delete(filePath);
                else
                    throw new ApplicationException($"File {filePath} already exists.");

            var sql = $"select * FROM {viewName}";

            if (limit.HasValue && limit.Value > 0)
                sql = $"{sql} LIMIT {limit.Value}";

            using (var conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    var results = conn.Query(sql).ToList();
                    var output = DapperQueryToCsvString(results);
                    File.WriteAllText(filePath, output);

                    if (File.Exists(filePath))
                    {
                        Console.WriteLine($"CSV output for {viewName} successfully written to {filePath}.");
                    }
                    else
                    {
                        throw new ApplicationException($"Unknown error while generating CSV output for {viewName} to {filePath}.");
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    Log.Error(
                        $"Error: {e.Message} for sql:{sql}. viewName={viewName}, filePath={filePath}, deleteFileIfExists={deleteFileIfExists}.");
                    throw;
                }
            }
        }
    }
}