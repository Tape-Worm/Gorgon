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
// Created: Wednesday, May 02, 2012 10:18:24 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Configuration
{
	/// <summary>
	/// A base class used to handle settings for an application.
	/// </summary>
	public abstract class GorgonApplicationSettings
	{
		#region Variables.
		private XDocument _xmlSettings = null;															// XML document containing application settings.
		private string _path = string.Empty;															// Path to the XML settings.
		private IDictionary<PropertyInfo, ApplicationSettingAttribute> _properties = null;		// List of properties to serialize.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the application settings version.
		/// </summary>
		/// <remarks>Assigning NULL (Nothing in VB.Net) will bypass version checking.</remarks>
		public Version Version
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to reset the XML data.
		/// </summary>
		private void ResetXML()
		{
			if (Version != null)
				_xmlSettings = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
										new XElement("ApplicationSettings",
										new XAttribute("Version", Version.ToString())));
			else
				_xmlSettings = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
										new XElement("ApplicationSettings"));
		}

		/// <summary>
		/// Function to convert the specified value into a string.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>The string value for the object.</returns>
		private string ConvertValue(object value)
		{
			IList<TypeConverterAttribute> attrib = null;
			TypeConverter converter = null;
			Type type = null;

			if (value == null)
				return null;

			type = value.GetType();

			attrib = type.GetCustomAttributes(typeof(TypeConverterAttribute), true) as IList<TypeConverterAttribute>;

			if (attrib.Count > 0)
			{
				converter = Activator.CreateInstance(Type.GetType(attrib[0].ConverterTypeName)) as TypeConverter;

				if (converter.CanConvertTo(typeof(string)))
					return converter.ConvertToInvariantString(value);
			}

			return value.ToString();
		}

		/// <summary>
		/// Function to convert the specified value from a string.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="type">Type to convert into.</param>
		/// <returns>The value for the object.</returns>
		private object UnconvertValue(string value, Type type)
		{
			IList<TypeConverterAttribute> attrib = null;
			TypeConverter converter = null;

			if (value == null)
				return null;

			if (type != typeof(string))
			{
				if (value == string.Empty)
					return null;
			}
			else
				return value;

			if (type.IsEnum)
				return Enum.Parse(type, value);

			attrib = type.GetCustomAttributes(typeof(TypeConverterAttribute), true) as IList<TypeConverterAttribute>;

			if (attrib.Count > 0)
			{
				converter = Activator.CreateInstance(Type.GetType(attrib[0].ConverterTypeName)) as TypeConverter;

				if (converter.CanConvertFrom(typeof(string)))
					return converter.ConvertFromInvariantString(value);
			}

			return Convert.ChangeType(value, type);
		}

		/// <summary>
		/// Function to serialize the property settings.
		/// </summary>
		private void SerializeSettings()
		{
			ResetXML();

			if (_properties == null)
				GetProperties();

			// Get the unique sections.
			var sections = (from property in _properties
							where !string.IsNullOrEmpty(property.Value.Section)
							select property.Value.Section).Distinct();

			// Create section elements.
			foreach (var section in sections)
				AddSection(string.Empty, section);

			foreach (KeyValuePair<PropertyInfo, ApplicationSettingAttribute> property in _properties)
			{
				XElement section = GetSectionElement(property.Value.Section);

				if ((property.Key.PropertyType.IsGenericType) || (property.Key.PropertyType == typeof(IList)))
				{
					Type valueType = null;
					object collection = null;
					PropertyInfo collectionIndex = null;
					PropertyInfo countProperty = null;
					int count = 0;

					if (property.Key.PropertyType.IsGenericType)
					{
						Type[] genericType = property.Key.PropertyType.GetGenericArguments();

						valueType = genericType[0];

						if ((!valueType.IsPrimitive) && (valueType != typeof(string)) && (valueType != typeof(object)))
							throw new GorgonException(GorgonResult.CannotEnumerate, "Cannot enumerate the property '" + property.Key.Name + "' the generic parameter type is not a primitive or string type.");
					}
					else
						valueType = typeof(object);

					collection = property.Key.GetValue(this, null);
					collectionIndex = collection.GetType().GetProperty("Item");
					countProperty = collection.GetType().GetProperty("Count");

					count = Convert.ToInt32(countProperty.GetValue(collection, null));

					for (int i = 0; i < count; i++)
					{
						string name = string.Empty;

						name = (string.IsNullOrEmpty(property.Value.SettingName) ? property.Key.Name + "_item" : property.Value.SettingName + "_item");

						section.Add(new XElement("Setting",
									new XAttribute(property.Value.SettingName, ConvertValue(collectionIndex.GetValue(collection, new object[] { i })))));
					}
				}
				else
				{
					string name = string.Empty;

					name = (string.IsNullOrEmpty(property.Value.SettingName) ? property.Key.Name : property.Value.SettingName);
					section.Add(new XElement("Setting",
								new XAttribute(name, ConvertValue(property.Key.GetValue(this, null)))));
				}
			}
		}

		/// <summary>
		/// Function to deserialize the XML data into the properties used for the settings.
		/// </summary>
		private void DeserializeSettings()
		{
			GetProperties();

			foreach (KeyValuePair<PropertyInfo, ApplicationSettingAttribute> property in _properties)
			{
				if ((property.Value.PropertyType.IsGenericType) || (property.Value.PropertyType == typeof(IList)))
				{
					Type valueType = null;
					object collection = null;
					MethodInfo addMethod = null;
					MethodInfo clearMethod = null;
					object[] values = null;
					
					if (property.Key.PropertyType.IsGenericType)
					{
						Type[] genericType = property.Key.PropertyType.GetGenericArguments();
						valueType = genericType[0];

						if ((!valueType.IsPrimitive) && (valueType != typeof(string)) && (valueType != typeof(object)))
							throw new GorgonException(GorgonResult.CannotEnumerate, "Cannot enumerate the property '" + property.Key.Name + "' the generic parameter type is not a primitive or string type.");
					}
					else
						valueType = typeof(object);

					string settingName = string.Empty;

					settingName = (string.IsNullOrEmpty(property.Value.SettingName) ? property.Key.Name + "_item" : property.Value.SettingName);
					values = GetSettings(property.Value.Section, settingName, valueType);

					collection = property.Key.GetValue(this, null);
					addMethod = collection.GetType().GetMethod("Add");
					clearMethod = collection.GetType().GetMethod("Clear");

					clearMethod.Invoke(collection, null);

					for (int i = 0; i < values.Length; i++)
						addMethod.Invoke(collection, new object[] { values[i] });
				}
				else
				{
					string settingName = (string.IsNullOrEmpty(property.Value.SettingName) ? property.Key.Name : property.Value.SettingName);
					object value = GetSetting(property.Value.Section, settingName, property.Value.PropertyType);

					if ((value == null) && (property.Value.HasDefault))
						value = property.Value.DefaultValue;

					// Use application setting if we still don't have a value.
					if (value != null)
						property.Key.SetValue(this, value, null);
				}
			}
		}

		/// <summary>
		/// Function to retrieve the properties.
		/// </summary>
		private void GetProperties()
		{
			Type settingsType = this.GetType();
			PropertyInfo[] properties = null;

			_properties = new Dictionary<PropertyInfo, ApplicationSettingAttribute>();

			properties = settingsType.GetProperties();

			foreach (PropertyInfo info in properties)
			{
				ApplicationSettingAttribute[] attributes = (ApplicationSettingAttribute[])info.GetCustomAttributes(typeof(ApplicationSettingAttribute), true);

				if ((attributes != null) && (attributes.Length > 0))
				{
					_properties.Add(info, attributes[0]);
					if (!(info.PropertyType.IsGenericType) && (info.PropertyType != typeof(IList)) && (attributes[0].HasDefault))
						info.SetValue(this, attributes[0].DefaultValue, null);
				}
			}
		}

		/// <summary>
		/// Function to compare version numbers.
		/// </summary>
		/// <returns>TRUE if the versions match, FALSE if not.</returns>
		private bool CheckVersion()
		{
			XElement rootElement = _xmlSettings.Element("ApplicationSettings");
			Version compareVersion = null;

			if (Version == null)
				return true;

			if (rootElement == null)
				throw new GorgonException(GorgonResult.InvalidFileFormat, "Not a Gorgon application settings file.");

			// If we don't have a version attribute, then we're not versioning this file.
			if (rootElement.Attribute("Version") == null)
				return true;

			if (!Version.TryParse(rootElement.Attribute("Version").Value, out compareVersion))
				throw new GorgonException(GorgonResult.InvalidFileFormat, "The settings version number cannot be read.");

			return compareVersion == Version;
		}

		/// <summary>
		/// Function to return the section element defined by the name.
		/// </summary>
		/// <param name="sectionName">Name of the section to retrieve.</param>
		/// <returns>The element with the section name.</returns>
		private XElement GetSectionElement(string sectionName)
		{
			if (string.IsNullOrEmpty(sectionName))
				return _xmlSettings.Element("ApplicationSettings");

			return (from section in _xmlSettings.Descendants("Section")
					where ((section != null) && (string.Compare(section.Attribute("SectionName").Value, sectionName) == 0))
					select section).FirstOrDefault();
		}

		/// <summary>
		/// Function to add a section to the configuration.
		/// </summary>
		/// <param name="section">Section that will contain the new section.</param>
		/// <param name="name">Name of the section.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter was NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the applicationName parameter is empty or the <paramref name="section"/> does not exist.</exception>
		private void AddSection(string section, string name)
		{
			XElement currentSection = null;
				
			GorgonDebug.AssertParamString(name, "name");

			currentSection = GetSectionElement(section);
			currentSection.Add(new XElement("Section", 
							new XAttribute("SectionName", name)));
		}

		/// <summary>
		/// Function to retrieve multiple settings with the same name.
		/// </summary>
		/// <param name="section">Section the settings reside under.</param>
		/// <param name="settingName">Name of the setting to retrieve.</param>
		/// <param name="valueType">Type of values stored in the collection.</param>
		/// <returns>A list containing the settings with the name specified in <paramref name="settingName"/>.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settingName"/> parameter was NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the settingName parameter is empty or the <paramref name="section"/> or the <paramref name="settingName"/> could not be found.</exception>
		private object[] GetSettings(string section, string settingName, Type valueType)
		{
			XElement currentSection = null;
			IList settings = null;

			GorgonDebug.AssertParamString(settingName, "settingName");

			currentSection = GetSectionElement(section);
			if (currentSection == null)
				return new object[0];
			
			var currentSetting = (from setting in currentSection.Descendants("Setting")
								  where ((setting != null) && (setting.Attribute(settingName) != null))
								  select setting);

			settings = new ArrayList();
			if (currentSetting != null)
			{
				foreach (var settingItem in currentSetting)
					settings.Add(UnconvertValue(settingItem.Attribute(settingName).Value, valueType));
			}

			return ((ArrayList)settings).ToArray();
		}

		/// <summary>
		/// Function to retrieve a setting from the configuration.
		/// </summary>
		/// <param name="section">Name of the section that the setting belongs in.</param>
		/// <param name="settingName">Name of the setting.</param>
		/// <param name="valueType">The value type.</param>
		/// <returns>The value in the setting.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settingName"/> parameter was NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the settingName parameter is empty or the <paramref name="section"/> or the <paramref name="settingName"/> could not be found.</exception>
		private object GetSetting(string section, string settingName, Type valueType)
		{
			XElement currentSection = null;

			GorgonDebug.AssertParamString(settingName, "settingName");

			currentSection = GetSectionElement(section);
			if (currentSection == null)
				return null;

			var currentSetting = (from settings in currentSection.Descendants("Setting")
								  where ((settings != null) && (settings.Attribute(settingName) != null))
								  select settings).FirstOrDefault();

			if (currentSetting == null)
				return null;

			return UnconvertValue(currentSetting.Attribute(settingName).Value, valueType);
		}

		/// <summary>
		/// Function to update an existing setting.
		/// </summary>
		/// <param name="section">Section that the setting belongs in.</param>
		/// <param name="settingName">Name of the setting.</param>
		/// <param name="value">Value for the setting.</param>
		/// <remarks>If the setting does not exist, it will be added.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settingName"/> parameter was NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the settingName parameter is empty or the <paramref name="section"/> could not be found.</exception>
		private void UpdateSetting(string section, string settingName, string value)
		{
			XElement currentSection = null;

			GorgonDebug.AssertParamString(settingName, "settingName");

			currentSection = GetSectionElement(section);
			if (currentSection == null)
			{
				AddSection(string.Empty, section);
				currentSection = GetSectionElement(section);
			}
						
			var currentSetting = (from settings in currentSection.Descendants("Setting")
								  where ((settings != null) && (settings.Attribute(settingName) != null))
								  select settings).FirstOrDefault();

			if (currentSetting != null)
				currentSetting.Attribute(settingName).Value = value;
			else
				currentSection.Add(new XElement("Setting", new XAttribute(settingName, value)));
		}

		/// <summary>
		/// Function to save the settings to a file.
		/// </summary>
		/// <remarks>No versioning will be applied to the settings file when the <see cref="P:GorgonLibrary.Configuration.GorgonApplicationSettings.Version">Version</see> property is NULL (Nothing in VB.Net).</remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the file being saved is not of the same format as an Gorgon application setting file.</exception>
		public void Save()
		{
			SerializeSettings();

			if (Version != null)
			{
				XElement rootElement = _xmlSettings.Element("ApplicationSettings");

				if (rootElement == null)
					throw new GorgonException(GorgonResult.InvalidFileFormat, "The file is not a Gorgon application settings file.");

				if (rootElement.Attribute("Version") != null)
					rootElement.Attribute("Version").Value = Version.ToString();
				else
					rootElement.Add(new XAttribute("Version", Version.ToString()));
			}


			if (!Directory.Exists(Path.GetDirectoryName(_path)))
				Directory.CreateDirectory(Path.GetDirectoryName(_path));
			_xmlSettings.Save(_path);
		}
		
		/// <summary>
		/// Function to load the settings from a file.
		/// </summary>
		/// <remarks>If a <see cref="P:GorgonLibrary.Configuration.GorgonApplicationSettings.Version">Version</see> is specified and the version of the loaded settings file 
		/// is not the same, then the settings are reset to their initial values.</remarks>
		public void Load()
		{
			if (File.Exists(_path))
			{
				_xmlSettings = XDocument.Load(_path);
				if (CheckVersion())
					DeserializeSettings();
				else
					Clear();
			}
			else
				Clear();
		}

		/// <summary>
		/// Function to clear the settings.
		/// </summary>
		public void Clear()
		{
			ResetXML();
			GetProperties();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonApplicationSettings"/> class.
		/// </summary>
		/// <param name="applicationName">Name of the application.</param>
		/// <param name="settingsVersion">The version of the settings file.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="settingsVersion"/> parameter will bypass version checking for the settings.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="applicationName"/> parameter was NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the applicationName parameter is empty.</exception>
		public GorgonApplicationSettings(string applicationName, Version settingsVersion)
		{
			GorgonDebug.AssertParamString(applicationName, "applicationName");

			Version = settingsVersion;

			_path = GorgonComputerInfo.FolderPath(Environment.SpecialFolder.ApplicationData);
			_path += Path.DirectorySeparatorChar.ToString();
			_path += applicationName.RemoveIllegalFilenameChars() + Path.DirectorySeparatorChar.ToString();
			_path += Path.ChangeExtension(GetType().Assembly.GetName().Name.RemoveIllegalFilenameChars(), "config.xml");

			Clear();
		}
		#endregion
	}
}
