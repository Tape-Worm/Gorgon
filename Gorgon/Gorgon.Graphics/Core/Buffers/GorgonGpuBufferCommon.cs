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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: March 26, 2026 9:01:06 PM
//

using TerraFX.Interop.Windows;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Common functionality for GPU buffers.
/// </summary>
/// <param name="graphics"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='graphics']"/></param>
/// <param name="name"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='name']"/></param>
/// <param name="info">Information used to create the buffer.</param>
public abstract class GorgonGpuBufferCommon(GorgonGraphics graphics, string name, GorgonCommonBufferInfo info)
        : GorgonGpuResource(graphics, name), IGorgonCommonBufferInfo
{
    private readonly GorgonCommonBufferInfo _info = info;

    /// <inheritdoc/>
    public long SizeInBytes => _info.SizeInBytes;

    /// <inheritdoc/>
    public bool IsUnorderedAccess => _info.IsUnorderedAccess;

    /// <summary>
    /// Property to return the offset, in bytes, of a suballocated buffer within a larger buffer.
    /// </summary>
    internal abstract ulong ResourceOffset
    {
        get;
    }

    /// <summary>
    /// Function to validate the settings for the buffer.
    /// </summary>
    private protected abstract void ValidateInfo();
}