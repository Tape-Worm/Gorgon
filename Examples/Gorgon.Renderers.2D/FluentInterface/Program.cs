
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: July 18, 2018 4:04:19 PM
// 


using System.Numerics;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Examples;

// This example shows off the fluent interface available on the 2D renderer. 
//
// The fluent interface allows developers to chain drawing methods together into a single block of code. This is an optional interface that can 
// be used with, or in place of the standard Gorgon2D renderer.

/// <summary>
/// Our example entry point
/// </summary>
static class Program
{

    // The core graphics functionality.
    private static GorgonGraphics _graphics;
    // Our swap chain that represents our "Screen".
    private static GorgonSwapChain _screen;
    // Our 2D renderer used to draw our sprites.
    private static IGorgon2DFluent _renderer;
    // The background texture view for the space scene.
    private static GorgonTexture2DView _spaceBackground;
    // The sprite for the space background.
    private static GorgonSprite _background;
    // The texture for the ship and engine glow.
    private static GorgonTexture2DView _shipTexture;
    // A list of "star" positions, used to give us a sense of motion.
    private static readonly Vector2[] _stars = new Vector2[100];
    // The ship sprite.
    private static GorgonSprite _ship;
    // The engine glow for the sprite.
    private static readonly GorgonSprite[] _engineGlow = new GorgonSprite[3];
    // The index for the glow animation frame.
    private static int _glowIndex;
    // The last time index.
    private static IGorgonTimer _glowAnimTimer;
    // Flag to indicate that the ship should float left.
    private static bool _floatLeft = true;
    // The offset for the floating ship.
    private static float _floatOffset;







