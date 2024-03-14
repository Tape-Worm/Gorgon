
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
// Created: January 16, 2019 8:44:21 AM
// 


using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Editor.ImageEditor.ViewModels;

/// <summary>
/// The view model for the image cropping/resizing settings view
/// </summary>
internal class CropResizeSettings
    : HostedPanelViewModelBase<HostedPanelViewModelParameters>, ICropResizeSettings
{

    // The file being imported.
    private string _importFile;
    // The imported image.
    private IGorgonImage _importImage;
    // The size of the target image.
    private DX.Size2 _targetImageSize;
    // Flag to indicate which modes are allowed.
    private CropResizeMode _allowedModes = CropResizeMode.Crop | CropResizeMode.Resize;
    // Flag to indicate that resizing is allowed.
    private CropResizeMode _currentMode = CropResizeMode.Crop;
    // The currently selected alignment.
    private Alignment _currentAlignment = Alignment.Center;
    // Flag to indicate that the aspect ratio should be preserved.
    private bool _preserveAspect;
    // The image filter to apply when resizing.
    private ImageFilter _imageFilter = ImageFilter.Point;
    // The directory for the imported file.
    private string _importFileDirectory;



    /// <summary>Property to return whether the panel is modal.</summary>
    public override bool IsModal => true;

    /// <summary>
    /// Property to set or return whether to preserve the aspect ratio of the image being resized.
    /// </summary>
    public bool PreserveAspect
    {
        get => _preserveAspect;
        set
        {
            if (_preserveAspect == value)
            {
                return;
            }

            OnPropertyChanging();
            _preserveAspect = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the image filter to use when resizing.
    /// </summary>
    public ImageFilter ImageFilter
    {
        get => _imageFilter;
        set
        {
            if (_imageFilter == value)
            {
                return;
            }

            OnPropertyChanging();
            _imageFilter = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to set or return the file being imported.</summary>
    /// <value>The import file.</value>
    public string ImportFile
    {
        get => _importFile;
        set
        {
            if (_importFile == value)
            {
                return;
            }

            OnPropertyChanging();
            _importFile = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the directory containing the file being imported.
    /// </summary>
    public string ImportFileDirectory
    {
        get => _importFileDirectory;
        set
        {
            if (string.Equals(_importFileDirectory, value, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            OnPropertyChanging();
            _importFileDirectory = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the image being imported.
    /// </summary>
    public IGorgonImage ImportImage
    {
        get => _importImage;
        set
        {
            if (_importImage == value)
            {
                return;
            }

            OnPropertyChanging();
            _importImage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to set or return the width/height of the target image.</summary>
    public DX.Size2 TargetImageSize
    {
        get => _targetImageSize;
        set
        {
            if (value.Equals(_targetImageSize))
            {
                return;
            }

            OnPropertyChanging();
            _targetImageSize = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return which modes are allowed.
    /// </summary>
    public CropResizeMode AllowedModes
    {
        get => _allowedModes;
        set
        {
            if (_allowedModes == value)
            {
                return;
            }

            OnPropertyChanging();
            _allowedModes = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return which mode is active.
    /// </summary>
    public CropResizeMode CurrentMode
    {
        get => _currentMode;
        set
        {
            if (_currentMode == value)
            {
                return;
            }

            OnPropertyChanging();
            _currentMode = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the currently selected alignment.
    /// </summary>
    public Alignment CurrentAlignment
    {
        get => _currentAlignment;
        set
        {
            if (_currentAlignment == value)
            {
                return;
            }

            OnPropertyChanging();
            _currentAlignment = value;
            OnPropertyChanged();
        }
    }



    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </remarks>
    protected override void OnInitialize(HostedPanelViewModelParameters injectionParameters)
    {
    }

}
