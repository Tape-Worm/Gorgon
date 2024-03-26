
// 
// Gorgon
// Copyright (C) 2016 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: July 26, 2016 10:35:58 PM
// 

using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A list of <see cref="GorgonVertexBufferBinding"/> values
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="GorgonVertexBufferBinding"/> is used to bind a vertex buffer to the GPU pipeline so that it may be used for rendering
/// </para>
/// </remarks>
public sealed class GorgonVertexBufferBindings
    : GorgonArray<GorgonVertexBufferBinding>
{
    /// <summary>
    /// The maximum number of vertex buffers allow to be bound at the same time.
    /// </summary>
    public const int MaximumVertexBufferCount = D3D11.InputAssemblerStage.VertexInputResourceSlotCount;

    /// <summary>
    /// Property to return the native items wrapped by this list.
    /// </summary>
    internal D3D11.VertexBufferBinding[] Native
    {
        get;
    }

    /// <summary>
    /// Property to return the input layout assigned to the buffer bindings.
    /// </summary>
    /// <remarks>
    /// The input layout defines how the vertex data is arranged within the vertex buffers.
    /// </remarks>
    public GorgonInputLayout InputLayout
    {
        get;
        internal set;
    }

    /// <inheritdoc/>
    protected override void OnMapDirtyItem(int index, int rangeIndex, bool isDirty)
    {
        GorgonVertexBufferBinding value = this[index];

        if (value.Equals(GorgonVertexBufferBinding.Empty))
        {
            Native[rangeIndex] = default;
        }
        else
        {
            Native[rangeIndex] = value.ToVertexBufferBinding();
        }
    }

    /// <summary>
    /// Function called when the array is cleared.
    /// </summary>
    protected override void OnClear() => Array.Clear(Native, 0, Native.Length);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonVertexBufferBindings" /> class.
    /// </summary>
    internal GorgonVertexBufferBindings()
        : base(MaximumVertexBufferCount) => Native = new D3D11.VertexBufferBinding[MaximumVertexBufferCount];

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonVertexBufferBindings"/> class.
    /// </summary>
    /// <param name="inputLayout">The input layout that describes the arrangement of the vertex data within the buffers being bound.</param>
    /// <param name="bindings">[Optional] A list of vertex buffers to apply.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="inputLayout"/> parameter is <b>null</b>.</exception>
    public GorgonVertexBufferBindings(GorgonInputLayout inputLayout, IReadOnlyList<GorgonVertexBufferBinding> bindings = null)
        : this()
    {
        InputLayout = inputLayout ?? throw new ArgumentNullException(nameof(inputLayout));

        if (bindings is null)
        {
            return;
        }

        for (int i = 0; i < bindings.Count.Min(MaximumVertexBufferCount); ++i)
        {
            this[i] = bindings[i];
        }
    }
}
