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

using System.Numerics;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Examples;

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
    // The batch state for drawing our render target.
    private static Gorgon2DBatchState _targetBatchState;
    // Player for our mp3.
    private static readonly AudioPlayback _mp3Player = new();
    // The task used while playing audio.
    private static Task _audioTask;
    #endregion

    #region Methods.
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Function to build our animation... like an 80's music video... this is going to be ugly.
    /// </summary>
    private static void MakeAn80sMusicVideo()
    {
        var animBuilder = new GorgonAnimationBuilder();

        // When building animations, you can create your own animation tracks to handle which properties on the 
        // sprite get updated. These often correspond to property names, so passing "Position" as the name will 
        // update the Position property on the sprite.

        // Set up some scaling...            
        animBuilder.EditVector2("Scale")
                   .SetInterpolationMode(TrackInterpolationMode.Spline)
                   .SetKey(new GorgonKeyVector2(0, new Vector2(1, 1)))
                   .SetKey(new GorgonKeyVector2(2, new Vector2(0.5f, 0.5f)))
                   .SetKey(new GorgonKeyVector2(4, new Vector2(4.0f, 4.0f)))
                   .SetKey(new GorgonKeyVector2(6, new Vector2(1, 1)))
                   .EndEdit()
                   // Set up some positions...
                   .EditVector2("Position")
                   .SetInterpolationMode(TrackInterpolationMode.Spline)
                   .SetKey(new GorgonKeyVector2(0, new Vector2(_screen.Width / 2.0f, _screen.Height / 2.0f)))
                   .SetKey(new GorgonKeyVector2(2, new Vector2(200, 220)))
                   .SetKey(new GorgonKeyVector2(3, new Vector2(_screen.Width - 2, 130)))
                   .SetKey(new GorgonKeyVector2(4, new Vector2(255, _screen.Height - _metal.Height)))
                   .SetKey(new GorgonKeyVector2(5, new Vector2(200, 180)))
                   .SetKey(new GorgonKeyVector2(6, new Vector2(180, 160)))
                   .SetKey(new GorgonKeyVector2(7, new Vector2(150, 180)))
                   .SetKey(new GorgonKeyVector2(8, new Vector2(150, 160)))
                   .SetKey(new GorgonKeyVector2(9, new Vector2(_screen.Width / 2, _screen.Height / 2)))
                   .EndEdit()
                   // Set up some colors...By changing the alpha, we can simulate a motion blur effect.
                   .EditColor("Color")
                   .SetInterpolationMode(TrackInterpolationMode.Spline)
                   .SetKey(new GorgonKeyGorgonColor(0, GorgonColor.Black))
                   .SetKey(new GorgonKeyGorgonColor(2, new GorgonColor(GorgonColor.RedPure, 0.25f)))
                   .SetKey(new GorgonKeyGorgonColor(4, new GorgonColor(GorgonColor.GreenPure, 0.5f)))
                   .SetKey(new GorgonKeyGorgonColor(6, new GorgonColor(GorgonColor.BluePure, 0.25f)))
                   .SetKey(new GorgonKeyGorgonColor(8, new GorgonColor(GorgonColor.LightCyan, 0.25f)))
                   .SetKey(new GorgonKeyGorgonColor(10, new GorgonColor(GorgonColor.Black, 1.0f)))
                   .EndEdit()
                   // And finally, some MuchMusic/MTV style rotation... because.
                   .EditSingle("Angle")
                   .SetInterpolationMode(TrackInterpolationMode.Spline)
                   .SetKey(new GorgonKeySingle(0, 0))
                   .SetKey(new GorgonKeySingle(2, 180))
                   .SetKey(new GorgonKeySingle(4, 270))
                   .SetKey(new GorgonKeySingle(6, 0))
                   .EndEdit();

        IGorgonTrackKeyBuilder<GorgonKeyTexture2D> trackBuilder = animBuilder.Edit2DTexture("Texture");
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
        GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);
        FormMain window = GorgonExample.Initialize(new DX.Size2(ExampleConfig.Default.Resolution.Width, ExampleConfig.Default.Resolution.Height), "Animation");

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
                                              Name = "Gorgon2D Animation Example Swap Chain"
                                          });

            _target = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo(_screen.RenderTargetView)
            {
                Name = "Video Target",
                Binding = TextureBinding.ShaderResource
            });
            _targetView = _target.GetShaderResourceView();
            _target.Clear(GorgonColor.CornFlowerBlue);

            // Load our textures.
            var gif = new GorgonCodecGif(decodingOptions: new GorgonGifDecodingOptions
            {
                ReadAllFrames = true
            });

            // Find out how long to display each frame for.
            // We'll also convert it to milliseconds since GIF stores the delays as 1/100th of a second.
            _frameDelays = gif.GetFrameDelays(Path.Combine(GorgonExample.GetResourcePath(@"\Textures\Animation\").FullName, "metal.gif"))
                              .Select(item => item / 100.0f).ToArray();

            _metal = GorgonTexture2DView.FromFile(_graphics,
                                                  Path.Combine(GorgonExample.GetResourcePath(@"\Textures\Animation\").FullName, "metal.gif"),
                                                  gif,
                                                  new GorgonTexture2DLoadOptions
                                                  {
                                                      Name = @"Metal \m/",
                                                      Usage = ResourceUsage.Immutable,
                                                      Binding = TextureBinding.ShaderResource
                                                  });
            _trooper = GorgonTexture2DView.FromFile(_graphics,
                                                    Path.Combine(GorgonExample.GetResourcePath(@"\Textures\Animation\").FullName, "trooper.png"),
                                                    new GorgonCodecPng(),
                                                    new GorgonTexture2DLoadOptions
                                                    {
                                                        Name = "Trooper",
                                                        Usage = ResourceUsage.Immutable,
                                                        Binding = TextureBinding.ShaderResource
                                                    });

            GorgonExample.LoadResources(_graphics);

            _renderer = new Gorgon2D(_graphics);
            _animatedSprite = new GorgonSprite
            {
                Position = new Vector2(_screen.Width / 2, _screen.Height / 2),
                Size = new DX.Size2F(_metal.Width, _metal.Height),
                Anchor = new Vector2(0.5f, 0.5f)
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

            return window;
        }
        finally
        {
            GorgonExample.EndInit();
        }
    }

    /// <summary>
    /// Function to begin audio playback.
    /// </summary>
    private static async void PlayAudio()
    {
        if (_audioTask is not null)
        {
            await _audioTask;
        }

        _audioTask = _mp3Player.PlayMp3Async(Path.Combine(GorgonExample.ResourceBaseDirectory.FullName, "AnimationExample.xwma"));
    }

    /// <summary>
    /// Function to perform operations while the CPU is idle.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
    private static bool Idle()
    {
        // Set the initial background color, we won't be clearing again...
        _screen.RenderTargetView.Clear(GorgonColor.CornFlowerBlue);

        if (!_mp3Player.IsPlaying)
        {
            PlayAudio();
        }

        if (_animController.CurrentAnimation is null)
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
        _renderer.End();

        GorgonExample.DrawStatsAndLogo(_renderer);

        // We need to call this once per frame so the animation will transition properly.
        _animController.Update();

        _screen.Present(1);

        return true;
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
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            GorgonApplication.Run(Initialize(), Idle);

            _mp3Player?.Stop();
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
        }
        finally
        {
            GorgonExample.UnloadResources();

            _mp3Player.Dispose();
            _renderer?.Dispose();
            _trooper?.Dispose();
            _metal?.Dispose();
            _targetView?.Dispose();
            _target?.Dispose();
            _screen?.Dispose();
            _graphics?.Dispose();
        }
    }
}
