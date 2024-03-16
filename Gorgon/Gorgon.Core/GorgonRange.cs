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
// Created: March 15, 2024 6:15:16 PM
//
 
using System.Runtime.InteropServices;
using Gorgon.Properties;
using Newtonsoft.Json;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Gorgon.Core;

/// <summary>
/// A type that represents a range between two values.
/// </summary>
/// <typeparam name="T">The type of numeric value. Must be a value type and implememnt <see cref="INumber{TSelf}"/>.</typeparam>
/// <remarks>
/// <para>
/// This type is used to represent the range from a <see cref="Minimum"/> to a <see cref="Maximum"/> value. It is useful for determining whether a value lies between that <see cref="Minimum"/> and 
/// <see cref="Maximum"/>, or even clamping a value between the <see cref="Minimum"/> and <see cref="Maximum"/>.
/// </para>
/// <para>
/// This type can also be converted, explicitly, to and from the .NET <see cref="Range"/> type. Making conversions like this possible: 
/// <code language="csharp">
/// <![CDATA[GorgonRange<int> myRange = (GorgonRange<int>)(10..20);]]>
/// </code>
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly struct GorgonRange<T>
    : IEquatable<GorgonRange<T>>, IComparable<GorgonRange<T>>
    where T : struct, INumber<T>
{
    /// <summary>
    /// The size of the this value, in bytes.
    /// </summary>
    public static readonly int SizeInBytes = Unsafe.SizeOf<GorgonRange<T>>();

    /// <summary>
    /// An empty range value.
    /// </summary>
    public static readonly GorgonRange<T> Empty = new(default, default);

    /// <summary>
    /// The minimum value in the range.
    /// </summary>
    public readonly T Minimum;

    /// <summary>
    /// The maximum value in the range.
    /// </summary>
    public readonly T Maximum;

    /// <summary>
    /// Property to return the range between the two values.
    /// </summary>
    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public readonly T Range => Maximum - Minimum;

    /// <summary>
    /// Property to return whether the range is empty or not.
    /// </summary>
    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public bool IsEmpty => Maximum.Equals(default) && Minimum.Equals(default);

    /// <summary>
    /// Function to deconstruct this type into its component values.
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    public void Deconstruct(out T min, out T max)
    {
        min = Minimum;
        max = Maximum;
    }

    /// <summary>
    /// Function to expand a <see cref="GorgonRange{T}"/> by a specific amount.
    /// </summary>
    /// <param name="range">A <see cref="GorgonRange{T}"/> to expand.</param>
    /// <param name="amount">The amount to expand the <see cref="GorgonRange{T}"/> by.</param>
    /// <param name="result">A new <see cref="GorgonRange{T}"/> value, increased in size by <paramref name="amount"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Expand(ref readonly GorgonRange<T> range, T amount, out GorgonRange<T> result)
    {
        T min = range.Minimum - amount;
        T max = range.Maximum + amount;

        result = new(min, max);
    }

    /// <summary>
    /// Function to expand a <see cref="GorgonRange{T}"/> by a specific amount.
    /// </summary>
    /// <param name="range">A <see cref="GorgonRange{T}"/> to expand.</param>
    /// <param name="amount">The amount to expand the <see cref="GorgonRange{T}"/> by.</param>
    /// <returns>A new <see cref="GorgonRange{T}"/> value, increased in size by <paramref name="amount"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRange<T> Expand(GorgonRange<T> range, T amount)
    {
        Expand(in range, amount, out GorgonRange<T> result);
        return result;
    }

    /// <summary>
    /// Function to shift the range <see cref="GorgonRange{T}.Minimum"/> and <see cref="GorgonRange{T}.Maximum"/> by a specific amount.
    /// </summary>
    /// <param name="range">A <see cref="GorgonRange{T}"/> to shift.</param>
    /// <param name="amount">The amount to shift the <see cref="GorgonRange{T}"/> extents by.</param>
    /// <param name="result">A new <see cref="GorgonRange{T}"/> value, with its <see cref="GorgonRange{T}.Minimum"/> and <see cref="GorgonRange{T}.Maximum"/> shifted by the <paramref name="amount"/>.</param>
    /// <remarks>
    /// <para>
    /// This will not alter the size of the range, just the <see cref="GorgonRange{T}.Minimum"/> and <see cref="GorgonRange{T}.Maximum"/>.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Shift(ref readonly GorgonRange<T> range, T amount, out GorgonRange<T> result)
    {
        T min = range.Minimum + amount;
        T max = range.Maximum + amount;

        result = new(min, max);
    }

    /// <summary>
    /// Function to shift the range <see cref="GorgonRange{T}.Minimum"/> and <see cref="GorgonRange{T}.Maximum"/> by a specific amount.
    /// </summary>
    /// <param name="range">A <see cref="GorgonRange{T}"/> to shift.</param>
    /// <param name="amount">The amount to shift the <see cref="GorgonRange{T}"/> extents by.</param>
    /// <returns>A new <see cref="GorgonRange{T}"/> value, with its <see cref="GorgonRange{T}.Minimum"/> and <see cref="GorgonRange{T}.Maximum"/> shifted by the <paramref name="amount"/>.</returns>
    /// <remarks>
    /// <para>
    /// This will not alter the size of the range, just the <see cref="GorgonRange{T}.Minimum"/> and <see cref="GorgonRange{T}.Maximum"/>.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRange<T> Shift(GorgonRange<T> range, T amount)
    {
        Shift(in range, amount, out GorgonRange<T> result);
        return result;
    }

    /// <summary>
    /// Function to produce the union of two ranges.
    /// </summary>
    /// <param name="left">The left <see cref="GorgonRange{T}"/> value to join in the union.</param>
    /// <param name="right">The right <see cref="GorgonRange{T}"/> value to join in the union..</param>
    /// <param name="result">A new <see cref="GorgonRange{T}"/> representing the union of both ranges.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Union(ref readonly GorgonRange<T> left, ref readonly GorgonRange<T> right, out GorgonRange<T> result)
    {
        T newMin = left.Minimum < right.Minimum ? left.Minimum : right.Minimum;
        T newMax = left.Maximum > right.Maximum ? left.Maximum : right.Maximum;

        result = new(newMin, newMax);
    }

    /// <summary>
    /// Function to produce the union of two ranges.
    /// </summary>
    /// <param name="left">The left <see cref="GorgonRange{T}"/> value to join in the union.</param>
    /// <param name="right">The right <see cref="GorgonRange{T}"/> value to join in the union..</param>
    /// <returns>A new <see cref="GorgonRange{T}"/> representing the union of both ranges.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRange<T> Union(GorgonRange<T> left, GorgonRange<T> right)
    {
        Union(in left, in right, out GorgonRange<T> result);
        return result;
    }

    /// <summary>
    /// Function to intersect two ranges.
    /// </summary>
    /// <param name="left">The left <see cref="GorgonRange{T}"/> to intersect.</param>
    /// <param name="right">The right <see cref="GorgonRange{T}"/> to intersect.</param>
    /// <param name="result">The result of the intersection.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Intersect(ref readonly GorgonRange<T> left, ref readonly GorgonRange<T> right, out GorgonRange<T> result)
    {
        if (!left.Intersects(right))
        {
            result = GorgonRange<T>.Empty;
            return;
        }

        T newMin = left.Minimum > right.Minimum ? left.Minimum : right.Minimum;
        T newMax = left.Maximum < right.Maximum ? left.Maximum : right.Maximum;

        result = new(newMin, newMax);
    }

    /// <summary>
    /// Function to intersect two ranges.
    /// </summary>
    /// <param name="left">The left <see cref="GorgonRange{T}"/> to intersect.</param>
    /// <param name="right">The right <see cref="GorgonRange{T}"/> to intersect.</param>
    /// <returns>The result of the intersection.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRange<T> Intersect(GorgonRange<T> left, GorgonRange<T> right)
    {
        Intersect(in left, in right, out GorgonRange<T> result);
        return result;
    }

    /// <summary>
    /// Function to return whether the value falls within this <see cref="GorgonRange{T}"/>.
    /// </summary>
    /// <param name="value">Value to evaluate.</param>
    /// <returns><b>true</b> if the value falls into the range, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T value) => value >= Minimum && value <= Maximum;

    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <param name="obj">Another object to compare to.</param>
    /// <returns>
    /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
    /// </returns>
    public override bool Equals(object obj) => obj is GorgonRange<T> range ? Equals(this, range) : base.Equals(obj);

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>
    /// A 32-bit signed integer that is the hash code for this instance.
    /// </returns>
    public override int GetHashCode() => HashCode.Combine(Minimum, Maximum);

    /// <summary>
    /// Returns the fully qualified type name of this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> containing a fully qualified type name.
    /// </returns>
    public override string ToString() => string.Format(Resources.GOR_TOSTR_GORGONRANGE, Minimum, Maximum, Range);

    /// <summary>
    /// Function to compare two instances for equality.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public static bool Equals(GorgonRange<T> left, GorgonRange<T> right) => (left.Minimum.Equals(right.Minimum)) && (left.Maximum.Equals(right.Maximum));

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(GorgonRange<T> other) => Equals(this, other);

    /// <summary>
    /// Compares the current object with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the other parameter.Zero This object is equal to other. Greater than zero This object is greater than other.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(GorgonRange<T> other) => Range.CompareTo(other.Range);

    /// <summary>
    /// Function to determine if two ranges would intersect each others <see cref="Minimum"/> and <see cref="Maximum"/>.
    /// </summary>
    /// <param name="other">The other range to compare.</param>
    /// <returns><b>true</b> if there is an intersection, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Intersects(GorgonRange<T> other) => (Minimum <= other.Maximum) && (Maximum >= other.Minimum);

    /// <summary>
    /// Function to clamp a value between the <see cref="Minimum"/> and <see cref="Maximum"/> values.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Clamp(T value)
    {
        if (value < Minimum)
        {
            return Minimum;
        }

        if (value > Maximum)
        {
            return Maximum;
        }

        return value;
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(GorgonRange<T> left, GorgonRange<T> right) => Equals(left, right);

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(GorgonRange<T> left, GorgonRange<T> right) => !Equals(left, right);

    /// <summary>
    /// Implements the operator >.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if left is greater than right.</returns>
    public static bool operator >(GorgonRange<T> left, GorgonRange<T> right) => left.Range > right.Range;

    /// <summary>
    /// Implements the operator >=.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if left is greater than right.</returns>
    public static bool operator >=(GorgonRange<T> left, GorgonRange<T> right) => left.Range >= right.Range;

    /// <summary>
    /// Implements the operator >.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if left is greater than right.</returns>
    public static bool operator <(GorgonRange<T> left, GorgonRange<T> right) => left.Range < right.Range;

    /// <summary>
    /// Implements the operator >=.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if left is greater than right.</returns>
    public static bool operator <=(GorgonRange<T> left, GorgonRange<T> right) => left.Range <= right.Range;

    /// <summary>
    /// Explicit operator to convert a <see cref="GorgonRange{T}"/> to a <see cref="Range"/> value.
    /// </summary>
    /// <param name="range">The Gorgon range value to convert.</param>
    /// <returns>The range type.</returns>
    public static explicit operator Range(GorgonRange<T> range) => ToRange(range);

    /// <summary>
    /// Explicit operator to convert a <see cref="Range"/> to a <see cref="GorgonRange{T}"/> value.
    /// </summary>
    /// <param name="range">The range value to convert.</param>
    /// <returns>The Gorgon range type.</returns>
    public static explicit operator GorgonRange<T>(Range range) => FromRange(range);

    /// <summary>
    /// Function to convert a <see cref="GorgonRange{T}"/> to a <see cref="Range"/> value.
    /// </summary>
    /// <param name="range">The Gorgon range value to convert.</param>
    /// <returns>The range type.</returns>
    public static Range ToRange(GorgonRange<T> range) => new(int.CreateChecked(range.Minimum), int.CreateChecked(range.Maximum));

    /// <summary>
    /// Explicit operator to convert a <see cref="Range"/> to a <see cref="GorgonRange{T}"/> value.
    /// </summary>
    /// <param name="range">The range value to convert.</param>
    /// <returns>The Gorgon range type.</returns>
    public static GorgonRange<T> FromRange(Range range) => new(!range.Start.IsFromEnd ? T.CreateChecked(range.Start.Value) : T.CreateChecked(range.End.Value),
                                                                !range.End.IsFromEnd ? T.CreateChecked(range.End.Value) : T.CreateChecked(range.Start.Value));

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonRange{T}"/> struct.
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    [JsonConstructor, System.Text.Json.Serialization.JsonConstructor]
    public GorgonRange(T min, T max)
    {
        if (min < max)
        {
            Minimum = min;
            Maximum = max;
        }
        else
        {
            Maximum = min;
            Minimum = max;
        }
    }
}