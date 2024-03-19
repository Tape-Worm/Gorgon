
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
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics;
using Gorgon.Math;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// A color selection control for the sprite pick tool mask color
/// </summary>
internal partial class SpritePickMaskColor
    : EditorSubPanelCommon, IDataContext<ISpritePickMaskEditor>
{

    // The original mask value.
    private ClipMask _originalMask;



    /// <summary>
    /// Property to return the data context for the view.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ISpritePickMaskEditor ViewModel
    {
        get;
        private set;
    }



    /// <summary>Handles the Click event of the RadioAlpha control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void RadioAlpha_Click(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            TableAlphaOnly.Visible = RadioAlpha.Checked;
            Picker.Visible = !RadioAlpha.Checked;
            return;
        }

        if (((ViewModel.ClipMaskType == ClipMask.Alpha) && (RadioAlpha.Checked))
            || ((ViewModel.ClipMaskType == ClipMask.Color) && (RadioColorAlpha.Checked)))
        {
            return;
        }

        ViewModel.ClipMaskType = RadioAlpha.Checked ? ClipMask.Alpha : ClipMask.Color;
    }

    /// <summary>Handles the ValueChanged event of the NumericAlpha control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericAlpha_ValueChanged(object sender, EventArgs e)
    {
        int alpha = (int)(SliderAlpha.ValuePercentual * 255);

        if (alpha == NumericAlpha.Value)
        {
            return;
        }

        SliderAlpha.ValuePercentual = (float)NumericAlpha.Value / 255.0f;
        ValidateOk();
    }

    /// <summary>Handles the PercentualValueChanged event of the SliderAlpha control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void SliderAlpha_PercentualValueChanged(object sender, EventArgs e)
    {
        int alpha = (int)(SliderAlpha.ValuePercentual * 255);
        ColorShow.LowerColor = Color.FromArgb(alpha, Color.White);

        if (alpha != NumericAlpha.Value)
        {
            NumericAlpha.Value = alpha;
        }

        if ((ViewModel is not null) && (!ViewModel.ClipMaskValue.Alpha.EqualsEpsilon(SliderAlpha.ValuePercentual)))
        {
            ViewModel.ClipMaskValue = new GorgonColor(Picker.SelectedColor, SliderAlpha.ValuePercentual);
        }

        ValidateOk();
    }

    /// <summary>Handles the ColorChanged event of the Picker control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ColorChangedEventArgs"/> instance containing the event data.</param>
    private void Picker_ColorChanged(object sender, ColorChangedEventArgs e)
    {
        if ((ViewModel is null) || (ViewModel.ClipMaskValue.Equals(e.Color)))
        {
            return;
        }

        ViewModel.ClipMaskValue = e.Color;
        ValidateOk();
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

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISpritePickMaskEditor.IsActive):
                // If we activate the control, then reset its values.
                if (ViewModel.IsActive)
                {
                    Picker.OriginalColor = Picker.SelectedColor = ViewModel.ClipMaskValue;
                    ColorShow.UpperColor = Color.FromArgb((int)(255 * ViewModel.ClipMaskValue.Alpha), Color.White);
                    SliderAlpha.ValuePercentual = ViewModel.ClipMaskValue.Alpha;
                }
                break;
            case nameof(ISpritePickMaskEditor.ClipMaskType):
                RadioAlpha.Checked = ViewModel.ClipMaskType == ClipMask.Alpha;
                TableAlphaOnly.Visible = RadioAlpha.Checked;
                Picker.Visible = !RadioAlpha.Checked;
                SliderAlpha.ValuePercentual = ViewModel.ClipMaskValue.Alpha;
                Picker.SelectedColor = ViewModel.ClipMaskValue;
                break;
            case nameof(ISpritePickMaskEditor.ClipMaskValue):
                if (ViewModel.ClipMaskType == ClipMask.Alpha)
                {
                    SliderAlpha.ValuePercentual = ViewModel.ClipMaskValue.Alpha;
                }
                else
                {
                    Picker.SelectedColor = ViewModel.ClipMaskValue;
                }
                break;
        }
    }

    /// <summary>
    /// Function to reset the values on the control for a null data context.
    /// </summary>
    private void ResetDataContext()
    {
        UnassignEvents();
        RadioAlpha.Checked = true;
        SliderAlpha.ValuePercentual = 0.0f;
        Picker.OriginalColor = Picker.SelectedColor = GorgonColors.BlackTransparent;
    }

    /// <summary>
    /// Function to initialize the control with the data context.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>
    private void InitializeFromDataContext(ISpritePickMaskEditor dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        _originalMask = dataContext.ClipMaskType;
        RadioAlpha.Checked = dataContext.ClipMaskType == ClipMask.Alpha;
        RadioColorAlpha.Checked = !RadioAlpha.Checked;
        TableAlphaOnly.Visible = RadioAlpha.Checked;
        Picker.Visible = !RadioAlpha.Checked;
        Picker.OriginalColor = Picker.SelectedColor = dataContext.ClipMaskValue;
        ColorShow.UpperColor = Color.FromArgb((int)(255 * dataContext.ClipMaskValue.Alpha), Color.White);
        SliderAlpha.ValuePercentual = dataContext.ClipMaskValue.Alpha;
    }

    /// <summary>Function to submit the change.</summary>
    protected override void OnSubmit()
    {
        base.OnSubmit();

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.IsActive = false;
    }

    /// <summary>Function to cancel the change.</summary>
    protected override void OnCancel()
    {
        base.OnCancel();

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.ClipMaskType = _originalMask;
        ViewModel.ClipMaskValue = Picker.OriginalColor;
        ViewModel.IsActive = false;
    }

    /// <summary>
    /// Function to validate the state of the OK button.
    /// </summary>
    /// <returns><b>true</b> if the OK button is valid, <b>false</b> if not.</returns>
    protected override bool OnValidateOk()
    {
        GorgonColor color;

        if (RadioAlpha.Checked)
        {
            color = new GorgonColor(Picker.OriginalColor, ColorShow.UpperColor.A / 255.0f);
        }
        else
        {
            color = Picker.OriginalColor;
        }

        return (ViewModel is not null) && ((_originalMask != ViewModel.ClipMaskType) || (!ViewModel.ClipMaskValue.Equals(color)));
    }

    /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
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
    public void SetDataContext(ISpritePickMaskEditor dataContext)
    {
        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged += DataContext_PropertyChanged;
    }



    /// <summary>Initializes a new instance of the <see cref="SpriteColor"/> class.</summary>
    public SpritePickMaskColor() => InitializeComponent();

}
