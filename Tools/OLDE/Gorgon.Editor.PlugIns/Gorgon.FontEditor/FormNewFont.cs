#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, March 9, 2013 6:38:42 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.FontEditorPlugIn.Properties;
using Gorgon.Graphics;
using Gorgon.UI;

namespace Gorgon.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Font creation form.
	/// </summary>
	partial class FormNewFont 
		: FlatForm
	{
		#region Variables.
		private Font _font;						// Font used for preview.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the selected font characters to use.
		/// </summary>
		public IEnumerable<char> FontCharacters
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the font name.
		/// </summary>
		public string FontName
		{
			get
			{
				return textName.Text;
			}
		}

		/// <summary>
		/// Property to return the font family name.
		/// </summary>
		public string FontFamilyName
		{
			get
			{
				return comboFonts.Text;
			}
		}

		/// <summary>
		/// Property to return whether to use points or pixels for the font size.
		/// </summary>
		public FontHeightMode FontHeightMode
		{
			get
			{
				return (string.Equals(comboSizeType.Text, "points", StringComparison.OrdinalIgnoreCase)) ? FontHeightMode.Points : FontHeightMode.Pixels;
			}
		}

		/// <summary>
		/// Property to return the font size.
		/// </summary>
		public float FontSize
		{
			get
			{
				return (float)numericSize.Value;
			}
		}

		/// <summary>
		/// Property to return the anti-aliasing mode.
		/// </summary>
		public FontAntiAliasMode FontAntiAliasMode
		{
			get
			{
				switch (comboAA.Text.ToLower())
				{
					case "none":
						return FontAntiAliasMode.None;
					case "anti-alias":
						return FontAntiAliasMode.AntiAlias;
					default:
						return FontAntiAliasMode.AntiAlias;
				}
			}
		}

		/// <summary>
		/// Property to return the font texture size.
		/// </summary>
		public Size FontTextureSize
		{
			get
			{
				return new Size((int)numericTextureWidth.Value, (int)numericTextureHeight.Value);
			}			
		}

        /// <summary>
        /// Property to set or return the maximum texture size.
        /// </summary>
	    public Size MaxTextureSize
	    {
	        get
	        {
	            return new Size((int)numericTextureWidth.Maximum, (int)numericTextureHeight.Maximum);
	        }
	        set
	        {
	            numericTextureWidth.Maximum = value.Width;
	            numericTextureHeight.Maximum = value.Height;
	        }
	    }

		/// <summary>
		/// Property to return the font style.
		/// </summary>
		public FontStyle FontStyle
		{
			get
			{
				var result = FontStyle.Regular;

				if (checkBold.Checked)
					result |= FontStyle.Bold;
				if (checkUnderline.Checked)
					result |= FontStyle.Underline;
				if (checkStrikeThrough.Checked)
					result |= FontStyle.Strikeout;
				if (checkItalic.Checked)
					result |= FontStyle.Italic;

				return result;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the buttonCharacterList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonCharacterList_Click(object sender, EventArgs e)
		{
			Font currentFont = null;
			FormCharacterPicker picker = null;

			try
			{
				currentFont = new Font(FontFamilyName, 16.0f, GraphicsUnit.Pixel);

				picker = new FormCharacterPicker
				         {
				             Characters = FontCharacters,
				             CurrentFont = currentFont
				         };

			    if (picker.ShowDialog(this) == DialogResult.OK)
				{
					FontCharacters = picker.Characters;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				if (currentFont != null)
				{
					currentFont.Dispose();
				}

				if (picker != null)
				{
					picker.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to validate the controls on the form.
		/// </summary>
		private void ValidateControls()
		{
		    if (string.IsNullOrEmpty(comboAA.Text))
		    {
				comboAA.Text = Resources.GORFNT_TEXT_ANTIALIAS;
		    }

			if (string.IsNullOrEmpty(comboFonts.Text))
			{
				buttonCharacterList.Enabled =
					buttonOK.Enabled =
					checkBold.Enabled = checkUnderline.Enabled = checkItalic.Enabled = checkStrikeThrough.Enabled = false;
			}
			else
			{
				var family = FontFamily.Families.SingleOrDefault(item =>
				                                                 string.Equals(item.Name,
				                                                               comboFonts.Text,
				                                                               StringComparison.OrdinalIgnoreCase));

				buttonCharacterList.Enabled = true;

				if (family == null)
				{
					checkBold.Checked = checkUnderline.Checked = checkItalic.Checked = checkStrikeThrough.Checked = false;
				}
				else
				{
					checkBold.Enabled = family.IsStyleAvailable(FontStyle.Bold) && family.IsStyleAvailable(FontStyle.Regular);
					checkUnderline.Enabled = family.IsStyleAvailable(FontStyle.Underline) && family.IsStyleAvailable(FontStyle.Regular);
					checkItalic.Enabled = family.IsStyleAvailable(FontStyle.Italic) && family.IsStyleAvailable(FontStyle.Regular);
					checkStrikeThrough.Enabled = family.IsStyleAvailable(FontStyle.Strikeout);

					buttonOK.Enabled = textName.Text.Length > 0;
				}
			}

			if ((!checkBold.Enabled)
				&& (checkBold.Checked))
			{
				checkBold.Checked = false;
			}

			if ((!checkUnderline.Enabled)
				&& (checkUnderline.Checked))
			{
				checkUnderline.Checked = false;
			}

			if ((!checkItalic.Enabled)
				&& (checkItalic.Checked))
			{
				checkItalic.Checked = false;
			}

			if ((!checkStrikeThrough.Enabled)
				&& (checkStrikeThrough.Checked))
			{
				checkStrikeThrough.Checked = false;
			}
		}

		/// <summary>
		/// Function to update the preview font.
		/// </summary>
		private void UpdatePreview()
		{
			var style = FontStyle.Regular;

			labelPreview.Font = Font;
		    if (_font != null)
		    {
		        _font.Dispose();
		    }

		    _font = null;

			if (checkBold.Checked)
			{
				style |= FontStyle.Bold;
			}
			if (checkUnderline.Checked)
			{
				style |= FontStyle.Underline;
			}
			if (checkItalic.Checked)
			{
				style |= FontStyle.Italic;
			}
			if (checkStrikeThrough.Checked)
			{
				style |= FontStyle.Strikeout;
			}

			_font = new Font(comboFonts.Text,
			                 (float)numericSize.Value,
			                 style,
			                 (string.Equals(comboSizeType.Text, "points", StringComparison.OrdinalIgnoreCase)
				                  ? GraphicsUnit.Point
				                  : GraphicsUnit.Pixel));
			labelPreview.Font = _font;
		}

		/// <summary>
		/// Function to restrict the texture size based on the font size.
		/// </summary>
		private void RestrictTexture()
		{
			var fontSize = (float)numericSize.Value;

			if (string.Equals(comboSizeType.Text, "points", StringComparison.OrdinalIgnoreCase))
			{
				fontSize = (float)System.Math.Ceiling(GorgonFontSettings.GetFontHeight(fontSize, 0));
			}

			if ((fontSize > (float)numericTextureHeight.Value) || (fontSize > (float)numericTextureWidth.Value))
			{
				numericTextureWidth.Value = (decimal)fontSize;
				numericTextureHeight.Value = (decimal)fontSize;
			}

			if (fontSize > 16)
			{
				numericTextureHeight.Minimum = numericTextureWidth.Minimum = (decimal)fontSize;
			}
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboFonts control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void comboFonts_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (sender == comboFonts)
				{
					ValidateControls();
				}

				if (sender == numericSize)
				{
					RestrictTexture();
				}

				// We call validate controls here because we need the preview to be capable of showing
				// the styles we select for the font.  If the font doesn't support the style we select,
				// then an exception occurs.  We don't want that.
				ValidateControls();

				UpdatePreview();

				GorgonFontEditorPlugIn.Settings.FontAntiAliasMode = string.Equals(comboAA.Text,
				                                                                  Resources.GORFNT_TEXT_NONE,
				                                                                  StringComparison.OrdinalIgnoreCase)
					                                                    ? FontAntiAliasMode.None
					                                                    : FontAntiAliasMode.AntiAlias;

				GorgonFontEditorPlugIn.Settings.FontSizeType = string.Equals(comboSizeType.Text,
				                                                             "points",
				                                                             StringComparison.OrdinalIgnoreCase)
					                                               ? FontHeightMode.Points
					                                               : FontHeightMode.Pixels;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the TextChanged event of the textName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textName_TextChanged(object sender, EventArgs e)
		{
			ValidateControls();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (_font != null)
			{
				_font.Dispose();
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            comboAA.Items.Clear();
		    comboAA.Items.Add(Resources.GORFNT_TEXT_NONE);
            comboAA.Items.Add(Resources.GORFNT_TEXT_ANTIALIAS);

			comboSizeType.Text = GorgonFontEditorPlugIn.Settings.FontSizeType.ToString();

			switch (GorgonFontEditorPlugIn.Settings.FontAntiAliasMode)
			{
				case FontAntiAliasMode.None:
					comboAA.Text = Resources.GORFNT_TEXT_NONE;
					break;
				default:
					comboAA.Text = Resources.GORFNT_TEXT_ANTIALIAS;
					break;
			}

			numericTextureWidth.Value = GorgonFontEditorPlugIn.Settings.FontTextureSize.Width;
			numericTextureHeight.Value = GorgonFontEditorPlugIn.Settings.FontTextureSize.Height;

			ValidateControls();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FormNewFont"/> class.
		/// </summary>
		public FormNewFont()
		{			
			InitializeComponent();			
		}
		#endregion
	}
}
