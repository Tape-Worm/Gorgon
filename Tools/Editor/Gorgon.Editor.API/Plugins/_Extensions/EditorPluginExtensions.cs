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
// Created: October 12, 2018 12:52:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Diagnostics;
using Gorgon.Editor.Properties;
using Gorgon.PlugIns;

namespace Gorgon.Editor.PlugIns
{
    /// <summary>
    /// Extension functionality relating to editor plug ins.
    /// </summary>
    public static class EditorPlugInExtensions
    {
        /// <summary>
        /// Function to retrieve a friendly description of a <see cref="PlugInType"/> value.
        /// </summary>
        /// <param name="pluginType">The plug in type to evaluate.</param>
        /// <returns>The friendly description.</returns>
        public static string GetDescription(this PlugInType pluginType) 
        {
            return pluginType switch
            {
                PlugInType.Writer => Resources.GOREDIT_PLUGIN_TYPE_WRITER,
                PlugInType.Content => Resources.GOREDIT_PLUGIN_TYPE_CONTENT,
                PlugInType.Tool => Resources.GOREDIT_PLUGIN_TYPE_TOOL,
                PlugInType.Reader => Resources.GOREDIT_PLUGIN_TYPE_READER,
                PlugInType.ContentImporter => Resources.GOREDIT_PLUGIN_TYPE_IMPORTER,
                _ => Resources.GOREDIT_PLUGIN_TYPE_UNKNOWN,
            };
            ;
        }

        /// <summary>Function to load all the specified plug in assemblies.</summary>
        /// <param name="pluginCache">The plugin cache that will hold the plug in assembies.</param>
        /// <param name="pluginAssemblyFiles">The list of plug in assembly paths to load.</param>
        /// <param name="log">The application logging interface.</param>
        /// <returns>A list of <see cref="PlugInAssemblyState"/> objects for each plug in assembly loaded.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginAssemblyFiles" /> parameter is <b>null</b></exception>
        public static IReadOnlyList<PlugInAssemblyState> ValidateAndLoadAssemblies(this GorgonMefPlugInCache pluginCache, IEnumerable<string> pluginAssemblyFiles, IGorgonLog log)
        {
            if (pluginAssemblyFiles is null)
            {
                throw new ArgumentNullException(nameof(pluginAssemblyFiles));
            }

            var records = new List<PlugInAssemblyState>();

            // We use this to determine whether the plug in can be loaded into the current platform.
            AssemblyPlatformType currentPlatform = IntPtr.Size == 8 ? AssemblyPlatformType.x64 : AssemblyPlatformType.x86;

            foreach (string file in pluginAssemblyFiles.Select(item => Path.GetFullPath(item)))
            {
                string fileName = Path.GetFileName(file);

                // If we've already got the assembly loaded into this cache, then there's no need to try and load it.
                if (pluginCache.PlugInAssemblies?.Any(item => string.Equals(item, file, StringComparison.OrdinalIgnoreCase)) ?? false)
                {
                    continue;
                }

                if (!File.Exists(file))
                {
                    log.Print($"ERROR: Plug in '{file}' was not found.", LoggingLevel.Verbose);
                    records.Add(new PlugInAssemblyState(file, Resources.GOREDIT_PLUGIN_LOAD_FAIL_NOT_FOUND, false));
                    continue;
                }

                (bool isManaged, AssemblyPlatformType platformType) = GorgonMefPlugInCache.IsManagedAssembly(file);

                if ((!isManaged) || (platformType == AssemblyPlatformType.Unknown))
                {
                    log.Print($"WARNING: Skipping '{file}'. Not a valid .NET assembly.", LoggingLevel.Verbose);
                    records.Add(new PlugInAssemblyState(file, string.Format(Resources.GOREDIT_PLUGIN_LOAD_FAIL_NOT_DOT_NET, fileName), false));
                    continue;
                }

                // Ensure that our platform type matches (AnyCPU is exempt and will always run, and DLLs don't allow Prefer 32 bit).
                if ((currentPlatform == AssemblyPlatformType.x86) && (platformType == AssemblyPlatformType.x64))
                {
                    log.Print($"ERROR: Cannot load assembly '{file}', currently executing in an x86 environment, but the assembly is x64.", LoggingLevel.Simple);
                    records.Add(new PlugInAssemblyState(file, string.Format(Resources.GOREDIT_PLUGIN_LOAD_FAIL_PLATFORM_MISMATCH, fileName, platformType, currentPlatform), true));
                    continue;
                }

                if ((currentPlatform == AssemblyPlatformType.x64) && (platformType == AssemblyPlatformType.x86))
                {
                    log.Print($"ERROR: Cannot load assembly '{file}', currently executing in an x64 environment, but the assembly is x86.", LoggingLevel.Simple);
                    records.Add(new PlugInAssemblyState(file, string.Format(Resources.GOREDIT_PLUGIN_LOAD_FAIL_PLATFORM_MISMATCH, fileName, platformType, currentPlatform), true));
                    continue;
                }

                try
                {
                    log.Print($"Loading plug in assembly '{file}'...", LoggingLevel.Simple);
                    pluginCache.LoadPlugInAssemblies(Path.GetDirectoryName(file), fileName);

                    records.Add(new PlugInAssemblyState(file, string.Empty, true));
                }
                catch (Exception ex)
                {
                    log.Print($"ERROR: Cannot load plug in assembly '{file}'.", LoggingLevel.Simple);
                    log.LogException(ex);
                    records.Add(new PlugInAssemblyState(file, string.Format(Resources.GOREDIT_PLUGIN_LOAD_FAIL_EXCEPTION, fileName, ex.Message), true));
                }
            }

            return records;
        }
    }
}
