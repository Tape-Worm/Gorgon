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
// Created: February 6, 2020 9:57:02 PM
// 
#endregion

using System;
using System.Linq;
using System.Threading;
using DX = SharpDX;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using Gorgon.Core;
using System.Windows.Forms;
using Gorgon.Math;
using Gorgon.Animation;
using Gorgon.Editor.UI;
using System.ComponentModel;

namespace Gorgon.Editor.Rendering
{
    /// <summary>
    /// A default implementation of a <see cref="IContentRenderer"/>.
    /// </summary>
    /// <typeparam name="T">The type of view model for the renderer. Must implement the <see cref="IVisualEditorContent"/> interface, and be a reference type.</typeparam>
    /// <remarks>
    /// <para>
    /// This renderer does the bare minimum to present content on the view. Content plug in UI developers should inherit from this class to take advantage of the default functionality it provides. 
    /// </para>
    /// <para>
    /// The default renderer provides support for panning, and zooming on content and will animate those functions. Inherited renderers will receive these functions, but can override them as needed. The 
    /// panning operation can activated by pressing CTRL + Middle mouse button, the scrollbars on the view control, or moving the mouse wheel (press Shift to move horizontally). Zooming can be activated by 
    /// moving the mouse wheel while holding the CTRL key.
    /// </para>
    /// <para>
    /// The panning and zooming operations (which were a pain in the ass to write) use a camera (a <see cref="Gorgon2DOrthoCamera"/>) to apply the offsets and scaling for the pan/zoom operations. This 
    /// camera is exposed to inherting classes through the <see cref="Camera"/> property and developers must pass this camera to the <see cref="Gorgon2D"/>.<see cref="Gorgon2D.Begin"/> method so that 
    /// panning and zooming can be applied to the content. Conversely, the developer can opt not to use the camera and render directly to the client area should they so choose. 
    /// </para>
    /// <para>
    /// When applying the camera to your own custom content rendering the coordinate system will change to be in world space, meaning that the content will be relative to the center of the content 
    /// dimensions. For example, if the content is 640x480, then 0x0 will be the center of the content, -320x-240 will be the upper left, and 320x240 will be the lower right. So rendering a sprite at 0x0 
    /// will put it in the center of the view.
    /// </para>
    /// <para>
    /// Renderers will also receive access to the view model applied to the view, so the renderer can respond to changes on the content and adjust the visuals appropriately. The view model must implement 
    /// the <see cref="IVisualEditorContent"/> interface before they can be used with a renderer.
    /// </para>
    /// </remarks>
    /// <seealso cref="IContentRenderer"/>
    /// <seealso cref="IVisualEditorContent"/>
    /// <seealso cref="Gorgon2D"/>
    /// <seealso cref="Gorgon2DOrthoCamera"/>
    public class DefaultContentRenderer<T>
        : GorgonNamedObject, IContentRenderer
        where T : class, IVisualEditorContent
    {
        #region Variables.
        // The synchronization lock for events.
        private readonly object _zoomEventLock = new object();
        private readonly object _offsetEventLock = new object();
        // Flag to indicate that the resources are loaded.
        private int _resourcesLoading;
        // The texture used to draw the background.
        private GorgonTexture2DView _backgroundPattern;
        // The swap chain for the content view.
        private readonly GorgonSwapChain _swapChain;
        // The camera for our content.
        private Gorgon2DOrthoCamera _camera;
        // The region to render the content and background into.
        private DX.RectangleF _renderRegion = DX.RectangleF.Empty;
        // The arguments for mouse events.
        private readonly MouseArgs _mouseArgs = new MouseArgs();
        // The argumments for zoom events.
        private readonly ZoomScaleEventArgs _zoomArgs = new ZoomScaleEventArgs();
        // The argumments for offset events.
        private readonly OffsetEventArgs _offsetArgs = new OffsetEventArgs();
        // The controller for a camera animation.
        private readonly CameraAnimationController<T> _camAnimController;
        // The builder for an animation.
        private readonly GorgonAnimationBuilder _animBuilder = new GorgonAnimationBuilder();        
        // The animation for manipulating the camera.
        private IGorgonAnimation _cameraAnimation;
        // Flag to indicate that panning is enabled.
        private bool _panHorzEnabled = true;
        private bool _panVertEnabled = true;
        // The starting point for the pan drag.
        private DX.Vector2? _panDragStart;
        private DX.Vector3 _camDragStart;        
        #endregion

        #region Events.
        // The event triggered when the camera is zoomed.
        private EventHandler<ZoomScaleEventArgs> ZoomEvent;
        // The event triggered when the camera is moved.
        private EventHandler<OffsetEventArgs> OffsetEvent;

        /// <summary>
        /// Event triggered when the camera is moved.
        /// </summary>
        event EventHandler<OffsetEventArgs> IContentRenderer.Offset
        {
            add
            {
                lock (_offsetEventLock)
                {
                    if (value == null)
                    {
                        OffsetEvent = null;
                        return;
                    }

                    OffsetEvent += value;
                }
            }
            remove
            {
                lock (_offsetEventLock)
                {
                    if (value == null)
                    {
                        return;
                    }

                    OffsetEvent -= value;
                }
            }
        }

        /// <summary>
        /// Event triggered when the camera is zoomed.
        /// </summary>
        event EventHandler<ZoomScaleEventArgs> IContentRenderer.ZoomScale
        {
            add
            {
                lock (_zoomEventLock)
                {
                    if (value == null)
                    {
                        ZoomEvent = null;
                        return;
                    }

                    ZoomEvent += value;
                }              
            }
            remove
            {
                lock (_zoomEventLock)
                {
                    if (value == null)
                    {
                        return;
                    }

                    ZoomEvent -= value;
                }
            }
        }
        #endregion
        
        #region Properties.
        /// <summary>
        /// Property to return the camera used to control the content view.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The camera is responsible for panning and zooming on the content. Developers should pass this in to their <see cref="Gorgon2D.Begin(Gorgon2DBatchState, IGorgon2DCamera)"/> calls so that their 
        /// content renders correctly.
        /// </para>
        /// <para>
        /// This property can also be used to retrieve the current camera position on the view, and the current zoom value.
        /// </para>
        /// </remarks>
        protected IGorgon2DCamera Camera => _camera;

        /// <summary>
        /// Property to return the 2D renderer used to draw onto the content view.
        /// </summary>
        protected Gorgon2D Renderer
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the graphics interface used to create graphics objects.
        /// </summary>
        protected GorgonGraphics Graphics => Renderer?.Graphics;

        /// <summary>
        /// Property to return the pixel format for the view.
        /// </summary>
        protected BufferFormat PixelFormat => _swapChain.Format;

        /// <summary>
        /// Property to return the size of the view client area.
        /// </summary>
        public DX.Size2 ClientSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return whether the content can be horizontally panned.
        /// </summary>
        public bool CanPanHorizontally
        {
            get => (_camera == null) || (!_panHorzEnabled) ? false : ContentSize.Width > ClientSize.Width;
            set => _panHorzEnabled = value;
        }

        /// <summary>
        /// Property to set or return whether the content can be vertically panned.
        /// </summary>
        public bool CanPanVertically
        {
            get => (_camera == null) || (!_panVertEnabled) ? false : ContentSize.Height > ClientSize.Height;
            set => _panVertEnabled = value;
        }

        /// <summary>
        /// Property to set or return the region to render into.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Developers can set this value to constrain where the content and the background are drawn on the view. This value does not have any scaling or offsetting applied and is always in the same space 
        /// as the the client area in the view.
        /// </para>
        /// <para>
        /// If an empty rectangle is passed to this property, then the full client area of the view is used.
        /// </para>
        /// </remarks>
        public DX.RectangleF RenderRegion
        {
            get => _renderRegion.IsEmpty ? new DX.RectangleF(0, 0, ClientSize.Width, ClientSize.Height) : _renderRegion;
            protected set
            {
                if (value.Equals(ref _renderRegion))
                {
                    return;
                }

                _renderRegion = value;

                if (_camera != null)
                {
                    _camera.ViewDimensions = new DX.Size2F(_renderRegion.Width, _renderRegion.Height);
                }
            }
        }

        /// <summary>
        /// Property to set or return whether the content can be zoomed.
        /// </summary>
        public bool CanZoom
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Property to set or return the color to use when clearing the swap chain.
        /// </summary>
        /// <remarks>
        /// This value defaults to the background color of the view.
        /// </remarks>
        public GorgonColor BackgroundColor
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the size of the content in camera space.
        /// </summary>
        public DX.Size2F ContentSize => _camera == null
                                            ? new DX.Size2F((int)RenderRegion.Width, (int)RenderRegion.Height)
                                            : new DX.Size2F((int)(RenderRegion.Width * _camera.Zoom.X), (int)(RenderRegion.Height * _camera.Zoom.X));

        /// <summary>Property to return the data context assigned to this view.</summary>
        public T DataContext
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the current zoom level.
        /// </summary>
        public ZoomLevels ZoomLevel
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to reset the "to window" zoom state.
        /// </summary>
        private void ResetToWindow()
        {
            if (ZoomLevel == ZoomLevels.ToWindow)
            {
                MoveTo(new DX.Vector2(ClientSize.Width * 0.5f, ClientSize.Height * 0.5f), -1);
            }
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        private void SetDataContext(T dataContext)
        {
            if (DataContext != null)
            {
                DataContext.PropertyChanging -= DataContext_PropertyChanging;
                DataContext.PropertyChanged -= DataContext_PropertyChanged;
            }

            DataContext = dataContext;

            if (DataContext == null)
            {
                return;
            }

            DataContext.PropertyChanging += DataContext_PropertyChanging;
            DataContext.PropertyChanged += DataContext_PropertyChanged;
        }

        /// <summary>
        /// Function to perform transformations to the mouse position and transfer data to the mouse args object.
        /// </summary>
        /// <param name="e">The mouse event parameters.</param>
        private void GetMouseArgs(MouseEventArgs e)
        {
            // Transform the mouse coordinate into world space.
            var halfClient = new DX.Vector2(ClientSize.Width * 0.5f, ClientSize.Height * 0.5f);
            var halfRenderRegion = new DX.Vector2(RenderRegion.Width * 0.5f, RenderRegion.Height * 0.5f);
            int wx = (int)(((e.X - halfClient.X) / ClientSize.Width) * halfRenderRegion.X);
            int wy = (int)(((e.Y - halfClient.Y) / ClientSize.Height) * halfRenderRegion.Y);
            int cx = (int)(wx + halfRenderRegion.X);
            int cy = (int)(wy + halfRenderRegion.Y);

            var cameraMousePos = new DX.Vector3(e.X, e.Y, 0);

            if (_camera != null)
            {
                // The camera space mouse position.
                cameraMousePos = _camera.Project(cameraMousePos);
            }
            else
            {
                cameraMousePos.X = cx;
                cameraMousePos.Y = cy;
            }

            _mouseArgs.CameraSpacePosition = (DX.Vector2)cameraMousePos;
            _mouseArgs.ClientPosition = new DX.Point(e.X, e.Y);
            _mouseArgs.MouseButtons = e.Button;
            _mouseArgs.MouseWheelDelta = e.Delta;
            _mouseArgs.WorldSpacePosition = new DX.Vector2(wx, wy);
            _mouseArgs.ContentSpacePosition = new DX.Vector2(cx, cy);
            _mouseArgs.Handled = false;
        }

        /// <summary>Handles the MouseUp event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseUp(object sender, MouseEventArgs e)
        {
            GetMouseArgs(e);

            OnMouseUp(_mouseArgs);

            if (_mouseArgs.Handled)
            {
                return;
            }

            _panDragStart = null;
        }

        /// <summary>Handles the MouseDown event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            GetMouseArgs(e);

            OnMouseDown(_mouseArgs);

            if ((_mouseArgs.Handled) || (_mouseArgs.MouseButtons != MouseButtons.Middle))
            {
                return;
            }

            _panDragStart = new DX.Vector2(_mouseArgs.ClientPosition.X, _mouseArgs.ClientPosition.Y) / _camera.Zoom.X;
            _camDragStart = _camera.Position;
        }

        /// <summary>Handles the MouseWheel event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseWheel(object sender, MouseEventArgs e)
        {
            GetMouseArgs(e);

            OnMouseWheel(_mouseArgs);

            if (_mouseArgs.Handled)
            {
                return;
            }

            if (((_mouseArgs.Modifiers & Keys.Control) == Keys.Control) && (CanZoom))
            {
                float targetZoomSize = (_mouseArgs.MouseWheelDelta < 0 ? _camera.Zoom.X.GetPrevNearest() 
                                                                       : _camera.Zoom.X.GetNextNearest()).GetScale();
                MoveTo(_mouseArgs.ClientPosition, targetZoomSize);
                return;
            }

            var newOffset = (DX.Vector2)_camera.Position;
            float horzAmount = (RenderRegion.Width * 0.0125f).Max(1);
            float vertAmount = (RenderRegion.Height * 0.0125f).Max(1);
            int regionWidth = (int)(RenderRegion.Width * 0.5f);
            int regionHeight = (int)(RenderRegion.Height * 0.5f);

            if ((CanPanVertically) && ((_mouseArgs.Modifiers & Keys.Shift) != Keys.Shift))
            {
                if (_mouseArgs.MouseWheelDelta < 0)
                {
                    newOffset.Y = (_camera.Position.Y - vertAmount).Max(-regionHeight).Min(regionHeight);
                }
                else
                {
                    newOffset.Y = (_camera.Position.Y + vertAmount).Max(-regionHeight).Min(regionHeight);
                }                
            }

            if ((CanPanHorizontally) && ((_mouseArgs.Modifiers & Keys.Shift) == Keys.Shift))
            {
                if (_mouseArgs.MouseWheelDelta < 0)
                {
                    newOffset.X = (_camera.Position.X - horzAmount).Max(-regionWidth).Min(regionWidth);
                }
                else
                {
                    newOffset.X = (_camera.Position.X + horzAmount).Max(-regionWidth).Min(regionWidth);                    
                }                
            }

            SetOffset(new DX.Vector2(newOffset.X, newOffset.Y));            
        }

        /// <summary>Handles the MouseMove event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            GetMouseArgs(e);
            OnMouseMove(_mouseArgs);

            if (_mouseArgs.Handled)
            {
                return;
            }

            if ((_mouseArgs.MouseButtons != MouseButtons.Middle) || (_panDragStart == null))
            {
                return;
            }

            var camPos = (DX.Vector2)_camDragStart;
            DX.Vector2 startDrag = _panDragStart.Value;
            DX.Vector2 endDrag = new DX.Vector2(_mouseArgs.ClientPosition.X, _mouseArgs.ClientPosition.Y) / _camera.Zoom.X;
            DX.Vector2.Subtract(ref startDrag, ref endDrag, out DX.Vector2 delta);            
            DX.Vector2.Add(ref camPos, ref delta, out DX.Vector2 newOffset);            

            var halfContentSize = new DX.Vector2(RenderRegion.Width * 0.5f, RenderRegion.Height * 0.5f);

            if ((!CanPanVertically) || (newOffset.Y < -halfContentSize.Y) || (newOffset.Y > halfContentSize.Y))
            {
                newOffset.Y = _camera.Position.Y;
            }

            if ((!CanPanHorizontally) || (newOffset.X < -halfContentSize.X) || (newOffset.X > halfContentSize.X))
            {
                newOffset.X = _camera.Position.X;
            }

            SetOffset(new DX.Vector2((int)newOffset.X, (int)newOffset.Y));
        }

