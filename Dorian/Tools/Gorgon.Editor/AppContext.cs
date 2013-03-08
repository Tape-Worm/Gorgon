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
// Created: Monday, April 30, 2012 9:20:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GorgonLibrary.UI;
using GorgonLibrary.FileSystem;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Application context for the editor.
	/// </summary>
	class AppContext
		: ApplicationContext
	{
		#region Variables.
		private formSplash _splash = null;			// Main splash screen.
		#endregion

		#region Methods.
        /// <summary>
        /// Function to load the plug-ins for the editor.
        /// </summary>
        private void LoadPlugIns()
        {
            int i = 0;

            // Load plug-ins.
            while (i < Program.Settings.PlugIns.Count)
            {
                string plugInPath = Program.Settings.PlugIns[i];

                Program.LogFile.Print("Loading plug-in assembly \"{0}\".", LoggingLevel.Verbose, plugInPath);
                if (File.Exists(Program.Settings.PlugIns[i]))
                {
                    _splash.UpdateVersion("Plug-in: " + Path.GetFileNameWithoutExtension(plugInPath));

                    // Ensure that we can load this assembly.
                    if (!Gorgon.PlugIns.IsPlugInAssembly(plugInPath))
                    {
                        Program.LogFile.Print("Assembly \"{0}\" is not a valid plug-in assembly.", LoggingLevel.Verbose, plugInPath);
                        GorgonDialogs.ErrorBox(null, "The assembly '" + plugInPath + "' is not a plug-in assembly.");
                        Program.Settings.PlugIns.RemoveAt(i);
                        continue;
                    }

                    // Make sure there are types that we can use in here.
                    var plugInTypes = Gorgon.PlugIns.EnumeratePlugIns(plugInPath);
                    if (plugInTypes.Count == 0)
                    {
                        Program.LogFile.Print("Assembly \"{0}\" does not contain any plug-ins.", LoggingLevel.Verbose, plugInPath);
                        GorgonDialogs.ErrorBox(null, "The assembly '" + plugInPath + "' does not contain any plug-ins.");
                        Program.Settings.PlugIns.RemoveAt(i);
                        continue;
                    }

                    // Finally load the assembly.
                    var assemblyName = Gorgon.PlugIns.LoadPlugInAssembly(Program.Settings.PlugIns[i]);
                    // Get the plug-ins from the assembly.
                    var plugIns = Gorgon.PlugIns.EnumeratePlugIns(assemblyName).Where(item => item is EditorPlugIn || item is GorgonFileSystemProviderPlugIn);

                    if (plugIns.Count() == 0)
                    {
                        Program.LogFile.Print("Assembly \"{0}\" does not contain any editor compatible plug-ins.", LoggingLevel.Verbose, plugInPath);
                        GorgonDialogs.ErrorBox(null, "The assembly '" + plugInPath + "' does not contain any editor compatible plug-ins.");
                        Program.Settings.PlugIns.RemoveAt(i);
                        continue;
                    }

                    foreach (var plugIn in plugIns)
                    {
                        var fileSystemProvider = plugIn as GorgonFileSystemProviderPlugIn;
                        var editorPlugIn = plugIn as EditorPlugIn;

                        if (fileSystemProvider != null)
                        {
                            Program.LogFile.Print("Found a file system provider plug-in: \"{0}\".", LoggingLevel.Verbose, fileSystemProvider.Name);
                        }

                        if (editorPlugIn == null)
                        {
                            continue;
                        }

                        Program.LogFile.Print("Found a {0} plug-in: \"{1}\".", LoggingLevel.Verbose, editorPlugIn.PlugInType.ToString(), editorPlugIn.Description);

                        // Categorize the editor plug-ins.
                        switch (editorPlugIn.PlugInType)
                        {
                            case PlugInType.Content:
                                ContentPlugIn contentPlugIn = editorPlugIn as ContentPlugIn;

                                if (contentPlugIn != null)
                                {
                                    Program.ContentPlugIns[editorPlugIn.Name] = contentPlugIn;
                                }
                                break;
                            case PlugInType.FileWriter:
                                // TODO: Write this.
                                break;
                        }
                    }
                }
                else
                {
                    Program.LogFile.Print("Plug-in assembly \"{0}\" was not found.", LoggingLevel.Verbose, plugInPath);
                }

                i++;
            }
        }

        /// <summary>
        /// Function to determine if the scratch area is accessible.
        /// </summary>
        /// <param name="path">Path to the scratch area.</param>
        /// <returns>TRUE if accessible, FALSE if not.</returns>
        private bool CanAccessScratch()
		{
			// Ensure that the device exists or is ready.
			if (!Directory.Exists(Path.GetPathRoot(Program.Settings.ScratchPath)))
			{
				return false;
			}

			try
			{
				DirectoryInfo directoryInfo = null;

				if (!Directory.Exists(Program.Settings.ScratchPath))
				{
					// If we created the directory, then hide it.
					directoryInfo = new DirectoryInfo(Program.Settings.ScratchPath);
					directoryInfo.Create();
					directoryInfo.Attributes = FileAttributes.Hidden | FileAttributes.NotContentIndexed;
					return true;
				}

				directoryInfo = new DirectoryInfo(Program.Settings.ScratchPath);
				var acl = directoryInfo.GetAccessControl();				
			}
			catch(Exception ex)
			{
				GorgonException.Log = Program.LogFile;
				GorgonException.Catch(ex);
				GorgonException.Log = Gorgon.Log;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Calls <see cref="M:System.Windows.Forms.ApplicationContext.ExitThreadCore"/>, which raises the <see cref="E:System.Windows.Forms.ApplicationContext.ThreadExit"/> event.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnMainFormClosed(object sender, EventArgs e)
		{
			base.OnMainFormClosed(sender, e);
			ExitThread();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="AppContext"/> class.
		/// </summary>
		public AppContext()
		{
			formMain mainForm = null;

			try
			{
				GorgonTimer timer = null;		

				_splash = new formSplash();
				MainForm = mainForm = new formMain();

				_splash.Show();
				_splash.Refresh();

				timer = new GorgonTimer();

				// Fade the splash screen in.
				while (_splash.Opacity < 1)
				{
					if (timer.Milliseconds > 7)
					{
						timer.Reset();
						_splash.Opacity += 0.01;
					}
				}
								
				timer.Reset();

				_splash.UpdateVersion("Creating logger...");
				Program.LogFile = new GorgonLogFile("Gorgon.Editor", "Tape_Worm");
				try
				{
					Program.LogFile.Open();
				}
#if DEBUG
				catch (Exception ex)
				{
					// If we can't open the log file in debug mode, let us know about it.
					GorgonDialogs.ErrorBox(null, ex);
				}
#else
				catch
				{
					// Do nothing.
				}
#endif
                Program.LogFile.Print("Loading plug-ins...", LoggingLevel.Verbose);
                _splash.UpdateVersion("Loading plug-ins...");
                LoadPlugIns();

				Program.LogFile.Print("Creating scratch area at \"{0}\"", LoggingLevel.Verbose, Program.Settings.ScratchPath);

				_splash.UpdateVersion("Creating scratch area...");

				Program.ScratchFiles = new FileSystem.GorgonFileSystem();

				// Destroy previous files.
				Program.CleanUpScratchArea();

				// Ensure we can actually access the scratch area.
				while (!CanAccessScratch())
				{
					Program.LogFile.Print("Could not access scratch area at \"{0}\"", LoggingLevel.Verbose, Program.Settings.ScratchPath);
					Program.Settings.ScratchPath = Program.SetScratchLocation();

					if (Program.Settings.ScratchPath == null)
					{
						// Exit the application if we cancel.
						MainForm.Dispose();
						Gorgon.Quit();
						return;
					}

					Program.LogFile.Print("Setting scratch area to \"{0}\".", LoggingLevel.Verbose, Program.Settings.ScratchPath);

					// Update with the new scratch path.
					Program.Settings.Save();
				}

				Program.ScratchFiles.WriteLocation = Program.Settings.ScratchPath;

				// Set the directory to hidden.  We don't want users really messing around in here if we can help it.
				var scratchDir = new System.IO.DirectoryInfo(Program.Settings.ScratchPath);
				if (((scratchDir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
					|| ((scratchDir.Attributes & FileAttributes.NotContentIndexed) != FileAttributes.NotContentIndexed))
				{
					scratchDir.Attributes = System.IO.FileAttributes.NotContentIndexed | System.IO.FileAttributes.Hidden;
				}

                _splash.UpdateVersion("Loading file system providers...");
                // Get any file system providers.
                Program.ScratchFiles.AddAllProviders();
				// TODO: Remove this, just here for testes.
				Program.ScratchFiles.Mount(@"..\..\..\..\Resources\FileSystems\BZipFileSystem.gorPack");

				_splash.UpdateVersion("Initializing graphics...");
				Program.InitializeGraphics();
				mainForm.LoadContentPane<DefaultContent>();

				_splash.UpdateVersion(string.Empty);

				while (timer.Milliseconds < 3000)
					System.Threading.Thread.Sleep(1);

				// Fade it out.
				while (_splash.Opacity > 0.02)
				{
					if (timer.Milliseconds > 5)
					{
						timer.Reset();
						_splash.Opacity -= 0.01;
					}
				}

				// Bring up our application form.
				MainForm.Show();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);

				if ((MainForm != null) && (!MainForm.IsDisposed))
				{
					MainForm.Dispose();
				}

				// Signal quit.
				Gorgon.Quit();
			}
			finally
			{
				if (_splash != null)
					_splash.Dispose();
				_splash = null;
			}
		}
		#endregion
	}
}
