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
// Created: December 7, 2019 11:56:02 AM
// 
#endregion

using System;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.PlugIns;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Parameters for the <see cref="IFile"/> view model.
    /// </summary>
    internal class FileParameters
        : ViewModelCommonParameters
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the virtual file wrapped by the view model.
        /// </summary>
        public IGorgonVirtualFile VirtualFile
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the parent directory for the file.
        /// </summary>
        public IDirectory Parent
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the metadata for the file.
        /// </summary>
        public ProjectItemMetadata Metadata
        {
            get;
            set;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FileParameters"/> class.</summary>
        /// <param name="hostServices">The services from the host application.</param>
        /// <param name="factory">The view model factory used to create this view model.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public FileParameters(IHostContentServices hostServices, ViewModelFactory factory)
            : base(hostServices, factory)
        {
        }
        #endregion
    }
}
