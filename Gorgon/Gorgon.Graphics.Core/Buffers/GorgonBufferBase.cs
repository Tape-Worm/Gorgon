#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 9, 2016 3:54:15 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// The type of data to be stored in the buffer.
	/// </summary>
	public enum BufferType
	{
		/// <summary>
		/// A generic raw buffer filled with byte data.
		/// </summary>
		Generic = 0,
		/// <summary>
		/// A constant buffer used to send data to a shader.
		/// </summary>
		Constant = 1,
		/// <summary>
		/// A vertex buffer used to hold vertex information.
		/// </summary>
		Vertex = 2,
		/// <summary>
		/// An index buffer used to hold index information.
		/// </summary>
		Index = 3,
		/// <summary>
		/// A structured buffer used to hold structured data.
		/// </summary>
		Structured = 4
	}

	/// <summary>
	/// A base class for buffers.
	/// </summary>
	public abstract class GorgonBufferBase
		: GorgonResource
	{
		#region Properties.
		/// <summary>
		/// Property to return the log used for debugging.
		/// </summary>
		protected IGorgonLog Log
		{
			get;
		}

		/// <summary>
		/// Property to return the D3D 11 buffer.
		/// </summary>
		protected internal D3D11.Buffer D3DBuffer
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the type of data in the resource.
		/// </summary>
		public override ResourceType ResourceType => ResourceType.Buffer;

		/// <summary>
		/// Property to return the type of buffer.
		/// </summary>
		public abstract BufferType BufferType
		{
			get;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBufferBase" /> class.
		/// </summary>
		/// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
		/// <param name="name">Name of this buffer.</param>
		/// <param name="log">[Optional] The log interface used for debug logging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="name"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		protected GorgonBufferBase(GorgonGraphics graphics, string name, IGorgonLog log)
			: base(graphics, name)
		{
			Log = log ?? GorgonLogDummy.DefaultInstance;
		}
		#endregion
	}
}
