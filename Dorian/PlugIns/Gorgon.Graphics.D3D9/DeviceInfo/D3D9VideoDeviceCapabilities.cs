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
// Created: Tuesday, July 26, 2011 8:30:31 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// Direct 3D9 video device capabilities.
	/// </summary>
	internal class D3D9VideoDeviceCapabilities
		: GorgonVideoDeviceCapabilities
	{
		#region Variables.
		private AdapterInformation _info = null;				// Adapter information.
		private Capabilities _caps = null;						// Capabilities.
		private Direct3D _d3d = null;							// Direct 3D interface.
		private DeviceType _deviceType = DeviceType.Hardware;	// Device type.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to enumerate the device capabilities.
		/// </summary>
		/// <returns>A dictionary with the device capabilities.</returns>
		private Dictionary<string, object> EnumerateCaps()
		{
			Dictionary<string, object> deviceCaps = new Dictionary<string, object>();

			var caps = _caps.GetType().GetProperties();

			// Extract device capabilities.
			foreach (var cap in caps)
			{
				if ((cap.PropertyType.IsEnum) || (cap.PropertyType == typeof(string)) || (cap.PropertyType.IsPrimitive) || (cap.PropertyType == typeof(Version)))
					deviceCaps.Add("D3D9Cap_" + cap.Name, cap.GetValue(_caps, null));
				else
				{
					if (cap.PropertyType.IsValueType)
					{
						object value = cap.GetValue(_caps, null);
						var subCap = value.GetType().GetProperties();

						foreach (var sub in subCap)
							deviceCaps.Add("D3D9Cap_" + cap.Name + "." + sub.Name, sub.GetValue(value, null));
					}
				}
			}

			return deviceCaps;
		}

		/// <summary>
		/// Function to retrieve the device capabilities.
		/// </summary>
		/// <returns>
		/// A collection of custom renderer specific capabilities.
		/// </returns>
		protected override IEnumerable<KeyValuePair<string, object>> GetCaps()
		{
			IEnumerable<KeyValuePair<string, object>> result = EnumerateCaps();
			IList<GorgonMSAAQualityLevel> multiSampleLevels = new List<GorgonMSAAQualityLevel>();

			VertexShaderVersion = _caps.VertexShaderVersion;
			PixelShaderVersion = _caps.PixelShaderVersion;
			AlphaComparisonFlags = D3DConvert.Convert(_caps.AlphaCompareCaps);
			DepthComparisonFlags = D3DConvert.Convert(_caps.DepthCompareCaps);

			if ((_caps.PresentationIntervals & PresentInterval.Immediate) == PresentInterval.Immediate)
				VSyncIntervals |= GorgonVSyncInterval.None;

			if ((_caps.PresentationIntervals & PresentInterval.One) == PresentInterval.One)
				VSyncIntervals |= GorgonVSyncInterval.One;

			if ((_caps.PresentationIntervals & PresentInterval.Two) == PresentInterval.Two)
				VSyncIntervals |= GorgonVSyncInterval.Two;

			if ((_caps.PresentationIntervals & PresentInterval.Three) == PresentInterval.Three)
				VSyncIntervals |= GorgonVSyncInterval.Three;

			if ((_caps.PresentationIntervals & PresentInterval.Four) == PresentInterval.Four)
				VSyncIntervals |= GorgonVSyncInterval.Four;
			
			
			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9VideoDeviceCapabilities"/> class.
		/// </summary>
		/// <param name="d3d">Direct 3D instance.</param>
		/// <param name="info">The adapter information.</param>
		/// <param name="caps">The caps.</param>
		internal D3D9VideoDeviceCapabilities(Direct3D d3d, DeviceType deviceType, AdapterInformation info, Capabilities caps)
		{
			if (d3d == null)
				throw new ArgumentNullException("d3d");
			if (info == null)
				throw new ArgumentNullException("info");
			if (caps == null)
				throw new ArgumentNullException("caps");

			_deviceType = deviceType;
			_d3d = d3d;
			_info = info;
			_caps = caps;
		}
		#endregion
	}
}
