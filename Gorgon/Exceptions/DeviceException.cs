#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Monday, October 02, 2006 12:25:13 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;

namespace GorgonLibrary
{
	/// <summary>
	/// Device error codes.
	/// </summary>
	public enum DeviceErrors
	{
		/// <summary>Cannot enumerate video devices.</summary>
		CannotEnumerateDevices = 0x7FFF0001,
		/// <summary>Unable to locate any devices that support hardware acceleration.</summary>
		NoAcceptableDevices = 0x7FFF0002,
		/// <summary>Video mode selected was invalid.</summary>
		InvalidVideoMode = 0x7FFF0003,
		/// <summary>No video modes for selected device.</summary>
		NoVideoModes = 0x7FFF0004,
		/// <summary>Unable to create the render target.</summary>
		CannotCreateRenderTarget = 0x7FFF0005,
		/// <summary>D3D device object has not been created.</summary>
		DeviceObjectNotFound = 0x7FFF0006,
		/// <summary>Unable to create D3D device.</summary>
		CannotCreateDevice = 0x7FFF0007,
		/// <summary>Unable to restore the device after a loss.</summary>
		CannotResetDevice = 0x7FFF0008
	}

	/// <summary>
	/// Base exception for device exceptions.
	/// </summary>
	public abstract class DeviceException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="code">Error code.</param>
		/// <param name="ex">Exception source.</param>
		public DeviceException(string message, DeviceErrors code, Exception ex)
			: base(message, (int)code, ex)
		{
			_errorCodeType = code.GetType();
		}
		#endregion
	}

	/// <summary>
	/// Cannot enumerate video devices exception.
	/// </summary>
	public class CannotEnumerateVideoDevicesException 
		: DeviceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public CannotEnumerateVideoDevicesException(string message, Exception ex)
			: base(message, DeviceErrors.CannotEnumerateDevices, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public CannotEnumerateVideoDevicesException(Exception ex)
			: this("Cannot enumerate the video devices.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// No hardware accelerated devices exception.
	/// </summary>
	public class NoHWAcceleratedDevicesException 
		: DeviceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public NoHWAcceleratedDevicesException(string message, Exception ex)
			: base(message, DeviceErrors.NoAcceptableDevices, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public NoHWAcceleratedDevicesException(Exception ex)
			: this("There are no installed devices with hardware acceleration.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Invalid video mode exception.
	/// </summary>
	public class InvalidVideoModeException 
		: DeviceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidVideoModeException(string message, Exception ex)
			: base(message, DeviceErrors.InvalidVideoMode, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mode">Video mode that caused the exception.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidVideoModeException(VideoMode mode, Exception ex)
			: this("Video mode " + mode.ToString() + "' is invalid.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// No video modes for the selected device exception.
	/// </summary>
	public class NoVideoModesException 
		: DeviceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public NoVideoModesException(string message, Exception ex)
			: base(message, DeviceErrors.NoVideoModes, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public NoVideoModesException(Exception ex)
			: this("The selected video adapter has no acceptable video modes available.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot create render target exception.
	/// </summary>
	public class CannotCreateRenderTargetException 
		: DeviceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public CannotCreateRenderTargetException(string message, Exception ex)
			: base(message, DeviceErrors.CannotCreateRenderTarget, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public CannotCreateRenderTargetException(Exception ex)
			: this("Cannot create the render target.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// No D3D device exception.
	/// </summary>
	public class NoDeviceObjectException 
		: DeviceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public NoDeviceObjectException(string message, Exception ex)
			: base(message, DeviceErrors.DeviceObjectNotFound, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public NoDeviceObjectException(Exception ex)
			: this("The D3D device object is null.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot create D3D device exception.
	/// </summary>
	public class CannotCreateDeviceException 
		: DeviceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public CannotCreateDeviceException(string message, Exception ex)
			: base(message, DeviceErrors.CannotCreateDevice, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public CannotCreateDeviceException(Exception ex)
			: this("Cannot create the Direct3D device object.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot reset D3D device exception.
	/// </summary>
	public class CannotResetDeviceException 
		: DeviceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public CannotResetDeviceException(string message, Exception ex)
			: base(message, DeviceErrors.CannotResetDevice, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public CannotResetDeviceException(Exception ex)
			: this("Cannot reset the Direct3D device object.", ex)
		{
		}
		#endregion
	}
}