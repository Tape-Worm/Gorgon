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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Gorgon.Core;
using Gorgon.Core.Properties;
using Gorgon.IO;
using Gorgon.Reflection;

namespace Gorgon.Configuration
{
	/// <summary>
	/// A base class that represents configuration settings for an application.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Many applications require their settings to be persisted to a data store and read back again, and in .NET there are a few ways to persist application settings. The <see cref="GorgonApplicationSettings"/> 
	/// object allows for more customizable functionality than what is present in the standard .NET offerings. For example, it allows the developer to specify where to store the settings on the file system, the 
	/// standard app.config functionality does not (at least, not easily). It is also capable of storing collections with more varied element types (other than just <see cref="string"/>. Finally, it has some 
	/// rudimentary (and fully optional) version control via the <see cref="Version"/> property to help transitioning between application versions.
	/// </para>
	/// <para>
	/// To set up an application with a settings file, the developer should inherit from this class, and provide properties that represent the settings for the application. These properties should be marked with 
	/// a <see cref="GorgonApplicationSettingAttribute"/> so that the base object knows which properties to serialize/deserialize. And that's all there is to it.
	/// </para>
	/// <para>
	/// While this class is flexible, there are some restrictions on the property types.  The property types must have:
	/// <list type="bullet">
	/// <item><description>Must have a getter at minimum.</description></item>
	/// <item><description>Must be a a primitive type, enum, value type (with a <see cref="TypeConverterAttribute"/>), <see cref="String"/>, <see cref="DateTime"/> or array.</description></item>
	/// <item><description>Collection types are supported, and must implement either <see cref="IList{T}"/>, or a <see cref="IDictionary{String,TValue}"/>.</description></item>
	/// </list>
	/// Properties that are collection types must be instantiated in the constructor of the class, or they will be skipped when serializing/deserializing. 
	/// </para>
	/// <para>
	/// Values for collection types (and arrays) must be one of:
	/// <list type="bullet">
	/// <item><description>A primitive type.</description></item>
	/// <item><description>An enum type.</description></item>
	/// <item><description>A value type that has a <see cref="TypeConverterAttribute"/>.</description></item>
	/// <item><description>A <see cref="String"/> type.</description></item>
	/// <item><description>A <see cref="DateTime"/> type.</description></item>
	/// </list>
	/// Furthermore, array types on properties are restricted to a single rank.
	/// </para>
	/// <para>
	/// Finally, types of <see cref="Nullable{T}"/> are not supported.
	/// </para>
	/// <para>
	/// <h2>About date formatting</h2>
	/// This object can (de)serialize a date/time value from/to a string in the backing file. To do this, the application takes the date value and formats it according to the <a href="http://www.iso.org/iso/catalogue_detail?csnumber=40874">ISO 8601</a> 
	/// specification.
	/// </para>
	/// </remarks>
	public abstract class GorgonApplicationSettings
	{
		#region Classes.
		/// <summary>
		/// A property item enumerated from the sub type of this object.
		/// </summary>
		private class PropertyItem
		{
			/// <summary>
			/// Property to return the property reflection information.
			/// </summary>
			public PropertyInfo PropertyInfo
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return whether this property has a default value.
			/// </summary>
			public bool HasDefault
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the property section grouping.
			/// </summary>
			public string PropertySection
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the name of the property.
			/// </summary>
			public string PropertyName
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the default value for the property.
			/// </summary>
			public object DefaultValue
			{
				get;
				private set;
			}
			/// <summary>
			/// Property to return the getter for the property.
			/// </summary>
			public PropertyGetter<GorgonApplicationSettings, object> Getter
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the setter for the property.
			/// </summary>
			public PropertySetter<GorgonApplicationSettings, object> Setter
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return whether the property points to an array type or not.
			/// </summary>
			public bool IsArray
			{
				get
				{
					return PropertyInfo.PropertyType.IsArray;
				}
			}

