#region MIT.
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Tuesday, September 11, 2012 8:35:18 PM
// 
#endregion

using Gorgon.UI;

namespace Gorgon.Examples;

/// <summary>
/// Entry point class.
/// </summary>
internal class Program
{
    /// <summary>
    /// The entry point for the application.
    /// </summary>
    [STAThread()]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        // This is here for any windows forms elements that get displayed.
        // Without this, the elements will not use the visual styles and will 
        // default to older styles.
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            GorgonApplication.Run(new Main());
        }
        catch (Exception ex)
        {
            GorgonDialogs.ErrorBox(null, ex);
        }
    }
}
