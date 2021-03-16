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
// Created: Sunday, September 13, 2015 1:54:33 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Diagnostics;
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
    /// 
    /// using (var assemblies = new GorgonMefPlugInCache())
    /// {
    ///		IGorgonPlugInService plugInService = new GorgonMefPlugInService(assemblies);
    ///		var factory = new GorgonGamingDeviceDriverFactory(plugInService);
    ///
    ///		// Load the assembly for the XInput driver.
    ///		assemblies.LoadAssemblies(".\", "Gorgon.Input.XInput.dll");
    /// 
    ///		// Get the correct driver from the plug ins via the factory. 
    ///		IGorgonGamingDeviceDriver driver = factory.Load("Gorgon.Input.GorgonXInputDriver");
    /// 
    ///		// Get connected devices only. You may change the parameter to false to retrieve all devices.
    ///		joysticks = driver.EnumerateGamingDevices(true);
    /// 
    ///		foreach(IGorgonGamingDeviceInfo info in joysticks)
    ///		{
    ///			Console.WriteLine($"Controller: {info.Description}");
    ///		}
    /// } 
    /// ]]>
    /// </code>
    /// </example>
    public abstract class GorgonGamingDeviceDriver
        : GorgonPlugIn, IGorgonGamingDeviceDriver
    {
        #region Properties.
        /// <summary>
        /// Property to return the logger used for debugging.
        /// </summary>
        protected internal IGorgonLog Log
        {
            get;
            internal set;
        }
        #endregion

        #region Methods.
        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected abstract void Dispose(bool disposing);
        
        /// <summary>
        /// Function to enumerate the gaming devices supported by this driver.
        /// </summary>
        /// <param name="connectedOnly">[Optional] <b>true</b> to only enumerate devices that are connected, or <b>false</b> to enumerate all devices.</param>
        /// <returns>A read only list of gaming device info values.</returns>
        /// <remarks>
        /// This will return only the devices supported by the driver. In some cases, the driver may not return a complete listing of all gaming devices attached to the system because the underlying provider 
        /// may not support those device types.
        /// </remarks>
        public abstract IReadOnlyList<IGorgonGamingDeviceInfo> EnumerateGamingDevices(bool connectedOnly = false);

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
        public abstract IGorgonGamingDevice CreateGamingDevice(IGorgonGamingDeviceInfo gamingDeviceInfo);

        /// <summary>
        /// Function to create all <see cref="GorgonGamingDevice"/> objects for this driver.
        /// </summary>
        /// <param name="connectedOnly">[Optional] <b>true</b> to only include connected gaming devices, <b>false</b> to include all devices.</param>
        /// <returns>A list of of <see cref="GorgonGamingDevice"/> objects for this driver.</returns>
        /// <remarks>
        /// This will create an instance of <see cref="IGorgonGamingDevice"/> for all devices supported by the driver at one time and return a list containing those instances.
        /// </remarks>
        public IReadOnlyList<IGorgonGamingDevice> CreateGamingDevices(bool connectedOnly = false)
        {
            IReadOnlyList<IGorgonGamingDeviceInfo> infoList = EnumerateGamingDevices(connectedOnly);

            if ((infoList is null) || (infoList.Count == 0))
            {
                return Array.Empty<GorgonGamingDevice>();
            }

            var result = new List<IGorgonGamingDevice>();

            foreach (IGorgonGamingDeviceInfo deviceInfo in infoList)
            {
                IGorgonGamingDevice device = CreateGamingDevice(deviceInfo);

                if ((!device.IsConnected) && (connectedOnly))
                {
                    device.Dispose();
                    continue;
                }

                result.Add(device);
            }

            return result;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// For implementors of a <see cref="GorgonGamingDeviceDriver"/> this dispose method is used to clean up any native resources that may be allocated by the driver. In such a case, put the clean up code for 
        /// the native resources in an override of this method. If the driver does not use native resources, then this method should be left alone.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonGamingDeviceDriver"/> class.
        /// </summary>
        /// <param name="description">The human readable description of the driver.</param>
        protected GorgonGamingDeviceDriver(string description)
            : base(description) => Log = GorgonLog.NullLog;
        #endregion

    }
}