			/// <summary>
			/// Property to return whether this property holds a list.
			/// </summary>
			public bool IsList
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return whether this property holds a dictionary.
			/// </summary>
			public bool IsDictionary
			{
				get;
				private set;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="PropertyItem"/> class.
			/// </summary>
			/// <param name="property">The property information.</param>
			/// <param name="attr">The <see cref="GorgonApplicationSettingAttribute"/> assigned to the property.</param>
			/// <param name="isDictionary"><c>true</c> if the property type is a dictionary, <c>false</c> if not.</param>
			/// <param name="isList"><c>true</c> if the property type is a list, <c>false</c> if not.</param>
			public PropertyItem(PropertyInfo property, GorgonApplicationSettingAttribute attr, bool isDictionary, bool isList)
			{
				PropertyInfo = property;
				HasDefault = attr.HasDefault;
				PropertySection = attr.Section;
				PropertyName = string.IsNullOrWhiteSpace(attr.SettingName) ? property.Name : attr.SettingName;
				DefaultValue = attr.DefaultValue;

				Getter = property.CreatePropertyGetter<GorgonApplicationSettings, object>();

				if (property.CanWrite)
				{
					Setter = property.CreatePropertySetter<GorgonApplicationSettings, object>();
				}

				IsList = isList;
				IsDictionary = isDictionary;
			}
		}

		#endregion

		#region Constants.
		// The name of the root node for the backing XML document.
		private const string RootNodeName = "ApplicationSettings";
		// The name of the node for an individual setting.
		private const string SectionNodeName = "Section";
		// The name of the attribute that holds the name of the section.
		private const string SectionNameAttr = "SectionName";
		// The name of the node for an individual setting.
		private const string SettingNodeName = "Setting";
		// The name of the attribute that holds the key for a dictionary item.
		private const string SettingKeyAttr = "_Dict_Key_";
		// The name of the attribute that holds the array index for an array item.
		private const string SettingIndexAttr = "_Arr_Index_";
		// The name of the attribute used to store version info.
		private const string VersionAttrName = "Version";
		// Date/time format when serializing/deserializing (using ISO 8601 format).
		private const string DateFormat = "yyyy-MM-ddTHH:mm:ss.fffffffzzz";
		#endregion

