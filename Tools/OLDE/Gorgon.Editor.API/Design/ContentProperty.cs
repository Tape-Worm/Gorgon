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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Gorgon.Core;
using Gorgon.Graphics;

namespace Gorgon.Editor
{
	/// <summary>
	/// A property for content.
	/// </summary>
	public class ContentProperty
		: GorgonNamedObject
	{
		#region Variables.
		private readonly object _owner;
		private readonly PropertyDescriptor _descriptor;
		private string _editorBase = string.Empty;
		private object _defaultValue;
		private readonly ContentProperty _parentProperty;
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
				if (_defaultValue == value)
				{
					return;
				}

				_defaultValue = value;
				HasDefaultValue = true;
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
			private set;
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
			DefaultValueAttribute defaultValue = null;
			ReadOnlyAttribute readOnly = null;
			RefreshPropertiesAttribute refresh = null;
			DescriptionAttribute description = null;
			CategoryAttribute category = null;
			EditorAttribute editor = null;
			TypeConverterAttribute typeConverter = null;
			DisplayNameAttribute displayName = null;

			foreach (Attribute attribute in _descriptor.Attributes.Cast<Attribute>())
			{
				if (defaultValue == null)
				{
					defaultValue = attribute as DefaultValueAttribute;
				}

				if (readOnly == null)
				{
					readOnly = attribute as ReadOnlyAttribute;
				}

				if (refresh == null)
				{
					refresh = attribute as RefreshPropertiesAttribute;
				}

				if (description == null)
				{
					description = attribute as DescriptionAttribute;
				}

				if (category == null)
				{
					category = attribute as CategoryAttribute;
				}

				if (editor == null)
				{
					editor = attribute as EditorAttribute;
				}

				if (typeConverter == null)
				{
					typeConverter = attribute as TypeConverterAttribute;
				}

				if (displayName == null)
				{
					displayName = attribute as DisplayNameAttribute;
				}
			}

			if (defaultValue != null)
			{
				DefaultValue = defaultValue.Value;
			}

			if (readOnly != null)
			{
				IsReadOnly = readOnly.IsReadOnly;
			}

			if (refresh != null)
			{
				RefreshProperties = refresh.RefreshProperties;
			}

			if (description != null)
			{
				Description = description.Description;
			}

			if (category != null)
			{
				Category = category.Category;
			}

			if (editor != null)
			{
				_editorBase = editor.EditorBaseTypeName;
				Editor = editor.EditorTypeName;
			}

			if (displayName != null)
			{
				DisplayName = displayName.DisplayName;
			}

			if (typeConverter != null)
			{
				Converter = typeConverter.ConverterTypeName;
			}
		}

		/// <summary>
		/// Function to retrieve a list of attributes for this property.
		/// </summary>
		/// <returns>A list of attributes.</returns>
		public Attribute[] RetrieveAttributes()
		{
			var attributes = new List<Attribute>
			                 {
				                 new BrowsableAttribute(true),
				                 new ReadOnlyAttribute(IsReadOnly)
			                 };

			if (RefreshProperties != RefreshProperties.None)
			{
				attributes.Add(new RefreshPropertiesAttribute(RefreshProperties));
			}

			if (HasDefaultValue)
			{
				attributes.Add(new DefaultValueAttribute(DefaultValue));
			}

			if (!string.IsNullOrEmpty(Category))
			{
				attributes.Add(new CategoryAttribute(Category));
			}

			if (!string.IsNullOrEmpty(Description))
			{
				attributes.Add(new DescriptionAttribute(Description));
			}

			if (!string.IsNullOrEmpty(Editor))
			{
				attributes.Add(new EditorAttribute(Editor, _editorBase));
			}

			if (!string.IsNullOrEmpty(Converter))
			{
				attributes.Add(new TypeConverterAttribute(Converter));
			}

			if (!string.IsNullOrEmpty(DisplayName))
			{
				attributes.Add(new DisplayNameAttribute(DisplayName));
			}

			return attributes.ToArray();
		}

		/// <summary>
		/// Function to set the type editor for this property.
		/// </summary>
		/// <param name="editorType">Type of the editor to set.</param>
		/// <param name="editorBaseType">[Optional] Base type of the editor.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="editorBaseType"/> will default the base type to UITypeEditor.</remarks>
		public void SetTypeEditor(Type editorType, Type editorBaseType = null)
		{
			if (editorType == null)
			{
				Editor = string.Empty;
				_editorBase = string.Empty;
				return;
			}

			Editor = editorType.AssemblyQualifiedName;
			_editorBase = editorBaseType == null ? typeof(UITypeEditor).AssemblyQualifiedName : editorBaseType.AssemblyQualifiedName;
		}

		/// <summary>
		/// Function to retrieve a value from an object instance for this property.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <returns>The value for the property.</returns>
		public T GetValue<T>()
		{
			object value = _descriptor.GetValue(_owner);

			if (value == null)
			{
				return default(T);
			}

			return (T)value;
		}

		/// <summary>
		/// Function to set a value on an object instance for this property.
		/// </summary>
		/// <param name="value">Value to set.</param>
		public void SetValue(object value)
		{
			ContentProperty topProperty = _parentProperty;

			if (IsReadOnly)
			{
				return;
			}

			if (topProperty != null)
			{
				// Go to the top of the property chain.
				while (topProperty._parentProperty != null)
				{
					topProperty = topProperty._parentProperty;
				}
			}

			// This is a little hack to convert a System.Drawing.Color to a GorgonColor.
			if ((_descriptor.PropertyType == typeof(GorgonColor))
			    && (value is Color))
			{
				_descriptor.SetValue(_owner, new GorgonColor((Color)value));
			}
			else
			{
				_descriptor.SetValue(_owner, value);
			}

			if ((_parentProperty == null) || (topProperty == null) || (topProperty._owner == null))
			{
				return;
			}

			var content = topProperty._owner as ContentObject;

			if (content == null)
			{
				return;
			}

			// Notify that the sub property has changed.
			content.NotifyPropertyChanged(_descriptor.Name, _owner);
		}

		/// <summary>
		/// Function to retrieve the instance of the property being accessed by this object.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <returns>The instance tied to this object.</returns>
		public T GetPropertyInstance<T>()
		{
			return (T)_owner;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentProperty"/> class.
		/// </summary>
		/// <param name="descriptor">The property descriptor.</param>
		/// <param name="owner">Object that owns this property.</param>
		/// <param name="parentProperty">[Optional] The property that owns this property.</param>
		public ContentProperty(PropertyDescriptor descriptor, object owner, ContentProperty parentProperty = null)
			: base(descriptor.Name)
		{
			_owner = owner;
			_descriptor = descriptor;
			_parentProperty = parentProperty;
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
