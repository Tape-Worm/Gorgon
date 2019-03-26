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
// Created: March 14, 2019 11:33:25 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DX = SharpDX;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.Editor.Rendering;
using Gorgon.Math;
using Gorgon.Editor.Services;
using Gorgon.Graphics;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Timing;
using System.Text;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The primary view for the sprite editor.
    /// </summary>
    internal partial class SpriteEditorView
        : ContentBaseControl, IDataContext<ISpriteContent>
    {
        #region Variables.
        // The form for the ribbon.
        private FormRibbon _ribbonForm;
        // The renderer for the application.
        private ISpriteContentRenderer _renderer;
        // The current zoom setting.
        private ZoomLevels _zoomLevel = ZoomLevels.ToWindow;
        // The rectangle clipping service used to capture sprite data.
        private IRectClipperService _clipperService;
        // The picking service used to capture sprite data.
        private IPickClipperService _pickService;
        // Flag to indicate that the surface is dragging.
        private bool _isDragging;
        // The starting point of the drag operation.
        private DX.Vector2 _dragStart;
        // Manual input for the sprite clipping rectangle.
        private FormManualRectInput _manualRectInput;
        // The UI refresh timer, used to keep UI elements from updating too rapidly.
        private IGorgonTimer _uiRefreshTimer = GorgonTimerQpc.SupportsQpc() ? (IGorgonTimer)new GorgonTimerMultimedia() : new GorgonTimerQpc();
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        public ISpriteContent DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to validate the controls on the view.
        /// </summary>
        private void ValidateControls()
        {
            if (DataContext?.Texture == null)
            {
                LabelSpriteInfo.Visible = false;
                LabelArrayIndex.Visible = LabelArrayIndexDetails.Visible = ButtonNextArrayIndex.Visible = ButtonPrevArrayIndex.Visible = false;
                return;
            }

            int arrayIndex = DataContext.ArrayIndex;

            switch (DataContext.CurrentTool)
            {
                case SpriteEditTool.SpriteClip:
                case SpriteEditTool.SpritePick:
                    arrayIndex = _renderer.TextureArrayIndex;
                    break;
            }

            LabelArrayIndex.Visible = LabelArrayIndexDetails.Visible = LabelSpriteInfo.Visible = true;
            ButtonNextArrayIndex.Visible = ButtonPrevArrayIndex.Visible = DataContext.SupportsArrayChange;
            ButtonNextArrayIndex.Enabled = DataContext.SetTextureCoordinatesCommand?.CanExecute((DataContext.TextureCoordinates, arrayIndex + 1)) ?? false;
            ButtonPrevArrayIndex.Enabled = DataContext.SetTextureCoordinatesCommand?.CanExecute((DataContext.TextureCoordinates, arrayIndex - 1)) ?? false;
        }


        /// <summary>Handles the Click event of the ButtonPrevArrayIndex control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPrevArrayIndex_Click(object sender, EventArgs e)
        {
            if ((DataContext == null) || ((DataContext.CurrentTool != SpriteEditTool.SpriteClip) && (DataContext.CurrentTool != SpriteEditTool.SpritePick)))
            {
                return;
            }

            --_renderer.TextureArrayIndex;
            ValidateControls();

            UpdateArrayPanel(DataContext);
        }

        /// <summary>Handles the Click event of the ButtonNextArrayIndex control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonNextArrayIndex_Click(object sender, EventArgs e)
        {
            if ((DataContext == null) || ((DataContext.CurrentTool != SpriteEditTool.SpriteClip) && (DataContext.CurrentTool != SpriteEditTool.SpritePick)))
            {
                return;
            }

            ++_renderer.TextureArrayIndex;
            ValidateControls();

            UpdateArrayPanel(DataContext);
        }

        /// <summary>Handles the Move event of the ParentForm control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ParentForm_Move(object sender, EventArgs e) => ManualInputWindowPositioning();

        /// <summary>Handles the PropertyChanged event of the ManualInput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void ManualInput_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IManualRectInputVm.Rectangle):
                    if (_manualRectInput.Visible)
                    {
                        _clipperService.Rectangle = _manualRectInput.DataContext.Rectangle;
                        Idle();
                    }
                    break;
            }
        }

        /// <summary>Handles the RectChanged event of the ClipperService control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ClipperService_RectChanged(object sender, EventArgs e)
        {
            if ((DataContext == null) || (DataContext.ActiveSubPanel != EditorSubPanel.SpriteClipManualInput))
            {
                return;
            }

            DataContext.ManualInput.Rectangle = _clipperService.Rectangle;
        }

        /// <summary>Handles the KeyboardIconClicked event of the ClipperService control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ClipperService_KeyboardIconClicked(object sender, EventArgs e) 
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.ManualInput.Rectangle = _clipperService.Rectangle;
            DataContext.ActiveSubPanel = DataContext.ActiveSubPanel == EditorSubPanel.SpriteClipManualInput ? EditorSubPanel.None : EditorSubPanel.SpriteClipManualInput;
        }

        /// <summary>Handles the Move event of the ManualRectInput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ManualRectInput_Move(object sender, EventArgs e)
        {
            Point pos = RenderControl.PointToScreen(new Point(RenderControl.ClientSize.Width - _manualRectInput.Width / 2, 0));
            var snapRect = new Rectangle(pos.X, pos.Y, _manualRectInput.Width / 2, _manualRectInput.Height / 2);

            if (snapRect.Contains(_manualRectInput.Location))
            {
                // Disable to stop the form from being dragged any further.
                _manualRectInput.Enabled = false;
                if (DataContext != null)
                {
                    DataContext.Settings.ManualRectInputBounds = null;
                }                
                ManualInputWindowPositioning();                
                _manualRectInput.Enabled = true;
                return;
            }

            if (DataContext == null)
            {
                return;
            }

            DataContext.Settings.ManualRectInputBounds = new DX.Rectangle
            {
                Left = _manualRectInput.DesktopBounds.Left,
                Top = _manualRectInput.DesktopBounds.Top,
                Right = _manualRectInput.DesktopBounds.Right,
                Bottom = _manualRectInput.DesktopBounds.Bottom,
            };
        }

        /// <summary>Handles the ClosePanel event of the ManualInput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ManualInput_ClosePanel(object sender, FormClosingEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }


            if (e.CloseReason == CloseReason.UserClosing)
            {
                DataContext.ActiveSubPanel = EditorSubPanel.None;
                e.Cancel = true;
            }
        }

        /// <summary>Handles the DragEnter event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_DragEnter(object sender, DragEventArgs e)
        {
            IContentFileDragData dragData = GetDragDropData(e, DataContext);

            if ((dragData != null) && (e.Effect != DragDropEffects.None))
            {
                return;
            }

            if (e.Effect != DragDropEffects.None)
            {
                OnBubbleDragEnter(e);
            }
        }

        /// <summary>Handles the DragDrop event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_DragDrop(object sender, DragEventArgs e)
        {
            IContentFileDragData dragData = GetDragDropData(e, DataContext);

            if ((dragData != null) && (e.Effect != DragDropEffects.None))
            {
                DataContext.Drop(dragData);
                return;
            }

            OnBubbleDragDrop(e);
        }

        /// <summary>Handles the Click event of the ButtonCenter control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonCenter_Click(object sender, EventArgs e) => _ribbonForm.ResetZoom();

        /// <summary>Handles the ValueChanged event of the ScrollHorizontal control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ScrollEventArgs"/> instance containing the event data.</param>
        private void ScrollHorizontal_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }
            Idle();
        }


        /// <summary>Handles the PreviewKeyDown event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if ((DataContext != null) && ((DataContext.CurrentTool == SpriteEditTool.SpriteClip)
                                            || (DataContext.CurrentTool == SpriteEditTool.SpritePick)))
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        _ribbonForm.SetTool(SpriteEditTool.None);
                        e.IsInputKey = true;
                        return;
                    case Keys.Escape:
                        // Reset to the original coordinates so we don't apply anything.
                        _clipperService.Rectangle = DX.RectangleF.Empty;
                        _renderer.TextureArrayIndex = DataContext.ArrayIndex;
                        _ribbonForm.SetTool(SpriteEditTool.None);
                        e.IsInputKey = true;
                        return;
                    case Keys.OemPeriod:
                        ButtonNextArrayIndex.PerformClick();
                        break;
                    case Keys.Oemcomma:
                        ButtonPrevArrayIndex.PerformClick();
                        break;
                }

                e.IsInputKey = _clipperService.KeyDown(e.KeyCode, e.Modifiers);
            }            
        }

        /// <summary>Handles the RenderToBitmap event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_RenderToBitmap(object sender, PaintEventArgs e) => RenderSwapChainToBitmap(e.Graphics);

        /// <summary>Handles the MouseWheel event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                float scale = _renderer.ZoomScaleValue;

                if (e.Delta < 0)
                {
                    _ribbonForm.SetZoom(scale.GetPrevNearest());
                }
                else if (e.Delta > 0)
                {
                    _ribbonForm.SetZoom(scale.GetNextNearest());
                }
                return;
            }

            if (e.Delta < 0)
            {
                if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    ScrollHorizontal.Value = (ScrollHorizontal.Value - ScrollHorizontal.SmallChange).Max(ScrollHorizontal.Minimum);
                }
                else
                {
                    ScrollVertical.Value = (ScrollVertical.Value - ScrollVertical.SmallChange).Max(ScrollVertical.Minimum);
                }
            }
            else if (e.Delta > 0)
            {
                if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    ScrollHorizontal.Value = (ScrollHorizontal.Value + ScrollHorizontal.SmallChange).Min(ScrollHorizontal.Maximum - ScrollHorizontal.LargeChange);
                }
                else
                {
                    ScrollVertical.Value = (ScrollVertical.Value + ScrollVertical.SmallChange).Min(ScrollVertical.Maximum - ScrollVertical.LargeChange);
                }
            }
            
        }
        
        /// <summary>Handles the MouseMove event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }
            
            var pos = new DX.Vector2(e.X, e.Y);

            if (DataContext.CurrentTool == SpriteEditTool.SpriteClip)
            {
                _clipperService.MousePosition = pos;
                if (_clipperService.MouseMove(e.Button))
                {
                    return;
                }
            }

            if (((ModifierKeys & Keys.Control) != Keys.Control) || (!_isDragging) || (DataContext.Texture == null))
            {
                _isDragging = false;
                return;
            }

            DX.RectangleF imageSize = _renderer.GetSpriteTextureSize(DataContext);
            var scale = new DX.Vector2(imageSize.Width / DataContext.Texture.Width, imageSize.Height / DataContext.Texture.Height);            
            var scaledPos = new DX.Vector2((int)(pos.X / scale.X), (int)(pos.Y / scale.Y));

            DX.Vector2.Subtract(ref _dragStart, ref scaledPos, out DX.Vector2 dragDelta);

            float hScrollAmount = ScrollHorizontal.Value + dragDelta.X;
            float vScrollAmount = ScrollVertical.Value + dragDelta.Y;

            ScrollHorizontal.Value = (int)(hScrollAmount).Max(ScrollHorizontal.Minimum).Min(ScrollHorizontal.Maximum - ScrollHorizontal.LargeChange);
            ScrollVertical.Value = (int)(vScrollAmount).Max(ScrollVertical.Minimum).Min(ScrollVertical.Maximum - ScrollVertical.LargeChange);

            _dragStart = scaledPos;
        }

        /// <summary>Handles the MouseDown event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_MouseDown(object sender, MouseEventArgs e) 
        {
            if (DataContext == null) 
            {
                return;
            }

            var pos = new DX.Vector2(e.X, e.Y);

            if (DataContext.CurrentTool == SpriteEditTool.SpriteClip)
            {                
                _clipperService.MousePosition = pos;
                if (_clipperService.MouseDown(e.Button))
                {
                    return;
                }
            }

            if (((ModifierKeys & Keys.Control) != Keys.Control) || (DataContext.Texture == null))
            {
                return;
            }

            DX.RectangleF imageSize = _renderer.GetSpriteTextureSize(DataContext);
            var scale = new DX.Vector2(imageSize.Width / DataContext.Texture.Width, imageSize.Height / DataContext.Texture.Height);

            _isDragging = true;
            _dragStart = new DX.Vector2((int)(pos.X / scale.X), (int)(pos.Y / scale.Y));
        }

        /// <summary>Handles the MouseUp event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_MouseUp(object sender, MouseEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }
                        
            var pos = new DX.Vector2(e.X, e.Y);

            switch (DataContext.CurrentTool)
            {
                case SpriteEditTool.SpriteClip:
                    _clipperService.MousePosition = pos;
                    if (_clipperService.MouseUp(e.Button))
                    {
                        return;
                    }
                    break;
                case SpriteEditTool.SpritePick:
                    if (_pickService.MouseUp(pos, e.Button, ModifierKeys))
                    {
                        return;
                    }
                    break;
            }

            _isDragging = false;
        }


        /// <summary>Handles the ImageZoomed event of the RibbonForm control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ZoomEventArgs"/> instance containing the event data.</param>
        private void RibbonForm_ImageZoomed(object sender, ZoomEventArgs e) => _zoomLevel = e.ZoomLevel;        

        /// <summary>
        /// Function to handle idle time and rendering.
        /// </summary>
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
        private bool Idle()
        {
            DX.RectangleF imageSize = _renderer.GetSpriteTextureSize(DataContext);

            if ((!_renderer.IsAnimating) && (DataContext?.Texture != null))
            {
                UpdateScrollBars(imageSize);

                var textureSize = new DX.Size2F(DataContext.Texture.Width / 2.0f, DataContext.Texture.Height / 2.0f);
                _renderer.ScrollOffset = new DX.Vector2(ScrollHorizontal.Value / textureSize.Width, ScrollVertical.Value / textureSize.Height);
            }
            else
            {
                ScrollHorizontal.Enabled = false;
                ScrollVertical.Enabled = false;
            }

            // Update every 2nd frame (if we're frame limited on the presentation).
            if ((_uiRefreshTimer.Milliseconds > 120)
                 && ((DataContext.CurrentTool == SpriteEditTool.SpritePick) || (DataContext.CurrentTool == SpriteEditTool.SpriteClip)))
            {
                UpdateSpriteDimensionsPanel(DataContext);
                _uiRefreshTimer.Reset();
            }           

            _renderer.Render(_zoomLevel, DataContext.CurrentTool);

            return true;
        }

        /// <summary>
        /// Function to update the scroll bars on the view based on the amount of space used by the image when zoomed.
        /// </summary>
        /// <param name="region">The region for the sprite.</param>
        private void UpdateScrollBars(DX.RectangleF region)
        {
            var clientSize = new DX.RectangleF(0, 0, RenderControl.ClientSize.Width, RenderControl.ClientSize.Height);

            if ((region.IsEmpty) || (DataContext?.Texture == null))
            {
                ScrollHorizontal.Value = 0;
                ScrollVertical.Value = 0;
                ScrollHorizontal.Enabled = false;
                ScrollVertical.Enabled = false;
                return;
            }
                        
            var delta = new DX.Vector2((region.Width - clientSize.Width).Max(0), (region.Height - clientSize.Height).Max(0));

            if (delta.X > 0)
            {
                int width = DataContext.Texture.Width / 2;

                ScrollHorizontal.Enabled = true;
                ScrollHorizontal.LargeChange = (int)(width * 0.25f).Max(1);
                ScrollHorizontal.SmallChange = (int)(width * 0.1f).Max(1);
                ScrollHorizontal.Maximum = width + ScrollHorizontal.LargeChange - 1;
                ScrollHorizontal.Minimum = -width;
            }
            else
            {
                ScrollHorizontal.Value = 0;
                ScrollHorizontal.Enabled = false;
            }

            if (delta.Y > 0)
            {
                int height = DataContext.Texture.Height / 2;

                ScrollVertical.Enabled = true;
                ScrollVertical.LargeChange = (int)(height * 0.25f).Max(1);
                ScrollVertical.SmallChange = (int)(height * 0.1f).Max(1);
                ScrollVertical.Maximum = height + ScrollVertical.LargeChange - 1;
                ScrollVertical.Minimum = -height;
            }
            else
            {
                ScrollVertical.Value = 0;
                ScrollVertical.Enabled = false;
            }
        }

        /// <summary>Function called when the view should be reset by a <b>null</b> data context.</summary>
        protected override void ResetDataContext()
        {
            if (DataContext?.ManualInput != null)
            {
                DataContext.ManualInput.PropertyChanged -= ManualInput_PropertyChanged;
            }

            base.ResetDataContext();
        }

        /// <summary>
        /// Function to initialize the view from the data context.
        /// </summary>
        /// <param name="dataContext">The data context to use.</param>
        private void InitializeFromDataContext(ISpriteContent dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            if (dataContext.ManualInput != null)
            {
                dataContext.ManualInput.PropertyChanged += ManualInput_PropertyChanged;
            }
                        
            UpdateArrayPanel(dataContext);
            UpdateSpriteDimensionsPanel(dataContext);
        }

        /// <summary>
        /// Function to notify when the sprite picking is finished.
        /// </summary>
        private void SpritePickFinished()
        {
            if ((DataContext?.SetTextureCoordinatesCommand == null) || (!DataContext.SetTextureCoordinatesCommand.CanExecute((_pickService.Rectangle, _renderer.TextureArrayIndex))))
            {
                return;
            }

            DataContext.SetTextureCoordinatesCommand.Execute((_pickService.Rectangle, _renderer.TextureArrayIndex));
        }

        /// <summary>
        /// Function to notify when the sprite clipping is finished.
        /// </summary>
        private void SpriteClipFinished()
        {
            if ((DataContext?.SetTextureCoordinatesCommand == null) || (!DataContext.SetTextureCoordinatesCommand.CanExecute((_clipperService.Rectangle, _renderer.TextureArrayIndex))))
            {
                return;
            }

            DataContext.SetTextureCoordinatesCommand.Execute((_clipperService.Rectangle, _renderer.TextureArrayIndex));
            ValidateControls();
        }

        /// <summary>
        /// Function to update the sprite dimensions control.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void UpdateSpriteDimensionsPanel(ISpriteContent dataContext)
        {
            if (dataContext?.Texture == null)
            {
                return;
            }

            DX.Rectangle spriteDims;

            switch (dataContext.CurrentTool)
            {
                case SpriteEditTool.SpriteClip:
                    spriteDims = _clipperService.Rectangle.ToRectangle();
                    break;
                case SpriteEditTool.SpritePick:
                    spriteDims = _pickService.Rectangle.ToRectangle();
                    break;
                default:
                    spriteDims = dataContext.Texture.ToPixel(dataContext.TextureCoordinates);
                    break;
            }

            string spriteInfoText = string.Format(Resources.GORSPR_TEXT_SPRITE_INFO, spriteDims.Left, spriteDims.Top, spriteDims.Right, spriteDims.Bottom, spriteDims.Width, spriteDims.Height);

            if (string.Equals(spriteInfoText, LabelSpriteInfo.Text, StringComparison.CurrentCulture))
            {
                return;
            }

            LabelSpriteInfo.Text = spriteInfoText;
        }

        /// <summary>
        /// Function to update the panel with the array controls.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void UpdateArrayPanel(ISpriteContent dataContext)
        {
            if (dataContext == null)
            {
                return;
            }

            int arrayIndex = dataContext.ArrayIndex;

            switch (dataContext.CurrentTool)
            {
                case SpriteEditTool.SpriteClip:
                case SpriteEditTool.SpritePick:
                    arrayIndex = _renderer.TextureArrayIndex;
                    break;
            }

            LabelArrayIndexDetails.Text = $"{arrayIndex + 1}/{dataContext.Texture?.Texture.ArrayCount ?? 0}";
        }

        /// <summary>Function called when a property is changing on the data context.</summary>
        /// <param name="e">The event parameters.</param>
        /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
        protected override void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.Texture):
                case nameof(ISpriteContent.ImageData):
                    _renderer.SetSprite(null);
                    break;
                case nameof(ISpriteContent.CurrentTool):
                    RenderControl.Cursor = Cursor.Current = Cursors.Default;

                    switch (DataContext.CurrentTool)
                    {
                        case SpriteEditTool.SpriteClip:
                            SpriteClipFinished();
                            break;
                        case SpriteEditTool.SpritePick:
                            SpritePickFinished();
                            break;
                    }
                    break;
                case nameof(ISpriteContent.ActiveSubPanel):
                    switch (DataContext.ActiveSubPanel)
                    {
                        case EditorSubPanel.SpriteClipManualInput:
                            ManualRectInput_Move(this, EventArgs.Empty);
                            _manualRectInput.Hide();
                            break;
                    }
                    break;
            }
        }

        /// <summary>Function called when a property is changed on the data context.</summary>
        /// <param name="e">The event parameters.</param>
        /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.Texture):
                    _renderer.SetSprite(DataContext);
                    UpdateArrayPanel(DataContext);
                    UpdateSpriteDimensionsPanel(DataContext);
                    break;
                case nameof(ISpriteContent.ImageData):
                case nameof(ISpriteContent.CurrentTool):
                    switch (DataContext.CurrentTool)
                    {
                        case SpriteEditTool.SpriteClip:
                            _renderer.SetSprite(DataContext);
                            break;
                        case SpriteEditTool.SpritePick:                            
                            RenderControl.Cursor = Cursor.Current = Cursors.Hand;
                            _renderer.SetSprite(DataContext);
                            break;
                    }
                    UpdateArrayPanel(DataContext);
                    UpdateSpriteDimensionsPanel(DataContext);
                    break;
                case nameof(ISpriteContent.ArrayIndex):
                    _renderer.TextureArrayIndex = DataContext.ArrayIndex;
                    UpdateArrayPanel(DataContext);                    
                    break;
                case nameof(ISpriteContent.ActiveSubPanel):
                    switch (DataContext.ActiveSubPanel)
                    {
                        case EditorSubPanel.SpriteClipManualInput:
                            // TODO: Turn off other panels.
                            _manualRectInput.Show(ParentForm);
                            ManualInputWindowPositioning();
                            break;
                    }                    
                    break;
            }

            ValidateControls();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Resize"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (_manualRectInput == null)
            {
                return;
            }

            ManualInputWindowPositioning();
        }

        /// <summary>Function to allow user defined setup of the graphics context with this control.</summary>
        /// <param name="context">The context being assigned.</param>
        /// <param name="swapChain">The swap chain assigned to the <see cref="P:Gorgon.Editor.UI.Views.ContentBaseControl.RenderControl"/>.</param>
        protected override void OnSetupGraphics(IGraphicsContext context, GorgonSwapChain swapChain)
        {
            IMarchingAnts marchAnts = new MarchingAnts(context.Renderer2D);
            _clipperService = new RectClipperService(context.Renderer2D, marchAnts);
            _clipperService.KeyboardIconClicked += ClipperService_KeyboardIconClicked;
            _clipperService.RectChanged += ClipperService_RectChanged;

            _pickService = new PickClipperService
            {
                ClipMask = DataContext?.Settings?.ClipMaskType ?? ClipMask.Alpha,
                ClipMaskValue = DataContext?.Settings?.ClipMaskValue ?? new GorgonColor(1, 0, 1, 0)
            };

            _renderer = new SpriteContentRenderer(context.Graphics, swapChain, context.Renderer2D, _clipperService, _pickService, marchAnts)
            {
                BackgroundColor = RenderControl.BackColor
            };
            _renderer.Load();

            _renderer.SetSprite(DataContext);
        }

        /// <summary>Function called to shut down the view and perform any clean up required (including user defined graphics objects).</summary>
        /// <remarks>Plug in developers do not need to clean up the <see cref="P:Gorgon.Editor.UI.Views.ContentBaseControl.SwapChain"/> as it will be returned to the swap chain pool automatically.</remarks>
        protected override void OnShutdown()
        {
            if (_clipperService != null)
            {
                _clipperService.KeyboardIconClicked -= ClipperService_KeyboardIconClicked;
                _clipperService.RectChanged -= ClipperService_RectChanged;
            }

            if (DataContext?.ManualInput != null)
            {
                DataContext.ManualInput.PropertyChanged -= ManualInput_PropertyChanged;
                DataContext.ManualInput.OnUnload();
            }
            
            DataContext?.OnUnload();
            _renderer?.Dispose();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            IdleMethod = Idle;

            DataContext?.OnLoad();

            // This will allow us to reposition our pop up window -after- the parent form stops moving.  Move will just fire non-stop, and the window 
            // position will be updated constantly (which actually slows things down quite a bit).  
            ParentForm.ResizeEnd += ParentForm_Move;

            // Force keyboard focus to our render window.
            ShowFocusState(true);
            RenderControl.Select();

            ValidateControls();            
        }

        /// <summary>Handles the GotFocus event of the RenderControl control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RenderControl_GotFocus(object sender, EventArgs e) => ShowFocusState(true);

        /// <summary>Handles the LostFocus event of the RenderControl control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RenderControl_LostFocus(object sender, EventArgs e) => ShowFocusState(false);

        /// <summary>
        /// Function used to update the position of the manual input window.
        /// </summary>
        private void ManualInputWindowPositioning()
        {
            if (!_manualRectInput.Visible)
            {
                return;
            }

            _manualRectInput.Move -= ManualRectInput_Move;
            try
            {
                if (DataContext?.Settings?.ManualRectInputBounds == null)
                {
                    _manualRectInput.Location = RenderControl.PointToScreen(new Point(RenderControl.ClientSize.Width - _manualRectInput.Width, 0));
                    return;
                }

                var pt = new Point(DataContext.Settings.ManualRectInputBounds.Value.Left, DataContext.Settings.ManualRectInputBounds.Value.Top);
                if (!Screen.AllScreens.Any(item => item.WorkingArea.Contains(pt)))
                {
                    _manualRectInput.Location = RenderControl.PointToScreen(new Point(RenderControl.ClientSize.Width - _manualRectInput.Width, 0));
                    DataContext.Settings.ManualRectInputBounds = null;
                    return;
                }

                _manualRectInput.Location = pt;
            }
            finally
            {
                _manualRectInput.Move += ManualRectInput_Move;
            }
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(ISpriteContent dataContext)            
        {
            base.SetDataContext(dataContext);

            InitializeFromDataContext(dataContext);
            _ribbonForm.SetDataContext(dataContext);            
            _manualRectInput.SetDataContext(dataContext?.ManualInput);

            DataContext = dataContext;

            ValidateControls();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor._Internal.Views.SpriteEditorView"/> class.</summary>
        public SpriteEditorView()
        {
            _manualRectInput = new FormManualRectInput();

            InitializeComponent();

            _ribbonForm = new FormRibbon();
            Ribbon = _ribbonForm.RibbonSpriteContent;

            _ribbonForm.ImageZoomed += RibbonForm_ImageZoomed;

            PanelRenderWindow.MouseWheel += PanelRenderWindow_MouseWheel;

            _manualRectInput.Move += ManualRectInput_Move;
            _manualRectInput.FormClosing += ManualInput_ClosePanel;
        }
        #endregion
    }
}
