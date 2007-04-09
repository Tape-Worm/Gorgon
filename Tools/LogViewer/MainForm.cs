#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Monday, May 08, 2006 12:31:08 AM
// 
#endregion

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using SharpUtilities.Utility;

namespace GorgonLibrary.Tools
{
	/// <summary>
	/// Main log viewer interface.
	/// </summary>
	public partial class MainForm : Form
	{
		#region Value Types.
		/// <summary>
		/// Value type containing information about the log file.
		/// </summary>
		private struct FileInfo
		{
			#region Variables.
			/// <summary>
			/// Time when file was created.
			/// </summary>
			public DateTime FileCreateTime;
			/// <summary>
			/// Time when file was last written to.
			/// </summary>
			public DateTime FileLastWriteTime;
			/// <summary>
			/// Size of the file.
			/// </summary>
			public long FileSize;
			#endregion

			#region Methods.
			/// <summary>
			/// Indicates whether this instance and a specified object are equal.
			/// </summary>
			/// <param name="obj">Another object to compare to.</param>
			/// <returns>
			/// true if obj and this instance are the same type and represent the same value; otherwise, false.
			/// </returns>
			public override bool Equals(object obj)
			{
				return base.Equals(obj);
			}

			/// <summary>
			/// Returns the hash code for this instance.
			/// </summary>
			/// <returns>
			/// A 32-bit signed integer that is the hash code for this instance.
			/// </returns>
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
			#endregion

			#region Constructor
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="filePath">Path to the file to check.</param>
			public FileInfo(string filePath)
			{
				// Get info.
				System.IO.FileInfo info = new System.IO.FileInfo(filePath);

				FileSize = info.Length;
				FileCreateTime = info.CreationTimeUtc;
				FileLastWriteTime = info.LastWriteTimeUtc;
			}
			#endregion


			#region Operators.
			/// <summary>
			/// Equality operator.
			/// </summary>
			/// <param name="left">Left item to test.</param>
			/// <param name="right">Right item to test.</param>
			/// <returns>TRUE if equal, FALSE if not.</returns>
			public static bool operator ==(FileInfo left, FileInfo right)
			{
				if ((left.FileCreateTime == right.FileCreateTime) && (left.FileLastWriteTime == right.FileLastWriteTime) && (left.FileSize == right.FileSize))
					return true;
				else
					return false;
			}

			/// <summary>
			/// Inequality operator.
			/// </summary>
			/// <param name="left">Left item to test.</param>
			/// <param name="right">Right item to test.</param>
			/// <returns>FALSE if equal, TRUE if not.</returns>
			public static bool operator !=(FileInfo left, FileInfo right)
			{
				return !(left == right);
			}
			#endregion
		}
		#endregion

		#region Variables.
		private string _logFile = string.Empty;			// Log file text.
		private StringDictionary _logs;					// List of logs.
		private string _lastPath = string.Empty;		// Last log path.
		private string _fileName = string.Empty;		// Current filename.
		private FileInfo _fileInfo;						// File information.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the interface.
		/// </summary>
		private void ValidateInterface()
		{
			comboSection.Enabled = false;

			if (_logs.Count > 0)
			{
				if (comboSection.Items.Count > 0)
					comboSection.Enabled = true;
			}
		}

		/// <summary>
		/// Function to retrieve the log path from the registry.
		/// </summary>
		private void GetLogPath()
		{
			RegistryKey pathKey = null;			// Key that holds the path.

			try
			{		
				// Open the key.
				pathKey = Registry.CurrentUser.OpenSubKey("Software\\Tape_Worm\\Gorgon\\LogViewer");

				if (pathKey == null)
					return;

				_lastPath = pathKey.GetValue("LastPath").ToString();
				if (_lastPath == null)
					_lastPath = string.Empty;
			}
			catch(Exception ex)
			{
				UI.ErrorBox(this, "Unable to retrieve the log file path from the registry.\n" + ex.Message);
			}
			finally
			{
				if (pathKey != null)
					pathKey.Close();
			}
		}

