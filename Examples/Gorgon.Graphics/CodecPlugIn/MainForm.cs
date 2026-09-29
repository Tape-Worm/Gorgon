
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
// Created: March 5, 2017 10:33:01 PM
// 

using System.Diagnostics;
using System.Numerics;
using Gorgon.Examples;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Graphics.Imaging.Codecs.Plugins;
using Gorgon.IO;
using Gorgon.Plugins;
using Gorgon.UI.WindowsForms;

namespace Graphics.Examples;

/// <summary>
/// Our main UI window for the example
/// </summary>
public partial class MainForm : System.Windows.Forms.Form
{
    // The cache that holds Plugin information.
    private readonly GorgonMefPluginCache _pluginCache;
    // The graphics object factory.
    private GorgonGraphicsFactory? _factory;
    // The main graphics interface.
    private GorgonGraphics? _graphics;
    // The swap chain to use.
    private GorgonSwapChain? _swap;
    // The blitter used to display the logo.
    private GorgonTextureBlitter? _blitter;
    // Image to display, loaded from our plugin.
    private IGorgonTextureView<GorgonTexture>? _texture;
    // The image in system memory.
    private IGorgonImage? _image;
    // Our custom codec loaded from the plugin.
    private IGorgonImageCodec? _customCodec;

    /// <summary>
    /// Function to handle a window resizing event.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void WindowResize(object? sender, EventArgs e)
    {
        Debug.Assert(_graphics is not null, "No graphics interface");
        Debug.Assert(_swap is not null, "No swap chain.");

        _swap.Resize(ClientSize.Width, ClientSize.Height);
    }

    /// <summary>
    /// Function called during idle time.
    /// </summary>
    /// <returns><b>true</b> to continue execution, <b>false</b> to stop.</returns>
    private bool Idle()
    {
        Debug.Assert(_graphics is not null, "No graphics interface");
        Debug.Assert(_swap is not null, "No swap chain.");
        Debug.Assert(_blitter is not null, "No blitter.");
        Debug.Assert(_texture is not null, "The texture was not created.");

        GorgonCommandList commandList = _graphics.GetCommandList("Main command list")
                                                 .ClearSwapChain(_swap, GorgonColors.White)
                                                 .SetViewport(new GorgonViewport(0, 0, ClientSize.Width, ClientSize.Height))
                                                 .SetScissorRectangle(new GorgonRectangle(0, 0, ClientSize.Width, ClientSize.Height))
                                                 .AddPresenter(_swap)
                                                 .SetRenderTarget(_swap.Target);

        Vector2 windowSize = new(ClientSize.Width, ClientSize.Height);
        Vector2 imageSize = new(_texture.Texture.Width, _texture.Texture.Height);

        // Calculate the scale between the images.
        Vector2 scale = new(windowSize.X / imageSize.X, windowSize.Y / imageSize.Y);

        // Only scale on a single axis if we don't have a 1:1 aspect ratio.
        if (scale.Y > scale.X)
        {
            scale.Y = scale.X;
        }
        else
        {
            scale.X = scale.Y;
        }

        // Scale the image.
        GorgonPoint size = new((int)(scale.X * imageSize.X), (int)(scale.Y * imageSize.Y));

        // Find the position.
        GorgonRectangle bounds = new((int)((windowSize.X / 2) - (size.X / 2)), (int)((windowSize.Y / 2) - (size.Y / 2)), size.X, size.Y);

        _blitter.Blit(commandList, _texture, bounds, sampler: GorgonSampler.Default(_graphics));

        GorgonExample.BlitLogo(commandList);

        _graphics.Submit(commandList);

        return true;
    }

    /// <summary>
    /// Function to load our useless image codec plugin.
    /// </summary>
    /// <returns><b>true</b> if successful, <b>false</b> if not.</returns>
    private bool LoadCodec()
    {
        const string PluginName = "Gorgon.Examples.TvImageCodecPlugin";

        // Load our plugin.
        _pluginCache.LoadPluginAssemblies(Application.StartupPath, "TVImageCodec.dll");

        // Activate the Plugin service.
        IGorgonPluginService pluginService = new GorgonMefPluginService(_pluginCache);

        // Find the Plugin.
        GorgonImageCodecPlugin? plugin = pluginService.GetPlugin<GorgonImageCodecPlugin>(PluginName);

        if ((plugin is null) || (plugin.Codecs.Count == 0))
        {
            return false;
        }

        // Normally you would enumerate the plugins, but in this case we know there's only one.
        _customCodec = plugin.CreateCodec(plugin.Codecs[0]);

        return _customCodec is not null;
    }

