#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 19, 2017 7:56:32 PM
// 
#endregion

using System.Collections.Generic;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Extension methods for blend state lists.
    /// </summary>
    public static class GorgonBlendStateListExtensions
    {
        /// <summary>
        /// Function to determine if two lists of <see cref="GorgonBlendState"/> objects are equal.
        /// </summary>
        /// <param name="thisState">The list of states to compare.</param>
        /// <param name="otherState">The other list of states to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool ListEquals(this IReadOnlyList<GorgonBlendState> thisState, IReadOnlyList<GorgonBlendState> otherState)
        {
            if (ReferenceEquals(thisState, otherState))
            {
                return true;
            }

            if ((thisState == null)
                || (otherState == null))
            {
                return false;
            }

            if (thisState.Count != otherState.Count)
            {
                return false;
            }

            for (int i = 0; i < thisState.Count; ++i)
            {
                if (!thisState[i].Equals(otherState[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
