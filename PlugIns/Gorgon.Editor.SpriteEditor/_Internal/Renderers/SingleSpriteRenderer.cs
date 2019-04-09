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
// Created: April 2, 2019 5:24:26 PM
// 
#endregion

using System.Linq;
using DX = SharpDX;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using Gorgon.Graphics;
using System.ComponentModel;
using Gorgon.Math;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// A renderer for a single sprite.
    /// </summary>
    internal class SingleSpriteRenderer
        : SpriteContentRenderer
    {
        #region Variables.
        // The actual sprite to render.
        private readonly GorgonSprite _workingSprite;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the active sprite texture sampler.
        /// </summary>
        protected GorgonSamplerState SpriteSampler
        {
            get => _workingSprite.TextureSampler;
            set => _workingSprite.TextureSampler = value;
        }

		/// <summary>
        /// Property to return the sprite used for rendering.
        /// </summary>
        protected GorgonSprite Sprite => _workingSprite;
        #endregion

        #region Methods.
		/// <summary>
        /// Function to retrieve the sprite texture coordinates, in texel space.
        /// </summary>
        /// <returns>The sprite texture coordinates.</returns>
        protected virtual DX.RectangleF GetSpriteTextureCoordinates() => SpriteContent.TextureCoordinates;

        /// <summary>Function called when the <see cref="P:Gorgon.Editor.SpriteEditor.SpriteContentRenderer.ZoomScaleValue"/> property is changed.</summary>
        protected override void OnZoomScaleChanged() => UpdateWorkingSprite();

        /// <summary>Function called when the <see cref="P:Gorgon.Editor.SpriteEditor.SpriteContentRenderer.ScrollOffset"/> property is changed.</summary>
        protected override void OnScrollOffsetChanged() => UpdateWorkingSprite();

        /// <summary>Function called when the texture array index value is updated.</summary>
        protected override void OnTextureArrayIndexChanged() => UpdateWorkingSprite();

        /// <summary>Function called when the <see cref="SpriteColor"/> property is changed.</summary>
        protected override void OnSpriteColorChanged() => UpdateWorkingSprite();

        /// <summary>
        /// Function to update the working sprite data from the sprite content.
        /// </summary>
        private void UpdateWorkingSprite()
        {
            if (_workingSprite == null)
            {
                return;
            }

            _workingSprite.Texture = SpriteContent?.Texture;
            _workingSprite.TextureArrayIndex = TextureArrayIndex;

            for (int i = 0; i < SpriteColor.Count.Min(_workingSprite.CornerColors.Count); ++i)
            {
                _workingSprite.CornerColors[i] = SpriteColor[i];
            }

            if (SpriteContent?.Texture == null)
            {
                return;
            }

			// Use the actual texture coordinates here because we need it to define the size and position of the sprite in image space.
            var spriteRegion = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF();
            DX.RectangleF scaledSprite = ToClient(spriteRegion).Truncate();

            _workingSprite.TextureRegion = GetSpriteTextureCoordinates();
            _workingSprite.Size = new DX.Size2F(scaledSprite.Width, scaledSprite.Height);
            _workingSprite.Position = new DX.Vector2(scaledSprite.Left, scaledSprite.Top);
        }

        /// <summary>Function called when the sprite has a property change.</summary>
        /// <param name="e">The event parameters.</param>
        protected override void OnSpriteChanged(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.SamplerState):
                    _workingSprite.TextureSampler = SpriteContent.SamplerState;
                    break;
                case nameof(ISpriteContent.Texture):
                case nameof(ISpriteContent.TextureCoordinates):
                    UpdateWorkingSprite();
                    break;
                case nameof(ISpriteContent.VertexColors):
                    for (int i = 0; i < 4; ++i)
                    {
                        _workingSprite.CornerColors[i] = SpriteContent.VertexColors[i];
                    }
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

            Renderer.Begin();

            // Draw the pattern layer.
            Renderer.DrawFilledRectangle(new DX.RectangleF(0,0, SwapChain.Width, SwapChain.Height), 
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
            Renderer.DrawSprite(_workingSprite);

            Renderer.End();

            return 1;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteContentRenderer"/> class.</summary>
        /// <param name="sprite">The sprite view model.</param>
        /// <param name="graphics">The graphics interface for the application.</param>
        /// <param name="swapChain">The swap chain for the render area.</param>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="initialZoom">The initial zoom scale value.</param>
        protected SingleSpriteRenderer(ISpriteContent sprite, GorgonGraphics graphics, GorgonSwapChain swapChain, Gorgon2D renderer, float initialZoom)
            : base(sprite, graphics, swapChain, renderer, initialZoom)
        {
            InitialTextureAlpha = 0;

            _workingSprite = new GorgonSprite
            {
                Texture = sprite.Texture,
                TextureRegion = sprite.TextureCoordinates,
                TextureArrayIndex = TextureArrayIndex,
                Size = sprite.Size,
                Scale = DX.Vector2.One,
                Anchor = DX.Vector2.Zero
            };

            for (int i = 0; i < 4; ++i)
            {
                _workingSprite.CornerColors[i] = sprite.VertexColors[i];
            }

            UpdateWorkingSprite();
        }
        #endregion
    }
}
