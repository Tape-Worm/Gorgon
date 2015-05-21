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
//
// Portions of this code were pulled from the DirectXPackedVector.inl and DirectXPackedVector.h from the Direct X APIs. 
// All Copyrights for those portions belong to Microsoft.
#endregion

using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Core.Extensions;
using Gorgon.Graphics.Properties;
using SharpDX;
using Vector4 = SlimMath.Vector4;
using Vector3 = SlimMath.Vector3;
using Color = System.Drawing.Color;

namespace Gorgon.Graphics
{
	/// <summary>
	/// 4 component (Red, Green, Blue, and Alpha) color value.
	/// </summary>
	/// <remarks>This type is immutable.</remarks>
	[StructLayout(LayoutKind.Sequential)]
	public struct GorgonColor
		: IEquatableByRef<GorgonColor>
	{
		#region Variables.
		/// <summary>
		/// Red color channel.
		/// </summary>
		public readonly float Red;
		/// <summary>
		/// Green color channel.
		/// </summary>
        public readonly float Green;
		/// <summary>
		/// Blue color channel.
		/// </summary>
        public readonly float Blue;
		/// <summary>
		/// Alpha channel.
		/// </summary>
        public readonly float Alpha;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a SharpDX color4 type.
		/// </summary>
		internal Color4 SharpDXColor4
		{
			get
			{
				return new Color4(Red, Green, Blue, Alpha);
			}
		}

		/// <summary>
		/// Property to return a SharpDX color3 type.
		/// </summary>
		internal Color3 SharpDXColor3
		{
			get
			{
				return new Color3(Red, Green, Blue);
			}
		}

        /// <summary>
        /// A completely transparent color.
        /// </summary>
        public static GorgonColor Transparent
        {
            get
            {
                return new GorgonColor(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// White.
        /// </summary>
        public static GorgonColor White
        {
            get
            {
                return new GorgonColor(1, 1, 1, 1);
            }
        }

        /// <summary>
        /// Black.
        /// </summary>
        public static GorgonColor Black
        {
            get
            {
                return new GorgonColor(0, 0, 0, 1);
            }
        }
		#endregion

		#region Methods.
		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
		    if (obj is GorgonColor)
		    {
		        return Equals((GorgonColor)obj);
		    }

		    return base.Equals(obj);
		}

		/// <summary>
		/// Function to convert a ABGR (GDI+) color into a GorgonColor.
		/// </summary>
		/// <param name="abgrColor">GDI+ ABGR packed pixel format.</param>
		/// <returns>The GorgonColor representation.</returns>
		public static GorgonColor FromABGR(int abgrColor)
		{
			GorgonColor channels = abgrColor;

			return new GorgonColor(channels.Blue, channels.Green, channels.Red, channels.Alpha);
		}

		/// <summary>
		/// Function to convert a BGRA (GDI+) color into a GorgonColor.
		/// </summary>
		/// <param name="bgraColor">GDI+ BGRA packed pixel format.</param>
		/// <returns>The GorgonColor representation.</returns>
		public static GorgonColor FromBGRA(int bgraColor)
		{
			GorgonColor channels = bgraColor;

			return new GorgonColor(channels.Green, channels.Red, channels.Alpha, channels.Blue);
		}

		/// <summary>
		/// Function to convert a RGBA color into a GorgonColor.
		/// </summary>
		/// <param name="rgbaColor">RGBA packed pixel format.</param>
		/// <returns>The GorgonColor representation.</returns>
		/// <remarks>By default, GorgonColor considers a packed int as ARGB.</remarks>
		public static GorgonColor FromRGBA(int rgbaColor)
		{
			GorgonColor channels = rgbaColor;

			return new GorgonColor(channels.Alpha, channels.Red, channels.Green, channels.Blue);
		}		

		/// <summary>
		/// Function to compare two colors for equality.
		/// </summary>
		/// <param name="left">Left color to compare.</param>
		/// <param name="right">Right color to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
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
			unchecked
			{
				return 281.GenerateHash(Red).GenerateHash(Green).GenerateHash(Blue).GenerateHash(Alpha);
			}
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
		    return string.Format(Resources.GORGFX_COLOR_TOSTR, Red, Green, Blue, Alpha);
		}

        /// <summary>
        /// Function to apply an alpha value to the specified color.
        /// </summary>
        /// <param name="color">Color to update.</param>
        /// <param name="alpha">Alpha value to set.</param>
        /// <param name="result">The resulting updated color.</param>
        /// <returns>A new color instance with the same RGB values but a modified alpha component.</returns>
        public static void SetAlpha(ref GorgonColor color, float alpha, out GorgonColor result)
        {
            result = new GorgonColor(color, alpha);
        }

		/// <summary>
		/// Function to perform linear interpolation between two colors.
		/// </summary>
		/// <param name="start">Starting color.</param>
		/// <param name="end">Ending color.</param>
		/// <param name="weight">Value between 0 and 1.0f to indicate weight.</param>
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
		/// Function to perform linear interpolation between two colors.
		/// </summary>
		/// <param name="start">Starting color.</param>
		/// <param name="end">Ending color.</param>
		/// <param name="weight">Value between 0 and 1.0f to indicate weight.</param>
		/// <param name="outColor">The resulting color.</param>
		public static void Lerp(ref GorgonColor start, ref GorgonColor end, float weight, out GorgonColor outColor)
		{
            outColor = new GorgonColor(start.Red + ((end.Red - start.Red) * weight),
                start.Green + ((end.Green - start.Green) * weight),
                start.Blue + ((end.Blue - start.Blue) * weight),
                start.Alpha + ((end.Alpha - start.Alpha) * weight));
		}

		/// <summary>
		/// Function to add two colors together.
		/// </summary>
		/// <param name="left">Left color to add.</param>
		/// <param name="right">Right color to add.</param>
		/// <param name="outColor">Total of two colors.</param>
		public static void Add(ref GorgonColor left, ref GorgonColor right, out GorgonColor outColor)
		{
            outColor = new GorgonColor(left.Red + right.Red,
                                        left.Green + right.Green,
                                        left.Blue + right.Blue,
                                        left.Alpha + right.Alpha);
		}

		/// <summary>
		/// Function to subtract two colors.
		/// </summary>
		/// <param name="left">Left color to subtract.</param>
		/// <param name="right">Right color to subtract.</param>
		/// <param name="outColor">Difference between the two colors.</param>
		public static void Subtract(ref GorgonColor left, ref GorgonColor right, out GorgonColor outColor)
		{
            outColor = new GorgonColor(left.Red - right.Red,
                                        left.Green - right.Green,
                                        left.Blue - right.Blue,
                                        left.Alpha - right.Alpha);
        }

		/// <summary>
		/// Function to multiply two colors.
		/// </summary>
		/// <param name="left">Left color to multiply.</param>
		/// <param name="right">Right color to multiply.</param>
		/// <param name="outColor">Product of the two colors.</param>
		public static void Multiply(ref GorgonColor left, ref GorgonColor right, out GorgonColor outColor)
		{
            outColor = new GorgonColor(left.Red * right.Red,
                                        left.Green * right.Green,
                                        left.Blue * right.Blue,
                                        left.Alpha * right.Alpha);
		}

		/// <summary>
		/// Function to multiply a color by a value.
		/// </summary>
		/// <param name="color">Color to multiply.</param>
		/// <param name="value">Value to multiply.</param>
		/// <param name="outColor">Product of the color and the value.</param>
		public static void Multiply(ref GorgonColor color, float value, out GorgonColor outColor)
		{
		    outColor = new GorgonColor(color.Red * value,
		                               color.Green * value,
		                               color.Blue * value,
		                               color.Alpha * value);
		}

		/// <summary>
		/// Function to return a packed color value.
		/// </summary>
		/// <returns>The packed color value.</returns>
		public int ToARGB()
		{
		    uint result = (((uint)(Alpha * 255.0f)) & 0xff) << 24 | (((uint)(Red * 255.0f)) & 0xff) << 16 |
		                  (((uint)(Green * 255.0f)) & 0xff) << 8 | ((uint)(Blue * 255.0f)) & 0xff;
			return (int)result;
		}

		/// <summary>
		/// Function to return a packed color value.
		/// </summary>
		/// <returns>The packed color value.</returns>
		public int ToRGBA()
		{
		    uint result = (((uint)(Red * 255.0f)) & 0xff) << 24 | (((uint)(Green * 255.0f)) & 0xff) << 16 |
		                  (((uint)(Blue * 255.0f)) & 0xff) << 8 | ((uint)(Alpha * 255.0f)) & 0xff;
			return (int)result;
		}

		/// <summary>
		/// Function to return a packed color value in BGRA (GDI+) packed pixel format.
		/// </summary>
		/// <returns>The packed color value.</returns>
		public int ToBGRA()
		{
		    uint result = (((uint)(Blue * 255.0f)) & 0xff) << 24 | (((uint)(Green * 255.0f)) & 0xff) << 16 |
		                  (((uint)(Red * 255.0f)) & 0xff) << 8 | ((uint)(Alpha * 255.0f)) & 0xff;
			return (int)result;
		}

		/// <summary>
		/// Function to return a packed color value in ABGR (GDI+) packed pixel format.
		/// </summary>
		/// <returns>The packed color value.</returns>
		public int ToABGR()
		{
		    uint result = (((uint)(Alpha * 255.0f)) & 0xff) << 24 | (((uint)(Blue * 255.0f)) & 0xff) << 16 |
		                  (((uint)(Green * 255.0f)) & 0xff) << 8 | ((uint)(Red * 255.0f)) & 0xff;
			return (int)result;
		}

		/// <summary>
		/// Function to convert this color into a .NET drawing color.
		/// </summary>
		/// <returns>A .NET drawing color.</returns>
		public Color ToColor()
		{
			return Color.FromArgb(ToARGB());
		}

		/// <summary>
		/// Function to convert this color into a 3D vector.
		/// </summary>
		/// <returns>The 3D vector.</returns>
		/// <remarks>This will map the R, G and B components to X, Y and Z respectively.</remarks>
		public Vector3 ToVector3()
		{
			return new Vector3(Red, Green, Blue);
		}

		/// <summary>
		/// Function to convert this color into a 4D vector.
		/// </summary>
		/// <returns>The 4D vector.</returns>
		/// <remarks>This will map the R, G, B and A components to X, Y, Z and W respectively.</remarks>
		public Vector4 ToVector4()
		{
			return new Vector4(Red, Green, Blue, Alpha);
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonColor operator +(GorgonColor left, GorgonColor right)
		{
			GorgonColor result;

			Add(ref left, ref right, out result);

			return result;
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonColor operator -(GorgonColor left, GorgonColor right)
		{
			GorgonColor result;

			Subtract(ref left, ref right, out result);

			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonColor operator *(GorgonColor left, GorgonColor right)
		{
			GorgonColor result;

			Multiply(ref left, ref right, out result);

			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="color">The color to multiply.</param>
		/// <param name="value">The value to multiply by.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonColor operator *(GorgonColor color, float value)
		{
			GorgonColor result;

			Multiply(ref color, value, out result);

			return result;
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left value.</param>
		/// <param name="right">The right value.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(GorgonColor left, GorgonColor right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(GorgonColor left, GorgonColor right)
		{
			return !Equals(ref left, ref right);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="Gorgon.Graphics.GorgonColor"/> to <see cref="System.Drawing.Color"/>.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Color(GorgonColor color)
		{
			return color.ToColor();
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="System.Drawing.Color"/> to <see cref="Gorgon.Graphics.GorgonColor"/>.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator GorgonColor(Color color)
		{
			return new GorgonColor(color);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="Gorgon.Graphics.GorgonColor"/> to <see cref="System.Int32"/>.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator int(GorgonColor color)
		{
			return color.ToARGB();
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="Gorgon.Graphics.GorgonColor"/>.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator GorgonColor(int color)
		{
			return new GorgonColor(color);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="Gorgon.Graphics.GorgonColor"/> to <see cref="SlimMath.Vector3"/>.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator Vector3(GorgonColor color)
		{
			return color.ToVector3();
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="SlimMath.Vector3"/> to <see cref="Gorgon.Graphics.GorgonColor"/>.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator GorgonColor(Vector3 color)
		{
			return new GorgonColor(color);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="Gorgon.Graphics.GorgonColor"/> to <see cref="SlimMath.Vector4"/>.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Vector4(GorgonColor color)
		{
			return color.ToVector4();
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="SlimMath.Vector4"/> to <see cref="Gorgon.Graphics.GorgonColor"/>.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator GorgonColor(Vector4 color)
		{
			return new GorgonColor(color);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonColor"/> struct.
		/// </summary>
		/// <param name="color">The SharpDX color to convert.</param>
		internal GorgonColor(Color3 color)
			: this(color.Red, color.Green, color.Blue, 1.0f)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonColor"/> struct.
		/// </summary>
		/// <param name="color">The SharpDX color to convert.</param>
		internal GorgonColor(Color4 color)
			: this(color.Red, color.Green, color.Blue, color.Alpha)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonColor"/> struct.
		/// </summary>
		/// <param name="r">Red component.</param>
		/// <param name="g">Green component.</param>
		/// <param name="b">Blue component.</param>
		/// <param name="a">Alpha component.</param>
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
		/// <param name="r">Red component.</param>
		/// <param name="g">Green component.</param>
		/// <param name="b">Blue component.</param>
		public GorgonColor(float r, float g, float b)
			: this(r, g, b, 1.0f)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonColor"/> struct.
		/// </summary>
		/// <param name="argb">Packed color components..</param>
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
		/// <param name="color">The .NET color value to convert.</param>
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
		/// <param name="color">The 3D vector to convert to a color.</param>
		public GorgonColor(Vector3 color)
			: this(color.X, color.Y, color.Z, 1.0f)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonColor"/> struct.
		/// </summary>
		/// <param name="color">The 3D vector to convert to a color.</param>
		public GorgonColor(Vector4 color)
			: this(color.X, color.Y, color.Z, color.W)
		{}


        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonColor"/> struct.
        /// </summary>
        /// <param name="color">The base RGB color.</param>
        /// <param name="alpha">The alpha value to assign to the color.</param>
        public GorgonColor(GorgonColor color, float alpha)
        {
            Red = color.Red;
            Green = color.Green;
            Blue = color.Blue;
            Alpha = alpha;
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
	}
}
