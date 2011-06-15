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
// Created: Saturday, June 23, 2007 6:21:03 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Graphics.Tools;
using GorgonLibrary.Graphics.Tools.Controls;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Interface for the sprite grid extraction functionality.
	/// </summary>
	public partial class formSpriteGridExtractOptions 
		: Form
	{
		#region Variables.
		private string _defaultPrefix = string.Empty;	// Default name prefix.
		private SpriteFinder _finder = null;			// Sprite finder.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the sprite finder.
		/// </summary>
		public SpriteFinder Finder
		{
			get
			{
				return _finder;
			}
		}

		/// <summary>
		/// Property to return the prefix for the sprite names.
		/// </summary>
		public string Prefix
		{
			get
			{
				return textPrefix.Text;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the count.
		/// </summary>
		private void UpdateCount()
		{
			Rectangle constraint = new Rectangle((int)numericLeft.Value, (int)numericTop.Value, (int)numericConstraintWidth.Value, (int)numericConstraintHeight.Value);
			int totalImageSize = constraint.Width * constraint.Height;
			int spriteSize = ((int)numericSpacingX.Value + (int)numericCellWidth.Value) * ((int)numericSpacingY.Value + (int)numericCellHeight.Value);

			// If the constraint is limited to the selected image size, then we can't get the sprite count.
			if ((constraint.Width == 0) || (constraint.Height == 0))
			{
				labelSpriteCount.Text = "Estimated sprite count: Varies with image size.";
				return;
			}

			if (spriteSize == 0)
				labelSpriteCount.Text = "Estimated sprite count: 0";
			else
			{
				int spriteCount = totalImageSize / spriteSize;		// Sprite count.

				labelSpriteCount.Text = "Estimated sprite count: " + spriteCount.ToString();
			}
		}

		/// <summary>
		/// Handles the Leave event of the textPrefix control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textPrefix_Leave(object sender, EventArgs e)
		{
			if (textPrefix.Text == string.Empty)
				textPrefix.Text = _defaultPrefix;
		}

		/// <summary>
		/// Handles the Click event of the buttonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{
			// Set the grid settings.
			_finder.GridConstraint = new Rectangle((int)numericLeft.Value, (int)numericTop.Value, (int)numericConstraintWidth.Value, (int)numericConstraintHeight.Value);
			_finder.GridCellSize = new Vector2D((float)numericCellWidth.Value, (float)numericCellHeight.Value);
			_finder.GridCellSpacing = new Vector2D((float)numericSpacingX.Value, (float)numericSpacingY.Value);
			_finder.TopToBottom = checkDirection.Checked;
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericLeft control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void numericLeft_ValueChanged(object sender, EventArgs e)
		{
			Rectangle constraint = new Rectangle((int)numericLeft.Value, (int)numericTop.Value, (int)numericConstraintWidth.Value, (int)numericConstraintHeight.Value);
			// Left: 32767 Top: 32767 Right: 32767 Bottom: 32767
			labelConstraint.Text = "Left: " + constraint.Left.ToString() + " Top: " + constraint.Top.ToString();
			if (constraint.Width == 0)
				labelConstraint.Text += " Right: Image width";
			else
				labelConstraint.Text += " Right: " + constraint.Right.ToString();

			if (constraint.Height == 0)
				labelConstraint.Text += " Bottom: Image height";
			else
				labelConstraint.Text += " Bottom: " + constraint.Bottom.ToString();

			UpdateCount();
		}

		/// <summary>
		/// Handles the TextChanged event of the textPrefix control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textPrefix_TextChanged(object sender, EventArgs e)
		{
			if (textPrefix.Text == string.Empty)
				buttonOK.Enabled = false;
			else
				buttonOK.Enabled = true;
		}

		/// <summary>
		/// Handles the KeyDown event of the formSpriteGridExtractOptions control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void formSpriteGridExtractOptions_KeyDown(object sender, KeyEventArgs e)
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
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			Settings.Root = "SpriteExtraction";
			Settings.SetSetting("NamingPrefix", textPrefix.Text);
			Settings.SetSetting("Left", numericLeft.Value.ToString());
			Settings.SetSetting("Top", numericTop.Value.ToString());
			Settings.SetSetting("Width", numericConstraintWidth.Value.ToString());
			Settings.SetSetting("Height", numericConstraintHeight.Value.ToString());
			Settings.SetSetting("CellSpacingX", numericSpacingX.Value.ToString());
			Settings.SetSetting("CellSpacingY", numericSpacingY.Value.ToString());
			Settings.SetSetting("CellWidth", numericCellWidth.Value.ToString());
			Settings.SetSetting("CellHeight", numericCellHeight.Value.ToString());
			Settings.Root = null;
		}

		/// <summary>
		/// Function to retrieve the settings.
		/// </summary>
		public void GetSettings()
		{
			// Set constraints to the numeric domain controls.
			numericConstraintWidth.Maximum = numericCellWidth.Maximum = numericLeft.Maximum = Gorgon.CurrentDriver.MaximumTextureWidth;
			numericConstraintHeight.Maximum = numericCellHeight.Maximum = numericTop.Maximum = Gorgon.CurrentDriver.MaximumTextureHeight;
			numericSpacingX.Maximum = Gorgon.CurrentDriver.MaximumTextureWidth / 2;
			numericSpacingY.Maximum = Gorgon.CurrentDriver.MaximumTextureHeight / 2;

			Settings.Root = "SpriteExtraction";
			textPrefix.Text = Settings.GetSetting("NamingPrefix", _defaultPrefix);
			numericLeft.Value = Convert.ToDecimal(Settings.GetSetting("Left", "0"));
			numericTop.Value = Convert.ToDecimal(Settings.GetSetting("Top", "0"));
			numericConstraintWidth.Value = Convert.ToDecimal(Settings.GetSetting("Width", "0"));
			numericConstraintHeight.Value = Convert.ToDecimal(Settings.GetSetting("Height", "0"));
			numericSpacingX.Value = Convert.ToDecimal(Settings.GetSetting("CellSpacingX", "0"));
			numericSpacingY.Value = Convert.ToDecimal(Settings.GetSetting("CellSpacingY", "0"));
			numericCellWidth.Value = Convert.ToDecimal(Settings.GetSetting("CellWidth", "32"));
			numericCellHeight.Value = Convert.ToDecimal(Settings.GetSetting("CellHeight", "32"));
			Settings.Root = null;

			numericLeft_ValueChanged(this, EventArgs.Empty);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formSpriteGridExtractOptions()
		{
			InitializeComponent();

			textPrefix.Focus();
			_finder = new SpriteFinder();
			_defaultPrefix = "ExtractedSprite_";
		}
		#endregion
	}
}