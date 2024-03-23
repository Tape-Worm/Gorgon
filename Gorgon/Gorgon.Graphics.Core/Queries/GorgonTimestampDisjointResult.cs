
// 
// Gorgon
// Copyright (C) 2021 Michael Winsor
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
// Created: January 16, 2021 5:59:25 PM
// 

using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The result from a <see cref="GorgonTimestampDisjointQuery"/>
/// </summary>    
public readonly struct GorgonTimestampDisjointResult
    : IGorgonEquatableByRef<GorgonTimestampDisjointResult>
{
    /// <summary>
    /// The frequency that the counter increments at, in Hz.
    /// </summary>
    public readonly ulong Frequency;

    /// <summary>
    /// Flag to indicate that the timestamp is disjointed.
    /// </summary>
    /// <remarks>
    /// If this is <b>true</b>, something occurred in between the <see cref="GorgonTimestampDisjointQuery"/> Begin and End calls that caused the timestamp counter to become discontinuous or 
    /// disjoint, such as unplugging the AC cord on a laptop, overheating, or throttling up/down due to laptop savings events. The timestamp returned for a timestamp query is only reliable if 
    /// Disjoint is <b>false</b>.
    /// </remarks>
    public readonly bool IsDisjoint;

    /// <summary>Returns a hash code for this instance.</summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode() => HashCode.Combine(Frequency, IsDisjoint);

    /// <summary>Returns a <see cref="string" /> that represents this instance.</summary>
    /// <returns>A <see cref="string" /> that represents this instance.</returns>
    public override string ToString() => string.Format(Resources.GORGFX_TOSTR_TIMESTAMP_DISJOINT, Frequency, IsDisjoint);

    /// <summary>Function to compare this instance with another.</summary>
    /// <param name="other">The other instance to use for comparison.</param>
    /// <returns>
    ///   <b>true</b> if equal, <b>false</b> if not.</returns>
    public bool Equals(ref readonly GorgonTimestampDisjointResult other) => (other.Frequency == Frequency) && (other.IsDisjoint == IsDisjoint);

    /// <summary>Function to compare this instance with another.</summary>
    /// <param name="other">The other instance to use for comparison.</param>
    /// <returns>
    ///   <b>true</b> if equal, <b>false</b> if not.</returns>
    public bool Equals(GorgonTimestampDisjointResult other) => Equals(in other);

    /// <summary>Determines whether the specified <see cref="object" /> is equal to this instance.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object obj) => (obj is GorgonTimestampDisjointResult result) ? result.Equals(in this) : base.Equals(obj);

    /// <summary>Initializes a new instance of the <see cref="GorgonTimestampDisjointResult" /> struct.</summary>
    /// <param name="timestamp">The timestamp to evaluate.</param>
    internal GorgonTimestampDisjointResult(D3D11.QueryDataTimestampDisjoint timestamp)
    {
        Frequency = (ulong)timestamp.Frequency;
        IsDisjoint = timestamp.Disjoint;
    }
}
