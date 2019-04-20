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
// Created: October 29, 2018 1:19:30 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Plugins;
using Newtonsoft.Json;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// The service used for managing the tool plugins.
    /// </summary>
    internal class ToolPluginService
        : IToolPluginManagerService, IDisposable
    {
        #region Variables.
        // The plugin list.
        private readonly Dictionary<string, ToolPlugin> _plugins = new Dictionary<string, ToolPlugin>(StringComparer.OrdinalIgnoreCase);
        // The list of disabled tool plug ins.
        private readonly Dictionary<string, IDisabledPlugin> _disabled = new Dictionary<string, IDisabledPlugin>(StringComparer.OrdinalIgnoreCase);
		// The list of ribbon buttons for all tools.
        private Dictionary<string, IReadOnlyList<IToolPluginRibbonButton>> _ribbonButtons = new Dictionary<string, IReadOnlyList<IToolPluginRibbonButton>>(StringComparer.CurrentCultureIgnoreCase);
        // The application graphics context for passing to tool plug ins.
        private readonly IGraphicsContext _graphicsContext;
        #endregion

        #region Properties.
        /// <summary>Property to return the list of tool plugins loaded in to the application.</summary>
        /// <value>The plugins.</value>
        public IReadOnlyDictionary<string, ToolPlugin> Plugins => _plugins;

        /// <summary>Property to return the list of disabled plug ins.</summary>
        public IReadOnlyDictionary<string, IDisabledPlugin> DisabledPlugins => _disabled;

        /// <summary>Property to return the list of ribbon buttons available</summary>
        public IReadOnlyDictionary<string, IReadOnlyList<IToolPluginRibbonButton>> RibbonButtons => _ribbonButtons;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to rebuild the list of sorted ribbon buttons.
        /// </summary>
        private void RebuildRibbonButtons()
        {
            var result = new Dictionary<string, IReadOnlyList<IToolPluginRibbonButton>>(StringComparer.CurrentCultureIgnoreCase);

            foreach (KeyValuePair<string, ToolPlugin> plugin in _plugins)
            {
                IToolPluginRibbonButton button = plugin.Value.Button;
                button.ValidateButton();

                List<IToolPluginRibbonButton> buttons;
                if (result.TryGetValue(button.GroupName, out IReadOnlyList<IToolPluginRibbonButton> roButtons))
                {
                    // This is safe because this is the implementation.
                    buttons = (List<IToolPluginRibbonButton>)roButtons;
                }
                else
                {
                    result[button.GroupName] = buttons = new List<IToolPluginRibbonButton>();
                }

                buttons.Add(button);
            }

            Clear();

            _ribbonButtons = result;
        }

        /// <summary>Function to add a tool plugin to the service.</summary>
        /// <param name="plugin">The plugin to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugin"/> parameter is <b>null</b>.</exception>
        public void AddToolPlugin(ToolPlugin plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            if (_plugins.ContainsKey(plugin.Name))
            {
                return;
            }

            _plugins[plugin.Name] = plugin;
            RebuildRibbonButtons();
        }

        /// <summary>Function to clear all of the tool plugins.</summary>
        public void Clear()
        {
            foreach (KeyValuePair<string, ToolPlugin> plugin in _plugins)
            {
                plugin.Value.Shutdown();
            }

            _plugins.Clear();

            foreach (KeyValuePair<string, IReadOnlyList<IToolPluginRibbonButton>> button in _ribbonButtons)
            {
                foreach (IDisposable disposer in button.Value.OfType<IDisposable>())
                {
                    disposer.Dispose();
                }
            }

            _ribbonButtons.Clear();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose() => Clear();

        /// <summary>
        /// Function to load all of the tool plug ins into the service.
        /// </summary>
        /// <param name="pluginCache">The plug in assembly cache.</param>
        /// <param name="pluginDir">The directory that contains the plug ins.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginCache"/>, or the <paramref name="pluginDir"/> parameter is <b>null</b>.</exception>
        public void LoadToolPlugins(GorgonMefPluginCache pluginCache, DirectoryInfo pluginDir)
        {
            if (pluginCache == null)
            {
                throw new ArgumentNullException(nameof(pluginCache));
            }

            if (pluginDir == null)
            {
                throw new ArgumentNullException(nameof(pluginDir));
            }

            IReadOnlyList<PluginRecord> assemblies = pluginCache.ValidateAndLoadAssemblies(pluginDir.GetFiles("*.dll"), Program.Log);

            if (assemblies.Count > 0)
            {
                foreach (PluginRecord record in assemblies.Where(item => !item.IsAssemblyLoaded && item.IsManaged))
                {
                    _disabled[Path.GetFileName(record.Path)] = new DisabledPlugin(DisabledReasonCode.Error, Path.GetFileName(record.Path), record.LoadFailureReason, record.Path);
                }
            }

            IGorgonPluginService plugins = new GorgonMefPluginService(pluginCache, Program.Log);            
            IReadOnlyList<ToolPlugin> pluginList = plugins.GetPlugins<ToolPlugin>();

            foreach (ToolPlugin plugin in pluginList)
            {
                try
                {
                    Program.Log.Print($"Creating tool plug in '{plugin.Name}'...", LoggingLevel.Simple);
                    plugin.Initialize(this, _graphicsContext, Program.Log);

                    // Check to see if this plug in can continue.
                    IReadOnlyList<string> validation = plugin.IsPluginAvailable();                    

                    if (validation.Count > 0)
                    {
                        // Shut the plug in down.
                        plugin.Shutdown();

                        Program.Log.Print($"WARNING: The tool plug in '{plugin.Name}' is disabled:", LoggingLevel.Simple);
                        foreach (string reason in validation)
                        {
                            Program.Log.Print($"WARNING: {reason}", LoggingLevel.Verbose);
                        }

                        _disabled[plugin.Name] = new DisabledPlugin(DisabledReasonCode.ValidationError, plugin.Name, string.Join("\n", validation), plugin.PlugInPath);

                        // Remove this plug in.
                        plugins.Unload(plugin.Name);
                        continue;
                    }

                    if (plugin.Button == null)
                    {
                        // Shut the plug in down.
                        plugin.Shutdown();

                        Program.Log.Print($"WARNING: The tool plug in '{plugin.Name}' is disabled:", LoggingLevel.Simple);
                        Program.Log.Print($"WARNING: {Resources.GOREDIT_ERR_TOOL_NO_BUTTON}", LoggingLevel.Verbose);

                        _disabled[plugin.Name] = new DisabledPlugin(DisabledReasonCode.ValidationError, plugin.Name, Resources.GOREDIT_ERR_TOOL_NO_BUTTON, plugin.PlugInPath);

                        // Remove this plug in.
                        plugins.Unload(plugin.Name);
                    }

                    AddToolPlugin(plugin);                    
                }
                catch (Exception ex)
                {
                    // Attempt to gracefully shut the plug in down if we error out.
                    plugin.Shutdown();

                    Program.Log.Print($"ERROR: Cannot create tool plug in '{plugin.Name}'.", LoggingLevel.Simple);
                    Program.Log.LogException(ex);

                    _disabled[plugin.Name] = new DisabledPlugin(DisabledReasonCode.Error, plugin.Name, string.Format(Resources.GOREDIT_DISABLE_CONTENT_PLUGIN_EXCEPTION, ex.Message), plugin.PlugInPath);
                }
            }

            RebuildRibbonButtons();
        }

        /// <summary>Function to remove a tool plugin from the service.</summary>
        /// <param name="plugin">The plugin to remove.</param>
        public void RemoveToolPlugin(ToolPlugin plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            if (!_plugins.ContainsKey(plugin.Name))
            {
                return;
            }

            _plugins.Remove(plugin.Name);
            plugin.Shutdown();

            RebuildRibbonButtons();
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the ToolPluginService class.</summary>
        /// <param name="graphicsContext">The graphics context used to pass the application graphics context to plug ins.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphicsContext"/> parameter is <b>null</b>.</exception>
        public ToolPluginService(IGraphicsContext graphicsContext) => _graphicsContext = graphicsContext ?? throw new ArgumentNullException(nameof(graphicsContext));        
        #endregion
    }
}
