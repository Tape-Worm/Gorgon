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
using Gorgon.Math;

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
		/// Property to return the feature level requested by the user.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A user may request a lower feature level than what is supported by the device to allow the application to run on older video devices that lack support for newer functionality. This requested feature 
		/// level will be returned by this property.
		/// </para>
		/// <para>
		/// If the user does not request a feature level, or has specified one higher than what the video device supports, then the highest feature level supported by the video device 
		/// (indicated by the <see cref="IGorgonVideoDeviceInfo.SupportedFeatureLevel"/> property in the <see cref="Info"/> property) will be returned.
		/// </para>
		/// </remarks>
		DeviceFeatureLevel RequestedFeatureLevel
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the supported functionality for a given format.
		/// </summary>
		/// <param name="format">The format to evaluate.</param>
		/// <returns>A <see cref="BufferFormatSupport"/> containing OR'd values representing the functionality supported by the format.</returns>
		/// <remarks>
		/// Use this method to determine if a format can be used with a specific resource type (e.g. a 2D texture, vertex buffer, etc...). The value returned will of the <see cref="BufferFormatSupport"/> 
		/// value enumeration type and will contain the supported functionality represented as OR'd values.
		/// </remarks>
		BufferFormatSupport GetBufferFormatSupport(BufferFormat format);

		/// <summary>
		/// Function to retrieve the supported unordered access view functionality for a given format.
		/// </summary>
		/// <param name="format">The format to evaluate.</param>
		/// <returns>A <see cref="BufferFormatUavSupport"/> containing OR'd values representing the functionality supported by the format.</returns>
		/// <remarks>
		/// Use this method to determine if a format can be used with specific unordered access view operations in a shader. The value returned will of the <see cref="BufferFormatUavSupport"/> value 
		/// enumeration type and will contain the supported functionality represented as OR'd values.
		/// </remarks>
		BufferFormatUavSupport GetBufferFormatUavSupport(BufferFormat format);

		/// <summary>
		/// Function to determine if a device supports using rendering commands from multiple threads.
		/// </summary>
		/// <returns><b>true</b> if support is available, <b>false</b> if not.</returns>
		bool SupportsMultiThreadedCommands();

		/// <summary>
		/// Function to determine if a device supports creating resources from multiple threads.
		/// </summary>
		/// <returns><b>true</b> if support is available, <b>false</b> if not.</returns>
		bool SupportsMultiThreadedCreation();

		/// <summary>
		/// Function to return the maximum number of quality levels supported by the device for multi sampling.
		/// </summary>
		/// <param name="format">A <see cref="BufferFormat"/> to evaluate.</param>
		/// <param name="count">Number of multi samples.</param>
		/// <param name="forTiledResources">[Optional] <b>true</b> to check for tiled resource multiple sample quality, <b>false</b> to exclude tiled resources.</param>
		/// <returns>A <see cref="GorgonMultiSampleInfo"/> containing the quality count and sample count for multi-sampling.</returns>
		/// <remarks>
		/// Use this to return the quality count for a given multi-sample sample count. This method will return a <see cref="GorgonMultiSampleInfo"/> value type that contains both the sample count passed 
		/// to this method, and the quality count for that sample count. If the <see cref="GorgonMultiSampleInfo.Quality"/> is less than 1, then the sample count is not supported by this video device.
		/// </remarks>
		GorgonMultiSampleInfo GetMultiSampleQuality(BufferFormat format, int count, bool forTiledResources = false);

		/// <summary>
		/// Function to find the full screen display mode the 
		/// </summary>
		/// <param name="output">The output to use when looking for a video mode.</param>
		/// <param name="videoMode">The <see cref="GorgonVideoMode"/> used to find the closest match.</param>
		/// <param name="newMode">A <see cref="GorgonVideoMode"/> that is the nearest match for the provided video mode.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="output"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
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
		///			<description><see cref="GorgonVideoMode.RefreshRate"/> should be set to <see cref="GorgonRationalNumber.Empty"/> in order to skip filtering by refresh rate.</description>
		///		</item>
		///		<item>
		///			<description><see cref="GorgonVideoMode.Scaling"/> should be set to <see cref="VideoModeDisplayModeScaling.Unspecified"/> in order to skip filtering by the scaling mode.</description>
		///		</item>
		///		<item>
		///			<description><see cref="GorgonVideoMode.ScanlineOrdering"/> should be set to <see cref="VideoModeScanlineOrder.Unspecified"/> in order to skip filtering by the scanline order.</description>
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
		void FindClosestMode(IGorgonVideoOutputInfo output, ref GorgonVideoMode videoMode, out GorgonVideoMode newMode);
		#endregion
	}
}