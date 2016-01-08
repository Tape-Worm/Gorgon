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
using Gorgon.Diagnostics;
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
	/// rudimentary and fully optional version control via the <see cref="Version"/> property to help transitioning between application versions.
	/// </para>
	/// <para>
	/// To set up an application with a settings file, the developer should inherit from this class, and provide properties that represent the settings for the application. These properties should be marked with 
	/// a <see cref="GorgonApplicationSettingAttribute"/> so that the base object knows which properties to serialize/deserialize. And that's all there is to it.
	/// </para>
	/// <para>
	/// <note>
	/// <h3>Limitations</h3>
	/// <para>
	/// While this class is flexible, there are some restrictions on the property types:
	/// <list type="bullet">
	/// <item><description>The property must have a property getter, at minimum.</description></item>
	/// <item><description>Be a a primitive type, enum, value type (with a <see cref="TypeConverterAttribute"/>), <see cref="string"/>, <see cref="DateTime"/> or array.</description></item>
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
	/// <item><description>A <see cref="string"/> type.</description></item>
	/// <item><description>A <see cref="DateTime"/> type.</description></item>
	/// </list>
	/// Furthermore, array types on properties are restricted to a single rank.
	/// </para>
	/// <para>
	/// Finally, types of <see cref="Nullable{T}"/> are not supported.
	/// </para>
	/// </note> 
	/// </para>
	/// <para>
	/// <note>
	/// <para>
	/// <h3>Versioning</h3>
	/// If a version has been supplied to the object for comparison against that of the file, then it will check to see if the file version is less than or equal to the given version. 
	/// If the file has a greater version number, then all items in the settings object will be reset to their default values.
	/// </para>
	/// <para>
	/// While you may load a file with new, or deleted properties with no issue, changing the type of the property between versions of a settings file may cause exceptions when loading 
	/// the file, regardless of whether or not it loads an older version of the file.
	/// </para>
	/// </note>
	/// </para>
	/// <para>
	/// <note>
	/// <h3>Arrays</h3>
	/// <para>
	/// Collection types like lists or dictionaries may have a variable number of elements, but array types are handled differently. If you initialize an array setting in your constructor to have 5 elements, then 
	/// the settings object will always assume there will only be 5 elements. If an array item in the XML has an index that exceeds the defined count, then it is ignored.  Likewise, if the XML file does not 
	/// contain an element for a given index, then it will be set to its default value.
	/// </para>
	/// </note>
	/// </para>
	/// <para>
	/// <note>
	/// <h3>Date format</h3>
	/// <para>
	/// This object can (de)serialize a date/time value from/to a string in the backing file. To do this, the application takes the date value and formats it according to the <a href="http://www.iso.org/iso/catalogue_detail?csnumber=40874" target="_blank">ISO 8601</a> 
	/// specification.
	/// </para> 
	/// </note>
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
			}

			/// <summary>
			/// Property to return whether this property has a default value.
			/// </summary>
			public bool HasDefault
			{
				get;
			}

			/// <summary>
			/// Property to return the property section grouping.
			/// </summary>
			public string PropertySection
			{
				get;
			}

			/// <summary>
			/// Property to return the name of the property.
			/// </summary>
			public string PropertyName
			{
				get;
			}

			/// <summary>
			/// Property to return the default value for the property.
			/// </summary>
			public object DefaultValue
			{
				get;
			}
			/// <summary>
			/// Property to return the getter for the property.
			/// </summary>
			public PropertyGetter<GorgonApplicationSettings, object> Getter
			{
				get;
			}

			/// <summary>
			/// Property to return the setter for the property.
			/// </summary>
			public PropertySetter<GorgonApplicationSettings, object> Setter
			{
				get;
			}

			/// <summary>
			/// Property to return whether the property points to an array type or not.
			/// </summary>
			public bool IsArray => PropertyInfo.PropertyType.IsArray;

			/// <summary>
			/// Property to return whether this property holds a list.
			/// </summary>
			public bool IsList
			{
				get;
			}

			/// <summary>
			/// Property to return whether this property holds a dictionary.
			/// </summary>
			public bool IsDictionary
			{
				get;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="PropertyItem"/> class.
			/// </summary>
			/// <param name="property">The property information.</param>
			/// <param name="settingAttr">The <see cref="GorgonApplicationSettingAttribute"/> assigned to the property.</param>
			/// <param name="isDictionary"><c>true</c> if the property type is a dictionary, <c>false</c> if not.</param>
			/// <param name="isList"><c>true</c> if the property type is a list, <c>false</c> if not.</param>
			public PropertyItem(PropertyInfo property, GorgonApplicationSettingAttribute settingAttr, bool isDictionary, bool isList)
			{
				PropertyInfo = property;
				HasDefault = settingAttr.HasDefault;
				PropertySection = settingAttr.Section;
				PropertyName = string.IsNullOrWhiteSpace(settingAttr.SettingName) ? property.Name : settingAttr.SettingName;
				DefaultValue = settingAttr.DefaultValue;

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
		/// Property to set or return the application logging interface to use.
		/// </summary>
		protected IGorgonLog Log
		{
			get;
		}

		/// <summary>
		/// Property to return the name of the application that these settings are used with.
		/// </summary>
		public string ApplicationName
		{
			get;
		}

		/// <summary>
		/// Property to set or return the path to the configuration file.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the path does not contain a file name, then the name of the assembly that the settings type resides in will be used as the file name.
		/// </para>
		/// <para>
		/// If the directory name is not present, then a path of "Users\&lt;Username&gt;\AppData\Roaming\&lt;<see cref="ApplicationName"/>&gt;\" will 
		/// be used.
		/// </para>
		/// <para>
		/// These defaults are the same as the default path that is created when the object is constructed.
		/// </para>
		/// </remarks>
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

				if (string.IsNullOrWhiteSpace(directory))
				{
					directory = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
					             + System.IO.Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)
					             + ApplicationName);
				}

				if (string.IsNullOrWhiteSpace(fileName))
				{
					fileName = System.IO.Path.ChangeExtension(GetType().Assembly.GetName().Name, "config.xml");
				}

				fileName = fileName.FormatFileName();
				directory = directory.FormatDirectory(System.IO.Path.DirectorySeparatorChar);

				_path = directory + fileName;
			}			
		}
		/// <summary>
		/// Property to set or return the application settings version.
		/// </summary>
		/// <remarks>
		/// <para>
		/// When this value is supplied (either in the constructor or directly on this property), that version number will be applied to the XML file generated by this object 
		/// when the settings are persisted. When those settings are read back in from the file, the version supplied to this object will be compared to the version in the file. 
		/// If the file version is less than or equal to the value of this property, then the settings will be loaded. If not, then the default values will be assigned to the 
		/// properties on the object.
		/// </para>
		/// <para>
		/// Assigning <b>null</b> (<i>Nothing</i> in VB.Net) to this property will turn off version checking.
		/// </para>
		/// </remarks>
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
			var convertibleValue = value as IConvertible;
			var formattableValue = value as IFormattable;

			if (value is DateTime)
			{
				return ((DateTime)value).ToString(DateFormat, CultureInfo.InvariantCulture);
			}

			if ((convertibleValue != null) || (formattableValue != null))
			{
				return convertibleValue?.ToString(CultureInfo.InvariantCulture) ?? formattableValue.ToString(null, CultureInfo.InvariantCulture);
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
				return type.IsValueType ? Activator.CreateInstance(type) : null;
		    }

		    if (type != typeof (string))
		    {
		        if (string.IsNullOrEmpty(value))
		        {
			        return type.IsValueType ? Activator.CreateInstance(type) : null;
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

			Log.Print("Persisting {0} items from property '{1}'.", LoggingLevel.Verbose, count, name);

			// Leave if no elements.
			if ((count == 0) || (arrayItem == null))
			{
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

			Log.Print("Persisting {0} items from property '{1}'.", LoggingLevel.Verbose, count, name);

			// Leave if no elements.
			if ((count == 0) || (arrayItem == null))
			{
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
				Log.Print("Creating setting section '{0}'.", LoggingLevel.Verbose, section);
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

				string value = ConvertValue(property.Getter(this));

				Log.Print("Persisting value {0} from property '{1}'.", LoggingLevel.Verbose, value, property.PropertyInfo.Name);
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
			dynamic dictionaryValue = item.Getter(this);

			if (dictionaryValue == null)
			{
				return;
			}

			Type dictionaryType = dictionaryValue.GetType();
			Type valueType = dictionaryType.GetGenericArguments()[1];
			dynamic typedValue = UnconvertValue(value, valueType);

			dictionaryValue[key] = typedValue;
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
					Log.Print("XML has setting element with missing property name attribute.", LoggingLevel.Simple);
					continue;
				}

				PropertyItem property;

				// No property with this name?  Ok, move on. 
				if (!_properties.TryGetValue(nameAttr.Name.LocalName, out property))
				{
					Log.Print("XML contains property name '{0}', but no such property exists on this object.", LoggingLevel.Intermediate, nameAttr.Name.LocalName);
					continue;
				}

				if (property.IsArray)
				{
					// If the array is null, then we won't be adding any elements into it.
					if (property.Getter(this) == null)
					{
						Log.Print("Array property '{0}' is null, cannot assign values.", LoggingLevel.Intermediate, property.PropertyInfo.Name);
						continue;
					}

					if (indexAttr == null)
					{
						throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR_ERR_SETTING_INVALID_FILE, _path), new NullReferenceException());
					}

					int itemIndex = int.Parse(indexAttr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
					Log.Print("Loading value <{0}> into array index {1} for property '{2}'.", LoggingLevel.Verbose, nameAttr.Value, itemIndex, property.PropertyInfo.Name);
					AddItemToArray(property, itemIndex, nameAttr.Value);
					continue;
				}

				if (property.IsList)
				{
					Log.Print("Loading value <{0}> into list for property '{1}'.", LoggingLevel.Verbose, nameAttr.Value, property.PropertyInfo.Name);
					AddItemToList(property, nameAttr.Value);
					continue;
				}

				if (property.IsDictionary)
				{
					if (string.IsNullOrWhiteSpace(dictionaryKeyAttr?.Value))
					{
						throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR_ERR_SETTING_INVALID_FILE, _path), new NullReferenceException());
					}

					Log.Print("Loading value <{0}> into dictionary key {1} for property '{2}'.", LoggingLevel.Verbose, nameAttr.Value, dictionaryKeyAttr.Value, property.PropertyInfo.Name);
					AddItemToDictionary(property, dictionaryKeyAttr.Value, nameAttr.Value);
					continue;
				}

				// Just assign the value.
				if (property.Setter == null)
				{
					Log.Print("Property '{0}' is read-only.", LoggingLevel.Intermediate, property.PropertyInfo.Name);
					continue;
				}

				Log.Print("Assigning value <{0}> to property '{1}'.", LoggingLevel.Verbose, nameAttr.Value, property.PropertyInfo.Name);
				property.Setter(this, UnconvertValue(nameAttr.Value, property.PropertyInfo.PropertyType));
			}
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
					Log.Print("Resetting array [{0}] to default values.", LoggingLevel.Verbose, item.PropertyInfo.Name);
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
					Log.Print("Clearing dictionary/list [{0}].", LoggingLevel.Verbose, item.PropertyInfo.Name);
					dynamic collectionValue = item.Getter(this);

					if (collectionValue != null)
					{
						collectionValue.Clear();
					}

					continue;
				}

				if (item.Setter == null)
				{
					Log.Print("Property [{0}] has no setter, skipping.", LoggingLevel.Verbose, item.PropertyInfo.Name);
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

				Log.Print("Resetting property [{0}] to {1}.", LoggingLevel.Verbose, item.PropertyInfo.Name, value ?? "Null");
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
				Log.Print("Log at '{0}' is unversioned.", LoggingLevel.Verbose, _path);
		        return true;
		    }

		    if (_rootNode == null)
		    {
		        throw new GorgonException(GorgonResult.InvalidFileFormat, 
                                            string.Format(Resources.GOR_ERR_SETTING_INVALID_FILE, _path));
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
                                            string.Format(Resources.GOR_ERR_SETTING_INVALID_FILE, _path),
                                            new InvalidCastException(Resources.GOR_ERR_SETTING_CANNOT_CONVERT_VERSION));
            }

			if (compareVersion > Version)
			{
				Log.Print("Log at '{0}' is version {1}, but we only support version {2}.", LoggingLevel.Simple, _path, compareVersion, Version);	
			}

		    return compareVersion <= Version;
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
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter was <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
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
		/// Function to save the settings in the properties on this object into an XML file.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method will gather the values of the properties marked with the <see cref="GorgonApplicationSettingAttribute"/> and persist them into an XML file at the location provided to the <see cref="Path"/> property.
		/// </para>
		/// <para>
		/// No versioning will be applied to the settings file when the <see cref="Version"/> property is <b>null</b> (<i>Nothing</i> in VB.Net).
		/// </para>
		/// </remarks>
		/// <exception cref="DirectoryNotFoundException">Thrown when the directory pointed at by the <see cref="Path"/> property does not exist.</exception>
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

				if (!_rootNode.HasAttributes)
				{ 
					_rootNode.Add(_versionAttr);
				}
			}

		    if (Version != null)
		    {
			    Log.Print("Saving settings v{1} to '{0}'.", LoggingLevel.Simple, _path, Version);
		    }
		    else
		    {
				Log.Print("Saving settings to '{0}'.", LoggingLevel.Simple, _path);
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
		/// Function to read the settings from an XML file and update the properties on the object with those values.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method will reset the values of the settings object right away. If the settings file could not be loaded, or the version of the file was greater than the requested <see cref="Version"/>, 
		/// then the object will remain at its default values.
		/// </para>
		/// </remarks>
		/// <exception cref="GorgonException">Thrown when the XML file format is not valid.</exception>
		public void Load()
		{
			Reset();

			if (!File.Exists(_path))
			{
				return;
			}

			Log.Print("Loading settings from '{0}'.", LoggingLevel.Simple, _path);

			_xmlSettings = XDocument.Load(_path);
			_rootNode = _xmlSettings.Element(RootNodeName);

			if (_rootNode == null)
			{
				throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR_ERR_SETTING_INVALID_FILE, _path));
			}

			if (CheckVersion())
			{
				DeserializeSettings();
			}
		}

		/// <summary>
		/// Function to reset the settings on this object.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will set the values for each property marked with the <see cref="GorgonApplicationSettingAttribute"/> to the default value for the property type. If the attribute has defined the 
		/// <see cref="GorgonApplicationSettingAttribute.DefaultValue"/> property, then that value is used instead.
		/// </para> 
		/// <para>
		/// For dictionaries and lists, their contents will be cleared if the property value is not <b>null</b> (<i>Nothing</i> in VB.Net). For array types, the array values will be reset to the 
		/// default value for the element type.
		/// </para>
		/// </remarks>
		public void Reset()
		{
			Log.Print("Resetting settings for [{0}]", LoggingLevel.Verbose, GetType().FullName);

			_xmlSettings = XDocument.Parse(Version != null ? Resources.SettingsDocVersion : Resources.SettingsDocNoVersion);
			_rootNode = _xmlSettings.Element(RootNodeName);

			Debug.Assert(_rootNode != null, "The settings XML root node should not be <b>null</b>!");

			_versionAttr = _rootNode.Attribute(VersionAttrName);

			ResetProperties();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonApplicationSettings"/> class.
		/// </summary>
		/// <param name="applicationName">Name of the application.</param>
		/// <param name="settingsVersion">The version of the settings file.</param>
		/// <param name="log">The application logging interface.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="applicationName"/> parameter was <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="applicationName"/> parameter is empty.</exception>
		/// <remarks>
		/// <para>
		/// Upon creation, this object will set its default values to the properties that have the <see cref="GorgonApplicationSettingAttribute"/>. These defaults can easily be overridden in the object 
		/// constructor. If the object contains any collection/array properties, those must be initialized in the constructor. These property types will not receive default values on startup.
		/// </para>
		/// <para>
		/// When this object is created, the <see cref="Path"/> property is filled in with a path that uses the current users roaming directory, it is formatted like this:<br/> 
		/// <c>Users\&lt;Your username&gt;\&lt;<see cref="ApplicationName"/>&gt;\&lt;Name of your assembly&gt;.config.xml</c><br/>
		/// The <c>&lt;Name of your assembly&gt;</c> is the name of the assembly that your settings object is contained within.
		/// </para>
		/// <para>
		/// If you wish to set the default property values after object construction, call the <see cref="Reset"/> method.
		/// </para>
		/// <para>
		/// There may be a slight delay when constructing objects that inherit this type. This is because the expressions used to manipulate the object are being compiled and a fair bit of reflection is 
		/// used to derive which properties are usable against those that are not. This means that the more members you have on your object, the more of a delay on creation.
		/// </para>
		/// <para>
		/// Objects that inherit this class are only required to submit the <paramref name="applicationName"/> parameter. This parameter just identifies which application is using the setting object type. 
		/// The <paramref name="settingsVersion"/> property can be set to <b>null</b> (<i>Nothing</i> in VB.Net) to disable version checking. Likewise, the <paramref name="log"/> parameter may be set to 
		/// <b>null</b> to disable logging for this object.
		/// </para>
		/// </remarks>
		protected GorgonApplicationSettings(string applicationName, Version settingsVersion, IGorgonLog log)
		{
			Log = log ?? GorgonLogDummy.DefaultInstance;

			if (applicationName == null)
			{
				throw new ArgumentNullException(nameof(applicationName));
			}

			if (string.IsNullOrWhiteSpace(applicationName))
			{
				throw new ArgumentException(Resources.GOR_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(applicationName));
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

			_path = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
			         + System.IO.Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)
			         + applicationName + System.IO.Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)
			         + System.IO.Path.ChangeExtension(GetType().Assembly.GetName().Name, "config.xml"))
				.FormatPath(System.IO.Path.DirectorySeparatorChar);

			_xmlSettings = XDocument.Parse(Version != null ? Resources.SettingsDocVersion : Resources.SettingsDocNoVersion);
			_rootNode = _xmlSettings.Element(RootNodeName);

			Debug.Assert(_rootNode != null, "The settings XML root node should not be <b>null</b>!");

			_versionAttr = _rootNode.Attribute(VersionAttrName);

			// This is not typically kosher since you're not supposed to call methods from the constructor like this.
			// But we need to populate the default values automatically, and this is the only way to do it without requiring 
			// the users to do it on their own.
			ResetProperties();
		}		
		#endregion
	}
}
