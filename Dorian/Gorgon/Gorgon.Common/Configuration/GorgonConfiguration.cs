#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, May 02, 2012 10:08:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Configuration
{
	/// <summary>
	/// Manages the configuration for the Gorgon applications.
	/// </summary>
	public static class GorgonConfiguration
	{
		#region Constants.
		private const string ConfigVersion = "1.0";				// Version to match in the configuration file.
		#endregion

		#region Variables.
		private static XElement _root = null;							// Root of the configuration document.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve a setting and its values from the configuration.
		/// </summary>
		/// <param name="sectionName">Section XML node containing the values.</param>
		/// <param name="settingName">Name of the setting to find.</param>
		/// <returns>An enumerable list of values for the setting or NULL (Nothing in VB.Net) if the setting was not found.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settingName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="settingName"/> parameter is an empty string.
		/// <para>-or-</para><para>Thrown when the <paramref name="sectionName"/> could not be found.</para>
		/// </exception>
		public static IEnumerable<GorgonConfigurationValue> GetSettings(string sectionName, string settingName)
		{
			IEnumerable<XElement> section = null;

			GorgonDebug.AssertParamString(settingName, "settingName");

			if (string.IsNullOrEmpty(sectionName))
				section = _root.Descendants();
			else
			{
				if (_root.Element(sectionName) != null)
					section = _root.Element(sectionName).Descendants();
				else
					throw new ArgumentException("Could not find the section '" + sectionName + "' in the configuration file.", "sectionName");
			}

			var settings = from setting in section
							where ((setting.Attribute("Value") != null) && (string.Compare(setting.Name.LocalName, settingName, true) == 0))
							select new GorgonConfigurationValue(setting.Name.LocalName, setting.Attribute("Value").Value);

			if ((settings == null) || (settings.Count() == 0))
				return null;

			return settings;
		}

		/// <summary>
		/// Function to load the configuration for the framework from an XML file.  
		/// </summary>
		/// <param name="configurationPath">Path to the configuration.</param>
		/// <param name="plugInSigningKeys">List of plug-in type names and keys that are allowed.</param>
		/// <remarks>Calling this will load all the plug-ins registered in the config file.  Existing plug-ins will not be removed, if a plug-in is already loaded then that one will be used instead.
		/// <para>If the user requires signed plug-ins, then they should pass in a dictionary of named keys (a name and an array of bytes containing the public key) for security.  Otherwise pass NULL (Nothing in VB.Net) to the <paramref name="plugInSigningKeys"/> parameter 
		/// to load regardless if the assembly is signed or not.  The name of the key should be the same as the file name of the assembly (without the extension).</para>
		/// <para>If checking for signed plug-ins, then the configuration should contain a line with the XML attribute TypeName and the value should be the fully qualified type name of the plug-in.  This name is matched against the list in the <paramref name="plugInSigningKeys"/> parameter.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="configurationPath"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="configurationPath"/> parameter is an empty string.
		/// <para>-or-</para><para>Thrown when the configuration file could not be read.</para>
		/// </exception>		
		public static void LoadConfiguration(string configurationPath, IDictionary<string, byte[]> plugInSigningKeys)
		{
			XDocument configFile = null;

			if (!File.Exists(configurationPath))
				throw new ArgumentException("The configuration file '" + configurationPath + "' does not exist.", "configurationPath");

			configFile = XDocument.Load(configurationPath);

			_root = configFile.Element("Header");

			if ((_root == null) || (_root.Attribute("Version") == null))
				throw new ArgumentException("'" + configurationPath + "' is not a valid configuration file.", "configurationPath");

			var version = _root.Attribute("Version");

			if (string.Compare(version.Value, ConfigVersion, true) != 0)
				throw new ArgumentException("'" + configurationPath + "' is not the correct version.  Need " + ConfigVersion + ", got " + version.Value, "configurationPath");

			// Load plug-ins.
			var plugIns = _root.Descendants("PlugIn");

			foreach (var plugIn in plugIns)
			{
				var path = plugIn.Attribute("Path");
				var plugInType = plugIn.Attribute("TypeName");
				string fullPath = string.Empty;
				string assemblyName = Path.GetFileNameWithoutExtension(configurationPath);

				if ((path == null) || (string.IsNullOrEmpty(path.Value)))
					throw new ArgumentException("'" + configurationPath + "' has malformed plug-in entries.", "configurationPath");

				fullPath = Path.GetFullPath(path.Value);

				if (File.Exists(fullPath))
				{
					byte[] key = null;

					if ((plugInSigningKeys != null) && (plugInSigningKeys.Count > 0))
					{
						if ((!string.IsNullOrEmpty(assemblyName)) && (plugInSigningKeys.ContainsKey(assemblyName)))
							key = plugInSigningKeys[assemblyName];
						else
							throw new ArgumentException("The plug-in '" + assemblyName + "' does not have a key associated with it.", "plugInSigningKeys");
					}

					if (key != null)
					{
						if (Gorgon.PlugIns.IsPlugInSigned(fullPath, key) != PlugInSigningResult.Signed)
							throw new ArgumentException("The plug-in '" + assemblyName + "' is not signed or is not signed with the correct key.");
					}

					Gorgon.PlugIns.LoadPlugInAssembly(fullPath);
				}
				else
					throw new FileNotFoundException("The plug-in assembly '" + fullPath + "' was not found.");
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes the <see cref="GorgonConfiguration"/> class.
		/// </summary>
		static GorgonConfiguration()
		{			
		}
		#endregion
	}
}
