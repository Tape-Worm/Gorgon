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
// Created: February 14, 2020 9:00:09 AM
// 
#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The view model for the image picker.
    /// </summary>
    internal interface IImagePicker
        : IViewModel
    {
        /// <summary>
        /// Property to return the settings for cropping/resizing/aligning an imported image.
        /// </summary>
        ICropResizeSettings CropResizeSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for picking subresources from the image being imported.
        /// </summary>
        ISourceImagePicker SourcePicker
        {
            get;
        }

        /// <summary>
        /// Property to return the image editor settings.
        /// </summary>
        ISettings Settings
        {
            get;
        }

        /// <summary>
        /// Property to return the name of the target image.
        /// </summary>
        string TargetImageName
        {
            get;
        }

        /// <summary>
        /// Property to return whether the image picker is active or not.
        /// </summary>
        bool IsActive
        {
            get;
        }

        /// <summary>
        /// Property to return the number of array indices.
        /// </summary>
        int ArrayCount
        {
            get;
        }

        /// <summary>
        /// Property to return the number of mip levels.
        /// </summary>
        int MipCount
        {
            get;
        }

        /// <summary>
        /// Property to return the width of the image at the current mip level.
        /// </summary>
        int MipWidth
        {
            get;
        }

        /// <summary>
        /// Property to return the height of the image at the current mip level.
        /// </summary>
        int MipHeight
        {
            get;
        }

        /// <summary>
        /// Property to return the depth of the 3D image at the current mip level.
        /// </summary>
        int MipDepth
        {
            get;
        }

        /// <summary>
        /// Property to return the current array index (for 2D images), or the current depth slice (for 3D images).
        /// </summary>
        int CurrentArrayIndexDepthSlice
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the current mip map level.
        /// </summary>
        int CurrentMipLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the currently selected file.
        /// </summary>
        ImagePickerImportData SelectedFile
        {   
            get;
        }

        /// <summary>
        /// Property to return whether the import image needs to be resized, cropped or aligned.
        /// </summary>
        bool NeedsTransformation
        {
            get;
        }

        /// <summary>
        /// Property to return whether the image being imported has more than one sub resource (mip map/array/depth).
        /// </summary>
        bool SourceHasMultipleSubresources
        {
            get;
        }

        /// <summary>
        /// Property to return the current image data to update.
        /// </summary>
        IGorgonImage ImageData
        {
            get;
        }

        /// <summary>
        /// Property to return the list of files being imported.
        /// </summary>
        IReadOnlyList<ImagePickerImportData> FilesToImport
        {
            get;
        }

        /// <summary>
        /// Property to return the list of changed sub resources.
        /// </summary>
        ObservableCollection<(int arrayDepth, int mipLevel)> ChangedSubResources
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to activate the image picker.
        /// </summary>
        IEditorCommand<ActivateImagePickerArgs> ActivateCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to deactivate the image picker.
        /// </summary>
        IEditorCommand<object> DeactivateCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to select a file.
        /// </summary>
        IEditorCommand<ImagePickerImportData> SelectFileCommand
        {   
            get;
        }

        /// <summary>
        /// Property to return the command to import the selected file.
        /// </summary>
        IEditorAsyncCommand<object> ImportCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to restore a changed subresource.
        /// </summary>
        IEditorCommand<object> RestoreCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to cancel the import of a file into the target image.
        /// </summary>
        IEditorCommand<object> CancelImportFileCommand
        {
            get;    
        }

        /// <summary>
        /// Property to return the command used to select a source image to import.
        /// </summary>        
        IEditorCommand<object> SelectSourceImageCommand
        {
            get;
        }
        
        /// <summary>
        /// Property to return the command used to update the source image.
        /// </summary>
        IEditorCommand<object> UpdateImageCommand
        {
            get;
        }

        /// <summary>
        /// Property to set or return the command to finalize the import process.
        /// </summary>
        IEditorCommand<object> OkCommand
        {
            get;
            set;
        }
    }
}
