
// 
// Gorgon
// Copyright (C) 2012 Michael Winsor
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


using System.Drawing;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Examples;

/// <summary>
/// Entry point class
/// </summary>
/// <remarks>This example doesn't do much, just uses the idle time to draw pixels to the window.  It's meant to show 
/// how to use the idle loop from outside of the form.</remarks>
internal static class Program
{

    private static readonly Random _rnd = new();			// Random number generator.
    private static FormMain _form;                              // Our application form.
    private static int _lastX;                                  // Last horizontal coordinate.
    private static int _lastY;                                  // Last vertical coordinate.
    private static float _lastTime;                             // Last time we drew.



    /// <summary>
    /// Function that's called during idle time.
    /// </summary>
    /// <returns><b>true</b> to continue execution, <b>false</b> to stop.</returns>
    private static bool Idle()
    {
        int x = _rnd.Next(0, _form.GraphicsSize.Width - 1);
        int y = _rnd.Next(0, _form.GraphicsSize.Height - 1);

        // Draw a connected line on the form every 256 milliseconds.
        // This will run continously until the application has ended.
        if (GorgonTiming.MillisecondsSinceStart - _lastTime >= 256)
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

        _form.DrawFPS("FPS: " + GorgonTiming.FPS.ToString("0.0"));

        return true;
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        try
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _form = new FormMain
            {
                ClientSize = new Size(640, 480)
            };

            // Get the initial time.
            _lastTime = GorgonTiming.MillisecondsSinceStart;

            // Run the application with an idle loop.
            //
            // The form will still control the life time of the application (i.e. the close button will end the application). 
            // However, you may specify only the idle method and use that to control the application life time, similar to 
            // standard windows application in C++.
            // Other overloads allow using only the form and assigning the idle method at another time (if at all), or setting
            // up an application context object to manage the life time of the application (with or without an idle loop 
            // method).
            GorgonApplication.Run(_form, Idle);
        }
        catch (Exception ex)
        {
            ex.Catch(_ => GorgonDialogs.ErrorBox(null, _), GorgonApplication.Log);
        }
    }

}
