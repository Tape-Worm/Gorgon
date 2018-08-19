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
// Created: August 19, 2018 10:22:18 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gorgon.Animation;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI;
using WMPLib;

namespace Animation
{
    /// <summary>
    /// Our example entry point.
    /// </summary>
    static class Program
    {
        #region Variables.
        // The primary graphics interface.
        private static GorgonGraphics _graphics;
        // Our swap chain representing our screen.
        private static GorgonSwapChain _screen;
        // Our metal \m/ gif (J-iff).
        private static GorgonTexture2DView _metal;
        // Our trooper image.
        private static GorgonTexture2DView _trooper;
        // Our logo.
        private static GorgonTexture2DView _logo;
        // Render target for our music video...
        private static GorgonRenderTarget2DView _target;
        // The texture view for the render target.
        private static GorgonTexture2DView _targetView;
        // The list of delays between each frame.
        private static IReadOnlyList<float> _frameDelays;
        // The 2D renderer.
        private static Gorgon2D _renderer;
        // Our animation controller.
        private static GorgonSpriteAnimationController _animController;
        // Our animation.
        private static IGorgonAnimation _animation;
        // The sprite to animate.
        private static GorgonSprite _animatedSprite;
        // The string builder for the FPS text.
        private static readonly StringBuilder _fpsString = new StringBuilder();
        // The batch state for drawing our render target.
        private static Gorgon2DBatchState _targetBatchState;
        // Player for our mp3.
        private static readonly WindowsMediaPlayer _mp3Player = new WindowsMediaPlayer();
        #endregion

