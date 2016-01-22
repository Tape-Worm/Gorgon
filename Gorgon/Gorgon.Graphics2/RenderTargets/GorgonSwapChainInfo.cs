#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Wednesday, December 16, 2015 7:38:19 PM
// 
#endregion

using System;
using Gorgon.Math;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Type of presentation used for the <see cref="GorgonSwapChain"/> back buffers.
	/// </summary>
	/// <remarks>
	/// <para> 
	/// This defines how a <see cref="GorgonSwapChain"/> will present the contents of its back buffers to the front buffer.
	/// </para>
	/// <para>
	/// The primary difference between presentation models is how back buffer contents get to the Desktop Window Manager (DWM) for composition. In the flip model (which is the only model supported by 
	/// Gorgon), all back buffers are shared with the DWM. Therefore, the DWM can compose straight from those back buffers without any additional copy operations. This model also provides more features, such 
	/// as enhanced presentation statistics.
	/// </para>
	/// <para>
	/// // TODO: While this is true for IDXGISwapChain1, is this still true with IDXGISwapChain3?
	/// When you call <see cref="GorgonSwapChain.Present"/> on a flip model swap chain with 0 specified for its parameter, <see cref="GorgonSwapChain.Present"/>'s behavior will not only present the next frame 
	/// instead of any previously queued frames, it also terminates any remaining time left on the previously queued frames.
	/// </para>
	/// <para>
	/// <note type="warning">
	/// <para>
	/// The window handles assigned to swap chains cannot be used with GDI functionality, even after the destruction the swap chain.
	/// </para>
	/// </note>
	/// </para> 
	/// </remarks>
	/// <seealso cref="GorgonSwapChain"/>
	public enum PresentMode
	{
		/// <summary>
		/// <para>
		/// This flag specifies the flip presentation model and specifies that the contents of the back buffer persists after a call to <see cref="GorgonSwapChain.Present"/>.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// This flag cannot be used with multisampling.
		/// </para>
		/// </note>
		/// </para>  
		/// </summary>
		Flip = DXGI.SwapEffect.FlipSequential,
		/// <summary>
		/// <para>
		/// This flag specifies the flip presentation model and specifies that the contents of the back buffer should be discarded after a call to <see cref="GorgonSwapChain.Present"/>.
		/// </para>
		/// <para>
		/// This flag cannot be used with multisampling and partial presentation.
		/// </para>
		/// </summary>
		FlipDiscard = DXGI.SwapEffect.FlipDiscard
	}

	/// <summary>
	/// Usage flags for a swap chain.
	/// </summary>
	/// <remarks>
	/// These flags determine how a swap chain may be used with Gorgon.
	/// </remarks>
	[Flags]
	public enum SwapUsage
	{
		/// <summary>
		/// Allow the swap chain to be used as a render target output.
		/// </summary>
		RenderTarget = DXGI.Usage.RenderTargetOutput,
		/// <summary>
		/// Allow the swap chain to be used as an input on a shader.
		/// </summary>
		ShaderInput = DXGI.Usage.ShaderInput,
		/// <summary>
		/// Allow unordered access views to the swap chain.
		/// </summary>
		UnorderedAccess = DXGI.Usage.UnorderedAccess
	}

	/// <summary>
	/// Defines how the buffers in the swap chain should be scaled to match the output.
	/// </summary>
	public enum SwapScalingBehavior
	{
		/// <summary>
		/// No scaling.
		/// </summary>
		None = DXGI.Scaling.None,
		/// <summary>
		/// Stretch to match the size of the output.
		/// </summary>
		Stretch = DXGI.Scaling.Stretch,
		/// <summary>
		/// Stretch to match the size of the output, and ensure the aspect ratio is preserved.
		/// </summary>
		StretchAspectRatio = DXGI.Scaling.AspectRatioStretch
	}

	/// <summary>
	/// Defines the behavior of the alpha channel in a swap chain buffer surface.
	/// </summary>
	public enum SwapAlphaMode
	{
		/// <summary>
		/// Unspecified behavior.
		/// </summary>
		Unspecified = DXGI.AlphaMode.Unspecified,
		/// <summary>
		/// The transparency behavior is pre-multiplied. Each color is first scaled by the alpha value. The alpha value itself is the same in both straight and pre-multiplied alpha. Typically, 
		/// no color channel value is greater than the alpha channel value. If a color channel value in a pre-multiplied format is greater than the alpha channel, the standard source-over blending math 
		/// results in an additive blend.
		/// </summary>
		PreMultiplied = DXGI.AlphaMode.Premultiplied,
		/// <summary>
		/// The alpha channel indicates the transparency of the color.
		/// </summary>
		Straight = DXGI.AlphaMode.Straight,
		/// <summary>
		/// Ignore the transparency behavior.
		/// </summary>
		Ignore = DXGI.AlphaMode.Ignore 
	}

	/// <summary>
	/// Settings for creating a swap chain.
	/// </summary>
	/// <remarks>
	/// <para>
	/// For a full screen swap chain, there is an independent buffer used for the front buffer (buffer 0), and for windowed mode swap chains, the front buffer is the windows desktop.
	/// </para>
	/// </remarks>
	public class GorgonSwapChainInfo
	{
		#region Variables.
		// The number of back buffers.
		private int _backBufferCount;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the mode used to define how the back buffers are presented to the front buffer.
		/// </summary>
		/// <remarks>
		/// The default value for this property is <see cref="Graphics.PresentMode.FlipDiscard"/>.
		/// </remarks>
		/// <seealso cref="PresentMode"/>
		public PresentMode PresentMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the width, in pixels, of the swap chain.
		/// </summary>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height, in pixels, of the swap chain.
		/// </summary>
		public int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the display format for the buffers in the swap chain.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can only be 1 of the following values:
		/// <list type="bullet">
		/// <item>
		///		<description><see cref="BufferFormat.R8G8B8A8_UNorm"/></description>
		///		<description><see cref="BufferFormat.B8G8R8A8_UNorm"/></description>
		///		<description><see cref="BufferFormat.R16G16B16A16_Float"/></description>
		/// </item>
		/// Otherwise, an exception will occur when the swap chain is initialized.
		/// </list>
		/// </para>
		/// <para>
		/// To determine if a format is suitable for display, use the <see cref="IGorgonVideoDevice.GetBufferFormatSupport"/> method for a <see cref="IGorgonVideoDevice"/>.
		/// </para>
		/// <para>
		/// The default value for this property is <see cref="BufferFormat.R8G8B8A8_UNorm"/>.
		/// </para>
		/// </remarks>
		/// <see cref="BufferFormat"/>
		public BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of back buffers to use for the swap chain.
		/// </summary>
		/// <remarks>
		/// <para>
		/// When using full screen mode, ensure that the front buffer is also included in the count.
		/// </para>
		/// <para>
		/// This value is limited to values ranging from 2 to 16, any values that fall outside of that range will be clamped.
		/// </para>
		/// <para>
		/// The default value for this property is 3.
		/// </para>
		/// </remarks>
		public int BackBufferCount
		{
			get
			{
				return _backBufferCount;
			}
			set
			{
				_backBufferCount = value.Max(2).Min(16);
			}
		}

		/// <summary>
		/// Property to set or return the handle to the window bound to the swap chain.
		/// </summary>
		public IntPtr WindowHandle
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the desired usage for a buffer in a swap chain.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The default value is <see cref="SwapUsage.RenderTarget"/>.
		/// </para>
		/// </remarks>
		public SwapUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the scaling behavior for the swap chain compared to the output.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Scaling takes place when the size of the buffers in the swap chain does not match the output size. 
		/// </para>
		/// <para>
		/// The default value is <see cref="SwapScalingBehavior.Stretch"/>.
		/// </para>
		/// </remarks>
		public SwapScalingBehavior ScalingBehavior
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return how an alpha channel for a swap chain buffer surface should be handled.
		/// </summary>
		/// <remarks>
		/// The default value is <see cref="SwapAlphaMode.Unspecified"/>.
		/// </remarks>
		public SwapAlphaMode AlphaMode
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChainInfo"/> class.
		/// </summary>
		public GorgonSwapChainInfo()
		{
			PresentMode = PresentMode.FlipDiscard;
			Format = BufferFormat.R8G8B8A8_UNorm;
			BackBufferCount = 3;
			Usage = SwapUsage.RenderTarget;
			ScalingBehavior = SwapScalingBehavior.Stretch;
			AlphaMode = SwapAlphaMode.Unspecified;
		}
		#endregion
	}
}
