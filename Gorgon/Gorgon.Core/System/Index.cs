#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: March 11, 2021 10:21:52 PM
// 
#endregion

#if NET48
namespace System
{
    /// <summary>
    /// A placeholder to allow the .NET 4.8 functionality to compile. <b>DO NOT USE!</b>
    /// </summary>
    public struct Index
        : IEquatable<Index>
    {
#pragma warning disable IDE0060 // Remove unused parameter
        /// <summary>
        /// Dummy property.
        /// </summary>
        public int End => int.MaxValue;
        /// <summary>
        /// Dummy property.
        /// </summary>
        public bool IsFromEnd => false;
        /// <summary>
        /// Dummy property.
        /// </summary>
        public int Start => 0;
        /// <summary>
        /// Dummy property.
        /// </summary>
        public int Value => 0;

        /// <summary>
        /// Performs an implicit conversion from <see cref="int"/> to <see cref="Index"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Index(int value) => default;

        /// <summary>
        /// Gets the offset.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>int.</returns>
        public int GetOffset(int length) => 0;

        /// <summary>Initializes a new instance of the <see cref="T:System.Index">Index</see> struct.</summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">if set to <c>true</c> [p2].</param>
        public Index(int p1, bool p2)
        {
        }

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
        public bool Equals(Index other) => true;
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
#endif