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
// Created: January 9, 2019 2:49:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DX = SharpDX;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Renderers;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The common functionality for a texture viewer.
    /// </summary>
    internal abstract class TextureViewerCommon
        : ITextureViewerService
    {
        #region Value Types.
        /// <summary>
        /// Parameters to pass to the texture shader(s).
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 16, Pack = 16)]
        private struct TextureParams
        {
            /// <summary>
            /// Size of the structure, in bytes.
            /// </summary>
            public readonly static int Size = Unsafe.SizeOf<TextureParams>();

            /// <summary>
            /// Depth slice index.
            /// </summary>
            public float DepthSlice;
            /// <summary>
            /// Mip map level.
            /// </summary>
            public float MipLevel;
        }
        #endregion

        #region Variables.        
        // The graphics context to use.
        private readonly IGraphicsContext _context;
        // The swap chain for the rendering view.
        private readonly GorgonSwapChain _swapChain;
        // The horizontal scroll bar.
        private ScrollBar _hScroll;
        // The verical scroll bar.
        private ScrollBar _vScroll;
        // The pixel shader for the viewer.
        private GorgonPixelShader _pixelShader;
        // The parameters for the texture viewer shader.
        private GorgonConstantBuffer _textureParameters;
        // The current render batch state.
        private Gorgon2DBatchState _batchState;
        // The region boundaries for the texture.
        private DX.RectangleF _textureBounds;
        // Pixel shader used to render the image.
        private Gorgon2DShader<GorgonPixelShader> _batchShader;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the background texture.
        /// </summary>
        protected GorgonTexture2DView _background
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return the current zoom level.
        /// </summary>
        public ZoomLevels ZoomLevel
        {
            get;
            set;
        } = ZoomLevels.ToWindow;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the value used to multiply by when zooming.
        /// </summary>
        /// <returns>The zoom value as a normalized value.</returns>
        private float GetZoomValue()
        {
            switch (ZoomLevel)
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
        /// Function to scale the image to the window.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>A new rectangle containing the size and location of the scaled image.</returns>
        private void ScaleImageToClientArea(int width, int height)
        {
            var windowSize = new DX.Size2F(_swapChain.Width, _swapChain.Height);
            var textureSize = new DX.Size2F(width, height);
            var location = new DX.Vector2(_swapChain.Width / 2.0f, _swapChain.Height / 2.0f);

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
        /// Function to scale the image to the window.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>A new rectangle containing the size and location of the scaled image.</returns>
        private void ScaleImage(int width, int height, float scale)
        {
            var textureSize = new DX.Size2F(width, height);
            var location = new DX.Vector2(_swapChain.Width / 2.0f, _swapChain.Height / 2.0f);

            textureSize.Width = textureSize.Width * scale;
            textureSize.Height = textureSize.Height * scale;

            var delta = new DX.Vector2((textureSize.Width - _swapChain.Width) / 2.0f, (textureSize.Height - _swapChain.Height) / 2.0f);

            if (delta.X > 0)
            {
                _hScroll.Enabled = true;
                _hScroll.LargeChange = (int)delta.X.Min(100).Max(10);
                _hScroll.SmallChange = (_hScroll.LargeChange / 4).Max(1);
                _hScroll.Maximum = ((int)delta.X / 2) + _hScroll.LargeChange;
                _hScroll.Minimum = (int)delta.X / -2;
            }
            else
            {
                _hScroll.Enabled = false;
                _hScroll.Value = 0;
            }

            if (delta.Y > 0)
            {
                _vScroll.Enabled = true;
                _vScroll.LargeChange = (int)delta.Y.Min(100).Max(10);
                _vScroll.SmallChange = (_vScroll.LargeChange / 4).Max(1);
                _vScroll.Maximum = ((int)delta.Y / 2) + _vScroll.LargeChange;
                _vScroll.Minimum = (int)delta.Y / -2;
            }
            else
            {
                _vScroll.Enabled = false;
                _vScroll.Value = 0;
            }

            location.X = (location.X - textureSize.Width / 2.0f) - ((_hScroll.Value / delta.X) * textureSize.Width);
            location.Y = (location.Y - textureSize.Height / 2.0f) - ((_vScroll.Value / delta.Y) * textureSize.Height);

            _textureBounds = new DX.RectangleF((int)location.X, (int)location.Y, (int)textureSize.Width, (int)textureSize.Height);
        }

        /// <summary>
        /// Function to retrieve a new batch state 
        /// </summary>
        /// <param name="shader">The current pixel shader.</param>
        /// <returns>The new batch state.</returns>
        private Gorgon2DBatchState GetBatchState(Gorgon2DShader<GorgonPixelShader> shader)
        {
            var builder = new Gorgon2DBatchStateBuilder();

            return builder.ResetTo(_batchState)
                .PixelShader(shader)
                .Build();
        }

        /// <summary>
        /// Function to dispose any resources created by the implementation.
        /// </summary>
        protected abstract void OnDispose();

        /// <summary>
        /// Function called to create the pixel shader.
        /// </summary>
        /// <param name="graphics">The graphics interface used to create the shader.</param>
        /// <param name="shader">The source code for the shader.</param>
        /// <returns>The pixel shader for the viewer.</returns>
        protected abstract GorgonPixelShader OnGetPixelShader(GorgonGraphics graphics, string shader);

        /// <summary>
        /// Function to draw the texture.
        /// </summary>
        /// <param name="renderer">The renderer used to draw the texture.</param>
        /// <param name="textureBounds">The screen space region for the texture.</param>
        /// <param name="image">The image being rendered.</param>
        protected abstract void OnDrawTexture(Gorgon2D renderer, DX.RectangleF textureBounds, IImageContent image);

        /// <summary>
        /// Function to create the texture for the view.
        /// </summary>
        /// <param name="graphics">The graphics interface used to create the texture.</param>
        /// <param name="imageData">The image data to create the texture from.</param>
        /// <param name="name">The name of the texture.</param>
        protected abstract void OnCreateTexture(GorgonGraphics graphics, IGorgonImage imageData, string name);

        /// <summary>
        /// Function to retrieve the shader resource view for the texture pixel shader.
        /// </summary>
        /// <param name="builder">The builder used to create the shader.</param>
        /// <returns>The shader resource view.</returns>
        protected abstract void OnGetShaderResourceViews(Gorgon2DShaderBuilder<GorgonPixelShader> builder);

        /// <summary>
        /// Function to update the texture parameters for rendering.
        /// </summary>
        /// <param name="image">The image to retrieve the parameters from.</param>
        public void UpdateTextureParameters(IImageContent image)
        {
            if (image == null)
            {
                return;
            }

            var p = new TextureParams
            {
                DepthSlice = image.CurrentDepthSlice,
                MipLevel = image.CurrentMipLevel
            };

            _textureParameters.SetData(ref p);
        }

        /// <summary>
        /// Function to update the texture.
        /// </summary>
        /// <param name="image">The image to upload to the texture.</param>
        public void UpdateTexture(IImageContent image)
        {
            OnDispose();

            IGorgonImage imageData = image?.GetImage();

            if ((imageData == null) || (_context.Graphics == null))
            {
                return;
            }

            OnCreateTexture(_context.Graphics, imageData, image.File.Name);

            var shaderBuilder = new Gorgon2DShaderBuilder<GorgonPixelShader>();
            shaderBuilder
                .ResetTo(_batchShader)
                .ConstantBuffer(_textureParameters.GetView(), 1)
                .Shader(_pixelShader)
                .SamplerState(GorgonSamplerState.PointFiltering, 0)
                .SamplerState(GorgonSamplerState.PointFiltering, 1);

            OnGetShaderResourceViews(shaderBuilder);

            _batchShader = shaderBuilder.Build();
            _batchState = GetBatchState(_batchShader);

            UpdateTextureParameters(image);
        }

        /// <summary>
        /// Function to draw the texture.
        /// </summary>
        public void Draw(IImageContent image)
        {
            if ((image?.ImageType == null) || (image.ImageType == ImageType.Unknown))
            {
                return;
            }

            _context.Graphics.SetRenderTarget(_swapChain.RenderTargetView);
            _swapChain.RenderTargetView.Clear(GorgonColor.Gray25);

            // Calculate the image size relative to the client area.            
            if (ZoomLevel == ZoomLevels.ToWindow)
            {
                ScaleImageToClientArea(image.Width, image.Height);
            }
            else
            {
                ScaleImage(image.Width, image.Height, GetZoomValue());
            }

            // Draw the checkboard background.
            _context.Renderer2D.Begin();
            _context.Renderer2D.DrawFilledRectangle(_textureBounds,
                GorgonColor.White,
                _background,
                _background.ToTexel(new DX.Rectangle(0, 0, (int)_textureBounds.Width, (int)_textureBounds.Height)),
                textureSampler: GorgonSamplerState.Wrapping);
            _context.Renderer2D.End();

            // Draw our texture.
            _context.Renderer2D.Begin(_batchState);

            OnDrawTexture(_context.Renderer2D, _textureBounds, image);

            _context.Renderer2D.End();

            _swapChain.Present();
        }

        /// <summary>Function to create the resources required for the viewer.</summary>
        /// <param name="backgroundImage">The image used for display in the background.</param>
        public void CreateResources(GorgonTexture2DView backgroundImage)
        {
            _background = backgroundImage;

            _textureParameters = new GorgonConstantBuffer(_context.Graphics, new GorgonConstantBufferInfo
            {
                SizeInBytes = TextureParams.Size
            });

            _pixelShader = OnGetPixelShader(_context.Graphics, Resources.ImageViewShaders);            
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            OnDispose();

            _textureParameters?.Dispose();
            _textureParameters = null;

            _pixelShader?.Dispose();
            _pixelShader = null;
            _batchShader?.Dispose();
            _batchShader = null;
        }

        /// <summary>
        /// Function to retrieve the size, in pixels, if the current mip level.
        /// </summary>
        /// <param name="image">The image to retrieve mip information from.</param>
        /// <returns>The width and height of the mip level.</returns>
        public abstract DX.Size2 GetMipSize(IImageContent image);
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ImageEditor._Internal.Services.Texture2DViewer"/> class.</summary>
        /// <param name="graphicsContext">The graphics context for the application.</param>
        /// <param name="swapChain">The swap chain for presenting the content.</param>
        /// <param name="hScroll">The horizontal scroll bar.</param>
        /// <param name="vScroll">The vertical scroll bar.</param>
        protected TextureViewerCommon(IGraphicsContext graphicsContext, GorgonSwapChain swapChain, ScrollBar hScroll, ScrollBar vScroll)
        {
            _context = graphicsContext;
            _swapChain = swapChain;
            _hScroll = hScroll;
            _vScroll = vScroll;
        }
        #endregion
    }
}
