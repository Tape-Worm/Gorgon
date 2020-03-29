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
        private readonly Dictionary<SpriteEditTool, WeakReference<KryptonRibbonGroupButton>> _toolButtons = new Dictionary<SpriteEditTool, WeakReference<KryptonRibbonGroupButton>>();
        // The currently selected zoom level
        private ZoomLevels _zoomLevel = ZoomLevels.ToWindow;
        // The current scaling value applied for zooming.
        private float _zoomScale = -1.0f;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the current zoom level value.
        /// </summary>
        public ZoomLevels ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (_zoomLevel == value)
                {
                    return;
                }

                _zoomLevel = value;
                UpdateZoomMenu(false);
                ValidateButtons();
            }
        }

        /// <summary>
        /// Property to set or return the current scaling value used for zooming.
        /// </summary>
        public float ZoomScaling
        {
            get => _zoomScale;
            set
            {
                if (_zoomScale.EqualsEpsilon(value))
                {
                    return;
                }

                _zoomScale = value;
                UpdateZoomMenu(false);
                ValidateButtons();
            }
        }

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
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the zoom item menu to reflect the current selection.
        /// </summary>
        /// <param name="fireEvent"><b>true</b> to fire the zoom update event, <b>false</b> to only update the menu.</param>
        private void UpdateZoomMenu(bool fireEvent)
        {
            if (ZoomLevel == ZoomLevels.Custom)
            {
                ButtonZoomSprite.TextLine1 = string.Format(Resources.GORSPR_TEXT_ZOOM_BUTTON, (_zoomScale * 100.0f).ToString("0.#") + "%");
            }

            if (!_menuItems.TryGetValue(ZoomLevel, out ToolStripMenuItem currentItem))
            {
                return;
            }

            foreach (ToolStripMenuItem item in MenuZoom.Items.OfType<ToolStripMenuItem>().Where(item => item != currentItem))
            {
                item.Checked = false;
            }

            ButtonZoomSprite.TextLine1 = string.Format(Resources.GORSPR_TEXT_ZOOM_BUTTON, ZoomLevel.GetName());

            if (!fireEvent)
            {
                return;
            }
        }

        /// <summary>
        /// Function to ensure that visual states match the current tool setting.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void SetToolStates(ISpriteContent dataContext)
        {
            KryptonRibbonGroupButton button;


            if (dataContext.CurrentTool == SpriteEditTool.None)
            {
                foreach (KeyValuePair<SpriteEditTool, WeakReference<KryptonRibbonGroupButton>> buttonItem in _toolButtons)
                {
                    if (buttonItem.Value.TryGetTarget(out button))
                    {
                        button.Checked = false;
                    }
                }

                return;
            }

            if (!_toolButtons.TryGetValue(dataContext.CurrentTool, out WeakReference<KryptonRibbonGroupButton> buttonRef))
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
        private void ManualVertexEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
/*            switch (e.PropertyName)
            {
                case nameof(IManualInputViewModel.IsActive):
                    ButtonSpriteCornerManualInput.Checked = DataContext.ManualVertexEditor.IsActive;
                    break;
            }*/

            ValidateButtons();
        }

        /// <summary>Handles the PropertyChanged event of the ManualRectangleEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void ManualRectangleEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
