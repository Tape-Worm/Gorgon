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
using System.Collections.Generic;
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
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Timing;
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
        // The previous zoom setting for restoration.
        private ZoomLevels _oldZoomLevel = ZoomLevels.ToWindow;
        // The previous scroll setting for restoration.
        private DX.Vector2 _oldScroll;
        // The rectangle clipping service used to capture sprite data.
        private IRectClipperService _clipperService;
        // The service used to edit the sprite vertices.
        //private ISpriteVertexEditService _vertexEditService;
        // The picking service used to capture sprite data.
        private IPickClipperService _pickService;
        // The anchor editing service.
        //private IAnchorEditService _anchorService;
        // Marching ants renderer.
        private IMarchingAnts _marchAnts;
        // Flag to indicate that the surface is dragging.
        private bool _isDragging;
        // The starting point of the drag operation.
        private DX.Vector2 _dragStart;
        // Manual input for the sprite clipping rectangle.
        //private readonly FormManualRectangleEdit _manualRectEditor;
        // Manual input for the sprite vertex editing.
        //private readonly FormManualVertexEdit _manualVertexEditor;
        // The UI refresh timer, used to keep UI elements from updating too rapidly.
        private readonly IGorgonTimer _uiRefreshTimer = GorgonTimerQpc.SupportsQpc() ? (IGorgonTimer)new GorgonTimerMultimedia() : new GorgonTimerQpc();
        // The parent form that is hosting this control hierarchy.
        private Form _parentForm;
        // The views to render depending on state.
        //private readonly Dictionary<string, ISpriteContentRenderer> _renderers = new Dictionary<string, ISpriteContentRenderer>(StringComparer.OrdinalIgnoreCase);
        // Flag to indicate that scrolling should be disabled.
        private bool _disableScrolling;
        // The current sprite renderer.
        //private ISpriteContentRenderer _currentRenderer;
        // The texture for the anchor icon.
        private GorgonTexture2DView _anchorTexture;
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
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
        private void ValidateControls()
        {
            _ribbonForm.ValidateButtons();
            
            if (DataContext?.Texture == null)
            {
                LabelSpriteInfo.Visible = false;
                LabelArrayIndex.Visible = LabelArrayIndexDetails.Visible = ButtonNextArrayIndex.Visible = ButtonPrevArrayIndex.Visible = false;
                return;
            }
            
            int arrayIndex = DataContext.ArrayIndex;
            /*
            if (DataContext.CurrentTool != SpriteEditTool.None)
            {
                arrayIndex = _currentRenderer.TextureArrayIndex;
            }*/

            LabelArrayIndex.Visible = LabelArrayIndexDetails.Visible = LabelSpriteInfo.Visible = true;
            ButtonNextArrayIndex.Visible = ButtonPrevArrayIndex.Visible = DataContext.SupportsArrayChange;
            ButtonNextArrayIndex.Enabled = (arrayIndex) < DataContext.Texture.Texture.ArrayCount - 1;
            ButtonPrevArrayIndex.Enabled = (arrayIndex) > 0;            
        }

        /// <summary>Handles the Click event of the ButtonPrevArrayIndex control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPrevArrayIndex_Click(object sender, EventArgs e)
        {
            //--_currentRenderer.TextureArrayIndex;
            ValidateControls();

            UpdateArrayPanel(DataContext);
        }

        /// <summary>Handles the Click event of the ButtonNextArrayIndex control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonNextArrayIndex_Click(object sender, EventArgs e)
        {
            //++_currentRenderer.TextureArrayIndex;
            ValidateControls();

            UpdateArrayPanel(DataContext);
        }

        /// <summary>Handles the Move event of the ParentForm control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ParentForm_Move(object sender, EventArgs e)
        {
            //ManualInputWindowPositioning(_manualRectEditor);
            //ManualInputWindowPositioning(_manualVertexEditor);
        }

        /// <summary>Handles the PropertyChanged event of the ManualVertexEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void ManualVertexEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            /*switch (e.PropertyName)
            {
                case nameof(IManualVertexEditor.IsActive):
                    if (DataContext.ManualVertexEditor.IsActive)
                    {
                        ManualInputWindowPositioning(_manualVertexEditor);
                        return;
                    }

                    ManualInput_ResizeEnd(_manualVertexEditor, EventArgs.Empty);
                    break;
            }*/
        }

        /// <summary>Handles the PropertyChanged event of the ManualRectangleEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void ManualRectangleEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            /*switch (e.PropertyName)
            {
                case nameof(IManualRectangleEditor.IsActive):
                    if (DataContext.ManualRectangleEditor.IsActive)
                    {
                        ManualInputWindowPositioning(_manualRectEditor);
                        return;
                    }

                    ManualInput_ResizeEnd(_manualRectEditor, EventArgs.Empty);
                    break;
            }*/
        }

        /// <summary>Handles the ResizeEnd event of the ManualInput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ManualInput_ResizeEnd(object sender, EventArgs e)
        {
            /*var form = (IManualInputControl)sender;
            Point pos = RenderControl.PointToScreen(new Point(RenderControl.ClientSize.Width - 128, 0));
            var snapRect = new Rectangle(pos.X, pos.Y, 128, 128);

            if ((form.DesktopBounds.Right >= snapRect.Left) && (form.DesktopBounds.Right <= snapRect.Right)
                && (form.DesktopBounds.Top >= snapRect.Top) && (form.DesktopBounds.Top <= snapRect.Bottom))
            {
                // Disable to stop the form from being dragged any further.
                form.Enabled = false;
                if (DataContext != null)
                {
                    form.DataContext.Position = null;
                }
                ManualInputWindowPositioning(form);
                form.Enabled = true;
                return;
            }

            form.DataContext.Position = new DX.Point(form.DesktopBounds.Left, form.DesktopBounds.Top);*/
        }

        /// <summary>Handles the ClosePanel event of the ManualRectangleEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ManualInput_ClosePanel(object sender, FormClosingEventArgs e)
        {
            /*var form = (IManualInputControl)sender;

            if (DataContext == null)
            {
                return;
            }

            if (e.CloseReason == CloseReason.UserClosing)
            {
                IManualInputViewModel viewModel = form.DataContext;

                if (viewModel == null)
                {
                    form.Hide();
                }
                else
                {
                    viewModel.IsActive = false;
                }

                e.Cancel = true;
            }*/
        }

        /// <summary>Handles the Move event of the ManualInput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ManualInput_Move(object sender, EventArgs e) => Idle();

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
                case Keys.OemPeriod:
                    if (ButtonNextArrayIndex.Enabled)
                    {
                        ButtonNextArrayIndex.PerformClick();
                    }
                    break;
                case Keys.Oemcomma:
                    if (ButtonPrevArrayIndex.Enabled)
                    {
                        ButtonPrevArrayIndex.PerformClick();
                    }
                    break;
            }
        }

        /// <summary>
        /// Function to handle idle time and rendering.
        /// </summary>
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
        private bool Idle()
        {
            // Update every 2nd frame (if we're frame limited on the presentation).
            if (_uiRefreshTimer.Milliseconds > 120)
            {
                UpdateSpriteDimensionsPanel(DataContext);
                _uiRefreshTimer.Reset();
            }

            //_currentRenderer?.Render();

            return true;
        }

        /// <summary>
        /// Function to initialize the view from the data context.
        /// </summary>
        /// <param name="dataContext">The data context to use.</param>
        private void InitializeFromDataContext(ISpriteContent dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            _ribbonForm.SetDataContext(dataContext);
            /*_manualRectEditor.SetDataContext(dataContext?.ManualRectangleEditor);
            _manualVertexEditor.SetDataContext(dataContext?.ManualVertexEditor);
            SpriteColorSelector.SetDataContext(dataContext?.ColorEditor);
            SpriteAnchorSelector.SetDataContext(dataContext?.AnchorEditor);
            SpritePickMaskColor.SetDataContext(dataContext?.SpritePickMaskEditor);
            SpriteWrapping.SetDataContext(dataContext?.WrappingEditor);*/

            UpdateArrayPanel(dataContext);
            UpdateSpriteDimensionsPanel(dataContext);

            /*
            if (DataContext?.ManualRectangleEditor != null)
            {
                DataContext.ManualRectangleEditor.PropertyChanged += ManualRectangleEditor_PropertyChanged;
            }

            if (DataContext?.ManualVertexEditor != null)
            {
                DataContext.ManualVertexEditor.PropertyChanged += ManualVertexEditor_PropertyChanged;
            }*/
        }

        /// <summary>
        /// Function to update the sprite dimensions control.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void UpdateSpriteDimensionsPanel(ISpriteContent dataContext)
        {
            if (dataContext?.Texture == null)
            {
                return;
            }

            /*string spriteInfoText = _currentRenderer == null ? string.Empty : _currentRenderer.SpriteInfo;

            if (string.Equals(spriteInfoText, LabelSpriteInfo.Text, StringComparison.CurrentCulture))
            {
                return;
            }

            LabelSpriteInfo.Text = spriteInfoText;*/
        }

        /// <summary>
        /// Function to update the panel with the array controls.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void UpdateArrayPanel(ISpriteContent dataContext)
        {
            if (dataContext == null)
            {
                return;
            }

            int arrayIndex = dataContext.ArrayIndex;

            /*switch (dataContext.CurrentTool)
            {
                case SpriteEditTool.SpriteClip:
                case SpriteEditTool.SpritePick:
                    arrayIndex = _currentRenderer.TextureArrayIndex;
                    break;
            }*/

            LabelArrayIndexDetails.Text = $"{arrayIndex + 1}/{dataContext.Texture?.Texture.ArrayCount ?? 0}";
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
            /*switch (e.PropertyName)
            {
                case nameof(ISpriteContent.CurrentPanel):
                    if (DataContext.CurrentPanel != null)
                    {
                        DataContext.CurrentPanel.IsActive = false;
                    }

                    ClearPanelHost();
                    IsPanelHostActive = false;
                    break;
                case nameof(ISpriteContent.CurrentTool):
                    RenderControl.Cursor = Cursor.Current = Cursors.Default;
                    ValidateControls();
                    break;
            }*/
        }

        /// <summary>Function called when a property is changed on the data context.</summary>        
        /// <param name="propertyName">The name of the property that is updated.</param>
        /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
        protected override void OnPropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(ISpriteContent.Texture):
                case nameof(ISpriteContent.ImageData):
                    SwitchRenderer(nameof(SpriteEditTool.None), true);

                    //RenderControl.Cursor = _currentRenderer.CurrentCursor;

                    UpdateArrayPanel(DataContext);
                    UpdateSpriteDimensionsPanel(DataContext);
                    break;
