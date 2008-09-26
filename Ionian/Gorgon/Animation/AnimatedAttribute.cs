#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
