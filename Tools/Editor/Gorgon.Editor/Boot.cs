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
using Gorgon.Plugins;
using Gorgon.Diagnostics;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Editor.ViewModels;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.UI;
using Newtonsoft.Json;
using DX = SharpDX;
using Exception = System.Exception;
using Gorgon.IO.Providers;
using System.Collections.Generic;
using Gorgon.Editor.Plugins;
using System.Linq;
using Gorgon.Editor.Converters;

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
        // The cache for plugin assemblies.
        private GorgonMefPluginCache _pluginCache;
        // The plugin service used to manage content plugins.
        private ContentPluginService _contentPlugins;
		// The plugin service used to manage tool plugin.
        private ToolPluginService _toolPlugins;
        // The plugin service used to manage content import plugins.
        private ContentImporterPluginService _contentImporterPlugins;
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.ApplicationContext" /> and optionally releases the managed resources.</summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources. </param>
        protected override void Dispose(bool disposing)
        {
            ToolPluginService toolPlugins = Interlocked.Exchange(ref _toolPlugins, null);
            ContentPluginService contentPlugins = Interlocked.Exchange(ref _contentPlugins, null);
            ContentImporterPluginService contentImporterPlugins = Interlocked.Exchange(ref _contentImporterPlugins, null);
            GraphicsContext context = Interlocked.Exchange(ref _graphicsContext, null);
            GorgonMefPluginCache pluginCache = Interlocked.Exchange(ref _pluginCache, null);
            FormMain mainForm = Interlocked.Exchange(ref _mainForm, null);
            FormSplash splash = Interlocked.Exchange(ref _splash, null);

            toolPlugins?.Dispose();
            contentImporterPlugins?.Dispose();
            contentPlugins?.Dispose();
            context?.Dispose();
            pluginCache?.Dispose();
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
                    WindowState = (int)FormWindowState.Maximized,
                    PluginPath = Path.Combine(GorgonApplication.StartupPath.FullName, "Plugins").FormatDirectory(Path.DirectorySeparatorChar),
                    LastOpenSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).FormatDirectory(Path.DirectorySeparatorChar),
                    LastProjectWorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).FormatDirectory(Path.DirectorySeparatorChar)
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
                    result = JsonConvert.DeserializeObject<EditorSettings>(reader.ReadToEnd(), new JsonSharpDxRectConverter());
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

            if (string.IsNullOrWhiteSpace(result.PluginPath))
            {
                result.PluginPath = Path.Combine(GorgonApplication.StartupPath.FullName, "Plugins").FormatDirectory(Path.DirectorySeparatorChar);
            }

            var pluginPath = new DirectoryInfo(result.PluginPath);

            if (!pluginPath.Exists)
            {
                pluginPath.Create();
            }

            if (string.IsNullOrWhiteSpace(result.LastOpenSavePath))
            {
                result.LastOpenSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).FormatDirectory(Path.DirectorySeparatorChar);
            }

            var lastOpenSavePath = new DirectoryInfo(result.LastOpenSavePath);

            if (!lastOpenSavePath.Exists)
            {
                result.LastOpenSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).FormatDirectory(Path.DirectorySeparatorChar);
            }

            if ((string.IsNullOrWhiteSpace(result.LastProjectWorkingDirectory)) || (!Directory.Exists(result.LastProjectWorkingDirectory)))
            {
                result.LastProjectWorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).FormatDirectory(Path.DirectorySeparatorChar);
            }            

            // If we're not on one of the screens, then default to the main screen.
            if (result.WindowBounds != null)
            {
                var rect = new Rectangle(result.WindowBounds.Value.X,
                                         result.WindowBounds.Value.Y,
                                         result.WindowBounds.Value.Width,
                                         result.WindowBounds.Value.Height);
                var onScreen = Screen.FromRectangle(rect);

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
        /// Function to load any import content plugins used to create/edit content.
        /// </summary>
        /// <returns>The content plugin manager service used to manipulate the loaded content plugins.</returns>
        private ContentImporterPluginService LoadImportContentPlugins()
        {
            var contentPluginsDir = new DirectoryInfo(Path.Combine(_settings.PluginPath, "ContentImporters"));
            var contentPluginSettingsDir = new DirectoryInfo(Path.Combine(Program.ApplicationUserDirectory.FullName, "ContentImporterPlugins"));

            if (!contentPluginSettingsDir.Exists)
            {
                contentPluginSettingsDir.Create();
                contentPluginSettingsDir.Refresh();
            }

            var contentPlugins = new ContentImporterPluginService(contentPluginSettingsDir, _graphicsContext);

            try
            {
                _splash.InfoText = Resources.GOREDIT_TEXT_LOADING_IMPORT_CONTENT_PLUGINS;

                if (!contentPluginsDir.Exists)
                {
                    contentPluginsDir.Create();
                    contentPluginsDir.Refresh();
                }

                contentPlugins.LoadContentImporterPlugins(_pluginCache, contentPluginsDir);
            }
            catch (Exception ex)
            {
                Program.Log.LogException(ex);
                GorgonDialogs.ErrorBox(_splash, Resources.GOREDIT_ERR_LOADING_PLUGINS, Resources.GOREDIT_ERR_ERROR, ex);
            }

            return contentPlugins;
        }

        /// <summary>
        /// Function to load any tool plugins.
        /// </summary>
        /// <returns>The tool plugin manager service used to manipulate the loaded tool plugins.</returns>
        private ToolPluginService LoadToolPlugins()
        {
            var toolPluginsDir = new DirectoryInfo(Path.Combine(_settings.PluginPath, "Tools"));
            var toolPlugins = new ToolPluginService(_graphicsContext);

            try
            {
                _splash.InfoText = Resources.GOREDIT_TEXT_LOADING_TOOL_PLUGINS;

                if (!toolPluginsDir.Exists)
                {
                    toolPluginsDir.Create();
                    return toolPlugins;
                }

                toolPlugins.LoadToolPlugins(_pluginCache, toolPluginsDir);
            }
            catch (Exception ex)
            {
                Program.Log.LogException(ex);
                GorgonDialogs.ErrorBox(_splash, Resources.GOREDIT_ERR_LOADING_PLUGINS, Resources.GOREDIT_ERR_ERROR, ex);
            }

            return toolPlugins;
        }

        /// <summary>
        /// Function to load any content plugins used to create/edit content.
        /// </summary>
        /// <returns>The content plugin manager service used to manipulate the loaded content plugins.</returns>
        private ContentPluginService LoadContentPlugins()
        {
            var contentPluginsDir = new DirectoryInfo(Path.Combine(_settings.PluginPath, "Content"));
            var contentPluginSettingsDir = new DirectoryInfo(Path.Combine(Program.ApplicationUserDirectory.FullName, "ContentPlugins"));

            if (!contentPluginSettingsDir.Exists)
            {
                contentPluginSettingsDir.Create();
                contentPluginSettingsDir.Refresh();
            }

            var contentPlugins = new ContentPluginService(contentPluginSettingsDir, _graphicsContext);

            try
            {
                _splash.InfoText = Resources.GOREDIT_TEXT_LOADING_CONTENT_PLUGINS;

                if (!contentPluginsDir.Exists)
                {
                    contentPluginsDir.Create();
                    return contentPlugins;
                }

                contentPlugins.LoadContentPlugins(_pluginCache, contentPluginsDir);
            }
            catch (Exception ex)
            {
                Program.Log.LogException(ex);
                GorgonDialogs.ErrorBox(_splash, Resources.GOREDIT_ERR_LOADING_PLUGINS, Resources.GOREDIT_ERR_ERROR, ex);
            }

            return contentPlugins;
        }

        /// <summary>
        /// Function to load any plugins used to import or export files.
        /// </summary>
        /// <returns>A file system provider management interface.</returns>
        private FileSystemProviders LoadFileSystemPlugins()
        {
            var fileSystemPlugInsDir = new DirectoryInfo(Path.Combine(_settings.PluginPath, "Filesystem"));
            var result = new FileSystemProviders();

            try
            {
                _splash.InfoText = Resources.GOREDIT_TEXT_LOADING_FILESYSTEM_PLUGINS;

                // If the directory does not exist, then we have no plugins to load.
                if (!fileSystemPlugInsDir.Exists)
                {
                    fileSystemPlugInsDir.Create();
                    return result;
                }

                result.LoadProviders(_pluginCache, fileSystemPlugInsDir);

                return result;
            }
            catch (Exception ex)
            {
                Program.Log.LogException(ex);
                GorgonDialogs.ErrorBox(_splash, Resources.GOREDIT_ERR_LOADING_PLUGINS, Resources.GOREDIT_ERR_ERROR, ex);
            }

            return result;
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

                // Initalize the common resources.
                EditorCommonResources.LoadResources();

                _pluginCache = new GorgonMefPluginCache(Program.Log);
                _graphicsContext = GraphicsContext.Create(Program.Log);

                // Get any application settings we might have.
                _settings = LoadSettings();

                // Load our file system import/export plugins.
                FileSystemProviders fileSystemProviders = LoadFileSystemPlugins();

				// Load our tool plug ins.
                _toolPlugins = LoadToolPlugins();

                // Load our content service plugins.
                _contentPlugins = LoadContentPlugins();
                _contentImporterPlugins = LoadImportContentPlugins();                

                // Create the project manager for the application
                _projectManager = new ProjectManager(fileSystemProviders);
                
                _mainForm = new FormMain
                            {
                                Location = new Point(_settings.WindowBounds.Value.X, _settings.WindowBounds.Value.Y),
                                Size = new Size(_settings.WindowBounds.Value.Width, _settings.WindowBounds.Value.Height),
                                WindowState = FormWindowState.Normal
                            };

                await HideSplashAsync();

                MainForm = _mainForm;

                var factory = new ViewModelFactory(_settings,
                                                   fileSystemProviders,
                                                   _contentPlugins,
												   _toolPlugins,
                                                   _contentImporterPlugins,
                                                   _projectManager,
                                                   new MessageBoxService(),
                                                   new WaitCursorBusyState(),
                                                   new ClipboardService(),
                                                   new DirectoryLocateService(),
                                                   new FileScanService(_contentPlugins));

                FormWindowState windowState;
                // Ensure the window state values fall into an acceptable range.
                if (!Enum.IsDefined(typeof(FormWindowState), _settings.WindowState))
                {
                    windowState = FormWindowState.Maximized;
                }
                else
                {
                    windowState = (FormWindowState)_settings.WindowState;
                }

                _mainForm.GraphicsContext = _graphicsContext;
                _mainForm.SetDataContext(factory.CreateMainViewModel(_graphicsContext.Graphics.VideoAdapter.Name));
                _mainForm.Show();
                _mainForm.WindowState = windowState;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        #endregion
    }
}
