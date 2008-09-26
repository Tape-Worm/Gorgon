#region MIT.
//
// Gorgon.
// Copyright (C) 2004 Michael Winsor
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
// Created: Saturday, May 08, 2004 11:59:46 AM
//
#endregion

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using GorgonLibrary;
using Dialogs;

namespace GorgonLibrary.Framework
{
	/// <summary>
	/// Form to handle gorgon configuration.
	/// </summary>
	public partial class SetupDialog 
		: System.Windows.Forms.Form
	{
		#region Variables.
		private bool _windowed = true;			// Windowed mode flag.
		private VideoMode _mode;				// Current video mode.
		private Driver _driver;					// Current video driver.
		private VSyncIntervals _interval;		// VSync interval.
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public SetupDialog()
		{
			this.DoubleBuffered = true;
			GorgonAppSettings.Root = null;
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the selected vsync interval.
		/// </summary>
		public VSyncIntervals VSyncInterval
		{
			get
			{
				return _interval;
			}
		}

		/// <summary>
		/// Property to return whether background rendering should be enabled or not.
		/// </summary>
		public bool AllowBackgroundRendering
		{
			get
			{
				return checkAllowBackground.Checked;
			}
		}

		/// <summary>
		/// Property to return whether the screen saver should be enabled or not.
		/// </summary>
		public bool AllowScreensaver
		{
			get
			{
				return checkAllowScreensaver.Checked;
			}
		}

		/// <summary>
		/// Property to return the currently selected video mode.
		/// </summary>
		public VideoMode VideoMode
		{
			get
			{
				return _mode;
			}
		}

		/// <summary>
		/// Property to return the selected driver.
		/// </summary>
		public Driver VideoDriver
		{
			get
			{
				return _driver;
			}
		}

		/// <summary>
		/// Property to return selected windowed state.
		/// </summary>
		public bool WindowedFlag
		{
			get
			{
				return _windowed;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				if (Owner != null)
					Icon = Owner.Icon;

				_interval = VSyncIntervals.IntervalNone;
				checkAllowScreensaver.Checked = Gorgon.AllowScreenSaver;
				checkAllowBackground.Checked = Gorgon.AllowBackgroundRendering;

				// Default to 640x480x32 60Hz
				_mode = new VideoMode(640, 480, 60, BackBufferFormats.BufferRGB888);
				_windowed = true;

				// Fill in the renderers combo box.
				FillVideoDevices();

				// Get the settings from the registry.
				GetSettings();

				// Filter video modes.
				windowed.Checked = !_windowed;

				if (windowed.Checked)
					buttonTips.SetToolTip(windowed, "Currently using fullscreen mode.\nClick to use windowed mode.");
				else
					buttonTips.SetToolTip(windowed, "Currently using windowed mode.\nClick to use full screen mode.");

				FillVSyncIntervals();
			}
			catch (Exception Ex)
			{
				// Error message.
				UI.ErrorBox(this, Ex);
				Close();
			}
		}

		/// <summary>
		/// Check box checked.
		/// </summary>
		/// <param name="sender">Sender of this event.</param>
		/// <param name="e">Event arguments</param>
		private void windowed_CheckedChanged(object sender, System.EventArgs e)
		{
			if (windowed.Checked)
				buttonTips.SetToolTip(windowed, "Currently using fullscreen mode.\nClick to use windowed mode.");
			else
				buttonTips.SetToolTip(windowed, "Currently using windowed mode.\nClick to use full screen mode.");

			if (!windowed.Checked)
				comboVSync.Text = comboVSync.Items[0].ToString();
			comboVSync.Enabled = windowed.Checked;
			_windowed = !windowed.Checked;
		}

		/// <summary>
		/// Combo box item changed event.
		/// </summary>
		/// <param name="sender">Sender of this event.</param>
		/// <param name="e">Event arguments</param>
		private void videoDevices_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			try
			{
				// Change in the device?  Reset video modes.
				if (_driver.DriverIndex != videoDevices.SelectedIndex)
				{
					_mode.Width = 0;
					_mode.Height = 0;
					_mode.Format = BackBufferFormats.BufferRGB888;
					_mode.RefreshRate = 0;
				}

				if (videoDevices.SelectedIndex > -1)
					_driver = Gorgon.Drivers[videoDevices.SelectedIndex];
				else
					_driver = Gorgon.Drivers[0];
			}
			finally
			{
				// Attempt to fill the video modes.
				FillVideoModes();

				// Get first video mode.
				if (videoModes.FindStringExact(_mode.Width + "x" + _mode.Height + " Format: " + _mode.Format.ToString() + " (" + _mode.Bpp + " Bpp) " + _mode.RefreshRate + "Hz") < 0)
					_mode = _driver.VideoModes[0];
				videoModes.Text = _mode.Width + "x" + _mode.Height + " Format: " + _mode.Format.ToString() + " (" + _mode.Bpp + " Bpp) " + _mode.RefreshRate + "Hz";
			}
		}

		/// <summary>
		/// Combo box item changed event.
		/// </summary>
		/// <param name="sender">Sender of this event.</param>
		/// <param name="e">Event arguments</param>
		private void videoModes_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			string modeStr;		// Video mode search string.

			try
			{
				modeStr = videoModes.Text.Substring(0, videoModes.Text.Length - 2);

				// Parse the string.
				_mode.Width = Convert.ToInt32(modeStr.Substring(0, modeStr.IndexOf('x')));
				modeStr = modeStr.Substring(modeStr.IndexOf('x') + 1);
				_mode.Height = Convert.ToInt32(modeStr.Substring(0, modeStr.IndexOf(" Format:")));
				modeStr = modeStr.Substring(modeStr.IndexOf(" Format:") + 9);
				_mode.Format = (BackBufferFormats)Enum.Parse(typeof(BackBufferFormats), modeStr.Substring(0, modeStr.IndexOf(" (")));
				modeStr = modeStr.Substring(modeStr.IndexOf(") ") + 2);
				_mode.RefreshRate = Convert.ToInt32(modeStr);
			}
			catch (IndexOutOfRangeException)
			{
				// Default to first video mode.
				_mode = _driver.VideoModes[0];
			}
		}

