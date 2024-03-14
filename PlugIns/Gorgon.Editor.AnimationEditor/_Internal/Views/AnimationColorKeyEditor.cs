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
// Created: July 6, 2020 11:20:21 PM
// 
#endregion

using System.ComponentModel;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A color selection control for the sprite.
/// </summary>
internal partial class AnimationColorKeyEditor
    : EditorSubPanelCommon, IDataContext<IColorValueEditor>
{
    #region Variables.
    // Flag to indicate that the events are assigned.
    private int _eventsAssigned = 1;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the data context for the view.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IColorValueEditor ViewModel
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to disable the events on the control.
    /// </summary>
    private void DisableEvents()
    {
        if (Interlocked.Exchange(ref _eventsAssigned, 0) == 0)
        {
            return;
        }

        PickerColor.ColorChanged -= Picker_ColorChanged;
        PickerAlpha.AlphaValueChanged -= PickerAlpha_AlphaValueChanged;
    }

    /// <summary>
    /// Function to enable the events on the control.
    /// </summary>
    private void EnableEvents()
    {
        if (Interlocked.Exchange(ref _eventsAssigned, 1) == 1)
        {
            return;
        }

        PickerAlpha.AlphaValueChanged += PickerAlpha_AlphaValueChanged;
        PickerColor.ColorChanged += Picker_ColorChanged;
    }

    /// <summary>Handles the ColorChanged event of the Picker control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ColorChangedEventArgs"/> instance containing the event data.</param>
    private void Picker_ColorChanged(object sender, ColorChangedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        DisableEvents();
        try
        {
            ViewModel.NewColor = e.Color;
        }
        finally
        {
            EnableEvents();
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

    /// <summary>Handles the AlphaValueChanged event of the PickerAlpha control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void PickerAlpha_AlphaValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        DisableEvents();
        try
        {
            ViewModel.NewColor = Color.FromArgb(PickerAlpha.AlphaValue, ViewModel.OriginalColor);
        }
        finally
        {
            EnableEvents();
        }
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IColorValueEditor.Title):
                Text = ViewModel.Title;
                break;
            case nameof(IColorValueEditor.AlphaOnly):
                if (ViewModel.AlphaOnly)
                {
                    PickerAlpha.Show();
                    PickerColor.Hide();
                }
                else
                {
                    PickerAlpha.Hide();
                    PickerColor.Show();
                }
                break;
            case nameof(IColorValueEditor.WorkingSprite):
                DisableEvents();
                try
                {
                    PickerColor.OriginalColor = ViewModel.OriginalColor;
                    PickerColor.SelectedColor = ViewModel.NewColor;
                    PickerAlpha.AlphaValue = (int)(ViewModel.NewColor.Alpha * 255);
                }
                finally
                {
                    EnableEvents();
                }
                break;
            case nameof(IColorValueEditor.OriginalColor):
                PickerColor.OriginalColor = ViewModel.OriginalColor;
                break;
            case nameof(IColorValueEditor.NewColor):
                DisableEvents();
                try
                {
                    PickerColor.SelectedColor = ViewModel.NewColor;
                    PickerAlpha.AlphaValue = (int)(ViewModel.NewColor.Alpha * 255);
                }
                finally
                {
                    EnableEvents();
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
        PickerColor.OriginalColor = PickerColor.SelectedColor = GorgonColor.BlackTransparent;
    }

    /// <summary>
    /// Function to initialize the control with the data context.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>
    private void InitializeFromDataContext(IColorValueEditor dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        if (dataContext.AlphaOnly)
        {
            PickerAlpha.Show();
            PickerColor.Hide();
        }
        else
        {
            PickerAlpha.Hide();
            PickerColor.Show();
        }

        Text = dataContext.Title;

        DisableEvents();

        try
        {
            PickerAlpha.AlphaValue = (int)(dataContext.OriginalColor.Alpha * 255);
            PickerColor.OriginalColor = dataContext.OriginalColor;
            PickerColor.SelectedColor = dataContext.NewColor;
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
    public void SetDataContext(IColorValueEditor dataContext)
    {
        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged += DataContext_PropertyChanged;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="AnimationColorKeyEditor"/> class.</summary>
    public AnimationColorKeyEditor() => InitializeComponent();
    #endregion
}
