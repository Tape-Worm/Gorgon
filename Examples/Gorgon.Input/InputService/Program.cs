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
using System.Linq;
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
    /// The Input system in this version of Gorgon is very different from the previous version.  Input plugins now create factories which
    /// will create input device objects (e.g. a mouse device, keyboard device, etc...).  This allows mixing and matching of various input
    /// types.  For example, you can load a winforms plugin to handle keyboard input, a raw input plugin to handle mouse input and a
    /// plugin to handle XBox controller support.  In fact, the XBox controller plugin only has support for the controller and has no
    /// mouse or keyboard device support.
    /// 
    /// The first step in setting up the input interface is to create an input service.  In this example we'll load the plugins and create 
    /// their respective input services.  This example will go to the plugins directory (which can be configured in the config file) and
    /// load all of the input plugins by enumerating the DLLs and checking for input plugin.  It is important to note that this is done 
    /// for the example only, in most cases the developer will know the DLL and plugin type and create it directly.  For example:
    /// 
    /// // Load the raw input plugin DLL.
    /// GorgonInputService inputService;
    /// 
    /// using (var assemblyCache = new GorgonPluginAssemblyCache())
    /// {
    ///		assemblyCache.Load("[directory for plugins]\Gorgon.Input.Raw.DLL"); 
    /// 
    ///		// Create the plugin service.
    ///		var pluginService = new GorgonPluginService(assemblyCache);
    ///
    ///		// Create the factory for input services.  Notice that we're using the full type name of the plugin.
    ///		var inputServiceFactory = new GorgonInputServiceFactory(pluginService);
    /// 
    ///		// Finally, create the input service. Note the use of the fully qualified type name for the plugin name.
	///		inputService = inputServiceFactory.CreateInputService("Gorgon.Input.GorgonRawPlugIn");
    /// }
    /// 
    /// While this is more complex than the previous version of Gorgon, it's also far more flexible.
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
		/// Function to load in the input plugins.
		/// </summary>
		/// <returns>A list of input plugins.</returns>
		static IList<Tuple<GorgonInputServicePlugin, IGorgonInputService>> GetInputPlugIns()
		{
			// Access our plugin cache.
			using (IGorgonPluginAssemblyCache assemblies = new GorgonPluginAssemblyCache(GorgonApplication.Log))
			{
				// Get the files from the plugin directory.
				// The plugin directory can be changed in the configuration file
				// to point at wherever you'd like.  If a {0} place holder is
				// in the path, it will be replaced with whatever the build
				// configuration is set to (i.e. DEBUG or RELEASE).
				var files = Directory.GetFiles(PlugInPath, "*.dll");

				if (files.Length == 0)
				{
					return new Tuple<GorgonInputServicePlugin, IGorgonInputService>[0];
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
				IGorgonPluginService pluginService = new GorgonPluginService(assemblies, GorgonApplication.Log);

				// Create our input service factory.
				IGorgonInputServiceFactory serviceFactory = new GorgonInputServiceFactory2(pluginService, GorgonApplication.Log);

				// Retrieve the list of plugins from the input service factory.
				IEnumerable<GorgonInputServicePlugin> inputPlugIns = pluginService.GetPlugins<GorgonInputServicePlugin>();

				return (from plugin in inputPlugIns
				        let service = serviceFactory.CreateService(plugin.Name)
				        where service != null
				        select new Tuple<GorgonInputServicePlugin, IGorgonInputService>(plugin, service)).ToArray();
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

                Console.WriteLine("In this example we will perform the first step in accessing Gorgon's input");
                Console.WriteLine("device API. Unlike the previous version of Gorgon, this is done by means of");
                Console.WriteLine("an object called an input service.  The input service is responsible for");
                Console.WriteLine("creating input device objects like a mouse, keyboard or joystick which can");
                Console.WriteLine("then be used to monitor input from one of these devices.");
                Console.WriteLine();
                Console.WriteLine("It is important to note that this example does not create any of these devices");
                Console.WriteLine("since they require a Windows Form to operate.");

                Console.WriteLine();
				Console.WriteLine("Loading input plugins...");

				// Load our input factory plugins.
				var inputPlugIns = GetInputPlugIns();

                // No plugins?  No luck.
				if (inputPlugIns.Count == 0)
				{
					Console.ResetColor();
					Console.WriteLine("Could not find any input factory plugins.");
					return;
				}

			    // Display the plugin information.
			    Console.WriteLine();
			    Console.WriteLine("{0} input factory plugins found:", inputPlugIns.Count);					
			    foreach (var plugIn in inputPlugIns)
			    {
			        Console.ForegroundColor = ConsoleColor.Cyan;

			        // Enumerate the devices available on the system.
				    try
				    {
					    IReadOnlyList<IGorgonKeyboardInfo2> keyboards = plugIn.Item2.EnumerateKeyboards();
					    IReadOnlyList<IGorgonMouseInfo2> mice = plugIn.Item2.EnumerateMice();
						IReadOnlyList<IGorgonJoystickInfo2> joysticks = plugIn.Item2.EnumerateJoysticks();

						// The XBox Controller plugin always registers multiple devices, even when none
						// are physically attached to the system.  So in this case, we'll disregard those.
						Console.WriteLine("{0}", plugIn.Item1.Description);
						Console.ForegroundColor = ConsoleColor.Gray;
						Console.WriteLine("\t{0} keyboards, {1} mice, {2} joysticks found.",
										  keyboards.Count,
										  mice.Count,
										  joysticks.Count);
				    }
				    catch (Exception ex)
				    {
					    Console.ForegroundColor = ConsoleColor.Red;
					    Console.WriteLine("Cannot enumerate devices from {0}. Error:\n{1}", plugIn.Item2.GetType().FullName, ex.Message.Ellipses(Console.WindowWidth - 4));
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
