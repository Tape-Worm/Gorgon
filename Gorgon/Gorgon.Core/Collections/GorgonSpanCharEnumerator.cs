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
// Created: May 6, 2024 7:56:07 PM
//

namespace Gorgon.Collections;

/// <summary>
/// Initializes a new instance of the <see cref="GorgonSpanCharEnumerator"/> struct.
/// </summary>
/// <param name="chars">The characters to evaluate.</param>
/// <param name="separators">The characters to use as separators.</param>
/// <param name="includeBlanks"><b>true</b> to keep empty entries, <b>false</b> to skip.</param>
public ref struct GorgonSpanCharEnumerator(ReadOnlySpan<char> chars, ReadOnlySpan<char> separators, bool includeBlanks)
{
    // The list of characters to split
    private ReadOnlySpan<char> _chars = chars;
    // The characters used to split the characters.
    private readonly ReadOnlySpan<char> _separators = separators;
    // Flag to determine if blanks should be kept.
    private readonly bool _includeBlanks = includeBlanks;

    /// <summary>
    /// Property to return the current part of the string that we have split on.
    /// </summary>
    public ReadOnlySpan<char> Current
    {
        get;
        private set;
    }

    /// <summary>
    /// Function to retrieve the instance of this enumerator.
    /// </summary>
    /// <returns>This enumerator.</returns>
    public readonly GorgonSpanCharEnumerator GetEnumerator() => this;

    /// <summary>
    /// Function to move to the next part of the string that is split by its separators.
    /// </summary>
    /// <returns><b>true</b> if more entries exist, or <b>false</b> if done.</returns>
    public bool MoveNext()
    {
        ReadOnlySpan<char> current = _chars;

        if (current.IsEmpty)
        {
            return false;
        }

        int index = 0;

        while (index > -1)
        {
            index = current.IndexOfAny(_separators);

            if (index == -1)
            {
                _chars = [];
                Current = current;
                return true;
            }

            if (index < _chars.Length - 1)
            {
                _chars = current[(index + 1)..];
            }
            else
            {
                _chars = [];
            }

            ReadOnlySpan<char> item = current[..index];

            if (item.Length == 0)
            {
                if (!_includeBlanks)
                {
                    current = _chars;
                    continue;
                }
            }

            Current = item;
            return true;
        }

        return false;
    }
}
