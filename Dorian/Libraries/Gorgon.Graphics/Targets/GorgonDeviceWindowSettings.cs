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
// Created: Saturday, August 06, 2011 1:01:21 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics
{

	/// <summary>
	/// Values for setting up a <see cref="GorgonLibrary.Graphics.GorgonDeviceWindow">Gorgon Device Window</see>.
	/// </summary>
	public class GorgonDeviceWindowSettings
		: GorgonSwapChainSettings
	{
		#region Variables.
		private int _refreshDenominator = 1;	// Refresh rate denominator.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the video device to use.
		/// </summary>
		public GorgonVideoDevice Device
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to set or return the video output to use.
		/// </summary>
		public GorgonVideoOutput Output
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the width, height, format and refresh rate of the device window.
		/// </summary>
		public GorgonVideoMode DisplayMode
		{
			get
			{
				return new GorgonVideoMode(Width, Height, Format, RefreshRateNumerator, RefreshRateDenominator);
			}
		}

		/// <summary>
		/// Property to set or return the refresh rate numerator value.
		/// </summary>
		public int RefreshRateNumerator
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the refresh rate denominator value.
		/// </summary>
		public int RefreshRateDenominator
		{
			get
			{
				return _refreshDenominator;
			}
			set
			{
				if (value < 1)
					value = 1;
				_refreshDenominator = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the device window should go to full screen or windowed mode.
		/// </summary>
		public bool IsWindowed
		{
			get;
			set;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceWindowSettings"/> class.
		/// </summary>
		/// <param name="device">The video device to use.</param>
		/// <param name="output">The video output to use.</param>
		/// <param name="boundWindow">The window to bind to the device window.</param>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the default Gorgon <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">ApplicationWindow</see>.
		/// <para>Passing NULL (Nothing in VB.Net) to the <paramref name="device"/> or <paramref name="output"/> will use the device/output that holds the window.</para>
		/// </remarks>
		public GorgonDeviceWindowSettings(GorgonVideoDevice device, GorgonVideoOutput output, Control boundWindow)
			: base(boundWindow == null ? Gorgon.ApplicationForm : boundWindow)
		{
			Device = device;
			Output = output;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceWindowSettings"/> class.
		/// </summary>
		/// <param name="boundWindow">The window to bind to the device window.</param>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the default Gorgon <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">ApplicationWindow</see>.
		/// </remarks>
		public GorgonDeviceWindowSettings(Control boundWindow)
			: this(null, null, boundWindow)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceWindowSettings"/> class.
		/// </summary>
		public GorgonDeviceWindowSettings()
			: this(null, null, null)
		{			
		}
		#endregion
	}

	/// <summary>
	/// Specialized device window settings for multi-head devices.
	/// </summary>
	public class GorgonMultiHeadSettings
	{
		#region Variables.
		private GorgonDeviceWindowSettings[] _settings = null;		// Settings.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the multi-head settings for the devices.
		/// </summary>
		public IEnumerable<GorgonDeviceWindowSettings> Settings
		{
			get
			{
				return _settings;
			}
		}

		/// <summary>
		/// Property to return the video device for the multi-head settings.
		/// </summary>
		public GorgonVideoDevice Device
		{
			get;
			private set;
		}
		#endregion

		#region Methods.

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMultiHeadSettings"/> class.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="settings">The settings for each output.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="device"/> or <paramref name="settings"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the settings parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when the settings list does not contain the same number of heads on the video device.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the monitor for the bound window could not be found, or multiple windows are on the same monitor.</para>
		/// </exception>
		public GorgonMultiHeadSettings(GorgonVideoDevice device, IEnumerable<GorgonDeviceWindowSettings> settings)			
		{
			if (device == null)
				throw new ArgumentNullException("device");
			if (settings == null)
				throw new ArgumentNullException("settings");
			if ((settings.Count() == 0) || (settings.Count() != device.Outputs.Count))
				throw new ArgumentException("The number of bound windows is not the same as the output count for the video device.");

			Device = device;
			_settings = settings.ToArray();

			// Validate the settings.
			for (int i = 0; i < _settings.Length; i++)
			{
				if (_settings[i].BoundWindow == null)
					throw new ArgumentException("There was no window bound to the setting at index '" + i.ToString() + "'.", "settings");

				IntPtr monitor = Win32API.GetMonitor(_settings[i].BoundWindow);

				// Find the output for the window.
				if (monitor == IntPtr.Zero)
					throw new ArgumentException("Could not locate the monitor for the window '" + _settings[i].BoundWindow.Name + "'.", "boundWindows");

				var videoOutput = (from output in device.Outputs
								  where output.Handle == monitor
								  select output).Single();

				// Make sure the windows aren't on the same monitors.
				if (_settings.Count(item => (item != null) && (item.Output == videoOutput)) > 0)
					throw new ArgumentException("The window '" + _settings[i].BoundWindow.Name + "' is already on the output '" + videoOutput.Name + "'.  It must be placed on a different output to use the fullscreen multi-head display.");

				_settings[i].Device = Device;
				_settings[i].Output = videoOutput;
			}			
		}
		#endregion
	}
}
