using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Extra information for the raw input data info structures.
	/// </summary>
	interface IRawInputHumanInterfaceDeviceInfo
		: IGorgonHumanInterfaceDeviceInfo
	{
		/// <summary>
		/// Property to return the handle to the device.
		/// </summary>
		IntPtr Handle
		{
			get;
		}

		/// <summary>
		/// Property to return the HID usage.
		/// </summary>
		HIDUsage Usage
		{
			get;
		}

		/// <summary>
		/// Property to return the HID usage page.
		/// </summary>
		HIDUsagePage UsagePage
		{
			get;
		}
	}
}
