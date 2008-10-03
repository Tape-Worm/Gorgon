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
// Created: Saturday, June 16, 2007 1:07:17 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using Flobbster.Windows.Forms;

namespace GorgonLibrary.Graphics.Tools.PropBag
{
	/// <summary>
	/// Static object representing a class property enumerator.
	/// </summary>
	public static class PropertyEnumerator
	{
		#region Value types.
		/// <summary>
		/// Value type containing information about the property or field.
		/// </summary>
		public struct FieldPropertyData
		{
			#region Variables.
			private PropertyInfo _propertyInfo;					// Property information.
			private FieldInfo _fieldInfo;						// Field information.
			private string _name;								// Property name.
			private string _description;						// Property description.
			private string _category;							// Property category.
			private PropertyDefaultAttribute _default;			// Property default value.
			private Type _valueType;							// Value type.
			private bool _readOnly;								// Property is read-only.
			private string _editorType;							// Editor type.
			private string _converterType;						// Converter type.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the property information.
			/// </summary>
			public PropertyInfo PropertyInfo
			{
				get
				{
					return _propertyInfo;
				}
			}

			/// <summary>
			/// Property to return the name of the property.
			/// </summary>
			public string Name
			{
				get
				{
					return _name;
				}
			}

			/// <summary>
			/// Property to return the description of the property.
			/// </summary>
			public string Description
			{
				get
				{
					return _description;
				}
			}

			/// <summary>
			/// Property to return the category for the property.
			/// </summary>
			public string Category
			{
				get
				{
					if (string.IsNullOrEmpty(_category))
						return "Misc.";

					return _category;
				}
			}

			/// <summary>
			/// Property to return the default value for the property.
			/// </summary>
			public PropertyDefaultAttribute DefaultValue
			{
				get
				{
					return _default;
				}
			}

			/// <summary>
			/// Property to return the value of the property value.
			/// </summary>
			public Type ValueType
			{
				get
				{
					return _valueType;
				}
			}

			/// <summary>
			/// Property to return whether the property is read-only or not.
			/// </summary>
			public bool ReadOnly
			{
				get
				{
					return _readOnly;
				}
			}

			/// <summary>
			/// Property to return the editor type.
			/// </summary>
			public string EditorType
			{
				get
				{
					return _editorType;
				}
			}

