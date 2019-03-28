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
using System.Collections.Generic;
using Gorgon.Renderers;
using System.IO;
using Gorgon.Graphics.Imaging.Codecs;
using System.Buffers;

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
        private readonly FormRibbon _ribbonForm;
        // The previous zoom setting for restoration.
        private ZoomLevels _oldZoomLevel = ZoomLevels.ToWindow;
        // The previous scroll setting for restoration.
        private DX.Vector2 _oldScroll;
        // The rectangle clipping service used to capture sprite data.
        private IRectClipperService _clipperService;
        // The picking service used to capture sprite data.
        private IPickClipperService _pickService;
        // The anchor editing service.
        private IAnchorEditService _anchorService;
        // Marching ants renderer.
        private IMarchingAnts _marchAnts;
        // Flag to indicate that the surface is dragging.
        private bool _isDragging;
        // The starting point of the drag operation.
        private DX.Vector2 _dragStart;
        // Manual input for the sprite clipping rectangle.
        private readonly FormManualRectInput _manualRectInput;
        // The UI refresh timer, used to keep UI elements from updating too rapidly.
        private readonly IGorgonTimer _uiRefreshTimer = GorgonTimerQpc.SupportsQpc() ? (IGorgonTimer)new GorgonTimerMultimedia() : new GorgonTimerQpc();
        // The parent form that is hosting this control hierarchy.
        private Form _parentForm;
        // The views to render depending on state.
        private readonly Dictionary<string, ISpriteContentRenderer> _renderers = new Dictionary<string, ISpriteContentRenderer>(StringComparer.OrdinalIgnoreCase);
        // Flag to indicate that scrolling should be disabled.
        private bool _disableScrolling;
        // The current sprite renderer.
        private ISpriteContentRenderer _currentRenderer;
        // The texture for the anchor icon.
        private GorgonTexture2DView _anchorTexture;
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

            if (DataContext.CurrentTool != SpriteEditTool.None)
            {
                arrayIndex = _currentRenderer.TextureArrayIndex;
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
            --_currentRenderer.TextureArrayIndex;
            ValidateControls();

            UpdateArrayPanel(DataContext);
        }

        /// <summary>Handles the Click event of the ButtonNextArrayIndex control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonNextArrayIndex_Click(object sender, EventArgs e)
        {
            ++_currentRenderer.TextureArrayIndex;
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
                case nameof(IManualRectInputVm.IsActive):
                    if (DataContext.ManualInput.IsActive)
                    {
                        ManualInputWindowPositioning();
                        return;
                    }

                    ManualRectInput_ResizeEnd(this, EventArgs.Empty);
                    break;
            }
        }

        /// <summary>Handles the PropertyChanged event of the ColorEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void ColorEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ISpriteContentRenderer renderer;
            var panel = (IHostedPanelViewModel)sender;
            

            switch (e.PropertyName)
            {
                case nameof(ISpriteColorEdit.SpriteColor):
                    renderer = _renderers[panel.GetType().FullName];
                    GorgonColor[] colors = ArrayPool<GorgonColor>.Shared.Rent(4);

                    for (int i = 0; i < colors.Length; ++i)
                    {
                        colors[i] = DataContext.ColorEditor.SpriteColor;
                    }

                    // This copies the values, so returning the array to the pool should be harmless.
                    renderer.SpriteColor = colors;
                    ArrayPool<GorgonColor>.Shared.Return(colors);
                    break;
            }
        }

        /// <summary>Handles the Submit event of the SpriteAnchorEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SpriteAnchorEditor_Submit(object sender, EventArgs e)
        {
#warning Finish me.
        }

        /// <summary>Handles the ResizeEnd event of the ManualRectInput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ManualRectInput_ResizeEnd(object sender, EventArgs e)
        {            
            Point pos = RenderControl.PointToScreen(new Point(RenderControl.ClientSize.Width - _manualRectInput.Width / 2, 0));
            var snapRect = new Rectangle(pos.X, pos.Y, _manualRectInput.Width / 2, _manualRectInput.Height / 2);

            if (snapRect.Contains(new Point(_manualRectInput.DesktopBounds.Right, _manualRectInput.DesktopBounds.Top)))
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
                if ((DataContext.ToggleManualInputCommand != null) && (DataContext.ToggleManualInputCommand.CanExecute(null)))
                {
                    DataContext.ToggleManualInputCommand.Execute(null);
                }
                else
                {
                    _manualRectInput.Hide();
                }

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
        private void ButtonCenter_Click(object sender, EventArgs e)
        {
            if (DataContext?.Texture == null)
            {
                return;
            }

            ChangeZoomLevel(ZoomLevels.ToWindow);
            ScrollHorizontal.Value = ScrollVertical.Value = 0;
            UpdateRenderer(_ribbonForm.ZoomScaling, DX.Vector2.Zero);            
        }

        /// <summary>Handles the ValueChanged event of the ScrollHorizontal control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ScrollEventArgs"/> instance containing the event data.</param>
        private void ScrollHorizontal_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            UpdateRenderer(_ribbonForm.ZoomScaling, GetScrollValue());

            // Force rendering so we can have smooth scrolling.
            Idle();
        }

        /// <summary>Handles the PreviewKeyDown event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.OemPeriod:
                    if (ButtonNextArrayIndex.Enabled)
                    {
                        ButtonNextArrayIndex.PerformClick();
                    }
                    break;
                case Keys.Oemcomma:
                    if (ButtonPrevArrayIndex.Enabled)
                    {
                        ButtonPrevArrayIndex.PerformClick();
                    }
                    break;
            }

            if (DataContext.CurrentPanel == SpriteAnchorSelector)
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                    case Keys.Escape:
#warning Need to find a way to close the panel and either commit, or cancel the update.
#warning This should apply to tools as well so we can get rid of the switch above.
                        e.IsInputKey = true;
                        return;
                }

                e.IsInputKey = _anchorService.KeyDown(e.KeyCode, e.Modifiers);
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
            if (DataContext?.Texture == null)
            {
                return;
            }

            if (((ModifierKeys & Keys.Control) == Keys.Control) && ((DataContext.CurrentPanel == null) || (!DataContext.CurrentPanel.IsModal)))
            {
                if (_currentRenderer.IsAnimating)
                {
                    return;
                }

                ZoomLevels newScale = ZoomLevels.Percent100;
                DX.Vector2 mousePos = _currentRenderer.FromClient(new DX.Vector2(e.X, e.Y));
                var textureSizeHalf = new DX.Size2F(DataContext.Texture.Width * 0.5f, DataContext.Texture.Height * 0.5f);

                if (e.Delta < 0)
                {
                    newScale = _ribbonForm.ZoomScaling.GetPrevNearest();                        
                }
                else if (e.Delta > 0)
                {
                    newScale = _ribbonForm.ZoomScaling.GetNextNearest();                    
                }

                ChangeZoomLevel(newScale);

                // Set the scrolling value.                
                mousePos.X -= textureSizeHalf.Width;
                mousePos.Y -= textureSizeHalf.Height;
                mousePos.X /= textureSizeHalf.Width;
                mousePos.Y /= textureSizeHalf.Height;
                                
                UpdateRenderer(_ribbonForm.ZoomScaling, mousePos);
                return;
            }

            if (e.Delta < 0)
            {
                if (((ModifierKeys & Keys.Shift) == Keys.Shift) && (ScrollHorizontal.Enabled))
                {
                    ScrollHorizontal.Value = (ScrollHorizontal.Value - ScrollHorizontal.SmallChange).Max(ScrollHorizontal.Minimum);
                }
                else if (ScrollVertical.Enabled)
                {
                    ScrollVertical.Value = (ScrollVertical.Value - ScrollVertical.SmallChange).Max(ScrollVertical.Minimum);
                }
            }
            else if (e.Delta > 0)
            {
                if (((ModifierKeys & Keys.Shift) == Keys.Shift) && (ScrollHorizontal.Enabled))
                {
                    ScrollHorizontal.Value = (ScrollHorizontal.Value + ScrollHorizontal.SmallChange).Min(ScrollHorizontal.Maximum - ScrollHorizontal.LargeChange);
                }
                else if (ScrollVertical.Enabled)
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
            


#warning Need to update this to not rely on knowing the panel type.
#warning Should do the same with the tools.
            /*if (DataContext.ActiveSubPanel == EditorSubPanel.SpriteAnchor)
            {
                _anchorService.MousePosition = pos;
                if (_anchorService.MouseMove(e.Button))
                {
                    return;
                }
            }*/

            if (((ModifierKeys & Keys.Control) != Keys.Control) || (!_isDragging) || (DataContext.Texture == null) || ((DataContext.CurrentPanel != null) && (DataContext.CurrentPanel.IsModal)))
            {
                _isDragging = false;
                return;
            }

            var pos = new DX.Vector2(e.X, e.Y);
            var imageSize = new DX.Size2F(DataContext.Texture.Width * _currentRenderer.ZoomScaleValue, DataContext.Texture.Height * _currentRenderer.ZoomScaleValue);
            var scale = new DX.Vector2(imageSize.Width / DataContext.Texture.Width, imageSize.Height / DataContext.Texture.Height);            
            var scaledPos = new DX.Vector2((int)(pos.X / scale.X), (int)(pos.Y / scale.Y));

            DX.Vector2.Subtract(ref _dragStart, ref scaledPos, out DX.Vector2 dragDelta);

            float hScrollAmount =  ScrollHorizontal.Value + dragDelta.X;
            float vScrollAmount = ScrollVertical.Value + dragDelta.Y;

            if (ScrollHorizontal.Enabled)
            {
                ScrollHorizontal.Value = (int)(hScrollAmount).Max(ScrollHorizontal.Minimum).Min(ScrollHorizontal.Maximum - ScrollHorizontal.LargeChange);
            }

            if (ScrollVertical.Enabled)
            {
                ScrollVertical.Value = (int)(vScrollAmount).Max(ScrollVertical.Minimum).Min(ScrollVertical.Maximum - ScrollVertical.LargeChange);
            }

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

#warning Need to update this to not rely on knowing the panel type.
#warning Should do the same with the tools.

            /*if (DataContext.ActiveSubPanel == EditorSubPanel.SpriteAnchor)
            {
                _anchorService.MousePosition = pos;
                if (_anchorService.MouseDown(e.Button))
                {
                    return;
                }
            }*/

            if (((ModifierKeys & Keys.Control) != Keys.Control) || (DataContext.Texture == null) || ((DataContext.CurrentPanel != null) && (DataContext.CurrentPanel.IsModal)))
            {
                return;
            }

            var imageSize = new DX.Size2F(DataContext.Texture.Width * _currentRenderer.ZoomScaleValue, DataContext.Texture.Height * _currentRenderer.ZoomScaleValue);
            var scale = new DX.Vector2(imageSize.Width / DataContext.Texture.Width, imageSize.Height / DataContext.Texture.Height);

            _isDragging = true;
            _dragStart = new DX.Vector2((int)(pos.X / scale.X), (int)(pos.Y / scale.Y));
        }

        /// <summary>Handles the MouseUp event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_MouseUp(object sender, MouseEventArgs e)
        {
#warning Need to update this to not rely on knowing the panel type.
#warning Should do the same with the tools.

            /*           if (DataContext.ActiveSubPanel == EditorSubPanel.SpriteAnchor)
            {
                _anchorService.MousePosition = pos;
                if (_anchorService.MouseUp(e.Button))
                {
                    return;
                }
            }*/

            _isDragging = false;
        }

        /// <summary>
        /// Function to retrieve the scroll bar values as a range between -1 to 1.
        /// </summary>
        /// <returns>The scrollbar values as a range between -1 to 1.</returns>
        private DX.Vector2 GetScrollValue()
        {
            DX.Vector2 result = default;

            if (DataContext?.Texture == null)
            {
                return result;
            }

            var textureSize = new DX.Size2F(DataContext.Texture.Width, DataContext.Texture.Height);

            if (ScrollHorizontal.Enabled)
            {
                result.X = ScrollHorizontal.Value / (textureSize.Width * 0.5f);
            }

            if (!ScrollVertical.Enabled)
            {
                return result;
            }

            result.Y = ScrollVertical.Value / (textureSize.Height * 0.5f);

            return result;
        }

        /// <summary>
        /// Function to update the renderer.
        /// </summary>
        /// <param name="zoomScale">The amount of scaling to apply.</param>
        /// <param name="scrollOffset">Scrolling offset to apply.</param>
        /// <param name="targetAlpha">The target alpha for the background.</param>
        /// <param name="animate"><b>true</b> to animate the look at transition, <b>false</b> to snap to the specified values.</param>
        private void UpdateRenderer(float zoomScale, DX.Vector2 scrollOffset, float targetAlpha = 0.5f, bool animate = true)
        {
            UpdateScrollBars(zoomScale);
            _currentRenderer.LookAt(zoomScale, scrollOffset, targetAlpha, animate);
        }

        /// <summary>Handles the ImageZoomed event of the RibbonForm control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ZoomEventArgs"/> instance containing the event data.</param>
        private void RibbonForm_ImageZoomed(object sender, ZoomEventArgs e)
        {
            if (DataContext?.Texture == null)
            {
                return;
            }

            ChangeZoomLevel(e.ZoomLevel);
            UpdateRenderer(_ribbonForm.ZoomScaling, GetScrollValue());
        }

        /// <summary>
        /// Function to handle idle time and rendering.
        /// </summary>
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
        private bool Idle()
        {           
            // Update every 2nd frame (if we're frame limited on the presentation).
            if (_uiRefreshTimer.Milliseconds > 120)
            {
                UpdateSpriteDimensionsPanel(DataContext);
                _uiRefreshTimer.Reset();
            }           

            _currentRenderer?.Render();

            return true;
        }

        /// <summary>
        /// Function to update the scroll bars on the view based on the amount of space used by the image when zoomed.
        /// </summary>
        /// <param name="zoomScale">The current zoom scale.</param>
        private void UpdateScrollBars(float zoomScale)
        {
            var clientSize = new DX.RectangleF(0, 0, RenderControl.ClientSize.Width, RenderControl.ClientSize.Height);
            DX.Size2F region = DX.Size2F.Empty;

            if (DataContext?.Texture != null)
            {
                region = new DX.Size2F(DataContext.Texture.Width * zoomScale, DataContext.Texture.Height * zoomScale);
            }

            if ((region.Width.EqualsEpsilon(0)) || (region.Height.EqualsEpsilon(0)) || (DataContext?.Texture == null) || (_disableScrolling) || (_currentRenderer.IsAnimating))
            {
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
                ScrollVertical.Enabled = false;
            }
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
                        
            UpdateArrayPanel(dataContext);
            UpdateSpriteDimensionsPanel(dataContext);
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

            string spriteInfoText = _currentRenderer == null ? string.Empty : _currentRenderer.SpriteInfo;

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
                    arrayIndex = _currentRenderer.TextureArrayIndex;
                    break;
            }

            LabelArrayIndexDetails.Text = $"{arrayIndex + 1}/{dataContext.Texture?.Texture.ArrayCount ?? 0}";
        }        

        /// <summary>
        /// Function to change the current zoom level along with the associated scaling factor.
        /// </summary>
        /// <param name="zoomLevel">The current zoom level to assign.</param>
        private void ChangeZoomLevel(ZoomLevels zoomLevel)
        {
            if (DataContext?.Texture == null)
            {
                _ribbonForm.ZoomLevel = ZoomLevels.ToWindow;
                _ribbonForm.ZoomScaling = 1.0f;                
                return;
            }            

            _ribbonForm.ZoomLevel = zoomLevel;

            // Do not calculate a scaling value for custom zoom.  We need to set that ourselves.
            if (zoomLevel == ZoomLevels.Custom)
            {
                return;
            }

            _ribbonForm.ZoomScaling = zoomLevel == ZoomLevels.ToWindow ? CalcZoomToSize(new DX.Size2F(DataContext.Texture.Width, DataContext.Texture.Height)) : zoomLevel.GetScale();
        }


        /// <summary>
        /// Function to zoom to the active sprite region.
        /// </summary>
        /// <param name="disableScrollbars"><b>true</b> to disable the scroll bars, <b>false</b> to leave them active.</param>
        /// <param name="panelWidth">The width of the panel, in pixels.</param>
        private void ShiftRenderRegionForPanel(bool disableScrollbars, int panelWidth)
        {
            // Because we're setting a custom zoom level, we need to store the previous zoom states so we can restore it.
            if (_ribbonForm.ZoomLevel != ZoomLevels.Custom)
            {
                _oldScroll = _currentRenderer.ScrollOffset;
                _oldZoomLevel = _ribbonForm.ZoomLevel;
            }

            ChangeZoomLevel(ZoomLevels.Custom);
            _ribbonForm.ZoomScaling = CalcZoomToSize(new DX.Size2F(DataContext.Size.Width + (DataContext.Size.Width * 0.05f), DataContext.Size.Height + (DataContext.Size.Height * 0.05f)));

            float scaledTextureWidth = DataContext.Texture.Width * _ribbonForm.ZoomScaling;
            float spriteLeft = DataContext.TextureCoordinates.Left * scaledTextureWidth;
            float panelOffset = (RenderControl.ClientSize.Width - (PresentationPanel.ClientSize.Width - panelWidth)) * 0.5f;
            var spriteUpperLeft = new DX.Vector2((spriteLeft + panelOffset) / scaledTextureWidth, DataContext.TextureCoordinates.Top);

            var scroll = new DX.Vector2((spriteUpperLeft.X - 0.5f) * 2 + DataContext.TextureCoordinates.Width,
                                        (spriteUpperLeft.Y - 0.5f) * 2 + DataContext.TextureCoordinates.Height);

            _disableScrolling = disableScrollbars;
            UpdateRenderer(_ribbonForm.ZoomScaling, scroll, 0.0f);
        }

        /// <summary>
        /// Function to switch the current renderer.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void SwitchRenderer(ISpriteContent dataContext)
        {
            if (dataContext == null)
            {
                return;
            }

            // Utility method to assign the current renderer.
            void SetRenderer(ISpriteContentRenderer renderer, float scaling, DX.Vector2 offset, float alpha, bool animation)
            {
                _currentRenderer = renderer;
                _currentRenderer.Load();
                UpdateRenderer(scaling, offset, alpha, animation);
            }

            string panelID = dataContext.CurrentPanel?.GetType().FullName;
            DX.Vector2 scroll;
            _disableScrolling = false;

            if (_currentRenderer != null)
            {                
                _ribbonForm.ZoomScaling = _currentRenderer.ZoomScaleValue;
                scroll = _currentRenderer.ScrollOffset;

                // Release any temporary resources.
                _currentRenderer.Unload();
            }
            else
            {
                scroll = GetScrollValue();
            }

            if (dataContext.Texture == null)
            {
                SetRenderer(_renderers["null"], _ribbonForm.ZoomScaling, scroll, 1.0f, false);
                return;
            }

            if ((panelID == null) && (dataContext.CurrentTool == SpriteEditTool.None))
            {
                bool animate = false;
                float textureAlpha = _currentRenderer?.TextureAlpha ?? 0.5f;
                ISpriteContentRenderer newRenderer = _renderers["default"];                

                // If we were at a custom zoom level, then reset back to the original zoom level and scroll values.
                if (_ribbonForm.ZoomLevel == ZoomLevels.Custom)
                {
                    SetRenderer(newRenderer, _ribbonForm.ZoomScaling, scroll, textureAlpha, false);
                    ChangeZoomLevel(_oldZoomLevel);
                    scroll = _oldScroll;                    
                    animate = true;
                }

                SetRenderer(newRenderer, _ribbonForm.ZoomScaling, scroll, 0.5f, animate);
                return;
            }

            if (dataContext.CurrentTool != SpriteEditTool.None)
            {
                SetRenderer(_renderers[dataContext.CurrentTool.ToString()], _ribbonForm.ZoomScaling, scroll, 0.5f, false);
                return;
            }

            // Update the current renderer with our most current zoom scaling value, and scroll offset.
            SetRenderer(_renderers[panelID], _ribbonForm.ZoomScaling, scroll, 0.5f, false);
            Control panel = GetRegisteredPanel<Control>(panelID);

            if (panel == null)
            {
                ShiftRenderRegionForPanel(false, 0);
            }
            else
            {
                ShiftRenderRegionForPanel(dataContext.CurrentPanel.IsModal, panel.Width);
            }
        }

        /// <summary>Function called when a property is changing on the data context.</summary>
        /// <param name="e">The event parameters.</param>
        /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
        protected override void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.CurrentPanel):
                    if (DataContext.CurrentPanel != null)
                    {
                        DataContext.CurrentPanel.IsActive = false;
                    }

                    ClearPanelHost();
                    IsPanelHostActive = false;
                    break;
                case nameof(ISpriteContent.CurrentTool):
                    RenderControl.Cursor = Cursor.Current = Cursors.Default;
                    ValidateControls();
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
                case nameof(ISpriteContent.ImageData):
                    SwitchRenderer(DataContext);

                    ButtonCenter.PerformClick();                    

                    RenderControl.Cursor = _currentRenderer.CurrentCursor;

                    UpdateArrayPanel(DataContext);
                    UpdateSpriteDimensionsPanel(DataContext);
                    break;
                case nameof(ISpriteContent.CurrentTool):
                    SwitchRenderer(DataContext);
                    RenderControl.Cursor = _currentRenderer.CurrentCursor;
                    UpdateArrayPanel(DataContext);
                    UpdateSpriteDimensionsPanel(DataContext);
                    break;
                case nameof(ISpriteContent.ArrayIndex):
                    _currentRenderer.TextureArrayIndex = DataContext.ArrayIndex;
                    UpdateArrayPanel(DataContext);                    
                    break;
                case nameof(ISpriteContent.CurrentPanel):
                    if (DataContext.CurrentPanel != null)
                    {
                        Control panel = GetRegisteredPanel<Control>(DataContext.CurrentPanel.GetType().FullName);
                        IsPanelHostActive = true;
                        panel.Visible = true;
                        AddControlToPanelHost(panel);
                        DataContext.ColorEditor.IsActive = true;                        
                    }

                    SwitchRenderer(DataContext);
                    break;                
            }

            ValidateControls();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Resize"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if ((DataContext?.Texture == null) || (_currentRenderer == null))
            {
                return;
            }

            // Resize the image to fit the new boundaries.
            if (_ribbonForm.ZoomLevel == ZoomLevels.ToWindow)
            {
                ChangeZoomLevel(ZoomLevels.ToWindow);
            }
            else
            {                
                string panelID = DataContext.CurrentPanel?.GetType().FullName;

                if (!string.IsNullOrWhiteSpace(panelID))
                {
                    UpdateRenderer(_ribbonForm.ZoomScaling, GetScrollValue(), 0.5f, false);
                    Control panel = GetRegisteredPanel<Control>(panelID);

                    if (panel == null)
                    {
                        ShiftRenderRegionForPanel(false, 0);
                    }
                    else
                    {
                        ShiftRenderRegionForPanel(DataContext.CurrentPanel.IsModal, panel.Width);
                    }
                }
                else
                {
                    ChangeZoomLevel(ZoomLevels.ToWindow);
                }
            }

            UpdateRenderer(_ribbonForm.ZoomScaling, GetScrollValue());

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
            _marchAnts = new MarchingAnts(context.Renderer2D);
            _clipperService = new RectClipperService(context.Renderer2D, _marchAnts);

            _pickService = new PickClipperService
            {
                ClipMask = DataContext?.Settings?.ClipMaskType ?? ClipMask.Alpha,
                ClipMaskValue = DataContext?.Settings?.ClipMaskValue ?? new GorgonColor(1, 0, 1, 0)
            };

            using (var stream = new MemoryStream(Resources.anchor_24x24))
            {
                _anchorTexture = GorgonTexture2DView.FromStream(context.Graphics, stream, new GorgonCodecDds(),
                    options: new GorgonTexture2DLoadOptions
                    {
                        Name = "Sprite Editor Anchor Sprite",
                        Binding = TextureBinding.ShaderResource,
                        Usage = ResourceUsage.Immutable
                    });
            }

            _anchorService = new AnchorEditService(context.Renderer2D, new GorgonSprite
            {
                Texture = _anchorTexture,
                Size = new DX.Size2F(_anchorTexture.Width, _anchorTexture.Height),
                // Place the hotspot on rope hole at the top of the handle.
                Anchor = new DX.Vector2(0.4975f, 0.164167f)                
            });

            ChangeZoomLevel(ZoomLevels.ToWindow);

            _renderers["null"] = new NoTextureRenderer(context.Graphics, swapChain, context.Renderer2D);
            _renderers["default"] = new DefaultSpriteRenderer(DataContext, context.Graphics, swapChain, context.Renderer2D, _marchAnts, _ribbonForm.ZoomScaling);
            _renderers[nameof(SpriteEditTool.SpriteClip)] = new SpriteClipRenderer(DataContext, context.Graphics, swapChain, context.Renderer2D, _clipperService, _ribbonForm.ZoomScaling);
            _renderers[nameof(SpriteEditTool.SpritePick)] = new SpritePickRenderer(DataContext, context.Graphics, swapChain, context.Renderer2D, _pickService, _marchAnts, _ribbonForm.ZoomScaling);
            _renderers[typeof(SpriteColorEdit).FullName] = new SingleSpriteRenderer(DataContext, context.Graphics, swapChain, context.Renderer2D, _ribbonForm.ZoomScaling);
            _renderers[typeof(SpriteAnchorEdit).FullName] = new SpriteAnchorRenderer(DataContext, context.Graphics, swapChain, context.Renderer2D, _anchorService, _ribbonForm.ZoomScaling);

            foreach (ISpriteContentRenderer renderer in _renderers.Values)
            {
                renderer.BackgroundColor = RenderControl.BackColor;
                renderer.ScrollUpdated += Renderer_ScrollUpdated;
            }

            SwitchRenderer(DataContext);            
        }

        /// <summary>Handles the ScrollUpdated event of the Renderer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Renderer_ScrollUpdated(object sender, EventArgs e)
        {
            var renderer = (ISpriteContentRenderer)sender;

            if (DataContext?.Texture == null)
            {
                return;
            }

            ScrollHorizontal.ValueChanged -= ScrollHorizontal_ValueChanged;
            ScrollVertical.ValueChanged -= ScrollHorizontal_ValueChanged;

            try
            {
                var textureSize = new DX.Size2F(DataContext.Texture.Width * 0.5f, DataContext.Texture.Height * 0.5f);

                int offsetX = (int)(renderer.ScrollOffset.X * textureSize.Width);
                int offsetY = (int)(renderer.ScrollOffset.Y * textureSize.Height);

                if (ScrollHorizontal.Value != offsetX)
                {
                    ScrollHorizontal.Value = offsetX.Max(ScrollHorizontal.Minimum).Min(ScrollHorizontal.Maximum - ScrollHorizontal.LargeChange);
                }

                if (ScrollVertical.Value == offsetY) 
                {
                    return;
                }

                ScrollVertical.Value = offsetY.Max(ScrollVertical.Minimum).Min(ScrollVertical.Maximum - ScrollVertical.LargeChange);
            }
            finally
            {
                ScrollHorizontal.ValueChanged += ScrollHorizontal_ValueChanged;
                ScrollVertical.ValueChanged += ScrollHorizontal_ValueChanged;
            }
        }

        /// <summary>Function called to shut down the view and perform any clean up required (including user defined graphics objects).</summary>
        /// <remarks>Plug in developers do not need to clean up the <see cref="P:Gorgon.Editor.UI.Views.ContentBaseControl.SwapChain"/> as it will be returned to the swap chain pool automatically.</remarks>
        protected override void OnShutdown()
        {
            if (_clipperService != null)
            {
                _clipperService.Dispose();
            }
            
            _marchAnts?.Dispose();
            _anchorTexture?.Dispose();
            
            DataContext?.OnUnload();

            foreach (ISpriteContentRenderer renderer in _renderers.Values)
            {
                renderer.ScrollUpdated -= Renderer_ScrollUpdated;
                renderer.Dispose();
            }
        }

        /// <summary>Handles the ParentChanged event of the SpriteEditorView control.</summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnParentChanged(EventArgs e)
        {
            if (_parentForm != null)
            {
                _parentForm.ResizeEnd -= ParentForm_Move;
                _parentForm = null;
            }

            if (ParentForm != null)
            {
                // This will allow us to reposition our pop up window -after- the parent form stops moving.  Move will just fire non-stop, and the window 
                // position will be updated constantly (which actually slows things down quite a bit).  
                _parentForm = ParentForm;
                _parentForm.ResizeEnd += ParentForm_Move;
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (IsDesignTime)
            {
                return;
            }

            IdleMethod = Idle;

            DataContext?.OnLoad();

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

            _manualRectInput.ResizeEnd -= ManualRectInput_ResizeEnd;
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
                _manualRectInput.ResizeEnd += ManualRectInput_ResizeEnd;
            }
        }

        /// <summary>
        /// Function to calculate scaling to the specified size, bounded by the client area of the rendering control.
        /// </summary>
        /// <param name="size">The size of the area to zoom into.</param>
        /// <returns>The scaling factor to apply.</returns>
        private float CalcZoomToSize(DX.Size2F size)
        {
            var windowSize = new DX.Size2F(RenderControl.ClientSize.Width, RenderControl.ClientSize.Height);
            var scaling = new DX.Vector2(windowSize.Width / size.Width, windowSize.Height / size.Height);

            return scaling.X.Min(scaling.Y);
        }

        /// <summary>Function to unassign events for the data context.</summary>
        protected override void UnassignEvents()
        {
            base.UnassignEvents();

            if (DataContext?.ManualInput != null)
            {
                DataContext.ManualInput.PropertyChanged -= ManualInput_PropertyChanged;
            }

            if (DataContext?.ColorEditor == null)
            {
                return;
            }

            DataContext.ColorEditor.PropertyChanged -= ColorEditor_PropertyChanged;
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
            SpriteColorSelector.SetDataContext(dataContext?.ColorEditor);
            SpriteAnchorSelector.SetDataContext(dataContext?.AnchorEditor);

            DataContext = dataContext;

            if (IsHandleCreated)
            {
                ValidateControls();
            }

            if (DataContext?.ManualInput != null)
            {
                DataContext.ManualInput.PropertyChanged += ManualInput_PropertyChanged;
            }

            if (DataContext?.ColorEditor == null)
            {
                return;
            }

            DataContext.ColorEditor.PropertyChanged += ColorEditor_PropertyChanged;            
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor._Internal.Views.SpriteEditorView"/> class.</summary>
        public SpriteEditorView()
        {
            InitializeComponent();

            _ribbonForm = new FormRibbon();
            _ribbonForm.ImageZoomed += RibbonForm_ImageZoomed;
            Ribbon = _ribbonForm.RibbonSpriteContent;           

            PanelRenderWindow.MouseWheel += PanelRenderWindow_MouseWheel;

            _manualRectInput = new FormManualRectInput();
            _manualRectInput.ResizeEnd += ManualRectInput_ResizeEnd;
            _manualRectInput.FormClosing += ManualInput_ClosePanel;
            RegisterChildPanel(typeof(SpriteColorEdit).FullName, SpriteColorSelector);
            RegisterChildPanel(typeof(SpriteAnchorEdit).FullName, SpriteAnchorSelector);
        }        
        #endregion
    }
}
