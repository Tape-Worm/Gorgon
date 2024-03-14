
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
// Created: April 20, 2019 2:21:44 PM
// 


using Gorgon.Core;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// Settings view model for image codecs
/// </summary>
internal interface ISettings
    : ISettingsCategory
{
    /// <summary>
    /// Property to return the path to the image editor to use when editing the texture.
    /// </summary>        
    string ImageEditorApplicationPath
    {
        get;
    }

    /// <summary>
    /// Property to set or return the to the directory that was last used for importing/exporting.
    /// </summary>
    string LastImportExportPath
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the last used alpha value when setting the alpha channel on an image.
    /// </summary>
    int LastAlphaValue
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the width of the picker window.
    /// </summary>
    int PickerWidth
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the height of the picker window.
    /// </summary>
    int PickerHeight
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the state of the picker window.
    /// </summary>
    int PickerWindowState
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the last used alpha value when setting the alpha channel on an image.
    /// </summary>
    GorgonRange LastAlphaRange
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the command used to update the path.
    /// </summary>
    IEditorCommand<string> UpdatePathCommand
    {
        get;
    }
}