		/// <summary>
		/// Ok button click event.
		/// </summary>
		/// <param name="sender">Sender of this event.</param>
		/// <param name="e">Event arguments.</param>
		private void OK_Click(object sender, System.EventArgs e)
		{
			SetSettings();
		}

		/// <summary>
		/// Device info button click event.
		/// </summary>
		/// <param name="sender">Sender of this event.</param>
		/// <param name="e">Event arguments.</param>
		private void deviceInfo_Click(object sender, System.EventArgs e)
		{
			DeviceInformationDialog deviceInfo = null;		// Device info form.

			try
			{
				Cursor.Current = Cursors.WaitCursor;

				deviceInfo = new DeviceInformationDialog(_driver);
				deviceInfo.ShowDialog(this);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				if (deviceInfo != null)
					deviceInfo.Dispose();
				deviceInfo = null;

				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the CheckedChanged event of the checkAdvanced control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void checkAdvanced_CheckedChanged(object sender, EventArgs e)
		{
			if (checkAdvanced.Checked)
			{
				comboVSync.Visible = true;
				checkAllowBackground.Visible = true;
				checkAllowScreensaver.Visible = true;
				Height = 305;
			}
			else
			{
				Height = 238;
				comboVSync.Visible = false;
				checkAllowBackground.Visible = false;
				checkAllowScreensaver.Visible = false;
			}
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboVSync control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void comboVSync_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (comboVSync.Text)
			{
				case "One retrace. (VSyncInterval.IntervalOne)":
					_interval = VSyncIntervals.IntervalOne;
					return;
				case "Two retraces. (VSyncInterval.IntervalTwo)":
					_interval = VSyncIntervals.IntervalTwo;
					return;
				case "Three retraces. (VSyncInterval.IntervalThree)":
					_interval = VSyncIntervals.IntervalThree;
					return;
				case "Four retraces. (VSyncInterval.IntervalFour)":
					_interval = VSyncIntervals.IntervalFour;
					return;
			}

			_interval = VSyncIntervals.IntervalNone;
		}

		/// <summary>
		/// Function to fill in video device combo box.
		/// </summary>
		private void FillVideoDevices()
		{
			// Clear combo
			videoDevices.Items.Clear();

			foreach (Driver dev in Gorgon.Drivers)
				videoDevices.Items.Add(dev.Description);

			// Set to current video device.
			if (Gorgon.Drivers.Count < 2) 
				videoDevices.Enabled = false;
			else
				videoDevices.Enabled = true;
		}

		/// <summary>
		/// Function to fill in video mode combo box.
		/// </summary>
		private void FillVideoModes()
		{
			// Clear video modes
			videoModes.Items.Clear();

			// Refresh video modes.
			_driver.VideoModes.WidthFilter = 640;
			_driver.VideoModes.HeightFilter = 480;
			_driver.VideoModes.FilterCompare = CompareFunctions.GreaterThanOrEqual;
			_driver.VideoModes.Refresh();

			if (_driver.VideoModes.Count < 1)
			{
				UI.ErrorBox(this, "There are no available video modes for this device.");
				return;
			}

			// Add to combo.
			foreach (VideoMode mode in _driver.VideoModes)
				videoModes.Items.Add(mode.Width + "x" + mode.Height + " Format: " + mode.Format.ToString() +  " (" + mode.Bpp + " Bpp) " + mode.RefreshRate + "Hz");
		}

		/// <summary>
		/// Function to fill in the vsync interval combo.
		/// </summary>
		private void FillVSyncIntervals()
		{
			comboVSync.Items.Clear();

			comboVSync.Items.Add("No retraces. (VSyncInterval.IntervalNone)");

			if ((_driver.PresentIntervalSupport & VSyncIntervals.IntervalOne) != 0)
				comboVSync.Items.Add("One retrace. (VSyncInterval.IntervalOne)");
			if ((_driver.PresentIntervalSupport & VSyncIntervals.IntervalTwo) != 0)
				comboVSync.Items.Add("Two retraces. (VSyncInterval.IntervalTwo)");
			if ((_driver.PresentIntervalSupport & VSyncIntervals.IntervalThree) != 0)
				comboVSync.Items.Add("Three retraces. (VSyncInterval.IntervalThree)");
			if ((_driver.PresentIntervalSupport & VSyncIntervals.IntervalFour) != 0)
				comboVSync.Items.Add("Four retraces. (VSyncInterval.IntervalFour)");

			if (comboVSync.Items.Count < 2)
			{
				_interval = VSyncIntervals.IntervalNone;
				comboVSync.Enabled = false;
			}
			else
				comboVSync.Enabled = true;

			if ((_interval & VSyncIntervals.IntervalNone) != 0)
				comboVSync.Text = "No retraces. (VSyncInterval.IntervalNone)";
			if ((_interval & VSyncIntervals.IntervalOne) != 0)
				comboVSync.Text = "One retrace. (VSyncInterval.IntervalOne)";
			if ((_interval & VSyncIntervals.IntervalTwo) != 0)
				comboVSync.Text = "Two retraces. (VSyncInterval.IntervalTwo)";
			if ((_interval & VSyncIntervals.IntervalThree) != 0)
				comboVSync.Text = "Three retraces. (VSyncInterval.IntervalThree)";
			if ((_interval & VSyncIntervals.IntervalFour) != 0)
				comboVSync.Text = "Four retraces. (VSyncInterval.IntervalFour)";

			if (comboVSync.Items.Count > 1)
			{
				if (_windowed)
					comboVSync.Text = comboVSync.Items[0].ToString();
				comboVSync.Enabled = !_windowed;
			}
		}

		/// <summary>
		/// Function to retrieve video mode settings from the registry.
		/// </summary>
		private void GetSettings()
		{
			int width = 640;												// Video mode width.
			int height = 480;												// Video mode height.
			int refresh = 60;												// Video mode refresh rate.
			BackBufferFormats format = BackBufferFormats.BufferRGB888;		// Video mode format.

			try
			{
				_driver = Gorgon.CurrentDriver;

				if (GorgonAppSettings.Load("Config.xml"))
				{
					GorgonAppSettings.Root = "Device";

					try
					{
						_driver = Gorgon.Drivers[GorgonAppSettings.GetSetting("DriverName", Gorgon.CurrentDriver.DriverName)];
					}
					finally
					{
						_driver = Gorgon.CurrentDriver;
					}

					GorgonAppSettings.Root = "VideoMode";
					width = Convert.ToInt32(GorgonAppSettings.GetSetting("Width", "640"));
					height = Convert.ToInt32(GorgonAppSettings.GetSetting("Height", "480"));
					format = (BackBufferFormats)Enum.Parse(typeof(BackBufferFormats), GorgonAppSettings.GetSetting("Format", "BufferRGB888"));
					refresh = Convert.ToInt32(GorgonAppSettings.GetSetting("RefreshRate", "60"));

					GorgonAppSettings.Root = null;
					_windowed = Convert.ToBoolean(GorgonAppSettings.GetSetting("Windowed", "true"));
					_interval = (VSyncIntervals)Enum.Parse(typeof(VSyncIntervals), GorgonAppSettings.GetSetting("VSyncInterval", "IntervalNone"));
					checkAllowBackground.Checked = Convert.ToBoolean(GorgonAppSettings.GetSetting("AllowBackgroundRender", Gorgon.AllowBackgroundRendering.ToString()));
					checkAllowScreensaver.Checked = Convert.ToBoolean(GorgonAppSettings.GetSetting("AllowScreenSaver", Gorgon.AllowScreenSaver.ToString()));
				}					
			}
			catch (Exception ex)
			{
				width = 640;
				height = 480;
				refresh = 60;
				format = BackBufferFormats.BufferRGB888;
				_windowed = true;
				UI.ErrorBox(this, ex);
			}
			finally
			{
				_mode.Width = width;
				_mode.Height = height;
				_mode.RefreshRate = refresh;
				_mode.Format = format;

				try
				{
					// Try to find the mode, if we can't then use the default.
					_mode = _driver.VideoModes[_mode];
				}
				catch (KeyNotFoundException)
				{
					_mode = _driver.VideoModes[0];
				}

				// Set current values.
				videoDevices.Text = _driver.Description;
				videoModes.Text = _mode.Width + "x" + _mode.Height + " Format: " + _mode.Format.ToString() + " (" + _mode.Bpp + " Bpp) " + _mode.RefreshRate + "Hz";

				// Refill vsync intervals.
				FillVSyncIntervals();

				// Enable/disable the vsync.
				if (_windowed)
					comboVSync.Text = comboVSync.Items[0].ToString();
				comboVSync.Enabled = !_windowed;
			}
		}

		/// <summary>
		/// Function to save the settings to the registry.
		/// </summary>
		private void SetSettings()
		{
			try
			{
				GorgonAppSettings.Root = "Device";
				GorgonAppSettings.SetSetting("DriverName", _driver.DriverName);

				GorgonAppSettings.Root = "VideoMode";
				GorgonAppSettings.SetSetting("Width", _mode.Width.ToString());
				GorgonAppSettings.SetSetting("Height", _mode.Height.ToString());
				GorgonAppSettings.SetSetting("Format", _mode.Format.ToString());
				GorgonAppSettings.SetSetting("RefreshRate", _mode.RefreshRate.ToString());

				GorgonAppSettings.Root = null;
				GorgonAppSettings.SetSetting("Windowed", _windowed.ToString());
				GorgonAppSettings.SetSetting("VSyncInterval", _interval.ToString());
				GorgonAppSettings.SetSetting("AllowBackgroundRender", checkAllowBackground.Checked.ToString());
				GorgonAppSettings.SetSetting("AllowScreenSaver", checkAllowScreensaver.Checked.ToString());

				GorgonAppSettings.Save("Config.xml");
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Could not save the settings.", ex);
			}
		}
		#endregion        
	}
}
