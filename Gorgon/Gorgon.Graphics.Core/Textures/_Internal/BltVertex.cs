﻿#region MIT
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
// Created: March 6, 2017 10:58:50 PM
// 
#endregion

using System.Runtime.InteropServices;
using Gorgon.Graphics.Core;
using Gorgon.Native;
using DX = SharpDX;

namespace Gorgon.Graphics
{
	/// <summary>
	/// The vertex of the blitter used to blit textures to the current render target.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct BltVertex
	{
		/// <summary>
		/// The size of the vertex, in bytes.
		/// </summary>
		public static readonly int Size = DirectAccess.SizeOf<BltVertex>();

		/// <summary>
		/// The position of the vertex.
		/// </summary>
		[InputElement(0, "SV_POSITION")]
		public DX.Vector4 Position;

		/// <summary>
		/// The texture coordinate for the vertex.
		/// </summary>
		[InputElement(1, "TEXCOORD")]
		public DX.Vector2 Uv;

        /// <summary>
        /// The color of the vertex.
        /// </summary>
        [InputElement(2, "COLOR")]
	    public GorgonColor Color;
	}
}
