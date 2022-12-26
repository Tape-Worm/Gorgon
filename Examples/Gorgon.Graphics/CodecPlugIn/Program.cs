#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Monday, November 03, 2014 8:32:52 PM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Examples;
using Gorgon.UI;

namespace Graphics.Examples;

/// <summary>
/// This is an example to show a developer how to create their own image codec for loading/saving images.
/// 
/// A custom image codec may be implemented as a plug in that can be loaded dynamically, or statically within an application.  This example 
/// will focus on using a plug in to create a fairly useless image codec that will save only the red, green, blue and alpha channels as one 
/// channel per pixel.  That is, the first pixel will be the red channel, the second will be the green, etc... 
/// 
/// This application will be the UI for the image codec plug in and will do nothing more than load a DDS image, save it in our custom format 
/// and then load it as a texture for display in the window.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Defines the entry point of the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        try
        {
#if NET6_0_OR_GREATER
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            GorgonApplication.Run(new Form());
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
        }
    }
}
