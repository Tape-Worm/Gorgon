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
// Created: Saturday, June 8, 2013 4:14:32 PM
// 
#endregion

namespace Gorgon.Graphics
{
	/// <summary>
	/// Interface for all depth/stencil settings.
	/// </summary>
	public interface IDepthStencilSettings
		: ITextureSettings
	{
		/// <summary>
		/// Property to set or return the texture format for the depth/stencil buffer texture.
		/// </summary>
		/// <remarks>
		/// If the <see cref="AllowShaderView"/> setting is set to <b>true</b>, then this value needs to be set to a typeless format.  This is because 
		/// a depth/stencil buffer is not capable of having a format that is not one of the depth formats.
		/// <para>If this value is set to Unknown, then an exception will be thrown when trying to create the depth/stencil buffer.</para>
		/// <para>This value is only applicable on video devices that have a feature level of SM4 or better if no multisampling is present on the depth/stencil buffer.  
		/// If the depth/stencil buffer is multisampled then a video device with a feature level of SM4_1 or better is required.</para>
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		BufferFormat TextureFormat
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow shader views on the depth/stencil buffer.
		/// </summary>
		/// <remarks>
		/// Set this value to <b>true</b> if depth buffer needs to be bound to the shader.
		/// <para>This value is only applicable on video devices that have a feature level of SM4 or better.</para>
		/// <para>The default value is <b>false</b>.</para>
		/// </remarks>
		bool AllowShaderView
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the flags used for the default depth/stencil view.
        /// </summary>
        /// <remarks>Use this to determine how the default depth/stencil view is bound to the pipeline.
        /// <para>A value other than None requires a video device with a feature level of SM5 or better.</para>
        /// <para>The default value is None.</para>
        /// </remarks>
        DepthStencilViewFlags DefaultDepthStencilViewFlags
        {
            set;
            get;
        }
	}
}