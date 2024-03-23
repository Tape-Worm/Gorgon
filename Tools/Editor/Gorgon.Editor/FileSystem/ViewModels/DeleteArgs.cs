
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: November 18, 2018 12:04:42 AM
// 

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// The arguments to pass to the <see cref="IFileExplorer.DeleteDirectoryCommand"/>, or the <see cref="IFileExplorer.DeleteFileCommand"/>
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="DeleteArgs"/> class.</remarks>
/// <param name="id">The ID of the directory or file to delete.</param>
internal class DeleteArgs(string id)
        : EventArgs
{
    /// <summary>
    /// Property to return the ID of the directory or file to delete.
    /// </summary>
    public string DeleteID
    {
        get;
    } = id;

    /// <summary>
    /// Property to set or return whether any items were deleted or not.
    /// </summary>
    public bool ItemsDeleted
    {
        get;
        set;
    }
}
