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
using System.Linq;
using System.Windows.Forms;
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
		/// Property to set or return the settings for the application.
		/// </summary>
		public static GorgonEditorSettings Settings
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

				Gorgon.PlugIns.AssemblyResolver = (appDomain, e) => appDomain.GetAssemblies()
				                                                             .FirstOrDefault(assembly => assembly.FullName == e.Name);

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

				// Clean up the plug-ins.
				foreach (var plugInItem in PlugIns.ContentPlugIns)
				{
					plugInItem.Value.Dispose();
				}

				foreach (var plugInItem in PlugIns.WriterPlugIns)
				{
					plugInItem.Value.Dispose();
				}

				// Shut down the graphics interface.
				if (ContentObject.Graphics != null)
				{
					ContentObject.Graphics.Dispose();
					ContentObject.Graphics = null;
				}

				// Clean up temporary files in scratch area.
				if (Settings != null)
				{
					ScratchArea.DestroyScratchArea();
				}

				EditorLogging.Close();
			}
		}
	}
}
