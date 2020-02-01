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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.Editor.Converters;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Editor.ViewModels;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.PlugIns;
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
        // The cache for plugin assemblies.
        private GorgonMefPlugInCache _pluginCache;
        // The service for managing tool plug ins.
        private ToolPlugInService _toolPlugIns;
        // The service for managing content plug ins.
        private ContentPlugInService _contentPlugIns;
        #endregion

        #region Methods.
        /// <summary>Handles the AssemblyResolve event of the CurrentDomain control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ResolveEventArgs"/> instance containing the event data.</param>
        /// <returns>The referenced assembly, if found.</returns>
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // If we don't know who wants the assembly, then we can't load it.
            if (args.RequestingAssembly == null)
            {
                return null;
            }

            // Step 1. - Find all currently loaded assemblies.
            Assembly[] loaded = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in loaded)
            {
                if (string.Equals(args.Name, assembly.FullName, StringComparison.InvariantCulture))
                {
                    return assembly;
                }
            }

            string[] paths =
            {
                args.RequestingAssembly.Location,
                GorgonApplication.StartupPath.FullName
            };

            // Step 2. - We did not locate the assembly in the loaded assembly list.  Check the local directory for the assembly requesting the reference.
            var name = new AssemblyName(args.Name);

            foreach (string path in paths)
            {
                // This can happen if the requesting assembly is loaded using a byte array.
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                var file = new FileInfo(Path.Combine(path, name.Name));

                if (file.Exists)
                {
                    var assembly = Assembly.LoadFile(file.FullName);
                    AssemblyName loadedName = assembly.GetName();

                    if (AssemblyName.ReferenceMatchesDefinition(loadedName, name))
                    {
                        return assembly;
                    }
                }
            }

            return null;
        }

        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.ApplicationContext" /> and optionally releases the managed resources.</summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources. </param>
        protected override void Dispose(bool disposing)
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                        
            ToolPlugInService toolPlugIns = Interlocked.Exchange(ref _toolPlugIns, null);
            ContentPlugInService contentPlugIns = Interlocked.Exchange(ref _contentPlugIns, null);
            GraphicsContext context = Interlocked.Exchange(ref _graphicsContext, null);
            GorgonMefPlugInCache pluginCache = Interlocked.Exchange(ref _pluginCache, null);
            FormMain mainForm = Interlocked.Exchange(ref _mainForm, null);
            FormSplash splash = Interlocked.Exchange(ref _splash, null);

            toolPlugIns?.Dispose();
            contentPlugIns?.Dispose();
            context?.Dispose();
            pluginCache?.Dispose();
            mainForm?.Dispose();
            splash?.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Function to show the splash screen for our application boot up procedure.
        /// </summary>
        private void ShowSplash()
        {
            _splash = new FormSplash();
            _splash.Show();

            Application.DoEvents();
        }

        /// <summary>
        /// Function to hide the splash screen.
        /// </summary>
        private void HideSplash()
        {
            if (_splash == null)
            {
                return;
            }

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

            if (string.IsNullOrWhiteSpace(result.LastOpenSavePath))
            {
                result.LastOpenSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).FormatDirectory(Path.DirectorySeparatorChar);
            }

            var lastOpenSavePath = new DirectoryInfo(result.LastOpenSavePath);

            if (!lastOpenSavePath.Exists)
            {
                result.LastOpenSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).FormatDirectory(Path.DirectorySeparatorChar);
            }

            if ((string.IsNullOrWhiteSpace(result.LastProjectWorkingDirectory)) || (!System.IO.Directory.Exists(result.LastProjectWorkingDirectory)))
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
        /// Function to load any tool plugins.
        /// </summary>
        /// <param name="plugInDir">The directory containing the plug ins.</param>
        /// <param name="hostServices">The services to pass to the tool plug ins.</param>
        private void LoadToolPlugIns(DirectoryInfo plugInDir, HostContentServices hostServices)
        {
            string toolPlugInsDir = Path.Combine(plugInDir.FullName, "Tools");
            string toolPlugInSettingsDir = Path.Combine(Program.ApplicationUserDirectory.FullName, "ToolPlugIns");
            _toolPlugIns = new ToolPlugInService(toolPlugInSettingsDir, hostServices);
            
            hostServices.ToolPlugInService = _toolPlugIns;

            if (!System.IO.Directory.Exists(toolPlugInsDir))
            {
                return;
            }

            if (!System.IO.Directory.Exists(toolPlugInSettingsDir))
            {
                System.IO.Directory.CreateDirectory(toolPlugInSettingsDir);
            }

            try
            {
                _splash.InfoText = Resources.GOREDIT_TEXT_LOADING_TOOL_PLUGINS;
                _toolPlugIns.LoadToolPlugIns(_pluginCache, toolPlugInsDir);
            }
            catch (Exception ex)
            {
                Program.Log.LogException(ex);
                GorgonDialogs.ErrorBox(_splash, Resources.GOREDIT_ERR_LOADING_PLUGINS, Resources.GOREDIT_ERR_ERROR, ex);
            }            
        }

        /// <summary>
        /// Function to load any content plugins used to create/edit content.
        /// </summary>
        /// <param name="plugInDir">The directory containing the plug ins.</param>
        /// <param name="hostServices">The services to pass to the content plug ins.</param>
        private void LoadContentPlugIns(DirectoryInfo plugInDir, HostContentServices hostServices)
        {
            string contentPlugInsDir = Path.Combine(plugInDir.FullName, "Content");
            string contentPlugInSettingsDir = Path.Combine(Program.ApplicationUserDirectory.FullName, "ContentPlugIns");
            _contentPlugIns = new ContentPlugInService(contentPlugInSettingsDir, hostServices);

            hostServices.ContentPlugInService = _contentPlugIns;

            if (!System.IO.Directory.Exists(contentPlugInsDir))
            {
                return;
            }

            if (!System.IO.Directory.Exists(contentPlugInSettingsDir))
            {
                System.IO.Directory.CreateDirectory(contentPlugInSettingsDir);
            }

            try
            {                
                _splash.InfoText = Resources.GOREDIT_TEXT_LOADING_CONTENT_PLUGINS;
                _contentPlugIns.LoadContentPlugIns(_pluginCache, contentPlugInsDir);
            }
            catch (Exception ex)
            {
                Program.Log.LogException(ex);
                GorgonDialogs.ErrorBox(_splash, Resources.GOREDIT_ERR_LOADING_PLUGINS, Resources.GOREDIT_ERR_ERROR, ex);
            }
        }

        /// <summary>
        /// Function to load any plugins used to import or export files.
        /// </summary>
        /// <param name="plugInDir">The directory containing the plug ins.</param>
        /// <param name="hostServices">The services to pass to the file system plug ins.</param>
        private FileSystemProviders LoadFileSystemPlugIns(DirectoryInfo plugInDir, IHostServices hostServices)
        {
            string fileSystemPlugInsDir = Path.Combine(plugInDir.FullName, "Filesystem");
            var result = new FileSystemProviders(hostServices);

            if (!System.IO.Directory.Exists(fileSystemPlugInsDir))
            {
                return result;
            }

            try
            {
                _splash.InfoText = Resources.GOREDIT_TEXT_LOADING_FILESYSTEM_PLUGINS;
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
        public void BootStrap()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            try
            {
                Program.Log.Print("Booting application...", LoggingLevel.All);

                ShowSplash();
                Cursor.Current = Cursors.WaitCursor;

                // Initalize the common resources.
                CommonEditorResources.LoadResources();

                var hostServices = new HostContentServices
                {
                    Log = Program.Log
                };
                
                _pluginCache = new GorgonMefPlugInCache(Program.Log);
                _graphicsContext = GraphicsContext.Create(Program.Log);

                // Get any application settings we might have.
                EditorSettings settings = LoadSettings();

                // Set up the host services that we will pass to our plug ins.
                hostServices.FolderBrowser = new FileSystemFolderBrowseService();
                hostServices.BusyService = new WaitCursorBusyState();
                hostServices.MessageDisplay = new MessageBoxService();
                hostServices.ClipboardService = new ClipboardService();
                hostServices.GraphicsContext = _graphicsContext;

                var plugInLocation = new DirectoryInfo(Path.Combine(GorgonApplication.StartupPath.FullName, "PlugIns"));

                if (!plugInLocation.Exists)
                {
                    Program.Log.Print($"[ERROR] Plug in path '{plugInLocation.FullName}' was not found.  No plug ins will be loaded.", LoggingLevel.Simple);
                    GorgonDialogs.ErrorBox(null, Resources.GOREDIT_ERR_LOADING_PLUGINS);
                }

                // Load our file system import/export plugins.
                FileSystemProviders fileSystemProviders = LoadFileSystemPlugIns(plugInLocation, hostServices);
                
                // Load our tool plug ins.
                LoadToolPlugIns(plugInLocation, hostServices);

                // Load our content service plugins.
                LoadContentPlugIns(plugInLocation, hostServices);

                // Create the project manager for the application
                var projectManager = new ProjectManager(fileSystemProviders, Program.Log);

                // Setup the factory used to build view models for the application.
                var factory = new ViewModelFactory(settings, projectManager, fileSystemProviders, hostServices);
                
                // Show our main interface.
                _mainForm = new FormMain
                {
                    Location = new Point(settings.WindowBounds.Value.X, settings.WindowBounds.Value.Y),
                    Size = new Size(settings.WindowBounds.Value.Width, settings.WindowBounds.Value.Height),
                    WindowState = FormWindowState.Normal,
                    GraphicsContext = _graphicsContext
                };

                MainForm = _mainForm;

                FormWindowState windowState;
                // Ensure the window state values fall into an acceptable range.
                if (!Enum.IsDefined(typeof(FormWindowState), settings.WindowState))
                {
                    windowState = FormWindowState.Maximized;
                }
                else
                {
                    windowState = (FormWindowState)settings.WindowState;
                }

                _mainForm.SetDataContext(factory.CreateMainViewModel(_graphicsContext.Graphics.VideoAdapter.Name));

                HideSplash();

                ToolStripManager.Renderer = new DarkFormsRenderer();

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
