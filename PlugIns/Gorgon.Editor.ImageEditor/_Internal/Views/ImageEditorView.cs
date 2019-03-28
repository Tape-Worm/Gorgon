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
using DX = SharpDX;
using Gorgon.Editor.UI.Views;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Core;
using Gorgon.Editor.Rendering;
using System.Diagnostics;
using Gorgon.Math;
using Gorgon.Editor.UI;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.ImageEditor.Properties;
using System.Collections.Generic;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The main view for the image editor.
    /// </summary>
    internal partial class ImageEditorView 
        : ContentBaseControl, IDataContext<IImageContent>
    {
        #region Variables.        
        // The form for the ribbon.
        private readonly FormRibbon _ribbonForm;
        // The background texture.
        private GorgonTexture2DView _background;
        // The texture viewer service.
        private ITextureViewerService _textureViewer;
        // The available viewers.
        private readonly Dictionary<ImageType, ITextureViewerService> _viewers = new Dictionary<ImageType, ITextureViewerService>();
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
        /// <summary>Handles the MouseMove event of the PanelImage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void PanelImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (_textureViewer == null)
            {
                return;
            }

            _textureViewer.MouseMove(e.X, e.Y, e.Button, DataContext);
        }

        /// <summary>Handles the MouseDown event of the PanelImage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void PanelImage_MouseDown(object sender, MouseEventArgs e)
        {
            if (_textureViewer == null)
            {
                return;
            }

            _textureViewer.MouseDown(e.X, e.Y, e.Button, DataContext);
        }

        /// <summary>Handles the MouseUp event of the PanelImage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void PanelImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (_textureViewer == null)
            {
                return;
            }

            _textureViewer.MouseUp(e.X, e.Y, e.Button, DataContext);
        }

        /// <summary>Ribbons the form image zoomed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RibbonForm_ImageZoomed(object sender, ZoomEventArgs e)
        {
            if ((_textureViewer == null) || (DataContext == null))
            {
                return;
            }

            if (_textureViewer.ZoomLevel == e.ZoomLevel)
            {
                return;
            }

            _textureViewer.SetZoomLevel(e.ZoomLevel, DataContext);

            if (_textureViewer.ZoomLevel != ZoomLevels.ToWindow)
            {
                return;
            }

            ScrollHorizontal.Value = 0;
            ScrollVertical.Value = 0;
        }

        /// <summary>
        /// Function to validate the controls on the view.
        /// </summary>
        private void ValidateControls()
        {
            if (DataContext == null)
            {
                LabelMipDetails.Visible = LabelMipLevel.Visible = ButtonPrevMip.Visible = ButtonNextMip.Visible = false;
                LabelArrayIndexDetails.Visible = LabelArrayIndex.Visible = ButtonPrevArrayIndex.Visible = ButtonNextArrayIndex.Visible = false;
                LabelDepthSliceDetails.Visible = LabelDepthSlice.Visible = ButtonPrevDepthSlice.Visible = ButtonNextDepthSlice.Visible = false;
                PanelBottomBar.Visible = false;
                return;
            }

            LabelMipDetails.Visible = LabelMipLevel.Visible = ButtonPrevMip.Visible = ButtonNextMip.Visible = DataContext.MipCount > 1;
            LabelArrayIndexDetails.Visible = LabelArrayIndex.Visible = ButtonPrevArrayIndex.Visible = ButtonNextArrayIndex.Visible = DataContext.ArrayCount > 1;
            LabelDepthSliceDetails.Visible = LabelDepthSlice.Visible = ButtonPrevDepthSlice.Visible = ButtonNextDepthSlice.Visible = DataContext.ImageType == ImageType.Image3D;                        

            ButtonNextMip.Enabled = DataContext.CurrentMipLevel < DataContext.MipCount - 1;
            ButtonPrevMip.Enabled = DataContext.CurrentMipLevel > 0;
            ButtonNextArrayIndex.Enabled = DataContext.CurrentArrayIndex < DataContext.ArrayCount - 1;
            ButtonPrevArrayIndex.Enabled = DataContext.CurrentArrayIndex > 0;
            ButtonNextDepthSlice.Enabled = DataContext.CurrentDepthSlice < DataContext.DepthCount - 1;
            ButtonPrevDepthSlice.Enabled = DataContext.CurrentDepthSlice > 0;

            PanelBottomBar.Visible = true;
        }

        /// <summary>
        /// Function to update the image size details.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void UpdateImageSizeDetails(IImageContent dataContext)
        {
            if (dataContext == null)
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
            if (dataContext == null)
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
            if (dataContext == null)
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
            if ((dataContext == null) || (_textureViewer == null))
            {
                return;
            }

            DX.Size2 size = _textureViewer.GetMipSize(dataContext);

            LabelMipDetails.Text = string.Format(Resources.GORIMG_TEXT_MIP_DETAILS, dataContext.CurrentMipLevel + 1, DataContext.MipCount, size.Width, size.Height);
        }

        /// <summary>Handles the DoubleClick event of the PanelImage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        private void PanelImage_DoubleClick(object sender, EventArgs e) => _textureViewer?.EndAnimation();

        /// <summary>
        /// Function to determine if files from explorer can be dragged onto this view.
        /// </summary>
        /// <param name="e">The event parameters for the drag/drop event.</param>
        /// <returns>The data in the drag operation, or <b>null</b> if the data cannot be dragged and dropped onto this control.</returns>
        private IExplorerFilesDragData GetExplorerDragDropData(DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            if (DataContext == null)
            {
                return null;
            }

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return null;
            }

            if (!(e.Data.GetData(DataFormats.FileDrop) is IReadOnlyList<string> dragData))
            {
                return null;
            }

            IExplorerFilesDragData result = new ExplorerFilesDragData(dragData);

            if (!DataContext.CanDrop(result))
            {                
                return null;
            }

            e.Effect = DragDropEffects.Copy;
            return result;
        }

        /// <summary>Handles the DragDrop event of the ImageEditorView control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ImageEditorView_DragDrop(object sender, DragEventArgs e)
        {
            IContentFileDragData contentFiles = GetDragDropData(e, DataContext);

            if (contentFiles != null)
            {
                DataContext.Drop(contentFiles);
                return;
            }

            IExplorerFilesDragData explorerFiles = GetExplorerDragDropData(e);

            if (explorerFiles != null)
            {
                DataContext.Drop(explorerFiles);
                return;
            }

            OnBubbleDragDrop(e);
        }

        /// <summary>Handles the DragEnter event of the ImageEditorView control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ImageEditorView_DragEnter(object sender, DragEventArgs e)
        {
            IContentFileDragData contentFiles = GetDragDropData(e, DataContext);

            if ((contentFiles != null) && (e.Effect != DragDropEffects.None))
            {
                return;
            }

            IExplorerFilesDragData explorerFiles = GetExplorerDragDropData(e);

            if ((explorerFiles != null) && (e.Effect != DragDropEffects.None))
            {
                return;
            }

            if (e.Effect != DragDropEffects.None)
            {
                OnBubbleDragEnter(e);
            }
        }

        /// <summary>Function called when a property is changing on the data context.</summary>
        /// <param name="e">The event parameters.</param>
        /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
        protected override void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IImageContent.CurrentPanel):
                    IsPanelHostActive = false;
                    ClearPanelHost();
                    break;
            }
        }

        /// <summary>Function called when a property is changed on the data context.</summary>
        /// <param name="e">The event parameters.</param>
        /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e) 
        {
            switch (e.PropertyName)
            {
                case nameof(IImageContent.CurrentPanel):
                    if (DataContext.CurrentPanel == null)
                    {
                        break;
                    }

                    Control panel = GetRegisteredPanel<Control>(DataContext.CurrentPanel.GetType().FullName);
                    
                    if (Controls.Contains(panel))
                    {
                        Controls.Remove(panel);
                    }

                    panel.Visible = true;
                    AddControlToPanelHost(panel);
                    IsPanelHostActive = true;
                    break;
                case nameof(IImageContent.ArrayCount):
                    UpdateArrayDetails(DataContext);
                    break;
                case nameof(IImageContent.DepthCount):
                    UpdateDepthSliceDetails(DataContext);
                    UpdateMipDetails(DataContext);
                    break;
                case nameof(IImageContent.MipCount):
                    UpdateMipDetails(DataContext);
                    break;
                case nameof(IImageContent.CurrentArrayIndex):
                    // Don't need to call update texture parameters here, we already use an index by default.
                    UpdateArrayDetails(DataContext);
                    break;
                case nameof(IImageContent.CurrentDepthSlice):
                    UpdateDepthSliceDetails(DataContext);
                    _textureViewer.UpdateTextureParameters(DataContext);
                    break;
                case nameof(IImageContent.CurrentMipLevel):
                    UpdateMipDetails(DataContext);
                    _textureViewer.UpdateTextureParameters(DataContext);
                    break;
                case nameof(IImageContent.Width):
                case nameof(IImageContent.Height):
                    UpdateImageSizeDetails(DataContext);
                    if (_textureViewer == null)
                    {
                        _textureViewer = _viewers[DataContext.ImageType];
                    }

                    _textureViewer.UpdateTexture(DataContext);
                    break;
                case nameof(IImageContent.ImageType):
                    if (!_viewers.ContainsKey(DataContext.ImageType))
                    {
                        break;
                    }

                    _textureViewer?.UpdateTexture(null);
                    _textureViewer = _viewers[DataContext.ImageType];                    
                    _textureViewer.UpdateTexture(DataContext);
                    break;                
                default:
                    if (_textureViewer == null)
                    {
                        _textureViewer = _viewers[DataContext.ImageType];
                    }
                                        
                    _textureViewer.UpdateTexture(DataContext);
                    UpdateMipDetails(DataContext);
                    break;
            }

            ValidateControls();
        }

        /// <summary>Handles the Click event of the ButtonCenter control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonCenter_Click(object sender, EventArgs e)
        {
            ScrollHorizontal.Value = ScrollVertical.Value = 0;
            _ribbonForm.ResetZoom();
        }

        /// <summary>Handles the ValueChanged event of the ScrollVertical control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void ScrollVertical_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            _textureViewer?.Scroll(DataContext);
            Idle();
        }

        /// <summary>Handles the Click event of the ButtonNextMip control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
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
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Function to initialize the view from the data context.
        /// </summary>
        /// <param name="dataContext">The data context to use.</param>
        private void InitializeFromDataContext(IImageContent dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            _ribbonForm.SetDataContext(dataContext);
            CropResizeSettings.SetDataContext(dataContext.CropOrResizeSettings);
            DimensionSettings.SetDataContext(dataContext.DimensionSettings);
            GenMipMapSettings.SetDataContext(dataContext.MipMapSettings);
        }

        /// <summary>Handles the RenderToBitmap event of the PanelImage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void PanelImage_RenderToBitmap(object sender, PaintEventArgs e) => RenderSwapChainToBitmap(e.Graphics);


        /// <summary>Handles the PreviewKeyDown event of the PanelImage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        private void PanelImage_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (e.Shift)
                    {
                        DataContext.CurrentArrayIndex -= (DataContext.ArrayCount / 2).Max(1).Min(6);
                        DataContext.CurrentDepthSlice -= (DataContext.DepthCount / 2).Max(1).Min(6);
                    }
                    else
                    {
                        --DataContext.CurrentArrayIndex;
                        --DataContext.CurrentDepthSlice;
                    }
                    e.IsInputKey = true;
                    return;
                case Keys.Right:
                    if (e.Shift)
                    {
                        DataContext.CurrentArrayIndex += (DataContext.ArrayCount / 2).Max(1).Min(6);
                        DataContext.CurrentDepthSlice += (DataContext.DepthCount / 2).Max(1).Min(6);
                    }
                    else
                    {
                        ++DataContext.CurrentArrayIndex;
                        ++DataContext.CurrentDepthSlice;
                    }
                    e.IsInputKey = true;
                    return;
                case Keys.Up:
                    if (e.Shift)
                    {
                        DataContext.CurrentMipLevel -= 2;
                    }
                    else
                    {
                        --DataContext.CurrentMipLevel;
                    }
                    e.IsInputKey = true;
                    return;
                case Keys.Down:
                    if (e.Shift)
                    {
                        DataContext.CurrentMipLevel += 2;
                    }
                    else
                    {
                        ++DataContext.CurrentMipLevel;
                    }
                    e.IsInputKey = true;
                    return;
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Resize"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if ((IsDesignTime) || (_textureViewer == null) || (DataContext == null))
            {
                return;
            }

            _textureViewer.WindowResize(DataContext);
        }

        /// <summary>Raises the <a href="http://msdn.microsoft.com/en-us/library/system.windows.forms.control.paint.aspx" target="_blank">Paint</a> event.</summary>
        /// <param name="e">A <a href="http://msdn.microsoft.com/en-us/library/system.windows.forms.painteventargs.aspx" target="_blank">PaintEventArgs</a> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (IsDesignTime)
            {
                return;
            }

            Idle();
        }

        /// <summary>Raises the <a href="http://msdn.microsoft.com/en-us/library/system.windows.forms.control.mousewheel.aspx" target="_blank">MouseWheel</a> event.</summary>
        /// <param name="e">A <a href="http://msdn.microsoft.com/en-us/library/system.windows.forms.mouseeventargs.aspx" target="_blank">MouseEventArgs</a> that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!ScrollVertical.Enabled)
            {
                return;
            }

            ScrollVertical.Value = (ScrollVertical.Value - (e.Delta.Sign() * ScrollVertical.LargeChange)).Min(ScrollVertical.Maximum - ScrollVertical.LargeChange).Max(ScrollVertical.Minimum);            
        }

        /// <summary>
        /// Function to render during idle time.
        /// </summary>
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
        private bool Idle()
        {
            if ((DataContext == null) || (DataContext.ImageType == ImageType.Unknown) || (_textureViewer == null))
            {
                return false;
            }

            _textureViewer.Draw(DataContext);
            return true;
        }

        /// <summary>Function to allow user defined setup of the graphics context with this control.</summary>
        /// <param name="context">The context being assigned.</param>
        /// <param name="swapChain">
        /// The swap chain assigned to the <span class="nolink">RenderControl<span class="languageSpecificText"><span class="cs">()</span><span class="cpp">()</span><span class="nu">()</span><span class="fs">()</span></span></span>.
        /// </param>
        protected override void OnSetupGraphics(IGraphicsContext context, GorgonSwapChain swapChain)
        {
            Debug.Assert(EditorCommonResources.CheckerBoardPatternImage != null, "Background texture was not loaded.");

            _ribbonForm.GraphicsContext = context;

            _background = GorgonTexture2DView.CreateTexture(context.Graphics, new GorgonTexture2DInfo("Editor_BG_Texture")
            {
                Format = EditorCommonResources.CheckerBoardPatternImage.Format,
                Width = EditorCommonResources.CheckerBoardPatternImage.Width,
                Height = EditorCommonResources.CheckerBoardPatternImage.Height,
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Immutable
            }, EditorCommonResources.CheckerBoardPatternImage);

            _viewers[ImageType.Image2D] = new Texture2DViewer(context, swapChain, ScrollHorizontal, ScrollVertical);
            _viewers[ImageType.Image2D].CreateResources(_background);
            _viewers[ImageType.ImageCube] = new TextureCubeViewer(context, swapChain, ScrollHorizontal, ScrollVertical);
            _viewers[ImageType.ImageCube].CreateResources(_background);
            _viewers[ImageType.Image3D] = new Texture3DViewer(context, swapChain, ScrollHorizontal, ScrollVertical);
            _viewers[ImageType.Image3D].CreateResources(_background);
                        
            if (DataContext?.ImageType == null)
            {
                ValidateControls();
                return;
            }            

            _textureViewer = _viewers[DataContext.ImageType];
            _textureViewer.UpdateTexture(DataContext);
            UpdateImageSizeDetails(DataContext);
            UpdateMipDetails(DataContext);
            UpdateArrayDetails(DataContext);
            UpdateDepthSliceDetails(DataContext);

            ValidateControls();
        }

        /// <summary>Function called to shut down the view.</summary>
        protected override void OnShutdown()
        {
            DataContext?.OnUnload();

            // Reset the view.
            SetDataContext(null);

            foreach (ITextureViewerService service in _viewers.Values)
            {
                service.Dispose();
            }

            _background?.Dispose();
            _background = null;            
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
            IdleMethod = Idle;

            PanelImage.Select();

            ValidateControls();
        }

        /// <summary>Function called when the view should be reset by a <b>null</b> data context.</summary>
        protected override void ResetDataContext()
        {
            base.ResetDataContext();

            if (DataContext == null)
            {
                return;
            }

            _ribbonForm.SetDataContext(null);
            CropResizeSettings.SetDataContext(null);
            DimensionSettings.SetDataContext(null);
            GenMipMapSettings.SetDataContext(null);
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IImageContent dataContext)
        {
            base.SetDataContext(dataContext);

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
            _ribbonForm.ImageZoomed += RibbonForm_ImageZoomed;
            Ribbon = _ribbonForm.RibbonImageContent;

            RegisterChildPanel(typeof(DimensionSettings).FullName, DimensionSettings);
            RegisterChildPanel(typeof(CropResizeSettings).FullName, CropResizeSettings);
            RegisterChildPanel(typeof(MipMapSettings).FullName, GenMipMapSettings);
        }
        #endregion
    }
}
