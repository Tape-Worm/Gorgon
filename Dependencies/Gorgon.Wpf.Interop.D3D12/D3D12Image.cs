// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Created: October 22, 2026 5:52:43 PM
//

using System;
using System.Windows;
using System.Windows.Interop;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

namespace Gorgon.Wpf.Interop;

/// <summary>
/// An image source that presents Direct3D 12 rendering in WPF.
/// </summary>
/// <remarks>
/// <para>
/// WPF can only present a Direct3D 9 surface. This image owns a Direct3D 9Ex device and a shared render target texture, sized with
/// <see cref="SetPixelSize"/>, and hands the texture's shared handle to the <see cref="OnRender"/> callback. The Direct3D 12 renderer opens that handle with
/// <c>ID3D12Device::OpenSharedHandle</c> and renders into the resulting <c>ID3D12Resource</c> directly.
/// </para>
/// <para>
/// The opened resource is a 2D texture with 1 mip level, in the <c>DXGI_FORMAT_B8G8R8A8_UNORM</c> format, with the <c>D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET</c> and
/// <c>D3D12_RESOURCE_FLAG_ALLOW_SIMULTANEOUS_ACCESS</c> flags. Because it allows simultaneous access, it stays in the <c>D3D12_BARRIER_LAYOUT_COMMON</c> layout: with enhanced
/// barriers, use access-only barriers in that layout; with legacy barriers, no transitions are needed. WPF treats the alpha channel as premultiplied.
/// </para>
/// <para>
/// The Direct3D 12 device must be created on the same adapter as this image's Direct3D 9Ex device. Set <see cref="AdapterLuid"/> to the LUID of the Direct3D 12 adapter
/// (<c>ID3D12Device::GetAdapterLuid</c>) before the first call to <see cref="RequestRender"/>.
/// </para>
/// <para>
/// This object must be used on the thread that owns its dispatcher, and must be disposed when it is no longer needed.
/// </para>
/// </remarks>
public sealed unsafe class D3D12Image
    : D3DImage, IDisposable
{
    /// <summary>
    /// The dependency property for <see cref="OnRender"/>.
    /// </summary>
    public static readonly DependencyProperty OnRenderProperty = DependencyProperty.Register(nameof(OnRender), typeof(Action<nint, bool>), typeof(D3D12Image),
                                                                                             new UIPropertyMetadata(null));

    /// <summary>
    /// The dependency property for <see cref="WindowOwner"/>.
    /// </summary>
    public static readonly DependencyProperty WindowOwnerProperty = DependencyProperty.Register(nameof(WindowOwner), typeof(nint), typeof(D3D12Image),
                                                                                                new UIPropertyMetadata((nint)0, DeviceSettingChanged));

    /// <summary>
    /// The dependency property for <see cref="AdapterLuid"/>.
    /// </summary>
    public static readonly DependencyProperty AdapterLuidProperty = DependencyProperty.Register(nameof(AdapterLuid), typeof(long), typeof(D3D12Image),
                                                                                                new UIPropertyMetadata(0L, DeviceSettingChanged));

    // The Direct3D 9Ex device that owns the shared texture.
    private D3D9Device? _device;
    // The shared render target texture.
    private ComPtr<IDirect3DTexture9> _texture;
    // The first mip level of the shared texture, which is the back buffer for WPF.
    private ComPtr<IDirect3DSurface9> _surface;
    // The shared handle for the texture.
    private HANDLE _sharedHandle;
    // The requested size of the texture, in pixels.
    private int _pixelWidth;
    private int _pixelHeight;
    // Flag to indicate that the image has been disposed.
    private bool _disposed;

    /// <summary>
    /// Property to set or return the callback that renders into the shared texture.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The first parameter is the shared handle for the texture, to pass to <c>ID3D12Device::OpenSharedHandle</c>. The second parameter is <b>true</b> when the texture is
    /// new: the first render, after <see cref="SetPixelSize"/> changes the size, and after the Direct3D 9Ex device is recreated. When it is <b>true</b>, open the handle again
    /// and release the resource opened from the previous handle. The handle belongs to this image; do not close it.
    /// </para>
    /// <para>
    /// WPF reads the texture on its own device as soon as the callback returns, with no GPU synchronization. The callback must not return until the GPU has finished
    /// writing to the texture (for example, signal a fence after the work is submitted, and wait on it).
    /// </para>
    /// </remarks>
    public Action<nint, bool>? OnRender
    {
        get => (Action<nint, bool>?)GetValue(OnRenderProperty);
        set => SetValue(OnRenderProperty, value);
    }

    /// <summary>
    /// Property to set or return the handle of the window that owns the Direct3D 9Ex device.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this is 0, the desktop window is used. Changing this value recreates the Direct3D 9Ex device and the shared texture on the next render.
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public nint WindowOwner
    {
        get => (nint)GetValue(WindowOwnerProperty);
        set => SetValue(WindowOwnerProperty, value);
    }

    /// <summary>
    /// Property to set or return the LUID of the adapter used by the Direct3D 12 renderer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The LUID is packed as <c>(HighPart &lt;&lt; 32) | LowPart</c>. When this is 0, the default adapter is used. Changing this value recreates the Direct3D 9Ex device and
    /// the shared texture on the next render.
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public long AdapterLuid
    {
        get => (long)GetValue(AdapterLuidProperty);
        set => SetValue(AdapterLuidProperty, value);
    }

    /// <summary>
    /// Function called when a property that the Direct3D 9Ex device depends on changes.
    /// </summary>
    /// <param name="sender">The image.</param>
    /// <param name="e">The event parameters.</param>
    private static void DeviceSettingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is D3D12Image image)
        {
            image.ReleaseDevice();
        }
    }

    /// <summary>
    /// Function called when the front buffer becomes available or unavailable.
    /// </summary>
    /// <param name="sender">The image.</param>
    /// <param name="e">The event parameters.</param>
    private void FrontBufferAvailableChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        // The front buffer usually comes back after a lock screen, a user switch or a full screen exclusive application, any of which can take the device with it.
        if (!IsFrontBufferAvailable)
        {
            return;
        }

        if ((_device is not null) && (_device.IsLost))
        {
            ReleaseDevice();
        }

        RequestRender();
    }

    /// <summary>
    /// Function to release the shared texture and detach it from WPF.
    /// </summary>
    private void ReleaseSurface()
    {
        if (_surface.Get() is null)
        {
            return;
        }

        Lock();
        SetBackBuffer(D3DResourceType.IDirect3DSurface9, 0);
        Unlock();

        _surface.Dispose();
        _texture.Dispose();
        _sharedHandle = HANDLE.NULL;
    }

    /// <summary>
    /// Function to release the Direct3D 9Ex device, and the shared texture with it.
    /// </summary>
    private void ReleaseDevice()
    {
        ReleaseSurface();
        _device?.Dispose();
        _device = null;
    }

    /// <inheritdoc/>
    protected override Freezable CreateInstanceCore() => new D3D12Image();

    /// <summary>
    /// Function to render a frame through the <see cref="OnRender"/> callback and present it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Nothing is rendered when there is no <see cref="OnRender"/> callback, when <see cref="SetPixelSize"/> has not been given a size of at least 1x1, or when the device
    /// has been lost and the front buffer is unavailable. The Direct3D 9Ex device and the shared texture are created on the first render, and recreated if the device has been lost.
    /// </para>
    /// <para>
    /// Rendering continues while the front buffer is unavailable and the device is intact (for example, when WPF composes in software), and WPF displays the surface through its
    /// software fallback.
    /// </para>
    /// <para>
    /// Exceptions thrown by the <see cref="OnRender"/> callback are not caught.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown if the image has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no Direct3D 9 adapter matches <see cref="AdapterLuid"/>.</exception>
    /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if the Direct3D 9Ex device or the shared texture could not be created.</exception>
    public void RequestRender()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Action<nint, bool>? onRender = OnRender;

        if ((onRender is null) || (_pixelWidth < 1) || (_pixelHeight < 1))
        {
            return;
        }

        if ((_device is not null) && (_device.IsLost))
        {
            // With software fallback WPF keeps reading the surface while the front buffer is unavailable, so rendering only stops when the device is gone too.
            // FrontBufferAvailableChanged recreates it once the front buffer is back.
            if (!IsFrontBufferAvailable)
            {
                return;
            }

            ReleaseDevice();
        }

        _device ??= new D3D9Device(WindowOwner, AdapterLuid);

        bool isNewSurface = _surface.Get() is null;

        if (isNewSurface)
        {
            _sharedHandle = _device.CreateSharedTexture(_pixelWidth, _pixelHeight, out _texture, out _surface);
        }

        Lock();

        try
        {
            if (isNewSurface)
            {
                SetBackBuffer(D3DResourceType.IDirect3DSurface9, (nint)_surface.Get(), true);
            }

            onRender((nint)_sharedHandle.Value, isNewSurface);
            AddDirtyRect(new Int32Rect(0, 0, PixelWidth, PixelHeight));
        }
        finally
        {
            Unlock();
        }
    }

    /// <summary>
    /// Function to set the size of the shared texture.
    /// </summary>
    /// <param name="pixelWidth">The width of the texture, in pixels.</param>
    /// <param name="pixelHeight">The height of the texture, in pixels.</param>
    /// <remarks>
    /// <para>
    /// When the size changes, the current texture is released and a new one is rendered immediately through <see cref="RequestRender"/>. A width or height less than 1
    /// releases the texture and stops rendering until a valid size is set.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown if the image has been disposed.</exception>
    public void SetPixelSize(int pixelWidth, int pixelHeight)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if ((pixelWidth == _pixelWidth) && (pixelHeight == _pixelHeight))
        {
            return;
        }

        _pixelWidth = pixelWidth;
        _pixelHeight = pixelHeight;

        ReleaseSurface();
        RequestRender();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        IsFrontBufferAvailableChanged -= FrontBufferAvailableChanged;
        ReleaseDevice();
        _disposed = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="D3D12Image"/> class.
    /// </summary>
    public D3D12Image() => IsFrontBufferAvailableChanged += FrontBufferAvailableChanged;
}
