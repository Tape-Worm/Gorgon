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
// Created: December 23, 2018 1:59:47 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Parameters for the <see cref="IContentPreviewVm"/> view model.
    /// </summary>
    internal class ContentPreviewVmParameters
        : ViewModelCommonParameters
    {
        #region Variables.

        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the file explorer view model.
        /// </summary>
        public IFileExplorerVm FileExplorer
        {
            get;
        }
        #endregion

        #region Methods.

        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ViewModels.ContentPreviewVmParameters"/> class.</summary>
        /// <param name="fileExplorer">The file explorer view model.</param>
        /// <param name="viewModelFactory">The view model factory for creating view models.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public ContentPreviewVmParameters(IFileExplorerVm fileExplorer, ViewModelFactory viewModelFactory)
            : base(viewModelFactory)
        {            
            FileExplorer = fileExplorer ?? throw new ArgumentNullException(nameof(fileExplorer));
        }
        #endregion
    }
}
