#region MIT
// 
// Gorgon
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
// Created: Thursday, September 17, 2015 8:15:57 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.PlugIns;

namespace Gorgon.Input
{
	/// <summary>
	/// The required functionality for a gaming device driver.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A gaming device driver provides access to various gaming input devices like a joystick, game pad, etc... This allows for various input providers to be used when accessing these devices. For example, 
	/// Gorgon comes with 2 drivers: One for XBOX controllers (via XInput), and one for generic joysticks/game pads (via Direct Input).
	/// </para>
	/// <para>
	/// <note type="important">
	/// The XInput driver does not enumerate or use any other type of controllers other than XBox controllers, and the Direct Input driver purposely ignores XBox controllers so that only generic devices 
	/// get enumerated. This is done because the XInput controller provides more features for the XBox controllers, and the mapping of the device interface (i.e. axes, POV, etc...) is not as straight 
	/// forward.
	/// </note>
	/// </para>
	/// <para>
	/// These drivers are typically loaded as plug ins from  a <see cref="IGorgonGamingDeviceDriverFactory"/>. The driver is responsible for enumerating the available devices that it supports, and will 
	/// create instances of a <see cref="IGorgonGamingDevice"/> that represents the functionality of the gaming device as provided by the underlying native provider. 
	/// </para>
	/// <para>
	/// Because drivers may need to set up native resources for internal use, this interface implements <see cref="IDisposable"/>, and in order to avoid leakage of resource data, the <see cref="IDisposable.Dispose"/> 
	/// method <b>must</b> be called when done with the driver.
	/// </para>
	/// </remarks>
	/// <example>
	/// <see cref="GorgonMefPlugInCache.LoadPlugInAssemblies"/>
	/// The following shows how to load a <see cref="IGorgonGamingDeviceDriver"/> and enumerate the devices:
	/// <code language="csharp">
	/// <![CDATA[
	/// IReadOnlyList<GorgonGamingDeviceInfo> joysticks = null;
	/// TODO: This needs fixing.
	/// using (var assemblies = new GorgonPlugInAssemblyCache())
	/// {
	///		var plugInService = new GorgonPlugInService(assemblies);
	///		var factory = new GorgonGamingDeviceDriverFactory(plugInService);
	///
	///		// Load the assembly for the XInput driver.
	///		assemblies.Load(".\Gorgon.Input.XInput.dll");
	/// 
	///		// Get the correct driver from the plug ins via the factory. 
	///		IGorgonGamingDeviceDriver driver = factory.Load("Gorgon.Input.GorgonXInputDriver");
	/// 
	///		// Get connected devices only. You may change the parameter to false to retrieve all devices.
	///		joysticks = driver.EnumerateGamingDevices(true);
	/// 
	///		foreach(IGorgonGamingDeviceInfo info in joysticks)
	///		{
	///			Console.WriteLine("Controller: {0}", info.Description);
	///		}
	/// } 
	/// ]]>
	/// </code>
	/// </example>
	public interface IGorgonGamingDeviceDriver 
		: IGorgonNamedObject, IDisposable
	{
		#region Properties.
		/// <summary>
		/// Property to return a description of the driver.
		/// </summary>
		string Description
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to enumerate the gaming devices supported by this driver.
		/// </summary>
		/// <param name="connectedOnly">[Optional] <b>true</b> to only enumerate devices that are connected, or <b>false</b> to enumerate all devices.</param>
		/// <returns>A read only list of gaming device info values.</returns>
		/// <remarks>
		/// This will return only the devices supported by the driver. In some cases, the driver may not return a complete listing of all gaming devices attached to the system because the underlying provider 
		/// may not support those device types.
		/// </remarks>
		IReadOnlyList<IGorgonGamingDeviceInfo> EnumerateGamingDevices(bool connectedOnly = false);

		/// <summary>
		/// Function to create a <see cref="GorgonGamingDevice"/> object.
		/// </summary>
		/// <param name="gamingDeviceInfo">The <see cref="IGorgonGamingDeviceInfo"/> used to determine which device to associate with the resulting object.</param>
		/// <returns>A <see cref="GorgonGamingDevice"/> representing the data from the physical device.</returns>
		/// <remarks>
		/// <para>
		/// This will create a new instance of a <see cref="IGorgonGamingDevice"/> which will relay data from the physical device using the native provider. 
		/// </para>
		/// <para>
		/// Some devices may allocate native resources in order to communicate with the underlying native providers, and because of this, it is important to call the <see cref="IDisposable.Dispose"/> method 
		/// on the object when you are done with the object so that those resources may be freed.
		/// </para>
		/// </remarks>
		IGorgonGamingDevice CreateGamingDevice(IGorgonGamingDeviceInfo gamingDeviceInfo);

		/// <summary>
		/// Function to create all <see cref="GorgonGamingDevice"/> objects for this driver.
		/// </summary>
		/// <param name="connectedOnly">[Optional] <b>true</b> to only include connected gaming devices, <b>false</b> to include all devices.</param>
		/// <returns>A list of of <see cref="GorgonGamingDevice"/> objects for this driver.</returns>
		/// <remarks>
		/// This will create an instance of <see cref="IGorgonGamingDevice"/> for all devices supported by the driver at one time and return a list containing those instances.
		/// </remarks>
		IReadOnlyList<IGorgonGamingDevice> CreateGamingDevices(bool connectedOnly = false);
		#endregion
	}
}