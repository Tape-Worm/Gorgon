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
// Created: May 14, 2026 10:22:44 PM
//

using Gorgon.Core;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A handle returned from the <see cref="GorgonVirtualTexture.TryAllocate(ref readonly GorgonBoxF, out GorgonVirtualTextureHandle, short, short)"/> method on a <see cref="GorgonVirtualTexture"/>.
/// </summary>
/// <remarks>
/// <para>
/// This represents an opaque value for an allocation handle on a <see cref="GorgonVirtualTexture"/>. This is used to indentify the portion of memory allocated within the texture.
/// </para>
/// </remarks>
/// <seealso cref="GorgonVirtualTexture.TryAllocate(ref readonly GorgonBoxF, out GorgonVirtualTextureHandle, short, short)"/>
public readonly struct GorgonVirtualTextureHandle
    : IEquatable<GorgonVirtualTextureHandle>
{
    /// <summary>
    /// A null representation of a virtual texture handle.
    /// </summary>
    public static readonly GorgonVirtualTextureHandle Null = new(ulong.MaxValue, ulong.MaxValue);

    /// <summary>
    /// The ID of the texture that this allocation handle comes from.
    /// </summary>
    internal readonly ulong TextureID = ulong.MaxValue;

    /// <summary>
    /// The handle to the memory allocated.
    /// </summary>
    internal readonly ulong Handle = ulong.MaxValue;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is GorgonVirtualTextureHandle handle && Equals(handle);

    /// <inheritdoc/>
    public bool Equals(GorgonVirtualTextureHandle other) => other.Handle == Handle && other.TextureID == TextureID;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(TextureID, Handle);

    /// <inheritdoc/>
    public override string ToString() => $"0x{TextureID.FormatHex()}:0x{Handle.FormatHex()}";

    /// <summary>
    /// Operator to determine if two handles are equal.
    /// </summary>
    /// <param name="left">The left handle to compare.</param>
    /// <param name="right">The right handle to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public static bool operator ==(GorgonVirtualTextureHandle left, GorgonVirtualTextureHandle right) => left.Equals(right);

    /// <summary>
    /// Operator to determine if two handles are not equal.
    /// </summary>
    /// <param name="left">The left handle to compare.</param>
    /// <param name="right">The right handle to compare.</param>
    /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
    public static bool operator !=(GorgonVirtualTextureHandle left, GorgonVirtualTextureHandle right) => !left.Equals(right);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonVirtualTextureHandle"/> value.
    /// </summary>
    /// <param name="textureID">The ID of the texture that has the allocation.</param>
    /// <param name="handle">The handle to the memory allocation.</param>
    internal GorgonVirtualTextureHandle(ulong textureID, ulong handle)
    {
        TextureID = textureID;
        Handle = handle;
    }
}
