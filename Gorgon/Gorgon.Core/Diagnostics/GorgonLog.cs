
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Diagnostics.LogProviders;
using Gorgon.Properties;

namespace Gorgon.Diagnostics;

/// <inheritdoc cref="IGorgonLog"/>
public abstract class GorgonLog
    : IGorgonLog
{
    // Logging filter.
    private LoggingLevel _filterLevel = LoggingLevel.All;
    // Synchronization lock for multiple threads.
    private readonly object _syncLock = new();
    // The application version.
    private readonly Version? _appVersion;

    /// <summary>
    /// Property to return the active provider for the log.
    /// </summary>
    /// <remarks>
    /// Logging implementations that inherit from this class will be able to assign their own provider in the base constructor.
    /// </remarks>
    protected IGorgonLogProvider Provider
    {
        get;
        set;
    } = DummyLogProvider.NullProvider;

    /// <summary>
    /// An instance of a log that does no logging and merely contains empty methods.
    /// </summary>
    public static readonly IGorgonLog NullLog = new LogDummy();

    /// <inheritdoc/>
    public int ThreadID
    {
        get;
    }

    /// <inheritdoc/>
    public LoggingLevel LogFilterLevel
    {
        get => _filterLevel;
        set
        {
            if (_filterLevel != value)
            {
                Print($"\n**** Filter level: {value}\n", LoggingLevel.All);
            }

            _filterLevel = value;
        }
    }

    /// <inheritdoc/>
    public string LogApplication
    {
        get;
    }

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
        GorgonSpanCharEnumerator lines = stack.GetReverseSplitEnumerator("\n");

        // Output to each log file.
        resultMessage.AppendFormat("{0}Stack trace:\n", indicator);
        foreach (ReadOnlySpan<char> line in lines)
        {
            int inIndex = line.LastIndexOf(") in ", StringComparison.Ordinal);
            int pathIndex = line.LastIndexOf('\\');

            resultMessage.Append(indicator);

            if ((inIndex > -1) && (pathIndex > -1))
            {
                resultMessage.Append(line[..(inIndex + 5)]);
                resultMessage.Append(line[(pathIndex + 1)..]);
            }
            else
            {
                resultMessage.Append(line);
            }

            resultMessage.Append('\n');
        }

        resultMessage.AppendFormat("{0}<<<{1}>>>\n", indicator, Resources.GOR_EXCEPT_STACK_END);
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

        GorgonSpanCharEnumerator lines = message.GetSplitEnumerator("\n");

        bool first = true;

        foreach (ReadOnlySpan<char> line in lines)
        {
            if (first)
            {
                resultMessage.Append(indicator);
                resultMessage.AppendFormat("{0}: ", Resources.GOR_LOG_EXCEPTION);
                resultMessage.Append(line);
                first = false;
            }
            else
            {
                resultMessage.Append(indicator);
                resultMessage.Append("           ");
                resultMessage.Append(line);
            }

            resultMessage.Append('\n');
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

        resultMessage.AppendFormat("{2}{3}: {0}.{1}\n",
                    ex.TargetSite.DeclaringType.FullName,
                    ex.TargetSite.Name,
                    indicator,
                    Resources.GOR_EXCEPT_TARGET_SITE);
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
                                  Justification = "Using RuntimeFeature.IsDynamicCodeSupported flag to skip the code.")]
    public void PrintException(Exception ex)
    {
        string indicator = string.Empty; // Inner exception indicator.
        string branch = string.Empty; // Branching character.
        StringBuilder exception = new(1024);

        if ((ex is null)
            || (LogFilterLevel == LoggingLevel.NoLogging))
        {
            return;
        }

        exception.Append(" \n");
        exception.Append("================================================\n");
        exception.AppendFormat("\t{0}!!\n", Resources.GOR_LOG_EXCEPTION.ToUpper());
        exception.Append("================================================\n");

        Exception? inner = ex;

        while (inner is not null)
        {

            if ((inner == ex) || (LogFilterLevel == LoggingLevel.Verbose) || (LogFilterLevel == LoggingLevel.All))
            {
                FormatMessage(inner.Message, indicator, exception);

                exception.AppendFormat("{1}{2}: {0}\n",
                            inner.GetType().FullName,
                            indicator,
                            Resources.GOR_EXCEPT_EXCEPT_TYPE);

                if (inner.Source is not null)
                {
                    exception.AppendFormat("{1}{2}: {0}\n", inner.Source, indicator, Resources.GOR_EXCEPT_SRC);
                }

                if (RuntimeFeature.IsDynamicCodeSupported)
                {
                    AddTargetSite(exception, indicator, inner);
                }

                if (inner is GorgonException gorgonException)
                {
                    exception.AppendFormat("{3}{4}: [{0}] {1} (0x{2:X})\n",
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
                exception.AppendFormat("{0}\n", indicator);
                exception.AppendFormat("{0}{1}:\n", indicator, Resources.GOR_EXCEPT_CUSTOM_INFO);
                exception.AppendFormat("{0}------------------------------------------------------------\n", indicator);

                foreach (DictionaryEntry item in extraInfo)
                {
                    if (item.Value is not null)
                    {
                        exception.AppendFormat("{0}{1}:  {2}\n", indicator, item.Key, item.Value);
                    }
                }

                exception.AppendFormat("{0}------------------------------------------------------------\n", indicator);
                exception.AppendFormat("{0}\n", indicator);
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
                    exception.AppendFormat("{0}|--> {1}\n", branch, new string('=', 95 - indicator.Length));
                    branch += " ";
                    indicator = branch + "|   ";
                }
                else
                {
                    exception.AppendFormat("{0}|-> ============================================================================================\n", branch);
                    indicator = "|   ";
                }

                exception.AppendFormat("{0}  {1}\n", indicator, Resources.GOR_EXCEPT_NEXT_EXCEPTION);
                exception.AppendFormat("{0}{1}\n", indicator, new string('=', 96 - indicator.Length));
            }

            inner = inner.InnerException;
        }

        exception.Append(" \n");

        SendToLog(exception.ToString(), MessageType.Critical);
    }

    /// <summary>
    /// Function to append a line of logging text to the log file.
    /// </summary>
    /// <param name="message">The pre-formatted text message.</param>
    /// <param name="messageType">The type of message being sent to the log.</param>
    private void SendToLog(string message, MessageType messageType)
    {
        string prefix = messageType switch
        {
            MessageType.Info => string.Empty,
            MessageType.Warning => " [Warning]:",
            MessageType.Error => " [Error]:",
            MessageType.Critical => " [Exception]:",
            _ => string.Empty,
        };

        string finalMessage;

        if ((string.IsNullOrEmpty(message)) || (message == "\n") || (message == "\r"))
        {
            finalMessage = $"[{Environment.CurrentManagedThreadId}][{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}]";
        }
        else if ((!message.Contains('\n')) && (!message.Contains('\r')))
        {
            finalMessage = $"[{Environment.CurrentManagedThreadId}][{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}]{prefix} {message}";
        }
        else
        {
            // Get a list of lines.
            GorgonSpanCharEnumerator spans = message.GetSplitEnumerator("\n", true);
            StringBuilder formatted = new(128);

            foreach (ReadOnlySpan<char> span in spans)
            {
                if (formatted.Length > 0)
                {
                    formatted.Append('\n');
                }

                formatted.AppendFormat("[{0}][{1} {2}]{3} ", Environment.CurrentManagedThreadId, DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), prefix);
                formatted.Append(span);
            }

            finalMessage = formatted.ToString();
        }

        lock (_syncLock)
        {
            Provider.SendMessage(finalMessage, messageType);
        }
    }

    /// <inheritdoc/>
    public void PrintError(string message, LoggingLevel level)
    {
        if ((LogFilterLevel == LoggingLevel.NoLogging) ||
            ((LogFilterLevel != LoggingLevel.All) && (level != LoggingLevel.All) && (level > LogFilterLevel)))
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
            ((LogFilterLevel != LoggingLevel.All) && (level != LoggingLevel.All) && (level > LogFilterLevel)))
        {
            return;
        }

        lock (_syncLock)
        {
            SendToLog(message, MessageType.Warning);
        }
    }

    /// <inheritdoc/>
    public void Print(string message, LoggingLevel level)
    {
        if ((LogFilterLevel == LoggingLevel.NoLogging) ||
            ((LogFilterLevel != LoggingLevel.All) && (level != LoggingLevel.All) && (level > LogFilterLevel)))
        {
            return;
        }

        lock (_syncLock)
        {
            SendToLog(message, MessageType.Info);
        }
    }

    /// <inheritdoc/>
    public void LogEnd()
    {
        if (ThreadID != Environment.CurrentManagedThreadId)
        {
            return;
        }

        // Clean up.
        lock (_syncLock)
        {
            Provider.Close($"**** {LogApplication} (Version {_appVersion?.ToString() ?? "N/A"}) logging ends on thread ID: 0x{ThreadID.FormatHex()}. ****");
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// If calling this method from a thread that is <b>not</b> the thread that created the object, then no new file will be created.
    /// </para>
    /// </remarks>
    public void LogStart(IGorgonComputerInfo? computerInfo = null)
    {
        if (ThreadID != Environment.CurrentManagedThreadId)
        {
            return;
        }

        Provider.Open($"**** {LogApplication} (Version {_appVersion?.ToString() ?? "N/A"}) logging begins on thread ID 0x{ThreadID.FormatHex()} ****\n{(LogFilterLevel != LoggingLevel.NoLogging ? $"**** Filter level: {LogFilterLevel}\n" : string.Empty)}");

        if (computerInfo is null)
        {
            return;
        }

        Print("System Information:", LoggingLevel.All);
        Print($"Processor Count: {computerInfo.Cpus.Count}", LoggingLevel.All);
        for (int i = 0; i < computerInfo.Cpus.Count; ++i)
        {
            IGorgonCpuInfo cpuInfo = computerInfo.Cpus[i];

            Print($"CPU {i}: {cpuInfo.CpuName}", LoggingLevel.All);
            Print($"CPU {i} Identifier: {cpuInfo.CpuIdentifier}", LoggingLevel.All);
            Print($"CPU {i} Vendor: {cpuInfo.CpuVendor}", LoggingLevel.All);
            Print($"CPU {i} Core Count: {cpuInfo.CoreCount}", LoggingLevel.All);
            Print($"CPU {i} SMT/Hyperthread Count: {cpuInfo.SmtCount}", LoggingLevel.All);
        }
        Print($"Architecture: {computerInfo.PlatformArchitecture}", LoggingLevel.All);
        Print($"Installed Memory: {computerInfo.TotalPhysicalRAM.FormatMemory()}", LoggingLevel.All);
        Print($"Available Memory: {computerInfo.AvailablePhysicalRAM.FormatMemory()}", LoggingLevel.All);
        Print($"Operating System: {computerInfo.OperatingSystemVersionText} ({computerInfo.OperatingSystemArchitecture})", LoggingLevel.All);
        Print(string.Empty, LoggingLevel.All);
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
