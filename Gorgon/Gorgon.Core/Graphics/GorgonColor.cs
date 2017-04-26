#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Friday, September 02, 2011 6:32:30 AM
// 
#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Core.Properties;

namespace Gorgon.Graphics
{
	/// <summary>
	/// An immutable 4 component (Red, Green, Blue, and Alpha) color value.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This value type represents an RGBA (Red, Green, Blue, Alpha) color using a <see cref="float"/> for each color component. 
	/// </para>
	/// <para>
	/// Primarily this is used in graphical operations and can be converted to a <see cref="Color"/> value implicitly for use in <see cref="System.Drawing"/> operations.
	/// </para>
	/// </remarks>
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct GorgonColor
		: IGorgonEquatableByRef<GorgonColor>, ISerializable
	{
		#region Variables.
		/// <summary>
		/// A completely transparent color.
		/// </summary>
		public static readonly GorgonColor Transparent = new GorgonColor(0, 0, 0, 0);
		/// <summary>
		/// The color white.
		/// </summary>
		public static readonly GorgonColor White = new GorgonColor(1, 1, 1, 1);
		/// <summary>
		/// The color black.
		/// </summary>
		public static readonly GorgonColor Black = new GorgonColor(0, 0, 0, 1);

		/// <summary>
		/// The Red color channel component.
		/// </summary>
		public readonly float Red;
		/// <summary>
		/// The Green color channel component.
		/// </summary>
        public readonly float Green;
		/// <summary>
		/// The Blue color channel component.
		/// </summary>
        public readonly float Blue;
		/// <summary>
		/// The Alpha channel component.
		/// </summary>
        public readonly float Alpha;
		#endregion

		#region Methods.
		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// 	<b>true</b> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <b>false</b>.
		/// </returns>
		public override bool Equals(object obj)
		{
		    if (obj is GorgonColor)
		    {
		        return ((GorgonColor)obj).Equals(this);
		    }

		    return base.Equals(obj);
		}

		/// <summary>
		/// Function to convert a ABGR color into a <see cref="GorgonColor"/>.
		/// </summary>
		/// <param name="abgrColor">An <see cref="int"/> representing the color in ABGR format.</param>
		/// <returns>The <see cref="GorgonColor"/> representation.</returns>
		public static GorgonColor FromABGR(int abgrColor)
		{
			byte a = (byte)((abgrColor >> 24) & 0xff);
			byte b = (byte)((abgrColor >> 16) & 0xff);
			byte g = (byte)((abgrColor >> 8) & 0xff);
			byte r = (byte)(abgrColor & 0xff);

			return new GorgonColor(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
		}

		/// <summary>
		/// Function to convert a BGRA color into a <see cref="GorgonColor"/>.
		/// </summary>
		/// <param name="bgraColor">An <see cref="int"/> representing the color in BGRA format.</param>
		/// <returns>The GorgonColor representation.</returns>
		public static GorgonColor FromBGRA(int bgraColor)
		{
			byte b = (byte)((bgraColor >> 24) & 0xff);
			byte g = (byte)((bgraColor >> 16) & 0xff);
			byte r = (byte)((bgraColor >> 8) & 0xff);
			byte a = (byte)(bgraColor & 0xff);

			return new GorgonColor(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
		}

		/// <summary>
		/// Function to convert a RGBA color into a <see cref="GorgonColor"/>.
		/// </summary>
		/// <param name="rgbaColor">An <see cref="int"/> representing the color in RGBA format.</param>
		/// <returns>The <see cref="GorgonColor"/> representation.</returns>
		public static GorgonColor FromRGBA(int rgbaColor)
		{
			byte r = (byte)((rgbaColor >> 24) & 0xff);
			byte g = (byte)((rgbaColor >> 16) & 0xff);
			byte b = (byte)((rgbaColor >> 8) & 0xff);
			byte a = (byte)(rgbaColor & 0xff);

			return new GorgonColor(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
		}		

		/// <summary>
		/// Function to compare two colors for equality.
		/// </summary>
		/// <param name="left">Left color to compare.</param>
		/// <param name="right">Right color to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(ref GorgonColor left, ref GorgonColor right)
		{
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return left.Red == right.Red && left.Green == right.Green && left.Blue == right.Blue && left.Alpha == right.Alpha;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(Red).GenerateHash(Green).GenerateHash(Blue).GenerateHash(Alpha);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
		    return string.Format(Resources.GOR_TOSTR_GORGONCOLOR, Red, Green, Blue, Alpha);
		}

        /// <summary>
		/// Function to apply an alpha value to the specified <see cref="GorgonColor"/>.
        /// </summary>
		/// <param name="color">The <see cref="GorgonColor"/> to update.</param>
        /// <param name="alpha">The alpha value to set.</param>
		/// <param name="result">The resulting updated <see cref="GorgonColor"/>.</param>
		/// <returns>A new <see cref="GorgonColor"/> instance with the same <see cref="Red"/>, <see cref="Green"/>, and <see cref="Blue"/> values but with a modified <see cref="Alpha"/> component.</returns>
        public static void SetAlpha(ref GorgonColor color, float alpha, out GorgonColor result)
        {
            result = new GorgonColor(color, alpha);
        }

		/// <summary>
		/// Function to perform linear interpolation between two <see cref="GorgonColor"/> values.
		/// </summary>
		/// <param name="start">The starting <see cref="GorgonColor"/>.</param>
		/// <param name="end">The ending <see cref="GorgonColor"/>.</param>
		/// <param name="weight">Value between 0 and 1.0f to indicate weighting between start and end.</param>
		/// <returns>The new <see cref="GorgonColor"/> representing a color between the <paramref name="start"/> and <paramref name="end"/> values.</returns>
		/// <remarks>
		/// This will compute a new <see cref="GorgonColor"/> from the <paramref name="start"/> and <paramref name="end"/> parameters based on the <paramref name="weight"/> passed in. For example, if the 
		/// <paramref name="start"/> is Red = 0, Green = 0, Blue = 0, and Alpha = 0, and the <paramref name="end"/> is Red = 1, Green = 0, Blue = 1, and Alpha 0.5f. Then, with a <paramref name="weight"/> of 
		/// 0.5f, the result will be Red = 0.5f, Green = 0, Blue = 0.5f, and an Alpha = 0.25f.
		/// </remarks>
		public static GorgonColor Lerp(GorgonColor start, GorgonColor end, float weight)
		{
		    var outColor = new GorgonColor(
		        start.Red + ((end.Red - start.Red) * weight),
		        start.Green + ((end.Green - start.Green) * weight),
		        start.Blue + ((end.Blue - start.Blue) * weight),
		        start.Alpha + ((end.Alpha - start.Alpha) * weight));

			return outColor;
		}

		/// <summary>
		/// Function to perform linear interpolation between two <see cref="GorgonColor"/> values.
		/// </summary>
		/// <param name="start">The starting <see cref="GorgonColor"/>.</param>
		/// <param name="end">The ending <see cref="GorgonColor"/>.</param>
		/// <param name="weight">Value between 0 and 1.0f to indicate weighting between start and end.</param>
		/// <param name="outColor">The new <see cref="GorgonColor"/> representing a color between the <paramref name="start"/> and <paramref name="end"/> values.</param>
		/// <remarks>
		/// This will compute a new <see cref="GorgonColor"/> from the <paramref name="start"/> and <paramref name="end"/> parameters based on the <paramref name="weight"/> passed in. For example, if the 
		/// <paramref name="start"/> is Red = 0, Green = 0, Blue = 0, and Alpha = 0, and the <paramref name="end"/> is Red = 1, Green = 0, Blue = 1, and Alpha 0.5f. Then, with a <paramref name="weight"/> of 
		/// 0.5f, the result will be Red = 0.5f, Green = 0, Blue = 0.5f, and an Alpha = 0.25f.
		/// </remarks>
		public static void Lerp(ref GorgonColor start, ref GorgonColor end, float weight, out GorgonColor outColor)
		{
            outColor = new GorgonColor(start.Red + ((end.Red - start.Red) * weight),
                start.Green + ((end.Green - start.Green) * weight),
                start.Blue + ((end.Blue - start.Blue) * weight),
                start.Alpha + ((end.Alpha - start.Alpha) * weight));
		}

		/// <summary>
		/// Function to add two <see cref="GorgonColor"/> values together.
		/// </summary>
		/// <param name="left">The left color to add.</param>
		/// <param name="right">The right color to add.</param>
		/// <param name="outColor">The total of the two colors.</param>
		/// <remarks>
		/// This method does not clamp its output. Values greater than 1 or less than 0 are possible.
		/// </remarks>
		public static void Add(ref GorgonColor left, ref GorgonColor right, out GorgonColor outColor)
		{
            outColor = new GorgonColor(left.Red + right.Red,
                                        left.Green + right.Green,
                                        left.Blue + right.Blue,
                                        left.Alpha + right.Alpha);
		}

		/// <summary>
		/// Function to add two <see cref="GorgonColor"/> values together.
		/// </summary>
		/// <param name="left">The left color to add.</param>
		/// <param name="right">The right color to add.</param>
		/// <returns>The total of the two colors.</returns>
		/// <remarks>
		/// This method does not clamp its output. Values greater than 1 or less than 0 are possible.
		/// </remarks>
		public static GorgonColor Add(GorgonColor left, GorgonColor right)
		{
			return new GorgonColor(left.Red + right.Red,
			                       left.Green + right.Green,
			                       left.Blue + right.Blue,
			                       left.Alpha + right.Alpha);
		}

		/// <summary>
		/// Function to subtract two <see cref="GorgonColor"/> values from each other.
		/// </summary>
		/// <param name="left">The left color to subtract.</param>
		/// <param name="right">The right color to subtract.</param>
		/// <param name="outColor">The difference between the two colors.</param>
		/// <remarks>
		/// This method does not clamp its output. Values greater than 1 or less than 0 are possible.
		/// </remarks>
		public static void Subtract(ref GorgonColor left, ref GorgonColor right, out GorgonColor outColor)
		{
            outColor = new GorgonColor(left.Red - right.Red,
                                        left.Green - right.Green,
                                        left.Blue - right.Blue,
                                        left.Alpha - right.Alpha);
        }

		/// <summary>
		/// Function to subtract two <see cref="GorgonColor"/> values from each other.
		/// </summary>
		/// <param name="left">The left color to subtract.</param>
		/// <param name="right">The right color to subtract.</param>
		/// <returns>The difference between the two colors.</returns>
		/// <remarks>
		/// This method does not clamp its output. Values greater than 1 or less than 0 are possible.
		/// </remarks>
		public static GorgonColor Subtract(GorgonColor left, GorgonColor right)
		{
			return new GorgonColor(left.Red - right.Red,
			                       left.Green - right.Green,
			                       left.Blue - right.Blue,
			                       left.Alpha - right.Alpha);
		}

		/// <summary>
		/// Function to multiply two <see cref="GorgonColor"/> values together.
		/// </summary>
		/// <param name="left">The left color to multiply.</param>
		/// <param name="right">The right color to multiply.</param>
		/// <param name="outColor">Product of the two colors.</param>
		public static void Multiply(ref GorgonColor left, ref GorgonColor right, out GorgonColor outColor)
		{
            outColor = new GorgonColor(left.Red * right.Red,
                                        left.Green * right.Green,
                                        left.Blue * right.Blue,
                                        left.Alpha * right.Alpha);
		}

		/// <summary>
		/// Function to multiply two <see cref="GorgonColor"/> values together.
		/// </summary>
		/// <param name="left">The left color to multiply.</param>
		/// <param name="right">The right color to multiply.</param>
		/// <returns>Product of the two colors.</returns>
		public static GorgonColor Multiply(GorgonColor left, GorgonColor right)
		{
			return new GorgonColor(left.Red * right.Red,
			                       left.Green * right.Green,
			                       left.Blue * right.Blue,
			                       left.Alpha * right.Alpha);
		}

		/// <summary>
		/// Function to multiply a <see cref="GorgonColor"/> by a value.
		/// </summary>
		/// <param name="color">The color to multiply.</param>
		/// <param name="value">The value to multiply.</param>
		/// <param name="outColor">Product of the <paramref name="color"/> and the <paramref name="value"/>.</param>
		public static void Multiply(ref GorgonColor color, float value, out GorgonColor outColor)
		{
		    outColor = new GorgonColor(color.Red * value,
		                               color.Green * value,
		                               color.Blue * value,
		                               color.Alpha * value);
		}

		/// <summary>
		/// Function to multiply a <see cref="GorgonColor"/> by a value.
		/// </summary>
		/// <param name="color">The color to multiply.</param>
		/// <param name="value">The value to multiply.</param>
		/// <returns>Product of the <paramref name="color"/> and the <paramref name="value"/>.</returns>
		public static GorgonColor Multiply(GorgonColor color, float value)
		{
			return new GorgonColor(color.Red * value,
			                       color.Green * value,
			                       color.Blue * value,
			                       color.Alpha * value);
		}

		/// <summary>
		/// Function to convert this <see cref="GorgonColor"/> value into an <see cref="int"/> value with a ARGB format.
		/// </summary>
		/// <returns>An <see cref="int"/> representing the color value in ARGB format.</returns>
		/// <remarks>
		/// The format indicates the byte position of each color component in the <see cref="int"/> value.
		/// </remarks>
		public int ToARGB()
		{
		    uint result = (((uint)(Alpha * 255.0f)) & 0xff) << 24 | (((uint)(Red * 255.0f)) & 0xff) << 16 |
		                  (((uint)(Green * 255.0f)) & 0xff) << 8 | ((uint)(Blue * 255.0f)) & 0xff;
			return (int)result;
		}

		/// <summary>
		/// Function to convert this <see cref="GorgonColor"/> value into an <see cref="int"/> value with a RGBA format.
		/// </summary>
		/// <returns>An <see cref="int"/> representing the color value in ARGB format.</returns>
		/// <remarks>
		/// The format indicates the byte position of each color component in the <see cref="int"/> value.
		/// </remarks>
		public int ToRGBA()
		{
		    uint result = (((uint)(Red * 255.0f)) & 0xff) << 24 | (((uint)(Green * 255.0f)) & 0xff) << 16 |
		                  (((uint)(Blue * 255.0f)) & 0xff) << 8 | ((uint)(Alpha * 255.0f)) & 0xff;
			return (int)result;
		}

		/// <summary>
		/// Function to convert this <see cref="GorgonColor"/> value into an <see cref="int"/> value with a BGRA format.
		/// </summary>
		/// <returns>An <see cref="int"/> representing the color value in ARGB format.</returns>
		/// <remarks>
		/// The format indicates the byte position of each color component in the <see cref="int"/> value.
		/// </remarks>
		public int ToBGRA()
		{
		    uint result = (((uint)(Blue * 255.0f)) & 0xff) << 24 | (((uint)(Green * 255.0f)) & 0xff) << 16 |
		                  (((uint)(Red * 255.0f)) & 0xff) << 8 | ((uint)(Alpha * 255.0f)) & 0xff;
			return (int)result;
		}

		/// <summary>
		/// Function to convert this <see cref="GorgonColor"/> value into an <see cref="int"/> value with a ABGR format.
		/// </summary>
		/// <returns>An <see cref="int"/> representing the color value in ARGB format.</returns>
		/// <remarks>
		/// The format indicates the byte position of each color component in the <see cref="int"/> value.
		/// </remarks>
		public int ToABGR()
		{
		    uint result = (((uint)(Alpha * 255.0f)) & 0xff) << 24 | (((uint)(Blue * 255.0f)) & 0xff) << 16 |
		                  (((uint)(Green * 255.0f)) & 0xff) << 8 | ((uint)(Red * 255.0f)) & 0xff;
			return (int)result;
		}

		/// <summary>
		/// Function to convert this <see cref="GorgonColor"/> into a <see cref="Color"/>.
		/// </summary>
		/// <returns>The <see cref="Color"/> value.</returns>
		public Color ToColor()
		{
			return Color.FromArgb(ToARGB());
		}

		/// <summary>
		/// Function to convert this <see cref="GorgonColor"/> into a <see cref="DX.Vector3"/>.
		/// </summary>
		/// <returns>The <see cref="DX.Vector3"/> value.</returns>
		/// <remarks>
		/// This will map the <see cref="Red"/>, <see cref="Green"/> and <see cref="Blue"/> components to the <see cref="DX.Vector3.X"/>, <see cref="DX.Vector3.Y"/> and <see cref="DX.Vector3.Z"/> values respectively.
		/// </remarks>
		public DX.Vector3 ToVector3()
		{
			return new DX.Vector3(Red, Green, Blue);
		}

		/// <summary>
		/// Function to convert this <see cref="GorgonColor"/> into a <see cref="DX.Vector4"/>.
		/// </summary>
		/// <returns>The <see cref="DX.Vector4"/> value.</returns>
		/// <remarks>
		/// This will map the <see cref="Red"/>, <see cref="Green"/>, <see cref="Blue"/> and <see cref="Alpha"/> components to the <see cref="DX.Vector4.X"/>, <see cref="DX.Vector4.Y"/>, <see cref="DX.Vector4.Z"/> and <see cref="DX.Vector4.W"/> values respectively.
		/// </remarks>
		public DX.Vector4 ToVector4()
		{
			return new DX.Vector4(Red, Green, Blue, Alpha);
		}

		/// <summary>
		/// An operator to add two <see cref="GorgonColor"/> values together.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns>The result of the operator.</returns>
		/// <remarks>
		/// This method does not clamp its output. Values greater than 1 or less than 0 are possible.
		/// </remarks>
		public static GorgonColor operator +(GorgonColor left, GorgonColor right)
		{

			Add(ref left, ref right, out GorgonColor result);

			return result;
		}

		/// <summary>
		/// An operator to subtract two <see cref="GorgonColor"/> values from each other.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns>The result of the operator.</returns>
		/// <remarks>
		/// This method does not clamp its output. Values greater than 1 or less than 0 are possible.
		/// </remarks>
		public static GorgonColor operator -(GorgonColor left, GorgonColor right)
		{

			Subtract(ref left, ref right, out GorgonColor result);

			return result;
		}

		/// <summary>
		/// An operator to multiply two <see cref="GorgonColor"/> values together.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonColor operator *(GorgonColor left, GorgonColor right)
		{

			Multiply(ref left, ref right, out GorgonColor result);

			return result;
		}

		/// <summary>
		/// An operator to multiply a <see cref="GorgonColor"/> and a <see cref="float"/> value.
		/// </summary>
		/// <param name="color">The color to multiply.</param>
		/// <param name="value">The value to multiply by.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonColor operator *(GorgonColor color, float value)
		{

			Multiply(ref color, value, out GorgonColor result);

			return result;
		}

		/// <summary>
		/// An operator to determine if two instances are equal.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool operator ==(GorgonColor left, GorgonColor right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// An operator to determine if two instances are not equal.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
		public static bool operator !=(GorgonColor left, GorgonColor right)
		{
			return !Equals(ref left, ref right);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="GorgonColor"/> to <see cref="Color"/>.
		/// </summary>
		/// <param name="color">The color to convert.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Color(GorgonColor color)
		{
			return color.ToColor();
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="Color"/> to <see cref="GorgonColor"/>.
		/// </summary>
		/// <param name="color">The color to convert.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator GorgonColor(Color color)
		{
			return new GorgonColor(color);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="GorgonColor"/> to <see cref="int"/>.
		/// </summary>
		/// <param name="color">The color to convert.</param>
		/// <returns>The result of the conversion.</returns>
		/// <remarks>
		/// This will return the color in ARGB format.
		/// </remarks>
		public static implicit operator int(GorgonColor color)
		{
			return color.ToARGB();
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="int"/> to <see cref="GorgonColor"/>.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns>The result of the conversion.</returns>
		/// <remarks>
		/// This operator assumes the <paramref name="color"/> is in ARGB format.
		/// </remarks>
		public static implicit operator GorgonColor(int color)
		{
			return new GorgonColor(color);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonColor"/> to <see cref="DX.Vector3"/>.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns>The result of the conversion.</returns>
		/// <remarks>
		/// This will map the <see cref="Red"/>, <see cref="Green"/> and <see cref="Blue"/> components to the <see cref="DX.Vector3.X"/>, <see cref="DX.Vector3.Y"/> and <see cref="DX.Vector3.Z"/> values respectively.
		/// </remarks>
		public static explicit operator DX.Vector3(GorgonColor color)
		{
			return color.ToVector3();
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="DX.Vector3"/> to <see cref="GorgonColor"/>.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns>The result of the conversion.</returns>
		/// <remarks>
		/// This will map the <see cref="DX.Vector3.X"/>, <see cref="DX.Vector3.Y"/> and <see cref="DX.Vector3.Z"/> components to the <see cref="Red"/>, <see cref="Green"/> and <see cref="Blue"/> values respectively. 
		/// The <see cref="Alpha"/> value is set to 1.0f (opaque) for this conversion.
		/// </remarks>
		public static explicit operator GorgonColor(DX.Vector3 color)
		{
			return new GorgonColor(color);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="GorgonColor"/> to <see cref="DX.Vector4"/>.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns>The result of the conversion.</returns>
		/// <remarks>
		/// This will map the <see cref="Red"/>, <see cref="Green"/>, <see cref="Blue"/> and <see cref="Alpha"/> components to the <see cref="DX.Vector4.X"/>, <see cref="DX.Vector4.Y"/>, <see cref="DX.Vector4.Z"/> and <see cref="DX.Vector4.W"/> values respectively.
		/// </remarks>
		public static implicit operator DX.Vector4(GorgonColor color)
		{
			return color.ToVector4();
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="DX.Vector4"/> to <see cref="GorgonColor"/>.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns>The result of the conversion.</returns>
		/// <remarks>
		/// This will map the <see cref="DX.Vector4.X"/>, <see cref="DX.Vector4.Y"/>, <see cref="DX.Vector4.Z"/> and <see cref="DX.Vector4.W"/> components to the <see cref="Red"/>, <see cref="Green"/>, <see cref="Blue"/> and <see cref="Alpha"/> values respectively.
		/// </remarks>
		public static implicit operator GorgonColor(DX.Vector4 color)
		{
			return new GorgonColor(color);
		}
		#endregion
		
		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonColor"/> struct.
		/// </summary>
		/// <param name="r">The red component.</param>
		/// <param name="g">The green component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="a">The alpha component.</param>
		public GorgonColor(float r, float g, float b, float a)
		{
			Alpha = a;
			Red = r;
			Green = g;
			Blue = b;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonColor"/> struct.
		/// </summary>
		/// <param name="r">The red component.</param>
		/// <param name="g">The green component.</param>
		/// <param name="b">The blue component.</param>
		public GorgonColor(float r, float g, float b)
			: this(r, g, b, 1.0f)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonColor"/> struct.
		/// </summary>
		/// <param name="argb">An <see cref="int"/> value representing a color in ARGB format.</param>
		public GorgonColor(int argb)			
		{
			Alpha = ((argb >> 24) & 0xff) / 255.0f;
			Red = ((argb >> 16) & 0xff) / 255.0f;
			Green = ((argb >> 8) & 0xff) / 255.0f;
			Blue = (argb & 0xff) / 255.0f;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonColor"/> struct.
		/// </summary>
		/// <param name="color">The <see cref="Color"/> that will be used to generate this <see cref="GorgonColor"/>.</param>
		public GorgonColor(Color color)
		{
			Alpha = color.A / 255.0f;
			Red = color.R / 255.0f;
			Green = color.G / 255.0f;
			Blue = color.B / 255.0f;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonColor"/> struct.
		/// </summary>
		/// <param name="color">The <see cref="DX.Vector3"/> that will be used to generate this <see cref="GorgonColor"/>.</param>
		/// <remarks>
		/// This will map the <see cref="DX.Vector3.X"/>, <see cref="DX.Vector3.Y"/> and <see cref="DX.Vector3.Z"/> components to the <see cref="Red"/>, <see cref="Green"/> and <see cref="Blue"/> values respectively. 
		/// The <see cref="Alpha"/> value is set to 1.0f (opaque) for this conversion.
		/// </remarks>
		public GorgonColor(DX.Vector3 color)
			: this(color.X, color.Y, color.Z, 1.0f)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonColor"/> struct.
		/// </summary>
		/// <param name="color">The <see cref="DX.Vector4"/> that will be used to generate this <see cref="GorgonColor"/>.</param>
		/// <remarks>
		/// This will map the <see cref="DX.Vector4.X"/>, <see cref="DX.Vector4.Y"/>, <see cref="DX.Vector4.Z"/> and <see cref="DX.Vector4.W"/> components to the <see cref="Red"/>, <see cref="Green"/>, <see cref="Blue"/> and <see cref="Alpha"/> values respectively.
		/// </remarks>
		public GorgonColor(DX.Vector4 color)
			: this(color.X, color.Y, color.Z, color.W)
		{
			
		}


        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonColor"/> struct.
        /// </summary>
        /// <param name="color">The base <see cref="GorgonColor"/>.</param>
        /// <param name="alpha">The alpha value to assign to the color.</param>
        /// <remarks>
		/// This will retrieve the <see cref="Red"/>, <see cref="Green"/>, and <see cref="Blue"/> values from the <paramref name="color"/> parameter.
        /// </remarks>
        public GorgonColor(GorgonColor color, float alpha)
        {
            Red = color.Red;
            Green = color.Green;
            Blue = color.Blue;
            Alpha = alpha;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonColor"/> struct.
		/// </summary>
		/// <param name="info">The information.</param>
		/// <param name="context">The context.</param>
		private GorgonColor(SerializationInfo info, StreamingContext context)
		{
			int colorValue = info.GetInt32("Color");

			Alpha = ((colorValue >> 24) & 0xff) / 255.0f;
			Red = ((colorValue >> 16) & 0xff) / 255.0f;
			Green = ((colorValue >> 8) & 0xff) / 255.0f;
			Blue = (colorValue & 0xff) / 255.0f;
		}
		#endregion

		#region IEquatable<GorgonColor> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonColor other)
		{
			return Equals(ref this, ref other);
		}
		#endregion

		#region IEquatableByRef<GorgonColor> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type by reference.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(ref GorgonColor other)
		{
			return Equals(ref this, ref other);
		}
		#endregion

		#region ISerializable Members
		/// <summary>
		/// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
		/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.</param>
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Color", ToARGB());
		}
		#endregion
	}
}
