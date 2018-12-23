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
// Created: December 22, 2018 8:59:01 PM
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
using Gorgon.Editor.UI;
using Gorgon.Editor.ViewModels;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics.Core;
using Gorgon.Graphics;
using Gorgon.Renderers;
using System.IO;
using Gorgon.Editor.Properties;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Graphics.Imaging;
using System.Diagnostics;
using Gorgon.Core;
using Gorgon.Math;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// The preview window for editor content.
    /// </summary>
    internal partial class ContentPreview 
        : EditorBaseControl, IDataContext<IContentPreviewVm>
    {
        #region Variables.
        // The swap chain for the window.
        private GorgonSwapChain _swapChain;
        // The renderer used to draw the preview image.
        private Gorgon2D _renderer;
        // The default texture to render if no preview is found.
        private GorgonTexture2DView _defaultTexture;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the application graphics context.
        /// </summary>
        public IGraphicsContext GraphicsContext
        {
            get;
            set;
        }

        /// <summary>Property to return the data context assigned to this view.</summary>
        [Browsable(false)]
        public IContentPreviewVm DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to clean up the resources for the preview window.
        /// </summary>
        private void CleanupResources()
        {
            _defaultTexture?.Dispose();

            if (_swapChain != null)
            {
                GraphicsContext.ReturnSwapPresenter(ref _swapChain);
            }            
        }

        /// <summary>
        /// Function to create the resources for the preview window.
        /// </summary>
        private void CreateResources()
        {
            _swapChain = GraphicsContext.LeaseSwapPresenter(PanelDisplay);
            _renderer = GraphicsContext.Renderer2D;

            using (var stream = new MemoryStream(Resources.no_thumbnail_256x256))
            {
                _defaultTexture = GorgonTexture2DView.FromStream(GraphicsContext.Graphics, stream, new GorgonCodecDds(), options: new GorgonTexture2DLoadOptions
                {
                    Name = "DefaultPreviewTexture",
                    Binding = TextureBinding.ShaderResource,
                    Usage = ResourceUsage.Immutable
                });
            }
        }        

        /// <summary>
        /// Function used to unassign events on the data context.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext == null)
            {
                return;
            }
        }

        /// <summary>
        /// Function to reset the view to its default state when the data context is reset.
        /// </summary>
        private void ResetDataContext()
        {
        }

        /// <summary>
        /// Function to initialize the view with the data context.
        /// </summary>
        /// <param name="dataContext">The data context being assigned.</param>
        private void InitializeFromDataContext(IContentPreviewVm dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }            
        }

        /// <summary>
        /// Function to render the image into the preview area.
        /// </summary>
        private void RenderImage()
        {
            if ((IsDesignTime) || (_swapChain == null))
            {
                return;
            }

            GorgonTexture2DView image = _defaultTexture;

            _swapChain.RenderTargetView.Clear(BackColor);

            GraphicsContext.Graphics.SetRenderTarget(_swapChain.RenderTargetView);

            _renderer.Begin();

            float scale = ((float)ClientSize.Width / image.Width).Min((float)ClientSize.Height / image.Height);
            float width = image.Width * scale;
            float height = image.Height * scale;
            float x = (ClientSize.Width / 2.0f) - (width / 2.0f);
            float y = (ClientSize.Height / 2.0f) - (height / 2.0f);

            _renderer.DrawFilledRectangle(new DX.RectangleF(x, y, width, height), GorgonColor.White, _defaultTexture, new DX.RectangleF(0, 0, 1, 1));
            _renderer.End();

            _swapChain.Present(1);
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Resize"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RenderImage();
        }

        /// <summary>Paints the background of the control.</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            RenderImage();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            RenderImage();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            if (IsDesignTime)
            {
                return;
            }

            DataContext?.OnLoad();
            CreateResources();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IContentPreviewVm dataContext)
        {
            UnassignEvents();

            DataContext = null;
            InitializeFromDataContext(dataContext);
            DataContext = dataContext;

            if (DataContext == null)
            {
                return;
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.Views.ContentPreview"/> class.</summary>
        public ContentPreview()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            InitializeComponent();
        }        
        #endregion
    }
}
