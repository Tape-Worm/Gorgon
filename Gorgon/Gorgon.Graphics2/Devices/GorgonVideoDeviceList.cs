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
using System.Linq;
using System.Windows.Forms;
using Gorgon.Collections;
using Gorgon.Collections.Specialized;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using D3DCommon = SharpDX.Direct3D;
using D3D12 = SharpDX.Direct3D12;
using System.Collections;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Functionality to retrieve information about the installed video devices on the system.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Use this to retrieve a list of video devices available on the system. A video device may be a discreet video card, or a device on the motherboard.
	/// </para>
	/// <para>
	/// This list will contain <see cref="IGorgonVideoDeviceInfo"/> objects which can then be passed to a <see cref="GorgonGraphics"/> instance. This allows applications or users to pick and choose which device 
	/// they wish to use.
	/// </para>
	/// <para>
	/// This list will also allow enumeration of the WARP/Reference devices.  WARP is a high performance software device that will emulate much of the functionality that a real video device would have. The 
	/// reference device is a fully featured device for debugging driver issues.
	/// </para>
	/// </remarks>
	public class GorgonVideoDeviceList
		: IGorgonNamedObjectReadOnlyList<IGorgonVideoDeviceInfo>
	{
		#region Variables.
		// The backing store for the device list.
		private readonly List<IGorgonVideoDeviceInfo> _devices = new List<IGorgonVideoDeviceInfo>();
		// Log used for debugging info.
		private readonly IGorgonLog _log;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the keys are case sensitive.
		/// </summary>
		public bool KeysAreCaseSensitive => false;

		/// <summary>
		/// Gets the number of elements in the collection.
		/// </summary>
		/// <returns>
		/// The number of elements in the collection. 
		/// </returns>
		public int Count => _devices.Count;

		/// <summary>
		/// Gets the element at the specified index in the read-only list.
		/// </summary>
		/// <returns>
		/// The element at the specified index in the read-only list.
		/// </returns>
		/// <param name="index">The zero-based index of the element to get. </param>
		public IGorgonVideoDeviceInfo this[int index] => _devices[index];

		/// <summary>
		/// Property to return an item in this list by its name.
		/// </summary>
		public IGorgonVideoDeviceInfo this[string name]
		{
			get
			{
				int index = IndexOf(name);

				if (index == -1)
				{
					throw new KeyNotFoundException(string.Format(Resources.GORGFX_ERR_NO_VIDEO_DEVICE_WITH_NAME, name));
				}

				return _devices[index];
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to print device log information.
		/// </summary>
		/// <param name="device">Device to print.</param>
		private void PrintLog(IGorgonVideoDeviceInfo device)
		{
			_log.Print($"Device found: {device.Name}", LoggingLevel.Simple);
			_log.Print("===================================================================", LoggingLevel.Simple);
			_log.Print($"Supported feature level: {device.SupportedFeatureLevel}", LoggingLevel.Simple);
			_log.Print($"Video memory: {device.DedicatedVideoMemory.FormatMemory()}", LoggingLevel.Simple);
			_log.Print($"System memory: {device.DedicatedSystemMemory.FormatMemory()}", LoggingLevel.Intermediate);
			_log.Print($"Shared memory: {device.SharedSystemMemory.FormatMemory()}", LoggingLevel.Intermediate);
			_log.Print($"Device ID: 0x{device.DeviceID.FormatHex()}", LoggingLevel.Verbose);
			_log.Print($"Sub-system ID: 0x{device.SubSystemID.FormatHex()}", LoggingLevel.Verbose);
			_log.Print($"Vendor ID: 0x{device.VendorID.FormatHex()}", LoggingLevel.Verbose);
			_log.Print($"Revision: {device.Revision}", LoggingLevel.Verbose);
			_log.Print($"Unique ID: 0x{device.Luid.FormatHex()}", LoggingLevel.Verbose);
			_log.Print($"Graphics Preemption Granularity: {device.GraphicsPreemptionGranularity}", LoggingLevel.Verbose);
			_log.Print($"Compute Preemption Granularity: {device.ComputePreemptionGranularity}", LoggingLevel.Verbose);
			_log.Print("===================================================================", LoggingLevel.Simple);

			foreach (IGorgonVideoOutputInfo output in device.Outputs)
			{
				_log.Print($"Found output '{output.Name}'.", LoggingLevel.Simple);
				_log.Print("===================================================================", LoggingLevel.Verbose);
				_log.Print($"Output bounds: ({output.DesktopBounds.Left}x{output.DesktopBounds.Top})-({output.DesktopBounds.Right}x{output.DesktopBounds.Bottom})",
				           LoggingLevel.Verbose);
				_log.Print($"Monitor handle: 0x{output.MonitorHandle.FormatHex()}", LoggingLevel.Verbose);
				_log.Print($"Attached to desktop: {output.IsAttachedToDesktop}", LoggingLevel.Verbose);
				_log.Print($"Monitor rotation: {output.Rotation}", LoggingLevel.Verbose);
				_log.Print("===================================================================", LoggingLevel.Simple);

				_log.Print($"Retrieving video modes for output '{output.Name}'...", LoggingLevel.Simple);
				_log.Print("===================================================================", LoggingLevel.Simple);

				foreach (GorgonVideoMode mode in output.VideoModes)
				{
					_log.Print($"Size: {mode.Width}x{mode.Height}, Format: {mode.Format}, Refresh Rate: {mode.RefreshRate.Numerator}/{mode.RefreshRate.Denominator}, " +
					           $"Scaling: {mode.Scaling}, Scanline order: {mode.ScanlineOrdering}, Is stereo: {mode.Stereo}",
					           LoggingLevel.Verbose);
				}

				_log.Print("===================================================================", LoggingLevel.Verbose);
				_log.Print($"Found {output.VideoModes.Count} video modes for output '{output.Name}'.", LoggingLevel.Simple);
				_log.Print("===================================================================", LoggingLevel.Simple);
			}
		}

		/// <summary>
		/// Function to determine if a device supports a specific display format.
		/// </summary>
		/// <param name="D3DDevice">The device to check.</param>
		/// <param name="displayModeFormat">The format to check.</param>
		/// <returns><b>true</b> if supported, <b>false</b> if not.</returns>
		private static bool SupportsDisplayMode(D3D12.Device D3DDevice, DXGI.Format displayModeFormat)
		{
			var formatSupport = new D3D12.FeatureDataFormatSupport
			{
				Format = displayModeFormat
			};

			if (!D3DDevice.CheckFeatureSupport(D3D12.Feature.FormatSupport, ref formatSupport))
			{
				return false;
			}

			return (formatSupport.Support1 & D3D12.FormatSupport1.Display) == D3D12.FormatSupport1.Display;
		}

		/// <summary>
		/// Function to retrieve the video modes for the output.
		/// </summary>
		/// <param name="D3DDevice">D3D device for filtering supported display modes.</param>
		/// <param name="giOutput">Output that contains the video modes.</param>
		/// <returns>The list of video modes for the device and output.</returns>
		private static IReadOnlyList<GorgonVideoMode> GetVideoModes(D3D12.Device D3DDevice, DXGI.Output4 giOutput)
		{
			var formats = (BufferFormat[])Enum.GetValues(typeof(BufferFormat));
			var result = new List<GorgonVideoMode>();

			// Test each format for display compatibility.
			foreach (var format in formats)
			{
				var giFormat = (DXGI.Format)format;
				DXGI.ModeDescription1[] modes = giOutput.GetDisplayModeList1(giFormat,
				                                                             DXGI.DisplayModeEnumerationFlags.Scaling | DXGI.DisplayModeEnumerationFlags.Interlaced);

				if ((modes == null) || (modes.Length <= 0))
				{
					continue;
				}

				result.AddRange(from mode in modes
				                where SupportsDisplayMode(D3DDevice, mode.Format)
				                select new GorgonVideoMode(mode));
			}

			return result;
		}

		/// <summary>
		/// Function to retrieve the list of outputs for the video device.
		/// </summary>
		/// <param name="adapter">Adapter containing the outputs.</param>
		/// <param name="D3DDevice">D3D device to find closest matching mode.</param>
		/// <param name="outputCount">The number of outputs attached to the device.</param>
		private IGorgonNamedObjectReadOnlyList<IGorgonVideoOutputInfo> GetOutputs(D3D12.Device D3DDevice, DXGI.Adapter3 adapter, int outputCount)
		{
			var result = new GorgonNamedObjectList<IGorgonVideoOutputInfo>(false);

			// Devices created under RDP/TS do not support output selection.
			if (SystemInformation.TerminalServerSession)
			{
				_log.Print("Devices under terminal services and WARP devices devices do not use outputs, no outputs enumerated.", LoggingLevel.Verbose);
				return result;
			}

			// Get outputs.
			for (int i = 0; i < outputCount; i++)
			{
				using (DXGI.Output giOutput = adapter.GetOutput(i))
				{
					using (DXGI.Output4 giOutput4 = giOutput.QueryInterface<DXGI.Output4>())
					{
						IReadOnlyList<GorgonVideoMode> modes = GetVideoModes(D3DDevice, giOutput4);
						var output = new VideoOutputInfo(i, giOutput4, modes);
						result.Add(output);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Function to create a new D3D 12 device object with the specified adapter.
		/// </summary>
		/// <param name="adapter">The adapter to use when creating the device object.</param>
		/// <returns>The device object if D3D 12 is supported by the adapter, or null if not.</returns>
		private D3D12.Device CreateDevice(DXGI.Adapter3 adapter)
		{
			try
			{
				// This sucks, I -hate- using exceptions for control flow, but we've got no other option 
				// here because there is no other way to determine if the device supports Direct 3D 12.
				return new D3D12.Device(adapter, D3DCommon.FeatureLevel.Level_11_0);
			}
			catch (DX.SharpDXException)
			{
				_log.Print($"WARNING: The device '{adapter.Description2.Description.Replace("\0", string.Empty)}' is not compatible with Direct 3D 12.",
				           LoggingLevel.Simple);
				return null;
			}
		}

		/// <summary>
		/// Function to retrieve the maximum supported feature level for a video device.
		/// </summary>
		/// <param name="device">The device to query.</param>
		/// <returns>The maximum supported feature level for the device.</returns>
		private static unsafe DeviceFeatureLevel GetFeatureLevel(D3D12.Device device)
		{
			D3DCommon.FeatureLevel* requestedLevels = stackalloc D3DCommon.FeatureLevel[4];
			requestedLevels[0] = D3DCommon.FeatureLevel.Level_12_1;
			requestedLevels[1] = D3DCommon.FeatureLevel.Level_12_0;
			requestedLevels[2] = D3DCommon.FeatureLevel.Level_11_1;
			requestedLevels[3] = D3DCommon.FeatureLevel.Level_11_0;

			D3D12.FeatureDataFeatureLevels featureLevels = new D3D12.FeatureDataFeatureLevels
			                                               {
				                                               FeatureLevelCount = 4,
				                                               FeatureLevelsRequestedPointer = new IntPtr(requestedLevels),
				                                               MaxSupportedFeatureLevel = D3DCommon.FeatureLevel.Level_9_1
			                                               };

			// If this fails then continue with enumeration for other devices.
			if (!device.CheckFeatureSupport(D3D12.Feature.FeatureLevels, ref featureLevels))
			{
				return DeviceFeatureLevel.Unsupported;
			}

			// If any other value than what we support is returned, then keep the feature level at unsupported.
			if (!Enum.IsDefined(typeof(DeviceFeatureLevel), (int)featureLevels.MaxSupportedFeatureLevel))
			{
				return DeviceFeatureLevel.Unsupported;
			}

			return (DeviceFeatureLevel)featureLevels.MaxSupportedFeatureLevel;
		}

		/// <summary>
		/// Function to return whether an item with the specified name exists in this collection.
		/// </summary>
		/// <param name="name">Name of the item to find.</param>
		/// <returns><b>true</b> if found, <b>false</b> if not.</returns>
		public bool Contains(string name) => IndexOf(name) != -1;

		/// <summary>
		/// Determines the index of a specific item in the list.
		/// </summary>
		/// <param name="name">Name of the item to find.</param>
		/// <returns>
		/// The index of <paramref name="name"/> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf(string name)
		{
			for (int i = 0; i < _devices.Count; ++i)
			{
				if (string.Equals(name, _devices[i].Name, StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Determines the index of a specific item in the list.
		/// </summary>
		/// <param name="item">The object to locate in the list.</param>
		/// <returns>
		/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf(IGorgonVideoDeviceInfo item) => _devices.IndexOf(item);

		/// <summary>
		/// Determines whether the list contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the list.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in the list; otherwise, false.
		/// </returns>
		public bool Contains(IGorgonVideoDeviceInfo item) => IndexOf(item) != -1;

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// An enumerator that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<IGorgonVideoDeviceInfo> GetEnumerator() => _devices.GetEnumerator();

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_devices).GetEnumerator();
		}

		/// <summary>
		/// Function to perform an enumeration of the video devices attached to the system and populate this list.
		/// </summary>
		/// <param name="enumerateWARPDevice">[Optional] <b>true</b> to enumerate the WARP software device.  <b>false</b> to exclude it.</param>
		/// <remarks>
		/// <para>
		/// Use this method to populate this list with information about the video devices installed in the system.
		/// </para>
		/// <para>
		/// You may include the WARP device, which is a software based device that emulates most of the functionality of a video device, by setting the <paramref name="enumerateWARPDevice"/> to <b>true</b>.
		/// </para>
		/// </remarks>
		public void Enumerate(bool enumerateWARPDevice = false)
		{
			using (DXGI.Factory4 factory4 = new DXGI.Factory4())
			{
				_devices.Clear();

				int adapterCount = factory4.GetAdapterCount1();

				_log.Print("Enumerating video devices...", LoggingLevel.Simple);

				// Begin gathering device information.
				for (int i = 0; i < adapterCount; i++)
				{
					// Get the video device information.
					DXGI.Adapter3 adapter3;
					DXGI.Adapter1 adapter1 = factory4.GetAdapter1(i);
					using (adapter3 = adapter1.QueryInterface<DXGI.Adapter3>())
					{
						// Only count hardware devices in this loop.
						if (((adapter3.Description2.Flags & DXGI.AdapterFlags.Remote) != 0)
						    || ((adapter3.Description2.Flags & DXGI.AdapterFlags.Software) != 0))
						{
							continue;
						}

						// We create a D3D device here to filter out unsupported video modes from the format list.
						using (var D3DDevice = CreateDevice(adapter3))
						{
							// If no device was created then we don't support D3D 12 on this adapter.
							if (D3DDevice == null)
							{
								continue;
							}

							DeviceFeatureLevel featureLevel = GetFeatureLevel(D3DDevice);

							// Don't allow unsupported devices.
							if (featureLevel == DeviceFeatureLevel.Unsupported)
							{
								continue;
							}

							D3DDevice.Name = "Output enumerator device.";

							IGorgonNamedObjectReadOnlyList<IGorgonVideoOutputInfo> outputs = GetOutputs(D3DDevice, adapter3, adapter3.GetOutputCount());

							// Ensure we actually have outputs to use.
							if (outputs.Count <= 0)
							{
								_log.Print($"WARNING: Video device {adapter3.Description2.Description.Replace("\0", string.Empty)} has no outputs. Full screen mode will not be possible.", LoggingLevel.Verbose);
							}

							var videoDevice = new VideoDeviceInfo(i, adapter3, featureLevel, outputs, VideoDeviceType.Hardware);
							_devices.Add(videoDevice);
							PrintLog(videoDevice);
						}
					}
				}

				// Get software devices.
				if (enumerateWARPDevice)
				{
					using (DXGI.Adapter warpAdapter = factory4.GetWarpAdapter())
					{
						using (DXGI.Adapter3 adapter3 = warpAdapter.QueryInterface<DXGI.Adapter3>())
						{
							using (D3D12.Device device = CreateDevice(adapter3))
							{
								if (device != null)
								{
									device.Name = "Output enumerator device.";

									var videoDevice = new VideoDeviceInfo(_devices.Count,
									                                      adapter3,
									                                      GetFeatureLevel(device),
									                                      new GorgonNamedObjectList<IGorgonVideoOutputInfo>(),
									                                      VideoDeviceType.Software);
									_devices.Add(videoDevice);
									PrintLog(videoDevice);
								}
							}
						}
					}
				}
			}

			_log.Print($"Found {_devices.Count} video devices.", LoggingLevel.Simple);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonVideoDeviceList"/> class.
		/// </summary>
		/// <param name="log">[Optional] The log used to capture debugging information.</param>
		public GorgonVideoDeviceList(IGorgonLog log = null)
		{
			_log = log ?? GorgonLogDummy.DefaultInstance;
		}
		#endregion
	}
}
