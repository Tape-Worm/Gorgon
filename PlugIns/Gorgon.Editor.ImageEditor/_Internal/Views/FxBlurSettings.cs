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
// Created: February 22, 2020 5:32:41 PM
// 
#endregion

using System.ComponentModel;
using Gorgon.Editor.ImageEditor.Fx;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// Settings for the blur effect.
/// </summary>
internal partial class FxBlurSettings 
    : EditorSubPanelCommon, IDataContext<IFxBlur>
{
    #region Properties.
    /// <summary>Property to return the data context assigned to this view.</summary>
    public IFxBlur ViewModel
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>Handles the ValueChanged event of the NumericBlurAmount control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericBlurAmount_ValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        int currentValue = (int)NumericBlurAmount.Value;

        if (currentValue == ViewModel.BlurAmount)
        {
            return;
        }

        ViewModel.BlurAmount = currentValue;
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
            case nameof(IFxBlur.BlurAmount):
                NumericBlurAmount.Value = ViewModel.BlurAmount;
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
    private void InitializeFromDataContext(IFxBlur dataContext)
    {
        if (dataContext is null)
        {
            NumericBlurAmount.ValueChanged -= NumericBlurAmount_ValueChanged;
            NumericBlurAmount.Value = 1;
            NumericBlurAmount.ValueChanged += NumericBlurAmount_ValueChanged;
            return;
        }

        NumericBlurAmount.Value = dataContext.BlurAmount;
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IFxBlur dataContext)
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
    /// <summary>Initializes a new instance of the <see cref="FxBlurSettings"/> class.</summary>
    public FxBlurSettings() => InitializeComponent();
    #endregion
}
