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
// Created: June 14, 2020 10:21:02 PM
// 
#endregion

using System;
using Gorgon.Animation;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI.ViewModels;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// Parameters for a the <see cref="ITrack"/> view model.
/// </summary>
internal class TrackParameters
    : ViewModelInjection<IHostContentServices>
{
    #region Properties.
    /// <summary>
    /// Property to return the registration for the track.
    /// </summary>
    public GorgonTrackRegistration Registration
    {
        get;
    }

    /// <summary>
    /// Property to return the interpolation mode for the track.
    /// </summary>
    public TrackInterpolationMode InterpolationMode
    {
        get;
    }

    /// <summary>
    /// Property to return the types of interpolation supported by the track.
    /// </summary>
    public TrackInterpolationMode SupportedInterpolationMode
    {
        get;
    }

    /// <summary>
    /// Property to return the number of keys in this track.
    /// </summary>
    public int KeyCount
    {
        get;
    }

    /// <summary>
    /// Property to return the service that handles undo/redo functionality.
    /// </summary>
    public IUndoService UndoService
    {
        get;
    }

    /// <summary>
    /// Property to set or return the metadata for the key type in the track.
    /// </summary>
    public KeyValueMetadata KeyMetadata
    {
        get;
        set;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="TrackParameters{T}"/> class.</summary>
    /// <param name="registration">The registration data for the track.</param>
    /// <param name="interpolationMode">The type of interpolation for the track.</param>
    /// <param name="supportedInterpolationMode">The type of interpolation modes supported by the track.</param>
    /// <param name="keyCount">The number of keys in this track.</param>
    /// <param name="undoService">The service that handles undo/redo functionality.</param>        
    /// <param name="hostServices">The services from the host application.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
    public TrackParameters(GorgonTrackRegistration registration, 
                        TrackInterpolationMode interpolationMode, 
                        TrackInterpolationMode supportedInterpolationMode,
                        int keyCount,
                        IUndoService undoService,                            
                        IHostContentServices hostServices)
        : base(hostServices)
    {
        Registration = registration ?? throw new ArgumentNullException(nameof(registration));
        UndoService = undoService ?? throw new ArgumentNullException(nameof(undoService));
        SupportedInterpolationMode = supportedInterpolationMode;
        InterpolationMode = interpolationMode;
        KeyCount = keyCount;
    }
    #endregion
}
