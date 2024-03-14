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
// Created: August 23, 2018 4:42:41 PM
// 
#endregion

using System;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI;
using Drawing = System.Drawing;
using DX = SharpDX;

namespace Gorgon.Examples;

/// <summary>
/// Common functionality for the example applications.
/// </summary>
public static class GorgonExample
{
    #region Variables.
    // The Gorgon logo.
    private static GorgonTexture2DView _logo;
    // The font factory to use.
    private static GorgonFontFactory _factory;
    // The font used for statistics.
    private static GorgonFont _statsFont;
    // Blitter for displaying rendering.
    private static GorgonTextureBlitter _blitter;
    // The string containing our statistics.
    private static readonly StringBuilder _statsText = new();
    // The main window for the application.
    private static FormMain _mainForm;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to set or return the path to the plug in directory.
    /// </summary>
    public static DirectoryInfo PlugInLocationDirectory
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the base directory for application resources.
    /// </summary>
    public static DirectoryInfo ResourceBaseDirectory
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

    /// <summary>
    /// Property to return the blitter used to draw textures on the current render target.
    /// </summary>
    public static GorgonTextureBlitter Blitter => _blitter;

    /// <summary>
    /// Property to return the font factory used to handle font creation for our examples.
    /// </summary>
    public static GorgonFontFactory Fonts => _factory;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to retrieve the directory that contains the plugins for an application.
    /// </summary>
    /// <returns>A directory information object for the plugin path.</returns>
    public static DirectoryInfo GetPlugInPath()
    {
        string path = PlugInLocationDirectory?.FullName;

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new IOException("No plug in path has been assigned.");
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

        return new DirectoryInfo(Path.GetFullPath(path));
    }

    /// <summary>
    /// Function to return the complete path to the application resources.
    /// </summary>
    /// <param name="extraPath">Extra path information to append.</param>
    /// <returns>A directory info object for the resource path.</returns>
    public static DirectoryInfo GetResourcePath(string extraPath)
    {
        string path = ResourceBaseDirectory?.FullName;

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
        return new DirectoryInfo(Path.GetFullPath(path));
    }

    /// <summary>
    /// Function to mark the end of the initialization.
    /// </summary>
    public static void EndInit()
    {
        if (_mainForm is not null)
        {
            _mainForm.IsLoaded = true;
        }

        Cursor.Current = Cursors.Default;
    }

    /// <summary>
    /// Function to blit the logo without the aid of the 2D renderer.
    /// </summary>
    /// <param name="graphics">The graphics interface to use.</param>
    public static void BlitLogo(GorgonGraphics graphics)
    {
        GorgonRenderTargetView currentRtv = graphics.RenderTargets[0];

        if ((currentRtv is null) || (_logo is null))
        {
            return;
        }

        var logoRegion = new DX.Rectangle(currentRtv.Width - _logo.Width - 5, currentRtv.Height - _logo.Height - 2, _logo.Width, _logo.Height);
        _blitter.Blit(_logo, logoRegion, blendState: GorgonBlendState.Default);
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
        ex.Catch(e => GorgonDialogs.ErrorBox(null, "There was an error running the application and it must now close.", "Error", ex), GorgonApplication.Log);
    }

    /// <summary>
    /// Function to draw the statistics and the logo for the example.
    /// </summary>
    /// <param name="renderer">The 2D renderer that we are using.</param>
    public static void DrawStatsAndLogo(IGorgon2DFluent renderer)
    {
        renderer.ValidateObject(nameof(renderer));

        GorgonGraphics graphics = _factory?.Graphics;
        GorgonRenderTargetView currentRtv = graphics.RenderTargets[0];

        if ((currentRtv is null) || (_logo is null) || (_statsFont is null))
        {
            return;
        }

        // We won't include these in the draw call count. 
        ref readonly GorgonGraphicsStatistics stats = ref graphics.Statistics;

        _statsText.Length = 0;            
        _statsText.AppendFormat("Average FPS: {0:0.0}\nFrame Delta: {1:0.00#} seconds\nDraw Call Count: {2} ({3} triangles)", GorgonTiming.AverageFPS, GorgonTiming.Delta, stats.DrawCallCount, stats.TriangleCount);

        DX.Size2F measure = _statsText.ToString().MeasureText(_statsFont, true);
        var statsRegion = new DX.RectangleF(0, 0, currentRtv.Width, measure.Height + 4);
        var logoRegion = new DX.RectangleF(currentRtv.Width - _logo.Width - 5, currentRtv.Height - _logo.Height - 2, _logo.Width, _logo.Height);

        renderer
            .Begin()
            .DrawIf(() => ShowStatistics, r =>
            {
                // Draw translucent window.
                r.DrawFilledRectangle(statsRegion, new GorgonColor(0, 0, 0, 0.5f));
                // Draw lines for separators.
                r.DrawLine(0, measure.Height + 3, currentRtv.Width, measure.Height + 3, GorgonColor.White);
                r.DrawLine(0, measure.Height + 4, currentRtv.Width, measure.Height + 4, GorgonColor.Black);

                // Draw FPS text.
                r.DrawString(_statsText.ToString(), Vector2.One, _statsFont, GorgonColor.White);
            })
            .DrawFilledRectangle(logoRegion, GorgonColor.White, _logo, new DX.RectangleF(0, 0, 1, 1))
            .End();
    }

