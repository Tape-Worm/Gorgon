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
// Created: March 15, 2019 11:56:19 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Animation;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Provides rendering functionality for the sprite editor.
    /// </summary>
    internal abstract class SpriteContentRenderer
        : ISpriteContentRenderer
    {
        #region Variables.
        // The target used to render the sprite image.
        private GorgonRenderTarget2DView _imageBuffer;
        // The controller used to animate our sprite image.
        private readonly ImageAnimationController _animController;
        // The current animation.
        private IGorgonAnimation _animation;
        // The current zoom scale.
        private float _zoomScaleValue;
        // The current image scroll offset.
        private DX.Vector2 _scrollOffset;
        // The current texture array index to use.
        private int _textureArrayIndex = 0;
        // The texture for rendering the updated sprite texture. 
        private GorgonTexture2DView _imageBufferTexture;
        // The background pattern texture.
        private GorgonTexture2DView _backgroundPattern;
        // The current sprite color.
        private readonly GorgonColor[] _spriteColor = new[]
        {
            GorgonColor.White,
            GorgonColor.White,
            GorgonColor.White,
            GorgonColor.White
        };
        #endregion

        #region Events.
        /// <summary>
        /// Event triggered when the scroll values are updated.
        /// </summary>
        public event EventHandler ScrollUpdated;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the current sprite content being edited.
        /// </summary>
        protected ISpriteContent SpriteContent
        {
            get;

        }

        /// <summary>
        /// Property to return the graphics interface for the application.
        /// </summary>
        protected GorgonGraphics Graphics
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the swap chain for the rendering area.
        /// </summary>
        protected GorgonSwapChain SwapChain
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the 2D renderer for the application.
        /// </summary>
        protected Gorgon2D Renderer
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the background pattern texture.
        /// </summary>
        protected GorgonTexture2DView BackgroundPattern
        {
            get => _backgroundPattern;
            private set => _backgroundPattern = value;
        }

        /// <summary>
        /// Property to return the texture that the sprite texture is rendered into.
        /// </summary>
        protected GorgonTexture2DView ImageBufferTexture
        {
            get => _imageBufferTexture;
            private set => _imageBufferTexture = value;
        }

        /// <summary>
        /// Property to set or return the alpha for the texture.
        /// </summary>
        public float TextureAlpha
        {
            get;
            set;
        } = 0.5f;

        /// <summary>
        /// Property to return the initial texture alpha for the background sprite texture (starting value for animation).
        /// </summary>
        public float InitialTextureAlpha
        {
            get;
            protected set;
        } = 0.5f;

        /// <summary>
        /// Property to set or return the offset for scrolling the render region.
        /// </summary>
        public DX.Vector2 ScrollOffset
        {
            get => _scrollOffset;
            set
            {
                if (value.Equals(ref _scrollOffset))
                {
                    return;
                }

                _scrollOffset = value;

                EventHandler handler = ScrollUpdated;
                handler?.Invoke(this, EventArgs.Empty);

                OnScrollOffsetChanged();
            }
        }

        /// <summary>
        /// Property to return the current zoom scaling value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value will update while the zoom animation is running, so it may not be constant.
        /// </para>
        /// </remarks>
        public float ZoomScaleValue
        {
            get => _zoomScaleValue;
            set
            {
                if (_zoomScaleValue.EqualsEpsilon(value))
                {
                    return;
                }

                _zoomScaleValue = value;

                OnZoomScaleChanged();
            }
        }


        /// <summary>
        /// Property to set or return the color to clear the background with.
        /// </summary>
        public GorgonColor BackgroundColor
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return whether or not we're animating.
        /// </summary>
        public bool IsAnimating => (_animation != null) && (_animController.State == AnimationState.Playing);

        /// <summary>Property to set or return the current texture array index to use.</summary>
        /// <value>The index of the texture array.</value>
        public int TextureArrayIndex
        {
            get => SpriteContent.Texture == null ? 0 : _textureArrayIndex;
            set
            {
                if (_textureArrayIndex == value)
                {
                    return;
                }

                if (SpriteContent.Texture == null)
                {
                    _textureArrayIndex = 0;
                    OnTextureArrayIndexChanged();
                    return;
                }

                _textureArrayIndex = value.Max(0).Min(SpriteContent.Texture.Texture.ArrayCount - 1);

                OnTextureArrayIndexChanged();
            }
        }

        /// <summary>
        /// Property to set or return the color of the sprite.
        /// </summary>
        public IReadOnlyList<GorgonColor> SpriteColor
        {
            get => _spriteColor;
            set
            {
                for (int i = 0; i < value.Count.Min(_spriteColor.Length); ++i)
                {
                    _spriteColor[i] = value[i];
                }

                OnSpriteColorChanged();
            }
        }

        /// <summary>
        /// Property to return the currently active cursor.
        /// </summary>
        public virtual Cursor CurrentCursor => Cursors.Default;

        /// <summary>
        /// Property to return some information to display about the sprite given then current renderer context.
        /// </summary>
        public virtual string SpriteInfo
        {
            get
            {
                DX.Rectangle spriteTextureBounds = SpriteContent.Texture == null ?
                    DX.Rectangle.Empty
                    : SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates);

                return string.Format(Resources.GORSPR_TEXT_SPRITE_INFO, spriteTextureBounds.Left,
                                                                spriteTextureBounds.Top,
                                                                spriteTextureBounds.Right,
                                                                spriteTextureBounds.Bottom,
                                                                spriteTextureBounds.Width,
                                                                spriteTextureBounds.Height);
            }
        }
        #endregion

        #region Methods.
        /// <summary>Handles the PropertyChanged event of the Sprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void Sprite_PropertyChanged(object sender, PropertyChangedEventArgs e) => OnSpriteChanged(e);

        /// <summary>Handles the PropertyChanging event of the Sprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void Sprite_PropertyChanging(object sender, PropertyChangingEventArgs e) => OnSpriteChanging(e);

        /// <summary>
        /// Function to destroy the image buffer.
        /// </summary>
        private void DestroyImageBuffer()
        {
            GorgonTexture2DView imgTexture = Interlocked.Exchange(ref _imageBufferTexture, null);
            GorgonRenderTarget2DView imgBuffer = Interlocked.Exchange(ref _imageBuffer, null);
            imgTexture?.Dispose();
            imgBuffer?.Dispose();
        }

        /// <summary>
        /// Function to build the image buffer render target.
        /// </summary>
        private void CreateImageBuffer()
        {
            DestroyImageBuffer();

            _imageBuffer = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, new GorgonTexture2DInfo("Sprite Editor Image Buffer")
            {
                Width = SwapChain.Width,
                Height = SwapChain.Height,
                Format = BufferFormat.R8G8B8A8_UNorm,
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Default
            });

            ImageBufferTexture = _imageBuffer.GetShaderResourceView();

            _imageBuffer.Clear(GorgonColor.BlackTransparent);
        }

        /// <summary>Handles the AfterSwapChainResized event of the SwapChain control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AfterSwapChainResizedEventArgs"/> instance containing the event data.</param>
        private void SwapChain_AfterSwapChainResized(object sender, AfterSwapChainResizedEventArgs e)
        {
            CreateImageBuffer();

            OnAfterSwapChainResized();
        }

        /// <summary>Handles the BeforeSwapChainResized event of the SwapChain control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="BeforeSwapChainResizedEventArgs"/> instance containing the event data.</param>
        private void SwapChain_BeforeSwapChainResized(object sender, BeforeSwapChainResizedEventArgs e)
        {
            StopAnimation();
            DestroyImageBuffer();

            OnBeforeSwapChainResized();
        }

        /// <summary>
        /// Function to stop an animation.
        /// </summary>
        private void StopAnimation()
        {
            // Jump to the end of the animation (doing this will set the animation to stopped, so we don't need to call Stop here - Doing so will reset the animation).
            if ((_animController.CurrentAnimation != null) && (_animController.State == AnimationState.Playing))
            {
                _animController.Time = _animController.CurrentAnimation.Length;
                _animController.Update();
            }
        }

        /// <summary>
        /// Function to start the zooming animation.
        /// </summary>
        /// <param name="newZoomScale">The final scale value.</param>
        /// <param name="scrollOffset">The scroll offset target.</param>
        private void StartAnimation(float newZoomScale, DX.Vector2 scrollOffset, float targetAlpha)
        {
            var builder = new GorgonAnimationBuilder();

            float maxTime = 0.65f;

            // Capture the current values here, when we stop the animation, it will jump to the end and set the values and we won't be able to resume properly.
            float currentZoomScale = ZoomScaleValue;
            DX.Vector2 currentScroll = ScrollOffset;
            float currentAlpha = TextureAlpha;

            if ((_animation != null) && (_animController.State == AnimationState.Playing))
            {
                maxTime = _animation.Length - _animController.Time;
            }

            StopAnimation();

            // If the region we're resizing to is smaller than our swap chain, then "slide" the image to the center of the swap chain.
            float x = scrollOffset.X;
            float y = scrollOffset.Y;

            if (!targetAlpha.EqualsEpsilon(currentAlpha))
            {
                builder.EditSingle("Opacity")
                    .SetKey(new GorgonKeySingle(0, currentAlpha))
                    .SetKey(new GorgonKeySingle(maxTime, targetAlpha))
                    .SetInterpolationMode(TrackInterpolationMode.Spline)
                    .EndEdit();
            }

            if ((!currentScroll.X.EqualsEpsilon(x)) || (!currentScroll.Y.EqualsEpsilon(y)))
            {
                builder.EditVector2("ScrollOffset")
                        .SetKey(new GorgonKeyVector2(0, currentScroll))
                        .SetKey(new GorgonKeyVector2(maxTime, new DX.Vector2(x, y)))
                        .SetInterpolationMode(TrackInterpolationMode.Spline)
                        .EndEdit();                        
            }

            if (!currentZoomScale.EqualsEpsilon(newZoomScale))
            {
                builder.EditSingle("Zoom")
                    .SetKey(new GorgonKeySingle(0, currentZoomScale))
                    .SetKey(new GorgonKeySingle(maxTime, newZoomScale))
                    .SetInterpolationMode(TrackInterpolationMode.Spline)
                    .EndEdit();                    
            }

            _animation = builder.Build("Sprite Renderer Transition Animation");

            _animController.Play(this, _animation);
        }

        /// <summary>
        /// Function called when the <see cref="SpriteColor"/> property is changed.
        /// </summary>
        protected virtual void OnSpriteColorChanged()
        {

        }

        /// <summary>
        /// Function called when the <see cref="ScrollOffset"/> property is changed.
        /// </summary>
        protected virtual void OnScrollOffsetChanged()
        {

        }

        /// <summary>
        /// Function called when the <see cref="ZoomScaleValue"/> property is changed.
        /// </summary>
        protected virtual void OnZoomScaleChanged()
        {

        }

        /// <summary>
        /// Function called when the texture array index value is updated.
        /// </summary>
        protected virtual void OnTextureArrayIndexChanged()
        {

        }

        /// <summary>
        /// Function called when after the swap chain is resized.
        /// </summary>
        protected virtual void OnAfterSwapChainResized()
        {

        }

        /// <summary>
        /// Function called before the swap chain is resized.
        /// </summary>
        protected virtual void OnBeforeSwapChainResized()
        {

        }

        /// <summary>
        /// Function to draw the docking hilight for a docked window.
        /// </summary>
        protected void DrawDockRect()
        {
            Renderer.DrawFilledRectangle(new DX.RectangleF(SwapChain.Width - 128, 0, 128, 128), new GorgonColor(GorgonColor.BluePure, 0.4f));
            Renderer.DrawRectangle(new DX.RectangleF(SwapChain.Width - 128, 1, 128, 128), GorgonColor.BluePure, 2);
        }

        /// <summary>
        /// Function to render a textured rectangle, in sprite texture pixel space,
        /// </summary>
        /// <param name="region">The region to draw.</param>
        /// <param name="color">The color to use.</param>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="textureCoords">The texture coordinates to use.</param>
        /// <param name="textureArrayIndex">The texture array index to use.</param>
        /// <param name="sampler">The texture sampler to apply.</param>
        protected void RenderRect(DX.RectangleF region, GorgonColor color, GorgonTexture2DView texture = null, DX.RectangleF? textureCoords = null, int textureArrayIndex = 0, GorgonSamplerState sampler = null)
        {
            if ((texture != null) && (textureCoords == null))
            {
                textureCoords = new DX.RectangleF(0, 0, 1, 1);
            }

            DX.RectangleF renderArea = ToClient(region).Truncate();

            Renderer.DrawFilledRectangle(renderArea, color, texture, textureCoords, textureArrayIndex, sampler);
        }

        /// <summary>
        /// Function to render the sprite texture to a render target, but without the currently active sprite.
        /// </summary>
        /// <param name="imageRegion">The region encompassing the sprite texture.</param>
        /// <param name="spriteRegion">The region encompassing the sprite texture coordinates.</param>
        protected void RenderSpriteTextureWithoutSprite(DX.RectangleF imageRegion, DX.RectangleF spriteRegion)
        {
            // If we didn't set up an image buffer to render into, then we don't need to continue.
            if (_imageBuffer == null)
            {
                return;
            }

            Graphics.SetRenderTarget(_imageBuffer);
            _imageBuffer.Clear(GorgonColor.BlackTransparent);

            Renderer.Begin();
            RenderRect(imageRegion, GorgonColor.White, SpriteContent.Texture, textureArrayIndex: _textureArrayIndex);
            RenderRect(spriteRegion, GorgonColor.BlackTransparent);
            Renderer.End();

            Graphics.SetRenderTarget(SwapChain.RenderTargetView);
        }

        /// <summary>
        /// Function called to render the sprite data.
        /// </summary>
        /// <returns>The presentation interval to use when rendering.</returns>
        protected abstract int OnRender();

        /// <summary>
        /// Function called when the sprite has a property change.
        /// </summary>
        /// <param name="e">The event parameters.</param>
        protected virtual void OnSpriteChanged(PropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Function called when the sprite is changing a property.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSpriteChanging(PropertyChangingEventArgs e)
        {
        }


        /// <summary>
        /// Function called to perform custom loading of resources.
        /// </summary>
        protected virtual void OnLoad()
        {

        }

        /// <summary>
        /// Function called to perform custom unloading of resources.
        /// </summary>
        protected virtual void OnUnload()
        {

        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public virtual void Dispose()
        {
            SwapChain.BeforeSwapChainResized -= SwapChain_BeforeSwapChainResized;
            SwapChain.AfterSwapChainResized -= SwapChain_AfterSwapChainResized;

            if (SpriteContent != null)
            {
                SpriteContent.PropertyChanging -= Sprite_PropertyChanging;
                SpriteContent.PropertyChanged -= Sprite_PropertyChanged;
            }

            GorgonTexture2DView bgPattern = Interlocked.Exchange(ref _backgroundPattern, null);
            GorgonRenderTarget2DView imgBuffer = Interlocked.Exchange(ref _imageBuffer, null);

            bgPattern?.Dispose();
            imgBuffer?.Dispose();
        }

        /// <summary>Function to unload all required resources for the renderer.</summary>
        public void Unload()
        {
            DestroyImageBuffer();
            OnUnload();
        }

        /// <summary>Function to load all required resources for the renderer.</summary>
        public void Load()
        {
            CreateImageBuffer();

            if (BackgroundPattern == null)
            {
                BackgroundPattern = GorgonTexture2DView.CreateTexture(Graphics, new GorgonTexture2DInfo("SpriteEditor_Bg_Pattern")
                {
                    Usage = ResourceUsage.Immutable,
                    Width = EditorCommonResources.CheckerBoardPatternImage.Width,
                    Height = EditorCommonResources.CheckerBoardPatternImage.Height
                }, EditorCommonResources.CheckerBoardPatternImage);
            }

            _textureArrayIndex = SpriteContent?.ArrayIndex ?? 0;

            OnLoad();
        }

        /// <summary>
        /// Function to convert a point from screen space into renderer space.
        /// </summary>
        /// <param name="point">The point coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        public DX.Vector2 FromClient(DX.Vector2 point)
        {
            if (SpriteContent?.Texture == null)
            {
                return DX.Vector2.Zero;
            }

            var clientSize = new DX.Size2F(SwapChain.Width, SwapChain.Height);
            var spriteTextureSize = new DX.Size2F(SpriteContent.Texture.Width * ZoomScaleValue, SpriteContent.Texture.Height * ZoomScaleValue);
            var renderRegion = new DX.Vector2(clientSize.Width * 0.5f - spriteTextureSize.Width * 0.5f, clientSize.Height * 0.5f - spriteTextureSize.Height * 0.5f);

            point = new DX.Vector2(point.X - renderRegion.X, point.Y - renderRegion.Y);

            var anchor = new DX.Vector2(SpriteContent.Texture.Width * 0.5f * ScrollOffset.X, SpriteContent.Texture.Height * 0.5f * ScrollOffset.Y);
            point = new DX.Vector2((point.X / ZoomScaleValue) + anchor.X, (point.Y / ZoomScaleValue) + anchor.Y);

            return point;
        }

        /// <summary>
        /// Function to convert a point from screen space into renderer space.
        /// </summary>
        /// <param name="rect">The rectangle coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        public DX.RectangleF FromClient(DX.RectangleF rect)
        {
            if (SpriteContent?.Texture == null)
            {
                return DX.RectangleF.Empty;
            }

            var clientSize = new DX.Size2F(SwapChain.Width, SwapChain.Height);
            var spriteTextureSize = new DX.Size2F(SpriteContent.Texture.Width * ZoomScaleValue, SpriteContent.Texture.Height * ZoomScaleValue);
            var renderRegion = new DX.Vector2(clientSize.Width * 0.5f - spriteTextureSize.Width * 0.5f, clientSize.Height * 0.5f - spriteTextureSize.Height * 0.5f);

            rect = new DX.RectangleF(rect.X - renderRegion.X, rect.Y - renderRegion.Y, rect.Width, rect.Height);

            var anchor = new DX.Vector2(SpriteContent.Texture.Width * 0.5f * ScrollOffset.X, SpriteContent.Texture.Height * 0.5f * ScrollOffset.Y);
            return new DX.RectangleF((rect.X / ZoomScaleValue) + anchor.X, (rect.Y / ZoomScaleValue) + anchor.Y, rect.Width / ZoomScaleValue, rect.Height / ZoomScaleValue);
        }

        /// <summary>
        /// Function to convert a point from renderer space into swap screen space.
        /// </summary>
        /// <param name="rect">The rectangle coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        public DX.Vector2 ToClient(DX.Vector2 point)
        {
            if (SpriteContent?.Texture == null)
            {
                return DX.Vector2.Zero;
            }

            var anchor = new DX.Vector2(SpriteContent.Texture.Width * 0.5f * ScrollOffset.X, SpriteContent.Texture.Height * 0.5f * ScrollOffset.Y);
            point = new DX.Vector2((point.X - anchor.X) * ZoomScaleValue, (point.Y - anchor.Y) * ZoomScaleValue);

            // Project into screen space.
            var clientSize = new DX.Size2F(SwapChain.Width, SwapChain.Height);
            var spriteTextureSize = new DX.Size2F(SpriteContent.Texture.Width * ZoomScaleValue, SpriteContent.Texture.Height * ZoomScaleValue);
            var renderRegion = new DX.Vector2(clientSize.Width * 0.5f - spriteTextureSize.Width * 0.5f, clientSize.Height * 0.5f - spriteTextureSize.Height * 0.5f);

            return new DX.Vector2(renderRegion.X + point.X, renderRegion.Y + point.Y);
        }

        /// <summary>
        /// Function to convert a rectangle from renderer space into swap screen space.
        /// </summary>
        /// <param name="rect">The rectangle coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        public DX.RectangleF ToClient(DX.RectangleF rect)
        {
            if (SpriteContent?.Texture == null)
            {
                return DX.RectangleF.Empty;
            }

            var anchor = new DX.Vector2(SpriteContent.Texture.Width * 0.5f * ScrollOffset.X, SpriteContent.Texture.Height * 0.5f * ScrollOffset.Y);
            rect = new DX.RectangleF((rect.X - anchor.X) * ZoomScaleValue, (rect.Y - anchor.Y) * ZoomScaleValue, rect.Width * ZoomScaleValue, rect.Height * ZoomScaleValue);

            // Project into screen space.
            var clientSize = new DX.Size2F(SwapChain.Width, SwapChain.Height);
            var spriteTextureSize = new DX.Size2F(SpriteContent.Texture.Width * ZoomScaleValue, SpriteContent.Texture.Height * ZoomScaleValue);
            var renderRegion = new DX.RectangleF(clientSize.Width * 0.5f - spriteTextureSize.Width * 0.5f, clientSize.Height * 0.5f - spriteTextureSize.Height * 0.5f, spriteTextureSize.Width, spriteTextureSize.Height);

            return new DX.RectangleF(renderRegion.X + rect.X, renderRegion.Y + rect.Y, rect.Width, rect.Height);
        }

        /// <summary>Function to render the sprite content.</summary>
        public void Render()
        {
            _animController.Update();

            SwapChain.Present(OnRender());
        }

        /// <summary></summary>
        /// <param name="zoomScaleValue">The new scaling value to assign.</param>
        /// <param name="centerPoint">The center point of the zoomed area.</param>
        /// <param name="targetAlpha">The target alpha value for the background.</param>
        /// <param name="animate"><b>true</b> to animate the look at transition, <b>false</b> to snap to the specified values.</param>
        /// <remarks>
        ///   <para>
        /// The <paramref name="centerPoint" /> must be a vector ranging from -1 to 1 on the X and Y axes, where -1, -1 equals the upper left corner, and 1, 1, equals the lower right corner of the image.
        /// </para>
        /// </remarks>
        public void LookAt(float zoomScaleValue, DX.Vector2 centerPoint, float targetAlpha, bool animate)
        {
            if (centerPoint.X < -1)
            {
                centerPoint.X = -1;
            }

            if (centerPoint.Y < -1)
            {
                centerPoint.Y = -1;
            }

            if (centerPoint.X > 1)
            {
                centerPoint.X = 1;
            }

            if (centerPoint.Y > 1)
            {
                centerPoint.Y = 1;
            }

            if ((SpriteContent?.Texture != null) && (animate) && ((!zoomScaleValue.EqualsEpsilon(ZoomScaleValue)) || (!targetAlpha.EqualsEpsilon(TextureAlpha))))
            {
                StartAnimation(zoomScaleValue, centerPoint, targetAlpha);
            }
            else
            {
                _zoomScaleValue = zoomScaleValue;
                _scrollOffset = centerPoint;
                TextureAlpha = targetAlpha;

                EventHandler handler = ScrollUpdated;
                handler?.Invoke(this, EventArgs.Empty);

                OnZoomScaleChanged();
                OnScrollOffsetChanged();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteContentRenderer"/> class.</summary>
        /// <param name="sprite">The sprite view model.</param>
        /// <param name="graphics">The graphics interface for the application.</param>
        /// <param name="swapChain">The swap chain for the render area.</param>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="initialZoom">The initial zoom scale value.</param>
        protected SpriteContentRenderer(ISpriteContent sprite, GorgonGraphics graphics, GorgonSwapChain swapChain, Gorgon2D renderer, float initialZoom)
        {
            Graphics = graphics;
            SwapChain = swapChain;
            Renderer = renderer;
            SpriteContent = sprite;

            if (SwapChain != null)
            {
                SwapChain.BeforeSwapChainResized += SwapChain_BeforeSwapChainResized;
                SwapChain.AfterSwapChainResized += SwapChain_AfterSwapChainResized;
            }

            _zoomScaleValue = initialZoom;

            if (SpriteContent != null)
            {
                SpriteContent.PropertyChanging += Sprite_PropertyChanging;
                SpriteContent.PropertyChanged += Sprite_PropertyChanged;
            }

            _animController = new ImageAnimationController();
        }
        #endregion
    }
}
