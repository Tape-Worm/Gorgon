﻿
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
// Created: March 28, 2019 9:48:28 AM
// 

using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Gorgon.Animation;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A panel used to add new tracks to an animation
/// </summary>
internal partial class AnimationAddTrack
    : EditorSubPanelCommon, IDataContext<IAddTrack>
{
    /// <summary>
    /// A list box item for the track.
    /// </summary>
    /// <remarks>Initializes a new instance of the <see cref="TrackListItem"/> class.</remarks>
    /// <param name="track">The track.</param>
    private class TrackListItem(GorgonTrackRegistration track)
    {
        /// <summary>
        /// Property to return the track.
        /// </summary>
        public GorgonTrackRegistration TrackRegistration
        {
            get;
        } = track;

        /// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public override string ToString() => TrackRegistration.Description;
    }

    /// <summary>
    /// Property to return the data context for the view.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IAddTrack ViewModel
    {
        get;
        private set;
    }

    /// <summary>Handles the CollectionChanged event of the AvailableTracks control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
    private void AvailableTracks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        ListTracks.SelectedValueChanged -= ListTracks_SelectedValueChanged;

        try
        {
            ListTracks.SelectedItems.Clear();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    GorgonTrackRegistration added = (GorgonTrackRegistration)e.NewItems[0];

                    Debug.Assert(added is not null, "Must have a value to add to the list.");

                    if (ListTracks.Items.OfType<TrackListItem>().FirstOrDefault(item => item.TrackRegistration == added) is not null)
                    {
                        return;
                    }

                    ListTracks.Items.Add(new TrackListItem(added));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    GorgonTrackRegistration removed = (GorgonTrackRegistration)e.OldItems[0];

                    Debug.Assert(removed is not null, "Must have a value to remove from the list.");

                    TrackListItem listItem = ListTracks.Items.OfType<TrackListItem>().FirstOrDefault(item => item.TrackRegistration == removed);
                    if (listItem is null)
                    {
                        return;
                    }

                    ListTracks.Items.Remove(listItem);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ListTracks.Items.Clear();
                    break;
            }

            ValidateOk();
        }
        finally
        {
            ListTracks.SelectedValueChanged += ListTracks_SelectedValueChanged;
        }
    }

    /// <summary>
    /// Function to unassign the events from the data context.
    /// </summary>
    private void UnassignEvents()
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.AvailableTracks.CollectionChanged -= AvailableTracks_CollectionChanged;
    }

    /// <summary>Handles the SelectedValueChanged event of the ListTracks control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ListTracks_SelectedValueChanged(object sender, EventArgs e)
    {
        IReadOnlyList<GorgonTrackRegistration> selectedTracks = ListTracks.SelectedItems.OfType<TrackListItem>().Select(item => item.TrackRegistration).ToArray();

        if ((ViewModel?.SelectTracksCommand is null) || (!ViewModel.SelectTracksCommand.CanExecute(selectedTracks)))
        {
            return;
        }

        ViewModel.SelectTracksCommand.Execute(selectedTracks);
        ValidateOk();
    }

    /// <summary>Handles the DoubleClick event of the ListTracks control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ListTracks_DoubleClick(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ValidateOk();
        OnSubmit();
    }

    /// <summary>
    /// Function to populate the list with available track names.
    /// </summary>
    /// <param name="dataContext">The data context for the view.</param>
    private void FillList(IAddTrack dataContext)
    {
        ListTracks.BeginUpdate();
        ListTracks.Items.Clear();
        try
        {
            if (dataContext is null)
            {
                return;
            }

            for (int i = 0; i < dataContext.AvailableTracks.Count; ++i)
            {
                ListTracks.Items.Add(new TrackListItem(dataContext.AvailableTracks[i]));
            }
        }
        finally
        {
            ListTracks.EndUpdate();
        }
    }

    /// <summary>
    /// Function to reset the values on the control for a null data context.
    /// </summary>
    private void ResetDataContext() => FillList(null);

    /// <summary>
    /// Function to initialize the control with the data context.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>
    private void InitializeFromDataContext(IAddTrack dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        FillList(dataContext);
    }

    /// <summary>Function to submit the change.</summary>
    protected override void OnSubmit()
    {
        base.OnSubmit();

        if ((ViewModel?.OkCommand is null) || (!ViewModel.OkCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.OkCommand.Execute(null);
    }

    /// <summary>Function to cancel the change.</summary>
    protected override void OnCancel()
    {
        base.OnCancel();

        if ((ViewModel?.CancelCommand is null) || (!ViewModel.CancelCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.CancelCommand.Execute(null);
    }

    /// <summary>
    /// Function to validate the state of the OK button.
    /// </summary>
    /// <returns><b>true</b> if the OK button is valid, <b>false</b> if not.</returns>
    protected override bool OnValidateOk() => (ViewModel?.OkCommand is not null) && (ViewModel.OkCommand.CanExecute(null));

    /// <summary>Raises the <see cref="UserControl.Load"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (IsDesignTime)
        {
            return;
        }

        ViewModel?.Load();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IAddTrack dataContext)
    {
        UnassignEvents();

        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;

        if (ViewModel is null)
        {
            ValidateOk();
            return;
        }

        ViewModel.AvailableTracks.CollectionChanged += AvailableTracks_CollectionChanged;

        ValidateOk();
    }

    /// <summary>Initializes a new instance of the <see cref="AnimationAddTrack"/> class.</summary>
    public AnimationAddTrack() => InitializeComponent();

}
