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
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// A viewer for a 2D texture.
    /// </summary>
    internal class Texture2DViewer
        : TextureViewerCommon
    {
        #region Variables.
        // The texture to display.
        private GorgonTexture2D _texture;
        // The view for the texture.
        private GorgonTexture2DView _textureView;
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

        /// <summary>Function to draw the texture.</summary>
        /// <param name="renderer">The renderer used to draw the texture.</param>
        /// <param name="image">The image being rendered.</param>
        /// <param name="batchState">The currently active batch render state.</param>
        protected override void OnDrawTexture(Gorgon2D renderer, IImageContent image, Gorgon2DBatchState batchState) =>
            // We can use this for 3D textures because the texture is in slot 1, and slot 0, where the 2D texture is usually located is vacant and not used by the pixel shader.
            renderer.DrawFilledRectangle(TextureBounds,
                new GorgonColor(GorgonColor.White, Alpha),
                _textureView,
                new DX.RectangleF(0, 0, 1, 1),
                image.CurrentArrayIndex,
                textureSampler: GorgonSamplerState.PointFiltering);

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
        public Texture2DViewer(IGraphicsContext graphicsContext, GorgonSwapChain swapChain, ScrollBar hScroll, ScrollBar vScroll)
            : base(graphicsContext, swapChain, hScroll, vScroll)
        {

        }
        #endregion
    }
}
