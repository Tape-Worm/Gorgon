#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: August 6, 2018 2:40:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI;
using Drawing = System.Drawing;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        #region Variables.
        // The primary graphics interface.
        private static GorgonGraphics _graphics;
        // The main "screen" for the application.
        private static GorgonSwapChain _screen;
        // Our 2D renderer.
        private static Gorgon2D _renderer;
        // The images to be displayed.
        private static GorgonTexture2DView[] _images;
        // The current image being rendered.
        private static int _currentImage;
        // The compositor for drawing our effects.
        private static Gorgon2DCompositor _compositor;
        // The blur effect.
        private static Gorgon2DGaussBlurEffect _blurEffect;
        // The grayscale effect.
        private static Gorgon2DGrayScaleEffect _grayScaleEffect;
        // The posterize effect.
        private static Gorgon2DPosterizedEffect _posterizeEffect;
        // The 1 bit effect.
        private static Gorgon2D1BitEffect _1BitEffect;
        // The burn effect.
        private static Gorgon2DBurnDodgeEffect _burnEffect;
        // The dodge effect.
        private static Gorgon2DBurnDodgeEffect _dodgeEffect;
        // The invert effect.
        private static Gorgon2DInvertEffect _invertEffect;
        // The sharpen effect.
        private static Gorgon2DSharpenEmbossEffect _sharpenEffect;
        // The emboss effect.
        private static Gorgon2DSharpenEmbossEffect _embossEffect;
        // The sobel edge detection effect.
        private static Gorgon2DSobelEdgeDetectEffect _sobelEffect;
        // The old film effect.
        private static Gorgon2DOldFilmEffect _oldFilmEffect;
        // The bloom effect.
        private static Gorgon2DBloomEffect _bloomEffect;
        // The chromatic aberration effect.
        private static Gorgon2DChromaticAberrationEffect _chromatic;
        // The buttons for the composition passes.
        private static Button[] _buttons;
        // The button currently being dragged.
        private static Button _dragButton;
        // The offset of the mouse cursor when dragging started.
        private static DX.Vector2 _dragOffset;
        // The starting position of the drag.
        private static DX.Vector2 _dragStart;        
        #endregion

        #region Methods.
        /// <summary>
        /// Function to present the rendering to the main window.
        /// </summary>
        private static void Present()
        {
            if (_graphics.RenderTargets[0] != _screen.RenderTargetView)
            {
                _graphics.SetRenderTarget(_screen.RenderTargetView);
            }

            GorgonExample.DrawStatsAndLogo(_renderer);

            _screen.Present(1);
        }

        /// <summary>
        /// Function called when the application goes into an idle state.
        /// </summary>
        /// <returns><b>true</b> to continue executing, <b>false</b> to stop.</returns>
        private static bool Idle()
        {
            _screen.RenderTargetView.Clear(GorgonColor.White);

            _compositor.Render(_images[_currentImage], _screen.RenderTargetView);

            _renderer.Begin();

            // Draw our simple GUI.
            for (int i = 0; i < _buttons.Length; ++i)
            {
                Button button = _buttons[i];

                if (button.IsDragging)
                {
                    continue;
                }

                _renderer.DrawFilledRectangle(button.Bounds, button.BackColor);
            }

            for (int i = 0; i < _buttons.Length; ++i)
            {
                Button button = _buttons[i];

                if (button.IsDragging)
                {
                    continue;
                }

                _renderer.DrawString(button.Text, button.Bounds.TopLeft, color: button.ForeColor);
            }

            // Always draw the dragging button on top.
            if (_dragButton != null)
            {
                Drawing.Point cursorPosition = _screen.Window.PointToClient(Cursor.Position);

                var pos = new DX.RectangleF(cursorPosition.X - _dragOffset.X, cursorPosition.Y - _dragOffset.Y, _dragButton.Bounds.Width, _dragButton.Bounds.Height);
                _renderer.DrawFilledRectangle(pos, _dragButton.BackColor);
                _renderer.DrawString(_dragButton.Text, pos.TopLeft, color: _dragButton.ForeColor);
            }

            _renderer.End();

            Present();

            // This effect requires a time value to animate.  
            _oldFilmEffect.Time = GorgonTiming.SecondsSinceStart;

            // Shake the old film for a few seconds.
            if ((_compositor["Olde Film"].Enabled) && ((GorgonTiming.SecondsSinceStart % 10) >= 6))
            {
                _oldFilmEffect.ShakeOffset = new DX.Vector2(GorgonRandom.RandomSingle(-2, 2), GorgonRandom.RandomSingle(-2, 2));
            }
            else
            {
                _oldFilmEffect.ShakeOffset = DX.Vector2.Zero;
            }

            return true;
        }

        /// <summary>
        /// Function to load the images for use with the example.
        /// </summary>
        /// <param name="codec">The codec for the images.</param>
        /// <returns><b>true</b> if files were loaded successfully, <b>false</b> if not.</returns>
        private static bool LoadImageFiles(IGorgonImageCodec codec)
        {
            DirectoryInfo dirInfo = GorgonExample.GetResourcePath(@"\Textures\PostProcess");
            FileInfo[] files = dirInfo.GetFiles("*.dds", SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
            {
                GorgonDialogs.ErrorBox(null, $"No DDS images found in {dirInfo.FullName}.");
                GorgonApplication.Quit();
                return false;
            }

            _images = new GorgonTexture2DView[files.Length];

            // Load all the DDS files from the resource area.
            for (int i = 0; i < files.Length; ++i)
            {
                _images[i] = GorgonTexture2DView.FromFile(_graphics,
                                                          files[i].FullName,
                                                          codec,
                                                          new GorgonTexture2DLoadOptions
                                                          {
                                                              Binding = TextureBinding.ShaderResource,
                                                              Usage = ResourceUsage.Immutable,
                                                              Name = Path.GetFileNameWithoutExtension(files[i].Name)
                                                          });
            }

            return true;
        }

        /// <summary>
        /// Function to perform the button layout.
        /// </summary>
        private static void LayoutGUI()
        {
            float maxWidth = 0;
            var position = new DX.Vector2(0, 70);

            for (int i = 0; i < _buttons.Length; ++i)
            {
                if (_buttons[i] != null)
                {
                    _buttons[i].Click -= Button_Click;
                }

                _buttons[i] = new Button(_compositor[i]);
                DX.Size2F size = _renderer.DefaultFont.MeasureText(_buttons[i].Text, false);
                maxWidth = maxWidth.Max(size.Width);
                _buttons[i].Bounds = new DX.RectangleF(0, position.Y, 0, size.Height);
                position = new DX.Vector2(0, position.Y + size.Height + 2);
            }

            for (int i = 0; i < _buttons.Length; ++i)
            {
                Button button = _buttons[i];
                button.Bounds = new DX.RectangleF(button.Bounds.Left, button.Bounds.Top, maxWidth, button.Bounds.Height);
                button.Click += Button_Click;
            }
        }


        /// <summary>
        /// Function to handle the button click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Button_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            button.Pass.Enabled = !button.Pass.Enabled;
        }

        /// <summary>
        /// Function to initialize the effects and the effect compositor.
        /// </summary>
        private static void InitializeEffects()
        {
            // The blur effect.
            _blurEffect = new Gorgon2DGaussBlurEffect(_renderer)
            {
                PreserveAlpha = true
            };
            _blurEffect.Precache();

            // The grayscale effect.
            _grayScaleEffect = new Gorgon2DGrayScaleEffect(_renderer);

            // The posterize effect.
            _posterizeEffect = new Gorgon2DPosterizedEffect(_renderer)
            {
                Bits = 10
            };

            // The 1 bit effect.
            _1BitEffect = new Gorgon2D1BitEffect(_renderer)
            {
                Threshold = new GorgonRangeF(0.5f, 1.0f),
                ConvertAlphaChannel = false
            };

            // The burn effect.
            _burnEffect = new Gorgon2DBurnDodgeEffect(_renderer)
            {
                UseDodge = false
            };

            // The dodge effect.
            _dodgeEffect = new Gorgon2DBurnDodgeEffect(_renderer)
            {
                UseDodge = true
            };

            // The invert effect.
            _invertEffect = new Gorgon2DInvertEffect(_renderer);

            // The sharpen effect.
            _sharpenEffect = new Gorgon2DSharpenEmbossEffect(_renderer)
            {
                UseEmbossing = false,
                Amount = 1.0f
            };
            // The emboss effect.
            _embossEffect = new Gorgon2DSharpenEmbossEffect(_renderer)
            {
                UseEmbossing = true,
                Amount = 1.0f
            };

            // The sobel edge detection effect.
            _sobelEffect = new Gorgon2DSobelEdgeDetectEffect(_renderer)
            {
                LineThickness = 2.5f,
                EdgeThreshold = 0.80f
            };

            // An old film effect.
            _oldFilmEffect = new Gorgon2DOldFilmEffect(_renderer)
            {
                ScrollSpeed = 0.05f
            };
            // A HDR bloom effect.
            _bloomEffect = new Gorgon2DBloomEffect(_renderer)
            {
                Threshold = 1.11f,
                BloomIntensity = 35.0f,
                BlurAmount = 6.0f,
                ColorIntensity = 1.15f
            };
            // A chromatic aberration effect.
            _chromatic = new Gorgon2DChromaticAberrationEffect(_renderer)
            {
                Intensity = 0.25f
            };

            _compositor = new Gorgon2DCompositor(_renderer);
            // Set up each pass for the compositor.
            // As you can see, we're not strictly limited to using our 2D effect objects, we can define custom passes as well.
            // And, we can also define how existing effects are rendered for things like animation and such.
            _compositor.RenderingPass("Rendering Pass", (renderer, texture, target) =>
                       {
                           renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, target.Width, target.Height),
                                                      GorgonColor.White,
                                                      texture,
                                                      new DX.RectangleF(0, 0, 1, 1));
                           
                           var midPoint = new DX.Vector2(target.Width * 0.5f, target.Height * 0.5f);

                           for (int i = 160; i >= 100; --i)
                           {
                               var region = new DX.RectangleF(midPoint.X - i * 0.5f, midPoint.Y - i * 0.5f, i, i);

                               renderer.DrawFilledEllipse(region, GorgonColor.Lerp(GorgonColor.RedPure, GorgonColor.YellowPure, 1.0f - ((i - 100) / 60.0f)));
                           }
                       })
                       .EffectPass("1-Bit Color", _1BitEffect)
                       .EffectPass("Grayscale", _grayScaleEffect)
                       .EffectPass("Emboss", _embossEffect)
                       .EffectPass("Sharpen", _sharpenEffect)
                       .EffectPass("Blur", _blurEffect)
                       .EffectPass("Posterize", _posterizeEffect)
                       .EffectPass("Burn", _burnEffect)
                       .EffectPass("Dodge", _dodgeEffect)
                       .EffectPass("Sobel Edge Detection", _sobelEffect)
                       .EffectPass("Bloom", _bloomEffect)
                       .EffectPass("Olde Film", _oldFilmEffect)
                       .EffectPass("Chromatic Aberration", _chromatic)
                       .EffectPass("Invert", _invertEffect)
                       .InitialClearColor(GorgonColor.White)
                       .FinalClearColor(GorgonColor.White);

            foreach (IGorgon2DCompositorPass pass in _compositor)
            {
                pass.Enabled = false;
            }
            _compositor["Rendering Pass"].Enabled = true;
        }

        /// <summary>
        /// Function to initialize the application.
        /// </summary>
        /// <returns>The main window for the application.</returns>
        private static FormMain Initialize()
        {
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(Settings.Default.ResourceLocation);
            FormMain window = GorgonExample.Initialize(new DX.Size2(Settings.Default.Resolution.Width, Settings.Default.Resolution.Height), "Post Processing");

            try
            {
                IReadOnlyList<IGorgonVideoAdapterInfo> videoDevices = GorgonGraphics.EnumerateAdapters(log: GorgonApplication.Log);

                if (videoDevices.Count == 0)
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                                              "Gorgon requires at least a Direct3D 11.4 capable video device.\nThere is no suitable device installed on the system.");
                }

                // Find the best video device.
                _graphics = new GorgonGraphics(videoDevices.OrderByDescending(item => item.FeatureSet).First());

                _screen = new GorgonSwapChain(_graphics,
                                              window,
                                              new GorgonSwapChainInfo("Gorgon2D Effects Example Swap Chain")
                                              {
                                                  Width = Settings.Default.Resolution.Width,
                                                  Height = Settings.Default.Resolution.Height,
                                                  Format = BufferFormat.R8G8B8A8_UNorm
                                              });

                // Tell the graphics API that we want to render to the "screen" swap chain.
                _graphics.SetRenderTarget(_screen.RenderTargetView);

                // Initialize the renderer so that we are able to draw stuff.
                _renderer = new Gorgon2D(_graphics);

                GorgonExample.LoadResources(_graphics);

                // Load the Gorgon logo to show in the lower right corner.
                var ddsCodec = new GorgonCodecDds();

                // Import the images we'll use in our post process chain.
                if (!LoadImageFiles(ddsCodec))
                {
                    return window;
                }

                // Initialize the effects that we'll use for compositing, and the compositor itself.
                InitializeEffects();

                // Set up our quick and dirty GUI.
                _buttons = new Button[_compositor.Count];
                LayoutGUI();

                // Select a random image to start
                _currentImage = GorgonRandom.RandomInt32(0, _images.Length - 1);

                window.KeyUp += Window_KeyUp;
                window.MouseUp += Window_MouseUp;
                window.MouseMove += Window_MouseMove;
                window.MouseDown += Window_MouseDown;
                window.IsLoaded = true;

                return window;
            }
            finally
            {
                GorgonExample.EndInit();
            }
        }

        /// <summary>
        /// Handles the MouseDown event of the Window control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        private static void Window_MouseDown(object sender, MouseEventArgs e)
        {
            if (_dragButton != null)
            {
                return;
            }

            _dragStart = new DX.Vector2(e.X, e.Y);
        }

        /// <summary>
        /// Handles the MouseMove event of the Window control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        private static void Window_MouseMove(object sender, MouseEventArgs e)
        {
            bool targetAcquired = false;

            for (int i = 0; i < _buttons.Length; ++i)
            {
                Button button = _buttons[i];
                button.State = ButtonState.Normal;

                if ((_dragButton == null) && (!button.Bounds.Contains(e.X, e.Y)))
                {
                    continue;
                }

                if (_dragButton != null)
                {
                    var dragPosition = new DX.RectangleF(e.X - _dragOffset.X, e.Y - _dragOffset.Y, _dragButton.Bounds.Width, _dragButton.Bounds.Height);

                    if ((!button.Bounds.Intersects(dragPosition)) || (targetAcquired))
                    {
                        continue;
                    }

                    targetAcquired = true;
                }

                button.State = ButtonState.Hovered;

                if ((e.Button != MouseButtons.Left) || (_dragButton != null))
                {
                    continue;
                }

                DX.Vector2 diff = new DX.Vector2(e.X, e.Y) - _dragStart;

                if ((diff.X.Abs() < 5) && (diff.Y.Abs() < 5))
                {
                    continue;
                }

                _dragOffset = new DX.Vector2(_dragStart.X - button.Bounds.Left, _dragStart.Y - button.Bounds.Top);
                button.IsDragging = true;
                _dragButton = button;
            }
        }

        /// <summary>
        /// Handles the MouseUp event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private static void Window_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (_dragButton != null)
            {
                var dragPosition = new DX.RectangleF(e.X - _dragOffset.X, e.Y - _dragOffset.Y, _dragButton.Bounds.Width, _dragButton.Bounds.Height);
                int index = -1;
                for (int i = 0; i < _buttons.Length; ++i)
                {
                    Button overButton = _buttons[i];

                    if (!overButton.Bounds.Intersects(dragPosition))
                    {
                        continue;
                    }

                    index = i;
                    break;
                }

                _compositor.MovePass(_dragButton.Pass.Name, index != -1 ? index : _buttons.Length);

                _dragButton.IsDragging = false;
                _dragButton = null;
                _dragOffset = DX.Vector2.Zero;
                _dragStart = DX.Vector2.Zero;

                LayoutGUI();
                return;
            }

            for (int i = 0; i < _buttons.Length; ++i)
            {
                Button button = _buttons[i];
                if (!button.Bounds.Contains(e.X, e.Y))
                {
                    continue;
                }

                button.PerformClick();
            }
        }

        /// <summary>
        /// Handles the KeyUp event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private static void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    GorgonApplication.Quit();
                    break;
                case Keys.Right:
                    ++_currentImage;

                    if (_currentImage >= _images.Length)
                    {
                        _currentImage = 0;
                    }
                    break;
                case Keys.Left:
                    --_currentImage;

                    if (_currentImage < 0)
                    {
                        _currentImage = _images.Length - 1;
                    }
                    break;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                GorgonApplication.Run(Initialize(), Idle);
            }
            catch (Exception ex)
            {
                GorgonExample.HandleException(ex);
            }
            finally
            {
                GorgonExample.UnloadResources();

                if (_images != null)
                {
                    for (int i = 0; i < _images.Length; ++i)
                    {
                        _images[i]?.Dispose();
                    }
                }

                if (_compositor != null)
                {
                    _blurEffect?.Dispose();        
                    _grayScaleEffect?.Dispose();
                    _posterizeEffect?.Dispose();
                    _1BitEffect?.Dispose();
                    _burnEffect?.Dispose();
                    _dodgeEffect?.Dispose();
                    _invertEffect?.Dispose();
                    _sharpenEffect?.Dispose();
                    _embossEffect?.Dispose();
                    _sobelEffect?.Dispose();
                    _oldFilmEffect?.Dispose();
                    _bloomEffect?.Dispose();
                    _chromatic?.Dispose();
                    _compositor.Dispose();
                }

                _renderer?.Dispose();
                _screen?.Dispose();
                _graphics?.Dispose();
            }
        }
        #endregion
    }
}
