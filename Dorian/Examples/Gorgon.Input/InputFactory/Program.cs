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
using System.Linq;
using System.IO;
using System.Reflection;
using GorgonLibrary;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.UI;
using GorgonLibrary.PlugIns;
using GorgonLibrary.Input;

namespace GorgonLibrary.Examples
{
	/// <summary>
	/// Entry point class.
	/// </summary>
	/// <remarks>
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
				string plugInPath = Properties.Settings.Default.InputPlugInPath;

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
			IList<Tuple<GorgonInputPlugIn, GorgonInputFactory>> result = new Tuple<GorgonInputPlugIn, GorgonInputFactory>[] { };
			var files = Directory.GetFiles(PlugInPath, "*.dll");

			if (files.Length == 0)
			{
				return result;
			}
				
			// Find our plug-ins in the DLLs.
			foreach(var file in files)
			{
				var name = AssemblyName.GetAssemblyName(file);

				// Skip any assemblies that aren't a plug-in assembly.
				if (!Gorgon.PlugIns.IsPlugInAssembly(name))
				{
					continue;
				}

				// Load the assembly DLL.
				Gorgon.PlugIns.LoadPlugInAssembly(name);

				// Now try to retrieve our input plug-ins.
				var plugIns = Gorgon.PlugIns.EnumeratePlugIns(name);
				var inputPlugIns = plugIns
									.Where(item => item is GorgonInputPlugIn)
									.Select(item => item.GetType());

				// If we have input plug-ins, and we haven't initialized our list, then do so now.
				if ((result.Count == 0) && (inputPlugIns.Count() > 0))
				{
					result = new List<Tuple<GorgonInputPlugIn, GorgonInputFactory>>();
				}

				// Add each input factory to our list.
				foreach (var inputPlugIn in inputPlugIns)
				{
					var factory = GorgonInputFactory.CreateInputFactory(inputPlugIn);

					if (factory != null)
					{
						result.Add(new Tuple<GorgonInputPlugIn, GorgonInputFactory>(
							(GorgonInputPlugIn)Gorgon.PlugIns[inputPlugIn.FullName], 
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
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Loading input plug-ins...");

				// Load our input factory plug-ins.
				var inputPlugIns = GetInputPlugIns();

				if (inputPlugIns.Count == 0)
				{
					Console.ResetColor();
					Console.WriteLine("Could not find any input factory plug-ins.");
					return;
				}
				else
				{
					Console.WriteLine();
					Console.WriteLine("{0} input factory plug-ins found:", inputPlugIns.Count);
					Console.ForegroundColor = ConsoleColor.Cyan;
					foreach (var plugIn in inputPlugIns)
					{
						Console.WriteLine("{0}", plugIn.Item1.Description);
					}
				}

				Console.ReadKey();
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
#if DEBUG
				Console.ReadKey();
#endif
			}
		}
		#endregion
	}
}
