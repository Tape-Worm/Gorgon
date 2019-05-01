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
// Created: July 28, 2018 11:33:52 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI;

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
        // The texture used for the logo.
        private static GorgonTexture2DView _backgroundLogoTexture;
        // The texture used for the torch.
        private static GorgonTexture2DView _torchTexture;
        // The final render target for display.
        private static GorgonRenderTarget2DView _finalTarget;
        // The final texture for display.
        private static GorgonTexture2DView _finalTexture;
        // Our 2D renderer.
        private static Gorgon2D _renderer;
        // Sprite used to draw our logo.
        private static GorgonSprite _logoSprite;
        // Sprite used to draw our logo.
        private static GorgonSprite _torchSprite;
        // Our lighting effect.
        private static Gorgon2DDeferredLightingEffect _lightEffect;
        // Flag to indicate the direction of light brightness.
        private static bool _lightBrightDir = true;
        // The actual light color value.
        private static float _lightValue = 0.5f;
        // The min and max bounds for the lighting values.
        private static readonly GorgonRangeF _lightMinMax = new GorgonRangeF(0.75f, 1.0f);
        // The timer for switching torch frames.
        private static IGorgonTimer _torchFrameTime;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to draw the scene that needs lighting.
        /// </summary>
        /// <param name="pass">Not used.</param>
        /// <param name="passCount">Not used.</param>
        /// <param name="outputSize">The size of the output render target.</param>
        private static void DrawLitScene(int pass, int passCount, DX.Size2 outputSize)
        {
            _logoSprite.Position = new DX.Vector2(outputSize.Width / 2.0f, outputSize.Height / 2.0f);
            _renderer.DrawSprite(_logoSprite);
        }

        /// <summary>
        /// Function called when the application goes into an idle state.
        /// </summary>
        /// <returns><b>true</b> to continue executing, <b>false</b> to stop.</returns>
        private static bool Idle()
        {
            if (_lightBrightDir)
            {
                _lightValue += 1.0f * GorgonTiming.Delta;
            }
            else
            {
                _lightValue -= 1.0f * GorgonTiming.Delta;
            }

            if (_torchFrameTime.Milliseconds > 250)
            {
                _torchSprite.TextureRegion =
                    _torchTexture.Texture.ToTexel(new DX.Rectangle(_torchSprite.TextureRegion.Left == 0 ? 56 : 0, 0, 55, _torchTexture.Height));
                _torchFrameTime.Reset();

                _lightValue = _torchSprite.TextureRegion.Left == 0 ? _lightMinMax.Maximum : _lightMinMax.Minimum;
            }

            if (_lightValue < _lightMinMax.Minimum)
            {
                _lightValue = _lightMinMax.Minimum;
                _lightBrightDir = !_lightBrightDir;
            }

            if (_lightValue > _lightMinMax.Maximum)
            {
                _lightValue = _lightMinMax.Maximum;
                _lightBrightDir = !_lightBrightDir;
            }

            _lightEffect.Lights[0].Color = new GorgonColor((_lightValue * 253.0f) / 255.0f, (_lightValue * 248.0f) / 255.0f,  (_lightValue  * 230.0f) / 255.0f);
            _lightEffect.Lights[0].SpecularPower = (1.0f - (_lightValue / 1.2f)) * 15;

            _finalTarget.Clear(GorgonColor.BlackTransparent);
            _screen.RenderTargetView.Clear(GorgonColor.Black);

            // Render the lit sprite.
            _lightEffect.Render(DrawLitScene, _finalTarget);

            // Blit our final texture to the main screen.
            _graphics.SetRenderTarget(_screen.RenderTargetView);
            _renderer.Begin();
            _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _screen.Width, _screen.Height),
                                          GorgonColor.White,
                                          _finalTexture,
                                          new DX.RectangleF(0, 0, 1, 1));

            _renderer.DrawSprite(_torchSprite);

            _renderer.DrawString($"Specular Power: {_lightEffect.Lights[0].SpecularPower:0.0#####}\nLight [c #{GorgonColor.CornFlowerBlue.ToHex()}]Z[/c]: {_lightEffect.Lights[0].Position.Z:0.0}",
                                 new DX.Vector2(0, 64));
            _renderer.End();

            GorgonExample.DrawStatsAndLogo(_renderer);

            _screen.Present(1);
            return true;
        }

        /// <summary>
        /// Function to update the render target and texture view.
        /// </summary>
        private static void UpdateRenderTarget()
        {
            // This will be our final output target for our effect.
            _finalTarget = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo(_screen.RenderTargetView, "Final Render Target")
                                                                                  {
                                                                                      Binding = TextureBinding.ShaderResource
                                                                                  });
            // And this is the texture we will display at the end of the frame.
            _finalTexture = _finalTarget.GetShaderResourceView();
        }

        /// <summary>
        /// Function to initialize the application.
        /// </summary>
        /// <returns>The main window for the application.</returns>
        private static FormMain Initialize()
        {
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(Settings.Default.ResourceLocation);

            FormMain window = GorgonExample.Initialize(new DX.Size2(Settings.Default.Resolution.Width, Settings.Default.Resolution.Height), "Lights");

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

                // Load in our texture for our logo background.
                _backgroundLogoTexture = GorgonTexture2DView.FromFile(_graphics,
                                                            Path.Combine(GorgonExample.GetResourcePath(@"Textures\Lights\").FullName, "lights.dds"),
                                                            new GorgonCodecDds(),
                                                            new GorgonTexture2DLoadOptions
                                                            {
                                                                Usage = ResourceUsage.Immutable,
                                                                Binding = TextureBinding.ShaderResource,
                                                                Name = "Logo"
                                                            });

                _torchTexture = GorgonTexture2DView.FromFile(_graphics,
                                                             Path.Combine(GorgonExample.GetResourcePath(@"Textures\Lights\").FullName, "Torch.png"),
                                                             new GorgonCodecPng(),
                                                             new GorgonTexture2DLoadOptions
                                                             {
                                                                 Usage = ResourceUsage.Immutable,
                                                                 Binding = TextureBinding.ShaderResource,
                                                                 Name = "Torch"
                                                             });

                UpdateRenderTarget();

                // Initialize the renderer so that we are able to draw stuff.
                _renderer = new Gorgon2D(_graphics);

                _logoSprite = new GorgonSprite
                              {
                                  Texture = _backgroundLogoTexture,
                                  Bounds = new DX.RectangleF(0, 0, _backgroundLogoTexture.Width, _backgroundLogoTexture.Height),
                                  TextureRegion = new DX.RectangleF(0, 0, 1, 1),
                                  Anchor = new DX.Vector2(0.5f, 0.5f)
                              };

                _torchSprite = new GorgonSprite
                               {
                                   Texture = _torchTexture,
                                   Bounds = new DX.RectangleF(0, 0, 55, _torchTexture.Height),
                                   TextureRegion = _torchTexture.Texture.ToTexel(new DX.Rectangle(0, 0, 55, _torchTexture.Height)),
                               };

                // Create the effect that will light up our sprite(s).
                _lightEffect = new Gorgon2DDeferredLightingEffect(_renderer);
                _lightEffect.Lights.Add(new Gorgon2DLight
                                        {
                                            Color = new GorgonColor(0.25f, 0.25f, 0.25f),
                                            Attenuation = 1.0f,
                                            LightType = LightType.Point,
                                            SpecularEnabled = true,
                                            SpecularPower = 5.0f,
                                            Position = new DX.Vector3((_screen.Width / 2.0f) - 150.0f, (_screen.Height / 2.0f) - 150.0f, 100)
                                        });
                _lightEffect.Lights.Add(new Gorgon2DLight
                                        {
                                            Color = GorgonColor.White,
                                            Attenuation = 0.025f,
                                            LightType = LightType.Directional,
                                            SpecularEnabled = false,
                                            Position = new DX.Vector3(0, 0, 200),
                                            LightDirection = new DX.Vector3(1, -1, 1)
                                        });

                GorgonExample.LoadResources(_graphics);
                
                window.MouseMove += Window_MouseMove;
                window.KeyDown += Window_KeyDown;

                _torchFrameTime = new GorgonTimerQpc();

                return window;
            }
            finally
            {
                GorgonExample.EndInit();
            }
        }

        /// <summary>
        /// Windows the key down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        private static void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    GorgonApplication.Quit();
                    return;
                case Keys.Z:
                    _lightEffect.Lights[0].Position =
                        !e.Shift
                            ? new DX.Vector3(_lightEffect.Lights[0].Position.X, _lightEffect.Lights[0].Position.Y, _lightEffect.Lights[0].Position.Z + 1.0f)
                            : new DX.Vector3(_lightEffect.Lights[0].Position.X, _lightEffect.Lights[0].Position.Y, _lightEffect.Lights[0].Position.Z - 1.0f);
                    break;
            }
        }

        /// <summary>
        /// Handles the MouseMove event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private static void Window_MouseMove(object sender, MouseEventArgs e)
        {
            _lightEffect.Lights[0].Position = new DX.Vector3(e.X, e.Y, _lightEffect.Lights[0].Position.Z);
            _torchSprite.Position = new DX.Vector2(e.X - 11, e.Y - 23);
        }

        /// <summary>
        /// Screens the after swap chain resized.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="AfterSwapChainResizedEventArgs" /> instance containing the event data.</param>
        private static void Screen_AfterSwapChainResized(object sender, AfterSwapChainResizedEventArgs e) => UpdateRenderTarget();

        /// <summary>
        /// Screens the before swap chain resized.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="BeforeSwapChainResizedEventArgs" /> instance containing the event data.</param>
        private static void Screen_BeforeSwapChainResized(object sender, BeforeSwapChainResizedEventArgs e)
        {
            // Need to remove these prior to resizing the back buffer, otherwise the size will be mismatched.
            _finalTexture?.Dispose();
            _finalTarget?.Dispose();
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
                
                _lightEffect?.Dispose();
                _renderer?.Dispose();
                _finalTexture?.Dispose();
                _finalTarget?.Dispose();
                _backgroundLogoTexture?.Dispose();
                _screen?.Dispose();
                _graphics?.Dispose();
            }
        }
        #endregion
    }
}
