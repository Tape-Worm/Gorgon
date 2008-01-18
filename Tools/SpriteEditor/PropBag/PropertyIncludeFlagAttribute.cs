#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
