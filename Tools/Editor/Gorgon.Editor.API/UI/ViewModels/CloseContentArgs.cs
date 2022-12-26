#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: January 4, 2019 10:58:00 AM
// 
#endregion

using System.ComponentModel;

namespace Gorgon.Editor.UI;

/// <summary>
/// Arguments for the <see cref="IEditorContent.CloseContentCommand"/>.
/// </summary>
public class CloseContentArgs
    : CancelEventArgs
{
    #region Properties.
    /// <summary>
    /// Property to return whether or not to check the content for changes prior to closing.
    /// </summary>
    public bool CheckChanges
    {
        get;
    }
    #endregion

    #region Constructor.

    /// <summary>Initializes a new instance of the <see cref="CloseContentArgs"/> class.</summary>
    /// <param name="checkForChanges"><b>true</b> to check for changes prior to closing, <b>false</b> to skip the check and force a close.</param>
    public CloseContentArgs(bool checkForChanges) => CheckChanges = checkForChanges;
    #endregion
}
