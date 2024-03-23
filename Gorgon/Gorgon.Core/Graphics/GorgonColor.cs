// 
// Gorgon.
// Copyright (C) 2024 Michael Winsor
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

using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Gorgon.Json;
using Gorgon.Math;
using Gorgon.Properties;
using Newtonsoft.Json;

namespace Gorgon.Graphics;

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
[StructLayout(LayoutKind.Sequential, Pack = 4), JsonConverter(typeof(GorgonColorJsonConverter))]
public readonly struct GorgonColor
    : IEquatable<GorgonColor>, ISerializable
{
    /// <summary>
    /// The size of the value, in bytes.
    /// </summary>
    public static readonly int SizeInBytes = Unsafe.SizeOf<GorgonColor>();

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

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
    /// <returns>
    /// 	<b>true</b> if the specified <see cref="object"/> is equal to this instance; otherwise, <b>false</b>.
    /// </returns>
    public override bool Equals(object obj) => obj is GorgonColor color ? Equals(this, color) : base.Equals(obj);

    /// <summary>
    /// Function to convert a <see cref="Vector3"/> to a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>The converted color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor FromVector3(Vector3 vector) => new(vector.X, vector.Y, vector.Z, 1.0f);

    /// <summary>
    /// Function to convert a <see cref="Vector4"/> to a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>The converted color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor FromVector4(Vector4 vector) => new(vector.X, vector.Y, vector.Z, vector.W);

    /// <summary>
    /// Function to convert a <see cref="Color"/> to a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The converted color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor FromColor(Color color) => new(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

    /// <summary>
    /// Function to convert a ARGB color into a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="argbColor">An <see cref="int"/> representing the color in ARGB format.</param>
    /// <returns>The <see cref="GorgonColor"/> representation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor FromARGB(uint argbColor)
    {
        byte a = (byte)((argbColor >> 24) & 0xff);
        byte r = (byte)((argbColor >> 16) & 0xff);
        byte g = (byte)((argbColor >> 8) & 0xff);
        byte b = (byte)(argbColor & 0xff);

        return new GorgonColor(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
    }

    /// <summary>
    /// Function to convert a ABGR color into a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="argbColor">An <see cref="int"/> representing the color in ARGB format.</param>
    /// <returns>The <see cref="GorgonColor"/> representation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor FromARGB(int argbColor) => FromARGB((uint)argbColor);

    /// <summary>
    /// Function to convert a ABGR color into a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="abgrColor">An <see cref="int"/> representing the color in ABGR format.</param>
    /// <returns>The <see cref="GorgonColor"/> representation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor FromABGR(uint abgrColor)
    {
        byte a = (byte)((abgrColor >> 24) & 0xff);
        byte b = (byte)((abgrColor >> 16) & 0xff);
        byte g = (byte)((abgrColor >> 8) & 0xff);
        byte r = (byte)(abgrColor & 0xff);

        return new GorgonColor(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
    }

    /// <summary>
    /// Function to convert a ABGR color into a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="abgrColor">An <see cref="int"/> representing the color in ABGR format.</param>
    /// <returns>The <see cref="GorgonColor"/> representation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor FromABGR(int abgrColor) => FromABGR((uint)abgrColor);

    /// <summary>
    /// Function to convert a BGRA color into a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="bgraColor">An <see cref="int"/> representing the color in BGRA format.</param>
    /// <returns>The GorgonColor representation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor FromBGRA(uint bgraColor)
    {
        byte b = (byte)((bgraColor >> 24) & 0xff);
        byte g = (byte)((bgraColor >> 16) & 0xff);
        byte r = (byte)((bgraColor >> 8) & 0xff);
        byte a = (byte)(bgraColor & 0xff);

        return new GorgonColor(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
    }

    /// <summary>
    /// Function to convert a BGRA color into a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="bgraColor">An <see cref="int"/> representing the color in BGRA format.</param>
    /// <returns>The GorgonColor representation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor FromBGRA(int bgraColor) => FromBGRA((uint)bgraColor);

    /// <summary>
    /// Function to convert a RGBA color into a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="rgbaColor">An <see cref="int"/> representing the color in RGBA format.</param>
    /// <returns>The <see cref="GorgonColor"/> representation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor FromRGBA(uint rgbaColor)
    {
        byte r = (byte)((rgbaColor >> 24) & 0xff);
        byte g = (byte)((rgbaColor >> 16) & 0xff);
        byte b = (byte)((rgbaColor >> 8) & 0xff);
        byte a = (byte)(rgbaColor & 0xff);

        return new GorgonColor(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
    }

    /// <summary>
    /// Function to convert a RGBA color into a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="rgbaColor">An <see cref="int"/> representing the color in RGBA format.</param>
    /// <returns>The <see cref="GorgonColor"/> representation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor FromRGBA(int rgbaColor) => FromRGBA((uint)rgbaColor);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode() => HashCode.Combine(Red, Green, Blue, Alpha);

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> that represents this instance.
    /// </returns>
    public override string ToString() => string.Format(Resources.GOR_TOSTR_GORGONCOLOR, Red, Green, Blue, Alpha);

    /// <summary>
    /// Function to clamp the color values from 0 to 1.
    /// </summary>
    /// <param name="color">The color to clamp.</param>
    /// <returns>The color with the values clamped to 0 and 1.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor Clamp(GorgonColor color) => new(color.Red.Clamp(0.0f, 1.0f),
                                                              color.Green.Clamp(0.0f, 1.0f),
                                                              color.Blue.Clamp(0.0f, 1.0f),
                                                              color.Alpha.Clamp(0.0f, 1.0f));

    /// <summary>
    /// Function to convert a color value to a color value premultiplied by its alpha value.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The premultiplied alpha color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor PremultiplyAlpha(GorgonColor color) => new(color.Red * color.Alpha, color.Green * color.Alpha, color.Blue * color.Alpha, color.Alpha);

    /// <summary>
    /// Function to convert a color value to a non-premultiplied color value.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The non-premultiplied color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor RemovePremultipliedAlpha(GorgonColor color)
    {
        if (color.Alpha.EqualsEpsilon(0))
        {
            return new(0, 0, 0, 0);
        }

        return new(color.Red / color.Alpha, color.Green / color.Alpha, color.Blue / color.Alpha, color.Alpha);
    }

    /// <summary>
    /// Function to perform linear interpolation between two <see cref="GorgonColor"/> values.
    /// </summary>
    /// <param name="start">The starting <see cref="GorgonColor"/>.</param>
    /// <param name="end">The ending <see cref="GorgonColor"/>.</param>
    /// <param name="weight">Value between 0 and 1.0f to indicate weighting between start and end.</param>
    /// <returns>The new <see cref="GorgonColor"/> representing a color between the <paramin name="start"/> and <paramin name="end"/> values.</returns>
    /// <remarks>
    /// <para>
    /// This will compute a new <see cref="GorgonColor"/> from the <paramin name="start"/> and <paramin name="end"/> parameters based on the <paramin name="weight"/> passed in. For example, if the 
    /// <paramin name="start"/> is Red = 0, Green = 0, Blue = 0, and Alpha = 0, and the <paramin name="end"/> is Red = 1, Green = 0, Blue = 1, and Alpha 0.5f. Then, with a <paramin name="weight"/> of 
    /// 0.5f, the result will be Red = 0.5f, Green = 0, Blue = 0.5f, and an Alpha = 0.25f.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor Lerp(GorgonColor start, GorgonColor end, float weight) => new(start.Red.Lerp(end.Red, weight),
                                                                                           start.Green.Lerp(end.Green, weight),
                                                                                           start.Blue.Lerp(end.Blue, weight),
                                                                                           start.Alpha.Lerp(end.Alpha, weight));

    /// <summary>
    /// Function to add two <see cref="GorgonColor"/> values together.
    /// </summary>
    /// <param name="left">The left color to add.</param>
    /// <param name="right">The right color to add.</param>
    /// <returns>The total of the two colors.</returns>
    /// <remarks>
    /// This method does not clamp its output. Values greater than 1 or less than 0 are possible.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor Add(GorgonColor left, GorgonColor right) => new(left.Red + right.Red,
                                                                            left.Green + right.Green,
                                                                            left.Blue + right.Blue,
                                                                            left.Alpha + right.Alpha);

    /// <summary>
    /// Function to subtract two <see cref="GorgonColor"/> values from each other.
    /// </summary>
    /// <param name="left">The left color to subtract.</param>
    /// <param name="right">The right color to subtract.</param>
    /// <returns>The difference between the two colors.</returns>
    /// <remarks>
    /// This method does not clamp its output. Values greater than 1 or less than 0 are possible.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor Subtract(GorgonColor left, GorgonColor right) => new(left.Red - right.Red,
                                                                                   left.Green - right.Green,
                                                                                   left.Blue - right.Blue,
                                                                                   left.Alpha - right.Alpha);

    /// <summary>
    /// Function to multiply two <see cref="GorgonColor"/> values together.
    /// </summary>
    /// <param name="left">The left color to multiply.</param>
    /// <param name="right">The right color to multiply.</param>
    /// <returns>Product of the two colors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor Multiply(GorgonColor left, GorgonColor right) => new(left.Red * right.Red,
                                                                                   left.Green * right.Green,
                                                                                   left.Blue * right.Blue,
                                                                                   left.Alpha * right.Alpha);

    /// <summary>
    /// Function to multiply a <see cref="GorgonColor"/> by a value.
    /// </summary>
    /// <param name="color">The color to multiply.</param>
    /// <param name="value">The value to multiply.</param>
    /// <returns>Product of the <paramin name="color"/> and the <paramin name="value"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonColor Multiply(GorgonColor color, float value) => new(color.Red * value,
                                                                            color.Green * value,
                                                                            color.Blue * value,
                                                                            color.Alpha * value);

    /// <summary>
    /// Function to convert this <see cref="GorgonColor"/> value into a hexadecimal formatting string.
    /// </summary>
    /// <returns>The color represented as a hexadecimal string.</returns>
    /// <remarks>
    /// <para>
    /// The format of the string will be as follows: AARRGGBB.
    /// </para>
    /// </remarks>
    public string ToHex()
    {
        (int r, int g, int b, int a) = GetIntegerComponents();

        return $"{a:x2}{r:x2}{g:x2}{b:x2}".ToUpperInvariant();
    }

    /// <summary>
    /// Function to convert this <see cref="GorgonColor"/> value into an <see cref="int"/> value with a ARGB format.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>An <see cref="int"/> representing the color value in ARGB format.</returns>
    /// <remarks>
    /// The format indicates the byte position of each color component in the <see cref="int"/> value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToARGB(GorgonColor color)
    {
        (int r, int g, int b, int a) = color.GetIntegerComponents();

        uint result = ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | ((uint)b);
        return (int)result;
    }

    /// <summary>
    /// Function to convert this <see cref="GorgonColor"/> value into an <see cref="int"/> value with a RGBA format.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>An <see cref="int"/> representing the color value in ARGB format.</returns>
    /// <remarks>
    /// <para>
    /// The format indicates the byte position of each color component in the <see cref="int"/> value.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToRGBA(GorgonColor color)
    {
        (int r, int g, int b, int a) = color.GetIntegerComponents();

        uint result = ((uint)r << 24) | ((uint)g << 16) | ((uint)b << 8) | ((uint)a);
        return (int)result;
    }

    /// <summary>
    /// Function to convert this <see cref="GorgonColor"/> value into an <see cref="int"/> value with a BGRA format.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>An <see cref="int"/> representing the color value in ARGB format.</returns>
    /// <remarks>
    /// The format indicates the byte position of each color component in the <see cref="int"/> value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToBGRA(GorgonColor color)
    {
        (int r, int g, int b, int a) = color.GetIntegerComponents();

        uint result = ((uint)b << 24) | ((uint)g << 16) | ((uint)r << 8) | ((uint)a);
        return (int)result;
    }

    /// <summary>
    /// Function to convert this <see cref="GorgonColor"/> value into an <see cref="int"/> value with a ABGR format.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>An <see cref="int"/> representing the color value in ARGB format.</returns>
    /// <remarks>
    /// The format indicates the byte position of each color component in the <see cref="int"/> value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToABGR(GorgonColor color)
    {
        (int r, int g, int b, int a) = color.GetIntegerComponents();

        uint result = ((uint)a << 24) | ((uint)b << 16) | ((uint)g << 8) | ((uint)r);
        return (int)result;
    }

    /// <summary>
    /// Function to convert the sRGB version of the color to a linear color value.
    /// </summary>
    /// <param name="color">The sRGB color to convert.</param>
    /// <returns>The linear color value.</returns>
    public static GorgonColor ToLinear(GorgonColor color)
    {
        float r = color.Red;
        float g = color.Green;
        float b = color.Blue;

        if (r <= 0.04045f)
        {
            r /= 12.92f;
        }
        else
        {
            r = ((r + 0.055f) / 1.055f).Pow(2.4f);
        }

        if (g <= 0.04045f)
        {
            g /= 12.92f;
        }
        else
        {
            g = ((g + 0.055f) / 1.055f).Pow(2.4f);
        }

        if (b <= 0.04045f)
        {
            b /= 12.92f;
        }
        else
        {
            b = ((b + 0.055f) / 1.055f).Pow(2.4f);
        }

        return new(r, g, b, color.Alpha);
    }

    /// <summary>
    /// Function to convert the linear version of the color to a sRGB color value.
    /// </summary>
    /// <param name="color">The sRGB color to convert.</param>
    /// <returns>The linear color value.</returns>
    public static GorgonColor ToSRgb(GorgonColor color)
    {
        const float pow = 1.0f / 2.4f;

        float r = color.Red;
        float g = color.Green;
        float b = color.Blue;

        if (r <= 0.0031308f)
        {
            r *= 12.92f;
        }
        else
        {
            r = r.Pow(pow) * 1.055f - 0.055f;
        }

        if (g <= 0.0031308f)
        {
            g *= 12.92f;
        }
        else
        {
            g = g.Pow(pow) * 1.055f - 0.055f;
        }

        if (b <= 0.0031308f)
        {
            b *= 12.92f;
        }
        else
        {
            b = b.Pow(pow) * 1.055f - 0.055f;
        }

        return new(r, g, b, color.Alpha);
    }

    /// <summary>
    /// Function to determine if two <see cref="GorgonColor"/> values are equal.
    /// </summary>
    /// <param name="left">The left color to compare.</param>
    /// <param name="right">The right color to compare.</param>
    /// <returns><b>true</b> if both colors are the same, or <b>false</b> if not.</returns>
    public static bool Equals(GorgonColor left, GorgonColor right) => (left.Red.EqualsEpsilon(right.Red))
                                          && (left.Green.EqualsEpsilon(right.Green))
                                          && (left.Blue.EqualsEpsilon(right.Blue))
                                          && (left.Alpha.EqualsEpsilon(right.Alpha));

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramin name="other"/> parameter; otherwise, false.
    /// </returns>
    public bool Equals(GorgonColor other) => Equals(this, other);

    /// <summary>
    /// Populates a <see cref="SerializationInfo" /> with the data needed to serialize the target object.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo" /> to populate with data.</param>
    /// <param name="context">The destination (see <see cref="StreamingContext" />) for this serialization.</param>
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) => info.AddValue("Color", ToARGB(this));

    /// <summary>
    /// An operator to add two <see cref="GorgonColor"/> values together.
    /// </summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>The result of the operator.</returns>
    /// <remarks>
    /// This method does not clamp its output. Values greater than 1 or less than 0 are possible.
    /// </remarks>
    public static GorgonColor operator +(GorgonColor left, GorgonColor right) => Add(left, right);

    /// <summary>
    /// An operator to subtract two <see cref="GorgonColor"/> values from each other.
    /// </summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>The result of the operator.</returns>
    /// <remarks>
    /// This method does not clamp its output. Values greater than 1 or less than 0 are possible.
    /// </remarks>
    public static GorgonColor operator -(GorgonColor left, GorgonColor right) => Subtract(left, right);

    /// <summary>
    /// An operator to multiply two <see cref="GorgonColor"/> values together.
    /// </summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>The result of the operator.</returns>
    public static GorgonColor operator *(GorgonColor left, GorgonColor right) => Multiply(left, right);

    /// <summary>
    /// An operator to multiply a <see cref="GorgonColor"/> and a <see cref="float"/> value.
    /// </summary>
    /// <param name="color">The color to multiply.</param>
    /// <param name="value">The value to multiply by.</param>
    /// <returns>The result of the operator.</returns>
    public static GorgonColor operator *(GorgonColor color, float value) => Multiply(color, value);

    /// <summary>
    /// An operator to determine if two instances are equal.
    /// </summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public static bool operator ==(GorgonColor left, GorgonColor right) => Equals(left, right);

    /// <summary>
    /// An operator to determine if two instances are not equal.
    /// </summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
    public static bool operator !=(GorgonColor left, GorgonColor right) => !Equals(left, right);

    /// <summary>
    /// Performs an implicit conversion from <see cref="GorgonColor"/> to <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Color(GorgonColor color) => ToColor(color);

    /// <summary>
    /// Performs an implicit conversion from <see cref="Color"/> to <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator GorgonColor(Color color) => FromColor(color);

    /// <summary>
    /// Performs an implicit conversion from <see cref="GorgonColor"/> to <see cref="int"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The result of the conversion.</returns>
    /// <remarks>
    /// This will return the color in ARGB format.
    /// </remarks>
    public static explicit operator int(GorgonColor color) => ToARGB(color);

    /// <summary>
    /// Performs an implicit conversion from an <see cref="int"/> (ARGB) to <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    /// <remarks>
    /// <para>
    /// This operator assumes the <paramin name="color"/> is in ARGB format.
    /// </para>
    /// </remarks>
    public static explicit operator GorgonColor(int color) => FromARGB(color);

    /// <summary>
    /// Performs an explicit conversion from <see cref="GorgonColor"/> to Vector3.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    /// <remarks>
    /// This will map the <see cref="Red"/>, <see cref="Green"/> and <see cref="Blue"/> components to the Vector3.X, Vector3.Y and Vector3.Z values respectively.
    /// </remarks>
    public static explicit operator Vector3(GorgonColor color) => ToVector3(color);

    /// <summary>
    /// Performs an explicit conversion from Vector3 to <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    /// <remarks>
    /// This will map the Vector3.X, Vector3.Y and Vector3.Z components to the <see cref="Red"/>, <see cref="Green"/> and <see cref="Blue"/> values respectively. 
    /// The <see cref="Alpha"/> value is set to 1.0f (opaque) for this conversion.
    /// </remarks>
    public static explicit operator GorgonColor(Vector3 color) => FromVector3(color);

    /// <summary>
    /// Performs an implicit conversion from <see cref="GorgonColor"/> to Vector4.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    /// <remarks>
    /// This will map the <see cref="Red"/>, <see cref="Green"/>, <see cref="Blue"/> and <see cref="Alpha"/> components to the Vector4.X, Vector4.Y, Vector4.Z and Vector4.W values respectively.
    /// </remarks>
    public static implicit operator Vector4(GorgonColor color) => ToVector4(color);

    /// <summary>
    /// Performs an implicit conversion from Vector4 to <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    /// <remarks>
    /// This will map the Vector4.X, Vector4.Y, Vector4.Z and Vector4.W components to the <see cref="Red"/>, <see cref="Green"/>, <see cref="Blue"/> and <see cref="Alpha"/> values respectively.
    /// </remarks>
    public static implicit operator GorgonColor(Vector4 color) => FromVector4(color);

    /// <summary>
    /// Function to perform an implicit conversion from <see cref="GorgonColor"/> to Vector4.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    /// <remarks>
    /// This will map the <see cref="Red"/>, <see cref="Green"/>, <see cref="Blue"/> and <see cref="Alpha"/> components to the Vector4.X, Vector4.Y, Vector4.Z and Vector4.W values respectively.
    /// </remarks>
    public static Vector4 ToVector4(GorgonColor color) => new(color.Red, color.Green, color.Blue, color.Alpha);

    /// <summary>
    /// Function to perform an explicit conversion from <see cref="GorgonColor"/> to Vector3.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    /// <remarks>
    /// This will map the <see cref="Red"/>, <see cref="Green"/> and <see cref="Blue"/> components to the Vector3.X, Vector3.Y and Vector3.Z values respectively.
    /// </remarks>
    public static Vector3 ToVector3(GorgonColor color) => new(color.Red, color.Green, color.Blue);

    /// <summary>
    /// Function to perform an implicit conversion from <see cref="GorgonColor"/> to <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The result of the conversion.</returns>
    public static Color ToColor(GorgonColor color) => Color.FromArgb(ToARGB(color));

    /// <summary>
    /// Function to apply a gamma value to a color to increase or decrease its intensity.
    /// </summary>
    /// <param name="color">The color to apply the gamma value into.</param>
    /// <param name="gammaValue">The gamma value to apply.</param>
    /// <returns>The adjusted color.</returns>
    public static GorgonColor ApplyGamma(GorgonColor color, float gammaValue) => new(color.Red * 2.0f.Pow(gammaValue), color.Green * 2.0f.Pow(gammaValue), color.Blue * 2.0f.Pow(gammaValue), color.Alpha);

    /// <summary>
    /// Function to deconstruct the color into individual color components.
    /// </summary>
    /// <returns>A tuple containing the color channels as integer values scaled from 0 to 255.</returns>
    public (int R, int G, int B, int A) GetIntegerComponents() => ((int)(Red * 255.0f),
                                                                   (int)(Green * 255.0f),
                                                                   (int)(Blue * 255.0f),
                                                                   (int)(Alpha * 255.0f));

    /// <summary>
    /// Function to deconstruct the color into individual color components.
    /// </summary>
    /// <param name="r">The red component for the color.</param>
    /// <param name="g">The green component for the color.</param>
    /// <param name="b">The blue component for the color.</param>
    /// <param name="a">The alpha component for the color.</param>
    /// <returns>A tuple containing the color channels as integer values scaled from 0 to 255.</returns>
    public void Deconstruct(out float r, out float g, out float b, out float a)
    {
        r = Red;
        g = Green;
        b = Blue;
        a = Alpha;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonColor"/> struct.
    /// </summary>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    /// <param name="a">[Optional] The alpha component.</param>
    public GorgonColor(float r, float g, float b, float a = 1.0f)
    {
        Alpha = a;
        Red = r;
        Green = g;
        Blue = b;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonColor"/> struct.
    /// </summary>
    /// <param name="color">The base <see cref="GorgonColor"/>.</param>
    /// <param name="alpha">The alpha value to assign to the color.</param>
    /// <remarks>
    /// <para>
    /// This will retrieve the <see cref="Red"/>, <see cref="Green"/>, and <see cref="Blue"/> values from the <paramref name="color"/> parameter. 
    /// </para>
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
}