/*                case nameof(ISpriteContent.CurrentTool):
                    SwitchRenderer(DataContext);
                    RenderControl.Cursor = _currentRenderer.CurrentCursor;
                    UpdateArrayPanel(DataContext);
                    UpdateSpriteDimensionsPanel(DataContext);
                    break;
                case nameof(ISpriteContent.ArrayIndex):
                    _currentRenderer.TextureArrayIndex = DataContext.ArrayIndex;
                    UpdateArrayPanel(DataContext);
                    break;
                case nameof(ISpriteContent.CurrentPanel):
                    if (DataContext.CurrentPanel != null)
                    {
                        Control panel = GetRegisteredPanel<Control>(DataContext.CurrentPanel.GetType());
                        IsPanelHostActive = true;
                        panel.Visible = true;
                        AddControlToPanelHost(panel);
                        DataContext.ColorEditor.IsActive = true;
                    }

                    SwitchRenderer(DataContext);
                    break;*/
            }

            ValidateControls();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Resize"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            /*if ((DataContext?.Texture == null) || (_currentRenderer == null))
            {
                return;
            }

            // Resize the image to fit the new boundaries.
            if (_ribbonForm.ZoomLevel == ZoomLevels.ToWindow)
            {
                ChangeZoomLevel(ZoomLevels.ToWindow);
            }
            else
            {
                string panelID = DataContext.CurrentPanel?.GetType().FullName;

                if (!string.IsNullOrWhiteSpace(panelID))
                {
                    UpdateRenderer(_ribbonForm.ZoomScaling, GetScrollValue(), 0.5f, false);
                    Control panel = GetRegisteredPanel<Control>(panelID);

                    if (panel == null)
                    {
                        ShiftRenderRegionForPanel(false, 0);
                    }
                    else
                    {
                        ShiftRenderRegionForPanel(DataContext.CurrentPanel.IsModal, panel.Width);
                    }
                }
                else
                {
                    ChangeZoomLevel(ZoomLevels.ToWindow);
                }
            }

            UpdateRenderer(_ribbonForm.ZoomScaling, GetScrollValue(), _currentRenderer.InitialTextureAlpha);

            ManualInputWindowPositioning(_manualRectEditor);
            ManualInputWindowPositioning(_manualVertexEditor);*/
        }
        /*
        /// <summary>Function to allow user defined setup of the graphics context with this control.</summary>
        /// <param name="context">The context being assigned.</param>
        /// <param name="swapChain">The swap chain assigned to the <see cref="P:Gorgon.Editor.UI.Views.ContentBaseControl.RenderControl"/>.</param>
        private void SetupGraphics123(IGraphicsContext context, GorgonSwapChain swapChain)
        {
            _marchAnts = new MarchingAnts(context.Renderer2D);
            _clipperService = new RectClipperService(context.Renderer2D, _marchAnts);
            _vertexEditService = new SpriteVertexEditService(context.Renderer2D);

            _pickService = new PickClipperService
            {
                ClipMask = DataContext?.Settings?.ClipMaskType ?? ClipMask.Alpha,
                ClipMaskValue = DataContext?.Settings?.ClipMaskValue ?? new GorgonColor(1, 0, 1, 0)
            };

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

            _anchorService = new AnchorEditService(context.Renderer2D, new GorgonSprite
            {
                Texture = _anchorTexture,
                Size = new DX.Size2F(_anchorTexture.Width, _anchorTexture.Height),
                // Place the hotspot on rope hole at the top of the handle.
                Anchor = new DX.Vector2(0.5f, 0.125f)
            });

            ChangeZoomLevel(ZoomLevels.ToWindow);

            _renderers["null"] = new NoTextureRenderer(context.Graphics, swapChain, context.Renderer2D);
            _renderers["default"] = new DefaultSpriteRenderer(DataContext, context.Graphics, swapChain, context.Renderer2D, _marchAnts, _ribbonForm.ZoomScaling);
            _renderers[nameof(SpriteEditTool.SpriteClip)] = new SpriteClipRenderer(DataContext, context.Graphics, swapChain, context.Renderer2D, _clipperService, _ribbonForm.ZoomScaling);
            _renderers[nameof(SpriteEditTool.SpritePick)] = new SpritePickRenderer(DataContext, context.Graphics, swapChain, context.Renderer2D, _pickService, _marchAnts, _ribbonForm.ZoomScaling);
            _renderers[typeof(SpriteColorEdit).FullName] = new SpriteColorRenderer(DataContext, context.Graphics, swapChain, context.Renderer2D, _ribbonForm.ZoomScaling);
            _renderers[typeof(SpriteAnchorEdit).FullName] = new SpriteAnchorRenderer(DataContext, context.Graphics, swapChain, context.Renderer2D, _anchorService, _ribbonForm.ZoomScaling);
            _renderers[nameof(SpriteEditTool.CornerResize)] = new SpriteVertexOffsetRenderer(DataContext, context.Graphics, swapChain, context.Renderer2D, _vertexEditService, _ribbonForm.ZoomScaling);
            _renderers[typeof(SpriteWrappingEditor).FullName] = new SpriteWrappingRenderer(DataContext, context.Graphics, swapChain, context.Renderer2D, _ribbonForm.ZoomScaling);

            foreach (ISpriteContentRenderer renderer in _renderers.Values)
            {
                renderer.BackgroundColor = RenderControl.BackColor;
                renderer.ScrollUpdated += Renderer_ScrollUpdated;
            }

            SwitchRenderer(DataContext);
        }
        */

        /// <summary>Function called to shut down the view and perform any clean up required (including user defined graphics objects).</summary>
        protected override void OnShutdown()
        {
            DataContext?.OnUnload();

            _clipperService?.Dispose();
            //_vertexEditService?.Dispose();
            _marchAnts?.Dispose();
            _anchorTexture?.Dispose();
            /*
            foreach (ISpriteContentRenderer renderer in _renderers.Values)
            {
                renderer.ScrollUpdated -= Renderer_ScrollUpdated;
                renderer.Dispose();
            }*/
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

        /// <summary>Function to allow applications to set up rendering for their specific use cases.</summary>
        /// <param name="context">The graphics context from the host application.</param>
        /// <param name="swapChain">The current swap chain for the rendering panel.</param>
        protected override void OnSetupContentRenderer(IGraphicsContext context, GorgonSwapChain swapChain)
        {
            base.OnSetupContentRenderer(context, swapChain);

            _marchAnts = new MarchingAnts(context.Renderer2D);

            ISpriteViewer noTexture = new NoTextureViewer(context.Renderer2D, swapChain, DataContext);
            ISpriteViewer defaultViewer = new DefaultSpriteViewer(context.Renderer2D, swapChain, DataContext, _marchAnts);
            noTexture.CreateResources();
            defaultViewer.CreateResources();

            AddRenderer(noTexture.Name, noTexture);
            AddRenderer(nameof(SpriteEditTool.None), defaultViewer);

            SwitchRenderer(DataContext.Texture == null ? noTexture.Name : defaultViewer.Name, true);
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

            ValidateControls();
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
        /*private void ManualInputWindowPositioning(IManualInputControl manualInput)
        {
            if ((manualInput?.DataContext == null) || (!manualInput.Visible))
            {
                return;
            }

            manualInput.ResizeEnd -= ManualInput_ResizeEnd;
            try
            {
                if (manualInput.DataContext.Position == null)
                {
                    manualInput.Location = RenderControl.PointToScreen(new Point(RenderControl.ClientSize.Width - manualInput.Width, 0));
                    return;
                }

                var pt = new Point(manualInput.DataContext.Position.Value.X, manualInput.DataContext.Position.Value.Y);
                if (!Screen.AllScreens.Any(item => item.WorkingArea.Contains(pt)))
                {
                    manualInput.Location = RenderControl.PointToScreen(new Point(RenderControl.ClientSize.Width - manualInput.Width, 0));
                    manualInput.DataContext.Position = null;
                    return;
                }

                manualInput.Location = pt;
            }
            finally
            {
                manualInput.ResizeEnd += ManualInput_ResizeEnd;
            }
        }*/

        /// <summary>Function to unassign events for the data context.</summary>
        protected override void UnassignEvents()
        {
            base.UnassignEvents();

            /*if (DataContext?.ManualRectangleEditor != null)
            {
                DataContext.ManualRectangleEditor.PropertyChanged -= ManualRectangleEditor_PropertyChanged;
            }

            if (DataContext?.ManualVertexEditor != null)
            {
                DataContext.ManualVertexEditor.PropertyChanged -= ManualVertexEditor_PropertyChanged;
            }*/
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
            Ribbon = _ribbonForm.RibbonSpriteContent;
            
            /*
            _manualRectEditor = new FormManualRectangleEdit();
            _manualRectEditor.ResizeEnd += ManualInput_ResizeEnd;
            _manualRectEditor.FormClosing += ManualInput_ClosePanel;
            _manualRectEditor.Move += ManualInput_Move;

            _manualVertexEditor = new FormManualVertexEdit();
            _manualVertexEditor.ResizeEnd += ManualInput_ResizeEnd;
            _manualVertexEditor.FormClosing += ManualInput_ClosePanel;
            _manualVertexEditor.Move += ManualInput_Move;

            RegisterChildPanel(typeof(SpriteColorEdit).FullName, SpriteColorSelector);
            RegisterChildPanel(typeof(SpriteAnchorEdit).FullName, SpriteAnchorSelector);
            RegisterChildPanel(typeof(ISpritePickMaskEditor).FullName, SpritePickMaskColor);
            RegisterChildPanel(typeof(SpriteWrappingEditor).FullName, SpriteWrapping);*/
        }
        #endregion
    }
}
