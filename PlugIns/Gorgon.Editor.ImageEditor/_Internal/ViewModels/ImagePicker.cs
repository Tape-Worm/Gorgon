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
// Created: February 14, 2020 9:22:52 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// View model for the image picker.
/// </summary>
internal class ImagePicker
    : ViewModelBase<ImagePickerParameters, IHostContentServices>, IImagePicker
{
    #region Variables.
    // Flag to indicate that the image picker is active.
    private bool _isActive;
    // List of import data items.
    private IReadOnlyList<ImagePickerImportData> _importData;
    // The target image file.
    private IContentFile _targetImageFile;
    // The target image data.
    private IGorgonImage _targetImage;
    private IGorgonImage _originalImage;
    // The current array index/depth slice.
    private int _currentArrayDepth;
    // The current mip map level.
    private int _mipLevel;
    // The currently selected file.
    private ImagePickerImportData _selectedFile;
    // The service used to handle image I/O.
    private IImageIOService _ioService;
    // The service used to update an image.
    private IImageUpdaterService _imageUpdater;
    // Flag to indicate that the image needs resizing, cropping or alignment.
    private bool _needsTransform;
    // Flag to indicate that the image being imported has multiple sub resources.
    private bool _multiSubResources;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the settings for cropping/resizing/aligning an imported image.
    /// </summary>
    public ICropResizeSettings CropResizeSettings
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the view model for picking subresources from the image being imported.
    /// </summary>
    public ISourceImagePicker SourcePicker
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the image editor settings.
    /// </summary>
    public ISettings Settings
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the name of the target image.
    /// </summary>
    public string TargetImageName => _targetImageFile.Name;

    /// <summary>
    /// Property to return the number of array indices or depth slices.
    /// </summary>
    public int ArrayCount => _targetImage is null ? 0 : (_targetImage.ImageType == ImageType.Image3D ? MipDepth : _targetImage.ArrayCount);

    /// <summary>
    /// Property to return the number of mip levels.
    /// </summary>
    public int MipCount => _targetImage is null ? 0 : _targetImage.MipCount;

    /// <summary>
    /// Property to return the width of the image at the current mip level.
    /// </summary>
    public int MipWidth => _targetImage is null ? 0 : (_targetImage.Width >> _mipLevel).Max(1);

    /// <summary>
    /// Property to return the height of the image at the current mip level.
    /// </summary>
    public int MipHeight => _targetImage is null ? 0 : (_targetImage.Height >> _mipLevel).Max(1);

    /// <summary>
    /// Property to return the depth of the 3D image at the current mip level.
    /// </summary>
    public int MipDepth => _targetImage is null ? 0 : (_targetImage.GetDepthCount(_mipLevel)).Max(1);

    /// <summary>Property to set or return whether the image picker is active or not.</summary>
    public bool IsActive
    {
        get => _isActive;
        private set
        {
            if (_isActive == value)
            {
                return;
            }

            OnPropertyChanging();
            _isActive = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the current array index (for 2D images), or the current depth slice (for 3D images).
    /// </summary>
    public int CurrentArrayIndexDepthSlice
    {
        get => _targetImage is null
                ? 0
                : _currentArrayDepth.Max(0).Min(_targetImage.ImageType == ImageType.Image3D ? _targetImage.GetDepthCount(_mipLevel) - 1 : _targetImage.ArrayCount - 1);
        set
        {
            if ((_currentArrayDepth == value) || (_targetImage is null))
            {
                return;
            }

            OnPropertyChanging();
            _currentArrayDepth = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the current mip map level.
    /// </summary>
    public int CurrentMipLevel
    {
        get => _targetImage is null ? 0 : _mipLevel.Max(0).Min(_targetImage.MipCount - 1);
        set
        {
            if ((_mipLevel == value) || (_targetImage is null))
            {
                return;
            }

            OnPropertyChanging();
            _mipLevel = value;
            OnPropertyChanged();

            NotifyPropertyChanging(nameof(IImagePicker.CurrentArrayIndexDepthSlice));
            NotifyPropertyChanged(nameof(IImagePicker.CurrentArrayIndexDepthSlice));
        }
    }

    /// <summary>Property to set or return the current image data to update.</summary>
    public IGorgonImage ImageData
    {
        get => _targetImage;
        private set
        {
            _targetImage?.Dispose();

            OnPropertyChanging();                
            _targetImage = value;
            OnPropertyChanged();

            NotifyPropertyChanged(nameof(ArrayCount));
            NotifyPropertyChanged(nameof(MipCount));

            CropResizeSettings.TargetImageSize = new DX.Size2(_targetImage?.Width ?? 0, _targetImage?.Height ?? 0);                
        }
    }

    /// <summary>Property to set or return the list of files being imported.</summary>
    public IReadOnlyList<ImagePickerImportData> FilesToImport
    {
        get => _importData;
        private set
        {
            if (_importData == value)
            {
                return;
            }

            OnPropertyChanging();
            _importData = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the list of changed sub resources.
    /// </summary>
    public ObservableCollection<(int arrayDepth, int mipLevel)> ChangedSubResources
    {
        get;
    } = new ObservableCollection<(int arrayDepth, int mipLevel)>();

    /// <summary>Property to return the command used to activate the image picker.</summary>
    public IEditorCommand<ActivateImagePickerArgs> ActivateCommand
    {
        get;
    }

    /// <summary>Property to return the command to deactivate the image picker.</summary>
    public IEditorCommand<object> DeactivateCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to select a file.
    /// </summary>
    public IEditorCommand<ImagePickerImportData> SelectFileCommand
    {   
        get;
    }

    /// <summary>Property to return the currently selected file.</summary>
    public ImagePickerImportData SelectedFile
    {
        get => _selectedFile;
        private set
        {
            if (value == _selectedFile)
            {
                return;
            }

            OnPropertyChanging();
            _selectedFile =value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the command to restore a changed subresource.
    /// </summary>
    public IEditorCommand<object> RestoreCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to import the selected file.
    /// </summary>
    public IEditorAsyncCommand<object> ImportCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to cancel the import of a file into the target image.
    /// </summary>
    public IEditorCommand<object> CancelImportFileCommand
    {
        get;
    }

    /// <summary>
    /// Property to set or return the command to finalize the import process.
    /// </summary>
    public IEditorCommand<object> OkCommand
    {
        get;
        set;
    }

    /// <summary>Property to return whether the import image needs transformation or not.</summary>
    public bool NeedsTransformation
    {
        get => _needsTransform;
        private set
        {
            if (_needsTransform == value)
            {
                return;
            }

            OnPropertyChanging();
            _needsTransform = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return whether the image being imported has more than one sub resource (mip map/array/depth).</summary>
    public bool SourceHasMultipleSubresources
    {
        get => _multiSubResources;
        private set
        {
            if (_multiSubResources == value)
            {
                return;
            }

            OnPropertyChanging();
            _multiSubResources = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the command used to select a source image to import.</summary>
    public IEditorCommand<object> SelectSourceImageCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to update the source image.
    /// </summary>
    public IEditorCommand<object> UpdateImageCommand
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to check whether the image needs cropping or resizing.
    /// </summary>
    /// <param name="importImage">The imported image.</param>
    /// <returns><b>true</b> to cropping/resizing is required, <b>false</b> if not.</returns>
    private bool CheckForCropResize(IGorgonImage importImage)
    {
        int arrayOrDepth = CurrentArrayIndexDepthSlice;
        int width = ImageData.Buffers[CurrentMipLevel, arrayOrDepth].Width;
        int height = ImageData.Buffers[CurrentMipLevel, arrayOrDepth].Height;

        if ((width == importImage.Width) && (height == importImage.Height))
        {
            return false;
        }

        CropResizeSettings.AllowedModes = ((_targetImage.Width < importImage.Width) || (_targetImage.Height < importImage.Height)) ? (CropResizeMode.Crop | CropResizeMode.Resize) : CropResizeMode.Resize;
        if ((CropResizeSettings.CurrentMode & CropResizeSettings.AllowedModes) != CropResizeSettings.CurrentMode)
        {
            CropResizeSettings.CurrentMode = CropResizeMode.None;
        }
        else if ((CropResizeSettings.CurrentMode == CropResizeMode.None) && (((CropResizeSettings.AllowedModes) & (CropResizeMode.Crop)) == CropResizeMode.Crop))
        {
            CropResizeSettings.CurrentMode = CropResizeMode.Crop;
        }

        CropResizeSettings.ImportFile = string.IsNullOrWhiteSpace(_selectedFile?.OriginalFilePath) ? string.Empty : Path.GetFileName(_selectedFile.OriginalFilePath);
        CropResizeSettings.TargetImageSize = new DX.Size2(_targetImage.Width, _targetImage.Height);
        CropResizeSettings.ImportImage = importImage;
        NeedsTransformation = true;

        return true;
    }

    /// <summary>
    /// Function to cancel the import of an image into the target image.
    /// </summary>
    private void DoCancelSourceImageImport()
    {
        try
        {
            NeedsTransformation = false;
            SourceHasMultipleSubresources = false;

            CropResizeSettings.ImportImage?.Dispose();
            CropResizeSettings.ImportImage = null;
            SourcePicker.SourceImage = null;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("ERROR: Error cancelling the resize/crop operation.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to copy the imported image into the target image.
    /// </summary>
    /// <param name="imageToCopy">The imported image to copy.</param>
    /// <param name="imageAlignment">The alignment of the image if it's not resized.</param>
    /// <param name="mode">The mode to apply to the image data.</param>
    /// <param name="imageFilter">The image filter to use when resizing.</param>
    /// <param name="preserveAspect"><b>true</b> to keep the aspect ratio when resizing, <b>false</b> to ignore.</param>
    private void UpdateTargetImage(IGorgonImage imageToCopy, Alignment imageAlignment, CropResizeMode mode, ImageFilter imageFilter, bool preserveAspect)
    {
        try
        {
            if (imageToCopy.Format != _targetImage.Format)
            {
                imageToCopy.BeginUpdate()
                           .ConvertToFormat(_targetImage.Format)
                           .EndUpdate();
            }

            var newSize = new DX.Size2(MipWidth, MipHeight);

            NotifyPropertyChanging(nameof(ImageData));

            switch (mode)
            {
                case CropResizeMode.Crop:
                    _imageUpdater.CropTo(imageToCopy, newSize, imageAlignment);
                    break;
                case CropResizeMode.Resize:
                    _imageUpdater.Resize(imageToCopy, newSize, imageFilter, preserveAspect);
                    break;
            }

            _imageUpdater.CopyTo(imageToCopy, _targetImage, CurrentMipLevel, CurrentArrayIndexDepthSlice, imageAlignment, false);

            NotifyPropertyChanged(nameof(ImageData));

            (int arrayDepth, int mipLevel) item = (CurrentArrayIndexDepthSlice, CurrentMipLevel);

            if (ChangedSubResources.Contains(item))
            {
                return;
            }

            ChangedSubResources.Add(item);
        }
        finally
        {
            // Get rid of this guy, it's no longer necessary.
            imageToCopy?.Dispose();
        }
    }

    /// <summary>
    /// Function to update the source image to fit on the target.
    /// </summary>
    private void DoUpdateImage()
    {
        IGorgonImage image = null;
        HostServices.BusyService.SetBusy();

        try
        {                
            image = CropResizeSettings.ImportImage;

            CropResizeMode mode = CropResizeSettings.CurrentMode;
            Alignment alignment = ((mode == CropResizeMode.Resize) && (!CropResizeSettings.PreserveAspect)) ? Alignment.UpperLeft : CropResizeSettings.CurrentAlignment;
            bool keepRatio = CropResizeSettings.PreserveAspect;
            ImageFilter filter = CropResizeSettings.ImageFilter;

            CropResizeSettings.ImportImage = null;
            NeedsTransformation = false;                

            UpdateTargetImage(image, alignment, mode, filter, keepRatio);
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
        }
        finally
        {
            image?.Dispose();
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>
    /// Function to select the source image.
    /// </summary>
    private void DoSelectSourceImage()
    {
        IGorgonImage newImage = null;
        IGorgonImage image = null;

        HostServices.BusyService.SetBusy();

        try
        {
            image = SourcePicker.SourceImage;

            // Turn off the interface.
            int arrayDepth = SourcePicker.CurrentArrayIndexDepthSlice;
            int mipLevel = SourcePicker.CurrentMipLevel;
            int width = SourcePicker.MipWidth;
            int height = SourcePicker.MipHeight;

            SourceHasMultipleSubresources = false;

            // Convert the image into a single slice with a single mip channel.
            newImage = new GorgonImage(new GorgonImageInfo(ImageType.Image2D, image.Format)
            {
                Width = width,
                Height = height,
                Depth = 1,
                ArrayCount = 1,
                MipCount = 1
            });

            // Copy the selected slice into a new source image.
            image.Buffers[mipLevel, arrayDepth].CopyTo(newImage.Buffers[0]);                

            SourcePicker.SourceImage = null;

            string imageName = Path.GetFileName(_selectedFile.OriginalFilePath);
            if (CheckForCropResize(newImage))
            {
                return;
            }

            UpdateTargetImage(newImage, Alignment.UpperLeft, CropResizeMode.None, ImageFilter.Point, false);                
        }
        catch (Exception ex)
        {
            newImage?.Dispose();
            HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
        }
        finally
        {
            image?.Dispose();
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>
    /// Function to determine if a subresource on the image can be restored to its original state.
    /// </summary>
    /// <returns><b>true</b> if the subresource can be restored, <b>false</b> if not.</returns>
    private bool CanRestore() => ChangedSubResources.Contains((CurrentArrayIndexDepthSlice, CurrentMipLevel));         

    /// <summary>
    /// Function to restore a subresource image to the original.
    /// </summary>
    private void DoRestore()
    {
        HostServices.BusyService.SetBusy();

        try
        {
            NotifyPropertyChanging(nameof(ImageData));
            _originalImage.Buffers[CurrentMipLevel, CurrentArrayIndexDepthSlice].CopyTo(_targetImage.Buffers[CurrentMipLevel, CurrentArrayIndexDepthSlice]);
            NotifyPropertyChanged(nameof(ImageData));

            ChangedSubResources.Remove((CurrentArrayIndexDepthSlice, CurrentMipLevel));
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
        }
        finally
        {
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>
    /// Function to determine if the selected file can be imported.
    /// </summary>
    /// <returns><b>true</b> if the file can be imported, <b>false</b> if not.</returns>
    private bool CanImport() => _selectedFile is not null;

    /// <summary>
    /// Function to perform the import of a file.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoImportAsync()
    {
        IGorgonImage imageToImport = null;

        try
        {
            ShowWaitPanel(Resources.GORIMG_TEXT_IMPORTING);

            imageToImport = await Task.Run(() =>
            {
                Stream stream = null;
                string workPath = $"/import/{Guid.NewGuid():N}";

                try
                {
                    stream = _ioService.ScratchArea.OpenStream(SelectedFile.FromFile.FullPath, FileMode.Open);
                    (IGorgonImage importImage, _, _) = _ioService.LoadImageFile(stream, workPath);

                    if (importImage.Format != _targetImage.Format)
                    {
                        if (!importImage.CanConvertToFormat(_targetImage.Format))
                        {
                            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_CONVERT_FORMAT, _targetImage.Format));
                        }                            
                    }

                    return importImage;
                }
                finally
                {
                    if (_ioService.ScratchArea.FileSystem.GetFile(workPath) is not null)
                    {
                        _ioService.ScratchArea.DeleteFile(workPath);
                    }
                    stream?.Dispose();
                }
            });

            if (imageToImport is null)
            {
                return;
            }

            // If the image we're importing has multiple sub resources, then allow the user to pick the sub resource to import.
            string imageName = Path.GetFileName(_selectedFile.OriginalFilePath);
            if ((imageToImport.MipCount > 1) || (imageToImport.Depth > 1) || (imageToImport.ArrayCount > 1))
            {
                SourcePicker.ImageName = imageName;
                SourcePicker.SourceImage = imageToImport;
                SourceHasMultipleSubresources = true;
                return;
            }

            if (CheckForCropResize(imageToImport))
            {
                return;
            }

            UpdateTargetImage(imageToImport, Alignment.UpperLeft, CropResizeMode.None, ImageFilter.Point, false);
            return;
            /*
            // We need to transform the image, so get data to perform the transformation.
            if (needsTransform)
            {
                CropResizeSettings.AllowedModes = ((_targetImage.Width < imageToImport.Width) || (_targetImage.Height < imageToImport.Height)) ? (CropResizeMode.Crop | CropResizeMode.Resize) : CropResizeMode.Resize;
                if ((CropResizeSettings.CurrentMode & CropResizeSettings.AllowedModes) != CropResizeSettings.CurrentMode)
                {
                    CropResizeSettings.CurrentMode = CropResizeMode.None;
                }
                else if ((CropResizeSettings.CurrentMode == CropResizeMode.None) && (((CropResizeSettings.AllowedModes) & (CropResizeMode.Crop)) == CropResizeMode.Crop))
                {
                    CropResizeSettings.CurrentMode = CropResizeMode.Crop;
                }

                CropResizeSettings.ImportFile = string.IsNullOrWhiteSpace(_selectedFile?.OriginalFilePath) ? string.Empty : Path.GetFileName(_selectedFile.OriginalFilePath);
                CropResizeSettings.TargetImageSize = new DX.Size2(_targetImage.Width, _targetImage.Height);
                CropResizeSettings.ImportImage = imageToImport;                    

                PreviewImage = imageToImport;

                NeedsTransformation = needsTransform;
                return;
            }

            NotifyPropertyChanging(nameof(ImageData));
            NotifyPropertyChanged(nameof(ImageData));*/
        }
        catch(Exception ex)
        {
            imageToImport?.Dispose();
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GORIMG_ERR_IMPORT_FAIL, _selectedFile?.OriginalFilePath));
        }
        finally
        {
            HideWaitPanel();
        }
    }

    /// <summary>
    /// Function to determine if a file can be selected or not.
    /// </summary>
    /// <param name="data">The selected file data.</param>
    /// <returns><b>true</b> if the file can be selected, <b>false</b> if not.</returns>
    private bool CanSelectFile(ImagePickerImportData data) => (!_needsTransform) && (!_multiSubResources);

    /// <summary>
    /// Function to select a file for import.
    /// </summary>
    /// <param name="data">The data for the file to import.</param>
    private void DoSelectFile(ImagePickerImportData data)
    {
        HostServices.BusyService.SetBusy();

        try
        {                
            SelectedFile = data;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GORIMG_ERR_IMPORT_FAIL, data.OriginalFilePath));
        }
        finally
        {
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>
    /// Function to determine if the image picker can be activated or not.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    /// <returns><b>true</b> if the command can be executed, <b>false</b> if not.</returns>
    private bool CanActivate(ActivateImagePickerArgs args) => (args?.FilesToImport is not null) && (args.FilesToImport.Count > 0) && (args.ImageData is not null);

    /// <summary>
    /// Function to activate the image editor.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    private void DoActivate(ActivateImagePickerArgs args)
    {
        try
        {
            FilesToImport = args.FilesToImport;
            _originalImage = args.ImageData;
            ImageData?.Dispose();
            ImageData = _originalImage.Clone();
            CurrentArrayIndexDepthSlice = args.CurrentArrayIndexDepthSlice;
            CurrentMipLevel = args.MipLevel;

            SelectedFile = FilesToImport.Count > 0 ? FilesToImport[0] : null;

            // This is a hack for optimization purposes. We don't need to keep the thumbnail data at this point (it's already on the view), so 
            // rather than keep it around wasting memory, we can dump it.
            foreach (ImagePickerImportData data in FilesToImport)
            {
                data.Thumbnail?.Dispose();
            }

            IsActive = true;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_CANNOT_OPEN_IMG_PICKER);
            args.Cancel = true;
        }
    }

    /// <summary>
    /// Function to deactivate the image picker.
    /// </summary>
    private void DoDeactivate()
    {
        try
        {
            SourceHasMultipleSubresources = false;
            NeedsTransformation = false;
            IsActive = false;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("ERROR: Cannot deactivate the image picker.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>
    ///   <para>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </para>
    ///   <para>
    /// This method is only ever called after the view model has been created, and never again during the lifetime of the view model.
    /// </para>
    /// </remarks>
    protected override void OnInitialize(ImagePickerParameters injectionParameters)
    {
        _targetImageFile = injectionParameters.File;    
        _ioService = injectionParameters.ImageServices.ImageIO;
        _imageUpdater = injectionParameters.ImageServices.ImageUpdater;
        CropResizeSettings = injectionParameters.CropResizeSettings;            
        SourcePicker = injectionParameters.SourceImagePicker;
        Settings = injectionParameters.Settings;
    }

    /// <summary>Function called when the associated view is loaded.</summary>
    /// <remarks>
    ///   <para>
    /// This method should be overridden when there is a need to set up functionality (e.g. events) when the UI first loads with the view model attached. Unlike the <see cref="ViewModelBase.Initialize"/> method, this
    /// method may be called multiple times during the lifetime of the application.
    /// </para>
    ///   <para>
    /// Anything that requires tear down should have their tear down functionality in the accompanying <see cref="Unload"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="ViewModelBase.Initialize" />
    /// <seealso cref="Unload" />
    protected override void OnLoad()
    {
        base.OnLoad();

        ChangedSubResources.Clear();
    }

    /// <summary>Function called when the associated view is unloaded.</summary>
    /// <remarks>This method is used to perform tear down and clean up of resources.</remarks>
    protected override void OnUnload()
    {
        SourcePicker.SourceImage?.Dispose();
        SourcePicker.SourceImage = null;
        CropResizeSettings.ImportImage?.Dispose();
        CropResizeSettings.ImportImage = null;
        ImageData?.Dispose();
        ImageData = null;
        ChangedSubResources.Clear();

        base.OnUnload();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="ImagePicker"/> class.</summary>
    public ImagePicker()
    {
        ActivateCommand = new EditorCommand<ActivateImagePickerArgs>(DoActivate, CanActivate);
        DeactivateCommand = new EditorCommand<object>(DoDeactivate);
        SelectFileCommand = new EditorCommand<ImagePickerImportData>(DoSelectFile, CanSelectFile);
        ImportCommand = new EditorAsyncCommand<object>(DoImportAsync, CanImport);
        CancelImportFileCommand = new EditorCommand<object>(DoCancelSourceImageImport);
        SelectSourceImageCommand = new EditorCommand<object>(DoSelectSourceImage);
        UpdateImageCommand = new EditorCommand<object>(DoUpdateImage);
        RestoreCommand = new EditorCommand<object>(DoRestore, CanRestore);
    }
    #endregion
}
