
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Tuesday, April 17, 2012 9:15:18 AM
// 


using Gorgon.Core;
using Gorgon.Graphics.Fonts.Properties;

namespace Gorgon.Graphics.Fonts;

/// <summary>
/// A kerning pair value
/// </summary>
/// <remarks>Kerning pairs are used to offset a pair of characters when they are next to each other.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonKerningPair"/> struct
/// </remarks>
/// <param name="leftChar">The left char.</param>
/// <param name="rightChar">The right char.</param>
public readonly struct GorgonKerningPair(char leftChar, char rightChar)
        : IGorgonEquatableByRef<GorgonKerningPair>
{

    /// <summary>
    /// Left character.
    /// </summary>
    public readonly char LeftCharacter = leftChar;
    /// <summary>
    /// Right character.
    /// </summary>
    public readonly char RightCharacter = rightChar;



    /// <summary>
    /// Function to determine if 2 kerning pairs are the same.
    /// </summary>
    /// <param name="left">Left kerning pair to compare.</param>
    /// <param name="right">Right kerning pair to compare.</param>
    /// <returns><b>true</b> if the same, <b>false</b> if not.</returns>
    public static bool Equals(in GorgonKerningPair left, in GorgonKerningPair right) => ((left.LeftCharacter == right.LeftCharacter) && (left.RightCharacter == right.RightCharacter));

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
    /// <returns>
    ///   <b>true</b> if the specified <see cref="object"/> is equal to this instance; otherwise, <b>false</b>.
    /// </returns>
    public override bool Equals(object obj) => obj is GorgonKerningPair kernPair ? kernPair.Equals(this) : base.Equals(obj);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode() => HashCode.Combine(LeftCharacter, RightCharacter);

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> that represents this instance.
    /// </returns>
    public override string ToString() => string.Format(Resources.GORGFX_TOSTR_FONT_KERNING_PAIR, LeftCharacter, RightCharacter);

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator ==(in GorgonKerningPair left, in GorgonKerningPair right) => Equals(in left, in right);

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator !=(in GorgonKerningPair left, in GorgonKerningPair right) => !Equals(in left, in right);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramin name="other"/> parameter; otherwise, false.
    /// </returns>
    public bool Equals(GorgonKerningPair other) => Equals(in this, in other);

    /// <summary>
    /// Function to compare this instance with another.
    /// </summary>
    /// <param name="other">The other instance to use for comparison.</param>
    /// <returns>
    ///   <b>true</b> if equal, <b>false</b> if not.</returns>
    public bool Equals(in GorgonKerningPair other) => Equals(in this, in other);




}
