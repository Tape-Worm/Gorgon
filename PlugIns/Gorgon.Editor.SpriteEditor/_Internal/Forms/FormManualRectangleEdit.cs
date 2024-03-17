
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
// Created: March 23, 2019 9:42:34 PM
// 


using System.ComponentModel;
using Gorgon.Editor.UI;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The window for the manual rectangle input interface
/// </summary>
internal partial class FormManualRectangleEdit
    : Form, IDataContext<ISpriteClipContext>
{

    // Flag to indicate that the value changed event for the numeric controls should not fire.
    private bool _noValueEvent;



    /// <summary>Property to return the data context assigned to this view.</summary>
    /// <value>The data context.</value>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ISpriteClipContext ViewModel
    {
        get;
        private set;
    }



    /// <summary>Handles the ValueChanged event of the NumericLeft control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericLeft_ValueChanged(object sender, EventArgs e)
    {
        if ((ViewModel is null) || (_noValueEvent))
        {
            return;
        }

        DX.RectangleF newRect = new()
        {
            Left = (float)NumericLeft.Value,
            Top = (float)NumericTop.Value,
            Right = (float)NumericRight.Value,
            Bottom = (float)NumericBottom.Value
        };

        if (newRect.Equals(ViewModel.SpriteRectangle))
        {
            return;
        }

        ViewModel.SpriteRectangle = newRect;
    }

    /// <summary>
    /// Function to assign the values for the rectangle into the numeric inputs.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void SetRectangleInputs(ISpriteClipContext dataContext)
    {
        try
        {
            _noValueEvent = true;

            NumericLeft.Value = (decimal)dataContext.SpriteRectangle.Left.Max(0);
            NumericTop.Value = (decimal)dataContext.SpriteRectangle.Top.Max(0);
            NumericRight.Value = (decimal)dataContext.SpriteRectangle.Right.Max(1);
            NumericBottom.Value = (decimal)dataContext.SpriteRectangle.Bottom.Max(1);
        }
        finally
        {
            _noValueEvent = false;
        }
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISpriteClipContext.FixedSize):
                LabelRight.Enabled = LabelBottom.Enabled =
                NumericRight.Enabled = NumericBottom.Enabled = ViewModel.FixedSize is null;
                break;
            case nameof(ISpriteClipContext.SpriteRectangle):
                SetRectangleInputs(ViewModel);
                break;
        }
    }

    /// <summary>
    /// Function to unassign the events assigned to the datacontext.
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
    /// Function called when the view should be reset by a <b>null</b> data context.
    /// </summary>
    private void ResetDataContext()
    {
        NumericLeft.Maximum = NumericRight.Maximum = NumericTop.Maximum = NumericBottom.Maximum = 16384;
        NumericLeft.Value = NumericTop.Value = NumericRight.Value = NumericBottom.Value = 0;
        LabelRight.Enabled = LabelBottom.Enabled = NumericRight.Enabled = NumericBottom.Enabled = true;
    }

    /// <summary>
    /// Function to initialize the view from the current data context.
    /// </summary>
    /// <param name="dataContext">The data context being assigned.</param>
    private void InitializeFromDataContext(ISpriteClipContext dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        LabelRight.Enabled = LabelBottom.Enabled =
        NumericRight.Enabled = NumericBottom.Enabled = dataContext.FixedSize is null;

        SetRectangleInputs(dataContext);
    }

    /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.ResizeBegin"/> event.</summary>
    /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnResizeBegin(EventArgs e)
    {
        base.OnResizeBegin(e);

        if (ViewModel is null)
        {
            return;
        }

        //DataContext.IsMoving = true;
    }

    /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.ResizeEnd"/> event.</summary>
    /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnResizeEnd(EventArgs e)
    {
        base.OnResizeEnd(e);

        if (ViewModel is null)
        {
            return;
        }

        //DataContext.IsMoving = false;
    }

    /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }

        ViewModel?.Load();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(ISpriteClipContext dataContext)
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



    /// <summary>Initializes a new instance of the <see cref="FormManualRectangleEdit"/> class.</summary>
    public FormManualRectangleEdit() => InitializeComponent();

}
