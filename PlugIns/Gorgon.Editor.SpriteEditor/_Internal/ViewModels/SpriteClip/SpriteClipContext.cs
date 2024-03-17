
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
// Created: May 4, 2020 12:18:53 AM
// 


using Gorgon.Diagnostics;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The context controller for the sprite clipping context
/// </summary>
internal class SpriteClipContext
    : EditorContext<SpriteClipContextParameters>, ISpriteClipContext
{

    /// <summary>
    /// The name of the viewer.
    /// </summary>
    public const string ViewerName = "ContextSpriteClip";



    // The sprite content view model.
    private ISpriteContent _spriteContent;
    // The services from the host application.
    private IHostContentServices _hostServices;
    // The rectangle for the sprite.
    private DX.RectangleF _rect;
    // The current array index for the sprite texture.
    private int _arrayIndex;
    // The size for the fixed width/height sprite clipping.
    private DX.Size2F? _fixedSize;



    /// <summary>Property to return the context name.</summary>
    /// <remarks>This value is used as a unique ID for the context.</remarks>
    public override string Name => ViewerName;

    /// <summary>
    /// Property to set or return the command used to apply the updated clipping coordinates.
    /// </summary>
    public IEditorCommand<object> ApplyCommand
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the command used to cancel the sprite clipping operation.
    /// </summary>
    public IEditorCommand<object> CancelCommand
    {
        get;
    }

    /// <summary>Property to set or return the rectangle representing the sprite.</summary>
    public DX.RectangleF SpriteRectangle
    {
        get => _rect;
        set
        {
            if (_rect == value)
            {
                return;
            }

            OnPropertyChanging();
            NotifyPropertyChanging(nameof(ISpriteInfo.SpriteInfo));

            _rect = value;
            OnPropertyChanged();
            NotifyPropertyChanged(nameof(ISpriteInfo.SpriteInfo));
        }
    }

    /// <summary>
    /// Property to return the size of the fixed width and height for sprite clipping.
    /// </summary>
    public DX.Size2F? FixedSize
    {
        get => _fixedSize;
        private set
        {
            if (value == _fixedSize)
            {
                return;
            }

            OnPropertyChanging();
            _fixedSize = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the current texture array index.
    /// </summary>
    public int ArrayIndex
    {
        get => _arrayIndex;
        private set
        {
            if (_arrayIndex == value)
            {
                return;
            }

            OnPropertyChanging();
            _arrayIndex = value;
            OnPropertyChanged();
        }
    }


    /// <summary>
    /// Property to return the command used to update the array index.
    /// </summary>
    public IEditorCommand<int> UpdateArrayIndexCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to set the sprite size to the full size of the sprite texture.
    /// </summary>
    public IEditorCommand<object> FullSizeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to enable or disable fixed size clipping.
    /// </summary>
    public IEditorCommand<DX.Size2F?> FixedSizeCommand
    {
        get;
    }

    /// <summary>Property to return whether the currently loaded texture supports array changes.</summary>
    bool ISpriteInfo.SupportsArrayChange => (_spriteContent.Texture is not null) && (_spriteContent.ArrayCount > 1);

    /// <summary>Property to return the total number of array indices in the sprite texture.</summary>
    int ISpriteInfo.ArrayCount => _spriteContent.ArrayCount;

    /// <summary>Property to return information about the sprite.</summary>
    string ISpriteInfo.SpriteInfo
    {
        get
        {
            DX.Rectangle rect = SpriteRectangle.ToRectangle();
            return string.Format(Resources.GORSPR_TEXT_SPRITE_INFO, rect.Left, rect.Top, rect.Right, rect.Bottom, rect.Width, rect.Height);
        }
    }



    /// <summary>
    /// Function to determine whether the array index for the sprite can be updated.
    /// </summary>
    /// <param name="amount">The amount of indices to update by.</param>
    /// <returns><b>true</b> if the array index can be updated, <b>false</b> if not.</returns>
    private bool CanUpdateArrayIndex(int amount)
    {
        if ((_spriteContent.Texture is null) || (_spriteContent.CurrentPanel is not null))
        {
            return false;
        }

        int nextIndex = _arrayIndex + amount;

        return (nextIndex < _spriteContent.Texture.Texture.ArrayCount)
            && (nextIndex >= 0);
    }

    /// <summary>
    /// Function to update the array index for the sprite texture.
    /// </summary>
    /// <param name="amount">The amount of indices to update by.</param>
    private void DoUpdateArrayIndex(int amount)
    {
        _hostServices.BusyService.SetBusy();

        try
        {
            ArrayIndex += amount;
        }
        catch (Exception ex)
        {
            _hostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
        }
        finally
        {
            _hostServices.BusyService.SetIdle();
        }
    }

    /// <summary>
    /// Function to determine if the operation can be cancelled.
    /// </summary>
    /// <returns><b>true</b> if the operation can be cancelled, <b>false</b> if not.</returns>
    private bool CanCancel() => _spriteContent.CurrentPanel is null;

    /// <summary>
    /// Function to cancel the clipping operation.
    /// </summary>
    private void DoCancel()
    {
        try
        {
            _spriteContent.CommandContext = null;
        }
        catch (Exception ex)
        {
            _hostServices.Log.Print("Error cancelling sprite clipping.", LoggingLevel.Verbose);
            _hostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the sprite can be set to full size or not.
    /// </summary>
    /// <returns><b>true</b> if the sprite can be set to size of the texture, or <b>false</b> if not.</returns>
    private bool CanFullSize()
    {
        if ((_spriteContent.Texture is null) || (_spriteContent.CurrentPanel is not null))
        {
            return false;
        }

        DX.RectangleF fullCoords = new(0, 0, _spriteContent.Texture.Width, _spriteContent.Texture.Height);

        return !SpriteRectangle.Equals(ref fullCoords);
    }

    /// <summary>
    /// Function to update the sprite to set its dimensions to match the full size of the sprite texture.
    /// </summary>
    private void DoFullSize()
    {
        _hostServices.BusyService.SetBusy();

        try
        {
            SpriteRectangle = new DX.RectangleF(0, 0, _spriteContent.Texture.Width, _spriteContent.Texture.Height);
        }
        catch (Exception ex)
        {
            _hostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
        }
        finally
        {
            _hostServices.BusyService.SetIdle();
        }
    }

    /// <summary>
    /// Function to determine if fixed size clipping is available.
    /// </summary>
    /// <param name="size">The size of the fixed clip area.</param>
    /// <returns><b>true</b> if fixed clipping is available, <b>false</b> if not.</returns>
    private bool CanUseFixedSize(DX.Size2F? size) => ((_spriteContent.Texture is not null) && (_spriteContent.CurrentPanel is null));

    /// <summary>
    /// Function to enable or disable fixed size clipping.
    /// </summary>
    /// <param name="size">The size to use, or <b>null</b> to disable.</param>
    private void DoUseFixedSize(DX.Size2F? size)
    {
        try
        {
            FixedSize = size;
        }
        catch (Exception ex)
        {
            _hostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
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
    protected override void OnInitialize(SpriteClipContextParameters injectionParameters)
    {
        _hostServices = injectionParameters.HostServices;
        _spriteContent = injectionParameters.SpriteContent;

        _arrayIndex = _spriteContent.ArrayIndex;
        _rect = _spriteContent.Texture?.ToPixel(_spriteContent.TextureCoordinates).ToRectangleF() ?? DX.RectangleF.Empty;
    }



    /// <summary>Initializes a new instance of the <see cref="SpriteClipContext"/> class.</summary>
    public SpriteClipContext()
    {
        UpdateArrayIndexCommand = new EditorCommand<int>(DoUpdateArrayIndex, CanUpdateArrayIndex);
        FullSizeCommand = new EditorCommand<object>(DoFullSize, CanFullSize);
        FixedSizeCommand = new EditorCommand<DX.Size2F?>(DoUseFixedSize, CanUseFixedSize);
        CancelCommand = new EditorCommand<object>(DoCancel, CanCancel);
    }

}
