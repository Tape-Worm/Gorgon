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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.UI;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Font creation form.
	/// </summary>
	partial class formNewFont 
		: ZuneForm
	{
		#region Variables.
		private Font _font = null;						// Font used for preview.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the content object to design.
		/// </summary>
		public GorgonFontContent Content
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
		public Graphics.FontHeightMode FontHeightMode
		{
			get
			{
				return (string.Compare(comboSizeType.Text, "points", true) == 0) ? Graphics.FontHeightMode.Points : Graphics.FontHeightMode.Pixels;
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
		public Graphics.FontAntiAliasMode FontAntiAliasMode
		{
			get
			{
				switch (comboAA.Text.ToLower())
				{
					case "none":
						return Graphics.FontAntiAliasMode.None;
					case "anti-alias":
						return Graphics.FontAntiAliasMode.AntiAlias;
					default:
						return Graphics.FontAntiAliasMode.AntiAliasHQ;
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
		/// Property to return the font style.
		/// </summary>
		public FontStyle FontStyle
		{
			get
			{
				FontStyle result = System.Drawing.FontStyle.Regular;

				if (checkBold.Checked)
					result |= System.Drawing.FontStyle.Bold;
				if (checkUnderline.Checked)
					result |= System.Drawing.FontStyle.Underline;
				if (checkStrikeThrough.Checked)
					result |= System.Drawing.FontStyle.Strikeout;
				if (checkItalic.Checked)
					result |= System.Drawing.FontStyle.Italic;

				return result;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the controls on the form.
		/// </summary>
		private void ValidateControls()
		{
			if (string.IsNullOrEmpty(comboAA.Text))
				comboAA.Text = "Anti-Alias (High Quality)";

			buttonOK.Enabled = checkBold.Enabled = checkUnderline.Enabled = checkItalic.Enabled = checkStrikeThrough.Enabled = false;

			if (string.IsNullOrEmpty(comboFonts.Text))
				buttonOK.Enabled = checkBold.Enabled = checkUnderline.Enabled = checkItalic.Enabled = checkStrikeThrough.Enabled = false;
			else
			{
				var family = FontFamily.Families.Where(item => string.Compare(item.Name, comboFonts.Text, true) == 0).SingleOrDefault();

				if ((family == null) || (!GorgonFontEditorPlugIn.CachedFonts.ContainsKey(family.Name)))
					checkBold.Checked = checkUnderline.Checked = checkItalic.Checked = checkStrikeThrough.Checked = false;
				else
				{
					Font font = GorgonFontEditorPlugIn.CachedFonts[family.Name];

					checkBold.Enabled = family.IsStyleAvailable(FontStyle.Bold) && family.IsStyleAvailable(FontStyle.Regular);
					checkUnderline.Enabled = family.IsStyleAvailable(FontStyle.Underline) && family.IsStyleAvailable(FontStyle.Regular);
					checkItalic.Enabled = family.IsStyleAvailable(FontStyle.Italic) && family.IsStyleAvailable(FontStyle.Regular);
					checkStrikeThrough.Enabled = family.IsStyleAvailable(FontStyle.Strikeout);

					checkBold.Checked = ((font.Style & FontStyle.Bold) == FontStyle.Bold);
					checkUnderline.Checked = ((font.Style & FontStyle.Underline) == FontStyle.Underline);
					checkItalic.Checked = ((font.Style & FontStyle.Italic) == FontStyle.Italic);
					checkStrikeThrough.Checked = false;

					buttonOK.Enabled = textName.Text.Length > 0;
				}
			}			
		}

		/// <summary>
		/// Function to update the preview font.
		/// </summary>
		private void UpdatePreview()
		{
			FontStyle style = FontStyle.Regular;

			labelPreview.Font = this.Font;
			if (_font != null)
				_font.Dispose();
			_font = null;

			if (checkBold.Checked)
				style |= FontStyle.Bold;
			if (checkUnderline.Checked)
				style |= FontStyle.Underline;
			if (checkItalic.Checked)
				style |= FontStyle.Italic;
			if (checkStrikeThrough.Checked)
				style |= FontStyle.Strikeout;

			_font = new Font(comboFonts.Text, (float)numericSize.Value, style, (string.Compare(this.comboSizeType.Text, "points", true) == 0 ? GraphicsUnit.Point : GraphicsUnit.Pixel));
			labelPreview.Font = _font;
		}

		/// <summary>
		/// Function to restrict the texture size based on the font size.
		/// </summary>
		private void RestrictTexture()
		{
			float fontSize = (float)numericSize.Value;
			if (string.Compare(comboSizeType.Text, "points", true) == 0)
				fontSize = (float)System.Math.Ceiling(GorgonFontSettings.GetFontHeight(fontSize, 0));

			if ((fontSize > (float)numericTextureHeight.Value) || (fontSize > (float)numericTextureWidth.Value))
			{
				numericTextureWidth.Value = (decimal)fontSize;
				numericTextureHeight.Value = (decimal)fontSize;
			}

			if (fontSize > 16)
				numericTextureHeight.Minimum = numericTextureWidth.Minimum = (decimal)fontSize;
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
					ValidateControls();

				if (sender == numericSize)
					RestrictTexture();

				UpdatePreview();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
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
				_font.Dispose();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			numericTextureWidth.Maximum = Content.Graphics.Textures.MaxWidth;
			numericTextureHeight.Maximum = Content.Graphics.Textures.MaxHeight;
			comboSizeType.Text = GorgonFontEditorPlugIn.Settings.FontSizeType.ToString();

			switch (GorgonFontEditorPlugIn.Settings.FontAntiAliasMode)
			{
				case Graphics.FontAntiAliasMode.None:
					comboAA.Text = "None";
					break;
				case Graphics.FontAntiAliasMode.AntiAlias:
					comboAA.Text = "Anti-Alias";
					break;
				default:
					comboAA.Text = "Anti-Alias (High Quality)";
					break;
			}

			numericTextureWidth.Value = GorgonFontEditorPlugIn.Settings.FontTextureSize.Width;
			numericTextureHeight.Value = GorgonFontEditorPlugIn.Settings.FontTextureSize.Height;

			ValidateControls();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formNewFont"/> class.
		/// </summary>
		public formNewFont()
		{			
			InitializeComponent();			
		}
		#endregion
	}
}