    /// <summary>
    /// Function to convert the image to use our custom codec.
    /// </summary>
    private void ConvertImage()
    {
        Debug.Assert(_image is not null, "No image available to convert.");
        Debug.Assert(_customCodec is not null, "Custom image codec not found.");
        Debug.Assert(_graphics is not null, "Graphics interface not found.");

        // The path to our image file for our custom codec.
        string tempPath = Path.ChangeExtension(Path.GetTempPath().FormatDirectory(Path.DirectorySeparatorChar) + Path.GetRandomFileName(), "tvImage");

        try
        {
            // Save the current texture using our useless new custom codec.
            _customCodec.Save(_image.BeginUpdate()
                                    .ConvertToFormat(BufferFormat.R8G8B8A8_UNorm)
                                    .EndUpdate(), tempPath);
            _image.Dispose();
            _texture?.Dispose();

            _image = _customCodec.FromFile(tempPath);

            _texture = IGorgonTextureView<GorgonTexture>.CreateTexture(_graphics, "Converted Texture", _image);
        }
        catch
        {
            // Clean up the new texture should we have an exception (this shouldn't happen, better safe than sorry).
            _image?.Dispose();
            throw;
        }
        finally
        {
            try
            {
                File.Delete(tempPath);
            }
            catch
            {
                // Intentionally left blank.
                // If we can't clean up the temp file, then it's no big deal right now.
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.KeyCode == Keys.Escape)
        {
            Close();
        }
    }

    /// <intheritdoc/>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        GorgonExample.UnloadResources();

        _pluginCache?.Dispose();
        _texture?.Dispose();
        _blitter?.Dispose();
        _swap?.Dispose();
        _graphics?.Dispose();
        _factory?.Dispose();
        _image?.Dispose();
    }

    /// <intheritdoc/>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        Cursor.Current = Cursors.WaitCursor;

        try
        {
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);

            // Load the custom codec.
            if (!LoadCodec())
            {
                GorgonDialogs.Error(this, "Unable to load the image codec plugin.");
                Application.Exit();
                return;
            }

            // Set up the graphics interface.
            // Find out which devices we have installed in the system.
            // Create the factory used to create our graphics functionality.
#if DEBUG
            _factory = new(GorgonGraphicsDebugFlags.SynchronizedCommandQueueValidation
                | GorgonGraphicsDebugFlags.GpuBasedValidation
                | GorgonGraphicsDebugFlags.GpuBasedStateTrackingValidation
                | GorgonGraphicsDebugFlags.ObjectTracking, log: GorgonExample.Log);
#else
            _factory = new(log: log);
#endif     
            IReadOnlyList<GorgonVideoAdapterInfo> deviceList = _factory.EnumerateAdapters();

            if (deviceList.Count == 0)
            {
                GorgonDialogs.Error(this, "There are no suitable video adapters available in the system. This example is unable to continue and will now exit.");
                Application.Exit();
                return;
            }

            _graphics = _factory.CreateGraphics(deviceList[0]);

            _swap = new GorgonSwapChain(_graphics,
                                        "Codec plugin SwapChain",
                                        Handle,
                                        new GorgonSwapChainInfo(ClientSize.Width, ClientSize.Height, BufferFormat.R8G8B8A8_UNorm));

            _blitter = new GorgonTextureBlitter(_graphics);

            // Load the image to use as a texture.
            IGorgonImageCodec png = new GorgonCodecPng();
            _image = png.FromFile(Path.Combine(GorgonExample.GetResourcePath(@"Textures\CodecPlugin\").FullName, "SourceTexture.png"));
                        
            GorgonExample.LoadResources(_graphics);

            ConvertImage();

            GorgonExample.EndInit();

            Resize += WindowResize;

            GorgonExample.Loop.Run(Idle);
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
            Application.Exit();
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainForm"/> class.
    /// </summary>
    public MainForm()
    {
        InitializeComponent();

        _pluginCache = new GorgonMefPluginCache(GorgonExample.Log);
    }

}
