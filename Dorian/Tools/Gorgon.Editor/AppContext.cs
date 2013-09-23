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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Application context for the editor.
	/// </summary>
	class AppContext
		: ApplicationContext
	{
		#region Variables.
		private readonly formSplash _splash;			// Main splash screen.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the splash screen when a plug-in is loaded.
		/// </summary>
		/// <param name="text">Plug-in name to display.</param>
		private void UpdateSplashPlugInText(string text)
		{
			// This had to be moved into this function because resharper complained
			// about accessing a disposed object from a closure.  Even though the
			// _splash object could not be disposed until well after the closure
			// had completed execution.  By all rights, this should fail too, and
			// does not.
			_splash.UpdateVersion(string.Format(Resources.GOREDIT_SPLASH_LOAD_PLUGIN,
												Path.GetFileNameWithoutExtension(text)
													.Ellipses(40, true)));
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
			try
			{
				_splash = new formSplash();
				MainForm = new formMain();

				_splash.Show();
				_splash.Refresh();

				var timer = new GorgonTimer();

				// Fade the splash screen in.
				while (_splash.Opacity < 1)
				{
					if (timer.Milliseconds <= 7)
					{
						continue;
					}

					timer.Reset();
					_splash.Opacity += 0.01;
				}
								
				timer.Reset();

				_splash.UpdateVersion("Creating logger...");
				Program.LogFile = new GorgonLogFile("Gorgon.Editor", "Tape_Worm");
				try
				{
					Program.LogFile.Open();
				}
				catch (Exception ex)
				{
#if DEBUG
					// If we can't open the log file in debug mode, let us know about it.
					GorgonDialogs.ErrorBox(null, ex);
#endif
				}

                // Initialize our graphics interface.
                _splash.UpdateVersion(Resources.GOREDIT_SPLASH_INIT_GFX);
                Program.InitializeGraphics();

                Program.LogFile.Print("Loading plug-ins...", LoggingLevel.Verbose);
                _splash.UpdateVersion(Resources.GOREDIT_SPLASH_LOAD_PLUGINS);

				PlugIns.LoadPlugIns(UpdateSplashPlugInText);

				Program.LogFile.Print("Creating scratch area at \"{0}\"", LoggingLevel.Verbose, Program.Settings.ScratchPath);

				_splash.UpdateVersion(Resources.GOREDIT_SPLASH_CREATE_SCRATCH);
							
				// Ensure that we're not being clever and trying to mess up our system.
				if (ScratchArea.CanAccessScratch(Program.Settings.ScratchPath) == ScratchAccessibility.SystemArea)
				{
					GorgonDialogs.ErrorBox(null, Resources.GOREDIT_CANNOT_USESYS_SCRATCH);
				}
				else
				{
					// Destroy previous scratch area files if possible.
					// Do not do this when we have it set to a system area, this will keep us from 
					// destroying anything critical.
					ScratchArea.CleanOldScratchAreas();
				}

				// Ensure we can actually access the scratch area.
				while (ScratchArea.CanAccessScratch(Program.Settings.ScratchPath) != ScratchAccessibility.Accessible)
				{
					Program.LogFile.Print("Could not access scratch area at \"{0}\"", LoggingLevel.Verbose, Program.Settings.ScratchPath);

					if (ScratchArea.SetScratchLocation() == ScratchAccessibility.Canceled)
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
                
                ScratchArea.InitializeScratch();
				ScratchArea.ScratchFiles.Providers.LoadAllProviders();

                // Load the last opened file.
                if ((Program.Settings.AutoLoadLastFile) && (!string.IsNullOrWhiteSpace(Program.Settings.LastEditorFile)))
                {
                    try
                    {
						if (ScratchArea.ScratchFiles.Providers.Any(item => item.CanReadFile(Program.Settings.LastEditorFile)))
						{
							_splash.UpdateVersion(Resources.GOREDIT_SPLASH_LOAD_PREV_FILE);
							Program.OpenEditorFile(Program.Settings.LastEditorFile);
						}
                    }
                    catch (Exception ex)
                    {
                        GorgonDialogs.ErrorBox(null, "There was an error opening '" + Program.Settings.LastEditorFile + "'", ex);
                        Program.Settings.LastEditorFile = string.Empty;
                    }
                }

                // Set up the default pane.
				((formMain)MainForm).LoadContentPane<DefaultContent>();

                // Keep showing the splash screen.
				while (timer.Milliseconds < 3000)
				{
					System.Threading.Thread.Sleep(1);
				}

				// Fade it out.
				while (_splash.Opacity > 0.02)
				{
					if (timer.Milliseconds <= 5)
					{
						continue;
					}

					timer.Reset();
					_splash.Opacity -= 0.01;
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
				{
					_splash.Dispose();
				}
				_splash = null;
			}
		}
		#endregion
	}
}
