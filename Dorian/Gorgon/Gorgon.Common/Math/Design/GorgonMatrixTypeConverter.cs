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
// Created: Friday, January 13, 2012 9:50:37 AM
// 
#endregion

//
//  Most of the code in this file was modified or taken directly from the SlimMath project by Mike Popoloski.
//  SlimMath may be downloaded from: http://code.google.com/p/slimmath/
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using GorgonLibrary.Design;

namespace GorgonLibrary.Math.Design
{
	/// <summary>
	/// Type converter for the Matrix type.
	/// </summary>
	public class GorgonMatrixTypeConverter
		: GorgonExpandableObjectTypeConverter
	{
		#region Methods.
		/// <summary>
		/// Converts the given object to the type of this converter, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"/> to use as the current culture.</param>
		/// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the converted value.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var values = GetValues<float>(context, culture, value);

			if (values != null)
				return new GorgonMatrix(values);

			return base.ConvertFrom(context, culture, value);
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
		/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == null)
				throw new ArgumentNullException("destinationType");

			if (value is GorgonMatrix)
			{
				var matrix = (GorgonMatrix)value;

				if (destinationType == typeof(string))
					GetString(context, culture, matrix.ToArray());

				if (destinationType == typeof(InstanceDescriptor))
				{
					var constructor = Type.GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float), typeof(float),
																	typeof(float), typeof(float), typeof(float), typeof(float),
																	typeof(float), typeof(float), typeof(float), typeof(float),
																	typeof(float), typeof(float), typeof(float), typeof(float) });
					if (constructor != null)
						return new InstanceDescriptor(constructor, matrix.ToArray());
				}
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		/// <summary>
		/// Creates an instance of the type that this <see cref="T:System.ComponentModel.TypeConverter"/> is associated with, using the specified context, given a set of property values for the object.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="propertyValues">An <see cref="T:System.Collections.IDictionary"/> of new property values.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> representing the given <see cref="T:System.Collections.IDictionary"/>, or null if the object cannot be created. This method always returns null.
		/// </returns>
		public override object CreateInstance(ITypeDescriptorContext context, System.Collections.IDictionary propertyValues)
		{
			if (propertyValues == null)
				throw new ArgumentNullException("propertyValues");

			var matrix = new GorgonMatrix
			{
				m11 = (float)propertyValues["m11"],
				m12 = (float)propertyValues["m12"],
				m13 = (float)propertyValues["m13"],
				m14 = (float)propertyValues["m14"],
				m21 = (float)propertyValues["m21"],
				m22 = (float)propertyValues["m22"],
				m23 = (float)propertyValues["m23"],
				m24 = (float)propertyValues["m24"],
				m31 = (float)propertyValues["m31"],
				m32 = (float)propertyValues["m32"],
				m33 = (float)propertyValues["m33"],
				m34 = (float)propertyValues["m34"],
				m41 = (float)propertyValues["m41"],
				m42 = (float)propertyValues["m42"],
				m43 = (float)propertyValues["m43"],
				m44 = (float)propertyValues["m44"]
			};

			return matrix;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMatrixTypeConverter"/> class.
		/// </summary>
		public GorgonMatrixTypeConverter()
			: base(typeof(GorgonMatrix))
		{
			Properties = new PropertyDescriptorCollection(new[] 
			{
				new GorgonFieldDescriptor(Type.GetField("m11")), 
				new GorgonFieldDescriptor(Type.GetField("m12")),
				new GorgonFieldDescriptor(Type.GetField("m13")),
				new GorgonFieldDescriptor(Type.GetField("m14")),

				new GorgonFieldDescriptor(Type.GetField("m21")), 
				new GorgonFieldDescriptor(Type.GetField("m22")),
				new GorgonFieldDescriptor(Type.GetField("m23")),
				new GorgonFieldDescriptor(Type.GetField("m24")),

				new GorgonFieldDescriptor(Type.GetField("m31")), 
				new GorgonFieldDescriptor(Type.GetField("m32")),
				new GorgonFieldDescriptor(Type.GetField("m33")),
				new GorgonFieldDescriptor(Type.GetField("m34")),

				new GorgonFieldDescriptor(Type.GetField("m41")), 
				new GorgonFieldDescriptor(Type.GetField("m42")),
				new GorgonFieldDescriptor(Type.GetField("m43")),
				new GorgonFieldDescriptor(Type.GetField("m44")),
			});
		}
		#endregion
	}
}
