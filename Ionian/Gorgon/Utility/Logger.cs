//
// SharpUtils.
// Copyright (C) 2005 Michael Winsor
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
// Created: Thursday, March 24, 2005 10:01:18 AM
//

using System;
using System.IO;
using System.Text;
using System.Security.Permissions;
using GorgonLibrary.Internal;

namespace GorgonLibrary
{
    /// <summary>
    /// Enumeration containing the logging levels.
    /// </summary>
    public enum LoggingLevel
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
	/// An object representing a logging interface.
	/// </summary>
	/// <remarks>
	/// Logging information can be output to the trace console, or to a file or both.
	/// </remarks>
	public class Logger 
		: NamedObject, IDisposable
	{
		#region Variables.
		private StreamWriter _stream = null;		// File stream object.
		private string _filename;					// Filename of the log.
		private bool _debug;						// TRUE to send output to the console.
		private LoggingLevel _level;				// Filtering level of the log.
		private string _lastSection;				// Last section name.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the filtering level of this log.
		/// </summary>
		public LoggingLevel LogFilterLevel
		{
			get
			{
				return _level;
			}
			set
			{
				_level = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the messages go to the console or not.
		/// </summary>
		public bool UseConsole
		{
			get
			{
				return _debug;
			}
			set
			{
				if ((_debug) && (_stream == null))
					return;

				_debug = value;
			}
		}
		#endregion

		#region Functions.
		/// <summary>
		/// Print a line to the logfile.
		/// </summary>
		/// <param name="section">Section that the line pertains to.</param>
		/// <param name="formatSpecifier">Format specifier for the line.</param>
		/// <param name="level">Level that this message falls under.</param>
		/// <param name="arguments">List of optional arguments.</param>
		public void Print(string section, string formatSpecifier, LoggingLevel level, params object[] arguments)
		{
			StringBuilder outputLine = new StringBuilder(512);			// Output string 

			try
			{
				if (((level <= _level) || (level == LoggingLevel.All)) && (_level != LoggingLevel.None))
				{
					// Write section header.
					if (_lastSection != section)
					{
						if ((_lastSection != null) && (_lastSection != string.Empty))
						{
							outputLine.Append("(");
							outputLine.Append(Name);
							outputLine.Append(") ");
							outputLine.Append("[End: ");
							outputLine.Append(_lastSection);
							outputLine.Append("]\n");
						}
						if ((section != null) && (section != string.Empty))
						{
							outputLine.Append("\n(");
							outputLine.Append(Name);
							outputLine.Append(") ");
							outputLine.Append("[Begin: ");
							outputLine.Append(section);
							outputLine.Append("]\n");
						}
						_lastSection = section;
					}

					outputLine.Append("(");
					outputLine.Append(Name);
					outputLine.Append(") ");
					outputLine.Append("[");
					outputLine.Append(System.DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt"));
					outputLine.Append("] ");					
					outputLine.Append(string.Format(formatSpecifier + "\n", arguments));

					// Write to the text file.
					if (_stream != null)
					{
						_stream.Write(outputLine.ToString());
						_stream.Flush();
					}

					if (_debug)
						System.Diagnostics.Trace.Write(outputLine.ToString());
				}
			}
			catch(Exception ex)
			{
                if ((_filename != null) && (_filename != string.Empty))
                    throw new LogfileCannotWriteException(_filename, ex);
                else
                    throw new LogfileCannotWriteException("Console trace log.", ex);
			}
		}

		/// <summary>
		/// Print a line to the logfile.
		/// </summary>
		/// <param name="formatSpecifier">Format specifier for the line.</param>
		/// <param name="level">Level that this message falls under.</param>
		/// <param name="arguments">List of optional arguments.</param>
		public void Print(string formatSpecifier,LoggingLevel level,params object[] arguments)
		{
			Print(null, formatSpecifier, level, arguments);
		}

		/// <summary>
		/// Function to close the log file.
		/// </summary>
		public void Close()
		{
			if (_level != LoggingLevel.None)
			{
				// Clean up.
				Print("**** {0} Logging ends. ****", LoggingLevel.All, Name);

				if (_stream != null)
				{
					_stream.Close();
					_stream = null;
				} 
			}
		}

		/// <summary>
		/// Function to open the log file.
		/// </summary>
		public void Open()
		{
			if (_level != LoggingLevel.None)
			{
				// Do nothing if we're just dumping to the debug window.
				if (_stream != null)
					return;

				try
				{
					// Create the directory if it doesn't exist.
					if (!Directory.Exists(Path.GetDirectoryName(_filename)))
						Directory.CreateDirectory(Path.GetDirectoryName(_filename));

					// Open the stream.
					_stream = new StreamWriter(File.Open(_filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read), Encoding.UTF8);
					_stream.Flush();

					// Write an initial blank line.
					_stream.Write("\n");					

					Print("**** {0} logging begins ****", LoggingLevel.All, Name);
				}
                catch(Exception ex)
				{
                    if ((_filename != null) && (_filename != string.Empty))
                        throw new LogfileCannotOpenException(_filename, ex);
                    else
                        throw new LogfileCannotOpenException("Console trace log.", ex);
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.		
		/// </summary>
		/// <param name="appname">File name for the log file.</param>
		public Logger(string appname) : this(appname, null)
		{			
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="appname">File name for the log file.</param>
		/// <param name="path">File path for the file. NULL to force the console.</param>
		public Logger(string appname,string path) : base(appname)
		{
			_level = LoggingLevel.Intermediate;
			_lastSection = string.Empty;

            if (string.IsNullOrEmpty(appname))
                Name = "Console";

			// Force the use of the console.
			if (path == null)
				_debug = true;
			else
			{
				_debug = false;
				_filename = Path.ChangeExtension(path, ".log");
			}
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
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
