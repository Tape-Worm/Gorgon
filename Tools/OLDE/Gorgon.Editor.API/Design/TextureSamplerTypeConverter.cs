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
// Created: Tuesday, November 25, 2014 9:54:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using Gorgon.Editor.Properties;
using Gorgon.Graphics;

namespace Gorgon.Editor.Design
{
	/// <summary>
	/// Type converter for renderable texture sampling parameters.
	/// </summary>
	public class TextureSamplerTypeConverter
		: TypeConverter
	{
		#region Variables.
		#endregion

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
			return destinationType == typeof(string);
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
			return string.Empty;
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
			return true;
		}

		/// <summary>
		/// Returns a collection of properties for the type of array specified by the value parameter, using the specified context and attributes.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
		/// <param name="value">An <see cref="T:System.Object" /> that specifies the type of array for which to get properties.</param>
		/// <param name="attributes">An array of type <see cref="T:System.Attribute" /> that is used as a filter.</param>
		/// <returns>
		/// A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> with the properties that are exposed for this data type, or null if there are no properties.
		/// </returns>
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			var typeDesc = (ContentTypeDescriptor)context.Instance;
			PropertyDescriptorCollection result = TypeDescriptor.GetProperties(value, attributes);
			var properties = new List<ContentPropertyDescriptor>();

			// Build the property descriptors for the blending type.	
			foreach (PropertyDescriptor descriptor in result)
			{
				var contentProp = new ContentProperty(descriptor, value, typeDesc["TextureSampler"])
				                  {
					                  HasDefaultValue = true,
									  RefreshProperties = RefreshProperties.All,
									  IsReadOnly = typeDesc["TextureSampler"].IsReadOnly
				                  };

				if (string.Equals(descriptor.Name, "TextureFilter", StringComparison.OrdinalIgnoreCase))
				{
					contentProp.DefaultValue = TextureFilter.Point;
					contentProp.Converter = typeof(TextureFilterTypeConverter).AssemblyQualifiedName;
				    contentProp.DisplayName = APIResources.PROP_SAMPLE_FILTER_NAME;
				    contentProp.Description = APIResources.PROP_SAMPLE_FILTER_DESC;
				}

				if (string.Equals(descriptor.Name, "HorizontalWrapping", StringComparison.OrdinalIgnoreCase))
				{
					contentProp.RefreshProperties = RefreshProperties.All;
					contentProp.DefaultValue = TextureAddressing.Clamp;
					contentProp.Converter = typeof(TextureAddressingTypeConverter).AssemblyQualifiedName;
				    contentProp.DisplayName = APIResources.PROP_SAMPLE_HORZADDR_NAME;
				    contentProp.Description = APIResources.PROP_SAMPLE_HORZADDR_DESC;
				}

				if (string.Equals(descriptor.Name, "VerticalWrapping", StringComparison.OrdinalIgnoreCase))
				{
					contentProp.RefreshProperties = RefreshProperties.All;
					contentProp.DefaultValue = TextureAddressing.Clamp;
					contentProp.Converter = typeof(TextureAddressingTypeConverter).AssemblyQualifiedName;
                    contentProp.DisplayName = APIResources.PROP_SAMPLE_VERTADDR_NAME;
                    contentProp.Description = APIResources.PROP_SAMPLE_VERTADDR_DESC;
                }

				if (string.Equals(descriptor.Name, "BorderColor", StringComparison.OrdinalIgnoreCase))
				{
					contentProp.DefaultValue = GorgonColor.Transparent;
					contentProp.Converter = typeof(RGBATypeConverter).AssemblyQualifiedName;
					contentProp.SetTypeEditor(typeof(RGBAEditor), typeof(UITypeEditor));
                    contentProp.DisplayName = APIResources.PROP_SAMPLE_BORDER_NAME;
                    contentProp.Description = APIResources.PROP_SAMPLE_BORDER_DESC;
                }

				properties.Add(new ContentPropertyDescriptor(contentProp));
			}
			
			return new PropertyDescriptorCollection(properties.Cast<PropertyDescriptor>().OrderBy(item => item.DisplayName ?? item.Name).ToArray());
		}
		#endregion
	}
}
