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
using Gorgon.IO;
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
        private static GorgonTexture2DView _logoTexture;
        // The final render target for display.
        private static GorgonRenderTarget2DView _finalTarget;
        // The final texture for display.
        private static GorgonTexture2DView _finalTexture;
        // Our 2D renderer.
        private static Gorgon2D _renderer;
        // Sprite used to draw our logo.
        private static GorgonSprite _logoSprite;
        // Our lighting effect.
        private static Gorgon2DDeferredLightingEffect _lightEffect;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to draw the scene that needs lighting.
        /// </summary>
        private static void DrawLitScene()
        {
            _logoSprite.Position = new DX.Vector2(_screen.Width / 2.0f, _screen.Height / 2.0f);
            _renderer.DrawSprite(_logoSprite);
        }

        /// <summary>
        /// Function called when the application goes into an idle state.
        /// </summary>
        /// <returns><b>true</b> to continue executing, <b>false</b> to stop.</returns>
        private static bool Idle()
        {
            _finalTarget.Clear(GorgonColor.BlackTransparent);
            _screen.RenderTargetView.Clear(GorgonColor.Black);

            // Render the lit sprite.
            _lightEffect.RenderEffect(DrawLitScene, _finalTarget);

            // Blit our final texture to the main screen.
            _graphics.SetRenderTarget(_screen.RenderTargetView);
            _renderer.Begin();
            _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _screen.Width, _screen.Height),
                                          GorgonColor.White,
                                          _finalTexture,
                                          new DX.RectangleF(0, 0, 1, 1));
            
            _renderer.DrawString($"FPS: {GorgonTiming.AverageFPS:0.0}\nFrame delta: {GorgonTiming.Delta:0.000} seconds.", DX.Vector2.Zero);
            _renderer.End();

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
            _finalTexture = _finalTarget.Texture.GetShaderResourceView();
        }

        /// <summary>
        /// Function to initialize the application.
        /// </summary>
        /// <returns>The main window for the application.</returns>
        private static FormMain Initialize()
        {
            var window = new FormMain
                         {
                             ClientSize = Settings.Default.Resolution
                         };
            window.Show();

            // Process any pending events so the window shows properly.
            Application.DoEvents();

            Cursor.Current = Cursors.WaitCursor;

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
                _logoTexture = GorgonTexture2DView.FromFile(_graphics,
                                                            GetResourcePath(@"Textures\Lights\lights.dds"),
                                                            new GorgonCodecDds(),
                                                            new GorgonTextureLoadOptions
                                                            {
                                                                Usage = ResourceUsage.Immutable,
                                                                Binding = TextureBinding.ShaderResource,
                                                                Name = "Logo"
                                                            });

                UpdateRenderTarget();

                // Initialize the renderer so that we are able to draw stuff.
                _renderer = new Gorgon2D(_screen.RenderTargetView);

                _logoSprite = new GorgonSprite
                              {
                                  Texture = _logoTexture,
                                  Bounds = new DX.RectangleF(0, 0, _logoTexture.Width, _logoTexture.Height),
                                  TextureRegion = new DX.RectangleF(0, 0, 1, 1),
                                  Anchor = new DX.Vector2(0.5f, 0.5f)
                              };


                // Create the effect that will light up our sprite(s).
                _lightEffect = new Gorgon2DDeferredLightingEffect(_renderer);
                _lightEffect.Lights.Add(new Gorgon2DLight
                                        {
                                            Color = GorgonColor.White,
                                            Attenuation = 1.0f,
                                            LightType = LightType.Point,
                                            SpecularEnabled = true,
                                            SpecularPower = 64.0f,
                                            Position = new DX.Vector3(_screen.Width / 2.0f - 150.0f, _screen.Height / 2.0f - 150.0f, 100)
                                        });
                
                window.IsLoaded = true;
                window.MouseMove += Window_MouseMove;

                return window;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Handles the MouseMove event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private static void Window_MouseMove(object sender, MouseEventArgs e)
        {
            _lightEffect.Lights[0].Position = new DX.Vector3(e.X, e.Y, 40);
        }

        /// <summary>
        /// Screens the after swap chain resized.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Gorgon.Graphics.Core.AfterSwapChainResizedEventArgs" /> instance containing the event data.</param>
        private static void Screen_AfterSwapChainResized(object sender, AfterSwapChainResizedEventArgs e)
        {
            UpdateRenderTarget();
        }

        /// <summary>
        /// Screens the before swap chain resized.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Gorgon.Graphics.Core.BeforeSwapChainResizedEventArgs" /> instance containing the event data.</param>
        private static void Screen_BeforeSwapChainResized(object sender, BeforeSwapChainResizedEventArgs e)
        {
            // Need to remove these prior to resizing the back buffer, otherwise the size will be mismatched.
            _finalTexture?.Dispose();
            _finalTarget?.Dispose();
        }

        /// <summary>
        /// Property to return the path to the resources for the example.
        /// </summary>
        /// <param name="resourceItem">The directory or file to use as a resource.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="resourceItem"/> was NULL (<i>Nothing</i> in VB.Net) or empty.</exception>
        public static string GetResourcePath(string resourceItem)
        {
            string path = Settings.Default.ResourceLocation;

            if (string.IsNullOrEmpty(resourceItem))
            {
                throw new ArgumentException(@"The resource was not specified.", nameof(resourceItem));
            }

            path = path.FormatDirectory(Path.DirectorySeparatorChar);

            // If this is a directory, then sanitize it as such.
            if (resourceItem.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path += resourceItem.FormatDirectory(Path.DirectorySeparatorChar);
            }
            else
            {
                // Otherwise, format the file name.
                path += resourceItem.FormatPath(Path.DirectorySeparatorChar);
            }

            // Ensure that we have an absolute path.
            return Path.GetFullPath(path);
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
                ex.Catch(e => GorgonDialogs.ErrorBox(null, "There was an error running the application and it must now close.", "Error", ex));
            }
            finally
            {
                _lightEffect?.Dispose();
                _renderer?.Dispose();
                _finalTexture?.Dispose();
                _finalTarget?.Dispose();
                _logoTexture?.Dispose();
                _screen?.Dispose();
                _graphics?.Dispose();
            }
        }
        #endregion
    }
}
