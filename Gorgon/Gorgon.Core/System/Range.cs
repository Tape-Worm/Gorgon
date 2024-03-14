
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
// Created: March 11, 2021 10:33:09 PM
// 


#if NET48
namespace System;


/// <summary>
/// A placeholder to allow .NET 4.8 functionality to compile. <b>DO NOT USE!</b>
/// </summary>
public struct Range
    : IEquatable<Range>
{
#pragma warning disable IDE0060 // Remove unused parameter
    /// <summary>Gets the start.</summary>
    public readonly Index Start => default;

    /// <summary>Gets the end.</summary>
    public readonly Index End => default;

    /// <summary>Gets all.</summary>
    public static Range All => default;

    /// <summary>Ends at.</summary>
    /// <param name="_">The end.</param>
    /// <returns>Range.</returns>
    public static Range EndAt(Index _) => default;

    /// <summary>Starts at.</summary>
    /// <param name="_">The end.</param>
    /// <returns>Range.</returns>
    public static Range StartAt(Index _) => default;

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <span class="keyword">
    ///     <span class="languageSpecificText">
    ///       <span class="cs">true</span>
    ///       <span class="vb">True</span>
    ///       <span class="cpp">true</span>
    ///     </span>
    ///   </span>
    ///   <span class="nu">
    ///     <span class="keyword">true</span> (<span class="keyword">True</span> in Visual Basic)</span> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <span class="keyword"><span class="languageSpecificText"><span class="cs">false</span><span class="vb">False</span><span class="cpp">false</span></span></span><span class="nu"><span class="keyword">false</span> (<span class="keyword">False</span> in Visual Basic)</span>.
    /// </returns>
    public readonly bool Equals(Range other) => true;

    /// <summary>Gets the length of the offset and.</summary>
    /// <param name="length">The length.</param>
    /// <returns>System.ValueTuple&lt;System.Int32, System.Int32&gt;.</returns>
    public readonly (int, int) GetOffsetAndLength(int length) => (0, length);

    /// <summary>Initializes a new instance of the <see cref="T:System.Range">Range</see> struct.</summary>
    /// <param name="p1">The start.</param>
    /// <param name="p2">The end.</param>
    public Range(Index p1, Index p2)
    {
    }
#pragma warning restore IDE0060 // Remove unused parameter
}
#endif
