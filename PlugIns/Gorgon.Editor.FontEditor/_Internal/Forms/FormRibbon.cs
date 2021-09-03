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
// Created: February 13, 2021 4:04:07 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using Drawing = System.Drawing;
using System.Windows.Forms;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;
using Krypton.Toolkit;
using Gorgon.Graphics.Fonts;

namespace Gorgon.Editor.FontEditor
{
    /// <summary>
    /// Provides a ribbon interface for the plug in view.
    /// </summary>
    internal partial class FormRibbon
        : KryptonForm, IDataContext<IFontContent>
    {
        #region Variables.
        // The list of menu items associated with the zoom level.
        private readonly Dictionary<ZoomLevels, ToolStripMenuItem> _menuZoomItems = new();

        // The current zoom level.
        private ZoomLevels _zoomLevel = ZoomLevels.ToWindow;
        // The renderer for the content.
        private IContentRenderer _contentRenderer;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the data context for the ribbon on the form.
        /// </summary>
        public IFontContent DataContext
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
        #endregion

        #region Methods.
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
                    if (DataContext.CommandContext is not null)
                    {
                        DataContext.CommandContext.PropertyChanged -= CommandContext_PropertyChanged;
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
                    ComboFonts.Text = DataContext.FontFamily;
                    break;
                case nameof(IFontContent.FontSize):
                    NumericSize.ValueChanged -= NumericSize_ValueChanged;
                    NumericSize.Value = (decimal)DataContext.FontSize;
                    NumericSize.ValueChanged += NumericSize_ValueChanged;
                    break;
                case nameof(IFontContent.FontUnits):
                    RadioPointUnits.Checked = DataContext.FontUnits == FontHeightMode.Points;
                    RadioPixelUnits.Checked = !RadioPointUnits.Checked;
                    break;
                case nameof(IFontContent.IsBold):
                    CheckBold.Checked = DataContext.IsBold;
                    break;
                case nameof(IFontContent.IsItalic):
                    CheckItalics.Checked = DataContext.IsItalic;
                    break;
                case nameof(IFontContent.IsAntiAliased):
                    CheckAntiAlias.Checked = DataContext.IsAntiAliased;
                    break;
                case nameof(IFontContent.CommandContext):
                    if (DataContext.CommandContext is not null)
                    {
                        DataContext.CommandContext.PropertyChanged += CommandContext_PropertyChanged;
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
                    CheckPremultiply.Checked = DataContext.TextureEditor.UsePremultipliedAlpha;
                    break;
            }
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the MenuItemSolid control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void MenuItemSolid_Click(object sender, EventArgs e)
        {
            if ((DataContext?.TextureEditor?.ActivateSolidBrushCommand is null) || (!DataContext.TextureEditor.ActivateSolidBrushCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.TextureEditor.ActivateSolidBrushCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the MenuItemPattern control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void MenuItemPattern_Click(object sender, EventArgs e)
        {
            if ((DataContext?.TextureEditor?.ActivatePatternBrushCommand is null) || (!DataContext.TextureEditor.ActivatePatternBrushCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.TextureEditor.ActivatePatternBrushCommand.Execute(null);
            ValidateButtons();
        }


        /// <summary>Handles the Click event of the MenuItemGradient control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void MenuItemGradient_Click(object sender, EventArgs e)
        {
            if ((DataContext?.TextureEditor?.ActivateGradientBrushCommand is null) || (!DataContext.TextureEditor.ActivateGradientBrushCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.TextureEditor.ActivateGradientBrushCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the MenuItemTextured control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void MenuItemTextured_Click(object sender, EventArgs e)
        {
            if ((DataContext?.TextureEditor?.ActivateTextureBrushCommand is null) || (!DataContext.TextureEditor.ActivateTextureBrushCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.TextureEditor.ActivateTextureBrushCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the save button control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ButtonSave_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SaveContentCommand is null) || (!DataContext.SaveContentCommand.CanExecute(SaveReason.UserSave)))
            {
                return;
            }

            await DataContext.SaveContentCommand.ExecuteAsync(SaveReason.UserSave);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonCharacterSelection control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ButtonCharacterSelection_Click(object sender, EventArgs e)
        {
            if ((DataContext?.ActivateCharacterSelectionCommand is null) || (!DataContext.ActivateCharacterSelectionCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ActivateCharacterSelectionCommand.Execute(null);
        }

        /// <summary>Handles the CheckedChanged event of the CheckBold control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private async void CheckBold_CheckedChanged(object sender, EventArgs e)
        {
            if ((DataContext?.SetBoldCommand is null) || (!DataContext.SetBoldCommand.CanExecute(CheckBold.Checked)))
            {
                return;
            }

            await DataContext.SetBoldCommand.ExecuteAsync(CheckBold.Checked);
            ValidateButtons();
        }

        /// <summary>Handles the CheckedChanged event of the CheckItalics control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private async void CheckItalics_CheckedChanged(object sender, EventArgs e)
        {
            if ((DataContext?.SetItalicCommand is null) || (!DataContext.SetItalicCommand.CanExecute(CheckItalics.Checked)))
            {
                return;
            }

            await DataContext.SetItalicCommand.ExecuteAsync(CheckItalics.Checked);
            ValidateButtons();
        }

        /// <summary>Handles the CheckedChanged event of the CheckAntiAlias control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private async void CheckAntiAlias_CheckedChanged(object sender, EventArgs e)
        {
            if ((DataContext?.SetAntiAliasCommand is null) || (!DataContext.SetItalicCommand.CanExecute(CheckAntiAlias.Checked)))
            {
                return;
            }

            await DataContext.SetAntiAliasCommand.ExecuteAsync(CheckAntiAlias.Checked);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the CheckPremultiply control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private async void CheckPremultiply_Click(object sender, EventArgs e)
        {
            if ((DataContext?.TextureEditor?.SetPremultipliedCommand is null) || (!DataContext.TextureEditor.SetPremultipliedCommand.CanExecute(CheckPremultiply.Checked)))
            {
                return;
            }

            await DataContext.TextureEditor.SetPremultipliedCommand.ExecuteAsync(CheckPremultiply.Checked);
            ValidateButtons();
        }

        /// <summary>Handles the CheckedChanged event of the RadioPointUnits control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private async void RadioPointUnits_CheckedChanged(object sender, EventArgs e)
        {
            if ((DataContext?.SetFontUnitsCommand is null) || (!DataContext.SetFontUnitsCommand.CanExecute(RadioPointUnits.Checked)))
            {
                return;
            }

            await DataContext.SetFontUnitsCommand.ExecuteAsync(RadioPointUnits.Checked);
            ValidateButtons();
        }

        /// <summary>Handles the CheckedChanged event of the RadioPixelUnits control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private async void RadioPixelUnits_CheckedChanged(object sender, EventArgs e)
        {
            if ((DataContext?.SetFontUnitsCommand is null) || (!DataContext.SetFontUnitsCommand.CanExecute(!RadioPixelUnits.Checked)))
            {
                return;
            }

            await DataContext.SetFontUnitsCommand.ExecuteAsync(!RadioPixelUnits.Checked);
            ValidateButtons();
        }

        /// <summary>
        /// Function to unassign the events for the data context.
        /// </summary>
        private void UnassignEvents()
        {
            ComboFonts.SelectedIndexChanged -= ComboFonts_SelectedIndexChanged;
            NumericSize.ValueChanged -= NumericSize_ValueChanged;            

            if (DataContext is null)
            {
                return;
            }

            DataContext.PropertyChanging -= DataContext_PropertyChanging;
            DataContext.PropertyChanged -= DataContext_PropertyChanged;
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

            if ((DataContext?.SetSizeCommand is null) || (!DataContext.SetSizeCommand.CanExecute(value)))
            {
                return;
            }

            await DataContext.SetSizeCommand.ExecuteAsync(value);
        }

        /// <summary>Handles the SelectedIndexChanged event of the ComboFonts control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private async void ComboFonts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((DataContext?.SetFontFamilyCommand is null) || (!DataContext.SetFontFamilyCommand.CanExecute(ComboFonts.Text)))
            {
                return;
            }

            await DataContext.SetFontFamilyCommand.ExecuteAsync(ComboFonts.Text);
        }

        /// <summary>Handles the Click event of the ItemZoomToWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void ItemZoom_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

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

            ContentRenderer?.MoveTo(new Vector2(ContentRenderer.ClientSize.Width * 0.5f, ContentRenderer.ClientSize.Height * 0.5f),
                                    _zoomLevel.GetScale());
        }

        /// <summary>Handles the Click event of the ButtonFontUndo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFontUndo_Click(object sender, EventArgs e)
        {
            if ((DataContext?.UndoCommand is null) || (!DataContext.UndoCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.UndoCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonFontRedo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFontRedo_Click(object sender, EventArgs e)
        {
            if ((DataContext?.RedoCommand is null) || (!DataContext.RedoCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.RedoCommand.Execute(null);
            ValidateButtons();
        }


        /// <summary>Handles the Click event of the ButtonOutline control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ButtonOutline_Click(object sender, EventArgs e)
        {
            if ((DataContext?.ActivateOutlineCommand is null) || (!DataContext.ActivateOutlineCommand.CanExecute(ButtonOutline.Checked)))
            {
                return;
            }

            DataContext.ActivateOutlineCommand.Execute(ButtonOutline.Checked);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonTexture control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ButtonTexture_Click(object sender, EventArgs e)
        {
            if ((DataContext?.ActivateTextureContextCommand is null) || (!DataContext.ActivateTextureContextCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ActivateTextureContextCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonPrevTexture control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ButtonPrevTexture_Click(object sender, EventArgs e)
        {
            if ((DataContext?.TextureEditor?.PrevTextureCommand is null) || (!DataContext.TextureEditor.PrevTextureCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.TextureEditor.PrevTextureCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonNextTexture control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ButtonNextTexture_Click(object sender, EventArgs e)
        {
            if ((DataContext?.TextureEditor?.NextTextureCommand is null) || (!DataContext.TextureEditor.NextTextureCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.TextureEditor.NextTextureCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonfirstTexture control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ButtonfirstTexture_Click(object sender, EventArgs e)
        {
            if ((DataContext?.TextureEditor?.FirstTextureCommand is null) || (!DataContext.TextureEditor.FirstTextureCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.TextureEditor.FirstTextureCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonLastTexture control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ButtonLastTexture_Click(object sender, EventArgs e)
        {
            if ((DataContext?.TextureEditor?.LastTextureCommand is null) || (!DataContext.TextureEditor.LastTextureCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.TextureEditor.LastTextureCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonResizeTexture control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ButtonResizeTexture_Click(object sender, EventArgs e)
        {
            if ((DataContext?.TextureEditor?.ActivateTextureSizeCommand is null) || (!DataContext.TextureEditor.ActivateTextureSizeCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.TextureEditor.ActivateTextureSizeCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonGlyphPad control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ButtonGlyphPad_Click(object sender, EventArgs e)
        {
            if ((DataContext?.TextureEditor?.ActivatePaddingCommand is null) || (!DataContext.TextureEditor.ActivatePaddingCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.TextureEditor.ActivatePaddingCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonNew control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private async void ButtonNew_Click(object sender, EventArgs e)
        {
            if ((DataContext?.NewFontCommand is null) || (!DataContext.NewFontCommand.CanExecute(null)))
            {
                return;
            }

            await DataContext.NewFontCommand.ExecuteAsync(null);
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
            RadioPointUnits.Checked = dataContext.FontUnits == FontHeightMode.Points;
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
            LabelFontFamily.Enabled = CustomFonts.Enabled = ComboFonts.Enabled = DataContext?.SetFontFamilyCommand?.CanExecute(ComboFonts.Text) ?? false;
            RadioPointUnits.Enabled = RadioPixelUnits.Enabled = DataContext?.SetFontUnitsCommand?.CanExecute(true) ?? false;
            ButtonSaveFont.Enabled = DataContext?.SaveContentCommand?.CanExecute(SaveReason.UserSave) ?? false;
            ButtonTextUndo.Enabled = DataContext?.UndoCommand?.CanExecute(null) ?? false;
            ButtonTextRedo.Enabled = DataContext?.RedoCommand?.CanExecute(null) ?? false;
            CheckBold.Enabled = DataContext?.SetBoldCommand?.CanExecute(true) ?? false;
            CheckItalics.Enabled = DataContext?.SetItalicCommand?.CanExecute(true) ?? false;
            CheckAntiAlias.Enabled = DataContext?.SetAntiAliasCommand?.CanExecute(true) ?? false;
            LabelSize.Enabled = CustomControlSize.Enabled = NumericSize.Enabled = DataContext?.SetSizeCommand?.CanExecute((float)NumericSize.Value) ?? false;
            ButtonOutline.Enabled = DataContext?.ActivateOutlineCommand?.CanExecute(ButtonOutline.Checked) ?? false;
            ButtonTexture.Enabled = DataContext?.ActivateTextureContextCommand?.CanExecute(null) ?? false;
            CheckPremultiply.Enabled = DataContext?.TextureEditor?.SetPremultipliedCommand?.CanExecute(true) ?? false;
            ButtonPrevTexture.Enabled = DataContext?.TextureEditor?.PrevTextureCommand?.CanExecute(null) ?? false;
            ButtonNextTexture.Enabled = DataContext?.TextureEditor?.NextTextureCommand?.CanExecute(null) ?? false;
            ButtonfirstTexture.Enabled = DataContext?.TextureEditor?.FirstTextureCommand?.CanExecute(null) ?? false;
            ButtonLastTexture.Enabled = DataContext?.TextureEditor?.LastTextureCommand?.CanExecute(null) ?? false;
            ButtonResizeTexture.Enabled = DataContext?.TextureEditor?.ActivateTextureSizeCommand?.CanExecute(null) ?? false;
            ButtonGlyphPad.Enabled = DataContext?.TextureEditor?.ActivatePaddingCommand?.CanExecute(null) ?? false;
            ButtonCharacterSelection.Enabled = DataContext?.ActivateCharacterSelectionCommand?.CanExecute(null) ?? false;
            ButtonNew.Enabled = DataContext?.NewFontCommand?.CanExecute(null) ?? false;
            MenuItemSolid.Enabled = DataContext?.TextureEditor?.ActivateSolidBrushCommand?.CanExecute(null) ?? false;
            MenuItemPattern.Enabled = DataContext?.TextureEditor?.ActivatePatternBrushCommand?.CanExecute(null) ?? false;
            MenuItemGradient.Enabled = DataContext?.TextureEditor?.ActivateGradientBrushCommand?.CanExecute(null) ?? false;
            MenuItemTextured.Enabled = DataContext?.TextureEditor?.ActivateTextureBrushCommand?.CanExecute(null) ?? false;
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IFontContent dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);

            DataContext = dataContext;
            ValidateButtons();

            if (DataContext is null)
            {
                return;
            }

            DataContext.PropertyChanging += DataContext_PropertyChanging;
            DataContext.PropertyChanged += DataContext_PropertyChanged;            
            ComboFonts.SelectedIndexChanged += ComboFonts_SelectedIndexChanged;
            NumericSize.ValueChanged += NumericSize_ValueChanged;
        }        
        #endregion

        #region Constructor.
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
        #endregion
    }
}
