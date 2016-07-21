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
using Gorgon.Diagnostics;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Buffer usage types.
	/// </summary>
	public enum BufferUsage
	{
		/// <summary>
		/// Allows read/write access to the buffer from the GPU.
		/// </summary>
		Default = D3D11.ResourceUsage.Default,
		/// <summary>
		/// Can only be read by the GPU, cannot be written to or read from by the CPU, and cannot be written to by the GPU.
		/// </summary>
		/// <remarks>Pre-initialize any buffer created with this usage, or else you will not be able to after it's been created.</remarks>
		Immutable = D3D11.ResourceUsage.Immutable,
		/// <summary>
		/// Allows read access by the GPU and write access by the CPU.
		/// </summary>
		Dynamic = D3D11.ResourceUsage.Dynamic,
		/// <summary>
		/// Allows reading/writing by the CPU and can be copied to a GPU compatible buffer (but not used directly by the GPU).
		/// </summary>
		Staging = D3D11.ResourceUsage.Staging
	}

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
	public abstract class GorgonBuffer
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
		/// Property to return the graphics interface used to create and manipulate the buffer.
		/// </summary>
		protected GorgonGraphics Graphics
		{
			get;
			private set;
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
		/// Initializes a new instance of the <see cref="GorgonBuffer" /> class.
		/// </summary>
		/// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
		/// <param name="name">Name of this buffer.</param>
		/// <param name="log">[Optional] The log interface used for debug logging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="name"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		protected GorgonBuffer(GorgonGraphics graphics, string name, IGorgonLog log)
			: base(graphics, name)
		{
			Graphics = graphics;
			Log = log ?? GorgonLogDummy.DefaultInstance;
		}
		#endregion
	}
}
