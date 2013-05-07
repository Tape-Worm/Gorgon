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
// Created: Monday, May 14, 2012 3:53:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A property for content.
	/// </summary>
	public class ContentProperty
		: GorgonNamedObject
	{
		#region Variables.
		private ContentObject _owner = null;
		private PropertyDescriptor _descriptor = null;
		private string _editorBase = string.Empty;
		private object _defaultValue = null;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type returned/set by the property.
		/// </summary>
		public Type PropertyType
		{
			get
			{
				return _descriptor.PropertyType;
			}
		}

		/// <summary>
		/// Property to set or return whether the property is read only.
		/// </summary>
		public bool IsReadOnly
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether a property has a default value or not.
		/// </summary>
		public bool HasDefaultValue
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the default value for the property.
		/// </summary>
		public object DefaultValue
		{
			get
			{
				return _defaultValue;
			}
			set
			{
				if (_defaultValue != value)
				{
					_defaultValue = value;
					HasDefaultValue = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the fully qualified type name for the type converter.
		/// </summary>
		public string Converter
		{
			get;
			set;
		}
		
		/// <summary>
		/// Property to set or return the fully qualified type name for the property editor.
		/// </summary>
		public string Editor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the category for the property.
		/// </summary>
		public string Category
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the description for the property.
		/// </summary>
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the display name for the property.
		/// </summary>
		public string DisplayName
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether a change in this property will refresh all properties.
		/// </summary>
		public RefreshProperties RefreshProperties
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to hide this property from the property grid.
		/// </summary>
		public bool HideProperty
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve any attributes from this property.
		/// </summary>
		private void GetAttributes()
		{
			var Attributes = _descriptor.Attributes.Cast<Attribute>();

			var defaultValue = (from attribute in Attributes
							   where attribute is DefaultValueAttribute
							   select attribute as DefaultValueAttribute).SingleOrDefault();

			var readOnly = (from attribute in Attributes
							where attribute is ReadOnlyAttribute
							select attribute as ReadOnlyAttribute).SingleOrDefault();

			var refresh = (from attribute in Attributes
							where attribute is RefreshPropertiesAttribute
							select attribute as RefreshPropertiesAttribute).SingleOrDefault();

			var description = (from attribute in Attributes
							where attribute is DescriptionAttribute
							select attribute as DescriptionAttribute).SingleOrDefault();

			var category = (from attribute in Attributes
							   where attribute is CategoryAttribute
							   select attribute as CategoryAttribute).SingleOrDefault();

			var editor = (from attribute in Attributes
							   where attribute is EditorAttribute
							   select attribute as EditorAttribute).SingleOrDefault();

			var typeconverter = (from attribute in Attributes
								 where attribute is TypeConverterAttribute
								select attribute as TypeConverterAttribute).SingleOrDefault();

			var displayname = (from attribute in Attributes
								 where attribute is DisplayNameAttribute
							   select attribute as DisplayNameAttribute).SingleOrDefault();
			
			if (defaultValue != null)
				DefaultValue = defaultValue.Value;

			if (readOnly != null)
				IsReadOnly = readOnly.IsReadOnly;

			if (refresh != null)
				RefreshProperties = refresh.RefreshProperties;

			if (description != null)
				Description = description.Description;

			if (category != null)
				Category = category.Category;

			if (editor != null)
			{
				_editorBase = editor.EditorBaseTypeName;
				Editor = editor.EditorTypeName;
			}

			if (displayname != null)
				DisplayName = displayname.DisplayName;

			if (typeconverter != null)
				Converter = typeconverter.ConverterTypeName;
		}

		/// <summary>
		/// Function to retrieve a list of attributes for this property.
		/// </summary>
		/// <returns>A list of attributes.</returns>
		public Attribute[] RetrieveAttributes()
		{
			var attributes = new List<Attribute>();

			attributes.Add(new BrowsableAttribute(true));
			attributes.Add(new ReadOnlyAttribute(IsReadOnly));
			
			if (RefreshProperties != System.ComponentModel.RefreshProperties.None)
				attributes.Add(new RefreshPropertiesAttribute(RefreshProperties));

			if (HasDefaultValue)
				attributes.Add(new DefaultValueAttribute(DefaultValue));

			if (!string.IsNullOrEmpty(Category))
				attributes.Add(new CategoryAttribute(Category));

			if (!string.IsNullOrEmpty(Description))
				attributes.Add(new DescriptionAttribute(Description));

			if (!string.IsNullOrEmpty(Editor))
				attributes.Add(new EditorAttribute(Editor, _editorBase));

			if (!string.IsNullOrEmpty(Converter))
				attributes.Add(new TypeConverterAttribute(Converter));

			if (!string.IsNullOrEmpty(DisplayName))
				attributes.Add(new DisplayNameAttribute(DisplayName));

			return attributes.ToArray();
		}

		/// <summary>
		/// Function to retrieve a value from an object instance for this property.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <returns>The value for the property.</returns>
		public T GetValue<T>()
		{
			return (T)_descriptor.GetValue(_owner);
		}

		/// <summary>
		/// Function to set a value on an object instance for this property.
		/// </summary>
		/// <param name="value">Value to set.</param>
		public void SetValue(object value)
		{
			if (IsReadOnly)
				return;

			_descriptor.SetValue(_owner, value);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentProperty"/> class.
		/// </summary>
		/// <param name="descriptor">The property descriptor.</param>
		/// <param name="owner">Content that owns this property.</param>
		internal ContentProperty(PropertyDescriptor descriptor, ContentObject owner)
			: base(descriptor.Name)
		{
			_owner = owner;
			_descriptor = descriptor;
			Editor = string.Empty;
			Category = string.Empty;
			Description = string.Empty;
			DisplayName = _descriptor.Name;
			Converter = string.Empty;
			HideProperty = false;
			RefreshProperties = RefreshProperties.None;
			GetAttributes();
		}
		#endregion
	}
}
