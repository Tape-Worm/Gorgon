﻿#region MIT
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
// Created: January 16, 2020 4:04:14 PM
// 
#endregion

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A data structure used to copy keyframe data.
/// </summary>
internal class KeyFrameCopyMoveData : IKeyFrameCopyMoveData
{
    /// <summary>
    /// Property to set or return the index of the key that will receive the copied data.
    /// </summary>
    public int DestinationKeyIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the type of operation.
    /// </summary>
    public CopyMoveOperation Operation
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the keyframes to copy.
    /// </summary>
    public IReadOnlyList<TrackKeySelection> KeyFrames
    {
        get;
        set;
    }
}
