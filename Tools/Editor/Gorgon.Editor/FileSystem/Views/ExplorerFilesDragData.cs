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
// Created: September 27, 2018 10:11:34 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Views;

/// <summary>
/// Import data for importing files and directories from Windows Explorer.
/// </summary>
internal class ExplorerImportData
    : IImportData
{
    #region Properties.
    /// <summary>Property to return the list of files/directories from Windows Explorer.</summary>
    public List<string> PhysicalPaths
    {
        get;
    }

    /// <summary>
    /// Property to set or return the node that is the target for the drop operation.
    /// </summary>
    public DirectoryTreeNode TargetNode
    {
        get;
        set;
    }

    /// <summary>Property to return the ID to the virtual directory that will receive the imported files.</summary>
    public string DestinationDirectory => TargetNode?.Name ?? string.Empty;

    /// <summary>
    /// Property to set or return whether any files or directories were successfully imported.
    /// </summary>
    public bool ItemsImported
    {
        get;
        set;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="ExplorerImportData"/> class.
    /// </summary>
    /// <param name="explorerPaths">The paths to the explorer files.</param>
    /// <param name="dragOperation">The desired drag operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="explorerPaths"/> parameter is <b>null</b>.</exception>
    public ExplorerImportData(IReadOnlyList<string> explorerPaths) => PhysicalPaths = new List<string>(explorerPaths ?? throw new ArgumentNullException(nameof(explorerPaths)));
    #endregion
}
