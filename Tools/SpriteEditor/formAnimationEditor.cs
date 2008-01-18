#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Tuesday, July 10, 2007 11:59:31 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpUtilities;
using SharpUtilities.Utility;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Enumeration for time units.
	/// </summary>
	public enum TimeUnits
	{
		/// <summary>
		/// Milliseconds.
		/// </summary>
		Milliseconds = 0,
		/// <summary>
		/// Seconds.
		/// </summary>
		Seconds = 1,
		/// <summary>
		/// Minutes.
		/// </summary>
		Minutes = 2
	}

	/// <summary>
	/// Interface for the animation editor.
	/// </summary>
	public partial class formAnimationEditor 
		: Form
	{
		#region Variables.
		private SpriteDocumentList _documents = null;					// Sprite document list.
		private Animation _animation = null;							// Current animation.
		private TimeUnits _currentUnit = TimeUnits.Milliseconds;		// Current time units.
		private frameAnimation _frameEditor = null;						// Frame animation editor.
		private colorAnimation _colorEditor = null;						// Color animation editor.
		private transformAnimation _transformEditor = null;				// Transformation editor.
		private bool _noevent = false;									// Flag to disable events.
		private IDropIn _dropIn = null;									// Drop in object.		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the currently loaded sprites.
		/// </summary>
		public SpriteDocumentList Sprites
		{
			get
			{
				return _documents;
			}
			set
			{
				_documents = value;
			}
		}

		/// <summary>
		/// Property to set or return the current animation.
		/// </summary>
		public Animation CurrentAnimation
		{
			get
			{
				return _animation;
			}
			set
			{
				_animation = value;

				if (_animation.Length < 1000.0f)
				{
					radioMilliseconds.Checked = true;
					UnitUpdate(radioMilliseconds, EventArgs.Empty);
				}

				if ((_animation.Length >= 1000.0f) && (_animation.Length < 60000.0f))
				{
					radioSeconds.Checked = true;
					UnitUpdate(radioSeconds, EventArgs.Empty);
				}

				if (_animation.Length >= 60000.0f)
				{
					radioMinutes.Checked = true;
					UnitUpdate(radioMinutes, EventArgs.Empty);
				}

				_frameEditor = new frameAnimation(_documents);
				_frameEditor.Dock = DockStyle.Fill;

				_colorEditor = new colorAnimation(_documents);
				_colorEditor.Dock = DockStyle.Fill;

				_transformEditor = new transformAnimation(_documents);
				_transformEditor.Dock = DockStyle.Fill;

				// Load the frame animation editor.
				comboTrack.Text = "animation frames";

				Text = "Animation Editor - " + _animation.Name;

				// Reset all the animations.
				foreach (Animation animation in _animation.Owner.Animations)
				{
					animation.Reset();
					_animation.Owner.ApplyAnimations();
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Scroll event of the trackTrack control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void trackTrack_Scroll(object sender, EventArgs e)
		{
			if (_noevent)
				return;
			numericTrackTime.Value = (decimal)trackTrack.Value;
			_dropIn.CurrentTime = GetConvertedTime();
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericTrackTime control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void numericTrackTime_ValueChanged(object sender, EventArgs e)
		{
			if (_noevent)
				return;
			trackTrack.Value = (int)numericTrackTime.Value;
			_dropIn.CurrentTime = GetConvertedTime();
		}

		/// <summary>
		/// Property to drop in the editor.
		/// </summary>
		/// <param name="editor">Editor to drop in.</param>
		private void DropInEditor(IDropIn editor)
		{
			float lastTime = 0.0f;		// Last time.
			
			// Remove the current control
			if (splitTrack.Panel1.Controls.Count != 0)
			{
				lastTime = ((IDropIn)splitTrack.Panel1.Controls[0]).CurrentTime;		// Get 'last' time.

				// Don't remove the editor.
				((IDropIn)splitTrack.Panel1.Controls[0]).CleanUp();
				splitTrack.Panel1.Controls.RemoveAt(0);
			}

			// Drop a frame editor in.
			splitTrack.Panel1.Controls.Add((Control)editor);
			_dropIn = editor;
			SetTime(lastTime);
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboTrack control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void comboTrack_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				// Load animation frame editor.
				if (string.Compare(comboTrack.Text, "animation frames", true) == 0)
				{
					DropInEditor(_frameEditor);

					// Reset time.
					_frameEditor.CurrentTrack = CurrentAnimation.FrameTrack;
				}

				if (string.Compare(comboTrack.Text, "color", true) == 0)
				{
					DropInEditor(_colorEditor);

					// Reset time.
					_colorEditor.CurrentTrack = CurrentAnimation.ColorTrack;
				}

				if (string.Compare(comboTrack.Text, "transformation", true) == 0)
				{
					DropInEditor(_transformEditor);

					// Reset time.
					_transformEditor.CurrentTrack = CurrentAnimation.TransformationTrack;
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, "Error while trying to open the track.", sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Error while trying to open the track.", ex);
			}
		}

		/// <summary>
		/// Function to validate the form.
		/// </summary>
		private void ValidateForm()
		{

			if (_animation.Length >= 1000.0f)
				radioSeconds.Enabled = true;
			else
				radioSeconds.Enabled = false;

			if (_animation.Length >= 60000.0f)
				radioMinutes.Enabled = true;
			else
				radioMinutes.Enabled = false;
		}

		/// <summary>
		/// Function to update the display for the selected unit.
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="e"></param>
		private void UnitUpdate(object Sender, EventArgs e)
		{
			Decimal maxTime = 0;						// Maximum animation time.
			int frequency = 0;							// Tick frequency.
			Decimal increment = 0;						// Increment.
			TimeUnits time = TimeUnits.Milliseconds;	// New time unit.
			Decimal currentTime = 0;					// Current time.

			if (Sender == radioSeconds)
				time = TimeUnits.Seconds;

			if (Sender == radioMinutes)
				time = TimeUnits.Minutes;
			
			switch(time)
			{
				case TimeUnits.Milliseconds:
					switch(_currentUnit)
					{
						case TimeUnits.Minutes:
							currentTime  = numericTrackTime.Value * 60000.0M;
							break;
						case TimeUnits.Seconds:
							currentTime  = numericTrackTime.Value * 1000.0M;
							break;
					}

					maxTime = (Decimal)(_animation.Length - 1.0f);
					labelUnit.Text = "msec.";
					break;
				case TimeUnits.Seconds:
					switch(_currentUnit)
					{
						case TimeUnits.Minutes:
							currentTime  = numericTrackTime.Value * 60.0M;							
							break;
						case TimeUnits.Milliseconds:
							currentTime  = numericTrackTime.Value / 1000.0M;
							break;
					}

					maxTime = (Decimal)((_animation.Length - 1.0f) / 1000.0f);
					labelUnit.Text = "sec.";
					break;
				case TimeUnits.Minutes:
					switch(_currentUnit)
					{
						case TimeUnits.Seconds:
							currentTime  = numericTrackTime.Value / 60.0M;							
							break;
						case TimeUnits.Milliseconds:
							currentTime  = numericTrackTime.Value / 60000.0M;
							break;
					}

					maxTime = (Decimal)((_animation.Length - 1.0f) / 60000.0f);
					labelUnit.Text = "min.";
					break;
			}

			if (maxTime >= 15.0M)
				frequency = (int)(maxTime / 15.0M);
			else
				frequency = 1;
			increment = (Decimal)frequency;

			// Clamp the increment.
			if (increment < 1.0M)
				increment = 1.0M;

			// Update the ranges for the controls.
			trackTrack.Maximum = (int)maxTime;
			trackTrack.TickFrequency = frequency;
			numericTrackTime.Maximum = maxTime;
			numericTrackTime.Value = currentTime;
			numericTrackTime.Increment = increment;
			_currentUnit = time;

			ValidateForm();
			if (_dropIn != null)
				_dropIn.CurrentTime = GetConvertedTime();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			try
			{
				// Ensure that the animation is in an enabled state upon exit.
				if (_animation != null)
					_animation.Enabled = true;

				Settings.Root = "AnimationEditor";
				Settings.SetSetting("WindowState", WindowState.ToString());				

				// Set window dimensions.
				if (WindowState != FormWindowState.Maximized)
				{
					Settings.SetSetting("Left", Left.ToString());
					Settings.SetSetting("Top", Top.ToString());
					Settings.SetSetting("Width", Width.ToString());
					Settings.SetSetting("Height", Height.ToString());
				}
				Settings.Root = null;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to save the animation window settings.",ex);
			}
		}

		/// <summary>
		/// Function to set the time on the timing controls.
		/// </summary>
		/// <param name="time">Time to set.</param>
		internal void SetTime(float time)
		{
			Decimal timeValue = (decimal)ConvertMillisecondsToUnit(time);

			if (timeValue < numericTrackTime.Minimum)
				timeValue = numericTrackTime.Minimum;
			if (timeValue > numericTrackTime.Maximum)
				timeValue = numericTrackTime.Maximum;

			_noevent = true;
			_dropIn.CurrentTime = time;
			numericTrackTime.Value = timeValue;
			trackTrack.Value = (int)numericTrackTime.Value;
			_noevent = false;
		}

		/// <summary>
		/// Function to convert the current time into milliseconds.
		/// </summary>
		/// <returns>Converted time value.</returns>
		public float GetConvertedTime()
		{
			float currentValue = (float)trackTrack.Value;		// Current track value.

			switch (_currentUnit)
			{
				case TimeUnits.Seconds:
					return currentValue * 1000.0f;
				case TimeUnits.Minutes:
					return currentValue * 60000.0f;
			}

			return currentValue;
		}

		/// <summary>
		/// Function to convert milliseconds to the requested unit.
		/// </summary>
		/// <param name="time">Time to convert.</param>
		/// <returns>Time in the units specified.</returns>
		public float ConvertMillisecondsToUnit(float time)
		{
			switch (_currentUnit)
			{
				case TimeUnits.Seconds:
					return time / 1000.0f;
				case TimeUnits.Minutes:
					return time / 60000.0f;
			}

			return time;
		}

		/// <summary>
		/// Function to retrieve animation editor settings.
		/// </summary>
		public void GetSettings()
		{
			Settings.Root = "AnimationEditor";
			WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), Settings.GetSetting("WindowState", "Normal"));

			// Set window dimensions.
			if (WindowState == FormWindowState.Normal)
			{
				Left = Convert.ToInt32(Settings.GetSetting("Left", formMain.Me.Left.ToString()));
				Top = Convert.ToInt32(Settings.GetSetting("Top", formMain.Me.Top.ToString()));
				Width = Convert.ToInt32(Settings.GetSetting("Width", "745"));
				Height = Convert.ToInt32(Settings.GetSetting("Height", "595"));
			}
			Settings.Root = null;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formAnimationEditor()
		{
			InitializeComponent();
		}
		#endregion
	}
}