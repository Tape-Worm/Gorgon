
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 26, 2018 12:49:15 PM
// 

using System.Diagnostics;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Properties;
using Gorgon.UI;

namespace Gorgon.Editor;

/// <summary>
/// The main entry point for the editor
/// </summary>
static class Program
{
    /// <summary>
    /// Property to return the directory used by the application for settings and other functionality.
    /// </summary>
    public static DirectoryInfo ApplicationUserDirectory
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the path to the directory that the executable is running from.
    /// </summary>
    public static DirectoryInfo ApplicationDirectory => GorgonApplication.StartupPath;

    /// <summary>
    /// Property to return the log used for the application.
    /// </summary>
    public static IGorgonLog Log
    {
        get;
        private set;
    }

    /// <summary>
    /// Function to locate a command line argument, and its subsequent value (if applicable).
    /// </summary>
    /// <param name="args">The arguments to search through/</param>
    /// <param name="argument">The argument to look up.</param>
    /// <returns></returns>
    private static (bool hasSwitch, string switchValue) GetCommandLineArgument(string[] args, string argument)
    {
        if ((string.IsNullOrWhiteSpace(argument))
            || (args is null)
            || (args.Length == 0))
        {
            return (false, string.Empty);
        }

        for (int i = 0; i < args.Length; ++i)
        {
            if (!string.Equals(args[i], argument, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return (i < args.Length - 1) && (!args[i + 1].StartsWith("-", StringComparison.OrdinalIgnoreCase)) && (!args[i + 1].StartsWith("/", StringComparison.OrdinalIgnoreCase))
                ? (true, args[i + 1])
                : (true, string.Empty);
        }

        return (false, string.Empty);
    }

    /// <summary>
    /// Function to start the logging for the application.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    private static void InitializeLogging(string[] args)
    {
        // Log everything.
        LoggingLevel level = LoggingLevel.Simple;

        // Logging is always active when compiled as DEBUG.
#if !DEBUG
        // Look for the log switch.
        (bool hasLogSwitch, string severity) = GetCommandLineArgument(args, "-log");

        // Logging is not active.
        if (!hasLogSwitch)
        {
            Log = GorgonApplication.Log = GorgonLog.NullLog;
            return;
        }

        if (!string.IsNullOrWhiteSpace(severity))
        {
            if (!Enum.TryParse(severity, out level))
            {
                level = LoggingLevel.Simple;
            }
        }
#endif
        (bool hasLogTypeSwitch, string logType) = GetCommandLineArgument(args, "-logtype");

        if (!hasLogTypeSwitch)
        {
#if DEBUG
            logType = "console";
#else
            logType = "file";
#endif
        }

        try
        {
            if (string.Equals(logType, "console", StringComparison.OrdinalIgnoreCase))
            {
                Log = GorgonApplication.Log = new GorgonLogConsole("Gorgon.Editor", typeof(Program).Assembly.GetName().Version)
                {
                    LogFilterLevel = level
                };
            }

            if (string.Equals(logType, "file", StringComparison.OrdinalIgnoreCase))
            {
                Log = GorgonApplication.Log = new GorgonTextFileLog("Gorgon.Editor", @"Tape_Worm\Gorgon.Editor\Logging\", typeof(Program).Assembly.GetName().Version)
                {
                    LogFilterLevel = level
                };
            }
        }
        catch (Exception ex)
        {
            Debug.Print($"Couldn't open the log for the editor: {ex.Message}.");
            Log = GorgonLog.NullLog;
        }
    }

    /// <summary>
    /// Function to initialize the application user directory.
    /// </summary>
    private static void InitApplicationUserDirectory()
    {
        DirectoryInfo dir = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tape_Worm", "Gorgon.Editor"));

        if (!dir.Exists)
        {
            dir.Create();
        }

        ApplicationUserDirectory = dir;
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    [STAThread]
    static void Main(string[] args)
    {
        Boot booter = null;

        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            InitApplicationUserDirectory();
            InitializeLogging(args);

            booter = new Boot();
            booter.BootStrap();

            GorgonApplication.Run(booter);
        }
        catch (Exception ex)
        {
            ex.Handle(e => GorgonDialogs.ErrorBox(null, Resources.GOREDIT_ERR_GENERAL_ERROR, Resources.GOREDIT_ERR_ERROR, e), Log);
        }
        finally
        {
            Log?.LogEnd();
            booter?.Dispose();

            CommonEditorResources.UnloadResources();
        }
    }
}
