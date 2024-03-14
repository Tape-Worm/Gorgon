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
// Created: December 10, 2018 9:31:34 PM
// 
#endregion

using System.Collections.Generic;
using Gorgon.IO;

namespace Gorgon.Editor.ProjectData;

/// <summary>
/// Data to pass to a file copy job.
/// </summary>
internal class FileCopyJob
{
    /// <summary>
    /// Property to return the buffer used to read the data.
    /// </summary>
    public byte[] ReadBuffer
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the list of files to decompress.
    /// </summary>
    public List<IGorgonVirtualFile> Files
    {
        get;
    } = [];
}
