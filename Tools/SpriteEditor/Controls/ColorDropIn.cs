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
// Created: Saturday, May 03, 2008 5:29:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Drop in for color animation.
	/// </summary>
	public partial class ColorDropIn 
		: AnimationDropIn
	{
		#region Variables.
		private ToolStripComboBox comboInterpolation = null;			// Drop down.
		private Drawing.Color _currentColor = Drawing.Color.White;		// Current color.
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
				KeyColor currentFrame;		// Frame at the current time.
				base.CurrentTime = value;

				if ((CurrentTrack != null) && (CurrentTrack.KeyCount > 0))
				{
					currentFrame = CurrentTrack[CurrentTime] as KeyColor;

					if (currentFrame != null)
					{
						numericA.Value = currentFrame.Value.A;
						numericR.Value = currentFrame.Value.R;
						numericG.Value = currentFrame.Value.G;
						numericB.Value = currentFrame.Value.B;
					}
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the ValueChanged event of the numericA control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void numericA_ValueChanged(object sender, EventArgs e)
		{
			_currentColor = Drawing.Color.FromArgb((int)numericA.Value, (int)numericR.Value, (int)numericG.Value, (int)numericB.Value);
			pictureColor.Refresh();
		}

		/// <summary>
		/// Handles the Paint event of the pictureColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
		private void pictureColor_Paint(object sender, PaintEventArgs e)
		{
			Drawing.SolidBrush brush = new Drawing.SolidBrush(_currentColor);
			e.Graphics.CompositingMode = Drawing.Drawing2D.CompositingMode.SourceOver;
			e.Graphics.FillRectangle(brush, pictureColor.ClientRectangle);
			brush.Dispose();
		}

		/// <summary>
		/// Handles the Click event of the buttonSelectColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonSelectColor_Click(object sender, EventArgs e)
		{
			ColorPicker picker = null;		// Color picker.

			try
			{
				picker = new ColorPicker();
				picker.Color = _currentColor;
				if (picker.ShowDialog(this.ParentForm) == DialogResult.OK)
				{
					numericR.Value = picker.Color.R;
					numericG.Value = picker.Color.G;
					numericB.Value = picker.Color.B;
					numericA.Value = picker.Color.A;
					if (checkAutoKey.Checked)
					{
						SetKeyFrame();
						ValidateForm();
					}
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this.ParentForm, "Error opening color dialog.", ex);
			}
			finally
			{
				if (picker != null)
					picker.Dispose();
			}
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboInterpolation control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void comboInterpolation_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (string.Compare(comboInterpolation.Text, "linear", true) == 0)
				CurrentTrack.InterpolationMode = InterpolationMode.Linear;
			else
				CurrentTrack.InterpolationMode = InterpolationMode.None;
		}

		/// <summary>
		/// Handles the Click event of the checkAutoKey control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void checkAutoKey_Click(object sender, EventArgs e)
		{
			Settings.Root = "AnimationDropInSettings";
			Settings.SetSetting("Color_AutoKey", checkAutoKey.Checked.ToString());
			Settings.Root = null;
		}

		/// <summary>
		/// Function to handle enter key presses in numeric up down fields.
		/// </summary>
		/// <returns>TRUE if handled, FALSE if not.</returns>
		protected override bool NumericFieldEnterPressed()
		{
			numericA_ValueChanged(this, EventArgs.Empty);
			SetKeyFrame();
			ValidateForm();
			return true;
		}

		/// <summary>
		/// Function to set the current key.
		/// </summary>
		protected override void SetKeyFrameImpl()
		{
			KeyColor key = null;						// Key that we're editing/creating.
			base.SetKeyFrameImpl();

			// Ensure that we have the current value.
			numericA_ValueChanged(this, EventArgs.Empty);

			// Now, check for an existing key.
			if (CurrentTrack.Contains(CurrentTime))
			{
				key = CurrentTrack[CurrentTime] as KeyColor;
				key.Value = _currentColor;
			}
			else
			{
				key = new KeyColor(CurrentTime, _currentColor);
				CurrentTrack.AddKey(key);
			}

			Sprite.Changed = true;
		}

		/// <summary>
		/// Function to validate the form.
		/// </summary>
		protected override void ValidateForm()
		{
			base.ValidateForm();
			if (!IsPlaying)
				buttonSetKeyFrame.Enabled = true;
			pictureColor.Refresh();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			comboInterpolation.Text = CurrentTrack.InterpolationMode.ToString();
			// Automatically add the first key.
			if (CurrentTrack.KeyCount == 0)
				_currentColor = Sprite.Sprite.Color;
			else
				_currentColor = ((KeyColor)CurrentTrack[0]).Value;				

			numericA.Value = _currentColor.A;
			numericR.Value = _currentColor.R;
			numericG.Value = _currentColor.G;
			numericB.Value = _currentColor.B;

			numericA.ValueChanged += new EventHandler(numericA_ValueChanged);
			numericR.ValueChanged += new EventHandler(numericA_ValueChanged);
			numericG.ValueChanged += new EventHandler(numericA_ValueChanged);
			numericB.ValueChanged += new EventHandler(numericA_ValueChanged);

			Settings.Root = "AnimationDropInSettings";
			checkAutoKey.Checked = Convert.ToBoolean(Settings.GetSetting("Color_AutoKey", "true"));
			Settings.Root = null;

			ValidateForm();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ColorDropIn"/> class.
		/// </summary>
		public ColorDropIn()
		{
			InitializeComponent();

			comboInterpolation = new ToolStripComboBox();
			comboInterpolation.Name = "comboInterpolation";
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
