﻿
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
// Created: August 27, 2018 12:52:19 AM
// 

using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// A new project view model
/// </summary>
internal interface INewProject
    : IViewModel
{
    /// <summary>
    /// Property to return the title of the project.
    /// </summary>
    string Title
    {
        get;
    }

    /// <summary>
    /// Property to return the workspace path.
    /// </summary>
    DirectoryInfo WorkspacePath
    {
        get;
    }

    /// <summary>
    /// Property to return the available RAM, in bytes.
    /// </summary>
    ulong AvailableRam
    {
        get;
    }

    /// <summary>
    /// Property to return the available drive space on the drive that hosts the working folder.
    /// </summary>
    ulong AvailableDriveSpace
    {
        get;
    }

    /// <summary>
    /// Property to set or return the active GPU name.
    /// </summary>
    string GPUName
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the reason that a workspace path may be invalid.
    /// </summary>
    string InvalidPathReason
    {
        get;
    }

    /// <summary>
    /// Property to set or return the currently active project.
    /// </summary>
    IProjectEditor CurrentProject
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the command to execute when the project should be created.
    /// </summary>
    IEditorCommand<object> CreateProjectCommand
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the command to execute when the project workspace is set.
    /// </summary>
    IEditorCommand<SetProjectWorkspaceArgs> SetProjectWorkspaceCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to execute when the project workspace should be selected.
    /// </summary>
    IEditorCommand<object> SelectProjectWorkspaceCommand
    {
        get;
    }
}
