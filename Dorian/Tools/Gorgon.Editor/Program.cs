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
// Created: Monday, April 30, 2012 6:28:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Main application interface.
	/// </summary>
	static class Program
    {
        #region Properties.
		/// <summary>
		/// Property to return the logging interface for the application.
		/// </summary>
		public static GorgonLogFile LogFile
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the settings for the application.
		/// </summary>
		public static GorgonEditorSettings Settings
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the graphics interface.
		/// </summary>
		public static GorgonGraphics Graphics
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="Program"/> class.
		/// </summary>
		static Program()
		{
			Settings = new GorgonEditorSettings();
		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				Settings.Load();

				Gorgon.PlugIns.AssemblyResolver = (appDomain, e) =>
				                                  {
					                                  var assemblies = appDomain.GetAssemblies();

					                                  // ReSharper disable once LoopCanBeConvertedToQuery
					                                  // ReSharper disable once ForCanBeConvertedToForeach
					                                  for (int i = 0; i < assemblies.Length; i++)
					                                  {
						                                  var assembly = assemblies[i];

						                                  if (assembly.FullName == e.Name)
						                                  {
							                                  return assembly;
						                                  }
					                                  }
					                                  return null;
				                                  };

				Gorgon.Run(new AppContext());
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
			}
			finally
			{
                Gorgon.PlugIns.AssemblyResolver = null;

				ContentManagement.UnloadCurrentContent();

				// Shut down the graphics interface.
				if (Graphics != null)
				{
					Graphics.Dispose();
					Graphics = null;
				}

				// Clean up temporary files in scratch area.
				if (Settings != null)
				{
					ScratchArea.DestroyScratchArea();
				}

				// Close the logging file.
				if (LogFile != null)
				{
					LogFile.Close();
				}
			}
		}
	}
}
