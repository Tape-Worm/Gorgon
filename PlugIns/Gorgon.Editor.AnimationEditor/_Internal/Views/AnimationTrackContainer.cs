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
// Created: June 10, 2020 4:04:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Math;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using System.Collections.Specialized;
using System.Threading;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// A container component for handling animation tracks and key frames.
    /// </summary>
    internal partial class AnimationTrackContainer
        : UserControl, IDataContext<IAnimationContent>
    {
        #region Variables.
        // The style to apply to column headers when a playing animation is on a specific key frame column.
        private readonly DataGridViewCellStyle _headerActive;
        private readonly DataGridViewCellStyle _activeCellStyle;
        // The image to display for key frames with data.
        private readonly Image _keyHasDataImage = Resources.key_with_data_16x16;
        // The reference count for the selection event assignment.
        private int _selectionEvent = 1;
        // The data to copy/move.
        private readonly KeyFrameCopyMoveData _copyMoveData = new();
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IAnimationContent DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to disable grid events.
        /// </summary>
        private void DisableGridEvents()
        {
            if (Interlocked.Exchange(ref _selectionEvent, 0) == 0)
            {
                return;
            }

            GridTrackKeys.SelectionChanged -= GridTrackKeys_SelectionChanged;
            GridTrackKeys.CellValueChanged -= GridTrackKeys_CellValueChanged;
        }

        /// <summary>
        /// Function to enable grid events.
        /// </summary>
        private void EnableGridEvents()
        {
            if (Interlocked.Exchange(ref _selectionEvent, 1) == 1)
            {
                return;
            }

            GridTrackKeys.CellValueChanged += GridTrackKeys_CellValueChanged;
            GridTrackKeys.SelectionChanged += GridTrackKeys_SelectionChanged;
        }

        /// <summary>Handles the CollectionChanged event of the Tracks control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void Tracks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ITrack track;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    track = (ITrack)e.NewItems[0];

                    track.PropertyChanged += Track_PropertyChanged;
                    AddGridRow(track);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    track = (ITrack)e.OldItems[0];

                    track.PropertyChanged -= Track_PropertyChanged;
                    RemoveGridRow(track);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (ITrack clearTrack in GridTrackKeys.Rows.OfType<DataGridViewRow>().Select(item => (ITrack)item.Tag))
                    {
                        clearTrack.PropertyChanged -= Track_PropertyChanged;
                    }

                    // Empty the grid, we no longer have data.
                    GridTrackKeys.ClearSelection();
                    GridTrackKeys.Rows.Clear();
                    GridTrackKeys.Columns.Clear();
                    break;
            }
        }

        /// <summary>Handles the PropertyChanged event of the Track control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void Track_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var track = (ITrack)sender;
            DataGridViewRow row;

            DisableGridEvents();

            try
            {
                switch (e.PropertyName)
                {
                    case nameof(ITrack.KeyFrames):
                        row = GridTrackKeys.Rows.OfType<DataGridViewRow>().FirstOrDefault(item => item.Tag == track);

                        if (row is null)
                        {
                            break;
                        }

                        int colStart = GridTrackKeys.FirstDisplayedScrollingColumnIndex;
                        int colEnd = ((GridTrackKeys.DisplayedColumnCount(false) - 2) + colStart).Min(GridTrackKeys.ColumnCount - 1).Max(1);

                        for (int i = colStart; i <= colEnd; ++i)
                        {
                            GridTrackKeys.InvalidateColumn(i);
                        }
                        break;
                    case nameof(ITrack.InterpolationMode):
                        row = GridTrackKeys.Rows.OfType<DataGridViewRow>().FirstOrDefault(item => item.Tag == track);

                        if (row is null)
                        {
                            break;
                        }

                        row.Cells[0].Value = track.InterpolationMode.GetDescription();
                        GridTrackKeys.InvalidateColumn(0);
                        break;
                }
            }
            finally
            {
                EnableGridEvents();
            }
        }

        /// <summary>
        /// Function to fill in the drop down for the interpolation column.
        /// </summary>
        /// <param name="row">The row representing the track.</param>
        private void FillInterpolationCell(DataGridViewRow row)
        {
            var cell = (DataGridViewComboBoxCell)row.Cells[0];
            var track = (ITrack)row.Tag;

            cell.Items.Clear();

            foreach (TrackInterpolationMode mode in track.InterpolationSupport.GetExplicitValues())
            {
                cell.Items.Add(mode.GetDescription());
            }

            cell.ReadOnly = cell.Items.Count < 2;
        }

        /// <summary>
        /// Function to set up the columns as key frames in the grid.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        /// <param name="keyCount">[Optional] The key count to use.</param>
        private void SetupGridColumns(IAnimationContent dataContext, int? keyCount = null)
        {
            GridTrackKeys.Columns.Clear();

            if (dataContext is null)
            {
                return;
            }

            if (keyCount is null)
            {
                keyCount = dataContext.MaxKeyCount;
            }

            var columns = new DataGridViewColumn[keyCount.Value + 1];
            float weight = 1.0f / columns.Length;

            columns[0] = new DataGridViewComboBoxColumn
            {
                FillWeight = weight,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                Resizable = DataGridViewTriState.False,
                HeaderText = Resources.GORANM_TEXT_INTERPOLATION,
                Name = $"TrackInterpolation",
                ReadOnly = false,
                DataPropertyName = null,
                Frozen = true,                
                FlatStyle = FlatStyle.Flat                
            };

            // We'll use our columns as key values.
            for (int i = 0; i < keyCount.Value; ++i)
            {
                float startTime = i / dataContext.Fps;
                float endTime = startTime + (1.0f / dataContext.Fps);

                columns[i + 1] = new DataGridViewImageColumn
                {
                    FillWeight = weight,
                    Width = 48,
                    Resizable = DataGridViewTriState.False,
                    HeaderText = $"{i + 1}",
                    Name = $"Key{i}",
                    ImageLayout = DataGridViewImageCellLayout.Normal,
                    ReadOnly = true,
                    DataPropertyName = null,
                    // Show the time for the frame, in seconds, within the tool tip for the column.
                    ToolTipText = $"{startTime:0.0##} - {endTime:0.0##} {Resources.GORANM_TEXT_SECONDS}."
                };
                columns[i + 1].DefaultCellStyle.NullValue = null;
            }

            GridTrackKeys.Columns.AddRange(columns);
        }

        /// <summary>
        /// Function to add a grid row representing a new animation track.
        /// </summary>
        /// <param name="track">The track to add.</param>
        private void AddGridRow(ITrack track)
        {
            // Don't allow duplication.
            if (GridTrackKeys.Rows.OfType<DataGridViewRow>()
                                  .Select(item => (ITrack)item.Tag)
                                  .Any(item => (item == track) || (item.ID == track.ID)))
            {
                return;
            }

            // If we drop a sprite onto an empty animation, then we'll have no columns and need to rebuild them.
            if (GridTrackKeys.Columns.Count == 0)
            {
                SetupGridColumns(DataContext);
            }

            var row = new DataGridViewRow
            {
                Tag = track
            };
            row.HeaderCell.Value = track.Description;
            row.CreateCells(GridTrackKeys);

            FillInterpolationCell(row);

            GridTrackKeys.Rows.Add(row);
        }

        /// <summary>
        /// Function to remove a grid row representing an existing animation track.
        /// </summary>
        /// <param name="track">The track to remove.</param>
        private void RemoveGridRow(ITrack track)
        {
            // Don't allow duplication.
            DataGridViewRow row = (from gridRow in GridTrackKeys.Rows.OfType<DataGridViewRow>()
                                   let gridTrack = (ITrack)gridRow.Tag
                                   where gridTrack.ID == track.ID
                                   select gridRow).FirstOrDefault();

            if (row is null)
            {
                return;
            }

            GridTrackKeys.Rows.Remove(row);

            if (GridTrackKeys.Rows.Count == 0)
            {
                GridTrackKeys.Columns.Clear();
            }
        }

        /// <summary>
        /// Property to set or return the current animation time for a playing animation.
        /// </summary>
        /// <param name="time">The current time in the animation.</param>
        private void AnimationTimeUpdated(float time)
        {
            if (DataContext is null)
            {
                return;
            }

            int index = ((int)(time * DataContext.Fps).Round()) + 1;

            if ((index < 1) || (index >= GridTrackKeys.ColumnCount))
            {
                return;
            }

            if (DataContext.State != AnimationState.Playing)
            {
                // If we've stopped playing, reset the entire grid.
                for (int x = 1; x < GridTrackKeys.ColumnCount; ++x)
                {
                    GridTrackKeys.Columns[x].HeaderCell.Style = null;
                }

                for (int y = 0; y < GridTrackKeys.RowCount; ++y)
                {
                    for (int x = 1; x < GridTrackKeys.ColumnCount; ++x)
                    {
                        GridTrackKeys.Rows[y].Cells[x].Style = null;
                    }
                }

                return;
            }

            int firstRow = GridTrackKeys.FirstDisplayedScrollingRowIndex;
            int lastRow = ((GridTrackKeys.DisplayedRowCount(false) - 1) + firstRow).Min(GridTrackKeys.RowCount - 1).Max(0);
            int firstCol = GridTrackKeys.FirstDisplayedScrollingColumnIndex;
            int lastCol = ((GridTrackKeys.DisplayedColumnCount(false) - 2) + firstCol).Min(GridTrackKeys.ColumnCount - 1).Max(1);

            // Scroll into view.
            if ((index != -1) && ((index < firstCol) || (index > lastCol)))
            {
                firstCol = GridTrackKeys.FirstDisplayedScrollingColumnIndex = index;
                lastCol = ((GridTrackKeys.DisplayedColumnCount(false) - 2) + firstCol).Min(GridTrackKeys.ColumnCount - 1).Max(1);
                GridTrackKeys.Refresh();
            }

            for (int x = firstCol; x <= lastCol; ++x)
            {
                GridTrackKeys.Columns[x].HeaderCell.Style = x == index ? _headerActive : null;
            }

            for (int y = firstRow; y <= lastRow; ++y)
            {
                DataGridViewRow row = GridTrackKeys.Rows[y];

                for (int x = firstCol; x <= lastCol; ++x)
                {
                    row.Cells[x].Style = x == index ? _activeCellStyle : null;
                }
            }
        }

        /// <summary>
        /// Function to select cells in the grid based on track/key selection.
        /// </summary>
        private void SelectCells()
        {
            if (GridTrackKeys.Rows.Count == 0)
            {
                return;
            }

            DisableGridEvents();
            try
            {
                GridTrackKeys.ClearSelection();
                if ((DataContext.Selected.Count > 0) && (DataContext.Selected[0].SelectedKeys.Count > 0))
                {
                    int selRowIndex = DataContext.Selected[0].TrackIndex;
                    int selColIndex = DataContext.Selected[0].SelectedKeys[0].KeyIndex + 1;

                    GridTrackKeys.CurrentCell = GridTrackKeys.Rows[selRowIndex].Cells[selColIndex];

                    for (int j = 0; j < DataContext.Selected.Count; ++j)
                    {
                        TrackKeySelection selection = DataContext.Selected[j];
                        for (int i = 0; i < selection.SelectedKeys.Count; ++i)
                        {
                            GridTrackKeys.Rows[selection.TrackIndex].Cells[selection.SelectedKeys[i].KeyIndex + 1].Selected = true;
                        }
                    }

                    return;
                }

                int rowIndex = DataContext.Selected.Count > 0 ? DataContext.Selected[0].TrackIndex : 0;
                GridTrackKeys.CurrentCell = GridTrackKeys.Rows[rowIndex].Cells[0];
            }
            finally
            {
                EnableGridEvents();
            }
        }

        /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
        }

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IAnimationContent.Length):
                case nameof(IAnimationContent.Fps):
                    FillGrid(DataContext);
                    break;
                case nameof(IAnimationContent.Selected):
                    SelectCells();
                    break;
                case nameof(IAnimationContent.PreviewKeyTime):
                case nameof(IAnimationContent.State):
                    AnimationTimeUpdated(DataContext.PreviewKeyTime);
                    break;
            }
        }

        /// <summary>
        /// Function to retrieve the indices for the selected tracks and key frames.
        /// </summary>
        /// <returns>The selection data.</returns>
        private IReadOnlyList<(int trackIndex, IReadOnlyList<int> keyIndices)> GetTrackKeySelectionIndices()
        {
            IEnumerable<DataGridViewCell> cells = GridTrackKeys.SelectedCells.OfType<DataGridViewCell>();
            bool hasInterpSelected = cells.Any(item => item.ColumnIndex == 0);

            if (hasInterpSelected)
            {
                int lastSelRowIndex = GridTrackKeys.SelectedCells[GridTrackKeys.SelectedCells.Count - 1].RowIndex;
                // Do not multi select the interpolation cell.                    
                if (GridTrackKeys.SelectedCells.Count > 1)
                {
                    GridTrackKeys.ClearSelection();
                    GridTrackKeys.Rows[lastSelRowIndex].Cells[0].Selected = true;
                }
                return new[] { 
                    (lastSelRowIndex, (IReadOnlyList<int>)Array.Empty<int>())
                };
            }

            List<int> keyIndices = null;
            int rowID = int.MinValue;
            var result = new List<(int trackIndex, IReadOnlyList<int> keyIndices)>();
            foreach (DataGridViewCell cell in GridTrackKeys.SelectedCells.OfType<DataGridViewCell>().OrderBy(c => c.RowIndex))
            {
                if (rowID != cell.RowIndex)
                {
                    keyIndices = new List<int>();
                    result.Add((cell.RowIndex, keyIndices));
                    rowID = cell.RowIndex;                    
                }

                int columnIndex = cell.ColumnIndex - 1;

                if (!keyIndices.Contains(columnIndex))
                {
                    keyIndices.Add(columnIndex);
                }
            }

            return result;
        }

        /// <summary>Handles the SizeChanged event of the GridTrackKeys control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void GridTrackKeys_SizeChanged(object sender, EventArgs e)
        {
            if (GridTrackKeys.SelectedCells.Count == 0)
            {
                return;
            }

            DataGridViewCell lastSelCell = GridTrackKeys.SelectedCells[GridTrackKeys.SelectedCells.Count - 1];
            int colStart = GridTrackKeys.FirstDisplayedScrollingColumnIndex;
            int colEnd = colStart + GridTrackKeys.DisplayedColumnCount(false) - 2;
            int rowStart = GridTrackKeys.FirstDisplayedScrollingRowIndex;
            int rowEnd = rowStart + GridTrackKeys.DisplayedRowCount(false);
            if ((lastSelCell is null) 
                || ((lastSelCell.RowIndex >= rowStart) && (lastSelCell.RowIndex <= rowEnd) && (lastSelCell.ColumnIndex >= colStart) && (lastSelCell.ColumnIndex <= colEnd)))
            {
                return;
            }

            GridTrackKeys.FirstDisplayedCell = lastSelCell;
        }

        /// <summary>Handles the CellValueChanged event of the GridTrackKeys control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellEventArgs"/> instance containing the event data.</param>
        private void GridTrackKeys_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DisableGridEvents();

            try
            {
                if ((e.ColumnIndex != 0) || (e.RowIndex < 0) || (e.RowIndex >= GridTrackKeys.Rows.Count))
                {
                    return;
                }

                var track = GridTrackKeys.Rows[e.RowIndex].Tag as ITrack;
                TrackInterpolationMode value = GridTrackKeys.Rows[e.RowIndex].Cells[0].EditedFormattedValue.IfNull(string.Empty).GetTrackInterpolationMode();

                if ((track?.SetInterpolationModeCommand is null) || (!track.SetInterpolationModeCommand.CanExecute(value)))
                {
                    return;
                }

                track.SetInterpolationModeCommand.Execute(value);
            }
            finally
            {
                EnableGridEvents();
            }
        }

        /// <summary>Handles the SelectionChanged event of the GridTrackKeys control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void GridTrackKeys_SelectionChanged(object sender, EventArgs e)
        {
            DisableGridEvents();

            try
            {
                IReadOnlyList<(int trackIndex, IReadOnlyList<int> keyIndices)> args = GetTrackKeySelectionIndices();

                if ((DataContext?.SelectTrackAndKeysCommand is null) || (!DataContext.SelectTrackAndKeysCommand.CanExecute(args)))
                {
                    return;
                }

                DataContext.SelectTrackAndKeysCommand.Execute(args);
            }
            finally
            {
                EnableGridEvents();
            }
        }

        /// <summary>Handles the RowHeaderMouseClick event of the GridTrackKeys control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellMouseEventArgs"/> instance containing the event data.</param>
        private void GridTrackKeys_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DisableGridEvents();
            try
            {
                GridTrackKeys.ClearSelection();
                GridTrackKeys.CurrentCell = GridTrackKeys.Rows[e.RowIndex].Cells[0];
                GridTrackKeys.Rows[e.RowIndex].Cells[0].Selected = true;

                GridTrackKeys_SelectionChanged(GridTrackKeys, EventArgs.Empty);
            }
            finally
            {
                EnableGridEvents();                
            }
        }

        /// <summary>Handles the CellValueNeeded event of the GridTrackKeys control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellValueEventArgs"/> instance containing the event data.</param>
        private void GridTrackKeys_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (DataContext?.GetTrackKeyCommand is null)
            {
                return;
            }

            if ((e.ColumnIndex == 0) && (e.RowIndex < DataContext.Tracks.Count))
            {
                e.Value = DataContext.Tracks[e.RowIndex].InterpolationMode.GetDescription();
                return;
            }

            var args = new GetTrackKeyArgs(e.ColumnIndex - 1, e.RowIndex);
            if (!DataContext.GetTrackKeyCommand.CanExecute(args))
            {
                return;
            }

            DataContext.GetTrackKeyCommand.Execute(args);

            if (args.Key is not null)
            {
                e.Value = _keyHasDataImage;
            }
        }

        /// <summary>Handles the DragDrop event of the GridTrackKeys control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private async void GridTrackKeys_DragDrop(object sender, DragEventArgs e)
        {
            if ((DataContext?.KeyEditor is null) || (!e.Data.GetDataPresent(typeof(KeyFrameCopyMoveData))))
            {
                return;
            }

            var data = (KeyFrameCopyMoveData)e.Data.GetData(typeof(KeyFrameCopyMoveData));

            if (data?.KeyFrames is null)
            {
                return;
            }

            if ((DataContext.KeyEditor.CopyMoveFramesCommand is null) || (!DataContext.KeyEditor.CopyMoveFramesCommand.CanExecute(data)))
            {
                return;
            }

            await DataContext.KeyEditor.CopyMoveFramesCommand.ExecuteAsync(data);
        }

        /// <summary>Handles the DragOver event of the GridTrackKeys control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void GridTrackKeys_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            if (DataContext?.KeyEditor is null)
            {
                return;
            }

            if (!e.Data.GetDataPresent(typeof(KeyFrameCopyMoveData)))
            {
                return;
            }

            Point clientPos = GridTrackKeys.PointToClient(new Point(e.X, e.Y));
            int buffer = (int)(GridTrackKeys.ClientSize.Width * 0.05f).Min(16).Max(4);
            if ((clientPos.X > GridTrackKeys.Right - buffer) && (GridTrackKeys.FirstDisplayedScrollingColumnIndex < GridTrackKeys.ColumnCount - 2))
            {
                GridTrackKeys.FirstDisplayedScrollingColumnIndex++;
            }

            if ((clientPos.X < buffer) && (GridTrackKeys.FirstDisplayedScrollingColumnIndex > 1))
            {
                GridTrackKeys.FirstDisplayedScrollingColumnIndex--;
            }

            DataGridView.HitTestInfo info = GridTrackKeys.HitTest(clientPos.X, clientPos.Y);

            if (info is null)
            {
                return;
            }

            var data = (KeyFrameCopyMoveData)e.Data.GetData(typeof(KeyFrameCopyMoveData));

            if (data?.KeyFrames is null)
            {
                return;
            }

            data.DestinationKeyIndex = info.ColumnIndex - 1;

            if ((DataContext.KeyEditor.CopyMoveFramesCommand is null) || (!DataContext.KeyEditor.CopyMoveFramesCommand.CanExecute(data)))
            {
                return;
            }

            e.Effect = data.Operation == CopyMoveOperation.Move ? DragDropEffects.Move : DragDropEffects.Copy;
        }

        /// <summary>Handles the CellsDrag event of the GridTrackKeys control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CellsDragEventArgs"/> instance containing the event data.</param>
        private void GridTrackKeys_CellsDrag(object sender, CellsDragEventArgs e)
        {
            if ((DataContext?.Selected is null) || (e.DraggedCells.Any(cell => cell.ColumnIndex == 0)))
            {
                return;
            }

            var data = new DataObject();
            _copyMoveData.Operation = ((ModifierKeys & Keys.Shift) == Keys.Shift) ? CopyMoveOperation.Copy : CopyMoveOperation.Move;
            _copyMoveData.KeyFrames = DataContext.Selected;
            _copyMoveData.DestinationKeyIndex = -1;
            data.SetData(typeof(KeyFrameCopyMoveData).FullName, true, _copyMoveData);

            GridTrackKeys.DoDragDrop(data, DragDropEffects.Copy | DragDropEffects.Move);
        }

        /// <summary>
        /// Function to unassign events from the current data context.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext is null)
            {
                return;
            }

            foreach (ITrack track in DataContext.Tracks)
            {
                track.PropertyChanged -= Track_PropertyChanged;
            }

            DataContext.Tracks.CollectionChanged -= Tracks_CollectionChanged;
            DataContext.PropertyChanged -= DataContext_PropertyChanged;
            DataContext.PropertyChanging -= DataContext_PropertyChanging;
        }

        /// <summary>
        /// Function to reset the view back to its original state.
        /// </summary>
        private void ResetDataContext() => FillGrid(null);

        /// <summary>
        /// Function to populate the grid with keys and tracks.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void FillGrid(IAnimationContent dataContext)
        {
            GridTrackKeys.Rows.Clear();
            GridTrackKeys.Columns.Clear();

            if (dataContext is null)
            {
                return;
            }

            GridTrackKeys.ClearSelection();

            SetupGridColumns(dataContext);

            // Add any tracks that we've created previously.
            var rows = new DataGridViewRow[dataContext.Tracks.Count];
            for (int i = 0; i < rows.Length; ++i)
            {
                var track = new DataGridViewRow
                {
                    Tag = dataContext.Tracks[i]
                };
                track.HeaderCell.Value = dataContext.Tracks[i].Description;
                track.CreateCells(GridTrackKeys);
                rows[i] = track;

                dataContext.Tracks[i].PropertyChanged += Track_PropertyChanged;

                FillInterpolationCell(rows[i]);
            }

            GridTrackKeys.Rows.AddRange(rows);
        }

        /// <summary>
        /// Function to initialize the view from the data context.
        /// </summary>
        /// <param name="dataContext">The data context to apply to the view.</param>
        private void InitializeFromDataContext(IAnimationContent dataContext)
        {
            ResetDataContext();

            if (dataContext is null)
            {                
                return;
            }

            // If we have no length, or any tracks, then leave.
            if ((dataContext.Length.EqualsEpsilon(0)) || (!dataContext.HasTracks))
            {
                return;
            }

            FillGrid(dataContext);
        }

        /// <summary>Raises the <see cref="UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            GridTrackKeys.ClearSelection();

            if (GridTrackKeys.Rows.Count == 0)
            {
                return;
            }

            if (GridTrackKeys.Columns.Count > 1)
            {
                GridTrackKeys.Rows[0].Cells[1].Selected = true;
            }
            else
            {
                GridTrackKeys.Rows[0].Cells[0].Selected = true;
            }
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IAnimationContent dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);

            DataContext = dataContext;

            if (DataContext is null)
            {
                return;
            }

            DataContext.PropertyChanged += DataContext_PropertyChanged;
            DataContext.PropertyChanging += DataContext_PropertyChanging;
            DataContext.Tracks.CollectionChanged += Tracks_CollectionChanged;
        }        
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="AnimationTrackContainer"/> class.</summary>
        public AnimationTrackContainer()
        {
            InitializeComponent();

            var selected = GorgonColor.Lerp(Color.LimeGreen, GridTrackKeys.DefaultCellStyle.SelectionBackColor, 0.4f);            

            _activeCellStyle = new DataGridViewCellStyle(GridTrackKeys.DefaultCellStyle)
            {
                SelectionBackColor = selected,
                SelectionForeColor = DarkFormsRenderer.FocusedForeground,
                BackColor = Color.LimeGreen,
                ForeColor = DarkFormsRenderer.CutForeground
            };
            
            _headerActive = new DataGridViewCellStyle(GridTrackKeys.ColumnHeadersDefaultCellStyle)
            {
                SelectionBackColor = selected,
                SelectionForeColor = DarkFormsRenderer.FocusedForeground,
                BackColor = Color.LimeGreen,
                ForeColor = DarkFormsRenderer.CutForeground
            };
        }
        #endregion
    }
}
