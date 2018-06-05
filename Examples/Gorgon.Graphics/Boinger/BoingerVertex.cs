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
// Created: June 4, 2018 8:04:41 PM
// 
#endregion

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Graphics.Core;

namespace Gorgon.Graphics.Example
{
    /// <summary>
    /// A vertex for our boinger objects.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BoingerVertex
    {
        /// <summary>
        /// Property to return the size of the vertex, in bytes.
        /// </summary>
        public static int Size => Unsafe.SizeOf<BoingerVertex>();

        /// <summary>
        /// Vertex position.
        /// </summary>
        [InputElement(0, "SV_POSITION")]
        public SharpDX.Vector4 Position;
        /// <summary>
        /// Texture coordinate.
        /// </summary>
        [InputElement(1, "TEXCOORD")]
        public SharpDX.Vector2 UV;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoingerVertex" /> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="uv">The texture coordinate.</param>
        public BoingerVertex(SharpDX.Vector3 position, SharpDX.Vector2 uv)
        {
            Position = new SharpDX.Vector4(position, 1.0f);
            UV = uv;
        }
    }
}