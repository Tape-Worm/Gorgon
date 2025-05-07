
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
// Created: Saturday, January 5, 2013 3:29:58 PM
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
        GorgonApplicationLoop? loop = null;
        IGorgonInput? input = null;

        try
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            log = new GorgonTextFileLog("Input", "Tape_Worm", typeof(Program).Assembly.GetName().Version);
            log.LogStart(new GorgonComputerInfo());

            // This is our input system. By passing in the device types in the flags, we can tell the system to immediately start working with 
            // the devices specified. 
            //
            // You will note the "ExclusiveMouse" flag. This tells the input system that we will be the exclusive owner of all the data from 
            // the mouse. This means the application will no longer respond to Windows mouse events. Both keyboards and mice can be exclusive 
            // to the input system, but be warned that the system will not respond to some system key presses like Alt+F4 when the keyboard 
            // device is exclusive.
            // 
            // Also, regardless of whether the mouse or keyboard are exclusive, they will stop sending data if the application is not focused.
            // Gaming devices however, will always receive data.
            input = GorgonInput.CreateInput(InputFlags.ExclusiveMouse | InputFlags.Keyboard | InputFlags.GamingDevices, log);

            // Create an application loop so we can update our display continuously.
            loop = GorgonApplicationLoop.Create(log);

            Application.Run(new Form()
            {
                Log = log,
                Loop = loop,
                Input = input
            });
        }
        catch (Exception ex)
        {
            ex.Handle(e => GorgonDialogs.Error(null, e), log);
        }
        finally
        {
            // Ensure we drop the application loop.
            loop?.Dispose();

            // Always dispose the input system. This will disable its background thread and restore our input back to normal.
            input?.Dispose();

            log.LogEnd();
        }
    }
}
