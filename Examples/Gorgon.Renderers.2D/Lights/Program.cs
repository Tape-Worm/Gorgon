
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
// Created: July 28, 2018 11:33:52 AM
// 

using System.Numerics;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Renderers;
using Gorgon.Renderers.Cameras;
using Gorgon.Renderers.Lights;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Examples;

/// <summary>
/// The main entry point for the application
/// </summary>
/// <remarks>
/// This example shows how to use the G-Buffer and Lighting effect to provide lighting for your sprites
/// </remarks>
static class Program
{

    // The primary graphics interface.
    private static GorgonGraphics _graphics;
    // The main "screen" for the application.
    private static GorgonSwapChain _screen;
    // The texture used for the logo.
    private static GorgonTexture2DView _backgroundLogoTexture;
    // The texture used for the torch.
    private static GorgonTexture2DView _torchTexture;
    // Our 2D renderer.
    private static Gorgon2D _renderer;
    // Sprite used to draw our logo.
    private static GorgonSprite _logoSprite;
    // Sprite used to draw our logo.
    private static GorgonSprite _torchSprite;
    // Our lighting effect.
    private static Gorgon2DLightingEffect _lightEffect;
    // The g-buffer which contains our buffers used for lighting.
    private static Gorgon2DGBuffer _gbuffer;
    // Flag to indicate the direction of light brightness.
    private static bool _lightBrightDir = true;
    // The actual light color value.
    private static float _lightValue = 0.5f;
    // The min and max bounds for the lighting values.
    private static readonly GorgonRange<float> _lightMinMax = new(0.75f, 1.0f);
    // The timer for switching torch frames.
    private static IGorgonTimer _torchFrameTime;
    // The point light that we control.
    private static GorgonPointLight _light;
    private static GorgonOrthoCamera _camera;
    // Flag to indicate we should rotate the logo.
    private static bool _rotate;

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

        if (_torchFrameTime.Milliseconds > 175)
        {
            _torchSprite.TextureRegion =
                _torchTexture.Texture.ToTexel(new GorgonRectangle(_torchSprite.TextureRegion.Left == 0 ? 56 : 0, 0, 55, _torchTexture.Height));
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

        _light.Color = new GorgonColor((_lightValue * 253.0f) / 255.0f, (_lightValue * 248.0f) / 255.0f, (_lightValue * 230.0f) / 255.0f);
        _light.SpecularPower = (1.0f - (_lightValue / 1.2f)) * 256;

        // Clear our buffers each frame.
        _screen.RenderTargetView.Clear(GorgonColors.Black);
        _gbuffer.ClearGBuffer();

        // Gorgon uses a G-Buffer, a collection of render targets used to facilitate lighting and other effects in order 
        // provide a basis for lighting. Other techniques could be implemented, some faster, some slower, however this one 
        // makes deferred lighting easier and is the better option for rendering multiple sprites affected by lighting.
        //
        // Gorgon's G-Buffer has one other critical advantage: It supports rotating sprites. Without special considerations 
        // rotation of sprites when lighting will not look correct because the normals need to be rotated as well. This 
        // is done in the G-Buffer on your behalf so your lighting will look correct no matter the orientation of the
        // sprite.

        // What are the normal and specular maps used for?
        // The Normal map is used to define how light behaves when it hits a surface in the scene. Without these, you will
        // not be able to see any depth in the image, and the lighting will not update as the light passes over the scene.
        // The Specular map is used to map out reflective spots on the image where the light will make the surface appear 
        // shiny or even wet. 

        // First we need to render the normal map, specular map and diffuse map to the buffer. For this example, these are
        // stored in texture array indices #2, #1, and #0 respectively. This is why we pass the 2 and 1 as the first
        // parameters for the gbuffer effect. We do not pass in the diffuse array index because the sprite being drawn
        // will have that value in its TextureArrayIndex property.
        _gbuffer.Begin(2, 1, camera: _camera);
        _renderer.DrawSprite(_logoSprite);
        _gbuffer.End();

        // Now we'll send it all to our screen using the lighting effect.
        _graphics.SetRenderTarget(_screen.RenderTargetView);
        _lightEffect.Render(_gbuffer, _screen.RenderTargetView);

        if (_rotate)
        {
            _logoSprite.Angle += 45 * GorgonTiming.Delta;
        }

        if (_logoSprite.Angle > 360.0f)
        {
            _logoSprite.Angle -= 360.0f;
        }

        // Draw the rest of the UI.
        _renderer.Begin();

        _renderer.DrawSprite(_torchSprite);

        _renderer.DrawString($"Specular Power: {_light.SpecularPower:0.0}\n" +
                             $"Light [c #{GorgonColors.CornFlowerBlue.ToHex()}]Z/z[/c]: {_light.Position.Z:0}\n" +
                             $"Camera Position: {_camera.Position.X:0}, {_camera.Position.Y:0} ([c #{GorgonColors.CornFlowerBlue.ToHex()}]W[/c], [c #{GorgonColors.CornFlowerBlue.ToHex()}]A[/c], [c #{GorgonColors.CornFlowerBlue.ToHex()}]S[/c], [c #{GorgonColors.CornFlowerBlue.ToHex()}]D[/c])\n" +
                             $"[c #{GorgonColors.CornFlowerBlue.ToHex()}]R[/c]otation: {(_rotate ? "Yes" : "No")}",
                             new Vector2(0, 64));
        _renderer.End();

        GorgonExample.DrawStatsAndLogo(_renderer);

        _screen.Present(1);
        return true;
    }

