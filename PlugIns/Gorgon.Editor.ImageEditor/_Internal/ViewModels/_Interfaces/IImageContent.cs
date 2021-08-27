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
// Created: November 9, 2018 3:30:20 PM
// 
#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Drawing = System.Drawing;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;

namespace Gorgon.Editor.ImageEditor.ViewModels
{
    /// <summary>
    /// The image content view model.
    /// </summary>
    internal interface IImageContent
        : IVisualEditorContent, IUndoHandler
    {
        /// <summary>
        /// Property to return the settings for the image editor plugin.
        /// </summary>
        ISettings Settings
        {
            get;
        }

        /// <summary>
        /// Property to return the image data.
        /// </summary>
        IGorgonImage ImageData
        {
            get;
        }

        /// <summary>
        /// Property to return whether mip maps are supported for the current format.
        /// </summary>
        bool MipSupport
        {
            get;
        }

        /// <summary>
        /// Property to return the list of codecs available.
        /// </summary>
        ObservableCollection<IGorgonImageCodec> Codecs
        {
            get;
        }

        /// <summary>
        /// Property to return the list of available image pixel formats (based on codec).
        /// </summary>
        ObservableCollection<BufferFormat> PixelFormats
        {
            get;
        }

        /// <summary>
        /// Property to return the current pixel format for the image.
        /// </summary>
        BufferFormat CurrentPixelFormat
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to export an image.
        /// </summary>
        IEditorCommand<IGorgonImageCodec> ExportImageCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to convert the image pixel format.
        /// </summary>
        IEditorCommand<BufferFormat> ConvertFormatCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the format support information for the current video card.
        /// </summary>
        IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> FormatSupport
        {
            get;
        }

        /// <summary>
        /// Property to return the type of image that is loaded.
        /// </summary>
        ImageType ImageType
        {
            get;
        }

        /// <summary>
        /// Property to return the number of mip maps in the image.
        /// </summary>
        int MipCount
        {
            get;
        }

        /// <summary>
        /// Property to return the number of array indices in the image.
        /// </summary>
        int ArrayCount
        {
            get;
        }

        /// <summary>
        /// Property to return the number of depth slices in the image.
        /// </summary>
        int DepthCount
        {
            get;
        }

        /// <summary>
        /// Property to set or return the current mip map level.
        /// </summary>
        int CurrentMipLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the current array index.
        /// </summary>
        int CurrentArrayIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the current depth slice.
        /// </summary>
        int CurrentDepthSlice
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the width of the image, in pixels.
        /// </summary>
        int Width
        {
            get;
        }

        /// <summary>
        /// Property to return the height of the image, in pixels.
        /// </summary>
        int Height
        {
            get;
        }

        /// <summary>
        /// Property to return whether this image is premultiplied.
        /// </summary>
        bool IsPremultiplied
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for picking images to import into the current image.
        /// </summary>
        IImagePicker ImagePicker
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the cropping/resizing settings.
        /// </summary>        
        ICropResizeSettings CropOrResizeSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the dimension editing settings.
        /// </summary>
        IDimensionSettings DimensionSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the mip map generation settings.
        /// </summary>
        IMipMapSettings MipMapSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the set alpha settings.
        /// </summary>
        IAlphaSettings AlphaSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the blur effect settings.
        /// </summary>
        IFxContext FxContext
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when changing the image type.
        /// </summary>
        IEditorCommand<ImageType> ChangeImageTypeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used import an image file into the current image as an array index or depth slice.
        /// </summary>
        IEditorAsyncCommand<float> ImportFileCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to show the image dimensions settings.
        /// </summary>
        IEditorCommand<object> ShowImageDimensionsCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to show the mip map generation settings.
        /// </summary>
        IEditorCommand<object> ShowMipGenerationCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to show the set alpha settings.
        /// </summary>
        IEditorCommand<object> ShowSetAlphaCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to show the FX items.
        /// </summary>
        IEditorCommand<object> ShowFxCommand
        {
            get;
        }

        /// <summary>
        /// Porperty to return the command used to edit the image in an external application.
        /// </summary>
        IEditorCommand<string> EditInAppCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to set the image to use premultiplied alpha.
        /// </summary>
        IEditorAsyncCommand<bool> PremultipliedAlphaCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to copy an image from the project into the current image at the selected array/depth/mip index.
        /// </summary>
        IEditorAsyncCommand<CopyToImageArgs> CopyToImageCommand
        {
            get;
        }

        /// <summary>
        /// Property to return information about the external editor.
        /// </summary>
        ref readonly (string ExePath, string FriendlyName, Drawing.Bitmap IconLarge, Drawing.Bitmap IconSmall) ExternalEditorInfo
        {
            get;
        }

        /// <summary>
        /// Property to return information about the user defined editor.
        /// </summary>
        ref readonly (string ExePath, string FriendlyName, Drawing.Bitmap IconLarge, Drawing.Bitmap IconSmall) UserEditorInfo
        {
            get;
        }
    }
}
