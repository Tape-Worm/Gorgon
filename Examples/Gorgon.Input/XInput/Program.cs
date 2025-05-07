
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
// Created: Friday, January 11, 2013 8:27:27 AM
// 

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Input;
using Gorgon.UI.WindowsForms;

namespace Gorgon.Examples;

/// <summary>
/// Example entry point
/// </summary>
/// <remarks>To see a description of this example, look in <see cref="Form"/>.cs</remarks>
internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        IGorgonLog log = GorgonLog.NullLog;
        IGorgonInput? input = null;
        GorgonApplicationLoop? loop = null;

        try
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            log = new GorgonTextFileLog("XInput", "Tape_Worm", typeof(Program).Assembly.GetName().Version);
            log.LogStart(new GorgonComputerInfo());

            // This is our input system. By passing in the device types in the flags, we can tell the system to immediately start working with 
            // the devices specified. 
            //
            // In this example, we're only interested in XInput gaming devices, so we only bind with gaming devices (we filter for XInput later).
            input = GorgonInput.CreateInput(InputFlags.GamingDevices, log);

            // Create our application loop so we can continuously update our window.
            loop = GorgonApplicationLoop.Create(log);

            Application.Run(new Form()
            {
                Log = log,
                Input = input,
                Loop = loop
            });
        }
        catch (Exception ex)
        {
            ex.Handle(e => GorgonDialogs.Error(null, e), log);
        }
        finally
        {
            // Shut down the application loop.
            loop?.Dispose();

            // Always dispose the input system. This will disable its background thread and restore our input back to normal.
            input?.Dispose();

            log.LogEnd();
        }
    }
}
