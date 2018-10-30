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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Plugins;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A service used to manage content plug ins for the application.
    /// </summary>
    internal interface IContentPluginManagerService
        : IContentPluginService
    {
        #region Properties.

        #endregion

        #region Methods.
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
        /// Function to clear all of the content plugins.
        /// </summary>
        void Clear();        
        #endregion
    }
}
