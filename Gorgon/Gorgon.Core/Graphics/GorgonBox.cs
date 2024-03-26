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
// Created: Thursday, March 15, 2012 7:34:32 PM
// 

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Json;
using Gorgon.Math;
using Gorgon.Properties;
using Newtonsoft.Json;

namespace Gorgon.Graphics;

/// <summary>
/// A structure with a width, height and depth.
/// </summary>
/// <remarks>
/// <para>
/// This data structure is used to easily pass around 3D coordinates for a cube shape. The box shape can be intersected and unioned. It also provides various methods to determine if another box intersects, or is 
/// contained completely within a box. 
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// This value type merely represents the dimensions of a box, it does not draw a box. 
/// </para>
/// </note>
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 4), JsonConverter(typeof(GorgonBoxJsonConverter))]
public struct GorgonBox
    : IGorgonEquatableByRef<GorgonBox>
{
    /// <summary>
    /// The size of the this value, in bytes.
    /// </summary>
    public static readonly int SizeInBytes = Unsafe.SizeOf<GorgonBox>();

    /// <summary>
    /// An empty box.
    /// </summary>
    public static readonly GorgonBox Empty = new()
    {
        X = 0,
        Y = 0,
        Z = 0,
        Width = 0,
        Height = 0,
        Depth = 0
    };

    /// <summary>
    /// Horizontal position.
    /// </summary>
    public int X;
    /// <summary>
    /// Vertical position
    /// </summary>
    public int Y;
    /// <summary>
    /// Depth position.
    /// </summary>
    public int Z;
    /// <summary>
    /// Width of the box.
    /// </summary>
    public int Width;
    /// <summary>
    /// Height of the box.
    /// </summary>
    public int Height;
    /// <summary>
    /// Depth of the box.
    /// </summary>
    public int Depth;

    /// <summary>
    /// Property to determine if the box is empty.
    /// </summary>
    public readonly bool IsEmpty => Width == 0 && Height == 0 && Depth == 0 && X == 0 && Y == 0 && Z == 0;

    /// <summary>
    /// Property to set or return the left value for the box.
    /// </summary>
    public int Left
    {
        readonly get => X;
        set => X = value;
    }

    /// <summary>
    /// Property to set or return the top value for the box.
    /// </summary>
    public int Top
    {
        readonly get => Y;
        set => Y = value;
    }

    /// <summary>
    /// Property to set or return the front value for the box.
    /// </summary>
    public int Front
    {
        readonly get => Z;
        set => Z = value;
    }

    /// <summary>
    /// Property to set or return the right value of the box.
    /// </summary>
    public int Right
    {
        readonly get => unchecked(Width + X);
        set => Width = unchecked(value - X);
    }

    /// <summary>
    /// Property to set or return the bottom value of the box.
    /// </summary>
    public int Bottom
    {
        readonly get => unchecked(Height + Y);
        set => Height = unchecked(value - Y);
    }

    /// <summary>
    /// Property to set or return the back value of the box.
    /// </summary>
    public int Back
    {
        readonly get => unchecked(Depth + Z);
        set => Depth = unchecked(value - Z);
    }

    /// <summary>
    /// Property to return the top, left and front corner of the box.
    /// </summary>
    public readonly (int x, int y, int z) TopLeftFront => (Left, Top, Front);

    /// <summary>
    /// Property to return the bottom, left and front corner of the box.
    /// </summary>
    public readonly (int x, int y, int z) BottomLeftFront => (Left, Bottom, Front);

    /// <summary>
    /// Property to return the top, right and front corner of the box.
    /// </summary>
    public readonly (int x, int y, int z) TopRightFront => (Right, Top, Front);

    /// <summary>
    /// Property to return the bottom, right and front corner of the box.
    /// </summary>
    public readonly (int x, int y, int z) BottomRightFront => (Right, Bottom, Front);

    /// <summary>
    /// Property to return the top, left and back corner of the box.
    /// </summary>
    public readonly (int x, int y, int z) TopLeftBack => (Left, Top, Back);

    /// <summary>
    /// Property to return the bottom, left and back corner of the box.
    /// </summary>
    public readonly (int x, int y, int z) BottomLeftBack => (Left, Bottom, Back);

    /// <summary>
    /// Property to return the top, right and back corner of the box.
    /// </summary>
    public readonly (int x, int y, int z) TopRightBack => (Right, Top, Back);

    /// <summary>
    /// Property to return the bottom, right and back corner of the box.
    /// </summary>
    public readonly (int x, int y, int z) BottomRightBack => (Right, Bottom, Back);

    /// <summary>
    /// Property to return the center of the box.
    /// </summary>
    public readonly (int x, int y, int z) Center => (Width / 2 + Left, Height / 2 + Top, Depth / 2 + Front);

    /// <summary>
    /// Property to set or return the location of the box.
    /// </summary>
    public (int x, int y, int z) Location
    {
        readonly get => (X, Y, Z);
        set
        {
            X = value.x;
            Y = value.y;
            Z = value.z;
        }
    }

    /// <summary>
    /// Property to set or return the size of the box.
    /// </summary>
    public (int x, int y, int z) Size
    {
        readonly get => (Width, Height, Depth);
        set
        {
            Width = value.x;
            Height = value.y;
            Depth = value.z;
        }
    }

    /// <summary>
    /// Function to deconstruct this type into its components.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="depth">The depth.</param>
    public readonly void Deconstruct(out int x, out int y, out int z, out int width, out int height, out int depth)
    {
        x = X;
        y = Y;
        z = Z;
        width = Width;
        height = Height;
        depth = Depth;
    }

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override readonly string ToString() => string.Format(Resources.GOR_TOSTR_BOX, X, Y, Z, Right, Bottom, Back, Width, Height, Depth);

    /// <summary>
    /// Function to return a box from left, top, front, right, bottom and back coordinates.
    /// </summary>
    /// <param name="left">Left coordinate</param>
    /// <param name="top">Top coordinate.</param>
    /// <param name="front">Front coordinate.</param>
    /// <param name="right">Right coordinate.</param>
    /// <param name="bottom">Bottom coordinate.</param>
    /// <param name="back">Back coordinate.</param>
    /// <returns>A new box with the specified coordinates.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonBox FromLTFRBB(int left, int top, int front, int right, int bottom, int back) => new()
    {
        X = left,
        Y = top,
        Z = front,
        Width = right - left,
        Height = bottom - top,
        Depth = back - front
    };

    /// <summary>
    /// Function to determine the intersection between two <see cref="GorgonBox"/> values.
    /// </summary>
    /// <param name="box1">First box to intersect.</param>
    /// <param name="box2">Second box to intersect.</param>
    /// <param name="result">The resulting intersected box.</param>
    public static void Intersect(ref readonly GorgonBox box1, ref readonly GorgonBox box2, out GorgonBox result)
    {
        int left = box2.Left.Max(box1.Left);
        int top = box2.Top.Max(box1.Top);
        int front = box2.Front.Max(box1.Front);

        int right = box2.Right.Min(box1.Right);
        int bottom = box2.Bottom.Min(box1.Bottom);
        int back = box2.Back.Min(box1.Back);

        if ((right <= left) || (bottom <= top) || (back <= front))
        {
            result = Empty;
            return;
        }

        result = FromLTFRBB(left, top, front, right, bottom, back);
    }

    /// <summary>
    /// Function to determine if this box intersects with another box.
    /// </summary>
    /// <param name="box">The box to compare with.</param>
    /// <returns><b>true</b> if the boxes intersect, <b>false</b> if the boxes do not intersect.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IntersectsWith(ref readonly GorgonBox box) =>
            (box.Left < Right) && (Left < box.Right) &&
            (box.Top < Bottom) && (Top < box.Bottom) &&
            (box.Front < Back) && (Front < box.Back);

    /// <summary>
    /// Function to determine if another <see cref="GorgonBox"/> is completely contained within this box.
    /// </summary>
    /// <param name="box">The box to evaluate.</param>
    /// <returns><b>true</b> if the box is contained within this box, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(ref readonly GorgonBox box) => (box.Left >= Left) && (box.Right <= Right)
                                                              && (box.Top >= Top) && (box.Bottom <= Bottom)
                                                              && (box.Front >= Front) && (box.Back <= Back);

    /// <summary>
    /// Function to determine if a 3D point is contained within this <see cref="GorgonBox"/>.
    /// </summary>
    /// <param name="x">The X coordinate of the point to evaluate.</param>
    /// <param name="y">The Y coordinate of the point to evaluate.</param>
    /// <param name="z">The Z coordinate of the point to evaluate.</param>
    /// <returns><b>true</b> if the point is contained within the box, or <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(int x, int y, int z) => (x >= Left) && (y >= Top) && (z >= Front) && (x <= Right) && (y <= Bottom) && (z <= Back);

    /// <summary>
    /// Function to determine if a 3D point is contained within this <see cref="GorgonBox"/>.
    /// </summary>
    /// <param name="point">The 2D point to evaluate.</param>
    /// <param name="z">The Z coordinate of the point to evaluate.</param>
    /// <returns><b>true</b> if the point is contained within the box, or <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(GorgonPoint point, int z) => Contains(point.X, point.Y, z);

    /// <summary>
    /// Function to determine the union of two <see cref="GorgonBox"/> values.
    /// </summary>
    /// <param name="box1">First box to intersect.</param>
    /// <param name="box2">Second box to intersect.</param>
    /// <param name="result">The resulting intersected box.</param>
    public static void Union(ref readonly GorgonBox box1, ref readonly GorgonBox box2, out GorgonBox result)
    {
        int l = box1.Left.Min(box2.Left);
        int t = box1.Top.Min(box2.Top);
        int f = box1.Front.Min(box2.Front);
        int r = box1.Right.Max(box2.Right);
        int b = box1.Bottom.Max(box2.Bottom);
        int d = box1.Back.Max(box2.Back);

        if ((r < l) || (b < t) || (d < f))
        {
            result = Empty;
            return;
        }

        result = FromLTFRBB(l, t, f, r, b, d);
    }

    /// <summary>
    /// Function to determine the intersection between two <see cref="GorgonBox"/> values.
    /// </summary>
    /// <param name="box1">First box to intersect.</param>
    /// <param name="box2">Second box to intersect.</param>
    /// <returns>The intersected box.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonBox Intersect(GorgonBox box1, GorgonBox box2)
    {
        Intersect(in box1, in box2, out GorgonBox result);
        return result;
    }

    /// <summary>
    /// Function to determine the union of two <see cref="GorgonBox"/> values.
    /// </summary>
    /// <param name="box1">First box to intersect.</param>
    /// <param name="box2">Second box to intersect.</param>
    /// <returns>The resulting intersected box.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonBox Union(GorgonBox box1, GorgonBox box2)
    {
        Union(in box1, in box2, out GorgonBox result);
        return result;
    }

    /// <summary>
    /// Function to return whether two instances are equal or not.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(ref readonly GorgonBox left, ref readonly GorgonBox right) => ((left.X == right.X) && (left.Y == right.Y) && (left.Z == right.Z)
            && (left.Width == right.Width) && (left.Height == right.Height) && (left.Depth == right.Depth));

    /// <summary>
    /// Determines whether the specified <see cref="object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
    /// <returns>
    ///   <b>true</b> if the specified <see cref="object" /> is equal to this instance; otherwise, <b>false</b>.
    /// </returns>
    public override readonly bool Equals(object? obj) => obj is GorgonBox box ? box.Equals(in this) : base.Equals(obj);

    /// <summary>
    /// Operator to determine if 2 instances are equal.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public static bool operator ==(GorgonBox left, GorgonBox right) => Equals(in left, in right);

    /// <summary>
    /// Operator to determine if 2 instances are not equal.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
    public static bool operator !=(GorgonBox left, GorgonBox right) => !Equals(in left, in right);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override readonly int GetHashCode() => HashCode.Combine(Back, Right, Bottom, Left, Top, Front);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the other parameter; otherwise, false.
    /// </returns>
    public readonly bool Equals(GorgonBox other) => Equals(in this, in other);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the other parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(ref readonly GorgonBox other) => Equals(in this, in other);

    /// <summary>
    /// Function to convert a <see cref="GorgonBox"/> to a <see cref="GorgonBoxF"/> value.
    /// </summary>
    /// <param name="other">The box to convert.</param>
    /// <returns>The converted box value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonBoxF ToGorgonBoxF(ref readonly GorgonBox other) => new(other);

    /// <summary>
    /// Function to convert a <see cref="GorgonBox"/> to a <see cref="GorgonRectangle"/>.
    /// </summary>
    /// <param name="box">The box to convert.</param>
    /// <returns>A <see cref="GorgonRectangle"/> containing the same top, left, width and height of the box.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRectangle ToGorgonRectangle(ref readonly GorgonBox box) => new()
    {
        X = box.X,
        Y = box.Y,
        Width = box.Width,
        Height = box.Height
    };

    /// <summary>
    /// Function to expand or shrink a <see cref="GorgonBox"/>.
    /// </summary>
    /// <param name="box">The box to expand or shrink.</param>
    /// <param name="amount">The amount to expand or shrink by.</param>
    /// <param name="result">The expanded/shrunken box.</param>
    /// <remarks>
    /// <para>
    /// If the <paramref name="amount"/> value is negative, the box will shrink in size by the amount specified.
    /// </para>
    /// </remarks>
    public static void Expand(ref readonly GorgonBox box, int amount, out GorgonBox result)
    {
        if (amount == 0)
        {
            result = box;
            return;
        }

        result = new GorgonBox
        {
            X = box.X - amount,
            Y = box.Y - amount,
            Z = box.Z - amount,
            Width = box.Width + amount * 2,
            Height = box.Height + amount * 2,
            Depth = box.Depth + amount * 2
        };
    }

    /// <summary>
    /// Function to expand or shrink a <see cref="GorgonBox"/>.
    /// </summary>
    /// <param name="box">The box to expand or shrink.</param>
    /// <param name="amount">The amount to expand or shrink by.</param>
    /// <returns>The expanded/shrunken box.</returns>
    /// <remarks>
    /// <para>
    /// If the <paramref name="amount"/> value is negative, the box will shrink in size by the amount specified.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonBox Expand(GorgonBox box, int amount)
    {
        Expand(in box, amount, out GorgonBox result);
        return result;
    }

    /// <summary>
    /// Operator to implicitly convert this <see cref="GorgonBox"/> ot a <see cref="GorgonBoxF"/>.
    /// </summary>
    /// <param name="box">The box to convert.</param>
    /// <returns>The converted box value.</returns>    
    public static implicit operator GorgonBoxF(GorgonBox box) => ToGorgonBoxF(in box);

    /// <summary>
    /// Operator to explicitly convert this <see cref="GorgonBox"/> ot a <see cref="GorgonRectangle"/>.
    /// </summary>
    /// <param name="box">The box to convert.</param>
    /// <returns>The converted box value.</returns>
    public static explicit operator GorgonRectangle(GorgonBox box) => ToGorgonRectangle(in box);

    /// <summary>
    /// Function to round the values of a <see cref="GorgonBoxF"/> and convert to a <see cref="GorgonBox"/>.
    /// </summary>
    /// <param name="other">The box to convert.</param>
    /// <param name="result">The converted box value.</param>
    public static void Round(ref readonly GorgonBoxF other, out GorgonBox result) => result = new GorgonBox()
    {
        X = (int)other.X.Round(),
        Y = (int)other.Y.Round(),
        Z = (int)other.Z.Round(),
        Width = (int)other.Width.Round(),
        Height = (int)other.Height.Round(),
        Depth = (int)other.Depth.Round()
    };

    /// <summary>
    /// Function to round the values of a <see cref="GorgonBoxF"/> and convert to a <see cref="GorgonBox"/>.
    /// </summary>
    /// <param name="other">The box to convert.</param>
    /// <returns>The converted box value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonBox Round(GorgonBoxF other)
    {
        Round(in other, out GorgonBox result);
        return result;
    }

    /// <summary>
    /// Function to ceiling the values of a <see cref="GorgonBoxF"/> and convert to a <see cref="GorgonBox"/>.
    /// </summary>
    /// <param name="other">The box to convert.</param>
    /// <param name="result">The converted box value.</param>
    public static void Ceiling(ref readonly GorgonBoxF other, out GorgonBox result) => result = new()
    {
        X = (int)other.X.FastCeiling(),
        Y = (int)other.Y.FastCeiling(),
        Z = (int)other.Z.FastCeiling(),
        Width = (int)other.Width.FastCeiling(),
        Height = (int)other.Height.FastCeiling(),
        Depth = (int)other.Depth.FastCeiling()
    };

    /// <summary>
    /// Function to ceiling the values of a <see cref="GorgonBoxF"/> and convert to a <see cref="GorgonBox"/>.
    /// </summary>
    /// <param name="other">The box to convert.</param>
    /// <returns>The converted box value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonBox Ceiling(GorgonBoxF other)
    {
        Ceiling(in other, out GorgonBox result);
        return result;
    }

    /// <summary>
    /// Function to floor the values of a <see cref="GorgonBoxF"/> and convert to a <see cref="GorgonBox"/>.
    /// </summary>
    /// <param name="other">The box to convert.</param>
    /// <param name="result">The converted box value.</param>
    public static void Floor(ref readonly GorgonBoxF other, out GorgonBox result) => result = new()
    {
        X = (int)other.X.FastFloor(),
        Y = (int)other.Y.FastFloor(),
        Z = (int)other.Z.FastFloor(),
        Width = (int)other.Width.FastFloor(),
        Height = (int)other.Height.FastFloor(),
        Depth = (int)other.Depth.FastFloor()
    };

    /// <summary>
    /// Function to floor the values of a <see cref="GorgonBoxF"/> and convert to a <see cref="GorgonBox"/>.
    /// </summary>
    /// <param name="other">The box to convert.</param>
    /// <returns>The converted box value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonBox Floor(GorgonBoxF other)
    {
        Floor(in other, out GorgonBox result);
        return result;
    }

    /// <summary>
    /// Initializes an instance of the <see cref="GorgonBox"/> value.
    /// </summary>
    /// <param name="rect">The rectangle defining the horizontal and vertical position, as well as the width and height of the box.</param>
    /// <param name="z">The depth position of the box.</param>
    /// <param name="depth">The depth of the box.</param>
    public GorgonBox(GorgonRectangle rect, int z, int depth)
    {
        X = rect.X;
        Y = rect.Y;
        Z = z;
        Width = rect.Width;
        Height = rect.Height;
        Depth = depth;
    }

    /// <summary>
    /// Initializes an instance of the <see cref="GorgonBox"/> value.
    /// </summary>
    /// <param name="x">The horizontal position of the box.</param>
    /// <param name="y">The vertical position of the box.</param>
    /// <param name="z">The depth position of the box.</param>
    /// <param name="width">The width of the box.</param>
    /// <param name="height">The height of the box.</param>
    /// <param name="depth">The depth of the box.</param>
    public GorgonBox(int x, int y, int z, int width, int height, int depth)
    {
        X = x;
        Y = y;
        Z = z;
        Width = width;
        Height = height;
        Depth = depth;
    }
}
