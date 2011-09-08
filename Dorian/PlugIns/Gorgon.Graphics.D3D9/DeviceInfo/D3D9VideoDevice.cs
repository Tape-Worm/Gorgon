#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Thursday, July 21, 2011 3:41:06 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SlimDX;
using SlimDX.Direct3D9;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// A D3D9 video device.
	/// </summary>
	internal class D3D9VideoDevice
		: GorgonVideoDevice
	{
		#region Variables.
		private AdapterInformation _adapter = null;				// Adapter information.
		private Capabilities _caps = null;						// Adapter capabilities.
		private Direct3D _d3d = null;							// Direct 3D interface.
		private DeviceType _deviceType = DeviceType.Hardware;	// Device type.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D9 adapter index.
		/// </summary>
		public int AdapterIndex
		{
			get
			{
				return _caps.AdapterOrdinal;
			}
		}

		/// <summary>
		/// Property to return the index of the adapter head for the device.
		/// </summary>
		public int Head
		{
			get
			{
				return _caps.AdapterOrdinalInGroup;
			}
		}

		/// <summary>
		/// Property to return whether the device is hardware accelerated.
		/// </summary>
		public bool HWAccelerated
		{
			get
			{
				return (_caps.DeviceCaps & DeviceCaps.HWRasterization) == DeviceCaps.HWRasterization;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the device capabilities.
		/// </summary>
		/// <returns>
		/// A video device capabilities object.
		/// </returns>
		protected override GorgonVideoDeviceCapabilities CreateDeviceCapabilities()
		{
			return new D3D9VideoDeviceCapabilities(_d3d, _deviceType, _adapter, _caps);
		}

		/// <summary>
		/// Function to retrieve the outputs attached to the device.
		/// </summary>
		/// <returns>
		/// An enumerable list of video outputs.
		/// </returns>
		protected override IEnumerable<GorgonVideoOutput> GetOutputs()
		{
			MONITORINFOEX? monitorInfo = new MONITORINFOEX();

			// Get the primary output.
			monitorInfo = Win32API.GetMonitorInfo(_adapter.Monitor);

			Capabilities headCaps = null;
			List<D3D9VideoOutput> outputs = new List<D3D9VideoOutput>();

			MasterOutput = new D3D9VideoOutput(_d3d, _deviceType, _adapter, _caps.AdapterOrdinalInGroup, monitorInfo.Value);
			outputs.Add(MasterOutput as D3D9VideoOutput);
			
			// Get subordinate heads.
			if (_caps.NumberOfAdaptersInGroup > 0)
			{
			    foreach (var adapter in _d3d.Adapters)
				{
					// Skip the master.
					if (adapter.Adapter != _adapter.Adapter)
					{
						headCaps = adapter.GetCaps(_deviceType);

						// Ensure this head is on the correct device.
						if (headCaps.MasterAdapterOrdinal != AdapterIndex)
							continue;

						monitorInfo = Win32API.GetMonitorInfo(adapter.Monitor);
						if (monitorInfo != null)
							outputs.Add(new D3D9VideoOutput(_d3d, _deviceType, adapter, headCaps.AdapterOrdinalInGroup, monitorInfo.Value));
						else
							throw new GorgonException(GorgonResult.CannotEnumerate, "Could not enumerate the video outputs.  There was an error retrieving the monitor information.");
					}
				}
			}

			return outputs;
		}

		/// <summary>
		/// Function to return the highest multi sample/AA quality for a given MSAA level.
		/// </summary>
		/// <param name="level">The multi sample level to test.</param>
		/// <param name="format">Format to test for multi sampling capabilities.</param>
		/// <param name="windowed">TRUE if testing for windowed mode, FALSE if not.</param>
		/// <returns>
		/// The multi sample maximum quality and level supported, or NULL (Nothing in VB.Net) if not supported.
		/// </returns>
		public override int? GetMultiSampleQuality(GorgonMSAALevel level, GorgonBufferFormat format, bool windowed)
		{
			int quality = 0;

			if (_d3d.CheckDeviceMultisampleType(AdapterIndex, _deviceType, D3DConvert.Convert(format), windowed, D3DConvert.Convert(level), out quality))
				return quality;

			return null;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9VideoDevice"/> class.
		/// </summary>
		/// <param name="name">Name of the device.</param>
		/// <param name="index">Index of the driver in the collection.</param>
		/// <param name="d3d">Direct 3D interface.</param>
		/// <param name="deviceType">Device type to use.</param>
		/// <param name="adapter">D3D9 adapter.</param>
		/// <param name="capabilities">Adapter capabilities.</param>
		internal D3D9VideoDevice(string name, int index, Direct3D d3d, DeviceType deviceType, AdapterInformation adapter, Capabilities capabilities)
			: base(name, index)
		{
			if (d3d == null)
				throw new ArgumentNullException("d3d");
			if (adapter == null)
				throw new ArgumentNullException("adapter");
			if (capabilities == null)
				throw new ArgumentNullException("capabilities");

			_d3d = d3d;
			_adapter = adapter;
			_caps = capabilities;
			_deviceType = deviceType;

			DeviceID = _adapter.Details.DeviceId;
			DeviceName = _adapter.Details.DeviceName.Trim();
			DriverName = _adapter.Details.DriverName.Trim();
			DriverVersion = _adapter.Details.DriverVersion;
			Revision = _adapter.Details.Revision;
			SubSystemID = _adapter.Details.SubsystemId;
			VendorID = _adapter.Details.VendorId;
			DeviceGUID = _adapter.Details.DeviceIdentifier;
		}
		#endregion
	}
}
