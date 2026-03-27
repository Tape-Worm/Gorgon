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
// Created: July 6, 2025 4:11:03 PM
//

using System.Runtime.CompilerServices;
using System.Text;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Extension methods used to convert Gorgon and DXGI/Direct3D specific values.
/// </summary>
internal static class D3DConversionExtensions
{
    extension(DXGI_MODE_DESC1 mode)
    {
        /// <summary>
        /// Function to convert a DXGI_MODE_DESC1 into a <see cref="GorgonVideoMode"/>.
        /// </summary>
        /// <returns>The converted mode.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GorgonVideoMode ToGorgonVideoMode() => new(
            (int)mode.Width,
            (int)mode.Height,
            (BufferFormat)mode.Format,
            new GorgonRationalNumber((int)mode.RefreshRate.Numerator, (int)mode.RefreshRate.Denominator),
            mode.Stereo,
            (ModeScaling)mode.Scaling,
            (ModeScanlineOrder)mode.ScanlineOrdering);
    }

    extension(D3D12_RESOURCE_DIMENSION dimension)
    {
        /// <summary>
        /// Function to convert a D3D12_RESOURCE_DIMENSION to a <see cref="TextureType"/>.
        /// </summary>
        /// <returns>The texture type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextureType ToTextureType() => dimension switch
        {
            D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE1D => TextureType.Texture1D,
            D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE2D => TextureType.Texture2D,
            D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE3D => TextureType.Texture3D,
            _ => TextureType.Unknown
        };
    }

    extension(TextureType textureType)
    {
        /// <summary>
        /// Function to convert a <see cref="TextureType"/> to a D3D12_RESOURCE_DIMENSION.
        /// </summary>
        /// <returns>The resource dimension.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D3D12_RESOURCE_DIMENSION ToD3DResourceDimension() => textureType switch
        {
            TextureType.Texture1D => D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE1D,
            TextureType.Texture2D => D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE2D,
            TextureType.Texture3D => D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE3D,
            _ => D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_UNKNOWN
        };
    }

    extension(D3D_SHADER_MODEL d3dSm)
    {
        /// <summary>
        /// Function to convert a D3D shader model value to a <see cref="ShaderModel"/> value.
        /// </summary>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ShaderModel ToGorgonShaderModel() => d3dSm switch
        {
            D3D_SHADER_MODEL.D3D_SHADER_MODEL_6_6 => ShaderModel.ShaderModel_6_6,
            D3D_SHADER_MODEL.D3D_SHADER_MODEL_6_7 => ShaderModel.ShaderModel_6_7,
            D3D_SHADER_MODEL.D3D_SHADER_MODEL_6_8 => ShaderModel.ShaderModel_6_8,
            D3D_SHADER_MODEL.D3D_HIGHEST_SHADER_MODEL => ShaderModel.ShaderModel_6_8,
            _ => ShaderModel.Unsupported
        };
    }    

    extension(GorgonBox box)
    {
        /// <summary>
        /// Function to convert a <see cref="GorgonBox"/> type to a D3D 12 box type.
        /// </summary>
        /// <returns>The converted box type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D3D12_BOX ToD3DBox() => new(box.X, box.Y, box.Z, box.Right, box.Bottom, box.Back);
    }

    /// <typeparam name="T">The type of object being updated.</typeparam>
    extension<T>(ref T self) where T : unmanaged, ID3D12Object.Interface
    {
        /// <summary>
        /// Function to set a debug name on the underlying direct 3D object.
        /// </summary>        
        /// <param name="name">The new name for the object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetD3DDebugName(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                self.SetName(null);
                return;
            }

            if (name.Length > 512)
            {
                name = name[..512];
            }

            fixed (char* strPtr = name)
            {
                self.SetName(strPtr);
            }
        }
    }

    /// <typeparam name="T">The type of object being updated.</typeparam>
    extension<T>(ref T self) where T : unmanaged, IDXGIObject.Interface
    {
        /// <summary>
        /// Function to set a debug name on the underlying DXGI object.
        /// </summary>        
        /// <param name="name">The new name for the object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetDXGIDebugName(string? name)
        {
            Guid wkID = DirectX.WKPDID_D3DDebugObjectNameW;

            if (string.IsNullOrEmpty(name))
            {
                self.SetPrivateData(&wkID, 0, null);
                return;
            }

            if (name.Length > 512)
            {
                name = name[..512];
            }

            fixed (char* strPtr = name)
            {
                self.SetPrivateData(&wkID, (uint)Encoding.Unicode.GetByteCount(name), (ushort*)strPtr);
            }
        }
    }

    /// <typeparam name="T">The type of object being updated.</typeparam>
    extension<T>(ComPtr<T> self) where T : unmanaged, ID3D12Object.Interface
    {
        /// <summary>
        /// Function to set a debug name on the underlying direct 3D object.
        /// </summary>        
        /// <param name="name">The new name for the object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetD3DDebugName(string? name) => self.Get()->SetD3DDebugName(name);
    }

    /// <typeparam name="T">The type of object being updated.</typeparam>
    extension<T>(ComPtr<T> self) where T : unmanaged, IDXGIObject.Interface
    {
        /// <summary>
        /// Function to set a debug name on the underlying DXGI object.
        /// </summary>        
        /// <param name="name">The new name for the object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetDXGIDebugName(string? name) => self.Get()->SetDXGIDebugName(name);
    }

    extension(BufferUsage usage)
    {
        /// <summary>
        /// Function to convert a <see cref="BufferUsage"/> value to a D3D 12 heap type.
        /// </summary>
        /// <param name="graphics">The graphics object used to determine the type of heap.</param>
        /// <returns>The D3D 12 heap type.</returns>
        /// <exception cref="InvalidCastException">Thrown if the usage type is unknown.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D3D12_HEAP_TYPE ToD3DHeapType(GorgonGraphics graphics) => usage switch
        {
            BufferUsage.Default => D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT,
            BufferUsage.Upload => graphics.Adapter.HasGpuUploadSupport ? D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_GPU_UPLOAD : D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_UPLOAD,
            BufferUsage.Download => D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_READBACK,
            _ => throw new InvalidCastException()
        };
    }

}
