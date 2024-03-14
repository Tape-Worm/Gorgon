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
// Created: June 14, 2020 10:18:07 PM
// 
#endregion

using Gorgon.Animation;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Math;
using Gorgon.Renderers;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A view model for a track.
/// </summary>
internal class Track
    : ViewModelBase<TrackParameters, IHostContentServices>, ITrack
{
    #region Classes.
    /// <summary>
    /// Undo/redo arguments for changing the track interpolation.
    /// </summary>
    private class TrackInterpolationUndoRedoArgs
    {
        /// <summary>
        /// The track interpolation mode.
        /// </summary>
        public TrackInterpolationMode Mode;
    }
    #endregion

    #region Variables.
    // The registration for the track.
    private GorgonTrackRegistration _registration;
    // The current track interpolation mode.
    private TrackInterpolationMode _interpolation;
    // Flag to indicate that interpolation is supported by the track.
    private TrackInterpolationMode _supportsInterpolation;
    // The list of key frames for the track.
    private IKeyFrame[] _keyFrames = [];
    // The service for undo/redo functionality.
    private IUndoService _undoService;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the sprite property updated by this track.
    /// </summary>
    public TrackSpriteProperty SpriteProperty
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to set or return the interpolation mode for the track.
    /// </summary>
    public TrackInterpolationMode InterpolationMode
    {
        get => _supportsInterpolation != TrackInterpolationMode.None ? _interpolation : TrackInterpolationMode.None;
        private set
        {
            if ((_interpolation == value) || ((_supportsInterpolation & value) != value))
            {
                return;
            }

            OnPropertyChanging();
            _interpolation = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the types of interpolation supported by the track.
    /// </summary>
    public TrackInterpolationMode InterpolationSupport
    {
        get => _supportsInterpolation;
        private set
        {
            if (_supportsInterpolation == value)
            {
                return;
            }

            OnPropertyChanging();
            _supportsInterpolation = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the friendly track description used for display.
    /// </summary>
    public string Description => _registration.Description;

    /// <summary>Property to return the name of this object.</summary>
    /// <remarks>For best practice, the name should only be set once during the lifetime of an object. Hence, this interface only provides a read-only implementation of this
    /// property.</remarks>
    public string Name => _registration.TrackName;

    /// <summary>Property to return the type of key data for this track.</summary>
    public AnimationTrackKeyType KeyType => _registration.KeyType;

    /// <summary>Property to return the ID for the track registration.</summary>
    public int ID => _registration.ID;

    /// <summary>
    /// Property to set or return the list of key frames for the track.
    /// </summary>
    public IReadOnlyList<IKeyFrame> KeyFrames
    {
        get => _keyFrames;
        set
        {
            value ??= [];

            if ((_keyFrames == value) || (value.SequenceEqual(_keyFrames)))
            {
                return;
            }

            OnPropertyChanging();

            for (int i = 0; i < _keyFrames.Length.Max(0).Min(value.Count); ++i)
            {
                _keyFrames[i] = value[i];
            }

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the metadata for the type of key in this track.
    /// </summary>
    public KeyValueMetadata KeyMetadata
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the command used to assign the interpolation mode for the track.
    /// </summary>
    public IEditorCommand<TrackInterpolationMode> SetInterpolationModeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to assign a list of key frames to the track.
    /// </summary>
    public IEditorCommand<SetKeyFramesArgs> SetKeyFramesCommand
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to assign the track ID property.
    /// </summary>
    private void SetTrackID() => SpriteProperty = Name switch
    {
        null => TrackSpriteProperty.None,
        nameof(GorgonSprite.Position) => TrackSpriteProperty.Position,
        nameof(GorgonSprite.Anchor) => TrackSpriteProperty.Anchor,
        nameof(GorgonSprite.AbsoluteAnchor) => TrackSpriteProperty.AnchorAbsolute,
        nameof(GorgonSprite.Size) => TrackSpriteProperty.Size,
        nameof(GorgonSprite.Scale) => TrackSpriteProperty.Scale,
        nameof(GorgonSprite.ScaledSize) => TrackSpriteProperty.ScaledSize,
        nameof(GorgonSprite.CornerOffsets.LowerLeft) => TrackSpriteProperty.LowerLeft,
        nameof(GorgonSprite.CornerOffsets.LowerRight) => TrackSpriteProperty.LowerRight,
        nameof(GorgonSprite.CornerOffsets.UpperLeft) => TrackSpriteProperty.UpperLeft,
        nameof(GorgonSprite.CornerOffsets.UpperRight) => TrackSpriteProperty.UpperRight,
        GorgonSpriteAnimationController.LowerLeftColorTrackName => TrackSpriteProperty.LowerLeftColor,
        GorgonSpriteAnimationController.LowerRightColorTrackName => TrackSpriteProperty.LowerRightColor,
        GorgonSpriteAnimationController.UpperLeftColorTrackName => TrackSpriteProperty.UpperLeftColor,
        GorgonSpriteAnimationController.UpperRightColorTrackName => TrackSpriteProperty.UpperRightColor,
        nameof(GorgonSprite.Angle) => TrackSpriteProperty.Angle,
        GorgonSpriteAnimationController.OpacityTrackName => TrackSpriteProperty.Opacity,
        nameof(GorgonSprite.Bounds) => TrackSpriteProperty.Bounds,
        nameof(GorgonSprite.Color) => TrackSpriteProperty.Color,
        nameof(GorgonSprite.Texture) => TrackSpriteProperty.Texture,
        _ => TrackSpriteProperty.None,
    };

    /// <summary>
    /// Function called to assign the interpolation mode for the track.
    /// </summary>
    /// <param name="mode">The interpolation mode.</param>
    private void DoSetInterpolation(TrackInterpolationMode mode)
    {
        TrackInterpolationUndoRedoArgs trackInterpUndoArgs;
        TrackInterpolationUndoRedoArgs trackInterpRedoArgs;

        bool SetInterpolation(TrackInterpolationMode interpolationMode)
        {
            try
            {
                InterpolationMode = interpolationMode;
                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GORANM_ERR_TRACK_INTERPOLATION, Name));
                return false;
            }
        }

        Task UndoRedoAction(TrackInterpolationUndoRedoArgs args, CancellationToken cancelToken)
        {
            SetInterpolation(args.Mode);
            return Task.CompletedTask;
        }

        trackInterpUndoArgs = new TrackInterpolationUndoRedoArgs
        {
            Mode = InterpolationMode
        };

        trackInterpRedoArgs = new TrackInterpolationUndoRedoArgs
        {
            Mode = mode
        };

        if (SetInterpolation(mode))
        {
            _undoService.Record(string.Format(Resources.GORANM_DESC_UNDO_TRACK_INTERPOLATION, Name), UndoRedoAction, UndoRedoAction, trackInterpUndoArgs, trackInterpRedoArgs);
            NotifyPropertyChanged(nameof(InterpolationMode));
        }
    }

    /// <summary>
    /// Function to assign a list of key frames to the track.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    private void DoSetKeyFrames(SetKeyFramesArgs args)
    {
        HostServices.BusyService.SetBusy();

        try
        {
            NotifyPropertyChanging(nameof(KeyFrames));

            int keyFrameCount = args.MaxKeyFrameCount;

            if (keyFrameCount != _keyFrames.Length)
            {
                Array.Resize(ref _keyFrames, keyFrameCount);
            }

            for (int i = 0; i < _keyFrames.Length; ++i)
            {
                _keyFrames[i] = args.KeyFrames[i];
            }

            NotifyPropertyChanged(nameof(KeyFrames));
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GORANM_ERR_ASSIGN_KEYFRAMES, Name));
        }
        finally
        {
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>
    ///   <para>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </para>
    ///   <para>
    /// This method is only ever called after the view model has been created, and never again during the lifetime of the view model.
    /// </para>
    /// </remarks>
    protected override void OnInitialize(TrackParameters injectionParameters)
    {
        _registration = injectionParameters.Registration;
        _supportsInterpolation = injectionParameters.SupportedInterpolationMode;
        _interpolation = injectionParameters.InterpolationMode;
        _undoService = injectionParameters.UndoService;
        KeyMetadata = injectionParameters.KeyMetadata;

        _keyFrames = new IKeyFrame[injectionParameters.KeyCount.Max(1)];

        SetTrackID();
    }
    #endregion

    #region Constructor.
    /// <summary>Initializes a new instance of the <see cref="Track"/> class.</summary>
    public Track()
    {
        SetInterpolationModeCommand = new EditorCommand<TrackInterpolationMode>(DoSetInterpolation);
        SetKeyFramesCommand = new EditorCommand<SetKeyFramesArgs>(DoSetKeyFrames);
    }
    #endregion
}
