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
// Created: Wednesday, October 10, 2012 3:55:05 PM
// 
#endregion

using System;
using System.Reflection;
using Gorgon.Diagnostics;

namespace Gorgon.Animation
{
	/// <summary>
	/// Information about a property on an object that should be animated.
	/// </summary>
	public struct GorgonAnimatedProperty
	{
		/// <summary>
		/// Property information.
		/// </summary>
		/// The property reflection information.
		public readonly PropertyInfo Property;
		/// <summary>
		/// Display name for the property.
		/// </summary>
		/// <remarks>This is used to rename the property to something else.</remarks>
        public readonly string DisplayName;
		/// <summary>
		/// Type of data in the property.
		/// </summary>
        public readonly Type DataType;

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimatedProperty" /> struct.
		/// </summary>
		/// <param name="displayName">[Optional] The display name used to override the property name.</param>
		/// <param name="dataType">[Optional] Type of the data used to override the property type.</param>
		/// <param name="propertyInfo">The property info.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="propertyInfo"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public GorgonAnimatedProperty(PropertyInfo propertyInfo, string displayName = null, Type dataType = null)
		{
			propertyInfo.ValidateObject("propertyInfo");

			DisplayName = string.IsNullOrWhiteSpace(displayName) ? propertyInfo.Name : displayName;
			DataType = dataType ?? propertyInfo.PropertyType;
			Property = propertyInfo;
		}
	}
}
