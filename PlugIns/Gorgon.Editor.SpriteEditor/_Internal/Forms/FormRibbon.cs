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
// Created: April 2, 2019 11:23:30 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using ComponentFactory.Krypton.Ribbon;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Provides a ribbon interface for the plug in view.
    /// </summary>
    /// <remarks>
    /// We cannot provide a ribbon on the control directly. For some reason, the krypton components will only allow ribbons on forms.
    /// </remarks>
    internal partial class FormRibbon
        : KryptonForm, IDataContext<ISpriteContent>
    {
        #region Variables.
        // The list of menu items associated with the zoom level.
        private readonly Dictionary<ZoomLevels, ToolStripMenuItem> _menuItems = new Dictionary<ZoomLevels, ToolStripMenuItem>();
        // The buttons on the ribbon.
        private readonly List<WeakReference<KryptonRibbonGroupButton>> _ribbonButtons = new List<WeakReference<KryptonRibbonGroupButton>>();
        // The numeric controls on the ribbon.
        private readonly List<WeakReference<KryptonRibbonGroupNumericUpDown>> _ribbonNumerics = new List<WeakReference<KryptonRibbonGroupNumericUpDown>>();
        // A list of buttons mapped to the tool structure.
        private readonly Dictionary<string, WeakReference<KryptonRibbonGroupButton>> _toolButtons = new Dictionary<string, WeakReference<KryptonRibbonGroupButton>>(StringComparer.OrdinalIgnoreCase);
        // The currently selected zoom level
        private ZoomLevels _zoomLevel = ZoomLevels.ToWindow;
        // The renderer for the content.
        private IContentRenderer _contentRenderer;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the current graphics context.
        /// </summary>        
        public IGraphicsContext GraphicsContext
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the data context for the ribbon on the form.
        /// </summary>
        public ISpriteContent DataContext
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

                if (_contentRenderer != null)
                {
                    ContentRenderer.ZoomScaleChanged -= ContentRenderer_ZoomScale;
                }

                _contentRenderer = value;

                if (_contentRenderer != null)
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
            _zoomLevel = _contentRenderer.ZoomLevel;
            UpdateZoomMenu();
        }

        /// <summary>
        /// Function to update the zoom item menu to reflect the current selection.
        /// </summary>
        private void UpdateZoomMenu()
        {
            if (!_menuItems.TryGetValue(_zoomLevel, out ToolStripMenuItem currentItem))
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

            ButtonZoomSprite.TextLine1 = string.Format(Resources.GORSPR_TEXT_ZOOM_BUTTON, _zoomLevel.GetName());
        }

        /// <summary>
        /// Function to ensure that visual states match the current tool setting.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void SetToolStates(ISpriteContent dataContext)
        {
            KryptonRibbonGroupButton button;            

            if (dataContext.CommandContext == null)
            {
                foreach (KeyValuePair<string, WeakReference<KryptonRibbonGroupButton>> buttonItem in _toolButtons)
                {
                    if (buttonItem.Value.TryGetTarget(out button))
                    {
                        button.Checked = false;
                    }
                }

                return;
            }

            if (!_toolButtons.TryGetValue(dataContext.CommandContext.Name, out WeakReference<KryptonRibbonGroupButton> buttonRef))
            {
                return;
            }

            if (!buttonRef.TryGetTarget(out button))
            {
                return;
            }

            button.Checked = true;
        }

        /// <summary>Handles the PropertyChanged event of the ManualVertexEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void SpriteVertexEditorContext_PropertyChanged(object sender, PropertyChangedEventArgs e) => ValidateButtons();

        /// <summary>Handles the PropertyChanged event of the SpritePickContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void SpritePickContext_PropertyChanged(object sender, PropertyChangedEventArgs e) => ValidateButtons();

        /// <summary>Handles the PropertyChanged event of the SpriteClipContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void SpriteClipContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteClipContext.FixedSize):
                    if (DataContext.SpriteClipContext.FixedSize != null)
                    {
                        DX.Size2F size = DataContext.SpriteClipContext.FixedSize.Value;
                        NumericFixedWidth.Value = ((decimal)size.Width).Min(NumericFixedWidth.Maximum).Max(NumericFixedWidth.Minimum);
                        NumericFixedHeight.Value = ((decimal)size.Height).Min(NumericFixedHeight.Maximum).Max(NumericFixedHeight.Minimum);
                    }
                    break;
            }

            ValidateButtons();
        }

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangedEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.IsPixellated):
                    MenuItemSmooth.Checked = !DataContext.IsPixellated;
                    MenuItemPixelated.Checked = !MenuItemSmooth.Checked;
                    break;
                case nameof(ISpriteContent.CommandContext):
                    SetToolStates(DataContext);
                    break;
                case nameof(ISpriteContent.Texture):
                    NumericFixedWidth.Maximum = DataContext?.Texture?.Width ?? 16384;
                    NumericFixedHeight.Maximum = DataContext?.Texture?.Height ?? 16384;
                    break;
            }

            ValidateButtons();
        }

        /// <summary>Handles the ValueChanged event of the NumericPadding control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericPadding_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext?.SpritePickContext == null)
            {
                return;
            }

            DataContext.SpritePickContext.Padding = (int)NumericPadding.Value;

            ValidateButtons();
        }

        /// <summary>Handles the ValueChanged event of the NumericFixedHeight control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericFixed_ValueChanged(object sender, EventArgs e)
        {
            var size = new DX.Size2F((float)NumericFixedWidth.Value, (float)NumericFixedHeight.Value);

            if ((!ButtonFixedSize.Checked)
                || (DataContext?.SpriteClipContext?.FixedSizeCommand == null)
                || (!DataContext.SpriteClipContext.FixedSizeCommand.CanExecute(size)))
            {
                return;
            }

            DataContext.SpriteClipContext.FixedSizeCommand.Execute(size);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonFixedSize control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFixedSize_Click(object sender, EventArgs e)
        {
            DX.Size2F? size = ButtonFixedSize.Checked ?  new DX.Size2F((float)NumericFixedWidth.Value, (float)NumericFixedHeight.Value) : (DX.Size2F?)null;

            if ((DataContext?.SpriteClipContext?.FixedSizeCommand == null) || (!DataContext.SpriteClipContext.FixedSizeCommand.CanExecute(size)))
            {
                return;
            }

            DataContext.SpriteClipContext.FixedSizeCommand.Execute(size);
            ValidateButtons();
        }


        /// <summary>Handles the Click event of the ButtonSpriteClipFullSize control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteClipFullSize_Click(object sender, EventArgs e)
        {
            // Ensure that fixed sizing is turned off.
            if (ButtonFixedSize.Checked)
            {
                ButtonFixedSize.PerformClick();
            }

            if ((DataContext?.SpriteClipContext?.FullSizeCommand == null) || (!DataContext.SpriteClipContext.FullSizeCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SpriteClipContext.FullSizeCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonPickMaskColor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPickMaskColor_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SpritePickContext?.ShowSpritePickMaskEditorCommand == null) || (!DataContext.SpritePickContext.ShowSpritePickMaskEditorCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SpritePickContext.ShowSpritePickMaskEditorCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteColor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteColor_Click(object sender, EventArgs e)
        {
            if ((DataContext?.ShowColorEditorCommand == null) || (!DataContext.ShowColorEditorCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ShowColorEditorCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteTextureWrap control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteTextureWrap_Click(object sender, EventArgs e)
        {
            if ((DataContext?.ShowWrappingEditorCommand == null) || (!DataContext.ShowWrappingEditorCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ShowWrappingEditorCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteAnchor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteAnchor_Click(object sender, EventArgs e)
        {
            if ((DataContext?.ShowAnchorEditorCommand == null) || (!DataContext.ShowAnchorEditorCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ShowAnchorEditorCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSaveSprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ButtonSaveSprite_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SaveContentCommand == null) || (!DataContext.SaveContentCommand.CanExecute(SaveReason.UserSave)))
            {
                return;
            }

            await DataContext.SaveContentCommand.ExecuteAsync(SaveReason.UserSave);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteRedo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteRedo_Click(object sender, EventArgs e)
        {
            if ((DataContext?.RedoCommand == null) || (!DataContext.RedoCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.RedoCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteUndo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteUndo_Click(object sender, EventArgs e)
        {
            if ((DataContext?.UndoCommand == null) || (!DataContext.UndoCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.UndoCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonPickSprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPickSprite_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SpritePickCommand == null) || (!DataContext.SpritePickCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SpritePickCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonClipSprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonClipSprite_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SpriteClipCommand == null) || (!DataContext.SpriteClipCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SpriteClipCommand.Execute(null);
            ValidateButtons();
        }


        /// <summary>Handles the Click event of the ButtonSpriteVertexOffsets control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteVertexOffsets_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SpriteVertexOffsetCommand == null) || (!DataContext.SpriteVertexOffsetCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SpriteVertexOffsetCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonNewSprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonNewSprite_Click(object sender, EventArgs e)
        {
            if ((DataContext?.NewSpriteCommand == null) || (!DataContext.NewSpriteCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.NewSpriteCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteCornerReset control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteCornerReset_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SpriteVertexEditContext?.ResetOffsetCommand == null) || (!DataContext.SpriteVertexEditContext.ResetOffsetCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SpriteVertexEditContext.ResetOffsetCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpritePickApply control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpritePickApply_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SpritePickContext?.ApplyCommand == null) || (!DataContext.SpritePickContext.ApplyCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SpritePickContext.ApplyCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpritePickCancel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpritePickCancel_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SpritePickContext?.CancelCommand == null) || (!DataContext.SpritePickContext.CancelCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SpritePickContext.CancelCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteClipApply control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteClipApply_Click(object sender, EventArgs e)
        {
            // Turn off the manual input window.
            if (ButtonClipManualInput.Checked)
            {
                ButtonClipManualInput.PerformClick();
            }

            if ((DataContext?.SpriteClipContext?.ApplyCommand == null) || (!DataContext.SpriteClipContext.ApplyCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SpriteClipContext.ApplyCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteClipCancel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteClipCancel_Click(object sender, EventArgs e)
        {
            // Turn off the manual input window.
            if (ButtonClipManualInput.Checked)
            {
                ButtonClipManualInput.PerformClick();
            }

            if ((DataContext?.SpriteClipContext?.CancelCommand == null) || (!DataContext.SpriteClipContext.CancelCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SpriteClipContext.CancelCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteCornerOffsetApply control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteCornerOffsetApply_Click(object sender, EventArgs e)
        {
            // Turn off the manual input window.
            if (ButtonSpriteCornerManualInput.Checked)
            {
                ButtonSpriteCornerManualInput.PerformClick();
            }
            
            if ((DataContext?.SpriteVertexEditContext?.ApplyCommand == null) || (!DataContext.SpriteVertexEditContext.ApplyCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SpriteVertexEditContext.ApplyCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteCornerOffsetCancel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteCornerOffsetCancel_Click(object sender, EventArgs e)
        {
            // Turn off the manual input window.
            if (ButtonSpriteCornerManualInput.Checked)
            {
                ButtonSpriteCornerManualInput.PerformClick();
            }

            if ((DataContext?.SpriteVertexEditContext?.CancelCommand == null) || (!DataContext.SpriteVertexEditContext.CancelCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SpriteVertexEditContext.CancelCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>
        /// Function to build the list of available ribbon buttons for ease of access.
        /// </summary>
        private void GetRibbonButtons()
        {
            foreach (KryptonRibbonTab tab in RibbonSpriteContent.RibbonTabs)
            {
                foreach (KryptonRibbonGroup grp in tab.Groups)
                {
                    // They really don't make it easy to get at the buttons do they?
                    foreach (KryptonRibbonGroupLines container in grp.Items.OfType<KryptonRibbonGroupLines>())
                    {
                        foreach (KryptonRibbonGroupButton item in container.Items.OfType<KryptonRibbonGroupButton>())
                        {
                            _ribbonButtons.Add(new WeakReference<KryptonRibbonGroupButton>(item));
                        }

                        foreach (KryptonRibbonGroupNumericUpDown item in container.Items.OfType<KryptonRibbonGroupNumericUpDown>())
                        {
                            _ribbonNumerics.Add(new WeakReference<KryptonRibbonGroupNumericUpDown>(item));
                        }
                    }

                    foreach (KryptonRibbonGroupTriple container in grp.Items.OfType<KryptonRibbonGroupTriple>())
                    {
                        foreach (KryptonRibbonGroupButton item in container.Items.OfType<KryptonRibbonGroupButton>())
                        {
                            _ribbonButtons.Add(new WeakReference<KryptonRibbonGroupButton>(item));
                        }


                        foreach (KryptonRibbonGroupNumericUpDown item in container.Items.OfType<KryptonRibbonGroupNumericUpDown>())
                        {
                            _ribbonNumerics.Add(new WeakReference<KryptonRibbonGroupNumericUpDown>(item));
                        }
                    }
                }
            }

            _toolButtons[nameof(SpriteClipContext)] = new WeakReference<KryptonRibbonGroupButton>(ButtonClipSprite);
            _toolButtons[nameof(SpritePickContext)] = new WeakReference<KryptonRibbonGroupButton>(ButtonPickSprite);
            _toolButtons[nameof(SpriteVertexEditContext)] = new WeakReference<KryptonRibbonGroupButton>(ButtonSpriteVertexOffsets);
        }

        /// <summary>
        /// Function to disable or enable all of the ribbon buttons.
        /// </summary>
        /// <param name="enable"><b>true</b> to enable all buttons, <b>false</b> to disable.</param>
        private void EnableRibbon(bool enable)
        {
            foreach (WeakReference<KryptonRibbonGroupButton> item in _ribbonButtons)
            {
                if (!item.TryGetTarget(out KryptonRibbonGroupButton button))
                {
                    continue;
                }

                button.Enabled = enable;
            }

            foreach (WeakReference<KryptonRibbonGroupNumericUpDown> item in _ribbonNumerics)
            {
                if (!item.TryGetTarget(out KryptonRibbonGroupNumericUpDown numeric))
                {
                    continue;
                }

                numeric.Enabled = enable;
            }
        }

        /// <summary>
        /// Function to unassign the events for the data context.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext == null)
            {
                return;
            }

            if (DataContext.SpriteClipContext != null)
            {
                DataContext.SpriteClipContext.PropertyChanged -= SpriteClipContext_PropertyChanged;
            }

            if (DataContext.SpritePickContext != null)
            {
                DataContext.SpritePickContext.PropertyChanged -= SpritePickContext_PropertyChanged;
            }

            if (DataContext.SpriteVertexEditContext != null)
            {
                DataContext.SpriteVertexEditContext.PropertyChanged -= SpriteVertexEditorContext_PropertyChanged;
            }

            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        /// <summary>
        /// Function to reset the view when no data context is assigned.
        /// </summary>
        private void ResetDataContext()
        {
            NumericFixedWidth.Minimum = 1;
            NumericFixedWidth.Maximum = 16384;
            NumericFixedWidth.Value = 32M;
            NumericFixedHeight.Minimum = 1;
            NumericFixedHeight.Maximum = 16384;
            NumericFixedHeight.Value = 32M;

            RibbonSpriteContent.Enabled = false;
            UpdateZoomMenu();
            ItemZoomToWindow.Checked = true;
        }

        /// <summary>Handles the Click event of the ItemZoomToWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void ItemZoom_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

            if ((item.Tag == null) || (!Enum.TryParse(item.Tag.ToString(), out ZoomLevels zoom)))
            {
                item.Checked = false;
                return;
            }

            // Do not let us uncheck.
            if (_zoomLevel == zoom)
            {
                item.Checked = true;
                return;
            }

            _zoomLevel = zoom;
            UpdateZoomMenu();

            ContentRenderer?.MoveTo(new DX.Vector2(ContentRenderer.ClientSize.Width * 0.5f, ContentRenderer.ClientSize.Height * 0.5f),
                                    _zoomLevel.GetScale());
        }

        /// <summary>Handles the Click event of the MenuItemSmooth control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemSmooth_Click(object sender, EventArgs e)
        {
            if (!MenuItemSmooth.Checked)
            {
                MenuItemSmooth.Checked = true;
                return;
            }

            MenuItemPixelated.Checked = false;

            if ((DataContext?.SetTextureFilteringCommand == null) || (!DataContext.SetTextureFilteringCommand.CanExecute(SampleFilter.MinMagMipLinear)))
            {
                return;
            }

            DataContext.SetTextureFilteringCommand.Execute(SampleFilter.MinMagMipLinear);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the MenuItemPixelated control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemPixelated_Click(object sender, EventArgs e)
        {
            if (!MenuItemPixelated.Checked)
            {
                MenuItemPixelated.Checked = true;
                return;
            }

            MenuItemSmooth.Checked = false;

            if ((DataContext?.SetTextureFilteringCommand == null) || (!DataContext.SetTextureFilteringCommand.CanExecute(SampleFilter.MinMagMipPoint)))
            {
                return;
            }

            DataContext.SetTextureFilteringCommand.Execute(SampleFilter.MinMagMipPoint);
            ValidateButtons();
        }

        /// <summary>
        /// Function to initialize the view based on the data context.
        /// </summary>
        /// <param name="dataContext">The data context used to initialize.</param>
        private void InitializeFromDataContext(ISpriteContent dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            if (dataContext.SpriteClipContext != null)
            {
                NumericPadding.Value = dataContext?.SpritePickContext.Padding.Min((int)NumericPadding.Maximum).Max((int)NumericPadding.Minimum) ?? 0;
                NumericFixedWidth.Maximum = dataContext?.Texture?.Width ?? 16384;
                NumericFixedHeight.Maximum = dataContext?.Texture?.Height ?? 16384;

                DX.Size2F size = dataContext.SpriteClipContext.FixedSize ?? new DX.Size2F(32, 32);
                NumericFixedWidth.Value = ((decimal)size.Width).Min(NumericFixedWidth.Maximum).Max(NumericFixedWidth.Minimum);
                NumericFixedHeight.Value = ((decimal)size.Height).Min(NumericFixedHeight.Maximum).Max(NumericFixedHeight.Minimum);
            }

            MenuItemSmooth.Checked = !dataContext.IsPixellated;
            MenuItemPixelated.Checked = !MenuItemSmooth.Checked;

            UpdateZoomMenu();
        }

        /// <summary>Raises the Load event.</summary>
        /// <param name="e">An EventArgs containing event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            ValidateButtons();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(ISpriteContent dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);

            DataContext = dataContext;
            ValidateButtons();

            if (DataContext == null)
            {
                return;
            }

            if (DataContext.SpriteClipContext != null)
            {
                DataContext.SpriteClipContext.PropertyChanged += SpriteClipContext_PropertyChanged;
            }

            if (dataContext.SpritePickContext != null)
            {
                DataContext.SpritePickContext.PropertyChanged += SpritePickContext_PropertyChanged;
            }

            if (DataContext.SpriteVertexEditContext != null)
            {
                DataContext.SpriteVertexEditContext.PropertyChanged += SpriteVertexEditorContext_PropertyChanged;
            }

            DataContext.PropertyChanged += DataContext_PropertyChanged;
        }        

        /// <summary>
        /// Function to validate the state of the buttons.
        /// </summary>
        public void ValidateButtons()
        {
            if (DataContext?.Texture == null)
            {
                EnableRibbon(false);
                return;
            }            

            ButtonNewSprite.Enabled = DataContext.NewSpriteCommand?.CanExecute(null) ?? false;
            ButtonClipSprite.Enabled = DataContext.SpriteClipCommand?.CanExecute(null) ?? false;
            ButtonPickSprite.Enabled = DataContext.SpritePickCommand?.CanExecute(null) ?? false;
            ButtonSpriteAnchor.Enabled = DataContext.ShowAnchorEditorCommand?.CanExecute(null) ?? false;
            ButtonSpriteVertexOffsets.Enabled = DataContext.SpriteVertexOffsetCommand?.CanExecute(null) ?? false;
            ButtonSpriteColor.Enabled = DataContext.ShowColorEditorCommand?.CanExecute(null) ?? false;

            ButtonSpriteUndo.Enabled = DataContext.UndoCommand?.CanExecute(null) ?? false;
            ButtonSpriteRedo.Enabled = DataContext.RedoCommand?.CanExecute(null) ?? false;

            ButtonSaveSprite.Enabled = DataContext.SaveContentCommand?.CanExecute(SaveReason.UserSave) ?? false;

            ButtonSpriteTextureFilter.Enabled = DataContext.SetTextureFilteringCommand?.CanExecute(MenuItemPixelated.Checked
                ? SampleFilter.MinMagMipPoint
                : SampleFilter.MinPointMagMipLinear) ?? false;
            ButtonSpriteTextureWrap.Enabled = DataContext.ShowWrappingEditorCommand?.CanExecute(null) ?? false;

            if (DataContext.SpritePickContext != null)
            {
                ButtonSpritePickApply.Enabled = DataContext.SpritePickContext.ApplyCommand?.CanExecute(null) ?? false;
                ButtonSpritePickCancel.Enabled = DataContext.SpritePickContext.CancelCommand?.CanExecute(null) ?? false;
                ButtonPickMaskColor.Enabled = DataContext.SpritePickContext.ShowSpritePickMaskEditorCommand?.CanExecute(null) ?? false;
            }

            if (DataContext.SpriteClipContext != null)
            {
                DX.Size2F? fixedSize = ButtonFixedSize.Enabled ? new DX.Size2F((float)NumericFixedWidth.Value, (float)NumericFixedHeight.Value)
                                                               : (DX.Size2F?)null;

                ButtonSpriteClipApply.Enabled = DataContext.SpriteClipContext.ApplyCommand?.CanExecute(null) ?? false;
                ButtonSpriteClipCancel.Enabled = DataContext.SpriteClipContext.CancelCommand?.CanExecute(null) ?? false;
                ButtonSpriteClipFullSize.Enabled = DataContext.SpriteClipContext.FullSizeCommand?.CanExecute(null) ?? false;
                ButtonFixedSize.Enabled = DataContext.SpriteClipContext.FixedSizeCommand?.CanExecute(fixedSize) ?? false;

                LabelFixedHeight.Enabled =
                    LabelFixedWidth.Enabled =
                    NumericFixedWidth.Enabled = 
                    NumericFixedWidth.NumericUpDown.Enabled =
                    NumericFixedHeight.Enabled = 
                    NumericFixedHeight.NumericUpDown.Enabled = DataContext.SpriteClipContext.FixedSize != null;

                ButtonClipManualInput.Enabled = DataContext.SpriteClipContext == DataContext.CommandContext;
            }

            if (DataContext.SpriteVertexEditContext != null)
            {
                ButtonSpriteCornerOffsetApply.Enabled = DataContext.SpriteVertexEditContext.ApplyCommand?.CanExecute(null) ?? false;
                ButtonSpriteCornerOffsetCancel.Enabled = DataContext.SpriteVertexEditContext.CancelCommand?.CanExecute(null) ?? false;
                ButtonSpriteCornerReset.Enabled = DataContext.SpriteVertexEditContext.ResetOffsetCommand?.CanExecute(null) ?? false;

                ButtonSpriteCornerManualInput.Enabled = (DataContext.SpriteVertexEditContext == DataContext.CommandContext) && (DataContext.SpriteVertexEditContext.SelectedVertexIndex != -1);
            }
        }

        /// <summary>
        /// Function to reset the zoom back to the default.
        /// </summary>
        public void ResetZoom()
        {
            _zoomLevel = ZoomLevels.ToWindow;
            UpdateZoomMenu();
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the FormRibbon class.</summary>
        public FormRibbon()
        {
            InitializeComponent();

            GetRibbonButtons();

            foreach (ToolStripMenuItem menuItem in MenuZoom.Items.OfType<ToolStripMenuItem>())
            {
                if (!Enum.TryParse(menuItem.Tag.ToString(), out ZoomLevels level))
                {
                    menuItem.Enabled = false;
                    continue;
                }

                menuItem.Text = level.GetName();
                _menuItems[level] = menuItem;
            }
        }
        #endregion
    }
}