		#region Variables.
		// Supported dictionary type.
		private readonly Type _dictionaryType = typeof(IDictionary<,>);
		// Supported list type.
		private readonly Type _listType = typeof(IList<>);
		// XML document containing application settings.
		private XDocument _xmlSettings;
		// Path to the XML settings.
		private string _path;
		// List of properties to serialize.
		private readonly IDictionary<string, PropertyItem> _properties;
		// Constructors for type converters.
		private readonly IDictionary<string, ObjectActivator<TypeConverter>> _typeConverterCtors = new Dictionary<string, ObjectActivator<TypeConverter>>(StringComparer.OrdinalIgnoreCase);
		// The root node for the XML document backing store.
		private XElement _rootNode;
		// The version attribute for the XML document.
		private XAttribute _versionAttr;
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
		public string Path
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
		/// <remarks>Assigning NULL (<i>Nothing</i> in VB.Net) will bypass version checking.</remarks>
		public Version Version
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert the specified value into a string.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>The string value for the object.</returns>
		private string ConvertValue(object value)
		{
		    if (value == null)
		    {
		        return string.Empty;
		    }

		    var type = value.GetType();
			var attrib = type.GetCustomAttribute<TypeConverterAttribute>(true);
			var convertableValue = value as IConvertible;
			var formattableValue = value as IFormattable;

			if (value is DateTime)
			{
				return ((DateTime)value).ToString(DateFormat, CultureInfo.InvariantCulture);
			}

			if ((convertableValue != null) || (formattableValue != null))
			{
				return convertableValue != null
					? convertableValue.ToString(CultureInfo.InvariantCulture)
					: formattableValue.ToString(null, CultureInfo.InvariantCulture);
			}

			if (attrib == null)
			{
				return value.ToString();
			}

			Type converterType = Type.GetType(attrib.ConverterTypeName);

			if (converterType == null)
			{
				return value.ToString();
			}

			ObjectActivator<TypeConverter> ctor;

			if (!_typeConverterCtors.TryGetValue(converterType.FullName, out ctor))
			{
				ctor = converterType.CreateActivator<TypeConverter>();
				_typeConverterCtors.Add(converterType.FullName, ctor);
			}
			
			var converter = ctor();

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
		private object UnconvertValue(string value, Type type)
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

			if (type == typeof(DateTime))
			{
				return DateTime.ParseExact(value, DateFormat, CultureInfo.InvariantCulture);
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

			var attrib = type.GetCustomAttribute<TypeConverterAttribute>(true);

			if (attrib == null)
			{
				return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
			}

			Type converterType = Type.GetType(attrib.ConverterTypeName);

			if (converterType == null)
			{
				return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
			}

			ObjectActivator<TypeConverter> ctor;

			if (!_typeConverterCtors.TryGetValue(converterType.FullName, out ctor))
			{
				ctor = converterType.CreateActivator<TypeConverter>();
				_typeConverterCtors.Add(converterType.FullName, ctor);
			}

			var converter = ctor();

			if ((converter != null) && (converter.CanConvertFrom(typeof(string))))
			{
				return converter.ConvertFromInvariantString(value);
			}

			return Convert.ChangeType(value, type);
		}

		/// <summary>
		/// Function to serialize a list of array values.
		/// </summary>
		/// <param name="name">The name or alias of the property.</param>
		/// <param name="isList"><c>true</c> if this is a list type, or <c>false</c> if this is an array type.</param>
		/// <param name="arrayItem">The item to serialize.</param>
		/// <param name="section">The section that owns the setting.</param>
		private void SerializeArrayOrList(string name, bool isList, dynamic arrayItem, XElement section)
		{
			int count = arrayItem != null ? ((isList) ? arrayItem.Count : arrayItem.Length) : 0;

			// Leave if no elements.
			if ((count == 0) || (arrayItem == null))
			{
				section.Add(new XElement(SettingNodeName, new XAttribute(XName.Get(name), string.Empty)));
				return;
			}

			for (int i = 0; i < count; ++i)
			{
				XElement settingNode = new XElement(SettingNodeName, new XAttribute(XName.Get(name), ConvertValue(arrayItem[i])));

				if (!isList)
				{
					settingNode.Add(new XAttribute(SettingIndexAttr, i));
				}

				section.Add(settingNode);
			}
		}

		/// <summary>
		/// Function to serialize a list of array values.
		/// </summary>
		/// <param name="name">The name or alias of the property.</param>
		/// <param name="arrayItem">The item to serialize.</param>
		/// <param name="section">The section that owns the setting.</param>
		private void SerializeDictionary(string name, dynamic arrayItem, XElement section)
		{
			int count = arrayItem != null ? arrayItem.Count : 0;

			// Leave if no elements.
			if ((count == 0) || (arrayItem == null))
			{
				section.Add(new XElement(SettingNodeName, new XAttribute(XName.Get(name), string.Empty)));
				return;
			}

			foreach(dynamic item in arrayItem)
			{
				section.Add(new XElement(SettingNodeName,
				                    new XAttribute(SettingKeyAttr, item.Key),
				                    new XAttribute(XName.Get(name), ConvertValue(item.Value))));
			}
		}

		/// <summary>
		/// Function to serialize the property settings.
		/// </summary>
		private void SerializeSettings()
		{
			_rootNode.RemoveAll();

		    // Get the unique sections.
			var sections = (from property in _properties
							where !string.IsNullOrEmpty(property.Value.PropertySection)
							select property.Value.PropertySection).Distinct();

			// Create section elements.
		    foreach (var section in sections)
		    {
		        AddSection(string.Empty, section);
		    }

		    foreach (PropertyItem property in _properties.Select(item => item.Value))
			{
				XElement section = GetSectionElement(property.PropertySection);

				// Serialize array or list types.
				if ((property.IsArray) || (property.IsList))
				{
					SerializeArrayOrList(property.PropertyName, property.IsList, property.Getter(this), section);
					continue;
				}

				// Serialize the dictionary type.
				if (property.IsDictionary)
				{
					SerializeDictionary(property.PropertyName, property.Getter(this), section);
					continue;
				}

				section.Add(new XElement(SettingNodeName, new XAttribute(XName.Get(property.PropertyName), ConvertValue(property.Getter(this)))));
			}
		}

		/// <summary>
		/// Function to add a deserialized item into a list on the settings object.
		/// </summary>
		/// <param name="item">Property item to evaluate.</param>
		/// <param name="value">Value to deserialize and assign.</param>
		private void AddItemToList(PropertyItem item, string value)
		{
			dynamic listValue = item.Getter(this);

			if (listValue == null)
			{
				return;
			}

			Type listType = listValue.GetType();
			Type valueType = listType.GetGenericArguments()[0];
			dynamic typedValue = UnconvertValue(value, valueType);

			listValue.Add(typedValue);
		}

		/// <summary>
		/// Function to add a deserialized item into a dictionary on the settings object.
		/// </summary>
		/// <param name="item">Property item to evaluate.</param>
		/// <param name="key">The key name to use.</param>
		/// <param name="value">Value to deserialize and assign.</param>
		private void AddItemToDictionary(PropertyItem item, string key, string value)
		{
			dynamic dictonaryValue = item.Getter(this);

			if (dictonaryValue == null)
			{
				return;
			}

			Type dictionaryType = dictonaryValue.GetType();
			Type valueType = dictionaryType.GetGenericArguments()[1];
			dynamic typedValue = UnconvertValue(value, valueType);

			dictonaryValue[key] = typedValue;
		}

		/// <summary>
		/// Function to add an item to an array at the specified index.
		/// </summary>
		/// <param name="item">Property to update.</param>
		/// <param name="itemIndex">Index to place the item into</param>
		/// <param name="value">Value to assign.</param>
		private void AddItemToArray(PropertyItem item, int itemIndex, string value)
		{
			dynamic arrayValue = item.Getter(this);

			if ((arrayValue == null) || (itemIndex < 0) || (itemIndex >= arrayValue.Length))
			{
				return;
			}

			Type elementType = item.PropertyInfo.PropertyType.GetElementType();
			dynamic typedValue = UnconvertValue(value, elementType);

			arrayValue[itemIndex] = typedValue;
		}

		/// <summary>
		/// Function to deserialize the XML data into the properties used for the settings.
		/// </summary>
		private void DeserializeSettings()
		{
			IEnumerable<XElement> settings = _rootNode.Descendants(SettingNodeName).Where(item => item.HasAttributes);

			foreach (XElement element in settings)
			{
				XAttribute indexAttr = element.Attribute(SettingIndexAttr);
				XAttribute dictionaryKeyAttr = element.Attribute(SettingKeyAttr);
				XAttribute nameAttr = element.Attributes().FirstOrDefault(item => !string.Equals(item.Name.LocalName, SettingKeyAttr, StringComparison.Ordinal));

				// We cannot get a name attribute, so we can't process this node.
				if (nameAttr == null)
				{
					continue;
				}

				PropertyItem property;

				// No property with this name?  Ok, move on. 
				// TODO: Implement logging so we can watch for this stuff.
				if (!_properties.TryGetValue(nameAttr.Name.LocalName, out property))
				{
					continue;
				}

				if (property.IsArray)
				{
					if (indexAttr == null)
					{
						throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR_SETTING_INVALID_FILE, _path), new NullReferenceException());
					}

					int itemIndex = int.Parse(indexAttr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
					
					AddItemToArray(property, itemIndex, nameAttr.Value);
					continue;
				}

				if (property.IsList)
				{
					AddItemToList(property, nameAttr.Value);
					continue;
				}

				if (property.IsDictionary)
				{
					if ((dictionaryKeyAttr == null) || (string.IsNullOrWhiteSpace(dictionaryKeyAttr.Value)))
					{
						throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR_SETTING_INVALID_FILE, _path), new NullReferenceException());
					}

					AddItemToDictionary(property, dictionaryKeyAttr.Value, nameAttr.Value);
					continue;
				}

