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
// Created: August 9, 2018 9:32:15 PM
// 
#endregion

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Graphics;
using DX = SharpDX;


namespace Gorgon.Renderers
{
    /// <summary>
    /// Provides vertex shader data for polygon sprites.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct PolyVertexShaderData
    {
        /// <summary>
        /// The size of the data structure, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Unsafe.SizeOf<PolyVertexShaderData>();

        /// <summary>
        /// World matrix.
        /// </summary>
        public DX.Matrix World;
        /// <summary>
        /// Color information.
        /// </summary>
        public GorgonColor Color;
        /// <summary>
        /// Texture transformation data.
        /// </summary>
        public DX.Vector4 TextureTransform;
        /// <summary>
        /// Flags used for flipping the texture and rotation values.
        /// </summary>
        public DX.Vector4 MiscInfo;
        /// <summary>
        /// The texture array index to use.
        /// </summary>
        public float TextureArrayIndex;
    }
}
