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
// Created: Saturday, June 16, 2007 1:17:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics.Tools.PropBag
{
	/// <summary>
	/// Attribute to assign a default value for the property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class PropertyDefaultAttribute
		: Attribute
	{
		#region Variables.
		private object _default = null;		// Default value for the property.
		private Type _propertyType = null;	// Type of the property.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the default value.
		/// </summary>
		public object DefaultValue
		{
			get
			{
				return _default;
			}
		}

		/// <summary>
		/// Property to return the type of the property.
		/// </summary>
		public Type PropertyType
		{
			get
			{
				return _propertyType;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="value">Value for the property.</param>
		/// <param name="propertyType">Type of property.</param>
		public PropertyDefaultAttribute(object value, Type propertyType)
		{
			_default = value;
			_propertyType = propertyType;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="value">Value for the property.</param>
		public PropertyDefaultAttribute(object value)
			: this(value, (value == null) ? null : value.GetType())
		{			
		}
		#endregion
	}
}
