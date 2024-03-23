
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
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
// Created: March 14, 2019 11:39:34 AM
// 

using System.Collections.ObjectModel;
using System.Numerics;
using Gorgon.Animation;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// The view model for animation content
/// </summary>
internal interface IAnimationContent
    : IVisualEditorContent, IUndoHandler, ISpriteLoader
{
    /// <summary>
    /// Property to return the add track view model.
    /// </summary>
    IAddTrack AddTrack
    {
        get;
    }

    /// <summary>
    /// Property to set or return the starting position of the primary sprite.
    /// </summary>
    Vector2 PrimarySpriteStartPosition
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the animation properties view model.
    /// </summary>
    IProperties Properties
    {
        get;
    }

    /// <summary>
    /// Property to return the key editor context view model.
    /// </summary>
    IKeyEditorContext KeyEditor
    {
        get;
    }

    /// <summary>
    /// Property to return the maximum number of keys for the animation.
    /// </summary>
    int MaxKeyCount
    {
        get;
    }

    /// <summary>
    /// Property to return the settings view model for the plug in.
    /// </summary>
    ISettings Settings
    {
        get;
    }

    /// <summary>
    /// Property to return the primary sprite for the animation.
    /// </summary>
    GorgonSprite PrimarySprite
    {
        get;
    }

    /// <summary>
    /// Property to return the sprite used in the animation preview (a copy of <see cref="PrimarySprite"/>).
    /// </summary>
    GorgonSprite WorkingSprite
    {
        get;
    }

    /// <summary>
    /// Property to return the current state of the animation.
    /// </summary>
    AnimationState State
    {
        get;
    }

    /// <summary>
    /// Property to return the key time for an animation preview.
    /// </summary>
    float PreviewKeyTime
    {
        get;
    }

    /// <summary>
    /// Property to return the length of the animation, in seconds.
    /// </summary>
    float Length
    {
        get;
    }

    /// <summary>
    /// Property to return the frames per second of the animation.
    /// </summary>
    float Fps
    {
        get;
    }

    /// <summary>
    /// Property to return whether any tracks are assigned.
    /// </summary>
    bool HasTracks
    {
        get;
    }

    /// <summary>
    /// Property to set or return whether the animation will loop back to the beginning when it ends.
    /// </summary>
    bool IsLooping
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the background image.
    /// </summary>
    GorgonTexture2DView BackgroundImage
    {
        get;
    }

    /// <summary>
    /// Property to return the list of tracks to animate data on a sprite.
    /// </summary>
    ObservableCollection<ITrack> Tracks
    {
        get;
    }

    /// <summary>
    /// Property to return the list of tracks and keys that were selected.
    /// </summary>
    IReadOnlyList<TrackKeySelection> Selected
    {
        get;
    }

    /// <summary>
    /// Property to return the command that will activate the add track interface.
    /// </summary>
    IEditorCommand<object> ShowAddTrackCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command that will activate the animation properties interface.
    /// </summary>
    IEditorCommand<object> ShowAnimationPropertiesCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to delete all selected tracks.
    /// </summary>
    IEditorCommand<object> DeleteTrackCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to clear the tracks and keys from the animation.
    /// </summary>
    IEditorCommand<object> ClearAnimationCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the key frame editor.
    /// </summary>
    IEditorCommand<object> ActivateKeyEditorCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to update the animation during preview.
    /// </summary>
    IEditorCommand<object> UpdateAnimationPreviewCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to start playing the animation.
    /// </summary>
    IEditorCommand<object> PlayAnimationCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to stop playing the animation.
    /// </summary>
    IEditorCommand<object> StopAnimationCommand
    {
        get;
    }

    /// <summary>
    /// Property to return a specific key frame for a track index/key index.
    /// </summary>
    IEditorCommand<GetTrackKeyArgs> GetTrackKeyCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used update the lists of selections.
    /// </summary>
    IEditorCommand<IReadOnlyList<(int trackIndex, IReadOnlyList<int> keyIndices)>> SelectTrackAndKeysCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to load a background image.
    /// </summary>
    IEditorAsyncCommand<object> LoadBackgroundImageCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to clear the background image.
    /// </summary>
    IEditorCommand<object> ClearBackgroundImageCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to select the first key.
    /// </summary>
    IEditorCommand<object> FirstKeyCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to select the last key.
    /// </summary>
    IEditorCommand<object> LastKeyCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to select the previous key.
    /// </summary>
    IEditorCommand<object> PrevKeyCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to select the next key.
    /// </summary>
    IEditorCommand<object> NextKeyCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to create a new animation.
    /// </summary>
    IEditorAsyncCommand<object> NewAnimationCommand
    {
        get;
    }
}
