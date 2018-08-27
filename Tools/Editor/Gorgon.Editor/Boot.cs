#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: August 26, 2018 5:37:32 PM
// 
#endregion

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Editor.ViewModels;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.UI;
using Newtonsoft.Json;
using DX = SharpDX;
using Exception = System.Exception;

namespace Gorgon.Editor
{
    /// <summary>
    /// Bootstrap functionality for the application.
    /// </summary>
    internal class Boot
        : ApplicationContext
    {
        #region Variables.
        // Splash screen.
        private FormSplash _splash;
        // The main application form.
        private FormMain _mainForm;
        // Our context for rendering with Gorgon.
        private GraphicsContext _graphicsContext;
        // The application settings.
        private EditorSettings _settings;
        // The project manager for the application.
        private ProjectManager _projectManager;
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.ApplicationContext" /> and optionally releases the managed resources.</summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources. </param>
        protected override void Dispose(bool disposing)
        {
            GraphicsContext context = Interlocked.Exchange(ref _graphicsContext, null);
            FormMain mainForm = Interlocked.Exchange(ref _mainForm, null);
            FormSplash splash = Interlocked.Exchange(ref _splash, null);

            context?.Dispose();
            mainForm?.Dispose();
            splash?.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Function to show the splash screen for our application boot up procedure.
        /// </summary>
        private async Task ShowSplashAsync()
        {
            _splash = new FormSplash();
            _splash.Show();
            await _splash.FadeAsync(true, 16);
        }

        /// <summary>
        /// Function to hide the splash screen.
        /// </summary>
        private async Task HideSplashAsync()
        {
            if (_splash == null)
            {
                return;
            }

            await _splash.FadeAsync(false, 16);

            _splash.Dispose();
            _splash = null;
        }

        /// <summary>
        /// Function to load the application settings.
        /// </summary>
        /// <returns>The settings for the application.</returns>
        private EditorSettings LoadSettings()
        {
#if DEBUG
            var settingsFile = new FileInfo(Path.Combine(Program.ApplicationUserDirectory.FullName, "Gorgon.Editor.Settings.DEBUG.json"));
#else
            var settingsFile = new FileInfo(Path.Combine(Program.ApplicationUserDirectory.FullName, "Gorgon.Editor.Settings.json"));
#endif
            EditorSettings result = null;

            _splash.InfoText = "Loading application settings...";

            var defaultSize = new Size(1280.Min(Screen.PrimaryScreen.WorkingArea.Width), 800.Min(Screen.PrimaryScreen.WorkingArea.Height));
            var defaultLocation = new Point((Screen.PrimaryScreen.WorkingArea.Width / 2) - (defaultSize.Width / 2) + Screen.PrimaryScreen.WorkingArea.X,
                                              (Screen.PrimaryScreen.WorkingArea.Height / 2) - (defaultSize.Height / 2) + Screen.PrimaryScreen.WorkingArea.Y);
            
            EditorSettings CreateEditorSettings()
            {
                return new EditorSettings
                       {
                           WindowBounds = new DX.Rectangle(defaultLocation.X,
                                                           defaultLocation.Y,
                                                           defaultSize.Width,
                                                           defaultSize.Height),
                           WindowState = FormWindowState.Maximized
                       };
            }

            if (!settingsFile.Exists)
            {
                Program.Log.Print($"Settings file '{settingsFile.FullName}' does not exist. Using new settings.", LoggingLevel.Intermediate);
            }
            else
            {
                StreamReader reader = null;

                try
                {
                    Program.Log.Print($"Loading application settings from '{settingsFile.FullName}'", LoggingLevel.Intermediate);
                    reader = new StreamReader(settingsFile.FullName, Encoding.UTF8, true);
                    result = JsonConvert.DeserializeObject<EditorSettings>(reader.ReadToEnd());
                    Program.Log.Print("Application settings loaded.", LoggingLevel.Intermediate);
                }
                catch (IOException ioex)
                {
                    Program.Log.Print($"Failed to load settings from '{settingsFile.FullName}'. Using fresh settings.", LoggingLevel.Intermediate);
                    Program.Log.LogException(ioex);
                    result = CreateEditorSettings();
                }
                finally
                {
                    reader?.Dispose();
                }
            }

            if (result == null)
            {
                result = CreateEditorSettings();
            }

            // If we're not on one of the screens, then default to the main screen.
            if (result.WindowBounds != null)
            {
                var rect = new Rectangle(result.WindowBounds.Value.X,
                                         result.WindowBounds.Value.Y,
                                         result.WindowBounds.Value.Width,
                                         result.WindowBounds.Value.Height);
#pragma warning disable IDE0007 // Use implicit type (how the hell is this explicit Microsoft??  For all I know, this could be returning an IntPtr to a monitor handle!)
                Screen onScreen = Screen.FromRectangle(rect);
#pragma warning restore IDE0007 // Use implicit type

                // If we detected that we're on the primary screen (meaning we aren't on any of the others), but we don't intersect with the working area,
                // then we need to reset.
                // Shrink the target rect so that we can ensure won't just get a sliver of the window.
                rect.Inflate(-50, -50);
                if ((onScreen == Screen.PrimaryScreen) && (!onScreen.WorkingArea.IntersectsWith(rect)))
                {
                    result.WindowBounds = new DX.Rectangle(defaultLocation.X,
                                                           defaultLocation.Y,
                                                           defaultSize.Width,
                                                           defaultSize.Height);
                }
            }
            else
            {
                result.WindowBounds = new DX.Rectangle(defaultLocation.X,
                                                       defaultLocation.Y,
                                                       defaultSize.Width,
                                                       defaultSize.Height);
            }

            return result;
        }

        /// <summary>
        /// Function to persist the settings back to the file system.
        /// </summary>
        private void PersistSettings()
        {
            StreamWriter writer = null;
#if DEBUG
            var settingsFile = new FileInfo(Path.Combine(Program.ApplicationUserDirectory.FullName, $"Gorgon.Editor.Settings.DEBUG.json"));
#else
            var settingsFile = new FileInfo(Path.Combine(Program.ApplicationUserDirectory.FullName, $"Gorgon.Editor.Settings.json"));
#endif

            try
            {
                // Do not capture the window state if we're minimized, that way lies madness.
                if (_mainForm.WindowState != FormWindowState.Minimized)
                {
                    if (_mainForm.WindowState != FormWindowState.Maximized)
                    {
                        _settings.WindowBounds = new DX.Rectangle(_mainForm.Location.X,
                                                                  _mainForm.Location.Y,
                                                                  _mainForm.Size.Width,
                                                                  _mainForm.Size.Height);
                    }
                    else
                    {
                        _settings.WindowBounds = new DX.Rectangle(_mainForm.RestoreBounds.X,
                                                                  _mainForm.RestoreBounds.Y,
                                                                  _mainForm.RestoreBounds.Width,
                                                                  _mainForm.RestoreBounds.Height);
                    }

                    _settings.WindowState = _mainForm.WindowState;
                }

                writer = new StreamWriter(settingsFile.FullName, false, Encoding.UTF8);
                writer.Write(JsonConvert.SerializeObject(_settings));
            }
            catch (Exception ex)
            {
                Debug.Print($"Error saving settings.\n{ex.Message}");
            }
            finally
            {
                writer?.Dispose();
            }
        }

        /// <summary>
        /// Function to perform the boot strapping operation.
        /// </summary>
        /// <returns>The main application window.</returns>
        public async void BootStrap()
        {
            try
            {
                // Get our initial context.
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

                Program.Log.Print("Booting application...", LoggingLevel.All);

                Cursor.Current = Cursors.WaitCursor;

                await ShowSplashAsync();

                _graphicsContext = GraphicsContext.Create(Program.Log);

                // Get any application settings we might have.
                _settings = LoadSettings();

                // Create the project manager for the application
                _projectManager = new ProjectManager();

                Debug.Assert(_settings.WindowBounds != null, "Window bounds should not be null.");
                
                _mainForm = new FormMain
                            {
                                Location = new Point(_settings.WindowBounds.Value.X, _settings.WindowBounds.Value.Y),
                                Size = new Size(_settings.WindowBounds.Value.Width, _settings.WindowBounds.Value.Height),
                                WindowState = FormWindowState.Normal
                            };
                Program.Log.Print("Applying theme to main window...", LoggingLevel.Verbose);
                _mainForm.LoadTheme();
                _mainForm.FormClosing += (sender, args) => PersistSettings();

                await HideSplashAsync();

                MainForm = _mainForm;

                var factory = new ViewModelFactory(_settings,
                                                   _projectManager,
                                                   new MessageBoxService(),
                                                   new WaitCursorBusyState());

                _mainForm.SetDataContext(factory.CreateMainViewModel());
                _mainForm.Show();
                _mainForm.WindowState = _settings.WindowState;
                
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        #endregion
    }
}
