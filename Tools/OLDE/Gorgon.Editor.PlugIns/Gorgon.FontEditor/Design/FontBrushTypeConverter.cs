#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Wednesday, November 13, 2013 9:30:12 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Globalization;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Type converter for the font brush.
	/// </summary>
	class FontBrushTypeConverter
		: TypeConverter
	{
		#region Methods.
		/// <summary>
		/// Returns whether this converter can convert the object to the specified type, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="destinationType">A <see cref="T:System.Type"/> that represents the type you want to convert to.</param>
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
		}

		/// <summary>
		/// Converts the given value object to the specified type, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo"/>. If null is passed, the current culture is assumed.</param>
		/// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
		/// <param name="destinationType">The <see cref="T:System.Type"/> to convert the <paramref name="value"/> parameter to.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the converted value.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="destinationType"/> parameter is null. </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
		    var brush = value as GorgonGlyphBrush;

		    if (destinationType != typeof(string))
		    {
		        return base.ConvertTo(context, culture, value, destinationType);
		    }
            

		    CultureInfo prevCulture = Resources.Culture;
		    Resources.Culture = culture;

		    try
		    {

		        if (brush == null)
		        {
		            return Resources.GORFNT_TEXT_NONE;
		        }

		        switch (brush.BrushType)
		        {
		            case GlyphBrushType.Hatched:
		                return Resources.GORFNT_TEXT_PATTERN_BRUSH;
		            case GlyphBrushType.LinearGradient:
		                return Resources.GORFNT_TEXT_GRADIENT_BRUSH;
		            case GlyphBrushType.Solid:
		                return Resources.GORFNT_TEXT_SOLID_BRUSH;
		            case GlyphBrushType.Texture:
		                return Resources.GORFNT_TEXT_TEXTURE_BRUSH;
		            default:
		                return Resources.GORFNT_ERR_UNKNOWN_BRUSH;

		        }
		    }
		    finally
		    {
		        Resources.Culture = prevCulture;
		    }
		}
		#endregion
	}
}
