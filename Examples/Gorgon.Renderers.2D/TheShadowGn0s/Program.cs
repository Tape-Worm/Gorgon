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
// Created: August 25, 2018 10:57:09 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Renderers;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// The entry point for the example application.
    /// </summary>
    static class Program
    {
        #region Constants.
        // Text to display for "help".
        private const string HelpText = @"Help:
F1 - Show/hide this text.
S - Show frame stats.
Click - move other ship into foreground.
Mousewheel - blur/sharpen background.
ESC - Quit.";
        #endregion

        #region Variables.
        // The graphics interface.
        private static GorgonGraphics _graphics;
        // Our swap chain representing our "screen".
        private static GorgonSwapChain _screen;
        // The 2D renderer.
        private static Gorgon2D _renderer;
        // The texture for our background.
        private static GorgonTexture2DView _backgroundTexture;
        // The sprite sheet texture.
        private static GorgonTexture2DView _spriteTexture;
        // The sprites.
        private static GorgonSprite _sprite1;
        private static GorgonSprite _sprite2;
        // Background and foreground sprites.
        private static GorgonSprite _bgSprite;
        private static GorgonSprite _fgSprite;
        // The render target representing a layer.
        private static GorgonRenderTarget2DView _layer1Target;
        private static GorgonTexture2DView _layer1Texture;
        // The render target for our blurred image.
        private static GorgonRenderTarget2DView _blurTarget;
        private static GorgonTexture2DView _blurTexture;
        // Shadow texture and sprites.
        private static GorgonTexture2DView _shadowTexture;
        private static GorgonSprite[] _shadowSprites;
        // Our blurring effect for drawing the soft shadows.
        private static Gorgon2DGaussBlurEffect _gaussBlur;
        // The number of times to blur the background.
        private static int _bgBlurAmount;
        // Blend state for our layer RTV.
        private static Gorgon2DBatchState _rtvBlendState;
        // Font for our help text.
        private static GorgonFont _helpFont;
        // Flag to indicate that the help text should be visible.
        private static bool _showHelp = true;
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>
        /// Function to draw the lower layer.
        /// </summary>
        private static void DrawLayer1()
        {
            GorgonSprite shadowSprite = _bgSprite == _sprite2 ? _shadowSprites[1] : _shadowSprites[0];

            _layer1Target.Clear(GorgonColor.BlackTransparent);
            _graphics.SetRenderTarget(_layer1Target);

            _renderer.Begin(_rtvBlendState);

            // Background sprites should be smaller.
            _bgSprite.Scale = new DX.Vector2(0.5f, 0.5f);
            shadowSprite.Scale = new DX.Vector2(0.5f, 0.5f);

            shadowSprite.Position = _bgSprite.Position +
                                    (new DX.Vector2(_bgSprite.Position.X - (_screen.Width / 2.0f), _bgSprite.Position.Y - (_screen.Height / 2.0f)) * _bgSprite.Scale * 0.075f);

            var bgRegion = new DX.RectangleF(0, 0, _screen.Width, _screen.Height);
            _renderer.DrawFilledRectangle(bgRegion,
                                          GorgonColor.White,
                                          _backgroundTexture,
                                          new DX.RectangleF(0,
                                                            0,
                                                            (float)_screen.Width / _backgroundTexture.Width,
                                                            (float)_screen.Height / _backgroundTexture.Height),
                                          textureSampler: GorgonSamplerState.PointFilteringWrapping);
            _renderer.DrawSprite(shadowSprite);
            _renderer.DrawSprite(_bgSprite);

            _renderer.End();
        }

        /// <summary>
        /// Function to draw the background as a blurred image.
        /// </summary>
        private static void DrawBlurredBackground()
        {
            DrawLayer1();

            _graphics.SetRenderTarget(_blurTarget);

            // Send the background layer to our blur target so we have an initial image to blur.
            _renderer.Begin();
            _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _blurTexture.Width, _blurTexture.Height),
                                          GorgonColor.White,
                                          _layer1Texture,
                                          new DX.RectangleF(0, 0, 1, 1));
            _renderer.End();

            // If we have no blurring, then we are done.
            if (_bgBlurAmount == 0)
            {
                return;
            }

            // Otherwise, run the blur effect on the layer multiple times.
            // 
            // Note: In this example, we are blurring many times each frame. This is incredibly inefficient. In a real application we 
            //       should re-blur when the blur amount has changed.  This way we end up having to send out fewer draw calls/frame.
            for (int i = 0; i < _bgBlurAmount; ++i)
            {
                _gaussBlur.Render(_blurTexture, _blurTarget);
            }
        }

        /// <summary>
        /// Function to do perform processing for the application during idle time.
        /// </summary>
        /// <returns><b>true</b> to continue execution, <b>false</b> to stop.</returns>
        private static bool Idle()
        {
            GorgonSprite shadowSprite = _fgSprite == _sprite2 ? _shadowSprites[1] : _shadowSprites[0];

            // Draw our background that includes our background texture, and the sprite that's currently in the background (along with its shadow).
            // Blurring may or may not be applied depending on whether the user has applied it with the mouse wheel.
            DrawBlurredBackground();

            // Reset scales for our sprites. Foreground sprites will be larger than our background ones.
            shadowSprite.Scale = _fgSprite.Scale = DX.Vector2.One;

            // Ensure we're on the "screen" when we render.
            _graphics.SetRenderTarget(_screen.RenderTargetView);

            _renderer.Begin();

            // Draw our blurred (or not) background.
            _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _screen.Width, _screen.Height),
                                          GorgonColor.White,
                                          _blurTexture,
                                          new DX.RectangleF(0, 0, 1, 1));

            // Draw an ellipse to indicate our light source.
            var lightPosition = new DX.RectangleF((_screen.Width / 2.0f) - 10, (_screen.Height / 2.0f) - 10, 20, 20);
            _renderer.DrawFilledEllipse(lightPosition, GorgonColor.White, 0.5f);

            // Draw the sprite and its corresponding shadow.
            // We'll adjust the shadow position to be altered by our distance from the light source, and the quadrant of the screen that we're in.
            shadowSprite.Position = _fgSprite.Position + (new DX.Vector2(_fgSprite.Position.X - (_screen.Width / 2.0f), _fgSprite.Position.Y - (_screen.Height / 2.0f)) * 0.125f);

            _renderer.DrawSprite(shadowSprite);
            _renderer.DrawSprite(_fgSprite);

            if (_showHelp)
            {
                _renderer.DrawString(HelpText, new DX.Vector2(2, 2), _helpFont, GorgonColor.White);
            }

            _renderer.End();

            GorgonExample.DrawStatsAndLogo(_renderer);

            _screen.Present(1);

            return true;
        }

        /// <summary>
        /// Function to build our render targets and textures.
        /// </summary>
        /// <param name="size">The size of the render targets.</param>
        private static void BuildRenderTargets(DX.Size2 size)
        {
            _layer1Target = GorgonRenderTarget2DView.CreateRenderTarget(_graphics,
                                                                        new GorgonTexture2DInfo("Layer 1")
                                                                        {
                                                                            Width = size.Width,
                                                                            Height = size.Height,
                                                                            Format = BufferFormat.R8G8B8A8_UNorm,
                                                                            Binding = TextureBinding.ShaderResource,
                                                                            Usage = ResourceUsage.Default
                                                                        });
            _blurTarget = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo(_layer1Target, "Blurred Image"));

            _layer1Texture = _layer1Target.GetShaderResourceView();
            _blurTexture = _blurTarget.GetShaderResourceView();
        }

        /// <summary>
        /// Function to initialize the example.
        /// </summary>
        /// <returns>The main window for the application.</returns>
        private static FormMain Initialize()
        {
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(Settings.Default.ResourceLocation);
            GorgonExample.ShowStatistics = false;

            FormMain window = GorgonExample.Initialize(new DX.Size2(Settings.Default.Resolution.Width, Settings.Default.Resolution.Height), "The Shadow Gn0s");

            try
            {
                // Create our primary graphics interface.
                IReadOnlyList<IGorgonVideoAdapterInfo> adapters = GorgonGraphics.EnumerateAdapters(log: GorgonApplication.Log);

                if (adapters.Count == 0)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, "This example requires a Direct3D 11.4 capable video card.\nThe application will now close.");
                }

                _graphics = new GorgonGraphics(adapters[0], log: GorgonApplication.Log);

                // Create our "screen".
                _screen = new GorgonSwapChain(_graphics, window, new GorgonSwapChainInfo("TheShadowGn0s Screen Swap chain")
                {
                    Width = Settings.Default.Resolution.Width,
                    Height = Settings.Default.Resolution.Height,
                    Format = BufferFormat.R8G8B8A8_UNorm
                });

                BuildRenderTargets(new DX.Size2(_screen.Width, _screen.Height));


                _backgroundTexture = GorgonTexture2DView.FromFile(_graphics,
                                                                  Path.Combine(GorgonExample.GetResourcePath(@"Textures\TheShadowGn0s\").FullName,
                                                                               "VBBack.jpg"),
                                                                  new GorgonCodecJpeg(),
                                                                  new GorgonTexture2DLoadOptions
                                                                  {
                                                                      Name = "Background Texture",
                                                                      Binding = TextureBinding.ShaderResource,
                                                                      Usage = ResourceUsage.Immutable
                                                                  });

                // Create our 2D renderer.
                _renderer = new Gorgon2D(_graphics);

                _spriteTexture = GorgonTexture2DView.FromFile(_graphics,
                                                              Path.Combine(GorgonExample.GetResourcePath(@"Textures\TheShadowGn0s\").FullName,
                                                                           "0_HardVacuum.png"),
                                                              new GorgonCodecPng(),
                                                              new GorgonTexture2DLoadOptions
                                                              {
                                                                  Name = "/Images/0_HardVacuum.png",
                                                                  Binding = TextureBinding.ShaderResource,
                                                                  Usage = ResourceUsage.Immutable
                                                              });

                var spriteCodec = new GorgonV2SpriteCodec(_renderer);
                _sprite1 = spriteCodec.FromFile(Path.Combine(GorgonExample.GetResourcePath(@"Sprites\TheShadowGn0s\").FullName, "Mother.gorSprite"));
                _sprite2 = spriteCodec.FromFile(Path.Combine(GorgonExample.GetResourcePath(@"Sprites\TheShadowGn0s\").FullName, "Mother2c.gorSprite"));

                _gaussBlur = new Gorgon2DGaussBlurEffect(_renderer, 9)
                {
                    BlurRenderTargetsSize = new DX.Size2(_screen.Width / 2, _screen.Height / 2)
                };

                var shadowBuilder = new ShadowBuilder(_renderer, _gaussBlur, _sprite1, _sprite2);
                (GorgonSprite[] shadowSprites, GorgonTexture2DView shadowTexture) = shadowBuilder.Build();
                _shadowSprites = shadowSprites;
                _shadowTexture = shadowTexture;

                var batchStateBuilder = new Gorgon2DBatchStateBuilder();
                var blendStateBuilder = new GorgonBlendStateBuilder();
                _rtvBlendState = batchStateBuilder
                                 .BlendState(blendStateBuilder
                                             .ResetTo(GorgonBlendState.Default)
                                             .DestinationBlend(alpha: Blend.InverseSourceAlpha))
                                 .Build();

                _sprite2.Position = new DX.Vector2((int)(_screen.Width / 2.0f), (int)(_screen.Height / 4.0f));
                _sprite1.Position = new DX.Vector2((int)(_screen.Width / 4.0f), (int)(_screen.Height / 5.0f));

                _bgSprite = _sprite2;
                _fgSprite = _sprite1;

                _screen.BeforeSwapChainResized += (sender, args) =>
                                                  {
                                                      _blurTexture?.Dispose();
                                                      _blurTarget?.Dispose();
                                                      _layer1Texture?.Dispose();
                                                      _layer1Target?.Dispose();
                                                  };

                window.MouseMove += Window_MouseMove;
                window.MouseUp += Window_MouseUp;
                window.MouseWheel += Window_MouseWheel;
                window.KeyUp += Window_KeyUp;

                _screen.AfterSwapChainResized += (sender, args) => BuildRenderTargets(args.Size);

                GorgonExample.LoadResources(_graphics);

                _helpFont = GorgonExample.Fonts.GetFont(new GorgonFontInfo("Segoe UI", 12.0f, FontHeightMode.Points, "Segoe UI 12pt Bold, Outlined")
                {
                    FontStyle = FontStyle.Bold,
                    OutlineColor2 = GorgonColor.Black,
                    OutlineColor1 = GorgonColor.Black,
                    OutlineSize = 2,
                    TextureWidth = 512,
                    TextureHeight = 256
                });

                return window;
            }
            finally
            {
                GorgonExample.EndInit();
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
                case Keys.F1:
                    _showHelp = !_showHelp;
                    break;
                case Keys.S:
                    GorgonExample.ShowStatistics = !GorgonExample.ShowStatistics;
                    break;
                case Keys.Escape:
                    GorgonApplication.Quit();
                    break;
            }
        }

        /// <summary>
        /// Handles the MouseWheel event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private static void Window_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                _bgBlurAmount++;
            }
            else
            {
                _bgBlurAmount--;
            }

            if (_bgBlurAmount > 10)
            {
                _bgBlurAmount = 10;
            }

            if (_bgBlurAmount < 0)
            {
                _bgBlurAmount = 0;
            }
        }

        /// <summary>
        /// Handles the MouseUp event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private static void Window_MouseUp(object sender, MouseEventArgs e)
        {
            _bgSprite.Position = new DX.Vector2(e.X, e.Y);
            _fgSprite.Position = new DX.Vector2(e.X, e.Y);

            if (_bgSprite == _sprite2)
            {
                _bgSprite = _sprite1;
                _fgSprite = _sprite2;
            }
            else
            {
                _bgSprite = _sprite2;
                _fgSprite = _sprite1;
            }
        }

        /// <summary>
        /// Handles the MouseMove event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private static void Window_MouseMove(object sender, MouseEventArgs e) => _fgSprite.Position = new DX.Vector2(e.X, e.Y);
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                GorgonApplication.Run(Initialize(), Idle);
            }
            catch (Exception ex)
            {
                GorgonExample.HandleException(ex);
            }
            finally
            {
                GorgonExample.UnloadResources();

                _helpFont?.Dispose();
                _shadowTexture?.Dispose();
                _gaussBlur?.Dispose();
                _spriteTexture?.Dispose();
                _backgroundTexture?.Dispose();
                _renderer?.Dispose();
                _blurTexture?.Dispose();
                _blurTarget?.Dispose();
                _layer1Texture?.Dispose();
                _layer1Target?.Dispose();
                _screen?.Dispose();
                _graphics?.Dispose();
            }
        }
        #endregion
    }
}
