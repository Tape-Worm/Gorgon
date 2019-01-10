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
using System.Data;
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
using Gorgon.Renderers;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
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
        private FormRibbon _ribbonForm;
        // The background texture.
        private GorgonTexture2DView _background;
        // The current zoom level.
        private ZoomLevels _zoomLevel = ZoomLevels.ToWindow;
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
        /// <summary>
        /// Function to validate the controls on the view.
        /// </summary>
        private void ValidateControls()
        {
            if (DataContext == null)
            {
                PanelMipSelector.Visible = false;
                return;
            }

            PanelMipSelector.Visible = DataContext.MipCount > 1;
            ButtonNextMip.Enabled = DataContext.CurrentMipLevel < DataContext.MipCount - 1;
            ButtonPrevMip.Enabled = DataContext.CurrentMipLevel > 0;
        }

        /// <summary>
        /// Function to update the current mip map details.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void UpdateMipDetails(IImageContent dataContext)
        {
            DX.Size2 size = DX.Size2.Zero;

            if ((dataContext == null) || (_textureViewer == null))
            {
                return;
            }

            size = _textureViewer.GetMipSize(dataContext);

            LabelMipDetails.Text = string.Format(Resources.GORIMG_TEXT_MIP_DETAILS, dataContext.CurrentMipLevel + 1, DataContext.MipCount, size.Width, size.Height);
        }

        /// <summary>Handles the DoubleClick event of the PanelImage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        private void PanelImage_DoubleClick(object sender, EventArgs e) => _textureViewer?.EndAnimation();

        /// <summary>Handles the DragDrop event of the ImageEditorView control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ImageEditorView_DragDrop(object sender, DragEventArgs e) => OnBubbleDragDrop(e);

        /// <summary>Handles the DragEnter event of the ImageEditorView control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ImageEditorView_DragEnter(object sender, DragEventArgs e) => OnBubbleDragEnter(e);

        /// <summary>Function called when a property is changed on the data context.</summary>
        /// <param name="e">The event parameters.</param>
        /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e) 
        {
            switch (e.PropertyName)
            {
                case nameof(IImageContent.ContentState):                    
                    return;
                case nameof(IImageContent.File):
                    // This only matters when we save, so there's no real visual update.
                    return;
                case nameof(IImageContent.DepthCount):
                case nameof(IImageContent.MipCount):
                    UpdateMipDetails(DataContext);
                    break;
                case nameof(IImageContent.CurrentDepthSlice):
                    // TODO:
                    break;
                case nameof(IImageContent.CurrentMipLevel):
                    UpdateMipDetails(DataContext);
                    _textureViewer.UpdateTextureParameters(DataContext);
                    return;
                default:
                    if (_textureViewer == null)
                    {
                        _textureViewer = _viewers[DataContext.ImageType];
                    }
                                        
                    _textureViewer.UpdateTexture(DataContext);
                    UpdateMipDetails(DataContext);
                    ValidateControls();
                    break;
            }            
        }
               
        /// <summary>Handles the ValueChanged event of the ScrollVertical control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void ScrollVertical_ValueChanged(object sender, EventArgs e) => Idle();

        /// <summary>Handles the Click event of the ItemZoomToWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void ItemZoom_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

            if ((item.Tag == null) || (!Enum.TryParse(item.Tag.ToString(), out ZoomLevels zoom)))
            {
                item.Checked = false;
                return;
            }

            // Do not let us uncheck.
            if (_zoomLevel == zoom)
            {
                item.Checked = true;
                return;
            }

            MenuZoomItems.Text = item.Text;
            _zoomLevel = zoom;
            UpdateZoomMenu();

            if (item != ItemZoomToWindow)
            {
                return;
            }

            ScrollHorizontal.Value = 0;
            ScrollVertical.Value = 0;
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

        /// <summary>
        /// Function to retrieve the value used to multiply by when zooming.
        /// </summary>
        /// <returns>The zoom value as a normalized value.</returns>
        private float GetZoomValue()
        {
            switch (_zoomLevel)
            {
                case ZoomLevels.Percent12:
                    return 0.125f;                    
                case ZoomLevels.Percent25:
                    return 0.25f;                    
                case ZoomLevels.Percent50:
                    return 0.5f;
                case ZoomLevels.Percent200:
                    return 2.0f;
                case ZoomLevels.Percent400:
                    return 4.0f;                    
                case ZoomLevels.Percent800:
                    return 8.0f;                    
                case ZoomLevels.Percent1600:
                    return 16.0f;
                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// Function to update the zoom item menu to reflect the current selection.
        /// </summary>
        private void UpdateZoomMenu()
        {
            ToolStripMenuItem currentItem;

            switch (_zoomLevel)
            {
                case ZoomLevels.Percent12:
                    currentItem = Item12Percent;
                    break;
                case ZoomLevels.Percent25:
                    currentItem = Item25Percent;
                    break;
                case ZoomLevels.Percent50:
                    currentItem = Item50Percent;
                    break;
                case ZoomLevels.Percent100:
                    currentItem = Item100Percent;
                    break;
                case ZoomLevels.Percent200:
                    currentItem = Item200Percent;
                    break;
                case ZoomLevels.Percent400:
                    currentItem = Item400Percent;
                    break;
                case ZoomLevels.Percent800:
                    currentItem = Item800Percent;
                    break;
                case ZoomLevels.Percent1600:
                    currentItem = Item1600Percent;
                    break;
                default:
                    currentItem = ItemZoomToWindow;
                    break;
            }

            foreach (ToolStripMenuItem item in MenuZoom.Items.OfType<ToolStripMenuItem>().Where(item => item != currentItem))
            {
                item.Checked = false;
            }

            if (_textureViewer != null)
            {
                _textureViewer.ZoomLevel = _zoomLevel;
            }
        }

        /// <summary>
        /// Function to reset the view to its original state.
        /// </summary>
        protected override void ResetDataContext()
        {
            base.ResetDataContext();
            // Reset the zoom menu.
            _zoomLevel = ZoomLevels.ToWindow;
            UpdateZoomMenu();
            ItemZoomToWindow.Checked = true;
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
                        
            UpdateZoomMenu();
        }

        /// <summary>Handles the RenderToBitmap event of the PanelImage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void PanelImage_RenderToBitmap(object sender, PaintEventArgs e) => RenderSwapChainToBitmap(e.Graphics);

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
            _viewers[ImageType.Image3D] = new Texture3DViewer(context, swapChain, ScrollHorizontal, ScrollVertical);
            _viewers[ImageType.Image3D].CreateResources(_background);
            // TODO: Other types.
                        
            if (DataContext?.ImageType == null)
            {
                ValidateControls();
                return;
            }            

            _textureViewer = _viewers[DataContext.ImageType];
            _textureViewer.UpdateTexture(DataContext);
            UpdateMipDetails(DataContext);

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

            ValidateControls();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IImageContent dataContext)
        {
            base.SetDataContext(dataContext);

            InitializeFromDataContext(dataContext);

            DataContext = dataContext;
            _ribbonForm.SetDataContext(dataContext);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageEditorView class.</summary>
        public ImageEditorView()
        {
            InitializeComponent();

            _ribbonForm = new FormRibbon();
            Ribbon = _ribbonForm.RibbonImageContent;
        }
        #endregion
    }
}
