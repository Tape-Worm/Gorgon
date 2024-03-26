
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: May 30, 2018 10:23:53 PM
// 

using Gorgon.Collections;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A list of <see cref="GorgonReadWriteViewBinding"/> values
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="GorgonReadWriteViewBinding"/> is used to bind a <see cref="GorgonReadWriteView"/>to the GPU pipeline so that it may be used for rendering from a shader
/// </para>
/// <para>
/// When binding an unordered access view from a pixel shader it is important to note that they occupy the same slots as render targets. Thus, if the user sets an unordered access view at slot 0, and there 
/// is a render target already bound at that slot, then the target will be unbound and the unordered access view will replace it.  Conversely, if the user binds a render target at slot 0 and there is 
/// already an unordered access view there, then it will be unbound. It is the responsibility of the developer to ensure there are no clashes in slot management
/// </para>
/// </remarks>
public sealed class GorgonReadWriteViewBindings
    : GorgonArray<GorgonReadWriteViewBinding>
{
    /// <summary>
    /// The maximum number of vertex buffers allow to be bound at the same time.
    /// </summary>
    public const int MaximumReadWriteViewCount = 64;

    /// <summary>
    /// Property to return the native items wrapped by this list.
    /// </summary>
    internal D3D11.UnorderedAccessView[] Native
    {
        get;
    }

    /// <summary>
    /// Property to return the native initial counts for append/consume buffers.
    /// </summary>
    internal int[] Counts
    {
        get;
    }

    /// <inheritdoc/>
    protected override void OnMapDirtyItem(int index, int rangeIndex, bool isDirty)
    {
        GorgonReadWriteViewBinding value = this[index];

        Native[rangeIndex] = value.ReadWriteView?.Native;
        Counts[rangeIndex] = value.ReadWriteView is null ? 0 : value.InitialCount;
    }

    /// <summary>
    /// Function called when the array is cleared.
    /// </summary>
    protected override void OnClear()
    {
        Array.Clear(Native, 0, Native.Length);
        Array.Clear(Counts, 0, Counts.Length);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonReadWriteViewBindings"/> class.
    /// </summary>
    public GorgonReadWriteViewBindings()
        : base(MaximumReadWriteViewCount)
    {
        Native = new D3D11.UnorderedAccessView[MaximumReadWriteViewCount];
        Counts = new int[MaximumReadWriteViewCount];

        // Force us to clear to the empty views instead of the default for the type.
        Clear();
    }
}
