#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: August 16, 2020 11:22:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Renderers;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// Our example entry point.
    /// </summary>
    static class Program
    {
        #region Variables.
        // The core graphics functionality.
        private static GorgonGraphics _graphics;
        // Our swap chain that represents our "Screen".
        private static GorgonSwapChain _screen;
        // Our 2D renderer used to draw our sprites.
        private static Gorgon2D _renderer;
        // The texture for the ship and engine glow.
        private static GorgonTexture2DView _circleTexture;
        // The ship sprite.
        private static GorgonSprite _particleSprite;
        private static ParticleEmitter _emitter;
        // Render target for our unbloomed image.
        private static GorgonRenderTarget2DView _rtv;
        private static GorgonTexture2DView _srv;
        // Bloom effect to add glow to our particles.
        private static Gorgon2DBloomEffect _bloom;
        // Flag to enable/disable bloom.
        private static bool _bloomDisabled;
        // Flag to show help for the example.
        private static bool _showHelp = true;
        // Particles emit an explosion effect.
        private static bool _explosion = true;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the emitter to change the particle effect.
        /// </summary>
        private static void UpdateEmitter()
        {
            if (_explosion)
            {
                _emitter.ParticleLifetimeRange = (5.0f, 5.0f);
                _emitter.ParticleSizeRange = (1.0f, 0.01f);
                _emitter.ParticleSpeedRange = (300, 300);
                _emitter.RadialAccelerationRange = (0, 0);
                _emitter.Spread = 360.0f;
                _emitter.Relative = false;
                _emitter.Reset();
                return;
            }

            _emitter.Relative = true;
            _emitter.Spread = 10.0f;
            _emitter.ParticleLifetimeRange = (0.5f, 0.0f);
            _emitter.ParticleSizeRange = (0.5f, 0.007f);
            _emitter.ParticleSpeedRange = (0, 800);
            _emitter.RadialAccelerationRange = (-1, 1);
            _emitter.Reset();
        }
        
        /// <summary>
        /// Function to build the render target for blooming.
        /// </summary>
        private static void BuildRenderTarget()
        {
            _srv?.Dispose();
            _rtv?.Dispose();

            _rtv = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo(_screen.RenderTargetView)
            {
                Name = "Bloom_RTV",
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Default
            });
            _srv = _rtv.GetShaderResourceView();
        }

        /// <summary>
        /// Function called during CPU idle time.
        /// </summary>
        /// <returns><b>true</b> to continue executing, <b>false</b> to stop.</returns>
        private static bool Idle()
        {
            _emitter.Update();

            _rtv.Clear(GorgonColor.Black);

            // Draw our particles into the render target.
            _graphics.SetRenderTarget(_rtv);
            _renderer.Begin(Gorgon2DBatchState.AdditiveBlend);
            _emitter.Draw();
            _renderer.End();

            // Draw the render target to the main swap chain.
            if (!_bloomDisabled)
            {
                _bloom.Render(_srv, _screen.RenderTargetView);
            }
            else
            {
                _graphics.SetRenderTarget(_screen.RenderTargetView);

                _renderer.Begin(Gorgon2DBatchState.NoBlend);
                _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _srv.Width, _srv.Height), GorgonColor.White, _srv, new DX.RectangleF(0, 0, 1, 1));
                _renderer.End();
            }

            if (_showHelp)
            {
                _renderer.Begin();
                _renderer.DrawString("Example help:\nF1 - Show/hide help.\nF2 - Change emitter type.\nSpace - Pause/unpause.\nLeft Mouse Button - Restart emitter at mouse cursor.\nRight Mouse Button (while moving cursor) - Drag emitter.\nMiddle Mouse Button - Enable/Disable bloom effect.", new Vector2(0, 72), color: GorgonColor.YellowPure);
                _renderer.End();
            }

            GorgonExample.DrawStatsAndLogo(_renderer);

            _screen.Present(1);

            return true;
        }

        /// <summary>
        /// Function to initialize the graphics objects and the primary window.
        /// </summary>
        /// <returns>The form that will display our graphical scene.</returns>
        private static FormMain Initialize()
        {
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);

            // Create the window, and size it to our resolution.
            FormMain window = GorgonExample.Initialize(new DX.Size2(ExampleConfig.Default.Resolution.Width, ExampleConfig.Default.Resolution.Height), "Particles");

            try
            {
                IReadOnlyList<IGorgonVideoAdapterInfo> videoDevices = GorgonGraphics.EnumerateAdapters(log: GorgonApplication.Log);

                if (videoDevices.Count == 0)
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                                              "Gorgon requires at least a Direct3D 11.2 capable video device.\nThere is no suitable device installed on the system.");
                }

                // Find the best video device.
                _graphics = new GorgonGraphics(videoDevices.OrderByDescending(item => item.FeatureSet).First());

                _screen = new GorgonSwapChain(_graphics,
                                              window,
                                              new GorgonSwapChainInfo(ExampleConfig.Default.Resolution.Width,
                                                                           ExampleConfig.Default.Resolution.Height,
                                                                           BufferFormat.R8G8B8A8_UNorm)
                                              {
                                                  Name = "Gorgon2D Sprites Example Swap Chain"
                                              });

                // Tell the graphics API that we want to render to the "screen" swap chain.
                _graphics.SetRenderTarget(_screen.RenderTargetView);

                // Create our target.
                BuildRenderTarget();

                // Initialize the renderer so that we are able to draw stuff.
                _renderer = new Gorgon2D(_graphics);

                // Get our particle texture.
                _circleTexture = GorgonTexture2DView.FromFile(_graphics,
                                                            Path.Combine(GorgonExample.GetResourcePath(@"Textures\Particles\").FullName, "circle.dds"),
                                                            new GorgonCodecDds(),
                                                            new GorgonTexture2DLoadOptions
                                                            {
                                                                Usage = ResourceUsage.Immutable
                                                            });

                // This is the sprite we'll use for each particle.
                _particleSprite = new GorgonSprite
                {
                    Bounds = new DX.RectangleF(0, 0, _circleTexture.Width, _circleTexture.Height),
                    Texture = _circleTexture,
                    // Calculate the ship texture coordinates.
                    TextureRegion = new DX.RectangleF(0, 0, 1, 1),
                    Anchor = new Vector2(0.5f, 0.5f)
                };

                // Create a new emitter that we can move around.
                _emitter = new ParticleEmitter(_renderer, _particleSprite, new Vector2(_screen.Width * 0.5f, _screen.Height * 0.5f));
                UpdateEmitter();

                // Create a bloom filter to intensify the glow for the particles.
                _bloom = new Gorgon2DBloomEffect(_renderer)
                {
                    BloomIntensity = 3.125f,
                    BlurAmount = 4.0f,                    
                    ColorIntensity = 2.0f,
                    Threshold = 1.02f,
                };
                  
                _bloom.Precache();

                GorgonExample.LoadResources(_graphics);

                window.IsLoaded = true;
                window.MouseUp += Window_MouseUp;
                window.MouseMove += Window_MouseMove;
                window.KeyUp += Window_KeyUp;
                _screen.SwapChainResized += Screen_SwapChainResized;

                return window;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>Handles the KeyUp event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private static void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1:
                    _showHelp = !_showHelp;
                    break;
                case Keys.F2:
                    _explosion = !_explosion;
                    UpdateEmitter();
                    break;
                case Keys.Space:
                    _emitter.Paused = !_emitter.Paused;
                    break;
            }
        }

        /// <summary>Handles the SwapChainResized event of the Screen control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SwapChainResizedEventArgs"/> instance containing the event data.</param>
        private static void Screen_SwapChainResized(object sender, SwapChainResizedEventArgs e) => BuildRenderTarget();

        /// <summary>Handles the MouseUp event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private static void Window_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                _bloomDisabled = !_bloomDisabled;
                return;
            }

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            _emitter.Reset();
            _emitter.Move(e.X, e.Y);
        }

        /// <summary>Handles the MouseMove event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private static void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            if (_explosion)
            {
                _emitter.Move(e.X, e.Y);
            }
            else
            {
                _emitter.Position = new Vector2(e.X, e.Y);
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
#if NET5_0_OR_GREATER
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
#endif
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
                _screen.SwapChainResized -= Screen_SwapChainResized;
                GorgonExample.UnloadResources();
                _circleTexture?.Dispose();
                _bloom?.Dispose();
                _renderer?.Dispose();
                _rtv?.Dispose();
                _screen?.Dispose();
                _graphics?.Dispose();
            }
        }
        #endregion
    }
}
