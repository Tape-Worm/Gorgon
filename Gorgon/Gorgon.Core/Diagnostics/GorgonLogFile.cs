#region MIT.
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Tuesday, June 14, 2011 8:50:49 PM
// 
#endregion

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Gorgon.Core;
using Gorgon.Core.Properties;
using Gorgon.IO;

namespace Gorgon.Diagnostics
{
	/// <summary>
	/// Enumeration containing the logging levels.
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
	/// Sends logging information to a file.
	/// </summary>
	/// <remarks>
	/// This is a concrete implementation of the <see cref="IGorgonLog"/> interface.
	/// </remarks>
	public sealed class GorgonLogFile : 
		IGorgonThreadedLog, IGorgonLogFile
	{
		#region Variables.
		// Logging filter.
		private LoggingLevel _filterLevel = LoggingLevel.All;
		// Buffer used to send data to the log file.
		private readonly StringBuilder _outputBuffer = new StringBuilder(1024);
		// Synchronization lock for multiple threads.
		private readonly object _syncLock = new object();
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the filtering level of this log.
		/// </summary>
		public LoggingLevel LogFilterLevel
		{
			get => _filterLevel;
			set
			{
				if ((_filterLevel != value) && (value != LoggingLevel.NoLogging))
				{
					Print("\n**** Filter level: {0}\n", LoggingLevel.All, value);
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
		/// Property to return the path to the log.
		/// </summary>
		public string LogPath
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to format a stack trace to be more presentable.
		/// </summary>
		/// <param name="writer">The stream to the log file.</param>
		/// <param name="stack">Stack trace to format.</param>
		/// <param name="indicator">Inner exception indicator.</param>
		private void FormatStackTrace(StreamWriter writer, string stack, string indicator)
		{
			if (string.IsNullOrEmpty(stack))
			{
				return;
			}

			stack = stack.Replace('\t', ' ');
			string[] lines = stack.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			// Output to each log file.
			SendToLog(writer, "{0}Stack trace:", indicator);
			for (int i = lines.Length - 1; i >= 0; i--)
			{
				int inIndex = lines[i].LastIndexOf(") in ", StringComparison.Ordinal);
				int pathIndex = lines[i].LastIndexOf(@"\", StringComparison.Ordinal);

				if ((inIndex > -1) && (pathIndex > -1))
				{
					lines[i] = lines[i].Substring(0, inIndex + 5) + lines[i].Substring(pathIndex + 1);
				}

				SendToLog(writer, "{1}{0}", lines[i], indicator);
			}

			SendToLog(writer, "{0}<<<{1}>>>", indicator, Resources.GOR_EXCEPT_STACK_END);
		}

		/// <summary>
		/// Function to format the exception message for the log output.
		/// </summary>
		/// <param name="stream">File stream to write into.</param>
		/// <param name="message">Message to format.</param>
		/// <param name="indicator">Inner exception indicator.</param>
		private void FormatMessage(StreamWriter stream, string message, string indicator)
		{
			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			message = message.Replace('\t', ' ');
			string[] lines = message.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < lines.Length; i++)
			{
				SendToLog(stream, i == 0 ? "{1}{2}: {0}" : "{1}           {0}", lines[i], indicator, Resources.GOR_LOG_EXCEPTION);
			}
		}

		/// <summary>
		/// Function to send an exception to the log.
		/// </summary>
		/// <param name="ex">The exception to log.</param>
		/// <remarks>
		/// <para>
		/// If the <see cref="GorgonLogFile.LogFilterLevel"/> is set to <c>LoggingLevel.NoLogging</c>, then the exception will not be logged. If the filter is set to any other setting, it will be logged 
		/// regardless of filter level.
		/// </para>
		/// </remarks>
		public void LogException(Exception ex)
		{
			string indicator = string.Empty; // Inner exception indicator.
			string branch = string.Empty; // Branching character.

			if ((ex == null)
				|| (LogFilterLevel == LoggingLevel.NoLogging))
			{
				return;
			}

			lock (_syncLock)
			{
				using (var stream = new StreamWriter(File.Open(LogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
				{
					SendToLog(stream, string.Empty);
					SendToLog(stream, "================================================");
					SendToLog(stream, "\t{0}!!", Resources.GOR_LOG_EXCEPTION.ToUpper());
					SendToLog(stream, "================================================");

					Exception inner = ex;

					while (inner != null)
					{
						var gorgonException = inner as GorgonException;

						if ((inner == ex) || (LogFilterLevel == LoggingLevel.Verbose))
						{
							FormatMessage(stream, inner.Message, indicator);

							SendToLog(stream,
							          "{1}{2}: {0}",
							          inner.GetType().FullName,
							          indicator,
							          Resources.GOR_EXCEPT_EXCEPT_TYPE);

							if (inner.Source != null)
							{
								SendToLog(stream, "{1}{2}: {0}", inner.Source, indicator, Resources.GOR_EXCEPT_SRC);
							}

							if (inner.TargetSite?.DeclaringType != null)
							{
								SendToLog(stream,
								          "{1}{2}: {0}",
								          inner.TargetSite.DeclaringType.FullName + "." + inner.TargetSite.Name,
								          indicator,
								          Resources.GOR_EXCEPT_TARGET_SITE);
							}

							if (gorgonException != null)
							{
								SendToLog(stream,
								          "{3}{4}: [{0}] {1} (0x{2:X})",
								          gorgonException.ResultCode.Name,
								          gorgonException.ResultCode.Description,
								          gorgonException.ResultCode.Code,
								          indicator,
								          Resources.GOR_EXCEPT_GOREXCEPT_RESULT);
							}
						}

						IDictionary extraInfo = inner.Data;

						// Print custom information.
						if ((LogFilterLevel == LoggingLevel.Verbose) && (extraInfo.Count > 0))
						{
							SendToLog(stream, "{0}", indicator);
							SendToLog(stream, "{0}{1}:", indicator, Resources.GOR_EXCEPT_CUSTOM_INFO);
							SendToLog(stream, "{0}------------------------------------------------------------", indicator);

							foreach (DictionaryEntry item in extraInfo)
							{
								if (item.Value != null)
								{
									SendToLog(stream, "{0}{1}:  {2}", indicator, item.Key, item.Value);
								}
							}
							SendToLog(stream, "{0}------------------------------------------------------------", indicator);
							SendToLog(stream, "{0}", indicator);
						}

						if ((ex == inner) || (LogFilterLevel == LoggingLevel.Verbose))
						{
							FormatStackTrace(stream, inner.StackTrace, indicator);
						}

						if ((inner.InnerException != null) && (LogFilterLevel == LoggingLevel.Verbose))
						{
							if (!string.IsNullOrWhiteSpace(indicator))
							{
								SendToLog(stream, "{0}================================================================================================", branch + "|->   ");
								branch += "  ";
								indicator = branch + "|   ";
							}
							else
							{
								SendToLog(stream, "{0}================================================================================================", branch + "|-> ");
								indicator = "|   ";
							}

							SendToLog(stream, "{0}  {2} \"{1}\"", indicator, inner.Message, Resources.GOR_EXCEPT_NEXT_EXCEPTION);
							SendToLog(stream, "{0}================================================================================================", indicator);
						}

						inner = inner.InnerException;
					}
					SendToLog(stream, string.Empty);
				}
			}
		}

		/// <summary>
		/// Function to append a line of logging text to the log file.
		/// </summary>
		/// <param name="writer">Stream writer used to write to the log file.</param>
		/// <param name="formatSpecifier">The pre-formatted text message.</param>
		/// <param name="arguments">The arguments to pass to the pre-formatted text.</param>
		private void SendToLog(StreamWriter writer, string formatSpecifier, params object[] arguments)
		{
			_outputBuffer.Length = 0;

			if (string.IsNullOrEmpty(formatSpecifier) || (formatSpecifier == "\n") || (formatSpecifier == "\r"))
			{
				_outputBuffer.AppendFormat("[{0} {1}]\r\n", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
			}
			else
			{
				// Get a list of lines.
				string[] lines = formatSpecifier.Split(new[]
				                                       {
					                                       '\r',
					                                       '\n'
				                                       },
				                                       StringSplitOptions.RemoveEmptyEntries);

				foreach (string line in lines)
				{
					_outputBuffer.AppendFormat("[{0} {1}] ", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
					_outputBuffer.Append(string.Format(line + "\r\n", arguments));
				}

				writer.Write(_outputBuffer.ToString());
				writer.Flush();
			}
		}

		/// <summary>
		/// Function to print a formatted line of text to the log.
		/// </summary>
		/// <param name="formatSpecifier">Format specifier for the line.</param>
		/// <param name="level">Level that this message falls under.</param>
		/// <param name="arguments">List of optional arguments.</param>
		public void Print(string formatSpecifier, LoggingLevel level, params object[] arguments)
		{
			if ((LogFilterLevel == LoggingLevel.NoLogging) || 
				((level > LogFilterLevel) && (level != LoggingLevel.All)))
			{
				return;
			}

			lock (_syncLock)
			{
				using (var stream = new StreamWriter(File.Open(LogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8))
				{ 
					SendToLog(stream, formatSpecifier, arguments);
				}
			}
		}

		/// <summary>
		/// Function to end logging to the file.
		/// </summary>
		public void End()
		{
			// Clean up.
			lock (_syncLock)
			{
				using (var writer = new StreamWriter(File.Open(LogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8))
				{
					SendToLog(writer,
					              "**** {0} (Version {1}) logging ends on thread ID: 0x{2}. ****",
					              LogApplication,
					              GetType().Assembly.GetName().Version.ToString(),
					              ThreadID.FormatHex());
				}
			}
		}


		/// <summary>
		/// Function to start logging to a new file.
		/// </summary>
		/// <remarks>
		/// If calling this method from a thread that is <b>not</b> the thread that created the object, then no new file will be created.
		/// </remarks>
		public void Begin()
		{
			StreamWriter stream = null;
			string directory = Path.GetDirectoryName(LogPath);

			// If we can't get a directory, then leave.
			if ((string.IsNullOrWhiteSpace(directory))
				|| (ThreadID != Thread.CurrentThread.ManagedThreadId))
			{
				return;
			}

			try
			{
				// Create the directory if it doesn't exist.
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				// Open the stream.
				stream = new StreamWriter(File.Open(LogPath, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8);
				stream.WriteLine("**** {0} (Version {1}) logging begins on thread ID 0x{2} ****",
					                LogApplication,
					                GetType().Assembly.GetName().Version,
					                ThreadID.FormatHex());
				if (LogFilterLevel != LoggingLevel.NoLogging)
				{
					stream.WriteLine("**** Filter level: {0}", LogFilterLevel);
				}
				stream.WriteLine();
				stream.Flush();
			}
			finally
			{
				stream?.Close();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonLogFile"/> class.
		/// </summary>
		/// <param name="appname">File name for the log file.</param>
		/// <param name="extraPath">Additional directories for the path.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="appname"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="appname"/> parameter is empty.</exception>
		public GorgonLogFile(string appname, string extraPath)
		{
			ThreadID = Thread.CurrentThread.ManagedThreadId;

			appname.ValidateString("appname");

			LogApplication = appname;

			LogPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			LogPath += Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);

			// Verify the extra path information.
			if (!string.IsNullOrEmpty(extraPath))
			{
				// Remove any text up to and after the volume separator character.
				if (extraPath.Contains(Path.VolumeSeparatorChar.ToString(CultureInfo.InvariantCulture)))
				{
					extraPath = extraPath.IndexOf(Path.VolumeSeparatorChar) < (extraPath.Length - 1)
						            ? extraPath.Substring(extraPath.IndexOf(Path.VolumeSeparatorChar) + 1)
						            : string.Empty;
				}

			    if ((extraPath.StartsWith(Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
			        || (extraPath.StartsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture))))
			    {
			        extraPath = extraPath.Substring(1);
			    }

			    if (!extraPath.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
			    {
			        extraPath += Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
			    }

			    if (!string.IsNullOrEmpty(extraPath))
				{
					LogPath += extraPath;					
				}
			}

			LogPath += appname;
			LogPath = LogPath.FormatDirectory(Path.DirectorySeparatorChar);
			LogPath += "ApplicationLogging.txt";
		}
		#endregion

		#region IGorgonThreadedLog Members
		/// <summary>
		/// Property to return the ID of the thread that created the log object.
		/// </summary>
		public int ThreadID
		{
			get;
		}
		#endregion
	}
}
