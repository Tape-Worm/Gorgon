#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, May 03, 2008 8:12:20 PM
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
	/// 2D vector value animation drop in.
	/// </summary>
	public partial class Vector2DDropIn 
		: AnimationDropIn
	{
		#region Variables.
		private ToolStripComboBox comboInterpolation = null;			// Drop down.
		private Vector2D _currentValue = Vector2D.Zero;					// Current value.
		private bool _isDragging = false;								// Flag to indicate that we're dragging.
		private Sprite _spriteClone = null;								// Clone of the sprite.
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
				KeyVector2D currentFrame;		// Frame at the current time.
				base.CurrentTime = value;

				if ((CurrentTrack != null) && (CurrentTrack.KeyCount > 0))
				{
					currentFrame = CurrentTrack[CurrentTime] as KeyVector2D;

					if (currentFrame != null)
					{
						numericX.Value = (Decimal)currentFrame.Value.X;
						numericY.Value = (Decimal)currentFrame.Value.Y;
					}
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the MouseDown event of the panelRender control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void panelRender_MouseDown(object sender, MouseEventArgs e)
		{
			if ((Sprite.Sprite.AABB.Contains(e.Location)) && (CurrentTrack.EditCanDragValues))
			{
				_isDragging = true;
				_spriteClone = Sprite.Sprite.Clone() as Sprite;
				foreach (Animation animation in _spriteClone.Animations)
					animation.Enabled = false;
				_spriteClone.Color = Color.FromArgb(200, 200, 200, 255);
			}
		}

		/// <summary>
		/// Handles the MouseMove event of the panelRender control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void panelRender_MouseMove(object sender, MouseEventArgs e)
		{
			if ((_isDragging) && (e.Button == MouseButtons.Left) && (_spriteClone != null))
				typeof(Sprite).GetProperty(CurrentTrack.Name).SetValue(_spriteClone, new Vector2D(e.Location), null);
		}

		/// <summary>
		/// Handles the MouseUp event of the panelRender control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void panelRender_MouseUp(object sender, MouseEventArgs e)
		{
			if (_isDragging)
			{
				numericX.Value = (Decimal)_spriteClone.Position.X;
				numericY.Value = (Decimal)_spriteClone.Position.Y;
				SetKeyFrame();
				ValidateForm();
				_isDragging = false;
				_spriteClone = null;
			}
		}

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
			_currentValue.X = (float)numericX.Value;
			_currentValue.Y = (float)numericY.Value;
		}

		/// <summary>
		/// Function to allow rendering code from a child class.
		/// </summary>
		protected override void IdleEvent()
		{
			base.IdleEvent();

			if (_spriteClone != null)
				_spriteClone.Draw();
		}

		/// <summary>
		/// Function to reset the sprite.
		/// </summary>
		protected override void ResetSprite()
		{
			Vector2D value = Vector2D.Zero;		// Value.
			base.ResetSprite();

			value = (Vector2D)Sprite.Sprite.GetType().GetProperty(CurrentTrack.Name).GetValue(Sprite.Sprite, null);
			numericX.Value = (Decimal)value.X;
			numericY.Value = (Decimal)value.Y;
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
			KeyVector2D key = null;						// Key that we're editing/creating.

			// Ensure that we have the current value.
			numericFloat_ValueChanged(this, EventArgs.Empty);

			// Now, check for an existing key.
			if (CurrentTrack.Contains(CurrentTime))
			{
				key = CurrentTrack[CurrentTime] as KeyVector2D;
				key.Value = _currentValue;
			}
			else
			{
				key = new KeyVector2D(CurrentTime, _currentValue);
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
			KeyVector2D key = null;		// New key.
			float minValue = 0.0f;		// Min value.
			float maxValue = 0.0f;		// Max value.

			base.OnLoad(e);
			comboInterpolation.Text = CurrentTrack.InterpolationMode.ToString();

			if (CurrentTrack.KeyCount == 0)
			{
				key = new KeyVector2D(0.0f, (Vector2D)(Sprite.Sprite.GetType().GetProperty(CurrentTrack.Name).GetValue(Sprite.Sprite,null)));
				CurrentTrack.AddKey(key);
			}
			else
				key = CurrentTrack[0.0f] as KeyVector2D;

			// Get the range.
			minValue = CurrentTrack.DataRange.Minimum;
			maxValue = CurrentTrack.DataRange.Maximum;

			if (minValue < (float)Decimal.MinValue / 2.0f)
				minValue = (float)Decimal.MinValue / 2.0f;
			if (maxValue > (float)Decimal.MaxValue / 2.0f)
				maxValue = (float)Decimal.MaxValue / 2.0f;
			numericX.Minimum = (Decimal)minValue;
			numericX.Maximum = (Decimal)maxValue;
			numericY.Minimum = (Decimal)minValue;
			numericY.Maximum = (Decimal)maxValue;
			numericX.Value = (Decimal)key.Value.X;
			numericY.Value = (Decimal)key.Value.Y;
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
		/// Initializes a new instance of the <see cref="Vector2DDropIn"/> class.
		/// </summary>
		public Vector2DDropIn()
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
