#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Wednesday, May 29, 2013 11:07:59 PM
// 
#endregion

using DX = SharpDX;
using GI = SharpDX.DXGI;
using DXCommon = SharpDX.Direct3D;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A shader view for buffers.
	/// </summary>
	/// <remarks>Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow 
	/// the resource to be cast to any format within the same group.</remarks>
	public class GorgonBufferShaderView
		: GorgonShaderView
	{
		#region Properties.
		/// <summary>
		/// Property to return the offset of the view from the first element in the buffer.
		/// </summary>
		public int ElementStart
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of elements that the view will encompass.
		/// </summary>
		public int ElementCount
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform initialization of the shader view resource.
		/// </summary>
		protected override void InitializeImpl()
		{
			var desc = new SharpDX.Direct3D11.ShaderResourceViewDescription
				{
					BufferEx = new SharpDX.Direct3D11.ShaderResourceViewDescription.ExtendedBufferResource
						{
							FirstElement = ElementStart,
							ElementCount = ElementCount,
							Flags = IsRaw ? D3D.ShaderResourceViewExtendedBufferFlags.Raw : D3D.ShaderResourceViewExtendedBufferFlags.None
						},
					Dimension = DXCommon.ShaderResourceViewDimension.ExtendedBuffer,
					Format = (GI.Format)Format
				};

			D3DView = new SharpDX.Direct3D11.ShaderResourceView(Resource.Graphics.D3DDevice, Resource.D3DResource, desc)
				{
					DebugName = "Gorgon Shader View for " + Resource.GetType().FullName
				};
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBufferShaderView"/> class.
		/// </summary>
		/// <param name="buffer">The buffer to bind to the view.</param>
		/// <param name="format">Format of the view.</param>
		/// <param name="elementStart">The starting element for the view.</param>
		/// <param name="elementCount">The number of elements in the view.</param>
		/// <param name="isRaw">TRUE to use a raw view, FALSE to use a normal view.</param>
		internal GorgonBufferShaderView(GorgonResource buffer, BufferFormat format, int elementStart, int elementCount, bool isRaw)
			: base(buffer, format, isRaw)
		{
			ElementStart = elementStart;
			ElementCount = elementCount;
		}
		#endregion
    }
}