#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Wednesday, April 30, 2008 9:32:46 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Attribute to define if a property can be animated or not.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class AnimatedAttribute
		: Attribute
	{
		#region Variables.
		private Type _dataType = null;					// Data type.
		private InterpolationMode _interpolation;		// Interpolation mode.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the data type to be animated.
		/// </summary>
		public Type DataType
		{
			get
			{
				return _dataType;
			}
		}

		/// <summary>
		/// Property to return the interpolation mode.
		/// </summary>
		public InterpolationMode InterpolationMode
		{
			get
			{
				return _interpolation;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="AnimatedAttribute"/> class.
		/// </summary>
		/// <param name="dataType">Type of the data for the property.</param>
		/// <param name="interpolation">Interpolation mode for this data.</param>
		public AnimatedAttribute(Type dataType, InterpolationMode interpolation)			
		{
			_dataType = dataType;
			_interpolation = interpolation;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimatedAttribute"/> class.
		/// </summary>
		/// <param name="dataType">Type of the data for the property.</param>
		public AnimatedAttribute(Type dataType)
			: this(dataType, InterpolationMode.Linear)
		{
		}
		#endregion
	}
}
