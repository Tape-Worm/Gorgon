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
// Created: January 5, 2019 1:48:36 PM
// 
#endregion

using Gorgon.Editor.Content;
using Gorgon.Editor.Services;
using Gorgon.Graphics.Imaging.Codecs;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// Dialog serivce used for retrieving paths for exporting image data.
/// </summary>
internal interface IExportImageDialogService
    : IFileDialogService
{
    /// <summary>
    /// Property to set or return the content image file being exported.
    /// </summary>
    IContentFile ContentFile
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the codec used for exporting.
    /// </summary>
    IGorgonImageCodec SelectedCodec
    {
        get;
        set;
    }
}
