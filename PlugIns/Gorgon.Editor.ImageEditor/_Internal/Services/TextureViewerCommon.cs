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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gorgon.Animation;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The type of animation to perform.
    /// </summary>
    internal enum AnimationType
    {
        /// <summary>
        /// No animation.
        /// </summary>
        None = 0,
        /// <summary>
        /// A fade in/fade out animation.
        /// </summary>
        Fade = 1,
        /// <summary>
        /// A zoom in/out animation.
        /// </summary>
        Zoom = 2
    }

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
        private readonly ScrollBar _hScroll;
        // The verical scroll bar.
        private readonly ScrollBar _vScroll;
        // The pixel shader for the viewer.
        private GorgonPixelShader _pixelShader;
        // The parameters for the texture viewer shader.
        private GorgonConstantBuffer _textureParameters;
        // The current render batch state.
        private Gorgon2DBatchState _batchState;
        // Pixel shader used to render the image.
        private Gorgon2DShaderState<GorgonPixelShader> _batchShaderState;
        // The animation builder used to create animations.
        private readonly GorgonAnimationBuilder _animBuilder = new GorgonAnimationBuilder();
        // The list of available animations.
        private readonly Dictionary<AnimationType, IGorgonAnimation> _animations = new Dictionary<AnimationType, IGorgonAnimation>();
        // The current animation.
        private AnimationType _currentAnimation;
        // The region for drawing the background layer.
        private DX.RectangleF _backgroundRegion;
        private DX.RectangleF _textureBounds = DX.RectangleF.Empty;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the current animation to play.
        /// </summary>
        protected AnimationType CurrentAnimation
        {
            get => _currentAnimation;
            set
            {
                if (_currentAnimation == value)
                {
                    return;
                }

                if (value == AnimationType.None)
                {
                    _currentAnimation = value;
                    return;
                }

                if (!_animations.ContainsKey(value))
                {
                    return;
                }

                _currentAnimation = value;
            }
        }

        /// <summary>
        /// Property to return the animation controller.
        /// </summary>
        protected GorgonAnimationController<ITextureViewerService> AnimationController
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the background texture.
        /// </summary>
        protected GorgonTexture2DView Background
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
            private set;
        } = ZoomLevels.ToWindow;

        /// <summary>Property to return whether or not the viewer is in the middle of an animation.</summary>
        /// <value>
        ///   <c>true</c> if this instance is animating; otherwise, <c>false</c>.</value>
        public bool IsAnimating => AnimationController.State == AnimationState.Playing;

        /// <summary>Property to set or return the alpha for the image.</summary>
        public float Alpha
        {
            get;
            set;
        } = 1.0f;

        /// <summary>
        /// Property to return the current texture boundaries.
        /// </summary>
        public DX.RectangleF TextureBounds
        {
            get => _textureBounds;
            set
            {
                if (value.Equals(ref _textureBounds))
                {
                    return;
                }

                _backgroundRegion = _textureBounds = value;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to disable scrolling of the image.
        /// </summary>
        protected void DisableScrolling()
        {
            _hScroll.Enabled = false;
            _hScroll.Value = 0;
            _vScroll.Enabled = false;
            _vScroll.Value = 0;
        }

        /// <summary>
        /// Function to scale the image to the window.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>A new rectangle containing the size and location of the scaled image.</returns>
        protected virtual DX.RectangleF OnScaleImageToClientArea(int width, int height)
        {
            DisableScrolling();

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

            textureSize.Width *= scale.X;
            textureSize.Height *= scale.Y;

            location.X = (location.X - textureSize.Width / 2.0f);
            location.Y = (location.Y - textureSize.Height / 2.0f);

            return new DX.RectangleF((int)location.X, (int)location.Y, (int)textureSize.Width, (int)textureSize.Height);
        }

        /// <summary>
        /// Function to scale the image to the window.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="scale">The scaling value to apply.</param>
        /// <returns>A new rectangle containing the size and location of the scaled image.</returns>
        protected virtual DX.RectangleF OnScaleImage(int width, int height, float scale)
        {
            var textureSize = new DX.Size2F(width, height);
            var location = new DX.Vector2(_swapChain.Width / 2.0f, _swapChain.Height / 2.0f);

            textureSize.Width *= scale;
            textureSize.Height *= scale;

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

            return new DX.RectangleF((int)location.X, (int)location.Y, (int)textureSize.Width, (int)textureSize.Height);
        }

        /// <summary>
        /// Function to retrieve a new batch state 
        /// </summary>
        /// <param name="shader">The current pixel shader.</param>
        /// <returns>The new batch state.</returns>
        private Gorgon2DBatchState GetBatchState(Gorgon2DShaderState<GorgonPixelShader> shader)
        {
            var builder = new Gorgon2DBatchStateBuilder();

            return builder.ResetTo(_batchState)
                .PixelShaderState(shader)
                .Build();
        }

        /// <summary>
        /// Function to update the animation.
        /// </summary>
        private void UpdateAnimation()
        {
            if (CurrentAnimation == AnimationType.None)
            {
                return;
            }

            if ((!_animations.TryGetValue(CurrentAnimation, out IGorgonAnimation animation))
                || (animation == null))
            {
                return;
            }

            if (animation != AnimationController.CurrentAnimation)
            {
                AnimationController.Play(this, animation);
            }
        }

        /// <summary>
        /// Function called when the zoom level is changed.
        /// </summary>
        /// <param name="zoomLevel">The current zoom level.</param>
        /// <param name="oldTargetRegion">The previous target render region.</param>
        /// <param name="newTargetRegion">The new target render region.</param>
        protected virtual void OnZoom(ZoomLevels zoomLevel, DX.RectangleF oldTargetRegion, DX.RectangleF newTargetRegion)
        {

        }

        /// <summary>
        /// Function to dispose any resources created by the implementation.
        /// </summary>
        protected virtual void OnDispose()
        {

        }

        /// <summary>
        /// Function to dispose any texture resources.
        /// </summary>
        protected abstract void OnDestroyTexture();

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
        /// <param name="image">The image being rendered.</param>
        /// <param name="batchState">The currently active batch render state.</param>
        protected abstract void OnDrawTexture(Gorgon2D renderer, IImageContent image, Gorgon2DBatchState batchState);

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
        protected abstract void OnGetShaderResourceViews(Gorgon2DShaderStateBuilder<GorgonPixelShader> builder);

        /// <summary>
        /// Function called when a playing animation should end.
        /// </summary>
        protected virtual void OnEndAnimation()
        {

        }

        /// <summary>
        /// Function to retrieve the region for the background of the image.
        /// </summary>
        /// <returns>The screen space region of the background.</returns>
        protected virtual DX.RectangleF OnGetBackgroundRegion() => _backgroundRegion;

        /// <summary>
        /// Function called before drawing begins.
        /// </summary>
        protected virtual void OnBeforeDraw()
        {

        }

        /// <summary>
        /// Function called after drawing begins.
        /// </summary>
        protected virtual void OnAfterDraw()
        {

        }

        /// <summary>
        /// Function called when the image is scrolled.
        /// </summary>
        protected virtual void OnScroll()
        {

        }

        /// <summary>
        /// Function called when the render window changes size.
        /// </summary>
        /// <param name="size">The size of the window.</param>
        protected virtual void OnWindowResize(DX.Size2 size)
        {

        }

        /// <summary>
        /// Function called during resource creation.
        /// </summary>
        /// <param name="context">The current application graphics context.</param>
        /// <param name="swapChain">The swap chain for presenting the rendered data.</param>
        protected virtual void OnCreateResources(IGraphicsContext context, GorgonSwapChain swapChain)
        {

        }

        /// <summary>
        /// Function called when the render window changes size.
        /// </summary>
        /// <param name="image">The image to display.</param>
        public void WindowResize(IImageContent image)
        {
            EndAnimation();

            if (ZoomLevel == ZoomLevels.ToWindow)
            {
                _backgroundRegion = TextureBounds = OnScaleImageToClientArea(image.Width, image.Height);
            }
            else
            {
                _backgroundRegion = TextureBounds = OnScaleImage(image.Width, image.Height, ZoomLevel.GetScale());
            }

            OnWindowResize(new DX.Size2(_swapChain.Width, _swapChain.Height));
        }

        /// <summary>Function to scroll the image.</summary>
        /// <param name="image">The image to scroll.</param>
        public void Scroll(IImageContent image)
        {
            if (image == null)
            {
                return;
            }

            EndAnimation();
            _backgroundRegion = TextureBounds = OnScaleImage(image.Width, image.Height, ZoomLevel.GetScale());
            OnScroll();
        }

        /// <summary>
        /// Function to indicate that the current animation (if one is playing) should end.
        /// </summary>
        public void EndAnimation()
        {
            if (!IsAnimating)
            {
                return;
            }

            // Jump to the end of the animation (doing this will set the animation to stopped, so we don't need to call Stop here - Doing so will reset the animation).
            AnimationController.Time = AnimationController.CurrentAnimation.Length;
            AnimationController.Update();
            OnEndAnimation();

            CurrentAnimation = AnimationType.None;
        }

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
                DepthSlice = image.DepthCount <= 1 ? 0 : ((float)(image.CurrentDepthSlice) / (image.DepthCount - 1)),
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
            EndAnimation();
            OnDestroyTexture();

            IGorgonImage imageData = image?.ImageData;

            if ((imageData == null) || (_context.Graphics == null))
            {
                return;
            }

            OnCreateTexture(_context.Graphics, imageData, image.File.Name);

            SetZoomLevel(ZoomLevel, image);

            var shaderBuilder = new Gorgon2DShaderStateBuilder<GorgonPixelShader>();
            shaderBuilder
                .ResetTo(_batchShaderState)
                .ConstantBuffer(_textureParameters.GetView(), 1)
                .Shader(_pixelShader)
                .SamplerState(GorgonSamplerState.PointFiltering, 0)
                .SamplerState(GorgonSamplerState.PointFiltering, 1);

            OnGetShaderResourceViews(shaderBuilder);

            _batchShaderState = shaderBuilder.Build();
            _batchState = GetBatchState(_batchShaderState);

            UpdateTextureParameters(image);

            // Calculate the image size relative to the client area.            
            if (ZoomLevel == ZoomLevels.ToWindow)
            {
                _backgroundRegion = TextureBounds = OnScaleImageToClientArea(image.Width, image.Height);
            }
            else
            {
                _backgroundRegion = TextureBounds = OnScaleImage(image.Width, image.Height, ZoomLevel.GetScale());
            }
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

            UpdateAnimation();

            OnBeforeDraw();

            _context.Graphics.SetRenderTarget(_swapChain.RenderTargetView);
            _swapChain.RenderTargetView.Clear(GorgonColor.Gray25);

            // Get the region for the background.
            DX.RectangleF backgroundRegion = OnGetBackgroundRegion();

            // Draw the checkboard background.
            _context.Renderer2D.Begin();
            _context.Renderer2D.DrawFilledRectangle(backgroundRegion,
                GorgonColor.White,
                Background,
                Background.ToTexel(new DX.Rectangle(0, 0, (int)backgroundRegion.Width, (int)backgroundRegion.Height)),
                textureSampler: GorgonSamplerState.Wrapping);
            _context.Renderer2D.End();

            // Draw our texture.
            _context.Renderer2D.Begin(_batchState);

            OnDrawTexture(_context.Renderer2D, image, _batchState);

            _context.Renderer2D.End();

            _swapChain.Present(1);

            OnAfterDraw();

            // Update any playing animation.
            if (AnimationController.State == AnimationState.Playing)
            {
                AnimationController.Update();

                // If we've finished the animation, then update our current animation state.
                if (AnimationController.State == AnimationState.Stopped)
                {
                    CurrentAnimation = AnimationType.None;
                }
            }
        }

        /// <summary>Function to create the resources required for the viewer.</summary>
        /// <param name="backgroundImage">The image used for display in the background.</param>
        public void CreateResources(GorgonTexture2DView backgroundImage)
        {
            Background = backgroundImage;

            _textureParameters = new GorgonConstantBuffer(_context.Graphics, new GorgonConstantBufferInfo
            {
                SizeInBytes = TextureParams.Size
            });

            _pixelShader = OnGetPixelShader(_context.Graphics, Resources.ImageViewShaders);

            _animations[AnimationType.Fade] = _animBuilder.EditSingle("Opacity")
                .SetKey(new GorgonKeySingle(0, 0))
                .SetKey(new GorgonKeySingle(0.35f, 1.0f))
                .EndEdit()
                .Build(nameof(AnimationType.Fade));

            // Set to start with this animation.                        
            CurrentAnimation = AnimationType.Fade;

            OnCreateResources(_context, _swapChain);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            OnDestroyTexture();
            OnDispose();

            _textureParameters?.Dispose();
            _textureParameters = null;

            _pixelShader?.Dispose();
            _pixelShader = null;
        }

        /// <summary>
        /// Function to retrieve the size, in pixels, if the current mip level.
        /// </summary>
        /// <param name="image">The image to retrieve mip information from.</param>
        /// <returns>The width and height of the mip level.</returns>
        public abstract DX.Size2 GetMipSize(IImageContent image);

        /// <summary>Function to set the zoom level for the specified image.</summary>
        /// <param name="zoomLevel">The zoom level to apply.</param>
        /// <param name="image">The image to zoom.</param>
        public void SetZoomLevel(ZoomLevels zoomLevel, IImageContent image)
        {
            float animTime = 0.5f;

            if (image == null)
            {
                return;
            }

            DX.RectangleF old = TextureBounds;
            DX.RectangleF newBounds;

            if ((CurrentAnimation == AnimationType.Zoom) && (AnimationController.State == AnimationState.Playing))
            {
                animTime -= AnimationController.Time;
            }

            EndAnimation();

            // Calculate the image size relative to the client area.            
            if (zoomLevel == ZoomLevels.ToWindow)
            {
                newBounds = OnScaleImageToClientArea(image.Width, image.Height);
            }
            else
            {
                newBounds = OnScaleImage(image.Width, image.Height, zoomLevel.GetScale());
            }

            if (ZoomLevel != zoomLevel)
            {
                _animations[AnimationType.Zoom] = _animBuilder
                    .Clear()                    
                    .EditRectangle("TextureBounds")
                    .SetInterpolationMode(TrackInterpolationMode.Spline)
                    .SetKey(new GorgonKeyRectangle(0, old))
                    .SetKey(new GorgonKeyRectangle(animTime, newBounds))
                    .EndEdit()
                    .Build("Zoom_Animation");

                CurrentAnimation = AnimationType.Zoom;
            }

            ZoomLevel = zoomLevel;

            OnZoom(zoomLevel, old, newBounds);
        }

        /// <summary>
        /// Function called when the mouse is moved.
        /// </summary>
        /// <param name="position">The position of the mouse cursor, relative to the image region.</param>
        /// <param name="buttons">The buttons held down while moving.</param>
        /// <param name="image">The current image.</param>
        protected virtual void OnMouseMove(DX.Vector2 position, MouseButtons buttons, IImageContent image)
        {

        }

        /// <summary>
        /// Function called when a mouse button is held down.
        /// </summary>
        /// <param name="position">The position of the mouse cursor.</param>
        /// <param name="buttons">The button(s) held down.</param>
        /// <param name="image">The current image.</param>
        protected virtual void OnMouseDown(DX.Vector2 position, MouseButtons buttons, IImageContent image)
        {

        }

        /// <summary>
        /// Function called when a mouse button is released.
        /// </summary>
        /// <param name="position">The position of the mouse cursor.</param>
        /// <param name="buttons">The button(s) released.</param>
        /// <param name="image">The current image.</param>
        protected virtual void OnMouseUp(DX.Vector2 position, MouseButtons buttons, IImageContent image)
        {

        }

        /// <summary>Function called when the mouse is moved.</summary>
        /// <param name="x">The horizontal position of the mouse.</param>
        /// <param name="y">The vertical position of the mouse.</param>
        /// <param name="buttons">The button(s) held down while moving.</param>
        /// <param name="image">The current image.</param>
        public void MouseMove(int x, int y, MouseButtons buttons, IImageContent image) => OnMouseMove(new DX.Vector2(x, y), buttons, image);

        /// <summary>Function called when a button on the mouse is held down.</summary>
        /// <param name="x">The horizontal position of the mouse.</param>
        /// <param name="y">The vertical position of the mouse.</param>
        /// <param name="buttons">The button(s) held down.</param>
        /// <param name="image">The current image.</param>
        public void MouseDown(int x, int y, MouseButtons buttons, IImageContent image) => OnMouseDown(new DX.Vector2(x, y), buttons, image);

        /// <summary>Function called when a button the on mouse is released.</summary>
        /// <param name="x">The horizontal position of the mouse.</param>
        /// <param name="y">The vertical position of the mouse.</param>
        /// <param name="buttons">The button(s) released.</param>
        /// <param name="image">The current image.</param>
        public void MouseUp(int x, int y, MouseButtons buttons, IImageContent image) => OnMouseUp(new DX.Vector2(x, y), buttons, image);
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

            AnimationController = new ImageAnimationController();
        }
        #endregion
    }
}
