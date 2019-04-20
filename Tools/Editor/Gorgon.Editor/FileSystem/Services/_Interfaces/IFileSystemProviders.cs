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
// Created: September 24, 2018 11:14:09 AM
// 
#endregion

using System.Collections.Generic;
using System.IO;
using Gorgon.Editor.Plugins;
using Gorgon.IO;
using Gorgon.IO.Providers;
using Gorgon.Plugins;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// Functionality to capture and load file system providers from plugins.
    /// </summary>
    internal interface IFileSystemProviders
        : IDisabledPluginService
    {
        #region Properties.
        /// <summary>
        /// Property to return all loaded file system reader providers.
        /// </summary>
        IReadOnlyDictionary<string, IGorgonFileSystemProvider> Readers
        {
            get;
        }

        /// <summary>
        /// Property to return all loaded file system writer plug ins.
        /// </summary>
        IReadOnlyDictionary<string, FileWriterPlugin> Writers
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the available file extensions for all readers.
        /// </summary>
        /// <returns>A list of all file extensions available for all readers.</returns>
        IReadOnlyList<(string desc, IReadOnlyList<GorgonFileExtension> extensions)> GetReaderFileExtensions();

        /// <summary>
        /// Function to retrieve the available file extensions for all writers.
        /// </summary>
        /// <returns>A list of all file extensions available for all writers.</returns>
        IReadOnlyList<(string desc, FileWriterPlugin plugin, IReadOnlyList<GorgonFileExtension> extensions)> GetWriterFileExtensions();

        /// <summary>
        /// Function to load the file system provider plug ins.
        /// </summary>
        /// <param name="pluginCache">The MEF plug in cache used to load the file system plug ins.</param>
        /// <param name="pluginDir">The plug in directory.</param>
        void LoadProviders(GorgonMefPluginCache pluginCache, DirectoryInfo pluginDir);

        /// <summary>
        /// Function to find the most suitable provider for the file specified in the path.
        /// </summary>
        /// <param name="file">The file to evaluate.</param>
        /// <returns>The best suitable provider, or <b>null</b> if none could be located.</returns>
        IGorgonFileSystemProvider GetBestReader(FileInfo file);

        /// <summary>
        /// Function to return the <see cref="FileWriterPlugin"/> by its plugin name.
        /// </summary>
        /// <param name="writerName">The name of the writer plug in to locate.</param>
        /// <param name="useV2PluginName">[Optional] Use the v2 compatible plugin name.</param>
        /// <returns>The <see cref="FileWriterPlugin"/>, or <b>null</b> if no writer could be found.</returns>
        FileWriterPlugin GetWriterByName(string writerName, bool useV2PluginName = false);
        #endregion
    }
}
