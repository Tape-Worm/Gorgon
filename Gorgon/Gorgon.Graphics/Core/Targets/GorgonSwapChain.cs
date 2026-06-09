// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: December 29, 2025 12:58:41 AM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A swap chain used to send rendering output to the display device.
/// </summary>
public unsafe sealed class GorgonSwapChain
    : IGorgonSwapChainInfo, IDisposable, IGorgonNamedObject
{
    private ComPtr<IDXGISwapChain4> _dxgiSwapChain;

    private GorgonSwapChainInfo _info;
    private readonly HANDLE _waitHandle;
    private uint _descFlags;
    private GorgonRenderTargetView[] _renderTargetViews = [];

    /// <summary>
    /// Property to return the internal DXGI swap chain.
    /// </summary>
    internal ref readonly ComPtr<IDXGISwapChain4> DXGISwapChain => ref _dxgiSwapChain;    

    /// <inheritdoc/>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Property to return the graphics object associated with this swap chain.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Property to return the window handle that the swap chain is bound with.
    /// </summary>
    public nint WindowHandle
    {
        get;
    }

    /// <summary>
    /// Property to return the index of the current back buffer.
    /// </summary>
    public int CurrentBackBufferIndex
    {
        get;
        private set;
    } = -1;

    /// <summary>
    /// Property to return the target view handle for the swap chain render target textures.
    /// </summary>
    public GorgonRenderTargetView Target => _renderTargetViews[CurrentBackBufferIndex];

    /// <inheritdoc/>
    public bool FlipDiscard => _info.FlipDiscard;

    /// <inheritdoc/>
    public BufferFormat Format => _info.Format;

    /// <inheritdoc/>
    public bool TripleBuffer => _info.TripleBuffer;

    /// <inheritdoc/>
    public int Height => _info.Height;

    /// <inheritdoc/>
    public int Width => _info.Width;

    /// <inheritdoc/>
    public bool AllowScaling => _info.AllowScaling;

    /// <summary>
    /// Property to return whether the swap chain is in windowed mode, or fullscreen mode.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the swap chain is in fullscreen mode, this value returns <b>true</b>. To enter fullscreen mode, call the <see cref="EnterFullscreen"/> method. To return this value to <b>false</b>, simply call 
    /// the <see cref="ExitFullscreen"/> method.
    /// </para>
    /// <para>
    /// <inheritdoc cref="EnterFullscreen" path="/remarks/para/note[@type='warning']"/>
    /// </para>
    /// </remarks>
    /// <seealso cref="EnterFullscreen"/>
    /// <seealso cref="ExitFullscreen"/>
    public bool IsWindowed => FullscreenOutput == GorgonVideoOutputInfo.Empty;

    /// <summary>
    /// Property to return the output that is being used for fullscreen mode.
    /// </summary>
    /// <remarks>
    /// If the <see cref="IsWindowed"/> property is set to <b>true</b> (by calling <see cref="EnterFullscreen"/>), then this will return information about the output that is being used. Otherwise, if the 
    /// <see cref="IsWindowed"/> property is set to <b>false</b> (by calling <see cref="ExitFullscreen"/>), then this will return <b>null</b>.
    /// </remarks>
    /// <seealso cref="IsWindowed"/>
    /// <seealso cref="EnterFullscreen"/>
    /// <seealso cref="ExitFullscreen"/>
    public GorgonVideoOutputInfo FullscreenOutput
    {
        get;
        private set;
    } = GorgonVideoOutputInfo.Empty;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        // If we're consumed by the finalizer for whatever reason, then force windowed mode again no matter if we've disposed directly or not.
        if (_dxgiSwapChain.Get() is not null)
        {
            _dxgiSwapChain.Get()->SetFullscreenState(false, null);
        }        

        if (disposing)
        {
            this.UnregisterDisposable(Graphics);

            FullscreenOutput = GorgonVideoOutputInfo.Empty;

            for (int i = 0; i < _renderTargetViews.Length; i++)
            {
                _renderTargetViews[i]?.Dispose();
            }

            if (_waitHandle != HANDLE.NULL)
            {
                Graphics.Log.Print($"Closing swap chain wait event handle.", LoggingLevel.Verbose);
            }

            Graphics.Log.Print($"Destroying {nameof(IDXGISwapChain4)} '{Name}'.", LoggingLevel.Verbose);

            _renderTargetViews =[];
        }

        if ((_waitHandle != HANDLE.NULL) && (_waitHandle != HANDLE.INVALID_VALUE))
        {
            Win32.CloseHandle(_waitHandle);
        }
        _dxgiSwapChain.Dispose();
    }

    /// <summary>
    /// Function to reset the back buffers for the swap chain.
    /// </summary>
    private void OnBeforeResize()
    {
        for (int i = 0; i < _renderTargetViews.Length; ++i)
        {
            _renderTargetViews[i].Dispose();
        }

        Array.Clear(_renderTargetViews);

        CurrentBackBufferIndex = -1;
    }

    /// <summary>
    /// Function to restore and resize the back buffers for the swap chain.
    /// </summary>
    private void OnAfterResize()
    {
        DXGI_SWAP_CHAIN_DESC1 desc = _info.ToDXGI();

        _dxgiSwapChain.Get()->ResizeBuffers(0, desc.Width, desc.Height, DXGI_FORMAT.DXGI_FORMAT_UNKNOWN, desc.Flags)
            .ThrowIfFailed(GorgonResult.CannotInitialize, () => string.Format(Resources.GORGFX_ERR_CANNOT_RESIZE_SWAPCHAIN, Name));

        ResizeResources();

        CurrentBackBufferIndex = (int)_dxgiSwapChain.Get()->GetCurrentBackBufferIndex();
    }

    /// <summary>
    /// Function to validate the information for the swap chain.
    /// </summary>
    /// <param name="info">The information used to create the swap chain.</param>
    /// <exception cref="GorgonException">Thrown if the swap chain information parameters are invalid.</exception>
    private void ValidateInfo(GorgonSwapChainInfo info)
    {
        if ((!Graphics.FormatSupport.TryGetValue(info.Format, out GorgonBufferFormatSupport? format)) || (!format.IsDisplayFormat))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FORMAT_INVALID_DISPLAY_FORMAT, info.Format));
        }

        if (info.Width < 1)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_WIDTH_TOO_SMALL, info.Width, 1));
        }

        if (info.Height < 1)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_WIDTH_TOO_SMALL, info.Height, 1));
        }

        Graphics.Log.Print($"Swap chain format: {info.Format}.", LoggingLevel.Intermediate);
        Graphics.Log.Print($"Swap chain size: {info.Width}x{info.Height}.", LoggingLevel.Intermediate);
        Graphics.Log.Print($"Swap chain is triple buffered: {info.TripleBuffer}.", LoggingLevel.Intermediate);
    }

    /// <summary>
    /// Function to perform the resize of the back buffers for the swap chain.
    /// </summary>
    private void ResizeResources()
    {   
        for (uint i = 0; i < _renderTargetViews.Length; ++i)
        {
            GorgonTexture backBuffer = GorgonTexture.FromSwapChain(this, i);
            _renderTargetViews[i] = backBuffer.GetRenderTargetView();
        }
    }

    /// <summary>
    /// Function to build the native objects behind the swap chain.
    /// </summary>
    /// <param name="factory">The DXGI factory interface to use when creating the swap chain.</param>
    /// <param name="cmdQueue">The D3D graphics command queue to bind to the swap chain.</param>
    /// <returns>Returns the swap chain, render target views, and the swap chain wait handle.</returns>
    private (ComPtr<IDXGISwapChain4> SwapChain, HANDLE waitHandle) CreateNativeObjects(ref readonly ComPtr<IDXGIFactory7> factory, ref readonly ComPtr<ID3D12CommandQueue> cmdQueue)
    {
        using ComPtr<IDXGISwapChain1> initialObj = default;

        ComPtr<IDXGISwapChain4> result = default;        

        HWND hwnd = new((void*)WindowHandle);
        HANDLE waitHandle = HANDLE.NULL;
        string swapChainName = $"Gorgon DXGI Swap Chain '{Name}'";        

        DXGI_SWAP_CHAIN_DESC1 desc = _info.ToDXGI();

        Graphics.Log.Print($"Creating {nameof(IDXGISwapChain4)} '{Name}'.", LoggingLevel.Verbose);

        // Store for later use in Resize method.
        _descFlags = desc.Flags;
        factory.Get()->CreateSwapChainForHwnd((PID3D12CommandQueue)cmdQueue.Get(), hwnd, &desc, null, null, initialObj.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_SWAPCHAIN, Name));

        Graphics.Log.PrintError(factory.Get()->MakeWindowAssociation(hwnd, DXGI.DXGI_MWA_NO_ALT_ENTER | DXGI.DXGI_MWA_NO_WINDOW_CHANGES),
            "Could not assign window association flags for the swap chain. This swap chain may have unpredictable behaviour.", LoggingLevel.Simple);

        initialObj.As(ref result)
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_SWAPCHAIN, Name));
        
        result.SetDXGIDebugName(swapChainName);

        waitHandle = result.Get()->GetFrameLatencyWaitableObject();

        if ((waitHandle == HANDLE.INVALID_VALUE) || (waitHandle == HANDLE.NULL))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_SWAPCHAIN_WAIT_HANDLE_INVALID, Name));
        }

        Graphics.Log.Print($"Swap chain wait handle: 0x{((nint)waitHandle.Value).FormatHex()}.", LoggingLevel.Verbose);

        CurrentBackBufferIndex = 0;

        return (result, waitHandle);
    }

    /// <summary>
    /// Function to flip the current back buffer to the display.
    /// </summary>
    /// <param name="interval">The presentation interval.</param>
    /// <exception cref="GorgonException">Thrown if the presentation failed due to a device removal, or some internal error.</exception>
    /// <remarks>
    /// <para>
    /// This will present the current back buffer to the display, and return the display buffer to the back of the back buffer queue. Presentations are synchronized between vertical blank periods as 
    /// specified by the <paramref name="interval"/> parameter.
    /// </para>
    /// <para>
    /// If the <paramref name="interval"/> is set to:
    /// <list type="bullet">
    ///     <item>
    ///         <description>0 - The presentation occurs immediately.</description>
    ///     </item>
    ///     <item>
    ///         <description>1 to 4 - Synchronize for at least <paramref name="interval"/> vertical blank periods.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// <para>
    /// If the device is physcially removed, this method will throw an exception on presentation. The HRESULT code that represents the device removal reason will be included in the exception message.
    /// </para>
    /// </remarks>
    internal void Present(uint interval)
    {
        uint flags = (interval == 0) && (IsWindowed) && (Graphics.Adapter.AllowTearing) ? DXGI.DXGI_PRESENT_ALLOW_TEARING : 0;

        HRESULT err = _dxgiSwapChain.Get()->Present(interval, flags);

        if (err.FAILED)
        {
            if (err.Value == DXGI.DXGI_ERROR_DEVICE_REMOVED)
            {
                err = Graphics.D3DDevice.Get()->GetDeviceRemovedReason();
                Graphics.Log.PrintError(err, "Device removed.", LoggingLevel.Verbose);
            }
            else
            {
                Graphics.Log.PrintError(err, "Fatal error during presentation.", LoggingLevel.Verbose);
            }

            err.ThrowIfFailed(GorgonResult.DriverError, () => Resources.GORGFX_ERR_DEVICE_REMOVED);
        }

        CurrentBackBufferIndex = (int)_dxgiSwapChain.Get()->GetCurrentBackBufferIndex();
    }

    /// <summary>
    /// Function to resize the swap chain buffers.
    /// </summary>
    /// <param name="width">The new width of the swap chain.</param>
    /// <param name="height">The new height of the swap chain.</param>
    /// <remarks>
    /// <para>
    /// This method is used to change the size of the swap chain buffers. Usually applications will call this in response to a window size change event of some kind so that the swap chain back buffers are 
    /// set to the same size as the client area of the window. However, applications can call this to set the buffer size to anything they wish (between 1 to 16384 pixels for width/height).
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// It is recommended to keep the swap buffer size the same as the window client size when in fullscreen mode, or in windowed mode when Multi Plane Overlays are in effect. Keeping the buffers at the 
    /// correct size will yield performance improvements, but a mismatch in size will impair performance by returning control to the DWM and increasing latency.
    /// </para>
    /// <para>
    /// See this article for more information: <a target="_blank" href="https://wiki.special-k.info/en/SwapChain">https://wiki.special-k.info/en/SwapChain</a>.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public void Resize(int width, int height)
    {
        width = width.Max(1).Min(D3D12.D3D12_REQ_TEXTURE2D_U_OR_V_DIMENSION);
        height = height.Max(1).Min(D3D12.D3D12_REQ_TEXTURE2D_U_OR_V_DIMENSION);

        if ((_info.Width == width) && (_info.Height == height))
        {
            return;
        }

        Graphics.Log.Print($"Resizing swap chain {Name}...", LoggingLevel.Verbose);

        // Wait for the GPU to finish its current work.
        Graphics.WaitForGpu(GorgonGraphics.WaitFenceTimeout * 6);
               
        OnBeforeResize();

        _info = _info with
        {
            Width = width,
            Height = height
        };

        OnAfterResize();

        Graphics.Log.Print($"Swap chain {Name} resized to {_info.Width}x{_info.Height}.", LoggingLevel.Verbose);
    }


    /// <summary>
    /// Function to set the swap chain to fullscreen mode.
    /// </summary>
    /// <param name="output">[Optional] The output that will host the fullscreen swap chain.</param>
    /// <exception cref="GorgonException">Thrown if the swap chain failed being set to fullscreen mode.</exception>
    /// <remarks>
    /// <para>
    /// Use this to enter fullscreen mode on the swap chain. This method will find the video mode that is closest to the client size of the window, and use that as the dimensions for fullscreen mode.
    /// </para>
    /// <para>
    /// If the <paramref name="output"/> is <b>null</b>, then Gorgon will find the output that contains the largest portion of the window bound to the swap chain. The output that is used for fullscreen mode 
    /// will be assigned to the <see cref="FullscreenOutput"/> property.
    /// </para>
    /// <para>
    /// Applications can determine if they're in fullscreen mode by checking the <see cref="IsWindowed"/> property. To exit fullscreen mode, call the <see cref="ExitFullscreen"/> method.
    /// </para>
    /// <para>
    /// If the swap chain is already in fullscreen mode, then this method will do nothing.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// An "exclusive" fullscreen mode no longer provides any benefit and has been replaced by DirectFlip/Fullscreen performance optimizations starting with Windows 10. Instead, the desktop is switched to 
    /// the appropriate video mode, and a fullscreen borderless window is used. 
    /// </para>
    /// <para>
    /// The best practice is to make a full screen borderless window, render to a render target texture that is sized to the desired resolution, and then copy the contents of the render target to the swap 
    /// chain. This also has the added benefit of allowing developers to resize the image as they see fit, provide filtering (or no filtering), post processing effects like CRT shaders, etc...
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="IsWindowed"/>
    /// <seealso cref="FullscreenOutput"/>
    /// <seealso cref="ExitFullscreen"/>
    public void EnterFullscreen(GorgonVideoOutputInfo? output = null)
    {
        if (!IsWindowed)
        {
            return;
        }

        Graphics.WaitForGpu(GorgonGraphics.WaitFenceTimeout * 6);

        Graphics.Queues.GraphicsQueue.Tracker.Signal();
        Graphics.Queues.ComputeQueue.Tracker.Signal();
        Graphics.Queues.CopyQueue.Tracker.Signal();

        RECT rect = default;
        Win32.GetClientRect(new HWND((void*)WindowHandle), &rect);

        OnBeforeResize();

        if ((output is null) || (output == GorgonVideoOutputInfo.Empty))
        {
            output = Graphics.Adapter.Outputs.GetOutputFromWindowHandle(WindowHandle);

            if (output == GorgonVideoOutputInfo.Empty)
            {
                throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_ERR_CANNOT_SET_FULLSCREEN, Graphics.Adapter.Name));
            }
        }

        // For the backbuffers to resize to whatever our window client area is.
        // D3D 12 uses the current back buffer size to locate the video mode, but I prefer this.
        if (((rect.bottom - rect.top) != _info.Height) || ((rect.right - rect.left) != _info.Width))
        {
            _info = _info with
            {
                Width = rect.right - rect.left,
                Height = rect.bottom - rect.top
            };

            DXGI_SWAP_CHAIN_DESC1 desc = _info.ToDXGI();

            _dxgiSwapChain.Get()->ResizeBuffers(desc.BufferCount, desc.Width, desc.Height, desc.Format, desc.Flags)
                .ThrowIfFailed(GorgonResult.CannotInitialize, () => string.Format(Resources.GORGFX_ERR_CANNOT_RESIZE_SWAPCHAIN, Name));
        }

        using ComPtr<IDXGIOutput> dxgiOutput = default;
        Graphics.DXGIAdapter.Get()->EnumOutputs((uint)output.Index, dxgiOutput.GetAddressOf())
            .ThrowIfFailed(GorgonResult.AccessDenied, () => string.Format(Resources.GORGFX_ERR_CANNOT_SET_FULLSCREEN, Graphics.Adapter.Name));

        _dxgiSwapChain.Get()->SetFullscreenState(true, dxgiOutput.Get())
            .ThrowIfFailed(GorgonResult.CannotExecute, () => string.Format(Resources.GORGFX_ERR_CANNOT_SET_SCREEN_MODE, Graphics.Adapter.Name));

        FullscreenOutput = output;

        OnAfterResize();
    }

    /// <summary>
    /// Function to set the swap chain to windows mode.
    /// </summary>
    /// <exception cref="GorgonException">Thrown if the swap chain failed being set to windowed mode.</exception>
    /// <remarks>
    /// <para>
    /// Use this to exit fullscreen mode on the swap chain. The window containing the swap chain will be restored to its previous settings.
    /// </para>
    /// <para>
    /// Applications can determine if they're in fullscreen mode by checking the <see cref="IsWindowed"/> property. To enter fullscreen mode, call the <see cref="EnterFullscreen"/> method.
    /// </para>
    /// <para>
    /// If the swap chain is already in windowed mode, then this method will do nothing.
    /// </para>
    /// </remarks>
    /// <seealso cref="IsWindowed"/>
    /// <seealso cref="FullscreenOutput"/>
    /// <seealso cref="EnterFullscreen"/>
    public void ExitFullscreen()
    {
        if (IsWindowed)
        {
            return;
        }

        Graphics.WaitForGpu(GorgonGraphics.WaitFenceTimeout * 6);

        Graphics.Queues.GraphicsQueue.Tracker.Signal();
        Graphics.Queues.ComputeQueue.Tracker.Signal();
        Graphics.Queues.CopyQueue.Tracker.Signal();

        OnBeforeResize();

        _dxgiSwapChain.Get()->SetFullscreenState(false, null)
            .ThrowIfFailed(GorgonResult.CannotExecute, () => string.Format(Resources.GORGFX_ERR_CANNOT_SET_SCREEN_MODE, Graphics.Adapter.Name));

        FullscreenOutput = GorgonVideoOutputInfo.Empty;

        OnAfterResize();
    }

    /// <summary>
    /// Function to force the system to wait for the previous frame to display.
    /// </summary>
    /// <param name="timeout">[Optional] The number of milliseconds to wait before continuing.</param>
    /// <remarks>
    /// <para>
    /// This method allows an object to lower frame latency by waiting until the previous frame is rendered and displayed before moving on to the next frame. Applications should call this method before any 
    /// rendering operations are executed (including the very first frame). 
    /// </para>
    /// <para>
    /// The <paramref name="timeout"/> value defaults to infinity, but users may enter a shorter time frame to avoid having the application freeze indefinitely (this should never really happen).
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WaitForFrameLatency(int timeout = Timeout.Infinite)
    {
        if ((_waitHandle == HANDLE.NULL) || (_waitHandle == HANDLE.INVALID_VALUE))
        {
            return;
        }

        _ = Win32.WaitForSingleObjectEx(_waitHandle, (uint)timeout, true);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Graphics.Log.Print($"Destroying {nameof(GorgonSwapChain)} '{Name}'.", LoggingLevel.Simple);

        Dispose(true);

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~GorgonSwapChain() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonSwapChain"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface to associate with this swap chain.</param>
    /// <param name="name">The name of the swap chain.</param>
    /// <param name="windowHandle">The native handle (HWND) to the window that will host the swap chain.</param>
    /// <param name="info">The information used to define the swap chain properties.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="windowHandle"/> parameter is <b>null</b> (<c>nint.Zero or IntPtr.Zero</c>)(</exception> 
    public GorgonSwapChain(GorgonGraphics graphics, string name, nint windowHandle, GorgonSwapChainInfo info)
    {
        if (windowHandle == nint.Zero)
        {
            throw new ArgumentNullException(nameof(windowHandle), Resources.GORGFX_NULL_WINDOW_HANDLE);
        }        

        Graphics = graphics;
        Name = GorgonGraphicsFactory.GenerateName(name, nameof(GorgonSwapChain));
        WindowHandle = windowHandle;

        Graphics.Log.Print($"Creating swap chain '{Name}'.", LoggingLevel.Simple);

        ValidateInfo(info);

        _info = new GorgonSwapChainInfo(info);
        _renderTargetViews = new GorgonRenderTargetView[_info.ResourceCount];

        (_dxgiSwapChain, _waitHandle) = CreateNativeObjects(in graphics.DXGIFactory, in graphics.Queues.GraphicsQueue.D3DQueue);
        ResizeResources();

        this.RegisterDisposable(Graphics);
    }
}
