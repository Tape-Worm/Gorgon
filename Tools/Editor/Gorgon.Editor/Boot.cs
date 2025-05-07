
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 26, 2018 5:37:32 PM
// 

using System.Reflection;
using System.Text;
using System.Text.Json;
using Gorgon.Diagnostics;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Editor.ViewModels;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Plugins;
using Gorgon.UI.OLDE;

namespace Gorgon.Editor;

/// <summary>
/// Bootstrap functionality for the application
/// </summary>
internal class Boot
    : ApplicationContext
{

    // Splash screen.
    private FormSplash _splash;
    // The main application form.
    private FormMain _mainForm;
    // Our context for rendering with Gorgon.
    private GraphicsContext _graphicsContext;
    // The cache for Plugin assemblies.
    private GorgonMefPluginCache _pluginCache;
    // The service for managing tool plugins.
    private ToolPluginService _toolPlugins;
    // The service for managing content plugins.
    private ContentPluginService _contentPlugins;

    /// <summary>Handles the AssemblyResolve event of the CurrentDomain control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The <see cref="ResolveEventArgs"/> instance containing the event data.</param>
    /// <returns>The referenced assembly, if found.</returns>
    private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        // If we don't know who wants the assembly, then we can't load it.
        if (args.RequestingAssembly is null)
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
        [
            args.RequestingAssembly.Location,
            GorgonApplication.StartupPath.FullName
        ];

        // Step 2. - We did not locate the assembly in the loaded assembly list.  Check the local directory for the assembly requesting the reference.
        AssemblyName name = new(args.Name);

        foreach (string path in paths)
        {
            // This can happen if the requesting assembly is loaded using a byte array.
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            FileInfo file = new(Path.Combine(path, name.Name));

            if (file.Exists)
            {
                Assembly assembly = Assembly.LoadFile(file.FullName);
                AssemblyName loadedName = assembly.GetName();

                if (AssemblyName.ReferenceMatchesDefinition(loadedName, name))
                {
                    return assembly;
                }
            }
        }

        return null;
    }

    /// <summary>Releases the unmanaged resources used by the <see cref="ApplicationContext" /> and optionally releases the managed resources.</summary>
    /// <param name="disposing">
    /// <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources. </param>
    protected override void Dispose(bool disposing)
    {
        AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;

        ToolPluginService toolPlugins = Interlocked.Exchange(ref _toolPlugins, null);
        ContentPluginService contentPlugins = Interlocked.Exchange(ref _contentPlugins, null);
        GraphicsContext context = Interlocked.Exchange(ref _graphicsContext, null);
        GorgonMefPluginCache PluginCache = Interlocked.Exchange(ref _pluginCache, null);
        FormMain mainForm = Interlocked.Exchange(ref _mainForm, null);
        FormSplash splash = Interlocked.Exchange(ref _splash, null);

        toolPlugins?.Dispose();
        contentPlugins?.Dispose();
        context?.Dispose();
        PluginCache?.Dispose();
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
        if (_splash is null)
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
        FileInfo settingsFile = new(Path.Combine(Program.ApplicationUserDirectory.FullName, "Gorgon.Editor.Settings.DEBUG.json"));
#else
        var settingsFile = new FileInfo(Path.Combine(Program.ApplicationUserDirectory.FullName, "Gorgon.Editor.Settings.json"));
#endif
        EditorSettings result = null;

        _splash.InfoText = "Loading application settings...";

        Size defaultSize = new(1280.Min(Screen.PrimaryScreen.WorkingArea.Width), 800.Min(Screen.PrimaryScreen.WorkingArea.Height));
        Point defaultLocation = new((Screen.PrimaryScreen.WorkingArea.Width / 2) - (defaultSize.Width / 2) + Screen.PrimaryScreen.WorkingArea.X,
                                          (Screen.PrimaryScreen.WorkingArea.Height / 2) - (defaultSize.Height / 2) + Screen.PrimaryScreen.WorkingArea.Y);

        EditorSettings CreateEditorSettings() => new()
        {
            WindowBounds = new GorgonRectangle(defaultLocation.X,
                                               defaultLocation.Y,
                                               defaultSize.Width,
                                               defaultSize.Height),
            WindowState = (int)FormWindowState.Maximized,
            LastOpenSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).FormatDirectory(Path.DirectorySeparatorChar),
            LastProjectWorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).FormatDirectory(Path.DirectorySeparatorChar)
        };

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
                string json = reader.ReadToEnd();
                result = JsonSerializer.Deserialize<EditorSettings>(json);
                Program.Log.Print("Application settings loaded.", LoggingLevel.Intermediate);
            }
            catch (JsonException jEx)
            {
                Program.Log.Print($"Failed to load settings from '{settingsFile.FullName}'. Using fresh settings.", LoggingLevel.Intermediate);
                Program.Log.PrintException(jEx);
                result = CreateEditorSettings();
            }
            catch (IOException ioex)
            {
                Program.Log.Print($"Failed to load settings from '{settingsFile.FullName}'. Using fresh settings.", LoggingLevel.Intermediate);
                Program.Log.PrintException(ioex);
                result = CreateEditorSettings();
            }
            finally
            {
                reader?.Dispose();
            }
        }

        result ??= CreateEditorSettings();

        if (string.IsNullOrWhiteSpace(result.LastOpenSavePath))
        {
            result.LastOpenSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).FormatDirectory(Path.DirectorySeparatorChar);
        }

        DirectoryInfo lastOpenSavePath = new(result.LastOpenSavePath);

        if (!lastOpenSavePath.Exists)
        {
            result.LastOpenSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).FormatDirectory(Path.DirectorySeparatorChar);
        }

        if ((string.IsNullOrWhiteSpace(result.LastProjectWorkingDirectory)) || (!System.IO.Directory.Exists(result.LastProjectWorkingDirectory)))
        {
            result.LastProjectWorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).FormatDirectory(Path.DirectorySeparatorChar);
        }

        // If we're not on one of the screens, then default to the main screen.
        if (result.WindowBounds is not null)
        {
            Rectangle rect = new(result.WindowBounds.Value.X,
                                     result.WindowBounds.Value.Y,
                                     result.WindowBounds.Value.Width,
                                     result.WindowBounds.Value.Height);
            Screen onScreen = Screen.FromRectangle(rect);

            // If we detected that we're on the primary screen (meaning we aren't on any of the others), but we don't intersect with the working area,
            // then we need to reset.
            // Shrink the target rect so that we can ensure won't just get a sliver of the window.
            rect.Inflate(-50, -50);
            if ((onScreen == Screen.PrimaryScreen) && (!onScreen.WorkingArea.IntersectsWith(rect)))
            {
                result.WindowBounds = new GorgonRectangle(defaultLocation.X,
                                                       defaultLocation.Y,
                                                       defaultSize.Width,
                                                       defaultSize.Height);
            }
        }
        else
        {
            result.WindowBounds = new GorgonRectangle(defaultLocation.X,
                                                   defaultLocation.Y,
                                                   defaultSize.Width,
                                                   defaultSize.Height);
        }

        return result;
    }

    /// <summary>
    /// Function to load any tool Plugins.
    /// </summary>
    /// <param name="pluginDir">The directory containing the plugins.</param>
    /// <param name="hostServices">The services to pass to the tool plugins.</param>
    private void LoadToolPlugins(DirectoryInfo pluginDir, HostContentServices hostServices)
    {
        string toolPluginsDir = Path.Combine(pluginDir.FullName, "Tools");
        string toolPluginSettingsDir = Path.Combine(Program.ApplicationUserDirectory.FullName, "ToolPlugins");
        _toolPlugins = new ToolPluginService(toolPluginSettingsDir, hostServices);

        hostServices.ToolPluginService = _toolPlugins;

        if (!System.IO.Directory.Exists(toolPluginsDir))
        {
            return;
        }

        if (!System.IO.Directory.Exists(toolPluginSettingsDir))
        {
            System.IO.Directory.CreateDirectory(toolPluginSettingsDir);
        }

        try
        {
            _splash.InfoText = Resources.GOREDIT_TEXT_LOADING_TOOL_pluginS;
            _toolPlugins.LoadToolPlugins(_pluginCache, toolPluginsDir);
        }
        catch (Exception ex)
        {
            Program.Log.PrintException(ex);
            GorgonDialogs.ErrorBox(_splash, Resources.GOREDIT_ERR_LOADING_pluginS, Resources.GOREDIT_ERR_ERROR, ex);
        }
    }

    /// <summary>
    /// Function to load any content Plugins used to create/edit content.
    /// </summary>
    /// <param name="pluginDir">The directory containing the plugins.</param>
    /// <param name="hostServices">The services to pass to the content plugins.</param>
    private void LoadContentPlugins(DirectoryInfo pluginDir, HostContentServices hostServices)
    {
        string contentPluginsDir = Path.Combine(pluginDir.FullName, "Content");
        string contentPluginSettingsDir = Path.Combine(Program.ApplicationUserDirectory.FullName, "ContentPlugins");
        _contentPlugins = new ContentPluginService(contentPluginSettingsDir, hostServices);

        hostServices.ContentPluginService = _contentPlugins;

        if (!System.IO.Directory.Exists(contentPluginsDir))
        {
            return;
        }

        if (!System.IO.Directory.Exists(contentPluginSettingsDir))
        {
            System.IO.Directory.CreateDirectory(contentPluginSettingsDir);
        }

        try
        {
            _splash.InfoText = Resources.GOREDIT_TEXT_LOADING_CONTENT_pluginS;
            _contentPlugins.LoadContentPlugins(_pluginCache, contentPluginsDir);
        }
        catch (Exception ex)
        {
            Program.Log.PrintException(ex);
            GorgonDialogs.ErrorBox(_splash, Resources.GOREDIT_ERR_LOADING_pluginS, Resources.GOREDIT_ERR_ERROR, ex);
        }
    }

    /// <summary>
    /// Function to load any Plugins used to import or export files.
    /// </summary>
    /// <param name="pluginDir">The directory containing the plugins.</param>
    /// <param name="hostServices">The services to pass to the file system plugins.</param>
    private FileSystemProviders LoadFileSystemPlugins(DirectoryInfo pluginDir, IHostServices hostServices)
    {
        string fileSystemPluginsDir = Path.Combine(pluginDir.FullName, "Filesystem");
        FileSystemProviders result = new(hostServices);

        if (!System.IO.Directory.Exists(fileSystemPluginsDir))
        {
            return result;
        }

        try
        {
            _splash.InfoText = Resources.GOREDIT_TEXT_LOADING_FILESYSTEM_pluginS;
            result.LoadProviders(_pluginCache, fileSystemPluginsDir);

            return result;
        }
        catch (Exception ex)
        {
            Program.Log.PrintException(ex);
            GorgonDialogs.ErrorBox(_splash, Resources.GOREDIT_ERR_LOADING_pluginS, Resources.GOREDIT_ERR_ERROR, ex);
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

            HostContentServices hostServices = new()
            {
                Log = Program.Log
            };

            _pluginCache = new GorgonMefPluginCache(Program.Log);
            _graphicsContext = GraphicsContext.Create(Program.Log);

            // Get any application settings we might have.
            EditorSettings settings = LoadSettings();

            // Set up the host services that we will pass to our plugins.
            hostServices.BusyService = new WaitCursorBusyState();
            hostServices.MessageDisplay = new MessageBoxService(Program.Log);
            hostServices.ClipboardService = new ClipboardService();
            hostServices.ColorPicker = new ColorPickerService();
            hostServices.GraphicsContext = _graphicsContext;

            DirectoryInfo PluginLocation = new(Path.Combine(GorgonApplication.StartupPath.FullName, "Plugins"));

            if (!PluginLocation.Exists)
            {
                Program.Log.PrintError($"Plug in path '{PluginLocation.FullName}' was not found.  No plugins will be loaded.", LoggingLevel.Simple);
                GorgonDialogs.ErrorBox(null, Resources.GOREDIT_ERR_LOADING_pluginS);
            }

            // Load our file system import/export Plugins.
            FileSystemProviders fileSystemProviders = LoadFileSystemPlugins(PluginLocation, hostServices);

            // Load our tool plugins.
            LoadToolPlugins(PluginLocation, hostServices);

            // Load our content service Plugins.
            LoadContentPlugins(PluginLocation, hostServices);

            // Create the project manager for the application
            ProjectManager projectManager = new(fileSystemProviders, Program.Log);

            // Setup the factory used to build view models for the application.
            ViewModelFactory factory = new(settings, projectManager, fileSystemProviders, hostServices);

            // Show our main interface.
            _mainForm = new FormMain(settings);

            IMain mainViewModel = factory.CreateMainViewModel(_graphicsContext.Graphics.VideoAdapter.Name);
            hostServices.FolderBrowser = new FileSystemFolderBrowseService(mainViewModel);

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

            _mainForm.SetDataContext(mainViewModel);

            _mainForm.Location = new Point(settings.WindowBounds.Value.X, settings.WindowBounds.Value.Y);
            _mainForm.Size = new Size(settings.WindowBounds.Value.Width, settings.WindowBounds.Value.Height);
            _mainForm.WindowState = FormWindowState.Normal;
            _mainForm.GraphicsContext = _graphicsContext;

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
}
