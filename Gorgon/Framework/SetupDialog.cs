#region "LGPL."
//
// Gorgon.
// Copyright (C) 2004 Michael Winsor
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
// Created: Saturday, May 08, 2004 11:59:46 AM
//
#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Win32;
using SharpUtilities;
using SharpUtilities.Collections;
using SharpUtilities.Utility;
using GorgonLibrary;

namespace GorgonLibrary.Framework
{
	/// <summary>
	/// Form to handle gorgon configuration.
	/// </summary>
	public partial class SetupDialog : System.Windows.Forms.Form
	{
		// 308 -> 227
		#region Variables.
		private bool _windowed;					// Windowed mode flag.
		private VideoMode _mode;				// Current video mode.
		private Driver _driver;					// Current video driver.
		private bool _fadeDirection;			// Direction of fading.
		private DialogResult _result;			// Dialog result.
		private VSyncIntervals _interval;		// VSync interval.
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public SetupDialog()
		{
			this.DoubleBuffered = true;
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
		/// Function to run the _setup.
		/// </summary>
		/// <param name="owner">Owner of this window.</param>
		/// <returns>TRUE if cancelled, FALSE if not.</returns>
		public DialogResult Run(Form owner)
		{
			if (owner != null)
				Icon = owner.Icon;

			return ShowDialog(owner);
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
				throw new NoVideoModesException(null);

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
		}

		/// <summary>
		/// Function to retrieve video mode settings from the registry.
		/// </summary>
		private void GetSettings()
		{
			RegistryKey regKey = null;										// Gorgon registry key.
			int width = 640;												// Video mode width.
			int height = 480;												// Video mode height.
			int refresh = 60;												// Video mode refresh rate.
			BackBufferFormats format = BackBufferFormats.BufferRGB888;		// Video mode format.

			try
			{
				regKey = Registry.CurrentUser.OpenSubKey("Software\\Tape_Worm\\Gorgon\\Examples");

				// If the key doesn't exist, then exit.
				if (regKey == null)
				{
					_driver = Gorgon.Driver;
					return;
				}

				// Set the renderer and get the default driver.
				_driver = Gorgon.Driver;
				if (regKey.GetValue("Driver") != null)
					_driver = Gorgon.Drivers[regKey.GetValue("Driver", "").ToString()];

				// Get video mode information.
				width = (int)regKey.GetValue("Width", 640);
				height = (int)regKey.GetValue("Height", 480);
				format = (BackBufferFormats)Enum.Parse(typeof(BackBufferFormats), regKey.GetValue("Format", "BufferRGB888").ToString());
				refresh = (int)regKey.GetValue("Refresh", 60);
				_interval = (VSyncIntervals)Enum.Parse(typeof(VSyncIntervals), regKey.GetValue("VSyncInterval", "IntervalNone").ToString());
				checkAllowBackground.Checked = Convert.ToBoolean(regKey.GetValue("AllowBackground",Gorgon.AllowBackgroundRendering));
				checkAllowScreensaver.Checked = Convert.ToBoolean(regKey.GetValue("AllowScreenSaver",Gorgon.AllowScreenSaver));
				_windowed = Convert.ToBoolean(regKey.GetValue("Windowed", "false").ToString());				
			}
			catch (SharpException sex)
			{
				width = 640;
				height = 480;
				refresh = 60;
				format = BackBufferFormats.BufferRGB888;
				_windowed = true;
				UI.ErrorBox(this, sex.Message, sex.ErrorLog);
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
				// Clean up.
				if (regKey != null)
					regKey.Close();
				regKey = null;
				
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
			}
		}

		/// <summary>
		/// Function to save the settings to the registry.
		/// </summary>
		private void SetSettings()
		{
			RegistryKey regKey = null;	// Gorgon registry key.

			try
			{
				regKey = Registry.CurrentUser.CreateSubKey("Software\\Tape_Worm\\Gorgon\\Examples");

				// Save settings.
				regKey.SetValue("Driver", _driver.DriverName);
				regKey.SetValue("Width", _mode.Width);
				regKey.SetValue("Height", _mode.Height);
				regKey.SetValue("Format", _mode.Format);
				regKey.SetValue("Refresh", _mode.RefreshRate);
				regKey.SetValue("Windowed", _windowed);
				regKey.SetValue("VSyncInterval",_interval);
				regKey.SetValue("AllowBackground",checkAllowBackground.Checked);
				regKey.SetValue("AllowScreensaver",checkAllowScreensaver.Checked);
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, "Could not save the settings to the registry.", sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Could not save the settings to the registry.", ex);
			}
			finally
			{
				// Clean up.
				if (regKey != null)
					regKey.Close();
				regKey = null;
			}
		}

        /// <summary>
        /// Function to close the dialog.
        /// </summary>
        /// <param name="reason">Reason for closure.</param>
        public void CloseDialog(DialogResult reason)
        {
            _result = reason;
            _fadeDirection = false;
            fadeTimer.Enabled = true;
        }
		#endregion        

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Closing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (_result == DialogResult.None)
			{
				DialogResult = DialogResult.None;
				_result = DialogResult.Cancel;
				if (e.CloseReason == CloseReason.UserClosing)
				{
                    CloseDialog(DialogResult.Cancel);
					e.Cancel = true;
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				_interval = VSyncIntervals.IntervalNone;
				checkAllowScreensaver.Checked = Gorgon.AllowScreenSaver;
				checkAllowBackground.Checked = Gorgon.AllowBackgroundRendering;

				// Default to 640x480x32 60Hz
				_mode = new VideoMode(640, 480, 60, BackBufferFormats.BufferRGB888);
				_windowed = true;
				_result = DialogResult.None;

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

				_fadeDirection = true;
				fadeTimer.Enabled = true;

				FillVSyncIntervals();
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, sEx.Message, sEx.ErrorLog);
                CloseDialog(DialogResult.Cancel);
            }
			catch (Exception Ex)
			{
				// Error message.
				UI.ErrorBox(this, Ex);
                CloseDialog(DialogResult.Cancel);
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
			catch
			{
				// Default to first device.
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
			catch (IndexOutOfBoundsException)
			{
				// Default to first video mode.
				_mode = _driver.VideoModes[0];
			}
		}

		/// <summary>
		/// Cancel button click event.
		/// </summary>
		/// <param name="sender">Sender of this event.</param>
		/// <param name="e">Event arguments.</param>
		private void cancel_Click(object sender, System.EventArgs e)
		{
            CloseDialog(DialogResult.Cancel);
		}

		/// <summary>
		/// Ok button click event.
		/// </summary>
		/// <param name="sender">Sender of this event.</param>
		/// <param name="e">Event arguments.</param>
		private void OK_Click(object sender, System.EventArgs e)
		{
			SetSettings();
            CloseDialog(DialogResult.OK);
		}

		/// <summary>
		/// Device info button click event.
		/// </summary>
		/// <param name="sender">Sender of this event.</param>
		/// <param name="e">Event arguments.</param>
		private void deviceInfo_Click(object sender, System.EventArgs e)
		{
			DeviceInformationDialog deviceInfo;		// Device info form.

			deviceInfo = new DeviceInformationDialog(_driver);
			deviceInfo.ShowDialog(this);
			deviceInfo.Dispose();
			deviceInfo = null;
		}

		/// <summary>
		/// Handles the Tick event of the fadeTimer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void fadeTimer_Tick(object sender, EventArgs e)
		{
			if (_fadeDirection)
			{
				Opacity += 0.025;
				if (Opacity >= 1.0)
					fadeTimer.Enabled = false;
			}
			else
			{
				Opacity -= 0.025;
				if (Opacity <= 0.1)
				{
					fadeTimer.Enabled = false;
					DialogResult = _result;
				}
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
				Height = 308;
			else
				Height = 227;
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
	}
}
