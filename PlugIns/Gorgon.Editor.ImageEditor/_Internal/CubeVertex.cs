#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: March 4, 2017 10:29:35 AM
// 
#endregion

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Graphics.Core;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// This represents a single vertex in our cube.
    /// 
    /// It will contain a position, and a texture coordinate. We have to specify the packing and the layout ordering so we can safely transfer the data from the managed 
    /// environment of .NET into the unmanaged world of Direct 3D.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct CubeVertex
    {
        /// <summary>
        /// This is the size of the vertex, in bytes. 
        /// 
        /// We'll need this to tell Direct 3D how many vertices are stored in a vertex buffer.
        /// </summary>
        public static int SizeInBytes = Unsafe.SizeOf<CubeVertex>();

        /// <summary>
        /// This will be the position of the vertex in object space.
        /// 
        /// Note that the member is decorated with an InputElement attribute. This tells the shader how to interpret the vertex data by the semantic provided in the string, and the order of the data
        /// as indicated by the integer parameter.
        /// </summary>
        [InputElement(0, "SV_POSITION")]
        public Vector4 Position;

        /// <summary>
        /// This will be the texture coordinate for the vertex.
        /// 
        /// This specifies the location on the texture that will be sampled by the pixel shader when rendering. This value is in texel space coordinates where 0, 0 is the upper-left of the 
        /// texture, and 1, 1 is the lower-right of the texture.
        /// </summary>
        [InputElement(1, "TEXCOORD")]
        public Vector3 UVW;

        /// <summary>
        /// Initializes a new instance of the <see cref="CubeVertex"/> struct.
        /// </summary>
        /// <param name="position">The position of the vertex in object space.</param>
        /// <param name="uv">The texture coordinate for this vertex.</param>
        public CubeVertex(Vector3 position, Vector3 uvw)
        {
            // Note that we're passing a 3D vector, but storing a 4D vector. We need the W coordinate set to 1.0f to indicate that the coordinates are normalized.
            // For more information about the W component, go to http://www.tomdalling.com/blog/modern-opengl/explaining-homogenous-coordinates-and-projective-geometry/
            Position = new Vector4(position, 1.0f);
            UVW = uvw;
        }
    }

}
