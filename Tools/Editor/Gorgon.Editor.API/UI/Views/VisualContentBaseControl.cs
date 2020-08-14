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
// Created: February 6, 2020 8:21:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DX = SharpDX;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Properties;
using Gorgon.Core;
using Gorgon.Math;
using System.Threading;
using Gorgon.Graphics;
using Gorgon.Editor.UI.Controls;
using Gorgon.Timing;

namespace Gorgon.Editor.UI.Views
{
    /// <summary>
    /// A base content editor view that is used to display renderable content.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This base editor view is used to render custom content and provide controls/functionality for zooming, and panning that content. This content editor view takes a view model that implements the 
    /// <see cref="IVisualEditorContent"/> interface.
    /// </para>
    /// <para>
    /// Content plug in developers should inherit from this control to provide standardized functionality for viewing content via the <see cref="GorgonGraphics"/>, and <see cref="Gorgon2D"/> interfaces. 
    /// This should help simplify plug in UI development and allow developers to focus on creating UIs for editing their content without having to worry about developing boilerplate code for view 
    /// manipulation.
    /// </para>
    /// <para>
    /// By default this control will render a background using a checkerboard texture to illustrate opacity. This can be overridden by the developer on the view model.
    /// </para>
    /// </remarks>
    public partial class VisualContentBaseControl
        : ContentBaseControl
    {
        #region Variables.
        // A list of renderers used to draw our content to the UI.
        private readonly Dictionary<string, IContentRenderer> _renderers = new Dictionary<string, IContentRenderer>(StringComparer.OrdinalIgnoreCase);
        // Flag for event transformation registration.
        private int _transformEventRegister;
        // The data context for the view.
        private IVisualEditorContent _dataContext;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the current renderer.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected IContentRenderer Renderer
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return whether horizontal scrolling is supported.
        /// </summary>
        [Browsable(true), Category("Design"), Description("Shows or hides the horizontal scroll bar."), DefaultValue(true)]
        public bool ShowHorizontalScrollBar
        {
            get => ScrollHorizontal.Visible;
            set
            {
                if (ScrollHorizontal.Visible == value)
                {
                    return;
                }

                ScrollHorizontal.Visible = value;
                ButtonCenter.Visible = ScrollHorizontal.Visible && ScrollVertical.Visible;

                if (Renderer != null)
                {
                    Renderer.CanPanHorizontally = value;
                    Renderer.SetOffset(DX.Vector2.Zero);
                }
            }
        }
        /// <summary>
        /// Property to set or return whether vertical scrolling is supported.
        /// </summary>
        [Browsable(true), Category("Design"), Description("Shows or hides the vertical scroll bar."), DefaultValue(true)]
        public bool ShowVerticalScrollBar
        {
            get => ScrollVertical.Visible;
            set
            {
                if (ScrollVertical.Visible == value)
                {
                    return;
                }

                ScrollVertical.Visible = value;
                ButtonCenter.Visible = ScrollHorizontal.Visible && ScrollVertical.Visible;

                if (Renderer != null)
                {
                    Renderer.CanPanVertically = value;
                    Renderer.SetOffset(DX.Vector2.Zero);
                }
            }
        }

        /// <summary>
        /// Property to return the panel that will be used for presentation of the content.
        /// </summary>
        [Browsable(false)]
        public Panel StatusPanel => PanelStatus;

        /// <summary>
        /// Property to return the panel that will be used to receive content rendering.
        /// </summary>
        [Browsable(false)]
        public Panel RenderWindow => PanelRenderWindow;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to handle idle time and rendering.
        /// </summary>
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
        private bool Idle()
        {
            OnBeforeRender();
            Renderer?.Render();
            OnAfterRender();

            return true;
        }

        /// <summary>
        /// Function to enable the events for the scroll bars.
        /// </summary>
        private void EnableScrollEvents()
        {
            if (Interlocked.Exchange(ref _transformEventRegister, 1) == 1)
            {
                return;
            }

            ScrollHorizontal.ValueChanged += ScrollHorizontal_ValueChanged;
            ScrollVertical.ValueChanged += ScrollVertical_ValueChanged;
        }

        /// <summary>
        /// Function to disable the events for the scroll bars.
        /// </summary>
        private void DisableScrollEvents()
        {
            ScrollHorizontal.ValueChanged -= ScrollHorizontal_ValueChanged;
            ScrollVertical.ValueChanged -= ScrollVertical_ValueChanged;

            Interlocked.Exchange(ref _transformEventRegister, 0);
        }

        /// <summary>Handles the Click event of the ButtonCenter control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonCenter_Click(object sender, EventArgs e)
        {
            if (Renderer == null)
            {
                return;
            }

            Renderer.MoveTo(DX.Vector2.Zero, - 1);
        }

        /// <summary>Handles the ValueChanged event of the ScrollVertical control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ScrollVertical_ValueChanged(object sender, EventArgs e)
        {
            if (Renderer == null)
            {
                return;
            }

            Renderer.SetOffset(new DX.Vector2(ScrollHorizontal.Value, ScrollVertical.Value));
            Renderer.Render();
        }

        /// <summary>Handles the ValueChanged event of the ScrollHorizontal control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ScrollHorizontal_ValueChanged(object sender, EventArgs e)
        {
            if (Renderer == null)
            {
                return;
            }

            Renderer.SetOffset(new DX.Vector2(ScrollHorizontal.Value, ScrollVertical.Value));
            Renderer.Render();
        }

        /// <summary>Handles the ZoomScale event of the CurrentRenderer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ZoomScaleEventArgs"/> instance containing the event data.</param>
        private void CurrentRenderer_ZoomScale(object sender, ZoomScaleEventArgs e) => SetupScrollBars();

        /// <summary>Handles the Offset event of the CurrentRenderer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="OffsetEventArgs"/> instance containing the event data.</param>
        private void CurrentRenderer_Offset(object sender, OffsetEventArgs e)
        {
            DisableScrollEvents();
            try
            {                
                ScrollHorizontal.Value = (int)e.Offset.X.Min(ScrollHorizontal.Maximum - 1).Max(ScrollHorizontal.Minimum);
                ScrollVertical.Value = (int)e.Offset.Y.Min(ScrollVertical.Maximum - 1).Max(ScrollVertical.Minimum);
            }
            finally
            {
                EnableScrollEvents();
            }
        }

        /// <summary>
        /// Function to set up the scroll bars.
        /// </summary>
        private void SetupScrollBars()
        {
            DisableScrollEvents();

            try
            {
                if ((Renderer == null) || (RenderControl == null))
                {
                    ScrollVertical.Enabled = ScrollHorizontal.Enabled = false;
                    return;
                }

                if (Renderer.CanPanHorizontally)
                {
                    int width = (int)(Renderer.RenderRegion.Width * 0.5f);

                    ScrollHorizontal.Enabled = true;
                    ScrollHorizontal.LargeChange = (int)(width * 0.25f).Max(1);
                    ScrollHorizontal.SmallChange = (int)(width * 0.1f).Max(1);
                    ScrollHorizontal.Maximum = width + ScrollHorizontal.LargeChange - 1;
                    ScrollHorizontal.Minimum = -width;
                }
                else
                {
                    ScrollHorizontal.Enabled = false;
                }

                if (!Renderer.CanPanVertically)
                {
                    ScrollVertical.Enabled = false;
                    return;
                }

                int height = (int)(Renderer.RenderRegion.Height * 0.5f);

                ScrollVertical.Enabled = true;
                ScrollVertical.LargeChange = (int)(height * 0.25f).Max(1);
                ScrollVertical.SmallChange = (int)(height * 0.1f).Max(1);
                ScrollVertical.Maximum = height + ScrollVertical.LargeChange - 1;
                ScrollVertical.Minimum = -height;
            }
            finally
            {
                EnableScrollEvents();
            }
        }

        /// <summary>Handles the DragDrop event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_DragDrop(object sender, DragEventArgs e) => OnRenderWindowDragDrop(e);

        /// <summary>Handles the DragEnter event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_DragEnter(object sender, DragEventArgs e) => OnRenderWindowDragEnter(e);

        /// <summary>Handles the DragOver event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_DragOver(object sender, DragEventArgs e) => OnRenderWindowDragOver(e);

        /// <summary>Handles the Resize event of the PanelRenderWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PanelRenderWindow_Resize(object sender, EventArgs e)
        {
            if (Renderer == null)
            {
                return;
            }

            SetupScrollBars();
        }

        /// <summary>
        /// Function to handle a drag enter event on the render control.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        /// <remarks>
        /// <para>
        /// Content editor developers can override this method to handle a drag enter event when an item is dragged into the rendering area on the view.
        /// </para>
        /// </remarks>
        protected virtual void OnRenderWindowDragEnter(DragEventArgs e) => OnBubbleDragEnter(e);

        /// <summary>
        /// Function to handle a drag over event on the render control.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        /// <remarks>
        /// <para>
        /// Content editor developers can override this method to handle a drag over event when an item is dragged over the rendering area on the view.
        /// </para>
        /// </remarks>
        protected virtual void OnRenderWindowDragOver(DragEventArgs e) => OnBubbleDragOver(e);

        /// <summary>
        /// Function to handle a drag drop event on the render control.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        /// <remarks>
        /// <para>
        /// Content editor developers can override this method to handle a drop event when an item is dropped into the rendering area on the view.
        /// </para>
        /// </remarks>
        protected virtual void OnRenderWindowDragDrop(DragEventArgs e) => OnBubbleDragDrop(e);

        /// <summary>Raises the <see cref="Control.Resize"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e) 
        {
            base.OnResize(e);

            if (Renderer == null)
            {
                return;
            }

            SetupScrollBars();
        }

        /// <summary>Function called when a property is changing on the data context.</summary>
        /// <param name="propertyName">The name of the property that is updating.</param>
        /// <remarks>Implementors should override this method in order to handle a property change notification from their data context.</remarks>
        protected override void OnPropertyChanging(string propertyName)
        {
            base.OnPropertyChanging(propertyName);

            switch (propertyName)
            {
                case nameof(IVisualEditorContent.CurrentPanel):
                    if (_dataContext.CurrentPanel == null)
                    {
                        break;
                    }

                    string viewModelTypeName = _dataContext.CurrentPanel.GetType().FullName;
                    Control hostControl = GetRegisteredPanel<EditorSubPanelCommon>(viewModelTypeName);

                    if (hostControl != null)
                    {
                        hostControl.Visible = false;
                    }

                    SetupScrollBars();
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
                case nameof(IVisualEditorContent.CurrentPanel):
                    if (_dataContext.CurrentPanel == null)
                    {
                        HideHostedPanels();
                        SetupScrollBars();
                        break;
                    }

                    string viewModelTypeName = _dataContext.CurrentPanel.GetType().FullName;
                    Control hostControl = GetRegisteredPanel<EditorSubPanelCommon>(viewModelTypeName);

                    ShowHostedPanel(hostControl);
                    SetupScrollBars();
                    break;
            }
        }

        /// <summary>
        /// Function called after rendering ends.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Developers can use this method to perform operations immediately after rendering the content.
        /// </para>
        /// </remarks>
        protected virtual void OnAfterRender()
        {

        }

        /// <summary>
        /// Function called before rendering begins.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Developers can use this method to perform last minute updates prior to rendering the content.
        /// </para>
        /// </remarks>
        protected virtual void OnBeforeRender()
        {

        }

        /// <summary>
        /// Function called when the renderer is switched.
        /// </summary>
        /// <param name="renderer">The current renderer.</param>
        /// <param name="resetZoom"><b>true</b> if the zoom should be reset, <b>false</b> if not.</param>
        protected virtual void OnSwitchRenderer(IContentRenderer renderer, bool resetZoom)
        {
        
        }

        /// <summary>
        /// Function to register a new renderer with the view.
        /// </summary>
        /// <param name="name">The name of the renderer.</param>
        /// <param name="renderer">The renderer to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <exception cref="ArgumentException">Thrown if a renderer is already registered with the same <paramref name="name"/>.</exception>
        /// <remarks>
        /// <para>
        /// Some content plug ins will require the rendered view to change depending on state. For example, a specific tool is selected and the view needs to switch to a portion of the content. In such 
        /// cases it is not practical to have a single renderer performing all manner of state changes, so this method provides a means of passing different renderer instances to the view.
        /// </para>
        /// <para>
        /// Developers of the content editing view should register these renderers in the <see cref="OnSetupGraphics(IGraphicsContext, GorgonSwapChain)"/> method so that the renderers are available right 
        /// away. Failure to do so can lead to broken rendering.
        /// </para>
        /// </remarks>
        /// <seealso cref="IContentRenderer"/>
        protected void AddRenderer(string name, IContentRenderer renderer)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentEmptyException(nameof(name));
            }