    /// <summary>
    /// Function called during CPU idle time.
    /// </summary>
    /// <returns><b>true</b> to continue executing, <b>false</b> to stop.</returns>
    private static bool Idle()
    {
        _renderer
            .Update(_ =>
            {
                _background.Position = new Vector2(_background.Position.X, _background.Position.Y + (0.3f * GorgonTiming.Delta));

                // Just wrap around if we hit the top of the background.
                if (_background.Position.Y > _background.Bounds.Height)
                {
                    _background.Position = new Vector2(_background.Position.X, _screen.Height);
                }

                if (_floatLeft)
                {
                    _floatOffset -= GorgonTiming.Delta * (_screen.Width / 16.0f);
                }
                else
                {
                    _floatOffset += GorgonTiming.Delta * (_screen.Width / 16.0f);
                }

                float floatBounds = _screen.Width / 4.0f;
                if (_floatOffset < -floatBounds)
                {
                    _floatOffset = -floatBounds;
                    _floatLeft = !_floatLeft;
                }
                else if (_floatOffset > floatBounds)
                {
                    _floatOffset = floatBounds;
                    _floatLeft = !_floatLeft;
                }

                _ship.Position = new Vector2((_screen.Width / 2) + _floatOffset, _screen.Height - 120);
            })
            .Begin()
                .DrawSprite(_background)
                .DrawLoop(_stars.Length, (i, r) =>
                {
                    // Our renderer interface is passed to this method so that we can call back into it for things like loops and such.
                    // Draw our "stars".
                    ref Vector2 star = ref _stars[i];
                    star = new Vector2(star.X, star.Y + (((i % 2) == 0 ? 0.3f : 0.4f) * GorgonTiming.Delta));

                    if (star.Y > 1.0f)
                    {
                        star = new Vector2(GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle() * -1.0f);
                        return true;
                    }

                    var position = new Vector2(star.X * _screen.Width, star.Y * _screen.Height);

                    float starColorValue = GorgonRandom.RandomSingle(0.35f, 1.0f);
                    var starColor = new GorgonColor(starColorValue, starColorValue, starColorValue, 1.0f);

                    r.DrawFilledRectangle(new DX.RectangleF(position.X, position.Y, 1, 1), starColor);

                    return true;
                })
            .End()
            .Begin(Gorgon2DBatchState.AdditiveBlend)
                .Update(_ => _engineGlow[_glowIndex].Position = new Vector2(_ship.Bounds.Left - (_ship.Bounds.Width / 2) - 10, _ship.Position.Y - (_engineGlow[_glowIndex].Bounds.Height / 2.0f)))
                .DrawSprite(_engineGlow[_glowIndex])
            .End()
            .Begin()
                .DrawSprite(_ship)
            .End();

        GorgonExample.DrawStatsAndLogo(_renderer);

        _screen.Present(1);

        // Swap flow animation frames after 33 milliseconds.
        if (_glowAnimTimer.Milliseconds > 33)
        {
            ++_glowIndex;

            if (_glowIndex > 2)
            {
                _glowIndex = 0;
            }

            _glowAnimTimer.Reset();
        }

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
        FormMain window = GorgonExample.Initialize(new DX.Size2(ExampleConfig.Default.Resolution.Width, ExampleConfig.Default.Resolution.Height), "Sprites");

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
            _screen.SwapChainResized += Screen_AfterSwapChainResized;
            // Tell the graphics API that we want to render to the "screen" swap chain.
            _graphics.SetRenderTarget(_screen.RenderTargetView);

            // Initialize the renderer so that we are able to draw stuff.
            _renderer = new Gorgon2D(_graphics);

            // Re-use the background texture from the Effects example.
            _spaceBackground = GorgonTexture2DView.FromFile(_graphics,
                                                            Path.Combine(GorgonExample.GetResourcePath(@"Textures\").FullName, "HotPocket.dds"),
                                                            new GorgonCodecDds(),
                                                            new GorgonTexture2DLoadOptions
                                                            {
                                                                Usage = ResourceUsage.Immutable
                                                            });

            // Get our space ship texture.
            _shipTexture = GorgonTexture2DView.FromFile(_graphics,
                                                        Path.Combine(GorgonExample.GetResourcePath(@"Textures\Sprites\").FullName, "wship1.png"),
                                                        new GorgonCodecPng(),
                                                        new GorgonTexture2DLoadOptions
                                                        {
                                                            Usage = ResourceUsage.Immutable
                                                        });

            _ship = new GorgonSprite
            {
                Bounds = new DX.RectangleF(0, 0, 206, 369),
                Texture = _shipTexture,
                // Calculate the ship texture coordinates.
                TextureRegion = _shipTexture.Texture.ToTexel(new DX.Rectangle(34, 10, 206, 369)),
                Anchor = new Vector2(0.5f, 1.0f)
            };
            _engineGlow[0] = new GorgonSprite
            {
                Bounds = new DX.RectangleF(0, 0, 250, 85),
                Texture = _shipTexture,
                TextureRegion = _shipTexture.Texture.ToTexel(new DX.Rectangle(512, 11, 250, 85))
            };
            _engineGlow[1] = new GorgonSprite
            {
                Bounds = new DX.RectangleF(0, 0, 250, 85),
                Texture = _shipTexture,
                TextureRegion = _shipTexture.Texture.ToTexel(new DX.Rectangle(512, 114, 250, 85))
            };
            _engineGlow[2] = new GorgonSprite
            {
                Bounds = new DX.RectangleF(0, 0, 250, 85),
                Texture = _shipTexture,
                TextureRegion = _shipTexture.Texture.ToTexel(new DX.Rectangle(512, 207, 250, 85))
            };

            // Space background sprite texture positioning.
            _background = new GorgonSprite
            {
                Texture = _spaceBackground,
                Bounds = new DX.RectangleF(_screen.Width / 2,
                                                         _screen.Height,
                                                         _spaceBackground.Width,
                                                         _spaceBackground.Height),
                TextureRegion = new DX.RectangleF(0, 0, 1, 1),
                Anchor = new Vector2(0.5f, 1.0f)
            };

            GorgonExample.LoadResources(_graphics);

            for (int i = 0; i < _stars.Length; ++i)
            {
                _stars[i] = new Vector2(GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle());
            }

            window.IsLoaded = true;

            _glowAnimTimer = new GorgonTimerQpc();

            return window;
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }
    }

    /// <summary>
    /// Handles the AfterSwapChainResized event of the Screen control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SwapChainResizedEventArgs"/> instance containing the event data.</param>
    private static void Screen_AfterSwapChainResized(object sender, SwapChainResizedEventArgs e) =>
        // We'll need to readjust the background scroller.
        _background.Position = new Vector2(_background.Position.X, e.Size.Height + (_background.Position.Y - e.OldSize.Height));

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
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
            _shipTexture?.Dispose();
            _spaceBackground?.Dispose();
            _renderer?.Dispose();
            _screen?.Dispose();
            _graphics?.Dispose();
        }
    }

}
