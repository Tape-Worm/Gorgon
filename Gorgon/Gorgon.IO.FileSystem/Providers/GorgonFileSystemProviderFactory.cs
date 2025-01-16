
// 
// Gorgon
// Copyright (C) 2015 Michael Winsor
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
using Gorgon.PlugIns;

namespace Gorgon.IO.FileSystem.Providers;

/// <inheritdoc cref="IGorgonFileSystemProvider"/>
public sealed class GorgonFileSystemProviderFactory(GorgonMefPlugInCache plugInCache, IGorgonLog? log = null)
        : IGorgonFileSystemProviderFactory
{
    // The service for locating plug-ins.
    private readonly GorgonMefPlugInCache _plugInCache = plugInCache ?? throw new ArgumentNullException(nameof(plugInCache));
    // The application log file.
    private readonly IGorgonLog _log = log ?? GorgonLog.NullLog;
    // The list of loaded plug-ins.
    private readonly Dictionary<string, GorgonFileSystemProviderPlugIn> _plugIns = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    [RequiresAssemblyFiles("Plug ins will not work with trimming and Native AOT.")]
    public IReadOnlyDictionary<string, GorgonFileSystemProviderPlugIn> PlugIns => _plugIns;

    /// <inheritdoc/>
    [RequiresAssemblyFiles("Plug ins will not work with trimming and Native AOT.")]
    public IGorgonFileSystemProvider CreateProvider(string path, string providerPlugInName)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(providerPlugInName);

        if (_plugIns.TryGetValue(providerPlugInName, out GorgonFileSystemProviderPlugIn? plugin))
        {
            return plugin.CreateProvider(_log);
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

        _plugInCache.LoadPlugInAssemblies(dirName, fileName);

        _log.Print($"Creating file system provider '{providerPlugInName}'.", LoggingLevel.Simple);

        GorgonMefPlugInService plugInService = new(_plugInCache);

        plugin = plugInService.GetPlugIn<GorgonFileSystemProviderPlugIn>(providerPlugInName)
            ?? throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORFS_ERR_NO_PROVIDER_PLUGIN, providerPlugInName));

        _plugIns[providerPlugInName] = plugin;

        return plugin.CreateProvider(_log);
    }

    /// <inheritdoc/>
    [RequiresAssemblyFiles("Plug ins will not work with trimming and Native AOT.")]
    public IReadOnlyList<IGorgonFileSystemProvider> CreateProviders(string path)
    {
        GorgonMefPlugInService plugInService = new(_plugInCache);

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

        _plugInCache.LoadPlugInAssemblies(dirName, fileName);

        List<IGorgonFileSystemProvider> providers = [];

        IReadOnlyList<string> plugInNames = plugInService.GetPlugInNames();

        foreach (string plugInName in plugInNames)
        {
            if (!_plugIns.TryGetValue(plugInName, out GorgonFileSystemProviderPlugIn? plugIn))
            {
                plugIn = plugInService.GetPlugIn<GorgonFileSystemProviderPlugIn>(plugInName);

                if (plugIn is null)
                {
                    _log.Print($"Found plug-in named '{plugInName}' but it is not a Gorgon file system provider plug-in. Skipping.", LoggingLevel.Verbose);
                    continue;
                }

                _plugIns[plugInName] = plugIn;
            }

            providers.Add(plugIn.CreateProvider(_log));
        }

        return providers;
    }
}
