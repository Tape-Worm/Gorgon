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
// Created: June 7, 2020 8:25:18 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Gorgon.Animation;
using Gorgon.Editor.AnimationEditor.Services;
using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// Parameters for the <see cref="IAnimationContent"/> view model.
/// </summary>
internal class AnimationContentParameters
    : ContentViewModelInjection
{
    /// <summary>
    /// Property to return the settings for the plug in.
    /// </summary>
    public ISettings Settings
    {
        get;
    }

    /// <summary>
    /// Property to return the file containing the animation data.
    /// </summary>
    public IContentFile AnimationFile
    {
        get;
    }

    /// <summary>
    /// Property to return the animation.
    /// </summary>
    public IGorgonAnimation Animation
    {
        get;
    }

    /// <summary>
    /// Property to return the animation controller.
    /// </summary>
    public GorgonSpriteAnimationController Controller
    {
        get;
    }

    /// <summary>
    /// Property to set or return the primary sprite animated by the animation.
    /// </summary>
    public AnimationIOService.PrimarySpriteDependency PrimarySprite
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the services for the content plug in.
    /// </summary>
    public ContentServices ContentServices
    {
        get;
    }

    /// <summary>
    /// Property to return the track view models.
    /// </summary>
    public ObservableCollection<ITrack> Tracks
    {
        get;
    }

    /// <summary>
    /// Property to return the list of tracks not supported by the editor.
    /// </summary>
    public IReadOnlyList<ITrack> ExcludedTracks
    {
        get;
    }

    /// <summary>
    /// Property to return the add track view model.
    /// </summary>
    public IAddTrack AddTrack
    {
        get;
    }

    /// <summary>
    /// Property to return the animation properties view model.
    /// </summary>
    public IProperties Properties
    {
        get;
    }

    /// <summary>
    /// Property to return the key editor context view model.
    /// </summary>
    public IKeyEditorContext KeyEditorContext
    {
        get;
    }

    /// <summary>
    /// Property to set or return the texture used for the background guide.
    /// </summary>
    public GorgonTexture2DView BackgroundTexture
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the file containing the <see cref="BackgroundTexture"/>.
    /// </summary>
    public IContentFile BackgroundTextureFile
    {
        get;
        set;
    }

    /// <summary>Initializes a new instance of the <see cref="AnimationContentParameters"/> class.</summary>
    /// <param name="animationFile">The file containing the animation data.</param>
    /// <param name="animation">The animation to edit.</param>
    /// <param name="codec">The codec for saving the animation.</param>
    /// <param name="tracks">The track view models.</param>
    /// <param name="excludedTracks">The tracks that are not supported.</param>
    /// <param name="addTrackViewModel">The view model for the new track interface.</param>        
    /// <param name="animProperties">The view model for the animation properties.</param>
    /// <param name="keyEditor">The view model for the key editor context.</param>
    /// <param name="controller">The controller for the animation.</param>
    /// <param name="settings">The settings for the animation plug in.</param>
    /// <param name="fileManager">The file manager for content files.</param>
    /// <param name="contentServices">The services for the content plug in.</param>
    /// <param name="commonServices">The common services for the application.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the required parameters are <b>null</b>.</exception>
    public AnimationContentParameters(IContentFile animationFile,                                    
                                    IGorgonAnimation animation,
                                    ObservableCollection<ITrack> tracks,
                                    IReadOnlyList<ITrack> excludedTracks,
                                    IAddTrack addTrackViewModel,
                                    IProperties animProperties,
                                    IKeyEditorContext keyEditor,
                                    GorgonSpriteAnimationController controller,
                                    ISettings settings,
                                    IContentFileManager fileManager, 
                                    ContentServices contentServices,
                                    IHostContentServices commonServices)
        : base(fileManager, animationFile, commonServices)
    {
        AnimationFile = animationFile ?? throw new ArgumentNullException(nameof(animationFile));
        Animation = animation ?? throw new ArgumentNullException(nameof(animation));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Tracks = tracks ?? throw new ArgumentNullException(nameof(tracks));
        ExcludedTracks = excludedTracks ?? throw new ArgumentNullException(nameof(excludedTracks));
        Controller = controller ?? throw new ArgumentNullException(nameof(controller));
        ContentServices = contentServices ?? throw new ArgumentNullException(nameof(contentServices));
        AddTrack = addTrackViewModel ?? throw new ArgumentNullException(nameof(addTrackViewModel));
        Properties = animProperties ?? throw new ArgumentNullException(nameof(animProperties));
        KeyEditorContext = keyEditor ?? throw new ArgumentNullException(nameof(keyEditor));
    }
}
