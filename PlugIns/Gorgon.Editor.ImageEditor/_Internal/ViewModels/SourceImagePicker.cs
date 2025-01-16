
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: February 18, 2020 7:27:02 PM
// 

using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// The view model for picking a source image subresource
/// </summary>
internal class SourceImagePicker
    : ViewModelBase<SourceImagePickerParameters, IHostContentServices>, ISourceImagePicker
{

    // The image being imported.
    private IGorgonImage _sourceImage;
    // The name of the image.
    private string _imageName;
    // The current array index/depth slice.
    private int _currentArrayDepth;
    // The current mip map level.
    private int _mipLevel;

    /// <summary>Property to return the source image being imported.</summary>
    public IGorgonImage SourceImage
    {
        get => _sourceImage;
        set
        {
            if (_sourceImage == value)
            {
                return;
            }

            OnPropertyChanging();
            _sourceImage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the name of the image.
    /// </summary>
    public string ImageName
    {
        get => _imageName ?? string.Empty;
        set
        {
            if (string.Equals(_imageName, value, StringComparison.CurrentCulture))
            {
                return;
            }

            OnPropertyChanging();
            _imageName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the number of array indices.
    /// </summary>
    public int ArrayCount => _sourceImage is null ? 0 : _sourceImage.ImageType == ImageDataType.Image3D ? MipDepth : _sourceImage.ArrayCount;

    /// <summary>
    /// Property to return the number of mip levels.
    /// </summary>
    public int MipCount => _sourceImage is null ? 0 : _sourceImage.MipCount;

    /// <summary>
    /// Property to return the width of the image at the current mip level.
    /// </summary>
    public int MipWidth => _sourceImage is null ? 0 : (_sourceImage.Width >> _mipLevel).Max(1);

    /// <summary>
    /// Property to return the height of the image at the current mip level.
    /// </summary>
    public int MipHeight => _sourceImage is null ? 0 : (_sourceImage.Height >> _mipLevel).Max(1);

    /// <summary>
    /// Property to return the depth of the 3D image at the current mip level.
    /// </summary>
    public int MipDepth => _sourceImage is null ? 0 : (_sourceImage.GetDepthCount(_mipLevel)).Max(1);

    /// <summary>
    /// Property to return the current array index (for 2D images), or the current depth slice (for 3D images).
    /// </summary>
    public int CurrentArrayIndexDepthSlice
    {
        get => _sourceImage is null
                ? 0
                : (_currentArrayDepth.Max(0).Min(_sourceImage.ImageType == ImageDataType.Image3D ? _sourceImage.GetDepthCount(_mipLevel) - 1 : _sourceImage.ArrayCount - 1));
        set
        {
            if ((_currentArrayDepth == value) || (_sourceImage is null))
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
        get => _sourceImage is null ? 0 : _mipLevel.Max(0).Min(_sourceImage.MipCount - 1);
        set
        {
            if ((_mipLevel == value) || (_sourceImage is null))
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
    protected override void OnInitialize(SourceImagePickerParameters injectionParameters)
    {
    }

    /// <summary>Function called when the associated view is loaded.</summary>
    /// <remarks>
    ///   <para>
    /// This method should be overridden when there is a need to set up functionality (e.g. events) when the UI first loads with the view model attached. Unlike the <see cref="M:Gorgon.Editor.UI.ViewModelBase`2.Initialize(`0)"/> method, this
    /// method may be called multiple times during the lifetime of the application.
    /// </para>
    ///   <para>
    /// Anything that requires tear down should have their tear down functionality in the accompanying <see cref="ViewModelBase.OnUnload"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="ViewModelBase.Initialize" />
    /// <seealso cref="ViewModelBase.OnUnload" />
    protected override void OnLoad()
    {
        base.OnLoad();

        CurrentArrayIndexDepthSlice = 0;
        CurrentMipLevel = 0;
    }

    /// <summary>Function called when the associated view is unloaded.</summary>
    /// <remarks>This method is used to perform tear down and clean up of resources.</remarks>
    /// <seealso cref="ViewModelBase.OnLoad" />
    protected override void OnUnload()
    {
        CurrentArrayIndexDepthSlice = 0;
        CurrentMipLevel = 0;

        base.OnUnload();
    }
}
