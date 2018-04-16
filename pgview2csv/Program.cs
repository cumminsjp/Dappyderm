using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using Common.Logging;
using Dapper.Dappyderm;
using Npgsql.Diagnostics;
using OfficeOpenXml;

namespace pg2csv
{
    /// <summary>
    /// Program to query postgresql tables or views and create CSV files
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Entry point into program.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="System.ApplicationException">No database connection string defined.</exception>
        private static void Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var options = new CommandLineOptions();

                if (Parser.Default.ParseArguments(args, options))
                {
                    Log.Debug("Completed ParseArguments");


                    var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                    var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);

                    if (ShowHelp(options))
                    {
                        Environment.ExitCode = 1;

                        return;
                    }

                    var version = versionInfo.ProductVersion;

                    var exeinfomsg = $"{Path.GetFileName(Assembly.GetExecutingAssembly().Location)} v.{version} (last modified on {fileInfo.LastWriteTime})";

                    // Console.WriteLine(exeinfomsg);
                    Log.Info(exeinfomsg);

                    string connectionString = null;

                    var envConnectionString =
                        NpgsqlConnectionHelper.BuildConnectionString(Environment.GetEnvironmentVariables());

                    if (!string.IsNullOrEmpty(envConnectionString))
                    {
                        connectionString = envConnectionString;
                        Log.Info("Connection string read from PG environment variables.");
                    }
                    else if (!string.IsNullOrEmpty(options.DatabaseConnectionString))
                    {
                        connectionString = options.DatabaseConnectionString;
                        Log.Info("Connection string read from Command Line Parameter.");
                    }

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        Log.Error("No database connection string defined.");
                        throw new ApplicationException("No database connection string defined.");
                    }

                    int? limit = null;

                    if (options.Limit > 0)
                        limit = options.Limit;

                    Log.Info(NpgsqlConnectionHelper.GetSanitizedConnectionString(connectionString));
                    DapperHelper.WriteViewToCsv(connectionString, options.ViewName, options.OutputFile, limit, true);

                    

                    stopwatch.Stop();

                    Log.InfoFormat("{0} run duration: {1}ms", Process.GetCurrentProcess().MainModule.FileName,
                        stopwatch.ElapsedMilliseconds);


                }
                else
                    // the command line wasn't parsed.
                    Environment.ExitCode = 1;
            }
            catch (Exception ex)
            {
                // Log.Error(ex.Message, ex);

                Log.InfoFormat("{0} run duration(with error): {1}ms", Process.GetCurrentProcess().MainModule.FileName,
                    stopwatch.ElapsedMilliseconds);

                Log.Error(ex.Message, ex);

                Console.WriteLine(ex.Message);

                Environment.ExitCode = 1;
            }
        }

        /// <summary>
        /// Shows the help manual
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        private static bool ShowHelp(CommandLineOptions options)
        {
            Log.DebugFormat("Enter");

            var showHelp =
                Environment.GetCommandLineArgs()
                    .Intersect(new[] { "-h", "--help", "/?", "-?" }, StringComparer.InvariantCultureIgnoreCase)
                    .Any();

            var detailed =
                Environment.GetCommandLineArgs()
                    .Intersect(new[] { "--detail", "--detailed" }, StringComparer.InvariantCultureIgnoreCase)
                    .Any();

            Log.DebugFormat("showHelp={0},showHelp={1},", showHelp, detailed);

            if (!showHelp) return false;

            Console.WriteLine(options.GetUsage());

            if (!detailed) return true;

            var extendedHelp = GetExtendedHelpText();

            if (string.IsNullOrEmpty(extendedHelp)) return true;

            // Console.WriteLine(extendedHelp);

            // for now, use a text editor.
            var path = Path.GetTempFileName();
            var outputFilePath = Path.ChangeExtension(path, "txt");
            File.WriteAllText(outputFilePath, extendedHelp);
            Process.Start(outputFilePath);

            return true;
        }

        /// <summary>
        ///     Writes the error message to the console
        /// </summary>
        /// <param name="text">The text.</param>
        private static void ConsoleWriteErrorMessage(string text)
        {
            Log.Error(text);

            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        /// <summary>
        /// Gets the extended help text.
        /// </summary>
        /// <returns></returns>
        private static string GetExtendedHelpText()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "pgview2csv.ExtendedHelp.txt";

            // var names = assembly.GetManifestResourceNames();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                    using (var reader = new StreamReader(stream))
                    {
                        var result = reader.ReadToEnd();

                        return result;
                    }
            }

            ConsoleWriteErrorMessage("The Extended Help is missing from the exe.");

            return string.Empty;
        }

        #region Fields

        /// <summary>
        ///     Log4net log
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion Fields
    }
}