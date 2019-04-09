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
// Created: March 31, 2019 11:24:36 PM
// 
#endregion

using System.ComponentModel;
using System.Linq;
using DX = SharpDX;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using Gorgon.Math;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The renderer to use when there is no tool state, or panel state.
    /// </summary>
    internal class DefaultSpriteRenderer
        : SpriteContentRenderer
    {
        #region Variables.
        // Marching ants rectangle.
        private readonly IMarchingAnts _marchAnts;
        // The actual sprite to render.
        private readonly GorgonSprite _workingSprite;
        #endregion

        #region Methods.
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

            if (SpriteContent?.Texture == null)
            {
                return;
            }

            var spriteRegion = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF();
            DX.Vector2 scaledSpritePosition = ToClient(spriteRegion.TopLeft).Truncate();

            _workingSprite.TextureSampler = SpriteContent.SamplerState;
            _workingSprite.TextureRegion = SpriteContent.TextureCoordinates;
            _workingSprite.Size = spriteRegion.Size;
            _workingSprite.Position = scaledSpritePosition;
            _workingSprite.Scale = new DX.Vector2(ZoomScaleValue);
        }

        /// <summary>Function called when the <see cref="P:Gorgon.Editor.SpriteEditor.SpriteContentRenderer.ZoomScaleValue"/> property is changed.</summary>
        protected override void OnZoomScaleChanged() => UpdateWorkingSprite();

        /// <summary>Function called when the <see cref="P:Gorgon.Editor.SpriteEditor.SpriteContentRenderer.ScrollOffset"/> property is changed.</summary>
        protected override void OnScrollOffsetChanged() => UpdateWorkingSprite();

        /// <summary>Function called when the texture array index value is updated.</summary>
        protected override void OnTextureArrayIndexChanged() => UpdateWorkingSprite();

        /// <summary>Function called when the <see cref="SpriteColor"/> property is changed.</summary>
        protected override void OnSpriteColorChanged() => UpdateWorkingSprite();

        /// <summary>Function called to render the sprite data.</summary>
        /// <returns>The presentation interval to use when rendering.</returns>
        protected override int OnRender()
        {
            var spriteRegion = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF();
            var imageRegion = new DX.RectangleF(0, 0, SpriteContent.Texture.Width, SpriteContent.Texture.Height);

            RenderSpriteTextureWithoutSprite(imageRegion, spriteRegion);

            SwapChain.RenderTargetView.Clear(BackgroundColor);

            Renderer.Begin();

            if (IsAnimating)
            {
                // Draw the pattern layer.
                Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, SwapChain.Width, SwapChain.Height),
                    new GorgonColor(GorgonColor.White, (0.5f - TextureAlpha.Min(0.5f).Max(0)) * 2),
                    BackgroundPattern,
                    new DX.RectangleF(0, 0, SwapChain.Width / BackgroundPattern.Width, SwapChain.Height / BackgroundPattern.Height));

                RenderRect(imageRegion, new GorgonColor(GorgonColor.White, TextureAlpha * 2), BackgroundPattern, new DX.RectangleF(0, 0, (imageRegion.Width * ZoomScaleValue) / BackgroundPattern.Width, (imageRegion.Height * ZoomScaleValue) / BackgroundPattern.Height));
                RenderRect(spriteRegion, GorgonColor.White, BackgroundPattern, new DX.RectangleF(0, 0, (spriteRegion.Width * ZoomScaleValue) / BackgroundPattern.Width, (spriteRegion.Height * ZoomScaleValue) / BackgroundPattern.Height));
            }
            else
            {
                // Draw the pattern layer.
                RenderRect(imageRegion, GorgonColor.White, BackgroundPattern, new DX.RectangleF(0, 0, (imageRegion.Width * ZoomScaleValue) / BackgroundPattern.Width, (imageRegion.Height * ZoomScaleValue) / BackgroundPattern.Height));
            }

            // Draw the image buffer layer.            
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, SwapChain.Width, SwapChain.Height), new GorgonColor(GorgonColor.White, TextureAlpha), ImageBufferTexture, new DX.RectangleF(0, 0, 1, 1));

            // Draw the sprite layer.
            Renderer.DrawSprite(_workingSprite);

            // Draw an indicator for the anchor.
            DX.Vector2 anchorPos = ToClient(new DX.Vector2(SpriteContent.Anchor.X * spriteRegion.Width + spriteRegion.Left,
                                                           SpriteContent.Anchor.Y * spriteRegion.Height + spriteRegion.Top)).Truncate();
            
            Renderer.DrawEllipse(new DX.RectangleF(anchorPos.X - 4, anchorPos.Y - 4, 8, 8), GorgonColor.Black);
            Renderer.DrawEllipse(new DX.RectangleF(anchorPos.X - 3, anchorPos.Y - 3, 6, 6), GorgonColor.White);

            // We convert to integer first so we can clip the decimal places.
            _marchAnts.Draw(Renderer.MeasureSprite(_workingSprite).Truncate());

            Renderer.End();

            return 1;
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
                case nameof(ISpriteContent.VertexOffsets):
                    for (int i = 0; i < 4; ++i)
                    {
                        _workingSprite.CornerOffsets[i] = SpriteContent.VertexOffsets[i];
                    }
                    break;
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.DefaultSpriteRenderer"/> class.</summary>
        /// <param name="sprite">The sprite view model.</param>
        /// <param name="graphics">The graphics interface for the application.</param>
        /// <param name="swapChain">The swap chain for the render area.</param>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="ants">The marching ants rectangle used to draw selection rectangles.</param>
        /// <param name="initialScale">The initial scaling value to apply to the render.</param>
        public DefaultSpriteRenderer(ISpriteContent sprite, GorgonGraphics graphics, GorgonSwapChain swapChain, Gorgon2D renderer, IMarchingAnts ants, float initialScale)
            : base(sprite, graphics, swapChain, renderer, initialScale)
        {
            _marchAnts = ants;            

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
                _workingSprite.CornerOffsets[i] = sprite.VertexOffsets[i];
            }

            UpdateWorkingSprite();
        }
        #endregion
    }
}
