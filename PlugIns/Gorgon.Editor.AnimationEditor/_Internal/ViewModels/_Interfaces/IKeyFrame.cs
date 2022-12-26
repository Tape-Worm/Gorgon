#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: June 16, 2020 3:32:00 PM
// 
#endregion

using System.Numerics;
using Gorgon.Animation;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A view model for a key frame value.
/// </summary>
internal interface IKeyFrame
    : IViewModel
{
    /// <summary>
    /// Property to set or return the time index for the key frame.
    /// </summary>
    float Time
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the type of data for this key frame.
    /// </summary>
    AnimationTrackKeyType DataType
    {
        get;
    }

    /// <summary>
    /// Property to set or return a texture value for the key frame.
    /// </summary>
    TextureValue TextureValue
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return one to four floating point values for the key frame.
    /// </summary>
    Vector4 FloatValue
    {
        get;
        set;
    }
}
