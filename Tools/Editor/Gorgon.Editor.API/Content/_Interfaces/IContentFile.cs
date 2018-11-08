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
// Created: October 30, 2018 7:11:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Plugins;

namespace Gorgon.Editor.Content
{
    /// <summary>
    /// A data structure representing a file containing content.
    /// </summary>
    public interface IContentFile
    {
        #region Properties.
        /// <summary>
        /// Property to return the path to the file.
        /// </summary>
        string Path
        {
            get;
        }

        /// <summary>
        /// Property to return the name for the file.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Property to return the extension for the file.
        /// </summary>
        string Extension
        {
            get;
        }

        /// <summary>
        /// Property to return the plugin associated with the file.
        /// </summary>
        ContentPlugin ContentPlugin
        {
            get;
        }

        /// <summary>
        /// Property to return the metadata associated with the file.
        /// </summary>
        ProjectItemMetadata Metadata
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to open the file for reading.
        /// </summary>
        /// <returns>A stream containing the file data.</returns>
        Stream OpenRead();

        // TODO: OpenWrite so we can send data back out.

        /// <summary>
        /// Function to notify that the metadata should be refreshed.
        /// </summary>
        void RefreshMetadata();
        #endregion
    }
}
