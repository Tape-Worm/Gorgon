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
// Created: August 29, 2018 8:26:27 PM
// 
#endregion

namespace Gorgon.UI;

/// <summary>
/// Arguments for the <see cref="GorgonFolderBrowser.FolderEntered"/> and <see cref="GorgonFolderBrowser.FolderSelected"/> events.
/// </summary>
/// <seealso cref="GorgonFolderBrowser"/>
/// <remarks>
/// Initializes a new instance of the <see cref="FolderSelectedArgs"/> class.
/// </remarks>
/// <param name="folder">The folder that was selected.</param>
public class FolderSelectedArgs(string folder)
        : EventArgs
{
    #region Properties.
    /// <summary>
    /// Property to return the path to the folder.
    /// </summary>
    /// <remarks>
    /// If this value is empty, then the selector is at the drive selection area.
    /// </remarks>
    public string FolderPath
    {
        get;
    } = folder ?? string.Empty;

    #endregion

}
