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
// Created: Thursday, August 16, 2012 9:57:32 PM
// 
#endregion

namespace Gorgon.Graphics
{
	/// <summary>
	/// Rendering statistics.
	/// </summary>
	/// <remarks>Returns various bits of information about rendering.</remarks>
	public static class GorgonRenderStatistics
	{
		#region Properties.
		/// <summary>
		/// Property to return the number of draw calls.
		/// </summary>
		public static ulong DrawCallCount
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the number of vertex buffers allocated.
		/// </summary>
		public static int VertexBufferCount
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the size, in bytes, of all the vertex buffers allocated.
		/// </summary>
		public static int VertexBufferSize
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the number of index buffers allocated.
		/// </summary>
		public static int IndexBufferCount
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the size, in bytes, of all the index buffers allocated.
		/// </summary>
		public static int IndexBufferSize
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the number of constant buffers allocated.
		/// </summary>
		public static int ConstantBufferCount
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the size, in bytes, of all the constant buffers allocated.
		/// </summary>
		public static int ConstantBufferSize
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the number of generic buffers allocated.
		/// </summary>
		public static int BufferCount
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the number of structured buffers allocated.
		/// </summary>
		public static int StructuredBufferCount
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the number of typed buffers allocated.
		/// </summary>
		public static int TypedBufferCount
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the number of raw buffers allocated.
		/// </summary>
		public static int RawBufferCount
		{
			get;
			internal set;
		}

        /// <summary>
        /// Property to return the number of indirect argument buffers allocated.
        /// </summary>
        public static int IndirectBufferCount
        {
            get;
            internal set;
        }

		/// <summary>
		/// Property to return the size, in bytes, of all generic buffers allocated.
		/// </summary>
		public static int BufferSize
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the size, in bytes, of all structured buffers allocated.
		/// </summary>
		public static int StructuredBufferSize
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the size, in bytes, of all typed buffers allocated.
		/// </summary>
		public static int TypedBufferSize
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the size, in bytes, of all typed buffers allocated.
		/// </summary>
		public static int RawBufferSize
		{
			get;
			internal set;
		}

        /// <summary>
        /// Property to return the size, in bytes, of all indirect argument buffers allocated.
        /// </summary>
        public static int IndirectBufferSize
        {
            get;
            internal set;
        }

		/// <summary>
		/// Property to return the number of textures allocated.
		/// </summary>
		public static int TextureCount
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the size, in bytes, of all the textures allocated.
		/// </summary>
		public static int TextureSize
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the number of render targets allocated.
		/// </summary>
		public static int RenderTargetCount
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the size, in bytes, of all the render targets allocated.
		/// </summary>
		public static int RenderTargetSize
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the number of depth buffers allocated.
		/// </summary>
		public static int DepthBufferCount
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the size, in bytes, of all the depth buffers allocated.
		/// </summary>
		public static int DepthBufferSize
		{
			get;
			internal set;
		}

        /// <summary>
        /// Property to return the number of input layouts that have been created.
        /// </summary>
        public static int InputLayoutCount
        {
            get;
            internal set;
        }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to reset various frame-related pieces of data.
		/// </summary>
		public static void EndFrame()
		{
			DrawCallCount = 0;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonRenderStatistics"/> class.
		/// </summary>
		static GorgonRenderStatistics()
		{
			DrawCallCount = 0;
		    InputLayoutCount = 0;
			VertexBufferCount = 0;
			VertexBufferSize = 0;
			IndexBufferSize = 0;
			IndexBufferCount = 0;
			ConstantBufferCount = 0;
			ConstantBufferSize = 0;
			TextureCount = 0;
			TextureSize = 0;
			RenderTargetSize = 0;
			RenderTargetCount = 0;
			DepthBufferCount = 0;
			DepthBufferSize = 0;
			BufferCount = 0;
			StructuredBufferCount = 0;
			TypedBufferCount = 0;
			BufferSize = 0;
			StructuredBufferSize = 0;
			TypedBufferSize = 0;
			RawBufferCount = 0;
			RawBufferSize = 0;
		    IndirectBufferCount = 0;
		    IndirectBufferSize = 0;
		}
		#endregion
	}
}
