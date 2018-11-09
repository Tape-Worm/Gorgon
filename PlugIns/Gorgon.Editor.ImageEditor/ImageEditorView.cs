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

namespace Gorgon.Editor.ImageEditor
{
    public partial class ImageEditorView 
        : ContentBaseControl
    {
        #region Variables.
        // The background texture.
        private GorgonTexture2DView _background;
        // The loaded texture as a 2D texture.
        private GorgonTexture2DView _texture2D;
        // The loaded texture as a 3D texture.
        private GorgonTexture3DView _texture3D;
        // The state for the background image.
        private Gorgon2DBatchStateBuilder _batchStateBuilder = new Gorgon2DBatchStateBuilder();
        // The batch state used for drawing the background texture.
        private Gorgon2DBatchState _backgroundTextureState;
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
        /// Function to scale the image to the window.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="bounds">The boundaries for the texture rectangle.</param>
        /// <returns>A new rectangle containing the size and location of the scaled image.</returns>
        private void ScaleImage(int width, int height, out DX.RectangleF bounds)
        {
            var windowSize = new DX.Size2F(RenderPanel.ClientSize.Width, RenderPanel.ClientSize.Height);
            var textureSize = new DX.Size2F(width, height);
            var location = new DX.Vector2(RenderPanel.ClientSize.Width / 2.0f, RenderPanel.ClientSize.Height / 2.0f);

            //if (!buttonActualSize.Checked)
            {
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
            }

            location.X = (location.X - textureSize.Width / 2.0f).Max(0);// - scrollHorz.Value;
            location.Y = (location.Y - textureSize.Height / 2.0f).Max(0);// - scrollVert.Value;

            bounds = new DX.RectangleF((int)location.X, (int)location.Y, (int)textureSize.Width, (int)textureSize.Height);
        }

        /// <summary>
        /// Function to draw the 2D texture.
        /// </summary>
        private void Draw2DTexture()
        {
            // Calculate the image size relative to the client area.            
            ScaleImage(_texture2D.Width, _texture2D.Height, out DX.RectangleF bounds);

            GraphicsContext.Renderer2D.DrawFilledRectangle(bounds, GorgonColor.White, _texture2D, new DX.RectangleF(0, 0, 1, 1));
        }

        /// <summary>
        /// Function to render during idle time.
        /// </summary>
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
        private bool Idle()
        {
            GraphicsContext.Graphics.SetRenderTarget(SwapChain.RenderTargetView);

            SwapChain.RenderTargetView.Clear(GorgonColor.White);

            // Draw the checkboard background.
            GraphicsContext.Renderer2D.Begin(_backgroundTextureState);

            GraphicsContext.Renderer2D.DrawFilledRectangle(new DX.RectangleF(0, 0, RenderPanel.ClientSize.Width, RenderPanel.ClientSize.Height), 
                GorgonColor.White, 
                _background, 
                _background.ToTexel(new DX.Rectangle(0, 0, RenderPanel.ClientSize.Width, RenderPanel.ClientSize.Height)));

            GraphicsContext.Renderer2D.End();

            // Draw our texture.
            GraphicsContext.Renderer2D.Begin();            
            Draw2DTexture();
            GraphicsContext.Renderer2D.End();

            SwapChain.Present();
            return true;
        }

        /// <summary>Function to allow user defined setup of the graphics context with this control.</summary>
        /// <param name="context">The context being assigned.</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="context" /> is <b>null</b>, then applications should use this method to shut down/dispose of any created graphics items if they cannot be removed in the
        /// <see cref="ContentBaseControl.OnShutdown"/> method.
        /// </para>
        /// </remarks>
        protected override void OnSetupGraphics(IGraphicsContext context)
        {
            // We are shutting down.
            if (context == null)
            {
                return;
            }
            
            Debug.Assert(EditorCommonResources.CheckerBoardPatternImage != null, "Background texture was not loaded.");

            var pixelShaderBuilder = new Gorgon2DShaderBuilder<GorgonPixelShader>();
            var samplerStateBuilder = new GorgonSamplerStateBuilder(context.Graphics);

            _background = GorgonTexture2DView.CreateTexture(context.Graphics, new GorgonTexture2DInfo("Editor_BG_Texture")
            {
                Format = EditorCommonResources.CheckerBoardPatternImage.Format,
                Width = EditorCommonResources.CheckerBoardPatternImage.Width,
                Height = EditorCommonResources.CheckerBoardPatternImage.Height,
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Immutable
            }, EditorCommonResources.CheckerBoardPatternImage);

            pixelShaderBuilder.SamplerState(GorgonSamplerState.Wrapping);

            _batchStateBuilder.ResetTo(Gorgon2DBatchState.NoBlend);
            _batchStateBuilder.PixelShader(pixelShaderBuilder.Build());        
            
            _backgroundTextureState = _batchStateBuilder.Build();
        }

        /// <summary>Function called to shut down the view.</summary>
        protected override void OnShutdown()
        {
            _background?.Dispose();
            _texture3D?.Dispose();
            _texture2D?.Dispose();

            _background = null;
            _texture3D = null;
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
