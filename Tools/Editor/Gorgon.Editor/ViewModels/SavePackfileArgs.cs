﻿#region MIT
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
// Created: October 15, 2018 3:00:13 PM
// 
#endregion

using System.ComponentModel;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// Arguments to pass to the <see cref="IMain.SavePackFileCommand"/>.
/// </summary>
internal class SavePackFileArgs
    : CancelEventArgs
{
    /// <summary>
    /// Property to return the currently active project.
    /// </summary>
    public IProjectEditor CurrentProject
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SavePackFileArgs"/> class.
    /// </summary>
    /// <param name="currentProject">The currently active project.</param>
    public SavePackFileArgs(IProjectEditor currentProject) => CurrentProject = currentProject;
}