    /// <summary>
    /// Function to initialize the application.
    /// </summary>
    /// <returns>The main window for the application.</returns>
    private static FormMain Initialize()
    {
        GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);

        FormMain window = GorgonExample.Initialize(new GorgonPoint(ExampleConfig.Default.Resolution.X, ExampleConfig.Default.Resolution.Y), "Lights");

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
                                              Name = "Gorgon2D Effects Example Swap Chain"
                                          });

            _screen.SwapChainResized += Screen_SwapChainResized;

            _camera = new GorgonOrthoCamera(_graphics, new Vector2(_screen.Width, _screen.Height))
            {
                Anchor = new Vector2(0.5f, 0.5f),
                Position = new Vector3(0, 0, -70),
                AllowUpdateOnResize = false
            };

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

            // Initialize the renderer so that we are able to draw stuff.
            _renderer = new Gorgon2D(_graphics);

            _gbuffer = new Gorgon2DGBuffer(_renderer, _screen.Width, _screen.Height);

            _logoSprite = new GorgonSprite
            {
                Texture = _backgroundLogoTexture,
                Bounds = new GorgonRectangleF(0, 0, _backgroundLogoTexture.Width, _backgroundLogoTexture.Height),
                TextureRegion = new GorgonRectangleF(0, 0, 1, 1),
                Anchor = new Vector2(0.5f, 0.5f)
            };

            _torchSprite = new GorgonSprite
            {
                Texture = _torchTexture,
                Bounds = new GorgonRectangleF(0, 0, 55, _torchTexture.Height),
                TextureRegion = _torchTexture.Texture.ToTexel(new GorgonRectangle(0, 0, 55, _torchTexture.Height)),
            };

            // This is the light we'll control with the mouse.
            _light = new GorgonPointLight("Main Point Light")
            {
                ConstantAttenuation = 0,
                LinearAttenuation = 0.007f,
                QuadraticAttenuation = 0.0002f,
                Color = GorgonColors.White,
                SpecularEnabled = true,
                SpecularPower = 6.0f,
                Intensity = 2,
                SpecularIntensity = 4,
                Position = new Vector3(0, 0, _camera.Position.Z),
                Range = 4000
            };

            // Create the effect that will light up our sprite(s).
            _lightEffect = new Gorgon2DLightingEffect(_renderer)
            {
                AmbientColor = new GorgonColor(0.06f, 0.06f, 0.06f)
            };

            _lightEffect.Lights.Add(_light);

            GorgonExample.LoadResources(_graphics);

            window.MouseMove += Window_MouseMove;
            window.KeyDown += Window_KeyDown;
            window.MouseWheel += Window_MouseWheel;

            _torchFrameTime = new GorgonTimerQpc();

            return window;
        }
        finally
        {
            GorgonExample.EndInit();
        }
    }

    /// <summary>Handles the SwapChainResized event of the Screen control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SwapChainResizedEventArgs" /> instance containing the event data.</param>
    private static void Screen_SwapChainResized(object sender, SwapChainResizedEventArgs e)
    {
        _gbuffer.Resize(e.Size.X, e.Size.Y);
        _camera.ViewDimensions = e.Size;
    }

    /// <summary>
    /// Windows the key down.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
    private static void Window_KeyDown(object sender, KeyEventArgs e)
    {
        Control window = (Control)sender;
        Point cursor = window.PointToClient(Cursor.Position);

        switch (e.KeyCode)
        {
            case Keys.Escape:
                GorgonApplication.Quit();
                return;
            case Keys.R:
                _rotate = !_rotate;
                break;
            case Keys.C:
                _lightEffect.CheckLightDepth = !_lightEffect.CheckLightDepth;
                break;
            case Keys.Z:
                _light.Position =
                    !e.Shift
                        ? new Vector3(_light.Position.X, _light.Position.Y, _light.Position.Z + 1.0f)
                        : new Vector3(_light.Position.X, _light.Position.Y, _light.Position.Z - 1.0f);
                break;
            case Keys.W:
                _camera.Position = new Vector3(_camera.Position.X, _camera.Position.Y - 10, _camera.Position.Z);
                _light.Position = new Vector3(cursor.X + _camera.ViewableRegion.X + _camera.Position.X, cursor.Y + _camera.ViewableRegion.Top + _camera.Position.Y, _light.Position.Z);
                break;
            case Keys.S:
                _camera.Position = new Vector3(_camera.Position.X, _camera.Position.Y + 10, _camera.Position.Z);
                _light.Position = new Vector3(cursor.X + _camera.ViewableRegion.X + _camera.Position.X, cursor.Y + _camera.ViewableRegion.Top + _camera.Position.Y, _light.Position.Z);
                break;
            case Keys.A:
                _camera.Position = new Vector3(_camera.Position.X - 10, _camera.Position.Y, _camera.Position.Z);
                _light.Position = new Vector3(cursor.X + _camera.ViewableRegion.X + _camera.Position.X, cursor.Y + _camera.ViewableRegion.Top + _camera.Position.Y, _light.Position.Z);
                break;
            case Keys.D:
                _camera.Position = new Vector3(_camera.Position.X + 10, _camera.Position.Y, _camera.Position.Z);
                _light.Position = new Vector3(cursor.X + _camera.ViewableRegion.X + _camera.Position.X, cursor.Y + _camera.ViewableRegion.Top + _camera.Position.Y, _light.Position.Z);
                break;
        }
    }

    /// <summary>Handles the MouseWheel event of the Window control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
    private static void Window_MouseWheel(object sender, MouseEventArgs e)
    {
        if (e.Delta < 0)
        {
            _logoSprite.Angle -= 1.0f;
        }
        else if (e.Delta > 0)
        {
            _logoSprite.Angle += 1.0f;
        }
    }

    /// <summary>
    /// Handles the MouseMove event of the Window control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
    private static void Window_MouseMove(object sender, MouseEventArgs e)
    {
        _light.Position = new Vector3(e.X + _camera.ViewableRegion.X + _camera.Position.X, e.Y + _camera.ViewableRegion.Top + _camera.Position.Y, _light.Position.Z);
        _torchSprite.Position = new Vector2(e.X - 11, e.Y - 23);
    }

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

            _gbuffer?.Dispose();
            _lightEffect?.Dispose();
            _renderer?.Dispose();
            _backgroundLogoTexture?.Dispose();
            _screen?.Dispose();
            _graphics?.Dispose();
        }
    }
}
