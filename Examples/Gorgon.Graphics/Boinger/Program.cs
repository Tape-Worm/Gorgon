
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
// Created: March 2, 2017 7:46:37 PM
// 

using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.UI.WindowsForms;

namespace Gorgon.Examples;

/// <summary>
/// A boot strap class to launch our example application.
/// </summary>
internal static partial class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            // Create the main application form.
            FormMain mainForm = GorgonExample.Initialize(ExampleConfig.Default.Resolution, "Boinger");

            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(Path.GetFullPath(ExampleConfig.Default.ResourceLocation));

            // Create the factory used to create our graphics functionality.
#if DEBUG
            using GorgonGraphicsFactory factory = new(GorgonGraphicsDebugFlags.SynchronizedCommandQueueValidation
                | GorgonGraphicsDebugFlags.GpuBasedValidation
                | GorgonGraphicsDebugFlags.GpuBasedStateTrackingValidation
                | GorgonGraphicsDebugFlags.ObjectTracking, log: GorgonExample.Log);
#else
            using GorgonGraphicsFactory factory = new(log: log);
#endif     

            // Ensure that we have an appropriate GPU that we can use.
            IReadOnlyList<GorgonVideoAdapterInfo> adapters = factory.EnumerateAdapters();

            if (adapters.Count == 0)
            {
                GorgonDialogs.Error(mainForm, "No valid Direct 3D 12 GPU was found on the system. Please see the log for details.");
                return;
            }

            // Create the graphics interface for the application.
            using GorgonGraphics graphics = factory.CreateGraphics(adapters[0]);

            // Create our primary swap chain.
            using GorgonSwapChain swap = new(graphics, "Boinger Swap Chain", mainForm.Handle, 
                new GorgonSwapChainInfo(ExampleConfig.Default.Resolution.X, 
                                             ExampleConfig.Default.Resolution.Y, 
                                             BufferFormat.R8G8B8A8_UNorm));

            // Start up our example application.
            Boing.Run(mainForm, graphics, swap);
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
        }
    }
}
