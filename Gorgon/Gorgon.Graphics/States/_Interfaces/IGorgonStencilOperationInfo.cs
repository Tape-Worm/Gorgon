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
// Created: July 29, 2016 7:40:55 PM
// 
#endregion

using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Information used to create the stencil portion of a <see cref="IGorgonDepthStencilStateInfo"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This provides an immutable view of the stencil operation state information so that it cannot be modified after the state is created.
	/// </para>
	/// </remarks>
	public interface IGorgonStencilOperationInfo
	{
		/// <summary>
		/// Property to return the comparison function to use for stencil operations.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This specifies the function to evaluate with stencil data being read/written and existing stencil data.
		/// </para>
		/// <para>
		/// The default value is <c>Always</c>.
		/// </para>
		/// </remarks>
		D3D11.Comparison Comparison
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the operation to perform when the depth testing function fails, but stencil testing passes.
		/// </summary>
		/// <remarks>
		/// The default value is <c>Keep</c>.
		/// </remarks>
		D3D11.StencilOperation DepthFailOperation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the operation to perform when the stencil testing fails.
		/// </summary>
		/// <remarks>
		/// The default value is <c>Keep</c>.
		/// </remarks>
		D3D11.StencilOperation FailOperation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the operation to perform when the stencil testing passes.
		/// </summary>
		/// <remarks>
		/// The default value is <c>Keep</c>.
		/// </remarks>
		D3D11.StencilOperation PassOperation
		{
			get;
		}
	}
}