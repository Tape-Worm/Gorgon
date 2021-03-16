#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, January 3, 2013 9:43:42 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Gorgon.Core;
using Gorgon.Input;
using Gorgon.PlugIns;
using Gorgon.UI;

namespace Gorgon.Examples
{
    /// <summary>
    /// Entry point class.
    /// </summary>
    /// <remarks>
    /// The first step in loading a gaming device driver is to create a plug in assembly cache and plug in service to load the driver. 
    /// After that, use the GorgonGamingDeviceDriverFactory object to load the driver plug in from the loaded assemblies. In this example 
    /// we will load the both the XInput and Direct Input drivers and display the supported gaming devices for these drivers in the 
    /// console window. The example will load the DLLs for these drivers from the plug ins directory (which is configured in the app.config 
    /// file). It is important to note that this is done for the example only, in most cases the developer will know the DLL and plugin 
    /// type and create it directly.  For example:
    /// 
    /// // Load the Xinput plugin DLL.
    /// GorgonGamingDeviceDriver xInputDriver;
    /// 
    /// using (var assemblyCache = new GorgonPlugInAssemblyCache())
    /// {
    ///		assemblyCache.Load("[directory for plugins]\Gorgon.Input.XInput.DLL"); 
    /// 
    ///		// Create the plugin service.
    ///		var pluginService = new GorgonPlugInService(assemblyCache);
    ///
    ///		// Create the factory for loading the drivers and load the driver.
    ///		var driverFactory = new GorgonGamingDeviceDriverFactory(inputServiceFactory);
    /// 
    ///		// Finally, create the input service. Note the use of the fully qualified type name for the plugin name.
    ///		xInputDriver = driverFactory.LoadDriver("Gorgon.Input.GorgonXInputDriver");
    /// }
    /// 
    /// While this is more complex than the previous version of Gorgon, it's also far more flexible when dealing with object composition 
    /// techniques like dependency injection.
    /// </remarks>
    internal static class Program
    {
        #region Variables.
        // The cache that will hold our plugin instances.
        private static GorgonMefPlugInCache _pluginCache;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to load in the gaming device driver plug ins.
        /// </summary>
        /// <returns>A list of gaming device driver plug ins.</returns>
        private static IReadOnlyList<IGorgonGamingDeviceDriver> GetGamingDeviceDrivers()
        {
            GorgonExample.PlugInLocationDirectory = new DirectoryInfo(ExampleConfig.Default.PlugInLocation);

            // Create a plug in assembly cache for our input assemblies.
            _pluginCache = new GorgonMefPlugInCache(GorgonApplication.Log);

            // Create our input service factory.
            var factory = new GorgonGamingDeviceDriverFactory(_pluginCache, GorgonApplication.Log);

            // Retrieve the list of driver plug ins from the input service factory.
            return factory.LoadAllDrivers(Path.Combine(GorgonExample.GetPlugInPath().FullName, "Gorgon.Input.*.dll"));
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            try
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.White;
                Console.Title = "Gorgon Example - Input Services.";

                Console.WriteLine("In this example we will perform the first step in accessing Gorgon's gaming");
                Console.WriteLine("device driver API. These drivers will allow access to any gaming device (e.g. a ");
                Console.WriteLine("joystick, game pad, etc...) that is supported by its underlying native provider.");
                Console.WriteLine("These drivers are loaded as plug ins and can be used to mix and match various");
                Console.WriteLine("types of gaming devices.");
                Console.WriteLine();
                Console.WriteLine("Only connected gaming device drivers will be reported by this example.");
                Console.WriteLine();
                Console.WriteLine("It is important to note that this example does not create any of these devices ");
                Console.WriteLine("since some providers (e.g. Direct Input) require a window handle to operate.");

                Console.WriteLine();
                Console.WriteLine("Loading gaming device drivers...");

                // Load our input factory plugins.
                IReadOnlyList<IGorgonGamingDeviceDriver> inputPlugIns = GetGamingDeviceDrivers();

                // No plugins?  No luck.
                if (inputPlugIns.Count == 0)
                {
                    Console.ResetColor();
                    Console.WriteLine("Could not find any gaming device driver plug ins.");
                    return;
                }

                // Display the plugin information.
                Console.WriteLine();
                Console.WriteLine($"{inputPlugIns.Count} gaming device driver plug ins found:");
                foreach (IGorgonGamingDeviceDriver plugIn in inputPlugIns)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;

                    // Enumerate the devices available on the system.
                    try
                    {
                        IReadOnlyList<IGorgonGamingDeviceInfo> devices = plugIn.EnumerateGamingDevices(true);

                        Console.WriteLine($"{plugIn.Description} ({devices.Count} device(s))");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        for (int i = 0; i < devices.Count; ++i)
                        {
                            Console.WriteLine($"\t{i + 1}. {devices[i].Description}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Cannot enumerate devices from {plugIn.Description}. Error:\n{ex.Message.Ellipses(Console.WindowWidth - 4)}");
                    }
                }

                Console.ResetColor();

                Console.WriteLine();
                Console.WriteLine("Press any key...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                ex.Catch(e =>
                         {
                             Console.Clear();
                             Console.ForegroundColor = ConsoleColor.Red;
                             Console.WriteLine($"Exception:\n{e.Message}\n\nStack Trace:{e.StackTrace}");
                         });
                Console.ResetColor();
                Console.ReadKey();
            }
            finally
            {
                _pluginCache?.Dispose();
            }
        }
        #endregion
    }
}
