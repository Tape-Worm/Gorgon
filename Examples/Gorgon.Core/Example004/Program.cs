﻿#region MIT.
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
// Created: Tuesday, September 18, 2012 8:00:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Plugins;

namespace Gorgon.Examples
{
	/// <summary>
	/// Entry point class.
	/// </summary>
	/// <remarks>
	/// This example will show how to load a plugin and how to build a simple plugin.
	/// 
	/// The plugin in composed of 2 parts:
	/// 1.  The plugin entry point object, which inherits from GorgonPlugin.
	/// 2.  The object that will be created by this entry point.
	/// 
	/// This is a factory pattern that allows the plugin to create instances of objects whose functionality we 
	/// wish to override and/or implement. It is these objects that can be used and swapped in and out of an 
	/// application.
	/// 
	/// The entry point object will be responsible for creating the concrete classes based on an abstract class
	/// in the host application (this abstract class must implement GorgonPlugin at minimum).  From there this 
	/// class should have a method that will create the object that we wish to use. An assembly (DLL, EXE, etc...) 
	/// may contain multiple plugin entry points to allow for returning multiple interfaces or could have some 
	/// form of input to allow the developer to determine which type of interface is returned. The entry point 
	/// should be an abstract object in the host application that is implemented one or more times in the plugin 
	/// assembly.
	/// 
	/// The plugin interface is the actual interface for the functionality.  It should be an interface or class
	/// that is inherited in the plugin assembly and will implement specific functionality for that plugin.
	/// 
	/// To load a plugin, the user should first load the assembly using the GorgonPluginAssemblyCache object. 
	/// This will load the assembly with the plugins that we want. Then a GorgonPluginService object should be created 
	/// to create an instance of the plugin interface by using the fully qualified type name of the plugin type.
	/// </remarks>
	static class Program
	{
		#region Methods.
		/// <summary>
		/// Function to retrieve a list of our plugin assemblies.
		/// </summary>
		/// <returns>The list of plugin assemblies.</returns>
		static IEnumerable<string> GetPluginAssemblies()
		{
			return Directory.EnumerateFiles(GorgonApplication.ApplicationDirectory, "Example004.*Plugin.dll");
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// Set up the assembly cache.
			// We'll need the assemblies loaded into this object in order to load our plugin types.
			var pluginAssemblies = new GorgonPluginAssemblyCache(GorgonApplication.Log);

			// Create our plugin service.
			// This takes the cache of assemblies that we just loaded.
			var pluginService = new GorgonPluginService(pluginAssemblies, GorgonApplication.Log);

			try
			{
				Console.Title = "Example #4 - Gorgon Plug-Ins.";
				Console.ForegroundColor = ConsoleColor.White;

				Console.WriteLine("This is an example to show how to create and use custom plugins.");
				Console.WriteLine("The plugin interface in Gorgon is quite flexible and gives the developer");
				Console.WriteLine("the ability to allow extensions to their own applications.\n");

				Console.ResetColor();

				var pluginFiles = GetPluginAssemblies().ToArray();

				// Load the plugins into Gorgon.
				foreach (string pluginPath in pluginFiles)
				{
					pluginAssemblies.Load(pluginPath);
				}

				Console.WriteLine("{0} plugin assemblies found.", pluginAssemblies.PluginAssemblies.Count);
				
				if (pluginFiles.Length == 0)
				{
					return;
				}

				// Our text writer plugin interfaces.
				IList<TextColorWriter> writers = new List<TextColorWriter>(); 

				// Create our plugin instances, we'll limit to 9 entries just for giggles.
				var plugins = (from pluginName in pluginService.GetPluginNames()
							   let plugin = pluginService.GetPlugin<TextColorPlugIn>(pluginName)
				               where plugin != null
				               select plugin).Take(9).ToArray();

				// Display a list of the available plugins.
				Console.WriteLine("\n{0} Plug-ins loaded:\n", plugins.Length);
				for (int i = 0; i < plugins.Length; i++)
				{
					// Here's where we make use of our description.
					Console.WriteLine("{0}. {1} ({2})", i + 1, plugins[i].Description, plugins[i].GetType().FullName);

					// Create the text writer interface and add it to the list.
					writers.Add(plugins.ElementAt(i).CreateWriter());
				}

				Console.Write("0. Quit\n\nSelect a plugin:  ");

				// Loop until we quit.
				while (true)
				{
					if (!Console.KeyAvailable)
					{
						continue;
					}

					Console.ResetColor();

					// Remember our cursor coordinates.
					int cursorX = Console.CursorLeft; // Cursor position.
					int cursorY = Console.CursorTop;

					var keyValue = Console.ReadKey(false);

					if (char.IsNumber(keyValue.KeyChar))
					{
						if (keyValue.KeyChar == '0')
						{
							break;
						}

						// Move to the next line and clear the previous line of text.
						Console.WriteLine();
						Console.Write(new string(' ', Console.BufferWidth - 1));
						Console.CursorLeft = 0;

						// Call our text color writer to print the text in the plugin color.
						int writerIndex = keyValue.KeyChar - '0';
						writers[writerIndex - 1].WriteString(string.Format("You pressed #{0}.", writerIndex));
					}

					Console.CursorTop = cursorY;
					Console.CursorLeft = cursorX;
				}
			}
			catch (Exception ex)
			{
				ex.Catch(_ =>
				         {
					         Console.Clear();
					         Console.ForegroundColor = ConsoleColor.Red;
					         Console.WriteLine("Exception:\n{0}\n\nStack Trace:{1}", ex.Message, ex.StackTrace);
				         },
				         GorgonApplication.Log);
				Console.ResetColor();
#if DEBUG
				Console.ReadKey(true);
#endif
			}
			finally
			{
				// Always call dispose so we can unload our temporary application domain.
				pluginAssemblies.Dispose();
				GorgonApplication.Log.Close();
			}
		}
		#endregion
	}
}
