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
// Created: August 17, 2016 11:04:58 PM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Core;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A list of <see cref="GorgonRenderTargetView"/> objects, and optionally, a <see cref="GorgonDepthStencilView"/>, to apply to the pipeline.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This interface defines an immutable list of <see cref="GorgonRenderTargetView"/> objects and a <see cref="GorgonDepthStencilView"/>.
	/// </para>
	/// </remarks>
	public interface IGorgonRenderTargetViews 
		: IReadOnlyList<GorgonRenderTargetView>
	{
		/// <summary>
		/// Property to set or return the currently active depth/stencil view.
		/// </summary>
		/// <exception cref="GorgonException">Thrown when the resource type for the resource bound to the <see cref="GorgonDepthStencilView"/> being assigned does not match the resource type for resources 
		/// attached to other views in this list.
		/// <para>-or-</para>
		/// <para>Thrown when the array (or depth) index, or the array (or depth) count does not match the other views in this list.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the resource <see cref="GorgonMultisampleInfo"/> does not match the <see cref="GorgonMultisampleInfo"/> for other resources bound to other views on this list.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// When binding a <see cref="GorgonDepthStencilView"/>, the resource must be of the same type as other resources for other views in this list. If they do not match, an exception will be thrown.
		/// </para>
		/// <para>
		/// All <see cref="GorgonDepthStencilView"/> parameters, such as array (or depth) index and array (or depth) count must be the same as the other views in this list. If they are not, an exception 
		/// will be thrown. Mip slices may be different. An exception will also be raised if the resources attached to <see cref="GorgonRenderTargetView">GorgonRenderTargetViews</see> in this list do not 
		/// have the same array/depth count.
		/// </para>
		/// <para>
		/// If the <see cref="GorgonRenderTargetView">GorgonRenderTargetViews</see> are attached to resources with multisampling enabled through <see cref="GorgonMultisampleInfo"/>, then the 
		/// <see cref="GorgonMultisampleInfo"/> of the resource attached to the <see cref="GorgonDepthStencilView"/> being assigned must match, or an exception will be thrown.
		/// </para>
		/// <para>
		/// These limitations also apply to the <see cref="DepthStencilView"/> property. All views must match the mip slice, array (or depth) index, and array (or depth) count, and the <see cref="ResourceType"/> 
		/// for the resources attached to the <see cref="GorgonRenderTargetView">GorgonRenderTargetViews</see> must be the same.
		/// </para>
		/// <para>
		/// The format for the view may differ from the formats of other views in this list.
		/// </para>
		/// <para>
		/// <note type="information">
		/// <para>
		/// The exceptions raised when validating a view against other views in this list are only thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		GorgonDepthStencilView DepthStencilView
		{
			get;
			set;
		}
	}
}