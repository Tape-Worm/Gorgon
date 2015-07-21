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
// Created: Sunday, March 10, 2013 10:57:24 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Editor.FontEditorPlugIn.Properties;
using Gorgon.UI;

namespace Gorgon.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Dialog to pick characters for conversion into bitmap glyphs.
	/// </summary>
	partial class FormCharacterPicker 
		: GorgonFlatForm
	{
		#region Variables.
		private int _page;													// Page for characters.
		private IDictionary<string, GorgonRange> _characterRanges;			// Character ranges for the font.
		private System.Drawing.Graphics _graphics;							// GDI+ graphics.
		private IntPtr _hFont = IntPtr.Zero;								// Font handle.
		private IntPtr _hDc = IntPtr.Zero;									// Device context.
		private IntPtr _prevHObj = IntPtr.Zero;								// Previous object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current font.
		/// </summary>
		public Font CurrentFont
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the list of characters.
		/// </summary>
		public IEnumerable<char> Characters
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the buttonSelectAll control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonSelectAll_Click(object sender, EventArgs e)
		{
			try
			{
				var item = (KeyValuePair<string, GorgonRange>)listRanges.SelectedItems[0].Tag;

				string newString = string.Empty;

				SelectFont();

				for (int index = item.Value.Minimum; index <= item.Value.Maximum; index++)
				{
					char c = Convert.ToChar(index);
					if (IsCharacterSupported(c))
						newString += Convert.ToChar(index);
				}

				textCharacters.Text = @" " + newString;
				UpdateSelections();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				DeselectFont();
				ValidateButtons();
			}
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the listRanges control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void listRanges_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				_page = 0;
				scrollVertical.Value = 0;
			    if (listRanges.SelectedItems.Count > 0)
			    {
			        buttonSelectAll.Text = $"{Resources.GORFNT_ACC_TEXT_SELECT_ALL_OF} {listRanges.SelectedItems[0].SubItems[1].Text}";
			    }

			    GetCharacterSets();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateButtons();
			}
		}

		/// <summary>
		/// Function to validate the buttons.
		/// </summary>
		private void ValidateButtons()
		{
			buttonOK.Enabled = textCharacters.Text != string.Empty;
		    buttonSelectAll.Enabled = listRanges.SelectedItems.Count > 0;
		}

		/// <summary>
		/// Handles the Scroll event of the scrollVertical control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ScrollEventArgs"/> instance containing the event data.</param>
		private void scrollVertical_Scroll(object sender, ScrollEventArgs e)
		{
			_page = scrollVertical.Value * 8;
			GetCharacterSets();
		}

		/// <summary>
		/// Handles the MouseEnter event of the fontControl control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void fontControl_MouseEnter(object sender, EventArgs e)
		{
			var check = (CheckBox)sender;

			string name = Win32API.GetCodePointName(check.Text[0]);

		    if (string.IsNullOrEmpty(name))
		    {
		        name = $"'{check.Text}'";
		    }

		    tipChar.SetToolTip(check,
		                       $"U+0x{Convert.ToUInt16(check.Text[0]).FormatHex()} ({Convert.ToUInt16(check.Text[0])}, {name})");
		}

		/// <summary>
		/// Handles the CheckedChanged event of the fontControl control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void fontControl_CheckedChanged(object sender, EventArgs e)
		{
		    string chars = textCharacters.Text;		// Characters.

		    // Go through each character.
			var fontControl = (CheckBox)sender;
			int pos = chars.IndexOf(fontControl.Text, StringComparison.Ordinal);

			if (pos > -1)
			{
			    if (!fontControl.Checked)
			    {
			        chars = chars.Remove(pos, 1);
			    }
			}
			else
			{
			    if (fontControl.Checked)
			    {
			        chars += fontControl.Text;
			    }
			}

		    if ((chars.Length < 1)
		        || (chars.IndexOf(" ", StringComparison.Ordinal) < 0))
		    {
		        chars += " ";
		    }

		    chars = chars.Replace('\0', ' ');

			textCharacters.Text = chars;
		}

		/// <summary>
		/// Function to update the selections.
		/// </summary>
		private void UpdateSelections()
		{
		    // Go through each character.			
			for (int i = 1; i <= 48; i++)
			{
			    var fontControl = (CheckBox)panelCharacters.Controls["checkBox" + i];		// Font character.

			    if (!fontControl.Enabled)
			    {
			        continue;
			    }

			    fontControl.CheckedChanged -= fontControl_CheckedChanged;

			    fontControl.Checked = textCharacters.Text.IndexOf(fontControl.Text, StringComparison.Ordinal) > -1;

			    fontControl.CheckedChanged += fontControl_CheckedChanged;
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the textCharacters control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void textCharacters_KeyDown(object sender, KeyEventArgs e)
		{
			if ((textCharacters.Text != string.Empty) && (e.KeyCode == Keys.Enter))
			{
				e.Handled = true;
				UpdateSelections();
			}
			ValidateButtons();
		}

		/// <summary>
		/// Handles the Leave event of the textCharacters control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textCharacters_Leave(object sender, EventArgs e)
		{
		    if (textCharacters.Text == string.Empty)
		    {
		        textCharacters.Text = @" ";
		    }

		    UpdateSelections();
			ValidateButtons();
			panelCharacters.Focus();
		}

		/// <summary>
		/// Handles the Click event of the buttonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{
		    if (textCharacters.Text.IndexOf(@" ", StringComparison.Ordinal) < 0)
		    {
		        textCharacters.Text += @" ";
		    }

		    Characters = textCharacters.Text;
		}

		/// <summary>
		/// Function to determine if a character is supported by the font.
		/// </summary>
		/// <param name="c">Character to check.</param>
		/// <returns><b>true</b> if supported, <b>false</b> if not.</returns>
		private bool IsCharacterSupported(char c)
		{
			if ((char.IsWhiteSpace(c)) || (char.IsControl(c)))
			{
				return false;
			}

			if (!Win32API.IsGlyphSupported(c, _hDc))
			{
				return false;
			}

			int charValue = Convert.ToInt32(c);
						
			return _characterRanges.Any(item => ((charValue >= item.Value.Minimum) && (charValue <= item.Value.Maximum)));
		}

		/// <summary>
		/// Function to retrieve a list of character sets.
		/// </summary>
		private void GetCharacterSets()
		{
		    if (CurrentFont == null)
				return;

			if (listRanges.SelectedIndices.Count == 0)
				return;
					
			ListViewItem charSet = listRanges.SelectedItems[0];
			GorgonRange range = ((KeyValuePair<string, GorgonRange>)charSet.Tag).Value;
			int lineCount = ((int)System.Math.Ceiling(range.Range / 8.0f)) - 1;
			if (lineCount > 6)
			{
				scrollVertical.Enabled = true;
				scrollVertical.Minimum = 0;
				scrollVertical.Maximum = lineCount;
				scrollVertical.SmallChange = 1;
				scrollVertical.LargeChange = 6;
			}
			else
				scrollVertical.Enabled = false;

			try
			{
				SelectFont();

				// Set the font.
				for (int i = 1; i <= 48; i++)
				{
					string checkBoxName = "checkBox" + i;
					var fontControl = (CheckBox)panelCharacters.Controls[checkBoxName];					// Font character.			

					fontControl.MouseEnter -= fontControl_MouseEnter;
					fontControl.CheckedChanged -= fontControl_CheckedChanged;

					// If the range has less characters than our character list, then disable the rest of the check boxes.
					if (i > range.Range)
					{
						fontControl.Enabled = false;
						fontControl.Font = Font;
						fontControl.BackColor = DarkFormsRenderer.DisabledColor;
						fontControl.Text = "";
						fontControl.Checked = false;
						continue;
					}

					char c = Convert.ToChar(i + range.Minimum + _page);


					if ((!IsCharacterSupported(c)) || (Convert.ToInt32(c) > range.Maximum))
					{
						fontControl.Enabled = false;
						fontControl.Font = Font;
						fontControl.BackColor = DarkFormsRenderer.DisabledColor;
						fontControl.Text = "";
						fontControl.Checked = false;
						continue;
					}

				    fontControl.Enabled = true;
				    fontControl.BackColor = Color.White;

				    fontControl.Font = CurrentFont;
					fontControl.Text = c.ToString(CultureInfo.CurrentUICulture);
				    fontControl.Checked = textCharacters.Text.IndexOf(fontControl.Text, StringComparison.Ordinal) != -1;

				    fontControl.CheckedChanged += fontControl_CheckedChanged;
					fontControl.MouseEnter += fontControl_MouseEnter;
				}
			}
			finally
			{
				DeselectFont();
				ValidateButtons();
			}
		}

		/// <summary>
		/// Function to fill the unicode ranges based on the font.
		/// </summary>
		private void FillRanges()
		{
			listRanges.Items.Clear();
		    if (_characterRanges.Count <= 0)
		    {
		        return;
		    }

		    listRanges.BeginUpdate();
		    var sortedRanges = _characterRanges.OrderBy(item => item.Value.Minimum);
		    foreach (var range in sortedRanges)
		    {
		        var item = new ListViewItem(((ushort)range.Value.Minimum).FormatHex() + ".." + ((ushort)range.Value.Maximum).FormatHex());
		        item.SubItems.Add(range.Key);
		        item.Tag = range;
		        listRanges.Items.Add(item);
		    }
		    listRanges.EndUpdate();
		    listRanges.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
		    listRanges.SelectedIndices.Add(0);
		}

		/// <summary>
		/// Function to select the font object into the device context.
		/// </summary>
		private void SelectFont()
		{
		    if (_hDc == IntPtr.Zero)
		    {
		        _hDc = _graphics.GetHdc();
		    }

		    if (_hFont == IntPtr.Zero)
		    {
		        _hFont = CurrentFont.ToHfont();
		    }

		    if (_prevHObj == IntPtr.Zero)
		    {
		        _prevHObj = Win32API.SelectObject(_hDc, _hFont);
		    }
		}

		/// <summary>
		/// Function to deselect a font from the device context.
		/// </summary>
		private void DeselectFont()
		{
		    if ((_hDc == IntPtr.Zero)
		        || (_prevHObj == IntPtr.Zero))
		    {
		        return;
		    }

		    Win32API.SelectObject(_hDc, _prevHObj);
			_prevHObj = IntPtr.Zero;

			_graphics.ReleaseHdc();
			_hDc = IntPtr.Zero;
		}

        /// <summary>
        /// Function to localize the controls on the form.
        /// </summary>
	    private void LocalizeForm()
        {
            Text = Resources.GORFNT_DLG_CHARACTER_PICKER_CAPTION;
            labelCharacters.Text = $"{Resources.GORFNT_TEXT_CHARACTERS}:";
            buttonOK.Text = Resources.GORFNT_ACC_TEXT_OK;
            buttonCancel.Text = Resources.GORFNT_ACC_TEXT_CANCEL;
            buttonSelectAll.Text = $"{Resources.GORFNT_ACC_TEXT_SELECT_ALL_OF} ???";
        }

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
                LocalizeForm();

				_graphics = System.Drawing.Graphics.FromHwnd(Handle);

				SelectFont();

				textCharacters.Text = string.Join(string.Empty, Characters);
				_characterRanges = Win32API.GetUnicodeRanges(CurrentFont, _hDc);
				FillRanges();
				GetCharacterSets();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
				Close();
			}
			finally
			{
				DeselectFont();
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			DeselectFont();

		    if (_graphics != null)
		    {
		        _graphics.Dispose();
		    }
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FormCharacterPicker"/> class.
		/// </summary>
		public FormCharacterPicker()
		{
			InitializeComponent();
		}
		#endregion
	}
}
