#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: March 1, 2020 8:12:59 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DX = SharpDX;
using Gorgon.Renderers;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Graphics;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The service used to generate the effects to apply to the image currently being edited.
    /// </summary>
    internal class FxService
        : IFxService, IFxPreviewer
    {
        #region Variables.
        // The graphics context for the application.
        private readonly IGraphicsContext _graphics;
        // The blur effect shader.
        private Gorgon2DGaussBlurEffect _blur;
        // The gray scale effect.
        private Gorgon2DGrayScaleEffect _grayScale;
        // The sharpen/emboss effect.
        private Gorgon2DSharpenEmbossEffect _sharpEmboss;
        // The invert effect.
        private Gorgon2DInvertEffect _invert;
        // The target used to render the effect into.
        private GorgonRenderTarget2DView _effectTargetPing;
        private GorgonRenderTarget2DView _effectTargetPong;
        // The texture for rendering the render target into the swap chain.
        private GorgonTexture2DView _effectTexturePing;
        private GorgonTexture2DView _effectTexturePong;
        private GorgonTexture2DView _texture;
        // The working image that our textures are based on.
        private IGorgonImage _workingImage;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the image that will contain the effect output.
        /// </summary>
        public IGorgonImage EffectImage => _workingImage;

        /// <summary>
        /// Property to return the texture holding the unalterd image.
        /// </summary>
        public GorgonTexture2DView OriginalTexture => _texture;

        /// <summary>
        /// Property to return the texture that contains the blurred preview image.
        /// </summary>
        public GorgonTexture2DView PreviewTexture
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to destroy the texture data.
        /// </summary>
        private void DestroyTexture()
        {
            GorgonRenderTarget2DView rtv = Interlocked.Exchange(ref _effectTargetPing, null);
            rtv?.Dispose();
            rtv = Interlocked.Exchange(ref _effectTargetPong, null);
            rtv?.Dispose();

            GorgonTexture2DView tv = Interlocked.Exchange(ref _effectTexturePing, null);
            tv?.Dispose();
            tv = Interlocked.Exchange(ref _effectTexturePong, null);
            tv?.Dispose();
            tv = Interlocked.Exchange(ref _texture, null);
            tv?.Dispose();

            PreviewTexture = null;
        }

        /// <summary>
        /// Function to create the texture.
        /// </summary>
        private void CreateTexture()
        {
            _texture = GorgonTexture2DView.CreateTexture(_graphics.Graphics, new GorgonTexture2DInfo
            {
                Width = _workingImage.Width,
                Height = _workingImage.Height,
                Format = _workingImage.Format,
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Default
            }, _workingImage);

            _effectTargetPing = GorgonRenderTarget2DView.CreateRenderTarget(_graphics.Graphics, new GorgonTexture2DInfo(_texture, "Ping Target")
            {
                ArrayCount = 1,
                MipLevels = 1,
                Usage = ResourceUsage.Default,
                IsCubeMap = false
            });
            _effectTargetPong = GorgonRenderTarget2DView.CreateRenderTarget(_graphics.Graphics, new GorgonTexture2DInfo(_texture, "Pong Target")
            {
                ArrayCount = 1,
                MipLevels = 1,
                Usage = ResourceUsage.Default,
                IsCubeMap = false
            });
            _effectTexturePing = _effectTargetPing.GetShaderResourceView();
            _effectTexturePong = _effectTargetPong.GetShaderResourceView();

            PreviewTexture = _texture;

            int size = ((_texture.Width > _texture.Height) ? _texture.Width : _texture.Height);
            _blur.BlurTargetFormat = _texture.Format;
            _blur.BlurRenderTargetsSize = new DX.Size2(size, size);
        }

        /// <summary>Function to apply the grayscale effect.</summary>
        public void ApplyGrayScale()
        {
            GorgonRenderTargetView originalRtv = _graphics.Graphics.RenderTargets[0];

            // Render the blurring effect by ping-ponging between the render targets to generate the image.
            _grayScale.Render(_texture, _effectTargetPing);

            using (IGorgonImage image = _effectTexturePing.Texture.ToImage())
            {
                image.Buffers[0].CopyTo(_workingImage.Buffers[0]);
            }

            _graphics.Graphics.SetRenderTarget(originalRtv);

            // Recreate the texture so we can see the result.
            CreateTexture();
        }

        /// <summary>
        /// Function to apply the invert effect.
        /// </summary>
        public void ApplyInvert()
        {
            GorgonRenderTargetView originalRtv = _graphics.Graphics.RenderTargets[0];

            // Render the blurring effect by ping-ponging between the render targets to generate the image.
            _invert.Render(_texture, _effectTargetPing);

            using (IGorgonImage image = _effectTexturePing.Texture.ToImage())
            {
                image.Buffers[0].CopyTo(_workingImage.Buffers[0]);
            }

            _graphics.Graphics.SetRenderTarget(originalRtv);

            // Recreate the texture so we can see the result.
            CreateTexture();
        }

        /// <summary>Function to apply the current effect that is using a preview.</summary>
        public void ApplyPreviewedEffect()
        {
            if (PreviewTexture == null)
            {
                return;
            }

            using (IGorgonImage image = PreviewTexture.Texture.ToImage())
            {
                image.Buffers[0].CopyTo(_workingImage.Buffers[0]);
            }

            // Recreate the texture so we can see the result.
            CreateTexture();
        }

        /// <summary>
        /// Function to generate a blurred image preview.
        /// </summary>
        /// <param name="blurAmount">The amount to blur.</param>
        public void GenerateBlurPreview(int blurAmount)
        {
            // Reset to the original texture.
            PreviewTexture = _texture;
            GorgonRenderTarget2DView outputTarget = _effectTargetPing;

            GorgonRenderTargetView originalRtv = _graphics.Graphics.RenderTargets[0];

            // Render the blurring effect by ping-ponging between the render targets to generate the image.
            for (int i = 0; i < blurAmount; ++i)
            {
                float normalized = (i + 1) * 0.005f;
                float powScale = 0.45f.Pow(1 + normalized * 0.2f).Max(0.05f);
                float pow = normalized.Pow(powScale) * 4;

                _blur.BlurRadius = (int)pow.FastCeiling();

                outputTarget.Clear(GorgonColor.Transparent);

                _blur.Render(PreviewTexture, outputTarget);

                outputTarget = outputTarget == _effectTargetPing ? _effectTargetPong : _effectTargetPing;
                PreviewTexture = outputTarget == _effectTargetPong ? _effectTexturePing : _effectTexturePong;
            }

            _graphics.Graphics.SetRenderTarget(originalRtv);
        }

        /// <summary>
        /// Function to generate a sharpen image preview.
        /// </summary>
        /// <param name="sharpenAmount">The amount to sharpen.</param>
        public void SharpenPreview(int sharpenAmount)
        {
            // Reset to the original texture.
            GorgonRenderTargetView originalRtv = _graphics.Graphics.RenderTargets[0];

            // Render the blurring effect by ping-ponging between the render targets to generate the image.
            _sharpEmboss.UseEmbossing = false;
            _sharpEmboss.Amount = sharpenAmount / 100.0f;
            _sharpEmboss.Render(_texture, _effectTargetPing);
            PreviewTexture = _effectTexturePing;

            _graphics.Graphics.SetRenderTarget(originalRtv);
        }

        /// <summary>
        /// Function to assign an image for editing.
        /// </summary>
        /// <param name="image">The image to edit.</param>        
        /// <param name="arrayDepth">The selected array index or depth slice (volume textures).</param>
        /// <param name="mipLevel">The currently selected mip map level.</param>
        public void SetImage(IGorgonImage image, int arrayDepth, int mipLevel)
        {
            IGorgonImage oldImage = Interlocked.Exchange(ref _workingImage, null);
            oldImage?.Dispose();

            // Destroy any previous texture.
            DestroyTexture();

            if (image == null)
            {
                return;
            }

            // If we only have 1 sub resource in the image, then just clone it.
            if ((image.MipCount == 1) && (image.ArrayCount == 1) && (image.Depth == 1))
            {
                _workingImage = image.Clone();
            }
            else
            {
                // Strip out the other sub resources and work with the one we've got selected.
                IGorgonImageBuffer srcBuffer = image.Buffers[mipLevel, arrayDepth];
                _workingImage = new GorgonImage(new GorgonImageInfo(image)
                {
                    ArrayCount = 1,
                    MipCount = 1,
                    Depth = 1,
                    Width = srcBuffer.Width,
                    Height = srcBuffer.Height
                });
                srcBuffer.CopyTo(_workingImage.Buffers[0]);
            }

            _sharpEmboss.TextureSize = new DX.Size2F(_workingImage.Width, _workingImage.Height);            

            CreateTexture();
        }

        /// <summary>
        /// Function to load the resources required for the service.
        /// </summary>
        public void LoadResources()
        {
            _grayScale = new Gorgon2DGrayScaleEffect(_graphics.Renderer2D);
            _invert = new Gorgon2DInvertEffect(_graphics.Renderer2D);
            _sharpEmboss = new Gorgon2DSharpenEmbossEffect(_graphics.Renderer2D);
            _blur = new Gorgon2DGaussBlurEffect(_graphics.Renderer2D, 9)
            {
                BlurRadius = 1
            };
            _blur.Precache();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            SetImage(null, 0, 0);

            Gorgon2DGrayScaleEffect grayScale = Interlocked.Exchange(ref _grayScale, null);
            Gorgon2DInvertEffect invert = Interlocked.Exchange(ref _invert, null);
            Gorgon2DSharpenEmbossEffect sharp = Interlocked.Exchange(ref _sharpEmboss, null);
            Gorgon2DGaussBlurEffect blur = Interlocked.Exchange(ref _blur, null);
            grayScale?.Dispose();
            sharp?.Dispose();
            invert?.Dispose();
            blur?.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FxService"/> class.</summary>
        /// <param name="graphics">The graphics context for the application.</param>
        public FxService(IGraphicsContext graphics) => _graphics = graphics;
        #endregion
    }
}
