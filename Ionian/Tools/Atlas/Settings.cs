#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Monday, May 07, 2007 4:56:52 PM
// 
#endregion

using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using Dialogs;

namespace Atlas
{
	/// <summary>
	/// Object to represent a method to store and retrieve settings.
	/// </summary>
	public static class Settings
	{
		#region Variables.
		private static string _locationFolder = string.Empty;		// Location of the settings.
		private static XmlDocument _xmlData = null;					// XML document that holds the settings.
		private static XmlNode _xmlRoot = null;						// Root element.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the location of the config file.
		/// </summary>
		public static string Location
		{
			get
			{
				return _locationFolder;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
					value = @".\";

				value = value.Replace("/", @"\");

				// Remove any doubled up slashses.
				while(value.IndexOf(@"\\") > -1)
					value = value.Replace(@"\\", @"\");

				// Append a path separator.
				if (value[value.Length - 1] != '\\')
					value += @"\";

				_locationFolder = value;
			}
		}

		/// <summary>
		/// Property to set or return the root node for the settings.
		/// </summary>
		public static string Root
		{
			get
			{
				return _xmlRoot.Name;
			}
			set
			{
				XmlNode element = null;		// XML element.

				if (string.IsNullOrEmpty(value))
					value = "Settings";

				// Find the node.
				element = _xmlData.SelectSingleNode("//" + value);

				// If found, replace the root.
				if (element != null)
					_xmlRoot = element;
				else
				{
					// Create the root.
					element = _xmlData.CreateElement(value);
					_xmlRoot = _xmlRoot.AppendChild(element);
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return a setting from the file.
		/// </summary>
		/// <param name="settingName">Name of the setting.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>The value for the setting.</returns>
		public static string GetSetting(string settingName, string defaultValue)
		{
			XmlNode element = null;		// Node with the element name.

			// Get the setting node.
			element = _xmlRoot.SelectSingleNode(settingName);

			if (element != null)
				return element.InnerText;
			else
				return defaultValue;
		}

		/// <summary>
		/// Function to set a value to the config file.
		/// </summary>
		/// <param name="settingName">Name of the setting.</param>
		/// <param name="value">Value to set.</param>
		public static void SetSetting(string settingName, string value)
		{
			XmlNode element = null;		// Node with the element name.

			if (value == null)
				value = string.Empty;

			// Look up the existing name.
			element = _xmlRoot.SelectSingleNode(settingName);

			// If not found, then create it.
			if (element == null)
			{
				// Add the node.
				element = _xmlData.CreateElement(settingName);
				element.InnerText = value;
				_xmlRoot.AppendChild(element);
			}
			else
				element.InnerText = value;
		}

		/// <summary>
		/// Function to load the settings file.
		/// </summary>
		/// <param name="fileName">File that contains the settings.</param>
		/// <returns>TRUE if the file was loaded, FALSE if not.</returns>
		public static bool Load(string fileName)
		{
			try
			{
				// Extract the file name.
				fileName = Path.GetFileName(fileName);

				// Prepend the path.
				fileName = _locationFolder + fileName;

				// If the file doesn't exist, then exit.
				if (!File.Exists(fileName))
					return false;

				// Load the document.
				_xmlData.Load(fileName);

				// Get the root.
				_xmlRoot = _xmlData.SelectSingleNode("//Settings");

				if (_xmlRoot == null)
					UI.ErrorBox(null, "The configuration file is invalid.");

				return true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(null, ex);
			}

			return false;
		}

		/// <summary>
		/// Function to determine if a setting exists.
		/// </summary>
		/// <param name="settingName">Setting to check.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public static bool HasSetting(string settingName)
		{
			return (_xmlRoot.SelectSingleNode(settingName) != null);
		}
		
		/// <summary>
		/// Function to determine if a setting has a value or not.
		/// </summary>
		/// <param name="settingName">Name of the setting.</param>
		/// <returns>TRUE if there's a value, FALSE if not.</returns>
		public static bool HasValue(string settingName)
		{
			if (!HasSetting(settingName))
				return false;

			return (!string.IsNullOrEmpty(_xmlRoot.SelectSingleNode(settingName).InnerText));
		}

		/// <summary>
		/// Function to save the current settings.
		/// </summary>
		/// <param name="fileName">File that will contain the settings.</param>
		public static void Save(string fileName)
		{
			try
			{
				// Switch back to the root.
				Root = string.Empty;

				// If we have no settings, don't bother.
				if ((_xmlRoot == null) || (_xmlData.ChildNodes.Count < 1) || (_xmlRoot.ChildNodes.Count < 1))
					return;

				// Extract the file name.
				fileName = Path.GetFileName(fileName);

				// Prepend the path.
				fileName = _locationFolder + fileName;

				// Create the directory if it doesn't exist.
				if (!Directory.Exists(_locationFolder))
					Directory.CreateDirectory(_locationFolder);

				// Save the document.
				_xmlData.Save(fileName);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(null, ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		static Settings()
		{
			// Get the user folder.
			_locationFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			Location = _locationFolder + @"\Tape_Worm\" + Application.ProductName + @"\";

			// Initialize document.
			_xmlData = new XmlDocument();
			_xmlData.AppendChild(_xmlData.CreateProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\""));
			_xmlRoot = _xmlData.CreateElement("Settings");
			_xmlData.AppendChild(_xmlRoot);
		}
		#endregion
	}
}
