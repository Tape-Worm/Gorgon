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
// Created: March 28, 2019 9:48:28 AM
// 
#endregion

using System.ComponentModel;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The view for changing the font outline parameters.
/// </summary>
internal partial class FontOutlineView
    : EditorSubPanelCommon, IDataContext<IFontOutline>
{
    #region Variables.
    // The event hook counter.
    private int _eventHook;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the data context for the view.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IFontOutline ViewModel
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to unhook the events.
    /// </summary>
    private void UnhookEvents()
    {
        if (Interlocked.Exchange(ref _eventHook, 0) == 0)
        {
            return;
        }

        NumericSize.ValueChanged -= NumericSize_ValueChanged;
        ColorEnd.ColorChanged -= ColorEnd_ColorChanged;
        ColorStart.ColorChanged -= ColorStart_ColorChanged;
    }

    /// <summary>
    /// Function to hook events.
    /// </summary>
    private void HookEvents()
    {
        if (Interlocked.Exchange(ref _eventHook, 1) != 0)
        {
            return;
        }

        ColorEnd.ColorChanged += ColorEnd_ColorChanged;
        ColorStart.ColorChanged += ColorStart_ColorChanged;
        NumericSize.ValueChanged += NumericSize_ValueChanged;
    }

    /// <summary>
    /// Function to validate the controls on the panel.
    /// </summary>
    private void ValidateControls()
    {
        if (ViewModel is null)
        {
            NumericSize.Enabled =
            ColorStart.Enabled =
            ColorEnd.Enabled =
            LabelSize.Enabled =
            LabelStartColor.Enabled =
            LabelEndColor.Enabled = false;
            return;
        }

        LabelSize.Enabled = NumericSize.Enabled = true;            
        LabelStartColor.Enabled = ColorStart.Enabled = ViewModel.OutlineSize > 0;
        LabelEndColor.Enabled = ColorEnd.Enabled = ViewModel.OutlineSize > 2;
    }

    /// <summary>Handles the ValueChanged event of the NumericSize control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void NumericSize_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.OutlineSize = (int)NumericSize.Value;
    }

    /// <summary>Handles the ColorChanged event of the Picker control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ColorChangedEventArgs"/> instance containing the event data.</param>
    private void ColorStart_ColorChanged(object sender, ColorChangedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.SelectedStartColor = e.Color;
        ValidateControls();
    }

    /// <summary>Handles the ColorChanged event of the ColorEnd control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ColorChangedEventArgs" /> instance containing the event data.</param>
    private void ColorEnd_ColorChanged(object sender, ColorChangedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.SelectedEndColor = e.Color;
        ValidateControls();
    }

    /// <summary>
    /// Function to unassign the events from the data context.
    /// </summary>
    private void UnassignEvents()
    {
        UnhookEvents();

        if (ViewModel is null)
        {
            return;
        }

        // Always unassign your view model events. Failure to do so can result in an event leak, causing the view to stay 
        // in memory for the lifetime of the application.

        ViewModel.PropertyChanged -= DataContext_PropertyChanged;
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UnhookEvents();
        switch (e.PropertyName)
        {
            case nameof(IFontOutline.OutlineSize):
                NumericSize.Value = ViewModel.OutlineSize;
                break;
            case nameof(IFontOutline.OriginalStartColor):
                ColorStart.OriginalColor = ViewModel.OriginalStartColor;
                break;
            case nameof(IFontOutline.OriginalEndColor):
                ColorEnd.OriginalColor = ViewModel.OriginalEndColor;
                break;
            case nameof(IFontOutline.SelectedStartColor):
                ColorStart.SelectedColor = ViewModel.SelectedStartColor;
                break;
            case nameof(IFontOutline.SelectedEndColor):
                ColorEnd.SelectedColor = ViewModel.SelectedEndColor;
                break;
        }
        HookEvents();

        ValidateControls();
    }

    /// <summary>
    /// Function to reset the values on the control for a null data context.
    /// </summary>
    private void ResetDataContext()
    {
        UnassignEvents();

        NumericSize.Value = 0;
        ColorStart.OriginalColor = 
        ColorStart.SelectedColor = 
        ColorEnd.OriginalColor = 
        ColorEnd.SelectedColor = GorgonColor.BlackTransparent;
    }

    /// <summary>
    /// Function to initialize the control with the data context.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>
    private void InitializeFromDataContext(IFontOutline dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        NumericSize.Value = dataContext.OutlineSize;
        ColorStart.OriginalColor = dataContext.OriginalStartColor;
        ColorStart.SelectedColor = dataContext.SelectedStartColor;
        ColorEnd.OriginalColor = dataContext.OriginalEndColor;
        ColorEnd.SelectedColor = dataContext.SelectedEndColor;
    }

    /// <summary>Function called to validate the OK button.</summary>
    /// <returns>
    ///   <b>true</b> if the OK button is valid, <b>false</b> if not.</returns>
    protected override bool OnValidateOk()
    {
        if (ViewModel?.OkCommand is not null)
        {
            return ViewModel.OkCommand.CanExecute(null);
        }

        return base.OnValidateOk();
    }

    /// <summary>Function to cancel the change.</summary>
    protected override void OnCancel() 
    {
        if (ViewModel is not null)
        {
            ViewModel.IsActive = false;
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

        NumericSize.Select();

        ValidateControls();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IFontOutline dataContext)
    {
        UnassignEvents();

        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;

        if (ViewModel is null)
        {
            ValidateControls();
            return;
        }

        HookEvents();

        ViewModel.PropertyChanged += DataContext_PropertyChanged;
        ValidateControls();
    }        
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="FontOutlineView"/> class.</summary>
    public FontOutlineView() => InitializeComponent();
    #endregion
}
