using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Common.Logging;
using Dapper;
// ReSharper disable UseStringInterpolation
// ReSharper disable once CheckNamespace

namespace Npgsql.Diagnostics
{
    /// <summary>
    ///     Helper class for Npgsql Connection.  
    /// </summary>
    public static class NpgsqlConnectionHelper
    {
        #region Fields

        /// <summary>
        ///     The Log (Common.Logging)
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion Fields

        /// <summary>
        /// Builds the NPGSQL connection string.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
        public static NpgsqlConnectionStringBuilder CreateConnectionStringBuilder(IDictionary dictionary)
        {
            var builder = new NpgsqlConnectionStringBuilder();

            var host = dictionary["PGHOST"]?.ToString();
            var user = dictionary["PGUSER"]?.ToString();
            var port = dictionary["PGPORT"]?.ToString();
            var database = dictionary["PGDATABASE"]?.ToString();
            var pwd = dictionary["PGPASSWORD"]?.ToString();

            if (!string.IsNullOrEmpty(host))
                builder.Host = host;
            
            if (!string.IsNullOrEmpty(user))
                builder.Username = user;
            
            if (!string.IsNullOrEmpty(port))
                builder.Port =  Convert.ToInt32(port);

            if (!string.IsNullOrEmpty(database))
                builder.Database = database;

            if (!string.IsNullOrEmpty(pwd))
                builder.Password = pwd;

            return builder;
        }

        /// <summary>
        /// Builds the connection string from a Dictionary (usually comes from Environment.GetEnvironmentVariables)
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
        public static string BuildConnectionString(IDictionary dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            var builder = CreateConnectionStringBuilder(dictionary);

            if (builder == null)
                throw new ApplicationException($"Could not create a connection string builder from dictionary.");
            
            return builder.ToString();
        }
        
        /// <summary>
        ///     Gets the sanitized connection string (removes password if it exists)
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <returns>
        ///     connection string without a password
        /// </returns>
        public static string GetSanitizedConnectionString(this NpgsqlConnection conn)
        {
            var builder1 = new NpgsqlConnectionStringBuilder(conn.ConnectionString);

            builder1.Remove("Password");

            return builder1.ToString();
        }

        /// <summary>
        ///     Gets the sanitized connection string (removes password if it exists)
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>
        ///     connection string without a password
        /// </returns>
        public static string GetSanitizedConnectionString(string connectionString)
        {
            var builder1 = new NpgsqlConnectionStringBuilder(connectionString);

            builder1.Remove("Password");

            return builder1.ToString();
        }

        /// <summary>
        ///     Returns a copy of the connetion string with the application name set to the new  applicationName value.
        ///     Note: This will increase the database connection pool so the Postgresql connection limit may need to be increased.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns></returns>
        public static string SetApplicationName(string connectionString, string applicationName)
        {
            var builder1 = new NpgsqlConnectionStringBuilder(connectionString) { ApplicationName = applicationName };

            return builder1.ToString();
        }

        /// <summary>
        /// Appends a string to the application name of an existing connection string.  Note: This MAY INCREASE the database connection pool so the Postgresql connection limit may need to be increased.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        public static string AppendToApplicationName(string connectionString, string applicationName, string separator = "_")
        {
            var builder1 = new NpgsqlConnectionStringBuilder(connectionString);

            if (string.IsNullOrEmpty(builder1.ApplicationName))
            {
                builder1.ApplicationName = applicationName;
            }
            else
            {
                builder1.ApplicationName = string.Format("{0}{1}{2}", builder1.ApplicationName, separator, applicationName);
            }

            return builder1.ToString();
        }

        /// <summary>
        ///     Logs the search path to debug
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <returns></returns>
        public static void LogSearchPath(this NpgsqlConnection conn)
        {
            Log.DebugFormat("");

            Log.DebugFormat("Search path: {0} ({1})", conn.GetSearchPath(), conn.GetSanitizedConnectionString());
        }

        /// <summary>
        ///     Gets the actual search path of the connection.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <returns></returns>
        public static string GetSearchPath(this NpgsqlConnection conn)
        {
            var searchPath = conn.ExecuteScalar<string>("show search_path");

            return searchPath;
        }
    }

    /// <summary>
    ///     Diagnostic Classes for the PostgreSQL .NET Data Provider Npgsql (http://www.npgsql.org/)
    /// </summary>
    [CompilerGenerated]
    internal class NamespaceDoc
    {
    }
}