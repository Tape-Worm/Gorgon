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

using System.IO;
using System.Linq;
using System.Threading;
using DX = SharpDX;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Renderers;
using Gorgon.Math;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Animation;
using Gorgon.Editor.Services;
using System.Windows.Forms;
using System.Diagnostics;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Provides rendering functionality for the sprite editor.
    /// </summary>
    internal class SpriteContentRenderer
        : ISpriteContentRenderer
    {
        #region Variables.
        // The graphics interface for the application.
        private GorgonGraphics _graphics;
        // The swap chain for the render area.
        private GorgonSwapChain _swapChain;
        // The 2D renderer for the application.
        private Gorgon2D _renderer;
        // The target used to render the sprite image.
        private GorgonRenderTarget2DView _imageBuffer;
        // The texture to display when a sprite lacks a texture association.
        private GorgonTexture2DView _noImage;
        // The pattern to display behind the sprite texture.
        private GorgonTexture2DView _bgPattern;
        // The texture for the image buffer.
        private GorgonTexture2DView _imageBufferTexture;
        // The size of the sprite texture.
        private DX.Size2F _spriteTextureSize;
        // Marching ants rectangle.
        private IMarchingAnts _marchAnts;
        // The current scaling.
        private ZoomLevels _currentZoom = ZoomLevels.ToWindow;
        // The controller used to animate our sprite image.
        private ImageAnimationController _animController;
        // The current animation.
        private IGorgonAnimation _animation;
        // The clipper service used to clip a rectangular region.
        private IRectClipperService _clipperService;
        // The sprite that we are editing.
        private ISpriteContent _sprite;
        // The current zoom scale.
        private float _zoomScaleValue;
        // The current image scroll offset.
        private DX.Vector2 _scrollOffset;
        // The sprite picker used to automatically clip sprite data.
        private IPickClipperService _pickClipper;
        // The current texture array index to use.
        private int _textureArrayIndex = 0;
        #endregion

        #region Properties.
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
                _clipperService.Refresh();
            }
        }

        /// <summary>
        /// Property to set or return the current zoom scaling value.
        /// </summary>
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
                _clipperService.Refresh();
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
            get => _sprite.Texture == null ? 0 : _textureArrayIndex;
            set
            {
                if (_textureArrayIndex == value)
                {
                    return;
                }

                _textureArrayIndex = value.Max(0).Max(_imageBufferTexture.Texture.ArrayCount - 1);
                _clipperService.Refresh();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to calculate scaling to the size of the window.
        /// </summary>
        /// <param name="imageSize">The size of the image.</param>
        /// <returns>The scaling factor to apply.</returns>
        private float CalcToWindow(DX.Size2F imageSize)
        {
            var scaling = new DX.Vector2(_swapChain.Width / imageSize.Width, _swapChain.Height / imageSize.Height);

            return scaling.X.Min(scaling.Y);
        }

        /// <summary>
        /// Function to retrieve the amount of scaling to apply based on zoom level.
        /// </summary>
        /// <param name="zoom">The current zoom level.</param>
        /// <param name="imageSize">The size of the sprite texture.</param>
        /// <returns>A scalar value used to scale the rendering area by.</returns>
        private float GetZoomScaleFactor(ZoomLevels zoom, DX.Size2F imageSize) => zoom == ZoomLevels.ToWindow ? CalcToWindow(imageSize) : zoom.GetScale();

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
            _imageBuffer = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo("Sprite Editor Image Buffer")
            {
                Width = _swapChain.Width,
                Height = _swapChain.Height,
                Format = BufferFormat.R8G8B8A8_UNorm,
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Default
            });

            _imageBufferTexture = _imageBuffer.Texture.GetShaderResourceView();

            _imageBuffer.Clear(GorgonColor.BlackTransparent);
        }

        /// <summary>Handles the AfterSwapChainResized event of the SwapChain control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AfterSwapChainResizedEventArgs"/> instance containing the event data.</param>
        private void SwapChain_AfterSwapChainResized(object sender, AfterSwapChainResizedEventArgs e)
        {
            CreateImageBuffer();
            _clipperService.Refresh();
        }

        /// <summary>Handles the BeforeSwapChainResized event of the SwapChain control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="BeforeSwapChainResizedEventArgs"/> instance containing the event data.</param>
        private void SwapChain_BeforeSwapChainResized(object sender, BeforeSwapChainResizedEventArgs e)
        {
            StopAnimation();
            DestroyImageBuffer();
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
        /// <param name="spriteTextureSize">The size of the sprite texture (with current transformation applied).</param>
        private void StartAnimation(float newZoomScale, DX.Size2F spriteTextureSize)
        {
            var builder = new GorgonAnimationBuilder();

            float maxTime = 0.5f;

            if ((_animation != null) && (_animController.State == AnimationState.Playing))
            {
                maxTime = _animation.Length - _animController.Time;
            }

            StopAnimation();

            // If the region we're resizing to is smaller than our swap chain, then "slide" the image to the center of the swap chain.
            float x = ScrollOffset.X;
            float y = ScrollOffset.Y;

            if (_swapChain.Width >= spriteTextureSize.Width)
            {
                x = 0;
            }

            if (_swapChain.Height >= spriteTextureSize.Height)
            {
                y = 0;
            }

            if ((!ScrollOffset.X.EqualsEpsilon(x)) || (!ScrollOffset.Y.EqualsEpsilon(y)))
            {
                builder.EditPositions()
                        .SetKey(new GorgonKeyVector3(0, ScrollOffset))
                        .SetKey(new GorgonKeyVector3(maxTime, new DX.Vector2(x, y)))
                        .EndEdit()
                        .PositionInterpolationMode(TrackInterpolationMode.Spline);
            }

            builder.EditScale()
                .SetKey(new GorgonKeyVector3(0, new DX.Vector3(ZoomScaleValue)))
                .SetKey(new GorgonKeyVector3(maxTime, new DX.Vector3(newZoomScale)))
                .EndEdit()
                .ScaleInterpolationMode(TrackInterpolationMode.Spline);

            _animation = builder.Build("Zoom Animation");

            _animController.Play(this, _animation);
        }

        /// <summary>
        /// Function to render an image when the sprite content doesn't have a texture attached to it.
        /// </summary>
        private void RenderNoTexture()
        {
            var clientSize = new DX.Size2F(_swapChain.Width, _swapChain.Height);
            float newSize = clientSize.Width < clientSize.Height ? clientSize.Width : clientSize.Height;
            var size = new DX.Size2F(newSize.Min(_noImage.Width), newSize.Min(_noImage.Width));
            var halfClient = new DX.Size2F(clientSize.Width / 2.0f, clientSize.Height / 2.0f);
            var pos = new DX.Vector2((int)(halfClient.Width - size.Width / 2.0f), (int)(halfClient.Height - size.Height / 2.0f));

            _graphics.SetRenderTarget(_swapChain.RenderTargetView);
            _swapChain.RenderTargetView.Clear(BackgroundColor);

            _renderer.Begin();
            _renderer.DrawFilledRectangle(new DX.RectangleF(pos.X, pos.Y, size.Width, size.Height), GorgonColor.White, _noImage, new DX.RectangleF(0, 0, 1, 1));
            _renderer.End();
        }

        /// <summary>
        /// Function to convert a point from screen space into swap space.
        /// </summary>
        /// <param name="point">The point coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        private DX.Vector2 ScreenToSwapPoint(DX.Vector2 point)
        {
            // Convert into client coordinates.
            var ctlPoint = new System.Drawing.Point((int)point.X, (int)point.Y);
            ctlPoint = _swapChain.Window.PointToClient(ctlPoint);
            return new DX.Vector2(ctlPoint.X, ctlPoint.Y);
        }

        /// <summary>
        /// Function to convert a point from screen space into image space.
        /// </summary>
        /// <param name="point">The point coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        private DX.Vector2 SwapToImagePoint(DX.Vector2 point)
        {
            var clientSize = new DX.Size2F(_swapChain.Width, _swapChain.Height);
            var spriteTextureSize = new DX.Size2F(_spriteTextureSize.Width * ZoomScaleValue, _spriteTextureSize.Height * ZoomScaleValue);
            var renderRegion = new DX.Vector2(clientSize.Width / 2.0f - spriteTextureSize.Width / 2.0f, clientSize.Height / 2.0f - spriteTextureSize.Height / 2.0f);

            point = new DX.Vector2(point.X - renderRegion.X, point.Y - renderRegion.Y);

            var anchor = new DX.Vector2(_spriteTextureSize.Width / 2 * ScrollOffset.X, _spriteTextureSize.Height / 2 * ScrollOffset.Y);
            point = new DX.Vector2((point.X / ZoomScaleValue) + anchor.X, (point.Y / ZoomScaleValue) + anchor.Y);

            return point;
        }

        /// <summary>
        /// Function to convert a point from screen space into swap space.
        /// </summary>
        /// <param name="rect">The rectangle coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        private DX.RectangleF ScreenToSwapRect(DX.RectangleF point)
        {
            // Convert into client coordinates.
            var ctlRect = new System.Drawing.Rectangle((int)point.X, (int)point.Y, (int)point.Width, (int)point.Height);
            ctlRect = _swapChain.Window.RectangleToClient(ctlRect);
            return new DX.RectangleF(ctlRect.X, ctlRect.Y, ctlRect.Width, ctlRect.Height);
        }

        /// <summary>
        /// Function to convert a point from screen space into image space.
        /// </summary>
        /// <param name="rect">The rectangle coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        private DX.RectangleF SwapToImageRect(DX.RectangleF rect)
        {
            var clientSize = new DX.Size2F(_swapChain.Width, _swapChain.Height);
            var spriteTextureSize = new DX.Size2F(_spriteTextureSize.Width * ZoomScaleValue, _spriteTextureSize.Height * ZoomScaleValue);
            var renderRegion = new DX.Vector2(clientSize.Width / 2.0f - spriteTextureSize.Width / 2.0f, clientSize.Height / 2.0f - spriteTextureSize.Height / 2.0f);

            rect = new DX.RectangleF(rect.X - renderRegion.X, rect.Y - renderRegion.Y, rect.Width, rect.Height);

            var anchor = new DX.Vector2(_spriteTextureSize.Width / 2 * ScrollOffset.X, _spriteTextureSize.Height / 2 * ScrollOffset.Y);
            return new DX.RectangleF((rect.X / ZoomScaleValue) + anchor.X, (rect.Y / ZoomScaleValue) + anchor.Y, rect.Width / ZoomScaleValue, rect.Height / ZoomScaleValue);
        }

        /// <summary>
        /// Function to convert a point from image space into swap screen space.
        /// </summary>
        /// <param name="rect">The rectangle coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        private DX.Vector2 ImagePointToSwap(DX.Vector2 point)
        {
            var anchor = new DX.Vector2(_spriteTextureSize.Width / 2 * ScrollOffset.X, _spriteTextureSize.Height / 2 * ScrollOffset.Y);
            point = new DX.Vector2((point.X - anchor.X) * ZoomScaleValue, (point.Y - anchor.Y) * ZoomScaleValue);

            // Project into screen space.
            var clientSize = new DX.Size2F(_swapChain.Width, _swapChain.Height);
            var spriteTextureSize = new DX.Size2F(_spriteTextureSize.Width * ZoomScaleValue, _spriteTextureSize.Height * ZoomScaleValue);
            var renderRegion = new DX.Vector2(clientSize.Width / 2.0f - spriteTextureSize.Width / 2.0f, clientSize.Height / 2.0f - spriteTextureSize.Height / 2.0f);

            return new DX.Vector2(renderRegion.X + point.X, renderRegion.Y + point.Y);
        }

        /// <summary>
        /// Function to convert a rectangle from image space into swap screen space.
        /// </summary>
        /// <param name="rect">The rectangle coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        private DX.RectangleF ImageRectToSwap(DX.RectangleF rect)
        {
            var anchor = new DX.Vector2(_spriteTextureSize.Width / 2 * ScrollOffset.X, _spriteTextureSize.Height / 2 * ScrollOffset.Y);
            rect = new DX.RectangleF((rect.X - anchor.X) * ZoomScaleValue, (rect.Y - anchor.Y) * ZoomScaleValue, rect.Width * ZoomScaleValue, rect.Height * ZoomScaleValue);

            // Project into screen space.
            var clientSize = new DX.Size2F(_swapChain.Width, _swapChain.Height);
            var spriteTextureSize = new DX.Size2F(_spriteTextureSize.Width * ZoomScaleValue, _spriteTextureSize.Height * ZoomScaleValue);
            var renderRegion = new DX.RectangleF(clientSize.Width / 2.0f - spriteTextureSize.Width / 2.0f, clientSize.Height / 2.0f - spriteTextureSize.Height / 2.0f, spriteTextureSize.Width, spriteTextureSize.Height);

            return new DX.RectangleF(renderRegion.X + rect.X, renderRegion.Y + rect.Y, rect.Width, rect.Height);
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
        private void RenderRect(DX.RectangleF region, GorgonColor color, GorgonTexture2DView texture = null, DX.RectangleF? textureCoords = null, int textureArrayIndex = 0, GorgonSamplerState sampler = null)
        {
            if ((texture != null) && (textureCoords == null))
            {
                textureCoords = new DX.RectangleF(0, 0, 1, 1);
            }

            DX.RectangleF renderArea = ImageRectToSwap(region);

            _renderer.DrawFilledRectangle(renderArea, color, texture, textureCoords, textureArrayIndex, sampler);
        }

        /// <summary>
        /// Function to render the sprite in a clipping context.
        /// </summary>
        private void RenderClipping()
        {
            var spriteRegion = _sprite.Texture.ToPixel(_sprite.TextureCoordinates).ToRectangleF();
            var imageRegion = new DX.RectangleF(0, 0, _spriteTextureSize.Width, _spriteTextureSize.Height);

            _graphics.SetRenderTarget(_swapChain.RenderTargetView);
            _swapChain.RenderTargetView.Clear(BackgroundColor);

            _renderer.Begin();

            // Draw the pattern layer.
            RenderRect(imageRegion, GorgonColor.White, _bgPattern, new DX.RectangleF(0, 0, (imageRegion.Width * ZoomScaleValue) / _bgPattern.Width, (imageRegion.Height * ZoomScaleValue) / _bgPattern.Height));
            // Draw the image buffer layer.
            RenderRect(imageRegion, new GorgonColor(GorgonColor.White, 0.5f), _sprite.Texture, new DX.RectangleF(0, 0, 1, 1), _textureArrayIndex, GorgonSamplerState.PointFiltering);

            DX.RectangleF textureRegion = _clipperService.Rectangle.Truncate();

            RenderRect(textureRegion, GorgonColor.White, _sprite.Texture, _sprite.Texture.ToTexel(textureRegion.ToRectangle()), _textureArrayIndex, GorgonSamplerState.PointFiltering);

            _clipperService.Render();

            _renderer.End();
        }

        /// <summary>
        /// Function to render the sprite.
        /// </summary>
        private void RenderPicker()
        {
            DX.RectangleF spriteRegion = _pickClipper.Rectangle;
            var imageRegion = new DX.RectangleF(0, 0, _spriteTextureSize.Width, _spriteTextureSize.Height);

            _graphics.SetRenderTarget(_imageBuffer);
            _imageBuffer.Clear(GorgonColor.BlackTransparent);

            _renderer.Begin();
            RenderRect(imageRegion, GorgonColor.White, _sprite.Texture, textureArrayIndex: _textureArrayIndex,sampler: GorgonSamplerState.PointFiltering);
            RenderRect(spriteRegion, GorgonColor.BlackTransparent);
            _renderer.End();

            _graphics.SetRenderTarget(_swapChain.RenderTargetView);
            _swapChain.RenderTargetView.Clear(BackgroundColor);

            _renderer.Begin();

            // Draw the pattern layer.
            RenderRect(imageRegion, GorgonColor.White, _bgPattern, new DX.RectangleF(0, 0, (imageRegion.Width * ZoomScaleValue) / _bgPattern.Width, (imageRegion.Height * ZoomScaleValue) / _bgPattern.Height));
            // Draw the image buffer layer.
            _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _swapChain.Width, _swapChain.Height), new GorgonColor(GorgonColor.White, 0.5f), _imageBufferTexture, new DX.RectangleF(0, 0, 1, 1));
            // Draw the sprite layer.
            RenderRect(spriteRegion, GorgonColor.White, _sprite.Texture, _sprite.Texture.ToTexel(spriteRegion.ToRectangle()), _textureArrayIndex);

            _marchAnts.Draw(ImageRectToSwap(spriteRegion));

            _renderer.End();
        }

        /// <summary>
        /// Function to render the sprite.
        /// </summary>
        private void RenderSprite()
        {            
            var spriteRegion = _sprite.Texture.ToPixel(_sprite.TextureCoordinates).ToRectangleF();
            var imageRegion = new DX.RectangleF(0, 0, _spriteTextureSize.Width, _spriteTextureSize.Height);

            _graphics.SetRenderTarget(_imageBuffer);
            _imageBuffer.Clear(GorgonColor.BlackTransparent);

            _renderer.Begin();
            RenderRect(imageRegion, GorgonColor.White, _sprite.Texture, textureArrayIndex: _textureArrayIndex, sampler: GorgonSamplerState.PointFiltering);
            RenderRect(spriteRegion, GorgonColor.BlackTransparent);
            _renderer.End();

            _graphics.SetRenderTarget(_swapChain.RenderTargetView);
            _swapChain.RenderTargetView.Clear(BackgroundColor);

            _renderer.Begin();

            // Draw the pattern layer.
            RenderRect(imageRegion, GorgonColor.White, _bgPattern, new DX.RectangleF(0, 0, (imageRegion.Width * ZoomScaleValue) / _bgPattern.Width, (imageRegion.Height * ZoomScaleValue) / _bgPattern.Height));
            // Draw the image buffer layer.
            _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _swapChain.Width, _swapChain.Height), new GorgonColor(GorgonColor.White, 0.5f), _imageBufferTexture, new DX.RectangleF(0, 0, 1, 1));
            // Draw the sprite layer.
            RenderRect(spriteRegion, GorgonColor.White, _sprite.Texture, _sprite.Texture.ToTexel(spriteRegion.ToRectangle()), _textureArrayIndex);

            _marchAnts.Draw(ImageRectToSwap(spriteRegion));

            _renderer.End();
        }

        /// <summary>
        /// Function to retrieve the current size of the sprite texture, in pixels.
        /// </summary>
        /// <param name="sprite">The currently active sprite.</param>
        /// <returns>The size of the sprite texture, after transformation, in pixels.</returns>
        public DX.RectangleF GetSpriteTextureSize(ISpriteContent sprite)
        {
            if (sprite.Texture == null)
            {
                return DX.RectangleF.Empty;
            }

            var imageRegion = new DX.RectangleF(0, 0, sprite.Texture.Width, sprite.Texture.Height);
            return ImageRectToSwap(imageRegion);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _swapChain.BeforeSwapChainResized -= SwapChain_BeforeSwapChainResized;
            _swapChain.AfterSwapChainResized -= SwapChain_AfterSwapChainResized;

            IMarchingAnts ants = Interlocked.Exchange(ref _marchAnts, null);
            GorgonTexture2DView bgPattern = Interlocked.Exchange(ref _bgPattern, null);
            GorgonTexture2DView noImage = Interlocked.Exchange(ref _noImage, null);
            GorgonRenderTarget2DView imgBuffer = Interlocked.Exchange(ref _imageBuffer, null);

            ants?.Dispose();
            bgPattern?.Dispose();
            noImage?.Dispose();
            imgBuffer?.Dispose();
        }

        /// <summary>Function to load all required resources for the renderer.</summary>
        public void Load()
        {
            CreateImageBuffer();

            using (var stream = new MemoryStream(Resources.SpriteEditor_Bg_1024x1024))
            {
                _noImage = GorgonTexture2DView.FromStream(_graphics, stream, new GorgonCodecDds(), options: new GorgonTexture2DLoadOptions
                {
                    Name = "Sprite Editor - No texture default texture",
                    Usage = ResourceUsage.Immutable
                });
            }

            _bgPattern = GorgonTexture2DView.CreateTexture(_graphics, new GorgonTexture2DInfo("SpriteEditor_Bg_Pattern")
            {
                Usage = ResourceUsage.Immutable
            }, EditorCommonResources.CheckerBoardPatternImage);

            _animController = new ImageAnimationController();

            _swapChain.BeforeSwapChainResized += SwapChain_BeforeSwapChainResized;
            _swapChain.AfterSwapChainResized += SwapChain_AfterSwapChainResized;
        }

        /// <summary>Function to render the sprite content.</summary>
        /// <param name="zoom">The amount of zoom to apply to the render area.</param>
        /// <param name="tool">The active tool for the editor.</param>
        public void Render(ZoomLevels zoom, SpriteEditTool tool)
        {
            int presentRate = 1;

            switch (_sprite?.Texture)
            {
                case null:
                    RenderNoTexture();
                    break;
                default:
                    DX.Size2F spriteTextureSize = _spriteTextureSize;
                    float newScale = GetZoomScaleFactor(zoom, spriteTextureSize);

                    spriteTextureSize.Width *= newScale;
                    spriteTextureSize.Height *= newScale;

                    // The new scale and current scale are out of sync, so let's animate the scaling until they get back into sync.
                    if (_currentZoom != zoom)
                    {
                        StartAnimation(newScale, spriteTextureSize);
                        _currentZoom = zoom;
                    }
                    else if ((_animation == null) || (_animController.State != AnimationState.Playing))
                    {
                        ZoomScaleValue = newScale;
                    }

                    switch (tool)
                    {
                        case SpriteEditTool.SpriteClip:
                            // Adjust the presentation rate if we're dragging so we can get quicker visual feedback.
                            presentRate = _clipperService.IsDragging ? 0 : 1;
                            RenderClipping();
                            break;
                        case SpriteEditTool.SpritePick:
                            RenderPicker();
                            break;
                        default:
                            RenderSprite();
                            break;
                    }
                    break;
            }

            _swapChain.Present(presentRate);

            _animController.Update();
        }


        /// <summary>
        /// Function to assign the sprite to use.
        /// </summary>
        /// <param name="sprite">The sprite to assign.</param>
        public void SetSprite(ISpriteContent sprite)
        {
            if (sprite?.Texture == null)
            {
                _sprite = null;
                _spriteTextureSize = DX.Size2F.Empty;
                _pickClipper.Rectangle = _clipperService.Rectangle = DX.RectangleF.Empty;
                _pickClipper.ImageData = null;
                return;
            }

            _sprite = sprite;
            _spriteTextureSize = new DX.Size2F(sprite.Texture.Width, sprite.Texture.Height);
            _clipperService.Bounds = new DX.RectangleF(0, 0, sprite.Texture.Width, sprite.Texture.Height);
            _pickClipper.Rectangle = _clipperService.Rectangle = sprite.Texture.ToPixel(sprite.TextureCoordinates).ToRectangleF();
            TextureArrayIndex = sprite.ArrayIndex;

            _clipperService.TransformImageAreaToClient = r => ImageRectToSwap(r).Truncate();
            _pickClipper.TransformMouseToImage =_clipperService.TransformMouseToImage = p =>
            {
                DX.Vector2 pos = SwapToImagePoint(p);
                return new DX.Vector2((int)pos.X, (int)pos.Y);
            };

            _pickClipper.ImageData = _sprite.ImageData;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteContentRenderer"/> class.</summary>
        /// <param name="graphics">The graphics interface for the application.</param>
        /// <param name="swapChain">The swap chain for the render area.</param>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="rectClipper">The rectangle clipper used to clip out sprite data.</param>
        /// <param name="pickClipper">The sprite picker used to automatically clip sprite data.</param>
        /// <param name="ants">The marching ants rectangle used to draw selection rectangles.</param>
        public SpriteContentRenderer(GorgonGraphics graphics, GorgonSwapChain swapChain, Gorgon2D renderer, IRectClipperService rectClipper, IPickClipperService pickClipper, IMarchingAnts ants)
        {
            _graphics = graphics;
            _swapChain = swapChain;
            _renderer = renderer;
            _marchAnts = ants;
            _clipperService = rectClipper;
            _pickClipper = pickClipper;
        }
        #endregion
    }
}
