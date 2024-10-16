﻿
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
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
// Created: May 18, 2019 1:25:32 PM
// 

using System.Numerics;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Input;
using Gorgon.PlugIns;
using Gorgon.Renderers;
using Gorgon.Renderers.Cameras;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Examples;

/// <summary>
/// This example shows many of the features present in Gorgon to show how they could be used to build a 2D top down game with special effects
/// It employs the 2D rendering system, lighting effect, bloom effect, post processing and even 3D using the core graphics API
/// 
/// Due to the effects employed, this example requires a fair bit of video RAM and a fairly fast GPU. 
/// </summary>
static class Program
{

    // The cache for our plug-in assemblies.
    private static GorgonMefPlugInCache _assemblyCache;
    // The primary graphics interface.
    private static GorgonGraphics _graphics;
    // The main "screen" for the application.
    private static GorgonSwapChain _screen;
    // Our 2D renderer.
    private static Gorgon2D _renderer;
    // The system to give us access to the resources for the application (textures, sprites, etc...).
    private static ResourceManagement _resources;
    // The renderer used to draw the scene.
    private static SceneRenderer _sceneRenderer;
    // The main render target view.
    private static GorgonRenderTarget2DView _mainRtv;
    // THe main shader resource view.
    private static GorgonTexture2DView _mainSrv;
    // Our ship.
    private static Ship _ship;
    // Our ship.
    private static Ship _shipDeux;
    // A big ship.
    private static BigShip _bigShip;
    // The user input interface.
    private static GorgonRawInput _input;
    // The keyboard input interface.
    private static GorgonRawKeyboard _keyboard;
    // The aspect ratio for the main render target view.
    private static Vector2 _mainRtvAspect = Vector2.Zero;
    // The base resolution for our display.  
    // This is used to ensure that the display area scales correctly when the window is resized.  We ensure that the sprites and whatnot remain at the same size no matter what the actual 
    // screen size is by locking our scale to a base of the resolution set here. 
    private static readonly DX.Vector2 _baseResolution = new(1920, 1080);
    // The font used to draw the help text.
    private static GorgonFont _helpFont;
    // Text sprite for instructions.
    private static GorgonTextSprite _textSprite;
    // Flag to indicate that the instructions be shown.
    private static bool _showInstructions = true;

