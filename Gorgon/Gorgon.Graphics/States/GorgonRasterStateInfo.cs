﻿#region MIT
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
// Created: July 27, 2016 7:02:11 PM
// 
#endregion

using System;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Information used to describe a rasterization state.
	/// </summary>
	public class GorgonRasterStateInfo 
		: IGorgonRasterStateInfo
	{
		#region Common States
		/// <summary>
		/// Property to return the default raster state.
		/// </summary>
		public static GorgonRasterStateInfo Default
		{
			get;
		} = new GorgonRasterStateInfo();

		/// <summary>
		/// Wireframe, with no culling.
		/// </summary>
		public static GorgonRasterStateInfo WireFrameNoCulling
		{
			get;
		} = new GorgonRasterStateInfo
		    {
			    CullMode = D3D11.CullMode.None,
			    FillMode = D3D11.FillMode.Wireframe
		    };

		/// <summary>
		/// Back face culling.
		/// </summary>
		public static GorgonRasterStateInfo CullBackFace
		{
			get;
		} = new GorgonRasterStateInfo
		    {
			    CullMode = D3D11.CullMode.Back
		    };

		/// <summary>
		/// No culling.
		/// </summary>
		public static GorgonRasterStateInfo NoCulling
		{
			get;
		} = new GorgonRasterStateInfo
		    {
			    CullMode = D3D11.CullMode.None
		    };

		/// <summary>
		/// Scissor test enabled.
		/// </summary>
		public static GorgonRasterStateInfo ScissorTestEnabled
		{
			get;
		} = new GorgonRasterStateInfo
		    {
			    IsScissorClippingEnabled = true
		    };
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current culling mode.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value is used to determine if a triangle is drawn or not by the direction it's facing.
		/// </para>
		/// <para>
		/// The default value is <c>Back</c>.
		/// </para>
		/// </remarks>
		public D3D11.CullMode CullMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the triangle fill mode.
		/// </summary>
		/// <remarks>
		/// The default value is <c>Solid</c>.
		/// </remarks>
		public D3D11.FillMode FillMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether a triangle is front or back facing.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value determines if a triangle is front or back facing by using the winding order of its vertices.
		/// </para>
		/// <para>
		/// The default value is <b>false</b>.
		/// </para>
		/// </remarks>
		public bool IsFrontCounterClockwise
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the value to be added to the depth of a pixel.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value is used to help z-fighting for co-planar polygons. This is often caused by a lack of precision for the depth in the view volume and the depth/stencil buffer. By adding a small offset 
		/// via this property, it can make a polygon appear to be in front of or behind another polygon even though they actually share the same (or very nearly the same depth value).
		/// </para>
		/// <para>
		/// The default value is 0.
		/// </para>
		/// </remarks>
		public int DepthBias
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the scalar used for a slope of a pixel.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is used in conjunction with the <see cref="DepthBias"/> to help overcome issues (typically "acne" for shadow maps) when rendering coplanar polygons. It is used to adjust the depth value 
		/// based on a slope.
		/// </para>
		/// <para>
		/// The default value is 0.0f.
		/// </para>
		/// </remarks>
		public float SlopeScaledDepthBias
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the clamping value for the <see cref="DepthBias"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The maximum <see cref="DepthBias"/> for a pixel.
		/// </para>
		/// <para>
		/// The default value is 0.0f.
		/// </para>
		/// </remarks>
		/// <seealso cref="DepthBias"/>
		public float DepthBiasClamp
		{
			get;
			set;
		}


		/// <summary>
		/// Property to set or return whether depth/clipping is enabled or not.
		/// </summary>
		/// <remarks>
		/// The default value is <b>true</b>.
		/// </remarks>
		public bool IsDepthClippingEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether scissor rectangle clipping is enabled or not.
		/// </summary>
		/// <remarks>
		/// The default value is <b>false</b>.
		/// </remarks>
		public bool IsScissorClippingEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether multisampling anti-aliasing or alpha line anti-aliasing for a render target is enabled or not.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Set this value to <b>true</b> to use quadrilateral anti-aliasing or <b>false</b> to use a alpha line algorithm. This will only apply to render targets that have a 
		/// <see cref="IGorgonTextureInfo.MultisampleInfo"/> <see cref="GorgonMultisampleInfo.Count"/> greater than 1.
		/// </para>
		/// <para>
		/// If the current <see cref="IGorgonVideoDevice"/> has a <see cref="IGorgonVideoDevice.RequestedFeatureLevel"/> of <see cref="FeatureLevelSupport.Level_10_0"/>, then setting this value to 
		/// <b>false</b> will disable anti-aliasing.
		/// </para>
		/// <para>
		/// This value overrides the value set in <see cref="IsAntialiasedLineEnabled"/>.
		/// </para>
		/// <para>
		/// The default value is <b>false</b>.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsAntialiasedLineEnabled"/>
		public bool IsMultisamplingEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether anti-aliased line drawing is enabled or not.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This only applies when drawing lines and <see cref="IsMultisamplingEnabled"/> is set to <b>false</b>.
		/// </para>
		/// <para>
		/// The default value is <b>false</b>.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsMultisamplingEnabled"/>
		public bool IsAntialiasedLineEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of samples to use when unordered access view rendering or rasterizing.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This forces the number of samples to use when rendering unordered access view data. The valid values are 0, 1, 2, 4, 8 and a value of 16 indicates that the sample count is not forced.
		/// </para>
		/// <para>
		/// The default value is 0.
		/// </para>
		/// <para>
		/// <note type="note">
		/// <para>
		/// If you want to render with sample count set to 1 or greater, you must follow these guidelines:
		/// <list type="bullet">
		///		<item> 
		///			<description>Don't bind depth-stencil views.</description>
		///		</item>
		///		<item>
		///			<description>Disable depth testing.</description>
		///		</item>
		///		<item>
		///			<description>Ensure the shader doesn't output depth.</description>
		///		</item>
		///		<item>
		///			<description>If you have any render-target views bound and this value is greater than 1, ensure that every render target has only a single sample.</description>
		///		</item>
		///		<item>
		///			<description>Don't operate the shader at sample frequency.</description>
		///		</item> 
		/// </list>
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public int ForcedUavSampleCount
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to compare equality for this and another <see cref="IGorgonRasterStateInfo"/>.
		/// </summary>
		/// <param name="info">The <see cref="IGorgonRasterStateInfo"/> to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public bool IsEqual(IGorgonRasterStateInfo info)
		{
			return info != null
			       && IsAntialiasedLineEnabled == info.IsAntialiasedLineEnabled
			       && CullMode == info.CullMode
			       && DepthBias == info.DepthBias
			       && DepthBiasClamp.EqualsEpsilon(info.DepthBiasClamp)
			       && IsDepthClippingEnabled == info.IsDepthClippingEnabled
			       && FillMode == info.FillMode
			       && ForcedUavSampleCount == info.ForcedUavSampleCount
			       && IsFrontCounterClockwise == info.IsFrontCounterClockwise
			       && IsMultisamplingEnabled == info.IsMultisamplingEnabled
			       && IsScissorClippingEnabled == info.IsScissorClippingEnabled
			       && SlopeScaledDepthBias.EqualsEpsilon(info.SlopeScaledDepthBias);
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRasterStateInfo"/> class.
		/// </summary>
		/// <param name="info">The <see cref="IGorgonRasterStateInfo"/> to copy settings from.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
		public GorgonRasterStateInfo(IGorgonRasterStateInfo info)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			IsAntialiasedLineEnabled = info.IsAntialiasedLineEnabled;
			CullMode = info.CullMode;
			DepthBias = info.DepthBias;
			DepthBiasClamp = info.DepthBiasClamp;
			IsDepthClippingEnabled = info.IsDepthClippingEnabled;
			FillMode = info.FillMode;
			ForcedUavSampleCount = info.ForcedUavSampleCount;
			IsFrontCounterClockwise = info.IsFrontCounterClockwise;
			IsMultisamplingEnabled = info.IsMultisamplingEnabled;
			IsScissorClippingEnabled = info.IsScissorClippingEnabled;
			SlopeScaledDepthBias = info.SlopeScaledDepthBias;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRasterStateInfo"/> class.
		/// </summary>
		public GorgonRasterStateInfo()
		{
			CullMode = D3D11.CullMode.Back;
			FillMode = D3D11.FillMode.Solid;
			IsDepthClippingEnabled = true;
		}
		#endregion
	}
}
