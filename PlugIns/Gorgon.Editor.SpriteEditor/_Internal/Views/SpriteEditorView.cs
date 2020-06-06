#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: March 14, 2019 11:33:25 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Renderers;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The primary view for the sprite editor.
    /// </summary>
    internal partial class SpriteEditorView
        : VisualContentBaseControl, IDataContext<ISpriteContent>
    {
        #region Variables.
        // The form for the ribbon.
        private readonly FormRibbon _ribbonForm;
        // The rectangle clipping service used to capture sprite data.
        private IRectClipperService _clipperService;
        // The service used to edit the sprite vertices.
        private SpriteVertexEditService _vertexEditService;
        // The picking service used to capture sprite data.
        private PickClipperService _pickService;        
        // The anchor editing service.
        private IAnchorEditService _anchorService;
        // Marching ants renderer.
        private IMarchingAnts _marchAnts;        
        // Manual input for the sprite clipping rectangle.
        private readonly FormManualRectangleEdit _manualRectEditor;
        // Manual input for the sprite vertex editing.
        private readonly FormManualVertexEdit _manualVertexEditor;
        // The parent form that is hosting this control hierarchy.
        private Form _parentForm;
        // The texture for the anchor icon.
        private GorgonTexture2DView _anchorTexture;
        // The current sprite information presenter.
        private ISpriteInfo _spriteInfo;
        // The current sprite texture array index updater.
        private IArrayUpdate _arrayUpdater;
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISpriteContent DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to validate the controls on the view.
        /// </summary>
        private void ValidateButtons()
        {
            _ribbonForm.ValidateButtons();
            _ribbonForm.ButtonZoomSprite.Enabled = (DataContext?.Texture != null) && (Renderer?.CanZoom ?? false);
            
            if (DataContext?.Texture == null)
            {
                LabelSpriteInfo.Visible = false;
                LabelArrayIndex.Visible = LabelArrayIndexDetails.Visible = ButtonNextArrayIndex.Visible = ButtonPrevArrayIndex.Visible = false;
                return;
            }

            LabelArrayIndex.Visible = LabelArrayIndexDetails.Visible = LabelSpriteInfo.Visible = true;
            ButtonNextArrayIndex.Visible = ButtonPrevArrayIndex.Visible = _spriteInfo?.SupportsArrayChange ?? false;

            ButtonNextArrayIndex.Enabled = _arrayUpdater?.UpdateArrayIndexCommand?.CanExecute(1) ?? false;
            ButtonPrevArrayIndex.Enabled = _arrayUpdater?.UpdateArrayIndexCommand?.CanExecute(-1) ?? false;
        }

        /// <summary>Handles the Click event of the ButtonPrevArrayIndex control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPrevArrayIndex_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            if ((_arrayUpdater?.UpdateArrayIndexCommand == null) || (!_arrayUpdater.UpdateArrayIndexCommand.CanExecute(-1)))
            {
                return;
            }
            _arrayUpdater.UpdateArrayIndexCommand.Execute(-1);

            ValidateButtons();
            UpdateArrayPanel();
        }

        /// <summary>Handles the Click event of the ButtonNextArrayIndex control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonNextArrayIndex_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            if ((_arrayUpdater?.UpdateArrayIndexCommand == null) || (!_arrayUpdater.UpdateArrayIndexCommand.CanExecute(1)))
            {
                return;
            }

            _arrayUpdater.UpdateArrayIndexCommand.Execute(1);

            ValidateButtons();

            UpdateArrayPanel();
        }

        /// <summary>Handles the Move event of the ParentForm control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ParentForm_Move(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            ManualInputWindowPositioning(_manualRectEditor, DataContext.Settings.ManualRectangleEditorBounds);
            ManualInputWindowPositioning(_manualVertexEditor, DataContext.Settings.ManualVertexEditorBounds);
        }

        /// <summary>Handles the PropertyChanged event of the SpritePickContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void SpritePickContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpritePickContext.ImageData):
                    RenderControl.Cursor = Cursor.Current = Cursors.Default;

                    UpdateArrayPanel();
                    UpdateSpriteDimensionsPanel();
                    break;
                case nameof(ISpritePickContext.SpriteInfo):
                    UpdateSpriteDimensionsPanel();
                    break;
                case nameof(ISpritePickContext.ArrayIndex):
                    UpdateArrayPanel();
                    break;
            }
        }

        /// <summary>Handles the PropertyChanged event of the SpriteClipContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void SpriteClipContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteClipContext.SpriteInfo):
                    UpdateSpriteDimensionsPanel();
                    break;
                case nameof(ISpriteClipContext.ArrayIndex):
                    UpdateArrayPanel();
                    break;
            }
        }

        /// <summary>Handles the ResizeEnd event of the ManualInput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ManualVertexInput_ResizeEnd(object sender, EventArgs e)
        {
            var form = (Form)sender;
            Point pos = RenderControl.PointToScreen(new Point(RenderControl.ClientSize.Width - 128, 0));
            var snapRect = new Rectangle(pos.X, pos.Y, 128, 128);

            if ((form.DesktopBounds.Right >= snapRect.Left) && (form.DesktopBounds.Right <= snapRect.Right)
                && (form.DesktopBounds.Top >= snapRect.Top) && (form.DesktopBounds.Top <= snapRect.Bottom)
                && ((ModifierKeys & Keys.Alt) != Keys.Alt))
            {
                // Disable to stop the form from being dragged any further.
                form.Enabled = false;
                if (DataContext != null)
                {
                    DataContext.Settings.ManualVertexEditorBounds = null;
                }
                ManualInputWindowPositioning(form, DataContext.Settings.ManualVertexEditorBounds);
                form.Enabled = true;
                return;
            }

            DataContext.Settings.ManualVertexEditorBounds = new DX.Rectangle(form.DesktopBounds.Left,
                                                                                form.DesktopBounds.Top,
                                                                                form.DesktopBounds.Width,
                                                                                form.DesktopBounds.Height);
        }

        /// <summary>Handles the ResizeEnd event of the ManualInput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ManualInput_ResizeEnd(object sender, EventArgs e)
        {
            var form = (Form)sender;
            Point pos = RenderControl.PointToScreen(new Point(RenderControl.ClientSize.Width - 128, 0));
            var snapRect = new Rectangle(pos.X, pos.Y, 128, 128);

            if ((form.DesktopBounds.Right >= snapRect.Left) && (form.DesktopBounds.Right <= snapRect.Right)
                && (form.DesktopBounds.Top >= snapRect.Top) && (form.DesktopBounds.Top <= snapRect.Bottom)
                && ((ModifierKeys & Keys.Alt) != Keys.Alt))
            {
                // Disable to stop the form from being dragged any further.
                form.Enabled = false;
                if (DataContext != null)
                {
                    DataContext.Settings.ManualRectangleEditorBounds = null;
                }
                ManualInputWindowPositioning(form, DataContext.Settings.ManualRectangleEditorBounds);
                form.Enabled = true;
                return;
            }

            DataContext.Settings.ManualRectangleEditorBounds = new DX.Rectangle(form.DesktopBounds.Left, 
                                                                                form.DesktopBounds.Top, 
                                                                                form.DesktopBounds.Width,
                                                                                form.DesktopBounds.Height);
        }

        /// <summary>Handles the ClosePanel event of the ManualRectangleEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ManualVertexInput_ClosePanel(object sender, FormClosingEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            if (e.CloseReason == CloseReason.UserClosing)
            {
                _manualVertexEditor.Hide();
                _ribbonForm.ButtonSpriteCornerManualInput.Checked = false;
                _ribbonForm.ValidateButtons();
                e.Cancel = true;
            }
        }

        /// <summary>Handles the ClosePanel event of the ManualRectangleEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ManualInput_ClosePanel(object sender, FormClosingEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            if (e.CloseReason == CloseReason.UserClosing)
            {
                _manualRectEditor.Hide();
                _ribbonForm.ButtonClipManualInput.Checked = false;
                _ribbonForm.ValidateButtons();
                e.Cancel = true;
            }
        }

        /// <summary>Handles the ToggleManualInput event of the VertexEditViewer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void VertexEditViewer_ToggleManualInput(object sender, EventArgs e)
        {
            if (_manualVertexEditor.Visible)
            {
                _manualVertexEditor.Hide();

                _ribbonForm.ButtonSpriteCornerManualInput.Checked = false;
                _ribbonForm.ValidateButtons();
                return;
            }

            _manualVertexEditor.Show(GorgonApplication.MainForm);
            ManualInputWindowPositioning(_manualVertexEditor, DataContext.Settings.ManualVertexEditorBounds);

            _ribbonForm.ButtonSpriteCornerManualInput.Checked = true;
            _ribbonForm.ValidateButtons();
        }

        /// <summary>Handles the ToggleManualInput event of the ClipViewer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ClipViewer_ToggleManualInput(object sender, EventArgs e) 
        {
            if (_manualRectEditor.Visible)
            {
                _manualRectEditor.Hide();

                _ribbonForm.ButtonClipManualInput.Checked = false;
                _ribbonForm.ValidateButtons();
                return;
            }

            _manualRectEditor.Show(GorgonApplication.MainForm);
            ManualInputWindowPositioning(_manualRectEditor, DataContext.Settings.ManualRectangleEditorBounds);
                        
            _ribbonForm.ButtonClipManualInput.Checked = true;
            _ribbonForm.ValidateButtons();
        }

        /// <summary>Handles the PreviewKeyDown event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (_ribbonForm.ButtonSpriteClipApply.Enabled)
                    {
                        _ribbonForm.ButtonSpriteClipApply.PerformClick();
                        e.IsInputKey = true;                        
                    }
                    break;
                case Keys.Escape:
                    if (_ribbonForm.ButtonSpriteClipCancel.Enabled)
                    {
                        _ribbonForm.ButtonSpriteClipCancel.PerformClick();
                        e.IsInputKey = true;
                    }
                    break;
                case Keys.OemPeriod:
                    if (ButtonNextArrayIndex.Enabled)
                    {
                        ButtonNextArrayIndex.PerformClick();
                        e.IsInputKey = true;
                    }
                    break;
                case Keys.Oemcomma:
                    if (ButtonPrevArrayIndex.Enabled)
                    {
                        ButtonPrevArrayIndex.PerformClick();
                        e.IsInputKey = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// Function to initialize the view from the data context.
        /// </summary>
        /// <param name="dataContext">The data context to use.</param>
        private void InitializeFromDataContext(ISpriteContent dataContext)
        {
            _spriteInfo = dataContext;
            _arrayUpdater = null;

            if (dataContext == null)
            {                
                ResetDataContext();
                return;
            }
            
            _ribbonForm.SetDataContext(dataContext);
            _manualRectEditor.SetDataContext(dataContext?.SpriteClipContext);
            _manualVertexEditor.SetDataContext(dataContext?.SpriteVertexEditContext);
            SpritePickMaskColor.SetDataContext(dataContext?.SpritePickContext.SpritePickMaskEditor);
            SpriteColorSelector.SetDataContext(dataContext?.ColorEditor);
            SpriteAnchorSelector.SetDataContext(dataContext?.AnchorEditor);
            SpriteWrapping.SetDataContext(dataContext?.WrappingEditor);

            UpdateArrayPanel();
            UpdateSpriteDimensionsPanel();
                        
            if (dataContext?.SpriteClipContext != null)
            {
                dataContext.SpriteClipContext.PropertyChanged += SpriteClipContext_PropertyChanged;
            }

            if (dataContext?.SpritePickContext != null)
            {
                dataContext.SpritePickContext.PropertyChanged += SpritePickContext_PropertyChanged;
            }
        }

        /// <summary>
        /// Function to update the sprite dimensions control.
        /// </summary>
        private void UpdateSpriteDimensionsPanel()
        {
            string spriteInfoText = _spriteInfo?.SpriteInfo ?? string.Empty;            

            if (string.Equals(spriteInfoText, LabelSpriteInfo.Text, StringComparison.CurrentCulture))
            {
                return;
            }

            LabelSpriteInfo.Text = spriteInfoText;
        }

        /// <summary>
        /// Function to update the panel with the array controls.
        /// </summary>
        private void UpdateArrayPanel()
        {
            string arrayInfo = _spriteInfo == null ? string.Empty : $"{_spriteInfo.ArrayIndex + 1}/{_spriteInfo.ArrayCount}";

            if (string.Equals(arrayInfo, LabelArrayIndexDetails.Text, StringComparison.CurrentCulture))
            {
                return;
            }

            LabelArrayIndexDetails.Text = arrayInfo;
        }

        /// <summary>Function called when the view should be reset by a <b>null</b> data context.</summary>
        protected override void ResetDataContext()
        {
            _ribbonForm.SetDataContext(null);
            _manualRectEditor.SetDataContext(null);
            _manualVertexEditor.SetDataContext(null);
            SpritePickMaskColor.SetDataContext(null);
            SpriteColorSelector.SetDataContext(null);
            SpriteWrapping.SetDataContext(null);
            SpriteAnchorSelector.SetDataContext(null);

            base.ResetDataContext();
        }

        /// <summary>Function to handle a drag enter event on the render control.</summary>
        /// <param name="e">The event arguments.</param>
        /// <remarks>Content editor developers can override this method to handle a drag enter event when an item is dragged into the rendering area on the view.</remarks>
        protected override void OnRenderWindowDragOver(DragEventArgs e)
        {
            IContentFileDragData contentData = GetContentFileDragDropData<IContentFileDragData>(e);

            if ((contentData == null) || (contentData.FilePaths.Count != 1))
            {
                e.Effect = DragDropEffects.None;
                return;
            }            
            
            var args = new SetTextureArgs(contentData.FilePaths[0]);

            if ((DataContext?.SetTextureCommand == null) || (!DataContext.SetTextureCommand.CanExecute(args)))
            {
                if (!args.Cancel)
                {
                    base.OnRenderWindowDragOver(e);
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
                return;
            }

            e.Effect = DragDropEffects.Move;
        }

        /// <summary>Function to handle a drag drop event on the render control.</summary>
        /// <param name="e">The event arguments.</param>
        /// <remarks>Content editor developers can override this method to handle a drop event when an item is dropped into the rendering area on the view.</remarks>
        protected async override void OnRenderWindowDragDrop(DragEventArgs e)
        {
            IContentFileDragData contentData = GetContentFileDragDropData<IContentFileDragData>(e);

            var args = new SetTextureArgs(contentData.FilePaths[0]);

            if ((DataContext?.SetTextureCommand != null) && (DataContext.SetTextureCommand.CanExecute(args)))
            {
                await DataContext.SetTextureCommand.ExecuteAsync(args);
            }
        }

        /// <summary>Function called when a property is changing on the data context.</summary>
        /// <param name="propertyName">The name of the property that is updating.</param>
        /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
        protected override void OnPropertyChanging(string propertyName)
        {
            base.OnPropertyChanging(propertyName);

            switch (propertyName)
            {
                case nameof(ISpriteContent.CommandContext):
                    DataContext.CommandContext?.OnUnload();
                    RenderControl.Cursor = Cursor.Current = Cursors.Default;
                    ValidateButtons();
                    break;
            }
        }

        /// <summary>Function called when a property is changed on the data context.</summary>        
        /// <param name="propertyName">The name of the property that is updated.</param>
        /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
        protected override void OnPropertyChanged(string propertyName)
        {
            string rendererName;

            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(ISpriteContent.Texture):
                    SwitchRenderer(DefaultSpriteViewer.ViewerName, true);
                    RenderControl.Cursor = Cursor.Current = Cursors.Default;

                    UpdateArrayPanel();
                    UpdateSpriteDimensionsPanel();
                    break;
                case nameof(ISpriteContent.SpriteInfo):
                    UpdateSpriteDimensionsPanel();
                    break;
                case nameof(ISpriteContent.CurrentPanel):
                    if ((DataContext.CurrentPanel == null) || (!HasRenderer(DataContext.CurrentPanel.GetType().FullName)))
                    {
                        rendererName = DataContext.Texture != null ? DefaultSpriteViewer.ViewerName : NoTextureViewer.ViewerName;

                        if (string.Equals(Renderer?.Name, rendererName, StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                    }
                    else
                    {
                        rendererName = DataContext.CurrentPanel.GetType().FullName;
                    }

                    // Only switch if we're not on the same renderer.
                    if (!string.Equals(Renderer?.Name, rendererName, StringComparison.OrdinalIgnoreCase))
                    {
                        SwitchRenderer(rendererName, false);
                    }

                    UpdateSpriteDimensionsPanel();
                    UpdateArrayPanel();
                    break;
                case nameof(ISpriteContent.CommandContext):
                    _spriteInfo = (DataContext.CommandContext as ISpriteInfo) ?? DataContext; 
                    _arrayUpdater = DataContext.CommandContext as IArrayUpdate;                    

                    if ((DataContext.CommandContext == null) || (!HasRenderer(DataContext.CommandContext.Name)))
                    {
                        rendererName = DataContext.Texture != null ? DefaultSpriteViewer.ViewerName : NoTextureViewer.ViewerName;
                    }
                    else
                    {
                        DataContext.CommandContext.OnLoad();
                        rendererName = DataContext.CommandContext.Name;                        
                    }

                    // Only switch if we're not on the same renderer.
                    if (!string.Equals(Renderer?.Name, rendererName, StringComparison.OrdinalIgnoreCase))
                    {
                        SwitchRenderer(rendererName, false);
                    }
                    UpdateSpriteDimensionsPanel();
                    UpdateArrayPanel();
                    break;
                case nameof(ISpriteContent.ArrayIndex):                                        
                    UpdateArrayPanel();
                    break;
            }

            ValidateButtons();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Resize"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (DataContext == null)
            {
                return;
            }

            ManualInputWindowPositioning(_manualRectEditor, DataContext.Settings.ManualRectangleEditorBounds);
            ManualInputWindowPositioning(_manualVertexEditor, DataContext.Settings.ManualVertexEditorBounds);
        }

        /// <summary>Function called to shut down the view and perform any clean up required (including user defined graphics objects).</summary>
        protected override void OnShutdown()
        {
            DataContext?.OnUnload();
                        
            _clipperService?.Dispose();            
            _vertexEditService?.Dispose();
            _marchAnts?.Dispose();
            _anchorTexture?.Dispose();

            base.OnShutdown();
        }

        /// <summary>Handles the ParentChanged event of the SpriteEditorView control.</summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnParentChanged(EventArgs e)
        {
            if (_parentForm != null)
            {
                _parentForm.Move -= ParentForm_Move;
                _parentForm = null;
            }

            if (ParentForm != null)
            {
                // This will allow us to reposition our pop up window -after- the parent form stops moving.  Move will just fire non-stop, and the window 
                // position will be updated constantly (which actually slows things down quite a bit).  
                _parentForm = ParentForm;
                _parentForm.Move += ParentForm_Move;
            }
        }

        /// <summary>Function called when the renderer is switched.</summary>
        /// <param name="renderer">The current renderer.</param>
        /// <param name="resetZoom"><b>true</b> if the zoom should be reset, <b>false</b> if not.</param>
        protected override void OnSwitchRenderer(IContentRenderer renderer, bool resetZoom)
        {
            base.OnSwitchRenderer(renderer, resetZoom);
            _ribbonForm.ContentRenderer = renderer;

            if (renderer == null)
            {
                return;
            }

            var spriteViewer = (ISpriteViewer)renderer;
            spriteViewer.DefaultZoom();

            if (resetZoom)
            {
                _ribbonForm.ResetZoom();
            }
        }

        /// <summary>Function to allow applications to set up rendering for their specific use cases.</summary>
        /// <param name="context">The graphics context from the host application.</param>
        /// <param name="swapChain">The current swap chain for the rendering panel.</param>
        protected override void OnSetupContentRenderer(IGraphicsContext context, GorgonSwapChain swapChain)
        {
            base.OnSetupContentRenderer(context, swapChain);

            using (var stream = new MemoryStream(Resources.anchor_24x24))
            {
                _anchorTexture = GorgonTexture2DView.FromStream(context.Graphics, stream, new GorgonCodecDds(),
                    options: new GorgonTexture2DLoadOptions
                    {
                        Name = "Sprite Editor Anchor Sprite",
                        Binding = TextureBinding.ShaderResource,
                        Usage = ResourceUsage.Immutable
                    });
            }

            _marchAnts = new MarchingAnts(context.Renderer2D);
            _clipperService = new RectClipperService(context.Renderer2D, _marchAnts);
            _pickService = new PickClipperService();
            _vertexEditService = new SpriteVertexEditService(context.Renderer2D);
            _anchorService = new AnchorEditService(context.Renderer2D, new GorgonSprite
            {
                Texture = _anchorTexture,
                Size = new DX.Size2F(_anchorTexture.Width, _anchorTexture.Height),
                // Place the hotspot on rope hole at the top of the handle.
                Anchor = new DX.Vector2(0.5f, 0.125f)
            }, DataContext?.AnchorEditor?.Bounds ?? new DX.Rectangle
            {
                Left = -context.Graphics.VideoAdapter.MaxTextureWidth / 2,
                Top = -context.Graphics.VideoAdapter.MaxTextureHeight / 2,
                Right = context.Graphics.VideoAdapter.MaxTextureWidth / 2 - 1,
                Bottom = context.Graphics.VideoAdapter.MaxTextureHeight / 2 - 1,
            });

            ISpriteViewer noTexture = new NoTextureViewer(context.Renderer2D, swapChain, DataContext);
            ISpriteViewer defaultViewer = new DefaultSpriteViewer(context.Renderer2D, swapChain, DataContext, _marchAnts);            
            var clipViewer = new ClipSpriteViewer(context.Renderer2D, swapChain, DataContext, _clipperService);
            var pickViewer = new PickSpriteViewer(context.Renderer2D, swapChain, DataContext, _pickService, _marchAnts);
            var colorEditViewer = new ColorEditViewer(context.Renderer2D, swapChain, DataContext);
            var vertexEditViewer = new VertexEditViewer(context.Renderer2D, swapChain, DataContext, _vertexEditService);
            var anchorEditViewer = new AnchorEditViewer(context.Renderer2D, swapChain, DataContext, _anchorService);
            var wrapEditViewer = new TextureWrapViewer(context.Renderer2D, swapChain, DataContext);
            noTexture.CreateResources();
            defaultViewer.CreateResources();
            clipViewer.CreateResources();
            pickViewer.CreateResources();
            colorEditViewer.CreateResources();
            vertexEditViewer.CreateResources();
            anchorEditViewer.CreateResources();
            wrapEditViewer.CreateResources();

            AddRenderer(noTexture.Name, noTexture);            
            AddRenderer(defaultViewer.Name, defaultViewer);
            AddRenderer(clipViewer.Name, clipViewer);
            AddRenderer(pickViewer.Name, pickViewer);
            AddRenderer(colorEditViewer.Name, colorEditViewer);
            AddRenderer(vertexEditViewer.Name, vertexEditViewer);
            AddRenderer(anchorEditViewer.Name, anchorEditViewer);
            AddRenderer(wrapEditViewer.Name, wrapEditViewer);

            string currentRenderer = DataContext.Texture == null ? noTexture.Name : defaultViewer.Name;
            SwitchRenderer(currentRenderer, true);

            clipViewer.ToggleManualInput += ClipViewer_ToggleManualInput;
            vertexEditViewer.ToggleManualInput += VertexEditViewer_ToggleManualInput;

            ValidateButtons();
        }        

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
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
            ShowFocusState(true);
            RenderControl?.Select();

            ValidateButtons();
        }

        /// <summary>Handles the GotFocus event of the RenderControl control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RenderControl_GotFocus(object sender, EventArgs e) => ShowFocusState(true);

        /// <summary>Handles the LostFocus event of the RenderControl control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RenderControl_LostFocus(object sender, EventArgs e) => ShowFocusState(false);

        /// <summary>
        /// Function used to update the position of the manual input window.
        /// </summary>
        /// <param name="manualInput">The form being positioned.</param>
        /// <param name="prevPositioning">The previous position of the window.</param>
        private void ManualInputWindowPositioning(Form manualInput, DX.Rectangle? prevPositioning)
        {
            if ((manualInput == null) || (!manualInput.Visible))
            {
                return;
            }

            manualInput.ResizeEnd -= ManualInput_ResizeEnd;
            try
            {
                if (prevPositioning == null)
                {
                    manualInput.Location = RenderControl.PointToScreen(new Point(RenderControl.ClientSize.Width - manualInput.Width, 0));
                    return;
                }

                var pt = new Point(prevPositioning.Value.X, prevPositioning.Value.Y);
                if (!Screen.AllScreens.Any(item => item.WorkingArea.Contains(pt)))
                {
                    manualInput.Location = RenderControl.PointToScreen(new Point(RenderControl.ClientSize.Width - manualInput.Width, 0));
                    DataContext.Settings.ManualRectangleEditorBounds = null;
                    DataContext.Settings.ManualVertexEditorBounds = null;
                    return;
                }

                manualInput.Location = pt;
            }
            finally
            {
                manualInput.ResizeEnd += ManualInput_ResizeEnd;
            }
        }

        /// <summary>Function to unassign events for the data context.</summary>
        protected override void UnassignEvents()
        {
            base.UnassignEvents();

            if (DataContext?.SpriteClipContext != null)
            {
                DataContext.SpriteClipContext.PropertyChanged -= SpriteClipContext_PropertyChanged;
            }

            if (DataContext?.SpritePickContext != null)
            {
                DataContext.SpritePickContext.PropertyChanged -= SpritePickContext_PropertyChanged;
            }
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(ISpriteContent dataContext)
        {
            OnSetDataContext(dataContext);

            InitializeFromDataContext(dataContext);

            DataContext = dataContext;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="SpriteEditorView"/> class.</summary>
        public SpriteEditorView()
        {
            InitializeComponent();

            _ribbonForm = new FormRibbon();
            _ribbonForm.ButtonClipManualInput.Click += ClipViewer_ToggleManualInput;
            _ribbonForm.ButtonSpriteCornerManualInput.Click += VertexEditViewer_ToggleManualInput;
            Ribbon = _ribbonForm.RibbonSpriteContent;

            if (RenderWindow != null)
            {
                RenderWindow.PreviewKeyDown += PanelRenderWindow_PreviewKeyDown;
            }
            
            _manualRectEditor = new FormManualRectangleEdit();
            _manualRectEditor.ResizeEnd += ManualInput_ResizeEnd;
            _manualRectEditor.FormClosing += ManualInput_ClosePanel;

            _manualVertexEditor = new FormManualVertexEdit();
            _manualVertexEditor.ResizeEnd += ManualVertexInput_ResizeEnd;
            _manualVertexEditor.FormClosing += ManualVertexInput_ClosePanel;

            RegisterChildPanel(typeof(SpriteColorEdit).FullName, SpriteColorSelector);
            RegisterChildPanel(typeof(SpritePickMaskEditor).FullName, SpritePickMaskColor);
            RegisterChildPanel(typeof(SpriteAnchorEdit).FullName, SpriteAnchorSelector);            
            RegisterChildPanel(typeof(SpriteTextureWrapEdit).FullName, SpriteWrapping);
        }
        #endregion
    }
}
