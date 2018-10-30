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
// Created: October 29, 2018 1:04:03 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Plugins;
using Gorgon.Plugins;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// Provides access to the various content specific plugins in the application.
    /// </summary>
    public interface IContentPluginService
    {
        #region Properties.
        /// <summary>
        /// Property to return the list of content plugins loaded in to the application.
        /// </summary>
        IReadOnlyDictionary<string, ContentPlugin> Plugins
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve a plug in that inherits or implements a specific type.
        /// </summary>
        /// <typeparam name="T">The type that is inherited/implemented. Must inherit <see cref="GorgonPlugin"/></typeparam>
        /// <returns>A list of plugins that implement, inherit or matches a specific type.</returns>
        IEnumerable<T> GetByType<T>() where T : ContentPlugin;
        #endregion
    }
}