				// Just assign the value.
				if (property.Setter != null)
				{
					property.Setter(this, UnconvertValue(nameAttr.Value, property.PropertyInfo.PropertyType));
				}
			}

/*
			foreach (PropertyItem property in _properties)
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
			}*/
		}

		/// <summary>
		/// Function to reset the properties.
		/// </summary>
		private void ResetProperties()
		{
			foreach (PropertyItem item in _properties.Select(item => item.Value))
			{
				// Handle special types.
				if (item.IsArray)
				{
					// Reset the array.
					Array arrayValue = item.Getter(this) as Array;

					if (arrayValue != null)
					{
						Array.Clear(arrayValue, 0, arrayValue.Length);
					}
					continue;
				}

				if ((item.IsList) || (item.IsDictionary))
				{
					dynamic collectionValue = item.Getter(this);

					if (collectionValue != null)
					{
						collectionValue.Clear();
					}

					continue;
				}

				if (item.Setter == null)
				{
					continue;
				}

				// For any property that has a setting, just create a new instance of the property type.
				// We won't be caching this with an expression tree because it may result in a very large cache.
				// So we'll take the hit of boxing and reflection.
				object value = item.HasDefault ? item.DefaultValue : null;

				// For value types, use the default constructor and create a new instance.
				if ((item.PropertyInfo.PropertyType.IsValueType) && (!item.HasDefault))
				{
					value = Activator.CreateInstance(item.PropertyInfo.PropertyType);
				}
				
				item.Setter(this, value);
			}
		}

