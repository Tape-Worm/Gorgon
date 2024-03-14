
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: July 4, 2020 10:31:35 PM
// 


using System.ComponentModel;
using System.Numerics;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Math;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// The panel used to display settings for image codec support
/// </summary>
internal partial class AnimationFloatKeyEditor
    : EditorSubPanelCommon, IDataContext<IKeyValueEditor>
{

    // The lock for events.
    private int _numericEventsAssigned = 1;



    /// <summary>Property to return the data context assigned to this view.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IKeyValueEditor ViewModel
    {
        get;
        private set;
    }



    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IKeyValueEditor.Title):
                Text = ViewModel.Title;
                break;
            case nameof(IKeyValueEditor.Track):
                UpdateWithMetadata(ViewModel.Track?.Track.KeyMetadata);
                SetNumericValues(ViewModel);
                break;
            case nameof(IKeyValueEditor.Value):
                SetNumericValues(ViewModel);
                break;
        }
    }

    /// <summary>Handles the ValueChanged event of the NumericValue1 control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericValue1_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Value = new Vector4((float)NumericValue1.Value.Round(NumericValue1.DecimalPlaces), ViewModel.Value.Y, ViewModel.Value.Z, ViewModel.Value.W);
    }

    /// <summary>Handles the ValueChanged event of the NumericValue2 control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericValue2_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Value = new Vector4(ViewModel.Value.X, (float)NumericValue2.Value.Round(NumericValue2.DecimalPlaces), ViewModel.Value.Z, ViewModel.Value.W);
    }

    /// <summary>Handles the ValueChanged event of the NumericValue3 control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericValue3_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Value = new Vector4(ViewModel.Value.X, ViewModel.Value.Y, (float)NumericValue3.Value.Round(NumericValue3.DecimalPlaces), ViewModel.Value.W);
    }

    /// <summary>Handles the ValueChanged event of the NumericValue4 control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericValue4_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Value = new Vector4(ViewModel.Value.X, ViewModel.Value.Y, ViewModel.Value.Z, (float)NumericValue4.Value.Round(NumericValue4.DecimalPlaces));
    }

    /// <summary>
    /// Function to assign the values to the numeric controls.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void SetNumericValues(IKeyValueEditor dataContext)
    {
        DisableNumericEvents();

        try
        {
            NumericValue1.Value = (decimal)(dataContext.Value.X).Max((float)NumericValue1.Minimum).Min((float)NumericValue1.Maximum);
            NumericValue2.Value = (decimal)(dataContext.Value.Y).Max((float)NumericValue2.Minimum).Min((float)NumericValue2.Maximum);
            NumericValue3.Value = (decimal)(dataContext.Value.Z).Max((float)NumericValue3.Minimum).Min((float)NumericValue3.Maximum);
            NumericValue4.Value = (decimal)(dataContext.Value.W).Max((float)NumericValue4.Minimum).Min((float)NumericValue4.Maximum);
        }
        finally
        {
            EnableNumericEvents();
        }
    }

    /// <summary>
    /// Function to disable the events on the numeric controls.
    /// </summary>
    private void DisableNumericEvents()
    {
        if (Interlocked.Exchange(ref _numericEventsAssigned, 0) == 0)
        {
            return;
        }

        NumericValue1.ValueChanged -= NumericValue1_ValueChanged;
        NumericValue2.ValueChanged -= NumericValue2_ValueChanged;
        NumericValue3.ValueChanged -= NumericValue3_ValueChanged;
        NumericValue4.ValueChanged -= NumericValue4_ValueChanged;
    }

    /// <summary>
    /// Function to enable the events on the numeric controls.
    /// </summary>
    private void EnableNumericEvents()
    {
        if (Interlocked.Exchange(ref _numericEventsAssigned, 1) == 1)
        {
            return;
        }

        NumericValue1.ValueChanged += NumericValue1_ValueChanged;
        NumericValue2.ValueChanged += NumericValue2_ValueChanged;
        NumericValue3.ValueChanged += NumericValue3_ValueChanged;
        NumericValue4.ValueChanged += NumericValue4_ValueChanged;
    }

    /// <summary>
    /// Function to update the controls using the metadata provided in the view model.
    /// </summary>
    /// <param name="metadata">The metadata to use.</param>
    private void UpdateWithMetadata(KeyValueMetadata metadata)
    {
        foreach (Control control in TableKeyValues.Controls)
        {
            control.Visible = false;
        }

        if (metadata is null)
        {
            return;
        }

        DisableNumericEvents();

        try
        {
            for (int i = 0; i < metadata.ValueCount; ++i)
            {
                Label label;
                NumericUpDown editControl;

                MetadataValues values = metadata.ValueMetadata[i];

                switch (i)
                {
                    case 0:
                        label = LabelValue1;
                        editControl = NumericValue1;
                        break;
                    case 1:
                        label = LabelValue2;
                        editControl = NumericValue2;
                        break;
                    case 2:
                        label = LabelValue3;
                        editControl = NumericValue3;
                        break;
                    case 3:
                        label = LabelValue4;
                        editControl = NumericValue4;
                        break;
                    default:
                        continue;
                }


                label.Text = values.DisplayName;
                label.Visible = true;

                editControl.Visible = true;
                editControl.DecimalPlaces = values.DecimalCount;
                editControl.Minimum = (decimal)values.MinMax.Minimum;
                editControl.Maximum = (decimal)values.MinMax.Maximum;
            }
        }
        finally
        {
            EnableNumericEvents();
        }
    }

    /// <summary>
    /// Function to unassign events from the data context.
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
    /// Function to restore the control to its default state.
    /// </summary>
    private void ResetDataContext()
    {
        Text = Resources.GORANM_DEFAULT_TITLE;
        UpdateWithMetadata(null);
    }

    /// <summary>
    /// Function to initialize the control from the specified data context.
    /// </summary>
    /// <param name="dataContext">The data context to apply.</param>
    private void InitializeFromDataContext(IKeyValueEditor dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        UpdateWithMetadata(dataContext.Track?.Track.KeyMetadata);
        Text = dataContext.Title;

        SetNumericValues(dataContext);
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IKeyValueEditor dataContext)
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



    /// <summary>Initializes a new instance of the <see cref="AnimationFloatKeyEditor"/> class.</summary>
    public AnimationFloatKeyEditor() => InitializeComponent();

}
