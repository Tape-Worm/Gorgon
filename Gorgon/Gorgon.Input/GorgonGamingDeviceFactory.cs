
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
// Created: Saturday, September 12, 2015 1:47:55 PM
// 

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Input.Properties;
using Gorgon.PlugIns;

namespace Gorgon.Input;

/// <summary>
/// A factory used to load gaming device drivers
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonGamingDeviceDriverFactory"/> class
/// </remarks>
/// <param name="pluginCache">The plug-in cache that will hold the plug-in assemblies.</param>
/// <param name="log">[Optional] The logger used for debugging.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginCache"/> is <b>null</b>.</exception>
public sealed class GorgonGamingDeviceDriverFactory(GorgonMefPlugInCache pluginCache, IGorgonLog log = null)
        : IGorgonGamingDeviceDriverFactory
{

    // The logger used for debugging.
    private readonly IGorgonLog _log = log ?? GorgonLog.NullLog;
    // The plug-in service to use when loading drivers.
    private readonly IGorgonPlugInService _plugInService = new GorgonMefPlugInService(pluginCache);
    // The cache holding the plug-in assemblies.
    private readonly GorgonMefPlugInCache _plugInCache = pluginCache ?? throw new ArgumentNullException(nameof(pluginCache));

    /// <summary>
    /// Function to load all drivers from the plug-in assemblies that are currently loaded.
    /// </summary>
    /// <param name="assemblyPath">The path to the assembly containing the gaming driver plug-ins.</param>
    /// <returns>A read only list containing an instance of each driver.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="assemblyPath"/> parameter is <b>null</b></exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="assemblyPath"/> parameter is empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="assemblyPath"/> was invalid.</exception>
    public IReadOnlyList<IGorgonGamingDeviceDriver> LoadAllDrivers(string assemblyPath)
    {
        if (assemblyPath is null)
        {
            throw new ArgumentNullException(nameof(assemblyPath));
        }

        if (string.IsNullOrWhiteSpace(assemblyPath))
        {
            throw new ArgumentEmptyException(nameof(assemblyPath));
        }

        string dirName = Path.GetDirectoryName(assemblyPath);
        if (string.IsNullOrWhiteSpace(dirName))
        {
            dirName = Directory.GetCurrentDirectory();
        }

        string fileName = Path.GetFileName(assemblyPath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(string.Format(Resources.GORINP_ERR_INVALID_PATH, assemblyPath), nameof(assemblyPath));
        }

        _plugInCache.LoadPlugInAssemblies(dirName, fileName);
        return _plugInService.GetPlugIns<GorgonGamingDeviceDriver>();
    }

    /// <summary>
    /// Function to load a gaming device driver from any loaded plug-in assembly.
    /// </summary>
    /// <param name="assemblyPath">The path to the assembly containing the gaming driver plug-ins.</param>
    /// <param name="driverType">The fully qualified type name of the driver to load.</param>
    /// <returns>The gaming device driver plug-in.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="assemblyPath"/>, or the <paramref name="driverType"/> parameter is <b>null</b></exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="assemblyPath"/>, or the <paramref name="driverType"/> parameter is empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the driver type name specified by <paramref name="driverType"/> was not found in any of the loaded plug-in assemblies.
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="assemblyPath"/> was invalid.</para>
    /// </exception>
    public IGorgonGamingDeviceDriver LoadDriver(string assemblyPath, string driverType)
    {
        if (assemblyPath is null)
        {
            throw new ArgumentNullException(nameof(assemblyPath));
        }

        if (string.IsNullOrWhiteSpace(assemblyPath))
        {
            throw new ArgumentEmptyException(nameof(assemblyPath));
        }

        if (driverType is null)
        {
            throw new ArgumentNullException(nameof(driverType));
        }

        if (string.IsNullOrWhiteSpace(driverType))
        {
            throw new ArgumentEmptyException(nameof(driverType));
        }

        string dirName = Path.GetDirectoryName(assemblyPath);
        if (string.IsNullOrWhiteSpace(dirName))
        {
            dirName = Directory.GetCurrentDirectory();
        }

        string fileName = Path.GetFileName(assemblyPath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(string.Format(Resources.GORINP_ERR_INVALID_PATH, assemblyPath), nameof(assemblyPath));
        }

        _plugInCache.LoadPlugInAssemblies(dirName, fileName);
        GorgonGamingDeviceDriver result = _plugInService.GetPlugIn<GorgonGamingDeviceDriver>(driverType) ?? throw new ArgumentException(string.Format(Resources.GORINP_ERR_DRIVER_NOT_FOUND, driverType));

        result.Log = _log;

        return result;
    }
}
