
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
// Created: October 12, 2018 12:52:19 PM
// 

using Gorgon.Diagnostics;
using Gorgon.Editor.Properties;
using Gorgon.Plugins;

namespace Gorgon.Editor.Plugins;

/// <summary>
/// Extension functionality relating to editor plugins
/// </summary>
public static class EditorPluginExtensions
{
    /// <summary>
    /// Function to retrieve a friendly description of a <see cref="PluginType"/> value.
    /// </summary>
    /// <param name="pluginType">The plugin type to evaluate.</param>
    /// <returns>The friendly description.</returns>
    public static string GetDescription(this PluginType pluginType)
    {
        return pluginType switch
        {
            PluginType.Writer => Resources.GOREDIT_plugin_TYPE_WRITER,
            PluginType.Content => Resources.GOREDIT_plugin_TYPE_CONTENT,
            PluginType.Tool => Resources.GOREDIT_plugin_TYPE_TOOL,
            PluginType.Reader => Resources.GOREDIT_plugin_TYPE_READER,
            PluginType.ContentImporter => Resources.GOREDIT_plugin_TYPE_IMPORTER,
            _ => Resources.GOREDIT_plugin_TYPE_UNKNOWN,
        };
        ;
    }

    /// <summary>Function to load all the specified plugin assemblies.</summary>
    /// <param name="pluginCache">The Plugin cache that will hold the plugin assembies.</param>
    /// <param name="pluginAssemblyFiles">The list of plugin assembly paths to load.</param>
    /// <param name="log">The application logging interface.</param>
    /// <returns>A list of <see cref="PluginAssemblyState"/> objects for each plugin assembly loaded.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginAssemblyFiles" /> parameter is <b>null</b></exception>
    public static IReadOnlyList<PluginAssemblyState> ValidateAndLoadAssemblies(this GorgonMefPluginCache pluginCache, IEnumerable<string> pluginAssemblyFiles, IGorgonLog log)
    {
        if (pluginAssemblyFiles is null)
        {
            throw new ArgumentNullException(nameof(pluginAssemblyFiles));
        }

        List<PluginAssemblyState> records = [];

        // We use this to determine whether the plugin can be loaded into the current platform.
        AssemblyPlatformType currentPlatform = Environment.Is64BitProcess ? AssemblyPlatformType.x64 : AssemblyPlatformType.x86;

        foreach (string file in pluginAssemblyFiles.Select(Path.GetFullPath))
        {
            string fileName = Path.GetFileName(file);

            // If we've already got the assembly loaded into this cache, then there's no need to try and load it.
            if (pluginCache.PluginAssemblies?.Any(item => string.Equals(item, file, StringComparison.OrdinalIgnoreCase)) ?? false)
            {
                continue;
            }

            if (!File.Exists(file))
            {
                log.PrintError($"Plug in '{file}' was not found.", LoggingLevel.Verbose);
                records.Add(new PluginAssemblyState(file, Resources.GOREDIT_plugin_LOAD_FAIL_NOT_FOUND, false));
                continue;
            }

            (bool isManaged, AssemblyPlatformType platformType) = GorgonMefPluginCache.IsManagedAssembly(file);

            if ((!isManaged) || (platformType == AssemblyPlatformType.Unknown))
            {
                log.PrintWarning($"Skipping '{file}'. Not a valid .NET assembly.", LoggingLevel.Verbose);
                records.Add(new PluginAssemblyState(file, string.Format(Resources.GOREDIT_plugin_LOAD_FAIL_NOT_DOT_NET, fileName), false));
                continue;
            }

            // Ensure that our platform type matches (AnyCPU is exempt and will always run, and DLLs don't allow Prefer 32 bit).
            if ((currentPlatform == AssemblyPlatformType.x86) && (platformType == AssemblyPlatformType.x64))
            {
                log.PrintError($"Cannot load assembly '{file}', currently executing in an x86 environment, but the assembly is x64.", LoggingLevel.Simple);
                records.Add(new PluginAssemblyState(file, string.Format(Resources.GOREDIT_plugin_LOAD_FAIL_PLATFORM_MISMATCH, fileName, platformType, currentPlatform), true));
                continue;
            }

            if ((currentPlatform == AssemblyPlatformType.x64) && (platformType == AssemblyPlatformType.x86))
            {
                log.PrintError($"Cannot load assembly '{file}', currently executing in an x64 environment, but the assembly is x86.", LoggingLevel.Simple);
                records.Add(new PluginAssemblyState(file, string.Format(Resources.GOREDIT_plugin_LOAD_FAIL_PLATFORM_MISMATCH, fileName, platformType, currentPlatform), true));
                continue;
            }

            try
            {
                log.Print($"Loading plugin assembly '{file}'...", LoggingLevel.Simple);
                pluginCache.LoadPluginAssemblies(Path.GetDirectoryName(file), fileName);

                records.Add(new PluginAssemblyState(file, string.Empty, true));
            }
            catch (Exception ex)
            {
                log.PrintError($"Cannot load plugin assembly '{file}'.", LoggingLevel.Simple);
                log.PrintException(ex);
                records.Add(new PluginAssemblyState(file, string.Format(Resources.GOREDIT_plugin_LOAD_FAIL_EXCEPTION, fileName, ex.Message), true));
            }
        }

        return records;
    }
}
