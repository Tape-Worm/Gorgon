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
// Created: October 29, 2018 1:19:30 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Plugins;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// The service used for managing the content plugins.
    /// </summary>
    internal class ContentPluginService
        : IContentPluginManagerService
    {
        #region Variables.
        // The plugin list.
        private Dictionary<string, ContentPlugin> _plugins = new Dictionary<string, ContentPlugin>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Properties.
        /// <summary>Property to return the list of content plugins loaded in to the application.</summary>
        /// <value>The plugins.</value>
        public IReadOnlyDictionary<string, ContentPlugin> Plugins => _plugins;
        #endregion

        #region Methods.
        /// <summary>Function to add a content plugin to the service.</summary>
        /// <param name="plugin">The plugin to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugin"/> parameter is <b>null</b>.</exception>
        public void AddContentPlugin(ContentPlugin plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            if (_plugins.ContainsKey(plugin.Name))
            {
                return;
            }

            _plugins[plugin.Name] = plugin;
        }

        /// <summary>Function to clear all of the content plugins.</summary>
        public void Clear()
        {
            foreach (KeyValuePair<string, ContentPlugin> plugin in _plugins)
            {
                plugin.Value.Shutdown();
            }

            _plugins.Clear();
        }

        /// <summary>Function to retrieve a plug in that inherits or implements a specific type.</summary>
        /// <typeparam name="T">The type that is inherited/implemented. Must inherit GorgonPlugin</typeparam>
        /// <returns>A list of plugins that implement, inherit or matches a specific type.</returns>
        public IEnumerable<T> GetByType<T>() where T : ContentPlugin => throw new NotImplementedException();

        /// <summary>Function to remove a content plugin from the service.</summary>
        /// <param name="plugin">The plugin to remove.</param>
        public void RemoveContentPlugin(ContentPlugin plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            if (!_plugins.ContainsKey(plugin.Name))
            {
                return;
            }

            _plugins.Remove(plugin.Name);
            plugin.Shutdown();            
        }
        #endregion
    }
}
