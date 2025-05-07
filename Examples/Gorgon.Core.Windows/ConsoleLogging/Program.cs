using Gorgon.Core;
using Gorgon.Diagnostics;

namespace Gorgon.Examples;

/// <summary>
/// An example that shows how to use the logging interface to log data in a log text file.
/// </summary>
internal class Program
{
    /// <summary>
    /// The entry point for our application.
    /// </summary>
    [STAThread()]
    static void Main()
    {
        IGorgonLog? log = null;

        try
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string assemblyName = typeof(Program).Assembly.GetName().Name ?? throw new ArgumentEmptyException("The name of the assembly could not be retrieved.");

            log = new GorgonLogConsole(assemblyName, new Version(1, 0, 0, 0))
            {
                // By setting the filter level, we can filter out more verbose messages. 
                // In this case, log messages sent with a logging level of Intermediate, or lower priority, will be sent to the log.
                // Logging levels with higher priority (e.g. Verbose) will be ignored.
                // Messages sent with a logging level of All will always be sent.
                LogFilterLevel = LoggingLevel.Intermediate
            };

            // This begins our logging by initializing the logging provider.
            log.LogStart();

            // Print will just print out arbitrary information text.
            log.Print("This is an information message. It will always be logged.", LoggingLevel.All);
            // As noted in the text here, this message will not make it to the log file. This is due to the filter level we've applied to the log above.
            log.Print("This is an information message. We will not see this one, because its logging level is set to Verbose.", LoggingLevel.Verbose);
            log.Print("This information message will be sent because it is Intermediate.", LoggingLevel.Intermediate);
            log.Print("This information message will be sent because it is Simple.", LoggingLevel.Simple);

            // Allow all logging to pass through.
            log.LogFilterLevel = LoggingLevel.All;

            log.Print("Click the buttons on the window to show other log messages.", LoggingLevel.All);

            Application.Run(new FormMain()
            {
                Log = log
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"There was an unhandled error in the example:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // This ends our logging, and cleans up any provider specific functionality.
            log?.LogEnd();
        }
    }
}
