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
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.UI;
using StructureMap;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Main entry point to the application.
	/// </summary>
	static class Program
	{
		// The container for our object graph.
		private static Container _objectContainer;

		/// <summary>
		/// Function to build the object graph for the application.
		/// </summary>
		/// <param name="obj">The configuration for our application.</param>
		/// <returns>The container for our object graph.</returns>
		private static void BuildGraph(ConfigurationExpression obj)
		{
			obj.ForConcreteType<GorgonLogFile>()
			   .Configure
			   .Ctor<string>("appname").Is("Gorgon.Editor")
			   .Ctor<string>("extraPath").Is("Tape_Worm")
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

			obj.For<IEditorSettings>()
			   .Use<EditorSettings>()
			   .OnCreation("Load settings",
			               settings =>
			               {
				               // Attempt to load our settings, but if it fails, then just reset the object and log the exception.
				               try
				               {
					               settings.Reset();
					               settings.Load();
				               }
				               catch (Exception ex)
				               {
					               settings.Reset();
					               GorgonException.Catch(ex);
				               }
			               });


			obj.For<IProxyObject<FormSplash>>()
			   .Use<ProxyObject<FormSplash>>();

			obj.For<IVideoDeviceSelector>()
			   .Use<VideoDeviceSelector>();

			obj.For<IGraphicsService>()
			   .Use<GraphicsService>();

			obj.For<IContentModel>()
			   .Use<NoContentModel>();

			obj.For<IEditorContentManager>()
			   .Use<EditorContentManager>();

			obj.ForConcreteType<FormMain>();

			obj.For<IPlugInRegistry>()
			   .Use<PlugInRegistry>();

			obj.ForConcreteType<GorgonFileSystem>()
			   .Configure
			   .AlwaysUnique();

			obj.For<IEditorFileSystem>()
			   .Use<EditorFileSystem>()
			   .Ctor<string>("filePath")
			   .Is(string.Empty);

			obj.For<IFileSystemService>()
			   .Use<EditorFileSystemService>();

			obj.For<IScratchArea>()
			   .Use<ScratchArea>();

			obj.For<IScratchLocator>()
			   .Use<ScratchLocator>();

			obj.For<IScratchService>()
			   .Use<ScratchService>();

			obj.For<IAppContext>()
			   .Use<AppContext>();
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			AppContext app = null;

			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				// Set up the plug-in assembly resolver to handle missing assemblies.
				Gorgon.PlugIns.AssemblyResolver = (appDomain, e) => appDomain.GetAssemblies()
				                                                             .FirstOrDefault(assembly => string.Equals(assembly.FullName, e.Name, StringComparison.Ordinal));

				_objectContainer = new Container(BuildGraph);
				
				app = _objectContainer.GetInstance<AppContext>();
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

				if (_objectContainer != null)
				{
					_objectContainer.Dispose();
				}
			}
		}
	}
}
