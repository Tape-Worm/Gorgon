#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Tuesday, November 14, 2006 10:55:05 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Internal
{    
    /// <summary>
    /// Enumeration for drawing modes.
    /// </summary>
    public enum DrawingMode
    {
        /// <summary>Solid polygons.</summary>
        Solid = 0,
        /// <summary>Wireframe polygons.</summary>
        Wireframe = 1,
        /// <summary>Vertex points.</summary>
        Points = 2
    }

    /// <summary>
    /// Enumeration for shading modes.
    /// </summary>
    public enum ShadingMode
    {
        /// <summary>Flat shading.</summary>
        Flat = 0,
        /// <summary>Gouraud shading.</summary>
        Gouraud = 1
    }
    
    /// <summary>
    /// Enumeration for buffer usage types.
    /// </summary>
    [Flags]
    public enum BufferUsages
    {
        /// <summary>
        /// Static buffers.  A static buffer should not be modified very often or at all
        /// if possible.  These buffers are typically stored in video RAM.
        /// </summary>
        Static = 1,
        /// <summary>
        /// Make a dynamic buffer, good for when there's going to be frequent changes
        /// to the buffer.
        /// </summary>
        Dynamic = 2,
        /// <summary>
        /// An optimization to make the buffer write only, reading from a buffer
        /// with writeonly set will result in failure.
        /// </summary>
        WriteOnly = 4,
        /// <summary>
        /// Force this buffer into standard memory as opposed to video RAM.
        /// </summary>
        ForceSoftware = 8,
        /// <summary>
        /// Put this buffer into system memory.  This buffer will be inaccessable to 
        /// Direct3D and can only be used as storage.
        /// </summary>
        UseSystemMemory = 16
    }

    /// <summary>
    /// Flags used for buffer locking.
    /// </summary>
    public enum BufferLockFlags
    {
        /// <summary>
        /// No locking flags, used with static buffers.
        /// </summary>
        Normal = 0,
        /// <summary>
        /// This flag will guarantee that we will not overwrite any data in
        /// the buffer and allow us to continue rendering.
        /// </summary>
        NoOverwrite = 1,
        /// <summary>
        /// This flag will discard the contents of the buffer and returns a
        /// new pointer to a memory area to avoid stalls.  Only to be used
        /// on dynamic buffers.
        /// </summary>
        Discard = 2,
        /// <summary>
        /// This flag will make the buffer read only, and give certain optimizations.
        /// </summary>
        ReadOnly = 3
    }

    /// <summary>
    /// Flags for index buffer types.
    /// </summary>
    public enum IndexBufferType
    {
        /// <summary>
        /// Use 16 bit indices.
        /// </summary>
        Index16 = 0,
        /// <summary>
        /// Use 32 bit indices.
        /// </summary>
        Index32 = 1
    }

    /// <summary>
    /// Enumeration containing the types of buffers.
    /// </summary>
    public enum BufferTypes
    {
        /// <summary>
        /// Vertex buffer.
        /// </summary>
        Vertex = 0,
        /// <summary>
        /// Index buffer.
        /// </summary>
        Index = 1,
        /// <summary>
        /// Image buffer.
        /// </summary>
        Image = 2
    }
}