			/// <summary>
			/// Property to return the converter type.
			/// </summary>
			public string ConverterType
			{
				get
				{
					return _converterType;
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to retrieve the attributes for the property.
			/// </summary>
			public void Update()
			{
				Attribute[] descAttributes = null;		// Description attributes.
				Attribute[] categoryAttributes = null;	// Category attributes.
				Attribute[] defaultAttributes = null;	// Default attributes.

				// Set to empty values.
				_name = string.Empty;
				_description = string.Empty;
				_category = string.Empty;
				_default = null;				


				if (_propertyInfo != null)
				{
					_name = _propertyInfo.Name;

					// Get attributes.
					descAttributes = (Attribute[])_propertyInfo.GetCustomAttributes(typeof(PropertyDescriptionAttribute), true);
					categoryAttributes = (Attribute[])_propertyInfo.GetCustomAttributes(typeof(PropertyCategoryAttribute), true);
					defaultAttributes = (Attribute[])_propertyInfo.GetCustomAttributes(typeof(PropertyDefaultAttribute), true);
					if (!_propertyInfo.CanWrite)
						_readOnly = true;

					if (_valueType == null)
						_valueType = _propertyInfo.PropertyType;

				}

				if (_fieldInfo != null)
				{
					_name = _fieldInfo.Name;

					// Get attributes.
					descAttributes = (Attribute[])_fieldInfo.GetCustomAttributes(typeof(PropertyDescriptionAttribute), true);
					categoryAttributes = (Attribute[])_fieldInfo.GetCustomAttributes(typeof(PropertyCategoryAttribute), true);
					defaultAttributes = (Attribute[])_fieldInfo.GetCustomAttributes(typeof(PropertyDefaultAttribute), true);
					if (_fieldInfo.IsInitOnly)
						_readOnly = true;

					if (_valueType == null)
						_valueType = _fieldInfo.FieldType;
				}

				if ((descAttributes != null) && (descAttributes.Length != 0))
					_description = ((PropertyDescriptionAttribute)descAttributes[0]).Description;

				if ((categoryAttributes != null) && (categoryAttributes.Length != 0))
					_category = ((PropertyCategoryAttribute)categoryAttributes[0]).Category;

				if ((defaultAttributes != null) && (defaultAttributes.Length != 0))
					_default = ((PropertyDefaultAttribute)defaultAttributes[0]);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="propertyInfo">Information for the property.</param>
			/// <param name="fieldInfo">Information for the field.</param>
			/// <param name="readOnly">TRUE to mark as read-only, FALSE to leave.</param>
			/// <param name="propertyType">Type of the property.</param>
			/// <param name="editorType">Editor type name.</param>
			/// <param name="converterType">Converter type name.</param>
			public FieldPropertyData(FieldInfo fieldInfo, PropertyInfo propertyInfo, bool readOnly, Type propertyType, string editorType, string converterType)
			{
				if ((fieldInfo == null) && (propertyInfo == null))
					throw new ArgumentNullException("fieldInfo and propertyInfo");

				_fieldInfo = fieldInfo;
				_propertyInfo = propertyInfo;

				// Set to empty values.
				_name = string.Empty;
				_description = string.Empty;
				_category = string.Empty;
				_default = null;
				_valueType = propertyType;
				_readOnly = readOnly;
				_editorType = editorType;
				_converterType = converterType;

				Update();
			}
			#endregion
		}
		#endregion

		#region Variables.
		private static List<FieldPropertyData> _data = null;		// Field & property data.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to get the list of properties that are included for the property bag.
		/// </summary>
		private static void GetProperties(Type objectType)
		{
			Attribute[] includeAttributes = null;	// Inclusion attributes.
			PropertyInfo[] properties = null;		// List of properties for the object.

			properties = objectType.GetProperties();
			
			// Enumerate the properties.
			foreach (PropertyInfo property in properties)
			{
				includeAttributes = (Attribute[])property.GetCustomAttributes(typeof(PropertyIncludeAttribute), true);				

				// Add included property.
				if ((includeAttributes != null) && (includeAttributes.Length != 0))
					_data.Add(new FieldPropertyData(null, property, ((PropertyIncludeAttribute)includeAttributes[0]).ReadOnly, ((PropertyIncludeAttribute)includeAttributes[0]).PropertyType,
						((PropertyIncludeAttribute)includeAttributes[0]).EditorType, ((PropertyIncludeAttribute)includeAttributes[0]).ConverterType));
			}
		}

		/// <summary>
		/// Function to get the list of fields that are included for the property bag.
		/// </summary>
		/// <param name="objectType">Object type.</param>
		private static void GetFields(Type objectType)
		{
			Attribute[] includeAttributes = null;	// Inclusion attributes.
			FieldInfo[] fields = null;				// List of fields for the object.

			fields = objectType.GetFields();

			// Enumerate the properties.
			foreach (FieldInfo field in fields)
			{
				includeAttributes = (Attribute[])field.GetCustomAttributes(typeof(PropertyIncludeAttribute), true);

				// Add included property.
				if ((includeAttributes != null) && (includeAttributes.Length != 0))
					_data.Add(new FieldPropertyData(field, null, ((PropertyIncludeAttribute)includeAttributes[0]).ReadOnly, ((PropertyIncludeAttribute)includeAttributes[0]).PropertyType,
						((PropertyIncludeAttribute)includeAttributes[0]).EditorType, ((PropertyIncludeAttribute)includeAttributes[0]).ConverterType));
			}
		}

		/// <summary>
		/// Function to create a property bag.
		/// </summary>
		/// <param name="objectSource">Object to create a property bag from.</param>
		/// <returns>A new property bag.</returns>
		public static PropertyBag CreatePropertyBag(IPropertyBagObject objectSource)
		{
			PropertyBag bag = null;					// Property bag.
			Type objectType = null;					// Object type.

			try
			{
				if (objectSource == null)
					throw new ArgumentNullException("objectSource");
								
				_data = new List<FieldPropertyData>();
				bag = new PropertyBag();
				objectType = objectSource.GetType();

				// Get the included properties and fields.
				GetProperties(objectType);
				GetFields(objectType);

				// Return nothing if there are no properties or fields.
				if (_data.Count == 0)
					return bag;

				// Set events for the bag.
				bag.GetValue += new PropertySpecEventHandler(objectSource.GetValue);
				bag.SetValue += new PropertySpecEventHandler(objectSource.SetValue);

				// Add properties.
				foreach (FieldPropertyData data in _data)
				{
					bag.Properties.Add(new PropertySpec(data.Name, data.ValueType, data.Category, data.Description, data.DefaultValue == null ? null : data.DefaultValue.DefaultValue, data.EditorType, data.ConverterType));
					if (data.ReadOnly)
						bag.Properties[bag.Properties.Count - 1].Attributes = new Attribute[] { ReadOnlyAttribute.Yes };					
				}

				_data.Clear();
				_data = null;
				return bag;
			}
			catch (Exception ex)
			{
				throw new PropertyBagException("Unable to create the property bag for the object '" + objectType.Name + "'.", ex);
			}
		}

		/// <summary>
		/// Function to create a property table.
		/// </summary>
		/// <param name="objectSource">Object to create a property table from.</param>
		/// <returns>A new property table.</returns>
		public static PropertyTable CreatePropertyTable(IPropertyTableObject objectSource)
		{
			PropertyTable table = null;				// Property table.
			Type objectType = null;					// Object type.

			try
			{
				if (objectSource == null)
					throw new ArgumentNullException("objectSource");

				_data = new List<FieldPropertyData>();
				table = new PropertyTable();
				objectType = objectSource.GetType();

				// Get the included properties and fields.
				GetProperties(objectType);
				GetFields(objectType);

				// Return nothing if there are no properties or fields.
				if (_data.Count == 0)
					return table;

				// Set events for the bag.
				table.GetValue += new PropertySpecEventHandler(objectSource.GetValue);
				table.SetValue += new PropertySpecEventHandler(objectSource.SetValue);

				// Add properties.
				foreach (FieldPropertyData data in _data)
					table.Properties.Add(new PropertySpec(data.Name, data.ValueType, data.Category, data.Description, data.DefaultValue.DefaultValue));

				_data.Clear();
				_data = null;
				return table;
			}
			catch (Exception ex)
			{
				throw new PropertyBagException("Unable to create the property table for the object '" + objectType.Name + "'.", ex);
			}
		}
		#endregion
	}
}
