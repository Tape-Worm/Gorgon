
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
// Created: August 23, 2018 4:42:41 PM
// 

using System.Numerics;
using System.Reflection;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
//using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
//using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI.WindowsForms;
using Gorgon.Graphics.Imaging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Gorgon.Examples;

/// <summary>
/// Common functionality for the example applications
/// </summary>
public static class GorgonExample
{
    private static IGorgonTextureView<GorgonTexture>? _logo;
    // The font factory to use.
    //private static GorgonFontFactory _factory;
    // The font used for statistics.
    //private static GorgonFont _statsFont;    
    // Blitter for displaying rendering.
    private static GorgonTextureBlitter? _blitter;
    // The string containing our statistics.
    private static readonly StringBuilder _statsText = new();
    // The main window for the application.
    private static FormMain? _mainForm;
    // The lazy instance of the log file.
    private readonly static Lazy<IGorgonLog> _lazyLog = new(() =>
    {
        Assembly assembly = Assembly.GetEntryAssembly() ?? throw new Exception("Can't get assembly");

        if (assembly is null)
        {
            return GorgonLog.NullLog;
        }

        AssemblyName assemblyName = assembly.GetName();
        Version version = assemblyName.Version ?? new Version(0, 0, 0, 0);

        GorgonTextFileLog log = new(assembly.GetName().Name ?? "Unknown Example", "Tape_Worm", version);
        log.LogStart(new GorgonComputerInfo());

        return log;
    }, LazyThreadSafetyMode.ExecutionAndPublication);
    // The application loop instance.
    private readonly static Lazy<GorgonApplicationLoop> _lazyLoop = new(() => GorgonApplicationLoop.Create(Log));

    /// <summary>
    /// Property to return the logging interface for the example.
    /// </summary>
    public static IGorgonLog Log => _lazyLog.Value;

    /// <summary>
    /// Property to return the application loop instance to use for this example.
    /// </summary>
    public static GorgonApplicationLoop Loop => _lazyLoop.Value;

    /// <summary>
    /// Property to set or return the path to the plugin directory.
    /// </summary>
    public static DirectoryInfo? PluginLocationDirectory
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the base directory for application resources.
    /// </summary>
    public static DirectoryInfo? ResourceBaseDirectory
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether statistics information should be shown.
    /// </summary>
    public static bool ShowStatistics
    {
        get;
        set;
    } = true;

    ///// <summary>
    ///// Property to return the font factory used to handle font creation for our examples.
    ///// </summary>
    //public static GorgonFontFactory Fonts => _factory;

    /// <summary>
    /// Function called when a key is pressed in the application.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private static void FormKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            _mainForm?.Close();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Function to retrieve the directory that contains the Plugins for an application.
    /// </summary>
    /// <returns>A directory information object for the Plugin path.</returns>
    public static DirectoryInfo GetPluginPath()
    {
        string path = PluginLocationDirectory?.FullName ?? throw new DirectoryNotFoundException();

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new IOException("No plugin path has been assigned.");
        }

        if (path.Contains("{0}"))
        {
#if DEBUG
            path = string.Format(path, "Debug");
#else
            path = string.Format(path, "Release");					
#endif
        }

