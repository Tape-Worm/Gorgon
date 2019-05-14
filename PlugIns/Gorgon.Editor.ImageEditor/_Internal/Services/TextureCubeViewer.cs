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
// Created: January 9, 2019 1:43:36 PM
// 
#endregion

using System.Windows.Forms;
using DX = SharpDX;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Renderers;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// A viewer for a 2D cube texture.
    /// </summary>
    internal class TextureCubeViewer
        : TextureViewerCommon
    {
        #region Variables.
        // The texture to display.
        private GorgonTexture2D _texture;
        // The view for the texture.
        private GorgonTexture2DView _textureView;
        // Bounds for each cube image.
        private readonly DX.RectangleF[] _cubeImageBounds = new DX.RectangleF[6];
        // The font used to print each axis.
        private GorgonFont _axisFont;
        // The selection rectangle.
        private IMarchingAnts _selectionRect;
        #endregion

        #region Methods.
        /// <summary>Function to dispose any texture resources.</summary>
        protected override void OnDestroyTexture()
        {
            _texture?.Dispose();
            _texture = null;
            _textureView = null;
        }

        /// <summary>Function to create the texture for the view.</summary>
        /// <param name="graphics">The graphics interface used to create the texture.</param>
        /// <param name="imageData">The image data to create the texture from.</param>
        /// <param name="name">The name of the texture.</param>
        protected override void OnCreateTexture(GorgonGraphics graphics, IGorgonImage imageData, string name)
        {
            _texture = imageData.ToTexture2D(graphics, new GorgonTexture2DLoadOptions
            {
                Name = name,
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Immutable,
                IsTextureCube = false
            });

            _textureView = _texture.GetShaderResourceView();
        }

        /// <summary>Function to retrieve the shader resource view for the texture pixel shader.</summary>
        /// <param name="builder">The builder used to create the shader.</param>
        /// <returns>The shader resource view.</returns>
        protected override void OnGetShaderResourceViews(Gorgon2DShaderStateBuilder<GorgonPixelShader> builder) => builder.ShaderResources(new[] { _textureView, null });

        /// <summary>Function called to create the pixel shader.</summary>
        /// <param name="graphics">The graphics interface used to create the shader.</param>
        /// <param name="shader">The source code for the shader.</param>
        /// <returns>The pixel shader for the viewer.</returns>
        protected override GorgonPixelShader OnGetPixelShader(GorgonGraphics graphics, string shader) => GorgonShaderFactory.Compile<GorgonPixelShader>(graphics, shader, "Gorgon2DTextureArrayView");

        /// <summary>
        /// Function to scale the image to the window.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>A new rectangle containing the size and location of the scaled image.</returns>
        protected override DX.RectangleF OnScaleImageToClientArea(int width, int height) => base.OnScaleImageToClientArea(width * 4, height * 3);
            

        /// <summary>Function to scale the image to the window.</summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="scale">The scaling value to apply.</param>
        /// <returns>A new rectangle containing the size and location of the scaled image.</returns>
        protected override DX.RectangleF OnScaleImage(int width, int height, float scale) => base.OnScaleImage(width * 4, height * 3, scale);

        /// <summary>Function called when a mouse button is released.</summary>
        /// <param name="position">The position of the mouse cursor.</param>
        /// <param name="buttons">The button(s) released.</param>
        /// <param name="image">The current image.</param>
        protected override void OnMouseUp(DX.Vector2 position, MouseButtons buttons, IImageContent image)
        {
            if (buttons != MouseButtons.Left)
            {
                return;
            }

            int cubeGroup = (image.CurrentArrayIndex / 6) * 6;

            for (int i = 0; i < _cubeImageBounds.Length; ++i)
            {
                if (_cubeImageBounds[i].Contains(position))
                {
                    image.CurrentArrayIndex = cubeGroup + i;
                    return;
                }
            }
        }

        /// <summary>Function to draw the texture.</summary>
        /// <param name="renderer">The renderer used to draw the texture.</param>
        /// <param name="image">The image being rendered.</param>
        /// <param name="batchState">The currently active batch render state.</param>
        protected override void OnDrawTexture(Gorgon2D renderer, IImageContent image, Gorgon2DBatchState batchState)
        {
            renderer.End();
            
            float imageWidth = TextureBounds.Width / 4;
            float imageHeight = TextureBounds.Height / 3;

            float centerCanvasX = TextureBounds.Width / 2.0f + TextureBounds.Left;
            float centerCanvasY = TextureBounds.Height / 2.0f + TextureBounds.Top;

            float imageX = centerCanvasX - imageWidth;
            float imageY = centerCanvasY - (imageHeight / 2.0f);

            var bounds = new DX.RectangleF(imageX, 
                imageY, 
                imageWidth, 
                imageHeight);

            int cubeGroup = image.CurrentArrayIndex / 6;

            _cubeImageBounds[0] = new DX.RectangleF(bounds.Left + bounds.Width, bounds.Top, bounds.Width, bounds.Height);
            _cubeImageBounds[1] = new DX.RectangleF(bounds.Left - bounds.Width, bounds.Top, bounds.Width, bounds.Height);
            _cubeImageBounds[2] = new DX.RectangleF(bounds.Left, bounds.Top - bounds.Height, bounds.Width, bounds.Height);
            _cubeImageBounds[3] = new DX.RectangleF(bounds.Left, bounds.Top + bounds.Height, bounds.Width, bounds.Height);
            _cubeImageBounds[4] = bounds;
            _cubeImageBounds[5] = new DX.RectangleF(bounds.Left + (bounds.Width * 2), bounds.Top, bounds.Width, bounds.Height);

            int selectedImage = image.CurrentArrayIndex % 6;

            renderer.Begin(batchState);

            for (int i = 0; i < 6; ++i)
            {
                renderer.DrawFilledRectangle(_cubeImageBounds[i],
                    new GorgonColor(GorgonColor.White, Alpha),
                    _textureView,
                    new DX.RectangleF(0, 0, 1, 1),
                    (cubeGroup * 6) + i,
                    textureSampler: GorgonSamplerState.PointFiltering);
            }

            renderer.End();

            renderer.Begin();

            for (int i = 0; i < _cubeImageBounds.Length; ++i)
            {
                renderer.DrawRectangle(_cubeImageBounds[i], new GorgonColor(GorgonColor.Black, 0.88f), 1);
            }

            _selectionRect.Draw(_cubeImageBounds[selectedImage]);

            var offset = new DX.Vector2(0, _axisFont.MeasureLine("+X", true).Height);

            renderer.DrawString("+X", _cubeImageBounds[0].BottomLeft - offset, _axisFont, GorgonColor.White);
            renderer.DrawString("-X", _cubeImageBounds[1].BottomLeft - offset, _axisFont, GorgonColor.White);
            renderer.DrawString("+Y", _cubeImageBounds[2].BottomLeft - offset, _axisFont, GorgonColor.White);
            renderer.DrawString("-Y", _cubeImageBounds[3].BottomLeft - offset, _axisFont, GorgonColor.White);
            renderer.DrawString("+Z", _cubeImageBounds[4].BottomLeft - offset, _axisFont, GorgonColor.White);
            renderer.DrawString("-Z", _cubeImageBounds[5].BottomLeft - offset, _axisFont, GorgonColor.White);

            renderer.End();
        }

        /// <summary>Function called during resource creation.</summary>
        /// <param name="context">The current application graphics context.</param>
        /// <param name="swapChain">The swap chain for presenting the rendered data.</param>
        protected override void OnCreateResources(IGraphicsContext context, GorgonSwapChain swapChain)
        {
            _axisFont = context.FontFactory.GetFont(new GorgonFontInfo("Segoe UI", 12, FontHeightMode.Points, "Segoe UI Bold 12pt - Axis Font")
            {
                OutlineColor1 = GorgonColor.Black,
                OutlineColor2 = GorgonColor.Black,
                OutlineSize = 3,
                FontStyle = FontStyle.Bold
            });

            _selectionRect = new MarchingAnts(context.Renderer2D);
        }

        /// <summary>Function to dispose any resources created by the implementation.</summary>
        protected override void OnDispose()
        {
            _selectionRect?.Dispose();
            _axisFont?.Dispose();
            base.OnDispose();
        }

        /// <summary>
        /// Function to retrieve the size, in pixels, if the current mip level.
        /// </summary>
        /// <param name="image">The image to retrieve mip information from.</param>
        /// <returns>The width and height of the mip level.</returns>
        public override DX.Size2 GetMipSize(IImageContent image) => image == null
                ? DX.Size2.Zero
                : new DX.Size2(_textureView.GetMipWidth(image.CurrentMipLevel), _textureView.GetMipHeight(image.CurrentMipLevel));
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="Texture2DViewer"/> class.</summary>
        /// <param name="graphicsContext">The graphics context for the application.</param>
        /// <param name="swapChain">The swap chain for presenting the content.</param>
        /// <param name="hScroll">The horizontal scroll bar.</param>
        /// <param name="vScroll">The vertical scroll bar.</param>
        public TextureCubeViewer(IGraphicsContext graphicsContext, GorgonSwapChain swapChain, ScrollBar hScroll, ScrollBar vScroll)
            : base(graphicsContext, swapChain, hScroll, vScroll)
        {
        }
        #endregion
    }
}
