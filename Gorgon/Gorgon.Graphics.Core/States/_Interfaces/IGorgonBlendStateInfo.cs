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
// Created: July 30, 2016 12:46:27 PM
// 
#endregion

using System.Collections.Generic;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Information used to build a <see cref="GorgonBlendStateInfo"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This provides an immutable view of the stencil operation state information so that it cannot be modified after the state is created.
	/// </para>
	/// </remarks>
	public interface IGorgonBlendStateInfo
	{
		/// <summary>
		/// Property to return whether alpha to coverage is enabled or not.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will use alpha to coverage as a multisampling technique when writing a pixel to a render target. Alpha to coverage is useful in situations where there are multiple overlapping polygons 
		/// that use transparency to define edges.
		/// </para>
		/// <para>
		/// The default value is <b>false</b>.
		/// </para>
		/// </remarks>
		bool IsAlphaToCoverageEnabled
		{
			get;
		}

		/// <summary>
		/// Property to return whether independent render target blending is enabled or not.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will specify whether to use different blending states for each render target. When this value is set to <b>true</b>, each render target blend state will be independent of other render 
		/// target blend states. When this value is set to <b>false</b>, then only the blend state of the first render target is used.
		/// </para>
		/// <para>
		/// The default value is <b>false</b>.
		/// </para>
		/// </remarks>
		bool IsIndependentBlendingEnabled
		{
			get;
		}

		/// <summary>
		/// Property to return the list of render target blend states.
		/// </summary>
		IReadOnlyList<IGorgonRenderTargetBlendStateInfo> RenderTargets
		{
			get;
		}
	}
}