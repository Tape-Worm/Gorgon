
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
// Created: Tuesday, June 2, 2015 9:56:56 PM
// 

using System.Diagnostics.CodeAnalysis;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO.FileSystem.Properties;
using Gorgon.Plugins;

namespace Gorgon.IO.FileSystem.Providers;

/// <inheritdoc cref="IGorgonFileSystemProvider"/>
public sealed class GorgonFileSystemProviderFactory(GorgonMefPluginCache pluginCache, IGorgonLog? log = null)
        : IGorgonFileSystemProviderFactory
{
    // The service for locating plugins.
    private readonly GorgonMefPluginCache _pluginCache = pluginCache ?? throw new ArgumentNullException(nameof(pluginCache));
    // The application log file.
    private readonly IGorgonLog _log = log ?? GorgonLog.NullLog;
    // The list of loaded plugins.
    private readonly Dictionary<string, GorgonFileSystemProviderPlugin> _plugins = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    [RequiresAssemblyFiles("Plug ins will not work with trimming and Native AOT.")]
    public IReadOnlyDictionary<string, GorgonFileSystemProviderPlugin> Plugins => _plugins;

    /// <inheritdoc/>
    [RequiresAssemblyFiles("Plug ins will not work with trimming and Native AOT.")]
    public IGorgonFileSystemProvider CreateProvider(string path, string providerPluginName)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(providerPluginName);

        if (_plugins.TryGetValue(providerPluginName, out GorgonFileSystemProviderPlugin? Plugin))
        {
            return Plugin.CreateProvider(_log);
        }

        string dirName = Path.GetDirectoryName(path) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(dirName))
        {
            dirName = Directory.GetCurrentDirectory();
        }

        string fileName = Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(string.Format(Resources.GORFS_ERR_PATH_INVALID, path), nameof(path));
        }

        _pluginCache.LoadPluginAssemblies(dirName, fileName);

        _log.Print($"Creating file system provider '{providerPluginName}'.", LoggingLevel.Simple);

        GorgonMefPluginService PluginService = new(_pluginCache);

        Plugin = PluginService.GetPlugin<GorgonFileSystemProviderPlugin>(providerPluginName)
            ?? throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORFS_ERR_NO_PROVIDER_PLUGIN, providerPluginName));

        _plugins[providerPluginName] = Plugin;

        return Plugin.CreateProvider(_log);
    }

    /// <inheritdoc/>
    [RequiresAssemblyFiles("Plug ins will not work with trimming and Native AOT.")]
    public IReadOnlyList<IGorgonFileSystemProvider> CreateProviders(string path)
    {
        GorgonMefPluginService PluginService = new(_pluginCache);

        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        string dirName = Path.GetDirectoryName(path) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(dirName))
        {
            dirName = Directory.GetCurrentDirectory();
        }

        string fileName = Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(string.Format(Resources.GORFS_ERR_PATH_INVALID, path), nameof(path));
        }

        _pluginCache.LoadPluginAssemblies(dirName, fileName);

        List<IGorgonFileSystemProvider> providers = [];

        IReadOnlyList<string> PluginNames = PluginService.GetPluginNames();

        foreach (string PluginName in PluginNames)
        {
            if (!_plugins.TryGetValue(PluginName, out GorgonFileSystemProviderPlugin? Plugin))
            {
                Plugin = PluginService.GetPlugin<GorgonFileSystemProviderPlugin>(PluginName);

                if (Plugin is null)
                {
                    _log.Print($"Found plugin named '{PluginName}' but it is not a Gorgon file system provider plugin. Skipping.", LoggingLevel.Verbose);
                    continue;
                }

                _plugins[PluginName] = Plugin;
            }

            providers.Add(Plugin.CreateProvider(_log));
        }

        return providers;
    }
}
