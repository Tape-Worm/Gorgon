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
using System.Reflection;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Input;

namespace Gorgon.Examples
{
	/// <summary>
	/// Entry point class.
	/// </summary>
	/// <remarks>
    /// The Input system in this version of Gorgon is very different from the previous version.  Input plug-ins now create factories which
    /// will create input device objects (e.g. a mouse device, keyboard device, etc...).  This allows mixing and matching of various input
    /// types.  For example, you can load a winforms plug-in to handle keyboard input, a raw input plug-in to handle mouse input and a
    /// plug-in to handle XBox controller support.  In fact, the XBox controller plug-in only has support for the controller and has no
    /// mouse or keyboard device support.
    /// 
    /// The first step in setting up the input interface is to create an input factory.  In this example we'll load the plug-ins and create 
    /// their respective input factories.  This example will go to the plug-ins directory (which can be configured in the config file) and
    /// load all of the input plug-ins by enumerating the DLLs and checking for input plug-in.  It is important to note that this is done 
    /// for the example only, in most cases the developer will know the DLL and plug-in type and create it directly.  For example:
    /// 
    /// // Load the raw input plug-in DLL.
    /// var assemblyName = GorgonApplication.PlugIns.LoadPlugInAssembly("[directory for plug-ins]\Gorgon.Input.Raw.DLL");
    ///
    /// // Create the factory.  Notice that we're using the full type name of the plug-in.
    /// // Alternatively, one could enumerate the plug-ins after loading the assembly by calling:
    /// // var plugIns = GorgonApplication.PlugIns.EnumeratePlugIns(assemblyName);
    /// // And then check for a GorgonInputPlugIn in the list returned.
    /// GorgonInputFactory rawInput = GorgonInputFactory.CreateInputFactory("Gorgon.Input.GorgonRawPlugIn");
    /// 
    /// While this is more complex than the previous version of Gorgon, it's also far more flexible.
	/// </remarks>
	static class Program
	{
		#region Properties.
		/// <summary>
		/// Property to return the path to the input plug-ins.
		/// </summary>
		static string PlugInPath
		{
			get
			{
				string plugInPath = Settings.Default.InputPlugInPath;

				// Map the plug-in path.
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
		/// Function to load in the input plug-ins.
		/// </summary>
		/// <returns>A list of input plug-ins.</returns>
		static IList<Tuple<GorgonInputPlugIn, GorgonInputFactory>> GetInputPlugIns()
		{            
			IList<Tuple<GorgonInputPlugIn, GorgonInputFactory>> result = new Tuple<GorgonInputPlugIn, GorgonInputFactory>[] { };    // A list of input factories and the plug-ins that created them.

            // Get the files from the plug-in directory.
            // The plug-in directory can be changed in the configuration file
            // to point at wherever you'd like.  If a {0} place holder is
            // in the path, it will be replaced with whatever the build
            // configuration is set to (i.e. DEBUG or RELEASE).
			var files = Directory.GetFiles(PlugInPath, "*.dll");

			if (files.Length == 0)
			{
				return result;
			}
				
			// Find our plug-ins in the DLLs.
			foreach(var file in files)
			{
                // Get the assembly name.  
                // This is the preferred method of loading a plug-in assembly.
                // It keeps us from going into DLL hell because it'll contain
                // version information, public key info, etc...  
				// We wrap this in this exception handler because if a DLL is
				// a native DLL, then it'll throw an exception.  And since
				// we can't load native DLLs as our plug-in, then we should
				// skip it.
				AssemblyName name;
				try
				{
					name = AssemblyName.GetAssemblyName(file);
				}
				catch (BadImageFormatException)
				{
					// This happens if we try and load a DLL that's not a .NET assembly.
					continue;
				}

				// Skip any assemblies that aren't a plug-in assembly.
				if (!GorgonApplication.PlugIns.IsPlugInAssembly(name))
				{
					continue;
				}

				// Load the assembly DLL.
                // This will not only load the assembly DLL into the application
                // domain (if it's not already loaded), but will also enumerate
                // the plug-in types.  If there are none, an exception will be
                // thrown.  This is why we do a check with IsPlugInAssembly before
                // we load the assembly.
				GorgonApplication.PlugIns.LoadPlugInAssembly(name);

				// Now try to retrieve our input plug-ins.
                // Retrieve the list of plug-ins from the assembly.  Once we have
                // the list we look for any plug-ins that are GorgonInputPlugIn
                // types and retrieve their type information.
				var inputPlugIns = GorgonApplication.PlugIns.EnumeratePlugIns(name)
									.Where(item => item is GorgonInputPlugIn)
									.Select(item => item.GetType()).ToArray();

				// If we have input plug-ins, and we haven't initialized our list, then do so now.
				if ((result.Count == 0) && (inputPlugIns.Length > 0))
				{
					result = new List<Tuple<GorgonInputPlugIn, GorgonInputFactory>>();
				}

				// Add each input factory and plug-in to our list.                
				foreach (var inputPlugIn in inputPlugIns)
				{
                    // Here we actually create the input factory from the plug-in
                    // by passing the input plug-in type.  
					var factory = GorgonInputFactory.CreateInputFactory(inputPlugIn);

					if (factory != null)
					{
						result.Add(new Tuple<GorgonInputPlugIn, GorgonInputFactory>(
							(GorgonInputPlugIn)GorgonApplication.PlugIns[inputPlugIn.FullName], 
							factory));
					}
				}
			}

			return result;
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
                Console.Title = "Gorgon Example - Input Factories.";

                Console.WriteLine("In this example we will perform the first step in accessing Gorgon's input");
                Console.WriteLine("device API. Unlike the previous version of Gorgon, this is done by means of");
                Console.WriteLine("an object called an input factory.  The input factory is responsible for");
                Console.WriteLine("creating input device objects like a mouse, keyboard or joystick which can");
                Console.WriteLine("then be used to monitor input from one of these devices.");
                Console.WriteLine();
                Console.WriteLine("It is important to note that this example does not create any of these devices");
                Console.WriteLine("since they require a Windows Form to operate.");

                Console.WriteLine();
				Console.WriteLine("Loading input plug-ins...");

				// Load our input factory plug-ins.
				var inputPlugIns = GetInputPlugIns();

                // No plug-ins?  No luck.
				if (inputPlugIns.Count == 0)
				{
					Console.ResetColor();
					Console.WriteLine("Could not find any input factory plug-ins.");
					return;
				}

			    // Display the plug-in information.
			    Console.WriteLine();
			    Console.WriteLine("{0} input factory plug-ins found:", inputPlugIns.Count);					
			    foreach (var plugIn in inputPlugIns)
			    {
			        Console.ForegroundColor = ConsoleColor.Cyan;

			        // Enumerate the devices available on the system.
			        plugIn.Item2.EnumerateDevices();

			        // The XBox Controller plug-in always registers multiple devices, even when none
			        // are physically attached to the system.  So in this case, we'll disregard those.
			        Console.WriteLine("{0}", plugIn.Item1.Description);
			        Console.ForegroundColor = ConsoleColor.Gray;
			        Console.WriteLine("\t{0} keyboards, {1} mice, {2} joysticks found.", 
			                          plugIn.Item2.KeyboardDevices.Count, 
			                          plugIn.Item2.PointingDevices.Count, 
			                          plugIn.Item2.JoystickDevices.Count(item => item.IsConnected));  

			        // Note that we're checking for connected joysticks, this is because a joystick
			        // like the XBox Controller is always present, but in a disconnected state.
			    }
			    Console.ResetColor();

			    Console.WriteLine();
                Console.WriteLine("Press any key...");
				Console.ReadKey();
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, _ => {
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
