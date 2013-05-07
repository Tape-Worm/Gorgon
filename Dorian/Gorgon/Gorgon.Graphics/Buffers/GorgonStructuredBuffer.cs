#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Monday, November 5, 2012 8:58:47 PM
// 
#endregion

using System.Linq;
using GorgonLibrary.IO;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The type of structured buffer.
	/// </summary>
	public enum StructuredBufferType
	{
		/// <summary>
		/// A standard structured buffer.
		/// </summary>
		Standard = 0,
		/// <summary>
		/// An append/consume buffer.
		/// </summary>
		AppendConsume = 1,
		/// <summary>
		/// A counter buffer.
		/// </summary>
		Counter = 2
	}

	/// <summary>
	/// A structured buffer for shaders.
	/// </summary>
	/// <remarks>Structured buffers are similar to <see cref="GorgonLibrary.Graphics.GorgonConstantBuffer">constant buffers</see> in that they're used to convey data to 
	/// a shader.  However, unlike constant buffers, these buffers allow for unordered access and are meant as containers for structured data (hence, structured buffers).
	/// <para>Structured buffers are only available to SM5 and above.</para>
	/// </remarks>
	public class GorgonStructuredBuffer
		: GorgonShaderBuffer
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of structured buffer.
		/// </summary>
		public StructuredBufferType StructuredBufferType
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of elements that can be stored in this buffer.
		/// </summary>
		public int ElementCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the size, in bytes, of an element.
		/// </summary>
		public int ElementSize
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a default resource view object.
		/// </summary>
		protected override void CreateDefaultResourceView()
		{
			if (BufferUsage != GorgonLibrary.Graphics.BufferUsage.Staging)
			{
				DefaultView = new GorgonResourceView(Graphics, "Gorgon Structured Buffer #" + Graphics.GetGraphicsObjectOfType<GorgonStructuredBuffer>().Count().ToString());
				DefaultView.Resource = this;
				DefaultView.BuildResourceView();
				Graphics.Shaders.Reseat(this);
			}
		}

		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
			if (D3DResource != null)
			{
				GorgonRenderStatistics.StructuredBufferCount--;
				GorgonRenderStatistics.StructuredBufferSize -= D3DBuffer.Description.SizeInBytes;
			}

			base.CleanUpResource();
		}

		/// <summary>
		/// Function to initialize the buffer.
		/// </summary>
		/// <param name="value">Value used to initialize the buffer.</param>
		protected override void InitializeImpl(GorgonDataStream value)
		{
			if (D3DResource != null)
			{
				D3DResource.Dispose();
				D3DResource = null;
			}

			var desc = new D3D.BufferDescription()
			{
				BindFlags = BindingFlags,
				CpuAccessFlags = D3DCPUAccessFlags,
				OptionFlags = D3D.ResourceOptionFlags.BufferStructured,
				SizeInBytes = SizeInBytes,
				StructureByteStride = ElementSize,
				Usage = D3DUsage
			};

			if (value != null)
			{
				long position = value.Position;

				using (var dxStream = new DX.DataStream(value.BasePointer, value.Length - position, true, true))
					D3DResource = new D3D.Buffer(Graphics.D3DDevice, dxStream, desc);
			}
			else
				D3DResource = new D3D.Buffer(Graphics.D3DDevice, desc);

			GorgonRenderStatistics.StructuredBufferCount++;
			GorgonRenderStatistics.StructuredBufferSize += ((D3D.Buffer)D3DResource).Description.SizeInBytes;

#if DEBUG
			D3DResource.DebugName = "Gorgon Structured Buffer #" + Graphics.GetGraphicsObjectOfType<GorgonStructuredBuffer>().Count().ToString();
#endif
			CreateDefaultResourceView();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonStructuredBuffer" /> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="elementCount">Number of elements that can be contained within this buffer.</param>
		/// <param name="elementSize">Size of an element, in bytes.</param>
		/// <param name="isUnorderedAccess">TRUE to allow unordered access to the buffer, FALSE to only allow ordered.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU write access to the buffer, FALSE to disallow.</param>
		internal GorgonStructuredBuffer(GorgonGraphics graphics, int elementCount, int elementSize, bool isUnorderedAccess, bool allowCPUWrite)
			: base(graphics, allowCPUWrite, isUnorderedAccess, elementCount * elementSize)
		{
			ElementCount = elementCount;
			ElementSize = elementSize;
			StructuredBufferType = StructuredBufferType.Standard;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonStructuredBuffer" /> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="elementCount">Number of elements that can be contained within this buffer.</param>
		/// <param name="elementSize">Size of an element, in bytes.</param>
		/// <param name="bufferType">Type of structured buffer.</param>
		internal GorgonStructuredBuffer(GorgonGraphics graphics, int elementCount, int elementSize, StructuredBufferType bufferType)
			: this(graphics, elementCount, elementSize, true, false)
		{
			StructuredBufferType = bufferType;
		}
		#endregion
	}
}
