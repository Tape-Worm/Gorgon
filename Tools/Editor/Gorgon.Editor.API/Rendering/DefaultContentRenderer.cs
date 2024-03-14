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

using System.ComponentModel;
using System.Numerics;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Renderers.Cameras;
using DX = SharpDX;

namespace Gorgon.Editor.Rendering;

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
/// The panning and zooming operations (which were a pain in the ass to write) use a camera (a <see cref="GorgonOrthoCamera"/>) to apply the offsets and scaling for the pan/zoom operations. This 
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
/// <seealso cref="GorgonOrthoCamera"/>
public class DefaultContentRenderer<T>
    : GorgonNamedObject, IContentRenderer
    where T : class, IVisualEditorContent
{
    #region Variables.
    // The synchronization lock for events.
    private readonly object _zoomEventLock = new();
    private readonly object _offsetEventLock = new();
    private readonly object _regionEventLock = new();
    // Flag to indicate that the resources are loaded.
    private int _resourcesLoading;
    // The swap chain for the content view.
    private readonly GorgonSwapChain _swapChain;
    // The camera for our content.
    private GorgonOrthoCamera _camera;
    // The region to render the content and background into.
    private DX.RectangleF _renderRegion = DX.RectangleF.Empty;
    // The arguments for mouse events.
    private readonly MouseArgs _mouseArgs = new();
    // The argumments for zoom events.
    private readonly ZoomScaleEventArgs _zoomArgs = new();
    // The argumments for offset events.
    private readonly OffsetEventArgs _offsetArgs = new();
    // The controller for a camera animation.
    private readonly CameraAnimationController<T> _camAnimController;
    // The builder for an animation.
    private readonly GorgonAnimationBuilder _animBuilder = new();
    // The animation for manipulating the camera.
    private IGorgonAnimation _cameraAnimation;
    // Flag to indicate that panning is enabled.
    private bool _panHorzEnabled = true;
    private bool _panVertEnabled = true;
    // The starting point for the pan drag.
    private Vector2? _panDragStart;
    private Vector3 _camDragStart;
    // Font factory a font factory for generating fonts to use with the renderer.
    private GorgonFontFactory _fontFactory;
    #endregion

    #region Events.
    // The event triggered when the camera is zoomed.
    private EventHandler<ZoomScaleEventArgs> _zoomEvent;
    // The event triggered when the camera is moved.
    private EventHandler<OffsetEventArgs> _offsetEvent;
    // The event triggered when the render region has changed its size.
    private EventHandler _renderRegionChangedEvent;

    /// <summary>Event triggered when the render region has changed its size.</summary>
    event EventHandler IContentRenderer.RenderRegionChanged
    {
        add
        {
            lock (_regionEventLock)
            {
                if (value is null)
                {
                    _renderRegionChangedEvent = null;
                    return;
                }

                _renderRegionChangedEvent += value;
            }
        }
        remove
        {
            lock (_regionEventLock)
            {
                if (value is null)
                {
                    return;
                }

                _renderRegionChangedEvent -= value;
            }
        }
    }

    /// <summary>
    /// Event triggered when the camera is moved.
    /// </summary>
    event EventHandler<OffsetEventArgs> IContentRenderer.OffsetChanged
    {
        add
        {
            lock (_offsetEventLock)
            {
                if (value is null)
                {
                    _offsetEvent = null;
                    return;
                }

                _offsetEvent += value;
            }
        }
        remove
        {
            lock (_offsetEventLock)
            {
                if (value is null)
                {
                    return;
                }

                _offsetEvent -= value;
            }
        }
    }

    /// <summary>
    /// Event triggered when the camera is zoomed.
    /// </summary>
    event EventHandler<ZoomScaleEventArgs> IContentRenderer.ZoomScaleChanged
    {
        add
        {
            lock (_zoomEventLock)
            {
                if (value is null)
                {
                    _zoomEvent = null;
                    return;
                }

                _zoomEvent += value;
            }
        }
        remove
        {
            lock (_zoomEventLock)
            {
                if (value is null)
                {
                    return;
                }

                _zoomEvent -= value;
            }
        }
    }
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the default texture used to draw the background.
    /// </summary>
    protected GorgonTexture2DView BackgroundPattern
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the camera used to control the content view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The camera is responsible for panning and zooming on the content. Developers should pass this in to their <see cref="Gorgon2D.Begin(Gorgon2DBatchState, GorgonCameraCommon)"/> calls so that their 
    /// content renders correctly.
    /// </para>
    /// <para>
    /// This property can also be used to retrieve the current camera position on the view, and the current zoom value.
    /// </para>
    /// <para>
    /// Plug in developers can override this property and return their own camera if necessary.
    /// </para>
    /// </remarks>
    protected virtual GorgonOrthoCamera Camera => _camera;

    /// <summary>
    /// Property to return the primary render target.
    /// </summary>
    /// <remarks>
    /// Developers can use this property to reset the render target back to the original target after rendering to another target.
    /// </remarks>
    protected GorgonRenderTarget2DView MainRenderTarget => _swapChain?.RenderTargetView;

    /// <summary>
    /// Property to return the 2D renderer used to draw onto the content view.
    /// </summary>
    protected Gorgon2D Renderer
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the font factory used to generate fonts for the renderer.
    /// </summary>
    protected GorgonFontFactory Fonts => _fontFactory;

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
        get => (Camera is not null) && (_panHorzEnabled) && (ContentSize.Width > ClientSize.Width);
        set => _panHorzEnabled = value;
    }

    /// <summary>
    /// Property to set or return whether the content can be vertically panned.
    /// </summary>
    public bool CanPanVertically
    {
        get => (Camera is not null) && (_panVertEnabled) && (ContentSize.Height > ClientSize.Height);
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

            if (Camera is not null)
            {
                _camera.ViewDimensions = new DX.Size2F(_renderRegion.Width, _renderRegion.Height);
            }

            _renderRegionChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Property to set or return whether the renderer is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get;
        set;
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
    public DX.Size2F ContentSize => Camera is null
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
        protected set;
    }

    /// <summary>
    /// Property to return the offset of the image.
    /// </summary>
    public Vector2 Offset => new(_camera.Position.X, _camera.Position.Y);

    /// <summary>
    /// Property to return the current zoom level.
    /// </summary>
    public float Zoom => _camera.Zoom.X;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to reset the "to window" zoom state.
    /// </summary>
    private void ResetToWindow()
    {
        if (ZoomLevel == ZoomLevels.ToWindow)
        {
            MoveTo(new Vector2(ClientSize.Width * 0.5f, ClientSize.Height * 0.5f), -1);
        }
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    private void SetDataContext(T dataContext)
    {
        if (DataContext is not null)
        {
            DataContext.PropertyChanging -= DataContext_PropertyChanging;
            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        DataContext = dataContext;

        if (DataContext is null)
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
        var cameraMousePos = new Vector3(e.X, e.Y, 0);

        if (Camera is not null)
        {
            // The camera space mouse position.
            cameraMousePos = _camera.Project(cameraMousePos);
        }

        _mouseArgs.CameraSpacePosition = new Vector2(cameraMousePos.X, cameraMousePos.Y);
        _mouseArgs.ClientPosition = new DX.Point(e.X, e.Y);
        _mouseArgs.MouseButtons = e.Button;
        _mouseArgs.MouseWheelDelta = e.Delta;
        _mouseArgs.Handled = false;
        _mouseArgs.ButtonClickCount = e.Clicks;
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

        _panDragStart = new Vector2(_mouseArgs.ClientPosition.X, _mouseArgs.ClientPosition.Y) / _camera.Zoom.X;
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

            MoveTo(_mouseArgs.ClientPosition.ToVector2(), targetZoomSize);
            return;
        }

        int regionWidth = (int)(RenderRegion.Width * 0.5f);
        int regionHeight = (int)(RenderRegion.Height * 0.5f);
        var newOffset = new Vector2(_camera.Position.X, _camera.Position.Y);
        float horzAmount = (RenderRegion.Width * 0.0125f).Max(1);
        float vertAmount = (RenderRegion.Height * 0.0125f).Max(1);

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

        SetOffset(new Vector2(newOffset.X, newOffset.Y));
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

        if ((_mouseArgs.MouseButtons != MouseButtons.Middle) || (_panDragStart is null))
        {
            return;
        }

        var camPos = new Vector2(_camDragStart.X, _camDragStart.Y);
        Vector2 startDrag = _panDragStart.Value;
        Vector2 endDrag = new Vector2(_mouseArgs.ClientPosition.X, _mouseArgs.ClientPosition.Y) / _camera.Zoom.X;
        var delta = Vector2.Subtract(startDrag, endDrag);
        var newOffset = Vector2.Add(camPos, delta);

        var halfContentSize = new Vector2(RenderRegion.Width * 0.5f, RenderRegion.Height * 0.5f);

        if ((!CanPanVertically) || (newOffset.Y < -halfContentSize.Y) || (newOffset.Y > halfContentSize.Y))
        {
            newOffset.Y = _camera.Position.Y;
        }

        if ((!CanPanHorizontally) || (newOffset.X < -halfContentSize.X) || (newOffset.X > halfContentSize.X))
        {
            newOffset.X = _camera.Position.X;
        }

        SetOffset(new Vector2((int)newOffset.X, (int)newOffset.Y));
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
        if (IsEnabled)
        {
            OnPropertyChanged(e.PropertyName);
        }
    }

    /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
    {
        if (IsEnabled)
        {
            OnPropertyChanging(e.PropertyName);
        }
    }

    /// <summary>Handles the AfterSwapChainResized event of the SwapChain control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SwapChainResizedEventArgs"/> instance containing the event data.</param>
    private void SwapChain_AfterSwapChainResized(object sender, SwapChainResizedEventArgs e)
    {
        // Because we have multiple swap chains, we need to reset it back to our swap chain.
        // Otherwise the camera will use the correct swap chain.
        if (Graphics.RenderTargets[0] != _swapChain.RenderTargetView)
        {
            Graphics.SetRenderTarget(_swapChain.RenderTargetView);
        }

        if (RenderRegion.IsEmpty)
        {
            _camera.ViewDimensions = new DX.Size2F(e.Size.Width, e.Size.Height);
        }
        else
        {
            _camera.ViewDimensions = new DX.Size2F(RenderRegion.Width, RenderRegion.Height);
        }

        ClientSize = e.Size;
        OnResizeEnd();

        ResetToWindow();
    }

    /// <summary>Handles the BeforeSwapChainResized event of the SwapChain control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SwapChainResizingEventArgs"/> instance containing the event data.</param>
    private void SwapChain_BeforeSwapChainResized(object sender, SwapChainResizingEventArgs e) => OnResizeBegin();

    /// <summary>
    /// Function to set the zoom scale to fit within the content window.
    /// </summary>
    /// <returns>The scale value needed to fit within the content window.</returns>
    private float ZoomToWindow()
    {
        var scaling = new Vector2(ClientSize.Width / RenderRegion.Width, ClientSize.Height / RenderRegion.Height);

        return scaling.X.Min(scaling.Y);
    }

    /// <summary>
    /// Function to convert a rectangle into camera space from client space.
    /// </summary>
    /// <param name="rect">The client space rectangle</param>
    /// <param name="targetSize">The size of the render target.</param>
    /// <returns>The camera space rectangle.</returns>
    /// <remarks>
    /// <para>
    /// If the <paramref name="targetSize"/> is omitted, then the current render target at slot 0 in <see cref="GorgonGraphics.RenderTargets"/> will be used.
    /// </para>
    /// </remarks>
    protected DX.RectangleF ToCamera(DX.RectangleF rect, DX.Size2? targetSize = null)
    {
        Vector3 topLeft = targetSize is null ? _camera.Project(new Vector3(rect.TopLeft.X, rect.TopLeft.Y, 0)) : _camera.Project(new Vector3(rect.TopLeft.X, rect.TopLeft.Y, 0), targetSize.Value);
        Vector3 bottomRight = targetSize is null ? _camera.Project(new Vector3(rect.BottomRight.X, rect.BottomRight.Y, 0)) : _camera.Project(new Vector3(rect.BottomRight.X, rect.BottomRight.Y, 0), targetSize.Value);
        return new DX.RectangleF
        {
            Left = topLeft.X,
            Top = topLeft.Y,
            Right = bottomRight.X,
            Bottom = bottomRight.Y
        };
    }

    /// <summary>
    /// Function to convert a vector into camera space from client space.
    /// </summary>
    /// <param name="vector">The client space vector.</param>
    /// <param name="targetSize">[Optional] The size of the render target.</param>
    /// <returns>The camera space vector.</returns>
    /// <remarks>
    /// <para>
    /// If the <paramref name="targetSize"/> is omitted, then the current render target at slot 0 in <see cref="GorgonGraphics.RenderTargets"/> will be used.
    /// </para>
    /// </remarks>
    protected Vector2 ToCamera(Vector2 vector, DX.Size2? targetSize = null)
    {
        Vector3 projected;

        if (targetSize == null)
        {
            projected = _camera.Project(new Vector3(vector.X, vector.Y, 0));
        }
        else
        {
            projected = _camera.Project(new Vector3(vector.X, vector.Y, 0), targetSize.Value);
        }

        return new Vector2(projected.X, projected.Y);
    }

    /// <summary>
    /// Function to convert a size into camera space from client space.
    /// </summary>
    /// <param name="size">The client space size.</param>
    /// <param name="targetSize">The size of the render target.</param>
    /// <returns>The camera space size.</returns>
    /// <remarks>
    /// <para>
    /// If the <paramref name="targetSize"/> is omitted, then the current render target at slot 0 in <see cref="GorgonGraphics.RenderTargets"/> will be used.
    /// </para>
    /// </remarks>
    protected DX.Size2F ToCamera(DX.Size2F size, DX.Size2? targetSize = null)
    {
        Vector3 result = targetSize is null ? _camera.Project(new Vector3(size.Width, size.Height, 0)) : _camera.Project(new Vector3(size.Width, size.Height, 0), targetSize.Value);
        return new DX.Size2F(result.X, result.Y);
    }

    /// <summary>
    /// Function to convert a rectangle into client space from camera space.
    /// </summary>
    /// <param name="rect">The camera space rectangle</param>
    /// <param name="targetSize">The size of the render target.</param>
    /// <returns>The client space rectangle.</returns>
    /// <remarks>
    /// <para>
    /// If the <paramref name="targetSize"/> is omitted, then the current render target at slot 0 in <see cref="GorgonGraphics.RenderTargets"/> will be used.
    /// </para>
    /// </remarks>
    protected DX.RectangleF ToClient(DX.RectangleF rect, DX.Size2? targetSize = null)
    {
        Vector3 topLeft = targetSize is null ? _camera.Unproject(new Vector3(rect.TopLeft.X, rect.TopLeft.Y, 0)) : _camera.Unproject(new Vector3(rect.TopLeft.X, rect.TopLeft.Y, 0), targetSize.Value);
        Vector3 bottomRight = targetSize is null ? _camera.Unproject(new Vector3(rect.BottomRight.X, rect.BottomRight.Y, 0)) : _camera.Unproject(new Vector3(rect.BottomRight.X, rect.BottomRight.Y, 0), targetSize.Value);
        return new DX.RectangleF
        {
            Left = topLeft.X,
            Top = topLeft.Y,
            Right = bottomRight.X,
            Bottom = bottomRight.Y
        };
    }

    /// <summary>
    /// Function to convert a vector into client space from camera space.
    /// </summary>
    /// <param name="vector">The camera space vector.</param>
    /// <param name="targetSize">The size of the render target.</param>
    /// <returns>The client space vector.</returns>
    /// <remarks>
    /// <para>
    /// If the <paramref name="targetSize"/> is omitted, then the current render target at slot 0 in <see cref="GorgonGraphics.RenderTargets"/> will be used.
    /// </para>
    /// </remarks>
    protected Vector2 ToClient(Vector2 vector, DX.Size2? targetSize = null)
    {
        Vector3 unprojected;

        if (targetSize == null)
        {
            unprojected = _camera.Unproject(new Vector3(vector.X, vector.Y, 0));
        }
        else
        {
            unprojected = _camera.Unproject(new Vector3(vector.X, vector.Y, 0), targetSize.Value);
        }

        return new Vector2(unprojected.X, unprojected.Y);
    }

    /// <summary>
    /// Function to convert a size into client space from camera space.
    /// </summary>
    /// <param name="size">The camera space size.</param>
    /// <param name="targetSize">The size of the render target.</param>
    /// <returns>The client space size.</returns>
    /// <remarks>
    /// <para>
    /// If the <paramref name="targetSize"/> is omitted, then the current render target at slot 0 in <see cref="GorgonGraphics.RenderTargets"/> will be used.
    /// </para>
    /// </remarks>
    protected DX.Size2F ToClient(DX.Size2F size, DX.Size2? targetSize = null)
    {
        Vector3 result = targetSize is null ? _camera.Unproject(new Vector3(size.Width, size.Height, 0)) : _camera.Unproject(new Vector3(size.Width, size.Height, 0), targetSize.Value);
        return new DX.Size2F(result.X, result.Y);
    }

    /// <summary>
    /// Function to retrieve the nearest zoom level that fits within the specified rectangle.
    /// </summary>
    /// <param name="region">The region to zoom in on.</param>
    /// <returns>The zoom level value needed to fit within the content window.</returns>
    protected ZoomLevels GetNearestZoomFromRectangle(DX.RectangleF region)
    {
        if ((region.Width < 1) || (region.Height < 1))
        {
            region = new DX.RectangleF(region.X, region.Y, 1, 1);
        }

        var scaling = new Vector2(ClientSize.Width / region.Width, ClientSize.Height / region.Height);
        float scaleValue = scaling.X.Min(scaling.Y);

        return scaleValue.GetZoomLevel();
    }

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

        GorgonFontFactory factory = Interlocked.Exchange(ref _fontFactory, null);

        UnloadResources();

        if (DataContext is not null)
        {
            SetDataContext(null);
        }
        factory?.Dispose();
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
        var textureSize = new DX.RectangleF(0, 0, RenderRegion.Width / BackgroundPattern.Width * _camera.Zoom.X, RenderRegion.Height / BackgroundPattern.Height * _camera.Zoom.X);

        Renderer.Begin(camera: Camera);
        Renderer.DrawFilledRectangle(new DX.RectangleF(RenderRegion.Width * -0.5f, RenderRegion.Height * -0.5f, RenderRegion.Width, RenderRegion.Height), GorgonColor.White, BackgroundPattern, textureSize);
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
    /// Function to center the view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Developers can override this method to provide custom centering of the view. The default is to zoom to the current client size of the window.
    /// </para>
    /// </remarks>
    protected virtual void OnCenterView() => MoveTo(Vector2.Zero, -1.0f);

    /// <summary>
    /// Function to adjust a zoom value based on the input zoom value.
    /// </summary>
    /// <param name="requestedZoom">The zoom value to adjust.</param>
    /// <returns>The adjusted zoom value.</returns>
    /// <remarks>
    /// <para>
    /// Developers can override this method to update the <paramref name="requestedZoom"/> value so that it conforms to the UI workflow. By default it adjusts the <paramref name="requestedZoom"/> value 
    /// to snap to the <see cref="ZoomLevels"/> values.
    /// </para>
    /// </remarks>
    protected virtual float GetZoomValue(float requestedZoom)
    {
        if (requestedZoom <= 0)
        {
            ZoomLevel = ZoomLevels.ToWindow;
            requestedZoom = ZoomToWindow();
        }
        else
        {
            ZoomLevel = requestedZoom.GetZoomLevel();
            requestedZoom = ZoomLevel.GetScale();
        }

        return requestedZoom;
    }

    /// <summary>
    /// Function to move the camera to the offset position, and, optionally, the zoom to the offset.
    /// </summary>
    /// <param name="offset">The offset to apply to the view, in client space.</param>
    /// <param name="zoom">The amount to zoom, normalized percentage (1 = 100%, 0.5 = 50%, 4 = 400%, etc...).</param>
    /// <param name="ignoreBoundaries"><b>true</b> to ignore the region boundaries, <b>false</b> to respect them.</param>
    /// <remarks>
    /// <para>
    /// This method animates the camera to move it to a location on the content, with an optional zoom scale value. To put the camera at a desired location without zooming, or animation use the 
    /// <see cref="SetOffset"/> method.
    /// </para>
    /// <para>
    /// To zoom to the current window size, set the <paramref name="zoom"/> value to less than or equal to 0.
    /// </para>
    /// </remarks>
    protected void ForceMoveTo(Vector2 offset, float zoom, bool ignoreBoundaries)
    {
        if (Camera is null)
        {
            return;
        }

        zoom = GetZoomValue(zoom);

        float currentSize = _camera.Zoom.X;
        Vector3 currentPos = _camera.Position;
        Vector3 targetPos = _camera.Project(new Vector3(offset.X, offset.Y, 0));

        int regionWidth = (int)(RenderRegion.Width * zoom);
        int regionHeight = (int)(RenderRegion.Height * zoom);
        var halfRegion = new Vector2(RenderRegion.Width * 0.5f, RenderRegion.Height * 0.5f);

        // If our target size is less than the current view size, then reset the target position to the center of the view.
        if ((!ignoreBoundaries) && (regionWidth <= ClientSize.Width))
        {
            targetPos.X = 0;
        }

        if ((!ignoreBoundaries) && (regionHeight <= ClientSize.Height))
        {
            targetPos.Y = 0;
        }

        float endTime = 0.65f;

        // Lock the target position to the edges of the image.
        targetPos.X = targetPos.X.Max(-halfRegion.X).Min(halfRegion.X);
        targetPos.Y = targetPos.Y.Max(-halfRegion.Y).Min(halfRegion.Y);

        // Ensure the animation is finished prior to starting a new one.
        if ((_cameraAnimation is not null) && (_camAnimController.CurrentAnimation is not null) && (_camAnimController.State == AnimationState.Playing))
        {
            endTime = _cameraAnimation.Length - _camAnimController.Time;
            _camAnimController.Time = _cameraAnimation.Length;
            _camAnimController.Update();
        }

        _cameraAnimation = _animBuilder.Clear()
                                       .EditVector2(nameof(GorgonOrthoCamera.Zoom))
                                       .SetInterpolationMode(TrackInterpolationMode.Spline)
                                       .SetKey(new GorgonKeyVector2(0, new Vector2(currentSize)))
                                       .SetKey(new GorgonKeyVector2(endTime, new Vector2(zoom)))
                                       .EndEdit()
                                       .EditVector3(nameof(GorgonOrthoCamera.Position))
                                       .SetInterpolationMode(TrackInterpolationMode.Spline)
                                       .SetKey(new GorgonKeyVector3(0, currentPos))
                                       .SetKey(new GorgonKeyVector3(endTime, targetPos))
                                       .EndEdit()
                                       .Build("CamAnim");

        _camAnimController.Play(_camera, _cameraAnimation);
    }

    /// <summary>
    /// Function to trigger the <see cref="_zoomEvent"/>.
    /// </summary>
    internal void OnZoom()
    {
        lock (_zoomEventLock)
        {
            _zoomArgs.ZoomScale = _camera.Zoom.X;
            EventHandler<ZoomScaleEventArgs> handler = _zoomEvent;
            handler?.Invoke(this, _zoomArgs);
        }

        OnCameraZoomed();
    }

    /// <summary>
    /// Function to trigger the <see cref="_offsetEvent"/>.
    /// </summary>
    internal void OnOffset()
    {
        lock (_offsetEventLock)
        {
            _offsetArgs.Offset = new Vector2(_camera.Position.X, _camera.Position.Y);
            EventHandler<OffsetEventArgs> handler = _offsetEvent;
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

        BackgroundPattern = GorgonTexture2DView.CreateTexture(Graphics, new GorgonTexture2DInfo(CommonEditorResources.CheckerBoardPatternImage.Width,
                                                                                                     CommonEditorResources.CheckerBoardPatternImage.Height,
                                                                                                     CommonEditorResources.CheckerBoardPatternImage.Format)
        {
            Name = "ContentEditor_Bg_Pattern",
            Usage = ResourceUsage.Immutable
        }, CommonEditorResources.CheckerBoardPatternImage);

        ClientSize = new DX.Size2(_swapChain.Width, _swapChain.Height);

        _swapChain.SwapChainResizing += SwapChain_BeforeSwapChainResized;
        _swapChain.SwapChainResized += SwapChain_AfterSwapChainResized;

        _swapChain.Window.MouseMove += Window_MouseMove;
        _swapChain.Window.MouseWheel += Window_MouseWheel;
        _swapChain.Window.MouseDown += Window_MouseDown;
        _swapChain.Window.MouseUp += Window_MouseUp;
        _swapChain.Window.KeyDown += Window_KeyDown;
        _swapChain.Window.KeyUp += Window_KeyUp;
        _swapChain.Window.PreviewKeyDown += Window_PreviewKeyDown;

        _camera = new GorgonOrthoCamera(Graphics, new DX.Size2F(RenderRegion.Width, RenderRegion.Height))
        {
            Anchor = new Vector2(0.5f, 0.5f)
        };

        // Let the custom renderer set up its own stuff.
        OnLoad();
    }

    /// <summary>Function to render the content.</summary>
    /// <remarks>This method is called by the view to render the content.</remarks>
    public void Render()
    {
        if (!IsEnabled)
        {
            return;
        }

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

        _zoomEvent = null;
        _offsetEvent = null;

        _swapChain.Window.PreviewKeyDown -= Window_PreviewKeyDown;
        _swapChain.Window.KeyDown -= Window_KeyDown;
        _swapChain.Window.KeyUp -= Window_KeyUp;
        _swapChain.Window.MouseDown -= Window_MouseDown;
        _swapChain.Window.MouseUp -= Window_MouseUp;
        _swapChain.Window.MouseWheel -= Window_MouseWheel;
        _swapChain.Window.MouseMove -= Window_MouseMove;
        _swapChain.SwapChainResized -= SwapChain_AfterSwapChainResized;
        _swapChain.SwapChainResizing -= SwapChain_BeforeSwapChainResized;

        BackgroundPattern?.Dispose();
        BackgroundPattern = null;
    }

    /// <summary>
    /// Function to set the offset of the view.
    /// </summary>
    /// <param name="offset">The offset to apply to the view.</param>
    public void SetOffset(Vector2 offset)
    {
        if (Camera is null)
        {
            return;
        }

        _camera.Position = new Vector3(CanPanHorizontally ? offset.X : _camera.Position.X, CanPanVertically ? offset.Y : _camera.Position.Y, 0);
        OnOffset();
    }

    /// <summary>
    /// Function to set the zoom on the view.
    /// </summary>
    /// <param name="zoom">The zoom value to apply.</param>
    public void SetZoom(float zoom)
    {
        if (Camera is null)
        {
            return;
        }

        zoom = GetZoomValue(zoom);

        _camera.Zoom = new Vector2(zoom, zoom);
        OnZoom();
    }

    /// <summary>
    /// Function to center the view.
    /// </summary>
    public void CenterView() => OnCenterView();

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
    public void MoveTo(Vector2 offset, float zoom)
    {
        if (Camera is null)
        {
            return;
        }

        if (!CanZoom)
        {
            zoom = 1.0f;
        }

        ForceMoveTo(offset, zoom, false);
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
        _fontFactory = new GorgonFontFactory(renderer.Graphics);

        SetDataContext(dataContext);
    }
    #endregion
}
