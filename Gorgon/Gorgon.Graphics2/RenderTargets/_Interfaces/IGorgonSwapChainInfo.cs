using System;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Settings for creating a swap chain.
	/// </summary>
	/// <remarks>
	/// <para>
	/// For a full screen swap chain, there is an independent buffer used for the front buffer (buffer 0), and for windowed mode swap chains, the front buffer is the windows desktop.
	/// </para>
	/// <para>
	/// This interface is read-only. To set the values for this interface, use the concrete <see cref="GorgonSwapChainInfo"/> class.
	/// </para>
	/// </remarks>
	public interface IGorgonSwapChainInfo
	{
		/// <summary>
		/// Property to return the mode used to define how the back buffers are presented to the front buffer.
		/// </summary>
		/// <remarks>
		/// The default value for this property is <see cref="Graphics.PresentMode.Discard"/>.
		/// </remarks>
		/// <seealso cref="PresentMode"/>
		PresentMode PresentMode
		{
			get;
		}

		/// <summary>
		/// Property to return the width, in pixels, of the swap chain.
		/// </summary>
		int Width
		{
			get;
		}

		/// <summary>
		/// Property to return the height, in pixels, of the swap chain.
		/// </summary>
		int Height
		{
			get;
		}

		/// <summary>
		/// Property to return the display format for the buffers in the swap chain.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the <see cref="PresentMode"/> is set to <see cref="Graphics.PresentMode.Flip"/>, then this value can only be 1 of the following values:
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
		/// To determine if a format is suitable for display, use the <see cref="IGorgonVideoDevice.SupportsDisplayFormat"/> method for a <see cref="IGorgonVideoDevice"/>.
		/// </para>
		/// <para>
		/// The default value for this property is <see cref="BufferFormat.R8G8B8A8_UNorm"/>.
		/// </para>
		/// </remarks>
		/// <see cref="BufferFormat"/>
		BufferFormat Format
		{
			get;
		}

		/// <summary>
		/// Property to return the number of back buffers to use for the swap chain.
		/// </summary>
		/// <remarks>
		/// <para>
		/// When using full screen mode, ensure that the front buffer is also included in the count.
		/// </para>
		/// <para>
		/// If the <see cref="PresentMode"/> value is set to <see cref="Graphics.PresentMode.Flip"/>, this value must be set between 2 and 16, otherwise an exception will be raised when the swap chain is 
		/// initialized.
		/// </para>
		/// <para>
		/// The default value for this property is 3.
		/// </para>
		/// </remarks>
		int BackBufferCount
		{
			get;
		}

		/// <summary>
		/// Property to return the handle to the window bound to the swap chain.
		/// </summary>
		IntPtr WindowHandle
		{
			get;
		}

		/// <summary>
		/// Property to return the multi-sample information for the swap chain.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Setting this value to another other than <see cref="GorgonMultiSampleInfo.NoMultiSampling"/> (count: 1, quality: 0) requires that the swap chain have a <see cref="PresentMode"/> of 
		/// <see cref="Graphics.PresentMode.Discard"/> or <see cref="Graphics.PresentMode.Sequential"/>. If the <see cref="Graphics.PresentMode.Flip"/> presentation mode is used, and this value is not set 
		/// to no multisampling, then an exception will be raised when the swap chain is initialized.
		/// </para>
		/// <para>
		/// The default value is <see cref="GorgonMultiSampleInfo.NoMultiSampling"/>.
		/// </para> 
		/// </remarks>
		/// <seealso cref="GorgonMultiSampleInfo"/>
		GorgonMultiSampleInfo MultiSampleInfo
		{
			get;
		}

		/// <summary>
		/// Property to return the desired usage for a buffer in a swap chain.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The default value is <see cref="SwapUsage.RenderTarget"/>.
		/// </para>
		/// </remarks>
		SwapUsage Usage
		{
			get;
		}

		/// <summary>
		/// Property to return the scaling behavior for the swap chain compared to the output.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Scaling takes place when the size of the buffers in the swap chain does not match the output size. 
		/// </para>
		/// <para>
		/// The default value is <see cref="SwapScalingBehavior.Stretch"/>.
		/// </para>
		/// </remarks>
		SwapScalingBehavior ScalingBehavior
		{
			get;
		}

		/// <summary>
		/// Property to return how an alpha channel for a swap chain buffer surface should be handled.
		/// </summary>
		/// <remarks>
		/// The default value is <see cref="SwapAlphaMode.Unspecified"/>.
		/// </remarks>
		SwapAlphaMode AlphaMode
		{
			get;
		}
	}
}