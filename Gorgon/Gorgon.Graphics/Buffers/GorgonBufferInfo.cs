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
// Created: June 15, 2016 9:39:42 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	/// Provides information on how to set up a buffer.
	/// </summary>
	public class GorgonBufferInfo
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the intended usage flags for this texture.
		/// </summary>
		/// <remarks>
		/// This value is defaulted to <see cref="BufferUsage.Default"/>.
		/// </remarks>
		public BufferUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the type of data to be stored in this buffer.
		/// </summary>
		public BufferType BufferType
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of bytes to allocate for the buffer.
		/// </summary>
		/// <remarks>
		/// <note type="important">
		/// <para>
		/// <para>
		/// If the buffer is intended to be used as a <see cref="Graphics.BufferType.Constant"/> buffer, then this size should be a multiple of 16. Constant buffer alignment rules require that they be sized to the nearest 16 bytes.
		/// </para>
		/// <para>
		/// If the buffer is not sized to a multiple of 16, Gorgon will attempt to adjust the size to fit the alignment requirement.
		/// </para>
		/// </para>
		/// </note>
		/// </remarks>
		public int SizeInBytes
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBufferInfo"/> class.
		/// </summary>
		public GorgonBufferInfo()
		{
			Usage = BufferUsage.Default;
		}
		#endregion
	}
}
