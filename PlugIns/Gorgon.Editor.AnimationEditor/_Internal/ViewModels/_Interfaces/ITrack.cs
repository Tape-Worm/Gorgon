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
// Created: June 14, 2020 10:18:49 PM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A view model for an animation track.
/// </summary>
internal interface ITrack
    : IViewModel, IGorgonNamedObject        
{
    /// <summary>
    /// Property to return the ID for the track registration.
    /// </summary>
    int ID
    {
        get;
    }

    /// <summary>
    /// Property to return the sprite property updated by this track.
    /// </summary>
    TrackSpriteProperty SpriteProperty
    {
        get;
    }

    /// <summary>
    /// Property to return the interpolation mode for the track.
    /// </summary>
    TrackInterpolationMode InterpolationMode
    {
        get;
    }

    /// <summary>
    /// Property to return the types of interpolation supported by the track.
    /// </summary>
    TrackInterpolationMode InterpolationSupport
    {
        get;
    }

    /// <summary>
    /// Property to return the type of key data for this track.
    /// </summary>
    AnimationTrackKeyType KeyType
    {
        get;
    }

    /// <summary>
    /// Property to return the friendly track description used for display.
    /// </summary>
    string Description
    {
        get;
    }

    /// <summary>
    /// Property to return the list of key frames for the track.
    /// </summary>
    IReadOnlyList<IKeyFrame> KeyFrames
    {
        get;
    }

    /// <summary>
    /// Property to return the metadata for the type of key in this track.
    /// </summary>
    KeyValueMetadata KeyMetadata
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to assign the interpolation mode for the track.
    /// </summary>
    IEditorCommand<TrackInterpolationMode> SetInterpolationModeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to assign a list of key frames to the track.
    /// </summary>
    IEditorCommand<SetKeyFramesArgs> SetKeyFramesCommand
    {
        get;
    }
}