		/// <summary>
		/// Function to save the log path to the registry.
		/// </summary>
		private void SaveLogPath()
		{
			RegistryKey pathKey = null;			// Key that holds the path.

			try
			{
				pathKey = Registry.CurrentUser.CreateSubKey("Software\\Tape_Worm\\Gorgon\\LogViewer");
				pathKey.SetValue("LastPath", _lastPath);
			}
			catch(Exception ex)
			{
				UI.ErrorBox(this, "Unable to retrieve the log file path from the registry.\n" + ex.Message);
			}
			finally
			{
				if (pathKey != null)
					pathKey.Close();
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Closing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"></see> that contains the event data.</param>
		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			SaveLogPath();
		}

		/// <summary>
		/// Function to load and parse the log file.
		/// </summary>
		/// <param name="fileName">Name of the file to load.</param>
		private void LoadAndParse(string fileName)
		{
			TextReader reader = null;		// Reader for log file.
			string[] lines;					// Lines in the file.
			string sectionName;				// Name of the section.
			int sectionStart = -1;			// Start of log section.
			int sectionEnd = -1;			// End of log section.

			try
			{
				comboSection.Items.Clear();
				textLog.Text = string.Empty;
				_logFile = string.Empty;
				_logs.Clear();

				ValidateInterface();

				// Get the file.
				reader = new StreamReader(fileName);
				_logFile = reader.ReadToEnd();

				// Begin parsing.

				// First, locate all "sections".
				lines = _logFile.Split('\n');

				// Find sections.
				comboSection.Items.Add("(Uncategorized)");
				foreach (string line in lines)
				{
					if ((line.IndexOf("[Begin: ") == 0) && (line.IndexOf("]") > -1))
					{
						// Add section.
						sectionName = line.Substring(8, line.IndexOf("]") - 8);
						if (comboSection.Items.IndexOf(sectionName)<0)
							comboSection.Items.Add(sectionName);
					}
				}

				// Begin parsing log text.
				foreach (string section in comboSection.Items)
				{
					int sectionLength = 0;				// Length of section string.
					string logText = string.Empty;		// Log text. 

					sectionStart = -1;
					sectionEnd = -1;
					sectionName = "[Begin: " + section + "]";
					// We have the start of a section.
					sectionStart = _logFile.IndexOf(sectionName);
					while (sectionStart > -1)
					{
						sectionEnd = _logFile.IndexOf("[End: " + section + "]") + 6 + section.Length;

						if ((sectionStart > -1) && (sectionEnd > sectionStart))
						{
							sectionLength = sectionEnd - sectionStart + 2;
							logText = _logFile.Substring(sectionStart + sectionName.Length+1, sectionLength - 10 - section.Length - sectionName.Length + 1);

							string sectionBegin = string.Empty;			// Beginning of log.
							string sectionRemainder = string.Empty;		// Remainder of log.

							// Remove text from original.
							if (sectionStart > 0)
								sectionBegin = _logFile.Substring(0, sectionStart-1);
							sectionRemainder = _logFile.Substring(sectionEnd + 2);
							_logFile = sectionBegin + sectionRemainder;

							// Add to log collection.
							if (!_logs.ContainsKey(section))
								_logs.Add(section, logText);
							else
								_logs[section] += logText;
						}
						sectionStart = _logFile.IndexOf(sectionName);
					}
				}

				if (_logFile.Length > 0)
					_logs.Add("(Uncategorized)", _logFile);

				if (comboSection.Items.Count > 0)
					comboSection.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				_logFile = string.Empty;
				_logs.Clear();

				UI.ErrorBox(this, "Could not open/parse the log file:\n" + ex.Message, ex.Message + "\nStack Trace:\n" + ex.StackTrace);
			}
			finally
			{
				if (reader != null)
					reader.Close();

				reader = null;
				ValidateInterface();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
			_logFile = string.Empty;
			_logs = new StringDictionary();
			GetLogPath();
			ValidateInterface();
		}
		#endregion

		/// <summary>
		/// Handles the Click event of the menuExit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuExit_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Handles the Click event of the menuOpen control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuOpen_Click(object sender, EventArgs e)
		{
			timerTail.Enabled = false;
			if (_lastPath != string.Empty)
				fileOpen.InitialDirectory = _lastPath;
			if (fileOpen.ShowDialog(this) == DialogResult.OK)
			{
				LoadAndParse(fileOpen.FileName);
				_fileName = fileOpen.FileName;
				_lastPath = Path.GetDirectoryName(fileOpen.FileName);
				_fileInfo = new FileInfo(fileOpen.FileName);
				timerTail.Enabled = true;
			}			
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboSection control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void comboSection_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_logs.ContainsKey(comboSection.Text))
			{
				textLog.Text = _logs[comboSection.Text].Replace("\n", "\r\n");
				comboSection.Focus();
			}
			ValidateInterface();
		}

		/// <summary>
		/// Handles the Tick event of the timerTail control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void timerTail_Tick(object sender, EventArgs e)
		{
			FileInfo fileInfo;

			timerTail.Enabled = false;
			if (_fileName != string.Empty)
			{
				if (!File.Exists(_fileName))
				{
					timerTail.Enabled = true;
					return;
				}

				fileInfo = new FileInfo(_fileName);	// File information.

				// File has differences, ask to reload.
				if (fileInfo != _fileInfo)
				{
					if (UI.YesNoBox("The file " + _fileName + " has changed, would you like to re-load it?"))
					{
						if (File.Exists(_fileName))
						{
							LoadAndParse(_fileName);
							_fileInfo = new FileInfo(_fileName);
						}
					}	
					else
						_fileInfo = new FileInfo(_fileName);
				}
				timerTail.Enabled = true;
			}
		}
	}
}