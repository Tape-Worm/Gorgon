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
