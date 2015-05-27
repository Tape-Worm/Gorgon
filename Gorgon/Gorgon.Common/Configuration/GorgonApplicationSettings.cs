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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Gorgon.Core;
using Gorgon.Core.Properties;
using Gorgon.Diagnostics;
using Gorgon.IO;

namespace Gorgon.Configuration
{
	/// <summary>
	/// A base class used to handle settings for an application.
	/// </summary>
	public abstract class GorgonApplicationSettings
	{
		#region Variables.
		private XDocument _xmlSettings;													            // XML document containing application settings.
		private string _path;																		// Path to the XML settings.
		private readonly IDictionary<PropertyInfo, ApplicationSettingAttribute> _properties;		// List of properties to serialize.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the name of the application that the settings are from.
		/// </summary>
		public string ApplicationName
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the path to the configuration file.
		/// </summary>
		public virtual string Path
		{
			get
			{
				return _path;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					return;
				}

				string fileName = System.IO.Path.GetFileName(value);
				string directory = System.IO.Path.GetDirectoryName(value);

				if (string.IsNullOrWhiteSpace(fileName))
				{
					fileName = System.IO.Path.ChangeExtension(GetType().Assembly.GetName().Name.RemoveIllegalFilenameChars(), "config.xml");
				}

				fileName = fileName.FormatFileName();
				directory = directory.FormatDirectory(System.IO.Path.DirectorySeparatorChar);

				_path = directory + fileName;
			}			
		}
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
		    {
		        _xmlSettings = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
		                                     new XElement("ApplicationSettings",
		                                                  new XAttribute("Version", Version.ToString())));
		    }
		    else
		    {
		        _xmlSettings = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
		                                     new XElement("ApplicationSettings"));
		    }
		}

		/// <summary>
		/// Function to convert the specified value into a string.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>The string value for the object.</returns>
		private static string ConvertValue(object value)
		{
		    if (value == null)
		    {
		        return string.Empty;
		    }

		    var type = value.GetType();
			var attrib = type.GetCustomAttributes(typeof(TypeConverterAttribute), true) as IList<TypeConverterAttribute>;
			var convertableValue = value as IConvertible;
			var formattableValue = value as IFormattable;

			if ((convertableValue != null) || (formattableValue != null))
			{
				return convertableValue != null
					? convertableValue.ToString(CultureInfo.InvariantCulture)
					: formattableValue.ToString(null, CultureInfo.InvariantCulture);
			}

			if ((attrib == null) || (attrib.Count <= 0))
			{
				return value.ToString();
			}

			Type converterType = Type.GetType(attrib[0].ConverterTypeName);

			if (converterType == null)
			{
				return value.ToString();
			}

			var converter = Activator.CreateInstance(converterType) as TypeConverter;

			if ((converter != null) && (converter.CanConvertTo(typeof(string))))
			{
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
		private static object UnconvertValue(string value, Type type)
		{
		    if (value == null)
		    {
		        return null;
		    }

		    if (type != typeof (string))
		    {
		        if (string.IsNullOrEmpty(value))
		        {
		            return null;
		        }
		    }
		    else
		    {
		        return value;
		    }

		    if (type.IsEnum)
		    {
			    string[] enumValues = Enum.GetNames(type);
			    int valueIndex = Array.IndexOf(enumValues, value);

			    if (valueIndex == -1)
			    {
				    valueIndex = 0;
			    }

			    return Enum.Parse(type, enumValues[valueIndex]);
		    }

		    var attrib = type.GetCustomAttributes(typeof(TypeConverterAttribute), true) as IList<TypeConverterAttribute>;

			if ((attrib == null) || (attrib.Count <= 0))
			{
				return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
			}

			Type converterType = Type.GetType(attrib[0].ConverterTypeName);

			if (converterType == null)
			{
				return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
			}

			var converter = Activator.CreateInstance(converterType) as TypeConverter;

			if ((converter != null) && (converter.CanConvertFrom(typeof(string))))
			{
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

            // If we haven't gotten the properties yet, cache them now.
		    if (_properties.Count == 0)
		    {
		        GetProperties();
		    }

		    // Get the unique sections.
			var sections = (from property in _properties
							where !string.IsNullOrEmpty(property.Value.Section)
							select property.Value.Section).Distinct();

			// Create section elements.
		    foreach (var section in sections)
		    {
		        AddSection(string.Empty, section);
		    }

		    foreach (KeyValuePair<PropertyInfo, ApplicationSettingAttribute> property in _properties)
			{
				XElement section = GetSectionElement(property.Value.Section);

				if ((property.Key.PropertyType.IsGenericType) || (property.Key.PropertyType == typeof(IList)))
				{
				    if (property.Key.PropertyType.IsGenericType)
					{
						Type[] genericType = property.Key.PropertyType.GetGenericArguments();

						Type valueType = genericType[0];

					    if ((!valueType.IsPrimitive) && (valueType != typeof (string)) && (valueType != typeof (object)))
					    {
					        throw new GorgonException(GorgonResult.CannotEnumerate,
					                                  string.Format(Resources.GOR_PROPERTY_NOT_PRIMITIVE_OR_STRING, property.Key.Name));
					    }
					}

					object collection = property.Key.GetValue(this, null);

					if (collection == null)
					{
						section.Add(new XElement("Setting", new XAttribute(property.Value.SettingName, string.Empty)));
						continue;
					}

					PropertyInfo collectionIndex = collection.GetType().GetProperty("Item");
					PropertyInfo countProperty = collection.GetType().GetProperty("Count");

					int count = Convert.ToInt32(countProperty.GetValue(collection, null));

					for (int i = 0; i < count; i++)
					{
					    section.Add(new XElement("Setting",
					                             new XAttribute(property.Value.SettingName,
					                                            ConvertValue(collectionIndex.GetValue(collection, new object[]
					                                                {
					                                                    i
					                                                })
                                                                )
                                                            )
                                                        )
                                                    );
					}
				}
				else
				{
				    string name = (string.IsNullOrEmpty(property.Value.SettingName) ? property.Key.Name : property.Value.SettingName);
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
					Type valueType;

					if (property.Key.PropertyType.IsGenericType)
					{
						Type[] genericType = property.Key.PropertyType.GetGenericArguments();
						valueType = genericType[0];

						if ((!valueType.IsPrimitive) && (valueType != typeof(string)) && (valueType != typeof(object)))
						{
							throw new GorgonException(GorgonResult.CannotEnumerate,
								string.Format(Resources.GOR_PROPERTY_NOT_PRIMITIVE_OR_STRING, property.Key.Name));
						}
					}
					else
					{
						valueType = typeof(object);
					}

					string settingName = (string.IsNullOrEmpty(property.Value.SettingName) ? property.Key.Name + "_item" : property.Value.SettingName);

					XElement currentSection = GetSectionElement(property.Value.Section);

					if (currentSection == null)
					{
						continue;
					}

					object[] values = (from setting in currentSection.Descendants("Setting")
									   where (setting.Attribute(settingName) != null)
									   select UnconvertValue(setting.Attribute(settingName).Value, valueType)).ToArray();

					// If the list setting doesn't exist, then use a default (if any).
					// If there are no settings in the list, then we can overwrite the default list.
					if (values.Length == 0)
					{
						continue;
					}

					object collection = property.Key.GetValue(this, null);

					// If the collection isn't instanced, then move on.  We will not be creating instances.
					if (collection == null)
					{
						continue;
					}

					MethodInfo addMethod = collection.GetType().GetMethod("Add");
					MethodInfo clearMethod = collection.GetType().GetMethod("Clear");
					
					clearMethod.Invoke(collection, null);
					
					foreach (object item in values)
					{
						addMethod.Invoke(collection, new[]
						                             {
							                             item
						                             });
					}
				}                
				else
				{
					string settingName = (string.IsNullOrEmpty(property.Value.SettingName) ? property.Key.Name : property.Value.SettingName);
					object value = GetSetting(property.Value.Section, settingName, property.Value.PropertyType);

				    if ((value == null) && (property.Value.HasDefault))
				    {
				        value = property.Value.DefaultValue;
				    }

				    // Use application setting if we still don't have a value.
				    if (value != null)
				    {
				        property.Key.SetValue(this, value, null);
				    }
				}
			}
		}

		/// <summary>
		/// Function to retrieve the properties.
		/// </summary>
		private void GetProperties()
		{
			Type settingsType = GetType();

			PropertyInfo[] properties = settingsType.GetProperties();

            _properties.Clear();

			foreach (PropertyInfo info in properties)
			{
				var attributes = (ApplicationSettingAttribute[])info.GetCustomAttributes(typeof(ApplicationSettingAttribute), true);

				if (attributes.Length <= 0)
				{
					continue;
				}

				_properties.Add(info, attributes[0]);
				if (!(info.PropertyType.IsGenericType) && (info.PropertyType != typeof (IList)) && (attributes[0].HasDefault))
				{
					info.SetValue(this, attributes[0].DefaultValue, null);
				}
			}
		}

		/// <summary>
		/// Function to compare version numbers.
		/// </summary>
		/// <returns><c>true</c> if the versions match, <c>false</c> if not.</returns>
		private bool CheckVersion()
		{
			XElement rootElement = _xmlSettings.Element("ApplicationSettings");
			Version compareVersion;

		    if (Version == null)
		    {
		        return true;
		    }

		    if (rootElement == null)
		    {
		        throw new GorgonException(GorgonResult.InvalidFileFormat, 
                                            string.Format(Resources.GOR_SETTING_INVALID_FILE, _path));
		    }

		    // If we don't have a version attribute, then we're not versioning this file.
		    if (rootElement.Attribute("Version") == null)
		    {
		        return true;
		    }

		    if (!Version.TryParse(rootElement.Attribute("Version").Value, out compareVersion))
		    {
                throw new GorgonException(GorgonResult.InvalidFileFormat,
                                            string.Format(Resources.GOR_SETTING_INVALID_FILE, _path),
                                            new InvalidCastException(Resources.GOR_SETTING_CANNOT_CONVERT_VERSION));
            }

		    return compareVersion == Version;
		}

		/// <summary>
		/// Function to return the section element defined by the name.
		/// </summary>
		/// <param name="sectionName">Name of the section to retrieve.</param>
		/// <returns>The element with the section name.</returns>
		private XElement GetSectionElement(string sectionName)
		{
		    if (string.IsNullOrWhiteSpace(sectionName))
		    {
		        return _xmlSettings.Element("ApplicationSettings");
		    }

			return _xmlSettings.Descendants("Section")
			                  .FirstOrDefault(
				                  item => string.Equals(item.Attribute("SectionName").Value, sectionName));
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
		    GorgonDebug.AssertParamString(name, "name");

			XElement currentSection = GetSectionElement(section);
			currentSection.Add(new XElement("Section", 
							new XAttribute("SectionName", name)));
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
		    if (settingName == null)
		    {
			    throw new ArgumentNullException("settingName");
		    }

			if (string.IsNullOrWhiteSpace(settingName))
			{
				throw new ArgumentException(Resources.GOR_PARAMETER_MUST_NOT_BE_EMPTY, "settingName");
			}

			XElement currentSection = GetSectionElement(section);

		    if (currentSection == null)
		    {
		        return null;
		    }

		    var currentSetting = (from settings in currentSection.Descendants("Setting")
								  where ((settings != null) && (settings.Attribute(settingName) != null))
								  select settings).FirstOrDefault();

			return currentSetting == null ? null : UnconvertValue(currentSetting.Attribute(settingName).Value, valueType);
		}

	    /// <summary>
		/// Function to save the settings to a file.
		/// </summary>
		/// <remarks>No versioning will be applied to the settings file when the <see cref="P:GorgonLibrary.Configuration.GorgonApplicationSettings.Version">Version</see> property is NULL (Nothing in VB.Net).</remarks>
		/// <exception cref="GorgonException">Thrown when the file being saved is not of the same format as an Gorgon application setting file.</exception>
		public void Save()
		{
			SerializeSettings();

			if (Version != null)
			{
				XElement rootElement = _xmlSettings.Element("ApplicationSettings");

			    if (rootElement == null)
			    {
                    throw new GorgonException(GorgonResult.InvalidFileFormat, 
                                                string.Format(Resources.GOR_SETTING_INVALID_FILE, _path));
			    }

			    if (rootElement.Attribute("Version") != null)
					rootElement.Attribute("Version").Value = Version.ToString();
				else
					rootElement.Add(new XAttribute("Version", Version.ToString()));
			}

	        string directory = System.IO.Path.GetDirectoryName(_path)
	                                 .FormatDirectory(System.IO.Path.DirectorySeparatorChar);

            // Ensure we have a directory to work with.
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new DirectoryNotFoundException();
            }

	        if (!Directory.Exists(directory))
	        {
	            Directory.CreateDirectory(directory);
	        }

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
		        {
		            DeserializeSettings();
		        }
		        else
		        {
		            Clear();
		        }
		    }
		    else
		    {
		        Clear();
		    }
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
		protected GorgonApplicationSettings(string applicationName, Version settingsVersion)
		{
			GorgonDebug.AssertParamString(applicationName, "applicationName");

            _properties = new Dictionary<PropertyInfo, ApplicationSettingAttribute>();

			ApplicationName = applicationName;
			Version = settingsVersion;

			_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			_path += System.IO.Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
			_path += applicationName.RemoveIllegalFilenameChars() + System.IO.Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
			_path += System.IO.Path.ChangeExtension(GetType().Assembly.GetName().Name.RemoveIllegalFilenameChars(), "config.xml");

			Clear();
		}		
		#endregion
	}
}
