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
// Created: May 6, 2024 7:39:13 PM
//

using Gorgon.Collections;

namespace Gorgon.Core;

/// <summary>
/// Extension methods for span values.
/// </summary>
public static class GorgonSpanExtensions
{
    /// <summary>
    /// Function to provide an enumerator that allows the splitting of a span of characters using the specified separators.
    /// </summary>
    /// <param name="chars">The read only span containing the characters to evaluate.</param>
    /// <param name="separators">The separators to use.</param>
    /// <param name="includeBlanks">[Optional] <b>true</b> to keep empty entries, <b>false</b> to skip.</param>
    /// <returns>The <see cref="GorgonSpanCharEnumerator"/> that will enumerate the span of characters.</returns>
    /// <remarks>
    /// <para>
    /// This provides an enumerator that does not perform any allocations, which will deliver a more performant way to split strings.
    /// </para>
    /// </remarks>
    public static GorgonSpanCharEnumerator Split(this ReadOnlySpan<char> chars, ReadOnlySpan<char> separators, bool includeBlanks = false) => new(chars, separators, includeBlanks);
}
