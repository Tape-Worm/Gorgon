
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: February 13, 2021 4:04:07 PM
// 

using System.ComponentModel;
using System.Numerics;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Fonts;
using Krypton.Toolkit;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// Provides a ribbon interface for the plug in view
/// </summary>
internal partial class FormRibbon
    : KryptonForm, IDataContext<IFontContent>
{

    // The list of menu items associated with the zoom level.
    private readonly Dictionary<ZoomLevels, ToolStripMenuItem> _menuZoomItems = [];

    // The current zoom level.
    private ZoomLevels _zoomLevel = ZoomLevels.ToWindow;
    // The renderer for the content.
    private IContentRenderer _contentRenderer;

    /// <summary>
    /// Property to set or return the data context for the ribbon on the form.
    /// </summary>
    public IFontContent ViewModel
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to set or return the currently active renderer.
    /// </summary>
    public IContentRenderer ContentRenderer
    {
        get => _contentRenderer;
        set
        {
            if (_contentRenderer == value)
            {
                return;
            }

            if (_contentRenderer is not null)
            {
                ContentRenderer.ZoomScaleChanged -= ContentRenderer_ZoomScale;
            }

            _contentRenderer = value;

            if (_contentRenderer is not null)
            {
                ContentRenderer.ZoomScaleChanged += ContentRenderer_ZoomScale;
                _zoomLevel = _contentRenderer.ZoomLevel;
            }

            UpdateZoomMenu();
        }
    }

    /// <summary>Handles the ZoomScale event of the ContentRenderer control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ZoomScaleEventArgs"/> instance containing the event data.</param>
    private void ContentRenderer_ZoomScale(object sender, ZoomScaleEventArgs e)
    {
        // This event callback is used to handle the case where the user zooms in/out on the content in the content 
        // renderer using CTRL + Middle Mouse Wheel.

        _zoomLevel = _contentRenderer.ZoomLevel;
        UpdateZoomMenu();
    }

    /// <summary>
    /// Function to update the zoom item menu to reflect the current selection.
    /// </summary>
    private void UpdateZoomMenu()
    {
        if (!_menuZoomItems.TryGetValue(_zoomLevel, out ToolStripMenuItem currentItem))
        {
            return;
        }

        foreach (ToolStripMenuItem item in MenuZoom.Items.OfType<ToolStripMenuItem>().Where(item => item != currentItem))
        {
            item.Checked = false;
        }

        if (!currentItem.Checked)
        {
            currentItem.Checked = true;
        }

        ButtonZoom.TextLine1 = string.Format(Resources.ZOOM_TEXT, _zoomLevel.GetName());
    }

    /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangingEventArgs" /> instance containing the event data.</param>
    private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IFontContent.CommandContext):
                if (ViewModel.CommandContext is not null)
                {
                    ViewModel.CommandContext.PropertyChanged -= CommandContext_PropertyChanged;
                }
                break;
        }
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The [PropertyChangedEventArgs] instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IFontContent.FontFamily):
                ComboFonts.Text = ViewModel.FontFamily;
                break;
            case nameof(IFontContent.FontSize):
                NumericSize.ValueChanged -= NumericSize_ValueChanged;
                NumericSize.Value = (decimal)ViewModel.FontSize;
                NumericSize.ValueChanged += NumericSize_ValueChanged;
                break;
            case nameof(IFontContent.FontUnits):
                RadioPointUnits.Checked = ViewModel.FontUnits == GorgonFontHeightMode.Points;
                RadioPixelUnits.Checked = !RadioPointUnits.Checked;
                break;
            case nameof(IFontContent.IsBold):
                CheckBold.Checked = ViewModel.IsBold;
                break;
            case nameof(IFontContent.IsItalic):
                CheckItalics.Checked = ViewModel.IsItalic;
                break;
            case nameof(IFontContent.IsAntiAliased):
                CheckAntiAlias.Checked = ViewModel.IsAntiAliased;
                break;
            case nameof(IFontContent.CommandContext):
                if (ViewModel.CommandContext is not null)
                {
                    ViewModel.CommandContext.PropertyChanged += CommandContext_PropertyChanged;
                }
                break;
        }

        ValidateButtons();
    }

    /// <summary>Handles the PropertyChanged event of the CommandContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
    private void CommandContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ITextureEditorContext.UsePremultipliedAlpha):
                CheckPremultiply.Checked = ViewModel.TextureEditor.UsePremultipliedAlpha;
                break;
        }
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the MenuItemSolid control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void MenuItemSolid_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.TextureEditor?.ActivateSolidBrushCommand is null) || (!ViewModel.TextureEditor.ActivateSolidBrushCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.TextureEditor.ActivateSolidBrushCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the MenuItemPattern control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void MenuItemPattern_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.TextureEditor?.ActivatePatternBrushCommand is null) || (!ViewModel.TextureEditor.ActivatePatternBrushCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.TextureEditor.ActivatePatternBrushCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the MenuItemGradient control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void MenuItemGradient_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.TextureEditor?.ActivateGradientBrushCommand is null) || (!ViewModel.TextureEditor.ActivateGradientBrushCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.TextureEditor.ActivateGradientBrushCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the MenuItemTextured control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void MenuItemTextured_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.TextureEditor?.ActivateTextureBrushCommand is null) || (!ViewModel.TextureEditor.ActivateTextureBrushCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.TextureEditor.ActivateTextureBrushCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the save button control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private async void ButtonSave_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.SaveContentCommand is null) || (!ViewModel.SaveContentCommand.CanExecute(SaveReason.UserSave)))
        {
            return;
        }

        await ViewModel.SaveContentCommand.ExecuteAsync(SaveReason.UserSave);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonCharacterSelection control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ButtonCharacterSelection_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.ActivateCharacterSelectionCommand is null) || (!ViewModel.ActivateCharacterSelectionCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.ActivateCharacterSelectionCommand.Execute(null);
    }

    /// <summary>Handles the CheckedChanged event of the CheckBold control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private async void CheckBold_CheckedChanged(object sender, EventArgs e)
    {
        if ((ViewModel?.SetBoldCommand is null) || (!ViewModel.SetBoldCommand.CanExecute(CheckBold.Checked)))
        {
            return;
        }

        await ViewModel.SetBoldCommand.ExecuteAsync(CheckBold.Checked);
        ValidateButtons();
    }

    /// <summary>Handles the CheckedChanged event of the CheckItalics control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private async void CheckItalics_CheckedChanged(object sender, EventArgs e)
    {
        if ((ViewModel?.SetItalicCommand is null) || (!ViewModel.SetItalicCommand.CanExecute(CheckItalics.Checked)))
        {
            return;
        }

        await ViewModel.SetItalicCommand.ExecuteAsync(CheckItalics.Checked);
        ValidateButtons();
    }

    /// <summary>Handles the CheckedChanged event of the CheckAntiAlias control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private async void CheckAntiAlias_CheckedChanged(object sender, EventArgs e)
    {
        if ((ViewModel?.SetAntiAliasCommand is null) || (!ViewModel.SetItalicCommand.CanExecute(CheckAntiAlias.Checked)))
        {
            return;
        }

        await ViewModel.SetAntiAliasCommand.ExecuteAsync(CheckAntiAlias.Checked);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the CheckPremultiply control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private async void CheckPremultiply_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.TextureEditor?.SetPremultipliedCommand is null) || (!ViewModel.TextureEditor.SetPremultipliedCommand.CanExecute(CheckPremultiply.Checked)))
        {
            return;
        }

        await ViewModel.TextureEditor.SetPremultipliedCommand.ExecuteAsync(CheckPremultiply.Checked);
        ValidateButtons();
    }

    /// <summary>Handles the CheckedChanged event of the RadioPointUnits control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private async void RadioPointUnits_CheckedChanged(object sender, EventArgs e)
    {
        if ((ViewModel?.SetFontUnitsCommand is null) || (!ViewModel.SetFontUnitsCommand.CanExecute(RadioPointUnits.Checked)))
        {
            return;
        }

        await ViewModel.SetFontUnitsCommand.ExecuteAsync(RadioPointUnits.Checked);
        ValidateButtons();
    }

    /// <summary>Handles the CheckedChanged event of the RadioPixelUnits control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private async void RadioPixelUnits_CheckedChanged(object sender, EventArgs e)
    {
        if ((ViewModel?.SetFontUnitsCommand is null) || (!ViewModel.SetFontUnitsCommand.CanExecute(!RadioPixelUnits.Checked)))
        {
            return;
        }

        await ViewModel.SetFontUnitsCommand.ExecuteAsync(!RadioPixelUnits.Checked);
        ValidateButtons();
    }

    /// <summary>
    /// Function to unassign the events for the data context.
    /// </summary>
    private void UnassignEvents()
    {
        ComboFonts.SelectedIndexChanged -= ComboFonts_SelectedIndexChanged;
        NumericSize.ValueChanged -= NumericSize_ValueChanged;

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanging -= DataContext_PropertyChanging;
        ViewModel.PropertyChanged -= DataContext_PropertyChanged;
    }

    /// <summary>
    /// Function to reset the view when no data context is assigned.
    /// </summary>
    private void ResetDataContext()
    {
        RibbonTextContent.Enabled = false;
        UpdateZoomMenu();
        ItemZoomToWindow.Checked = true;
    }

    /// <summary>Handles the ValueChanged event of the NumericSize control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private async void NumericSize_ValueChanged(object sender, EventArgs e)
    {
        float value = (float)NumericSize.Value;

        if ((ViewModel?.SetSizeCommand is null) || (!ViewModel.SetSizeCommand.CanExecute(value)))
        {
            return;
        }

        await ViewModel.SetSizeCommand.ExecuteAsync(value);
    }

    /// <summary>Handles the SelectedIndexChanged event of the ComboFonts control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private async void ComboFonts_SelectedIndexChanged(object sender, EventArgs e)
    {
        if ((ViewModel?.SetFontFamilyCommand is null) || (!ViewModel.SetFontFamilyCommand.CanExecute(ComboFonts.Text)))
        {
            return;
        }

        await ViewModel.SetFontFamilyCommand.ExecuteAsync(ComboFonts.Text);
    }

    /// <summary>Handles the Click event of the ItemZoomToWindow control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The [EventArgs] instance containing the event data.</param>
    private void ItemZoom_Click(object sender, EventArgs e)
    {
        ToolStripMenuItem item = (ToolStripMenuItem)sender;

        if ((item.Tag is null) || (!Enum.TryParse(item.Tag.ToString(), out ZoomLevels zoom)))
        {
            item.Checked = false;
            return;
        }

        // Do not let us uncheck.
        // Yes, this is annoying. Blame Microsoft for a lousy implementation.
        if (_zoomLevel == zoom)
        {
            item.Checked = true;
            return;
        }

        _zoomLevel = zoom;
        UpdateZoomMenu();

        ContentRenderer?.MoveTo(new Vector2(ContentRenderer.ClientSize.X * 0.5f, ContentRenderer.ClientSize.Y * 0.5f),
                                _zoomLevel.GetScale());
    }

    /// <summary>Handles the Click event of the ButtonFontUndo control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonFontUndo_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.UndoCommand is null) || (!ViewModel.UndoCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.UndoCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonFontRedo control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonFontRedo_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.RedoCommand is null) || (!ViewModel.RedoCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.RedoCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonOutline control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ButtonOutline_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.ActivateOutlineCommand is null) || (!ViewModel.ActivateOutlineCommand.CanExecute(ButtonOutline.Checked)))
        {
            return;
        }

        ViewModel.ActivateOutlineCommand.Execute(ButtonOutline.Checked);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonTexture control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ButtonTexture_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.ActivateTextureContextCommand is null) || (!ViewModel.ActivateTextureContextCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.ActivateTextureContextCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonPrevTexture control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ButtonPrevTexture_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.TextureEditor?.PrevTextureCommand is null) || (!ViewModel.TextureEditor.PrevTextureCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.TextureEditor.PrevTextureCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonNextTexture control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ButtonNextTexture_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.TextureEditor?.NextTextureCommand is null) || (!ViewModel.TextureEditor.NextTextureCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.TextureEditor.NextTextureCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonfirstTexture control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ButtonfirstTexture_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.TextureEditor?.FirstTextureCommand is null) || (!ViewModel.TextureEditor.FirstTextureCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.TextureEditor.FirstTextureCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonLastTexture control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ButtonLastTexture_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.TextureEditor?.LastTextureCommand is null) || (!ViewModel.TextureEditor.LastTextureCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.TextureEditor.LastTextureCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonResizeTexture control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ButtonResizeTexture_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.TextureEditor?.ActivateTextureSizeCommand is null) || (!ViewModel.TextureEditor.ActivateTextureSizeCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.TextureEditor.ActivateTextureSizeCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonGlyphPad control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ButtonGlyphPad_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.TextureEditor?.ActivatePaddingCommand is null) || (!ViewModel.TextureEditor.ActivatePaddingCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.TextureEditor.ActivatePaddingCommand.Execute(null);
        ValidateButtons();
    }

    /// <summary>Handles the Click event of the ButtonNew control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private async void ButtonNew_Click(object sender, EventArgs e)
    {
        if ((ViewModel?.NewFontCommand is null) || (!ViewModel.NewFontCommand.CanExecute(null)))
        {
            return;
        }

        await ViewModel.NewFontCommand.ExecuteAsync(null);
        ValidateButtons();
    }

    /// <summary>
    /// Function to initialize the view based on the data context.
    /// </summary>
    /// <param name="dataContext">The data context used to initialize.</param>
    private void InitializeFromDataContext(IFontContent dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        UpdateZoomMenu();
        ComboFonts.RefreshFonts(true);
        ComboFonts.Text = dataContext.FontFamily;
        NumericSize.Value = (decimal)dataContext.FontSize;
        RadioPointUnits.Checked = dataContext.FontUnits == GorgonFontHeightMode.Points;
        RadioPixelUnits.Checked = !RadioPointUnits.Checked;
        CheckBold.Checked = dataContext.IsBold;
        CheckItalics.Checked = dataContext.IsItalic;
        CheckAntiAlias.Checked = dataContext.IsAntiAliased;
        CheckPremultiply.Checked = dataContext.TextureEditor.UsePremultipliedAlpha;
    }

    /// <summary>
    /// Function to validate the state of the buttons.
    /// </summary>
    public void ValidateButtons()
    {
        ButtonZoom.Enabled = _contentRenderer?.CanZoom ?? true;
        LabelFontFamily.Enabled = CustomFonts.Enabled = ComboFonts.Enabled = ViewModel?.SetFontFamilyCommand?.CanExecute(ComboFonts.Text) ?? false;
        RadioPointUnits.Enabled = RadioPixelUnits.Enabled = ViewModel?.SetFontUnitsCommand?.CanExecute(true) ?? false;
        ButtonSaveFont.Enabled = ViewModel?.SaveContentCommand?.CanExecute(SaveReason.UserSave) ?? false;
        ButtonTextUndo.Enabled = ViewModel?.UndoCommand?.CanExecute(null) ?? false;
        ButtonTextRedo.Enabled = ViewModel?.RedoCommand?.CanExecute(null) ?? false;
        CheckBold.Enabled = ViewModel?.SetBoldCommand?.CanExecute(true) ?? false;
        CheckItalics.Enabled = ViewModel?.SetItalicCommand?.CanExecute(true) ?? false;
        CheckAntiAlias.Enabled = ViewModel?.SetAntiAliasCommand?.CanExecute(true) ?? false;
        LabelSize.Enabled = CustomControlSize.Enabled = NumericSize.Enabled = ViewModel?.SetSizeCommand?.CanExecute((float)NumericSize.Value) ?? false;
        ButtonOutline.Enabled = ViewModel?.ActivateOutlineCommand?.CanExecute(ButtonOutline.Checked) ?? false;
        ButtonTexture.Enabled = ViewModel?.ActivateTextureContextCommand?.CanExecute(null) ?? false;
        CheckPremultiply.Enabled = ViewModel?.TextureEditor?.SetPremultipliedCommand?.CanExecute(true) ?? false;
        ButtonPrevTexture.Enabled = ViewModel?.TextureEditor?.PrevTextureCommand?.CanExecute(null) ?? false;
        ButtonNextTexture.Enabled = ViewModel?.TextureEditor?.NextTextureCommand?.CanExecute(null) ?? false;
        ButtonfirstTexture.Enabled = ViewModel?.TextureEditor?.FirstTextureCommand?.CanExecute(null) ?? false;
        ButtonLastTexture.Enabled = ViewModel?.TextureEditor?.LastTextureCommand?.CanExecute(null) ?? false;
        ButtonResizeTexture.Enabled = ViewModel?.TextureEditor?.ActivateTextureSizeCommand?.CanExecute(null) ?? false;
        ButtonGlyphPad.Enabled = ViewModel?.TextureEditor?.ActivatePaddingCommand?.CanExecute(null) ?? false;
        ButtonCharacterSelection.Enabled = ViewModel?.ActivateCharacterSelectionCommand?.CanExecute(null) ?? false;
        ButtonNew.Enabled = ViewModel?.NewFontCommand?.CanExecute(null) ?? false;
        MenuItemSolid.Enabled = ViewModel?.TextureEditor?.ActivateSolidBrushCommand?.CanExecute(null) ?? false;
        MenuItemPattern.Enabled = ViewModel?.TextureEditor?.ActivatePatternBrushCommand?.CanExecute(null) ?? false;
        MenuItemGradient.Enabled = ViewModel?.TextureEditor?.ActivateGradientBrushCommand?.CanExecute(null) ?? false;
        MenuItemTextured.Enabled = ViewModel?.TextureEditor?.ActivateTextureBrushCommand?.CanExecute(null) ?? false;
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IFontContent dataContext)
    {
        UnassignEvents();

        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;
        ValidateButtons();

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanging += DataContext_PropertyChanging;
        ViewModel.PropertyChanged += DataContext_PropertyChanged;
        ComboFonts.SelectedIndexChanged += ComboFonts_SelectedIndexChanged;
        NumericSize.ValueChanged += NumericSize_ValueChanged;
    }

    /// <summary>Initializes a new instance of the FormRibbon class.</summary>
    public FormRibbon()
    {
        InitializeComponent();

        foreach (ToolStripMenuItem menuItem in MenuZoom.Items.OfType<ToolStripMenuItem>())
        {
            if (!Enum.TryParse(menuItem.Tag.ToString(), out ZoomLevels level))
            {
                menuItem.Enabled = false;
                continue;
            }

            menuItem.Text = level.GetName();
            _menuZoomItems[level] = menuItem;
        }
    }
}
