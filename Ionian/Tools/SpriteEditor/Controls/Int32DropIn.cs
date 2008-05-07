#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, May 03, 2008 8:12:22 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// 32bit Integer value animation drop in.
	/// </summary>
	public partial class Int32DropIn 
		: AnimationDropIn
	{
		#region Variables.
		private ToolStripComboBox comboInterpolation = null;			// Drop down.
		private int _currentValue = 0;									// Current value.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current key frame time.
		/// </summary>
		/// <value></value>
		public override float CurrentTime
		{
			get
			{
				return base.CurrentTime;
			}
			set
			{
				KeyInt32 currentFrame;		// Frame at the current time.
				base.CurrentTime = value;

				if ((CurrentTrack != null) && (CurrentTrack.KeyCount > 0))
				{
					currentFrame = CurrentTrack[CurrentTime] as KeyInt32;

					if (currentFrame != null)
						numericFloat.Value = (Decimal)currentFrame.Value;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboInterpolation control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void comboInterpolation_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch(comboInterpolation.Text.ToLower())
			{
				case "spline":
					CurrentTrack.InterpolationMode = InterpolationMode.Spline;
					break;
				case "linear":
					CurrentTrack.InterpolationMode = InterpolationMode.Linear;
					break;
				default:
					CurrentTrack.InterpolationMode = InterpolationMode.None;
					return;
			}
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericFloat control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void numericFloat_ValueChanged(object sender, EventArgs e)
		{
			_currentValue = (int)numericFloat.Value;
		}

		/// <summary>
		/// Function to reset the sprite.
		/// </summary>
		protected override void ResetSprite()
		{
			base.ResetSprite();

			_currentValue = (int)Sprite.Sprite.GetType().GetProperty(CurrentTrack.Name).GetValue(Sprite.Sprite, null);
			numericFloat.Value = (Decimal)_currentValue;
		}

		/// <summary>
		/// Function to handle enter key presses in numeric up down fields.
		/// </summary>
		/// <returns>TRUE if handled, FALSE if not.</returns>
		protected override bool NumericFieldEnterPressed()
		{
			SetKeyFrame();
			ValidateForm();
			return true;
		}
		
		/// <summary>
		/// Function to set the current key.
		/// </summary>
		protected override void SetKeyFrame()
		{
			base.SetKeyFrame();
			KeyInt32 key = null;						// Key that we're editing/creating.

			// Ensure that we have the current value.
			numericFloat_ValueChanged(this, EventArgs.Empty);

			// Now, check for an existing key.
			if (CurrentTrack.Contains(CurrentTime))
			{
				key = CurrentTrack[CurrentTime] as KeyInt32;
				key.Value = _currentValue;
			}
			else
			{
				key = new KeyInt32(CurrentTime, _currentValue);
				CurrentTrack.AddKey(key);
			}

			Sprite.Changed = true;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			KeyInt32 key = null;			// New key.
			Decimal minValue;				// Min value.
			Decimal maxValue;				// Max value.

			base.OnLoad(e);
			comboInterpolation.Text = CurrentTrack.InterpolationMode.ToString();
			//splitAnimation.SplitterDistance = 385;

			if (CurrentTrack.KeyCount == 0)
			{
				key = new KeyInt32(0.0f, (int)(Sprite.Sprite.GetType().GetProperty(CurrentTrack.Name).GetValue(Sprite.Sprite,null)));
				CurrentTrack.AddKey(key);
			}
			else
				key = CurrentTrack[0.0f] as KeyInt32;

			// Get the range.			
			if (CurrentTrack.DataRange == MinMaxRangeF.Empty)
			{
				minValue = (Decimal)Int32.MinValue;
				maxValue = (Decimal)Int32.MaxValue;
			}
			else
			{
				minValue = (Decimal)CurrentTrack.DataRange.Minimum;
				maxValue = (Decimal)CurrentTrack.DataRange.Maximum;
			}

			numericFloat.Minimum = minValue;
			numericFloat.Maximum = maxValue;
			numericFloat.Value = (Decimal)key.Value;
			labelName.Text = CurrentTrack.Name;
			ValidateForm();
		}

		/// <summary>
		/// Function to validate the form.
		/// </summary>
		protected override void ValidateForm()
		{
			base.ValidateForm();
			if (!IsPlaying)
				buttonSetKeyFrame.Enabled = true;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FloatDropIn"/> class.
		/// </summary>
		public Int32DropIn()
		{
			InitializeComponent();

			comboInterpolation = new ToolStripComboBox();
			comboInterpolation.Name = "comboInterpolation";
			comboInterpolation.Items.Add("Spline");
			comboInterpolation.Items.Add("Linear");
			comboInterpolation.Items.Add("None");
			comboInterpolation.DropDownStyle = ComboBoxStyle.DropDownList;
			comboInterpolation.SelectedIndexChanged += new EventHandler(comboInterpolation_SelectedIndexChanged);

			stripAnimation.Items.Add(new ToolStripSeparator());
			stripAnimation.Items.Add(new ToolStripLabel("Interpolation Mode:"));
			stripAnimation.Items.Add(comboInterpolation);
		}
		#endregion
	}
}
