#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Monday, October 02, 2006 12:25:13 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

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