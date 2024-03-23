
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
// Created: December 4, 2019 12:31:48 PM
// 

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// Extension method(s) for the file system
/// </summary>
internal static class FileSystemExtensions
{
    /// <summary>
    /// Function to determine if the file can be included in the file system.
    /// </summary>
    /// <param name="file">The file to evaluate.</param>
    /// <returns><b>true</b> if the file can be included, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// Only hidden and system files are excluded from the editor file system.
    /// </para>
    /// </remarks>
    public static bool IsValidFile(this FileInfo file)
    {
        FileAttributes attribs = file.Attributes;

        return (((attribs & FileAttributes.Hidden) != FileAttributes.Hidden)
            && ((attribs & FileAttributes.System) != FileAttributes.System)
            && ((attribs & FileAttributes.Directory) != FileAttributes.Directory));
    }

    /// <summary>
    /// Function to calculate the total number of bytes for all files under a directory.
    /// </summary>
    /// <param name="recursive"><b>true</b> to evaluate all files in all subdirectories, or <b>false</b> to include only this directory.</param>
    /// <param name="directory">The directory to start from.</param>
    /// <returns>The total number of bytes.</returns>
    public static long GetTotalByteCount(this IDirectory directory, bool recursive)
    {
        long result = 0;
        IEnumerable<string> files = System.IO.Directory.EnumerateFiles(directory.PhysicalPath, "*",
            recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        foreach (string filePath in files)
        {
            FileInfo fileInfo = new(filePath);
            FileAttributes attribs = fileInfo.Attributes;

            if (!IsValidFile(fileInfo))
            {
                continue;
            }

            result += fileInfo.Length;
        }

        return result;
    }
}
