#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
    /// Enumerator for vertex field contexts.
    /// Used to define in which context the field will be used.
    /// </summary>
    public enum VertexFieldContext
    {
        /// Position, 3 reals per vertex.
        Position,
        /// Normal, 3 reals per vertex.
        Normal,
        /// Blending weights.
        BlendWeights,
        /// Blending indices.
        BlendIndices,
        /// Diffuse colors.
        Diffuse,
        /// Specular colors.
        Specular,
        /// Texture coordinates.
        TexCoords,
        /// Binormal (Y axis if normal is Z).
        Binormal,
        /// Tangent (X axis if normal is Z).
        Tangent
    }


    /// <summary>
    /// Enumerator for vertex field types.
    /// Used to define what type of field we're using.
    /// </summary>
    public enum VertexFieldType
    {
        /// 1 Floating point number.
        Float1,
        /// 2 Floating point numbers.
        Float2,
        /// 3 Floating point numbers.
        Float3,
        /// 4 Floating point numbers.
        Float4,
        /// DWORD color value.
        Color,
        /// 1 signed short integers.
        Short1,
        /// 2 signed short integers.
        Short2,
        /// 3 signed short integers.
        Short3,
        /// 4 signed short integers.
        Short4,
        /// 4 Unsigned bytes.
        UByte4
    }
    
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
    /// Enumeration for primitive drawing style.
    /// </summary>
    public enum PrimitiveStyle
    {
        /// <summary>A series of individual points.</summary>
        PointList = 0,
        /// <summary>A series of individual lines.</summary>
        LineList = 1,
        /// <summary>A series of lines connected in a strip.</summary>
        LineStrip = 2,
        /// <summary>A series of individual triangles.</summary>
        TriangleList = 3,
        /// <summary>A series of triangles connected in a strip.</summary>
        TriangleStrip = 4,
        /// <summary>A series of triangles connected in a fan.</summary>
        TriangleFan = 5
    }

    /// <summary>
    /// Enumeration for culling modes.
    /// </summary>
    public enum CullingMode
    {
        /// <summary>Cull counter clockwise.</summary>
        CounterClockwise = 0,
        /// <summary>Cull clockwise.</summary>
        Clockwise = 1,
        /// <summary>No culling.</summary>
        None = 2,
    }

    /// <summary>
    /// Enumeration for buffer usage types.
    /// </summary>
    [Flags]
    public enum BufferUsage
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
