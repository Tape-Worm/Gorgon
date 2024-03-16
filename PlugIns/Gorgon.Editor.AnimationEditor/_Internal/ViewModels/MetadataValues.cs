
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: July 12, 2020 7:26:14 PM
// 


using Gorgon.Core;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// The values for a <see cref="KeyValueMetadata"/> item
/// </summary>
public class MetadataValues
{
    /// <summary>
    /// Property to set or return the display name for the metadata value.
    /// </summary>
    public string DisplayName
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the number of decimal places supported by the value.
    /// </summary>
    public int DecimalCount
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the minimum/maximum values for the value.
    /// </summary>
    public GorgonRange<float> MinMax
    {
        get;
        set;
    }
}
