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
// Created: Sunday, July 08, 2007 11:33:07 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Interface for adding a new animation or editing one.
	/// </summary>
	public partial class formNewAnimation 
		: Form
	{
		#region Variables.
		private Sprite _current = null;								// Current sprite.
		private Animation _editAnimation = null;					// Animation being edited.
		private TimeUnits _currentUnit = TimeUnits.Milliseconds;	// Current time units.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the sprite being edited.
		/// </summary>
		public Sprite CurrentSprite
		{
			set
			{
				_current = value;
			}
			get
			{
				return _current;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the TextChanged event of the textName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textName_TextChanged(object sender, EventArgs e)
		{
			if (textName.Text == string.Empty)
				buttonOK.Enabled = false;
			else
				buttonOK.Enabled = true;
		}

		/// <summary>
		/// Function to edit an animation.
		/// </summary>
		/// <param name="animationName">Name of the animation.</param>
		public void EditAnimation(string animationName)
		{
			_editAnimation = _current.Animations[animationName];

			Text = "Edit - " + _editAnimation.Name;
			textName.Text = _editAnimation.Name;

			if (_editAnimation.Looped)
				checkLoop.Checked = true;

			numericLength.Value = (Decimal)_editAnimation.Length;

			if (_editAnimation.Length < 1000.0f)
			{
				radioMilliseconds.Checked = true;
				UnitUpdate(radioMilliseconds, EventArgs.Empty);
			}

			if ((_editAnimation.Length >= 1000.0f) && (_editAnimation.Length < 60000.0f))
			{
				radioSeconds.Checked = true;
				UnitUpdate(radioSeconds, EventArgs.Empty);
			}

			if (_editAnimation.Length >= 60000.0f)
			{
				radioMinutes.Checked = true;
				UnitUpdate(radioMinutes, EventArgs.Empty);
			}			
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formNewAnimation()
		{
			InitializeComponent();
		}
		#endregion

		/// <summary>
		/// Handles the CheckedChanged event of the radioMilliseconds control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void UnitUpdate(object sender, EventArgs e)
		{
			TimeUnits time = TimeUnits.Milliseconds;	// New time unit.
			Decimal currentTime = 0;					// Current time.			


			if (sender == radioSeconds)
				time = TimeUnits.Seconds;

			if (sender == radioMinutes)
				time = TimeUnits.Minutes;

			currentTime = (Decimal)numericLength.Value;

			switch (time)
			{
				case TimeUnits.Milliseconds:
					switch (_currentUnit)
					{
						case TimeUnits.Minutes:
							currentTime = numericLength.Value * 60000.0M;
							break;
						case TimeUnits.Seconds:
							currentTime = numericLength.Value * 1000.0M;
							break;
					}
					numericLength.DecimalPlaces = 0;
					numericLength.Minimum = 1M;
					numericLength.Increment = 1000.0M;
					labelUnit.Text = "milliseconds";
					break;
				case TimeUnits.Seconds:
					switch (_currentUnit)
					{
						case TimeUnits.Minutes:
							currentTime = numericLength.Value * 60.0M;
							break;
						case TimeUnits.Milliseconds:
							currentTime = numericLength.Value / 1000.0M;
							break;
					}

					numericLength.DecimalPlaces = 3;
					numericLength.Minimum = 0.001M;
					numericLength.Increment = 100.0M;
					labelUnit.Text = "seconds";
					break;
				case TimeUnits.Minutes:
					switch (_currentUnit)
					{
						case TimeUnits.Seconds:
							currentTime = numericLength.Value / 60.0M;
							break;
						case TimeUnits.Milliseconds:
							currentTime = numericLength.Value / 60000.0M;
							break;
					}
					numericLength.DecimalPlaces = 6;
					numericLength.Minimum = 1.0M / 60000.0M;
					numericLength.Increment = 10.0M;
					labelUnit.Text = "minutes";
					break;
			}

			_currentUnit = time;
			numericLength.Value = currentTime;						
		}

		/// <summary>
		/// Handles the Click event of the buttonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{
			Animation animation = null;								// Animation object.
			float time = 0.0f;										// Timeframe.
			ConfirmationResult result = ConfirmationResult.None;	// Confirmation result.

			if (_current.Animations.Contains(textName.Text)) 
			{
				// If the name has changed, and it already exists ask to overwrite.
				if (((_editAnimation != null) && (string.Compare(textName.Text, _editAnimation.Name, true) != 0)) || (_editAnimation == null))
				{
					if (UI.ConfirmBox("The animation '" + textName.Text + "' already exists.\nDo you wish to overwrite it?") == ConfirmationResult.No)
					{
						DialogResult = DialogResult.None;
						return;
					}

					// Remove the previous animation.
					_current.Animations.Remove(textName.Text);
				}
			}

			time = (float)numericLength.Value;

			// Convert to milliseconds.
			if (radioSeconds.Checked)
				time *= 1000.0f; 
			if (radioMinutes.Checked)
				time *= 60000.0f;

			animation = _editAnimation;

			// Create the animation.
			if (animation == null)
				animation = _current.Animations.Create(textName.Text, time);
			else
			{
				if (time != animation.Length)
				{
					float timeScaler = 0.0f;		// Time scale.

					// Get the scale.
					timeScaler = time / animation.Length;

					// Set the time length.
					animation.Length = time;

					// If there's not much of a difference, then don't bother.
					if (!MathUtility.EqualFloat(timeScaler, 0.0f, 0.0001f))
					{
						// Scale for each track type.
						if (animation.FrameTrack.KeyCount > 0)
						{
							result = UI.ConfirmBox("This animation has frame keys assigned to it.\nWould you like to scale the time for the assigned frame keys?", false, true);

							if ((result & ConfirmationResult.Yes) == ConfirmationResult.Yes)
								animation.FrameTrack.ScaleKeys(timeScaler);
						}

						if (animation.TransformationTrack.KeyCount > 0)
						{
							if ((result & ConfirmationResult.ToAll) != ConfirmationResult.ToAll)
								result = UI.ConfirmBox("This animation has transformation keys assigned to it.\nWould you like to scale the time for the assigned transformation keys?", false, true);

							if ((result & ConfirmationResult.Yes) == ConfirmationResult.Yes)
								animation.TransformationTrack.ScaleKeys(timeScaler);
						}

						if (animation.ColorTrack.KeyCount > 0)
						{
							if ((result & ConfirmationResult.ToAll) != ConfirmationResult.ToAll)
								result = UI.ConfirmBox("This animation has color keys assigned to it.\nWould you like to scale the time for the assigned color keys?");

							if ((result & ConfirmationResult.Yes) == ConfirmationResult.Yes)
								animation.ColorTrack.ScaleKeys(timeScaler);
						}
					}
				}				

				// Rename if necessary.
				if (string.Compare(animation.Name, textName.Text, true) != 0)
					_current.Animations.Rename(animation.Name, textName.Text);
			}

			animation.Looped = checkLoop.Checked;
		}

		/// <summary>
		/// Handles the KeyDown event of the formNewAnimation control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void formNewAnimation_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
				DialogResult = DialogResult.Cancel;

			if ((e.KeyCode == Keys.Enter) && (buttonOK.Enabled))
			{
				DialogResult = DialogResult.OK;
				buttonOK_Click(this, EventArgs.Empty);				
			}
		}
	}
}