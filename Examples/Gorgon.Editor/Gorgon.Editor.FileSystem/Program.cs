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
// Created: July 18, 2018 4:04:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.IO.Providers;
using Gorgon.PlugIns;
using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// Our example entry point.
    /// </summary>
    static class Program
    {
        #region Variables.
        // The plug in assembly cache.
        private static GorgonMefPlugInCache _assemblyCache;
        // The core graphics functionality.
        private static GorgonGraphics _graphics;
        // Our swap chain that represents our "Screen".
        private static GorgonSwapChain _screen;
        // Our 2D renderer used to draw our sprites.
        private static Gorgon2D _renderer;        
        // The cache that will be used to manage the lifetimes of our texture resources.
        private static GorgonTextureCache<GorgonTexture2D> _textureCache;
        // The loader used to read content from the editor file system.
        private static IGorgonContentLoader _contentLoader;
        // The file system for the editor.
        private static IGorgonFileSystem _fileSystem;
        // The sprite to draw on the screen.
        private static GorgonSprite _dudeBro;
        // The animation for the sprite.
        private static IGorgonAnimation _animation;
        // The controller used to play and update the animation.
        private static GorgonSpriteAnimationController _animController;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to load the Gorgon pack file provider plugin.
        /// </summary>
        /// <returns>The file system provider.</returns>
        private static IGorgonFileSystemProvider LoadGorPackProvider()
        {
            // The Gorgon packed file provider plug in dll.
            const string gorPackDll = "Gorgon.FileSystem.GorPack.dll";
            // The name of the Gorgon packed file plugin.
            const string gorPackPlugInName = "Gorgon.IO.GorPack.GorPackProvider";

            // Like the zip file example, we'll just create the plugin infrastructure, grab the provider object 
            // and get rid of the plugin stuff since we won't need it again.
            _assemblyCache = new GorgonMefPlugInCache(GorgonApplication.Log);
            _assemblyCache.LoadPlugInAssemblies(GorgonExample.GetPlugInPath().FullName, gorPackDll);

            var plugIns = new GorgonMefPlugInService(_assemblyCache);
            return plugIns.GetPlugIn<GorgonFileSystemProvider>(gorPackPlugInName);
        }

        /// <summary>
        /// Function called during CPU idle time.
        /// </summary>
        /// <returns><b>true</b> to continue executing, <b>false</b> to stop.</returns>
        private static bool Idle()
        {
            _screen.RenderTargetView.Clear(GorgonColor.SteelBlue);

            _dudeBro.Position = new DX.Vector2(_screen.Width / 2, _screen.Height / 2);

            _renderer.Begin();
            _renderer.DrawSprite(_dudeBro);
            _renderer.End();

            _screen.Present(1);

            _animController.Update();

            return true;
        }

        /// <summary>
        /// Function to initialize the graphics objects and the primary window.
        /// </summary>
        private static async Task InitializeAsync(FormMain window)
        {
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(Settings.Default.ResourceLocation);
            GorgonExample.PlugInLocationDirectory = new DirectoryInfo(Settings.Default.PlugInLocation);

            try
            {
                // Load our packed file system plug in.
                window.UpdateStatus("Loading plugins...");

                // Load in the plug in that will allow us to read a packed file system.
                IGorgonFileSystemProvider provider = await Task.Run(LoadGorPackProvider);

                // Load the file system.
                _fileSystem = new GorgonFileSystem(provider);
                _fileSystem.Mount(Path.Combine(GorgonExample.GetResourcePath("FileSystems").FullName, "Gorgon.Editor.FileSystem.gorPack"));

                window.UpdateStatus("Initializing graphics...");
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
                                              new GorgonSwapChainInfo("Gorgon2D Gorgon.Editor Example Swap Chain")
                                              {
                                                  Width = Settings.Default.Resolution.Width,
                                                  Height = Settings.Default.Resolution.Height,
                                                  Format = BufferFormat.R8G8B8A8_UNorm
                                              });
                _screen.SwapChainResized += Screen_AfterSwapChainResized;
                // Tell the graphics API that we want to render to the "screen" swap chain.
                _graphics.SetRenderTarget(_screen.RenderTargetView);

                // Initialize the renderer so that we are able to draw stuff.
                _renderer = new Gorgon2D(_graphics);

                window.UpdateStatus("Loading resources...");
                GorgonExample.LoadResources(_graphics);

                // When using the editor file system we will load content that typically has dependencies upon texture resources.
                // Since these can be quite large, it is in our best interest to load them once and once only. To help manage the 
                // lifetime of these resources, we will use a texture cache so that we can hold the textures in memory for as long 
                // as we need them and loading textures will not introduce undue memory pressure. By only loading the texture only 
                // if it's absolutely necessary and returning the existing texture if it's already resident within the cache, we 
                // can keep the memory usage stable.
                //
                // The texture cache utilizes weak references so the garbage collector can pick up the textures when they're no 
                // longer needed, but, that may take time to do. To mitigate this, textures added to the cache are reference 
                // counted so that anything using the texture will increment the count, and when the user of the texture is 
                // finished with the texture resource, it can return it to the cache to decrement the reference count. When the 
                // count hits 0, the texture resource is unloaded from memory.
                //
                // Our editor content loader uses this texture caching functionality to load texture content that our content is 
                // dependent upon. 
                _textureCache = new GorgonTextureCache<GorgonTexture2D>(_graphics);

                // To create the content loader, we use an extension method on the IGorgonFileSystem object.
                // This loader will check to determine if the file system is an editor file system, and if not, it will exception.
                _contentLoader = _fileSystem.CreateContentLoader(_renderer, _textureCache);

                // Now that we have the loader, we can start bringing in the various resources for our example.
                //
                // Notice that our load methods are asynchronous. This is because it can take a bit of time to load large resources 
                // from the file system. This pushes the workload to the background so the appilcation can stay responsive while 
                // loading its data.
                //
                // Also, these content objects all use the same texture, and because we use a texture cache within the loader, the 
                // texture is only ever loaded one time and shared amongst the objects that use it.
                _animation = await _contentLoader.LoadAnimationAsync("/testBinAnim.gorAnim");
                _dudeBro = await _contentLoader.LoadSpriteAsync("/dudebro.gorsprite");

                // This sprite is kinda small, so we'll need to update its scale.
                _dudeBro.Scale = new DX.Vector2(8, 8);

                // Now that we have a sprite, and an animation to play against, we can set up the animation controller to play the 
                // animation when we enter our idle state.
                _animController = new GorgonSpriteAnimationController();
                _animController.Play(_dudeBro, _animation);

                GorgonApplication.IdleMethod = Idle;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                GorgonExample.EndInit();
            }
        }

        /// <summary>
        /// Handles the AfterSwapChainResized event of the Screen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SwapChainResizedEventArgs"/> instance containing the event data.</param>
        private static void Screen_AfterSwapChainResized(object sender, SwapChainResizedEventArgs e)
        {
        
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // This is necessary to get winforms to play nice with our background thread prior to running the application.
                WindowsFormsSynchronizationContext.AutoInstall = false;
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

                // Create the window, and size it to our resolution.
                FormMain window = GorgonExample.Initialize(new DX.Size2(Settings.Default.Resolution.Width, Settings.Default.Resolution.Height), "Gorgon.Editor.FileSystem - Loading content from an editor file system example.",
                        async (sender, _) => await InitializeAsync(sender as FormMain));

                GorgonApplication.Run(window);
            }
            catch (Exception ex)
            {
                GorgonExample.HandleException(ex);
            }
            finally
            {
                GorgonExample.UnloadResources();
                _textureCache?.Dispose();
                _renderer?.Dispose();
                _screen?.Dispose();
                _graphics?.Dispose();
                _assemblyCache?.Dispose();
            }
        }
        #endregion
    }
}
