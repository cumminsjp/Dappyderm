using System;
using System.Linq;
using System.Reflection;
using CommandLine;
using CommandLine.Text;

namespace pg2csv
{
    /// <summary>
    ///     Command line options for the madmin utility exe
    /// </summary>
    public class CommandLineOptions
    {
        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        /// <value>
        /// The input file.
        /// </value>
        [Option('f', "file", Required = true, HelpText = "The output CSV file path")]
        public string OutputFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the database connection string.
        /// </summary>
        /// <value>
        /// The database connection string.
        /// </value>
        [Option('c', "connectionString", Required = false, HelpText = "Database connection string. Note: PG environment variables can also be used.")]
        public string DatabaseConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the view.
        /// </summary>
        /// <value>
        /// The name of the view.
        /// </value>
        [Option( "view", Required = true, HelpText = "The name of the view (can include schema prefix).")]
        public string ViewName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        /// <value>
        /// The limit.
        /// </value>
        [Option("limit", Required = false, HelpText = "An optional value to limit the number of rows to return (uses LIMIT).")]
        public int Limit
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the last state of the parser.
        /// </summary>
        /// <value>
        /// The last state of the parser.
        /// </value>
        [ParserState]
        public IParserState LastParserState
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the usage.
        /// </summary>
        /// <returns></returns>
        [HelpOption]
        public string GetUsage()
        {

            // Console.WriteLine(GetOperationHelpText());

            var descriptionAttribute = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
                .OfType<AssemblyDescriptionAttribute>()
                .FirstOrDefault();

            var assDescription = string.Empty;
            if (descriptionAttribute != null)
                assDescription = descriptionAttribute.Description;


            Console.WriteLine("EXE Path: {0}", Assembly.GetEntryAssembly().Location);
            Console.WriteLine("Description: {0}", assDescription);

            return HelpText.AutoBuild(this,
                current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}