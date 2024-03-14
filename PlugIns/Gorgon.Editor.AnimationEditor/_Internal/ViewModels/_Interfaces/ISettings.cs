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
// Created: June 7, 2020 9:27:23 PM
// 
#endregion

using Gorgon.Editor.UI;
using DX = SharpDX;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// The view model used to manipulate the settings for the sprite editor plug in.
/// </summary>
internal interface ISettings
    : ISettingsCategory
{
    /// <summary>
    /// Property to set or return the offset of the splitter between the main view and lower view.
    /// </summary>
    int SplitterOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether to animate the background when no primary sprite is present.
    /// </summary>
    bool AnimateNoPrimarySpriteBackground
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether to use onion skinning for some editors.
    /// </summary>
    bool UseOnionSkinning
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether to create a texture track in an empty animation on primary sprite assignment.
    /// </summary>
    bool AddTextureTrackForPrimarySprite
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the default screen resolution for the animation.
    /// </summary>
    DX.Size2 DefaultResolution
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether a warning will be shown when an animation with unsupported tracks is loaded.
    /// </summary>
    bool WarnUnsupportedTracks
    {
        get;
        set;
    }
}
