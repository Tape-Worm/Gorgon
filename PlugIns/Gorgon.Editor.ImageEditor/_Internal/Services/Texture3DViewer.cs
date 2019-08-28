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
using Gorgon.Editor.ImageEditor.Properties;
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
    /// A viewer for a 3D texture.
    /// </summary>
    internal class Texture3DViewer
        : TextureViewerCommon
    {
        #region Variables.
        // The texture to display.
        private GorgonTexture3D _texture;
        // The view for the texture.
        private GorgonTexture3DView _textureView;
        // The volume renderer.
        private VolumeRenderer _volRenderer;
        #endregion

        #region Methods.
        /// <summary>Function called when the render window changes size.</summary>
        /// <param name="size">The size of the window.</param>
        protected override void OnWindowResize(DX.Size2 size) => _volRenderer.ResizeRenderRegion();

        /// <summary>Function to dispose any texture resources.</summary>
        protected override void OnDestroyTexture()
        {
            _texture?.Dispose();
            _textureView?.Dispose();

            _texture = null;
            _textureView = null;
        }

        /// <summary>Function to dispose any resources created by the implementation.</summary>
        protected override void OnDispose()
        {
            _volRenderer?.Dispose();
            _volRenderer = null;
        }

        /// <summary>Function to create the texture for the view.</summary>
        /// <param name="graphics">The graphics interface used to create the texture.</param>
        /// <param name="imageData">The image data to create the texture from.</param>
        /// <param name="name">The name of the texture.</param>
        protected override void OnCreateTexture(GorgonGraphics graphics, IGorgonImage imageData, string name)
        {
            _texture = imageData.ToTexture3D(graphics, new GorgonTextureLoadOptions
            {
                Name = name,
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Immutable
            });

            _textureView = _texture.GetShaderResourceView();
            _volRenderer.AssignTexture(_textureView);
        }

        /// <summary>Function to retrieve the shader resource view for the texture pixel shader.</summary>
        /// <param name="builder">The builder used to create the shader.</param>
        /// <returns>The shader resource view.</returns>
        protected override void OnGetShaderResourceViews(Gorgon2DShaderStateBuilder<GorgonPixelShader> builder) => builder.ShaderResources(new[] { null, _textureView });

        /// <summary>Function called to create the pixel shader.</summary>
        /// <param name="graphics">The graphics interface used to create the shader.</param>
        /// <param name="shader">The source code for the shader.</param>
        /// <returns>The pixel shader for the viewer.</returns>
        protected override GorgonPixelShader OnGetPixelShader(GorgonGraphics graphics, string shader) => GorgonShaderFactory.Compile<GorgonPixelShader>(graphics, shader, "Gorgon3DTextureView");

        /// <summary>Function to draw the texture.</summary>
        /// <param name="renderer">The renderer used to draw the texture.</param>
        /// <param name="image">The image being rendered.</param>
        /// <param name="batchState">The currently active batch render state.</param>
        protected override void OnDrawTexture(Gorgon2D renderer, IImageContent image, Gorgon2DBatchState batchState)
        {
            // We can use this for 3D textures because the texture is in slot 1, and slot 0, where the 2D texture is usually located is vacant and not used by the pixel shader.
            renderer.DrawFilledRectangle(TextureBounds,
                new GorgonColor(GorgonColor.White, Alpha),
                null,
                new DX.RectangleF(0, 0, 1, 1),
                0,
                textureSampler: GorgonSamplerState.PointFiltering);

            renderer.End();

            // Draw a frame around the volume rendering area.
            DX.RectangleF volRegion = _volRenderer.VolumeRegion;
            renderer.Begin();

            DX.Size2F textArea = renderer.DefaultFont.MeasureLine(Resources.GORIMG_TEXT_3DVIEW, false);
            renderer.DrawFilledRectangle(volRegion, new GorgonColor(GorgonColor.Black, 0.5f));
            renderer.DrawFilledRectangle(new DX.RectangleF(volRegion.Left - 1, volRegion.Bottom, volRegion.Width + 2, textArea.Height + 6), GorgonColor.White);
            renderer.DrawRectangle(new DX.RectangleF(volRegion.X - 1, volRegion.Y - 1, volRegion.Width + 2, volRegion.Height + 2), GorgonColor.White);
            renderer.DrawString("3D View", new DX.Vector2((volRegion.Right + volRegion.Left) / 2.0f - (textArea.Width / 2.0f), volRegion.Bottom + 3), color: GorgonColor.Black);

            renderer.End();

            _volRenderer.Render();

            return;
        }

        /// <summary>Function called during resource creation.</summary>
        /// <param name="context">The current application graphics context.</param>
        /// <param name="swapChain">The swap chain for presenting the rendered data.</param>
        protected override void OnCreateResources(IGraphicsContext context, GorgonSwapChain swapChain)
        {
            _volRenderer = new VolumeRenderer(context.Graphics, swapChain);
            _volRenderer.CreateResources();
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
        /// <summary>Initializes a new instance of the <see cref="Texture3DViewer"/> class.</summary>
        /// <param name="graphicsContext">The graphics context for the application.</param>
        /// <param name="swapChain">The swap chain for presenting the content.</param>
        /// <param name="hScroll">The horizontal scroll bar.</param>
        /// <param name="vScroll">The vertical scroll bar.</param>
        public Texture3DViewer(IGraphicsContext graphicsContext, GorgonSwapChain swapChain, ScrollBar hScroll, ScrollBar vScroll)
            : base(graphicsContext, swapChain, hScroll, vScroll)
        {
        }
        #endregion
    }
}
