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
// Created: March 21, 2026 1:08:33 PM
//

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The current barrier state used by a resource.
/// </summary>
/// <param name="queueType"><inheritdoc cref="QueueType" path="/summary"/></param>
internal struct GlobalBarrier(D3D12_COMMAND_LIST_TYPE queueType)
{
    /// <summary>
    /// Default barrier state.
    /// </summary>
    public static readonly GlobalBarrier Default = new(D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT);

    /// <summary>
    /// The list of barriers for a global barrier.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Buffers will only utilize the sync and access parts of the tuple, and will only ever have 1 entry in the list.
    /// </para>
    /// </remarks>
    public List<(BarrierSync Sync, BarrierAccess Access, BarrierLayout Layout, GorgonSubResourceRange Range)>? Barriers = [];

    /// <summary>
    /// The type of queue this resource belongs to.
    /// </summary>
    public D3D12_COMMAND_LIST_TYPE QueueType = queueType;
}
