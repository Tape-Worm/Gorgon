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
using System.Globalization;
using System.IO;
using System.Text;
using GorgonLibrary.IO;
using Gorgon.Core.Properties;

namespace GorgonLibrary.Diagnostics
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
	public class GorgonLogFile
		: IDisposable
	{
		#region Variables.
		private StreamWriter _stream;							// File stream object.
		private LoggingLevel _filterLevel = LoggingLevel.All;	// Logging filter.
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
					Print("**** {1}: {0}", LoggingLevel.All, value, Resources.GOR_LOG_FILTER_LEVEL);
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

		#region Functions.
		/// <summary>
		/// Print a line to the logfile.
		/// </summary>
		/// <param name="formatSpecifier">Format specifier for the line.</param>
		/// <param name="level">Level that this message falls under.</param>
		/// <param name="arguments">List of optional arguments.</param>
		public void Print(string formatSpecifier, LoggingLevel level, params object[] arguments)
		{
		    if (((LogFilterLevel == LoggingLevel.NoLogging) && (level != LoggingLevel.All)) || (IsClosed))
		    {
		        return;
		    }

			if ((level > LogFilterLevel) && (level != LoggingLevel.All))
			{
				return;
			}

			var outputLine = new StringBuilder(512);			// Output string 

			if (string.IsNullOrEmpty(formatSpecifier) || (formatSpecifier == "\n") || (formatSpecifier == "\r"))
			{
				outputLine.AppendFormat("[{0} {1}]\r\n", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
			}
			else
			{
				// Get a list of lines.
				string[] lines = formatSpecifier.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

				foreach (string line in lines)
				{
					outputLine.AppendFormat("[{0} {1}] ", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
					outputLine.Append(string.Format(line + "\r\n", arguments));
				}
			}

			_stream.Write(outputLine.ToString());
			_stream.Flush();
		}

		/// <summary>
		/// Function to close the log file.
		/// </summary>
		public void Close()
		{
			if (IsClosed)
			{
				return;
			}

			// Clean up.
			Print(string.Empty, LoggingLevel.All);
			Print("**** {0} ({2} {1}) {3}. ****", LoggingLevel.All, LogApplication,
			      GetType().Assembly.GetName().Version.ToString(), Resources.GOR_LOG_VERSION, Resources.GOR_LOG_ENDS);

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
		public void Open()
		{
			if (!IsClosed)
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
			Print("**** {0} ({2} {1}) {3} ****", LoggingLevel.All, LogApplication,
			      GetType().Assembly.GetName().Version.ToString(), Resources.GOR_LOG_VERSION, Resources.GOR_LOG_BEGINS);

			if (LogFilterLevel != LoggingLevel.NoLogging)
			{
				Print("**** {1}: {0}", LoggingLevel.All, LogFilterLevel, Resources.GOR_LOG_FILTER_LEVEL);
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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="appname"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the appname parameter is empty.</exception>
		public GorgonLogFile(string appname, string extraPath)
		{	
			GorgonDebug.AssertParamString(appname, "appname");

			IsClosed = true;

			LogApplication = appname;

			LogPath = GorgonComputerInfo.FolderPath(Environment.SpecialFolder.ApplicationData);
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

		#region IDisposable Members
		/// <summary>
		/// Function to remove resources.
		/// </summary>
		/// <param name="disposing">TRUE if we're removing managed resources and unmanaged, FALSE if only unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			Close();
		}

		/// <summary>
		/// Function to clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
