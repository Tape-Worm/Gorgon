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
	/// Video mode is not valid.
	/// </summary>
	public class DeviceVideoModeNotValidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mode">Video mode that is invalid.</param>
		/// <param name="ex">Source exception.</param>
		public DeviceVideoModeNotValidException(VideoMode mode, Exception ex)
			: base("The video mode " + mode.ToString() + " is not valid.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mode">Video mode that is invalid.</param>
		public DeviceVideoModeNotValidException(VideoMode mode)
			: this(mode, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Device creation failed.
	/// </summary>
	public class DeviceCreationFailureException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public DeviceCreationFailureException(Exception ex)
			: base("Unable to create the video device.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public DeviceCreationFailureException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Device reset failed.
	/// </summary>
	public class DeviceCannotResetException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="driverError">TRUE if the exception was due to a driver error, FALSE if not.</param>
		/// <param name="ex">Source exception.</param>
		public DeviceCannotResetException(bool driverError, Exception ex)
			: base((driverError ? "There was an internal driver error while trying to reset the device" : "Unable to reset the render window."), ex)
		{
		}

		/// <summary>
		/// constructor.
		/// </summary>
		/// <param name="driverError">TRUE if the exception was due to a driver error, FALSE if not.</param>
		public DeviceCannotResetException(bool driverError)
			: this(driverError, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public DeviceCannotResetException(Exception ex)
			: this(false, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public DeviceCannotResetException()
			: this(false, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Device has no valid video modes.
	/// </summary>
	public class DeviceHasNoVideoModesException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public DeviceHasNoVideoModesException(Exception ex)
			: base("Device has no valid video modes.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public DeviceHasNoVideoModesException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Device is invalid.
	/// </summary>
	public class DeviceNotValidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public DeviceNotValidException(Exception ex)
			: base("Video device is not valid.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public DeviceNotValidException()
			: this(null)
		{
		}
		#endregion
	}
}