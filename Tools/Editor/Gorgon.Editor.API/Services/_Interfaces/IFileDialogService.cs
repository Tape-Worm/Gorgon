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
// Created: September 24, 2018 12:45:12 PM
// 
#endregion

namespace Gorgon.Editor.Services;

/// <summary>
/// Provides a service that displays a file open or save dialog.
/// </summary>
public interface IFileDialogService
{
    #region Properties.
    /// <summary>
    /// Property to set or return a file filter.
    /// </summary>
    string FileFilter
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the initial directory.
    /// </summary>
    DirectoryInfo InitialDirectory
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the initial file path to use.
    /// </summary>
    string InitialFilePath
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the title for the dialog.
    /// </summary>
    string DialogTitle
    {
        get;
        set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to retrieve a single file name.
    /// </summary>
    /// <returns>The selected file path, or <b>null</b> if cancelled.</returns>
    string GetFilename();
    #endregion
}
