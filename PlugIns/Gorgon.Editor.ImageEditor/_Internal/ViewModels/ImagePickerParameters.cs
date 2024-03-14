#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: February 14, 2020 8:55:59 AM
// 
#endregion

using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// Parameters for the <see cref="IImagePicker"/> view model.
/// </summary>
/// <remarks>Initializes a new instance of the ContentViewModelInjectionCommon class.</remarks>
/// <param name="fileManager">The file manager for content files.</param>
/// <param name="file">The file that contains the content.</param>
/// <param name="commonServices">The common services for the application.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the required parameters are <b>null</b>.</exception>
internal class ImagePickerParameters(IContentFileManager fileManager, IContentFile file, IHostContentServices commonServices)
        : ContentViewModelInjection(fileManager, file, commonServices)
{
    #region Properties.
    /// <summary>
    /// Property to set or return the image editor services.
    /// </summary>
    public ImageEditorServices ImageServices
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the settings for cropping/resizing/aligning imported images.
    /// </summary>
    public ICropResizeSettings CropResizeSettings
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the view model for the sub resource picker for import images.
    /// </summary>
    public ISourceImagePicker SourceImagePicker
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the settings for the editor plug in.
    /// </summary>
    public ISettings Settings
    {
        get;
        set;
    }

    #endregion
}
