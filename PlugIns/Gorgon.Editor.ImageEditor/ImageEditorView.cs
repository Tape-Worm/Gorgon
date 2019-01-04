#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: October 30, 2018 8:12:24 PM
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
using DX = SharpDX;
using Gorgon.Editor.UI.Views;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using Gorgon.Editor.Rendering;
using System.Diagnostics;
using Gorgon.Math;
using Gorgon.Editor.UI;
using Gorgon.Editor.ImageEditor.ViewModels;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The main view for the image editor.
    /// </summary>
    internal partial class ImageEditorView 
        : ContentBaseControl, IDataContext<IImageContent>
    {
        #region Variables.        
        // The form for the ribbon.
        private FormRibbon _ribbonForm;
        // The background texture.
        private GorgonTexture2DView _background;
        // The 2D texture data.
        private GorgonTexture2D _texture2D;
        // The 2D texture data.
        private GorgonTexture3D _texture3D;
        // The loaded texture as a 2D texture view.
        private GorgonTexture2DView _texture2DView;
        // The loaded texture as a 3D texture view.
        private GorgonTexture3DView _texture3DView;
        // The current zoom level.
        private ZoomLevels _zoomLevel = ZoomLevels.ToWindow;
        // The boundaries for the texture.
        private DX.RectangleF _textureBounds;
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        /// <value>The data context.</value>
        public IImageContent DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the DragDrop event of the ImageEditorView control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ImageEditorView_DragDrop(object sender, DragEventArgs e) => OnBubbleDragDrop(e);

        /// <summary>Handles the DragEnter event of the ImageEditorView control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ImageEditorView_DragEnter(object sender, DragEventArgs e) => OnBubbleDragEnter(e);

        /// <summary>Function called when a property is changed on the data context.</summary>
        /// <param name="e">The event parameters.</param>
        /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e) 
        {
            switch (e.PropertyName)
            {
                case nameof(IImageContent.ContentState):
                case nameof(IImageContent.File):
                    // This only matters when we save, so there's no real visual update.
                    return;
                default:
                    UpdateTexture2D(GraphicsContext?.Graphics);
                    break;
            }            
        }
               
        /// <summary>Handles the ValueChanged event of the ScrollVertical control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void ScrollVertical_ValueChanged(object sender, EventArgs e) => Idle();

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

            MenuZoomItems.Text = item.Text;
            _zoomLevel = zoom;
            UpdateZoomMenu();

            if (item != ItemZoomToWindow)
            {
                return;
            }

            ScrollHorizontal.Value = 0;
            ScrollVertical.Value = 0;
        }

        /// <summary>
        /// Function to retrieve the value used to multiply by when zooming.
        /// </summary>
        /// <returns>The zoom value as a normalized value.</returns>
        private float GetZoomValue()
        {
            switch (_zoomLevel)
            {
                case ZoomLevels.Percent12:
                    return 0.125f;                    
                case ZoomLevels.Percent25:
                    return 0.25f;                    
                case ZoomLevels.Percent50:
                    return 0.5f;
                case ZoomLevels.Percent200:
                    return 2.0f;
                case ZoomLevels.Percent400:
                    return 4.0f;                    
                case ZoomLevels.Percent800:
                    return 8.0f;                    
                case ZoomLevels.Percent1600:
                    return 16.0f;
                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// Function to update the zoom item menu to reflect the current selection.
        /// </summary>
        private void UpdateZoomMenu()
        {
            ToolStripMenuItem currentItem;

            switch (_zoomLevel)
            {
                case ZoomLevels.Percent12:
                    currentItem = Item12Percent;
                    break;
                case ZoomLevels.Percent25:
                    currentItem = Item25Percent;
                    break;
                case ZoomLevels.Percent50:
                    currentItem = Item50Percent;
                    break;
                case ZoomLevels.Percent100:
                    currentItem = Item100Percent;
                    break;
                case ZoomLevels.Percent200:
                    currentItem = Item200Percent;
                    break;
                case ZoomLevels.Percent400:
                    currentItem = Item400Percent;
                    break;
                case ZoomLevels.Percent800:
                    currentItem = Item800Percent;
                    break;
                case ZoomLevels.Percent1600:
                    currentItem = Item1600Percent;
                    break;
                default:
                    currentItem = ItemZoomToWindow;
                    break;
            }

            foreach (ToolStripMenuItem item in MenuZoom.Items.OfType<ToolStripMenuItem>().Where(item => item != currentItem))
            {
                item.Checked = false;
            }
        }

        /// <summary>
        /// Function to reset the view to its original state.
        /// </summary>
        protected override void ResetDataContext()
        {
            base.ResetDataContext();
            // Reset the zoom menu.
            _zoomLevel = ZoomLevels.ToWindow;
            UpdateZoomMenu();
            ItemZoomToWindow.Checked = true;
        }

        /// <summary>
        /// Function to initialize the view from the data context.
        /// </summary>
        /// <param name="dataContext">The data context to use.</param>
        private void InitializeFromDataContext(IImageContent dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }
                        
            UpdateZoomMenu();
        }

        /// <summary>Raises the <a href="http://msdn.microsoft.com/en-us/library/system.windows.forms.control.paint.aspx" target="_blank">Paint</a> event.</summary>
        /// <param name="e">A <a href="http://msdn.microsoft.com/en-us/library/system.windows.forms.painteventargs.aspx" target="_blank">PaintEventArgs</a> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (IsDesignTime)
            {
                return;
            }

            Idle();
        }

        /// <summary>
        /// Function to scale the image to the window.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>A new rectangle containing the size and location of the scaled image.</returns>
        private void ScaleImage(int width, int height, float scale)
        {
            var textureSize = new DX.Size2F(width, height);
            var location = new DX.Vector2(PanelImage.ClientSize.Width / 2.0f, PanelImage.ClientSize.Height / 2.0f);

            textureSize.Width = textureSize.Width * scale;
            textureSize.Height = textureSize.Height * scale;

            var delta = new DX.Vector2((textureSize.Width - PanelImage.ClientSize.Width) / 2.0f, (textureSize.Height - PanelImage.ClientSize.Height) / 2.0f);

            if (delta.X > 0)
            {
                ScrollHorizontal.Enabled = true;
                ScrollHorizontal.LargeChange = (int)delta.X.Min(100).Max(10);
                ScrollHorizontal.SmallChange = (ScrollHorizontal.LargeChange / 4).Max(1);
                ScrollHorizontal.Maximum = ((int)delta.X / 2) + ScrollHorizontal.LargeChange;
                ScrollHorizontal.Minimum = (int)delta.X / -2;
            }
            else
            {
                ScrollHorizontal.Enabled = false;
                ScrollHorizontal.Value = 0;
            }

            if (delta.Y > 0)
            {
                ScrollVertical.Enabled = true;
                ScrollVertical.LargeChange = (int)delta.Y.Min(100).Max(10);
                ScrollVertical.SmallChange = (ScrollVertical.LargeChange / 4).Max(1);
                ScrollVertical.Maximum = ((int)delta.Y / 2) + ScrollVertical.LargeChange;
                ScrollVertical.Minimum = (int)delta.Y / -2;
            }
            else
            {
                ScrollVertical.Enabled = false;
                ScrollVertical.Value = 0;
            }

            location.X = (location.X - textureSize.Width / 2.0f) - ((ScrollHorizontal.Value / delta.X) * textureSize.Width);
            location.Y = (location.Y - textureSize.Height / 2.0f) - ((ScrollVertical.Value / delta.Y) * textureSize.Height);

            _textureBounds = new DX.RectangleF((int)location.X, (int)location.Y, (int)textureSize.Width, (int)textureSize.Height);
        }

        /// <summary>Raises the <a href="http://msdn.microsoft.com/en-us/library/system.windows.forms.control.mousewheel.aspx" target="_blank">MouseWheel</a> event.</summary>
        /// <param name="e">A <a href="http://msdn.microsoft.com/en-us/library/system.windows.forms.mouseeventargs.aspx" target="_blank">MouseEventArgs</a> that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!ScrollVertical.Enabled)
            {
                return;
            }

            ScrollVertical.Value = (ScrollVertical.Value - (e.Delta.Sign() * ScrollVertical.LargeChange)).Min(ScrollVertical.Maximum - ScrollVertical.LargeChange).Max(ScrollVertical.Minimum);            
        }

        /// <summary>
        /// Function to scale the image to the window.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>A new rectangle containing the size and location of the scaled image.</returns>
        private void ScaleImageToClientArea(int width, int height)
        {
            var windowSize = new DX.Size2F(PanelImage.ClientSize.Width, PanelImage.ClientSize.Height);
            var textureSize = new DX.Size2F(width, height);
            var location = new DX.Vector2(PanelImage.ClientSize.Width / 2.0f, PanelImage.ClientSize.Height / 2.0f);

            var scale = new DX.Vector2(windowSize.Width / textureSize.Width, windowSize.Height / textureSize.Height);

            if (scale.Y > scale.X)
            {
                scale.Y = scale.X;
            }
            else
            {
                scale.X = scale.Y;
            }

            textureSize.Width = textureSize.Width * scale.X;
            textureSize.Height = textureSize.Height * scale.Y;

            location.X = (location.X - textureSize.Width / 2.0f);
            location.Y = (location.Y - textureSize.Height / 2.0f);

            _textureBounds = new DX.RectangleF((int)location.X, (int)location.Y, (int)textureSize.Width, (int)textureSize.Height);
        }

        /// <summary>
        /// Function to draw the 2D texture.
        /// </summary>
        private void Draw2DTexture()
        {                       
            GraphicsContext.Renderer2D.DrawFilledRectangle(_textureBounds, new GorgonColor(GorgonColor.Black, 0.125f));
            GraphicsContext.Renderer2D.DrawFilledRectangle(_textureBounds, GorgonColor.White, _texture2DView, new DX.RectangleF(0, 0, 1, 1), textureSampler: GorgonSamplerState.PointFiltering);
        }

        /// <summary>
        /// Function to render during idle time.
        /// </summary>
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
        private bool Idle()
        {
            GraphicsContext.Graphics.SetRenderTarget(SwapChain.RenderTargetView);

            SwapChain.RenderTargetView.Clear(GorgonColor.White);

            // Calculate the image size relative to the client area.            
            if (_zoomLevel == ZoomLevels.ToWindow)
            {
                ScaleImageToClientArea(_texture2DView.Width, _texture2DView.Height);
            }
            else
            {
                ScaleImage(_texture2DView.Width, _texture2DView.Height, GetZoomValue());
            }

            // Draw the checkboard background.
            GraphicsContext.Renderer2D.Begin();

            GraphicsContext.Renderer2D.DrawFilledRectangle(new DX.RectangleF(0, 0, PanelImage.ClientSize.Width, PanelImage.ClientSize.Height), 
                GorgonColor.White, 
                _background, 
                _background.ToTexel(new DX.Rectangle(0, 0, PanelImage.ClientSize.Width, PanelImage.ClientSize.Height)),
                textureSampler: GorgonSamplerState.Wrapping);

            // Draw our texture.
            Draw2DTexture();
            GraphicsContext.Renderer2D.End();

            SwapChain.Present();
            return true;
        }

        /// <summary>
        /// Function to update the image texture for display.
        /// </summary>
        /// <param name="graphics">The graphics interface to use for generating the texture.</param>
        private void UpdateTexture2D(GorgonGraphics graphics)
        {
            _texture2D?.Dispose();

            IGorgonImage image = DataContext?.GetImage();

            if ((image == null) || (graphics == null))
            {
                return;
            }

            _texture2D = image.ToTexture2D(graphics, new GorgonTexture2DLoadOptions
            {
                Name = DataContext.File.Path,
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Immutable,
                IsTextureCube = false
            });

            _texture2DView = _texture2D.GetShaderResourceView(arrayIndex: 0, arrayCount: image.ArrayCount);
        }

        /// <summary>Function to allow user defined setup of the graphics context with this control.</summary>
        /// <param name="context">The context being assigned.</param>
        /// <param name="swapChain">
        /// The swap chain assigned to the <span class="nolink">RenderControl<span class="languageSpecificText"><span class="cs">()</span><span class="cpp">()</span><span class="nu">()</span><span class="fs">()</span></span></span>.
        /// </param>
        protected override void OnSetupGraphics(IGraphicsContext context, GorgonSwapChain swapChain)
        {
            Debug.Assert(EditorCommonResources.CheckerBoardPatternImage != null, "Background texture was not loaded.");

            _ribbonForm.GraphicsContext = context;

            _background = GorgonTexture2DView.CreateTexture(context.Graphics, new GorgonTexture2DInfo("Editor_BG_Texture")
            {
                Format = EditorCommonResources.CheckerBoardPatternImage.Format,
                Width = EditorCommonResources.CheckerBoardPatternImage.Width,
                Height = EditorCommonResources.CheckerBoardPatternImage.Height,
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Immutable
            }, EditorCommonResources.CheckerBoardPatternImage);

            UpdateTexture2D(context.Graphics);            
        }

        /// <summary>Function called to shut down the view.</summary>
        protected override void OnShutdown()
        {
            DataContext?.OnUnload();

            // Reset the view.
            SetDataContext(null);

            _background?.Dispose();
            _texture3D?.Dispose();
            _texture2D?.Dispose();

            _background = null;
            _texture3DView = null;
            _texture2DView = null;
        }

        /// <summary>Raises the <a href="http://msdn.microsoft.com/en-us/library/system.windows.forms.usercontrol.load.aspx" target="_blank">Load</a> event.</summary>
        /// <param name="e">An <a href="http://msdn.microsoft.com/en-us/library/system.eventargs.aspx" target="_blank">EventArgs</a> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (IsDesignTime)
            {
                return;
            }

            if (!_ribbonForm.IsHandleCreated)
            {
                _ribbonForm.CreateControl();
            }

            DataContext?.OnLoad();
            IdleMethod = Idle;
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IImageContent dataContext)
        {
            base.SetDataContext(dataContext);

            InitializeFromDataContext(dataContext);

            DataContext = dataContext;
            _ribbonForm.SetDataContext(dataContext);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageEditorView class.</summary>
        public ImageEditorView()
        {
            InitializeComponent();

            _ribbonForm = new FormRibbon();
            Ribbon = _ribbonForm.RibbonImageContent;            
        }
        #endregion
    }
}
