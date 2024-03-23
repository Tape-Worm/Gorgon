
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
// Created: September 5, 2018 9:09:28 AM
// 

using System.ComponentModel;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// Arguments for the <see cref="IDirectory.RenameCommand"/>
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RenameArgs"/> class
/// </remarks>
/// <param name="oldName">The old name.</param>
/// <param name="newName">The new name.</param>
internal class RenameArgs(string oldName, string newName)
        : CancelEventArgs(false)
{
    /// <summary>
    /// Property to return the old name.
    /// </summary>
    public string OldName
    {
        get;
    } = oldName;

    /// <summary>
    /// Property to set or return the ID of the file or directory being renamed.
    /// </summary>
    public string ID
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the new name.
    /// </summary>
    public string NewName
    {
        get;
        set;
    } = newName;

}
