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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DXGI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Collections.Specialized;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Enumerates information about the installed video devices on the system.
	/// </summary>
	/// <remarks>Use this to retrieve a list of video devices available on the system. A video device may be a discreet video card, or a device on the motherboard. 
	/// Retrieve the video device information by calling the <see cref="M:GorgonLibrary.Graphics.GorgonVideoDeviceEnumerator.Enumerate">Enumerate</see> method and the 
	/// <see cref="P:GorgonLibrary.Graphics.GorgonVideoDeviceEnumerator.VideoDevices">VideoDevices</see> property will be populated with video device information.  From 
	/// there you can pass a <see cref="GorgonLibrary.Graphics.GorgonVideoDevice">GorgonVideoDevice</see> object into the <see cref="GorgonLibrary.Graphics.GorgonGraphics">
	/// GorgonGraphics</see> constructor to use a specific video device.
	/// <para>This interface will allow enumeration of the WARP/Reference devices.  WARP is a high performance software device that will emulate much of the functionality 
	/// that a real video device would have. The reference device is a fully featured device, but is incredibly slow and useful when debugging driver issues.</para>
	/// </remarks>
	public static class GorgonVideoDeviceEnumerator
	{
		#region Variables.
		private static int _lockIncr = 0;		
		private static GorgonNamedObjectReadOnlyCollection<GorgonVideoDevice> _devices = null;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of available video devices installed on the system.
		/// </summary>
		public static GorgonNamedObjectReadOnlyCollection<GorgonVideoDevice> VideoDevices
		{
			get
			{
				return _devices;
			}
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
			GorgonVideoDevice device = null;

#if DEBUG
			using (var d3dDevice = new D3D.Device(SharpDX.Direct3D.DriverType.Warp, D3D.DeviceCreationFlags.Debug))
			{
#else
			using (var d3dDevice = new D3D.Device(SharpDX.Direct3D.DriverType.Warp, D3D.DeviceCreationFlags.None))
			{
#endif
				using (var giDevice = d3dDevice.QueryInterface<DXGI.Device1>())
				{
					using (var adapter = giDevice.GetParent<DXGI.Adapter1>())
					{
						device = new GorgonVideoDevice(adapter, VideoDeviceType.Software, index);

						PrintLog(device);

						// Get the outputs for the device.
						int outputCount = adapter.GetOutputCount();						
						GetOutputs(device, d3dDevice, adapter, outputCount);
					}
				}
			}
			
			return device;
		}

#if DEBUG
		/// <summary>
		/// Function to add the reference device.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		private static GorgonVideoDevice GetRefSoftwareDevice(int index)
		{
			GorgonVideoDevice device = null;

			using (var d3dDevice = new D3D.Device(SharpDX.Direct3D.DriverType.Reference, D3D.DeviceCreationFlags.Debug))
			{
				using (var giDevice = d3dDevice.QueryInterface<DXGI.Device1>())
				{
					using (var adapter = giDevice.QueryInterface<DXGI.Adapter1>())
					{
						device = new GorgonVideoDevice(adapter, VideoDeviceType.ReferenceRasterizer, index);
						
						PrintLog(device);

						int outputCount = adapter.GetOutputCount();
						GetOutputs(device, d3dDevice, adapter, outputCount);
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
			if (device.VideoDeviceType == VideoDeviceType.ReferenceRasterizer)
				Gorgon.Log.Print("Device found: {0} ---> !!!** WARNING:  A reference rasterizer has very poor performance.", Diagnostics.LoggingLevel.Simple, device.Name);
			else
				Gorgon.Log.Print("Device found: {0}", Diagnostics.LoggingLevel.Simple, device.Name);
			Gorgon.Log.Print("===================================================================", Diagnostics.LoggingLevel.Verbose);
			Gorgon.Log.Print("Hardware feature level: {0}", Diagnostics.LoggingLevel.Verbose, device.HardwareFeatureLevel);
			Gorgon.Log.Print("Limited to feature level: {0}", Diagnostics.LoggingLevel.Verbose, device.SupportedFeatureLevel);
			Gorgon.Log.Print("Video memory: {0}", Diagnostics.LoggingLevel.Verbose, device.DedicatedVideoMemory.FormatMemory());
			Gorgon.Log.Print("System memory: {0}", Diagnostics.LoggingLevel.Verbose, device.DedicatedSystemMemory.FormatMemory());
			Gorgon.Log.Print("Shared memory: {0}", Diagnostics.LoggingLevel.Verbose, device.SharedSystemMemory.FormatMemory());
			Gorgon.Log.Print("Device ID: 0x{0}", Diagnostics.LoggingLevel.Verbose, device.DeviceID.FormatHex());
			Gorgon.Log.Print("Sub-system ID: 0x{0}", Diagnostics.LoggingLevel.Verbose, device.SubSystemID.FormatHex());
			Gorgon.Log.Print("Vendor ID: 0x{0}", Diagnostics.LoggingLevel.Verbose, device.VendorID.FormatHex());
			Gorgon.Log.Print("Revision: {0}", Diagnostics.LoggingLevel.Verbose, device.Revision);
			Gorgon.Log.Print("Unique ID: 0x{0}", Diagnostics.LoggingLevel.Verbose, device.UUID.FormatHex());
			Gorgon.Log.Print("===================================================================", Diagnostics.LoggingLevel.Verbose);
		}
				
		/// <summary>
		/// Function to retrieve the video modes for the output.
		/// </summary>
		/// <param name="output">Output that owns the video modes.</param>
		/// <param name="d3dDevice">D3D device for filtering supported display modes.</param>
		/// <param name="giOutput">Output that contains the video modes.</param>
		private static void GetVideoModes(GorgonVideoOutput output, D3D.Device d3dDevice, DXGI.Output giOutput)
		{
			BufferFormat[] formats = (BufferFormat[])Enum.GetValues(typeof(BufferFormat));
			GorgonVideoMode[] videoModes = null;

			Gorgon.Log.Print("Retrieving video modes for output '{0}'...", Diagnostics.LoggingLevel.Simple, output.Name);
			Gorgon.Log.Print("===================================================================", Diagnostics.LoggingLevel.Verbose);

			// Test each format for display compatibility.
			foreach (var format in formats)
			{
				DXGI.Format giFormat = (DXGI.Format)format;
				DXGI.ModeDescription[] modes = giOutput.GetDisplayModeList(giFormat, DXGI.DisplayModeEnumerationFlags.Scaling | DXGI.DisplayModeEnumerationFlags.Interlaced);

				if ((modes != null) && (modes.Length > 0))
				{
					videoModes = (from mode in modes
								 where (d3dDevice.CheckFormatSupport(giFormat) & D3D.FormatSupport.Display) == D3D.FormatSupport.Display
								 select GorgonVideoMode.Convert(mode)).ToArray();

					if ((videoModes != null) && (videoModes.Length > 0))
					{
						output.VideoModes = new ReadOnlyCollection<GorgonVideoMode>(videoModes);
					}
				}
			}

			// Output to log.
			foreach (var videoMode in output.VideoModes)
			{
				Gorgon.Log.Print("Mode: {0}x{1}, Format: {2}, Refresh Rate: {3}/{4}", Diagnostics.LoggingLevel.Verbose, videoMode.Width, videoMode.Height, videoMode.Format, videoMode.RefreshRateNumerator, videoMode.RefreshRateDenominator);
			}

			Gorgon.Log.Print("===================================================================", Diagnostics.LoggingLevel.Verbose);
			Gorgon.Log.Print("Found {0} video modes for output '{1}'.", Diagnostics.LoggingLevel.Simple, output.VideoModes.Count, output.Name);
		}

		#pragma warning disable 0618
		/// <summary>
		/// Function to retrieve the list of outputs for the video device.
		/// </summary>
		/// <param name="adapter">Adapter containing the outputs.</param>
		/// <param name="d3ddevice">D3D device to find closest matching mode.</param>
		/// <param name="device">Device used to filter video modes that aren't supported.</param>
		/// <param name="outputCount">The number of outputs attached to the device.</param>
		private static void GetOutputs(GorgonVideoDevice device, D3D.Device d3ddevice, DXGI.Adapter1 adapter, int outputCount)
		{
			List<GorgonVideoOutput> outputs = new List<GorgonVideoOutput>(outputCount);

			// Get outputs.
			for (int i = 0; i < outputCount; i++)
			{
				using (DXGI.Output giOutput = adapter.GetOutput(i))
				{
					GorgonVideoOutput output = new GorgonVideoOutput(giOutput, device, i);

					DXGI.ModeDescription findMode = GorgonVideoMode.Convert(new GorgonVideoMode(output.OutputBounds.Width, output.OutputBounds.Height, BufferFormat.R8G8B8A8_UIntNormal, 60, 1));
					DXGI.ModeDescription result = default(DXGI.ModeDescription);

					// Get the default (desktop) video mode.
					giOutput.GetClosestMatchingMode(d3ddevice, findMode, out result);
					output.DefaultVideoMode = GorgonVideoMode.Convert(result);

					GetVideoModes(output, d3ddevice, giOutput);

					Gorgon.Log.Print("Found output {0}.", Diagnostics.LoggingLevel.Simple, output.Name);
					Gorgon.Log.Print("===================================================================", Diagnostics.LoggingLevel.Verbose);
					Gorgon.Log.Print("Output bounds: ({0}x{1})-({2}x{3})", Diagnostics.LoggingLevel.Verbose, output.OutputBounds.Left, output.OutputBounds.Top, output.OutputBounds.Right, output.OutputBounds.Bottom);
					Gorgon.Log.Print("Monitor handle: 0x{0}", Diagnostics.LoggingLevel.Verbose, output.Handle.FormatHex());
					Gorgon.Log.Print("Attached to desktop: {0}", Diagnostics.LoggingLevel.Verbose, output.IsAttachedToDesktop);
					Gorgon.Log.Print("Monitor rotation: {0}\u00B0", Diagnostics.LoggingLevel.Verbose, output.Rotation);
					Gorgon.Log.Print("===================================================================", Diagnostics.LoggingLevel.Verbose);

					if (output.VideoModes.Count > 0)
					{
						outputs.Add(output);
					}
					else
					{
						Gorgon.Log.Print("Output {0} on device {1} has no video modes!", Diagnostics.LoggingLevel.Verbose, output.Name, device.Name);
					}
				}
			}

			device.Outputs = new GorgonNamedObjectReadOnlyCollection<GorgonVideoOutput>(false, outputs);
		}

		/// <summary>
		/// Function to perform an enumeration of the video devices attached to the system.
		/// </summary>
		/// <param name="enumerateWARPDevice">TRUE to enumerate the WARP software device.  FALSE to exclude it.</param>
		/// <param name="enumerateREFDevice">TRUE to enumerate the reference device.  FALSE to exclude it.</param>
		/// <remarks>This method will populate the <see cref="P:GorgonLibrary.Graphics.VideoDevices">VideoDevices</see> list with information about the video devices 
		/// installed in the system.
		/// <para>You may include the WARP device, which is a software based device that emulates most of the functionality of a video device, by setting the <paramref name="enumerateWARPDevice"/> to TRUE.</para>
		/// <para>You may include the reference device, which is a software based device that all the functionality of a video device, by setting the <paramref name="enumerateREFDevice"/> to TRUE.  
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
		public static void Enumerate(bool enumerateWARPDevice, bool enumerateREFDevice)
		{
			List<GorgonVideoDevice> devices = null;

#if DEBUG
			// Turn on object tracking if it's not already enabled.
			if (!SharpDX.Configuration.EnableObjectTracking)
			{
				SharpDX.Configuration.EnableObjectTracking = true;
			}
#endif
		    try
		    {
		        // Create the DXGI factory object used to gather the information.
		        if (Interlocked.Increment(ref _lockIncr) > 1)
		        {
		            return;
		        }

		        using(var factory = new DXGI.Factory1())
		        {
		            int adapterCount = factory.GetAdapterCount1();

		            devices = new List<GorgonVideoDevice>(adapterCount + 2);

		            Gorgon.Log.Print("Enumerating video devices...", Diagnostics.LoggingLevel.Simple);

		            // Begin gathering device information.
		            for (int i = 0; i < adapterCount; i++)
		            {
		                // Get the video device information.
		                using(var adapter = factory.GetAdapter1(i))
		                {
		                    // Only enumerate local devices.
		                    int outputCount = adapter.GetOutputCount();

		                    if (((adapter.Description1.Flags & DXGI.AdapterFlags.Remote) == 0) && (outputCount > 0))
		                    {
		                        var videoDevice = new GorgonVideoDevice(adapter, VideoDeviceType.Hardware, i);

		                        // Don't allow unsupported devices.
		                        if (videoDevice.HardwareFeatureLevel == DeviceFeatureLevel.Unsupported)
		                        {
		                            continue;
		                        }

		                        // We create a D3D device here to filter out unsupported video modes from the format list.
		                        using(var d3dDevice = new D3D.Device(adapter))
		                        {
		                            d3dDevice.DebugName = "Output enumerator device.";
		                            PrintLog(videoDevice);

		                            GetOutputs(videoDevice, d3dDevice, adapter, outputCount);

		                            // Ensure we actually have outputs to use.
		                            if (videoDevice.Outputs.Count > 0)
		                            {
		                                devices.Add(videoDevice);
		                            }
		                            else
		                            {
		                                Gorgon.Log.Print("Video device {0} has no outputs!",
		                                                    Diagnostics.LoggingLevel.Verbose, videoDevice.Name);
		                            }
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
		            if (enumerateREFDevice)
		            {
		                var device = GetRefSoftwareDevice(devices.Count);

		                if (device.Outputs.Count > 0)
		                {
		                    devices.Add(device);
		                }
		            }
#endif
		        }

		        _devices = new GorgonNamedObjectReadOnlyCollection<GorgonVideoDevice>(false, devices);

                if (devices.Count == 0)
                {
                    throw new GorgonException(GorgonResult.CannotEnumerate, "Could not find any supported video devices.  Gorgon requires a device that can support a minimum of pixel shader model 2b and a vertex shader model of 2a.");
                }

		        Gorgon.Log.Print("Found {0} video devices.", Diagnostics.LoggingLevel.Simple, _devices.Count);
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
			_devices = new GorgonNamedObjectReadOnlyCollection<GorgonVideoDevice>(false, new GorgonVideoDevice[] { });
			Enumerate(false, false);
		}
		#endregion
	}
}
