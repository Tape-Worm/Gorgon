﻿
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
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Examples;

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
    private static Gorgon2D _renderer;
    // The render target for the space background.
    private static GorgonRenderTarget2DView _spaceBackgroundRtv;
    // The background texture view for the space scene.
    private static GorgonTexture2DView _spaceBackground;
    // The render target for the crawling text.
    private static GorgonRenderTarget2DView _crawlRtv;
    // The texture for the crawling text.
    private static GorgonTexture2DView _crawl;
    // The sprite for the crawl text render target.
    private static GorgonSprite _crawlSprite;
    // The text to render.
    private static GorgonTextSprite _crawlText;
    // The position of the crawl text.
    private static Vector2 _crawlPosition;

    /// <summary>
    /// Function to create the render targets to display.
    /// </summary>
    private static void CreateTargets()
    {
        _spaceBackground?.Dispose();
        _spaceBackgroundRtv?.Dispose();
        _crawl?.Dispose();
        _crawlRtv?.Dispose();

        _spaceBackgroundRtv = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo(_screen.Width, _screen.Height, _screen.Format)
        {
            Name = "Space Background",
            Binding = TextureBinding.ShaderResource
        });
        _spaceBackground = _spaceBackgroundRtv.GetShaderResourceView();

        _crawlRtv = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo(_spaceBackgroundRtv)
        {
            Name = "Crawl Text Target"
        });
        _crawlRtv.Clear(GorgonColors.BlackTransparent);
        _crawl = _crawlRtv.GetShaderResourceView();

        _spaceBackgroundRtv.Clear(GorgonColors.Black);

        _graphics.SetRenderTarget(_spaceBackgroundRtv);
        _renderer.Begin();

        for (int i = 0; i < 1024; ++i)
        {
            float brightness = GorgonRandom.RandomSingle(0.05f, 1.0f);
            Vector2 pos = new(GorgonRandom.RandomSingle(0, _spaceBackground.Width), GorgonRandom.RandomSingle(0, _spaceBackground.Height));
            _renderer.DrawFilledRectangle(new GorgonRectangleF(pos.X, pos.Y, 1, 1), new GorgonColor(brightness, brightness, brightness));
        }

        _renderer.End();

        _graphics.SetRenderTarget(_screen.RenderTargetView);

        _crawlSprite = new GorgonSprite
        {
            Texture = _crawl,
            TextureRegion = new GorgonRectangleF(0, 0, 1, 1),
            Size = new Vector2(_crawl.Width * 1.25f, _crawl.Height),
            Position = new Vector2(-_crawl.Width * 0.125f, 0)
        };
        _crawlSprite.CornerOffsets.UpperLeft = new Vector3(_crawl.Width * 0.55f - 16, _crawl.Height * 0.5f - 64, 0);
        _crawlSprite.CornerOffsets.UpperRight = new Vector3(-_crawl.Width * 0.55f + 16, _crawl.Height * 0.5f - 64, 0);
        _crawlSprite.CornerColors.UpperLeft = GorgonColors.BlackTransparent;
        _crawlSprite.CornerColors.UpperRight = GorgonColors.BlackTransparent;

        if (_crawlText is not null)
        {
            _crawlText.LayoutArea = new Vector2(_crawlRtv.Width, _crawlRtv.Height);
            _crawlText.Text = Resources.CrawlText.WordWrap(_crawlText.Font, _crawlRtv.Width - 50);
        }
    }

    /// <summary>
    /// Function called during CPU idle time.
    /// </summary>
    /// <returns><b>true</b> to continue executing, <b>false</b> to stop.</returns>
    private static bool Idle()
    {
        _crawlPosition = new Vector2(0, _crawlPosition.Y - (_crawlRtv.Height * 0.025f * GorgonTiming.Delta));

        // Once the bottom of the text is past the 0 alpha point, flip it back to start over.
        if ((_crawlText.Size.Y + _crawlText.Position.Y) < _crawlRtv.Height * 0.35f)
        {
            _crawlPosition = new Vector2(0, _crawlRtv.Height);
        }

        // Render the text to the crawling render target.
        _graphics.SetRenderTarget(_crawlRtv);
        _crawlRtv.Clear(GorgonColors.BlackTransparent);
        _renderer.Begin();
        _crawlText.Position = _crawlPosition;
        _renderer.DrawTextSprite(_crawlText);
        _renderer.End();

        // Compose the scene with our starry background and the skewed sprite.
        _graphics.SetRenderTarget(_screen.RenderTargetView);
        _renderer.Begin();
        _renderer.DrawFilledRectangle(new GorgonRectangleF(0, 0, _screen.Width, _screen.Height), GorgonColors.White, _spaceBackground);
        // Uncomment this line to see the scrolling text without deformation.
        //_renderer.DrawFilledRectangle(new GorgonRectangleF(0, 64, _screen.Width * 0.25f, _screen.Height * 0.25f), GorgonColors.White, _crawl, new GorgonRectangleF(0, 0, 1, 1));
        _renderer.DrawSprite(_crawlSprite);
        _renderer.End();

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
        FormMain window = GorgonExample.Initialize(new GorgonPoint(ExampleConfig.Default.Resolution.X, ExampleConfig.Default.Resolution.Y), "Opening Crawl");

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
                                          new GorgonSwapChainInfo(ExampleConfig.Default.Resolution.X,
                                                                       ExampleConfig.Default.Resolution.Y,
                                                                       BufferFormat.R8G8B8A8_UNorm)
                                          {
                                              Name = "Gorgon2D Sprites Example Swap Chain"
                                          });
            _screen.SwapChainResized += Screen_AfterSwapChainResized;
            _screen.SwapChainResizing += Screen_BeforeSwapChainResized;
            // Tell the graphics API that we want to render to the "screen" swap chain.
            _graphics.SetRenderTarget(_screen.RenderTargetView);

            // Initialize the renderer so that we are able to draw stuff.
            _renderer = new Gorgon2D(_graphics);

            GorgonExample.LoadResources(_graphics);

            CreateTargets();

            // Set up our text sprite so we can render formatted text to the render target.
            _crawlText = new GorgonTextSprite(GorgonExample.Fonts.GetFont(new GorgonFontInfo("Arial", 36.0f, GorgonFontHeightMode.Points)
            {
                Name = "Arial SW",
                FontStyle = GorgonFontStyle.Bold,
                Characters = Resources.CrawlText.Distinct()
            }))
            {
                Alignment = Alignment.UpperCenter,
                Color = GorgonColors.Yellow,
                LayoutArea = new Vector2(_screen.Width, _screen.Height),
                AllowColorCodes = true
            };

            _crawlText.Text = Resources.CrawlText.WordWrap(_crawlText.Font, _crawlRtv.Width - 50);
            _crawlPosition = new Vector2(0, _crawlRtv.Height);

            window.IsLoaded = true;

            return window;
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }
    }

    /// <summary>Handles the BeforeSwapChainResized event of the Screen control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SwapChainResizingEventArgs"/> instance containing the event data.</param>
    private static void Screen_BeforeSwapChainResized(object sender, SwapChainResizingEventArgs e)
    {
        _crawl?.Dispose();
        _crawlRtv?.Dispose();
        _spaceBackground?.Dispose();
        _spaceBackgroundRtv?.Dispose();
    }

    /// <summary>
    /// Handles the AfterSwapChainResized event of the Screen control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SwapChainResizedEventArgs"/> instance containing the event data.</param>
    private static void Screen_AfterSwapChainResized(object sender, SwapChainResizedEventArgs e) => CreateTargets();

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
            _screen.SwapChainResized -= Screen_AfterSwapChainResized;
            _screen.SwapChainResizing -= Screen_BeforeSwapChainResized;

            GorgonExample.UnloadResources();
            _crawl?.Dispose();
            _crawlRtv?.Dispose();
            _spaceBackgroundRtv?.Dispose();
            _spaceBackground?.Dispose();
            _renderer?.Dispose();
            _screen?.Dispose();
            _graphics?.Dispose();
        }
    }
}
