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
// Created: December 17, 2018 1:49:40 PM
// 
#endregion

using System;
using Gorgon.Editor.Content;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A service used to scan the files in the file system for content plugin associations.
    /// </summary>
    internal interface IFileScanService
    {
        /// <summary>
        /// Function to perform the scan used to determine whether a content file has an associated plugin or not.
        /// </summary>
        /// <param name="directory">The directory to scan.</param>
        /// <param name="contentFileManager">The file manager used to manage content file data.</param>
        /// <param name="scanProgress">The callback method used to report progress of the scan.</param>
        /// <param name="deepScan"><b>true</b> to perform a more time consuming scan, or <b>false</b> to just scan by file name extension.</param>
        /// <param name="forceScan">[Optional] <b>true</b> to force the scan, even if content metadata is already available, or <b>false</b> to skip files with content metadata already.</param>
        /// <returns><b>true</b> if the content plugin metadata was updated, <b>false</b> if not.</returns>
        bool Scan(IDirectory directory, OLDE_IContentFileManager contentFileManager, Action<string, int, int> scanProgress, bool deepScan, bool forceScan = false);
    }
}