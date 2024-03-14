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
// Created: March 24, 2020 10:38:50 PM
// 
#endregion

using System;
using System.ComponentModel;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// Settings for the burn effect.
/// </summary>
internal partial class Fx1BitSettings
    : EditorSubPanelCommon, IDataContext<IFxOneBit>
{
    #region Variables.

    #endregion

    #region Properties.
    /// <summary>Property to return the data context assigned to this view.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IFxOneBit ViewModel
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>Handles the Click event of the CheckInvert control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void CheckInvert_Click(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Invert = CheckInvert.Checked;
    }

    /// <summary>Handles the ValueChanged event of the NumericMinThreshold control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericMinThreshold_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.MinWhiteThreshold = TrackMinThreshold.Value = (int)NumericMinThreshold.Value;            
    }

    /// <summary>Handles the ValueChanged event of the NumericMaxThreshold control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericMaxThreshold_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.MaxWhiteThreshold = TrackMaxThreshold.Value = (int)NumericMaxThreshold.Value;
    }

    /// <summary>Handles the ValueChanged event of the TrackMinThreshold control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void TrackMinThreshold_ValueChanged(object sender, EventArgs e) => NumericMinThreshold.Value = TrackMinThreshold.Value;

    /// <summary>Handles the ValueChanged event of the TrackMaxThresold control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void TrackMaxThresold_ValueChanged(object sender, EventArgs e) => NumericMaxThreshold.Value = TrackMaxThreshold.Value;

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

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IFxOneBit.Invert):
                CheckInvert.Checked = ViewModel.Invert;
                break;
            case nameof(IFxOneBit.MinWhiteThreshold):
                NumericMinThreshold.Value = TrackMinThreshold.Value = ViewModel.MinWhiteThreshold;
                break;
            case nameof(IFxOneBit.MaxWhiteThreshold):
                NumericMaxThreshold.Value = TrackMaxThreshold.Value = ViewModel.MaxWhiteThreshold;
                break;
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

        ViewModel.PropertyChanged -= DataContext_PropertyChanged;
    }

    /// <summary>
    /// Function to initialize the control from the data context.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void InitializeFromDataContext(IFxOneBit dataContext)
    {
        if (dataContext is null)
        {
            NumericMinThreshold.Value = TrackMinThreshold.Value = 127;
            NumericMaxThreshold.Value = TrackMaxThreshold.Value = 255;
            CheckInvert.Checked = false;
            return;
        }

        NumericMinThreshold.ValueChanged -= NumericMinThreshold_ValueChanged;
        NumericMaxThreshold.ValueChanged -= NumericMaxThreshold_ValueChanged;

        CheckInvert.Checked = dataContext.Invert;
        NumericMinThreshold.Value = TrackMinThreshold.Value = dataContext.MinWhiteThreshold;
        NumericMaxThreshold.Value = TrackMaxThreshold.Value = dataContext.MaxWhiteThreshold;

        NumericMaxThreshold.ValueChanged += NumericMaxThreshold_ValueChanged;
        NumericMinThreshold.ValueChanged += NumericMinThreshold_ValueChanged;
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IFxOneBit dataContext)
    {
        UnassignEvents();

        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;

        if (dataContext is null)
        {
            return;
        }

        ViewModel.PropertyChanged += DataContext_PropertyChanged;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="Fx1BitSettings"/> class.</summary>
    public Fx1BitSettings() => InitializeComponent();
    #endregion
}
