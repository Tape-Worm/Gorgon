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
// Created: October 29, 2018 1:14:25 PM
// 
#endregion

using System.IO;
using Gorgon.Core;
using Gorgon.Editor.Content;
using Gorgon.Editor.Plugins;
using Gorgon.Plugins;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A service used to manage content plug ins for the application.
    /// </summary>
    internal interface IContentPluginManagerService
        : IContentPluginService
    {
        /// <summary>
        /// Function to load all of the content plug ins into the service.
        /// </summary>
        /// <param name="pluginCache">The plug in assembly cache.</param>
        /// <param name="pluginDir">The directory that contains the plug ins.</param>
        void LoadContentPlugins(GorgonMefPluginCache pluginCache, DirectoryInfo pluginDir);

        /// <summary>
        /// Function to add a content plugin to the service.
        /// </summary>
        /// <param name="plugin">The plugin to add.</param>
        void AddContentPlugin(ContentPlugin plugin);

        /// <summary>
        /// Function to remove a content plugin from the service.
        /// </summary>
        /// <param name="plugin">The plugin to remove.</param>
        void RemoveContentPlugin(ContentPlugin plugin);

        /// <summary>
        /// Function to set up the content plug in association for a content file.
        /// </summary>
        /// <param name="contentFile">The content file to evaluate.</param>
        /// <param name="fileManager">The file manager used to manage content files.</param>
        /// <param name="metadataOnly"><b>true</b> to indicate that only metadata should be used to scan the content file, <b>false</b> to scan, in depth, per plugin (slow).</param>
        /// <returns><b>true</b> if a content plug in was associated, <b>false</b> if not.</returns>
        bool AssignContentPlugin(IContentFile contentFile, IContentFileManager fileManager, bool metadataOnly);

        /// <summary>
        /// Function to assign a plugin to a content file.
        /// </summary>
        /// <param name="contentFile">The content file to update.</param>
        /// <param name="fileManager">The file manager used to manage content files.</param>
        /// <param name="plugin">The plugin to assign.</param>
        void AssignContentPlugin(IContentFile contentFile, IContentFileManager fileManager, IContentPluginMetadata plugin);

        /// <summary>
        /// Function to clear all of the content plugins.
        /// </summary>
        void Clear();        
    }
}
