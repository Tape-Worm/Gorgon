#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Monday, September 22, 2014 2:24:54 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;
using SlimMath;

namespace GorgonLibrary.Examples
{
    /// <summary>
    /// Main window interface for the application.
    /// </summary>
    class MainWindow
        : IDisposable
    {
        #region Enum
        /// <summary>
        /// Animation state for the window.
        /// </summary>
        private enum WindowAnimationState
        {
            FadeIn = 0,
            Idle = 0xffff
        }
        #endregion

        #region Variables.
        // Flag to indicate that the object was disposed.
        private bool _disposed;
        // Font used for window decoration.
        private GorgonFont _decorationFont;
        // Font used for window text.
        private GorgonFont _textFont;
        // Graphics interface.
        private GorgonGraphics _graphics;
        // Renderer interface.
        private Gorgon2D _renderer;
        // The blurred desktop area.
        private GorgonRenderTarget2D _blurredImage;
        // Control that provides the drawing area.
        private Form _form;
        // Image for the desktop.
        private GorgonTexture2D _desktop;
        // Sprite used to draw the blurring.
        private GorgonSprite _blurSprite;
        // The swap chain for the output.
        private GorgonSwapChain _swapChain;
        // Sprite used to draw the desktop.
        private GorgonSprite _desktopSprite;
        // The window animation state.
        private WindowAnimationState _animationState;
        // Function used to update animation(s).
        private Func<bool> _animationUpdate;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the window region.
        /// </summary>
        public Rectangle WindowRegion
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to perform a fading in of the background.
        /// </summary>
        /// <returns>TRUE to continue, FALSE to keep calling this method.</returns>
        private bool FadeBackground()
        {
            _desktopSprite.Opacity -= (GorgonTiming.Delta * 0.5f);
            _blurSprite.Opacity += (GorgonTiming.Delta * 0.5f);
            _desktopSprite.Draw();

            return _blurSprite.Opacity >= 1;
        }

        /// <summary>
        /// Function to create the fonts used by the window.
        /// </summary>
        private void CreateFonts()
        {
            _decorationFont = _graphics.Fonts.CreateFont("Marlett",
                                                         new GorgonFontSettings
                                                         {
                                                             FontFamilyName = "Marlett",
                                                             Size = 11.25f,
                                                             AntiAliasingMode = FontAntiAliasMode.AntiAlias,
                                                             FontHeightMode = FontHeightMode.Points,
                                                             TextureSize = new Size(64, 32),
                                                             Characters = "0r "
                                                         });

            _textFont = _graphics.Fonts.CreateFont("Segoe UI",
                                                   new GorgonFontSettings
                                                   {
                                                       FontFamilyName = "Segoe UI",
                                                       FontStyle = FontStyle.Bold,
                                                       OutlineColor1 = GorgonColor.Black,
                                                       OutlineSize = 1,
                                                       Size = 11.25f,
                                                       AntiAliasingMode = FontAntiAliasMode.AntiAlias,
                                                       FontHeightMode = FontHeightMode.Points
                                                   });
        }

        /// <summary>
        /// Function to render the desktop image as a grayscale image to the blur render target.
        /// </summary>
        /// <param name="pass">Current pass.</param>
        private void RenderGrayScaleDesktop(GorgonEffectPass pass)
        {
            _renderer.Target = _blurredImage;
            _renderer.Drawing.Blit(
                                   _desktop,
                                   new RectangleF(
                                       0,
                                       0,
                                       _blurredImage.Settings.Width,
                                       _blurredImage.Settings.Height));
        }

        /// <summary>
        /// Function to blur the background image.
        /// </summary>
        private void BlurBackground()
        {
            // Render the blurred image to the target.
            _renderer.Effects.GaussianBlur.BlurRenderTargetsSize = new Size(
                _blurredImage.Settings.Width,
                _blurredImage.Settings.Height);
            _renderer.Effects.GaussianBlur.BlurAmount = 4.5f;
            _renderer.Effects.GaussianBlur.RenderScene = pass => _renderer.Drawing.Blit(_blurredImage, Vector2.Zero);
            _renderer.Effects.GaussianBlur.Render();

            _renderer.Target = _blurredImage;
            _renderer.Drawing.Blit(_renderer.Effects.GaussianBlur.Output,
                new RectangleF(0, 0, _blurredImage.Settings.Width, _blurredImage.Settings.Height));
            _renderer.Drawing.FilledRectangle(new RectangleF(0,
                                                             0,
                                                             _blurredImage.Settings.Width,
                                                             _blurredImage.Settings.Height),
                                              Color.FromArgb(132, Color.LightBlue));

            _renderer.Target = null;
            _renderer.Effects.GaussianBlur.FreeResources();
        }

        /// <summary>
        /// Function to create the blurred image.
        /// </summary>
        private void CreateBlurredImage()
        {
            Screen currentScreen = Screen.FromControl(_form);

            _form.Opacity = 0;

            if (_blurredImage != null)
            {
                _blurredImage.Dispose();
            }

            if (_desktop != null)
            {
                _desktop.Dispose();
            }

            // Capture the current desktop into a texture.
            using(Image desktopBitmap = new Bitmap(currentScreen.Bounds.Width,
                                                   currentScreen.WorkingArea.Height,
                                                   PixelFormat.Format32bppArgb))
            {
                using(var g = System.Drawing.Graphics.FromImage(desktopBitmap))
                {
                    g.CopyFromScreen(currentScreen.Bounds.Left,
                                     currentScreen.Bounds.Top,
                                     0,
                                     0,
                                     new Size(currentScreen.Bounds.Width, currentScreen.WorkingArea.Height));
                }

                _desktop = _graphics.Textures.CreateTexture<GorgonTexture2D>("Desktop", desktopBitmap);
            }

            // Create a render target to hold our blurred image and desktop image.
            _blurredImage = _graphics.Output.CreateRenderTarget("BlurredImage",
                                                                new GorgonRenderTarget2DSettings
                                                                {
                                                                    Width = _form.Width / 2,
                                                                    Height = _form.Height / 2,
                                                                    Format = BufferFormat.R8G8B8A8_UIntNormal
                                                                });

            _desktopSprite = _renderer.Renderables.CreateSprite("Desktop",
                                                    new GorgonSpriteSettings
                                                    {
                                                        Size = new Vector2(_swapChain.Settings.Width,
                                                                           _swapChain.Settings.Height),
                                                        Color = GorgonColor.White,
                                                        Texture = _desktop,
                                                        TextureRegion = new RectangleF(0, 0, 1.0f, 1.0f)
                                                    });


            _renderer.Effects.GrayScale.RenderScene = RenderGrayScaleDesktop;
            _renderer.Effects.GrayScale.Render();

            // Blur the image.
            BlurBackground();

            // Create the sprite to draw the blurry background.
            _blurSprite = _renderer.Renderables.CreateSprite("WindowBackground",
                                                             new GorgonSpriteSettings
                                                             {
                                                                 Size = new Vector2(_swapChain.Settings.Width,
                                                                                    _swapChain.Settings.Height),
                                                                 Color = Color.FromArgb(0, Color.LightBlue),
                                                                 Texture = _blurredImage,
                                                                 TextureRegion = new RectangleF(0, 0, 1.0f, 1.0f)
                                                             });

            _blurSprite.TextureSampler.BorderColor = Color.Transparent;
            _blurSprite.TextureSampler.HorizontalWrapping = TextureAddressing.Border;
            _blurSprite.TextureSampler.VerticalWrapping = TextureAddressing.Border;
            _blurSprite.ScaledSize = new Vector2(currentScreen.WorkingArea.Width, currentScreen.WorkingArea.Height);

            _desktopSprite.TextureSampler.BorderColor = Color.Transparent;
            _desktopSprite.TextureSampler.HorizontalWrapping = TextureAddressing.Border;
            _desktopSprite.TextureSampler.VerticalWrapping = TextureAddressing.Border;
            _desktopSprite.ScaledSize = new Vector2(currentScreen.WorkingArea.Width, currentScreen.WorkingArea.Height);

            _blurSprite.Position = GetFormPosition();
            _desktopSprite.Position = _blurSprite.Position;

            _form.Opacity = 1;
        }

        /// <summary>
        /// Function to retrieve the current form position.
        /// </summary>
        /// <returns>The form position.</returns>
        private Vector2 GetFormPosition()
        {
            Screen currentScreen = Screen.FromControl(_form);
    
            return new Vector2(currentScreen.WorkingArea.Left - _form.Left, currentScreen.WorkingArea.Top - _form.Top);
        }

        /// <summary>
        /// Function to perform window animations.
        /// </summary>
        private void Animate()
        {
            if ((_animationUpdate == null)
                || (_animationState == WindowAnimationState.Idle))
            {
                return;
            }

            bool result = _animationUpdate();

            if (!result)
            {
                return;
            }

            switch (_animationState)
            {
                case WindowAnimationState.FadeIn:
                    _animationState = WindowAnimationState.Idle;
                    break;
            }
        }

        /// <summary>
        /// Function to render the window to the screen.
        /// </summary>
        public void Paint()
        {
            // Perform any pending animation states.
            Animate();

            _blurSprite.Draw();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <param name="renderer">The renderer interface to use.</param>
        /// <param name="windowRegion">The window region.</param>
        public MainWindow(Gorgon2D renderer, Rectangle windowRegion)
        {
            WindowRegion = windowRegion;
            _renderer = renderer;
            _graphics = renderer.Graphics;
            _swapChain = ((GorgonSwapChain)renderer.DefaultTarget);
            _form = (Form)_swapChain.Settings.Window;
            _animationUpdate = FadeBackground;

            CreateFonts();
            CreateBlurredImage();

            _form.GotFocus += (sender, args) =>
            {
                CreateBlurredImage();
                _desktopSprite.Opacity = 0;
                _blurSprite.Opacity = 1;
            };
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_decorationFont != null)
                {
                    _decorationFont.Dispose();
                }

                if (_textFont != null)
                {
                    _textFont.Dispose();
                }

                if (_blurredImage != null)
                {
                    _blurredImage.Dispose();
                }

                if (_desktop != null)
                {
                    _desktop.Dispose();
                }
            }

            _disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
