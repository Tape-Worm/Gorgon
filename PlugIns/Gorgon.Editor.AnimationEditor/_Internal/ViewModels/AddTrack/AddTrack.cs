#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: April 4, 2019 8:59:29 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Gorgon.Animation;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// The view model for adding a track to the animation.
    /// </summary>
    internal class AddTrack
        : HostedPanelViewModelBase<AddTrackParameters>, IAddTrack
    {
        #region Variables.
        // The list of selected tracks.
        private readonly List<GorgonTrackRegistration> _selectedTracks = new();
        #endregion

        #region Properties.

        /// <summary>Property to return whether the panel is modal.</summary>
        public override bool IsModal => true;

        /// <summary>Property to return the list of available tracks to select.</summary>
        public ObservableCollection<GorgonTrackRegistration> AvailableTracks
        {
            get;
            private set;
        }

        /// <summary>Property to set or return the list of selected tracks to add.</summary>
        public IReadOnlyList<GorgonTrackRegistration> SelectedTracks
        {
            get => _selectedTracks;
            set
            {
                if (_selectedTracks == value)
                {
                    return;
                }

                OnPropertyChanging();
                _selectedTracks.Clear();

                if (value is not null)
                {
                    _selectedTracks.AddRange(value);
                }
                OnPropertyChanged();                
            }
        }

        /// <summary>
        /// Property to return the command used to select tracks.
        /// </summary>
        public IEditorCommand<IReadOnlyList<GorgonTrackRegistration>> SelectTracksCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>Function to select the tracks to add.</summary>
        /// <param name="tracks">The list of tracks to add.</param>
        private void DoSelectTracks(IReadOnlyList<GorgonTrackRegistration> tracks)
        {
            try
            {
                SelectedTracks = tracks;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_SELECTING_TRACK);
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(AddTrackParameters injectionParameters) => AvailableTracks = injectionParameters.AvailableTracks;

        /// <summary>Function called when the associated view is unloaded.</summary>
        /// <remarks>This method is used to perform tear down and clean up of resources.</remarks>        
        protected override void OnUnload()
        {
            _selectedTracks.Clear();
            base.Unload();
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="AddTrack"/> class.</summary>
        public AddTrack() => SelectTracksCommand = new EditorCommand<IReadOnlyList<GorgonTrackRegistration>>(DoSelectTracks);
        #endregion
    }
}
