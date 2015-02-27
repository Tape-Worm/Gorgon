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
using System.Threading;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Application context for the editor.
	/// </summary>
	sealed class AppContext
		: ApplicationContext, IAppContext
	{
		#region Variables.
		// Main splash screen.
		private FormSplash _splash;
		// The application log file.
		private GorgonLogFile _log;
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// The editor settings interface.
		private readonly IEditorSettings _settings;
		// The initialization factory.
		private IProxyObject<GorgonGraphics> _graphicsFactory;
		// The splash screen proxy for this application.
		private readonly IProxyObject<FormSplash> _splashProxy;
		// The plug-in factory.
		private readonly IPlugInRegistry _plugInFactory;
		#endregion

		#region Methods.
		/*		/// <summary>
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
					_splash.UpdateVersion(string.Format(Resources.GOREDIT_TEXT_PLUG_IN,
														Path.GetFileNameWithoutExtension(text)
															.Ellipses(40, true)));
				}

				/// <summary>
				/// Function to initialize the plug-ins interface.
				/// </summary>
				private void InitializePlugIns()
				{
					PlugIns.PlugInPath = Program.Settings.PlugInDirectory;
					PlugIns.UserDisabledPlugIns = Program.Settings.DisabledPlugIns.ToArray();

					EditorLogging.Print("Loading plug-ins...", LoggingLevel.Verbose);
					_splash.UpdateVersion(Resources.GOREDIT_TEXT_LOADING_PLUGINS);

					PlugIns.LoadPlugIns(UpdateSplashPlugInText);
				}

				/// <summary>
				/// Function to initialize the scratch files area.
				/// </summary>
				private void InitializeScratchArea()
				{
					ScratchArea.ScratchPath = Program.Settings.ScratchPath;

					EditorLogging.Print("Creating scratch area at \"{0}\"", LoggingLevel.Verbose, ScratchArea.ScratchPath);

					_splash.UpdateVersion(Resources.GOREDIT_TEXT_CREATING_SCRATCH);

					// Ensure that we're not being clever and trying to mess up our system.
					if (ScratchArea.CanAccessScratch(Program.Settings.ScratchPath) == ScratchAccessibility.SystemArea)
					{
						GorgonDialogs.ErrorBox(null, Resources.GOREDIT_ERR_CANNOT_USESYS_SCRATCH);
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
						EditorLogging.Print("Could not access scratch area at \"{0}\"", LoggingLevel.Verbose, Program.Settings.ScratchPath);

						if (ScratchArea.SetScratchLocation() == ScratchAccessibility.Canceled)
						{
							// Exit the application if we cancel.
							MainForm.Dispose();
							Gorgon.Quit();
							return;
						}

						EditorLogging.Print("Setting scratch area to \"{0}\".", LoggingLevel.Verbose, Program.Settings.ScratchPath);

						// Update with the new scratch path.
						Program.Settings.Save();
					}

					ScratchArea.InitializeScratch();

					// Get only the providers that are not disabled.
					var plugIns = from plugIn in Gorgon.PlugIns
								  where plugIn is GorgonFileSystemProviderPlugIn
								  && PlugIns.UserDisabledPlugIns.All(name => !string.Equals(name, plugIn.Name, StringComparison.OrdinalIgnoreCase))
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

						_splash.UpdateVersion(Resources.GOREDIT_TEXT_LOAD_PREV_FILE);

						if (!File.Exists(Program.Settings.LastEditorFile))
						{
							return;
						}

						FileManagement.Open(Program.Settings.LastEditorFile);
					}
					catch (Exception ex)
					{
						GorgonDialogs.ErrorBox(null, string.Format(Resources.GOREDIT_DLG_OPEN_ERROR, Program.Settings.LastEditorFile, ex.Message), null, ex);
						Program.Settings.LastEditorFile = string.Empty;
					}
				}

				/// <summary>
				/// Function to load the input interface.
				/// </summary>
				private void InitializeInput()
				{
					string inputPlugInPath = Path.Combine(Gorgon.ApplicationDirectory, "Gorgon.Input.Raw.dll");

					_splash.UpdateVersion(Resources.GOREDIT_TEXT_LOAD_RAW_INPUT);

					if (!File.Exists(inputPlugInPath))
					{
						throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_ERR_INPUT_COULD_NOT_LOAD);
					}

					if (!Gorgon.PlugIns.IsPlugInAssembly(inputPlugInPath))
					{
						throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_ERR_INPUT_COULD_NOT_LOAD);
					}

					// Ensure that it's signed with the same public key.
					byte[] key = GetType().Assembly.GetName().GetPublicKey();

					if ((key != null) && (key.Length != 0) 
						&& (Gorgon.PlugIns.IsAssemblySigned(inputPlugInPath, key) != PlugInSigningResult.Signed))
					{
						throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_ERR_INPUT_COULD_NOT_LOAD);
					}

					// Load the plug-in.
					Gorgon.PlugIns.LoadPlugInAssembly(inputPlugInPath);

					if (!Gorgon.PlugIns.Contains(ContentObject.GorgonRawInputTypeName))
					{
						throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_ERR_INPUT_COULD_NOT_LOAD);
					}
				}*/

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.ApplicationContext" /> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_graphicsFactory != null)
					{
						_graphicsFactory.Dispose();
					}

					if (_splashProxy != null)
					{
						_splashProxy.Dispose();
					}

					if (_log != null)
					{
						_log.Print("Gorgon Editor shutting down.", LoggingLevel.All);
						_log.Close();
					}
				}

				_graphicsFactory = null;
				_log = null;
				_disposed = true;
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to show the main application splash screen and form.
		/// </summary>
		public void Show()
		{
			float startTime = GorgonTiming.SecondsSinceStart;

			try
			{
				_log.Print("Gorgon Editor starting...", LoggingLevel.All);

				//PlugIns.DefaultImageEditorPlugIn = Program.Settings.DefaultImageEditor;
				
				// Create the splash form.
				//_splash = _formFactory.CreateForm<FormSplash, GorgonLogFile>(_log, null, false);
				_splash = _splashProxy.Item;

				_splash.Show();

				// Fade in our splash screen.
				_splash.Fade(true, 500.0f);

				// Create our graphics interface.
				var graphics = _graphicsFactory.Item;

				// Retrieve our plug-ins for the application.
				_plugInFactory.ScanAndLoadPlugIns();

				/*                
                InitializePlugIns();
                InitializeScratchArea();
				InitializeInput();
				
                FileManagement.InitializeFileTypes();

                // Load the last opened file.
                if ((Program.Settings.AutoLoadLastFile) && (!string.IsNullOrWhiteSpace(Program.Settings.LastEditorFile)))
                {
                    LoadLastFile();
                }*/

				// Set up the default pane.
				_splash.InfoText = Resources.GOREDIT_TEXT_STARTING;
				/*ContentManagement.DefaultContentType = typeof(DefaultContent);
				ContentManagement.LoadDefaultContentPane();*/

				// Keep showing the splash screen.
				while ((GorgonTiming.SecondsSinceStart - startTime) < 3)
				{
					Thread.Sleep(1);
				}

				_splash.Fade(false, 250.0f);

				// Get rid of the splash screen.
				_splashProxy.Dispose();

				// Bring up our application form.
				MainForm.Show();

				Gorgon.Run(this);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(null, ex));

				if ((MainForm != null) && (!MainForm.IsDisposed))
				{
					MainForm.Dispose();
				}

				_log.Close();

				// Signal quit.
				Gorgon.Quit();
			}
			finally
			{
				_splashProxy.Dispose();
				_splash = null;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="AppContext"/> class.
		/// </summary>
		/// <param name="log">The application log file.</param>
		/// <param name="mainForm">The instance of the main form.</param>
		/// <param name="settings">The editor settings.</param>
		/// <param name="graphicsFactory">The factory to create a new graphics interface.</param>
		/// <param name="splashProxy">The factory to create forms for the application.</param>
		/// <param name="plugInFactory">The factory to load plug-ins for the application.</param>
		public AppContext(GorgonLogFile log, FormMain mainForm, IEditorSettings settings, IProxyObject<GorgonGraphics> graphicsFactory, IProxyObject<FormSplash> splashProxy, IPlugInRegistry plugInFactory)
		{
			_graphicsFactory = graphicsFactory;
			_settings = settings;
			_splashProxy = splashProxy;
			_log = log;
			_plugInFactory = plugInFactory;
			MainForm = mainForm;
		}
		#endregion
	}
}
