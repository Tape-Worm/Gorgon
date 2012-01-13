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
// Created: Tuesday, January 10, 2012 3:22:35 PM
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
	/// Type converter for 3D vectors.
	/// </summary>
	public class GorgonVector3TypeConverter
		: GorgonExpandableObjectTypeConverter
	{
		#region Methods.
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

			return new GorgonVector3((float)propertyValues["X"], (float)propertyValues["Y"], (float)propertyValues["Z"]);
		}

		/// <summary>
		/// Converts the given object to the type of this converter, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"/> to use as the current culture.</param>
		/// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the converted value.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		/// The conversion cannot be performed.
		/// </exception>
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			var values = GetValues<float>(context, culture, value);

			if (values != null)
				return new GorgonVector3(values);

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
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="destinationType"/> parameter is null.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The conversion cannot be performed.
		/// </exception>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if(destinationType == null)
				throw new ArgumentNullException("destinationType");
			
			if (value is GorgonVector3)
			{
				GorgonVector3 vector = (GorgonVector3)value;

				if (destinationType == typeof(string))
					return GetString(context, culture, vector.ToArray());

				if (destinationType == typeof(InstanceDescriptor))
				{
					ConstructorInfo constructor = Type.GetConstructor(new [] {typeof(float), typeof(float), typeof(float)});

					if (constructor != null)
						return new InstanceDescriptor(constructor, new object[] { vector.X, vector.Y, vector.Z});
				}
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector3TypeConverter"/> class.
		/// </summary>
		public GorgonVector3TypeConverter()
			: base(typeof(GorgonVector3))
		{
			Properties = new PropertyDescriptorCollection(new [] 
			{ 
				new GorgonFieldDescriptor(Type.GetField("X")), 
				new GorgonFieldDescriptor(Type.GetField("Y")), 
				new GorgonFieldDescriptor(Type.GetField("Z")) 
			});
		}
		#endregion
	}
}
