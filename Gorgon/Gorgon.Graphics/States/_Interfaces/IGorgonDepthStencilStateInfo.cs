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
// Created: July 29, 2016 7:58:11 PM
// 
#endregion

using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Information used to create a <see cref="GorgonDepthStencilState"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This provides an immutable view of the stencil operation state information so that it cannot be modified after the state is created.
	/// </para>
	/// </remarks>
	public interface IGorgonDepthStencilStateInfo
	{
		/// <summary>
		/// Property to return the depth comparison function.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Use this property to determine whether a depth value will be written into the buffer if the function specified evaluates to true using the data being written and existing data.
		/// </para>
		/// <para>
		/// The default value is <c>Less</c>.
		/// </para>
		/// </remarks>
		D3D11.Comparison DepthComparison
		{
			get;
		}

		/// <summary>
		/// Property to return whether to enable writing to the depth buffer or not.
		/// </summary>
		/// <remarks>
		/// The default value is <c>true</c>.
		/// </remarks>
		bool IsDepthWriteEnabled
		{
			get;
		}

		/// <summary>
		/// Property to return whether the depth buffer is enabled or not.
		/// </summary>
		/// <remarks>
		/// The default value is <b>false</b>.
		/// </remarks>
		bool IsDepthEnabled
		{
			get;
		}

		/// <summary>
		/// Property to return whether the stencil buffer is enabled or not.
		/// </summary>
		/// <remarks>
		/// The default value is <b>false</b>.
		/// </remarks>
		bool IsStencilEnabled
		{
			get;
		}

		/// <summary>
		/// Property to return the read mask for stencil operations.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Use this to filter out specific values from the stencil buffer during a read operation.
		/// </para>
		/// <para>
		/// The default value is <c>0xff</c>.
		/// </para>
		/// </remarks>
		byte StencilReadMask
		{
			get;
		}

		/// <summary>
		/// Property to return the write mask for stencil operations.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Use this to filter out specific values from the stencil buffer during a write operation.
		/// </para>
		/// <para>
		/// The default value is <c>0xff</c>.
		/// </para>
		/// </remarks>
		byte StencilWriteMask
		{
			get;
		}

		/// <summary>
		/// Property to return the setup information for stencil operations on front facing polygons.
		/// </summary>
		IGorgonStencilOperationInfo FrontFaceStencilOp
		{
			get;
		}

		/// <summary>
		/// Property to return the setup information for stencil operations on back facing polygons.
		/// </summary>
		IGorgonStencilOperationInfo BackFaceStencilOp
		{
			get;
		}
	}
}