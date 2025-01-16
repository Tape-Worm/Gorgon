
// 
// Gorgon
// Copyright (C) 2015 Michael Winsor
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
// Created: Sunday, September 20, 2015 12:06:21 AM
// 

namespace Gorgon.IO.FileSystem.Providers;

/// <summary>
/// A listing of directories and files present in the physical file system
/// </summary>
/// <param name="Directories">The physical and virtual paths for the directories.</param>
/// <param name="Files">The list of <see cref="IGorgonPhysicalFileInfo"/> files.</param>
/// <remarks>
/// <para>
/// Implementors of the <see cref="GorgonFileSystemProvider"/> plug-in will return this type when enumerating directories and files from the physical file system. Gorgon will use this information to 
/// populate the <see cref="IGorgonFileSystem"/> object with <see cref="IGorgonVirtualDirectory"/> and <see cref="IGorgonVirtualFile"/> entries
/// </para>
/// <para>
/// Directories are returned as a list of tuples containing the physical path and the virtual path for the directory. Files are returned as a list of <see cref="IGorgonPhysicalFileInfo"/> objects.
/// </para>
/// </remarks>
public record GorgonPhysicalFileSystemData(IReadOnlyList<(string PhysicalPath, string VirtualPath)> Directories, IReadOnlyList<IGorgonPhysicalFileInfo> Files);