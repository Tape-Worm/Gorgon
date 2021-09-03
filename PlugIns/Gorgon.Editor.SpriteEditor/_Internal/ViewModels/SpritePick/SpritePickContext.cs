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
// Created: May 4, 2020 12:18:53 AM
// 
#endregion

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Diagnostics;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The context controller for the FX context.
    /// </summary>
    internal class SpritePickContext
        : EditorContext<SpritePickContextParameters>, ISpritePickContext
    {
        #region Variables.
        // The sprite content view model.
        private ISpriteContent _spriteContent;
        // The services from the host application.
        private IHostContentServices _hostServices;
        // The rectangle for the sprite.
        private DX.RectangleF _rect;
        // The current array index for the sprite texture.
        private int _arrayIndex;
        // The sprite texture service.
        private SpriteTextureService _textureService;
        // The image data for the sprite texture.
        private IGorgonImage _imageData;
        // The padding, in pixels, around the picked rectangle.
        private int _padding;
        #endregion

        #region Properties.
        /// <summary>Property to return the context name.</summary>
        /// <remarks>This value is used as a unique ID for the context.</remarks>
        public override string Name => PickSpriteViewer.ViewerName;

        /// <summary>
        /// Property tor return the view model for the sprite picker mask color editor.
        /// </summary>
        public ISpritePickMaskEditor SpritePickMaskEditor
        {
            get;
            private set;
        }

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

        /// <summary>
        /// Property to return the image data for the sprite texture.
        /// </summary>
        public IGorgonImage ImageData
        {
            get => _imageData;
            private set
            {
                if (_imageData == value)
                {
                    return;
                }

                OnPropertyChanging();
                IGorgonImage current = Interlocked.Exchange(ref _imageData, null);

                current?.Dispose();
                current = value;

                Interlocked.Exchange(ref _imageData, current);
                OnPropertyChanged();
            }
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
        /// Property to return the command used to show the sprite sprite picker mask color editor.
        /// </summary>
        public IEditorCommand<object> ShowSpritePickMaskEditorCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to update the array index.
        /// </summary>
        public IEditorCommand<int> UpdateArrayIndexCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to extract image data from the sprite texture.
        /// </summary>
        public IEditorAsyncCommand<object> GetImageDataCommand
        {
            get;
        }

        /// <summary>
        /// Property to set or return the padding for the picked rectangle.
        /// </summary>
        public int Padding
        {
            get => _padding;
            set
            {
                if (_padding == value)
                {
                    return;
                }

                OnPropertyChanging();
                _padding = value.Max(0).Min(16);
                OnPropertyChanged();
            }
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
                var rect = SpriteRectangle.ToRectangle();
                return string.Format(Resources.GORSPR_TEXT_SPRITE_INFO, rect.Left, rect.Top, rect.Right, rect.Bottom, rect.Width, rect.Height);
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine whether the array index for the sprite can be updated.
        /// </summary>
        /// <param name="amount">The amount of indices to update by.</param>
        /// <returns><b>true</b> if the array index can be updated, <b>false</b> if not.</returns>
        private bool CanUpdateArrayIndex(int amount)
        {
            if ((_spriteContent.Texture is null) || ((_spriteContent.CurrentPanel is not null) && (_spriteContent.CurrentPanel != SpritePickMaskEditor)))
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
        private bool CanCancel() => (_spriteContent.CurrentPanel is null) || (_spriteContent.CurrentPanel == SpritePickMaskEditor);

        /// <summary>
        /// Function to cancel the clipping operation.
        /// </summary>
        private void DoCancel()
        {
            try
            {
                _spriteContent.CurrentPanel = null;
                _spriteContent.CommandContext = null;
            }
            catch (Exception ex)
            {
                _hostServices.Log.Print("Error cancelling sprite clipping.", LoggingLevel.Verbose);
                _hostServices.Log.LogException(ex);
            }
        }

        /// <summary>
        /// Function to extract the image data from the sprite texture.
        /// </summary>        
        /// <returns>A task for asynchronous operation.</returns>
        private async Task DoExtractImageAsync()
        {
            try
            {
                IGorgonImage image = Interlocked.Exchange(ref _imageData, null);
                image?.Dispose();

                if (_spriteContent.Texture is null)
                {
                    return;
                }

                image = await _textureService.GetSpriteTextureImageDataAsync(_spriteContent.Texture);

                ImageData = image;
            }
            catch (Exception ex)
            {
                _hostServices.Log.Print("Error extracting sprite image data.", LoggingLevel.Verbose);
                _hostServices.Log.LogException(ex);
            }
        }

        /// <summary>
        /// Function to determine if the sprite picker mask color editor panel can be shown.
        /// </summary>
        /// <returns><b>true</b> if the color editor panel can be shown, <b>false</b> if not.</returns>
        private bool CanShowPickMaskEditor() => (_spriteContent.Texture is not null) && ((_spriteContent.CurrentPanel is null) || (_spriteContent.CurrentPanel == SpritePickMaskEditor));

        /// <summary>
        /// Function to show or hide the sprite picker mask color editor.
        /// </summary>
        private void DoShowPickMaskEditor()
        {
            try
            {
                if (_spriteContent.CurrentPanel is not null)
                {
                    _spriteContent.CurrentPanel = null;
                    return;
                }
                _spriteContent.CurrentPanel = SpritePickMaskEditor;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
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
        protected override void OnInitialize(SpritePickContextParameters injectionParameters)
        {
            _hostServices = injectionParameters.HostServices;
            _spriteContent = injectionParameters.SpriteContent;
            _textureService = injectionParameters.TextureService;
            SpritePickMaskEditor = injectionParameters.SpritePickMaskEditor;            

            _arrayIndex = _spriteContent.ArrayIndex;
            _rect = _spriteContent.Texture?.ToPixel(_spriteContent.TextureCoordinates).ToRectangleF() ?? DX.RectangleF.Empty;
        }

        /// <summary>Function called when the associated view is loaded.</summary>
        /// <remarks>
        ///   <para>
        /// This method should be overridden when there is a need to set up functionality (e.g. events) when the UI first loads with the view model attached. 
        /// method may be called multiple times during the lifetime of the application.
        /// </para>
        /// </remarks>
        protected override void OnLoad()
        {
            base.OnLoad();

            SpritePickMaskEditor.Load();
        }

        /// <summary>Function called when the associated view is unloaded.</summary>
        /// <remarks>This method is used to perform tear down and clean up of resources.</remarks>        
        protected override void OnUnload()
        {
            SpritePickMaskEditor.Unload();
            ImageData = null;

            base.OnUnload();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="SpritePickContext"/> class.</summary>
        public SpritePickContext()
        {
            ShowSpritePickMaskEditorCommand = new EditorCommand<object>(DoShowPickMaskEditor, CanShowPickMaskEditor);
            GetImageDataCommand = new EditorAsyncCommand<object>(DoExtractImageAsync);
            UpdateArrayIndexCommand = new EditorCommand<int>(DoUpdateArrayIndex, CanUpdateArrayIndex);
            CancelCommand = new EditorCommand<object>(DoCancel, CanCancel);
        }
        #endregion
    }
}
