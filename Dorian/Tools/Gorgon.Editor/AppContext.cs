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
using System.Threading;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.Input;
using GorgonLibrary.IO;
using GorgonLibrary.PlugIns;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Application context for the editor.
	/// </summary>
	class AppContext
		: ApplicationContext
	{
		#region Constants.
		private string GorgonRawInputTypeName = "GorgonLibrary.Input.GorgonRawPlugIn";			// Type name for the raw input plug-in.
		#endregion

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
			// does not, therefore resharper is wrong.
			_splash.UpdateVersion(string.Format(Resources.GOREDIT_SPLASH_LOAD_PLUGIN,
												Path.GetFileNameWithoutExtension(text)
													.Ellipses(40, true)));
		}

        /// <summary>
        /// Function to fade the splash screen in or out.
        /// </summary>
        /// <param name="fadeIn">TRUE to fade in, FALSE to fade out.</param>
        /// <param name="time">Time, in milliseconds, for the fade.</param>
	    private void FadeSplashScreen(bool fadeIn, float time)
        {
            double startTime = GorgonTiming.MillisecondsSinceStart;
            double delta = 0;

            // Fade the splash screen in.
            while (delta <= 1)
            {
                delta = (GorgonTiming.MillisecondsSinceStart - startTime) / time;

                _splash.Opacity = fadeIn ? delta  : 1.0f - delta;
            }
	    }

        /// <summary>
        /// Function to initialize the log file.
        /// </summary>
	    private void InitializeLogFile()
	    {
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
				// Purposely left empty.  We don't care about the log file in release mode.
			}
#endif
		}

        /// <summary>
        /// Function to initialize the graphics interface.
        /// </summary>
	    private void InitializeGraphics()
	    {
            // Initialize our graphics interface.
            _splash.UpdateVersion(Resources.GOREDIT_SPLASH_INIT_GFX);

            // Find the best device in the system.
            GorgonVideoDeviceEnumerator.Enumerate(false, false);

            GorgonVideoDevice bestDevice = GorgonVideoDeviceEnumerator.VideoDevices[0];

            // If we have more than one device, use the best available device.
            if (GorgonVideoDeviceEnumerator.VideoDevices.Count > 1)
            {
                bestDevice = (from device in GorgonVideoDeviceEnumerator.VideoDevices
                              orderby device.SupportedFeatureLevel descending, GorgonVideoDeviceEnumerator.VideoDevices.IndexOf(device)
                              select device).First();
            }

            Program.Graphics = new GorgonGraphics(bestDevice);
	    }

        /// <summary>
        /// Function to initialize the plug-ins interface.
        /// </summary>
	    private void InitializePlugIns()
	    {
            Program.LogFile.Print("Loading plug-ins...", LoggingLevel.Verbose);
            _splash.UpdateVersion(Resources.GOREDIT_SPLASH_LOAD_PLUGINS);

            PlugIns.LoadPlugIns(UpdateSplashPlugInText);
	    }

        /// <summary>
        /// Function to initialize the scratch files area.
        /// </summary>
	    private void InitializeScratchArea()
	    {
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

			// Get only the providers that are not disabled.
			var plugIns = from plugIn in Gorgon.PlugIns
						  where plugIn is GorgonFileSystemProviderPlugIn
						  && Program.Settings.DisabledPlugIns.All(name => !string.Equals(name, plugIn.Name, StringComparison.OrdinalIgnoreCase))
						  select plugIn;

	        foreach (GorgonPlugIn plugIn in plugIns)
	        {
		        ScratchArea.ScratchFiles.Providers.LoadProvider(plugIn.Name);
	        }
	    }

        /// <summary>
        /// Function to load the previous file.
        /// </summary>
	    private void LoadLastFile()
	    {
            try
            {
                if (!ScratchArea.ScratchFiles.Providers.Any(item => item.CanReadFile(Program.Settings.LastEditorFile)))
                {
                    return;
                }

                _splash.UpdateVersion(Resources.GOREDIT_SPLASH_LOAD_PREV_FILE);
                FileManagement.Open(Program.Settings.LastEditorFile);
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(null, string.Format(Resources.GOREDIT_FILE_CANNOT_OPEN, Program.Settings.LastEditorFile), null, ex);
                Program.Settings.LastEditorFile = string.Empty;
            }
	    }

		/// <summary>
		/// Function to load the input interface.
		/// </summary>
		private void InitializeInput()
		{
			string inputPlugInPath = Path.Combine(Gorgon.ApplicationDirectory, "Gorgon.Input.Raw.dll");

			_splash.UpdateVersion(Resources.GOREDIT_SPLASH_LOADING_INPUT);

			if (!File.Exists(inputPlugInPath))
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_INPUT_COULD_NOT_LOAD);
			}

			if (!Gorgon.PlugIns.IsPlugInAssembly(inputPlugInPath))
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_INPUT_COULD_NOT_LOAD);
			}

			// Ensure that it's signed with the same public key.
			byte[] key = GetType().Assembly.GetName().GetPublicKey();

			if ((key != null) && (key.Length != 0) 
				&& (Gorgon.PlugIns.IsAssemblySigned(inputPlugInPath, key) != PlugInSigningResult.Signed))
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_INPUT_COULD_NOT_LOAD);
			}

			// Load the plug-in.
			Gorgon.PlugIns.LoadPlugInAssembly(inputPlugInPath);

			if (!Gorgon.PlugIns.Contains(GorgonRawInputTypeName))
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_INPUT_COULD_NOT_LOAD);
			}

			// Create the factory.
			Program.Input = GorgonInputFactory.CreateInputFactory(GorgonRawInputTypeName);
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
		    float startTime = GorgonTiming.SecondsSinceStart;

			try
			{
				_splash = new formSplash();
				MainForm = new formMain();

				_splash.Show();
				_splash.Refresh();

                // Fade in our splash screen.
				FadeSplashScreen(true, 500.0f);

                InitializeLogFile();
                InitializeGraphics();
                InitializePlugIns();
                InitializeScratchArea();
				InitializeInput();

                FileManagement.InitializeFileTypes();
                ContentManagement.InitializeContentFileTypes();

                // Load the last opened file.
                if ((Program.Settings.AutoLoadLastFile) && (!string.IsNullOrWhiteSpace(Program.Settings.LastEditorFile)))
                {
                    LoadLastFile();
                }

                // Set up the default pane.
				_splash.UpdateVersion(Resources.GOREDIT_SPLASH_LOAD_DEFAULT_CONTENT);
				ContentManagement.LoadDefaultContentPane();
                
                // Keep showing the splash screen.
				while ((GorgonTiming.SecondsSinceStart - startTime) < 3)
				{
					Thread.Sleep(1);
				}

                FadeSplashScreen(false, 250.0f);

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
