using Gorgon.Native;

namespace Gorgon.Input
{
	/// <summary>
	/// Provides capability information about a Raw Input Human Interface Device.
	/// </summary>
	public interface IGorgonRawHIDInfo : IGorgonRawInputDeviceInfo
	{
		/// <summary>
		/// Property to return the product ID for the device.
		/// </summary>
		int ProductID
		{
			get;
		}

		/// <summary>
		/// Property to return the vendor ID for the device.
		/// </summary>
		int VendorID
		{
			get;
		}

		/// <summary>
		/// Property to return the version number for the device.
		/// </summary>
		int Version
		{
			get;
		}

		/// <summary>
		/// Property to return the top level collection usage value for this device.
		/// </summary>
		HIDUsage Usage
		{
			get;
		}

		/// <summary>
		/// Property to return the top level collection usage page value for this device.
		/// </summary>
		HIDUsagePage UsagePage
		{
			get;
		}
	}
}