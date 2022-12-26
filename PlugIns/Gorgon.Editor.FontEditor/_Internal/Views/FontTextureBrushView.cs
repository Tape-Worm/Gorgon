#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: September 10, 2021 2:37:30 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Threading;
using DX = SharpDX;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The view for editing a glyph texture brush.
/// </summary>
internal partial class FontTextureBrushView
    : EditorSubPanelCommon, IDataContext<IFontTextureBrush>
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
    public IFontTextureBrush DataContext
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

        NumericLeft.ValueChanged -= Numeric_ValueChanged;
        NumericTop.ValueChanged -= Numeric_ValueChanged;
        NumericRight.ValueChanged -= Numeric_ValueChanged;
        NumericBottom.ValueChanged -= Numeric_ValueChanged;
        ComboWrapMode.SelectedIndexChanged -= ComboWrapMode_SelectedIndexChanged;
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

        NumericLeft.ValueChanged += Numeric_ValueChanged;
        NumericTop.ValueChanged += Numeric_ValueChanged;
        NumericRight.ValueChanged += Numeric_ValueChanged;
        NumericBottom.ValueChanged += Numeric_ValueChanged;
        ComboWrapMode.SelectedIndexChanged += ComboWrapMode_SelectedIndexChanged;
    }

    /// <summary>
    /// Function called when one of the numeric text boxes is updated.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void Numeric_ValueChanged(object sender, EventArgs e)
    {
        DX.RectangleF rect = new()
        {
            Left = (float)NumericLeft.Value,
            Top = (float)NumericTop.Value,
            Right = (float)NumericRight.Value,
            Bottom = (float)NumericBottom.Value
        };
        
        if ((DataContext?.SetRegionCommand is null) || (!DataContext.SetRegionCommand.CanExecute(rect)))
        {
            return;
        }

        DataContext.SetRegionCommand.Execute(rect);
        SetNumericMaxValues(DataContext.Texture, DataContext.Region);

        ValidateControls();
    }

    /// <summary>Handles the SelectedIndexChanged event of the ComboWrapMode control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ComboWrapMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        var item = (WrapModeComboItem)ComboWrapMode.SelectedItem;            

        if ((DataContext?.SetWrappingModeCommand is null) || (!DataContext.SetWrappingModeCommand.CanExecute(item.WrapMode)))
        {
            return;
        }

        DataContext.SetWrappingModeCommand.Execute(item.WrapMode);
    }

    /// <summary>
    /// Function to validate the controls on the panel.
    /// </summary>
    private void ValidateControls()
    {
        if (DataContext is null)
        {
            TableControls.Enabled = false;
            return;
        }

        TableControls.Enabled = DataContext.Texture is not null;

        ValidateOk();
    }

    /// <summary>
    /// Function to unassign the events from the data context.
    /// </summary>
    private void UnassignEvents()
    {
        UnhookEvents();

        if (DataContext is null)
        {
            return;
        }

        DataContext.PropertyChanged -= DataContext_PropertyChanged;
    }

    /// <summary>
    /// Function to set the upper bounds for the numeric values.
    /// </summary>
    /// <param name="image">The image to base the upper bounds on.</param>
    /// <param name="region">The current region.</param>
    private void SetNumericMaxValues(IGorgonImage image, DX.RectangleF region)
    {
        if (image is null)
        {
            NumericBottom.Minimum = NumericRight.Minimum = NumericLeft.Minimum = NumericTop.Minimum = 0;
            NumericBottom.Maximum = NumericRight.Maximum = NumericLeft.Maximum = NumericTop.Maximum = 16383;
            NumericBottom.Value = NumericRight.Value = NumericLeft.Value = NumericTop.Value = 0;
            return;
        }

        NumericBottom.Minimum = (decimal)region.Top + 1;
        NumericRight.Minimum = (decimal)region.Left + 1;

        NumericBottom.Maximum = image.Height - 1M;
        NumericRight.Maximum = image.Width - 1M;

        NumericTop.Minimum = 0;
        NumericLeft.Minimum = 0;

        NumericTop.Maximum = (decimal)region.Bottom - 1M;
        NumericLeft.Maximum = (decimal)region.Right - 1M;

        NumericTop.Value = ((decimal)region.Top).Min(NumericTop.Maximum).Max(NumericTop.Minimum);
        NumericLeft.Value = ((decimal)region.Left).Min(NumericLeft.Maximum).Max(NumericLeft.Minimum);
        NumericBottom.Value = ((decimal)region.Bottom).Min(NumericBottom.Maximum).Max(NumericBottom.Minimum);
        NumericRight.Value = ((decimal)region.Right).Min(NumericRight.Maximum).Max(NumericRight.Minimum);
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UnhookEvents();

        switch (e.PropertyName)
        {
            case nameof(IFontTextureBrush.Region):
            case nameof(IFontTextureBrush.Texture):
                SetNumericMaxValues(DataContext.Texture, DataContext.Region);
                break;
            case nameof(IFontTextureBrush.WrapMode):
                ComboWrapMode.SelectedItem = new WrapModeComboItem(DataContext.WrapMode);
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
        SetNumericMaxValues(null, new DX.RectangleF(0, 0, 1, 1));
        ComboWrapMode.SelectedIndex = 0;
    }

    /// <summary>
    /// Function to initialize the control with the data context.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>
    private void InitializeFromDataContext(IFontTextureBrush dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        SetNumericMaxValues(dataContext.Texture, dataContext.Region);
        ComboWrapMode.SelectedItem = new WrapModeComboItem(dataContext.WrapMode);
    }

    /// <summary>Function called to validate the OK button.</summary>
    /// <returns>
    ///   <b>true</b> if the OK button is valid, <b>false</b> if not.</returns>
    protected override bool OnValidateOk()
    {
        if (DataContext?.OkCommand is not null)
        {
            return DataContext.OkCommand.CanExecute(null);
        }

        return base.OnValidateOk();
    }

    /// <summary>Function to cancel the change.</summary>
    protected override void OnCancel() 
    {
        if (DataContext is not null)
        {
            DataContext.IsActive = false;
        }
    }

    /// <summary>Function to submit the change.</summary>
    protected override void OnSubmit()
    {
        base.OnSubmit();

        if ((DataContext?.OkCommand is null) || (!DataContext.OkCommand.CanExecute(null)))
        {
            return;
        }

        DataContext.OkCommand.Execute(null);
    }

    /// <summary>Raises the <see cref="System.Windows.Forms.UserControl.Load"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (IsDesignTime)
        {
            return;
        }

        DataContext?.Load();
                    
        NumericTop.Select();

        ValidateControls();
    }
    
    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IFontTextureBrush dataContext)
    {
        UnassignEvents();

        InitializeFromDataContext(dataContext);

        DataContext = dataContext;

        if (DataContext is null)
        {
            ValidateControls();
            return;
        }

        HookEvents();

        DataContext.PropertyChanged += DataContext_PropertyChanged;
        ValidateControls();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="FontTextureBrushView"/> class.</summary>
    public FontTextureBrushView() 
    {
        InitializeComponent();

        ComboWrapMode.Items.Clear();
        ComboWrapMode.Items.Add(new WrapModeComboItem(GlyphBrushWrapMode.Tile, Resources.GORFNT_TEXT_TILE));
        ComboWrapMode.Items.Add(new WrapModeComboItem(GlyphBrushWrapMode.Clamp, Resources.GORFNT_TEXT_CLAMP));
        ComboWrapMode.Items.Add(new WrapModeComboItem(GlyphBrushWrapMode.TileFlipX, Resources.GORFNT_TEXT_TILE_FILP_HORZ));
        ComboWrapMode.Items.Add(new WrapModeComboItem(GlyphBrushWrapMode.TileFlipY, Resources.GORFNT_TEXT_TILE_FLIP_VERT));
        ComboWrapMode.Items.Add(new WrapModeComboItem(GlyphBrushWrapMode.TileFlipXandY, Resources.GORFNT_TEXT_TILE_FLIP));
        ComboWrapMode.SelectedIndex = 0;
    }
    #endregion
}
