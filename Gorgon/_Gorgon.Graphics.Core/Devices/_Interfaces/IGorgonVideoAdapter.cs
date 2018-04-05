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
// Created: Wednesday, January 6, 2016 7:53:58 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Functionality to provide access to a video adapter.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A video adapter can be a physical video card installed within the system, or an integrated video sub system. In some cases, as with the WARP video adapter, the video adapter may be a software construct.
	/// </para>
	/// <para>
	/// This object makes the video adapter available for use for rendering graphics, and provides functionality to query information about support for particular features supplied by the video adapter.
	/// </para>
	/// </remarks>
	public interface IGorgonVideoAdapter 
		: IDisposable
	{
		#region Properties.
		/// <summary>
		/// Property to return the maximum number of array indices for 1D and 2D textures.
		/// </summary>
		int MaxTextureArrayCount
		{
			get;
		}

		/// <summary>
		/// Property to return the maximum width of a 1D or 2D texture.
		/// </summary>
		int MaxTextureWidth
		{
			get;
		}

		/// <summary>
		/// Property to return the maximum height of a 2D texture.
		/// </summary>
		int MaxTextureHeight
		{
			get;
		}

		/// <summary>
		/// Property to return the maximum width of a 3D texture.
		/// </summary>
		int MaxTexture3DWidth
		{
			get;
		}

		/// <summary>
		/// Property to return the maximum height of a 3D texture.
		/// </summary>
		int MaxTexture3DHeight
		{
			get;
		}

		/// <summary>
		/// Property to return the maximum depth of a 3D texture.
		/// </summary>
		int MaxTexture3DDepth
		{
			get;
		}

	    /// <summary>
	    /// Property to return the maximum number of allowed scissor rectangles.
	    /// </summary>
	    int MaxScissorCount
	    {
	        get;
	    }

        /// <summary>
        /// Property to return the maximum number of allowed viewports.
        /// </summary>
	    int MaxViewportCount
	    {
	        get;
	    }

        /// <summary>
        /// Property to return information about this video adapter.
        /// </summary>
        IGorgonVideoAdapterInfo Info
		{
			get;
		}

		/// <summary>
		/// Property to return the supported <see cref="FeatureLevelSupport"/> for the device.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A user may request a lower <see cref="FeatureLevelSupport"/> than what is supported by the device to allow the application to run on older video adapters that lack support for newer functionality. 
		/// This requested feature level will be returned by this property if supported by the device. 
		/// </para>
		/// <para>
		/// If the user does not request a feature level, or has specified one higher than what the video adapter supports, then the highest feature level supported by the video adapter 
		/// (indicated by the <see cref="VideoDeviceInfo.SupportedFeatureLevel"/> property in the <see cref="Info"/> property) will be returned.
		/// </para>
		/// </remarks>
		/// <seealso cref="FeatureLevelSupport"/>
		FeatureLevelSupport RequestedFeatureLevel
		{
			get;
		}

	    /// <summary>
	    /// Property to return the maximum number of render targets allow to be assigned at the same time.
	    /// </summary>
	    int MaxRenderTargetCount
	    {
	        get;
	    }

        /// <summary>
        /// Property to return the maximum size, in bytes, for a constant buffer.
        /// </summary>
        int MaxConstantBufferSize
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the supported functionality for a given format.
		/// </summary>
		/// <param name="format">The format to evaluate.</param>
		/// <returns>A <c>FormatSupport</c> containing OR'd values representing the functionality supported by the format.</returns>
		/// <remarks>
		/// <para>
		/// Use this method to determine if a format can be used with a specific resource type (e.g. a 2D texture, vertex buffer, etc...). The value returned will be of the <c>FormatSupport</c> 
		/// enumeration and will contain the supported functionality represented as OR'd values.
		/// </para>
		/// </remarks>
		BufferFormatSupport GetBufferFormatSupport(BufferFormat format);

		/// <summary>
		/// Function to retrieve the supported unordered access compute resource functionality for a given format.
		/// </summary>
		/// <param name="format">The format to evaluate.</param>
		/// <returns>A <c>ComputeShaderFormatSupport</c> containing OR'd values representing the functionality supported by the format.</returns>
		/// <remarks>
		/// <para>
		/// Use this method to determine if a format can be used with specific unordered access view operations in a compute shader. The value returned will be of the <c>ComputeShaderFormatSupport</c> 
		/// enumeration type and will contain the supported functionality represented as OR'd values.
		/// </para>
		/// <para>
		/// Regardless of whether limited compute shader support is available on some Direct3D 10 class devices, this will always return <c>ComputeShaderFormatSupport.None</c> on devices with lower than 
		/// Level_11_0 feature level support.
		/// </para>
		/// </remarks>
		D3D11.ComputeShaderFormatSupport GetBufferFormatComputeSupport(BufferFormat format);

		/// <summary>
		/// Function to return a <see cref="GorgonMultisampleInfo"/> with the best quality level for the given count and format.
		/// </summary>
		/// <param name="format">A <c>Format</c> to evaluate.</param>
		/// <param name="count">The number of samples.</param>
		/// <returns>A <see cref="GorgonMultisampleInfo"/> containing the quality count and sample count for multisampling.</returns>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="count"/> is not supported by this video adapter.</exception>
		/// <remarks>
		/// <para>
		/// Use this to return a <see cref="GorgonMultisampleInfo"/> containing the best quality level for a given <paramref name="count"/> and <paramref name="format"/>.
		/// </para>
		/// <para>
		/// If <c>Unknown</c> is passed to the <paramref name="format"/> parameter, then this method will return <see cref="GorgonMultisampleInfo.NoMultiSampling"/>.
		/// </para>
		/// <para>
		/// Before calling this method, call the <see cref="O:Gorgon.Graphics.IGorgonVideoDevice.SupportsMultisampleCount"/> method to determine if multisampling is supported for the given <paramref name="count"/> and <paramref name="format"/>.
		/// </para>
		/// </remarks>
		GorgonMultisampleInfo GetMultisampleInfo(BufferFormat format, int count);

		/// <summary>
		/// Function to return whether or not the device supports multisampling for the given format and sample count.
		/// </summary>
		/// <param name="format">A <c>Format</c> to evaluate.</param>
		/// <param name="count">The number of samples.</param>
		/// <returns><b>true</b> if the device supports the format, or <b>false</b> if not.</returns>
		/// <remarks>
		/// <para>
		/// Use this to determine if the video adapter will support multisampling with a specific sample <paramref name="count"/> and <paramref name="format"/>. 
		/// </para>
		/// <para>
		/// If <c>Unknown</c> is passed to the <paramref name="format"/> parameter, then this method will return <b>true</b> because this will equate to no multisampling.
		/// </para>
		/// </remarks>
		bool SupportsMultisampleCount(BufferFormat format, int count);

		/// <summary>
		/// Function to return whether or not the device supports multisampling for the given format and the supplied <see cref="GorgonMultisampleInfo"/>.
		/// </summary>
		/// <param name="format">A <c>Format</c> to evaluate.</param>
		/// <param name="multiSampleInfo">The multisample info to use when evaluating.</param>
		/// <returns><b>true</b> if the device supports the format, or <b>false</b> if not.</returns>
		/// <remarks>
		/// <para>
		/// Use this to determine if the video adapter will support multisampling with a specific <paramref name="multiSampleInfo"/> and <paramref name="format"/>. 
		/// </para>
		/// <para>
		/// If <c>Unknown</c> is passed to the <paramref name="format"/> parameter, then this method will return <b>true</b> because this will equate to no multisampling.
		/// </para>
		/// </remarks>
		bool SupportsMultisampleInfo(BufferFormat format, GorgonMultisampleInfo multiSampleInfo);

		/// <summary>
		/// Function to find a display mode supported by the Gorgon.
		/// </summary>
		/// <param name="output">The output to use when looking for a video mode.</param>
		/// <param name="videoMode">The <see cref="GorgonVideoMode"/> used to find the closest match.</param>
		/// <param name="suggestedMode">A <see cref="GorgonVideoMode"/> that is the nearest match for the provided video mode.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="output"/> parameter is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// Users may leave the <see cref="GorgonVideoMode"/> values at unspecified (either 0, or default enumeration values) to indicate that these values should not be used in the search.
		/// </para>
		/// <para>
		/// The following members in <see cref="GorgonVideoMode"/> may be skipped (if not listed, then this member must be specified):
		/// <list type="bullet">
		///		<item>
		///			<description><see cref="GorgonVideoMode.Width"/> and <see cref="GorgonVideoMode.Height"/>.  Both values must be set to 0 if not filtering by width or height.</description>
		///		</item>
		///		<item>
		///			<description><see cref="GorgonVideoMode.RefreshRate"/> should be set to empty in order to skip filtering by refresh rate.</description>
		///		</item>
		///		<item>
		///			<description><see cref="GorgonVideoMode.Scaling"/> should be set to <see cref="ModeScaling.Unspecified"/> in order to skip filtering by the scaling mode.</description>
		///		</item>
		///		<item>
		///			<description><see cref="GorgonVideoMode.ScanlineOrder"/> should be set to <see cref="ModeScanlineOrder.Unspecified"/> in order to skip filtering by the scanline order.</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// The <see cref="GorgonVideoMode.Format"/> member must be one of the UNorm format types and cannot be set to <see cref="BufferFormat.Unknown"/>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		void FindNearestVideoMode(IGorgonVideoOutputInfo output, ref GorgonVideoMode videoMode, out GorgonVideoMode suggestedMode);
		#endregion
	}
}