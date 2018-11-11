﻿#region MIT
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
// Created: September 24, 2018 11:05:00 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.IO.Providers;
using Gorgon.Plugins;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// Functionality to capture and load file system providers from plugins.
    /// </summary>
    internal class FileSystemProviders
        : IFileSystemProviders
    {
        #region Variables.
        // A list of available file system reader providers.
        private readonly Dictionary<string, IGorgonFileSystemProvider> _readers = new Dictionary<string, IGorgonFileSystemProvider>(StringComparer.OrdinalIgnoreCase);
        // A list of available file system writer providers.
        private readonly Dictionary<string, FileWriterPlugin> _writers = new Dictionary<string, FileWriterPlugin>(StringComparer.OrdinalIgnoreCase);
        // A list of disabled plug ins.
        private readonly Dictionary<string, IDisabledPlugin> _disabled = new Dictionary<string, IDisabledPlugin>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Properties.                
        /// <summary>
        /// Property to return the list of disabled provider plug ins.
        /// </summary>
        public IReadOnlyDictionary<string, IDisabledPlugin> DisabledPlugins => _disabled;

        /// <summary>
        /// Property to return all loaded file system reader providers.
        /// </summary>
        public IReadOnlyDictionary<string, IGorgonFileSystemProvider> Readers => _readers;

        /// <summary>
        /// Property to return all loaded file system writer plug ins.
        /// </summary>
        public IReadOnlyDictionary<string, FileWriterPlugin> Writers => _writers;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the file system provider that supports the specified file.
        /// </summary>
        /// <param name="file">The file to evaluate.</param>
        /// <returns>The file system provider that can read the file.</returns>
        private IGorgonFileSystemProvider GetBestFit(FileInfo file)
        {
            foreach (IGorgonFileSystemProvider provider in _readers.Values)
            {
                if (provider.CanReadFileSystem(file.FullName))
                {
                    return provider;
                }
            }

            return null;
        }

        /// <summary>Function to return the [FileWriterPlugin] by its plugin name.</summary>
        /// <param name="writerName">The name of the writer plug in to locate.</param>
        /// <param name="useV2PluginName">[Optional] Use the v2 compatible plugin name.</param>
        /// <returns>The [FileWriterPlugin], or <b>null</b> if no writer could be found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="writerName" /> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="writerName" /> parameter is empty.</exception>
        public FileWriterPlugin GetWriterByName(string writerName, bool useV2PluginName = false)
        {
            if (writerName == null)
            {
                throw new ArgumentNullException(nameof(writerName));
            }

            if (string.IsNullOrWhiteSpace(writerName))
            {
                throw new ArgumentEmptyException(nameof(writerName));
            }

            if (useV2PluginName)
            {
                FileWriterPlugin v2Plugin = _writers.FirstOrDefault(item => string.Equals(item.Value.V2PluginName, writerName, StringComparison.OrdinalIgnoreCase)).Value;

                if (v2Plugin != null)
                {
                    return v2Plugin;
                }
            }

            _writers.TryGetValue(writerName, out FileWriterPlugin result);

            return result;
        }

        /// <summary>
        /// Function to find the most suitable provider for the file specified in the path.
        /// </summary>
        /// <param name="file">The file to evaluate.</param>
        /// <returns>The best suitable provider, or <b>null</b> if none could be located.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b>.</exception>
        public IGorgonFileSystemProvider GetBestReader(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            // First, try to locate by extension.
            IGorgonFileSystemProvider result = _readers.Select(item => item.Value)
                .FirstOrDefault(item => item.PreferredExtensions.Contains(file.Extension));

            // No provider is registered with that extension, fall back to trying to read each file.
            if (result == null)
            {
                return GetBestFit(file);
            }
            else
            {
                if (!result.CanReadFileSystem(file.FullName))
                {
                    return GetBestFit(file);
                }
            }

            return result;
        }

        /// <summary>
        /// Function to retrieve the available file extensions for all writers.
        /// </summary>
        /// <returns>A list of all file extensions available for all writers.</returns>
        public IReadOnlyList<(string desc, FileWriterPlugin plugin, IReadOnlyList<GorgonFileExtension> extensions)> GetWriterFileExtensions()
        {
            var result = new Dictionary<string, (FileWriterPlugin, List<GorgonFileExtension>)>(StringComparer.CurrentCultureIgnoreCase);

            foreach (KeyValuePair<string, FileWriterPlugin> provider in _writers.OrderBy(item => item.Value.Description))
            {
                if (provider.Value.FileExtensions.Count == 0)
                {
                    continue;
                }

                string description = provider.Value.FileExtensions.FirstOrDefault(item => !string.IsNullOrWhiteSpace(item.Description)).Description;

                if (string.IsNullOrWhiteSpace(description))
                {
                    continue;
                }


                if (!result.TryGetValue(description, out (FileWriterPlugin plugin, List<GorgonFileExtension> extensions) extensions))
                {
                    result[description] = extensions = (provider.Value, new List<GorgonFileExtension>());
                }

                extensions.extensions.AddRange(provider.Value.FileExtensions.OrderBy(item => item.Extension));
            }

            return result.Where(item => item.Value.Item2.Count > 0).Select(item => (item.Key, item.Value.Item1, (IReadOnlyList<GorgonFileExtension>)item.Value.Item2)).ToArray();
        }

        /// <summary>
        /// Function to retrieve the available file extensions for all readers.
        /// </summary>
        /// <returns>A list of all file extensions available for all readers.</returns>
        public IReadOnlyList<(string desc, IReadOnlyList<GorgonFileExtension> extensions)> GetReaderFileExtensions()
        {
            var result = new Dictionary<string, List<GorgonFileExtension>>(StringComparer.CurrentCultureIgnoreCase);

            foreach (KeyValuePair<string, IGorgonFileSystemProvider> provider in _readers.OrderBy(item => item.Value.Description))
            {
                if (provider.Value.PreferredExtensions.Count == 0)
                {
                    continue;
                }

                string description = provider.Value.PreferredExtensions.FirstOrDefault(item => !string.IsNullOrWhiteSpace(item.Description)).Description;

                if (string.IsNullOrWhiteSpace(description))
                {
                    continue;
                }

                if (!result.TryGetValue(description, out List<GorgonFileExtension> extensions))
                {
                    result[description] = extensions = new List<GorgonFileExtension>();
                }

                extensions.AddRange(provider.Value.PreferredExtensions.OrderBy(item => item.Extension));
            }

            return result.Where(item => item.Value.Count > 0).Select(item => (item.Key, (IReadOnlyList<GorgonFileExtension>)item.Value)).ToArray();
        }

        /// <summary>
        /// Function to load the file system provider plug ins.
        /// </summary>
        /// <param name="pluginCache">The MEF plug in cache used to load the file system plug ins.</param>
        /// <param name="pluginDir">The plug in directory.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginCache"/>, or the <paramref name="pluginDir"/> parameter is <b>null</b>.</exception>
        public void LoadProviders(GorgonMefPluginCache pluginCache, DirectoryInfo pluginDir)
        {
            if (pluginCache == null)
            {
                throw new ArgumentNullException(nameof(pluginCache));
            }

            if (pluginDir == null)
            {
                throw new ArgumentNullException(nameof(pluginDir));
            }

            FileInfo[] assemblies = pluginDir.GetFiles("*.dll");

            foreach (FileInfo file in assemblies)
            {
                try
                {
                    Program.Log.Print($"Loading file system provider plug in assembly '{file.FullName}'...", LoggingLevel.Simple);
                    pluginCache.LoadPluginAssemblies(file.DirectoryName, file.Name);
                }
                catch (Exception ex)
                {
                    Program.Log.Print($"ERROR: Cannot load provider plug in assembly '{file.FullName}'.", LoggingLevel.Simple);
                    Program.Log.LogException(ex);
                }
            }

            IGorgonPluginService plugins = new GorgonMefPluginService(pluginCache, Program.Log);
            IReadOnlyList<GorgonFileSystemProvider> readers = plugins.GetPlugins<GorgonFileSystemProvider>();
            IReadOnlyList<FileWriterPlugin> writers = plugins.GetPlugins<FileWriterPlugin>();

            // Get readers.
            foreach (IGorgonFileSystemProvider reader in readers)
            {
                try
                {                   
                    Program.Log.Print($"Creating file system reader plug in '{reader.Name}'...", LoggingLevel.Simple);                    
                    _readers[reader.Name] = reader;
                }
                catch (Exception ex)
                {
                    Program.Log.Print($"ERROR: Cannot create file system reader plug in '{reader.Name}'.", LoggingLevel.Simple);
                    Program.Log.LogException(ex);

                    _disabled[reader.Name] = new DisabledPlugin(DisabledReasonCode.Error, reader.Name, string.Format(Resources.GOREDIT_DISABLE_FILE_PROVIDER_EXCEPTION, ex.Message));
                }
            }

            // Get writers
            foreach (FileWriterPlugin writer in writers)
            {
                IReadOnlyList<string> disabled = writer.IsPluginAvailable();

                try
                {
                    Program.Log.Print($"Creating file system writer plug in '{writer.Name}'...", LoggingLevel.Simple);

                    if (disabled.Count != 0)
                    {                        
                        Program.Log.Print($"WARNING: The file system writer plug in '{writer.Name}' is disabled:", LoggingLevel.Simple);
                        foreach (string reason in disabled)
                        {
                            Program.Log.Print($"WARNING: {reason}", LoggingLevel.Verbose);
                        }                        

                        _disabled[writer.Name] = new DisabledPlugin(DisabledReasonCode.ValidationError, writer.Name, string.Join("\n", disabled));
                        continue;
                    }

                    _writers[writer.GetType().FullName] = writer;
                }
                catch (Exception ex)
                {
                    Program.Log.Print($"ERROR: Cannot create file system writer plug in '{writer.Name}'.", LoggingLevel.Simple);
                    Program.Log.LogException(ex);

                    _disabled[writer.Name] = new DisabledPlugin(DisabledReasonCode.Error, writer.Name, string.Format(Resources.GOREDIT_DISABLE_FILE_PROVIDER_EXCEPTION, ex.Message));
                }
            }
        }
        #endregion
    }
}
