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
// Created: February 13, 2020 12:32:15 PM
// 
#endregion

using System;
using System.Numerics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DX = SharpDX;
using Gorgon.Graphics.Imaging.GdiPlus;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using System.Threading;
using Gorgon.Graphics;
using Gorgon.UI;
using Gorgon.Renderers;
using Gorgon.Math;
using System.Collections.Specialized;
using System.IO;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Dialog for assigning a list of images to array indices, depth slices, and mip levels.
    /// </summary>
    internal partial class FormImagePicker 
        : Form, IDataContext<IImagePicker>
    {
        #region Variables.
        // A list of images held until the form is loaded for the first time.
        private readonly List<(string, Image)> _deferredImages = new List<(string, Image)>();
        // The background texture.
        private GorgonTexture2DView _bgTextureView;
        private GorgonTexture2D _bgTexture;
        private GorgonTexture2DView _imageTexture2D;
        private GorgonTexture3DView _imageTexture3D;
        private GorgonTexture2DView _sourceTexture2D;
        private GorgonTexture3DView _sourceTexture3D;
        private GorgonTexture2DView _previewCropResize;
        // The swap chain to display the image.
        private GorgonSwapChain _imageSwapChain;
        private GorgonSwapChain _sourceSwapchain;
        // The previous idle method.
        private Func<bool> _prevIdle;
        // The pixel shaders for rendering an image.
        private GorgonPixelShader _imageShader2D;
        private GorgonPixelShader _imageShader3D;
        // The parameters for the texture viewer shader.
        private GorgonConstantBufferView _textureParameters;
        // The batch states for rendering.
        private Gorgon2DBatchState _batch2DState;
        private Gorgon2DBatchState _batch3DState;
        private Gorgon2DBatchState _batchSource3DState;
        private Gorgon2DBatchState _batchPreviewState;
        // The flag to indicate that we are resetting the selection.
        private bool _resetSelection;
        // The currently selected file.
        private ListViewItem _selectedFile;        
        // The wait panel to display when a long running operation is running.
        private readonly WaitPanelDisplay _waitPanel;
        // The progress panel to display when a long running operation is running.
        private readonly ProgressPanelDisplay _progressPanel;
        // The sprite for the preview.
        private GorgonSprite _previewSprite;
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        public IImagePicker DataContext
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return the graphics context for the application.
        /// </summary>
        public IGraphicsContext GraphicsContext
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to validate the button state on the form.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void ValidateButtons(IImagePicker dataContext)
        {
            if (dataContext == null)
            {
                ButtonOK.Enabled = ListImages.Enabled = TableArrayDepthControls.Enabled = false;                
                return;
            }

            var data = _selectedFile?.Tag as ImagePickerImportData;

            bool enableList = dataContext.SelectFileCommand?.CanExecute(data) ?? false;
            PanelDialogButtons.Visible = LabelImportDesc.Visible = ListImages.Visible = enableList;
            TableArrayDepthControls.Visible = enableList;            

            LabelAnchor.Visible = AlignmentPicker.Visible = (dataContext.CropResizeSettings.CurrentMode == CropResizeMode.Crop) 
                                                            || (dataContext.CropResizeSettings.CurrentMode == CropResizeMode.None);
            LabelResizeAnchor.Visible = AlignmentResize.Visible = (dataContext.CropResizeSettings.CurrentMode == CropResizeMode.Resize)
                                                            && (dataContext.CropResizeSettings.PreserveAspect);
            LabelImageFilter.Visible = PanelFilter.Visible = !LabelAnchor.Visible;
            
            ButtonPrevArrayDepth.Enabled = dataContext.CurrentArrayIndexDepthSlice > 0;
            ButtonPrevMip.Enabled = dataContext.CurrentMipLevel > 0;
            ButtonNextArrayDepth.Enabled = dataContext.CurrentArrayIndexDepthSlice < dataContext.ArrayCount - 1;
            ButtonNextMip.Enabled = dataContext.CurrentMipLevel < dataContext.MipCount - 1;
            ButtonImport.Enabled = dataContext.ImportCommand?.CanExecute(null) ?? false;
            ButtonRestore.Enabled = dataContext.RestoreCommand?.CanExecute(null) ?? false;
            ButtonOK.Enabled = dataContext.OkCommand?.CanExecute(null) ?? false;

            ISourceImagePicker sourcePicker = dataContext.SourcePicker;

            if (sourcePicker == null)
            {
                ButtonPrevSrcArray.Enabled = ButtonNextSrcArray.Enabled = ButtonPrevSrcMip.Enabled = ButtonNextSrcMip.Enabled = false;
                ButtonSrcImport.Enabled = false;
                return;
            }

            ButtonSrcImport.Enabled = true;
            ButtonPrevSrcArray.Enabled = sourcePicker.CurrentArrayIndexDepthSlice > 0;
            ButtonNextSrcArray.Enabled = sourcePicker.CurrentArrayIndexDepthSlice < sourcePicker.ArrayCount - 1;
            ButtonPrevSrcMip.Enabled = sourcePicker.CurrentMipLevel > 0;
            ButtonNextSrcMip.Enabled = sourcePicker.CurrentMipLevel < sourcePicker.MipCount - 1;
        }

        /// <summary>
        /// Function to set the UI text for the image resize/crop panel.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void SetUIText(ICropResizeSettings dataContext)
        {
            if (dataContext == null)
            {
                return;
            }

            LabelDesc.Text = string.Format(Resources.GORIMG_RESIZE_CROP_DESC, dataContext.ImportFile);
            LabelImportImageDimensions.Text = string.Format(Resources.GORIMG_TEXT_RESIZE_CROP_IMPORT_SIZE, dataContext.ImportImage?.Width ?? 0, dataContext.ImportImage?.Height ?? 0);
            LabelTargetImageDimensions.Text = string.Format(Resources.GORIMG_TEXT_RESIZE_CROP_IMAGE_SIZE, DataContext.MipWidth, DataContext.MipHeight);
            RadioCrop.Text = ((dataContext.AllowedModes & CropResizeMode.Crop) == CropResizeMode.Crop) ?
                string.Format(Resources.GORIMG_TEXT_CROP_TO, DataContext.MipWidth, DataContext.MipHeight)
                : Resources.GORIMG_TEXT_ALIGN_TO;
            RadioResize.Text = string.Format(Resources.GORIMG_TEXT_RESIZE_TO, DataContext.MipWidth, DataContext.MipHeight);
        }

        /// <summary>
        /// Function to set the UI text for the various panels.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void SetUIText(IImagePicker dataContext)
        {
            if (dataContext?.ImageData == null)
            {
                return;
            }

            Text = string.Format(Resources.GORIMG_CAPTION_IMG_PICKER, dataContext.TargetImageName);
            LabelImportDesc.Text = string.Format(Resources.GORIMG_TEXT_IMG_PICKER_INSTRUCTION, dataContext.TargetImageName.Ellipses(65));
            LabelArrayDepth.Text = dataContext.ImageData.ImageType == ImageType.Image3D ? Resources.GORIMG_TEXT_DEPTH_SLICE : Resources.GORIMG_TEXT_ARRAY_INDEX;
            LabelArrayDepthIndex.Text = $"{dataContext.CurrentArrayIndexDepthSlice + 1}/{(dataContext.ImageData.ImageType == ImageType.Image3D ? dataContext.MipDepth : dataContext.ArrayCount)}";
            LabelMipMapLevel.Text = dataContext.ImageData.ImageType == ImageType.Image3D 
                    ? $"{dataContext.CurrentMipLevel + 1}/{dataContext.MipCount} ({dataContext.MipWidth}x{dataContext.MipHeight}x{dataContext.MipDepth})" 
                    : $"{dataContext.CurrentMipLevel + 1}/{dataContext.MipCount} ({dataContext.MipWidth}x{dataContext.MipHeight})";            
        }

        /// <summary>
        /// Function to set the UI text for the source image picker.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void SetUIText(ISourceImagePicker dataContext)
        {
            if (dataContext?.SourceImage == null)
            {
                return;
            }

            LabelSourceImport.Text = string.Format(dataContext.SourceImage.ImageType == ImageType.Image3D ? Resources.GORIMG_TEXT_SRC_IMG_PICKER_DESC_DEPTH : Resources.GORIMG_TEXT_SRC_IMG_PICKER_DESC_ARRAY,
                                                   dataContext.ImageName);
            LabelSourceArrayDepth.Text = dataContext.SourceImage.ImageType == ImageType.Image3D ? Resources.GORIMG_TEXT_DEPTH_SLICE : Resources.GORIMG_TEXT_ARRAY_INDEX;
            LabelSrcArray.Text = $"{dataContext.CurrentArrayIndexDepthSlice + 1}/{(dataContext.SourceImage.ImageType == ImageType.Image3D ? dataContext.MipDepth : dataContext.ArrayCount)}";
            LabelSrcMips.Text = $"{dataContext.CurrentMipLevel + 1}/{dataContext.MipCount}";
        }

        /// <summary>
        /// Function to draw the cropping preview.
        /// </summary>      
        /// <param name="x">The horizontal offset.</param>
        /// <param name="y">The vertical offset.</param>
        /// <param name="width">The width of the image bounds after transformation.</param>
        /// <param name="height">The height of the image bounds after transformation.</param>
        /// <param name="scale">The scale applied to the drawing.</param>
        /// <param name="imageBounds">The bounds of the target image.</param>
        private void DrawPreview(float x, float y, float width, float height, float scale, DX.RectangleF imageBounds)
        {

            switch (DataContext.CropResizeSettings.CurrentAlignment)
            {
                case Alignment.UpperLeft:
                    _previewSprite.Position = new Vector2(x, y);
                    _previewSprite.AbsoluteAnchor = Vector2.Zero;
                    break;
                case Alignment.UpperCenter:
                    _previewSprite.Position = new Vector2(x + (width * 0.5f), y);
                    _previewSprite.Anchor = new Vector2(0.5f, 0);
                    break;
                case Alignment.UpperRight:
                    _previewSprite.Position = new Vector2(x + width, y);
                    _previewSprite.Anchor = new Vector2(1, 0);
                    break;
                case Alignment.CenterLeft:
                    _previewSprite.Position = new Vector2(x, y + (height * 0.5f));
                    _previewSprite.Anchor = new Vector2(0, 0.5f);
                    break;
                case Alignment.Center:
                    _previewSprite.Position = new Vector2(x + (width * 0.5f), y + (height * 0.5f));
                    _previewSprite.Anchor = new Vector2(0.5f, 0.5f);
                    break;
                case Alignment.CenterRight:
                    _previewSprite.Position = new Vector2(x + width, y + (height * 0.5f));
                    _previewSprite.Anchor = new Vector2(1, 0.5f);
                    break;
                case Alignment.LowerLeft:
                    _previewSprite.Position = new Vector2(x, y + height);
                    _previewSprite.Anchor = new Vector2(0, 1);
                    break;
                case Alignment.LowerCenter:
                    _previewSprite.Position = new Vector2(x + (width * 0.5f), y + height);
                    _previewSprite.Anchor = new Vector2(0.5f, 1);
                    break;
                case Alignment.LowerRight:
                    _previewSprite.Position = new Vector2(x + width, y + height);
                    _previewSprite.Anchor = new Vector2(1, 1);
                    break;
            }

            switch (DataContext.CropResizeSettings.CurrentMode)
            {
                case CropResizeMode.None:
                case CropResizeMode.Crop:
                    _previewSprite.Scale = new Vector2(scale, scale);

                    _previewSprite.TextureSampler = GorgonSamplerState.PointFiltering;
                    break;
                case CropResizeMode.Resize:
                    if (DataContext.CropResizeSettings.PreserveAspect)
                    {
                        if (_previewCropResize.Width < _previewCropResize.Height)
                        {
                            width = height * _previewCropResize.Width / _previewCropResize.Height;
                        }
                        else
                        {
                            height = width * _previewCropResize.Height / _previewCropResize.Width;
                        }
                    }

                    if (!DataContext.CropResizeSettings.PreserveAspect)
                    {
                        _previewSprite.Position = new Vector2((int)x, (int)y);
                        _previewSprite.Anchor = Vector2.Zero;
                    }
                    _previewSprite.ScaledSize = new DX.Size2F((int)width, (int)height);
                    _previewSprite.TextureSampler = DataContext.CropResizeSettings.ImageFilter == ImageFilter.Point ? GorgonSamplerState.PointFiltering : GorgonSamplerState.Default;
                    break;
            }

            DX.RectangleF clearRegion;
            _previewSprite.Color = new GorgonColor(GorgonColor.White, 1.00f);

            var importBounds = new DX.RectangleF(_previewSprite.Position.X - (_previewSprite.ScaledSize.Width * _previewSprite.Anchor.X),
                                                 _previewSprite.Position.Y - (_previewSprite.ScaledSize.Height * _previewSprite.Anchor.Y),
                                                 _previewSprite.ScaledSize.Width, _previewSprite.ScaledSize.Height);

            if (DataContext.CropResizeSettings.CurrentMode == CropResizeMode.Crop)
            {
                DX.RectangleF.Intersect(ref imageBounds, ref importBounds, out clearRegion);
            }
            else
            {
                clearRegion = importBounds;
            }

            GraphicsContext.Graphics.SetScissorRect(clearRegion.ToRectangle());

            GraphicsContext.Renderer2D.Begin(_batchPreviewState);
            GraphicsContext.Renderer2D.DrawFilledRectangle(importBounds,
                                                           GorgonColor.White,
                                                           _bgTextureView,
                                                           new DX.RectangleF(importBounds.X / _bgTextureView.Width,
                                                                             importBounds.Y / _bgTextureView.Height,
                                                                             importBounds.Width / _bgTextureView.Width,
                                                                             importBounds.Height / _bgTextureView.Height));
            GraphicsContext.Renderer2D.DrawSprite(_previewSprite);
            GraphicsContext.Renderer2D.End();
        }

        /// <summary>
        /// Function to render the source import image.
        /// </summary>
        private void DrawSourceImage()
        {
            if (_sourceSwapchain == null)
            {
                return;
            }

            IGorgonImage image = DataContext.SourcePicker.SourceImage;
            Gorgon2D renderer = GraphicsContext.Renderer2D;
            var halfClient = new Vector2((PanelSourceImage.ClientSize.Width - 6) * 0.5f, (PanelSourceImage.ClientSize.Height - 6) * 0.5f);
            float scale = ((float)(PanelSourceImage.ClientSize.Width - 6) / DataContext.SourcePicker.MipWidth).Min((float)(PanelSourceImage.ClientSize.Height - 6) / DataContext.SourcePicker.MipHeight);
            float width = DataContext.SourcePicker.MipWidth * scale;
            float height = DataContext.SourcePicker.MipHeight * scale;
            float x = halfClient.X - (width * 0.5f) + 2;
            float y = halfClient.Y - (height * 0.5f) + 2;
            var imageBounds = new DX.RectangleF(x, y, width, height);

            // Update the shader to display the correct depth slice/mip level.
            var tParams = new TextureViewer.TextureParams
            {
                DepthSlice = image.ImageType == ImageType.Image3D ? (float)DataContext.SourcePicker.CurrentArrayIndexDepthSlice / DataContext.SourcePicker.MipDepth : 0,
                MipLevel = DataContext.SourcePicker.CurrentMipLevel
            };
            _textureParameters.Buffer.SetData(in tParams);

            GraphicsContext.Graphics.SetRenderTarget(_sourceSwapchain.RenderTargetView);

            // Draw background layer.
            renderer.Begin();
            renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, PanelArrayDepth.ClientSize.Width, PanelArrayDepth.ClientSize.Height),
                                         GorgonColor.White,
                                         _bgTextureView,
                                         new DX.RectangleF(0, 0, (float)PanelArrayDepth.ClientSize.Width / _bgTextureView.Width, (float)PanelArrayDepth.ClientSize.Height / _bgTextureView.Height));            
            renderer.End();

            // Draw source image.
            renderer.Begin(image.ImageType == ImageType.Image3D ? _batchSource3DState : _batch2DState);
            renderer.DrawFilledRectangle(imageBounds,
                                         GorgonColor.White,
                                         image.ImageType == ImageType.Image3D ? null : _sourceTexture2D,
                                         new DX.RectangleF(0, 0, 1, 1),
                                         image.ImageType == ImageType.Image3D ? 0 : DataContext.SourcePicker.CurrentArrayIndexDepthSlice,
                                         textureSampler: GorgonSamplerState.PointFiltering);
            renderer.End();

            _sourceSwapchain.Present(1);
        }

        /// <summary>
        /// Function to process during idle time.
        /// </summary>
        /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
        private bool Idle()
        {
            if ((GraphicsContext == null) || (DataContext?.ImageData == null))
            {
                return false;
            }

            _prevIdle?.Invoke();

            Gorgon2D renderer = GraphicsContext.Renderer2D;
            IGorgonImage image = DataContext.ImageData;

            var halfClient = new Vector2((PanelArrayDepth.ClientSize.Width - 6) * 0.5f, (PanelArrayDepth.ClientSize.Height - 6) * 0.5f);
            float scale = ((float)(PanelArrayDepth.ClientSize.Width - 6) / DataContext.MipWidth).Min((float)(PanelArrayDepth.ClientSize.Height - 6) / DataContext.MipHeight);
            float width = DataContext.MipWidth * scale;
            float height = DataContext.MipHeight * scale;
            float x = halfClient.X - (width * 0.5f) + 3;
            float y = halfClient.Y - (height * 0.5f) + 3;
            
            // Update the shader to display the correct depth slice/mip level.
            var tParams = new TextureViewer.TextureParams
            {
                DepthSlice = image.ImageType == ImageType.Image3D ? (float)DataContext.CurrentArrayIndexDepthSlice / DataContext.MipDepth : 0,
                MipLevel = DataContext.CurrentMipLevel
            };
            _textureParameters.Buffer.SetData(in tParams);

            GraphicsContext.Graphics.SetRenderTarget(_imageSwapChain.RenderTargetView);

            // Draw background layers.
            renderer.Begin();
            renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, PanelArrayDepth.ClientSize.Width, PanelArrayDepth.ClientSize.Height),
                                         GorgonColor.White,
                                         _bgTextureView,
                                         new DX.RectangleF(0, 0, (float)PanelArrayDepth.ClientSize.Width / _bgTextureView.Width, (float)PanelArrayDepth.ClientSize.Height / _bgTextureView.Height));
            
            renderer.DrawRectangle(new DX.RectangleF((int)(x - 1), (int)(y - 1), (int)(width + 2), (int)(height + 2)), new GorgonColor(GorgonColor.Black, 0.8f), 2);
            renderer.End();

            // Draw the target texture.
            var imageBounds = new DX.RectangleF((int)x, (int)y, (int)width, (int)height);
            renderer.Begin(image.ImageType == ImageType.Image3D ? _batch3DState : _batch2DState);
            renderer.DrawFilledRectangle(imageBounds,
                                         GorgonColor.White,
                                         image.ImageType == ImageType.Image3D ? null : _imageTexture2D,
                                         new DX.RectangleF(0, 0, 1, 1),
                                         image.ImageType == ImageType.Image3D ? 0 : DataContext.CurrentArrayIndexDepthSlice,
                                         textureSampler: GorgonSamplerState.PointFiltering);
            renderer.End();

            // If we've got the resize panel open, then let's draw an image preview for our crop/resize.
            if (DataContext.NeedsTransformation)
            {
                DrawPreview(x, y, width, height, scale, imageBounds);
            }

            if (DataContext.SourceHasMultipleSubresources)
            {
                DrawSourceImage();
            }

            _imageSwapChain.Present(1);

            return true;
        }

        /// <summary>
        /// Function to clean up the graphics resources.
        /// </summary>
        private void CleanupGraphics()
        {
            if (GraphicsContext == null)
            {
                return;
            }

            CleanupSourceImagePicking();

            GorgonTexture2DView view =  Interlocked.Exchange(ref _bgTextureView, null);            
            GorgonTexture2D texture = Interlocked.Exchange(ref _bgTexture, null);
            GorgonSwapChain swapChain = Interlocked.Exchange(ref _imageSwapChain, null);
                       
            view?.Dispose();
            texture?.Dispose();

            view = Interlocked.Exchange(ref _imageTexture2D, null);
            view?.Dispose();

            view = Interlocked.Exchange(ref _previewCropResize, null);
            view?.Dispose();

            GorgonTexture3DView view3D = Interlocked.Exchange(ref _imageTexture3D, null);
            view3D?.Dispose();

            if (swapChain != null)
            {
                GraphicsContext.ReturnSwapPresenter(ref swapChain);
            }
        }

        /// <summary>
        /// Function to build up the source import image texture for display.
        /// </summary>
        private void BuildSourceImageTexture()
        {
            GorgonTexture2DView oldTexture2D = Interlocked.Exchange(ref _sourceTexture2D, null);
            GorgonTexture3DView oldTexture3D = Interlocked.Exchange(ref _sourceTexture3D, null);

            oldTexture3D?.Dispose();
            oldTexture2D?.Dispose();

            if (DataContext.SourcePicker.SourceImage == null)
            {
                return;
            }

            if (DataContext.SourcePicker.SourceImage.ImageType != ImageType.Image3D)
            {
                _sourceTexture2D = GorgonTexture2DView.CreateTexture(GraphicsContext.Graphics, new GorgonTexture2DInfo("SourceTexture")
                {
                    ArrayCount = DataContext.SourcePicker.SourceImage.ArrayCount,
                    MipLevels = DataContext.SourcePicker.SourceImage.MipCount,
                    Binding = TextureBinding.ShaderResource,
                    Format = DataContext.SourcePicker.SourceImage.Format,
                    Height = DataContext.SourcePicker.SourceImage.Height,
                    Width = DataContext.SourcePicker.SourceImage.Width,
                    Usage = ResourceUsage.Immutable
                }, DataContext.SourcePicker.SourceImage);
            }
            else
            {
                _sourceTexture3D = GorgonTexture3DView.CreateTexture(GraphicsContext.Graphics, new GorgonTexture3DInfo("SourceTexture")
                {
                    Depth = DataContext.SourcePicker.SourceImage.Depth,
                    MipLevels = DataContext.SourcePicker.SourceImage.MipCount,
                    Binding = TextureBinding.ShaderResource,
                    Format = DataContext.SourcePicker.SourceImage.Format,
                    Height = DataContext.SourcePicker.SourceImage.Height,
                    Width = DataContext.SourcePicker.SourceImage.Width,
                    Usage = ResourceUsage.Immutable
                }, DataContext.SourcePicker.SourceImage);
            }

            var shaderBuilder = new Gorgon2DShaderStateBuilder<GorgonPixelShader>();
            var batchBuilder = new Gorgon2DBatchStateBuilder();

            _batchSource3DState = batchBuilder.Clear()
                                .PixelShaderState(shaderBuilder
                                                    .ConstantBuffer(_textureParameters, 1)
                                                    .ShaderResource(_sourceTexture3D, 1)
                                                    .SamplerState(GorgonSamplerState.PointFiltering, 1)
                                                    .Shader(_imageShader3D))
                                .Build();
        }

        /// <summary>
        /// Function to build up the image texture for display.
        /// </summary>
        private void BuildImageTexture()
        {
            GorgonTexture2DView oldTexture2D = Interlocked.Exchange(ref _imageTexture2D, null);
            GorgonTexture3DView oldTexture3D = Interlocked.Exchange(ref _imageTexture3D, null);

            oldTexture3D?.Dispose();
            oldTexture2D?.Dispose();

            if (DataContext.ImageData.ImageType != ImageType.Image3D)
            {
                _imageTexture2D = GorgonTexture2DView.CreateTexture(GraphicsContext.Graphics, new GorgonTexture2DInfo("ImageTexture")
                {
                    ArrayCount = DataContext.ImageData.ArrayCount,
                    MipLevels = DataContext.ImageData.MipCount,
                    Binding = TextureBinding.ShaderResource,
                    Format = DataContext.ImageData.Format,
                    Height = DataContext.ImageData.Height,
                    Width = DataContext.ImageData.Width,
                    Usage = ResourceUsage.Immutable
                }, DataContext.ImageData);
            }
            else
            {
                _imageTexture3D = GorgonTexture3DView.CreateTexture(GraphicsContext.Graphics, new GorgonTexture3DInfo("ImageTexture")
                {
                    Depth = DataContext.ImageData.Depth,
                    MipLevels = DataContext.ImageData.MipCount,
                    Binding = TextureBinding.ShaderResource,
                    Format = DataContext.ImageData.Format,
                    Height = DataContext.ImageData.Height,
                    Width = DataContext.ImageData.Width,
                    Usage = ResourceUsage.Immutable
                }, DataContext.ImageData);
            }

            var shaderBuilder = new Gorgon2DShaderStateBuilder<GorgonPixelShader>();            
            var batchBuilder = new Gorgon2DBatchStateBuilder();            

            _batch2DState = batchBuilder.PixelShaderState(shaderBuilder
                                .ConstantBuffer(_textureParameters, 1)
                                .Shader(_imageShader2D))
                            .Build();
            _batch3DState = batchBuilder.Clear()
                                .PixelShaderState(shaderBuilder
                                .ConstantBuffer(_textureParameters, 1)
                                .ShaderResource(_imageTexture3D, 1)
                                .SamplerState(GorgonSamplerState.PointFiltering, 1)
                                .Shader(_imageShader3D))
                            .Build();            
            _batchPreviewState = batchBuilder.Clear()
                                .RasterState(GorgonRasterState.ScissorRectanglesEnabled)
                            .Build();
        }

        /// <summary>
        /// Function to clean up the graphics resources for the source image picking.
        /// </summary>
        private void CleanupSourceImagePicking()
        {
            GorgonSwapChain swap = Interlocked.Exchange(ref _sourceSwapchain, null);
            GorgonTexture2DView src2D = Interlocked.Exchange(ref _sourceTexture2D, null);
            GorgonTexture3DView src3D = Interlocked.Exchange(ref _sourceTexture3D, null);

            src3D?.Dispose();
            src2D?.Dispose();

            if (swap != null)
            {
                GraphicsContext?.ReturnSwapPresenter(ref swap);
            }
        }

        /// <summary>
        /// Function to initialize the graphics required for the source image.
        /// </summary>
        private void InitializeSourceImagePicking()
        {
            if (GraphicsContext == null)
            {
                return;
            }

            _sourceSwapchain = GraphicsContext.LeaseSwapPresenter(PanelSourceImage);
            BuildSourceImageTexture();
        }
        
        /// <summary>
        /// Function to initialize the graphics subsystem.
        /// </summary>
        private void InitializeGraphics()
        {
            if (GraphicsContext == null)
            {
                return;
            }

            _bgTexture = CommonEditorResources.CheckerBoardPatternImage.ToTexture2D(GraphicsContext.Graphics, new GorgonTexture2DLoadOptions
            {
                Binding = TextureBinding.ShaderResource,
                Name = "BackgroundTexture",
                Usage = ResourceUsage.Immutable
            });
            _bgTextureView = _bgTexture.GetShaderResourceView();

            _imageSwapChain = GraphicsContext.LeaseSwapPresenter(PanelArrayDepth);

            // Only create the pixel shaders and related resources once.
            if (_imageShader2D == null)
            {
                _imageShader2D = GorgonShaderFactory.Compile<GorgonPixelShader>(GraphicsContext.Graphics, Resources.ImageViewShaders, "Gorgon2DTextureArrayView");
                _imageShader3D = GorgonShaderFactory.Compile<GorgonPixelShader>(GraphicsContext.Graphics, Resources.ImageViewShaders, "Gorgon3DTextureView");
                _textureParameters = GorgonConstantBufferView.CreateConstantBuffer(GraphicsContext.Graphics, new GorgonConstantBufferInfo
                {
                    SizeInBytes = TextureViewer.TextureParams.Size
                });
            }

            BuildImageTexture();
        }

        /// <summary>Handles the DragDrop event of the PanelArrayDepth control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void PanelArrayDepth_DragDrop(object sender, DragEventArgs e) => ButtonImport.PerformClick();

        /// <summary>Handles the DragOver event of the PanelArrayDepth control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void PanelArrayDepth_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                e.Effect = DragDropEffects.None;
            }
            else
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        /// <summary>Handles the ItemDrag event of the ListImages control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemDragEventArgs"/> instance containing the event data.</param>
        private void ListImages_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            var data = new DataObject();
            data.SetData(typeof(ListViewItem), e.Item);

            ListImages.DoDragDrop(data, DragDropEffects.Move);
        }

        /// <summary>Handles the Leave event of the ListImages control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ListImages_Leave(object sender, EventArgs e) => TipsFiles.Hide(ListImages);

        /// <summary>Handles the MouseUp event of the ListImages control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void ListImages_MouseUp(object sender, MouseEventArgs e) => TipsFiles.Hide(ListImages);

        /// <summary>Handles the DoubleClick event of the ListImages control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ListImages_DoubleClick(object sender, EventArgs e) => ButtonImport.PerformClick();

        /// <summary>Handles the ItemMouseHover event of the ListImages control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemMouseHoverEventArgs"/> instance containing the event data.</param>
        private void ListImages_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            if (!(e.Item?.Tag is ImagePickerImportData data))
            {
                TipsFiles.Hide(ListImages);
                return;
            }

            Point cursorPos = ListImages.PointToClient(Cursor.Position);
            TipsFiles.ToolTipTitle = Path.GetFileName(data.OriginalFilePath);
            TipsFiles.Show(e.Item.ToolTipText, ListImages, cursorPos, 5000);
        }

        /// <summary>Handles the Click event of the ButtonCropResizeOk control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonCropResizeOk_Click(object sender, EventArgs e)
        {
            if ((DataContext?.UpdateImageCommand == null) || (!DataContext.UpdateImageCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.UpdateImageCommand.Execute(null);
        }

        /// <summary>Handles the Click event of the ButtonSrcImport control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSrcImport_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SelectSourceImageCommand == null) || (!DataContext.SelectSourceImageCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SelectSourceImageCommand.Execute(null);
        }

        /// <summary>Handles the Click event of the ButtonPrevSrcArray control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPrevSrcArray_Click(object sender, EventArgs e)
        {
            if (DataContext?.SourcePicker == null)
            {
                return;
            }

            DataContext.SourcePicker.CurrentArrayIndexDepthSlice--;
            ValidateButtons(DataContext);
        }

        /// <summary>Handles the Click event of the ButtonNextSrcArray control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonNextSrcArray_Click(object sender, EventArgs e)
        {
            if (DataContext?.SourcePicker == null)
            {
                return;
            }

            DataContext.SourcePicker.CurrentArrayIndexDepthSlice++;
            ValidateButtons(DataContext);
        }

        /// <summary>Handles the Click event of the ButtonPrevSrcMip control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPrevSrcMip_Click(object sender, EventArgs e)
        {
            if (DataContext?.SourcePicker == null)
            {
                return;
            }

            DataContext.SourcePicker.CurrentMipLevel--;
            ValidateButtons(DataContext);
        }

        /// <summary>Handles the Click event of the ButtonNextSrcMip control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonNextSrcMip_Click(object sender, EventArgs e)
        {
            if (DataContext?.SourcePicker == null)
            {
                return;
            }

            DataContext.SourcePicker.CurrentMipLevel++;
            ValidateButtons(DataContext);
        }

        /// <summary>Handles the SelectedIndexChanged event of the ComboImageFilter control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ComboImageFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DataContext?.CropResizeSettings == null)
            {
                return;
            }

            DataContext.CropResizeSettings.ImageFilter = (ImageFilter)ComboImageFilter.SelectedItem;
        }

        /// <summary>Handles the CheckedChanged event of the CheckPreserveAspect control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckPreserveAspect_CheckedChanged(object sender, EventArgs e)
        {
            if (DataContext?.CropResizeSettings == null)
            {
                return;
            }

            DataContext.CropResizeSettings.PreserveAspect = CheckPreserveAspect.Checked;
        }

        /// <summary>Handles the AlignmentChanged event of the AlignmentPicker control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void AlignmentPicker_AlignmentChanged(object sender, EventArgs e)
        {
            if (DataContext?.CropResizeSettings == null)
            {
                return;
            }

            var picker = (GorgonAlignmentPicker)sender;
            
            DataContext.CropResizeSettings.CurrentAlignment = picker.Alignment;
        }

        /// <summary>Handles the CheckedChanged event of the RadioCrop control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RadioCrop_CheckedChanged(object sender, EventArgs e)
        {
            if (DataContext?.CropResizeSettings == null)
            {
                return;
            }

            if ((DataContext.CropResizeSettings.AllowedModes & CropResizeMode.Crop) == CropResizeMode.Crop)
            {
                DataContext.CropResizeSettings.CurrentMode = CropResizeMode.Crop;
            }
            else
            {
                DataContext.CropResizeSettings.CurrentMode = CropResizeMode.None;
            }

            ValidateButtons(DataContext);
        }

        /// <summary>Handles the CheckedChanged event of the RadioResize control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RadioResize_CheckedChanged(object sender, EventArgs e)
        {
            if (DataContext?.CropResizeSettings == null)
            {
                return;
            }

            DataContext.CropResizeSettings.CurrentMode = CropResizeMode.Resize;
            ValidateButtons(DataContext);
        }

        /// <summary>Handles the Click event of the ButtonCropResizeCancel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonCropResizeCancel_Click(object sender, EventArgs e)
        {
            if ((DataContext?.CancelImportFileCommand == null) || (!DataContext.CancelImportFileCommand.CanExecute(null)))
            {
                return;    
            }

            DataContext.CancelImportFileCommand.Execute(null);
        }

        /// <summary>Handles the Click event of the ButtonSrcCancel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSrcCancel_Click(object sender, EventArgs e)
        {
            if ((DataContext?.CancelImportFileCommand == null) || (!DataContext.CancelImportFileCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.CancelImportFileCommand.Execute(null);
        }


        /// <summary>Handles the Click event of the ButtonRestore control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonRestore_Click(object sender, EventArgs e)
        {
            if ((DataContext?.RestoreCommand == null) || (!DataContext.RestoreCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.RestoreCommand.Execute(null);
        }

        /// <summary>Handles the Click event of the ButtonImport control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ButtonImport_Click(object sender, EventArgs e)
        {
            if ((DataContext?.ImportCommand == null) || (!DataContext.ImportCommand.CanExecute(null)))
            {
                return;
            }

            Enabled = false;
            await DataContext.ImportCommand.ExecuteAsync(null);
            Enabled = true;
            ValidateButtons(DataContext);
        }

        /// <summary>Handles the Click event of the ButtonOK control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if ((DataContext?.OkCommand == null) || (!DataContext.OkCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.OkCommand.Execute(null);
        }

        /// <summary>Handles the SelectedIndexChanged event of the ListImages control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ListImages_SelectedIndexChanged(object sender, EventArgs e)
        {
            TipsFiles.Hide(ListImages);

            if (TableResize.Visible)
            {
                if (_resetSelection)
                {
                    return;
                }

                // Disable re-entrancy.
                _resetSelection = true;

                ListImages.SelectedItems.Clear();
                if (_selectedFile != null)
                {
                    _selectedFile.Selected = true;
                }

                _resetSelection = false;
                return;
            }
                        
            _selectedFile = ListImages.SelectedItems.Count > 0 ? ListImages.SelectedItems[0] : null;
            var data = (ImagePickerImportData)_selectedFile?.Tag;

            if ((DataContext?.SelectFileCommand == null) || (!DataContext.SelectFileCommand.CanExecute(data)))
            {
                return;
            }

            DataContext.SelectFileCommand.Execute(data);
        }

        /// <summary>Handles the Click event of the ButtonNextArrayDepth control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonNextArrayDepth_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.CurrentArrayIndexDepthSlice++;
        }

        /// <summary>Handles the Click event of the ButtonPrevArrayDepth control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPrevArrayDepth_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.CurrentArrayIndexDepthSlice--;
        }

        /// <summary>Handles the Click event of the ButtonNextMip control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonNextMip_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.CurrentMipLevel++;
        }

        /// <summary>Handles the Click event of the ButtonPrevMip control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPrevMip_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.CurrentMipLevel--;
        }

        /// <summary>Handles the KeyDown event of the FormImagePicker control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void FormImagePicker_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Left:
                    ButtonPrevArrayDepth.PerformClick();
                    break;
                case Keys.Right:
                    ButtonNextArrayDepth.PerformClick();
                    break;
                case Keys.Up:
                    ButtonPrevMip.PerformClick();
                    break;
                case Keys.Down:
                    ButtonNextMip.PerformClick();
                    break;
            }
        }

        /// <summary>Handles the PreviewKeyDown event of the FormImagePicker control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        private void FormImagePicker_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    if (e.Control)
                    {
                        e.IsInputKey = true;
                    }
                    break;
            }
        }

        /// <summary>Handles the PropertyChanging event of the CropResizeSettings control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void CropResizeSettings_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ICropResizeSettings.ImportImage):
                    _previewCropResize?.Dispose();
                    _previewCropResize = null;
                    break;
            }
        }

        /// <summary>Handles the PropertyChanged event of the CropResizeSettings control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void CropResizeSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ICropResizeSettings.ImportImage):
                    if (DataContext.CropResizeSettings.ImportImage == null)
                    {
                        break;
                    }

                    _previewCropResize = GorgonTexture2DView.CreateTexture(GraphicsContext.Graphics, new GorgonTexture2DInfo("PreviewImage")
                    {
                        ArrayCount = 1,
                        MipLevels = 1,
                        Format = DataContext.CropResizeSettings.ImportImage.Format,
                        Width = DataContext.CropResizeSettings.ImportImage.Width,
                        Height = DataContext.CropResizeSettings.ImportImage.Height,
                        Binding = TextureBinding.ShaderResource,
                        Usage = ResourceUsage.Immutable
                    }, DataContext.CropResizeSettings.ImportImage);

                    _previewSprite = new GorgonSprite
                    {
                        Texture = _previewCropResize,
                        TextureRegion = new DX.RectangleF(0, 0, 1, 1),
                        Size = new DX.Size2F(_previewCropResize.Width, _previewCropResize.Height)
                    };
                    break;
            }

            SetUIText(DataContext.CropResizeSettings);
            ValidateButtons(DataContext);
        }

        /// <summary>Handles the PropertyChanging event of the SourcePicker control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void SourcePicker_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISourceImagePicker.SourceImage):
                    _sourceTexture3D?.Dispose();
                    _sourceTexture2D?.Dispose();

                    _sourceTexture2D = null;
                    _sourceTexture3D = null;
                    break;
            }
        }

        /// <summary>Handles the PropertyChanged event of the SourcePicker control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void SourcePicker_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISourceImagePicker.SourceImage):
                    if ((_sourceTexture2D != null) || (_sourceTexture3D != null))
                    {
                        BuildSourceImageTexture();
                    }
                    break;
            }

            ValidateButtons(DataContext);
            SetUIText(DataContext.SourcePicker);
        }

        /// <summary>Handles the CollectionChanged event of the ChangedSubResources control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void ChangedSubResources_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => ValidateButtons(DataContext);


        /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IImagePicker.SelectedFile):
                    ListImages.SelectedItems.Clear();
                    break;
                case nameof(IImagePicker.SourceHasMultipleSubresources):
                    DataContext.SourcePicker.OnUnload();
                    CleanupSourceImagePicking();
                    break;
                case nameof(IImagePicker.IsActive):
                    if (_prevIdle != null)
                    {
                        GorgonApplication.IdleMethod = _prevIdle;
                    }                    
                    CleanupGraphics();
                    break;
            }
        }

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IImagePicker.NeedsTransformation):
                    TableResize.Visible = DataContext.NeedsTransformation;
                    break;
                case nameof(IImagePicker.SourceHasMultipleSubresources):
                    TablePickSourceImage.Visible = DataContext.SourceHasMultipleSubresources;
                    if (DataContext.SourceHasMultipleSubresources)
                    {
                        InitializeSourceImagePicking();
                        DataContext.SourcePicker.OnLoad();
                    }
                    break;
                case nameof(IImagePicker.FilesToImport):
                    LoadThumbnails(DataContext);
                    GetFiles(DataContext);
                    break;
                case nameof(IImagePicker.ImageData):
                    // Only update the texture if we've already created it.
                    if ((_imageTexture2D != null) || (_imageTexture3D != null))
                    {
                        BuildImageTexture();
                    }
                    break;
                case nameof(IImagePicker.CurrentArrayIndexDepthSlice):
                case nameof(IImagePicker.CurrentMipLevel):
                    SetUIText(DataContext);
                    break;
                case nameof(IImagePicker.SelectedFile):
                    if (DataContext.SelectedFile == null)
                    {                        
                        break;
                    }

                    ListViewItem imageItem = ListImages.Items.OfType<ListViewItem>().FirstOrDefault(item => item.Tag == DataContext.SelectedFile);

                    if ((imageItem == null) || (imageItem.Selected))
                    {
                        break;
                    }

                    imageItem.Selected = true;
                    break;
                case nameof(IImagePicker.IsActive):
                    if (!DataContext.IsActive)
                    {
                        DataContext.OnUnload();
                        ResetDataContext();                        
                        Hide();
                        break;
                    }

                    DataContext.OnLoad();
                    SetUIText(DataContext);
                    InitializeGraphics();
                    
                    _prevIdle = GorgonApplication.IdleMethod;
                    GorgonApplication.IdleMethod = Idle;
                    break;
            }

            ValidateButtons(DataContext);
        }

        /// <summary>
        /// Function to load the thumbnail list into an image list.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void LoadThumbnails(IImagePicker dataContext)
        {
            ImageFiles.Images.Clear();

            if ((dataContext?.FilesToImport == null) || (dataContext.FilesToImport.Count == 0))
            {                
                return;
            }

            // Convert to GDI+ bitmaps.
            foreach (ImagePickerImportData data in dataContext.FilesToImport.Where(item => item.Thumbnail != null))
            {
                Image bitmap = data.Thumbnail.Buffers[0].ToBitmap();

                // If this is the first time this form is loading, then defer the image list update until after the form is loaded.
                // Otherwise the bitmap will be disposed and we'll get an error when adding to the image list.
                if (!IsHandleCreated)
                {
                    _deferredImages.Add((data.OriginalFilePath, bitmap));
                    continue;
                }

                try
                {
                    ImageFiles.Images.Add(data.OriginalFilePath, bitmap);
                }
                finally
                {
                    bitmap.Dispose();
                }
            }
        }

        /// <summary>
        /// Function to populate the import file list.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void GetFiles(IImagePicker dataContext)
        {
            ListImages.BeginUpdate();
            try
            {
                ListImages.Items.Clear();

                if (dataContext.FilesToImport == null)
                {
                    return;
                }

                foreach (ImagePickerImportData data in dataContext.FilesToImport)
                {
                    string fileName = Path.GetFileName(data.OriginalFilePath);

                    var item = new ListViewItem
                    {
                        Tag = data,
                        Name = data.FromFile.FullPath,
                        Text = fileName,
                        ImageKey = data.OriginalFilePath,
                        ToolTipText = data.OriginalMetadata.ImageType == ImageType.Image3D ? 
                                    string.Format(Resources.GORIMG_TIP_IMAGE3D, data.OriginalMetadata.ImageType, data.OriginalMetadata.Width, data.OriginalMetadata.Height, data.OriginalMetadata.Depth, data.OriginalMetadata.Format, data.OriginalMetadata.MipCount)
                                  : string.Format(Resources.GORIMG_TIP_IMAGE2D, data.OriginalMetadata.ImageType, data.OriginalMetadata.Width, data.OriginalMetadata.Height, data.OriginalMetadata.Format, data.OriginalMetadata.ArrayCount, data.OriginalMetadata.MipCount)
                    };

                    ListImages.Items.Add(item);
                }
            }
            finally
            {
                ListImages.EndUpdate();
            }
        }

        /// <summary>
        /// Function to unassign the events for the data context.
        /// </summary>
        private void UnassignEvents()
        {
            _progressPanel.SetDataContext(null);

            if (DataContext == null)
            {
                return;
            }

            DataContext.ChangedSubResources.CollectionChanged -= ChangedSubResources_CollectionChanged;
            DataContext.PropertyChanging -= DataContext_PropertyChanging;
            DataContext.PropertyChanged -= DataContext_PropertyChanged;            
            DataContext.CropResizeSettings.PropertyChanged -= CropResizeSettings_PropertyChanged;
            DataContext.CropResizeSettings.PropertyChanging -= CropResizeSettings_PropertyChanging;
            DataContext.SourcePicker.PropertyChanging -= SourcePicker_PropertyChanging;
            DataContext.SourcePicker.PropertyChanged -= SourcePicker_PropertyChanged;
        }        

        /// <summary>
        /// Function to reset the view back to its original state.
        /// </summary>
        private void ResetDataContext()
        {
            ListImages.Items.Clear();
            ImageFiles.Images.Clear();
        }

        /// <summary>
        /// Function to initialize the view from the speciifed data context.
        /// </summary>
        /// <param name="dataContext">The data context to apply.</param>
        private void InitializeFromDataContext(IImagePicker dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }
            
            LoadThumbnails(dataContext);
            GetFiles(dataContext);
            SetUIText(dataContext);

            RadioCrop.Checked = dataContext.CropResizeSettings?.CurrentMode != CropResizeMode.Resize;
            AlignmentPicker.Alignment = AlignmentResize.Alignment = dataContext.CropResizeSettings?.CurrentAlignment ?? Alignment.Center;
            CheckPreserveAspect.Checked = dataContext.CropResizeSettings?.PreserveAspect ?? false;
            ComboImageFilter.SelectedItem = dataContext.CropResizeSettings?.ImageFilter ?? ImageFilter.Point;
        }

        /// <summary>Raises the <see cref="Form.Load"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            if (_deferredImages.Count != 0)
            {
                // Load deferred images here.
                foreach ((string path, Image image) in _deferredImages)
                {
                    ImageFiles.Images.Add(path, image);
                    image.Dispose();
                }
            }

            _deferredImages.Clear();

            DataContext?.OnLoad();

            ValidateButtons(DataContext);

            if (DataContext == null)
            {
                return;
            }

            Size = new Size(DataContext.Settings.PickerWidth, DataContext.Settings.PickerHeight);
            CenterToParent();
                        
            var state = (FormWindowState)DataContext.Settings.PickerWindowState;
            if (state == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = FormWindowState.Normal;
            }                        
        }

        /// <summary>Raises the <see cref="Form.FormClosing"/> event.</summary>
        /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if ((e.CloseReason == CloseReason.UserClosing) && (DataContext != null))
            {
                e.Cancel = true;
            }

            if (DataContext != null)
            {
                DataContext.Settings.PickerWindowState = (int)WindowState;

                if (WindowState == FormWindowState.Normal)
                {
                    DataContext.Settings.PickerWidth = Size.Width;
                    DataContext.Settings.PickerHeight = Size.Height;
                }
            }           

            if ((DataContext?.DeactivateCommand != null) && (DataContext.DeactivateCommand.CanExecute(null)))
            {
                DataContext.DeactivateCommand.Execute(null);
            }

            DataContext?.OnUnload();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IImagePicker dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);

            DataContext = dataContext;

            _waitPanel.SetDataContext(dataContext);
            _progressPanel.SetDataContext(dataContext);
            ValidateButtons(dataContext);

            if (dataContext == null)
            {
                return;
            }

            DataContext.SourcePicker.PropertyChanging += SourcePicker_PropertyChanging;
            DataContext.SourcePicker.PropertyChanged += SourcePicker_PropertyChanged;
            DataContext.CropResizeSettings.PropertyChanged += CropResizeSettings_PropertyChanged;
            DataContext.CropResizeSettings.PropertyChanging += CropResizeSettings_PropertyChanging;
            DataContext.PropertyChanging += DataContext_PropertyChanging;
            DataContext.PropertyChanged += DataContext_PropertyChanged;
            DataContext.ChangedSubResources.CollectionChanged += ChangedSubResources_CollectionChanged;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FormImagePicker"/> class.</summary>
        public FormImagePicker() 
        {
            InitializeComponent();

            // Populate the image filter drop down.
            var filters = (ImageFilter[])Enum.GetValues(typeof(ImageFilter));

            foreach (ImageFilter filter in filters)
            {
                ComboImageFilter.Items.Add(filter);
            }

            _waitPanel = new WaitPanelDisplay(this);
            _progressPanel = new ProgressPanelDisplay(this);
        }
        #endregion
    }
}
