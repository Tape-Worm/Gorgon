#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Thursday, August 03, 2006 4:13:57 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpUtilities;
using SharpUtilities.Utility;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Form to provide meta-data for gorgon fonts from mudge fonts.
	/// </summary>
	public partial class FontMetaData : Form
	{
		#region Properties.
		/// <summary>
		/// Property to set or return whether this dialog is only used to name the font or not.
		/// </summary>
		public bool OnlyName
		{
			get
			{
				if (Height == 325)
					return false;
				else
					return true;
			}
			set
			{
				if (value)
				{
					labelImageSize.Visible = false;
					numericImageWidth.Visible = false;
					numericImageHeight.Visible = false;
					labelBy.Visible = false;
					Height = 114;
				}
				else
				{
					labelImageSize.Visible = true;
					numericImageWidth.Visible = true;
					numericImageHeight.Visible = true;
					labelBy.Visible = true;
					Height = 325;
				}
			}
		}

		/// <summary>
		/// Property to set or return the size of the font image.
		/// </summary>
		public Drawing.Size FontImageSize
		{
			get
			{
				return new Drawing.Size((int)numericImageWidth.Value, (int)numericImageHeight.Value);
			}
			set
			{
				numericImageWidth.Value = (decimal)value.Width;
				numericImageHeight.Value = (decimal)value.Height;
			}
		}

		/// <summary>
		/// Property to set or return whether the font is anti-aliased or not.
		/// </summary>
		public bool AntiAliased
		{
			get
			{
				return checkAntiAlias.Checked;
			}
			set
			{
				checkAntiAlias.Checked = value;
			}
		}

		/// <summary>
		/// Property to set or return the font name.
		/// </summary>
		public string FontName
		{
			get
			{
				return textFontName.Text;
			}
			set
			{
				textFontName.Text = value;
				ValidateButtons();
				UpdatePreview();
			}
		}

		/// <summary>
		/// Property to set or return what font this font derives from.
		/// </summary>
		public string SourceFont
		{
			get
			{
				return comboFontList.Text;
			}
			set
			{
				comboFontList.Text = value;
				ValidateButtons();
				UpdatePreview();
			}
		}

		/// <summary>
		/// Property to set or return the source font size.
		/// </summary>
		public float SourceFontSize
		{
			get
			{
				return (float)numericFontSize.Value;
			}
			set
			{
				numericFontSize.Value = (decimal)value;
				ValidateButtons();
				UpdatePreview();
			}
		}

		/// <summary>
		/// Property to set or return whether the font represents a bold font or not.
		/// </summary>
		public bool IsBold
		{
			get
			{
				return checkBold.Checked;
			}
			set
			{
				checkBold.Checked = value;
				UpdatePreview();
			}
		}

		/// <summary>
		/// Property to set or return whether the font represents a font with italics or not.
		/// </summary>
		public bool IsItalics
		{
			get
			{
				return checkItalics.Checked;
			}
			set
			{
				checkItalics.Checked = value;
				UpdatePreview();
			}
		}

		/// <summary>
		/// Property to set or return whether the font represents an underlined font or not.
		/// </summary>
		public bool IsUnderlined
		{
			get
			{
				return checkUnderline.Checked;
			}
			set
			{
				checkUnderline.Checked = value;
				UpdatePreview();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the preview label.
		/// </summary>
		private void UpdatePreview()
		{
			try
			{				
				Drawing.FontStyle style = Drawing.FontStyle.Regular;		// Font style.						

				
				if (IsBold)
					style |= Drawing.FontStyle.Bold;
				if (IsItalics)
					style |= Drawing.FontStyle.Italic;
				if (IsUnderlined)
					style |= Drawing.FontStyle.Underline;

				if (textFontName.Text.Trim() != string.Empty)
					labelPreview.Text = textFontName.Text + "\n" + comboFontList.Text + " " + numericFontSize.Value.ToString("##0.0") + "pt";
				else
					labelPreview.Text = comboFontList.Text + " " + numericFontSize.Value.ToString("##0.0") + "pt";
				labelPreview.Font = new Drawing.Font(comboFontList.Text, (float)numericFontSize.Value, style);
			}
			catch(Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the FontMetaData control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void FontMetaData_KeyDown(object sender, KeyEventArgs e)
		{
			ValidateButtons();
			if ((buttonOK.Enabled) && (e.KeyCode == Keys.Enter))
				DialogResult = DialogResult.OK;
			if (e.KeyCode == Keys.Escape)
				DialogResult = DialogResult.Cancel;
		}

		/// <summary>
		/// Function to validate the buttons.
		/// </summary>
		private void ValidateButtons()
		{
			buttonOK.Enabled = false;

			if ((textFontName.Text.Trim() != string.Empty) && ((OnlyName) || (comboFontList.Text != string.Empty) && (numericFontSize.Value > 0.0M)))
				buttonOK.Enabled = true;
		}

		/// <summary>
		/// Handles the TextChanged event of the textFontName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void textFontName_TextChanged(object sender, EventArgs e)
		{
			if (comboFontList.Items.Contains(textFontName.Text))
				comboFontList.Text = textFontName.Text;
			ValidateButtons();
			UpdatePreview();
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboFontList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void comboFontList_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericFontSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void numericFontSize_ValueChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		/// <summary>
		/// Handles the CheckedChanged event of the checkBold control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void checkBold_CheckedChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		/// <summary>
		/// Handles the CheckedChanged event of the checkItalics control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void checkItalics_CheckedChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		/// <summary>
		/// Handles the CheckedChanged event of the checkUnderline control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void checkUnderline_CheckedChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdatePreview();
			ValidateButtons();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public FontMetaData()
		{
			Drawing.Text.InstalledFontCollection fonts;		// Font collection.

			InitializeComponent();

			// Get the font faces.
			fonts = new Drawing.Text.InstalledFontCollection();

			// Fill the font combo.
			comboFontList.Items.Clear();
			foreach (Drawing.FontFamily ttFont in fonts.Families)
			{
				// Only add those fonts that support regular styles.
				if (ttFont.IsStyleAvailable(Drawing.FontStyle.Regular))
					comboFontList.Items.Add(ttFont.Name);
			}

			if (comboFontList.Items.Contains(textFontName.Text))
				comboFontList.Text = textFontName.Text;
			else
			{
				if (comboFontList.Items.Contains("Arial"))
					comboFontList.Text = "Arial";
				else
					comboFontList.Text = comboFontList.Items[0].ToString();
			}
		}
		#endregion
	}
}