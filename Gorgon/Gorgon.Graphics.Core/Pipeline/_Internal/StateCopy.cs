#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: May 29, 2018 4:41:12 PM
// 
#endregion

using Gorgon.Collections;
using Gorgon.Math;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Functions to assist with copying state in collections.
/// </summary>
internal static class StateCopy
{
    /// <summary>
    /// Function to copy a list of blend states to the list provided.
    /// </summary>
    /// <param name="dest">The destination list.</param>
    /// <param name="src">The source list.</param>
    /// <param name="startSlot">The starting index.</param>
    public static void CopyBlendStates(GorgonArray<GorgonBlendState> dest, IReadOnlyList<GorgonBlendState> src, int startSlot)
    {
        dest.Clear();

        if (src is null)
        {
            return;
        }

        int length = src.Count.Min(dest.Length - startSlot);

        for (int i = 0; i < length; ++i)
        {
            dest[i + startSlot] = src[i];
        }
    }

    /// <summary>
    /// Function to copy shader resource views.
    /// </summary>
    /// <param name="destStates">The destination shader resource views.</param>
    /// <param name="srcStates">The shader resource views to copy.</param>
    public static void CopySrvs(GorgonShaderResourceViews destStates, GorgonShaderResourceViews srcStates)
    {
        destStates.Clear();
        srcStates?.CopyTo(destStates);
    }

    /// <summary>
    /// Function to copy shader resource views.
    /// </summary>
    /// <param name="destStates">The destination shader resource views.</param>
    /// <param name="srcStates">The shader resource views to copy.</param>
    /// <param name="startSlot">The slot to start copying into.</param>
    public static void CopySrvs(GorgonShaderResourceViews destStates, IReadOnlyList<GorgonShaderResourceView> srcStates, int startSlot)
    {
        destStates.Clear();

        if (srcStates is null)
        {
            return;
        }

        int length = srcStates.Count.Min(destStates.Length - startSlot);

        for (int i = 0; i < length; ++i)
        {
            destStates[i + startSlot] = srcStates[i];
        }
    }

    /// <summary>
    /// Function to copy unordered access resource views.
    /// </summary>
    /// <param name="destStates">The destination unordered access views.</param>
    /// <param name="srcStates">The unordered access views to copy.</param>
    /// <param name="startSlot">The slot to start copying into.</param>
    public static void CopyReadWriteViews(GorgonReadWriteViewBindings destStates, IReadOnlyList<GorgonReadWriteViewBinding> srcStates, int startSlot)
    {
        destStates.Clear();

        if (srcStates is null)
        {
            return;
        }

        int length = srcStates.Count.Min(destStates.Length - startSlot);

        for (int i = 0; i < length; ++i)
        {
            destStates[i + startSlot] = srcStates[i];
        }
    }

    /// <summary>
    /// Function to copy samplers.
    /// </summary>
    /// <param name="destStates">The destination sampler states.</param>
    /// <param name="srcStates">The sampler states to copy.</param>
    /// <param name="startSlot">The slot to start copying into.</param>
    public static void CopySamplers(GorgonSamplerStates destStates, IReadOnlyList<GorgonSamplerState> srcStates, int startSlot)
    {
        destStates.Clear();

        if (srcStates is null)
        {
            return;
        }

        int length = srcStates.Count.Min(destStates.Length - startSlot);

        for (int i = 0; i < length; ++i)
        {
            destStates[i] = srcStates[i];
        }
    }

    /// <summary>
    /// Function to copy a list of constant buffers to the list provided.
    /// </summary>
    /// <param name="dest">The destination list.</param>
    /// <param name="src">The source list.</param>
    /// <param name="startSlot">The starting index.</param>
    public static void CopyConstantBuffers(GorgonConstantBuffers dest, IReadOnlyList<GorgonConstantBufferView> src, int startSlot)
    {
        dest.Clear();

        if (src is null)
        {
            return;
        }

        int length = src.Count.Min(dest.Length - startSlot);

        for (int i = 0; i < length; ++i)
        {
            dest[i + startSlot] = src[i];
        }
    }

    /// <summary>
    /// Function to copy stream output buffer bindings from one draw call to another
    /// </summary>
    /// <param name="destBindings">The bindings to update.</param>
    /// <param name="srcBindings">The bindings to copy.</param>
    public static void CopyStreamOutBuffers(GorgonStreamOutBindings destBindings, IReadOnlyList<GorgonStreamOutBinding> srcBindings)
    {
        destBindings.Clear();

        if (srcBindings is null)
        {
            return;
        }

        int count = srcBindings.Count.Min(GorgonStreamOutBindings.MaximumStreamOutCount);

        for (int i = 0; i < count; ++i)
        {
            destBindings[i] = srcBindings[i];
        }
    }

    /// <summary>
    /// Function to copy vertex buffer bindings from one draw call to another
    /// </summary>
    /// <param name="destBindings">The bindings to update.</param>
    /// <param name="srcBindings">The bindings to copy.</param>
    /// <param name="layout">The input layout.</param>
    public static void CopyVertexBuffers(GorgonVertexBufferBindings destBindings, IReadOnlyList<GorgonVertexBufferBinding> srcBindings, GorgonInputLayout layout)
    {
        destBindings.Clear();
        destBindings.InputLayout = layout;

        if (srcBindings is null)
        {
            return;
        }

        int count = srcBindings.Count.Min(GorgonVertexBufferBindings.MaximumVertexBufferCount);

        for (int i = 0; i < count; ++i)
        {
            destBindings[i] = srcBindings[i];
        }
    }
}
