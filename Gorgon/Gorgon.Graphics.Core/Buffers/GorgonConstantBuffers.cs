
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
// Created: July 9, 2016 3:47:30 PM
// 

using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A list of constant buffers used for shaders
/// </summary>
/// <remarks>
/// <para>
/// This is used to pass a set of <see cref="GorgonConstantBuffer"/> objects to a <see cref="GorgonDrawCallCommon"/> object. This allows an application to set a single or multiple constant buffers at 
/// the same time and thus provides a performance boost over setting them individually. 
/// </para>
/// </remarks>
public sealed class GorgonConstantBuffers
    : GorgonArray<GorgonConstantBufferView>
{
    /// <summary>
    /// The maximum size for a constant buffer binding list.
    /// </summary>
    public const int MaximumConstantBufferCount = D3D11.CommonShaderStage.ConstantBufferApiSlotCount;

    /// <summary>
    /// Property to return the native buffers.
    /// </summary>
    internal D3D11.Buffer[] Native
    {
        get;
    } = new D3D11.Buffer[MaximumConstantBufferCount];

    /// <summary>
    /// Property to return the start of the view.
    /// </summary>
    internal int[] ViewStart
    {
        get;
    } = new int[MaximumConstantBufferCount];

    /// <summary>
    /// Property to return the number of elements in the view.
    /// </summary>
    internal int[] ViewCount
    {
        get;
    } = new int[MaximumConstantBufferCount];

    /// <inheritdoc/>
    protected override void OnMapDirtyItem(int index, int rangeIndex, bool isDirty)
    {
        GorgonConstantBufferView value = this[index];
        Native[rangeIndex] = value?.Buffer?.Native;
        ViewStart[rangeIndex] = value?.StartElement * 16 ?? 0;
        ViewCount[rangeIndex] = value?.ElementCount * 16 ?? 0;
    }

    /// <summary>
    /// Function called when the array is cleared.
    /// </summary>
    protected override void OnClear()
    {
        Array.Clear(Native, 0, Native.Length);
        Array.Clear(ViewStart, 0, ViewStart.Length);
        Array.Clear(ViewCount, 0, ViewCount.Length);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonConstantBuffers"/> class.
    /// </summary>
    /// <param name="bufferViews">[Optional] A list of buffer views to copy into the the list.</param>
    public GorgonConstantBuffers(IReadOnlyList<GorgonConstantBufferView> bufferViews = null)
        : base(MaximumConstantBufferCount)
    {
        if (bufferViews is null)
        {
            return;
        }

        for (int i = 0; i < bufferViews.Count.Min(Length); ++i)
        {
            this[i] = bufferViews[i];
        }
    }    
}