    /// <summary>Handles the KeyUp event of the Keyboard control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="GorgonKeyboardEventArgs"/> instance containing the event data.</param>
    private static void Keyboard_KeyUp(object sender, GorgonKeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case Keys.C:
                if (_ship.LayerController is not null)
                {
                    LayerCamera controller = _ship.LayerController;
                    _ship.LayerController = null;
                    _shipDeux.LayerController = controller;
                }
                else
                {
                    LayerCamera controller = _shipDeux.LayerController;
                    _shipDeux.LayerController = null;
                    _ship.LayerController = controller;
                }
                break;
            case Keys.F1:
                _showInstructions = !_showInstructions;
                break;
        }
    }

    /// <summary>
    /// Function to render the GUI (such as it is) to give feedback to the user.
    /// </summary>
    /// <param name="renderArea">The area in which we render our graphics.</param>
    /// <remarks>
    /// <para>
    /// The render area is our view into the rendered region on the window.  We wish to place our GUI into this area, so in order to do so, we'll need to adjust our drawing by the region.
    /// </para>
    /// </remarks>
    private static void RenderGui(GorgonRectangleF renderArea)
    {
        _renderer.Begin();
        if (_showInstructions)
        {
            _textSprite.Text = string.Format("{0}\n\nShip 1: {1:0.0}x{2:0.0}\nShip 2: {3:0.0}x{4:0.0}\nBig Ship: {5:0.0}x{6:0.0}\nBloom Quality: {7}",
                                                        Resources.Instructions,
                                                        _ship.Position.X * 100,
                                                        _ship.Position.Y * 100,
                                                        _shipDeux.Position.X * 100,
                                                        _shipDeux.Position.Y * 100,
                                                        _bigShip.Position.X * 100,
                                                        _bigShip.Position.Y * 100,
                                                        ((Gorgon2DBloomEffect)_resources.Effects["bloom"]).LowQuality ? "low" : "high");
            _renderer.DrawTextSprite(_textSprite);
        }

        float speed = _ship.LayerController is not null ? _ship.Speed : _shipDeux.Speed;
        float maxSpeed = renderArea.Width * 0.12f * speed;
        GorgonRectangleF speedRegion = new(renderArea.Left + 5, renderArea.Bottom - 30, renderArea.Width * 0.12f, 25);
        GorgonRectangleF speedBar = new(speedRegion.X, speedRegion.Y, maxSpeed, speedRegion.Height);
        _renderer.DrawFilledRectangle(speedRegion, new GorgonColor(GorgonColors.Black, 0.5f));
        _renderer.DrawFilledRectangle(speedBar, new GorgonColor(GorgonColors.Green * 0.85f, 0.3f));
        _renderer.DrawString("Speed", new Vector2(speedRegion.Left, speedRegion.Top - _helpFont.LineHeight + 5), _helpFont, GorgonColors.White);
        _renderer.DrawRectangle(speedRegion, new GorgonColor(GorgonColors.White, 0.3f));

        _renderer.End();

        GorgonExample.DrawStatsAndLogo(_renderer);
    }

    /// <summary>
    /// Function called during CPU idle time.
    /// </summary>
    /// <returns><b>true</b> to continue, <b>false</b> to stop.</returns>
    private static bool Idle()
    {
        _shipDeux.UserInput(_keyboard.KeyStates);
        _shipDeux.Update();

        _ship.UserInput(_keyboard.KeyStates);
        _ship.Update();

        _sceneRenderer.Render();

        // Render the final image back to our swap chain.
        _graphics.SetRenderTarget(_screen.RenderTargetView);
        _renderer.Begin(Gorgon2DBatchState.NoBlend);

        // Copy our final rendering into the swap chain target.
        // Because we use a fixed render target size for our rendering, we'll need to stretch the view to accomodate the swap chain area. 
        // We also need to ensure our aspect ratio stays correct. Because of this, you may see black bars on the top/bottom or left/right of the swap chain image.
        float newWidth = _screen.Width;
        float newHeight = (newWidth / _mainRtvAspect.X);

        if (newHeight > _screen.Height)
        {
            newHeight = _screen.Height;
            newWidth = (newHeight * _mainRtvAspect.X);
        }

        GorgonRectangleF destRegion = new(_screen.Width * 0.5f - newWidth * 0.5f, _screen.Height * 0.5f - newHeight * 0.5f, newWidth, newHeight);

        _screen.RenderTargetView.Clear(GorgonColors.Black);
        _renderer.DrawFilledRectangle(destRegion,
            GorgonColors.White,
            _mainSrv,
            new GorgonRectangleF(0, 0, 1, 1),
            textureSampler: GorgonSamplerState.Default);
        _renderer.End();

        RenderGui(destRegion);

        _screen.Present();
        return true;
    }

    /// <summary>
    /// Function to initialize the scene to render.
    /// </summary>
    private static void SetupScene()
    {
        // This is our camera used to map our objects into relative space.
        // Because it's an Ortho camera, it doesn't really know how to handle aspect ratios, so we'll have to adjust for the current ratio.
        GorgonOrthoCamera camera = new(_graphics, new Vector2(2, 2), 0.1f, 5000)
        {
            Anchor = new Vector2(0.5f, 0.5f),
            AllowUpdateOnResize = false    // Since we're using a custom coordinate set, we don't want to change it automatically when we resize the swap chain.
        };
        // That means we are responsible for any adjustments required on resize.

        // Scenes are composed of layers, which are composed of sprites/meshes/etc...
        // Each layer is used to give an illusion of depth by employing parallax scrolling.  While we could use the Gorgon camera to do this, it will not give an illusion of depth 
        // because it is an orthographic projection camera which employs constant Z values.
        //
        // Each layer is added in the order in which it will be rendered.
        BgStarLayer bgLayer = LayerBuilder.GetBackgroundLayer(_renderer, _resources);
        SpritesLayer sunLayer = LayerBuilder.GetSunLayer(_renderer, _resources);
        PlanetLayer planetLayer = LayerBuilder.GetPlanetLayer(_graphics, _resources);
        SpritesLayer shipLayer = LayerBuilder.GetShipLayer(_renderer, _resources);
        SpritesLayer shipLayerDeux = LayerBuilder.GetShipLayer(_renderer, _resources);

        // Assign our rendering camera so that we have a means of projecting our coordinate information.
        sunLayer.Camera =
        planetLayer.Camera =
        shipLayerDeux.Camera =
        shipLayer.Camera = camera;

        // Link the light from the sun layer to only show on the planet layer.
        // With this particular setup, we can set a light to be parented on one layer, so that it will inherit its transformations, and affect a completely different layer.
        // In this example, our sun is distant, so when the layer camera moves, it should move slowly.  But in reality, the light source is fairly close to the planet to give 
        // a subtle (or not, depending on bloom) lighting effect.
        sunLayer.Lights[0].Layers.Add(planetLayer);

        // Here we'll set up the layer camera controller. This is what will give the illusion of movement across space by shifting the planet, sun, and other sprites.
        LayerCamera controller = new([bgLayer, sunLayer, planetLayer, shipLayerDeux, shipLayer]);

        // This is our renderer which is responsible the drawing the layers and applying any post processing effects.
        _sceneRenderer = new SceneRenderer(_renderer, _resources, _mainRtv, controller, camera);
        _sceneRenderer.LoadResources();

        // Our player ship.  Since this one is linked to the layer camera controller, we can use our keyboard to move around the scene.
        _ship = new Ship(shipLayer)
        {
            Position = new Vector2(120.0f, 75.0f),
            Angle = -45.0f,
            LayerController = controller
        };
        _ship.LoadResources();

        // A secondary ship. Just here to look pretty.
        _shipDeux = new Ship(shipLayerDeux)
        {
            Position = new Vector2(120.3f, 74.8f),
            Angle = -78.0f,
            Ai = new DummyAi()
        };
        _shipDeux.LoadResources();

        // Create a big ship for some scene variety.
        _bigShip = new BigShip(shipLayerDeux)
        {
            Position = new Vector2(120.3f, 74.5f),
            Angle = -80.0f
        };
        _bigShip.LoadResources();
    }

    /// <summary>
    /// Function to initialize the application.
    /// </summary>        
    private static async Task InitializeAsync(FormMain window)
    {
        try
        {
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);
            GorgonExample.PlugInLocationDirectory = new DirectoryInfo(ExampleConfig.Default.PlugInLocation);

            // Load our packed file system plug-in.
            window.UpdateStatus("Loading plugins...");

            _assemblyCache = new GorgonMefPlugInCache(GorgonApplication.Log);

            window.UpdateStatus("Initializing graphics...");

            // Retrieve the list of video adapters. We can do this on a background thread because there's no interaction between other threads and the 
            // underlying D3D backend yet.
            IReadOnlyList<IGorgonVideoAdapterInfo> videoDevices = await Task.Run(() => GorgonGraphics.EnumerateAdapters(log: GorgonApplication.Log));

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
                                              Name = "Gorgon2D Space Scene Example"
                                          });

            // Create a secondary render target for our scene. We use 16 bit floating point for the effect fidelity.
            // We'll lock our resolution to 1920x1080 (pretty common resolution for most people).
            _mainRtv = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo((int)_baseResolution.X, (int)_baseResolution.Y, BufferFormat.R16G16B16A16_Float)
            {
                Name = "Main RTV",
                Binding = TextureBinding.ShaderResource
            });
            _mainSrv = _mainRtv.GetShaderResourceView();
            _mainRtvAspect = _mainRtv.Width < _mainRtv.Height ? new Vector2(1, (float)_mainRtv.Height / _mainRtv.Width) : new Vector2((float)_mainRtv.Width / _mainRtv.Height, 1);

            // Initialize the renderer so that we are able to draw stuff.
            _renderer = new Gorgon2D(_graphics);

            // Set up our raw input.
            _input = new GorgonRawInput(window, GorgonApplication.Log);
            _keyboard = new GorgonRawKeyboard();
            _keyboard.KeyUp += Keyboard_KeyUp;
            _input.RegisterDevice(_keyboard);

            GorgonExample.LoadResources(_graphics);

            // Now for the fun stuff, load our asset resources. We can load this data by mounting a directory (which I did while developing), or use a packed file.
            //
            // The resource manager will hold all the data we need for the scene. Including 3D meshes, post processing effects, etc... 
            _resources = new ResourceManagement(_renderer, _assemblyCache);
            _resources.Load(Path.Combine(GorgonExample.GetResourcePath(@"FileSystems").FullName, "SpaceScene.gorPack"));

            window.UpdateStatus("Loading resources...");
            await _resources.LoadResourcesAsync();

            SetupScene();

            // Build up a font to use for rendering any GUI text.
            _helpFont = GorgonExample.Fonts.GetFont(new GorgonFontInfo("Segoe UI", 10.0f, GorgonFontHeightMode.Points)
            {
                Name = "Segoe UI 10pt",
                OutlineSize = 2,
                Characters = (Resources.Instructions + "yQHS:1234567890x").Distinct().ToArray(),
                FontStyle = GorgonFontStyle.Bold,
                AntiAliasingMode = GorgonFontAntiAliasMode.AntiAlias,
                OutlineColor1 = GorgonColors.Black,
                OutlineColor2 = GorgonColors.Black
            });

            _textSprite = new GorgonTextSprite(_helpFont)
            {
                Position = new Vector2(0, 64),
                DrawMode = TextDrawMode.OutlinedGlyphs,
                Color = GorgonColors.Yellow
            };

            GorgonExample.ShowStatistics = true;

            // Set the idle here. We don't want to try and render until we're done loading.
            GorgonApplication.IdleMethod = Idle;
        }
        finally
        {
            GorgonExample.EndInit();
        }
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    public static void Main()
    {
        try
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.DoEvents();

            // This is necessary to get winforms to play nice with our background thread prior to running the application.
            WindowsFormsSynchronizationContext.AutoInstall = false;
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

            FormMain window = GorgonExample.Initialize(new GorgonPoint(ExampleConfig.Default.Resolution.X, ExampleConfig.Default.Resolution.Y), "Space Scene",
                async (sender, _) => await InitializeAsync(sender as FormMain));

            GorgonApplication.Run(window);
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
        }
        finally
        {
            // Always perform your clean up.
            if (_keyboard is not null)
            {
                _keyboard.KeyUp -= Keyboard_KeyUp;
            }

            _helpFont?.Dispose();

            GorgonExample.UnloadResources();

            if (_keyboard is not null)
            {
                _input?.UnregisterDevice(_keyboard);
            }
            _input?.Dispose();
            _sceneRenderer?.Dispose();
            _resources?.Dispose();
            _renderer?.Dispose();
            _mainRtv?.Dispose();
            _screen?.Dispose();
            _graphics?.Dispose();
            _assemblyCache?.Dispose();
        }
    }
}
