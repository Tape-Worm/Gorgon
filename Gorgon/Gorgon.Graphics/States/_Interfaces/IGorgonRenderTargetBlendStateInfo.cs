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
// Created: July 30, 2016 12:41:48 PM
// 
#endregion

using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Information for independent blend states across multiple render targets.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This provides an immutable view of the stencil operation state information so that it cannot be modified after the state is created.
	/// </para>
	/// </remarks>
	public interface IGorgonRenderTargetBlendStateInfo
	{
		/// <summary>
		/// Property to return whether blending should be enabled for this render target.
		/// </summary>
		/// <remarks>
		/// The default value is <b>false</b>.
		/// </remarks>
		bool IsBlendingEnabled
		{
			get;
		}

		/// <summary>
		/// Property to return whether the logical operation for this blend state is enabled or not.
		/// </summary>
		/// <remarks>
		/// The default value is <b>false</b>.
		/// </remarks>
		bool IsLogicalOperationEnabled
		{
			get;
		}

		/// <summary>
		/// Property to return the blending operation to perform.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value specifies the type how to combine the <see cref="SourceColorBlend"/> and <see cref="DestinationColorBlend"/> operation results.
		/// </para>
		/// <para>
		/// The default value is <c>Add</c>.
		/// </para>
		/// </remarks>
		D3D11.BlendOperation ColorBlendOperation
		{
			get;
		}

		/// <summary>
		/// Property to return the blending operation to perform.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value specifies the type how to combine the <see cref="SourceAlphaBlend"/> and <see cref="DestinationAlphaBlend"/> operation results.
		/// </para>
		/// <para>
		/// The default value is <c>Add</c>.
		/// </para>
		/// </remarks>
		D3D11.BlendOperation AlphaBlendOperation
		{
			get;
		}

		/// <summary>
		/// Property to return the source blending operation.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This defines the type of operation to apply to the color (RGB) components of a pixel being blended from the source pixel data. 
		/// </para> 
		/// <para>
		/// The default value is <c>One</c>.
		/// </para>
		/// </remarks>
		D3D11.BlendOption SourceColorBlend
		{
			get;
		}

		/// <summary>
		/// Property to return the destination blending operation.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This defines the type of operation to apply to the color (RGB) components of a pixel being blended with the destination pixel data. 
		/// </para> 
		/// <para>
		/// The default value is <c>Zero</c>.
		/// </para>
		/// </remarks>
		D3D11.BlendOption DestinationColorBlend
		{
			get;
		}

		/// <summary>
		/// Property to return the source blending operation.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This defines the type of operation to apply to the alpha component of a pixel being blended from the source pixel data. 
		/// </para> 
		/// <para>
		/// The default value is <c>One</c>.
		/// </para>
		/// </remarks>
		D3D11.BlendOption SourceAlphaBlend
		{
			get;
		}

		/// <summary>
		/// Property to return the destination blending operation.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This defines the type of operation to apply to the alpha component of a pixel being blended with the destination pixel data. 
		/// </para> 
		/// <para>
		/// The default value is <c>Zero</c>.
		/// </para>
		/// </remarks>
		D3D11.BlendOption DestinationAlphaBlend
		{
			get;
		}

		/// <summary>
		/// Property to return the logical operation to apply when blending.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This provides extra functionality used when performing a blending operation. See <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/hh404484(v=vs.85).aspx">this link</a> for more details.
		/// </para>
		/// <para>
		/// The default value is <c>Noop</c>.
		/// </para>
		/// </remarks>
		D3D11.LogicOperation LogicOperation
		{
			get;
		}

		/// <summary>
		/// Property to return the flags used to mask which pixel component to write into.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This provides the ability to allow writes to only the specified component(s) defined in the mask. To define multiple components, combine the flags with the OR operator.
		/// </para>
		/// <para>
		/// The default value is <c>All</c>.
		/// </para>
		/// </remarks>
		D3D11.ColorWriteMaskFlags WriteMask
		{
			get;
		}

		/// <summary>
		/// Function to compare equality for this and another <see cref="IGorgonRenderTargetBlendStateInfo"/>.
		/// </summary>
		/// <param name="info">The <see cref="IGorgonRenderTargetBlendStateInfo"/> to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		bool IsEqual(IGorgonRenderTargetBlendStateInfo info);
	}
}