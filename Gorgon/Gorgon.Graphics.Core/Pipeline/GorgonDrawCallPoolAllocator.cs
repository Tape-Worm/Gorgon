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
// Created: May 31, 2018 1:37:08 PM
// 
#endregion

using System;
using Gorgon.Memory;
using Gorgon.Reflection;

namespace Gorgon.Graphics.Core;

/// <summary>
/// An allocator used to retrieve draw calls from a pool.
/// </summary>
/// <typeparam name="T">The type of draw call.</typeparam>
public class GorgonDrawCallPoolAllocator<T>
    : GorgonRingPool<T>
    where T : GorgonDrawCallCommon
{
    #region Variables.
    // The object creator.
    private static readonly Lazy<ObjectActivator<T>> _creator;
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonLinearPool{T}"/> class.
    /// </summary>
    /// <param name="objectCount">The number of total objects available to the allocator.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="objectCount"/> parameter is less than 1.</exception>
    public GorgonDrawCallPoolAllocator(int objectCount)
        : base(objectCount, () => _creator.Value())
    {
    }

    /// <summary>
    /// Initializes static members of the <see cref="GorgonDrawCallPoolAllocator{T}"/> class.
    /// </summary>
    static GorgonDrawCallPoolAllocator() => _creator = new Lazy<ObjectActivator<T>>(() => typeof(T).CreateActivator<T>(), true);
    #endregion

}
