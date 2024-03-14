
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
// Created: September 27, 2018 10:19:08 PM
// 


namespace Gorgon.Editor.ViewModels;

/// <summary>
/// The type of operation being performed when copying/moving data
/// </summary>
[Flags()]
internal enum CopyMoveOperation
{
    /// <summary>
    /// Unknown operation.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Copy operation.
    /// </summary>
    Copy = 1,
    /// <summary>
    /// Move operation.
    /// </summary>
    Move = 2
}

/// <summary>
/// Information used for moving/copying directories
/// </summary>
internal interface IDirectoryCopyMoveData
{
    /// <summary>
    /// Property to return the path of the directory being copied/moved.
    /// </summary>
    string SourceDirectory
    {
        get;
    }

    /// <summary>
    /// Property to return the path of the directory that is the target for the copy/move operation.
    /// </summary>
    string DestinationDirectory
    {
        get;
    }

    /// <summary>
    /// Property to return the type of operation to be performed.
    /// </summary>
    CopyMoveOperation Operation
    {
        get;
    }

    /// <summary>
    /// Property to set to return whether any directories or files were copied.
    /// </summary>
    bool DirectoriesCopied
    {
        get;
        set;
    }
}
