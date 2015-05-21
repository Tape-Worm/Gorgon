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

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using DX = SharpDX;
using GI = SharpDX.DXGI;
using DXCommon = SharpDX.Direct3D;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A shader view for buffers.
	/// </summary>
	/// <remarks>Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow 
	/// the resource to be cast to any format within the same group.</remarks>
	public class GorgonBufferShaderView
		: GorgonShaderView
    {
        #region Variables.
	    private GorgonBuffer _buffer;                                   // Buffer attached to this view.
	    private GorgonStructuredBuffer _structuredBuffer;               // Structured buffer attached to this view.
	    private GorgonVertexBuffer _vertexBuffer;                       // Vertex buffer attached to this view.
	    private GorgonIndexBuffer _indexBuffer;                         // Index buffer attached to this view.
        #endregion

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


        /// <summary>
        /// Property to return whether this is a raw view or not.
        /// </summary>
        public bool IsRaw
        {
            get;
            private set;
        }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform initialization of the shader view resource.
		/// </summary>
		protected override void OnInitialize()
		{
			GorgonApplication.Log.Print("Creating buffer shader view for {0}.", LoggingLevel.Verbose, Resource.Name);

			var desc = new D3D.ShaderResourceViewDescription
				{
					BufferEx = new D3D.ShaderResourceViewDescription.ExtendedBufferResource
						{
							FirstElement = ElementStart,
							ElementCount = ElementCount,
							Flags = IsRaw ? D3D.ShaderResourceViewExtendedBufferFlags.Raw : D3D.ShaderResourceViewExtendedBufferFlags.None
						},
					Dimension = DXCommon.ShaderResourceViewDimension.ExtendedBuffer,
					Format = (GI.Format)Format
				};

			D3DView = new D3D.ShaderResourceView(Resource.Graphics.D3DDevice, Resource.D3DResource, desc)
				{
					DebugName = "Gorgon Shader View for " + Resource.GetType().FullName
				};
		}

        /// <summary>
        /// Explicit operator to convert this view into its attached generic buffer.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The attached buffer.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the buffer is not a generic buffer.</exception>
	    public static explicit operator GorgonBuffer(GorgonBufferShaderView view)
	    {
            if (view._buffer == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_BUFFER, "generic"));
            }

            return view._buffer;
	    }

        /// <summary>
        /// Explicit operator to convert this view into its attached structured buffer.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The attached buffer.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the buffer is not a structured buffer.</exception>
        public static explicit operator GorgonStructuredBuffer(GorgonBufferShaderView view)
        {
            if (view._structuredBuffer == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_BUFFER, "structured"));
            }

            return view._structuredBuffer;
        }

        /// <summary>
        /// Explicit operator to convert this view into its attached vertex buffer.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The attached buffer.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the buffer is not a vertex buffer.</exception>
        public static explicit operator GorgonVertexBuffer(GorgonBufferShaderView view)
	    {
	        if (view._vertexBuffer == null)
	        {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_BUFFER, "vertex"));
	        }

	        return view._vertexBuffer;
	    }

        /// <summary>
        /// Explicit operator to convert this view into its attached index buffer.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The attached buffer.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the buffer is not a index buffer.</exception>
        public static explicit operator GorgonIndexBuffer(GorgonBufferShaderView view)
	    {
	        if (view._indexBuffer == null)
	        {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_BUFFER, "index"));
	        }

            return view._indexBuffer;
	    }

        /// <summary>
        /// Function to convert this view into its attached generic buffer.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The attached buffer.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the buffer is not a generic buffer.</exception>
        public static GorgonBuffer ToBuffer(GorgonBufferShaderView view)
        {
            if (view._buffer == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_BUFFER, "generic"));
            }

            return view._buffer;
        }

        /// <summary>
        /// Function to convert this view into its attached structured buffer.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The attached buffer.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the buffer is not a structured buffer.</exception>
        public static GorgonStructuredBuffer ToStructuredBuffer(GorgonBufferShaderView view)
        {
            if (view._structuredBuffer == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_BUFFER, "structured"));
            }

            return view._structuredBuffer;
        }

        /// <summary>
        /// Explicit operator to convert this view into its attached vertex buffer.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The attached buffer.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the buffer is not a vertex buffer.</exception>
        public static GorgonVertexBuffer ToVertexBuffer(GorgonBufferShaderView view)
        {
            if (view._vertexBuffer == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_BUFFER, "vertex"));
            }

            return view._vertexBuffer;
        }

        /// <summary>
        /// Explicit operator to convert this view into its attached index buffer.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The attached buffer.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the buffer is not a index buffer.</exception>
        public static GorgonIndexBuffer ToIndexBuffer(GorgonBufferShaderView view)
        {
            if (view._indexBuffer == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_BUFFER, "index"));
            }

            return view._indexBuffer;
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
			: base(buffer, format)
		{
		    IsRaw = isRaw;
			ElementStart = elementStart;
			ElementCount = elementCount;

		    _buffer = buffer as GorgonBuffer;
		    _structuredBuffer = buffer as GorgonStructuredBuffer;
		    _indexBuffer = buffer as GorgonIndexBuffer;
		    _vertexBuffer = buffer as GorgonVertexBuffer;
		}
		#endregion
    }
}