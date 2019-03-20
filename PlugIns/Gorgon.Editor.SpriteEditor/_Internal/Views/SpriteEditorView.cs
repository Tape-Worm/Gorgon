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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using DX = SharpDX;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.Editor.Rendering;
using Gorgon.Renderers;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Graphics;
using Gorgon.Editor.Services;
using System.Diagnostics;

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
        private IRectClipperService _clipperService = null;
        // Flag to indicate that the surface is dragging.
        private bool _isDragging;
        // The starting point of the drag operation.
        private DX.Vector2 _dragStart;
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
            if ((DataContext != null) && (DataContext.CurrentTool == SpriteEditTool.SpriteClip))
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        _ribbonForm.SetTool(SpriteEditTool.None);
                        e.IsInputKey = true;
                        return;
                    case Keys.Escape:
                        if (DataContext.Texture != null)
                        {
                            // Reset to the original coordinates so we don't apply anything.
                            _clipperService.Rectangle = DX.RectangleF.Empty;
                        }                        
                        _ribbonForm.SetTool(SpriteEditTool.None);
                        e.IsInputKey = true;
                        return;
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
            if (DataContext.CurrentTool == SpriteEditTool.SpriteClip)
            {
                _clipperService.MousePosition = pos;
                if (_clipperService.MouseUp(e.Button))
                {
                    return;
                }
            }

            _isDragging = false;
            if ((ModifierKeys & Keys.Control) != Keys.Control)
            {
                return;
            }
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
        }

        /// <summary>
        /// Function to notify when the sprite clipping is finished.
        /// </summary>
        private void SpriteClipFinished()
        {
            if ((DataContext?.SetTextureCoordinatesCommand == null) || (!DataContext.SetTextureCoordinatesCommand.CanExecute(_clipperService.Rectangle)))
            {
                return;
            }

            DataContext.SetTextureCoordinatesCommand.Execute(_clipperService.Rectangle);
        }

        /// <summary>Function called when a property is changing on the data context.</summary>
        /// <param name="e">The event parameters.</param>
        /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
        protected override void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.CurrentTool):
                    if (DataContext.CurrentTool == SpriteEditTool.SpriteClip)
                    {
                        SpriteClipFinished();
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
                    break;
                case nameof(ISpriteContent.CurrentTool):
                    if (DataContext.CurrentTool == SpriteEditTool.SpriteClip)
                    {
                        _renderer.SetSprite(DataContext);
                    }
                    break;
            }            
        }

        /// <summary>Function to allow user defined setup of the graphics context with this control.</summary>
        /// <param name="context">The context being assigned.</param>
        /// <param name="swapChain">The swap chain assigned to the <see cref="P:Gorgon.Editor.UI.Views.ContentBaseControl.RenderControl"/>.</param>
        protected override void OnSetupGraphics(IGraphicsContext context, GorgonSwapChain swapChain)
        {
            IMarchingAnts marchAnts = new MarchingAnts(context.Renderer2D);
            _clipperService = new RectClipperService(context.Renderer2D, marchAnts);

            _renderer = new SpriteContentRenderer(context.Graphics, swapChain, context.Renderer2D, _clipperService, marchAnts)
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

            // Force keyboard focus to our render window.
            ShowFocusState(true);
            RenderControl.Select();
        }

        /// <summary>Handles the GotFocus event of the RenderControl control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RenderControl_GotFocus(object sender, EventArgs e) => ShowFocusState(true);

        /// <summary>Handles the LostFocus event of the RenderControl control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RenderControl_LostFocus(object sender, EventArgs e) => ShowFocusState(false);

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(ISpriteContent dataContext)            
        {
            base.SetDataContext(dataContext);

            InitializeFromDataContext(dataContext);
            _ribbonForm.SetDataContext(dataContext);            

            DataContext = dataContext;            
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor._Internal.Views.SpriteEditorView"/> class.</summary>
        public SpriteEditorView()
        {
            InitializeComponent();

            _ribbonForm = new FormRibbon();
            Ribbon = _ribbonForm.RibbonSpriteContent;

            _ribbonForm.ImageZoomed += RibbonForm_ImageZoomed;

            PanelRenderWindow.MouseWheel += PanelRenderWindow_MouseWheel;
        }
        #endregion
    }
}
