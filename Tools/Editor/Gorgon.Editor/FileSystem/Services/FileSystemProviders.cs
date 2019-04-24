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
// Created: September 24, 2018 11:05:00 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI.ViewModels;
using Gorgon.IO;
using Gorgon.IO.Providers;
using Gorgon.PlugIns;

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
        private readonly Dictionary<string, FileWriterPlugIn> _writers = new Dictionary<string, FileWriterPlugIn>(StringComparer.OrdinalIgnoreCase);
        // A list of disabled plug ins.
        private readonly Dictionary<string, IDisabledPlugIn> _disabled = new Dictionary<string, IDisabledPlugIn>(StringComparer.OrdinalIgnoreCase);
		// Common application services.
        private readonly IViewModelInjection _commonServices;
        #endregion

        #region Properties.                
        /// <summary>
        /// Property to return the list of disabled provider plug ins.
        /// </summary>
        public IReadOnlyDictionary<string, IDisabledPlugIn> DisabledPlugIns => _disabled;

        /// <summary>
        /// Property to return all loaded file system reader providers.
        /// </summary>
        public IReadOnlyDictionary<string, IGorgonFileSystemProvider> Readers => _readers;

        /// <summary>
        /// Property to return all loaded file system writer plug ins.
        /// </summary>
        public IReadOnlyDictionary<string, FileWriterPlugIn> Writers => _writers;
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

        /// <summary>Function to return the [FileWriterPlugIn] by its plugin name.</summary>
        /// <param name="writerName">The name of the writer plug in to locate.</param>
        /// <param name="useV2PlugInName">[Optional] Use the v2 compatible plugin name.</param>
        /// <returns>The [FileWriterPlugIn], or <b>null</b> if no writer could be found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="writerName" /> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="writerName" /> parameter is empty.</exception>
        public FileWriterPlugIn GetWriterByName(string writerName, bool useV2PlugInName = false)
        {
            if (writerName == null)
            {
                throw new ArgumentNullException(nameof(writerName));
            }

            if (string.IsNullOrWhiteSpace(writerName))
            {
                throw new ArgumentEmptyException(nameof(writerName));
            }

            if (useV2PlugInName)
            {
                FileWriterPlugIn v2PlugIn = _writers.FirstOrDefault(item => string.Equals(item.Value.V2PlugInName, writerName, StringComparison.OrdinalIgnoreCase)).Value;

                if (v2PlugIn != null)
                {
                    return v2PlugIn;
                }
            }

            _writers.TryGetValue(writerName, out FileWriterPlugIn result);

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
        public IReadOnlyList<(string desc, FileWriterPlugIn plugin, IReadOnlyList<GorgonFileExtension> extensions)> GetWriterFileExtensions()
        {
            var result = new Dictionary<string, (FileWriterPlugIn, List<GorgonFileExtension>)>(StringComparer.CurrentCultureIgnoreCase);

            foreach (KeyValuePair<string, FileWriterPlugIn> provider in _writers.OrderBy(item => item.Value.Description))
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


                if (!result.TryGetValue(description, out (FileWriterPlugIn plugin, List<GorgonFileExtension> extensions) extensions))
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
        public void LoadProviders(GorgonMefPlugInCache pluginCache, DirectoryInfo pluginDir)
        {
            if (pluginCache == null)
            {
                throw new ArgumentNullException(nameof(pluginCache));
            }

            if (pluginDir == null)
            {
                throw new ArgumentNullException(nameof(pluginDir));
            }

            IReadOnlyList<PlugInAssemblyState> assemblies = pluginCache.ValidateAndLoadAssemblies(pluginDir.GetFiles("*.dll"), Program.Log);

            if (assemblies.Count > 0)
            {
                foreach (PlugInAssemblyState record in assemblies.Where(item => !item.IsAssemblyLoaded && item.IsManaged))
                {
                    _disabled[Path.GetFileName(record.Path)] = new DisabledPlugIn(DisabledReasonCode.Error, Path.GetFileName(record.Path), record.LoadFailureReason, record.Path);
                }
            }

            IGorgonPlugInService plugins = new GorgonMefPlugInService(pluginCache, Program.Log);
            IReadOnlyList<GorgonFileSystemProvider> readers = plugins.GetPlugIns<GorgonFileSystemProvider>();
            IReadOnlyList<FileWriterPlugIn> writers = plugins.GetPlugIns<FileWriterPlugIn>();

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

                    _disabled[reader.Name] = new DisabledPlugIn(DisabledReasonCode.Error, reader.Name, string.Format(Resources.GOREDIT_DISABLE_FILE_PROVIDER_EXCEPTION, ex.Message), reader.ProviderPath);
                }
            }

            // Get writers
            foreach (FileWriterPlugIn writer in writers)
            {
                IReadOnlyList<string> disabled = writer.IsPlugInAvailable();

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

                        _disabled[writer.Name] = new DisabledPlugIn(DisabledReasonCode.ValidationError, writer.Name, string.Join("\n", disabled), writer.PlugInPath);
                        continue;
                    }

                    writer.AssignCommonServices(_commonServices);
                    _writers[writer.GetType().FullName] = writer;
                }
                catch (Exception ex)
                {
                    Program.Log.Print($"ERROR: Cannot create file system writer plug in '{writer.Name}'.", LoggingLevel.Simple);
                    Program.Log.LogException(ex);

                    _disabled[writer.Name] = new DisabledPlugIn(DisabledReasonCode.Error, writer.Name, string.Format(Resources.GOREDIT_DISABLE_FILE_PROVIDER_EXCEPTION, ex.Message), writer.PlugInPath);
                }
            }
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="FileSystemProviders"/> class.</summary>
        /// <param name="commonServices">The common services for the application.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="commonServices"/> parameter is <b>null</b>.</exception>
        public FileSystemProviders(IViewModelInjection commonServices) => _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
        #endregion
    }
}
