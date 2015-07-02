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
// Created: Monday, September 3, 2012 8:10:46 PM
// 
#endregion

using System;

namespace Gorgon.Animation
{
	/// <summary>
	/// An attribute to define a property on an animated object as being animated.
	/// </summary>
    /// <remarks>Assign this attribute to any property that should be animated.</remarks>
    [Obsolete("TEMP: This is just here to keep the compiler happy until I figure out what I'm going to do with this API.")]
	[AttributeUsage(AttributeTargets.Property)]
	public class AnimatedPropertyAttribute
		: Attribute
	{
		#region Properties
		/// <summary>
		/// Property to return the data type.
		/// </summary>
		public Type DataType
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the name to display.
		/// </summary>
		public string DisplayName
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="AnimatedPropertyAttribute" /> class.
		/// </summary>
		public AnimatedPropertyAttribute()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimatedPropertyAttribute" /> class.
		/// </summary>
		/// <param name="displayName">The display name for this property.</param>
		/// <param name="dataType">Type of the data that this property represents.</param>
		public AnimatedPropertyAttribute(string displayName, Type dataType)
		{
			DisplayName = displayName;
			DataType = dataType;
		}
		#endregion
	}
}
