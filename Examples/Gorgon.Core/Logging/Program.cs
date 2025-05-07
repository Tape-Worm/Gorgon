using System.Diagnostics;
using Gorgon.Core;
using Gorgon.Diagnostics;

namespace Gorgon.Examples;

/// <summary>
/// An example that shows how to use the logging interface to log data in a log text file.
/// </summary>
internal class Program
{
    // The log interface for our application.
    private static IGorgonLog? _log;

    /// <summary>
    /// An exception for our sample.
    /// </summary>
    private static void Exception3() => throw new NullReferenceException("This is our root exception.");

    /// <summary>
    /// An exception for our sample.
    /// </summary>
    private static void Exception2()
    {
        try
        {
            Exception3();
        }
        catch (Exception ex)
        {
            throw new ArgumentException("This is another exception.", ex);
        }
    }

    /// <summary>
    /// An exception for our sample.
    /// </summary>
    private static void Exception1()
    {
        try
        {
            Exception2();
        }
        catch (Exception ex)
        {
            throw new GorgonException(GorgonResult.OutOfMemory, ex);
        }
    }

    /// <summary>
    /// An exception for our sample.
    /// </summary>
    private static void TriggerException()
    {
        Debug.Assert(_log is not null, "Log is null.");

        try
        {
            Exception1();
        }
        catch (Exception ex)
        {
            throw new InvalidCastException("The final exception.", ex);
        }
    }

    /// <summary>
    /// The entry point for our application.
    /// </summary>
    static void Main()
    {
        try
        {
            string assemblyName = typeof(Program).Assembly.GetName().Name ?? throw new ArgumentEmptyException("The name of the assembly could not be retrieved.");

            Console.Clear();
            Console.Title = "Logging Example";

            _log = new GorgonTextFileLog(assemblyName, "Tape_Worm", new Version(1, 0, 0, 0))
            {
                // By setting the filter level, we can filter out more verbose messages. 
                // In this case, log messages sent with a logging level of Intermediate, or lower priority, will be sent to the log.
                // Logging levels with higher priority (e.g. Verbose) will be ignored.
                // Messages sent with a logging level of All will always be sent.
                LogFilterLevel = LoggingLevel.Intermediate
            };

            // This begins our logging by initializing the logging provider.
            _log.LogStart();

            // Logs are stored in the %LOCALAPPDATA% directory, under the Tape_Worm directory (as specified in the log constructor), and under a folder with the same name of the application.
            // The log information is stored in a text file called "ApplicationLogging.txt".
            // So, in this case our path would be (under Windows):
            // c:\users\<username>\appdata\local\Tape_Worm\Logging\ApplicationLogging.txt
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tape_Worm", assemblyName, "ApplicationLogging.txt");

            // Print will just print out arbitrary information text.
            _log.Print("This is an information message. It will always be logged.", LoggingLevel.All);
            // As noted in the text here, this message will not make it to the log file. This is due to the filter level we've applied to the log above.
            _log.Print("This is an information message. We will not see this one, because its logging level is set to Verbose.", LoggingLevel.Verbose);
            _log.Print("This information message will be sent because it is Intermediate.", LoggingLevel.Intermediate);
            _log.Print("This information message will be sent because it is Simple.", LoggingLevel.Simple);

            // Allow all logging to pass through.
            _log.LogFilterLevel = LoggingLevel.All;

            _log.PrintWarning("This is a warning message. We use this to let users know that things may not work the way they might expect.", LoggingLevel.Verbose);
            _log.PrintError("This is an error message. We use this to let users know that something is not working.", LoggingLevel.Verbose);

            // We can also log exceptions with full stack trace information, and even inner exceptions.
            try
            {
                TriggerException();
            }
            catch (Exception ex)
            {
                _log.PrintError("We can also log exceptions with full stack traces, and inner exceptions.", LoggingLevel.Verbose);
                _log.PrintException(ex);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Enter some text and press Enter:");
            Console.ResetColor();

            string customLine = Console.ReadLine() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(customLine))
            {
                _log.Print($"Custom text: \"{customLine}\"", LoggingLevel.Simple);
            }

            // This ends our logging, and cleans up any provider specific functionality.
            _log.LogEnd();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Loading the log file from '{logPath}'");
            Console.ResetColor();

            ProcessStartInfo start = new(logPath)
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Maximized
            };

            // Launch the default text editor with our log file.
            using Process process = Process.Start(start) ?? throw new ArgumentException("Unable to launch application.");
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"There was an error:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
            Console.ResetColor();
        }
    }
}
