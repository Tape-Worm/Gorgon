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
// Created: Tuesday, September 18, 2012 8:00:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Core;

namespace Gorgon.Examples
{
	/// <summary>
	/// Entry point class.
	/// </summary>
	/// <remarks>
	/// This example will show how to load a plug-in and how to build a simple plug-in.
	/// 
	/// The plug-in in composed of 2 parts:
	/// 1.  The plug-in entry point object.
	/// 2.  The plug-in interface.
	/// 
	/// The entry point object is responsible for creating the concrete classes based on an abstract class
	/// in the host application.  It should have a method that will create the plug-in interface.  An assembly
	/// (DLL, EXE, etc...) may contain multiple plug-in entry points to allow for returning multiple interfaces
	/// or could have some form of input to allow the developer to determine which type of interface is 
	/// returned.  The entry point should be an abstract object in the host application that is implemented
	/// one or more times in the plug-in assembly.
	/// 
	/// The plug-in interface is the actual interface for the functionality.  It should be an interface or class
	/// that is inherited in the plug-in assembly and will implement specific functionality for that plug-in.
	/// 
	/// To load a plug-in, the user should first load the assembly using the GorgonApplication.PlugIns.LoadPlugInAssembly 
	/// method.  This will load all the plug-in entry point object types from that assembly.  The developer can
	/// then look up the plug-in entry point by the fully qualified type name of the plug-in and create an instance
	/// of the plug-in interface.
	/// </remarks>
	static class Program
	{
		#region Methods.
		/// <summary>
		/// Function to retrieve a list of our plug-in assemblies.
		/// </summary>
		/// <returns>The list of plug-in assemblies.</returns>
		static IEnumerable<string> GetPlugInAssemblies()
		{
			return Directory.EnumerateFiles(GorgonApplication.ApplicationDirectory, "Example004.*PlugIn.dll");
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
		    try
			{
			    Console.Title = "Example #4 - Gorgon Plug-Ins.";
				Console.ForegroundColor = ConsoleColor.White;

				Console.WriteLine("This is an example to show how to create and use custom plug-ins.");
				Console.WriteLine("The plug-in interface in Gorgon is quite flexible and gives the developer");
				Console.WriteLine("the ability to allow extensions to their own applications.\n");

				Console.ResetColor();

				var plugInFiles = GetPlugInAssemblies().ToArray();

			    Console.WriteLine("{0} plug-in assemblies found.", plugInFiles.Count());
				if (plugInFiles.Length == 0)
				{					
					return;
				}

				// Load the plug-ins into Gorgon (only take the first 9).
				IList<TextColorWriter> writers = new List<TextColorWriter>();			// Our text writer plug-in interfaces.
				foreach (var path in plugInFiles)
				{
					GorgonApplication.PlugIns.LoadPlugInAssembly(path);
				}

				// Get our plug-ins.
				// But limit to 9 entries.
				var plugIns = (from plugIn in GorgonApplication.PlugIns
								let textPlugIn = plugIn as TextColorPlugIn
								where textPlugIn != null
								select textPlugIn).Take(9).ToArray();

				// Display a list of the available plug-ins.
				Console.WriteLine("\n{0} Plug-ins loaded:\n", plugIns.Length);
                for (int i = 0; i < plugIns.Length; i++)
				{
					// Here's where we make use of our description.
					Console.WriteLine("{0}. {1} ({2})", i + 1, plugIns[i].Description, plugIns[i].GetType().FullName);

					// Create the text writer interface and add it to the list.
					writers.Add(plugIns.ElementAt(i).CreateWriter());
				}

				Console.Write("0. Quit\n\nSelect a plug-in:  ");

				// Loop until we quit.
				while (true)
				{
				    if (!Console.KeyAvailable)
				    {
				        continue;
				    }

				    Console.ResetColor();

				    // Remember our cursor coordinates.
				    int cursorX = Console.CursorLeft;		// Cursor position.
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

				        // Call our text color writer to print the text in the plug-in color.
				        int writerIndex = keyValue.KeyChar - '0';
				        writers[writerIndex - 1].WriteString(string.Format("You pressed #{0}.", writerIndex));
				    }

				    Console.CursorTop = cursorY;
				    Console.CursorLeft = cursorX;
				}
			}
			catch (Exception ex)
			{
				// Catch all exceptions here.  If we had logging for the application enabled, then this 
				// would record the exception in the log.
				GorgonException.Catch(ex, () => {
					Console.Clear();
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Exception:\n{0}\n\nStack Trace:{1}", ex.Message, ex.StackTrace);					
				});
				Console.ResetColor();
			}
		}
		#endregion
	}
}
