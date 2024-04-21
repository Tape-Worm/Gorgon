
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
// Created: March 2, 2019 2:09:04 AM
// 

using System.Buffers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using Gorgon.Animation;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.AnimationEditor.Services;
using Gorgon.Editor.Content;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// Content view model for a animation
/// </summary>
internal class AnimationContent
    : ContentEditorViewModelBase<AnimationContentParameters>, IAnimationContent
{
    /// <summary>
    /// Data stored for adding a track using undo/redo.
    /// </summary>
    private class AddTrackUndoRedoData
    {
        // The tracks that were added.
        public List<GorgonTrackRegistration> Tracks;
        // The list of tracks that were removed.
        public List<ITrack> RemovedTracks;
    }

    /// <summary>
    /// Data stored for removing a track using undo/redo.
    /// </summary>
    private class RemoveTrackUndoRedoData
    {
        // The tracks that were added.
        public IReadOnlyList<ITrack> Tracks;
    }

    /// <summary>
    /// Data stored for resizing an animation using undo/redo.
    /// </summary>
    private class ResizeAnimationUndoRedoData
    {
        // The length of the animation, in seconds.
        public float Length;
        // The number of frames to display in a second.
        public float Fps;
        // The total number of keys per track.
        public int KeyCount;
        // Flag to indicate that the animation is looped.
        public bool Looped;
        // The number of times to loop the animation.
        public int LoopCount;
        // The list of keys to store.
        public Dictionary<ITrack, List<IKeyFrame>> Keys;
    }

    // The primary sprite.
    private (GorgonSprite sprite, IContentFile spriteFile, IContentFile textureFile) _primarySprite;
    // The background image to display for guiding.
    private (GorgonTexture2DView texture, IContentFile file) _backImage;
    // The animation.
    private IGorgonAnimation _animation;
    // The controller for the animation.
    private GorgonSpriteAnimationController _controller;
    // The current animation state.
    private AnimationState _state = AnimationState.Stopped;
    // The length of the animation, in seconds.
    private float _length = 1.0f;
    // The frames per second for the animation.
    private float _fps = 60.0f;
    // The animation I/O service.
    private ContentServices _contentServices;
    // The currently active panel.
    private IHostedPanelViewModel _currentPanel;
    // Flag to indicate that the animation is looped.
    private bool _looping;
    // A list of tracks for synchronization with the observable collection.
    private readonly List<ITrack> _trackList = [];
    // The list of tracks unsupported by the editor.
    private IReadOnlyList<ITrack> _unsupportedTracks;
    // The list of selected tracks and keys.
    private IReadOnlyList<TrackKeySelection> _selected = [];
    // The starting position of the primary sprite.
    private Vector2 _primaryStart;

    /// <summary>
    /// Property to return the add track view model.
    /// </summary>
    public IAddTrack AddTrack
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to set or return the starting position of the primary sprite.
    /// </summary>
    public Vector2 PrimarySpriteStartPosition
    {
        get => _primaryStart;
        set
        {
            if (_primaryStart.Equals(value))
            {
                return;
            }

            OnPropertyChanging();
            _primaryStart = value;
            OnPropertyChanged();

            UpdatePrimarySprite();

            if (WorkingSprite is not null)
            {
                WorkingSprite.Position = _primaryStart;
            }

            ContentState = ContentState.Modified;
        }
    }

    /// <summary>
    /// Property to return the settings view model for the plug-in.
    /// </summary>
    public ISettings Settings
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the key editor context view model.
    /// </summary>
    public IKeyEditorContext KeyEditor
    {
        get;
        private set;
    }

    /// <summary>Property to return the currently active panel.</summary>
    public IHostedPanelViewModel CurrentPanel
    {
        get => _currentPanel;
        set
        {
            if (_currentPanel == value)
            {
                return;
            }

            if (_currentPanel is not null)
            {
                _currentPanel.PropertyChanged -= CurrentPanel_PropertyChanged;
                _currentPanel.IsActive = false;
            }

            OnPropertyChanging();
            _currentPanel = value;
            OnPropertyChanged();

            if (_currentPanel is not null)
            {
                _currentPanel.IsActive = true;
                _currentPanel.PropertyChanged += CurrentPanel_PropertyChanged;
            }
        }
    }

    /// <summary>
    /// Property to return the maximum number of keys for the animation.
    /// </summary>
    public int MaxKeyCount => (int)(Length * Fps).Round();

    /// <summary>Property to return the primary sprite for the animation.</summary>
    public GorgonSprite PrimarySprite
    {
        get => _primarySprite.sprite;
        private set
        {
            if (_primarySprite.sprite == value)
            {
                return;
            }

            OnPropertyChanging();
            _primarySprite = (value, _primarySprite.spriteFile, _primarySprite.textureFile);
            UpdatePrimarySprite();
            value?.CopyTo(WorkingSprite);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the sprite used in the animation preview (a copy of <see cref="PrimarySprite"/>).
    /// </summary>
    public GorgonSprite WorkingSprite
    {
        get;
    } = new GorgonSprite();

    /// <summary>Property to return the length of the animation, in seconds.</summary>
    public float Length
    {
        get => _length;
        private set
        {
            if (_length.EqualsEpsilon(value))
            {
                return;
            }

            NotifyPropertyChanging(nameof(MaxKeyCount));
            OnPropertyChanging();

            _length = value;

            OnPropertyChanged();
            NotifyPropertyChanged(nameof(MaxKeyCount));
        }
    }

    /// <summary>
    /// Property to return the frames per second of the animation.
    /// </summary>
    public float Fps
    {
        get => _fps;
        private set
        {
            if (_fps.EqualsEpsilon(value))
            {
                return;
            }

            NotifyPropertyChanging(nameof(MaxKeyCount));
            OnPropertyChanging();

            _fps = value;

            OnPropertyChanged();
            NotifyPropertyChanged(nameof(MaxKeyCount));
        }
    }

    /// <summary>
    /// Property to return the current state of the animation.
    /// </summary>
    public AnimationState State
    {
        get => _state;
        private set
        {
            if (value == _state)
            {
                return;
            }

            OnPropertyChanging();
            _state = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the key time for an animation preview.
    /// </summary>
    public float PreviewKeyTime => _controller.Time;

    /// <summary>
    /// Property to return whether any tracks are assigned.
    /// </summary>
    public bool HasTracks => Tracks.Count > 0;

    /// <summary>Property to return the list of tracks to animate data on a sprite.</summary>
    public ObservableCollection<ITrack> Tracks
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the list of tracks and keys that were selected.
    /// </summary>
    public IReadOnlyList<TrackKeySelection> Selected
    {
        get => _selected;
        private set
        {
            value ??= [];

            if ((value == _selected) || (_selected.SequenceEqual(value)))
            {
                return;
            }

            OnPropertyChanging();
            _selected = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return whether the animation will loop back to the beginning when it ends.
    /// </summary>
    public bool IsLooping
    {
        get => _looping;
        set
        {
            if (_looping == value)
            {
                return;
            }

            OnPropertyChanging();
            _looping = value;

            if (_animation is not null)
            {
                Properties.IsLooped = _animation.IsLooped = value;
            }

            OnPropertyChanged();

            ContentState = ContentState.Modified;
        }
    }

    /// <summary>
    /// Property to return the background image.
    /// </summary>
    public GorgonTexture2DView BackgroundImage
    {
        get => _backImage.texture;
        private set
        {
            if (_backImage.texture == value)
            {
                return;
            }

            OnPropertyChanging();
            _backImage = (value, _backImage.file);
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the command used to undo an operation.</summary>
    public IEditorCommand<object> UndoCommand
    {
        get;
    }

    /// <summary>Property to return the command used to redo an operation.</summary>
    public IEditorCommand<object> RedoCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to update the animation during preview.
    /// </summary>
    public IEditorCommand<object> UpdateAnimationPreviewCommand
    {
        get;
    }

    /// <summary>Property to return the type of content.</summary>
    /// <remarks>The content type is a user defined string that indicates what the data for the content represents. For example, for an image editor, this could return "Image".</remarks>
    public override string ContentType => CommonEditorContentTypes.AnimationType;

    /// <summary>Property to return the command used to start playing the animation.</summary>
    public IEditorCommand<object> PlayAnimationCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to stop playing the animation.
    /// </summary>
    public IEditorCommand<object> StopAnimationCommand
    {
        get;
    }

    /// <summary>
    /// Property to return a specific key frame for a track index/key index.
    /// </summary>
    public IEditorCommand<GetTrackKeyArgs> GetTrackKeyCommand
    {
        get;
    }

    /// <summary>Property to return the command that will activate the add track interface.</summary>
    public IEditorCommand<object> ShowAddTrackCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command that will activate the animation properties interface.
    /// </summary>
    public IEditorCommand<object> ShowAnimationPropertiesCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the key frame editor.
    /// </summary>
    public IEditorCommand<object> ActivateKeyEditorCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to delete all selected tracks.
    /// </summary>
    public IEditorCommand<object> DeleteTrackCommand
    {
        get;
    }

    /// <summary>Property to return the command used update the lists of selections.</summary>
    public IEditorCommand<IReadOnlyList<(int trackIndex, IReadOnlyList<int> keyIndices)>> SelectTrackAndKeysCommand
    {
        get;
    }

    /// <summary>Property to return the command used to clear the tracks and keys from the animation.</summary>
    public IEditorCommand<object> ClearAnimationCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to load a background image.
    /// </summary>
    public IEditorAsyncCommand<object> LoadBackgroundImageCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to clear the background image.
    /// </summary>
    public IEditorCommand<object> ClearBackgroundImageCommand
    {
        get;
    }

    /// <summary>Property to return the command used to select the first key.</summary>
    public IEditorCommand<object> FirstKeyCommand
    {
        get;
    }

    /// <summary>Property to return the command used to select the last key.</summary>
    public IEditorCommand<object> LastKeyCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to select the previous key.
    /// </summary>
    public IEditorCommand<object> PrevKeyCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to select the next key.
    /// </summary>
    public IEditorCommand<object> NextKeyCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to create a new animation.
    /// </summary>
    public IEditorAsyncCommand<object> NewAnimationCommand
    {
        get;
    }

    /// <summary>Property to return the animation properties view model.</summary>
    public IProperties Properties
    {
        get;
        private set;
    }

    /// <summary>Property to return the command used to load a sprite from the project (for use in texture animations).</summary>
    public IEditorAsyncCommand<IReadOnlyList<string>> LoadSpriteCommand
    {
        get;
    }

    /// <summary>
    /// Function to default the selection to the first keyframe in the first track.
    /// </summary>
    private void SelectDefault() => Selected = ((Tracks.Count > 0) && (Tracks[0].KeyFrames.Count > 0)) ? new[]
                                                                                                         {
                                                                                                            new TrackKeySelection(0, Tracks[0],
                                                                                                                                                [
                                                                                                                                                    new TrackKeySelection.KeySelection(Tracks[0], 0, _fps)
                                                                                                                                                ])
                                                                                                         }
                                                                                                       : [];

    /// <summary>
    /// Function to update the primary sprite position.
    /// </summary>
    private void UpdatePrimarySprite()
    {
        if (PrimarySprite is null)
        {
            return;
        }

        PrimarySprite.Position = PrimarySpriteStartPosition;
    }

    /// <summary>
    /// Function to deactivate any active command contexts.
    /// </summary>
    private void DeactivateCommandContexts()
    {
        // If we've an animation playing, then we need to stop it.
        if (_controller.State == AnimationState.Playing)
        {
            DoStopAnimation();
        }

        CurrentPanel = null;
        CommandContext = null;
    }

    /// <summary>
    /// Function to rebuild the animation.
    /// </summary>
    private void RebuildAnimation()
    {
        bool wasPlaying = false;
        float currentTime = 0;

        if (State != AnimationState.Stopped)
        {
            wasPlaying = true;
            _controller.Stop();
            State = AnimationState.Stopped;
        }
        else
        {
            currentTime = _controller.Time;
        }

        ITrack[] trackList = null;

        try
        {
            trackList = ArrayPool<ITrack>.Shared.Rent(Tracks.Count + _unsupportedTracks.Count);

            int index = 0;
            foreach (ITrack track in Tracks.Concat(_unsupportedTracks))
            {
                trackList[index++] = track;
            }

            _animation = AnimationFactory.CreateAnimation(File.Path, _looping, _animation?.LoopCount ?? 0, MaxKeyCount, _fps, trackList);

            NotifyPropertyChanging(nameof(WorkingSprite));

            _primarySprite.sprite?.CopyTo(WorkingSprite);

            _controller.Play(WorkingSprite, _animation);
            _controller.Time = currentTime;
            _controller.Refresh();

            NotifyPropertyChanged(nameof(WorkingSprite));

            if (!wasPlaying)
            {
                _controller.Pause();
            }
            else
            {
                State = AnimationState.Playing;
            }
        }
        finally
        {
            if (trackList is not null)
            {
                ArrayPool<ITrack>.Shared.Return(trackList, true);
            }
        }
    }

    /// <summary>Handles the PropertyChanged event of the Settings control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISettings.DefaultResolution):
                UpdatePrimarySprite();
                RebuildAnimation();
                break;
        }
    }

    /// <summary>Handles the PropertyChanged event of the KeyEditor control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void KeyEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IKeyEditorContext.CurrentEditor):
                if (CommandContext == KeyEditor)
                {
                    CurrentPanel = KeyEditor.CurrentEditor;
                }
                break;
        }
    }

    /// <summary>Handles the WaitPanelDeactivated event of the KeyEditor control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void KeyEditor_WaitPanelDeactivated(object sender, EventArgs e) => HideWaitPanel();

    /// <summary>Keys the editor wait panel activated.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    private void KeyEditor_WaitPanelActivated(object sender, WaitPanelActivateArgs e) => ShowWaitPanel(e);

    /// <summary>Handles the PropertyChanged event of the CurrentPanel control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void CurrentPanel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IHostedPanelViewModel.IsActive):
                if (CurrentPanel is not null)
                {
                    CurrentPanel = null;
                }
                else
                {
                    CurrentPanel = (IHostedPanelViewModel)sender;
                }
                break;
        }
    }

    /// <summary>Handles the PropertyChanged event of the Track control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void Track_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ITrack.KeyFrames):
            case nameof(ITrack.InterpolationMode):
                RebuildAnimation();
                ContentState = ContentState.Modified;
                break;
        }
    }

    /// <summary>
    /// Function called when the texture track collection is updated.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void Tracks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        NotifyPropertyChanging(nameof(HasTracks));

        ITrack track;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                track = (ITrack)e.NewItems[0];
                track.PropertyChanged += Track_PropertyChanged;
                _trackList.Add(track);
                break;
            case NotifyCollectionChangedAction.Remove:
                track = (ITrack)e.OldItems[0];
                track.PropertyChanged -= Track_PropertyChanged;
                _trackList.Remove(track);
                break;
            case NotifyCollectionChangedAction.Reset:
                foreach (ITrack clearTrack in _trackList)
                {
                    clearTrack.PropertyChanged -= Track_PropertyChanged;
                }
                _trackList.Clear();
                break;
        }

        NotifyPropertyChanged(nameof(HasTracks));
    }

    /// <summary>
    /// Function to determine if a sprite can be assigned as the primary sprite.
    /// </summary>
    /// <param name="spritePatsh">The paths to the sprite.</param>
    /// <returns><b>true</b> if the sprite can be assigned, <b>false</b> if not.</returns>
    private bool CanAssignPrimarySprite(IReadOnlyList<string> spritePaths)
    {
        string spritePath;

        if ((CurrentPanel is not null) || (CommandContext is not null))
        {
            return false;
        }

        if (spritePaths is null)
        {
            IReadOnlyList<string> spriteFiles = ContentFileManager.GetSelectedFiles();

            if ((spriteFiles.Count == 0)
                || (string.IsNullOrWhiteSpace(spriteFiles[0]))
                || (!ContentFileManager.FileExists(spriteFiles[0])))
            {
                return false;
            }

            spritePath = spriteFiles[0];
        }
        else
        {
            if (spritePaths.Count != 1)
            {
                return false;
            }

            spritePath = spritePaths[0];

            if ((!ContentFileManager.FileExists(spritePath)) || (CurrentPanel is not null))
            {
                return false;
            }
        }

        IContentFile spriteFile = ContentFileManager.GetFile(spritePath);

        return (spriteFile is not null) && (_contentServices.IOService.IsContentSprite(spriteFile));
    }

    /// <summary>
    /// Function to assign a sprite as the primary sprite for the animation.
    /// </summary>
    /// <param name="spritePaths">The paths to the sprite.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoAssignPrimarySpriteAsync(IReadOnlyList<string> spritePaths)
    {
        IKeyFrame[] keyFrames = null;
        string spritePath = string.Empty;

        try
        {
            IContentFile spriteFile;

            spritePaths ??= ContentFileManager.GetSelectedFiles();

            spritePath = spritePaths[0];
            spriteFile = ContentFileManager.GetFile(spritePath);

            ShowWaitPanel(string.Format(Resources.GORANM_TEXT_LOADING, spritePath.Ellipses(60, true)));

            GorgonTexture2DView prevTexture = _primarySprite.sprite?.Texture;

            (GorgonSprite sprite, IContentFile textureFile) = await _contentServices.IOService.LoadSpriteAsync(spriteFile);
            // We have to mark this as open because it's the primary sprite, normally we wouldn't mark the sprite as open as 
            // we copy its data into a keyframe, this is not the case here.
            _contentServices.IOService.UnloadSprite(_primarySprite);

            _primarySprite = (null, spriteFile, textureFile);
            PrimarySprite = sprite;
            spriteFile.IsOpen = true;

            RebuildAnimation();

            // If this is a brand new animation, then initialize it with a new texture track.
            if ((Settings.AddTextureTrackForPrimarySprite)
                && (File.Metadata.Attributes.ContainsKey(CommonEditorConstants.IsNewAttr))
                && (Tracks.Count == 0))
            {
                // Default to a new texture track.
                ITrack track = await _contentServices.ViewModelFactory.CreateDefaultTextureTrackAsync(_primarySprite, MaxKeyCount);
                Tracks.Add(track);
            }

            ContentState = ContentState.Modified;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GORANM_ERR_PRIMARY_SPRITE_LOAD, spritePath));
        }
        finally
        {
            if (keyFrames is not null)
            {
                ArrayPool<IKeyFrame>.Shared.Return(keyFrames, true);
            }

            HideWaitPanel();
        }
    }

    /// <summary>
    /// Function to determine if the animation can be played or not.
    /// </summary>
    /// <returns><b>true</b> if the animation can be played, <b>false</b> if not.</returns>
    private bool CanPlayAnimation() => (_primarySprite.sprite is not null)
                                    && (_controller.CurrentAnimation is not null)
                                    && (HasTracks)
                                    && (Tracks.Any(item => item.KeyFrames.Count > 0))
                                    && (_controller.State != AnimationState.Playing)
                                    && (CurrentPanel is null)
                                    && (CommandContext is null);

    /// <summary>
    /// Function to start playing the animation.
    /// </summary>
    private void DoPlayAnimation()
    {
        try
        {
            _controller.Resume();
            State = AnimationState.Playing;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_CANNOT_PLAY_ANIMATION);
        }
    }

    /// <summary>
    /// Function to determine if the animation can be stopped or not.
    /// </summary>
    /// <returns><b>true</b> if the animation can be stopped, <b>false</b> if not.</returns>
    private bool CanStopAnimation() => (_controller.CurrentAnimation is not null) && (_primarySprite.sprite is not null) && (CurrentPanel is null) && (CommandContext is null);

    /// <summary>
    /// Function to start playing the animation.
    /// </summary>
    private void DoStopAnimation()
    {
        float currentTime = 0;

        try
        {
            if (_selected.Count > 0)
            {
                currentTime = _selected.SelectMany(item => item.SelectedKeys)?.FirstOrDefault()?.TimeIndex ?? 0;
            }
            _controller.Reset();
            _controller.Pause();
            _controller.Time = currentTime;

            State = AnimationState.Stopped;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_CANNOT_STOP_ANIMATION);
        }
    }

    /// <summary>
    /// Function to determine if the animation preview can be updated.
    /// </summary>
    /// <returns><b>true</b> if the animation preview can be updated, or <b>false</b> if not.</returns>
    private bool CanUpdateAnimationPreview() => (_primarySprite.sprite is not null)
                                            && (_controller.CurrentAnimation is not null)
                                            && (_controller.State == AnimationState.Playing)
                                            && (CurrentPanel is null)
                                            && (CommandContext is null);

    /// <summary>
    /// Function to update the animation preview.
    /// </summary>
    private void DoUpdateAnimationPreview()
    {
        try
        {
            NotifyPropertyChanging(nameof(PreviewKeyTime));
            _controller.Update();
            NotifyPropertyChanged(nameof(PreviewKeyTime));

            if (_controller.State == AnimationState.Stopped)
            {
                _controller.Play(WorkingSprite, _animation);
                _controller.Pause();

                State = AnimationState.Stopped;
            }
        }
        catch (Exception ex)
        {
            HostServices.Log.PrintError("There was an error updating the animation preview.", LoggingLevel.Verbose);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if a key can be retrieved.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    /// <returns><b>true</b> if the key can be retrieved, <b>false</b> if not.</returns>
    private bool CanGetTrackKey(GetTrackKeyArgs args) => (args.TrackIndex >= 0) && (args.TrackIndex < Tracks.Count) && (args.KeyIndex >= 0) && (args.KeyIndex < MaxKeyCount);

    /// <summary>
    /// Function to retrieve a key with a given key index and track index.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    private void DoGetTrackKey(GetTrackKeyArgs args)
    {
        try
        {
            ITrack track = Tracks[args.TrackIndex];
            args.Key = track.KeyFrames[args.KeyIndex];
        }
        catch (Exception ex)
        {
            HostServices.Log.PrintError("There was an error retrieving the key frame.", LoggingLevel.Verbose);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if a track can be added to the animation.
    /// </summary>
    /// <returns></returns>
    private bool CanShowAddTrack() => (CurrentPanel is null) && (CommandContext is null) && (_primarySprite.sprite is not null) && (AddTrack.AvailableTracks.Count > 0);

    /// <summary>
    /// Function to show the add track interface.
    /// </summary>
    private void DoShowAddTrack()
    {
        // If we've an animation playing, then we need to stop it.
        if (_controller.State == AnimationState.Playing)
        {
            DoStopAnimation();
        }

        try
        {
            AddTrack.SelectedTracks = [];
            CurrentPanel = AddTrack;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_SHOW_ADD_TRACK);
        }
    }

    /// <summary>
    /// Function to determine if animation properties can be shown.
    /// </summary>
    /// <returns><b>true</b> if the properties can be shown at this time, or <b>false</b> if not.</returns>
    private bool CanShowProperties() => (CurrentPanel is null) && (CommandContext is null) && (_primarySprite.sprite is not null);

    /// <summary>
    /// Function to show the animation properties interface.
    /// </summary>
    private void DoShowProperties()
    {
        // If we've an animation playing, then we need to stop it.
        if (_controller.State == AnimationState.Playing)
        {
            DoStopAnimation();
        }

        try
        {
            Properties.Length = _animation.Length;
            Properties.Fps = _animation.Fps;
            Properties.IsLooped = _animation.IsLooped;
            Properties.LoopCount = _animation.LoopCount;

            CurrentPanel = Properties;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_SHOW_PROPERTIES);
        }
    }

    /// <summary>
    /// Function to determine if tracks can be added to the animation.
    /// </summary>
    /// <returns></returns>
    private bool CanAddTrack() => (_primarySprite.sprite is not null)
                               && (CurrentPanel == AddTrack)
                               && (CommandContext is null)
                               && (AddTrack.AvailableTracks.Count > 0)
                               && (AddTrack is not null)
                               && (AddTrack.SelectedTracks.Count > 0);

    /// <summary>
    /// Function to add tracks to the animation.
    /// </summary>
    private void DoAddTrack()
    {
        AddTrackUndoRedoData addTrackRedoUndoArgs;

        bool RemoveTracks(AddTrackUndoRedoData args)
        {
            HostServices.BusyService.SetBusy();

            args.RemovedTracks ??= [];

            try
            {
                DeactivateCommandContexts();

                foreach (GorgonTrackRegistration trackReg in args.Tracks)
                {
                    ITrack track = Tracks.FirstOrDefault(item => item.ID == trackReg.ID);

                    if (track is null)
                    {
                        continue;
                    }

                    args.RemovedTracks.Add(track);
                    Tracks.Remove(track);
                    AddTrack.AvailableTracks.Add(trackReg);
                }

                SelectDefault();
                RebuildAnimation();

                NotifyPropertyChanged(nameof(HasTracks));

                ContentState = ContentState.Modified;

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_ADD_TRACK);
                return false;
            }
            finally
            {
                HostServices.BusyService.SetIdle();
            }
        }

        bool AddTracks(AddTrackUndoRedoData args)
        {
            try
            {
                DeactivateCommandContexts();

                if (args.RemovedTracks is not null)
                {
                    foreach (ITrack track in args.RemovedTracks)
                    {
                        if (Tracks.Any(item => item.ID == track.ID))
                        {
                            continue;
                        }

                        GorgonTrackRegistration reg = AddTrack.AvailableTracks.FirstOrDefault(item => item.ID == track.ID);

                        Debug.Assert(reg is not null, "Track registation not found.");

                        Tracks.Add(track);
                        AddTrack.AvailableTracks.Remove(reg);
                    }
                }
                else
                {
                    foreach (GorgonTrackRegistration trackReg in args.Tracks)
                    {
                        if (Tracks.Any(item => item.ID == trackReg.ID))
                        {
                            continue;
                        }

                        Tracks.Add(_contentServices.ViewModelFactory.CreateTrack(trackReg, MaxKeyCount));
                        AddTrack.AvailableTracks.Remove(trackReg);
                    }
                }

                CurrentPanel = null;
                RebuildAnimation();

                ContentState = ContentState.Modified;

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_ADD_TRACK);
                return false;
            }
            finally
            {
                HostServices.BusyService.SetIdle();
            }
        }

        Task UndoAction(AddTrackUndoRedoData undoTrackArgs, CancellationToken cancelToken)
        {
            RemoveTracks(undoTrackArgs);
            return Task.CompletedTask;
        }

        Task RedoAction(AddTrackUndoRedoData redoTrackArgs, CancellationToken cancelToken)
        {
            AddTracks(redoTrackArgs);
            return Task.CompletedTask;
        }

        addTrackRedoUndoArgs = new AddTrackUndoRedoData
        {
            Tracks = new List<GorgonTrackRegistration>(AddTrack.SelectedTracks)
        };

        if (AddTracks(addTrackRedoUndoArgs))
        {
            _contentServices.UndoService.Record(Resources.GORANM_DESC_UNDO_ADDTRACKS, UndoAction, RedoAction, addTrackRedoUndoArgs, addTrackRedoUndoArgs);
            // Need to call this so the UI can register our updated undo stack.
            NotifyPropertyChanged(nameof(UndoCommand));
        }
    }

    /// <summary>
    /// Function to determine if tracks can be removed.
    /// </summary>
    /// <returns><b>true</b> if the selected tracks can be deleted, <b>false</b> if not.</returns>
    private bool CanDeleteTrack() => (_primarySprite.sprite is not null) && (CurrentPanel is null) && (CommandContext is null) && (_selected.Count != 0);

    /// <summary>
    /// Function to delete tracks from the animation.
    /// </summary>
    /// <param name="tracks">The tracks to remove.</param>
    private void DeleteTracks(IEnumerable<ITrack> tracks)
    {
        RemoveTrackUndoRedoData removeTrackUndoArgs;
        RemoveTrackUndoRedoData removeTrackRedoArgs;

        async Task<bool> RestoreAsync(RemoveTrackUndoRedoData removeTrackArgs)
        {
            ShowWaitPanel(Resources.GORANM_TEXT_PLEASE_WAIT);

            try
            {
                DeactivateCommandContexts();

                for (int i = 0; i < removeTrackArgs.Tracks.Count; ++i)
                {
                    ITrack track = removeTrackArgs.Tracks[i];

                    for (int j = 0; j < track.KeyFrames.Count; ++j)
                    {
                        IKeyFrame keyFrame = track.KeyFrames[j];
                        if ((keyFrame is null) || (keyFrame.TextureValue.TextureFile is null))
                        {
                            continue;
                        }

                        await _contentServices.KeyProcessor.RestoreTextureAsync(keyFrame);
                    }

                    Tracks.Add(track);
                    AddTrack.AvailableTracks.Remove(_controller.RegisteredTracks.FirstOrDefault(item => item.ID == track.ID));
                }

                SelectDefault();
                RebuildAnimation();

                NotifyPropertyChanged(nameof(HasTracks));

                ContentState = ContentState.Modified;
                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_DELETE_TRACKS);
                return false;
            }
            finally
            {
                HideWaitPanel();
            }
        }

        bool Delete(RemoveTrackUndoRedoData removeTrackArgs)
        {
            try
            {
                DeactivateCommandContexts();

                for (int i = 0; i < removeTrackArgs.Tracks.Count; ++i)
                {
                    ITrack track = removeTrackArgs.Tracks[i];

                    // If this track is a texture track, we need to dispose of any textures we are no longer using (as long as it's not attached to the primary sprite).                        
                    foreach (IKeyFrame textureFrame in track.KeyFrames.Where(item => (item is not null) && (item.TextureValue.Texture is not null)))
                    {
                        _contentServices.KeyProcessor.UnloadTextureKeyframe(textureFrame);
                    }

                    Tracks.Remove(track);
                    AddTrack.AvailableTracks.Add(_controller.RegisteredTracks.FirstOrDefault(item => item.ID == track.ID));
                }

                SelectDefault();
                RebuildAnimation();

                ContentState = ContentState.Modified;
                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_DELETE_TRACKS);
                return false;
            }
        }

        async Task UndoAction(RemoveTrackUndoRedoData removeTrackArgs, CancellationToken cancelToken) => await RestoreAsync(removeTrackArgs);
        Task RedoAction(RemoveTrackUndoRedoData removeTrackArgs, CancellationToken cancelToken)
        {
            Delete(removeTrackArgs);
            return Task.CompletedTask;
        }

        List<ITrack> filteredTracks = new(tracks);

        removeTrackUndoArgs = new RemoveTrackUndoRedoData
        {
            Tracks = filteredTracks
        };

        removeTrackRedoArgs = new RemoveTrackUndoRedoData
        {
            Tracks = filteredTracks
        };

        if (Delete(removeTrackRedoArgs))
        {
            _contentServices.UndoService.Record(Resources.GORANM_DESC_UNDO_REMOVETRACKS, UndoAction, RedoAction, removeTrackUndoArgs, removeTrackRedoArgs);
            // Need to call this so the UI can register our updated undo stack.
            NotifyPropertyChanged(nameof(UndoCommand));
        }
    }

    /// <summary>
    /// Function to delete the selected tracks.
    /// </summary>
    private void DoDeleteTrack()
    {
        if (HostServices.MessageDisplay.ShowConfirmation(Resources.GORANM_CONFIRM_DELETE_TRACKS) == MessageResponse.No)
        {
            return;
        }

        // Error handling and undo/redo are done in this method.
        ITrack[] tracks = null;

        try
        {
            tracks = ArrayPool<ITrack>.Shared.Rent(_selected.Count);

            for (int i = 0; i < _selected.Count; ++i)
            {
                tracks[i] = _selected[i].Track;
            }

            DeleteTracks(tracks.Where(item => item is not null));
        }
        finally
        {
            if (tracks is not null)
            {
                ArrayPool<ITrack>.Shared.Return(tracks, true);
            }
        }
    }

    /// <summary>
    /// Function to determine if the animation can be cleared.
    /// </summary>
    /// <returns><b>true</b> if the animation can be cleared, <b>false</b> if not.</returns>
    private bool CanClearAnimation() => (_primarySprite.sprite is not null) && (CurrentPanel is null) && (CommandContext is null) && (Tracks.Count > 0);

    /// <summary>
    /// Function to clear the animation.
    /// </summary>
    private void DoClearAnimation()
    {
        try
        {
            if (HostServices.MessageDisplay.ShowConfirmation(Resources.GORANM_CONFIRM_CLEAR_ANIM) == MessageResponse.No)
            {
                return;
            }

            DeleteTracks(Tracks);
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_CLEAR_ANIMATION);
        }
    }

    /// <summary>
    /// Function to determine if an undo operation is possible.
    /// </summary>
    /// <returns><b>true</b> if the last action can be undone, <b>false</b> if not.</returns>
    private bool CanUndo() => (_contentServices.UndoService.CanUndo) && (CommandContext is null) && (CurrentPanel is null);

    /// <summary>
    /// Function to determine if a redo operation is possible.
    /// </summary>
    /// <returns><b>true</b> if the last action can be redone, <b>false</b> if not.</returns>
    private bool CanRedo() => (_contentServices.UndoService.CanRedo) && (CommandContext is null) && (CurrentPanel is null);

    /// <summary>
    /// Function called when a redo operation is requested.
    /// </summary>
    private async void DoRedoAsync()
    {
        try
        {
            await _contentServices.UndoService.Redo();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_REDO);
        }
    }

    /// <summary>
    /// Function called when an undo operation is requested.
    /// </summary>
    private async void DoUndoAsync()
    {
        try
        {
            await _contentServices.UndoService.Undo();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_UNDO);
        }
    }

    /// <summary>
    /// Function to select tracks and keys.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    private void DoSelectTracksAndKeys(IReadOnlyList<(int trackIndex, IReadOnlyList<int> keyIndices)> args)
    {
        try
        {
            if ((args is null) || (args.Count == 0))
            {
                Selected = [];
                return;
            }

            List<TrackKeySelection> selected = [];

            for (int i = 0; i < args.Count; ++i)
            {
                (int trackIndex, IReadOnlyList<int> keyIndices) = args[i];
                ITrack selectedTrack = Tracks[trackIndex];

                if ((keyIndices is null) || (keyIndices.Count == 0) || (selectedTrack is null))
                {
                    continue;
                }

                TrackKeySelection.KeySelection[] keySelection = new TrackKeySelection.KeySelection[keyIndices.Count];

                for (int j = 0; j < keyIndices.Count; ++j)
                {
                    int keyIndex = keyIndices[j];
                    IKeyFrame keyFrame = selectedTrack.KeyFrames[keyIndex];

                    keySelection[j] = new TrackKeySelection.KeySelection(selectedTrack, keyIndex, _fps);
                }

                selected.Add(new TrackKeySelection(trackIndex, selectedTrack, keySelection));
            }

            float timeIndex = 0;

            if (selected.Count != 0)
            {
                timeIndex = selected[0].SelectedKeys[0].TimeIndex;
            }

            // Reset the working sprite so we can update correctly.
            NotifyPropertyChanging(nameof(WorkingSprite));
            _primarySprite.sprite?.CopyTo(WorkingSprite);

            // This will force an update the properties on the working sprite.
            if (selected.Count != 0)
            {
                _controller.Time = timeIndex;
                _controller.Refresh();
            }

            Selected = selected;

            NotifyPropertyChanged(nameof(WorkingSprite));
        }
        catch (Exception ex)
        {
            HostServices.Log.PrintError("There was an error selecting the tracks/keys for the animation.", LoggingLevel.Verbose);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if a background image can be loaded.
    /// </summary>
    /// <returns><b>true</b> if a background image can be loaded, <b>false</b> if not.</returns>
    private bool CanLoadBackgroundImage()
    {
        if ((CurrentPanel is not null) || (CommandContext is not null))
        {
            return false;
        }

        IReadOnlyList<string> selectedFiles = ContentFileManager.GetSelectedFiles();

        if (selectedFiles.Count == 0)
        {
            return false;
        }

        IContentFile file = ContentFileManager.GetFile(selectedFiles[0]);

        return (file is not null)
            && (file.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string contentType))
            && (string.Equals(contentType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Function to load the background image.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoLoadBackgroundImageAsync()
    {
        ShowWaitPanel(Resources.GORANM_TEXT_PLEASE_WAIT);

        try
        {
            IReadOnlyList<string> selectedFiles = ContentFileManager.GetSelectedFiles();
            IContentFile file = ContentFileManager.GetFile(selectedFiles[0]);

            _contentServices.IOService.UnloadBackgroundTexture(_backImage);
            _backImage = await _contentServices.IOService.LoadBackgroundTextureAsync(file);

            UpdatePrimarySprite();
            RebuildAnimation();

            ContentState = ContentState.Modified;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_LOAD_BG_IMAGE);
        }
        finally
        {
            HideWaitPanel();
        }
    }

    /// <summary>
    /// Function to determine if a background image can be cleared.
    /// </summary>
    /// <returns><b>true</b> if a background image can be cleared, <b>false</b> if not.</returns>
    private bool CanClearBackgroundImage() => (_backImage.texture is not null) && (CurrentPanel is null) && (CommandContext is null);

    /// <summary>
    /// Function to clear the background image.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    private void DoClearBackgroundImage()
    {
        try
        {
            NotifyPropertyChanging(nameof(BackgroundImage));

            _contentServices.IOService.UnloadBackgroundTexture(_backImage);
            _backImage = (null, null);

            UpdatePrimarySprite();
            RebuildAnimation();

            NotifyPropertyChanged(nameof(BackgroundImage));

            ContentState = ContentState.Modified;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_LOAD_BG_IMAGE);
        }
        finally
        {
            HideWaitPanel();
        }
    }

    /// <summary>
    /// Function to determine if we can move to the previous key.
    /// </summary>
    /// <returns><b>true</b> if it can move, <b>false</b> if not.</returns>
    private bool CanMoveToPrevKey() => (_primarySprite.sprite is not null)
                                        && (HasTracks)
                                        && (Length > 0)
                                        && (_selected.Count == 1)
                                        && (_selected[0].TrackIndex < Tracks.Count)
                                        && (_selected[0].SelectedKeys.Count == 1)
                                        && (_selected[0].SelectedKeys[0].KeyIndex > 0);

    /// <summary>
    /// Function to move to the previous key.
    /// </summary>
    private void DoMoveToPreviousKey()
    {
        try
        {
            ITrack track = _selected[0].Track;
            int selectedIndex = _selected[0].SelectedKeys[0].KeyIndex;
            int keyIndex = 0;
            float keyTime = 0;

            if (track.KeyFrames.Count == 0)
            {
                keyIndex = 0;
                keyTime = 0;
            }
            else
            {
                IKeyFrame frame = track.KeyFrames.Take(selectedIndex.Max(1).Min(MaxKeyCount - 1)).LastOrDefault(item => item is not null);

                if (frame is not null)
                {
                    keyTime = frame.Time;
                    keyIndex = track.KeyFrames.IndexOf(frame);
                }
            }

            Selected =
            [
                new TrackKeySelection(_selected[0].TrackIndex, track,
                [
                    new TrackKeySelection.KeySelection(track, keyIndex, _fps)
                ])
            ];
            _controller.Time = keyTime;
            NotifyPropertyChanged(nameof(PreviewKeyTime));
        }
        catch (Exception ex)
        {
            HostServices.Log.PrintError("Failed to move selection to the previous key.", LoggingLevel.Verbose);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if we can move to the next key.
    /// </summary>
    /// <returns><b>true</b> if it can move, <b>false</b> if not.</returns>
    private bool CanMoveToNextKey() => (_primarySprite.sprite is not null)
                                            && (HasTracks)
                                            && (Length > 0)
                                            && (_selected.Count == 1)
                                            && (_selected[0].TrackIndex < Tracks.Count)
                                            && (_selected[0].SelectedKeys.Count == 1)
                                            && (_selected[0].SelectedKeys[0].KeyIndex < _selected[0].Track.KeyFrames.Count - 1);

    /// <summary>
    /// Function to move to the next key.
    /// </summary>
    private void DoMoveToNextKey()
    {
        try
        {
            ITrack track = _selected[0].Track;
            int selectedIndex = _selected[0].SelectedKeys[0].KeyIndex;
            int keyIndex = 0;
            float keyTime = 0;

            keyIndex = (selectedIndex + 1).Max(0).Min(MaxKeyCount - 1);

            if (track.KeyFrames.Count == 0)
            {
                keyTime = keyIndex / _fps;
            }
            else
            {
                IKeyFrame frame = track.KeyFrames.Skip(keyIndex).FirstOrDefault(item => item is not null);

                if (frame is null)
                {
                    keyTime = Length - (1 / _fps);
                    keyIndex = track.KeyFrames.Count - 1;
                }
                else
                {
                    keyTime = frame.Time;
                    keyIndex = track.KeyFrames.IndexOf(frame);
                }
            }

            Selected =
            [
                new TrackKeySelection(_selected[0].TrackIndex, track,
                [
                    new TrackKeySelection.KeySelection(track, keyIndex, _fps)
                ])
            ];
            _controller.Time = keyTime;
            NotifyPropertyChanged(nameof(PreviewKeyTime));
        }
        catch (Exception ex)
        {
            HostServices.Log.PrintError("Failed to move selection to the next key.", LoggingLevel.Verbose);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if we can move to the first key.
    /// </summary>
    /// <returns><b>true</b> if it can move, <b>false</b> if not.</returns>
    private bool CanMoveToFirstKey() => (_primarySprite.sprite is not null)
                                    && (HasTracks)
                                    && (Length > 0)
                                    && (_selected.Count == 1)
                                    && (_selected[0].TrackIndex < Tracks.Count)
                                    && (_selected[0].SelectedKeys.Count == 1)
                                    && (_selected[0].SelectedKeys[0].KeyIndex > 0);

    /// <summary>
    /// Function to move to the first key.
    /// </summary>
    private void DoMoveToFirstKey()
    {
        int index = 0;
        float animTime = 0;
        ITrack selectedTrack = _selected[0].Track;

        try
        {
            if (selectedTrack.KeyFrames.Count != 0)
            {
                IKeyFrame frame = selectedTrack.KeyFrames.FirstOrDefault(item => item is not null);

                if (frame is not null)
                {
                    animTime = frame.Time;
                    index = selectedTrack.KeyFrames.IndexOf(frame);
                }
            }

            Selected =
            [
                new TrackKeySelection(_selected[0].TrackIndex, selectedTrack,
                [
                    new TrackKeySelection.KeySelection(selectedTrack, 0, _fps)
                ])
            ];
            _controller.Time = animTime;
            NotifyPropertyChanged(nameof(PreviewKeyTime));
        }
        catch (Exception ex)
        {
            HostServices.Log.PrintError("Failed to move selection to the first key.", LoggingLevel.Verbose);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if we can move to the first key.
    /// </summary>
    /// <returns><b>true</b> if it can move, <b>false</b> if not.</returns>
    private bool CanMoveToLastKey() => (_primarySprite.sprite is not null)
                                    && (HasTracks)
                                    && (Length > 0)
                                    && (_selected.Count == 1)
                                    && (_selected[0].TrackIndex < Tracks.Count)
                                    && (_selected[0].SelectedKeys.Count == 1)
                                    && (_selected[0].SelectedKeys[0].KeyIndex < _selected[0].Track.KeyFrames.Count - 1);

    /// <summary>
    /// Function to move to the last key.
    /// </summary>
    private void DoMoveToLastKey()
    {
        try
        {
            int index = 0;
            float animTime = 0;
            float frameTime = 1 / _fps;
            ITrack selectedTrack = _selected[0].Track;

            if (selectedTrack.KeyFrames.Count == 0)
            {
                index = MaxKeyCount - 1;
                animTime = _length - frameTime;
            }
            else
            {
                IKeyFrame frame = selectedTrack.KeyFrames.LastOrDefault(item => item is not null);
                if (frame is not null)
                {
                    animTime = frame.Time;
                    index = selectedTrack.KeyFrames.IndexOf(frame);
                }
                else
                {
                    index = MaxKeyCount - 1;
                    animTime = _length - frameTime;
                }
            }

            Selected =
            [
                new TrackKeySelection(_selected[0].TrackIndex, selectedTrack,
                [
                    new TrackKeySelection.KeySelection(selectedTrack, selectedTrack.KeyFrames.Count - 1, _fps)
                ])
            ];
            _controller.Time = animTime;
            NotifyPropertyChanged(nameof(PreviewKeyTime));
        }
        catch (Exception ex)
        {
            HostServices.Log.PrintError("Failed to move selection to the last key.", LoggingLevel.Verbose);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the animation properties can be updated.
    /// </summary>
    /// <returns><b>true</b> if the properties can be updated, <b>false</b> if not.</returns>
    private bool CanUpdateAnimationProperties()
    {

        if ((_primarySprite.sprite is null) || (_currentPanel != Properties))
        {
            return false;
        }

        return ((!Length.EqualsEpsilon(Properties.Length)) || (!Fps.EqualsEpsilon(Properties.Fps)) || (IsLooping != Properties.IsLooped) || (_animation.LoopCount != Properties.LoopCount));

    }

    /// <summary>
    /// Function to update the animation properties.
    /// </summary>
    private async void DoUpdateAnimationPropertiesAsync()
    {
        if ((Properties.Length < Length) && (HostServices.MessageDisplay.ShowConfirmation(Resources.GORANM_CONFIRM_TRUNCATE) == MessageResponse.No))
        {
            return;
        }

        // Arguments for undoing/redoing the operation.
        ResizeAnimationUndoRedoData undoArgs;
        ResizeAnimationUndoRedoData redoArgs;

        // Function to store the track key frames for recall.
        Dictionary<ITrack, List<IKeyFrame>> StoreKeyframes()
        {
            Dictionary<ITrack, List<IKeyFrame>> result = [];

            for (int i = 0; i < Tracks.Count; ++i)
            {
                ITrack track = Tracks[i];
                result[track] = new List<IKeyFrame>(track.KeyFrames);
            }

            return result;
        }

        async Task<bool> ResizeAsync(ResizeAnimationUndoRedoData args, CancellationToken cancelToken)
        {
            try
            {
                DoStopAnimation();

                ShowWaitPanel(Resources.GORANM_TEXT_PLEASE_WAIT);

                int count = Tracks.SelectMany(item => item.KeyFrames).Count(item => item?.TextureValue.Texture is not null);

                // Unload current textures.
                _contentServices.KeyProcessor.UnloadTextureKeyframes(Tracks);

                AnimationFactory.ResizeAnimation(args.KeyCount, args.Fps, Tracks, args.Keys);

                if ((!Length.EqualsEpsilon(args.Length)) && (!Fps.EqualsEpsilon(args.Fps)))
                {
                    // Only need 1 notification on the UI, so we only need to assign one property directly.
                    _fps = args.Fps;
                    Length = args.Length;
                }
                else
                {
                    // Only 1 will trigger here because only 1 item has changes
                    Length = args.Length;
                    Fps = args.Fps;
                }

                // Store the updated keys back into the redo buffer.
                args.Keys ??= StoreKeyframes();

                // Add each texture frame to the cache.

                foreach (IKeyFrame keyFrame in args.Keys.SelectMany(item => item.Value)
                                                        .Where(item => (item?.TextureValue.TextureFile is not null) && (item?.TextureValue.Texture is null)))
                {
                    await _contentServices.KeyProcessor.RestoreTextureAsync(keyFrame);
                }

                IsLooping = args.Looped;
                _animation.LoopCount = args.LoopCount;

                RebuildAnimation();

                // Reset selection.
                SelectDefault();

                ContentState = ContentState.Modified;
                CurrentPanel = null;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_APPLY_PROPERTIES);
                return false;
            }
            finally
            {
                HideWaitPanel();
            }

            return true;
        }

        undoArgs = new ResizeAnimationUndoRedoData
        {
            Fps = Fps,
            Length = Length,
            KeyCount = MaxKeyCount,
            Looped = IsLooping,
            LoopCount = _animation.LoopCount,
            Keys = StoreKeyframes()
        };

        redoArgs = new ResizeAnimationUndoRedoData
        {
            Fps = Properties.Fps,
            Length = Properties.Length,
            KeyCount = Properties.KeyCount,
            Looped = Properties.IsLooped,
            LoopCount = Properties.LoopCount
        };

        if (await ResizeAsync(redoArgs, CancellationToken.None))
        {
            _contentServices.UndoService.Record(Resources.GORANM_DESC_UNDO_ANIMATION_RESIZE, ResizeAsync, ResizeAsync, undoArgs, redoArgs);
            // Need to call this so the UI can register our updated undo stack.
            NotifyPropertyChanged(nameof(UndoCommand));
        }
    }

    /// <summary>
    /// Function to determine if the key editor can be activated or not.
    /// </summary>
    /// <returns><b>true</b> if the key editor can be activated, <b>false</b> if not.</returns>
    private bool CanActivateKeyEditor() => (_primarySprite.sprite is not null) && (((CurrentPanel is null) && (_selected.Count > 0)) || (CurrentPanel == KeyEditor.CurrentEditor));

    /// <summary>
    /// Function to activate the key editor.
    /// </summary>
    private void DoActivateKeyEditor()
    {
        try
        {
            CurrentPanel = null;

            if (CommandContext == KeyEditor)
            {
                CommandContext = null;
                RebuildAnimation();
                return;
            }

            CommandContext = KeyEditor;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_ACTIVATE_KEY_EDITOR);
        }
    }

    /// <summary>
    /// Function to determine if the animation can be saved.
    /// </summary>
    /// <param name="args">The save reason.</param>
    /// <returns><b>true</b> if the animation can be saved, <b>false</b> if not.</returns>
    private bool CanSave(SaveReason args) => (ContentState != ContentState.Unmodified) && (((CommandContext is null) && (CurrentPanel is null)) || (args != SaveReason.UserSave));

    /// <summary>
    /// Function to save the animation.
    /// </summary>
    /// <param name="args">The save reason.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoSaveAsync(SaveReason args)
    {
        try
        {
            ShowWaitPanel(Resources.GORANM_TEXT_SAVING);

            RebuildAnimation();

            await _contentServices.IOService.SaveAnimation(File.Path, _animation, _backImage.file, PrimarySpriteStartPosition, _primarySprite.spriteFile, _contentServices.KeyProcessor.GetKeyframeTextureFiles(Tracks), _unsupportedTracks);

            ContentState = ContentState.Unmodified;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_SAVE);
        }
        finally
        {
            HideWaitPanel();
        }
    }

    /// <summary>
    /// Function to determine if a new animation can be created.
    /// </summary>
    /// <returns><b>true</b> if a new animation can be created, <b>false</b> if not.</returns>
    private bool CanCreateAnimation() => (CommandContext is null) && (CurrentPanel is null);

    /// <summary>
    /// Function to create a new animation based on the current animation.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoCreateAnimationAsync()
    {
        try
        {
            IReadOnlyList<IContentFile> textureFiles = _contentServices.KeyProcessor.GetKeyframeTextureFiles(Tracks);

            // If this content is currently in a modified state, ask if we want to save first.
            if (ContentState != ContentState.Unmodified)
            {
                MessageResponse response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORANM_CONFIRM_CLOSE, File.Name), allowCancel: true);

                switch (response)
                {
                    case MessageResponse.Yes:
                        RebuildAnimation();
                        await _contentServices.IOService.SaveAnimation(File.Path, _animation, _backImage.file, PrimarySpriteStartPosition, _primarySprite.spriteFile, textureFiles, _unsupportedTracks);
                        ContentState = ContentState.Unmodified;
                        break;
                    case MessageResponse.Cancel:
                        return;
                }
            }

            string animationDirectory = Path.GetDirectoryName(File.Path).FormatDirectory('/');
            if (string.IsNullOrWhiteSpace(animationDirectory))
            {
                animationDirectory = "/";
            }

            (string newName, float length, float fps, IContentFile primarySpriteFile, IContentFile bgTextureFile) = _contentServices.NewAnimation
                                                                                                                                    .GetNewAnimationName(animationDirectory,
                                                                                                                                                         File.Name,
                                                                                                                                                         _primarySprite.spriteFile,
                                                                                                                                                         _backImage.file);

            if (newName is null)
            {
                return;
            }

            string animationPath = animationDirectory + newName.FormatFileName();

            IContentFile existingFile = ContentFileManager.GetFile(animationPath);
            if (existingFile is not null)
            {
                // We cannot overwrite a file that's already open for editing.
                if (existingFile.IsOpen)
                {
                    HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORANM_ERR_FILESYSTEM_ITEM_EXISTS, animationPath));
                    return;
                }

                if (HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORANM_CONFIRM_SPRITE_EXISTS, animationPath)) != MessageResponse.Yes)
                {
                    return;
                }
            }

            // Copy the current sprite state into a new file.
            ShowWaitPanel(Resources.GORANM_TEXT_SAVING);

            NotifyPropertyChanging(nameof(BackgroundImage));
            NotifyPropertyChanging(nameof(PrimarySprite));

            // Deallocate all textures for the current animation.
            _contentServices.KeyProcessor.UnloadTextureKeyframes(Tracks);

            // Remove all tracks and keyframes.
            Selected = [];
            Tracks.Clear();

            _contentServices.IOService.UnloadSprite(_primarySprite);
            (GorgonSprite primarySprite, IContentFile spriteTextureFile) = await _contentServices.IOService.LoadSpriteAsync(primarySpriteFile);
            _primarySprite = (primarySprite, primarySpriteFile, spriteTextureFile);

            // Load the background texture.
            _contentServices.IOService.UnloadBackgroundTexture(_backImage);
            _backImage = await _contentServices.IOService.LoadBackgroundTextureAsync(bgTextureFile);

            UpdatePrimarySprite();

            Length = length;
            Fps = fps;

            if ((primarySprite is not null) && (Settings.AddTextureTrackForPrimarySprite))
            {
                Tracks.Add(await _contentServices.ViewModelFactory.CreateDefaultTextureTrackAsync(_primarySprite, MaxKeyCount));
            }

            NotifyPropertyChanged(nameof(PrimarySprite));
            NotifyPropertyChanged(nameof(BackgroundImage));

            RebuildAnimation();

            // Write the updated data.
            File = await _contentServices.IOService.SaveAnimation(animationPath, _animation, _backImage.file, PrimarySpriteStartPosition, _primarySprite.spriteFile, textureFiles, _unsupportedTracks);

            // Remove all undo states since we're now working with a new file.
            ContentState = ContentState.Unmodified;
            _contentServices.UndoService.ClearStack();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_CREATE_ANIMATION);
            // If we fail to create the new sprite, we need to shut the editor down because our state
            // could be corrupt.
            await CloseContentCommand.ExecuteAsync(new CloseContentArgs(false));
        }
        finally
        {
            HideWaitPanel();
        }
    }

    /// <summary>Function to initialize the content.</summary>
    /// <param name="injectionParameters">Common view model dependency injection parameters from the application.</param>
    protected override void OnInitialize(AnimationContentParameters injectionParameters)
    {
        base.OnInitialize(injectionParameters);

        _unsupportedTracks = injectionParameters.ExcludedTracks ?? [];
        _animation = injectionParameters.Animation;
        Settings = injectionParameters.Settings;
        _primarySprite = (injectionParameters.PrimarySprite?.PrimarySprite, injectionParameters.PrimarySprite?.File, injectionParameters.PrimarySprite?.TextureFile);
        _primarySprite.sprite?.CopyTo(WorkingSprite);
        _controller = injectionParameters.Controller;
        _length = injectionParameters.Animation.Length;
        _fps = injectionParameters.Animation.Fps;
        _contentServices = injectionParameters.ContentServices;
        _looping = _animation?.IsLooped ?? false;
        _backImage = (injectionParameters.BackgroundTexture, injectionParameters.BackgroundTextureFile);
        AddTrack = injectionParameters.AddTrack;
        Properties = injectionParameters.Properties;
        KeyEditor = injectionParameters.KeyEditorContext;
        Tracks = injectionParameters.Tracks;
        _trackList.AddRange(Tracks);

        float x = 0;
        float y = 0;

        if (_primarySprite.sprite is not null)
        {
            x = _primarySprite.sprite.Position.X;
            y = _primarySprite.sprite.Position.Y;
        }

        if (File.Metadata.Attributes.TryGetValue(AnimationIOService.StartPositionAttrX, out string startX))
        {
            float.TryParse(startX, NumberStyles.Float, CultureInfo.InvariantCulture, out x);
        }

        if (File.Metadata.Attributes.TryGetValue(AnimationIOService.StartPositionAttrY, out string startY))
        {
            float.TryParse(startY, NumberStyles.Float, CultureInfo.InvariantCulture, out y);
        }

        if (WorkingSprite is not null)
        {
            _controller.Play(WorkingSprite, _animation);
            _controller.Pause();
        }

        _primaryStart = WorkingSprite.Position = new Vector2(x, y);

        if (PrimarySprite is not null)
        {
            PrimarySprite.Position = _primaryStart;
        }
    }

    /// <summary>Function called when the associated view is loaded.</summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        Settings.PropertyChanged += Settings_PropertyChanged;
        KeyEditor.WaitPanelActivated += KeyEditor_WaitPanelActivated;
        KeyEditor.WaitPanelDeactivated += KeyEditor_WaitPanelDeactivated;
        KeyEditor.PropertyChanged += KeyEditor_PropertyChanged;

        Tracks.CollectionChanged += Tracks_CollectionChanged;

        foreach (ITrack track in Tracks)
        {
            track.PropertyChanged += Track_PropertyChanged;
        }

        AddTrack.OkCommand = new EditorCommand<object>(DoAddTrack, CanAddTrack);
        Properties.OkCommand = new EditorCommand<object>(DoUpdateAnimationPropertiesAsync, CanUpdateAnimationProperties);
    }

    /// <summary>Function called when the associated view is unloaded.</summary>
    protected override void OnUnload()
    {
        KeyEditor.PropertyChanged -= KeyEditor_PropertyChanged;
        KeyEditor.WaitPanelActivated -= KeyEditor_WaitPanelActivated;
        KeyEditor.WaitPanelDeactivated -= KeyEditor_WaitPanelDeactivated;
        Settings.PropertyChanged -= Settings_PropertyChanged;

        Properties.OkCommand = null;
        AddTrack.OkCommand = null;

        foreach (ITrack track in _trackList)
        {
            track.PropertyChanged -= Track_PropertyChanged;
        }

        Tracks.CollectionChanged -= Tracks_CollectionChanged;

        _contentServices.IOService.UnloadBackgroundTexture(_backImage);
        _backImage = (null, null);

        _contentServices.KeyProcessor.UnloadTextureKeyframes(Tracks);
        _contentServices.IOService.UnloadSprite(_primarySprite);

        _primarySprite = (null, null, null);

        Tracks.Clear();

        base.OnUnload();
    }

    /// <summary>Function to determine the action to take when this content is closing.</summary>
    /// <returns>
    ///   <b>true</b> to continue with closing, <b>false</b> to cancel the close request.</returns>
    /// <remarks>PlugIn authors should override this method to confirm whether save changed content, continue without saving, or cancel the operation entirely.</remarks>
    protected override async Task<bool> OnCloseContentTaskAsync()
    {
        if (ContentState == ContentState.Unmodified)
        {
            return true;
        }

        MessageResponse response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORANM_CONFIRM_CLOSE, File.Name), allowCancel: true);

        switch (response)
        {
            case MessageResponse.Yes:
                await DoSaveAsync(SaveReason.ContentShutdown);
                return true;
            case MessageResponse.Cancel:
                return false;
            default:
                return true;
        }
    }

    /// <summary>Initializes a new instance of the <see cref="AnimationContent"/> class.</summary>
    public AnimationContent()
    {
        UpdateAnimationPreviewCommand = new EditorCommand<object>(DoUpdateAnimationPreview, CanUpdateAnimationPreview);
        PlayAnimationCommand = new EditorCommand<object>(DoPlayAnimation, CanPlayAnimation);
        StopAnimationCommand = new EditorCommand<object>(DoStopAnimation, CanStopAnimation);
        LoadSpriteCommand = new EditorAsyncCommand<IReadOnlyList<string>>(DoAssignPrimarySpriteAsync, CanAssignPrimarySprite);
        GetTrackKeyCommand = new EditorCommand<GetTrackKeyArgs>(DoGetTrackKey, CanGetTrackKey);
        ShowAddTrackCommand = new EditorCommand<object>(DoShowAddTrack, CanShowAddTrack);
        ShowAnimationPropertiesCommand = new EditorCommand<object>(DoShowProperties, CanShowProperties);
        UndoCommand = new EditorCommand<object>(DoUndoAsync, CanUndo);
        RedoCommand = new EditorCommand<object>(DoRedoAsync, CanRedo);
        SelectTrackAndKeysCommand = new EditorCommand<IReadOnlyList<(int trackIndex, IReadOnlyList<int> keyIndices)>>(DoSelectTracksAndKeys);
        DeleteTrackCommand = new EditorCommand<object>(DoDeleteTrack, CanDeleteTrack);
        ClearAnimationCommand = new EditorCommand<object>(DoClearAnimation, CanClearAnimation);
        FirstKeyCommand = new EditorCommand<object>(DoMoveToFirstKey, CanMoveToFirstKey);
        LastKeyCommand = new EditorCommand<object>(DoMoveToLastKey, CanMoveToLastKey);
        PrevKeyCommand = new EditorCommand<object>(DoMoveToPreviousKey, CanMoveToPrevKey);
        NextKeyCommand = new EditorCommand<object>(DoMoveToNextKey, CanMoveToNextKey);
        LoadBackgroundImageCommand = new EditorAsyncCommand<object>(DoLoadBackgroundImageAsync, CanLoadBackgroundImage);
        ClearBackgroundImageCommand = new EditorCommand<object>(DoClearBackgroundImage, CanClearBackgroundImage);
        ActivateKeyEditorCommand = new EditorCommand<object>(DoActivateKeyEditor, CanActivateKeyEditor);
        SaveContentCommand = new EditorAsyncCommand<SaveReason>(DoSaveAsync, CanSave);
        NewAnimationCommand = new EditorAsyncCommand<object>(DoCreateAnimationAsync, CanCreateAnimation);
    }
}