/*            switch (e.PropertyName)
            {
                case nameof(IManualInputViewModel.IsActive):
                    ButtonClipManualInput.Checked = DataContext.ManualRectangleEditor.IsActive;
                    break;
                case nameof(IManualRectangleEditor.IsFixedSize):
                    ButtonFixedSize.Checked = DataContext.ManualRectangleEditor.IsFixedSize;
                    break;
                case nameof(IManualRectangleEditor.Padding):
                    int padding = (int)NumericPadding.Value;

                    if (padding == DataContext.ManualRectangleEditor.Padding)
                    {
                        break;
                    }

                    NumericPadding.Value = DataContext.ManualRectangleEditor.Padding.Min((int)NumericPadding.Maximum).Max((int)NumericPadding.Minimum);
                    break;
                case nameof(IManualRectangleEditor.FixedSize):
                    NumericFixedWidth.Value = ((decimal)DataContext.ManualRectangleEditor.FixedSize.Width).Min(NumericFixedWidth.Maximum).Max(NumericFixedWidth.Minimum);
                    NumericFixedHeight.Value = ((decimal)DataContext.ManualRectangleEditor.FixedSize.Height).Min(NumericFixedHeight.Maximum).Max(NumericFixedHeight.Minimum);
                    break;
            }*/

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
                case nameof(ISpriteContent.CurrentTool):
                    SetToolStates(DataContext);
                    break;
                case nameof(ISpriteContent.Texture):
                    NumericFixedWidth.Maximum = DataContext?.Texture?.Width ?? 16384;
                    NumericFixedHeight.Maximum = DataContext?.Texture?.Height ?? 16384;
                    break;
            }

            ValidateButtons();
        }

        /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangingEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            // Not used yet.
        }

        /// <summary>Handles the ValueChanged event of the NumericPadding control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericPadding_ValueChanged(object sender, EventArgs e)
        {
/*            if (DataContext?.ManualRectangleEditor == null)
            {
                return;
            }

            DataContext.ManualRectangleEditor.Padding = (int)NumericPadding.Value;*/

            ValidateButtons();
        }

        /// <summary>Handles the ValueChanged event of the NumericFixedWidth control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericFixedWidth_ValueChanged(object sender, EventArgs e)
        {
/*            if (DataContext?.ManualRectangleEditor?.SetFixedWidthHeightCommand == null)
            {
                return;
            }

#warning This is here due to the bug in the ribbon and numeric enabling/disabling.
            if (!DataContext.ManualRectangleEditor.IsFixedSize)
            {
                return;
            }

            var size = new DX.Size2F((float)NumericFixedWidth.Value, DataContext.ManualRectangleEditor.FixedSize.Height);

            if (size.Width.EqualsEpsilon(DataContext.ManualRectangleEditor.FixedSize.Width))
            {
                return;
            }

            if (!DataContext.ManualRectangleEditor.SetFixedWidthHeightCommand.CanExecute(size))
            {
                return;
            }

            DataContext.ManualRectangleEditor.SetFixedWidthHeightCommand.Execute(size);*/
        }

        /// <summary>Handles the ValueChanged event of the NumericFixedHeight control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericFixedHeight_ValueChanged(object sender, EventArgs e)
        {
/*            if (DataContext?.ManualRectangleEditor?.SetFixedWidthHeightCommand == null)
            {
                return;
            }

#warning This is here due to the bug in the ribbon and numeric enabling/disabling.
            if (!DataContext.ManualRectangleEditor.IsFixedSize)
            {
                return;
            }

            var size = new DX.Size2F(DataContext.ManualRectangleEditor.FixedSize.Width, (float)NumericFixedHeight.Value);

            if (size.Height.EqualsEpsilon(DataContext.ManualRectangleEditor.FixedSize.Height))
            {
                return;
            }

            if (!DataContext.ManualRectangleEditor.SetFixedWidthHeightCommand.CanExecute(size))
            {
                return;
            }

            DataContext.ManualRectangleEditor.SetFixedWidthHeightCommand.Execute(size);*/
        }

        /// <summary>Handles the Click event of the ButtonFixedSize control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFixedSize_Click(object sender, EventArgs e)
        {
            var size = new DX.Size2F((float)NumericFixedWidth.Value, (float)NumericFixedHeight.Value);

/*            if ((DataContext?.ManualRectangleEditor?.ToggleFixedSizeCommand == null) || (!DataContext.ManualRectangleEditor.ToggleFixedSizeCommand.CanExecute(size)))
            {
                return;
            }

            DataContext.ManualRectangleEditor.ToggleFixedSizeCommand.Execute(size);*/
            ValidateButtons();
        }


        /// <summary>Handles the Click event of the ButtonSpriteClipFullSize control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteClipFullSize_Click(object sender, EventArgs e)
        {
/*            if ((DataContext?.Texture == null) || (DataContext?.ManualRectangleEditor == null))
            {
                return;
            }

            DataContext.ManualRectangleEditor.Rectangle = new DX.RectangleF(0, 0, DataContext.Texture.Width, DataContext.Texture.Height);*/
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonPickMaskColor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPickMaskColor_Click(object sender, EventArgs e)
        {
            if ((DataContext?.ShowSpritePickMaskEditorCommand == null) || (!DataContext.ShowSpritePickMaskEditorCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ShowSpritePickMaskEditorCommand.Execute(null);
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

        /// <summary>Handles the Click event of the ButtonClipManualInput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonClipManualInput_Click(object sender, EventArgs e)
        {
            if ((DataContext?.ToggleManualClipRectCommand == null) || (!DataContext.ToggleManualClipRectCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ToggleManualClipRectCommand.Execute(null);
        }


        /// <summary>Handles the Click event of the ButtonSpriteCornerManualInput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteCornerOffsetManualInput_Click(object sender, EventArgs e)
        {
            if ((DataContext?.ToggleManualVertexEditCommand == null) || (!DataContext.ToggleManualVertexEditCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ToggleManualVertexEditCommand.Execute(null);
            ValidateButtons();
        }


        /// <summary>Handles the Click event of the ButtonSpriteCornerReset control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteCornerReset_Click(object sender, EventArgs e)
        {
/*            if ((DataContext?.ManualVertexEditor?.ResetOffsetCommand == null) || (!DataContext.ManualVertexEditor.ResetOffsetCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ManualVertexEditor.ResetOffsetCommand.Execute(null);*/
            ValidateButtons();
        }


        /// <summary>Handles the Click event of the ButtonSpritePickApply control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpritePickApply_Click(object sender, EventArgs e)
        {
/*            if ((DataContext?.ManualRectangleEditor?.ApplyCommand == null) || (!DataContext.ManualRectangleEditor.ApplyCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ManualRectangleEditor.ApplyCommand.Execute(null);*/
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpritePickCancel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpritePickCancel_Click(object sender, EventArgs e)
        {
/*            if ((DataContext?.ManualRectangleEditor?.CancelCommand == null) || (!DataContext.ManualRectangleEditor.CancelCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ManualRectangleEditor.CancelCommand.Execute(null);*/
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteClipApply control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteClipApply_Click(object sender, EventArgs e)
        {
/*            if ((DataContext?.ManualRectangleEditor?.ApplyCommand == null) || (!DataContext.ManualRectangleEditor.ApplyCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ManualRectangleEditor.ApplyCommand.Execute(null);*/
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteClipCancel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteClipCancel_Click(object sender, EventArgs e)
        {
/*            if ((DataContext?.ManualRectangleEditor?.CancelCommand == null) || (!DataContext.ManualRectangleEditor.CancelCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ManualRectangleEditor.CancelCommand.Execute(null);*/
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteCornerOffsetApply control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteCornerOffsetApply_Click(object sender, EventArgs e)
        {
/*            if ((DataContext?.ManualVertexEditor?.ApplyCommand == null) || (!DataContext.ManualVertexEditor.ApplyCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ManualVertexEditor.ApplyCommand.Execute(null);*/
            ValidateButtons();

        }

        /// <summary>Handles the Click event of the ButtonSpriteCornerOffsetCancel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteCornerOffsetCancel_Click(object sender, EventArgs e)
        {
/*            if ((DataContext?.ManualVertexEditor?.CancelCommand == null) || (!DataContext.ManualVertexEditor.CancelCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ManualVertexEditor.CancelCommand.Execute(null);*/
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

            _toolButtons[SpriteEditTool.SpriteClip] = new WeakReference<KryptonRibbonGroupButton>(ButtonClipSprite);
            _toolButtons[SpriteEditTool.SpritePick] = new WeakReference<KryptonRibbonGroupButton>(ButtonPickSprite);
            _toolButtons[SpriteEditTool.CornerResize] = new WeakReference<KryptonRibbonGroupButton>(ButtonSpriteVertexOffsets);
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

/*            if (DataContext.ManualRectangleEditor != null)
            {
                DataContext.ManualRectangleEditor.PropertyChanged -= ManualRectangleEditor_PropertyChanged;
            }

            if (DataContext.ManualVertexEditor != null)
            {
                DataContext.ManualVertexEditor.PropertyChanged -= ManualVertexEditor_PropertyChanged;
            }*/

            DataContext.PropertyChanging -= DataContext_PropertyChanging;
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
            UpdateZoomMenu(true);
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

            try
            {
                // Do not let us uncheck.
                if (ZoomLevel == zoom)
                {
                    item.Checked = true;
                    return;
                }

                ZoomLevel = zoom;
                UpdateZoomMenu(true);
            }
            finally
            {
                ValidateButtons();
            }
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

/*            if (dataContext.ManualRectangleEditor != null)
            {
                NumericPadding.Value = dataContext.ManualRectangleEditor.Padding.Min((int)NumericPadding.Maximum).Max((int)NumericPadding.Minimum);
                NumericFixedWidth.Value = ((decimal)dataContext.ManualRectangleEditor.FixedSize.Width).Min(NumericFixedWidth.Maximum).Max(NumericFixedWidth.Minimum);
                NumericFixedHeight.Value = ((decimal)dataContext.ManualRectangleEditor.FixedSize.Height).Min(NumericFixedHeight.Maximum).Max(NumericFixedHeight.Minimum);
            }*/

            MenuItemSmooth.Checked = !dataContext.IsPixellated;
            MenuItemPixelated.Checked = !MenuItemSmooth.Checked;

            UpdateZoomMenu(true);
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

/*            if (DataContext.ManualRectangleEditor != null)
            {
                DataContext.ManualRectangleEditor.PropertyChanged += ManualRectangleEditor_PropertyChanged;
            }

            if (DataContext.ManualVertexEditor != null)
            {
                DataContext.ManualVertexEditor.PropertyChanged += ManualVertexEditor_PropertyChanged;
            }*/

            DataContext.PropertyChanged += DataContext_PropertyChanged;
            DataContext.PropertyChanging += DataContext_PropertyChanging;
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

            // Temporary
            EnableRibbon(true);

            ButtonZoomSprite.Enabled = true;

            ButtonNewSprite.Enabled = DataContext.NewSpriteCommand?.CanExecute(null) ?? false;
            ButtonClipSprite.Enabled = DataContext.SpriteClipCommand?.CanExecute(null) ?? false;
            ButtonPickSprite.Enabled = DataContext.SpritePickCommand?.CanExecute(null) ?? false;
            ButtonSpriteAnchor.Enabled = DataContext.ShowAnchorEditorCommand?.CanExecute(null) ?? false;
            ButtonSpriteVertexOffsets.Enabled = DataContext.SpriteVertexOffsetCommand?.CanExecute(null) ?? false;
            ButtonSpriteColor.Enabled = DataContext.ShowColorEditorCommand?.CanExecute(null) ?? false;

            ButtonSpriteUndo.Enabled = DataContext.UndoCommand?.CanExecute(null) ?? false;
            ButtonSpriteRedo.Enabled = DataContext.RedoCommand?.CanExecute(null) ?? false;

            ButtonSaveSprite.Enabled = DataContext.SaveContentCommand?.CanExecute(SaveReason.UserSave) ?? false;

            ButtonClipManualInput.Enabled = DataContext.ToggleManualClipRectCommand?.CanExecute(null) ?? false;
            ButtonSpriteCornerManualInput.Enabled = DataContext.ToggleManualVertexEditCommand?.CanExecute(-1) ?? false;
            //ButtonSpriteCornerReset.Enabled = DataContext.ManualVertexEditor?.ResetOffsetCommand?.CanExecute(null) ?? false;

            ButtonSpriteTextureFilter.Enabled = DataContext.SetTextureFilteringCommand?.CanExecute(MenuItemPixelated.Checked
                ? SampleFilter.MinMagMipPoint
                : SampleFilter.MinPointMagMipLinear) ?? false;
            ButtonSpriteTextureWrap.Enabled = DataContext.ShowWrappingEditorCommand?.CanExecute(null) ?? false;

            /*if (DataContext.ManualVertexEditor != null)
            {
                ButtonSpriteCornerOffsetApply.Enabled = DataContext.ManualVertexEditor.ApplyCommand?.CanExecute(null) ?? false;
            }

            if (DataContext.ManualRectangleEditor == null)
            {
                return;
            }

            ButtonPickMaskColor.Enabled = DataContext.ShowSpritePickMaskEditorCommand?.CanExecute(null) ?? false;

            ButtonSpriteClipApply.Enabled = DataContext.ManualRectangleEditor.ApplyCommand?.CanExecute(null) ?? false;
            ButtonSpriteClipCancel.Enabled = DataContext.ManualRectangleEditor.CancelCommand?.CanExecute(null) ?? false;
            ButtonSpritePickApply.Enabled = DataContext.ManualRectangleEditor.ApplyCommand?.CanExecute(null) ?? false;
            ButtonSpritePickCancel.Enabled = DataContext.ManualRectangleEditor.CancelCommand?.CanExecute(null) ?? false;*/

#warning There's some kind of bug in the krypton ribbon regarding the enabled state and numerics. 
            /*LabelFixedHeight.Enabled = 
				LabelFixedWidth.Enabled = 
				NumericFixedWidth.Enabled = 
				NumericFixedHeight.Enabled = 
				DataContext.ManualRectangleEditor.SetFixedWidthHeightCommand?.CanExecute(new DX.Size2F((float)NumericFixedWidth.Value, (float)NumericFixedHeight.Value)) ?? false;*/
            //ButtonSpriteClipFullSize.Enabled = DataContext.ManualRectangleEditor.SetFullSizeCommand?.CanExecute(null) ?? false;
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