        #region Methods.
        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Function to build our animation... like an 80's music video... this is going to be ugly.
        /// </summary>
        private static void MakeAn80sMusicVideo()
        {
            var animBuilder = new GorgonAnimationBuilder();
            
            animBuilder.PositionInterpolationMode(TrackInterpolationMode.Spline)
                       .ScaleInterpolationMode(TrackInterpolationMode.Spline)
                       .RotationInterpolationMode(TrackInterpolationMode.Spline)
                       .ColorInterpolationMode(TrackInterpolationMode.Spline)
                       // Set up some scaling...
                       .EditScale()
                       .SetKey(new GorgonKeyVector3(0, new DX.Vector2(1, 1)))
                       .SetKey(new GorgonKeyVector3(2, new DX.Vector2(0.5f, 0.5f)))
                       .SetKey(new GorgonKeyVector3(4, new DX.Vector2(4.0f, 4.0f)))
                       .SetKey(new GorgonKeyVector3(6, new DX.Vector2(1, 1)))
                       .EndEdit()
                       // Set up some positions...
                       .EditPositions()
                       .SetKey(new GorgonKeyVector3(0, new DX.Vector2(_screen.Width / 2.0f, _screen.Height / 2.0f)))
                       .SetKey(new GorgonKeyVector3(2, new DX.Vector2(200, 220)))
                       .SetKey(new GorgonKeyVector3(3, new DX.Vector2(_screen.Width - 2, 130)))
                       .SetKey(new GorgonKeyVector3(4, new DX.Vector2(255, _screen.Height - _metal.Height)))
                       .SetKey(new GorgonKeyVector3(5, new DX.Vector2(200, 180)))
                       .SetKey(new GorgonKeyVector3(6, new DX.Vector2(180, 160)))
                       .SetKey(new GorgonKeyVector3(7, new DX.Vector2(150, 180)))
                       .SetKey(new GorgonKeyVector3(8, new DX.Vector2(150, 160)))
                       .SetKey(new GorgonKeyVector3(9, new DX.Vector2(_screen.Width / 2, _screen.Height / 2)))
                       .EndEdit()
                       // Set up some colors...By changing the alpha, we can simulate a motion blur effect.
                       .EditColors()
                       .SetKey(new GorgonKeyGorgonColor(0, GorgonColor.Black))
                       .SetKey(new GorgonKeyGorgonColor(2, new GorgonColor(GorgonColor.RedPure, 0.25f)))
                       .SetKey(new GorgonKeyGorgonColor(4, new GorgonColor(GorgonColor.GreenPure, 0.5f)))
                       .SetKey(new GorgonKeyGorgonColor(6, new GorgonColor(GorgonColor.BluePure, 0.25f)))
                       .SetKey(new GorgonKeyGorgonColor(8, new GorgonColor(GorgonColor.LightCyan, 0.25f)))
                       .SetKey(new GorgonKeyGorgonColor(10, new GorgonColor(GorgonColor.Black, 1.0f)))
                       .EndEdit()
                       // And finally, some MuchMusic/MTV style rotation... because.
                       .EditRotation()
                       .SetKey(new GorgonKeyVector3(0, new DX.Vector3(0, 0, 0)))
                       .SetKey(new GorgonKeyVector3(2, new DX.Vector3(0, 0, 180)))
                       .SetKey(new GorgonKeyVector3(4, new DX.Vector3(0, 0, 270.0f)))
                       .SetKey(new GorgonKeyVector3(6, new DX.Vector3(0, 0, 0)))
                       .EndEdit();

            IGorgonTrackKeyBuilder<GorgonKeyTexture2D> trackBuilder = animBuilder.Edit2DTexture();
            float time = 0;

            // Now, add the animation frames from our GIF.
            for (int i = 0; i < _metal.ArrayCount; ++i)
            {
                trackBuilder.SetKey(new GorgonKeyTexture2D(time, _metal, new DX.RectangleF(0, 0, 1, 1), i));
                time += _frameDelays[i];
            }

            float delay = (10.0f - time).Max(0) / 15.0f;

            // Now add in a texture switch with coordinate update... because.
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0.25f, 0.25f, 0.5f, 0.5f), 0));
            time += delay;
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0.125f, 0.125f, 0.75f, 0.75f), 0));
            time += delay;
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0, 0, 1, 1), 0));
            time += delay;
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0.125f, 0.125f, 0.75f, 0.75f), 0));
            time += delay;
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0.25f, 0.25f, 0.5f, 0.5f), 0));
            time += delay;
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0.125f, 0.125f, 0.75f, 0.75f), 0));
            time += delay;
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0, 0, 1, 1), 0));
            time += delay;
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0.125f, 0.125f, 0.75f, 0.75f), 0));
            time += delay;
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0.25f, 0.25f, 0.5f, 0.5f), 0));
            time += delay;
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0.125f, 0.125f, 0.75f, 0.75f), 0));
            time += delay;
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0, 0, 1, 1), 0));
            time += delay;
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0.125f, 0.125f, 0.75f, 0.75f), 0));
            time += delay;
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0.25f, 0.25f, 0.5f, 0.5f), 0));
            time += delay;
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0.125f, 0.125f, 0.75f, 0.75f), 0));
            time += delay;
            trackBuilder.SetKey(new GorgonKeyTexture2D(time, _trooper, new DX.RectangleF(0, 0, 1, 1), 0));

            trackBuilder.EndEdit();

            _animation = animBuilder.Build(@"\m/");
            _animation.IsLooped = true;

            _animController = new GorgonSpriteAnimationController();
        }

        /// <summary>
        /// Function to initialize the application.
        /// </summary>
        /// <returns>The main window for the application.</returns>
        private static FormMain Initialize()
        {
            MemoryStream stream = null;

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
                                              new GorgonSwapChainInfo("Gorgon2D Animation Example Swap Chain")
                                              {
                                                  Width = Settings.Default.Resolution.Width,
                                                  Height = Settings.Default.Resolution.Height,
                                                  Format = BufferFormat.R8G8B8A8_UNorm
                                              });

                _target = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo(_screen.RenderTargetView, "Video Target")
                                                                                 {
                                                                                     Binding = TextureBinding.ShaderResource
                                                                                 });
                _targetView = _target.Texture.GetShaderResourceView();
                _target.Clear(GorgonColor.CornFlowerBlue);
                
                // Load our textures.
                var gif = new GorgonCodecGif(decodingOptions: new GorgonGifDecodingOptions
                                                              {
                                                                  ReadAllFrames = true
                                                              });

                // Find out how long to display each frame for.
                // We'll also convert it to milliseconds since GIF stores the delays as 1/100th of a second.
                _frameDelays = gif.GetFrameDelays(GetResourcePath(@"\Textures\Animation\metal.gif")).Select(item => item / 100.0f).ToArray();

                _metal = GorgonTexture2DView.FromFile(_graphics, 
                                                      GetResourcePath(@"\Textures\Animation\metal.gif"),
                                                      gif,
                                                      new GorgonTextureLoadOptions
                                                      {
                                                          Name = @"Metal \m/",
                                                          Usage = ResourceUsage.Immutable,
                                                          Binding = TextureBinding.ShaderResource
                                                      });
                _trooper = GorgonTexture2DView.FromFile(_graphics,
                                                        GetResourcePath(@"\Textures\Animation\trooper.png"),
                                                        new GorgonCodecPng(),
                                                        new GorgonTextureLoadOptions
                                                        {
                                                            Name = "Trooper",
                                                            Usage = ResourceUsage.Immutable,
                                                            Binding = TextureBinding.ShaderResource
                                                        });

                stream = new MemoryStream(Resources.Gorgon_Logo_Small);
                _logo = GorgonTexture2DView.FromStream(_graphics, stream, new GorgonCodecDds());

                _renderer = new Gorgon2D(_screen.RenderTargetView);
                _animatedSprite = new GorgonSprite
                                  {
                                      Position = new DX.Vector2(_screen.Width / 2, _screen.Height / 2),
                                      Size = new DX.Size2F(_metal.Width, _metal.Height),
                                      Anchor = new DX.Vector2(0.5f, 0.5f)
                                  };

                MakeAn80sMusicVideo();

                // We need to set up a blend state so that the alpha in the render target doesn't get overwritten.
                var builder = new Gorgon2DBatchStateBuilder();
                var blendBuilder = new GorgonBlendStateBuilder();
                _targetBatchState = builder.BlendState(blendBuilder
                                                       .ResetTo(GorgonBlendState.Default)
                                                       .DestinationBlend(alpha: Blend.DestinationAlpha)
                                                       .Build())
                                           .Build();

                _mp3Player.URL = GetResourcePath("AnimationExample.mp3");
                
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
        /// Function to perform operations while the CPU is idle.
        /// </summary>
        /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
        private static bool Idle()
        {
            // Set the initial background color, we won't be clearing again...
            _screen.RenderTargetView.Clear(GorgonColor.CornFlowerBlue);

            if (_mp3Player.playState == WMPPlayState.wmppsStopped)
            {
                _mp3Player.controls.play();
            }

            if (_animController.CurrentAnimation == null)
            {
                _animController.Play(_animatedSprite, _animation);
            }

            _graphics.SetRenderTarget(_target);
            _renderer.Begin(_targetBatchState);
            _renderer.DrawSprite(_animatedSprite);
            _renderer.End();
            
            _graphics.SetRenderTarget(_screen.RenderTargetView);
            _renderer.Begin();
            _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _screen.Width, _screen.Height),
                                          GorgonColor.White,
                                          _targetView,
                                          new DX.RectangleF(0, 0, 1, 1));

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

            // We need to call this once per frame so the animation will transition properly.
            _animController.Update();

            _screen.Present(1);

            return true;
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
                throw new ArgumentException("The resource was not specified.", nameof(resourceItem));
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
        #endregion

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
                _mp3Player.controls.stop();
                _renderer?.Dispose();
                _logo?.Dispose();
                _trooper?.Dispose();
                _metal?.Dispose();
                _targetView?.Dispose();
                _target?.Dispose();
                _screen?.Dispose();
                _graphics?.Dispose();
            }
        }
    }
}
