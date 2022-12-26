#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Monday, March 10, 2014 12:44:57 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;
using Gorgon.UI;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// A panel used to dislpay hatch patterns for font glyphs.
/// </summary>
internal partial class FontPatternBrushView
    : EditorSubPanelCommon, IDataContext<IFontPatternBrush>
{
    #region Variables.
    // Bitmap used to draw the preview.
    private Bitmap _previewBitmap;
    // Lock for reentrancy on events.
    private int _eventLock;
    #endregion

    #region Properties.
    /// <summary>Property to return the data context assigned to this view.</summary>
    public IFontPatternBrush DataContext
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Handles the Paint event of the panelPreview control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
    private void PanelPreview_Paint(object sender, PaintEventArgs e) => DrawPreview(e.Graphics);

    /// <summary>
    /// Funciton to create a glyph hatch brush.
    /// </summary>
    /// <returns>The hatch brush.</returns>
    private GorgonGlyphHatchBrush GetHatchBrush()
    {
        DrawPreview();
        return new()
        {
            BackgroundColor = PickerBackground.SelectedColor,
            ForegroundColor = PickerForeground.SelectedColor,
            HatchStyle = (GlyphBrushHatchStyle)ComboHatch.Style
        };
    }

    /// <summary>Handles the SelectedIndexChanged event of the ComboHatch control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ComboHatch_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (DataContext is null)
        {
            return;
        }

        DataContext.Brush = GetHatchBrush();            
    }

    /// <summary>Handles the ColorChanged event of the PickerBackground control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ColorChangedEventArgs" /> instance containing the event data.</param>
    private void PickerBackground_ColorChanged(object sender, ColorChangedEventArgs e)
    {
        if (DataContext is null)
        {
            return;
        }

        DataContext.Brush = GetHatchBrush();            
    }

    /// <summary>Handles the ColorChanged event of the PickerForeground control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ColorChangedEventArgs" /> instance containing the event data.</param>
    private void PickerForeground_ColorChanged(object sender, ColorChangedEventArgs e)
    {
        if (DataContext is null)
        {
            return;
        }

        DataContext.Brush = GetHatchBrush();            
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UnhookEvents();

        switch (e.PropertyName)
        {
            case nameof(IFontPatternBrush.Brush):
                ComboHatch.Style = (HatchStyle)DataContext.Brush.HatchStyle;
                PickerForeground.SelectedColor = DataContext.Brush.ForegroundColor;
                PickerBackground.SelectedColor = DataContext.Brush.BackgroundColor;
                break;
            case nameof(IFontPatternBrush.OriginalColor):
                PickerForeground.OriginalColor = DataContext.OriginalColor.Foreground;
                PickerBackground.OriginalColor = DataContext.OriginalColor.Background;
                break;
        }
        
        HookEvents();
        ValidateOk();
    }

    /// <summary>
    /// Function to draw the preview window.
    /// </summary>
    /// <param name="graphics">Graphics interface to use.</param>
    private void DrawPreview(System.Drawing.Graphics graphics = null)
    {
        if (!IsHandleCreated)
        {
            return;
        }

        using Brush brush = new HatchBrush(ComboHatch.Style, PickerForeground.SelectedColor, PickerBackground.SelectedColor);
        using Brush backBrush = new TextureBrush(Resources.Transparency_Pattern, WrapMode.Tile);
        using var g = System.Drawing.Graphics.FromImage(_previewBitmap);

        g.FillRectangle(backBrush, PanelPreview.ClientRectangle);
        g.FillRectangle(brush, PanelPreview.ClientRectangle);

        System.Drawing.Graphics panelGraphics = graphics;

        try
        {
            if (panelGraphics == null)
            {
                panelGraphics = PanelPreview.CreateGraphics();
                panelGraphics.CompositingMode = CompositingMode.SourceOver;
            }

            panelGraphics.DrawImage(_previewBitmap, Point.Empty);
        }
        finally
        {
            if (graphics is null)
            {
                panelGraphics?.Dispose();
            }
        }
    }

    /// <summary>
    /// Function to disable control events.
    /// </summary>
    private void UnhookEvents()
    {
        if (Interlocked.Exchange(ref _eventLock, 0) == 0)
        {
            return;
        }

        PickerForeground.ColorChanged -= PickerForeground_ColorChanged;
        PickerBackground.ColorChanged -= PickerBackground_ColorChanged;
        ComboHatch.SelectedIndexChanged -= ComboHatch_SelectedIndexChanged;
    }

    /// <summary>
    /// Function to enable control events.
    /// </summary>
    private void HookEvents()
    {
        if (Interlocked.Exchange(ref _eventLock, 1) == 1)
        {
            return;
        }

        DrawPreview();

        ComboHatch.SelectedIndexChanged += ComboHatch_SelectedIndexChanged;
        PickerForeground.ColorChanged += PickerForeground_ColorChanged;
        PickerBackground.ColorChanged += PickerBackground_ColorChanged;
    }

    /// <summary>
    /// Function to unassign events from the data context and control.
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
    /// Function to reset the control when the data context is null.
    /// </summary>
    private void ResetDataContext()
    {
        ComboHatch.Style = HatchStyle.BackwardDiagonal;
        PickerForeground.SelectedColor = PickerForeground.OriginalColor = GorgonColor.Black;
        PickerBackground.SelectedColor = PickerBackground.OriginalColor = GorgonColor.BlackTransparent;

        DrawPreview();
    }

    /// <summary>
    /// Function to initialize the control with the data context.
    /// </summary>
    /// <param name="dataContext">The data context used to initialize the control.</param>
    private void InitializeDataContext(IFontPatternBrush dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        ComboHatch.Style = (HatchStyle)dataContext.Brush.HatchStyle;
        PickerForeground.OriginalColor = PickerForeground.SelectedColor = dataContext.Brush.ForegroundColor;
        PickerBackground.OriginalColor = PickerBackground.SelectedColor = dataContext.Brush.BackgroundColor;
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

    /// <summary>
    /// Raises the <see cref="UserControl.Load" /> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        _previewBitmap = new Bitmap(PanelPreview.ClientSize.Width,
                                    PanelPreview.ClientSize.Height,
                                    PixelFormat.Format32bppArgb);

        PanelPreview.ScaleBitmapLogicalToDevice(ref _previewBitmap);

        ComboHatch.Select();

        DrawPreview();
        ValidateOk();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IFontPatternBrush dataContext)
    {
        UnassignEvents();

        InitializeDataContext(dataContext);

        DataContext = dataContext;
        if (DataContext is null)
        {
            return;
        }
                    
        DataContext.PropertyChanged += DataContext_PropertyChanged;
        HookEvents();
        ValidateOk();
    }
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="FontPatternBrushView"/> class.
    /// </summary>
    public FontPatternBrushView()
    {
        InitializeComponent();
        ComboHatch.RefreshPatterns();
    }
    #endregion
}
