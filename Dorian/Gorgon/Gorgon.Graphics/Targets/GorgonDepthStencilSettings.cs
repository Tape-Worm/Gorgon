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
		#region Properties.
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
        /// Property to set or return whether to allow shader views on the depth/stencil buffer.
        /// </summary>
        /// <remarks>
        /// Set this value to TRUE if depth buffer needs to be bound to the shader.
        /// <para>This value is only applicable on video devices that have a feature level of SM4_1 or better.</para>
        /// <para>The default value is FALSE.</para>
        /// </remarks>
        public bool AllowShaderView
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the texture format for the depth/stencil buffer texture.
        /// </summary>
        /// <remarks>
        /// If the <see cref="AllowShaderView"/> setting is set to TRUE, then this value needs to be set to a typeless format.  This is because 
        /// a depth/stencil buffer is not capable of having a format that is not one of the depth formats.
        /// <para>If this value is set to Unknown, then an exception will be thrown when trying to create the depth/stencil buffer.</para>
        /// <para>This value is only applicable on video devices that have a feature level of SM4 or better if no multisampling is present on the depth/stencil buffer.  
        /// If the depth/stencil buffer is multisampled then a video device with a feature level of SM4_1 or better is required.</para>
        /// <para>The default value is Unknown.</para>
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
		    AllowShaderView = false;
			MultiSample = new GorgonMultisampling(1, 0);
		}
		#endregion
	}
}
