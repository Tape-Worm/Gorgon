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
using System.Runtime.InteropServices;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using DX = TerraFX.Interop.DirectX.DirectX;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Wpf.Interop;

/// <summary>
/// The Direct3D 9Ex device that owns the surfaces handed to WPF.
/// </summary>
internal sealed unsafe class D3D9Device
    : IDisposable
{
    // D3D_SDK_VERSION from d3d9.h.
    private const uint SdkVersion = 32;
    // D3DCREATE_FPU_PRESERVE | D3DCREATE_MULTITHREADED | D3DCREATE_HARDWARE_VERTEXPROCESSING from d3d9.h. Same flags as Microsoft.Wpf.Interop.DirectX.
    private const uint CreateFlags = 0x00000002 | 0x00000004 | 0x00000040;
    // D3DPRESENT_INTERVAL_IMMEDIATE from d3d9.h.
    private const uint PresentIntervalImmediate = 0x80000000;
    // D3DUSAGE_RENDERTARGET from d3d9types.h.
    private const uint UsageRenderTarget = 0x00000001;

    // The Direct3D 9Ex object.
    private ComPtr<IDirect3D9Ex> _d3d9;
    // The Direct3D 9Ex device.
    private ComPtr<IDirect3DDevice9Ex> _device;

    /// <summary>
    /// Property to return whether the device has been lost, removed or hung, and must be recreated.
    /// </summary>
    public bool IsLost => _device.Get()->CheckDeviceState(HWND.NULL).FAILED;

    /// <summary>
    /// Function to throw if a call failed.
    /// </summary>
    /// <param name="hr">The result of the call.</param>
    /// <param name="what">The name of the call.</param>
    public static void Check(HRESULT hr, string what)
    {
        if (hr.FAILED)
        {
            throw new COMException($"'{what}' failed (0x{hr.Value:X8}).", hr.Value);
        }
    }

    /// <summary>
    /// Function to locate the Direct3D 9 adapter index for an adapter LUID.
    /// </summary>
    /// <param name="d3d9">The Direct3D 9Ex object.</param>
    /// <param name="adapterLuid">The LUID to find, or 0 for the default adapter.</param>
    /// <returns>The adapter index.</returns>
    private static uint FindAdapter(IDirect3D9Ex* d3d9, long adapterLuid)
    {
        if (adapterLuid == 0)
        {
            return 0;
        }

        uint count = d3d9->GetAdapterCount();

        for (uint i = 0; i < count; ++i)
        {
            LUID luid;

            if ((d3d9->GetAdapterLUID(i, &luid).SUCCEEDED) && ((((long)luid.HighPart << 32) | luid.LowPart) == adapterLuid))
            {
                return i;
            }
        }

        throw new InvalidOperationException($"No Direct3D 9 adapter has the LUID '0x{adapterLuid:X16}'. The surface can only be shared with a device on an adapter that Direct3D 9 can see (one with a display attached).");
    }

    /// <summary>
    /// Function to create a shared render target texture.
    /// </summary>
    /// <param name="width">The width of the texture, in pixels.</param>
    /// <param name="height">The height of the texture, in pixels.</param>
    /// <param name="texture">The texture.</param>
    /// <param name="surface">The surface for the first mip level of the texture, which is the back buffer for WPF.</param>
    /// <returns>The shared handle for the texture.</returns>
    public HANDLE CreateSharedTexture(int width, int height, out ComPtr<IDirect3DTexture9> texture, out ComPtr<IDirect3DSurface9> surface)
    {
        texture = default;
        surface = default;

        HANDLE sharedHandle = HANDLE.NULL;
        Check(_device.Get()->CreateTexture((uint)width, (uint)height, 1, UsageRenderTarget, D3DFORMAT.D3DFMT_A8R8G8B8, D3DPOOL.D3DPOOL_DEFAULT, texture.GetAddressOf(), &sharedHandle),
              "IDirect3DDevice9Ex::CreateTexture");
        Check(texture.Get()->GetSurfaceLevel(0, surface.GetAddressOf()), "IDirect3DTexture9::GetSurfaceLevel");

        return sharedHandle;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _device.Dispose();
        _d3d9.Dispose();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="D3D9Device"/> class.
    /// </summary>
    /// <param name="windowOwner">The window that owns the device, or 0 to use the desktop window.</param>
    /// <param name="adapterLuid">The LUID of the adapter to use, or 0 for the default adapter.</param>
    public D3D9Device(nint windowOwner, long adapterLuid)
    {
        try
        {
            Check(DX.Direct3DCreate9Ex(SdkVersion, _d3d9.GetAddressOf()), "Direct3DCreate9Ex");

            uint adapter = FindAdapter(_d3d9.Get(), adapterLuid);
            HWND focusWindow = windowOwner == 0 ? Win32.GetDesktopWindow() : (HWND)windowOwner;

            // The back buffer is never used; WPF only sees the textures created by CreateSharedTexture.
            D3DPRESENT_PARAMETERS presentParams = new()
            {
                BackBufferWidth = 1,
                BackBufferHeight = 1,
                BackBufferFormat = D3DFORMAT.D3DFMT_UNKNOWN,
                BackBufferCount = 1,
                SwapEffect = D3DSWAPEFFECT.D3DSWAPEFFECT_DISCARD,
                hDeviceWindow = focusWindow,
                Windowed = BOOL.TRUE,
                PresentationInterval = PresentIntervalImmediate
            };

            Check(_d3d9.Get()->CreateDeviceEx(adapter, D3DDEVTYPE.D3DDEVTYPE_HAL, focusWindow, CreateFlags, &presentParams, null, _device.GetAddressOf()), "IDirect3D9Ex::CreateDeviceEx");
        }
        catch
        {
            Dispose();
            throw;
        }
    }
}
