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
using SlimDX.Direct3D9;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// A D3D9 video device.
	/// </summary>
	public class D3D9VideoDevice
		: GorgonVideoDevice
	{
		#region Variables.
		private AdapterInformation _adapter = null;		// Adapter information.
		private Capabilities _caps = null;				// Adapter capabilities.
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
		/// An enumerable list of driver capabilities.
		/// </returns>
		protected override IEnumerable<KeyValuePair<string, string>> GetDeviceCapabilities()
		{
			IDictionary<string, string> deviceCaps = null;

			deviceCaps = new SortedList<string, string>();

			VertexShaderVersion = _caps.VertexShaderVersion;
			PixelShaderVersion = _caps.PixelShaderVersion;
						
			deviceCaps.Add("D3D_AdapterIndex", _caps.AdapterOrdinal.ToString());
			deviceCaps.Add("D3D_HeadIndex", _caps.AdapterOrdinalInGroup.ToString());
			deviceCaps.Add("D3D_DeviceID", "0x" + GorgonUtility.FormatHex(_adapter.Details.DeviceId));
			deviceCaps.Add("D3D_DeviceName", _adapter.Details.DeviceName.ToString());
			deviceCaps.Add("D3D_DriverName", _adapter.Details.DriverName.ToString());
			deviceCaps.Add("D3D_DriverVersion", _adapter.Details.DriverVersion.ToString());
			deviceCaps.Add("D3D_Revision", _adapter.Details.Revision.ToString());
			deviceCaps.Add("D3D_SubSystemID", "0x" + GorgonUtility.FormatHex(_adapter.Details.SubsystemId));
			deviceCaps.Add("D3D_VendorID", "0x" + GorgonUtility.FormatHex(_adapter.Details.VendorId));
			deviceCaps.Add("D3D_GUID", _adapter.Details.DeviceIdentifier.ToString());

			var caps = _caps.GetType().GetProperties().Where(item => !item.Name.StartsWith("AdapterOrdinal"));

			// Extract device capabilities.
			foreach (var cap in caps)
			{
				if ((cap.PropertyType.IsEnum) || (cap.PropertyType == typeof(string)) || (cap.PropertyType.IsPrimitive) || (cap.PropertyType == typeof(Version)))
					deviceCaps.Add("D3D9Cap_" + cap.Name, cap.GetValue(_caps, null).ToString());
				else
				{
					if (cap.PropertyType.IsValueType)
					{
						object value = cap.GetValue(_caps, null);
						var subCap = value.GetType().GetProperties();

						foreach (var sub in subCap)
							deviceCaps.Add("D3D9Cap_" + cap.Name + "." + sub.Name, sub.GetValue(value, null).ToString());
					}
				}
			}

			var capInfo = deviceCaps.Where(item => item.Key.StartsWith("D3D_"));
			Gorgon.Log.Print("Device Info:", Diagnostics.GorgonLoggingLevel.Intermediate);
			foreach (var item in capInfo)
				Gorgon.Log.Print("\t{0}: {1}", Diagnostics.GorgonLoggingLevel.Intermediate, item.Key, item.Value);

			capInfo = deviceCaps.Where(item => item.Key.StartsWith("D3D9Cap_"));
			Gorgon.Log.Print("Device Capabilities:", Diagnostics.GorgonLoggingLevel.Verbose);
			foreach (var item in capInfo)
				Gorgon.Log.Print("\t{0}: {1}", Diagnostics.GorgonLoggingLevel.Verbose, item.Key, item.Value);
			return deviceCaps;			
		}

		/// <summary>
		/// Function to retrieve the outputs attached to the device.
		/// </summary>
		/// <returns>
		/// An enumerable list of video outputs.
		/// </returns>
		protected override IEnumerable<GorgonVideoOutput> GetOutputs()
		{
			List<D3D9VideoOutput> outputs = new List<D3D9VideoOutput>();
			MONITORINFOEX monitorInfo = new MONITORINFOEX(0);

			/*if (Win32API.GetMonitorInfo(_adapter.Monitor, ref monitorInfo))
			{

			}*/

			return outputs;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9VideoDevice"/> class.
		/// </summary>
		/// <param name="adapter">D3D9 adapter.</param>
		/// <param name="capabilities">Adapter capabilities.</param>
		/// <param name="index">Index of the driver in the collection.</param>
		internal D3D9VideoDevice(AdapterInformation adapter, Capabilities capabilities, int index)
			: base(index)
		{
			if (adapter == null)
				throw new ArgumentNullException("adapter");
			if (capabilities == null)
				throw new ArgumentNullException("capabilities");

			_adapter = adapter;
			_caps = capabilities;
			Name = _adapter.Details.Description;			

			Gorgon.Log.Print("Video Device #{0}: {1}", Diagnostics.GorgonLoggingLevel.Simple, index, adapter.Details.Description);
		}
		#endregion
	}
}
