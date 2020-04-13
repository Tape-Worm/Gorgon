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
// Created: June 6, 2018 2:34:12 PM
// 
#endregion

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A vertex for a renderable object.
    /// </summary>		
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Gorgon2DVertex
    {
        /// <summary>
        /// The size of the vertex, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Unsafe.SizeOf<Gorgon2DVertex>();

        /// <summary>
        /// Position of the vertex.
        /// </summary>
        [InputElement(0, "SV_POSITION")]
        public Vector4 Position;
        /// <summary>
        /// Color of the vertex.
        /// </summary>
        [InputElement(1, "COLOR")]
        public GorgonColor Color;
        /// <summary>
        /// Texture coordinates for the vertex.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This texture coordinate member includes the <c>Z</c> component of the vector. This is used when passing an index into a texture array.
        /// </para>
        /// </remarks>
        [InputElement(2, "TEXCOORD")]
        public Vector4 UV;
        /// <summary>
        /// The Cosine and Sine for the angle of rotation.
        /// </summary>
        [InputElement(3, "ANGLE")]
        public Vector2 Angle;
    }
}