		/// <summary>
		/// Function to compare version numbers.
		/// </summary>
		/// <returns><b>true</b> if the versions match, <b>false</b> if not.</returns>
		private bool CheckVersion()
		{
			Version compareVersion;

		    if (Version == null)
		    {
		        return true;
		    }

		    if (_rootNode == null)
		    {
		        throw new GorgonException(GorgonResult.InvalidFileFormat, 
                                            string.Format(Resources.GOR_SETTING_INVALID_FILE, _path));
		    }

		    // If we don't have a version attribute, then we're not versioning this file.
			_versionAttr = _rootNode.Attribute(VersionAttrName);
		    if (_versionAttr == null)
		    {
		        return true;
		    }

		    if (!Version.TryParse(_versionAttr.Value, out compareVersion))
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
		        return _rootNode;
		    }

			return _xmlSettings.Descendants(SectionNodeName)
			                  .FirstOrDefault(
				                  item => string.Equals(item.Attribute(SectionNameAttr).Value, sectionName, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Function to add a section to the configuration.
		/// </summary>
		/// <param name="section">Section that will contain the new section.</param>
		/// <param name="name">Name of the section.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter was NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the applicationName parameter is empty or the <paramref name="section"/> does not exist.</exception>
		private void AddSection(string section, string name)
		{
			XElement currentSection = GetSectionElement(section);
			currentSection.Add(new XElement(SectionNodeName, 
							new XAttribute(SectionNameAttr, name)));
		}

		/// <summary>
		/// Function to determine if an array is suitable as a setting property.
		/// </summary>
		/// <param name="type">Type of array.</param>
		/// <returns><c>true</c> if supported, <c>false</c> if not.</returns>
		private static bool IsSupportedArrayType(Type type)
		{
			if ((!type.IsArray)
				|| (type.GetArrayRank() != 1))
			{
				return false;
			}

			Type elementType = type.GetElementType();

			return IsSupportedType(elementType);
		}

		/// <summary>
		/// Function to determine if the specified type is supported or not.
		/// </summary>
		/// <param name="type">Type to evaluate.</param>
		/// <returns><c>true</c> if supported, <c>false</c> if not</returns>
		private static bool IsSupportedType(Type type)
		{
			return (!type.IsConstructedGenericType && !type.IsGenericType && !type.IsGenericTypeDefinition)
			       && (type.IsPrimitive
			           || type.IsEnum
			           || (type.IsValueType && type.GetCustomAttribute<TypeConverterAttribute>(true) != null)
			           || type == typeof(string)
			           || type == typeof(DateTime));
		}

		/// <summary>
		/// Function to determine if a generic list type is supported or not.
		/// </summary>
		/// <param name="type">Type of generic list.</param>
		/// <returns><c>true</c> if supported, <c>false</c> if not.</returns>
		private bool IsSupportedListType(Type type)
		{
			if ((!type.IsGenericType) || (type.ContainsGenericParameters) || (type.IsArray))
			{
				return false;
			}

			// If we have a generic list, then check its generic type parameter.
			if ((type.GetGenericTypeDefinition() != _listType)
				|| (type.GenericTypeArguments.Length == 0))
			{
				return false;
			}

			return IsSupportedType(type.GenericTypeArguments[0]);
		}

		/// <summary>
		/// Function to determine if a generic dictionary type is supported or not.
		/// </summary>
		/// <param name="type">Type of generic dictionary.</param>
		/// <returns><c>true</c> if supported, <c>false</c> if not.</returns>
		private bool IsSupportedDictionaryType(Type type)
		{
			if ((!type.IsGenericType) || (type.ContainsGenericParameters))
			{
				return false;
			}

			if ((type.GetGenericTypeDefinition() != _dictionaryType)
				|| (type.GenericTypeArguments.Length == 0))
			{
				return false;
			}

			// Check the types on the dictionary.
			return type.GenericTypeArguments[0] == typeof(string) && IsSupportedType(type.GenericTypeArguments[1]);
		}

	    /// <summary>
		/// Function to save the settings to a file.
		/// </summary>
		/// <remarks>No versioning will be applied to the settings file when the <see cref="P:GorgonLibrary.Configuration.GorgonApplicationSettings.Version">Version</see> property is NULL (<i>Nothing</i> in VB.Net).</remarks>
		/// <exception cref="GorgonException">Thrown when the file being saved is not of the same format as an Gorgon application setting file.</exception>
		public void Save()
		{
			SerializeSettings();

			if (Version != null)
			{
				if (_versionAttr != null)
				{
					_versionAttr.Value = Version.ToString();
				}
				else
				{
					_versionAttr = new XAttribute(XName.Get(VersionAttrName), Version.ToString());
				}

				_rootNode.Add(_versionAttr);
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
		/// <remarks>
		/// <para>
		/// This method will reset the values of the settings object right away. If the settings file could not be loaded, or there was a version mismatch (if <see cref="Version"/> was supplied), 
		/// then the object will remain at its default values.
		/// </para>
		/// </remarks>
		public void Load()
		{
			Reset();

			if (!File.Exists(_path))
			{
				return;
			}

			_xmlSettings = XDocument.Load(_path);
			_rootNode = _xmlSettings.Element(RootNodeName);

			if (_rootNode == null)
			{
				throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR_SETTING_INVALID_FILE, _path));
			}

			if (CheckVersion())
			{
				DeserializeSettings();
			}
		}

		/// <summary>
		/// Function to reset the settings on this object.
		/// </summary>
		public void Reset()
		{
			_xmlSettings = XDocument.Parse(Version != null ? Resources.SettingsDocVersion : Resources.SettingsDocNoVersion);
			_rootNode = _xmlSettings.Element(RootNodeName);

			Debug.Assert(_rootNode != null, "The settings XML root node should not be NULL!");

			_versionAttr = _rootNode.Attribute(VersionAttrName);

			ResetProperties();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonApplicationSettings"/> class.
		/// </summary>
		/// <param name="applicationName">Name of the application.</param>
		/// <param name="settingsVersion">[Optional] The version of the settings file.</param>
		/// <remarks>Passing NULL (<i>Nothing</i> in VB.Net) to the <paramref name="settingsVersion"/> parameter will bypass version checking for the settings.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="applicationName"/> parameter was NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the applicationName parameter is empty.</exception>
		protected GorgonApplicationSettings(string applicationName, Version settingsVersion = null)
		{
			if (applicationName == null)
			{
				throw new ArgumentNullException("applicationName");
			}

			if (string.IsNullOrWhiteSpace(applicationName))
			{
				throw new ArgumentException(Resources.GOR_PARAMETER_MUST_NOT_BE_EMPTY, "applicationName");
			}
			
			// Get the properties for this interface.
			_properties = (from propertyInfo in GetType().GetProperties()
						   let attribute = propertyInfo.GetCustomAttribute<GorgonApplicationSettingAttribute>(true) 
						   let isList = IsSupportedListType(propertyInfo.PropertyType)
						   let isDictionary = IsSupportedDictionaryType(propertyInfo.PropertyType)
						   where attribute != null && propertyInfo.CanRead && (isList
																			   || isDictionary
																			   || IsSupportedType(propertyInfo.PropertyType)
																			   || IsSupportedArrayType(propertyInfo.PropertyType))
						   select new PropertyItem(propertyInfo, attribute, isDictionary, isList))
				.ToDictionary(key => key.PropertyName, value => value, StringComparer.Ordinal);

			ApplicationName = applicationName;
			Version = settingsVersion;

			_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			_path += System.IO.Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
			_path += applicationName.RemoveIllegalFilenameChars() + System.IO.Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
			_path += System.IO.Path.ChangeExtension(GetType().Assembly.GetName().Name.RemoveIllegalFilenameChars(), "config.xml");

			_xmlSettings = XDocument.Parse(Version != null ? Resources.SettingsDocVersion : Resources.SettingsDocNoVersion);
			_rootNode = _xmlSettings.Element(RootNodeName);

			Debug.Assert(_rootNode != null, "The settings XML root node should not be NULL!");

			_versionAttr = _rootNode.Attribute(VersionAttrName);

			// This is not typically kosher since you're not supposed to call methods from the constructor like this.
			// But we need to populate the default values automatically, and this is the only way to do it.
			ResetProperties();
		}		
		#endregion
	}
}
