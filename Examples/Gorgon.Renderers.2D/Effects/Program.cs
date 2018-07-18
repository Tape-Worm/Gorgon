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
        // The texture view for the background render target.
        private static GorgonTexture2DView _backgroundView;
        // The render target used for displacement.
        private static GorgonRenderTarget2DView _backgroundTarget;
        // Flag to indicate that the help text should be shown.
        private static bool _showHelp = true;
        // The cloaking animation controller.
        private static readonly CloakController _cloakController = new CloakController();
        #endregion

        #region Methods.
        /// <summary>
        /// Function to perform operations while the CPU is idle.
        /// </summary>
        /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
        private static bool Idle()
        {

            _backgroundTarget.Clear(GorgonColor.Black);

            DX.Vector2 textureSize = _background.Texture.ToTexel(new DX.Vector2(_backgroundTarget.Width, _backgroundTarget.Height));
            
            // Blit the background texture.
            _graphics.SetRenderTarget(_backgroundTarget);
            _renderer.Begin();
            _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _backgroundTarget.Width, _backgroundTarget.Height),
                                          GorgonColor.White,
                                          _background,
                                          new DX.RectangleF(_backgroundOffset.X, _backgroundOffset.Y, textureSize.X, textureSize.Y));
            _shipSprite.Color = new GorgonColor(GorgonColor.White, _cloakController.Opacity);
            _renderer.DrawSprite(_shipSprite);
            _renderer.End();

            _graphics.SetRenderTarget(_screen.RenderTargetView);

            // No sense in rendering the effect if it's not present.
            float strength = _cloakController.CloakAmount;
            if (strength > 0.0f)
            {
                _displacement.Strength = strength;
                _displacement.RenderEffect(() =>
                                           {
                                               _shipSprite.Color = GorgonColor.White;
                                               _renderer.DrawSprite(_shipSprite);
                                           },
                                           _backgroundView);
            }
            else
            {
                _renderer.Begin();
                _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _backgroundTarget.Width, _backgroundTarget.Height),
                                              GorgonColor.White,
                                              _backgroundView,
                                              new DX.RectangleF(0, 0, 1, 1));
                _renderer.End();
            }
            
            _renderer.Begin();
            _renderer.DrawString($"FPS: {GorgonTiming.AverageFPS:0.0}\nFrame Delta: {GorgonTiming.Delta: 0.000}", DX.Vector2.Zero);

            if (_showHelp)
            {
                _renderer.DrawString(HelpText, new DX.Vector2(0, 48), color: new GorgonColor(1.0f, 1.0f, 0));
            }

            _renderer.End();

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
                _randomOffset = new DX.Vector2(GorgonRandom.RandomSingle(_background.Width - _backgroundTarget.Width),
                                               GorgonRandom.RandomSingle(_background.Height - _backgroundTarget.Height));
            }

            // If we're previously out of frame, then push back until we're in frame.
            if (_randomOffset.Value.X + _backgroundTarget.Width > _background.Width)
            {
                _randomOffset = new DX.Vector2(_randomOffset.Value.X - _backgroundTarget.Width, _randomOffset.Value.Y);
            }

            if (_randomOffset.Value.Y + _backgroundTarget.Height > _background.Height)
            {
                _randomOffset = new DX.Vector2(_randomOffset.Value.X, _randomOffset.Value.Y - _backgroundTarget.Height);
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
            BuildBackgroundRenderTarget();
            InitializeBackgroundTexturePositioning();
        }

        /// <summary>
        /// Function to build up the background render target.
        /// </summary>
        private static void BuildBackgroundRenderTarget()
        {
            _backgroundTarget = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo("Background Render Target")
                                                                                       {
                                                                                           Width = _screen.Width,
                                                                                           Height = _screen.Height,
                                                                                           Format = _screen.Format
                                                                                       });
            _backgroundView = _backgroundTarget.Texture.GetShaderResourceView();
        }

        /// <summary>
        /// Function to initialize the application.
        /// </summary>
        /// <returns>The main window for the application.</returns>
        private static FormMain Initialize()
        {
            Cursor.Current = Cursors.WaitCursor;

            var window = new FormMain
                         {
                             ClientSize = Settings.Default.Resolution
                         };
            window.Show();

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
                _renderer = new Gorgon2D(_screen.RenderTargetView);

                // Create the displacement effect used for the "cloaking" effect.
                _displacement = new Gorgon2DDisplacementEffect(_renderer)
                                {
                                    Strength = 0
                                };

                // Load our texture with our space background in it.
                _background = GorgonTexture2DView.FromFile(_graphics,
                                                           GetResourcePath(@"Textures\Effects\space_bg.dds"),
                                                           new GorgonCodecDds(),
                                                           new GorgonTextureLoadOptions
                                                           {
                                                               Usage = ResourceUsage.Immutable,
                                                               Binding = TextureBinding.ShaderResource
                                                           });

                // Load up our super space ship image.
                _spaceShipTexture = GorgonTexture2DView.FromFile(_graphics,
                                                                 GetResourcePath(@"Textures\Effects\ship.png"),
                                                                 new GorgonCodecPng(),
                                                                 new GorgonTextureLoadOptions
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

                BuildBackgroundRenderTarget();
                InitializeBackgroundTexturePositioning();

                window.MouseMove += Window_MouseMove;
                window.MouseWheel += Window_MouseWheel;
                window.KeyUp += Window_KeyUp;

                return window;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Handles the BeforeSwapChainResized event of the Screen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="BeforeSwapChainResizedEventArgs"/> instance containing the event data.</param>
        private static void Screen_BeforeSwapChainResized(object sender, BeforeSwapChainResizedEventArgs e)
        {
            _backgroundTarget?.Dispose();
        }

        /// <summary>
        /// Handles the KeyUp event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private static void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                GorgonApplication.Quit();
                return;
            }

            if (e.KeyCode == Keys.F1)
            {
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
        private static void Window_MouseMove(object sender, MouseEventArgs e)
        {
            _shipSprite.Position = new DX.Vector2(e.X, e.Y);
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
                _background?.Dispose();
                _renderer?.Dispose();
                _screen?.Dispose();
                _graphics?.Dispose();
            }
        }
        #endregion
    }
}
