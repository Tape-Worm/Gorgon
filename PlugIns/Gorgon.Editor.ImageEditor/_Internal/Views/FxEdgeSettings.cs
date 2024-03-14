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
// Created: March 6, 2020 1:30:22 PM
// 
#endregion

using System.ComponentModel;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Math;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// The settings for the edge filter effect.
/// </summary>
internal partial class FxEdgeSettings
    : EditorSubPanelCommon, IDataContext<IFxEdgeDetect>
{
    #region Properties.
    /// <summary>Property to return the data context assigned to this view.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IFxEdgeDetect ViewModel
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>Handles the ValueChanged event of the TrackThreshold control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void TrackThreshold_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Threshold = TrackThreshold.Value;
    }

    /// <summary>Handles the ValueChanged event of the PickerLineColor control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void PickerLineColor_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.LineColor = new Graphics.GorgonColor(PickerLineColor.Value, ViewModel.LineColor.Alpha);
    }

    /// <summary>Handles the ValueChanged event of the SliderAlpha control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void SliderAlpha_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.LineColor = new Graphics.GorgonColor(ViewModel.LineColor, SliderAlpha.ValuePercentual);
    }

    /// <summary>Handles the Click event of the CheckOverlay control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void CheckOverlay_Click(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Overlay = CheckOverlay.Checked;
    }

    /// <summary>Handles the ValueChanged event of the NumericEmbossAmount control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericOffsetAmount_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        float currentValue = (float)NumericOffset.Value;

        if (currentValue.EqualsEpsilon(ViewModel.Offset))
        {
            return;
        }

        ViewModel.Offset = currentValue;
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

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IFxEdgeDetect.Offset):
                NumericOffset.Value = (decimal)ViewModel.Offset;
                break;
            case nameof(IFxEdgeDetect.Threshold):
                TrackThreshold.Value = ViewModel.Threshold;
                break;
            case nameof(IFxEdgeDetect.LineColor):
                ColorPreview.Color = ViewModel.LineColor.ToColor();
                LabelColorValue.Text = string.Format(Resources.GORIMG_TEXT_COLOR_VALUES, (int)(255 * ViewModel.LineColor.Red),
                                                                                         (int)(255 * ViewModel.LineColor.Green),
                                                                                         (int)(255 * ViewModel.LineColor.Blue),
                                                                                         (int)(255 * ViewModel.LineColor.Alpha));
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
    private void InitializeFromDataContext(IFxEdgeDetect dataContext)
    {
        if (dataContext is null)
        {
            TrackThreshold.Value = 50;
            SliderAlpha.ValuePercentual = 1.0f;
            ColorPreview.Color = Color.Black;
            PickerLineColor.ValuePercentual = new PointF(0, 1.0f);
            LabelColorValue.Text = string.Format(Resources.GORIMG_TEXT_COLOR_VALUES, 0, 0, 0, 255);
            NumericOffset.ValueChanged -= NumericOffsetAmount_ValueChanged;
            NumericOffset.Value = 1;
            NumericOffset.ValueChanged += NumericOffsetAmount_ValueChanged;
            CheckOverlay.Checked = true;
            return;
        }

        TrackThreshold.Value = dataContext.Threshold;
        SliderAlpha.ValuePercentual = dataContext.LineColor.Alpha;
        ColorPreview.Color = dataContext.LineColor.ToColor();
        LabelColorValue.Text = string.Format(Resources.GORIMG_TEXT_COLOR_VALUES, (int)(255 * dataContext.LineColor.Red),
                                                                                 (int)(255 * dataContext.LineColor.Green),
                                                                                 (int)(255 * dataContext.LineColor.Blue),
                                                                                 (int)(255 * dataContext.LineColor.Alpha));
        PickerLineColor.ValuePercentual = new PointF(((Color)dataContext.LineColor).GetHue(), ((Color)dataContext.LineColor).GetSaturation());
        CheckOverlay.Checked = dataContext.Overlay;
        NumericOffset.Value = (decimal)dataContext.Offset;
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IFxEdgeDetect dataContext)
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
    /// <summary>Initializes a new instance of the <see cref="FxEdgeSettings"/> class.</summary>
    public FxEdgeSettings() => InitializeComponent();
    #endregion
}
