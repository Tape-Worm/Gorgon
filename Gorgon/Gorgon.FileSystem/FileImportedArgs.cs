#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: January 25, 2020 10:56:15 AM
// 
#endregion

using System;

namespace Gorgon.IO;

/// <summary>
/// Event arguments for the <see cref="IGorgonFileSystemWriter{T}.FileImported"/>.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="FileImportedArgs"/> class.</remarks>
/// <param name="physicalFilePath">The physical file path to the file being imported.</param>
/// <param name="virtualFile">The virtual file representing the physical file that was imported into the file system.</param>
public class FileImportedArgs(string physicalFilePath, IGorgonVirtualFile virtualFile)
        : EventArgs
{
    #region Properties.
    /// <summary>
    /// Property to return the physical file path of the file being imported.
    /// </summary>
    public string PhysicalFilePath
    {
        get;
    } = physicalFilePath;

    /// <summary>
    /// Property to return the virtual file representing the imported physical file.
    /// </summary>
    public IGorgonVirtualFile VirtualFile
    {
        get;
    } = virtualFile;

    #endregion
    #region Constructor.
    #endregion
}
