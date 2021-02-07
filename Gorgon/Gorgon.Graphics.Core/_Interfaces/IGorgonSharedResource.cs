#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: February 11, 2021 11:46:04 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Graphics
{
    /// <summary>
    /// Provides the ability return a shared resource for passing objects to other APIs.
    /// </summary>
    public interface IGorgonSharedResource
        : IGorgonNativeResource
    {
        /// <summary>
        /// Function to retrieve the shared resource handle for this resource.
        /// </summary>
        /// <returns>A pointer representing a handle for sharing the resource data with other interfaces.</returns>
        /// <remarks>
        /// <para>
        /// This is used to retrieve a handle to the shared resource that allows applications to share the resource with other APIs (e.g. Direct 2D). 
        /// </para>
        /// </remarks>        
        IntPtr GetSharedHandle();
    }
}
