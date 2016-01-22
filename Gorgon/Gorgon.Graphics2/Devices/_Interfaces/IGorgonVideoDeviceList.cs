using Gorgon.Collections;

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
	/// This list will contain <see cref="GorgonVideoDeviceInfo"/> objects which can then be passed to a <see cref="GorgonGraphics"/> instance. This allows applications or users to pick and choose which device 
	/// they wish to use.
	/// </para>
	/// <para>
	/// This list will also allow enumeration of the WARP/Reference devices.  WARP is a high performance software device that will emulate much of the functionality that a real video device would have. The 
	/// reference device is a fully featured device for debugging driver issues.
	/// </para>
	/// </remarks>
	public interface IGorgonVideoDeviceList 
		: IGorgonNamedObjectReadOnlyList<GorgonVideoDeviceInfo>
	{
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
		void Enumerate(bool enumerateWARPDevice = false);
	}
}