    /// <summary>
    /// Function to draw the statistics and the logo for the example.
    /// </summary>
    /// <param name="renderer">The 2D renderer that we are using.</param>
    public static void DrawStatsAndLogo(Gorgon2D renderer)
    {
        renderer.ValidateObject(nameof(renderer));

        GorgonRenderTargetView currentRtv = renderer.Graphics.RenderTargets[0];

        if ((currentRtv is null) || (_logo is null) || (_statsFont is null))
        {
            return;
        }

        // We won't include these in the draw call count. 
        ref readonly GorgonGraphicsStatistics stats = ref renderer.Graphics.Statistics;

        _statsText.Length = 0;
        _statsText.AppendFormat("Average FPS: {0:0.0}\nFrame Delta: {1:0.00#} seconds\nDraw Call Count: {2} ({3} triangles)", GorgonTiming.AverageFPS, GorgonTiming.Delta, stats.DrawCallCount, stats.TriangleCount);

        DX.Size2F measure = _statsText.ToString().MeasureText(_statsFont, true);
        var statsRegion = new DX.RectangleF(0, 0, currentRtv.Width, measure.Height + 4);
        var logoRegion = new DX.RectangleF(currentRtv.Width - _logo.Width - 5, currentRtv.Height - _logo.Height - 2, _logo.Width, _logo.Height);

        renderer.Begin();

        if (ShowStatistics)
        {
            // Draw translucent window.
            renderer.DrawFilledRectangle(statsRegion, new GorgonColor(0, 0, 0, 0.5f));
            // Draw lines for separators.
            renderer.DrawLine(0, measure.Height + 3, currentRtv.Width, measure.Height + 3, GorgonColor.White);
            renderer.DrawLine(0, measure.Height + 4, currentRtv.Width, measure.Height + 4, GorgonColor.Black);

            // Draw FPS text.
            renderer.DrawString(_statsText.ToString(), Vector2.One, _statsFont, GorgonColor.White);
        }

        // Draw logo.
        renderer.DrawFilledRectangle(logoRegion, GorgonColor.White, _logo, new DX.RectangleF(0, 0, 1, 1));

        renderer.End();
    }

    /// <summary>
    /// Function to force the resources for the application to unload.
    /// </summary>
    public static void UnloadResources()
    {
        GorgonTextureBlitter blitter = Interlocked.Exchange(ref _blitter, null);
        GorgonTexture2DView logo = Interlocked.Exchange(ref _logo, null);
        GorgonFont font = Interlocked.Exchange(ref _statsFont, null);
        GorgonFontFactory factory = Interlocked.Exchange(ref _factory, null);

        blitter?.Dispose();
        logo?.Dispose();
        font?.Dispose();
        factory?.Dispose();
    }

    /// <summary>
    /// Function to load the logo for display in the application.
    /// </summary>
    /// <param name="graphics">The graphics interface to use.</param>
    public static void LoadResources(GorgonGraphics graphics)
    {
        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        _blitter = new GorgonTextureBlitter(graphics);

        _factory = new GorgonFontFactory(graphics);
        _statsFont = _factory.GetFont(new GorgonFontInfo("Segoe UI", 9, FontHeightMode.Points)
        {
            Name = "Segoe UI 9pt Bold Outlined",
            AntiAliasingMode = FontAntiAliasMode.AntiAlias,
            FontStyle = FontStyle.Bold,
            OutlineColor1 = GorgonColor.Black,
            OutlineColor2 = GorgonColor.Black,
            OutlineSize = 2,
            TextureWidth = 512,
            TextureHeight = 256
        });

        using var stream = new MemoryStream(Resources.Gorgon_Logo_Small);
        var ddsCodec = new GorgonCodecDds();
        _logo = GorgonTexture2DView.FromStream(graphics, stream, ddsCodec, options: new GorgonTexture2DLoadOptions
        {
            Name = "Gorgon Logo Texture",
            Binding = TextureBinding.ShaderResource,
            Usage = ResourceUsage.Immutable
        });
    }

    /// <summary>
    /// Function to initialize the application.
    /// </summary>
    /// <param name="resolution">The client side resolution to use.</param>
    /// <param name="appTitle">The title for the application.</param>
    /// <param name="formLoad">The method to execute when the form load event is triggered.</param>
    /// <returns>The newly created form.</returns>
    public static FormMain Initialize(DX.Size2 resolution, string appTitle, EventHandler formLoad = null)
    {
        _mainForm = new FormMain
        {
            Text = appTitle,
            ClientSize = new Drawing.Size(resolution.Width, resolution.Height)
        };

        if (formLoad is not null)
        {
            _mainForm.Load += formLoad;
        }

        _mainForm.Show();

        Application.DoEvents();

        Cursor.Current = Cursors.WaitCursor;

        return _mainForm;
    }
    #endregion
}
