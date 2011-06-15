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
using System.IO;
using System.Text;

namespace GorgonLibrary.Diagnostics
{
	/// <summary>
	/// Enumeration containing the logging levels.
	/// </summary>
	public enum GorgonLoggingLevel
	{
		/// <summary>None, this will disable the log.</summary>
		None = 0,
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
		private StreamWriter _stream = null;					// File stream object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the filtering level of this log.
		/// </summary>
		public GorgonLoggingLevel LogFilterLevel
		{
			get;
			set;
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
		public void Print(string formatSpecifier, GorgonLoggingLevel level, params object[] arguments)
		{
#if DEBUG
			StringBuilder outputLine = new StringBuilder(512);			// Output string 
			string[] lines = null;										// List of lines.

			if (((level <= LogFilterLevel) || (level == GorgonLoggingLevel.All)) && (LogFilterLevel != GorgonLoggingLevel.None) && (!IsClosed))
			{
				if (string.IsNullOrEmpty(formatSpecifier) || (formatSpecifier == "\n") || (formatSpecifier == "\r"))
				{
					outputLine.Append("[");
					outputLine.Append(System.DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt"));
					outputLine.Append("]\n");
				}
				else
				{
					lines = formatSpecifier.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

					for (int i = 0; i < lines.Length; i++)
					{
						outputLine.Append("[");
						outputLine.Append(System.DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt"));
						outputLine.Append("] ");

						outputLine.Append(string.Format(lines[i] + "\n", arguments));
					}
				}
		
				_stream.Write(outputLine.ToString());
				_stream.Flush();
			}
#endif
		}

		/// <summary>
		/// Function to close the log file.
		/// </summary>
		public void Close()
		{
#if DEBUG
			if ((LogFilterLevel != GorgonLoggingLevel.None) && (!IsClosed))
			{
				// Clean up.
				Print("**** {0} (Version {1}) Logging ends. ****", GorgonLoggingLevel.All, LogApplication, GetType().Assembly.GetName().Version.ToString());

				if (_stream != null)
				{
					_stream.Close();
					_stream = null;
				}
			}			
#endif
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Function to open the log file.
		/// </summary>
		public void Open()
		{
#if DEBUG
			if ((LogFilterLevel != GorgonLoggingLevel.None) && (IsClosed))
			{
				// Create the directory if it doesn't exist.
				if (!Directory.Exists(Path.GetDirectoryName(LogPath)))
					Directory.CreateDirectory(Path.GetDirectoryName(LogPath));

				// Open the stream.
				_stream = new StreamWriter(File.Open(LogPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read), Encoding.UTF8);
				_stream.Flush();

				IsClosed = false;
				Print("**** {0} (Version {1}) logging begins ****", GorgonLoggingLevel.All, LogApplication, GetType().Assembly.GetName().Version.ToString());				
			}
#endif
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonLogFile"/> class.
		/// </summary>
		/// <param name="appname">File name for the log file.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="System.ArgumentException">Thrown whent he parameter is empty.</exception>
		public GorgonLogFile(string appname)
		{
			GorgonUtility.AssertParamString(appname, "appname");

			IsClosed = true;

#if DEBUG
			LogFilterLevel = GorgonLoggingLevel.Intermediate;
			LogApplication = appname;

			LogPath = GorgonUtility.GetUserApplicationPath(appname) + Path.ChangeExtension(GorgonUtility.FormatFileName(GetType().Assembly.GetName().Name), ".log");

			if (string.IsNullOrEmpty(LogPath))
				throw new IOException("The assembly name is not valid for a file name.");
#endif
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to remove resources.
		/// </summary>
		/// <param name="disposing">TRUE if we're removing managed resources and unmanaged, FALSE if only unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				Close();
		}

		/// <summary>
		/// Function to clean up.
		/// </summary>
		void IDisposable.Dispose()
		{
			Dispose(true);			
		}
		#endregion
	}
}
