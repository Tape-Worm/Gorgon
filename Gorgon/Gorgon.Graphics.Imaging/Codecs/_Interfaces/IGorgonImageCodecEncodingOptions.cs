#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: June 28, 2016 8:03:41 PM
// 
#endregion

using Gorgon.Configuration;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// Provides options used when encoding a <see cref="IGorgonImage"/> for persistence.
/// </summary>
public interface IGorgonImageCodecEncodingOptions
{
    /// <summary>
    /// Property to set or return whether all frames in an image array should be persisted.
    /// </summary>
    /// <remarks>
    /// This flag only applies to codecs that support multiple image frames.  For all other codec types, this flag will be ignored.
    /// </remarks>
    bool SaveAllFrames
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the list of options available to the codec.
    /// </summary>
    IGorgonOptionBag Options
    {
        get;
    }
}
