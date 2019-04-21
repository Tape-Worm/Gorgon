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
// Created: February 21, 2019 12:00:20 AM
// 
#endregion

using System;
using System.Collections.Generic;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// The files dragged in from explorer into a view.
    /// </summary>
    public class ExplorerFilesDragData
        : IExplorerFilesDragData
    {
        #region Properties.
        /// <summary>Property to return the list of files being imported from explorer.</summary>
        public IReadOnlyList<string> Files
        {
            get;
        }

        /// <summary>Property to set or return whether to cancel the drag/drop operation.</summary>
        public bool Cancel
        {
            get;
            set;
        }
        #endregion

        #region Methods.

        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="ExplorerFilesDragData"/> class.</summary>
        /// <param name="files">The files.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="files"/> parameter is <strong>null</strong>.</exception>
        public ExplorerFilesDragData(IReadOnlyList<string> files) => Files = files ?? throw new ArgumentNullException(nameof(files));
        #endregion
    }
}
