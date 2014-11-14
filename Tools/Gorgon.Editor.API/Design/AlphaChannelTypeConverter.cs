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
// Created: Tuesday, May 15, 2012 3:01:37 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace GorgonLibrary.Editor.Design
{
	/// <summary>
	/// Type converter for the Alpha channel in a color.
	/// </summary>
	public class AlphaChannelTypeConverter
		: ColorConverter
	{
		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Converts the given object to the converter's native type.
		/// </summary>
		/// <param name="context">A <see cref="T:System.ComponentModel.TypeDescriptor"/> that provides a format context. You can use this object to get additional information about the environment from which this converter is being invoked.</param>
		/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo"/> that specifies the culture to represent the color.</param>
		/// <param name="value">The object to convert.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> representing the converted value.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">The conversion cannot be performed.</exception>
		///   
		/// <PermissionSet>
		///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
		///   </PermissionSet>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var colorValue = value as String;

			return string.IsNullOrEmpty(colorValue)
				       ? Color.Transparent
				       : Color.FromArgb(int.Parse(colorValue, culture.NumberFormat), 255, 255, 255);
		}

		/// <summary>
		/// Converts the specified object to another type.
		/// </summary>
		/// <param name="context">A formatter context. Use this object to extract additional information about the environment from which this converter is being invoked. Always check whether this value is null. Also, properties on the context object may return null.</param>
		/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo"/> that specifies the culture to represent the color.</param>
		/// <param name="value">The object to convert.</param>
		/// <param name="destinationType">The type to convert the object to.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> representing the converted value.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="destinationType"/> is null.</exception>
		///   
		/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed.</exception>
		///   
		/// <PermissionSet>
		///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
		///   </PermissionSet>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if ((!(value is Color)) || (destinationType != typeof(string)))
				return base.ConvertTo(context, culture, value, destinationType);

			return ((Color)value).A.ToString(culture);
		}
		#endregion
	}
}
