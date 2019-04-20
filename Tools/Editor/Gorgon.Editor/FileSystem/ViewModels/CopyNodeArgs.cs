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
// Created: September 21, 2018 3:57:31 PM
// 
#endregion

using System;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Arguments for the 
    /// </summary>
    internal class CopyNodeArgs
        : EventArgs
    {
        #region Properties.
        /// <summary>
        /// Property to return the source node to copy.
        /// </summary>
        public IFileExplorerNodeVm Source
        {
            get;            
        }

        /// <summary>
        /// Property to return the destination node that will receive the copy.
        /// </summary>
        public IFileExplorerNodeVm Dest
        {
            get;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="CopyNodeArgs"/> class.
        /// </summary>
        /// <param name="source">The source node to copy.</param>
        /// <param name="dest">The destination node that will receive the copy.</param>
        public CopyNodeArgs(IFileExplorerNodeVm source, IFileExplorerNodeVm dest)
        {
            Source = source;
            Dest = dest;
        }
        #endregion
    }
}
