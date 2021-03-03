#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: April 19, 2019 11:02:21 AM
// 
#endregion

using System;
using Gorgon.Core;

namespace Gorgon.Editor.PlugIns
{
    /// <summary>
    /// A record providing plug in state information.
    /// </summary>
    public class PlugInAssemblyState
    {
        /// <summary>
        /// Property to returnt he path to the plug in.
        /// </summary>
        public string Path
        {
            get;
        }

        /// <summary>
        /// Property to return whether the assembly was loaded.
        /// </summary>
        public bool IsAssemblyLoaded
        {
            get;
        }

        /// <summary>
        /// Property to return whether the assembly is a managed assembly or not.
        /// </summary>
        public bool IsManaged
        {
            get;
        }

        /// <summary>
        /// Property to return the reason why the assembly was not loaded.
        /// </summary>
        public string LoadFailureReason
        {
            get;
        }

        /// <summary>Initializes a new instance of the <see cref="PlugInAssemblyState"/> class.</summary>
        /// <param name="pluginAssemblyPath">The plugin assembly path.</param>
        /// <param name="loadFailure">The reason why the assembly was not loaded.</param>
        /// <param name="isManaged"><b>true</b> if the assembly DLL is managed, <b>false</b> if not.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginAssemblyPath"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrownw hen the <paramref name="pluginAssemblyPath"/> is empty.</exception>
        public PlugInAssemblyState(string pluginAssemblyPath, string loadFailure, bool isManaged)
        {
            if (pluginAssemblyPath is null)
            {
                throw new ArgumentNullException(nameof(pluginAssemblyPath));
            }

            if (string.IsNullOrWhiteSpace(pluginAssemblyPath))
            {
                throw new ArgumentEmptyException(nameof(pluginAssemblyPath));
            }

            Path = pluginAssemblyPath;
            IsAssemblyLoaded = string.IsNullOrWhiteSpace(loadFailure);
            LoadFailureReason = loadFailure ?? string.Empty;
            IsManaged = isManaged;
        }
    }
}