            if (_renderers.ContainsKey(name))
            {
                throw new ArgumentException(string.Format(Resources.GOREDIT_ERR_CONTENT_RENDERER_EXISTS, name), nameof(name));
            }

            _renderers[name] = renderer;
        }

        /// <summary>
        /// Function to determine if a renderer with the specified name is registered.
        /// </summary>
        /// <param name="name">The name of the renderer to evaluate.</param>
        /// <returns><b>true</b> if the renderer is registered, <b>false</b> if not.</returns>
        protected bool HasRenderer(string name)
        {
#pragma warning disable IDE0046 // Convert to conditional expression
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return string.IsNullOrWhiteSpace(name) ? throw new ArgumentEmptyException(nameof(name)) : _renderers.ContainsKey(name);
#pragma warning restore IDE0046 // Convert to conditional expression
        }

        /// <summary>
        /// Function to switch to another registered renderer.
        /// </summary>
        /// <param name="name">The name of the renderer to switch to.</param>
        /// <param name="resetZoom"><b>true</b> to reset the zoom back to the default, <b>false</b> to leave as-is.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <exception cref="ArgumentException">Thrown if no renderer with the specified <paramref name="name"/> was found.</exception>
        /// <remarks>
        /// <para>
        /// Developers will this method to switch between renderer types (if multiple renderers are used) to denote editor state. 
        /// </para>
        /// </remarks>
        protected void SwitchRenderer(string name, bool resetZoom)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentEmptyException(nameof(name));
            }

            if (!_renderers.TryGetValue(name, out IContentRenderer newRenderer))
            {
                throw new ArgumentException(string.Format(Resources.GOREDIT_ERR_CONTENT_RENDERER_NOT_FOUND, name), nameof(name));
            }

            DX.Vector2? offset = null;
            ZoomLevels zoomLevel = ZoomLevels.ToWindow;

            // Blow away any temporary resources registered to the renderer.
            if (Renderer != null)
            {
                offset = Renderer.Offset;
                zoomLevel = Renderer.ZoomLevel;
                Renderer.IsEnabled = false;
                Renderer.OffsetChanged -= CurrentRenderer_Offset;
                Renderer.ZoomScaleChanged -= CurrentRenderer_ZoomScale;
                Renderer.UnloadResources();
                OnSwitchRenderer(null, false);
            }
            Renderer = null;

            // Ensure temporary resources for the new renderer are available.
            newRenderer.LoadResources();
            Renderer = newRenderer;
            Renderer.IsEnabled = true;
            Renderer.ZoomScaleChanged += CurrentRenderer_ZoomScale;
            Renderer.OffsetChanged += CurrentRenderer_Offset;

            // Reset the current renderer back to the default zoom level.
            SetupScrollBars();

            if ((resetZoom) || (offset == null))
            {                
                Renderer.MoveTo(new DX.Vector2(ClientSize.Width * 0.5f, ClientSize.Height * 0.5f), -1);
            }
            else
            {
                Renderer.SetZoom(zoomLevel.GetScale());
                Renderer.SetOffset(offset.Value);                
            }

            OnSwitchRenderer(Renderer, resetZoom);

            ButtonCenter.Enabled = Renderer?.CanZoom ?? false;

            // Reset the timing for the idle loop so we don't end up with stupid delta values because it takes time to switch.
            GorgonTiming.Reset();
        }

        /// <summary>
        /// Function to allow applications to set up rendering for their specific use cases.
        /// </summary>
        /// <param name="context">The graphics context from the host application.</param>
        /// <param name="swapChain">The current swap chain for the rendering panel.</param>
        protected virtual void OnSetupContentRenderer(IGraphicsContext context, GorgonSwapChain swapChain)
        {

        }

        /// <summary>Function to allow user defined setup of the graphics context with this control.</summary>
        /// <param name="context">The context being assigned.</param>
        /// <param name="swapChain">The swap chain assigned to the <see cref="ContentBaseControl.RenderControl"/>.</param>        
        protected sealed override void OnSetupGraphics(IGraphicsContext context, GorgonSwapChain swapChain)
        {
            // Always insert the default renderer.
            _renderers["null"] = new DefaultContentRenderer<IVisualEditorContent>("null", context.Renderer2D, swapChain, _dataContext);

            OnSetupContentRenderer(context, swapChain);

            foreach (IContentRenderer renderer in _renderers.Values)
            {
                renderer.BackgroundColor = RenderControl?.BackColor ?? GorgonColor.White;
            }

            // If we didn't register any renderers, then do not switch to anything (nothing will be rendered).
            if (_renderers.Count == 0)
            {
                return;
            }

            if (Renderer == null)
            {
                _renderers["null"].IsEnabled = true;
                SwitchRenderer("null", true);
            }
            else
            {
                _renderers["null"].IsEnabled = false;
            }

            Renderer.CanPanHorizontally = ScrollHorizontal.Visible;
            Renderer.CanPanVertically = ScrollVertical.Visible;
        }

        /// <summary>
        /// Function to assign the data context to this object.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>
        /// <para>
        /// Applications must call this method when setting their own data context. Otherwise, some functionality will not work.
        /// </para>
        /// </remarks>
        protected void OnSetDataContext(IVisualEditorContent dataContext)
        {
            base.OnSetDataContext(dataContext);
            _dataContext = dataContext;
        }

        /// <summary>Function called to shut down the view and perform any clean up required (including user defined graphics objects).</summary>
        protected override void OnShutdown()
        {
            foreach (IContentRenderer renderer in _renderers.Values)
            {                
                renderer?.Dispose();
            }            

            base.OnShutdown();
        }

        /// <summary>Raises the <see cref="UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // If we're in design time, then do nothing. We don't want to potentially break the IDE.
            if (IsDesignTime)
            {
                return;
            }

            // Set the method used to render during idle time.
            IdleMethod = Idle;

            // Force keyboard focus to our render window.
            ShowFocusState(true);
            RenderControl.Select();
        }        
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="VisualContentBaseControl"/> class.</summary>
        public VisualContentBaseControl() => InitializeComponent();
        #endregion
    }
}
