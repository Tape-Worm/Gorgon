
// 
// Gorgon
// Copyright (C) 2017 Michael Winsor
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
// Created: July 25, 2017 9:28:39 PM
// 

using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A list of <see cref="GorgonStreamOutBinding"/> values
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="GorgonStreamOutBinding"/> is used to bind a vertex buffer to the GPU pipeline so that it may be used for rendering
/// </para>
/// </remarks>
public sealed class GorgonStreamOutBindings
    : GorgonArray<GorgonStreamOutBinding>
{
    /// <summary>
    /// The maximum number of vertex buffers allow to be bound at the same time.
    /// </summary>
    public const int MaximumStreamOutCount = 4;

    /// <summary>
    /// Property to return the native items wrapped by this list.
    /// </summary>
    internal D3D11.StreamOutputBufferBinding[] Native
    {
        get;
    }

    /// <inheritdoc/>
    protected override void OnMapDirtyItem(int index, int rangeIndex, bool isDirty)
    {
        D3D11.StreamOutputBufferBinding binding = default;
        GorgonStreamOutBinding value = this[index];

        if (value.Buffer is not null)
        {
            binding = new D3D11.StreamOutputBufferBinding(value.Buffer?.Native, value.Offset);
        }

        Native[rangeIndex] = binding;
    }

    /// <summary>
    /// Function called when the array is cleared.
    /// </summary>
    protected override void OnClear() => Array.Clear(Native, 0, Native.Length);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStreamOutBindings"/> class.
    /// </summary>
    /// <param name="bindings">[Optional] The list of bindings to copy.</param>
    public GorgonStreamOutBindings(IReadOnlyList<GorgonStreamOutBinding> bindings = null)
        : base(MaximumStreamOutCount)
    {
        Native = new D3D11.StreamOutputBufferBinding[MaximumStreamOutCount];

        if (bindings is null)
        {
            return;
        }

        for (int i = 0; i < bindings.Count.Min(Length); ++i)
        {
            this[i] = bindings[i];
        }
    }
}
