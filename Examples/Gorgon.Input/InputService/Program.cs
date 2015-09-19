﻿#region MIT.
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
using Gorgon.Examples.Properties;
using Gorgon.Input;
using Gorgon.Plugins;
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
    /// using (var assemblyCache = new GorgonPluginAssemblyCache())
    /// {
    ///		assemblyCache.Load("[directory for plugins]\Gorgon.Input.XInput.DLL"); 
    /// 
    ///		// Create the plugin service.
    ///		var pluginService = new GorgonPluginService(assemblyCache);
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
	static class Program
	{
		#region Properties.
		/// <summary>
		/// Property to return the path to the input plugins.
		/// </summary>
		static string PlugInPath
		{
			get
			{
				string plugInPath = Settings.Default.InputPlugInPath;

				// Map the plugin path.
				if (plugInPath.Contains("{0}"))
				{
#if DEBUG
					plugInPath = string.Format(plugInPath, "Debug");
#else
					plugInPath = string.Format(plugInPath, "Release");
#endif
				}

				if (!plugInPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					plugInPath += Path.DirectorySeparatorChar.ToString();
				}

				return Path.GetFullPath(plugInPath);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to load in the gaming device driver plug ins.
		/// </summary>
		/// <returns>A list of gaming device driver plug ins.</returns>
		static IReadOnlyList<IGorgonGamingDeviceDriver> GetGamingDeviceDrivers()
		{
			// Access our plugin cache.
			using (GorgonPluginAssemblyCache assemblies = new GorgonPluginAssemblyCache(GorgonApplication.Log))
			{
				// Get the files from the plugin directory.
				// The plugin directory can be changed in the configuration file
				// to point at wherever you'd like.  If a {0} place holder is
				// in the path, it will be replaced with whatever the build
				// configuration is set to (i.e. DEBUG or RELEASE).
				var files = Directory.GetFiles(PlugInPath, "*.dll");

				if (files.Length == 0)
				{
					return new IGorgonGamingDeviceDriver[0];
				}

				// Find our plugins in the DLLs.
				foreach (var file in files)
				{
					// Ensure that the plugin assemblies have plugins, and 
					// can be loaded as assemblies.
					if (!assemblies.IsPluginAssembly(file))
					{
						continue;
					}

					// Load the assembly DLL.
					assemblies.Load(file);
				}
				
				// Create our plugin service.
				var pluginService = new GorgonPluginService(assemblies, GorgonApplication.Log);

				// Create our input service factory.
				var factory = new GorgonGamingDeviceDriverFactory(pluginService, GorgonApplication.Log);

				// Retrieve the list of driver plug ins from the input service factory.
				return factory.LoadAllDrivers();
			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
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
			    Console.WriteLine("{0} gaming device driver plug ins found:", inputPlugIns.Count);					
			    foreach (var plugIn in inputPlugIns)
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
					    Console.WriteLine("Cannot enumerate devices from {0}. Error:\n{1}", plugIn.Description, ex.Message.Ellipses(Console.WindowWidth - 4));
				    }
			    }
			    Console.ResetColor();

			    Console.WriteLine();
                Console.WriteLine("Press any key...");
				Console.ReadKey();
			}
			catch (Exception ex)
			{
				ex.Catch(_ => {
					             Console.Clear();
					             Console.ForegroundColor = ConsoleColor.Red;
					             Console.WriteLine("Exception:\n{0}\n\nStack Trace:{1}", _.Message, _.StackTrace);					
				});
				Console.ResetColor();
#if DEBUG
				Console.ReadKey();
#endif
			}
		}
		#endregion
	}
}
