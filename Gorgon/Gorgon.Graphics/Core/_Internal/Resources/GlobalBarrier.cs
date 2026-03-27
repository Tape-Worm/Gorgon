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
// Created: March 21, 2026 1:08:33 PM
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The current barrier state used by a resource.
/// </summary>
/// <param name="sync"><inheritdoc cref="Sync" path="/summary"/></param>
/// <param name="access"><inheritdoc cref="Access" path="/summary"/></param>
/// <param name="layout"><inheritdoc cref="Layout" path="/summary"/></param>
internal struct GlobalBarrier(BarrierSync sync, BarrierAccess access, BarrierLayout layout)
{
    /// <summary>
    /// Empty barrier state.
    /// </summary>
    public static readonly GlobalBarrier Empty = new(BarrierSync.None, BarrierAccess.None, BarrierLayout.None);

    /// <summary>
    /// The access state for the resource.
    /// </summary>
    public BarrierAccess Access = access;
    /// <summary>
    /// The synchronization state for the resource.
    /// </summary>
    public BarrierSync Sync = sync;
    /// <summary>
    /// The layout for a texture resource.
    /// </summary>
    public BarrierLayout Layout = layout;
    /// <summary>
    /// The list of texture sub resources affected by the current barrier state.
    /// </summary>
    public List<GorgonSubResourceRange>? SubResources = [];
}
