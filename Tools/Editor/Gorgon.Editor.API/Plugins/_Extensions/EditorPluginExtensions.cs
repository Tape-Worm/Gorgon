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
            switch (pluginType)
            {
                case PlugInType.Writer:
                    return Resources.GOREDIT_PLUGIN_TYPE_WRITER;
                case PlugInType.Content:
                    return Resources.GOREDIT_PLUGIN_TYPE_CONTENT;
                case PlugInType.Tool:
                    return Resources.GOREDIT_PLUGIN_TYPE_TOOL;
                case PlugInType.Reader:
                    return Resources.GOREDIT_PLUGIN_TYPE_READER;
                case PlugInType.ContentImporter:
                    return Resources.GOREDIT_PLUGIN_TYPE_IMPORTER;
                default:
                    return Resources.GOREDIT_PLUGIN_TYPE_UNKNOWN;
            }
        }

        /// <summary>Function to load all the specified plug in assemblies.</summary>
        /// <param name="pluginCache">The plugin cache that will hold the plug in assembies.</param>
        /// <param name="pluginAssemblyFiles">The list of plug in assembly paths to load.</param>
        /// <param name="log">The application logging interface.</param>
        /// <returns>A list of <see cref="PlugInRecord"/> objects for each plug in assembly loaded.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginAssemblyFiles" /> parameter is <b>null</b></exception>
        public static IReadOnlyList<PlugInRecord> ValidateAndLoadAssemblies(this GorgonMefPlugInCache pluginCache, IEnumerable<FileInfo> pluginAssemblyFiles, IGorgonLog log)
        {
            if (pluginAssemblyFiles == null)
            {
                throw new ArgumentNullException(nameof(pluginAssemblyFiles));
            }

            var records = new List<PlugInRecord>();

			// We use this to determine whether the plug in can be loaded into the current platform.
            AssemblyPlatformType currentPlatform = IntPtr.Size == 8 ? AssemblyPlatformType.x64 : AssemblyPlatformType.x86;

            foreach (FileInfo file in pluginAssemblyFiles)
            {
				// If we've already got the assembly loaded into this cache, then there's no need to try and load it.
                if (pluginCache.PlugInAssemblies?.Any(item => string.Equals(item, file.FullName, StringComparison.OrdinalIgnoreCase)) ?? false)
                {
                    continue;
                }

                if (!file.Exists)
                {
                    log.Print($"[ERROR] Plug in '{file.FullName}' was not found.", LoggingLevel.Verbose);
                    records.Add(new PlugInRecord(file.FullName, Resources.GOREDIT_PLUGIN_LOAD_FAIL_NOT_FOUND, false));
                    continue;
                }

                (bool isManaged, AssemblyPlatformType platformType) = GorgonMefPlugInCache.IsManagedAssembly(file.FullName);

                if ((!isManaged) || (platformType == AssemblyPlatformType.Unknown))
                {
                    log.Print($"[WARNING] Skipping '{file.FullName}'. Not a valid .NET assembly.", LoggingLevel.Verbose);
                    records.Add(new PlugInRecord(file.FullName, string.Format(Resources.GOREDIT_PLUGIN_LOAD_FAIL_NOT_DOT_NET, file.Name), false));
                    continue;
                }

                // Ensure that our platform type matches (AnyCPU is exempt and will always run, and DLLs don't allow Prefer 32 bit).
                if ((currentPlatform == AssemblyPlatformType.x86) && (platformType == AssemblyPlatformType.x64))
                {
                    log.Print($"[ERROR] Cannot load assembly '{file.FullName}', currently executing in an x86 environment, but the assembly is x64.", LoggingLevel.Simple);
                    records.Add(new PlugInRecord(file.FullName, string.Format(Resources.GOREDIT_PLUGIN_LOAD_FAIL_PLATFORM_MISMATCH, file.Name, platformType, currentPlatform), true));
                    continue;
                }

                if ((currentPlatform == AssemblyPlatformType.x64) && (platformType == AssemblyPlatformType.x86))
                {
                    log.Print($"[ERROR] Cannot load assembly '{file.FullName}', currently executing in an x64 environment, but the assembly is x86.", LoggingLevel.Simple);
                    records.Add(new PlugInRecord(file.FullName, string.Format(Resources.GOREDIT_PLUGIN_LOAD_FAIL_PLATFORM_MISMATCH, file.Name, platformType, currentPlatform), true));
                    continue;
                }

                try
                {
                    log.Print($"Loading content plug in assembly '{file.FullName}'...", LoggingLevel.Simple);
                    pluginCache.LoadPlugInAssemblies(file.DirectoryName, file.Name);

                    records.Add(new PlugInRecord(file.FullName, string.Empty, true));
                }
                catch (Exception ex)
                {
                    log.Print($"ERROR: Cannot load content plug in assembly '{file.FullName}'.", LoggingLevel.Simple);
                    log.LogException(ex);
                    records.Add(new PlugInRecord(file.FullName, string.Format(Resources.GOREDIT_PLUGIN_LOAD_FAIL_EXCEPTION, file.Name, ex.Message), true));
                }
            }

            return records;
        }
    }
}
