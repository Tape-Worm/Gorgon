using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.UI;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Font creation form.
	/// </summary>
	public partial class formNewFont : Form
	{
		#region Variables.
		private Font _font = null;			// Font used for preview.
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the controls on the form.
		/// </summary>
		private void ValidateControls()
		{
			buttonOK.Enabled = checkBold.Enabled = checkUnderline.Enabled = checkItalic.Enabled = checkStrikeThrough.Enabled = checkAntiAliased.Enabled = false;

			if (string.IsNullOrEmpty(comboFonts.Text))
				buttonOK.Enabled = checkAntiAliased.Enabled = checkBold.Enabled = checkUnderline.Enabled = checkItalic.Enabled = checkStrikeThrough.Enabled = false;
			else
			{
				checkAntiAliased.Enabled = true;
				var family = FontFamily.Families.Where(item => string.Compare(item.Name, comboFonts.Text, true) == 0).SingleOrDefault();

				if ((family == null) || (!Program.CachedFonts.ContainsKey(family.Name)))
					checkBold.Checked = checkUnderline.Checked = checkItalic.Checked = checkStrikeThrough.Checked = checkAntiAliased.Checked = false;
				else
				{						
					Font font = Program.CachedFonts[family.Name];

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

			_font = new Font(comboFonts.Text, (float)numericSize.Value, style, (checkPoints.Checked ? GraphicsUnit.Point : GraphicsUnit.Pixel));
			labelPreview.Font = _font;
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
