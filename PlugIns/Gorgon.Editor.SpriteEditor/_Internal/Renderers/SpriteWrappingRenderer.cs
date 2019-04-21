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
// Created: April 16, 2019 8:04:41 PM
// 
#endregion

using System.Linq;
using DX = SharpDX;
using Gorgon.Math;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using System.ComponentModel;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// A renderer for editing sprite wrapping sampler states.
    /// </summary>
    internal class SpriteWrappingRenderer
		: SingleSpriteRenderer
    {
        #region Variables.
		// The render target containing the sprite.
        private GorgonRenderTarget2DView _rtv;
		// The texture for the render target.
        private GorgonTexture2DView _srv;
		// The builder for creating samplers.
        private GorgonSamplerStateBuilder _builder;
		// The current sampler state.
        private GorgonSamplerState _current;
        #endregion

        #region Properties.

        #endregion

        #region Methods.
		/// <summary>
        /// Function to build up the sprite render target view.
        /// </summary>
        private void BuildSpriteRtv()
        {
            if (SpriteContent.Texture == null)
            {
                _rtv?.Dispose();
                _rtv = null;
                return;
            }

            DX.Rectangle rect = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates);

            _rtv = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, new GorgonTexture2DInfo("WrappingRtv")
            {
                Width = rect.Width,
                Height = rect.Height,
                Format = SpriteContent.Texture.Format,
                Binding = TextureBinding.ShaderResource
            });

            _srv = _rtv.Texture.GetShaderResourceView();
        }

		/// <summary>
        /// Function to render the sprite to the render target so we can show the effects of wrapping.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This won't be representative of real world use with a texture atlas, and really only applies to sprites that use an entire image.
        /// </para>
        /// </remarks>
        private void RenderToRtv()
        {
            Graphics.SetRenderTarget(_rtv);
            _rtv.Clear(GorgonColor.BlackTransparent);

            Sprite.Position = DX.Vector2.Zero;
            Sprite.Anchor = DX.Vector2.Zero;
            Sprite.Scale = DX.Vector2.One;
            Sprite.Size = new DX.Size2F(_rtv.Width, _rtv.Height);

            Renderer.Begin();
            Renderer.DrawSprite(Sprite);
            Renderer.End();

            Graphics.SetRenderTarget(SwapChain.RenderTargetView);
        }

        /// <summary>Handles the PropertyChanged event of the WrappingEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void WrappingEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteWrappingEditor.BorderColor):
                case nameof(ISpriteWrappingEditor.HorizontalWrapping):
                case nameof(ISpriteWrappingEditor.VerticalWrapping):
                    _current = SpriteContent.WrappingEditor.GetSampler(SpriteContent.SamplerState.Filter);
                    break;
            }
        }


        /// <summary>Function called when the sprite has a property change.</summary>
        /// <param name="e">The event parameters.</param>
        protected override void OnSpriteChanged(PropertyChangedEventArgs e)
        {
            base.OnSpriteChanged(e);

            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.SamplerState):
                    _current = SpriteContent.WrappingEditor.GetSampler(SpriteContent.SamplerState.Filter);
                    SpriteContent.WrappingEditor.HorizontalWrapping = _current.WrapU;
                    SpriteContent.WrappingEditor.VerticalWrapping = _current.WrapV;
                    SpriteContent.WrappingEditor.OriginalBorderColor =
                    SpriteContent.WrappingEditor.BorderColor = _current.BorderColor;
                    break;
            }
        }

        /// <summary>Function called to render the sprite data.</summary>
        /// <returns>The presentation interval to use when rendering.</returns>
        protected override int OnRender()
        {
            var spriteRegion = SpriteContent.Texture.ToPixel(GetSpriteTextureCoordinates()).ToRectangleF();
            var imageRegion = new DX.RectangleF(0, 0, SpriteContent.Texture.Width, SpriteContent.Texture.Height);

            SwapChain.RenderTargetView.Clear(BackgroundColor);

            if (IsAnimating)
            {
                RenderSpriteTextureWithoutSprite(imageRegion, spriteRegion);
            }
            else
            {
                Graphics.SetRenderTarget(SwapChain.RenderTargetView);
            }

            RenderToRtv();

            Renderer.Begin();

            // Draw the pattern layer.
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, SwapChain.Width, SwapChain.Height),
                new GorgonColor(GorgonColor.Gray75, (0.5f - TextureAlpha.Min(0.5f).Max(0)) * 2),
                BackgroundPattern,
                new DX.RectangleF(0, 0, SwapChain.Width / BackgroundPattern.Width, SwapChain.Height / BackgroundPattern.Height));

            if (IsAnimating)
            {
                RenderRect(imageRegion, new GorgonColor(GorgonColor.White, TextureAlpha * 2), BackgroundPattern, new DX.RectangleF(0, 0, (imageRegion.Width * ZoomScaleValue) / BackgroundPattern.Width, (imageRegion.Height * ZoomScaleValue) / BackgroundPattern.Height));
            }

            RenderRect(spriteRegion, GorgonColor.White, BackgroundPattern, new DX.RectangleF(0, 0, (spriteRegion.Width * ZoomScaleValue) / BackgroundPattern.Width, (spriteRegion.Height * ZoomScaleValue) / BackgroundPattern.Height));

            if (IsAnimating)
            {
                Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, SwapChain.Width, SwapChain.Height), new GorgonColor(GorgonColor.White, TextureAlpha), ImageBufferTexture, new DX.RectangleF(0, 0, 1, 1));
            }

            // Draw the sprite layer.
            RenderRect(new DX.RectangleF(spriteRegion.Left, spriteRegion.Top, _rtv.Width, _rtv.Height), GorgonColor.White, _srv, new DX.RectangleF(-1.0f, -1.0f, 3.0f, 3.0f), sampler: _current);

            Renderer.End();

            return 1;
        }

        /// <summary>Function called to perform custom loading of resources.</summary>
        protected override void OnLoad()
        {
            base.OnLoad();

            if (_builder == null)
            {
                _builder = new GorgonSamplerStateBuilder(Graphics);
            }
			
            _current = SpriteContent.SamplerState;

            SpriteContent.WrappingEditor.HorizontalWrapping = _current.WrapU;
            SpriteContent.WrappingEditor.VerticalWrapping = _current.WrapV;
            SpriteContent.WrappingEditor.OriginalBorderColor =
            SpriteContent.WrappingEditor.BorderColor = _current.BorderColor;
            SpriteContent.WrappingEditor.PropertyChanged += WrappingEditor_PropertyChanged;

            BuildSpriteRtv();
        }

        /// <summary>Function called to perform custom unloading of resources.</summary>
        protected override void OnUnload()
        {
            SpriteContent.WrappingEditor.PropertyChanged -= WrappingEditor_PropertyChanged;

            _rtv?.Dispose();
            _rtv = null;

            base.OnUnload();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteWrappingRenderer"/> class.</summary>
        /// <param name="sprite">The sprite view model.</param>
        /// <param name="graphics">The graphics interface for the application.</param>
        /// <param name="swapChain">The swap chain for the render area.</param>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="initialZoom">The initial zoom scale value.</param>
        public SpriteWrappingRenderer(ISpriteContent sprite, GorgonGraphics graphics, GorgonSwapChain swapChain, Gorgon2D renderer, float initialZoom)
			: base(sprite, graphics, swapChain, renderer, initialZoom)
        {            
        }
        #endregion
    }
}
