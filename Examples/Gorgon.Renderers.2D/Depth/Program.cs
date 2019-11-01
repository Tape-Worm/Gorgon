#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: May 9, 2019 5:34:12 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.IO;
using Gorgon.IO.Providers;
using Gorgon.Math;
using Gorgon.PlugIns;
using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// This is an updated version of the "DeepAsAPuddle" example from Gorgon v1.x.
    /// 
    /// This example shows how to use the depth buffer in conjunction with 2D graphics to provide the illusion of a scene with 
    /// depth by having a character walk behind an obstacle (similar to old Sierra/Lucasarts adventure games).
    /// 
    /// By using a depth buffer, the order in which we draw our sprites no longer matters because the Depth property is used to 
    /// determine how far "in" a sprite is within the scene. For example, 2 sprites, A and B, where A has a depth of 0.2 and B 
    /// has a depth of 0.1 will always have B render before A (depending on depth stencil state).
    /// 
    /// Be aware there are limitations with this trick. If you render a sprite with translucency with a lower depth value before 
    /// a sprite with a high depth value, the pixels will overwrite what's behind it.
    /// </summary>
    static class Program
    {
        #region Enums.
        /// <summary>
        /// The name of an animation.
        /// </summary>
        private enum AnimationName
        {
            WalkUp = 0,
            Turn = 1,
            WalkLeft = 2
        }
        #endregion

        #region Variables.
        // The primary graphics interface.
        private static GorgonGraphics _graphics;
        // The main "screen" for the application.
        private static GorgonSwapChain _screen;
        // The depth buffer for the screen.
        private static GorgonDepthStencil2DView _depthBuffer;
        // Our 2D renderer.
        private static Gorgon2D _renderer;
        // The list of textures for the sprite.
        private static readonly List<GorgonTexture2D> _textures = new List<GorgonTexture2D>();
        // The size of the tiled screen.
        private static DX.Size2 _tileSize;
        // The tile used for the snow layer.
        private static GorgonSprite _snowTile;
        // The sprite for the guy.
        private static GorgonSprite _guySprite;
        // The icicle sprite.
        private static GorgonSprite _icicle;
        // The position of the guy.
        private static DX.Vector2 _guyPosition;
        // Up animation.
        private static Dictionary<AnimationName, IGorgonAnimation> _animations = new Dictionary<AnimationName, IGorgonAnimation>();
        // The animation controller.
        private static GorgonSpriteAnimationController _controller;
        // The current animation.
        private static AnimationName _current;
        // The cache for our plug in assemblies.
        private static GorgonMefPlugInCache _assemblyCache;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to draw the background layer.
        /// </summary>
        private static void DrawBackground()
        {
            for (int y = 0; y < _tileSize.Height + 1; y++)
            {
                for (int x = 0; x < _tileSize.Width + 1; x++)
                {
                    _snowTile.Position = new DX.Vector2(x * _snowTile.ScaledSize.Width, y * _snowTile.ScaledSize.Height);
                    _renderer.DrawSprite(_snowTile);
                }
            }
        }

        /// <summary>
        /// Function to draw an obstacle that we can move behind.
        /// </summary>
        private static void DrawIcicle()
        {
            _icicle.Position = new DX.Vector2(((_tileSize.Width - 2) / 2) * _snowTile.ScaledSize.Width, ((_tileSize.Height - 2) / 2) * _snowTile.ScaledSize.Height);
            _renderer.DrawSprite(_icicle);
        }

        /// <summary>
        /// Function to draw our character.
        /// </summary>
        private static void DrawGuy()
        {
            _guySprite.Position = _guyPosition;
            _renderer.DrawSprite(_guySprite);
        }

        /// <summary>
        /// Function used to transition the animation for the guy.
        /// </summary>
        private static void AnimationTransition()
        {
            // We'll cycle between different animations depending on where we are on the screen.
            switch (_current)
            {
                case AnimationName.WalkUp:
                    if (_controller.State != AnimationState.Playing)
                    {
                        _controller.Play(_guySprite, _animations[_current]);
                    }
                    _guyPosition.Y -= (_guySprite.ScaledSize.Height / 4.0f) * GorgonTiming.Delta;

                    if (_guySprite.Position.Y < _icicle.Position.Y + 16)
                    {
                        _controller.Stop();
                        _current = AnimationName.Turn;

                        // Make it so the guy will appear behind the icicle.
                        _guySprite.Depth = _icicle.Depth + 0.1f;

                        _controller.Play(_guySprite, _animations[_current]);

                    }
                    break;
                case AnimationName.Turn:
                    if (_controller.State == AnimationState.Stopped)
                    {
                        _current = AnimationName.WalkLeft;
                    }
                    break;
                case AnimationName.WalkLeft:
                    if (_controller.State != AnimationState.Playing)
                    {
                        _controller.Play(_guySprite, _animations[_current]);
                    }
                    _guyPosition.X -= (_guySprite.ScaledSize.Width / 2.25f) * GorgonTiming.Delta;

                    if (_guySprite.Position.X < -_guySprite.ScaledSize.Width * 1.25f)
                    {
                        _controller.Stop();

                        // If we reach the extreme left of the screen (and we're off screen), then move to the right side and continue this animation.						
                        if (_guyPosition.Y < _icicle.Position.Y + _snowTile.ScaledSize.Height)
                        {
                            _guyPosition = new DX.Vector2(_screen.Width + _guySprite.ScaledSize.Width * 1.25f, _icicle.Position.Y + _icicle.ScaledSize.Height / 2.0f);
                        }
                        else
                        {
                            // Otherwise, reset and start over.
                            _current = AnimationName.WalkUp;
                            _guyPosition = new DX.Vector2(_screen.Width / 2 + _guySprite.ScaledSize.Width * 1.25f, _screen.Height + _snowTile.ScaledSize.Height);
                        }

                        // Ensure that the guy's depth value is less than the icicle so he'll appear in front of it.
                        _guySprite.Depth = _icicle.Depth - 0.1f;
                    }
                    break;
            }
        }

        /// <summary>
        /// Function called when the application goes into an idle state.
        /// </summary>
        /// <returns><b>true</b> to continue executing, <b>false</b> to stop.</returns>
        private static bool Idle()
        {
            _tileSize = new DX.Size2((int)(_screen.Width / _snowTile.ScaledSize.Width), (int)(_screen.Height / _snowTile.ScaledSize.Height));

            _screen.RenderTargetView.Clear(GorgonColor.White);
            _depthBuffer.Clear(1.0f, 0);

            // We have to pass in a state that allows depth writing and testing. Otherwise the depth buffer won't be used.
            _renderer.Begin(Gorgon2DBatchState.DepthEnabled);

            DrawBackground();

            // Note that the order that we draw here is not important since the depth buffer will sort on our behalf.
            // As mentioned in the description for the example, alpha blending doesn't work all that well with this 
            // trick. You'll notice this when the guy walks behind the icicle as the icicle completely obscures the 
            // guy even though the icicle has alpha translucency.
            DrawIcicle();

            DrawGuy();

            _renderer.End();

            GorgonExample.DrawStatsAndLogo(_renderer);

            _controller.Update();

            AnimationTransition();

            _screen.Present(1);
            return true;
        }

        /// <summary>
        /// Function to build the animations.
        /// </summary>
        /// <param name="sprites">The list of sprites loaded from the file system.</param>
        private static void BuildAnimations(IReadOnlyDictionary<string, GorgonSprite> sprites)
        {
            var animBuilder = new GorgonAnimationBuilder();

            // Extract the sprites that have the animation frames.
            // We'll use the name of the sprite to determine the type of animation and ordering.
            // If we had an animation editor in the editor application, this would be a lot easier, but for now we'll have to do this.
            // If I create an editor, I'll try to replace this code with the animations in the editor file system.
            GorgonSprite[] upFrames = sprites.OrderBy(item => item.Key)
                                              .Where(item => item.Key.StartsWith("Guy_Up_", StringComparison.OrdinalIgnoreCase))
                                              .Select(item => item.Value)
                                              .ToArray();

            IEnumerable<GorgonSprite> turnFrames = sprites.OrderBy(item => item.Key)
                                                           .Where(item => item.Key.StartsWith("Guy_Turn_", StringComparison.OrdinalIgnoreCase))
                                                           .Select(item => item.Value);

            GorgonSprite[] walkLeftFrames = sprites.OrderBy(item => item.Key)
                                                    .Where(item => item.Key.StartsWith("Guy_Left_", StringComparison.OrdinalIgnoreCase))
                                                    .Select(item => item.Value)
                                                    .ToArray();

            float time = 0;

            // Build animation for walking up.
            for (int i = 0; i < upFrames.Length; ++i)
            {
                GorgonSprite sprite = upFrames[i];

                animBuilder.Edit2DTexture("Texture")
                            .SetKey(new GorgonKeyTexture2D(time, sprite.Texture, sprite.TextureRegion, 0))
                            .EndEdit();

                time += 0.15f;
            }

            // Reverse the animation so we don't get popping as it loops.
            for (int i = upFrames.Length - 2; i >= 1; --i)
            {
                GorgonSprite sprite = upFrames[i];

                animBuilder.Edit2DTexture("Texture")
                            .SetKey(new GorgonKeyTexture2D(time, sprite.Texture, sprite.TextureRegion, 0))
                            .EndEdit();

                time += 0.15f;
            }

            _animations[AnimationName.WalkUp] = animBuilder.Build("Walk Up");
            _animations[AnimationName.WalkUp].IsLooped = true;


            // Build animation for turning left.
            time = 0;
            animBuilder.Clear();
            foreach (GorgonSprite sprite in turnFrames)
            {
                animBuilder.Edit2DTexture("Texture")
                            .SetKey(new GorgonKeyTexture2D(time, sprite.Texture, sprite.TextureRegion, 0))
                            .EndEdit();

                time += 0.20f;
            }
            _animations[AnimationName.Turn] = animBuilder.Build("Turn Left");

            // Build animation for walking left.
            time = 0;
            animBuilder.Clear();
            for (int i = 0; i < walkLeftFrames.Length; ++i)
            {
                GorgonSprite sprite = walkLeftFrames[i];
                animBuilder.Edit2DTexture("Texture")
                            .SetKey(new GorgonKeyTexture2D(time, sprite.Texture, sprite.TextureRegion, 0))
                            .EndEdit();

                time += 0.15f;
            }

            _animations[AnimationName.WalkLeft] = animBuilder.Build("Walk Left");
            _animations[AnimationName.WalkLeft].IsLooped = true;

            // Finally, we'll need a controller to play and update the animations over time.
            _controller = new GorgonSpriteAnimationController();
        }

        /// <summary>
        /// Function to initialize the application.
        /// </summary>
        /// <returns>The main window for the application.</returns>
        private static FormMain Initialize()
        {
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(Settings.Default.ResourceLocation);
            GorgonExample.PlugInLocationDirectory = new DirectoryInfo(Settings.Default.PlugInLocation);

            FormMain window = GorgonExample.Initialize(new DX.Size2(Settings.Default.Resolution.Width, Settings.Default.Resolution.Height), "Depth");

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
                                              new GorgonSwapChainInfo("Gorgon2D Depth Buffer Example")
                                              {
                                                  Width = Settings.Default.Resolution.Width,
                                                  Height = Settings.Default.Resolution.Height,
                                                  Format = BufferFormat.R8G8B8A8_UNorm
                                              });

                _depthBuffer = GorgonDepthStencil2DView.CreateDepthStencil(_graphics, new GorgonTexture2DInfo(_screen.RenderTargetView)
                {
                    Binding = TextureBinding.DepthStencil,
                    Format = BufferFormat.D24_UNorm_S8_UInt
                });

                // Tell the graphics API that we want to render to the "screen" swap chain.
                _graphics.SetRenderTarget(_screen.RenderTargetView, _depthBuffer);

                // Initialize the renderer so that we are able to draw stuff.
                _renderer = new Gorgon2D(_graphics);

                GorgonExample.LoadResources(_graphics);

                // Load our packed file system plug in.
                _assemblyCache = new GorgonMefPlugInCache(GorgonApplication.Log);
                _assemblyCache.LoadPlugInAssemblies(GorgonExample.GetPlugInPath().FullName, "Gorgon.FileSystem.GorPack.dll");
                IGorgonPlugInService plugIns = new GorgonMefPlugInService(_assemblyCache);

                // Load the file system containing our application data (sprites, images, etc...)
                IGorgonFileSystemProviderFactory providerFactory = new GorgonFileSystemProviderFactory(plugIns, GorgonApplication.Log);
                IGorgonFileSystemProvider provider = providerFactory.CreateProvider("Gorgon.IO.GorPack.GorPackProvider");
                IGorgonFileSystem fileSystem = new GorgonFileSystem(provider, GorgonApplication.Log);

                // We can load the editor file system directly.
                // This is handy for switching a production environment where your data may be stored 
                // as a compressed file, and a development environment where your data consists of loose 
                // files.				
                // fileSystem.Mount(@"D:\unpak\scratch\DeepAsAPuddle.gorPack\fs\");

                // For now though, we'll load the packed file.
                fileSystem.Mount(Path.Combine(GorgonExample.GetResourcePath(@"FileSystems").FullName, "Depth.gorPack"));

                // Get our sprites.  These make up the frames of animation for our Guy.
                // If and when there's an animation editor, we'll only need to create a single sprite and load the animation.
                IGorgonVirtualFile[] spriteFiles = fileSystem.FindFiles("/Sprites/", "*", true).ToArray();

                // Load our sprite data (any associated textures will be loaded as well).
                Dictionary<string, GorgonSprite> sprites = new Dictionary<string, GorgonSprite>(StringComparer.OrdinalIgnoreCase);

                for (int i = 0; i < spriteFiles.Length; i++)
                {
                    IGorgonVirtualFile file = spriteFiles[i];
                    (GorgonSprite sprite, GorgonTexture2D texture) = fileSystem.LoadSprite(_renderer, file.FullPath);

                    // The LoadSprite extension method will automatically find and load your associated texture if you're using 
                    // a Gorgon editor file system. So it's important that you leep track of your textures, disposing of just 
                    // the associated GorgonTexture2DView won't cut it here, so you'll need to dispose the actual texture resource 
                    // when you're done with it.
                    if (!_textures.Contains(texture))
                    {
                        _textures.Add(texture);
                    }

                    // At super duper resolution, the example graphics would be really hard to see, so we'll scale them up.
                    sprite.Scale = new DX.Vector2((_screen.Width / (_screen.Height / 2)) * 2.0f);
                    sprites[file.Name] = sprite;
                }

                _snowTile = sprites["Snow"];
                _snowTile.Depth = 0.5f;

                _icicle = sprites["Icicle"];
                _icicle.Depth = 0.2f;

                _guySprite = sprites["Guy_Up_0"];
                _guySprite.Depth = 0.1f;
                _guyPosition = new DX.Vector2(_screen.Width / 2 + _guySprite.ScaledSize.Width * 1.25f, _screen.Height / 2 + _guySprite.ScaledSize.Height);

                BuildAnimations(sprites);
            }
            finally
            {
                GorgonExample.EndInit();
            }

            return window;
        }
        #endregion

        #region Constructor/Finalizer.
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

                foreach (GorgonTexture2D texture in _textures)
                {
                    texture?.Dispose();
                }

                _renderer?.Dispose();
                _depthBuffer?.Dispose();
                _screen?.Dispose();
                _graphics?.Dispose();
                _assemblyCache?.Dispose();
            }
        }
        #endregion
    }
}
