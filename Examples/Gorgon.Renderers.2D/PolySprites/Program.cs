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
// Created: August 10, 2018 9:11:41 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gorgon.Core;
using DX = SharpDX;
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
    /// Main entry point for the example application.
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
        // The polygonal sprite.
        private static GorgonPolySprite _polySprite;
        // The regular rectangular sprite.
        private static GorgonSprite _normalSprite;
        // The texture for the sprites.
        private static GorgonTexture2DView _texture;
        // Angles of rotation in degrees.
        private static float _angle1;
        private static float _angle2 = 360.0f;
        // The string builder for the FPS text.
        private static readonly StringBuilder _fpsString = new StringBuilder();
        // The logo for Gorgon.
        private static GorgonTexture2DView _logo;
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when the application goes into an idle state.
        /// </summary>
        /// <returns><b>true</b> to continue executing, <b>false</b> to stop.</returns>
        private static bool Idle()
        {
            _angle1 += 45.0f * GorgonTiming.Delta;
            _angle2 -= 30.0f * GorgonTiming.Delta;

            if (_angle1 > 360.0f)
            {
                _angle1 = _angle1 - 360.0f;
            }

            if (_angle2 < 0.0f)
            {
                _angle2 = 360.0f + _angle2;
            }

            _screen.RenderTargetView.Clear(new GorgonColor(0, 0, 0.2f));

            // Draw standard sprites.
            _renderer.Begin();

            _renderer.DrawString("Polygonal Sprite",
                                 new DX.Vector2(_screen.Width / 4.0f - _polySprite.Size.Width * 0.5f,
                                                (_screen.Height / 4.0f) - _polySprite.Size.Height * 0.5f - _renderer.DefaultFont.LineHeight));

            _renderer.DrawString("Polygonal Sprite (Wireframe)",
                                 new DX.Vector2(_screen.Width - (_screen.Width / 4.0f) - _polySprite.Size.Width * 0.5f,
                                                (_screen.Height / 4.0f) - _polySprite.Size.Height * 0.5f - _renderer.DefaultFont.LineHeight));

            _renderer.DrawString("Rectangular Sprite",
                                 new DX.Vector2(_screen.Width / 4.0f - _polySprite.Size.Width * 0.5f,
                                                _screen.Height - (_screen.Height / 4.0f) - _polySprite.Size.Height * 0.5f - _renderer.DefaultFont.LineHeight));

            _renderer.DrawString("Rectangular Sprite (Wireframe)",
                                 new DX.Vector2(_screen.Width - (_screen.Width / 4.0f) - _polySprite.Size.Width * 0.5f,
                                                _screen.Height - (_screen.Height / 4.0f) - _polySprite.Size.Height * 0.5f - _renderer.DefaultFont.LineHeight));

            _normalSprite.Texture = _texture;
            _normalSprite.Angle = _angle1;
            _normalSprite.Position = new DX.Vector2(_screen.Width / 4.0f, _screen.Height - (_screen.Height / 4.0f));

            _polySprite.Texture = _texture;
            _polySprite.Angle = _angle2;
            _polySprite.Position = new DX.Vector2(_screen.Width / 4.0f, (_screen.Height / 4.0f));

            _renderer.DrawSprite(_normalSprite);
            _renderer.DrawPolygonSprite(_polySprite);

            _renderer.End();

            // Draw wireframe versions.
            _renderer.Begin(Gorgon2DBatchState.WireFrameNoCulling);
            
            _normalSprite.Texture = null;
            _normalSprite.Angle = _angle2;
            _normalSprite.Position = new DX.Vector2(_screen.Width - (_screen.Width / 4.0f), _screen.Height - (_screen.Height / 4.0f));

            _polySprite.Texture = null;
            _polySprite.Angle = _angle1;
            _polySprite.Position = new DX.Vector2(_screen.Width - (_screen.Width / 4.0f), (_screen.Height / 4.0f));

            _renderer.DrawSprite(_normalSprite);
            _renderer.DrawPolygonSprite(_polySprite);

            _renderer.End();

            _renderer.Begin();
            _fpsString.Length = 0;
            _fpsString.AppendFormat("FPS: {0:0.0}\nFrame delta: {1:0.000} ms.", GorgonTiming.AverageFPS, GorgonTiming.Delta * 1000);

            DX.Size2F textSize = _renderer.DefaultFont.MeasureText(_fpsString.ToString(), false);

            _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _screen.Width, textSize.Height + 4), new GorgonColor(0, 0, 0, 0.5f));
            _renderer.DrawLine(0, textSize.Height + 4, _screen.Width, textSize.Height + 4, GorgonColor.White, 1.5f);
            _renderer.DrawLine(0, textSize.Height + 5, _screen.Width, textSize.Height + 5, new GorgonColor(0, 0, 0, 0.75f));

            _renderer.DrawString(_fpsString.ToString(), DX.Vector2.Zero, color: GorgonColor.White);
            DX.RectangleF pos = new DX.RectangleF(_screen.Width - _logo.Width - 5, _screen.Height - _logo.Height - 2, _logo.Width, _logo.Height);
            _renderer.DrawFilledRectangle(pos, GorgonColor.White, _logo, new DX.RectangleF(0, 0, 1, 1));
            _renderer.End();

            _screen.Present(1);

            return true;
        }

        /// <summary>
        /// Function to create the sprites to draw.
        /// </summary>
        private static void CreateSprites()
        {
            // Create the regular sprite first.
            _normalSprite = new GorgonSprite
                            {
                                Anchor = new DX.Vector2(0.5f, 0.5f),
                                Size = new DX.Size2F(_texture.Width, _texture.Height),
                                Texture = _texture,
                                TextureRegion = new DX.RectangleF(0, 0, 1, 1)
                            };

            _polySprite = PolygonHullParser.ParsePolygonHullString(_renderer, Resources.PolygonHull);
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

            MemoryStream stream = null;

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
                _renderer = new Gorgon2D(_screen.RenderTargetView);

                _texture = GorgonTexture2DView.FromFile(_graphics,
                                                        GetResourcePath(@"Textures\PolySprites\Ship.png"),
                                                        new GorgonCodecPng(),
                                                        new GorgonTextureLoadOptions
                                                        {
                                                            Binding = TextureBinding.ShaderResource, Name = "Ship Texture", Usage = ResourceUsage.Immutable
                                                        });

                stream = new MemoryStream(Resources.Gorgon_Logo_Small);
                _logo = GorgonTexture2DView.FromStream(_graphics, stream, new GorgonCodecDds());

                CreateSprites();
                
                
                window.IsLoaded = true;
                return window;
            }
            finally
            {
                stream?.Dispose();
                Cursor.Current = Cursors.Default;
            }
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
                Cursor.Show();
                ex.Catch(e => GorgonDialogs.ErrorBox(null, "There was an error running the application and it must now close.", "Error", ex));
            }
            finally
            {
                _polySprite?.Dispose();
                _texture?.Dispose();
                _renderer?.Dispose();
                _screen?.Dispose();
                _graphics?.Dispose();
            }
        }
        #endregion
    }
}
