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
// Created: December 17, 2018 10:20:38 PM
// 
#endregion

using System.IO;
using Gorgon.Editor.Plugins;
using Gorgon.Plugins;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A service used to manage content plug ins for the application.
    /// </summary>
    internal interface IContentImporterPluginManagerService
        : IContentImporterPluginService
    {
        /// <summary>
        /// Function to load all of the content importer plug ins into the service.
        /// </summary>
        /// <param name="pluginCache">The plug in assembly cache.</param>
        /// <param name="pluginDir">The directory that contains the plug ins.</param>
        void LoadContentImporterPlugins(GorgonMefPluginCache pluginCache, DirectoryInfo pluginDir);

        /// <summary>
        /// Function to add a content importer plugin to the service.
        /// </summary>
        /// <param name="plugin">The plugin to add.</param>
        void AddContentImportPlugin(ContentImportPlugin plugin);

        /// <summary>
        /// Function to remove a content importer plugin from the service.
        /// </summary>
        /// <param name="plugin">The plugin to remove.</param>
        void RemoveContentImportPlugin(ContentImportPlugin plugin);

        /// <summary>
        /// Function to clear all of the content importer plugins.
        /// </summary>
        void Clear();

        /// <summary>
        /// Function to retrieve the appropriate content importer for the file specified.
        /// </summary>
        /// <param name="file">The content file to evaluate.</param>
        /// <param name="metadataOnly"><b>true</b> to indicate that only metadata should be used to scan the content file, <b>false</b> to scan, in depth, per plugin (slow).</param>
        /// <returns>A <see cref="IEditorContentImporter"/>, or <b>null</b> if none was found.</returns>
        IEditorContentImporter GetContentImporter(FileInfo file);
    }
}
