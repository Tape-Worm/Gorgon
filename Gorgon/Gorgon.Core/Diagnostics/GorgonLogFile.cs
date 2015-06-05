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
	sealed public class GorgonLogFile : 
		IGorgonThreadedLog
	{
		#region Variables.
		// File stream object.
		private StreamWriter _stream;
		// Logging filter.
		private LoggingLevel _filterLevel = LoggingLevel.All;
		// Buffer used to send data to the log file.
		private readonly StringBuilder _outputBuffer = new StringBuilder(1024);
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the filtering level of this log.
		/// </summary>
		public LoggingLevel LogFilterLevel
		{
			get
			{
				return _filterLevel;
			}
			set
			{
				if ((_filterLevel != value) && (value != LoggingLevel.NoLogging))
				{
					Print(string.Empty, LoggingLevel.All);
					Print("**** Filter level: {0}", LoggingLevel.All, value);
					Print(string.Empty, LoggingLevel.All);
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
			private set;
		}

		/// <summary>
		/// Property to return the path to the log.
		/// </summary>
		public string LogPath
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether or not the log object is in a closed state.
		/// </summary>
		public bool IsClosed
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to format a stack trace to be more presentable.
		/// </summary>
		/// <param name="stack">Stack trace to format.</param>
		/// <param name="indicator">Inner exception indicator.</param>
		/// <param name="logLevel">Logging level to use.</param>
		private void FormatStackTrace(string stack, string indicator, LoggingLevel logLevel)
		{
			if (string.IsNullOrEmpty(stack))
			{
				return;
			}

			stack = stack.Replace('\t', ' ');
			string[] lines = stack.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			// Output to each log file.
			Print("{0}Stack trace:", logLevel, indicator);
			for (int i = lines.Length - 1; i >= 0; i--)
			{
				int inIndex = lines[i].LastIndexOf(") in ", StringComparison.Ordinal);
				int pathIndex = lines[i].LastIndexOf(@"\", StringComparison.Ordinal);

				if ((inIndex > -1) && (pathIndex > -1))
				{
					lines[i] = lines[i].Substring(0, inIndex + 5) + lines[i].Substring(pathIndex + 1);
				}

				Print("{1}{0}", logLevel, lines[i], indicator);
			}

			Print("{0}<<<{1}>>>", logLevel, indicator, Resources.GOR_DLG_ERR_STACK_END);
		}

		/// <summary>
		/// Function to format the exception message for the log output.
		/// </summary>
		/// <param name="message">Message to format.</param>
		/// <param name="indicator">Inner exception indicator.</param>
		/// <param name="logLevel">Logging level to use.</param>
		private void FormatMessage(string message, string indicator, LoggingLevel logLevel)
		{
			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			message = message.Replace('\t', ' ');
			string[] lines = message.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < lines.Length; i++)
			{
				Print(i == 0 ? "{1}{2}: {0}" : "{1}           {0}", logLevel, lines[i], indicator, Resources.GOR_LOG_EXCEPTION);
			}
		}

		/// <summary>
		/// Function to send an exception to the log file.
		/// </summary>
		/// <param name="ex">The exception to log.</param>
		/// <remarks>
		/// If the <see cref="LogFilterLevel"/> is set to <c>LoggingLevel.NoLogging</c>, then the exception will not be logged. If the filter is set to any other setting, it will be logged 
		/// regardless of filter level.
		/// </remarks>
		public void LogException(Exception ex)
		{
			string indicator = string.Empty; // Inner exception indicator.
			string branch = string.Empty; // Branching character.

			if ((ex == null)
			    || (IsClosed)
				|| (LogFilterLevel == LoggingLevel.NoLogging))
			{
				return;
			}

			Print("", LoggingLevel.All);
			Print("================================================", LoggingLevel.All);
			Print("\t{0}!!", LoggingLevel.All, Resources.GOR_LOG_EXCEPTION.ToUpper());
			Print("================================================", LoggingLevel.All);

			Exception inner = ex;

			while (inner != null)
			{
				var gorgonException = inner as GorgonException;

				FormatMessage(inner.Message, indicator, (inner == ex) ? LoggingLevel.All : LoggingLevel.Verbose);
				Print("{1}{2}: {0}", (inner == ex) ? LoggingLevel.All : LoggingLevel.Verbose, inner.GetType().FullName, indicator, Resources.GOR_DLG_ERR_EXCEPT_TYPE);
				if (inner.Source != null)
				{
					Print("{1}{2}: {0}", (inner == ex) ? LoggingLevel.All : LoggingLevel.Verbose, inner.Source, indicator, Resources.GOR_DLG_ERR_SRC);
				}

				if ((inner.TargetSite != null) && (inner.TargetSite.DeclaringType != null))
				{
					Print("{1}{2}: {0}",
					      (inner == ex) ? LoggingLevel.All : LoggingLevel.Verbose,
					      inner.TargetSite.DeclaringType.FullName + "." + inner.TargetSite.Name,
					      indicator,
					      Resources.GOR_DLG_ERR_TARGET_SITE);
				}

				if (gorgonException != null)
				{
					Print("{3}{4}: [{0}] {1} (0x{2:X})",
					      (inner == ex) ? LoggingLevel.All : LoggingLevel.Verbose,
					      gorgonException.ResultCode.Name,
					      gorgonException.ResultCode.Description,
					      gorgonException.ResultCode.Code,
					      indicator,
					      Resources.GOR_DLG_ERR_GOREXCEPT_RESULT);
				}

				IDictionary extraInfo = inner.Data;

				// Print custom information.
				if (extraInfo.Count > 0)
				{
					Print("{0}", LoggingLevel.Verbose, indicator);
					Print("{0}{1}:", LoggingLevel.Verbose, indicator, Resources.GOR_DLG_ERR_CUSTOM_INFO);
					Print("{0}------------------------------------------------------------", LoggingLevel.Verbose, indicator);
					foreach (DictionaryEntry item in extraInfo)
					{
						if (item.Value != null)
						{
							Print("{0}{1}:  {2}", LoggingLevel.Verbose, indicator, item.Key, item.Value);
						}
					}
					Print("{0}------------------------------------------------------------", LoggingLevel.Verbose, indicator);
					Print("{0}", LoggingLevel.Verbose, indicator);
				}

				FormatStackTrace(inner.StackTrace, indicator, (inner == ex) ? LoggingLevel.All : LoggingLevel.Verbose);

				if (inner.InnerException != null)
				{
					if (!string.IsNullOrWhiteSpace(indicator))
					{
						Print("{0}================================================================================================", LoggingLevel.Verbose, branch + "|->   ");
						branch += "  ";
						indicator = branch + "|   ";
					}
					else
					{
						Print("{0}================================================================================================", LoggingLevel.Verbose, branch + "|-> ");
						indicator = "|   ";
					}

					Print("{0}  {2} \"{1}\"", LoggingLevel.Verbose, indicator, inner.Message, Resources.GOR_DLG_ERR_NEXT_EXCEPTION);
					Print("{0}================================================================================================", LoggingLevel.Verbose, indicator);
				}

				inner = inner.InnerException;
			}
			Print("", LoggingLevel.All);
		}

		/// <summary>
		/// Function to print a line to the logfile.
		/// </summary>
		/// <param name="formatSpecifier">Format specifier for the line.</param>
		/// <param name="level">Level that this message falls under.</param>
		/// <param name="arguments">List of optional arguments.</param>
		/// <remarks>
		/// This method is thread safe and can be called from threads that did not create the this object. Writing will be synchronized to ensure that multiple threads do not 
		/// attempt to write to the file at the same time.
		/// </remarks>
		public void Print(string formatSpecifier, LoggingLevel level, params object[] arguments)
		{
			if ((LogFilterLevel == LoggingLevel.NoLogging) || (IsClosed) || ((level > LogFilterLevel) && (level != LoggingLevel.All)))
			{
				return;
			}

			bool locked = false;
			var spinLock = new SpinLock();

			try
			{
				spinLock.Enter(ref locked);

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
				}

				_stream.Write(_outputBuffer.ToString());
				_stream.Flush();
			}
			finally
			{
				if (locked)
				{
					spinLock.Exit();
				}
			}
		}

		/// <summary>
		/// Function to close the log file.
		/// </summary>
		/// <remarks>
		/// If this method is not called from the thread that the object was created on, then that call will do nothing.
		/// </remarks>
		public void Close()
		{
			if ((IsClosed)
				|| (ThreadID != Thread.CurrentThread.ManagedThreadId))
			{
				return;
			}

			// Clean up.
			Print(string.Empty, LoggingLevel.All);
			Print("**** {0} (Version {1}) logging ends. ****", LoggingLevel.All, LogApplication,
			      GetType().Assembly.GetName().Version.ToString());

			if (_stream != null)
			{
				_stream.Close();
				_stream = null;
			}

			IsClosed = true;
		}

		/// <summary>
		/// Function to open the log file.
		/// </summary>
		/// <remarks>
		/// If this method is not called from the thread that the object was created on, then that call will do nothing.
		/// </remarks>
		public void Open()
		{
			if ((!IsClosed)
				|| (ThreadID != Thread.CurrentThread.ManagedThreadId))
			{
				return;
			}

			string directory = Path.GetDirectoryName(LogPath);

			// If we can't get a directory, then leave.
			if (string.IsNullOrWhiteSpace(directory))
			{
				return;
			}

			// Create the directory if it doesn't exist.
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			// Open the stream.
			_stream = new StreamWriter(File.Open(LogPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read), Encoding.UTF8);
			_stream.Flush();

			IsClosed = false;
			Print("**** {0} (Version {1}) logging begins ****", LoggingLevel.All, LogApplication,
			      GetType().Assembly.GetName().Version.ToString());

			if (LogFilterLevel != LoggingLevel.NoLogging)
			{
				Print("**** Filter level: {0}", LoggingLevel.All, LogFilterLevel);
			}

			Print(string.Empty, LoggingLevel.All);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonLogFile"/> class.
		/// </summary>
		/// <param name="appname">File name for the log file.</param>
		/// <param name="extraPath">Additional directories for the path.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="appname"/> parameter is <b>null</b> (<i>Nothing</i> in VB.NET).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the appname parameter is empty.</exception>
		public GorgonLogFile(string appname, string extraPath)
		{
			ThreadID = Thread.CurrentThread.ManagedThreadId;

			GorgonDebug.AssertParamString(appname, "appname");

			IsClosed = true;

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
			private set;
		}
		#endregion
	}
}
