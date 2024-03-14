
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: December 4, 2019 10:34:23 AM
// 



namespace Gorgon.Editor.ViewModels;

/// <summary>
/// The arguments to pass to the <see cref="IDirectory.GetSizeInBytesCommand"/>
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="GetSizeInBytesCommandArgs"/> class.</remarks>
/// <param name="recursive"><b>true</b> to retrieve the size for all files plus all files within any subdirectories, <b>false</b> to only get the size of the files within the directory.</param>
internal class GetSizeInBytesCommandArgs(bool recursive)
{
    /// <summary>
    /// Property to return the flag that indicates the directory should recursively get the size.
    /// </summary>
    public bool Recursive
    {
        get;
    } = recursive;

    /// <summary>
    /// Property to set or return the total size, in bytes, of the files (and, optionally, files in any subdirectories) contained within this directory.
    /// </summary>
    public long SizeInBytes
    {
        get;
        set;
    }
}
