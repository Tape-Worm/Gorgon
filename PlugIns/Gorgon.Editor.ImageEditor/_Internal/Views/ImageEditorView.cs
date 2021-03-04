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
// Created: October 30, 2018 8:12:24 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.ImageEditor.Fx;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The main view for the image editor.
    /// </summary>
    internal partial class ImageEditorView
        : VisualContentBaseControl, IDataContext<IImageContent>
    {
        #region Variables.        
        // The form for the ribbon.
        private readonly FormRibbon _ribbonForm;
        // The form for the image picker.
        private readonly FormImagePicker _imagePickerForm;
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        /// <value>The data context.</value>
        public IImageContent DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to validate the controls on the view.
        /// </summary>
        private void ValidateControls()
        {
            if (DataContext is null)
            {
                LabelMipDetails.Visible = LabelMipLevel.Visible = ButtonPrevMip.Visible = ButtonNextMip.Visible = false;
                LabelArrayIndexDetails.Visible = LabelArrayIndex.Visible = ButtonPrevArrayIndex.Visible = ButtonNextArrayIndex.Visible = false;
                LabelDepthSliceDetails.Visible = LabelDepthSlice.Visible = ButtonPrevDepthSlice.Visible = ButtonNextDepthSlice.Visible = false;
                StatusPanel.Visible = false;
                return;
            }

            LabelMipDetails.Visible = LabelMipLevel.Visible = ButtonPrevMip.Visible = ButtonNextMip.Visible = DataContext.MipCount > 1;
            LabelArrayIndexDetails.Visible = LabelArrayIndex.Visible = ButtonPrevArrayIndex.Visible = ButtonNextArrayIndex.Visible = DataContext.ArrayCount > 1;
            LabelDepthSliceDetails.Visible = LabelDepthSlice.Visible = ButtonPrevDepthSlice.Visible = ButtonNextDepthSlice.Visible = DataContext.ImageType == ImageType.Image3D;

            ButtonNextMip.Enabled = (DataContext.CurrentPanel is null) && (DataContext.CommandContext is null) && (DataContext.CurrentMipLevel < DataContext.MipCount - 1);
            ButtonPrevMip.Enabled = (DataContext.CurrentPanel is null) && (DataContext.CommandContext is null) && (DataContext.CurrentMipLevel > 0);
            ButtonNextArrayIndex.Enabled = (DataContext.CurrentPanel is null) && (DataContext.CommandContext is null) && (DataContext.CurrentArrayIndex < DataContext.ArrayCount - 1);
            ButtonPrevArrayIndex.Enabled = (DataContext.CurrentPanel is null) && (DataContext.CommandContext is null) && (DataContext.CurrentArrayIndex > 0);
            ButtonNextDepthSlice.Enabled = (DataContext.CurrentPanel is null) && (DataContext.CommandContext is null) && (DataContext.CurrentDepthSlice < DataContext.DepthCount - 1);
            ButtonPrevDepthSlice.Enabled = (DataContext.CurrentPanel is null) && (DataContext.CommandContext is null) && (DataContext.CurrentDepthSlice > 0);

            StatusPanel.Visible = true;
        }

        /// <summary>Handles the PropertyChanged event of the ImagePicker control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void ImagePicker_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IImagePicker.IsActive):
                    if (DataContext.ImagePicker.IsActive)
                    {
                        _imagePickerForm.ShowDialog(ParentForm);
                    }
                    else
                    {
                        _imagePickerForm.Hide();
                    }
                    break;           
            }
        }

        /// <summary>
        /// Function to initialize the view from the data context.
        /// </summary>
        /// <param name="dataContext">The data context to use.</param>
        private void InitializeFromDataContext(IImageContent dataContext)
        {
            if (dataContext is null)
            {
                ResetDataContext();
                return;
            }

            _ribbonForm.SetDataContext(dataContext);            
            _imagePickerForm.SetDataContext(dataContext.ImagePicker);            
            CropResizeSettings.SetDataContext(dataContext.CropOrResizeSettings);
            DimensionSettings.SetDataContext(dataContext.DimensionSettings);
            GenMipMapSettings.SetDataContext(dataContext.MipMapSettings);
            SetAlpha.SetDataContext(dataContext.AlphaSettings);
            FxBlurSettings.SetDataContext(dataContext.FxContext.BlurSettings);
            FxSharpenSettings.SetDataContext(dataContext.FxContext.SharpenSettings);
            FxEmbossSettings.SetDataContext(dataContext.FxContext.EmbossSettings);
            FxEdgeSettings.SetDataContext(dataContext.FxContext.EdgeDetectSettings);
            FxPosterizeSettings.SetDataContext(dataContext.FxContext.PosterizeSettings);
            Fx1BitSettings.SetDataContext(dataContext.FxContext.OneBitSettings);

            if (dataContext.ImageType == ImageType.Image3D)
            {
                UpdateDepthSliceDetails(dataContext);
            }
            else
            {
                UpdateArrayDetails(dataContext);
            }

            UpdateImageSizeDetails(dataContext);
            UpdateMipDetails(dataContext);

            dataContext.ImagePicker.PropertyChanged += ImagePicker_PropertyChanged;
        }

        /// <summary>
        /// Function to update the image size details.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void UpdateImageSizeDetails(IImageContent dataContext)
        {
            if (dataContext is null)
            {
                return;
            }

            LabelImageSize.Text = string.Format(Resources.GORIMG_TEXT_IMAGE_SIZE_DETAILS, dataContext.Width, dataContext.Height);
        }

        /// <summary>
        /// Function to update the current depth slice details.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void UpdateDepthSliceDetails(IImageContent dataContext)
        {
            if (dataContext is null)
            {
                return;
            }

            LabelDepthSliceDetails.Text = $"{dataContext.CurrentDepthSlice + 1}/{dataContext.DepthCount}";
        }

        /// <summary>
        /// Function to update the current array details.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void UpdateArrayDetails(IImageContent dataContext)
        {
            if (dataContext is null)
            {
                return;
            }

            LabelArrayIndexDetails.Text = $"{dataContext.CurrentArrayIndex + 1}/{dataContext.ArrayCount}";
        }

        /// <summary>
        /// Function to update the current mip map details.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void UpdateMipDetails(IImageContent dataContext)
        {
            if (dataContext is null)
            {
                return;
            }

            var size = new DX.Size2((dataContext.Width >> dataContext.CurrentMipLevel).Max(1), (dataContext.Height >> dataContext.CurrentMipLevel).Max(1));

            LabelMipDetails.Text = string.Format(Resources.GORIMG_TEXT_MIP_DETAILS, dataContext.CurrentMipLevel + 1, dataContext.MipCount, size.Width, size.Height);
        }


        /// <summary>Handles the Click event of the ButtonNextMip control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ButtonNextMip_Click(object sender, EventArgs e)
        {
            if (DataContext != null)
            {
                DataContext.CurrentMipLevel++;
            }
            ValidateControls();
        }

        /// <summary>Handles the Click event of the ButtonPrevMip control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ButtonPrevMip_Click(object sender, EventArgs e)
        {
            if (DataContext != null)
            {
                DataContext.CurrentMipLevel--;
            }
            ValidateControls();
        }

        /// <summary>Handles the Click event of the ButtonPrevArrayIndex control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPrevArrayIndex_Click(object sender, EventArgs e)
        {
            if (DataContext != null)
            {
                DataContext.CurrentArrayIndex--;
            }
            ValidateControls();

        }

        /// <summary>Handles the Click event of the ButtonNextArrayIndex control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonNextArrayIndex_Click(object sender, EventArgs e)
        {
            if (DataContext != null)
            {
                DataContext.CurrentArrayIndex++;
            }
            ValidateControls();
        }

        /// <summary>Handles the Click event of the ButtonPrevDepthSlice control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPrevDepthSlice_Click(object sender, EventArgs e)
        {
            if (DataContext != null)
            {
                DataContext.CurrentDepthSlice--;
            }
            ValidateControls();
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonNextDepthSlice_Click(object sender, EventArgs e)
        {
            if (DataContext != null)
            {
                DataContext.CurrentDepthSlice++;
            }
            ValidateControls();
        }

        /// <summary>Function to handle a drag drop event on the render control.</summary>
        /// <param name="e">The event arguments.</param>
        /// <remarks>Content editor developers can override this method to handle a drop event when an item is dropped into the rendering area on the view.</remarks>
        protected override async void OnRenderWindowDragDrop(DragEventArgs e)
        {
            IContentFileDragData contentData = GetContentFileDragDropData<IContentFileDragData>(e);

            if (contentData is null)
            {                
                return;
            }

            float dpi;
            using (var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            {
                dpi = g.DpiX / 96.0f;
            }

            var args = new CopyToImageArgs(contentData.FilePaths, dpi);

            if ((DataContext?.CopyToImageCommand is null) || (!DataContext.CopyToImageCommand.CanExecute(args)))
            {
                if (args.Cancel)
                {
                    return;
                }

                OnBubbleDragDrop(e);
                return;
            }

            await DataContext.CopyToImageCommand.ExecuteAsync(args);
        }

        /// <summary>Function to handle a drag over event on the render control.</summary>
        /// <param name="e">The event arguments.</param>
        /// <remarks>Content editor developers can override this method to handle a drag over event when an item is dragged over the rendering area on the view.</remarks>
        protected override void OnRenderWindowDragOver(DragEventArgs e)
        {
            IContentFileDragData contentData = GetContentFileDragDropData<IContentFileDragData>(e);

            if (contentData is null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            var args = new CopyToImageArgs(contentData.FilePaths, 1.0f);

            if ((DataContext?.CopyToImageCommand is null) || (!DataContext.CopyToImageCommand.CanExecute(args)))
            {
                if (!args.Cancel)
                {
                    OnBubbleDragOver(e);
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
                return;
            }

            e.Effect = DragDropEffects.Move;
        }

        /// <summary>Function called when a property is changing on the data context.</summary>
        /// <param name="propertyName">The name of the property that is updating.</param>
        /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
        protected override void OnPropertyChanging(string propertyName)
        {
            base.OnPropertyChanging(propertyName);

            switch (propertyName)
            {
                case nameof(IImageContent.CommandContext):
                    if (DataContext.CommandContext != null)
                    {
                        DataContext.CommandContext.OnUnload();
                    }
                    break;
            }
        }

        /// <summary>Function called when a property is changed on the data context.</summary>
        /// <param name="propertyName">The name of the property that is updated.</param>
        /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(IImageContent.Width):
                case nameof(IImageContent.Height):
                    UpdateImageSizeDetails(DataContext);
                    break;
                case nameof(IImageContent.ArrayCount):
                    UpdateArrayDetails(DataContext);
                    break;
                case nameof(IImageContent.DepthCount):
                    UpdateImageSizeDetails(DataContext);
                    UpdateDepthSliceDetails(DataContext);
                    UpdateMipDetails(DataContext);
                    break;
                case nameof(IImageContent.MipCount):
                    UpdateMipDetails(DataContext);
                    break;
                case nameof(IImageContent.CurrentArrayIndex):
                    UpdateArrayDetails(DataContext);
                    break;
                case nameof(IImageContent.CurrentDepthSlice):
                    UpdateDepthSliceDetails(DataContext);                    
                    break;
                case nameof(IImageContent.CurrentMipLevel):
                    UpdateMipDetails(DataContext);                    
                    break;
                case nameof(IImageContent.CommandContext):
                    if ((DataContext.CommandContext is null) || (!HasRenderer(DataContext.CommandContext.Name)))
                    {
                        if (!HasRenderer(DataContext.ImageType.ToString()))
                        {
                            break;
                        }

                        string rendererName = DataContext.ImageType.ToString();

                        if ((Renderer is null) || (!string.Equals(Renderer.Name, rendererName, StringComparison.OrdinalIgnoreCase)))
                        {
                            SwitchRenderer(DataContext.ImageType.ToString(), false);
                        }
                        break;
                    }

                    DataContext.CommandContext.OnLoad();
                    SwitchRenderer(DataContext.CommandContext.Name, false);
                    break;
                case nameof(IImageContent.CurrentPanel) when (DataContext.CurrentPanel is null) && (DataContext.CommandContext is null):
                    if (!HasRenderer(DataContext.ImageType.ToString()))
                    {
                        break;
                    }

                    SwitchRenderer(DataContext.ImageType.ToString(), false);
                    break;
                case nameof(IImageContent.ImageType):
                    if (!HasRenderer(DataContext.ImageType.ToString()))
                    {
                        break;
                    }

                    SwitchRenderer(DataContext.ImageType.ToString(), true);
                    break;
            }

            ValidateControls();
        }

        /// <summary>Function called when the renderer is switched.</summary>
        /// <param name="renderer">The current renderer.</param>
        /// <param name="resetZoom"><b>true</b> if the zoom should be reset, <b>false</b> if not.</param>
        protected override void OnSwitchRenderer(IContentRenderer renderer, bool resetZoom)
        {
            base.OnSwitchRenderer(renderer, resetZoom);
            _ribbonForm.ContentRenderer = renderer;

            if (resetZoom)
            {
                _ribbonForm.ResetZoom();
            }
        }

        /// <summary>Function called when the view should be reset by a <b>null</b> data context.</summary>
        protected override void ResetDataContext()
        {
            base.ResetDataContext();

            if (DataContext is null)
            {
                return;
            }

            _ribbonForm.SetDataContext(null);
            CropResizeSettings.SetDataContext(null);
            DimensionSettings.SetDataContext(null);
            GenMipMapSettings.SetDataContext(null);
            SetAlpha.SetDataContext(null);
            FxBlurSettings.SetDataContext(null);
            FxSharpenSettings.SetDataContext(null);
            FxEmbossSettings.SetDataContext(null);
            FxEdgeSettings.SetDataContext(null);
        }

        /// <summary>Function called to shut down the view.</summary>
        protected override void OnShutdown()
        {
            DataContext?.OnUnload();

            // Reset the view.
            SetDataContext(null);

            base.OnShutdown();
        }

        /// <summary>Function to allow applications to set up rendering for their specific use cases.</summary>
        /// <param name="context">The graphics context from the host application.</param>
        /// <param name="swapChain">The current swap chain for the rendering panel.</param>
        protected override void OnSetupContentRenderer(IGraphicsContext context, GorgonSwapChain swapChain)
        {
            base.OnSetupContentRenderer(context, swapChain);

            // Grab the instance of the graphics context for any sub controls that require it.
            _imagePickerForm.GraphicsContext = context;

            ITextureViewer tex2DViewer = new Texture2DViewer(context.Renderer2D, swapChain, DataContext);
            ITextureViewer tex3DViewer = new Texture3DViewer(context.Renderer2D, swapChain, DataContext);
            ITextureViewer texCubeViewer = new TextureCubeViewer(context.Renderer2D, swapChain, context.FontFactory, DataContext);
            ITextureViewer texFxViewer = new TextureFxViewer(context.Renderer2D, swapChain, DataContext);
            tex2DViewer.CreateResources();
            tex3DViewer.CreateResources();
            texCubeViewer.CreateResources();
            texFxViewer.CreateResources();
            
            AddRenderer(tex2DViewer.Name, tex2DViewer);
            AddRenderer(tex3DViewer.Name, tex3DViewer);
            AddRenderer(texCubeViewer.Name, texCubeViewer);
            AddRenderer(texFxViewer.Name, texFxViewer);

            SwitchRenderer(DataContext.ImageType.ToString(), true);
        }

        /// <summary>Raises the <a href="http://msdn.microsoft.com/en-us/library/system.windows.forms.usercontrol.load.aspx" target="_blank">Load</a> event.</summary>
        /// <param name="e">An <a href="http://msdn.microsoft.com/en-us/library/system.eventargs.aspx" target="_blank">EventArgs</a> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (IsDesignTime)
            {
                return;
            }

            if (!_ribbonForm.IsHandleCreated)
            {
                _ribbonForm.CreateControl();
            }

            DataContext?.OnLoad();

            RenderControl?.Select();

            ValidateControls();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IImageContent dataContext)
        {
            OnSetDataContext(dataContext);

            InitializeFromDataContext(dataContext);

            DataContext = dataContext;
        }        
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageEditorView class.</summary>
        public ImageEditorView()
        {
            InitializeComponent();

            _ribbonForm = new FormRibbon();
            Ribbon = _ribbonForm.RibbonImageContent;

            _imagePickerForm = new FormImagePicker();

            RegisterChildPanel(typeof(DimensionSettings).FullName, DimensionSettings);
            RegisterChildPanel(typeof(CropResizeSettings).FullName, CropResizeSettings);
            RegisterChildPanel(typeof(MipMapSettings).FullName, GenMipMapSettings);
            RegisterChildPanel(typeof(AlphaSettings).FullName, SetAlpha);
            RegisterChildPanel(typeof(FxBlur).FullName, FxBlurSettings);
            RegisterChildPanel(typeof(FxSharpen).FullName, FxSharpenSettings);
            RegisterChildPanel(typeof(FxEmboss).FullName, FxEmbossSettings);
            RegisterChildPanel(typeof(FxEdgeDetect).FullName, FxEdgeSettings);
            RegisterChildPanel(typeof(FxPosterize).FullName, FxPosterizeSettings);
            RegisterChildPanel(typeof(FxOneBit).FullName, Fx1BitSettings);
        }
        #endregion
    }
}
