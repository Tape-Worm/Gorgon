
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

using System.ComponentModel;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Math;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A panel used to add new tracks to an animation
/// </summary>
internal partial class AnimationProperties
    : EditorSubPanelCommon, IDataContext<IProperties>
{

    // Flag to indicate that events have been assigned to the control.
    private int _eventsAssigned = 1;

    /// <summary>
    /// Property to return the data context for the view.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IProperties ViewModel
    {
        get;
        private set;
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        DisableEvents();

        try
        {
            switch (e.PropertyName)
            {
                case nameof(IProperties.Length):
                    NumericLength.Value = ((decimal)ViewModel.Length).Min(NumericLength.Maximum).Max(NumericLength.Minimum);
                    break;
                case nameof(IProperties.Fps):
                    NumericFps.Value = ((decimal)ViewModel.Fps).Min(NumericFps.Maximum).Max(NumericFps.Minimum);
                    break;
                case nameof(IProperties.IsLooped):
                    CheckLoop.Checked = ViewModel.IsLooped;
                    LabelLoopCount.Enabled = NumericLoopCount.Enabled = ViewModel.IsLooped;
                    break;
                case nameof(IProperties.LoopCount):
                    NumericLoopCount.Value = ((decimal)ViewModel.LoopCount).Min(NumericLoopCount.Maximum).Max(NumericLoopCount.Minimum);
                    break;
                case nameof(IProperties.KeyCount):
                    LabelFrameCount.Text = string.Format(Resources.GORANM_TEXT_FRAME_COUNT, ViewModel.KeyCount);
                    break;
            }
        }
        finally
        {
            EnableEvents();

            ValidateOk();
        }
    }

    /// <summary>Handles the ValueChanged event of the NumericLength control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericLength_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Length = (float)NumericLength.Value;
    }

    /// <summary>Handles the ValueChanged event of the NumericFps control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericFps_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Fps = (float)NumericFps.Value;
    }

    /// <summary>Handles the Click event of the CheckLoop control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void CheckLoop_Click(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        LabelLoopCount.Enabled = NumericLoopCount.Enabled = ViewModel.IsLooped = CheckLoop.Checked;
    }

    /// <summary>Handles the ValueChanged event of the NumericLoopCount control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericLoopCount_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.LoopCount = (int)NumericLoopCount.Value;
    }

    /// <summary>
    /// Function to disable the numeric input events.
    /// </summary>
    private void DisableEvents()
    {
        if (Interlocked.Exchange(ref _eventsAssigned, 0) == 0)
        {
            return;
        }

        NumericLength.ValueChanged -= NumericLength_ValueChanged;
        NumericFps.ValueChanged -= NumericFps_ValueChanged;
        NumericLoopCount.ValueChanged -= NumericLoopCount_ValueChanged;
    }

    /// <summary>
    /// Function to enable the numeric input events.
    /// </summary>
    private void EnableEvents()
    {
        if (Interlocked.Exchange(ref _eventsAssigned, 1) == 1)
        {
            return;
        }

        NumericLength.ValueChanged += NumericLength_ValueChanged;
        NumericFps.ValueChanged += NumericFps_ValueChanged;
        NumericLoopCount.ValueChanged += NumericLoopCount_ValueChanged;
    }

    /// <summary>
    /// Function to unassign the events from the data context.
    /// </summary>
    private void UnassignEvents()
    {
        DisableEvents();

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged -= DataContext_PropertyChanged;
    }

    /// <summary>
    /// Function to reset the values on the control for a null data context.
    /// </summary>
    private void ResetDataContext()
    {
        CheckLoop.Checked = false;
        NumericLength.Value = 1;
        NumericFps.Value = 60;
        NumericLoopCount.Value = 0;
        LabelFrameCount.Text = string.Format(Resources.GORANM_TEXT_FRAME_COUNT, 60);
    }

    /// <summary>
    /// Function to initialize the control with the data context.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>
    private void InitializeFromDataContext(IProperties dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        DisableEvents();

        try
        {
            LabelLoopCount.Enabled = NumericLoopCount.Enabled = dataContext.IsLooped;
            NumericLength.Value = (decimal)dataContext.Length;
            NumericFps.Value = (decimal)dataContext.Fps;
            CheckLoop.Checked = dataContext.IsLooped;
            NumericLoopCount.Value = dataContext.LoopCount;
            LabelFrameCount.Text = string.Format(Resources.GORANM_TEXT_FRAME_COUNT, dataContext.KeyCount);
        }
        finally
        {
            EnableEvents();
        }
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

        ValidateOk();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IProperties dataContext)
    {
        UnassignEvents();

        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged += DataContext_PropertyChanged;
    }

    /// <summary>Initializes a new instance of the <see cref="AnimationProperties"/> class.</summary>
    public AnimationProperties() => InitializeComponent();

}
