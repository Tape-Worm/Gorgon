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
// Created: July 30, 2016 12:11:11 PM
// 
#endregion

using System;
using System.Collections.Generic;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Information used to build a blending state.
	/// </summary>
	public class GorgonBlendStateInfo 
		: IGorgonBlendStateInfo
	{
		#region Variables.
		// The independent render target blend states.
		private readonly GorgonRenderTargetBlendStateInfo[] _targets = new GorgonRenderTargetBlendStateInfo[D3D11.OutputMergerStage.SimultaneousRenderTargetCount];

		/// <summary>
		/// The default blending state.
		/// </summary>
		public static readonly IGorgonBlendStateInfo Default = new GorgonBlendStateInfo();

		/// <summary>
		/// Render target 0 blending enabled, blending operations don't allow for blending.
		/// </summary>
		public static readonly IGorgonBlendStateInfo NoBlending;

		/// <summary>
		/// Modulated blending on render target 0.
		/// </summary>
		public static readonly IGorgonBlendStateInfo Modulated;

		/// <summary>
		/// Additive blending on render target 0.
		/// </summary>
		public static readonly IGorgonBlendStateInfo Additive;

		/// <summary>
		/// Premultiplied alpha blending on render target 0.
		/// </summary>
		public static readonly IGorgonBlendStateInfo Premultiplied;

		/// <summary>
		/// Inverse color blending on render target 0.
		/// </summary>
		public static readonly IGorgonBlendStateInfo Inverted;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether alpha to coverage is enabled or not.
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
		public bool IsAlphaToCoverageEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether independent render target blending is enabled or not.
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
		public bool IsIndependentBlendingEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the list of render target blend states.
		/// </summary>
		IReadOnlyList<IGorgonRenderTargetBlendStateInfo> IGorgonBlendStateInfo.RenderTargets => _targets;

		/// <summary>
		/// Property to return the list of render target blend states.
		/// </summary>
		public IReadOnlyList<GorgonRenderTargetBlendStateInfo> RenderTargets => _targets;
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBlendStateInfo"/> class.
		/// </summary>
		/// <param name="info">A <see cref="IGorgonBlendStateInfo"/> to copy settings from.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> is <b>null</b>.</exception>
		public GorgonBlendStateInfo(IGorgonBlendStateInfo info)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			IsAlphaToCoverageEnabled = info.IsAlphaToCoverageEnabled;
			IsIndependentBlendingEnabled = info.IsIndependentBlendingEnabled;

			for (int i = 0; i < _targets.Length; ++i)
			{
				_targets[i] = new GorgonRenderTargetBlendStateInfo(info.RenderTargets[i]);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBlendStateInfo"/> class.
		/// </summary>
		public GorgonBlendStateInfo()
		{
			for (int i = 0; i < _targets.Length; ++i)
			{
				_targets[i] = new GorgonRenderTargetBlendStateInfo();
			}
		}

		/// <summary>
		/// Initializes static members of the <see cref="GorgonBlendStateInfo"/> class.
		/// </summary>
		static GorgonBlendStateInfo()
		{
			var state = new GorgonBlendStateInfo();
			state.RenderTargets[0].IsBlendingEnabled = true;

			NoBlending = new GorgonBlendStateInfo(state);

			state.RenderTargets[0].SourceColorBlend = D3D11.BlendOption.SourceAlpha;
			state.RenderTargets[0].DestinationColorBlend = D3D11.BlendOption.InverseSourceAlpha;
			Modulated = new GorgonBlendStateInfo(state);

			state.RenderTargets[0].SourceColorBlend = D3D11.BlendOption.SourceAlpha;
			state.RenderTargets[0].DestinationColorBlend = D3D11.BlendOption.One;
			Additive = new GorgonBlendStateInfo(state);

			state.RenderTargets[0].SourceColorBlend = D3D11.BlendOption.One;
			state.RenderTargets[0].DestinationColorBlend = D3D11.BlendOption.InverseSourceAlpha;
			Premultiplied = new GorgonBlendStateInfo(state);

			state.RenderTargets[0].SourceColorBlend = D3D11.BlendOption.InverseDestinationColor;
			state.RenderTargets[0].DestinationColorBlend = D3D11.BlendOption.InverseSourceColor;
			Inverted = new GorgonBlendStateInfo(state);
		}
		#endregion
	}
}
