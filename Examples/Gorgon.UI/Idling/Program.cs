
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
// Created: Wednesday, September 12, 2012 8:26:10 PM
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
/// <remarks>This example doesn't do much, just uses the idle time to draw pixels to the window.  It's meant to show 
/// how to use the idle loop from outside of the form.</remarks>
internal static class Program
{
    // Our application form.
    private static Form? _form;
    // Last horizontal coordinate.
    private static int _lastX;
    // Last vertical coordinate.
    private static int _lastY;
    // Last time we drew.
    private static float _lastTime;

    /// <summary>
    /// Function that's called during idle time.
    /// </summary>
    /// <returns><b>true</b> to continue execution, <b>false</b> to stop.</returns>
    private static bool Idle()
    {
        Debug.Assert(_form is not null, "Form is null!");

        int x = GorgonRandom.RandomInt32(0, _form.GraphicsSize.Width - 1);
        int y = GorgonRandom.RandomInt32(0, _form.GraphicsSize.Height - 1);

        // Draw a connected line on the form every 256 milliseconds.
        // This will run continously until the application has ended.
        if (GorgonTiming.MillisecondsSinceStart - _lastTime >= 256)
        {
            _lastTime = GorgonTiming.MillisecondsSinceStart;
            _form.Draw(_lastX, _lastY, x, y, Color.FromArgb(GorgonRandom.RandomInt32(0, 255), GorgonRandom.RandomInt32(0, 255), GorgonRandom.RandomInt32(0, 255)));
            _lastX = x;
            _lastY = y;
        }
        else
        {
            _form.Draw(x, y, x, y, Color.FromArgb(GorgonRandom.RandomInt32(0, 255), GorgonRandom.RandomInt32(0, 255), GorgonRandom.RandomInt32(0, 255)));
        }

        // Flip the buffer.
        _form.Flip();

        _form.DrawFPS("FPS: " + GorgonTiming.FPS.ToString("0.0"));

        return true;
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        GorgonApplicationLoop? loop = null;
        IGorgonLog log = GorgonLog.NullLog;

        try
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _form = new Form
            {
                ClientSize = new Size(1280, 800),
                Log = log
            };

            // Get the initial time.
            _lastTime = GorgonTiming.MillisecondsSinceStart;

            log = new GorgonTextFileLog("Idling", "Tape_Worm", typeof(Program).Assembly.GetName().Version);
            log.LogStart(new GorgonComputerInfo());

            // Run the application with an idle loop.
            // This will set the idle loop running as soon as the application goes into an idle state (after the call to Application.Run).
            loop = GorgonApplicationLoop.Create(log);
            loop.Run(Idle);

            Application.Run(_form);
        }
        catch (Exception ex)
        {
            ex.Handle(e => GorgonDialogs.Error(null, e), log);
        }
        finally
        {
            loop?.Dispose();
            log.LogEnd();
        }
    }
}
