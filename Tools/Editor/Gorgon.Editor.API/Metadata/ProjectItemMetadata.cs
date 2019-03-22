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
// Created: September 5, 2018 12:35:20 PM
// 
#endregion


using System;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Editor.Plugins;
using Newtonsoft.Json;

namespace Gorgon.Editor.Metadata
{
    /// <summary>
    /// Metadata for a project item that is included in the project.
    /// </summary>
    public class ProjectItemMetadata
    {
        #region Variables.
        // The metadata for a content plugin.
        private IContentPluginMetadata _contentMetadata;
        #endregion

        #region Properties.        
        /// <summary>
        /// Property to set or return the name of the plugin associated with the metadata file path.
        /// </summary>
        /// <remarks>
        /// If this value is <b>null</b>, then the plugin hasn't been set.  If it's an empty string, then no plugin is associated with this metadata.
        /// </remarks>
        public string PluginName
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the custom attributes for this metadata.
        /// </summary>
        [JsonProperty]
        public Dictionary<string, string> Attributes
        {
            get;
            private set;
        } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Property to return the list of item paths that this item depends on.
        /// </summary>
        [JsonProperty]
        public Dictionary<string, string> Dependencies
        {
            get;
            private set;
        } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// Property to return the content plugin metadata associated with this project item.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting this value will set the value for <see cref="PluginName"/>.
        /// </para>
        /// </remarks>
        [JsonIgnore]
        public IContentPluginMetadata ContentMetadata
        {
            get => _contentMetadata;
            set
            {
                if (value == null)
                {
                    PluginName = null;
                }
                else
                {
                    PluginName = value.PluginName;
                }

                _contentMetadata = value;
            }
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the ProjectItemMetadata class.</summary>
        /// <param name="metadata">The metadata to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="metadata"/> parameter is <b>null</b>.</exception>
        public ProjectItemMetadata(ProjectItemMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }
                        
            _contentMetadata = metadata.ContentMetadata;
            PluginName = metadata.PluginName;

            foreach (KeyValuePair<string, string> attribute in metadata.Attributes)
            {
                Attributes[attribute.Key] = attribute.Value;
            }

            foreach (KeyValuePair<string, string> path in metadata.Dependencies)
            {
                Dependencies[path.Key] = path.Value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectItemMetadata"/> class.
        /// </summary>
        public ProjectItemMetadata()
        {
        }
        #endregion
    }
}
