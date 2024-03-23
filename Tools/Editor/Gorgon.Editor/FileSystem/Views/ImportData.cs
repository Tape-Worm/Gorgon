
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: January 29, 2020 11:19:40 PM
// 

using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Views;

/// <summary>
/// Data used to import physical file system paths to the virtual file system
/// </summary>
internal class ImportData
    : IImportData
{
    /// <summary>Property to return the list of files/directories from the physical file system..</summary>
    public List<string> PhysicalPaths
    {
        get;
    } = [];

    /// <summary>
    /// Property to set or return the directory to import into.
    /// </summary>
    public IDirectory Destination
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the ID to the destination.
    /// </summary>
    public string DestinationDirectory => Destination?.ID ?? string.Empty;

    /// <summary>Property to set or return whether any files or directories were successfully imported.</summary>
    public bool ItemsImported
    {
        get;
        set;
    }
}
