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
// Created: December 2, 2019 8:32:27 AM
// 
#endregion

using System;
using Gorgon.Editor.PlugIns;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Parameters for the <see cref="Directory"/> view model.
    /// </summary>
    internal class DirectoryParameters
        : ViewModelCommonParameters
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the virtual directory represented by the view model.
        /// </summary>
        public IGorgonVirtualDirectory VirtualDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the path to the directory on the physical file system that represents the root of the virtual file system.
        /// </summary>
        public string PhysicalPath
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the parent of the directory.
        /// </summary>
        public IDirectory Parent
        {
            get;
            set;
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="DirectoryParameters"/> class.</summary>
        /// <param name="hostServices">The services from the host application.</param>
        /// <param name="factory">The factory used to create view models.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the required parameters are <b>null</b>.</exception>
        public DirectoryParameters(IHostContentServices hostServices, ViewModelFactory factory)
            : base(hostServices, factory)
        {
        }
        #endregion
    }
}
