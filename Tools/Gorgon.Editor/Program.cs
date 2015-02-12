#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Tuesday, February 10, 2015 11:14:25 PM
// 
#endregion

using System;
using System.Linq;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.UI;
using StructureMap;
using StructureMap.Pipeline;
using StructureMap.Graph;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Main entry point to the application.
	/// </summary>
	static class Program
	{
		/// <summary>
		/// Function to build the object graph for the application.
		/// </summary>
		/// <returns>The container for our object graph.</returns>
		private static Container BuildGraph()
		{
			return new Container(obj =>
			                     {
				                     obj.For<GorgonLogFile>()
					                    .Use<GorgonLogFile>(() => new GorgonLogFile("Gorgon.Editor", "Tape_Worm"))
											.OnCreation("Open log",
											            log =>
											            {
															// Set the logging level based on whatever is in our config file.
															Gorgon.Log.LogFilterLevel = log.LogFilterLevel = Settings.Default.LogLevel;

															// Register this log with Gorgon's exception handler.
															GorgonException.Logs.Add(log);

												            try
												            {
													            log.Open();
												            }
												            catch
												            {
													            // We don't care about exceptions while opening the log file.
																// This can happen if two instances of the application are running, 
																// and while this is not supported, there's no need to worry about
																// whether we can use the log file or not from the 2nd app.
												            }
											            });

				                     obj.For<FormSplash>()
					                    .Use<FormSplash>();

				                     obj.For<FormMain>()
				                        .Use<FormMain>();

				                     obj.For<AppContext>()
					                    .Use<AppContext>();
			                     });
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Container objectContainer = null;
			AppContext app = null;

			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				// Set up the plug-in assembly resolver to handle missing assemblies.
				Gorgon.PlugIns.AssemblyResolver = (appDomain, e) => appDomain.GetAssemblies()
				                                                             .FirstOrDefault(assembly => string.Equals(assembly.FullName, e.Name, StringComparison.Ordinal));

				objectContainer = BuildGraph();
				
				app = objectContainer.GetInstance<AppContext>();
				app.Show();
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(null, ex));
			}
			finally
			{
				if (app != null)
				{
					app.Dispose();
				}

				if (objectContainer != null)
				{
					objectContainer.Dispose();
				}
			}
		}
	}
}
