#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, February 23, 2013 4:00:19 PM
// 
#endregion

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Collections;
using Gorgon.Collections.Specialized;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.UI;
using SharpDX.DXGI;
using DX = SharpDX;
using D3DCommon = SharpDX.Direct3D;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Enumerates information about the installed video devices on the system.
	/// </summary>
	/// <remarks>Use this to retrieve a list of video devices available on the system. A video device may be a discreet video card, or a device on the motherboard. 
	/// Retrieve the video device information by calling the <see cref="Gorgon.Graphics.GorgonVideoDeviceEnumerator.Enumerate">Enumerate</see> method and the 
	/// <see cref="P:Gorgon.Graphics.GorgonVideoDeviceEnumerator.VideoDevices">VideoDevices</see> property will be populated with video device information.  From 
	/// there you can pass a <see cref="Gorgon.Graphics.GorgonVideoDevice">GorgonVideoDevice</see> object into the <see cref="Gorgon.Graphics.GorgonGraphics">
	/// GorgonGraphics</see> constructor to use a specific video device.
	/// <para>This interface will allow enumeration of the WARP/Reference devices.  WARP is a high performance software device that will emulate much of the functionality 
	/// that a real video device would have. The reference device is a fully featured device, but is incredibly slow and useful when debugging driver issues.</para>
	/// </remarks>
	public static class GorgonVideoDeviceEnumerator
	{
		#region Variables.
		private static int _lockIncr;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of available video devices installed on the system.
		/// </summary>
		public static IGorgonNamedObjectReadOnlyList<GorgonVideoDevice> VideoDevices
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		#pragma warning disable 0618
		/// <summary>
		/// Function to add the WARP software device.
		/// </summary>
		/// <param name="index">Index of the device.</param>
		/// <returns>The video device used for WARP software rendering.</returns>
		private static GorgonVideoDevice GetWARPSoftwareDevice(int index)
		{
			GorgonVideoDevice device;

#if DEBUG
            using (var D3DDevice = new D3D.Device(D3DCommon.DriverType.Warp, D3D.DeviceCreationFlags.Debug, 
                D3DCommon.FeatureLevel.Level_10_1, D3DCommon.FeatureLevel.Level_10_0, D3DCommon.FeatureLevel.Level_9_3))
			{
#else
            using (var D3DDevice = new D3D.Device(D3DCommon.DriverType.Warp, D3D.DeviceCreationFlags.None, 
                D3DCommon.FeatureLevel.Level_10_1, D3DCommon.FeatureLevel.Level_10_0, D3DCommon.FeatureLevel.Level_9_3))
			{
#endif
                using (var giDevice = D3DDevice.QueryInterface<Device1>())
				{
					using (var adapter = giDevice.GetParent<Adapter1>())
					{
						device = new GorgonVideoDevice(adapter, VideoDeviceType.Software, index);

						PrintLog(device);

						// Get the outputs for the device.
						int outputCount = adapter.GetOutputCount();

						GetOutputs(device, D3DDevice, adapter, outputCount, outputCount == 0);
					}
				}
			}
			
			return device;
		}

#if DEBUG
		/// <summary>
		/// Function to add the reference device.
		/// </summary>
		/// <param name="index">Index of the device.</param>
        /// <returns>The video device used for reference software rendering.</returns>
		private static GorgonVideoDevice GetRefSoftwareDevice(int index)
		{
			GorgonVideoDevice device;

			using (var D3DDevice = new D3D.Device(D3DCommon.DriverType.Reference, D3D.DeviceCreationFlags.Debug))
			{
				using (var giDevice = D3DDevice.QueryInterface<Device1>())
                {
                    using (var adapter = giDevice.GetParent<Adapter1>())
					{
						device = new GorgonVideoDevice(adapter, VideoDeviceType.ReferenceRasterizer, index);

						PrintLog(device);

						int outputCount = adapter.GetOutputCount();
						GetOutputs(device, D3DDevice, adapter, outputCount, outputCount == 0);
					}
				}
			}

			return device;
		}
#endif
		#pragma warning restore 0618

		/// <summary>
		/// Function to print device log information.
		/// </summary>
		/// <param name="device">Device to print.</param>
		private static void PrintLog(GorgonVideoDevice device)
		{
			GorgonApplication.Log.Print(
				device.VideoDeviceType == VideoDeviceType.ReferenceRasterizer
					? "Device found: {0} ---> !!!** WARNING:  A reference rasterizer has very poor performance."
					: "Device found: {0}", LoggingLevel.Simple, device.Name);
			GorgonApplication.Log.Print("===================================================================", LoggingLevel.Verbose);
			GorgonApplication.Log.Print("Hardware feature level: {0}", LoggingLevel.Verbose, device.HardwareFeatureLevel);
			GorgonApplication.Log.Print("Limited to feature level: {0}", LoggingLevel.Verbose, device.SupportedFeatureLevel);
			GorgonApplication.Log.Print("Video memory: {0}", LoggingLevel.Verbose, device.DedicatedVideoMemory.FormatMemory());
			GorgonApplication.Log.Print("System memory: {0}", LoggingLevel.Verbose, device.DedicatedSystemMemory.FormatMemory());
			GorgonApplication.Log.Print("Shared memory: {0}", LoggingLevel.Verbose, device.SharedSystemMemory.FormatMemory());
			GorgonApplication.Log.Print("Device ID: 0x{0}", LoggingLevel.Verbose, device.DeviceID.FormatHex());
			GorgonApplication.Log.Print("Sub-system ID: 0x{0}", LoggingLevel.Verbose, device.SubSystemID.FormatHex());
			GorgonApplication.Log.Print("Vendor ID: 0x{0}", LoggingLevel.Verbose, device.VendorID.FormatHex());
			GorgonApplication.Log.Print("Revision: {0}", LoggingLevel.Verbose, device.Revision);
			GorgonApplication.Log.Print("Unique ID: 0x{0}", LoggingLevel.Verbose, device.UUID.FormatHex());
			GorgonApplication.Log.Print("===================================================================", LoggingLevel.Verbose);
		}
				
		/// <summary>
		/// Function to retrieve the video modes for the output.
		/// </summary>
		/// <param name="output">Output that owns the video modes.</param>
		/// <param name="D3DDevice">D3D device for filtering supported display modes.</param>
		/// <param name="giOutput">Output that contains the video modes.</param>
		private static void GetVideoModes(GorgonVideoOutput output, D3D.Device D3DDevice, Output giOutput)
		{
			var formats = (BufferFormat[])Enum.GetValues(typeof(BufferFormat));

			GorgonApplication.Log.Print("Retrieving video modes for output '{0}'...", LoggingLevel.Simple, output.Name);
			GorgonApplication.Log.Print("===================================================================", LoggingLevel.Verbose);

			// Test each format for display compatibility.
			foreach (var format in formats)
			{
				var giFormat = (Format)format;
				ModeDescription[] modes = giOutput.GetDisplayModeList(giFormat, DisplayModeEnumerationFlags.Scaling | DisplayModeEnumerationFlags.Interlaced);

				if ((modes == null) || (modes.Length <= 0))
				{
					continue;
				}

				GorgonVideoMode[] videoModes = (from mode in modes
				                                where (D3DDevice.CheckFormatSupport(giFormat) & D3D.FormatSupport.Display) == D3D.FormatSupport.Display
				                                select GorgonVideoMode.Convert(mode)).ToArray();

				if (videoModes.Length > 0)
				{
					output.VideoModes = new ReadOnlyCollection<GorgonVideoMode>(videoModes);
				}
			}

			// Output to log.
			foreach (var videoMode in output.VideoModes)
			{
				GorgonApplication.Log.Print("Mode: {0}x{1}, Format: {2}, Refresh Rate: {3}/{4}", LoggingLevel.Verbose, videoMode.Width, videoMode.Height, videoMode.Format, videoMode.RefreshRateNumerator, videoMode.RefreshRateDenominator);
			}

			GorgonApplication.Log.Print("===================================================================", LoggingLevel.Verbose);
			GorgonApplication.Log.Print("Found {0} video modes for output '{1}'.", LoggingLevel.Simple, output.VideoModes.Count, output.Name);
		}

		#pragma warning disable 0618
	    /// <summary>
	    /// Function to retrieve the list of outputs for the video device.
	    /// </summary>
	    /// <param name="adapter">Adapter containing the outputs.</param>
	    /// <param name="D3DDevice">D3D device to find closest matching mode.</param>
	    /// <param name="device">Device used to filter video modes that aren't supported.</param>
	    /// <param name="outputCount">The number of outputs attached to the device.</param>
	    /// <param name="noOutputDevice"><b>true</b> if the device has no outputs, <b>false</b> if it does.</param>
	    private static void GetOutputs(GorgonVideoDevice device, D3D.Device D3DDevice, Adapter adapter, int outputCount, bool noOutputDevice)
		{
			var outputs = new GorgonNamedObjectList<GorgonVideoOutput>(false);
			
			// We need to fake outputs.
			// Windows 8 does not support outputs on WARP devices and ref rasterizer devices.
			if ((noOutputDevice) || (SystemInformation.TerminalServerSession))
			{
				var output = new GorgonVideoOutput();

				GorgonApplication.Log.Print("Found output {0}.", LoggingLevel.Simple, output.Name);
				GorgonApplication.Log.Print("===================================================================", LoggingLevel.Verbose);
				GorgonApplication.Log.Print("Output bounds: ({0}x{1})-({2}x{3})", LoggingLevel.Verbose, output.OutputBounds.Left, output.OutputBounds.Top, output.OutputBounds.Right, output.OutputBounds.Bottom);
				GorgonApplication.Log.Print("Monitor handle: 0x{0}", LoggingLevel.Verbose, output.Handle.FormatHex());
				GorgonApplication.Log.Print("Attached to desktop: {0}", LoggingLevel.Verbose, output.IsAttachedToDesktop);
				GorgonApplication.Log.Print("Monitor rotation: {0}\u00B0", LoggingLevel.Verbose, output.Rotation);
				GorgonApplication.Log.Print("===================================================================", LoggingLevel.Verbose);

				outputs.Add(output);

				// No video modes for these devices.
				output.VideoModes = new GorgonVideoMode[0];

				device.Outputs = outputs;

				GorgonApplication.Log.Print("Output {0} on device {1} has no video modes.", LoggingLevel.Verbose, output.Name, device.Name);
				return;
			}

			// Get outputs.
			for (int i = 0; i < outputCount; i++)
			{
				using (Output giOutput = adapter.GetOutput(i))
				{
					var output = new GorgonVideoOutput(giOutput, device, i);

					ModeDescription findMode = GorgonVideoMode.Convert(new GorgonVideoMode(output.OutputBounds.Width, output.OutputBounds.Height, BufferFormat.R8G8B8A8_UIntNormal, 60, 1));
					ModeDescription result;

					// Get the default (desktop) video mode.
					giOutput.GetClosestMatchingMode(D3DDevice, findMode, out result);
					output.DefaultVideoMode = GorgonVideoMode.Convert(result);

					GetVideoModes(output, D3DDevice, giOutput);

					GorgonApplication.Log.Print("Found output {0}.", LoggingLevel.Simple, output.Name);
					GorgonApplication.Log.Print("===================================================================", LoggingLevel.Verbose);
					GorgonApplication.Log.Print("Output bounds: ({0}x{1})-({2}x{3})", LoggingLevel.Verbose, output.OutputBounds.Left, output.OutputBounds.Top, output.OutputBounds.Right, output.OutputBounds.Bottom);
					GorgonApplication.Log.Print("Monitor handle: 0x{0}", LoggingLevel.Verbose, output.Handle.FormatHex());
					GorgonApplication.Log.Print("Attached to desktop: {0}", LoggingLevel.Verbose, output.IsAttachedToDesktop);
					GorgonApplication.Log.Print("Monitor rotation: {0}\u00B0", LoggingLevel.Verbose, output.Rotation);
					GorgonApplication.Log.Print("===================================================================", LoggingLevel.Verbose);

					if (output.VideoModes.Count > 0)
					{
						outputs.Add(output);
					}
					else
					{
						GorgonApplication.Log.Print("Output {0} on device {1} has no video modes!", LoggingLevel.Verbose, output.Name, device.Name);
					}
				}
			}

			device.Outputs = outputs;
		}

		/// <summary>
		/// Function to perform an enumeration of the video devices attached to the system.
		/// </summary>
		/// <param name="enumerateWARPDevice"><b>true</b> to enumerate the WARP software device.  <b>false</b> to exclude it.</param>
		/// <param name="enumerateReferenceDevice"><b>true</b> to enumerate the reference device.  <b>false</b> to exclude it.</param>
		/// <remarks>This method will populate the <see cref="Gorgon.Graphics.GorgonVideoDeviceEnumerator">GorgonVideoDeviceEnumerator</see> with information about the video devices 
		/// installed in the system.
		/// <para>You may include the WARP device, which is a software based device that emulates most of the functionality of a video device, by setting the <paramref name="enumerateWARPDevice"/> to <b>true</b>.</para>
		/// <para>You may include the reference device, which is a software based device that all the functionality of a video device, by setting the <paramref name="enumerateReferenceDevice"/> to <b>true</b>.  
		/// If a reference device is used in rendering, the performance will be poor and as such, this device is only useful to diagnosing issues with video drivers.</para>
		/// <para>The reference device is a DEBUG only device, and as such, it will only appear under the following conditions:
		/// <list type="bullet">
		///		<item>
		///			<description>The DEBUG version of the Gorgon library is used.</description>
		///			<description>The Direct 3D SDK is installed.  The reference rasterizer is only included with the SDK.</description>
		///		</item>
		/// </list>
		/// </para>
		/// </remarks>
		public static void Enumerate(bool enumerateWARPDevice, bool enumerateReferenceDevice)
		{
#if DEBUG
			// Turn on object tracking if it's not already enabled.
			if (!DX.Configuration.EnableObjectTracking)
			{
				DX.Configuration.EnableObjectTracking = true;
			}
#endif
		    try
		    {
		        // Create the DXGI factory object used to gather the information.
		        if (Interlocked.Increment(ref _lockIncr) > 1)
		        {
		            return;
		        }

			    var devices = new GorgonNamedObjectList<GorgonVideoDevice>(false);

			    using(var factory = new Factory1())
		        {
		            int adapterCount = factory.GetAdapterCount1();

		            GorgonApplication.Log.Print("Enumerating video devices...", LoggingLevel.Simple);

		            // Begin gathering device information.
		            for (int i = 0; i < adapterCount; i++)
		            {
		                // Get the video device information.
		                using(var adapter = factory.GetAdapter1(i))
		                {
		                    // Only enumerate local devices.
		                    int outputCount = adapter.GetOutputCount();

			                if (((adapter.Description1.Flags & AdapterFlags.Remote) != 0) || (outputCount <= 0))
			                {
				                continue;
			                }

			                var videoDevice = new GorgonVideoDevice(adapter, VideoDeviceType.Hardware, i);

			                // Don't allow unsupported devices.
			                if (videoDevice.HardwareFeatureLevel == DeviceFeatureLevel.Unsupported)
			                {
				                continue;
			                }

			                // We create a D3D device here to filter out unsupported video modes from the format list.
			                using(var D3DDevice = new D3D.Device(adapter))
			                {
				                D3DDevice.DebugName = "Output enumerator device.";
				                PrintLog(videoDevice);

				                GetOutputs(videoDevice, D3DDevice, adapter, outputCount, false);

				                // Ensure we actually have outputs to use.
				                if (videoDevice.Outputs.Count > 0)
				                {
					                devices.Add(videoDevice);
				                }
				                else
				                {
					                GorgonApplication.Log.Print("Video device {0} has no outputs!",
					                                 LoggingLevel.Verbose, videoDevice.Name);
				                }
			                }
		                }
		            }

		            // Get software devices.
		            if (enumerateWARPDevice)
		            {
		                var device = GetWARPSoftwareDevice(devices.Count);

		                if (device.Outputs.Count > 0)
		                {
		                    devices.Add(device);
		                }
		            }

#if DEBUG
		            if (enumerateReferenceDevice)
		            {
		                var device = GetRefSoftwareDevice(devices.Count);

		                if (device.Outputs.Count > 0)
		                {
		                    devices.Add(device);
		                }
		            }
#endif
		        }

		        VideoDevices = devices;

                if (devices.Count == 0)
                {
	                throw new GorgonException(GorgonResult.CannotEnumerate, Resources.GORGFX_DEVICE_CANNOT_FIND_DEVICES);
                }

		        GorgonApplication.Log.Print("Found {0} video devices.", LoggingLevel.Simple, VideoDevices.Count);
		    }
		    finally
		    {
		        Interlocked.Decrement(ref _lockIncr);
		    }
		}
		#pragma warning restore 0618
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonVideoDeviceEnumerator"/> class.
		/// </summary>
		static GorgonVideoDeviceEnumerator()
		{
			VideoDevices = new GorgonNamedObjectList<GorgonVideoDevice>();
			Enumerate(false, false);
		}
		#endregion
	}
}
