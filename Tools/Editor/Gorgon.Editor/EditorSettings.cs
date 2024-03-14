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
// Created: August 26, 2018 8:19:04 PM
// 
#endregion

using System.Collections.Generic;
using System.Windows.Forms;
using Gorgon.Editor.ProjectData;
using DX = SharpDX;

namespace Gorgon.Editor;

/// <summary>
/// The settings used by the editor.
/// </summary>
internal class EditorSettings
{
    /// <summary>
    /// Property to set or return the window boundaries.
    /// </summary>
    public DX.Rectangle? WindowBounds
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the path used for the application plug ins.
    /// </summary>
    public string PlugInPath
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the last file open/save path.
    /// </summary>
    public string LastOpenSavePath
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the previous project working directory.
    /// </summary>
    public string LastProjectWorkingDirectory
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the last window state.
    /// </summary>
    public int WindowState
    {
        get;
        set;
    } = (int)FormWindowState.Maximized;

    /// <summary>
    /// Property to return the list of recent file items.
    /// </summary>
    public List<RecentItem> RecentFiles
    {
        get;
        private set;
    } = [];

    /// <summary>
    /// Property to set or return the flag to indicate that the file explorer is visible.
    /// </summary>
    public bool ShowFileExplorer
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Property to set or return the flag to indicate that the content preview is visible.
    /// </summary>
    public bool ShowContentPreview
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Property to set or return the distance between the main content panel, and the file explorer.
    /// </summary>
    /// <remarks>
    /// This value is a percentage value between 1 - 99.
    /// </remarks>
    public int SplitMainDistance
    {
        get;
        set;
    } = 80;

    /// <summary>
    /// Property to set or return the distance between the file explorer and the preview panel.
    /// </summary>
    /// <remarks>
    /// This value is a percentage value between 1 - 99.
    /// </remarks>
    public int SplitPreviewDistance
    {
        get;
        set;
    } = 60;

    /// <summary>
    /// Property to set or return the distance between the directory listing and the file listing in the file explorer panel.
    /// </summary>
    public int SplitDirFileDistance
    {
        get;
        set;
    } = 40;
}
