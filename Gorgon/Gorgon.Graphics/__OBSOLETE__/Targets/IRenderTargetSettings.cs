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
// Created: Wednesday, October 12, 2011 6:57:29 PM
// 
#endregion

namespace Gorgon.Graphics
{
    /// <summary>
    /// Type of render target.
    /// </summary>
    public enum RenderTargetType
    {
        /// <summary>
        /// A render target buffer.
        /// </summary>
        Buffer = 0,
        /// <summary>
        /// A 1D render target.
        /// </summary>
        Target1D = 1,
        /// <summary>
        /// A 2D render target.
        /// </summary>
        Target2D = 2,
        /// <summary>
        /// A 3D render target.
        /// </summary>
        Target3D = 3
    }

	/// <summary>
	/// Render target texture settings.
	/// </summary>
	public interface IRenderTargetTextureSettings
		: ITextureSettings
	{
		/// <summary>
		/// Property to return the type of render target.
		/// </summary>
		RenderTargetType RenderTargetType
		{
			get;
		}

		/// <summary>
		/// Property to set or return the format of the backing texture for the render target.
		/// </summary>
		/// <remarks>If this value is Unknown, then it will use the format from <see cref="Gorgon.Graphics.IImageSettings.Format">Format</see>.
		/// <para>If both the Format and this parameter is Unknown, an exception will be raised.</para>
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		BufferFormat TextureFormat
		{
			get;
			set;
		}


		/// <summary>
		/// Property to set or return the depth and/or stencil buffer format.
		/// </summary>
		/// <remarks>Setting this value to Unknown will create the target without a depth buffer.
		/// <para>This value is only valid for 1D and 2D render targets.  3D render targets can't have a depth buffer.</para>
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		BufferFormat DepthStencilFormat
		{
			get;
			set;
		}
	}
}
