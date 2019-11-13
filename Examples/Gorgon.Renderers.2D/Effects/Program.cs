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
// Created: July 17, 2018 3:12:16 PM
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
using DX = SharpDX;


namespace Gorgon.Examples
{
    /// <summary>
    /// Our example entry point.
    /// </summary>
    static class Program
    {
        #region Constants.
        // Help text.
        private const string HelpText = "F1 - Show/hide the help text.\nC - Cloak or uncloak the ship\nMouse wheel - Rotate the ship.\nEscape - Close this example.";
        #endregion

        #region Variables.
        // Our 2D renderer.
        private static Gorgon2D _renderer;
        // The primary graphics interface.
        private static GorgonGraphics _graphics;
        // Our swap chain representing our screen.
        private static GorgonSwapChain _screen;
        // The background texture.
        private static GorgonTexture2DView _background;
        // The space ship texture.
        private static GorgonTexture2DView _spaceShipTexture;
        // The sprite used for our space ship.
        private static GorgonSprite _shipSprite;
        // The offset for the background texture coordinates.
        private static DX.Vector2 _backgroundOffset;
        // The random value for the background texture offset.
        private static DX.Vector2? _randomOffset;
        // The effect used for displacement.
        private static Gorgon2DDisplacementEffect _displacement;
        // Old film effect - to make it look like the pulp serial films from ye olden days.
        private static Gorgon2DOldFilmEffect _oldFilm;
        // Gaussian blur effect.
        private static Gorgon2DGaussBlurEffect _gaussBlur;
        // The texture view for the 1st post process render target.
        private static GorgonTexture2DView _postView1;
        // The 1st post process render target 
        private static GorgonRenderTarget2DView _postTarget1;
        // The 2nd post process render target.
        private static GorgonRenderTarget2DView _postTarget2;
        // The texture view for the 2nd post process render target.
        private static GorgonTexture2DView _postView2;
        // The final process render target.
        private static GorgonRenderTarget2DView _finalTarget;
        // The texture view for the final post process render target.
        private static GorgonTexture2DView _finalView;
        // Flag to indicate that the help text should be shown.
        private static bool _showHelp = true;
        // The cloaking animation controller.
        private static readonly CloakController _cloakController = new CloakController();
        // Final render target brightness.
        private static float _finalBrightness = 1.0f;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to draw the displacement effect.
        /// </summary>
        /// <param name="passIndex">The current pass index.</param>
        private static void DrawDisplacement(int passIndex)
        {
            switch (passIndex)
            {
                // For this effect, we first draw whatever items we wish to use for displacing the image data.
                case 0:
                    
                    break;
                // Then, we draw the background that will have the displaced pixels.
                case 1:
                    DrawDisplacementBackground();
                    break;
            }

            // Function to draw the sprite that will displace the background image.
            void DrawDisplacementSprite()
            {
            }

            // Function to draw the displacement background.
            void DrawDisplacementBackground()
            {
                _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _postView1.Width, _postView1.Height),
                                              GorgonColor.White,
                                              _postView1,
                                              new DX.RectangleF(0, 0, 1, 1));
            }
        }

        /// <summary>
        /// Function to perform operations while the CPU is idle.
        /// </summary>
        /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
        private static bool Idle()
        {
            _postTarget1.Clear(GorgonColor.Black);

            DX.Vector2 textureSize = _background.Texture.ToTexel(new DX.Vector2(_postTarget1.Width, _postTarget1.Height));

            // Blit the background texture.
            _graphics.SetRenderTarget(_postTarget1);
            _renderer.Begin();
            _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _postTarget1.Width, _postTarget1.Height),
                                          GorgonColor.White,
                                          _background,
                                          new DX.RectangleF(_backgroundOffset.X, _backgroundOffset.Y, textureSize.X, textureSize.Y));
            _shipSprite.Color = new GorgonColor(GorgonColor.White, _cloakController.Opacity);
            _renderer.DrawSprite(_shipSprite);
            _renderer.End();

            // No sense in rendering the effect if it's not present.
            float strength = _cloakController.CloakAmount;

            if (strength > 0.0f)
            {                
                _graphics.SetRenderTarget(_postTarget2);

                _displacement.Strength = strength;
                _displacement.Begin(_postView1);
                _shipSprite.Color = GorgonColor.White;
                _renderer.DrawSprite(_shipSprite);
                _displacement.End();
            }
            else
            {
                // Send the undisplaced image to the 2nd post process target.
                _graphics.SetRenderTarget(_postTarget2);

                _renderer.Begin();
                _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _postTarget1.Width, _postTarget1.Height),
                                              GorgonColor.White,
                                              _postView1,
                                              new DX.RectangleF(0, 0, 1, 1));
                _renderer.End();
            }

            // Smooth our results.
            int blurRadiusUpdate = GorgonRandom.RandomInt32(0, 1000000);
            if (blurRadiusUpdate > 997000)
            {
                _gaussBlur.BlurRadius = (_gaussBlur.BlurRadius + 1).Min(_gaussBlur.MaximumBlurRadius / 2);
            }
            else if (blurRadiusUpdate < 3000)
            {
                _gaussBlur.BlurRadius = (_gaussBlur.BlurRadius - 1).Max(1);
            }

            // If we didn't blur (radius = 0), then just use the original view.
            if (_gaussBlur.BlurRadius > 0)
            {
                _gaussBlur.Render(_postView2, _postTarget2);
            }

            // Render as an old film effect.
            _oldFilm.Time = GorgonTiming.SecondsSinceStart * 2;
            _oldFilm.ShakeOffset = DX.Vector2.Zero;
            if (GorgonRandom.RandomInt32(0, 100) > 95)
            {
                _oldFilm.ShakeOffset = new DX.Vector2(GorgonRandom.RandomSingle(-2.0f, 2.0f), GorgonRandom.RandomSingle(-1.5f, 1.5f));
            }

            _oldFilm.Render(_postView2, _postTarget1);

            // Send to our screen.
            _screen.RenderTargetView.Clear(GorgonColor.Black);
            _graphics.SetRenderTarget(_screen.RenderTargetView);

            _renderer.Begin(Gorgon2DBatchState.NoBlend);
            if (GorgonRandom.RandomInt32(0, 100) < 2)
            {
                _finalBrightness = GorgonRandom.RandomSingle(0.65f, 1.0f);
            }

            _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _finalView.Width, _finalView.Height),
                                          new GorgonColor(_finalBrightness, _finalBrightness, _finalBrightness, 1.0f),
                                          _postView1,
                                          new DX.RectangleF(0, 0, 1, 1));
            _renderer.End();

            _renderer.Begin();
            if (_showHelp)
            {
                _renderer.DrawString(HelpText, new DX.Vector2(0, 64), color: new GorgonColor(1.0f, 1.0f, 0));
            }
            _renderer.End();

            GorgonExample.DrawStatsAndLogo(_renderer);

            // Flip our back buffers over.
            _screen.Present(1);

            _cloakController.Update();

            return true;
        }

        /// <summary>
        /// Function to initialize the background texture offset.
        /// </summary>
        private static void InitializeBackgroundTexturePositioning()
        {
            if (_randomOffset == null)
            {
                _randomOffset = new DX.Vector2(GorgonRandom.RandomSingle(_background.Width - _postTarget1.Width),
                                               GorgonRandom.RandomSingle(_background.Height - _postTarget1.Height));
            }

            // If we're previously out of frame, then push back until we're in frame.
            if (_randomOffset.Value.X + _postTarget1.Width > _background.Width)
            {
                _randomOffset = new DX.Vector2(_randomOffset.Value.X - _postTarget1.Width, _randomOffset.Value.Y);
            }

            if (_randomOffset.Value.Y + _postTarget1.Height > _background.Height)
            {
                _randomOffset = new DX.Vector2(_randomOffset.Value.X, _randomOffset.Value.Y - _postTarget1.Height);
            }

            if (_randomOffset.Value.X < 0)
            {
                _randomOffset = new DX.Vector2(0, _randomOffset.Value.Y);
            }

            if (_randomOffset.Value.Y < 0)
            {
                _randomOffset = new DX.Vector2(_randomOffset.Value.X, 0);
            }

            // Convert to texels.
            _backgroundOffset = _background.Texture.ToTexel(_randomOffset.Value);
        }

        /// <summary>
        /// Handles the AfterSwapChainResized event of the Screen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AfterSwapChainResizedEventArgs"/> instance containing the event data.</param>
        private static void Screen_AfterSwapChainResized(object sender, AfterSwapChainResizedEventArgs e)
        {
            BuildRenderTargets();
            InitializeBackgroundTexturePositioning();

            _gaussBlur.BlurRenderTargetsSize = new DX.Size2(_screen.Width / 2, _screen.Height / 2);
        }

        /// <summary>
        /// Function to build up the render targets for the application.
        /// </summary>
        private static void BuildRenderTargets()
        {
            _postTarget1 = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo("Post process render target #1")
            {
                Width = _screen.Width,
                Height = _screen.Height,
                Format = _screen.Format
            });
            _postView1 = _postTarget1.GetShaderResourceView();

            _postTarget2 = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo(_postTarget1, "Post process render target #2"));
            _postView2 = _postTarget2.GetShaderResourceView();

            _finalTarget = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo(_postTarget1, "Final post process render target."));
            _finalView = _finalTarget.GetShaderResourceView();
        }

        /// <summary>
        /// Function to initialize the application.
        /// </summary>
        /// <returns>The main window for the application.</returns>
        private static FormMain Initialize()
        {
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(Settings.Default.ResourceLocation);
            FormMain window = GorgonExample.Initialize(new DX.Size2(Settings.Default.Resolution.Width, Settings.Default.Resolution.Height), "Effects");

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
                _screen.BeforeSwapChainResized += Screen_BeforeSwapChainResized;
                _screen.AfterSwapChainResized += Screen_AfterSwapChainResized;
                // Tell the graphics API that we want to render to the "screen" swap chain.
                _graphics.SetRenderTarget(_screen.RenderTargetView);

                // Initialize the renderer so that we are able to draw stuff.
                _renderer = new Gorgon2D(_graphics);

                // Create the displacement effect used for the "cloaking" effect.
                _displacement = new Gorgon2DDisplacementEffect(_renderer)
                {
                    Strength = 0
                };

                // Create the old film effect for that old timey look.
                _oldFilm = new Gorgon2DOldFilmEffect(_renderer)
                {
                    ScrollSpeed = 0.05f
                };

                // Create the gaussian blur effect for that "soft" look.
                _gaussBlur = new Gorgon2DGaussBlurEffect(_renderer, 9)
                {
                    BlurRenderTargetsSize = new DX.Size2(_screen.Width / 2, _screen.Height / 2),
                    BlurRadius = 1
                };
                // The higher # of taps on the blur shader will introduce a stutter on first render, so precache its setup data.
                _gaussBlur.Precache();

                // Load our texture with our space background in it.
                _background = GorgonTexture2DView.FromFile(_graphics,
                                                           Path.Combine(GorgonExample.GetResourcePath(@"Textures\").FullName, "HotPocket.dds"),
                                                           new GorgonCodecDds(),
                                                           new GorgonTexture2DLoadOptions
                                                           {
                                                               Usage = ResourceUsage.Immutable,
                                                               Binding = TextureBinding.ShaderResource
                                                           });

                // Load up our super space ship image.
                _spaceShipTexture = GorgonTexture2DView.FromFile(_graphics,
                                                                 Path.Combine(GorgonExample.GetResourcePath(@"Textures\Effects\").FullName, "ship.png"),
                                                                 new GorgonCodecPng(),
                                                                 new GorgonTexture2DLoadOptions
                                                                 {
                                                                     Usage = ResourceUsage.Immutable,
                                                                     Binding = TextureBinding.ShaderResource
                                                                 });
                _shipSprite = new GorgonSprite
                {
                    Texture = _spaceShipTexture,
                    TextureRegion = new DX.RectangleF(0, 0, 1, 1),
                    Anchor = new DX.Vector2(0.5f, 0.5f),
                    Position = new DX.Vector2(_screen.Width / 2.0f, _screen.Height / 2.0f),
                    Size = new DX.Size2F(_spaceShipTexture.Width, _spaceShipTexture.Height)
                };

                BuildRenderTargets();
                InitializeBackgroundTexturePositioning();

                GorgonExample.LoadResources(_graphics);

                window.MouseMove += Window_MouseMove;
                window.MouseWheel += Window_MouseWheel;
                window.KeyUp += Window_KeyUp;

                return window;
            }
            finally
            {
                GorgonExample.EndInit();
            }
        }

        /// <summary>
        /// Handles the BeforeSwapChainResized event of the Screen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="BeforeSwapChainResizedEventArgs"/> instance containing the event data.</param>
        private static void Screen_BeforeSwapChainResized(object sender, BeforeSwapChainResizedEventArgs e)
        {
            _postView1?.Dispose();
            _postTarget1?.Dispose();

            _postView2?.Dispose();
            _postTarget2?.Dispose();

            _finalView?.Dispose();
            _finalTarget?.Dispose();
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
                    return;
                case Keys.F1:
                    _showHelp = !_showHelp;
                    return;
            }

            if (e.KeyCode != Keys.C)
            {
                return;
            }

            switch (_cloakController.Direction)
            {
                case CloakDirection.Cloak:
                case CloakDirection.CloakPulse:
                    _cloakController.Uncloak();
                    break;
                case CloakDirection.Uncloak:
                case CloakDirection.UncloakStopPulse:
                case CloakDirection.None:
                    _cloakController.Cloak();
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
                _shipSprite.Angle -= 4.0f;
            }

            if (e.Delta > 0)
            {
                _shipSprite.Angle += 4.0f;
            }

            if (_shipSprite.Angle > 360.0f)
            {
                _shipSprite.Angle = 0.0f;
            }

            if (_shipSprite.Angle < 0.0f)
            {
                _shipSprite.Angle = 360.0f;
            }
        }

        /// <summary>
        /// Handles the MouseMove event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private static void Window_MouseMove(object sender, MouseEventArgs e) => _shipSprite.Position = new DX.Vector2(e.X, e.Y);

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

                _gaussBlur?.Dispose();
                _oldFilm?.Dispose();
                _displacement?.Dispose();
                _finalView?.Dispose();
                _finalTarget?.Dispose();
                _postView2?.Dispose();
                _postTarget2?.Dispose();
                _postView1?.Dispose();
                _postTarget1?.Dispose();
                _background?.Dispose();
                _renderer?.Dispose();
                _screen?.Dispose();
                _graphics?.Dispose();
            }
        }
        #endregion
    }
}
