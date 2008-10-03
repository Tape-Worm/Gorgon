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
// Created: Saturday, June 16, 2007 1:08:24 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics.Tools.PropBag
{
	/// <summary>
	/// Attribute to flag a property for inclusion into a property bag.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class PropertyIncludeAttribute
		: Attribute
	{
		#region Variables.
		private bool _isReadOnly = false;				// Flag to indicate that this item is read-only.
		private Type _propertyType = null;				// Property type.
		private string _editorType = string.Empty;		// Editor type.
		private string _convertType = string.Empty;		// Converter type.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the property is read-only or not.
		/// </summary>
		public bool ReadOnly
		{
			get
			{
				return _isReadOnly;
			}
		}

		/// <summary>
		/// Property to return the type for the property.
		/// </summary>
		public Type PropertyType
		{
			get
			{
				return _propertyType;
			}
		}

		/// <summary>
		/// Property to return the converter type.
		/// </summary>
		public string ConverterType
		{
			get
			{
				return _convertType;
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
		#endregion
		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="readOnly">TRUE to mark as read-only, FALSE as read-write.</param>
		/// <param name="propertyType">Type of property.</param>
		/// <param name="editorType">Editor type string.</param>
		/// <param name="converterType">Converter type string.</param>
		public PropertyIncludeAttribute(bool readOnly, Type propertyType, string editorType, string converterType)
		{
			_isReadOnly = readOnly;
			_propertyType = propertyType;
			_editorType = editorType;
			_convertType = converterType;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="readOnly">TRUE to mark as read-only, FALSE as read-write.</param>
		/// <param name="propertyType">Type of property.</param>
		/// <param name="editorType">Editor type string.</param>
		/// <param name="converterType">Converter type string.</param>
		public PropertyIncludeAttribute(bool readOnly, Type propertyType, Type editorType, Type converterType)
			: this(readOnly, propertyType, string.Empty, string.Empty)
		{
			_editorType = editorType.AssemblyQualifiedName;
			_convertType = converterType.AssemblyQualifiedName;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="readOnly">TRUE to mark as read-only, FALSE as read-write.</param>
		/// <param name="propertyType">Type of property.</param>
		/// <param name="editorType">Editor type string.</param>
		/// <param name="converterType">Converter type string.</param>
		public PropertyIncludeAttribute(bool readOnly, Type propertyType, string editorType, Type converterType)
			: this(readOnly, propertyType, editorType, string.Empty)
		{
			_convertType = converterType.AssemblyQualifiedName;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="readOnly">TRUE to mark as read-only, FALSE as read-write.</param>
		/// <param name="propertyType">Type of property.</param>
		/// <param name="editorType">Editor type string.</param>
		/// <param name="converterType">Converter type string.</param>
		public PropertyIncludeAttribute(bool readOnly, Type propertyType, Type editorType, string converterType)
			: this(readOnly, propertyType, string.Empty, converterType)
		{
			_editorType = editorType.AssemblyQualifiedName;			
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="readOnly">TRUE to mark as read-only, FALSE as read-write.</param>
		/// <param name="editorType">Editor type string.</param>
		/// <param name="converterType">Converter type string.</param>
		public PropertyIncludeAttribute(bool readOnly, string editorType, string converterType)
			: this(readOnly, null, editorType, converterType)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="readOnly">TRUE to mark as read-only, FALSE as read-write.</param>
		/// <param name="editorType">Editor type string.</param>
		public PropertyIncludeAttribute(bool readOnly, string editorType)
			: this(readOnly, null, editorType, string.Empty)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="readOnly">TRUE to mark as read-only, FALSE as read-write.</param>
		/// <param name="propertyType">Type of property.</param>
		/// <param name="editorType">Editor type string.</param>
		public PropertyIncludeAttribute(bool readOnly, Type propertyType, string editorType)
			: this(readOnly, propertyType, editorType, string.Empty)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="readOnly">TRUE to mark as read-only, FALSE as read-write.</param>
		/// <param name="editorType">Editor type string.</param>
		/// <param name="converterType">Converter type string.</param>
		public PropertyIncludeAttribute(bool readOnly, Type editorType, Type converterType)
			: this(readOnly, null, editorType, converterType)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="readOnly">TRUE to mark as read-only, FALSE as read-write.</param>
		/// <param name="editorType">Editor type string.</param>
		/// <param name="converterType">Converter type string.</param>
		public PropertyIncludeAttribute(bool readOnly, string editorType, Type converterType)
			: this(readOnly, null, editorType, converterType)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="readOnly">TRUE to mark as read-only, FALSE as read-write.</param>
		/// <param name="propertyType">Type of property.</param>
		/// <param name="editorType">Editor type string.</param>
		public PropertyIncludeAttribute(bool readOnly, Type propertyType)
			: this(readOnly, propertyType, string.Empty, string.Empty)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="readOnly">TRUE to mark as read-only, FALSE as read-write.</param>
		/// <param name="propertyType">Type of property.</param>
		public PropertyIncludeAttribute(bool readOnly)
			: this(readOnly, null, string.Empty, string.Empty)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>		
		public PropertyIncludeAttribute()
			: this(false, null, string.Empty, string.Empty)
		{
		}
		#endregion
	}
}
