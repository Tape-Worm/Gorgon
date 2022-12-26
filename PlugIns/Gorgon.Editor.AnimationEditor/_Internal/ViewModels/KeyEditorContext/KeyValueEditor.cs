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
// Created: July 4, 2020 10:03:17 PM
// 
#endregion

using System.Numerics;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Renderers;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A view model for the keyframe value editor.
/// </summary>
internal class KeyValueEditor
    : HostedPanelViewModelBase<HostedPanelViewModelParameters>, IKeyValueEditor
{
    #region Variables.
    // The track that is being edited.
    private TrackKeySelection _editTrack;
    // The floating point values.
    private Vector4 _floatValues;
    // The sprite to update.
    private GorgonSprite _workingSprite;
    #endregion

    #region Properties.
    /// <summary>Property to return the title for the editor.</summary>
    public string Title => string.IsNullOrWhiteSpace(_editTrack?.Track.Description) ? Resources.GORANM_DEFAULT_TITLE : _editTrack?.Track.Description;

    /// <summary>Property to return whether the panel is modal.</summary>
    public override bool IsModal => false;

    /// <summary>
    /// Property to set or return the working sprite to update.
    /// </summary>
    public GorgonSprite WorkingSprite
    {
        get => _workingSprite;
        set
        {
            if (_workingSprite == value)
            {
                return;
            }

            OnPropertyChanging();
            _workingSprite = value;
            if (_editTrack is not null)
            {
                Value = _workingSprite?.GetFloatValues(_editTrack.Track.SpriteProperty) ?? Vector4.Zero;
            }
            OnPropertyChanged();
        }
    }

    /// <summary>Property to set or return the track being edited.</summary>
    public TrackKeySelection Track
    {
        get => _editTrack;
        set
        {
            if (_editTrack == value)
            {
                return;
            }

            OnPropertyChanging();
            NotifyPropertyChanging(nameof(Title));
            _editTrack = value;
            NotifyPropertyChanged(nameof(Title));
            OnPropertyChanged();

            OnTrackSet();
        }
    }

    /// <summary>Property to set or return the value for the key frame.</summary>
    public Vector4 Value
    {
        get => _floatValues;
        set
        {
            if (_floatValues.Equals(value))
            {
                return;
            }

            OnPropertyChanging();

            _floatValues = value;
            
            if ((_editTrack is not null) && (_workingSprite is not null))
            {
                _workingSprite.SetFloatValues(_editTrack.Track.SpriteProperty, _floatValues);
            }
            OnPropertyChanged();
        }
    }
    #endregion

    #region Methods.
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
    protected override void OnInitialize(HostedPanelViewModelParameters injectionParameters)
    {

    }

    /// <summary>
    /// Function used to notify when the <see cref="Track"/> property is assigned.
    /// </summary>
    protected virtual void OnTrackSet()
    {        
    }
    #endregion
}
