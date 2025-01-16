
// 
// Gorgon
// Copyright (C) 2015 Michael Winsor
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
// Created: Friday, May 23, 2015 2:36:10 PM
// 

using Gorgon.Diagnostics.LogProviders;

namespace Gorgon.Diagnostics;

/// <summary>
/// Enumeration containing the logging levels
/// </summary>
public enum LoggingLevel
{
    /// <summary>This will disable the log file.</summary>
    NoLogging = 0,
    /// <summary>This will only pass messages marked as simple.</summary>
    Simple = 1,
    /// <summary>This will only pass messages marked as intermediate.</summary>
    Intermediate = 2,
    /// <summary>This will only pass messages marked as verbose.</summary>
    Verbose = 3,
    /// <summary>This will print all messages regardless of level.</summary>
    All = 4
}

/// <summary>
/// Provides logging functionality for an application
/// </summary>
/// <remarks>
/// <para>
/// This object will send logging information to a logging data source. This could be a text file, XML document, a database, etc... 
/// </para>
/// </remarks>
public interface IGorgonLog
{
    /// <summary>
    /// Property to return the provider for this log.
    /// </summary>
    IGorgonLogProvider Provider
    {
        get;
    }

    /// <summary>
    /// Property to return the ID of the thread that created the log object.
    /// </summary>
    int ThreadID
    {
        get;
    }

    /// <summary>
    /// Property to set or return the filtering level of this log.
    /// </summary>
    LoggingLevel LogFilterLevel
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the name of the application that is being logged.
    /// </summary>
    string LogApplication
    {
        get;
    }

    /// <summary>
    /// Function to perform any one time inital logging.
    /// </summary>
    void LogStart();

    /// <summary>
    /// Function to perform any one time final logging.
    /// </summary>
    void LogEnd();

    /// <summary>
    /// Function to send an exception to the log.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <remarks>
    /// <para>
    /// If the <see cref="GorgonLog.LogFilterLevel"/> is set to <c>LoggingLevel.NoLogging</c>, then the exception will not be logged. If the filter is set to any other setting, it will be logged 
    /// regardless of filter level.
    /// </para>
    /// </remarks>
    void LogException(Exception ex);

    /// <summary>
    /// Function to send an error message to the log.
    /// </summary>
    /// <param name="message">The error message to write to the log.</param>
    /// <param name="level">Level that this message falls under.</param>    
    void PrintError(string message, LoggingLevel level);

    /// <summary>
    /// Function to send a warning message to the log.
    /// </summary>
    /// <param name="message">The warning message to write to the log.</param>
    /// <param name="level">Level that this message falls under.</param>    
    void PrintWarning(string message, LoggingLevel level);

    /// <summary>
    /// Function to print a formatted line of text to the log.
    /// </summary>
    /// <param name="message">The message to write to the log.</param>
    /// <param name="level">Level that this message falls under.</param>    
    void Print(string message, LoggingLevel level);
}
