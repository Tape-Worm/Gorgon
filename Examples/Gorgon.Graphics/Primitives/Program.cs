
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Thursday, August 7, 2014 9:38:25 PM
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Gorgon.Graphics.Core;
using Gorgon.UI.WindowsForms;

namespace Gorgon.Examples;

/// <summary>
/// Our application entry point
/// </summary>
internal static class Program
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
            Form window = GorgonExample.Initialize(ExampleConfig.Default.Resolution, "Primitives");

            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(Path.GetFullPath(ExampleConfig.Default.ResourceLocation));

            // Create the factory used to create our graphics functionality.
#if DEBUG
            using GorgonGraphicsFactory factory = new(GorgonGraphicsDebugFlags.SynchronizedCommandQueueValidation
                | GorgonGraphicsDebugFlags.GpuBasedValidation
                | GorgonGraphicsDebugFlags.GpuBasedStateTrackingValidation
                | GorgonGraphicsDebugFlags.ObjectTracking, log: GorgonExample.Log);
#else
            using GorgonGraphicsFactory factory = new(log: GorgonExample.Log);
#endif     

            // Ensure that we have an appropriate GPU that we can use.
            IReadOnlyList<GorgonVideoAdapterInfo> adapters = factory.EnumerateAdapters();

            if (adapters.Count == 0)
            {
                GorgonDialogs.Error(window, "No valid Direct 3D 12 GPU was found on the system. Please see the log for details.");
                return;
            }

            // Use the first viable graphics adapter.
            using Primitives app = new(window, factory, adapters[0]);
            app.Run();
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
        }
    }
}
