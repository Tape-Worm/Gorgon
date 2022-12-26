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
// Created: April 4, 2019 8:58:41 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Numerics;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Math;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// A view used to manually update the sprite anchor value.
/// </summary>
internal partial class SpriteAnchor
    : EditorSubPanelCommon, IDataContext<ISpriteAnchorEdit>
{
    #region Variables.
    // Mid point of top side.
    private float _midTop;
    // Mid point of left side.
    private float _midLeft;
    // Mid point of bottom side.
    private float _midBottom;
    // Mid point of right side.
    private float _midRight;
    #endregion

    #region Properties.
    /// <summary>Property to return the data context assigned to this view.</summary>
    public ISpriteAnchorEdit DataContext
    {
        get;
        private set;
    }
    #endregion

    #region Methods.        
    /// <summary>
    /// Function to set the mid points for the edges of the sprite.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void SetMidPoints(ISpriteAnchorEdit dataContext)
    {
        _midTop = dataContext.SpriteBounds[0].Y + (dataContext.SpriteBounds[1].Y - dataContext.SpriteBounds[0].Y) * 0.5f;
        _midLeft = dataContext.SpriteBounds[0].X + (dataContext.SpriteBounds[3].X - dataContext.SpriteBounds[0].X) * 0.5f;
        _midBottom = dataContext.SpriteBounds[3].Y + (dataContext.SpriteBounds[2].Y - dataContext.SpriteBounds[3].Y) * 0.5f;
        _midRight = dataContext.SpriteBounds[2].X + (dataContext.SpriteBounds[1].X - dataContext.SpriteBounds[2].X) * 0.5f;
    }

    /// <summary>Handles the ValueChanged event of the NumericHorizontal control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericHorizontal_ValueChanged(object sender, EventArgs e)
    {
        if (DataContext is null)
        {
            return;
        }

        DataContext.Anchor = new Vector2((float)NumericHorizontal.Value, DataContext.Anchor.Y);
        ValidateOk();
    }

    /// <summary>Handles the ValueChanged event of the NumericVertical control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NumericVertical_ValueChanged(object sender, EventArgs e)
    {
        if (DataContext is null)
        {
            return;
        }

        DataContext.Anchor = new Vector2(DataContext.Anchor.X, (float)NumericVertical.Value);
        ValidateOk();
    }

    /// <summary>Handles the Click event of the CheckRotate control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void CheckRotate_Click(object sender, EventArgs e)
    {
        if (DataContext is null)
        {
            return;
        }

        DataContext.PreviewRotation = CheckRotate.Checked;
        ValidateOk();
    }

    /// <summary>Handles the Click event of the CheckScale control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void CheckScale_Click(object sender, EventArgs e)
    {
        if (DataContext is null)
        {
            return;
        }

        DataContext.PreviewScale = CheckScale.Checked;
        ValidateOk();
    }

    /// <summary>Handles the AlignmentChanged event of the Alignment control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Alignment_AlignmentChanged(object sender, EventArgs e)
    {
        if (DataContext is null)
        {
            return;
        }

        switch (Alignment.Alignment)
        {
            case Gorgon.UI.Alignment.UpperLeft:
                NumericHorizontal.Value = (decimal)DataContext.SpriteBounds[0].X;
                NumericVertical.Value = (decimal)DataContext.SpriteBounds[0].Y;
                break;
            case Gorgon.UI.Alignment.UpperCenter:
                NumericHorizontal.Value = (decimal)DataContext.MidPoint.X;
                NumericVertical.Value = (decimal)_midTop;
                break;
            case Gorgon.UI.Alignment.UpperRight:
                NumericHorizontal.Value = (decimal)DataContext.SpriteBounds[1].X;
                NumericVertical.Value = (decimal)DataContext.SpriteBounds[1].Y;
                break;
            case Gorgon.UI.Alignment.CenterLeft:
                NumericHorizontal.Value = (decimal)_midLeft;
                NumericVertical.Value = (decimal)DataContext.MidPoint.Y;
                break;
            case Gorgon.UI.Alignment.Center:
                NumericHorizontal.Value = (decimal)DataContext.MidPoint.X;
                NumericVertical.Value = (decimal)DataContext.MidPoint.Y;
                break;
            case Gorgon.UI.Alignment.CenterRight:
                NumericHorizontal.Value = (decimal)_midRight;
                NumericVertical.Value = (decimal)DataContext.MidPoint.Y;
                break;
            case Gorgon.UI.Alignment.LowerLeft:
                NumericHorizontal.Value = (decimal)DataContext.SpriteBounds[3].X;
                NumericVertical.Value = (decimal)DataContext.SpriteBounds[3].Y;
                break;
            case Gorgon.UI.Alignment.LowerCenter:
                NumericHorizontal.Value = (decimal)DataContext.MidPoint.X;
                NumericVertical.Value = (decimal)_midBottom;
                break;
            case Gorgon.UI.Alignment.LowerRight:
                NumericHorizontal.Value = (decimal)DataContext.SpriteBounds[2].X;
                NumericVertical.Value = (decimal)DataContext.SpriteBounds[2].Y;
                break;
        }
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISpriteAnchorEdit.PreviewScale):
                CheckScale.Checked = DataContext.PreviewScale;
                break;
            case nameof(ISpriteAnchorEdit.PreviewRotation):
                CheckRotate.Checked = DataContext.PreviewRotation;
                break;
            case nameof(ISpriteAnchorEdit.SpriteBounds):
                SetMidPoints(DataContext);
                SetAlignment(DataContext);
                break;
            case nameof(ISpriteAnchorEdit.Bounds):
                NumericHorizontal.Minimum = DataContext.Bounds.Left;
                NumericHorizontal.Maximum = DataContext.Bounds.Right;

                NumericVertical.Minimum = DataContext.Bounds.Top;
                NumericVertical.Maximum = DataContext.Bounds.Bottom;

                SetAlignment(DataContext);
                break;
            case nameof(ISpriteAnchorEdit.Anchor):
                NumericHorizontal.Value = (decimal)DataContext.Anchor.X;
                NumericVertical.Value = (decimal)DataContext.Anchor.Y;

                SetAlignment(DataContext);
                break;
        }

        ValidateOk();
    }

    /// <summary>
    /// Function to set up the alignment on the alignment control.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void SetAlignment(ISpriteAnchorEdit dataContext)
    {
        Alignment.AlignmentChanged -= Alignment_AlignmentChanged;

        try
        {
            Alignment.Alignment = Gorgon.UI.Alignment.None;

            if (dataContext is null)
            {
                return;
            }

            // Corner positions.
            if ((dataContext.Anchor.X.EqualsEpsilon(dataContext.SpriteBounds[0].X)) && (dataContext.Anchor.Y.EqualsEpsilon(dataContext.SpriteBounds[0].Y)))
            {
                Alignment.Alignment = Gorgon.UI.Alignment.UpperLeft;
            }

            if ((dataContext.Anchor.Y.EqualsEpsilon(dataContext.SpriteBounds[1].X)) && (dataContext.Anchor.Y.EqualsEpsilon(dataContext.SpriteBounds[1].Y)))
            {
                Alignment.Alignment = Gorgon.UI.Alignment.UpperRight;
            }

            if ((dataContext.Anchor.Y.EqualsEpsilon(dataContext.SpriteBounds[2].X)) && (dataContext.Anchor.Y.EqualsEpsilon(dataContext.SpriteBounds[2].Y)))
            {
                Alignment.Alignment = Gorgon.UI.Alignment.LowerRight;
            }

            if ((dataContext.Anchor.Y.EqualsEpsilon(dataContext.SpriteBounds[3].X)) && (dataContext.Anchor.Y.EqualsEpsilon(dataContext.SpriteBounds[3].Y)))
            {
                Alignment.Alignment = Gorgon.UI.Alignment.LowerLeft;
            }

            // Center positions.
            if ((dataContext.Anchor.X.EqualsEpsilon(dataContext.MidPoint.X)) && (dataContext.Anchor.Y.EqualsEpsilon(dataContext.MidPoint.Y)))
            {
                Alignment.Alignment = Gorgon.UI.Alignment.Center;
            }

            if ((dataContext.Anchor.X.EqualsEpsilon(dataContext.MidPoint.X)) && (dataContext.Anchor.Y.EqualsEpsilon(_midTop)))
            {
                Alignment.Alignment = Gorgon.UI.Alignment.UpperCenter;
            }

            if ((dataContext.Anchor.Y.EqualsEpsilon(_midLeft)) && (dataContext.Anchor.Y.EqualsEpsilon(dataContext.MidPoint.Y)))
            {
                Alignment.Alignment = Gorgon.UI.Alignment.CenterLeft;
            }

            if ((dataContext.Anchor.X.EqualsEpsilon(dataContext.MidPoint.X)) && (dataContext.Anchor.Y.EqualsEpsilon(_midBottom)))
            {
                Alignment.Alignment = Gorgon.UI.Alignment.LowerCenter;
            }

            if ((dataContext.Anchor.Y.EqualsEpsilon(_midRight)) && (dataContext.Anchor.Y.EqualsEpsilon(dataContext.MidPoint.Y)))
            {
                Alignment.Alignment = Gorgon.UI.Alignment.CenterRight;
            }
        }
        finally
        {
            Alignment.AlignmentChanged += Alignment_AlignmentChanged;
        }
    }
    
    /// <summary>
    /// Function to unassign any events.
    /// </summary>
    private void UnassignEvents()
    {
        if (DataContext is null)
        {
            return;
        }

        DataContext.PropertyChanged -= DataContext_PropertyChanged;
    }

    /// <summary>
    /// Function to reset the control when a data context is removed.
    /// </summary>
    private void ResetDataContext()
    {
        if (DataContext is null)
        {
            return;
        }

        UnassignEvents();
        Alignment.Alignment = Gorgon.UI.Alignment.UpperLeft;
        NumericHorizontal.Value = NumericVertical.Value = 0M;
        NumericVertical.Minimum = NumericHorizontal.Minimum = -16384;
        NumericVertical.Maximum = NumericHorizontal.Maximum = 16384;
        CheckRotate.Checked = CheckScale.Checked = false;
    }
    
    /// <summary>Function called to validate the OK button.</summary>
    /// <returns>
    ///   <b>true</b> if the OK button is valid, <b>false</b> if not.</returns>
    protected override bool OnValidateOk() => (DataContext?.OkCommand is not null) && (DataContext.OkCommand.CanExecute(null));
     
    /// <summary>
    /// Function to initialize the control from the data context.
    /// </summary>
    /// <param name="dataContext">The data context being assigned.</param>
    private void InitializeDataContext(ISpriteAnchorEdit dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        NumericHorizontal.Value = (decimal)dataContext.Anchor.X;
        NumericVertical.Value = (decimal)dataContext.Anchor.Y;

        NumericHorizontal.Minimum = dataContext.Bounds.Left;
        NumericHorizontal.Maximum = dataContext.Bounds.Right;

        NumericVertical.Minimum = dataContext.Bounds.Top;
        NumericVertical.Maximum = dataContext.Bounds.Bottom;

        CheckRotate.Checked = dataContext.PreviewRotation;
        CheckScale.Checked = dataContext.PreviewScale;

        SetMidPoints(dataContext);
        SetAlignment(dataContext);

        ValidateOk();
    }

    /// <summary>
    /// Function to submit the change.
    /// </summary>
    protected override void OnSubmit()
    {
        base.OnSubmit();

        if ((DataContext?.OkCommand is null) || (!DataContext.OkCommand.CanExecute(null)))
        {
            return;
        }

        DataContext.OkCommand.Execute(null);
    }

    /// <summary>
    /// Function to cancel the change.
    /// </summary>
    protected override void OnCancel()
    {
        base.OnCancel();

        if ((DataContext?.CancelCommand is null) || (!DataContext.CancelCommand.CanExecute(null)))
        {
            return;
        }

        DataContext.CancelCommand.Execute(null);
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

        DataContext?.Load();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(ISpriteAnchorEdit dataContext)
    {
        InitializeDataContext(dataContext);

        DataContext = dataContext;

        if (DataContext is null)
        {
            return;
        }

        DataContext.PropertyChanged += DataContext_PropertyChanged;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="SpriteAnchor"/> class.</summary>
    public SpriteAnchor() => InitializeComponent();
    #endregion
}
