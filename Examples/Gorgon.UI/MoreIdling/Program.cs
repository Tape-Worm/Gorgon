
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
// Created: Tuesday, September 18, 2012 8:00:02 PM
// 

using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Timing;
using Gorgon.UI.WindowsForms;

namespace Gorgon.Examples;

/// <summary>
/// Entry point class
/// </summary>
/// <remarks>
/// This example is tiny bit more advanced.  It'll show how to use an application context with Gorgon and how to dynamically switch idle loops on the fly
/// 
/// It will also demonstrate the use of the Console log
/// </remarks>
internal static class Program
{
    // Random number generator.
    private static readonly Random _rnd = new();
    // Last horizontal coordinate.
    private static int _lastX;
    // Last vertical coordinate.
    private static int _lastY;
    // Last time we drew.
    private static float _lastTime;
    // Color for the bars.
    private static int _color;
    // Color component.
    private static int _component;
    // The application form.
    private static Form? _form;

    /// <summary>
    /// Function that's called during idle time.
    /// </summary>
    /// <returns><b>true</b> to continue execution, <b>false</b> to stop.</returns>
    /// <remarks>This is the secondary default idle loop.</remarks>
    public static bool NewIdle()
    {
        Debug.Assert(_form is not null, "Form is null");

        // Draw some bars every 16 ms.
        if ((GorgonTiming.MillisecondsSinceStart - _lastTime) >= 16.6f)
        {
            Color newColor = Color.Transparent;

            switch (_component)
            {
                case 0:
                    newColor = Color.FromArgb(_color, 0, 0);
                    break;
                case 1:
                    newColor = Color.FromArgb(0, _color, 0);
                    break;
                case 2:
                    newColor = Color.FromArgb(0, 0, _color);
                    break;
            }

            _lastTime = GorgonTiming.MillisecondsSinceStart;

            _form.Draw(_lastX, _lastY, _lastX, (_form.GraphicsSize.Height - 1) - (_lastY), newColor);

            _color += 3;
            _lastX++;

            if (_color >= 255)
            {
                _color = 0;
                _component++;
                if (_component > 2)
                {
                    _component = 0;
                }
            }

            if (_lastX >= _form.GraphicsSize.Width)
            {
                _lastX = 0;
            }

            _lastY = _rnd.Next(0, _form.GraphicsSize.Height / 4);
        }

        _form.Flip();

        _form.DrawFPS("Secondary Idle Loop - FPS: " + GorgonTiming.FPS.ToString("0.0"));

        return true;
    }

    /// <summary>
    /// Function that's called during idle time.
    /// </summary>
    /// <returns><b>true</b> to continue execution, <b>false</b> to stop.</returns>
    /// <remarks>This is the default idle loop.</remarks>
    public static bool Idle()
    {
        Debug.Assert(_form is not null, "Form is null");

        int x = _rnd.Next(0, _form.GraphicsSize.Width - 1);
        int y = _rnd.Next(0, _form.GraphicsSize.Height - 1);

        // Draw a connected line on the form every 256 milliseconds.
        // This will run continously until the application has ended.
        if ((GorgonTiming.MillisecondsSinceStart - _lastTime) >= 256)
        {
            _lastTime = GorgonTiming.MillisecondsSinceStart;
            _form.Draw(_lastX, _lastY, x, y, Color.FromArgb(_rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255)));
            _lastX = x;
            _lastY = y;
        }
        else
        {
            _form.Draw(x, y, x, y, Color.FromArgb(_rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255)));
        }

        // Flip the buffer.
        _form.Flip();

        _form.DrawFPS("Primary Idle Loop - FPS: " + GorgonTiming.FPS.ToString("0.0"));

        return true;
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread()]
    private static void Main()
    {
        // Use the console as a logging mechanism instead of a file.
        IGorgonLog log = GorgonLog.NullLog;

        try
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            log = new GorgonLogConsole("MoreIdling", typeof(Program).Assembly.GetName().Version);
            log.LogStart(new GorgonComputerInfo());

            // Get the initial time.
            _lastTime = GorgonTiming.MillisecondsSinceStart;

            Context context = new(log);
            _form = context.Form;

            // Run the application context with an idle loop.
            //
            // Here we specify that we want to run an application context and an idle loop.  The idle loop 
            // will kick in after the main form displays.
            Application.Run(context);
        }
        catch (Exception ex)
        {
            ex.Handle(e => GorgonDialogs.Error(null, e), log);
        }
        finally
        {
            log.LogEnd();
        }
    }
}