        /// <summary>Handles the KeyUp event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void Window_KeyUp(object sender, KeyEventArgs e) => OnKeyUp(e);

        /// <summary>Handles the KeyDown event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void Window_KeyDown(object sender, KeyEventArgs e) => OnKeyDown(e);

        /// <summary>Handles the PreviewKeyDown event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        private void Window_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) => OnPreviewKeyDown(e);

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);

            ResetToWindow();
        }

        /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e) => OnPropertyChanging(e.PropertyName);

        /// <summary>Handles the AfterSwapChainResized event of the SwapChain control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AfterSwapChainResizedEventArgs"/> instance containing the event data.</param>
        private void SwapChain_AfterSwapChainResized(object sender, AfterSwapChainResizedEventArgs e)
        {
            ClientSize = e.Size;
            OnResizeEnd();

            ResetToWindow();
        }

        /// <summary>Handles the BeforeSwapChainResized event of the SwapChain control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="BeforeSwapChainResizedEventArgs"/> instance containing the event data.</param>
        private void SwapChain_BeforeSwapChainResized(object sender, BeforeSwapChainResizedEventArgs e) => OnResizeBegin();

        /// <summary>
        /// Function called when a property on the <see cref="DataContext"/> is changing.
        /// </summary>
        /// <param name="propertyName">The name of the property that is changing.</param>
        /// <remarks>
        /// <para>
        /// Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.
        /// </para>
        /// </remarks>
        protected virtual void OnPropertyChanging(string propertyName)
        {

        }

        /// <summary>
        /// Function called when a property on the <see cref="DataContext"/> has been changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that was changed.</param>
        /// <remarks>
        /// <para>
        /// Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.
        /// </para>
        /// </remarks>
        protected virtual void OnPropertyChanged(string propertyName)
        {
        
        }

        /// <summary>
        /// Function called when the view is about to be resized.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Developers can override this method to handle cases where the view window is resized and the content has size dependent data (e.g. render targets).
        /// </para>
        /// </remarks>
        protected virtual void OnResizeBegin()
        {
        }

        /// <summary>
        /// Function to handle a mouse down event.
        /// </summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>
        /// <para>
        /// Developers can override this method to handle a mouse down event in their own content view.
        /// </para>
        /// </remarks>
        protected virtual void OnMouseDown(MouseArgs args)
        {
        
        }

        /// <summary>
        /// Function to handle a mouse up event.
        /// </summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>
        /// <para>
        /// Developers can override this method to handle a mouse up event in their own content view.
        /// </para>
        /// </remarks>
        protected virtual void OnMouseUp(MouseArgs args)
        {

        }

        /// <summary>
        /// Function to handle a mouse wheel event.
        /// </summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>
        /// <para>
        /// Developers can override this method to handle a mouse wheel event in their own content view.
        /// </para>
        /// </remarks>

        protected virtual void OnMouseWheel(MouseArgs args)
        {
        }

        /// <summary>
        /// Function to handle a mouse move event.
        /// </summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>
        /// <para>
        /// Developers can override this method to handle a mouse move event in their own content view.
        /// </para>
        /// </remarks>
        protected virtual void OnMouseMove(MouseArgs args)
        {
        }

        /// <summary>
        /// Function to handle a key down event.
        /// </summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>
        /// <para>
        /// Developers can override this method to handle a key down event in their own content view.
        /// </para>
        /// </remarks>
        protected virtual void OnKeyDown(KeyEventArgs args)
        {        
        }

        /// <summary>
        /// Function to handle a preview key down event.
        /// </summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>
        /// <para>
        /// Developers can override this method to handle a preview key down event in their own content view.
        /// </para>
        /// </remarks>
        protected virtual void OnPreviewKeyDown(PreviewKeyDownEventArgs args)
        {
        }

        /// <summary>
        /// Function to handle a key updown event.
        /// </summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>
        /// <para>
        /// Developers can override this method to handle a key up event in their own content view.
        /// </para>
        /// </remarks>
        protected virtual void OnKeyUp(KeyEventArgs args)
        {
        }

        /// <summary>
        /// Function called when the view has been resized.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Developers can override this method to handle cases where the view window is resized and the content has size dependent data (e.g. render targets).
        /// </para>
        /// </remarks>
        protected virtual void OnResizeEnd()
        {            
        }

        /// <summary>
        /// Function called when the renderer needs to load any resource data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated 
        /// <see cref="OnUnload"/> method.
        /// </para>
        /// </remarks>
        protected virtual void OnLoad()
        {
            
        }

        /// <summary>
        /// Function called when the renderer needs to clean up any resource data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Developers should always override this method if they've overridden the <see cref="OnLoad"/> method. Failure to do so can cause memory leakage.
        /// </para>
        /// </remarks>
        protected virtual void OnUnload()
        {
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (DataContext != null)
            {
                SetDataContext(null);
            }

            if (_backgroundPattern == null)
            {
                return;
            }

            UnloadResources();
        }


        /// <summary>
        /// Function to render the background.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Developers can override this method to render a custom background.
        /// </para>
        /// </remarks>
        protected virtual void OnRenderBackground()
        {
            var textureSize = new DX.RectangleF(0, 0, RenderRegion.Width / _backgroundPattern.Width * _camera.Zoom.X, RenderRegion.Height / _backgroundPattern.Height * _camera.Zoom.X);

            Renderer.Begin(camera: _camera);
            Renderer.DrawFilledRectangle(new DX.RectangleF(RenderRegion.Width * -0.5f, RenderRegion.Height * -0.5f, RenderRegion.Width, RenderRegion.Height), GorgonColor.White, _backgroundPattern, textureSize);
            Renderer.End();
        }

        /// <summary>
        /// Function to render the content.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the method that developers should override in order to draw their content to the view. 
        /// </para>
        /// </remarks>
        protected virtual void OnRenderContent()
        {
        }

        /// <summary>
        /// Function called when the camera is zoomed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Developers can override this method to implement a custom action when the camera is zoomed in or out.
        /// </para>
        /// </remarks>
        protected virtual void OnCameraZoomed()
        {        
        }

        /// <summary>
        /// Function called when the camera is panned.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Developers can override this method to implement a custom action when the camera is panned around the view.
        /// </para>
        /// </remarks>
        protected virtual void OnCameraMoved()
        {

        }

        /// <summary>
        /// Function to trigger the <see cref="ZoomEvent"/>.
        /// </summary>
        internal void OnZoom()
        {
            lock (_zoomEventLock)
            {
                _zoomArgs.ZoomScale = _camera.Zoom.X;
                EventHandler<ZoomScaleEventArgs> handler = ZoomEvent;
                handler?.Invoke(this, _zoomArgs);
            }

            OnCameraZoomed();
        }

        /// <summary>
        /// Function to trigger the <see cref="OffsetEvent"/>.
        /// </summary>
        internal void OnOffset()
        {
            lock (_offsetEventLock)
            {
                _offsetArgs.Offset = (DX.Vector2)_camera.Position;
                EventHandler<OffsetEventArgs> handler = OffsetEvent;
                handler?.Invoke(this, _offsetArgs);
            }

            OnCameraMoved();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Function to load resources for the renderer.</summary>
        /// <remarks>
        /// This method is used to load any required temporary resources for the renderer prior to rendering content. This must be paired with a call to <see cref="UnloadResources"/> when the renderer is
        /// no longer in use to ensure efficient memory usage.
        /// </remarks>
        public void LoadResources()
        {
            if (Interlocked.Exchange(ref _resourcesLoading, 1) == 1)
            {
                return;
            }

            Graphics.SetRenderTarget(_swapChain.RenderTargetView);

            _backgroundPattern = GorgonTexture2DView.CreateTexture(Graphics, new GorgonTexture2DInfo("ContentEditor_Bg_Pattern")
            {
                Usage = ResourceUsage.Immutable,
                Width = CommonEditorResources.CheckerBoardPatternImage.Width,
                Height = CommonEditorResources.CheckerBoardPatternImage.Height
            }, CommonEditorResources.CheckerBoardPatternImage);

            _swapChain.BeforeSwapChainResized += SwapChain_BeforeSwapChainResized;
            _swapChain.AfterSwapChainResized += SwapChain_AfterSwapChainResized;
            
            _swapChain.Window.MouseMove += Window_MouseMove;
            _swapChain.Window.MouseWheel += Window_MouseWheel;
            _swapChain.Window.MouseDown += Window_MouseDown;
            _swapChain.Window.MouseUp += Window_MouseUp;
            _swapChain.Window.KeyDown += Window_KeyDown;
            _swapChain.Window.KeyUp += Window_KeyUp;
            _swapChain.Window.PreviewKeyDown += Window_PreviewKeyDown;

            // Let the custom renderer set up its own stuff.
            OnLoad();

            _camera = new Gorgon2DOrthoCamera(Renderer, new DX.Size2F(RenderRegion.Width, RenderRegion.Height))
            {
                Anchor = new DX.Vector2(0.5f, 0.5f)
            };

            if ((_cameraAnimation != null) && (_camAnimController.CurrentAnimation != _cameraAnimation) && (_camAnimController.State != AnimationState.Playing))
            {
                ResetToWindow();
            }
        }

        /// <summary>Function to render the content.</summary>
        /// <remarks>This method is called by the view to render the content.</remarks>
        public void Render()
        {
            _swapChain.RenderTargetView.Clear(BackgroundColor);
            Graphics.SetRenderTarget(_swapChain.RenderTargetView);

            OnRenderBackground();

            OnRenderContent();

            _swapChain.Present(1);

            _camAnimController.Update();
        }

        /// <summary>Function to unload resources from the renderer.</summary>
        /// <remarks>This method is used to unload temporary resources for the renderer when it is no longer needed. Failure to call this may result in memory leakage.</remarks>
        public void UnloadResources()
        {
            if (Interlocked.Exchange(ref _resourcesLoading, 0) == 0)
            {
                return;
            }

            OnUnload();

            ZoomEvent = null;
            OffsetEvent = null;

            _swapChain.Window.PreviewKeyDown -= Window_PreviewKeyDown;
            _swapChain.Window.KeyDown -= Window_KeyDown;
            _swapChain.Window.KeyUp -= Window_KeyUp;
            _swapChain.Window.MouseDown -= Window_MouseDown;
            _swapChain.Window.MouseUp -= Window_MouseUp;
            _swapChain.Window.MouseWheel -= Window_MouseWheel;
            _swapChain.Window.MouseMove -= Window_MouseMove;
            _swapChain.AfterSwapChainResized -= SwapChain_AfterSwapChainResized;
            _swapChain.BeforeSwapChainResized -= SwapChain_BeforeSwapChainResized;

            GorgonTexture2DView bgTexture = Interlocked.Exchange(ref _backgroundPattern, null);

            bgTexture?.Dispose();
        }

        /// <summary>
        /// Function to set the offset of the view.
        /// </summary>
        /// <param name="offset">The offset to apply to the view.</param>
        public void SetOffset(DX.Vector2 offset)
        {
            if (_camera == null)
            {
                return;
            }            

            _camera.Position = new DX.Vector3(CanPanHorizontally ? offset.X : _camera.Position.X, CanPanVertically ? offset.Y : _camera.Position.Y, 0);
            OnOffset();
        }

        /// <summary>
        /// Function to set the zoom scale to fit within the content window.
        /// </summary>
        /// <returns>The scale value needed to fit within the content window.</returns>
        private float ZoomToWindow()
        {
            var scaling = new DX.Vector2(ClientSize.Width / RenderRegion.Width, ClientSize.Height / RenderRegion.Height);

            return scaling.X.Min(scaling.Y);
        }

        /// <summary>
        /// Function to move the camera to the offset position, and, optionally, the zoom to the offset.
        /// </summary>
        /// <param name="offset">The offset to apply to the view, in client space.</param>
        /// <param name="zoom">The amount to zoom, normalized percentage (1 = 100%, 0.5 = 50%, 4 = 400%, etc...).</param>
        /// <remarks>
        /// <para>
        /// This method animates the camera to move it to a location on the content, with an optional zoom scale value. To put the camera at a desired location without zooming, or animation use the 
        /// <see cref="SetOffset"/> method.
        /// </para>
        /// <para>
        /// To zoom to the current window size, set the <paramref name="zoom"/> value to less than or equal to 0.
        /// </para>
        /// </remarks>
        public void MoveTo(DX.Vector2 offset, float zoom)
        {
            if (_camera == null)
            {
                return;
            }

            if (!CanZoom)
            {
                zoom = 1.0f;
            }

            if (zoom <= 0)
            {
                ZoomLevel = ZoomLevels.ToWindow;
                zoom = ZoomToWindow();
            }
            else
            {
                ZoomLevel = zoom.GetZoomLevel();
                zoom = ZoomLevel.GetScale();
            }

            float currentSize = _camera.Zoom.X;
            DX.Vector3 currentPos = _camera.Position;            
            DX.Vector3 targetPos = _camera.Project((DX.Vector3)offset);

            int regionWidth = (int)(RenderRegion.Width * zoom);
            int regionHeight = (int)(RenderRegion.Height * zoom);

            // If our target size is less than the current view size, then reset the target position to the center of the view.
            if ((!_panHorzEnabled) || (regionWidth <= ClientSize.Width))
            {
                targetPos.X = 0;
            }

            if ((!_panVertEnabled) || (regionHeight <= ClientSize.Height))
            {
                targetPos.Y = 0;
            }

            float endTime = 0.65f;

            // Ensure the animation is finished prior to starting a new one.
            if ((_cameraAnimation != null) && (_camAnimController.CurrentAnimation != null) && (_camAnimController.State == AnimationState.Playing))
            {
                endTime = _cameraAnimation.Length - _camAnimController.Time;
                _camAnimController.Time = _cameraAnimation.Length;
                _camAnimController.Update();
            }
                
            _cameraAnimation = _animBuilder.Clear()
                                           .EditVector2(nameof(IGorgon2DCamera.Zoom))
                                           .SetInterpolationMode(TrackInterpolationMode.Spline)
                                           .SetKey(new GorgonKeyVector2(0, new DX.Vector2(currentSize)))
                                           .SetKey(new GorgonKeyVector2(endTime, new DX.Vector2(zoom)))
                                           .EndEdit()
                                           .EditVector3(nameof(IGorgon2DCamera.Position))
                                           .SetInterpolationMode(TrackInterpolationMode.Spline)
                                           .SetKey(new GorgonKeyVector3(0, currentPos))
                                           .SetKey(new GorgonKeyVector3(endTime, targetPos))
                                           .EndEdit()            
                                           .Build("CamAnim");

            _camAnimController.Play(_camera, _cameraAnimation);
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="DefaultContentRenderer{T}"/> class.</summary>
        /// <param name="name">The name of the renderer.</param>
        /// <param name="renderer">The main renderer for the content view.</param>
        /// <param name="swapChain">The swap chain for the content view.</param>
        /// <param name="dataContext">The view model to assign to the renderer.</param>
        protected internal DefaultContentRenderer(string name, Gorgon2D renderer, GorgonSwapChain swapChain, T dataContext)
            : base(name)
        {
            _camAnimController = new CameraAnimationController<T>(this);
            Renderer = renderer;
            _swapChain = swapChain;
            ClientSize = new DX.Size2(swapChain.Width, swapChain.Height);

            SetDataContext(dataContext);
        }
        #endregion
    }
}
