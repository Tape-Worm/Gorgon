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
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Functionality to provide access to a video device.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A video device can be a physical video card installed within the system, or an integrated video sub system. In some cases, as with the WARP video device, the video device may be a software construct.
	/// </para>
	/// <para>
	/// This object makes the video device available for use for rendering graphics, and provides functionality to query information about support for particular features supplied by the video device.
	/// </para>
	/// </remarks>
	public interface IGorgonVideoDevice 
		: IDisposable
	{
		#region Properties.
		/// <summary>
		/// Property to return information about this video device.
		/// </summary>
		IGorgonVideoDeviceInfo Info
		{
			get;
		}

		/// <summary>
		/// Property to return the actual supported <see cref="FeatureLevelSupport"/> from the device.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A user may request a lower <see cref="FeatureLevelSupport"/> than what is supported by the device to allow the application to run on older video devices that lack support for newer functionality. 
		/// This requested feature level will be returned by this property if supported by the device. 
		/// </para>
		/// <para>
		/// If the user does not request a feature level, or has specified one higher than what the video device supports, then the highest feature level supported by the video device 
		/// (indicated by the <see cref="VideoDeviceInfo.SupportedFeatureLevel"/> property in the <see cref="Info"/> property) will be returned.
		/// </para>
		/// </remarks>
		/// <seealso cref="FeatureLevelSupport"/>
		FeatureLevelSupport RequestedFeatureLevel
		{
			get;
		}

		/// <summary>
		/// Property to return the maximum number of render target view slots available.
		/// </summary>
		int MaxRenderTargetViewSlots
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

		/// <summary>
		/// Property to return the Direct 3D 11.1 device context.
		/// </summary>
		D3D11.DeviceContext1 D3DDeviceContext
		{
			get;
		}

		/// <summary>
		/// Property to return the Direct 3D 11.1 device object
		/// </summary>
		D3D11.Device1 D3DDevice
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
		D3D11.FormatSupport GetBufferFormatSupport(DXGI.Format format);

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
		D3D11.ComputeShaderFormatSupport GetBufferFormatComputeSupport(DXGI.Format format);

		/// <summary>
		/// Function to return the maximum number of quality levels supported by the device for multi sampling.
		/// </summary>
		/// <param name="format">A <c>Format</c> to evaluate.</param>
		/// <param name="count">Number of multi samples.</param>
		/// <returns>A <see cref="GorgonMultiSampleInfo"/> containing the quality count and sample count for multi-sampling.</returns>
		/// <remarks>
		/// <para>
		/// Use this to return the quality count for a given multi-sample sample count. This method will return a <see cref="GorgonMultiSampleInfo"/> value type that contains both the sample count passed 
		/// to this method, and the quality count for that sample count. If the <see cref="GorgonMultiSampleInfo.Quality"/> is less than 1, then the sample count is not supported by this video device.
		/// </para>
		/// </remarks>
		GorgonMultiSampleInfo GetMultiSampleQuality(DXGI.Format format, int count);

		/// <summary>
		/// Function to find a display mode supported by the Gorgon.
		/// </summary>
		/// <param name="output">The output to use when looking for a video mode.</param>
		/// <param name="videoMode">The <c>ModeDescription1</c> used to find the closest match.</param>
		/// <returns>A <c>ModeDescription1</c> that is the nearest match for the provided video mode.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="output"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// Users may leave the <c>ModeDescription1</c> values at unspecified (either 0, or default enumeration values) to indicate that these values should not be used in the search.
		/// </para>
		/// <para>
		/// The following members in <c>ModeDescription1</c> may be skipped (if not listed, then this member must be specified):
		/// <list type="bullet">
		///		<item>
		///			<description><c>ModeDescription1.Width</c> and <c>ModeDescription1.Height</c>.  Both values must be set to 0 if not filtering by width or height.</description>
		///		</item>
		///		<item>
		///			<description><c>ModeDescription1.RefreshRate</c> should be set to empty in order to skip filtering by refresh rate.</description>
		///		</item>
		///		<item>
		///			<description><c>ModeDescription1.Scaling</c> should be set to <c>DisplayModeScaling.Unspecified</c> in order to skip filtering by the scaling mode.</description>
		///		</item>
		///		<item>
		///			<description><c>ModeDescription1.ScanlineOrdering</c> should be set to <c>ScanlineOrder.Unspecified</c> in order to skip filtering by the scanline order.</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// The <c>ModeDescription1.Format</c> member must be one of the UNorm format types and cannot be set to <c>Format.Unknown</c>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		DXGI.ModeDescription1 FindNearestVideoMode(IGorgonVideoOutputInfo output, ref DXGI.ModeDescription1 videoMode);
		#endregion
	}
}