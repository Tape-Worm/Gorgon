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
// Created: August 14, 2018 7:28:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Drawing = System.Drawing;
using DX =SharpDX;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Examples
{
    /// <summary>
    /// The example entry point.
    /// </summary>
    static class Program
    {
        #region Variables.
        // The string builder used for drawing the FPS/frame delta info.
        private static readonly StringBuilder _fpsString = new StringBuilder();
        // The main graphics interface.
        private static GorgonGraphics _graphics;
        // The swap chain representing our "screen".
        private static GorgonSwapChain _screen;
        // Our 2D renderer, used to draw text.
        private static Gorgon2D _renderer;
        // Our factory used to generate fonts.
        private static GorgonFontFactory _fontFactory;
        // The logo for Gorgon.
        private static GorgonTexture2DView _logo;
        // The list of built-in Windows font family names to use.
        private static readonly List<string> _fontFamilies = new List<string>
        {
            "Usuzi",
            "Algerian",
            "Bauhaus 93",
            "Blackadder ITC",
            "Viner Hand ITC",
            "Broadway",
            "Rage Italic",
            "Chiller",
            "Old English Text MT",
            "Times New Roman",
            "Ravie",
            "Playbill"
        };
        // The fonts for the application. 
        private static readonly List<GorgonFont> _font = new List<GorgonFont>();
        // The starting time.
        private static float _startTime = float.MinValue;
        // The current font index.
        private static int _fontIndex;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to draw the FPS bar (and logo).
        /// </summary>
        private static void DrawFPS()
        {
            _renderer.Begin();
            _fpsString.Length = 0;
            _fpsString.AppendFormat("FPS: {0:0.0}\nFrame delta: {1:0.000} ms.", GorgonTiming.AverageFPS, GorgonTiming.Delta * 1000);

            DX.Size2F textSize = _renderer.DefaultFont.MeasureText(_fpsString.ToString(), false);

            _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _screen.Width, textSize.Height + 4), new GorgonColor(0, 0, 0, 0.5f));
            _renderer.DrawLine(0, textSize.Height + 4, _screen.Width, textSize.Height + 4, GorgonColor.White, 1.5f);
            _renderer.DrawLine(0, textSize.Height + 5, _screen.Width, textSize.Height + 5, new GorgonColor(0, 0, 0, 0.75f));

            _renderer.DrawString(_fpsString.ToString(), DX.Vector2.Zero, color: GorgonColor.White);
            DX.RectangleF pos = new DX.RectangleF(_screen.Width - _logo.Width - 5, _screen.Height - _logo.Height - 2, _logo.Width, _logo.Height);
            _renderer.DrawFilledRectangle(pos, GorgonColor.White, _logo, new DX.RectangleF(0, 0, 1, 1));
            _renderer.End();
        }

        /// <summary>
        /// Function called during idle time the application.
        /// </summary>
        /// <returns><b>true</b> to continue executing, <b>false</b> to stop.</returns>
        private static bool Idle()
        {
            GorgonFont currentFont = _font[_fontIndex];

            if (_startTime < 0)
            {
                _startTime = GorgonTiming.SecondsSinceStart;
            }

            _screen.RenderTargetView.Clear(GorgonColor.CornFlowerBlue);


            _renderer.Begin();
            
            DX.Size2F textSize = currentFont.MeasureText(Resources.Lorem_Ipsum, false);
            DX.Vector2 position = new DX.Vector2((_screen.Width / 2.0f - textSize.Width / 2.0f).Max(4.0f), (_screen.Height / 2.0f - textSize.Height / 2.0f).Max(100));

            _renderer.DrawString($"Now displaying '{currentFont.Name}'...", new DX.Vector2(4.0f, 64.0f));

            _renderer.DrawString(Resources.Lorem_Ipsum, position, currentFont);

            _renderer.End();

            DrawFPS();

            int timeDiff = (int)(GorgonTiming.SecondsSinceStart - _startTime).FastCeiling();

            if (timeDiff > 10)
            {
                _startTime = GorgonTiming.SecondsSinceStart;
                ++_fontIndex;
            }

            if (_fontIndex >= _font.Count)
            {
                _fontIndex = 0;
            }

            _screen.Present(1);

            return true;
        }

        /// <summary>
        /// Function to initialize the application.
        /// </summary>
        /// <returns>The main window for the application.</returns>
        private static FormMain Initialize()
        {
            MemoryStream stream = null;

            var window = new FormMain
                         {
                             ClientSize = Settings.Default.Resolution
                         };
            window.Show();

            // Process any pending events so the window shows properly.
            Application.DoEvents();

            Cursor.Current = Cursors.WaitCursor;

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
                                              new GorgonSwapChainInfo("Gorgon2D Effects Example Swap Chain")
                                              {
                                                  Width = Settings.Default.Resolution.Width,
                                                  Height = Settings.Default.Resolution.Height,
                                                  Format = BufferFormat.R8G8B8A8_UNorm
                                              });

                // Tell the graphics API that we want to render to the "screen" swap chain.
                _graphics.SetRenderTarget(_screen.RenderTargetView);

                // Initialize the renderer so that we are able to draw stuff.
                _renderer = new Gorgon2D(_screen.RenderTargetView);

                // Load our logo.
                stream = new MemoryStream(Resources.Gorgon_Logo_Small);
                _logo = GorgonTexture2DView.FromStream(_graphics, stream, new GorgonCodecDds());

                // We need to create a font factory so we can create/load (and cache) fonts.
                _fontFactory = new GorgonFontFactory(_graphics);

                // Load in a bunch of true type fonts.
                var dirInfo = new DirectoryInfo(GetResourcePath("Fonts"));
                FileInfo[] files = dirInfo.GetFiles("*.ttf", SearchOption.TopDirectoryOnly);

                // TODO: Put this in separate function
                var fontFamilies = new List<Drawing.FontFamily>();
                
                // Load all external true type fonts for this example.
                // This takes a while...
                foreach (FileInfo file in files)
                {
                    Drawing.FontFamily externFont = _fontFactory.LoadTrueTypeFontFamily(file.FullName);
                    _fontFamilies.Insert(0, externFont.Name);
                    fontFamilies.Add(externFont);
                }
                
                fontFamilies.AddRange(Drawing.FontFamily.Families);
                // TODO: End

                // TODO: Put in separate function.
                for (int i = 0; i < _fontFamilies.Count; ++i)
                {
                    string fontFamily = _fontFamilies[i];
                    
                    // Use this to determine if the font is avaiable.
                    if (fontFamilies.All(item => !string.Equals(item.Name, fontFamily, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        // Can't locate this one, move on...
                        continue;
                    }

                    // TODO: Pick specific fonts for special effects like outline, glow, texture, and gradient.
                    bool hasOutline = GorgonRandom.RandomInt32(0, 100) > 50;
                    bool isExternal = Drawing.FontFamily.Families.All(item => !string.Equals(item.Name, fontFamily, StringComparison.InvariantCultureIgnoreCase));

                    IGorgonFontInfo fontInfo = new GorgonFontInfo(fontFamily,
                                                                  32.0f,
                                                                  name:
                                                                  $"{fontFamily} 32px{(hasOutline ? " Outlined" : string.Empty)}{(isExternal ? " External TTF" : string.Empty)}")
                                               {
                                                   AntiAliasingMode = FontAntiAliasMode.AntiAlias,
                                                   OutlineSize = hasOutline ? 3 : 0,
                                                   OutlineColor1 = GorgonColor.Black,
                                                   OutlineColor2 = GorgonColor.Black
                                               };

                    _font.Add(_fontFactory.GetFont(fontInfo));
                }
                // TODO: End

                // TODO: Use TextSprite and animate line spacing for giggles.

                window.IsLoaded = true;

                return window;
            }
            finally
            {
                stream?.Dispose();
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Property to return the path to the resources for the example.
        /// </summary>
        /// <param name="resourceItem">The directory or file to use as a resource.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="resourceItem"/> was NULL (<i>Nothing</i> in VB.Net) or empty.</exception>
        public static string GetResourcePath(string resourceItem)
        {
            string path = Settings.Default.ResourceLocation;

            if (string.IsNullOrEmpty(resourceItem))
            {
                throw new ArgumentException(@"The resource was not specified.", nameof(resourceItem));
            }

            path = path.FormatDirectory(Path.DirectorySeparatorChar);

            // If this is a directory, then sanitize it as such.
            if (resourceItem.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path += resourceItem.FormatDirectory(Path.DirectorySeparatorChar);
            }
            else
            {
                // Otherwise, format the file name.
                path += resourceItem.FormatPath(Path.DirectorySeparatorChar);
            }

            // Ensure that we have an absolute path.
            return Path.GetFullPath(path);
        }

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
                Cursor.Show();
                ex.Catch(e => GorgonDialogs.ErrorBox(null, "There was an error running the application and it must now close.", "Error", ex));
            }
            finally
            {
                _fontFactory?.Dispose();
                _renderer?.Dispose();
                _screen?.Dispose();
                _graphics?.Dispose();
            }
        }
        #endregion
    }
}
