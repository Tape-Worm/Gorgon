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
// Created: September 19, 2018 8:25:24 PM
// 
#endregion

using System.ComponentModel;

namespace Gorgon.Editor.UI;

/// <summary>
/// Defines an interface that can handle clipboard functionality.
/// </summary>
public interface IClipboardHandler
    : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>
    /// Property to return whether the clipboard has data or not.
    /// </summary>
    bool HasData
    {
        get;
    }

    /// <summary>
    /// Property to return the command that returns the type of data present on the clipboard (if any).
    /// </summary>
    IEditorCommand<GetClipboardDataTypeArgs> GetClipboardDataTypeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to clear the clipboard.
    /// </summary>
    IEditorCommand<object> ClearCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to copy data into the clipboard.
    /// </summary>
    IEditorCommand<object> CopyDataCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to paste data from the clipboard.
    /// </summary>
    IEditorAsyncCommand<object> PasteDataCommand
    {
        get;
    }
}
