#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Friday, May 29, 2015 12:48:03 AM
// 
#endregion


using System;

namespace Gorgon.Core
{
    /// <summary>
    /// Extension method to generate hash codes for integer values.
    /// </summary>
    public static class GorgonHashGenerationExtension
    {
        /// <summary>
        /// Function to build upon the hash code from a value.
        /// </summary>
        /// <typeparam name="T">Type of value to generate a hash code from.</typeparam>
        /// <param name="previousHash">The hash code of the previous value.</param>
        /// <param name="item">New item to add to the hash code.</param>
        /// <returns>The hash code for the value.</returns>
        /// <remarks>
        /// <para>
        /// The common hash code generation equation is usually: <c>value1 ^ value2 ^ value3</c>. This leads to issues where hash code values don't have a uniform representation.  This method 
        /// is meant to help provide a more even distribution of values.
        ///  </para>
        /// <para>
        /// To use this method when generating your hash code pick a prime number and call the method with it:
        /// <code language="csharp">
        /// public int override GetHashCode()
        /// {
        ///    // Here we've picked a prime number and are using that as a basis for our hash code, then we chain together calls 
        ///    // in a fluent interface to combine the hash codes of the other members in this class.
        ///    return 281.GenerateHash(aMemberOfYourClass).GenerateHash(anotherMemberOfYourClass);
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        [Obsolete("Use System.HashCode.Combine instead.")]
        public static int GenerateHash<T>(this int previousHash, T item)
        {
            unchecked
            {
                return (397 * previousHash) + item.GetHashCode();       // 397 is our magic prime number.
            }
        }
    }
}
