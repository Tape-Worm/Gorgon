#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Saturday, August 8, 2015 7:41:33 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Native;

namespace Gorgon.Input.Raw.Devices
{
	/// <summary>
	/// The raw input implementation of joystick information.
	/// </summary>
	class RawInputJoystickInfo
		: IGorgonJoystickInfo2
	{
		#region Variables.
		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the handle for the device.
		/// </summary>
		public IntPtr Handle
		{
			get;
		}

		/// <inheritdoc/>
		public IGorgonJoystickAxisInfoList AxisInfo
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public int ButtonCount
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public JoystickCapabilityFlags Capabilities
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public string ClassName
		{
			get;
		}

		/// <inheritdoc/>
		public string Description
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public string HumanInterfaceDevicePath
		{
			get;
		}

		/// <inheritdoc/>
		public InputDeviceType InputDeviceType => InputDeviceType.Joystick;

		public bool IsConnected
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <inheritdoc/>
		public int ManufacturerID
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public int ProductID
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public IReadOnlyList<GorgonRange> VibrationMotorRanges
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert a logical range value into the proper type to be consumed by a GorgonRange type.
		/// </summary>
		/// <param name="bitSize">The number of bits for the range value.</param>
		/// <param name="value">Value to convert.</param>
		/// <returns>The converted range value.</returns>
		private static int ConvertRangeValue(ushort bitSize, int value)
		{
			switch (bitSize)
			{
				case 4:
					return value & 0xf;
				case 8:
					return value & 0xff;
				case 16:
					byte[] bitData = BitConverter.GetBytes(value);

					return BitConverter.ToUInt16(bitData, 0);
				default:
					return value;
			}
		}

		/// <summary>
		/// Function to parse the HID value caps into axis ranges.
		/// </summary>
		/// <param name="valueCaps">The HID value caps.</param>
		private void ParseValueCaps(IReadOnlyList<HIDP_VALUE_CAPS> valueCaps)
		{
			Capabilities = JoystickCapabilityFlags.None;
			var axisInfo = new Dictionary<JoystickAxis, GorgonJoystickAxisInfo>(new GorgonJoystickAxisEqualityComparer());

			foreach (HIDP_VALUE_CAPS caps in valueCaps)
			{
				JoystickAxis axis;

				switch (caps.NotRange.Usage)
				{
					case HIDUsage.X:
						axis = JoystickAxis.XAxis;
                        break;
					case HIDUsage.Y:
						axis = JoystickAxis.YAxis;
						break;
					case HIDUsage.Z:
					case HIDUsage.Slider:
						axis = JoystickAxis.ZAxis;

						if (caps.NotRange.Usage == HIDUsage.Slider)
						{
							Capabilities |= JoystickCapabilityFlags.SupportsThrottle;
						}
						break;
					case HIDUsage.RelativeX:
						axis = JoystickAxis.XAxis2;
						Capabilities |= JoystickCapabilityFlags.SupportsSecondaryXAxis;
						break;
					case HIDUsage.RelativeY:
						axis = JoystickAxis.YAxis2;
						Capabilities |= JoystickCapabilityFlags.SupportsSecondaryYAxis;
						break;
					case HIDUsage.RelativeZ:
						axis = JoystickAxis.ZAxis2;
						Capabilities |= JoystickCapabilityFlags.SupportsRudder;
						break;
					case HIDUsage.HatSwitch:
						Capabilities |= JoystickCapabilityFlags.SupportsPOV;

						if (caps.BitSize == 4)
						{
							Capabilities |= JoystickCapabilityFlags.SupportsDiscreetPOV;
						}
						else
						{
							Capabilities |= JoystickCapabilityFlags.SupportsContinuousPOV;
						}
						continue;
					default:
						continue;
				}

				int minValue = ConvertRangeValue(caps.BitSize, caps.LogicalMin);
				int maxValue = ConvertRangeValue(caps.BitSize, caps.LogicalMax);
				
				var range = new GorgonRange(minValue, maxValue);
				axisInfo[axis] = new GorgonJoystickAxisInfo(axis, range);
			}

			AxisInfo = new GorgonJoystickAxisInfoList(axisInfo.Select(item => item.Value));
		}

		/// <summary>
		/// Function to retrieve and parse out the device information settings for a raw input joystick.
		/// </summary>
		/// <param name="deviceInfo">Device information functionality.</param>
		/// <param name="hidInfo">Raw input device information.</param>
		public void AssignRawInputDeviceInfo(RawInputDeviceRegistryInfo deviceInfo, ref RID_DEVICE_INFO_HID hidInfo)
		{
			string newDescription = deviceInfo.GetJoystickName(HumanInterfaceDevicePath);

			if (!string.IsNullOrWhiteSpace(newDescription))
			{
				Description = newDescription;
			}

			using (GorgonPointer preParsedData = RawInputApi.GetPreparsedDeviceInfoData(Handle))
			{
				if (preParsedData == null)
				{
					return;
				}

				HIDP_CAPS caps = HIDApi.GetHIDCaps(preParsedData);
				IReadOnlyList<HIDP_VALUE_CAPS> valueCaps = HIDApi.GetAxisRanges(preParsedData, ref caps);

				ParseValueCaps(valueCaps);

				ButtonCount = HIDApi.GetJoystickButtonCount(preParsedData, ref caps);
				ManufacturerID = hidInfo.dwVendorId;
				ProductID = hidInfo.dwProductId;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawInputJoystickInfo"/> class.
		/// </summary>
		/// <param name="deviceDescription">The device description.</param>
		/// <param name="className">Class name of the device.</param>
		/// <param name="hidPath">Human interface device path.</param>
		/// <param name="handle">Handle to the device.</param>
		public RawInputJoystickInfo(string deviceDescription, string className, string hidPath, IntPtr handle)
		{
			Description = deviceDescription;
			ClassName = className;
			HumanInterfaceDevicePath = hidPath;
			Handle = handle;
			VibrationMotorRanges = new GorgonRange[0];
		}
		#endregion
	}
}
