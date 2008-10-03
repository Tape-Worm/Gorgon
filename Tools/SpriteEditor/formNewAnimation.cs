#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
		/// Handles the ValueChanged event of the numericLength control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void numericLength_ValueChanged(object sender, EventArgs e)
		{
			float time = 0.0f;		// Animation length in milliseconds.

			if (sender == numericFrameRate)
				numericLength.Increment = numericFrameRate.Value;

			time = (float)(numericLength.Value / numericFrameRate.Value);
			labelTimeCalculation.Text = "Animation will be " + time.ToString("0.000") + " seconds long.";

			if (time < 0.000001f)
				buttonOK.Enabled = false;
			else
				buttonOK.Enabled = true;
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

			// Calculate length of animation in milliseconds.
			time = (float)(numericLength.Value / numericFrameRate.Value) * 1000.0f;

			animation = _editAnimation;

			// Create the animation.
			if (animation == null)
			{
				animation = new Animation(textName.Text, time);
				animation.FrameRate = (int)numericFrameRate.Value;
				_current.Animations.Add(animation);
			}
			else
			{
				// Re-add if necessary.
				if (!_current.Animations.Contains(animation.Name))
					_current.Animations.Add(animation);

				animation.FrameRate = (int)numericFrameRate.Value;

				if (!MathUtility.EqualFloat(time,animation.Length, 0.0001f))
				{
					float timeScaler = 0.0f;		// Time scale.

					// Get the scale.
					timeScaler = time / animation.Length;

					// Set the time length.
					animation.Length = time;

					// If there's not much of a difference, then don't bother.
					if ((!MathUtility.EqualFloat(timeScaler, 0.0f, 0.0001f)) && (animation.HasKeys))
					{
						result = UI.ConfirmBox("This animation has keys assigned to it.\nWould you like to scale the time for the assigned keys?", false, true);

						if ((result & ConfirmationResult.Yes) == ConfirmationResult.Yes)
						{
							foreach (Track track in animation.Tracks)
								track.ScaleKeys(timeScaler);
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
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// Update our label.
			numericLength_ValueChanged(this, EventArgs.Empty);
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

			numericFrameRate.Value = (Decimal)_editAnimation.FrameRate;
			numericLength.Value = ((Decimal)_editAnimation.Length / 1000M) * numericFrameRate.Value;
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
	}
}