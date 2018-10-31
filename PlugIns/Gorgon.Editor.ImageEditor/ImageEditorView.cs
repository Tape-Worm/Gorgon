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

namespace Gorgon.Editor.ImageEditor
{
    public partial class ImageEditorView 
        : ContentBaseControl
    {
        #region Variables.
        // The loaded texture.
        private GorgonTexture2DView _texture2D;        
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        public void TempSetupImageToRender(IGorgonImage image, string filePath)
        {
            _texture2D?.Dispose();
            _texture2D = GorgonTexture2DView.CreateTexture(GraphicsContext.Graphics, new GorgonTexture2DInfo(filePath)
            {
                ArrayCount = image.ArrayCount,
                Binding = TextureBinding.ShaderResource,
                Format = image.Format,
                Usage = ResourceUsage.Default,
                MipLevels = image.MipCount,
                IsCubeMap = image.ImageType == ImageType.ImageCube,
                Width = image.Width,
                Height = image.Height
            }, image);
        }

        /// <summary>
        /// Function to render during idle time.
        /// </summary>
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
        private bool Idle()
        {
            GraphicsContext.Graphics.SetRenderTarget(SwapChain.RenderTargetView);

            SwapChain.RenderTargetView.Clear(GorgonColor.White);

            GraphicsContext.Renderer2D.Begin();

            GraphicsContext.Renderer2D.DrawFilledRectangle(new DX.RectangleF(0, 0, ClientSize.Width, ClientSize.Height), GorgonColor.White, _texture2D, new DX.RectangleF(0, 0, 1, 1));

            GraphicsContext.Renderer2D.End();

            SwapChain.Present();
            return true;
        }

        /// <summary>Function called to shut down the view.</summary>
        protected override void OnShutdown()
        {
            _texture2D?.Dispose();
            _texture2D = null;
        }

        /// <summary>Raises the <a href="http://msdn.microsoft.com/en-us/library/system.windows.forms.usercontrol.load.aspx" target="_blank">Load</a> event.</summary>
        /// <param name="e">An <a href="http://msdn.microsoft.com/en-us/library/system.eventargs.aspx" target="_blank">EventArgs</a> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            IdleMethod = Idle;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageEditorView class.</summary>
        public ImageEditorView()
        {
            InitializeComponent();
        }
        #endregion
    }
}
