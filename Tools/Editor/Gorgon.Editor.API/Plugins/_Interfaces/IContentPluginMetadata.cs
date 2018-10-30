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
// Created: October 29, 2018 9:04:44 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Gorgon.Editor.Plugins
{
    /// <summary>
    /// Provides metadata for a plugin.
    /// </summary>
    public interface IContentPluginMetadata
    {
        #region Properties.
        /// <summary>
        /// Property to return the ID of the small icon for this plug in.
        /// </summary>
        Guid SmallIconID
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if the content plugin can open the specified file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns><b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
        bool CanOpenContent(string filePath);

        /// <summary>
        /// Function to retrieve the small icon for the content plug in.
        /// </summary>
        /// <returns>An image for the small icon.</returns>
        Image GetSmallIcon();
        #endregion
    }
}
