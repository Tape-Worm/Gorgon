﻿
// 
// Gorgon
// Copyright (C) 2011 Michael Winsor
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
// Created: Tuesday, June 14, 2011 8:50:49 PM
// 

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics.LogProviders;
using Gorgon.Properties;

namespace Gorgon.Diagnostics;

/// <summary>
/// Base class for logging objects
/// </summary>
public abstract class GorgonLog
    : IGorgonLog
{
    /// <summary>
    /// The type of message being sent to the log.
    /// </summary>
    protected enum MessageType
    {
        /// <summary>
        /// General information.
        /// </summary>
        Info = 0,
        /// <summary>
        /// A warning message.
        /// </summary>
        Warning = 1,
        /// <summary>
        /// An error message.
        /// </summary>
        Error = 2,
        /// <summary>
        /// A critical exception.
        /// </summary>
        Critical = 3
    }

    // List of separators for new lines.
    private static readonly char[] _newLineSeparators = ['\n', '\r'];

    // Logging filter.
    private LoggingLevel _filterLevel = LoggingLevel.All;
    // Buffer used to send data to the log file.
    private readonly List<string> _outputBuffer = new(1024);
    // The thread buffers used to store messages per thread.
    private readonly ConcurrentDictionary<int, List<string>> _threadBuffers = new();
    // Synchronization lock for multiple threads.
    private readonly object _syncLock = new();
    // The application version.
    private readonly Version? _appVersion;

    /// <summary>
    /// An instance of a log that does no logging and merely contains empty methods.
    /// </summary>
    public static readonly IGorgonLog NullLog = new LogDummy();

    /// <summary>
    /// Property to return the ID of the thread that created the log object.
    /// </summary>
    public int ThreadID
    {
        get;
    }

    /// <summary>
    /// Property to set or return the filtering level of this log.
    /// </summary>
    public LoggingLevel LogFilterLevel
    {
        get => _filterLevel;
        set
        {
            if (_filterLevel != value)
            {
                Print($"\r\n**** Filter level: {value}\r\n", LoggingLevel.All);
            }

            _filterLevel = value;
        }
    }

    /// <summary>
    /// Property to return the name of the application that is being logged.
    /// </summary>
    public string LogApplication
    {
        get;
    }

    /// <summary>
    /// Property to return the provider for this log.
    /// </summary>
    /// <remarks>
    /// Logging implementations that inherit from this class will be able to assign their own provider in the base constructor.
    /// </remarks>
    public IGorgonLogProvider Provider
    {
        get;
        protected set;
    } = DummyLogProvider.NullProvider;

    /// <summary>
    /// Function to format a stack trace to be more presentable.
    /// </summary>
    /// <param name="stack">Stack trace to format.</param>
    /// <param name="indicator">Inner exception indicator.</param>
    /// <param name="resultMessage">The resulting message.</param>
    private static void FormatStackTrace(string? stack, string indicator, StringBuilder resultMessage)
    {
        if (string.IsNullOrEmpty(stack))
        {
            return;
        }

        stack = stack.Replace('\t', ' ');
        string[] lines = stack.Split(_newLineSeparators, StringSplitOptions.RemoveEmptyEntries);

        // Output to each log file.
        resultMessage.AppendFormat("{0}Stack trace:\r\n", indicator);
        for (int i = lines.Length - 1; i >= 0; i--)
        {
            int inIndex = lines[i].LastIndexOf(") in ", StringComparison.Ordinal);
            int pathIndex = lines[i].LastIndexOf('\\');

            if ((inIndex > -1) && (pathIndex > -1))
            {
                lines[i] = lines[i][..(inIndex + 5)] + lines[i][(pathIndex + 1)..];
            }

            resultMessage.AppendFormat("{1}{0}\r\n", lines[i], indicator);
        }

        resultMessage.AppendFormat("{0}<<<{1}>>>\r\n", indicator, Resources.GOR_EXCEPT_STACK_END);
    }

    /// <summary>
    /// Function to format the exception message for the log output.
    /// </summary>
    /// <param name="message">Message to format.</param>
    /// <param name="indicator">Inner exception indicator.</param>
    /// <param name="resultMessage">The resulting message.</param>
    private static void FormatMessage(string message, string indicator, StringBuilder resultMessage)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        string[] lines = message.Split(_newLineSeparators,
                                       StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            resultMessage.AppendFormat(i == 0 ? "{1}{2}: {0}\r\n" : "{1}           {0}\r\n", lines[i], indicator, Resources.GOR_LOG_EXCEPTION);
        }

        resultMessage.Replace('\t', ' ');
    }

    /// <summary>
    /// Function to add target site information to the exception message.
    /// </summary>
    /// <param name="resultMessage">The message to update.</param>
    /// <param name="indicator">The indicator for the message.</param>
    /// <param name="ex">The exception being evaluated.</param>
    [RequiresUnreferencedCode("Calls System.Exception.TargetSite")]
    private static void AddTargetSite(StringBuilder resultMessage, string indicator, Exception ex)
    {
        if (ex.TargetSite?.DeclaringType is null)
        {
            return;
        }

        resultMessage.AppendFormat("{2}{3}: {0}.{1}\r\n",
                    ex.TargetSite.DeclaringType.FullName,
                    ex.TargetSite.Name,
                    indicator,
                    Resources.GOR_EXCEPT_TARGET_SITE);
    }

    /// <summary>
    /// Function to send an exception to the log.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <remarks>
    /// <para>
    /// If the <see cref="LogFilterLevel"/> is set to <c>LoggingLevel.NoLogging</c>, then the exception will not be logged. If the filter is set to any other setting, it will be logged 
    /// regardless of filter level.
    /// </para>
    /// </remarks>    
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
                                  Justification = "Using RuntimeFeature.IsDynamicCodeSupported flag to skip the code.")]
    public void LogException(Exception ex)
    {
        string indicator = string.Empty; // Inner exception indicator.
        string branch = string.Empty; // Branching character.
        StringBuilder exception = new(1024);

        if ((ex is null)
            || (LogFilterLevel == LoggingLevel.NoLogging))
        {
            return;
        }

        lock (_syncLock)
        {
            exception.Append(" \r\n");
            exception.Append("================================================\r\n");
            exception.AppendFormat("\t{0}!!\r\n", Resources.GOR_LOG_EXCEPTION.ToUpper());
            exception.Append("================================================\r\n");

            Exception? inner = ex;

            while (inner is not null)
            {

                if ((inner == ex) || (LogFilterLevel == LoggingLevel.Verbose) || (LogFilterLevel == LoggingLevel.All))
                {
                    FormatMessage(inner.Message, indicator, exception);

                    exception.AppendFormat("{1}{2}: {0}\r\n",
                                inner.GetType().FullName,
                                indicator,
                                Resources.GOR_EXCEPT_EXCEPT_TYPE);

                    if (inner.Source is not null)
                    {
                        exception.AppendFormat("{1}{2}: {0}\r\n", inner.Source, indicator, Resources.GOR_EXCEPT_SRC);
                    }

                    if (RuntimeFeature.IsDynamicCodeSupported)
                    {
                        AddTargetSite(exception, indicator, inner);
                    }

                    if (inner is GorgonException gorgonException)
                    {
                        exception.AppendFormat("{3}{4}: [{0}] {1} (0x{2:X})\r\n",
                                    gorgonException.ResultCode.Name,
                                    gorgonException.ResultCode.Description,
                                    gorgonException.ResultCode.Code,
                                    indicator,
                                    Resources.GOR_EXCEPT_GOREXCEPT_RESULT);
                    }
                }

                IDictionary extraInfo = inner.Data;

                // Print custom information.
                if (((LogFilterLevel == LoggingLevel.Verbose) || (LogFilterLevel == LoggingLevel.All))
                    && (extraInfo.Count > 0))
                {
                    exception.AppendFormat("{0}\r\n", indicator);
                    exception.AppendFormat("{0}{1}:\r\n", indicator, Resources.GOR_EXCEPT_CUSTOM_INFO);
                    exception.AppendFormat("{0}------------------------------------------------------------\r\n", indicator);

                    foreach (DictionaryEntry item in extraInfo)
                    {
                        if (item.Value is not null)
                        {
                            exception.AppendFormat("{0}{1}:  {2}\r\n", indicator, item.Key, item.Value);
                        }
                    }

                    exception.AppendFormat("{0}------------------------------------------------------------\r\n", indicator);
                    exception.AppendFormat("{0}\r\n", indicator);
                }

                if ((ex == inner) || (LogFilterLevel == LoggingLevel.Verbose) || (LogFilterLevel == LoggingLevel.All))
                {
                    FormatStackTrace(inner.StackTrace, indicator, exception);
                }

                if ((inner.InnerException is not null) && ((LogFilterLevel == LoggingLevel.Verbose)
                                                          || (LogFilterLevel == LoggingLevel.All)))
                {
                    if (!string.IsNullOrWhiteSpace(indicator))
                    {
                        exception.AppendFormat("{0}|-> {1}\r\n", branch, new string('=', 96 - indicator.Length));
                        branch += "  ";
                        indicator = branch + "|   ";
                    }
                    else
                    {
                        exception.AppendFormat("{0}|-> ============================================================================================\r\n", branch);
                        indicator = "|   ";
                    }

                    exception.AppendFormat("{0}  {1}\r\n", indicator, Resources.GOR_EXCEPT_NEXT_EXCEPTION);
                    exception.AppendFormat("{0}{1}\r\n", indicator, new string('=', 96 - indicator.Length));
                }

                inner = inner.InnerException;
            }

            exception.Append(" \r\n");

            SendToLog(exception.ToString(), MessageType.Critical);
        }
    }

    /// <summary>
    /// Function to flush all the lines of text to the file.
    /// </summary>
    /// <param name="lines">The lines to flush out</param>
    private void FlushLines(List<string> lines)
    {
        string finalLines;

        if (ThreadID != Environment.CurrentManagedThreadId)
        {
            // If the buffer is larger than or equal to 100 lines, then we'll tell the thread to write out everything it has.
            // Otherwise, we can wait until we get back to the main thread.
            if (lines.Count < 100)
            {
                return;
            }

            finalLines = string.Join("\r\n", lines);
        }
        else
        {
            // If we're in the main thread, then dump everything.
            finalLines = string.Join("\r\n",
                                     _threadBuffers.SelectMany(item => item.Value)
                                                   .Concat(lines));

            // Remove all thread buffers since we're dumping everything.
            _threadBuffers.Clear();
        }

        // Dump all lines.
        Provider.SendMessage(finalLines);

        lines.Clear();
    }

    /// <summary>
    /// Function to append a line of logging text to the log file.
    /// </summary>
    /// <param name="message">The pre-formatted text message.</param>
    /// <param name="messageType">The type of message being sent to the log.</param>
    private void SendToLog(string message, MessageType messageType)
    {
        List<string> lines = _outputBuffer;

        if (ThreadID != Environment.CurrentManagedThreadId)
        {
            lines = _threadBuffers.GetOrAdd(Environment.CurrentManagedThreadId,
                                            id => []);
        }

        string prefix = messageType switch
        {
            MessageType.Info => string.Empty,
            MessageType.Warning => " [Warning]:",
            MessageType.Error => " [Error]:",
            MessageType.Critical => " [Exception]:",
            _ => string.Empty,
        };

        if ((string.IsNullOrEmpty(message)) || (message == "\n") || (message == "\r"))
        {
            lines.Add($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}]");
        }
        else
        {
            // Get a list of lines.
            string[] formattedLines = message.Split(_newLineSeparators,
                                                            StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in formattedLines)
            {
                lines.Add($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}]{prefix} {line}");
            }
        }

        FlushLines(lines);
    }

    /// <inheritdoc/>
    public void PrintError(string message, LoggingLevel level)
    {
        if ((LogFilterLevel == LoggingLevel.NoLogging) ||
            ((LogFilterLevel != LoggingLevel.All) && (level != LoggingLevel.All) && (level < LogFilterLevel)))
        {
            return;
        }

        lock (_syncLock)
        {
            SendToLog(message, MessageType.Error);
        }
    }

    /// <inheritdoc/>
    public void PrintWarning(string message, LoggingLevel level)
    {
        if ((LogFilterLevel == LoggingLevel.NoLogging) ||
            ((LogFilterLevel != LoggingLevel.All) && (level != LoggingLevel.All) && (level < LogFilterLevel)))
        {
            return;
        }

        lock (_syncLock)
        {
            SendToLog(message, MessageType.Warning);
        }
    }

    /// <summary>
    /// Function to print a formatted line of text to the log.
    /// </summary>
    /// <param name="message">The message to write to the log.</param>
    /// <param name="level">Level that this message falls under.</param>
    public void Print(string message, LoggingLevel level)
    {
        if ((LogFilterLevel == LoggingLevel.NoLogging) ||
            ((LogFilterLevel != LoggingLevel.All) && (level != LoggingLevel.All) && (level < LogFilterLevel)))
        {
            return;
        }

        lock (_syncLock)
        {
            SendToLog(message, MessageType.Info);
        }
    }

    /// <summary>
    /// Function to end logging to the file.
    /// </summary>
    public void LogEnd()
    {
        if (ThreadID != Environment.CurrentManagedThreadId)
        {
            return;
        }

        // Clean up.
        lock (_syncLock)
        {
            FlushLines(_outputBuffer);
            Provider.Close($"**** {LogApplication} (Version {_appVersion?.ToString() ?? "N/A"}) logging ends on thread ID: 0x{ThreadID.FormatHex()}. ****");
        }
    }

    /// <summary>
    /// Function to perform any one time inital logging.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If calling this method from a thread that is <b>not</b> the thread that created the object, then no new file will be created.
    /// </para>
    /// </remarks>
    public void LogStart()
    {
        if (ThreadID != Environment.CurrentManagedThreadId)
        {
            return;
        }

        Provider.Open($"**** {LogApplication} (Version {_appVersion?.ToString() ?? "N/A"}) logging begins on thread ID 0x{ThreadID.FormatHex()} ****\r\n{(LogFilterLevel != LoggingLevel.NoLogging ? $"**** Filter level: {LogFilterLevel}\r\n" : string.Empty)}");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonLog"/> class.
    /// </summary>
    /// <param name="appName">File name for the log file.</param>
    /// <param name="version">[Optional] The version of the application that is logging.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="appName"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This constructor is meant for developers to implement their own logging implementation without having to recreate all functionality in the <see cref="IGorgonLog"/> interface.
    /// </para>
    /// </remarks>
    protected GorgonLog(string appName, Version? version = null)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(appName);

        ThreadID = Environment.CurrentManagedThreadId;

        LogApplication = appName;

        _appVersion = version;
    }
}
