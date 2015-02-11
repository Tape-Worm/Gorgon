#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Saturday, November 15, 2014 9:51:08 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Globalization;
using GorgonLibrary.Editor.SpriteEditorPlugIn.Properties;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Editor.SpriteEditorPlugIn.Design
{
	/// <summary>
	/// A type converter for textures on a sprite.
	/// </summary>
	class TextureTypeConverter
		: TypeConverter
	{
		#region Methods.
		/// <summary>
		/// Returns whether this converter can convert the object to the specified type, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
		/// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type you want to convert to.</param>
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
		}

		/// <summary>
		/// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
		/// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from.</param>
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return false;
		}

		/// <summary>
		/// Converts the given value object to the specified type, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
		/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" />. If null is passed, the current culture is assumed.</param>
		/// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
		/// <param name="destinationType">The <see cref="T:System.Type" /> to convert the <paramref name="value" /> parameter to.</param>
		/// <returns>
		/// An <see cref="T:System.Object" /> that represents the converted value.
		/// </returns>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType != typeof(string))
			{
				return base.ConvertTo(context, culture, value, destinationType);
			}

			var texture = value as GorgonTexture2D;

			return texture == null ? Resources.GORSPR_PROP_NO_TEXTURE : texture.Name;
		}

		/// <summary>
		/// Returns whether this object supports properties, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
		/// <returns>
		/// true if <see cref="M:System.ComponentModel.TypeConverter.GetProperties(System.Object)" /> should be called to find the properties of this object; otherwise, false.
		/// </returns>
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return false;
		}

		/// <summary>
		/// Returns whether this object supports a standard set of values that can be picked from a list, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
		/// <returns>
		/// true if <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> should be called to find a common set of values the object supports; otherwise, false.
		/// </returns>
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return false;
		}
		#endregion
	}
}