        if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            path += Path.DirectorySeparatorChar.ToString();
        }

        DirectoryInfo result = new(Path.GetFullPath(path));

        Log.Print($"Example plug in path: {result.FullName}", LoggingLevel.Simple);

        return result;
    }

    /// <summary>
    /// Function to return the complete path to the application resources.
    /// </summary>
    /// <param name="extraPath">Extra path information to append.</param>
    /// <returns>A directory info object for the resource path.</returns>
    public static DirectoryInfo GetResourcePath(string extraPath)
    {
        string path = ResourceBaseDirectory?.FullName ?? throw new DirectoryNotFoundException();

        if (string.IsNullOrEmpty(path))
        {
            throw new IOException("The resource path was not specified.");
        }

        path = path.FormatDirectory(Path.DirectorySeparatorChar);

        // If this is a directory, then sanitize it as such.
        if (string.IsNullOrWhiteSpace(extraPath))
        {
            return ResourceBaseDirectory;
        }

        path += extraPath.FormatDirectory(Path.DirectorySeparatorChar);

        // Ensure that we have an absolute path.
        DirectoryInfo result = new(Path.GetFullPath(path));

        Log.Print($"Example resources path: {result.FullName}", LoggingLevel.Simple);

        return result;
    }

    /// <summary>
    /// Function to mark the end of the initialization.
    /// </summary>
    public static void EndInit()
    {
        _mainForm?.IsLoaded = true;

        Cursor.Current = Cursors.Default;
    }

    /// <summary>
    /// Function to blit the logo without the aid of the 2D renderer.
    /// </summary>
    /// <param name="list">The command list to use.</param>
    public static void BlitLogo(GorgonCommandList list)
    {
        Debug.Assert(_blitter is not null, "Blitter not loaded.");

        if ((_logo is null) || (list.RenderTargets.Length < 1))
        {
            list.Graphics.Log.PrintError("There is not render target to blit the logo on to.", LoggingLevel.Verbose);
            return;
        }

        GorgonRenderTargetView currentRtv = list.RenderTargets[0] ?? throw new GorgonException(GorgonResult.CannotRead, "There is no render target at render target slot 0 on the command list.");

        GorgonRectangle logoRegion = new(currentRtv.Texture.Width - _logo.Texture.Width - 5, currentRtv.Texture.Height - _logo.Texture.Height - 2, _logo.Texture.Width, _logo.Texture.Height);

        _blitter.Blit(list, _logo, logoRegion, sampler: GorgonSampler.Linear(list.Graphics), blendState: GorgonBlendState.Default);
    }

    /// <summary>
    /// Function to handle an exception should one occur.
    /// </summary>
    /// <param name="ex">The exception to handle.</param>
    public static void HandleException(Exception ex)
    {
        if (ex is null)
        {
            return;
        }

        Cursor.Show();
        ex.Handle(e => GorgonDialogs.Error(null, e, "There was an error running the application and it must now close.", "Error"), Log);
    }

    ///// <summary>
    ///// Function to draw the statistics and the logo for the example.
    ///// </summary>
    ///// <param name="renderer">The 2D renderer that we are using.</param>
    //public static void DrawStatsAndLogo(IGorgon2DFluent renderer)
    //{
    //    OLDE.GorgonGraphics graphics = _factory?.Graphics;
    //    OLDE.GorgonRenderTargetView currentRtv = graphics.RenderTargets[0];

    //    if ((currentRtv is null) || (_logoOlde is null) || (_statsFont is null))
    //    {
    //        return;
    //    }

    //    // We won't include these in the draw call count. 
    //    ref readonly OLDE.GorgonGraphicsStatistics stats = ref graphics.Statistics;

    //    _statsText.Length = 0;
    //    _statsText.AppendFormat("Average FPS: {0:0.0}\nFrame Delta: {1:0.00#} seconds\nDraw Call Count: {2} ({3} triangles)", GorgonTiming.AverageFPS, GorgonTiming.Delta, stats.DrawCallCount, stats.TriangleCount);

    //    Vector2 measure = _statsText.ToString().MeasureText(_statsFont, true);
    //    GorgonRectangleF statsRegion = new(0, 0, currentRtv.Width, measure.Y + 4);
    //    GorgonRectangleF logoRegion = new(currentRtv.Width - _logoOlde.Width - 5, currentRtv.Height - _logoOlde.Height - 2, _logoOlde.Width, _logoOlde.Height);

    //    renderer
    //        .Begin()
    //        .DrawIf(() => ShowStatistics, r =>
    //        {
    //            // Draw translucent window.
    //            r.DrawFilledRectangle(statsRegion, new GorgonColor(0, 0, 0, 0.5f));
    //            // Draw lines for separators.
    //            r.DrawLine(0, measure.Y + 3, currentRtv.Width, measure.Y + 3, GorgonColors.White);
    //            r.DrawLine(0, measure.Y + 4, currentRtv.Width, measure.Y + 4, GorgonColors.Black);

    //            // Draw FPS text.
    //            r.DrawString(_statsText.ToString(), Vector2.One, _statsFont, GorgonColors.White);
    //        })
    //        .DrawFilledRectangle(logoRegion, GorgonColors.White, _logoOlde, new GorgonRectangleF(0, 0, 1, 1))
    //        .End();
    //}

    ///// <summary>
    ///// Function to draw the statistics and the logo for the example.
    ///// </summary>
    ///// <param name="renderer">The 2D renderer that we are using.</param>
    //public static void DrawStatsAndLogo(Gorgon2D renderer)
    //{
    //    OLDE.GorgonRenderTargetView currentRtv = renderer.Graphics.RenderTargets[0];

    //    if ((currentRtv is null) || (_logoOlde is null) || (_statsFont is null))
    //    {
    //        return;
    //    }

    //    // We won't include these in the draw call count. 
    //    ref readonly OLDE.GorgonGraphicsStatistics stats = ref renderer.Graphics.Statistics;

    //    _statsText.Length = 0;
    //    _statsText.AppendFormat("Average FPS: {0:0.0}\nFrame Delta: {1:0.00#} seconds\nDraw Call Count: {2} ({3} triangles)", GorgonTiming.AverageFPS, GorgonTiming.Delta, stats.DrawCallCount, stats.TriangleCount);

    //    Vector2 measure = _statsText.ToString().MeasureText(_statsFont, true);
    //    GorgonRectangleF statsRegion = new(0, 0, currentRtv.Width, measure.Y + 4);
    //    GorgonRectangleF logoRegion = new(currentRtv.Width - _logoOlde.Width - 5, currentRtv.Height - _logoOlde.Height - 2, _logoOlde.Width, _logoOlde.Height);

    //    renderer.Begin();

    //    if (ShowStatistics)
    //    {
    //        // Draw translucent window.
    //        renderer.DrawFilledRectangle(statsRegion, new GorgonColor(0, 0, 0, 0.5f));
    //        // Draw lines for separators.
    //        renderer.DrawLine(0, measure.Y + 3, currentRtv.Width, measure.Y + 3, GorgonColors.White);
    //        renderer.DrawLine(0, measure.Y + 4, currentRtv.Width, measure.Y + 4, GorgonColors.Black);

    //        // Draw FPS text.
    //        renderer.DrawString(_statsText.ToString(), Vector2.One, _statsFont, GorgonColors.White);
    //    }

    //    // Draw logo.
    //    renderer.DrawFilledRectangle(logoRegion, GorgonColors.White, _logoOlde, new GorgonRectangleF(0, 0, 1, 1));

    //    renderer.End();
    //}

    /// <summary>
    /// Function to force the resources for the application to unload.
    /// </summary>
    public static void UnloadResources()
    {
        //OLDE.GorgonTextureBlitter blitter = Interlocked.Exchange(ref _blitterOlde, null);
        //OLDE.GorgonTexture2DView logo = Interlocked.Exchange(ref _logoOlde, null);        
        //GorgonFont font = Interlocked.Exchange(ref _statsFont, null);
        //GorgonFontFactory factory = Interlocked.Exchange(ref _factory, null);

        _logo?.Dispose();
        _blitter?.Dispose();
        //logo?.Dispose();
        //font?.Dispose();
        //factory?.Dispose();
    }

    /// <summary>
    /// Function called when the application is shutting down.
    /// </summary>
    public static void ShutDown()
    {
        UnloadResources();

        if (_lazyLoop.IsValueCreated)
        {
            Loop.Dispose();
        }

        if (_lazyLog.IsValueCreated)
        {
            Log.LogEnd();
        }
    }

    /// <summary>
    /// Function to load the logo for display in the application.
    /// </summary>
    /// <param name="graphics">The graphics interface to use.</param>
    public static void LoadResources(GorgonGraphics graphics)
    {
        Log.Print("Loading example resources...", LoggingLevel.Simple);

        _blitter = new GorgonTextureBlitter(graphics);

        using MemoryStream stream = new(Resources.Gorgon_Logo_Small);
        GorgonCodecDds ddsCodec = new();
        using IGorgonImage image = ddsCodec.FromStream(stream);
        _logo = IGorgonTextureView<GorgonTexture>.CreateTexture(graphics, "Gorgon Logo Tetxure", image);
    }

    ///// <summary>
    ///// Function to load the logo for display in the application.
    ///// </summary>
    ///// <param name="graphics">The graphics interface to use.</param>
    //public static void LoadResources_OLDE(OLDE.GorgonGraphics graphics)
    //{
    //    if (graphics is null)
    //    {
    //        throw new ArgumentNullException(nameof(graphics));
    //    }

    //    Log.Print("Loading example resources...", LoggingLevel.Simple);

    //    _blitterOlde = new OLDE.GorgonTextureBlitter(graphics);

    //    _factory = new GorgonFontFactory(graphics);
    //    _statsFont = _factory.GetFont(new GorgonFontInfo("Segoe UI", 9, GorgonFontHeightMode.Points)
    //    {
    //        Name = "Segoe UI 9pt Bold Outlined",
    //        AntiAliasingMode = GorgonFontAntiAliasMode.AntiAlias,
    //        FontStyle = GorgonFontStyle.Bold,
    //        OutlineColor1 = GorgonColors.Black,
    //        OutlineColor2 = GorgonColors.Black,
    //        OutlineSize = 2,
    //        TextureWidth = 512,
    //        TextureHeight = 256
    //    });

    //    using MemoryStream stream = new(Resources.Gorgon_Logo_Small);
    //    GorgonCodecDds ddsCodec = new();
    //    _logoOlde = OLDE.GorgonTexture2DView.FromStream(graphics, stream, ddsCodec, options: new OLDE.GorgonTexture2DLoadOptions
    //    {
    //        Name = "Gorgon Logo Texture",
    //        Binding = OLDE.TextureBinding.ShaderResource,
    //        Usage = OLDE.ResourceUsage.Immutable
    //    });
    //}

    /// <summary>
    /// Function to initialize the application.
    /// </summary>
    /// <param name="resolution">The client side resolution to use.</param>
    /// <param name="appTitle">The title for the application.</param>
    /// <param name="formLoad">The method to execute when the form load event is triggered.</param>
    /// <returns>The newly created form.</returns>
    [MemberNotNull(nameof(_mainForm))]
    public static FormMain Initialize(GorgonPoint resolution, string appTitle, EventHandler? formLoad = null)
    {
        Log.Print("Initializing example...", LoggingLevel.Simple);

        _mainForm = new FormMain
        {
            Text = appTitle            
        };

        _mainForm.ClientSize = _mainForm.LogicalToDeviceUnits(new Size(resolution.X, resolution.Y));

        _mainForm.KeyDown += FormKeyDown;

        if (formLoad is not null)
        {
            _mainForm.Load += formLoad;
        }

        _mainForm.Show();

        Application.DoEvents();

        Cursor.Current = Cursors.WaitCursor;

        return _mainForm;
    }
}
