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
// Created: November 3, 2018 1:07:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.Metadata
{
    /// <summary>
    /// The state for the plugin when associated with an included item.
    /// </summary>
    public enum MetadataPluginState
    {
        /// <summary>
        /// No plugin was ever assigned.
        /// </summary>
        Unassigned,
        /// <summary>
        /// The plugin was assigned.
        /// </summary>
        Assigned,
        /// <summary>
        /// The plugin was not found.
        /// </summary>
        NotFound
    }

    /// <summary>
    /// Extension methods for the <see cref="ProjectItemMetadata"/>.
    /// </summary>
    public static class ProjectItemMetadataExtensions
    {
        /// <summary>
        /// Function to retrieve the actual plug in based on the name associated with the project metadata item.
        /// </summary>
        /// <param name="metadata">The metadata item to evaluate.</param>
        /// <param name="plugins">The service that manages the content plug ins.</param>
        /// <returns>The plug in, and the <see cref="MetadataPluginState"/> used to evaluate whether a deep inspection is required.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="metadata"/>, or the <paramref name="plugins"/> parameter is <b>null</b>.</exception>
        public static (ContentPlugin plugin, MetadataPluginState state) GetContentPlugin(this ProjectItemMetadata metadata, IContentPluginService plugins)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            if (plugins == null)
            {
                throw new ArgumentNullException(nameof(plugins));
            }

            // If the name is null, then we've never assigned the content plugin.  So look it up.
            if (metadata.PluginName == null)
            {
                return (null, MetadataPluginState.Unassigned);
            }

            // If we have an empty string
            if (string.IsNullOrWhiteSpace(metadata.PluginName))
            {
                return (null, MetadataPluginState.NotFound);
            }

            if (!plugins.Plugins.TryGetValue(metadata.PluginName, out ContentPlugin plugin))
            {
                return (null, MetadataPluginState.NotFound);
            }

            return (plugin, MetadataPluginState.Assigned);
        }
    }
}
