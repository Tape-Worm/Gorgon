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
// Created: September 27, 2018 10:19:08 PM
// 
#endregion

using System.Collections.Generic;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// Data used to import files/directories from the physical file system to the virtual file system.
/// </summary>
internal interface IImportData
{
    /// <summary>
    /// Property to return the list of files/directories from the physical file system..
    /// </summary>
    List<string> PhysicalPaths
    {
        get;
    }

    /// <summary>
    /// Property to return the ID to the virtual directory that will receive the imported files.
    /// </summary>
    string DestinationDirectory
    {
        get;
    }

    /// <summary>
    /// Property to set or return whether any files or directories were successfully imported.
    /// </summary>
    bool ItemsImported
    {
        get;
        set;
    }
}
