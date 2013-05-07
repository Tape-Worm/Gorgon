#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Tuesday, November 22, 2011 11:37:14 AM
// 
#endregion

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for a depth/stencil buffer.
	/// </summary>
	public class GorgonDepthStencilSettings
	{
		#region Methods.
		/// <summary>
		/// Property to set or return the format for the depth/stencil buffer.
		/// </summary>
		public BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the multi sampling settings for the depth/stencil buffer.
		/// </summary>
		/// <remarks>Ensure that these settings match the render target that the depth buffer will be paired with.</remarks>
		public GorgonMultisampling MultiSample
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the width of the depth/stencil buffer.
		/// </summary>
		/// <remarks>Ensure that this setting will match the render target that the depth buffer will be paired with.</remarks>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height of the depth/stencil buffer.
		/// </summary>
		/// <remarks>Ensure that this setting will match the render target that the depth buffer will be paired with.</remarks>
		public int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format for the texture used by the depth/stencil buffer.
		/// </summary>
		/// <remarks>This is typically used for accessing the depth buffer through a shader.  The shader will require a typeless format, of which there is none for a depth buffer.  If this
		/// value is set to Unknown, then this property will be ignored when creating/updating the depth/stencil buffer and will make the buffer inaccessible to a shader.
		/// <para>The default value is Unknown.</para>
		/// <para>Note that this only applies to the SM_4_1 and higher feature levels, other feature levels will ignore this setting.</para>
		/// </remarks>
		public BufferFormat TextureFormat
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencilSettings"/> class.
		/// </summary>
		public GorgonDepthStencilSettings()
		{
			TextureFormat = BufferFormat.Unknown;
		}
		#endregion
	}
}
