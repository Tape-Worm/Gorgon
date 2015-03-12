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
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
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
		// The graphics service.
		private readonly IGraphicsService _graphicsService;
		// The graphics interface.
		private GorgonGraphics _graphics;
		// The splash screen proxy for this application.
		private readonly IProxyObject<FormSplash> _splashProxy;
		// The plug-in factory.
		private readonly IPlugInRegistry _plugInFactory;
		// Services for scratch area manipulation.
		private readonly IScratchService _scratchService;
		// The file system service for packed files.
		private readonly IFileSystemService _fileSystemService;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the scratch files area.
		/// </summary>
		/// <returns>TRUE if the init was successful, FALSE if not.</returns>
		private bool InitializeScratchArea()
		{
			_splash.InfoText = Resources.GOREDIT_TEXT_CLEANING_STALE_SCRATCH;

			// Clean out previous versions of the scratch file area.
			_scratchService.ScratchArea.CleanUp(true);

			_splash.InfoText = Resources.GOREDIT_TEXT_CREATING_SCRATCH;

			if ((!string.IsNullOrWhiteSpace(_settings.ScratchPath))
				&& (_scratchService.ScratchArea.SetScratchDirectory(_settings.ScratchPath) == ScratchAccessibility.Accessible))
			{
				return true;
			}

			ScratchAccessibility result;

			do
			{
				result = _scratchService.ScratchLocator.ChangeLocation();

				if (result == ScratchAccessibility.Canceled)
				{
					return false;
				}
			} while (result != ScratchAccessibility.Accessible);

			// Update the settings config file.
			_settings.Save();

			return true;
		}

		/// <summary>
		/// Function to perform initialization on the file system service.
		/// </summary>
		private void InitializeFileSystemService()
		{
			// Retrieve all the file system providers.
			_fileSystemService.LoadFileSystemProviders();

			if ((!_settings.AutoLoadLastFile)
				|| (string.IsNullOrWhiteSpace(_settings.LastEditorFile)))
			{
				return;
			}

			string filePath = string.Empty;

			try
			{
				filePath = Path.GetFullPath(_settings.LastEditorFile);

				// If the file no longer exists, then it's no cause for alarm.  Just leave.
				if (!File.Exists(filePath))
				{
					_log.Print("FileSystemService: Could not auto load the file '{0}'.  The file was not found.", LoggingLevel.Verbose, filePath);
					return;
				}

				// Attempt to load the last file that we saved.
				_splash.InfoText = Resources.GOREDIT_TEXT_LOAD_PREV_FILE;
				
				// Load the last file and send it on to the main application.
				((FormMain)MainForm).CurrentFile = _fileSystemService.LoadFile(filePath);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex,
				                      () =>
				                      GorgonDialogs.ErrorBox(_splash,
				                                             string.Format(Resources.GOREDIT_ERR_OPEN_PACK_FILE, filePath.Ellipses(80, true)),
				                                             null,
															 ex));

				// Reset the last file loaded if we can no longer load it.
				_settings.LastEditorFile = string.Empty;
			}
		}

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
					if (_graphics != null)
					{
						_graphics.Dispose();
					}

					if (_splash != null)
					{
						_splash.Dispose();
					}

					if (_log != null)
					{
						_log.Print("Gorgon Editor shutting down.", LoggingLevel.All);
						_log.Close();
					}
				}

				_splash = null;
				_graphics = null;
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
				_splash = _splashProxy.Item;

				_splash.Show();

				// Fade in our splash screen.
				_splash.Fade(true, 500.0f);

				// Create our graphics interface.
				_graphics = _graphicsService.GetGraphics();

				// Retrieve our plug-ins for the application.
				_plugInFactory.ScanAndLoadPlugIns();

				// Create our scratch area.
				if (!InitializeScratchArea())
				{
					Gorgon.Quit();
					return;
				}

				// Initialize our file system service.
				InitializeFileSystemService();

				// TODO: Make this smarter.
				using (var stream = File.Open(_settings.ThemeDirectory.FormatDirectory(Path.DirectorySeparatorChar) + "Darktheme.Xml", FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var serializer = new XmlSerializer(typeof(EditorTheme));
					((FlatForm)MainForm).Theme = (EditorTheme)serializer.Deserialize(stream);
				}

				// Set up the default pane.
				_splash.InfoText = Resources.GOREDIT_TEXT_STARTING;

				// Keep showing the splash screen.
				while ((GorgonTiming.SecondsSinceStart - startTime) < 3)
				{
					Thread.Sleep(1);
				}

				_splash.Fade(false, 250.0f);

				// Get rid of the splash screen.
				_splash.Dispose();
				_splash = null;

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
				// Unload the current file if one exists.
				_scratchService.ScratchArea.CleanUp();

				if (_splash != null)
				{
					_splash.Dispose();
					_splash = null;
				}
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
		/// <param name="graphicsService">The service used to create a new graphics interface.</param>
		/// <param name="splashProxy">The factory to create forms for the application.</param>
		/// <param name="plugInFactory">The factory to load plug-ins for the application.</param>
		/// <param name="scratchService">The service pertaining to scratch area manipulation.</param>
		/// <param name="fileSystemService">The service that handles packed file systems.</param>
		public AppContext(GorgonLogFile log,
			FormMain mainForm, 
			IEditorSettings settings, 
			IGraphicsService graphicsService, 
			IProxyObject<FormSplash> splashProxy, 
			IPlugInRegistry plugInFactory,
			IScratchService scratchService,
			IFileSystemService fileSystemService)
		{
			_graphicsService = graphicsService;
			_settings = settings;
			_splashProxy = splashProxy;
			_log = log;
			_plugInFactory = plugInFactory;
			_scratchService = scratchService;
			_fileSystemService = fileSystemService;
			MainForm = mainForm;
		}
		#endregion
	}
}
