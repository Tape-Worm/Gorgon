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
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Interface for the sprite extraction functionality.
	/// </summary>
	public partial class formSpriteExtractOptions 
		: Form
	{
		#region Variables.
		private ColorPicker _picker = null;				// Color picker dialog.
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
		/// Handles the Click event of the buttonColorSelect control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonColorSelect_Click(object sender, EventArgs e)
		{
			try
			{
				_picker.Color = _finder.MaskColor;
				_picker.UseAlpha = checkAlpha.Checked;

				if (_picker.ShowDialog(this) == DialogResult.OK)
				{
					_finder.MaskColor = _picker.Color;

					// Extract the alpha.
					if (!checkAlpha.Checked)
						_finder.MaskColor = Color.FromArgb(255, _finder.MaskColor);
					UpdateInterface();
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "There was an error trying to retrieve the masking color.", ex);
			}
		}

		/// <summary>
		/// Handles the Paint event of the pictureMaskColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
		private void pictureMaskColor_Paint(object sender, PaintEventArgs e)
		{
			Color theColor;
			SolidBrush brush;

			theColor = Color.White;

			if (checkColor.Checked)
				theColor = Color.FromArgb(255, _finder.MaskColor);

			if (checkAlpha.Checked)
				theColor = Color.FromArgb(_finder.MaskColor.A, theColor);

			brush = new SolidBrush(theColor);
			e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
			e.Graphics.FillRectangle(brush, pictureMaskColor.DisplayRectangle);
			brush.Dispose();
		}

		/// <summary>
		/// Function to update the interface.
		/// </summary>
		private void UpdateInterface()
		{		
			pictureMaskColor.Invalidate(true);
			checkAlpha.Checked = (_finder.MaskOptions & MaskOptions.Alpha) == MaskOptions.Alpha;
			checkColor.Checked = (_finder.MaskOptions & MaskOptions.Color) == MaskOptions.Color;

			if (!checkAlpha.Checked)
				checkColor.Enabled = false;
			else
				checkColor.Enabled = true;
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
		/// Handles the Click event of the checkColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void checkColor_Click(object sender, EventArgs e)
		{
			if (checkColor.Checked)
				_finder.MaskOptions |= MaskOptions.Color;
			else
				_finder.MaskOptions &= ~MaskOptions.Color;

			if ((!checkAlpha.Checked) && (!checkColor.Checked))
			{
				checkColor.Checked = true;
				_finder.MaskOptions = MaskOptions.Color;
			}

			UpdateInterface();
		}

		/// <summary>
		/// Handles the Click event of the checkAlpha control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void checkAlpha_Click(object sender, EventArgs e)
		{
			if (checkAlpha.Checked)
				_finder.MaskOptions |= MaskOptions.Alpha;
			else
				_finder.MaskOptions &= ~MaskOptions.Alpha;

			if ((!checkAlpha.Checked) && (!checkColor.Checked))
			{
				checkColor.Checked = true;
				_finder.MaskOptions = MaskOptions.Color;
			}

			UpdateInterface();
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
		/// Handles the KeyDown event of the formSpriteExtractOptions control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void formSpriteExtractOptions_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
				DialogResult = DialogResult.Cancel;

			if ((e.KeyCode == Keys.Enter) && (buttonOK.Enabled))
				DialogResult = DialogResult.OK;
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
			Settings.SetSetting("MaskColor", _finder.MaskColor.ToArgb().ToString());
			Settings.SetSetting("MaskOptions", _finder.MaskOptions.ToString());
			Settings.Root = null;

			if (_picker != null)
				_picker.Dispose();

			_picker = null;
		}

		/// <summary>
		/// Function to retrieve the settings.
		/// </summary>
		public void GetSettings()
		{
			Settings.Root = "SpriteExtraction";
			textPrefix.Text = Settings.GetSetting("NamingPrefix", _defaultPrefix);
			_finder.MaskColor = Color.FromArgb(Convert.ToInt32(Settings.GetSetting("MaskColor", Color.Pink.ToArgb().ToString())));
			_finder.MaskOptions = (MaskOptions)Enum.Parse(typeof(MaskOptions), Settings.GetSetting("MaskOptions", "3"));
			Settings.Root = null;

			UpdateInterface();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formSpriteExtractOptions()
		{
			InitializeComponent();

			textPrefix.Focus();
			_picker = new ColorPicker();
			_finder = new SpriteFinder();
			_defaultPrefix = "ExtractedSprite_";
		}
		#endregion
	}
}