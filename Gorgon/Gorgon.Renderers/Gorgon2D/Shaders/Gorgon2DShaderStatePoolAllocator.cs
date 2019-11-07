#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: September 23, 2019 4:33:14 PM
// 
#endregion

using Gorgon.Graphics.Core;
using Gorgon.Memory;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An allocator used to retrieve 2D shader states from a pool.
    /// </summary>    
    public class Gorgon2DShaderStatePoolAllocator<T>
        : GorgonRingPool<Gorgon2DShaderState<T>>
        where T : GorgonShader
    {
        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="Gorgon2DShaderStatePoolAllocator{T}"/> class.</summary>
        /// <param name="objectCount">[Optional] The number of objects to initialize the pool with.</param>
        public Gorgon2DShaderStatePoolAllocator(int objectCount = 128)
            : base(objectCount) => ItemAllocator = () => new Gorgon2DShaderState<T>();        
        #endregion        
    }
}
