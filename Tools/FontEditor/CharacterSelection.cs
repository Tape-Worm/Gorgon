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
// Created: Wednesday, November 15, 2006 3:08:44 PM
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
	/// Form for character selection.
	/// </summary>
	public partial class CharacterSelection : Form
	{
		#region Variables.
		private Font _charFont = null;					// Character font.
		private int _page = 0;							// Page for characters.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the characters.
		/// </summary>
		public string Characters
		{
			get
			{
				return textCharacters.Text;
			}
			set
			{
				if ((value == null) || (value == string.Empty))
					textCharacters.Text = " ";
				else
					textCharacters.Text = value;
			}
		}

		/// <summary>
		/// Property to set or return the character font.
		/// </summary>
		public Font CharacterFont
		{
			get
			{
				return _charFont;
			}
			set
			{
				_charFont = new Font(value.FontFamily, 12.0f, value.Style);	
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the buttons.
		/// </summary>
		private void ValidateButtons()
		{
			buttonOK.Enabled = false;
			if (textCharacters.Text != string.Empty)
				buttonOK.Enabled = true;
		}

		/// <summary>
		/// Function to show the character list.
		/// </summary>
		private void UpdateCharacterList()
		{
			CheckBox fontCharacter;		// Font character.
			Point position;				// Position of the character.
			Size size;					// Size of the checkbox.

			if (_charFont == null)
				return;

			size = Size.Empty;
			position = Point.Empty;

			// Set the font.
			for (int i = 1; i <= 54; i++)
			{
				if (panelCharacters.Controls["checkBox" + i.ToString()] is CheckBox)
				{					
					fontCharacter = (CheckBox)panelCharacters.Controls["checkBox" + i.ToString()];
					fontCharacter.MouseEnter -= new EventHandler(fontCharacter_MouseEnter);
					fontCharacter.CheckedChanged -= new EventHandler(fontCharacter_CheckedChanged);					
					fontCharacter.Font = _charFont;
					fontCharacter.Text = Convert.ToChar(i + _page).ToString();
					if (textCharacters.Text.IndexOf(Convert.ToChar(i + _page).ToString()) != -1)
						fontCharacter.Checked = true;
					else
						fontCharacter.Checked = false;

					fontCharacter.CheckedChanged += new EventHandler(fontCharacter_CheckedChanged);
					fontCharacter.MouseEnter += new EventHandler(fontCharacter_MouseEnter);
				}
			}

			ValidateButtons();
		}

		/// <summary>
		/// Function to update the selections.
		/// </summary>
		private void UpdateSelections()
		{
			CheckBox fontCharacter = null;		// Font character.

			// Go through each character.			
			for (int i = 1; i <=54; i++)
			{
				if (panelCharacters.Controls["checkBox" + i.ToString()] is CheckBox)
				{
					fontCharacter = (CheckBox)panelCharacters.Controls["checkBox" + i.ToString()];
					fontCharacter.CheckedChanged -= new EventHandler(fontCharacter_CheckedChanged);
					if (textCharacters.Text.IndexOf(fontCharacter.Text) > -1)
						fontCharacter.Checked = true;
					else
						fontCharacter.Checked = false;
					fontCharacter.CheckedChanged += new EventHandler(fontCharacter_CheckedChanged);
				}
			}
		}

		/// <summary>
		/// Handles the MouseLeave event of the fontCharacter control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void fontCharacter_MouseLeave(object sender, EventArgs e)
		{
			labelCharacter.Text = "Character: ";
		}

		/// <summary>
		/// Handles the CheckedChanged event of the fontCharacter control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void fontCharacter_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox fontCharacter = null;			// Font character.
			string chars = textCharacters.Text;		// Characters.
			int pos = -1;							// Character position.

			// Go through each character.
			fontCharacter = (CheckBox)sender;
			pos = chars.IndexOf(fontCharacter.Text);
			if (pos > -1)
			{
				if (!fontCharacter.Checked)
					chars = chars.Remove(pos,1);
			}
			else
			{
				if (fontCharacter.Checked)
					chars += fontCharacter.Text;
			}

			if ((chars.Length < 1) || (chars.IndexOf(" ") < 0))
				chars += " ";
			chars.Replace('\0', ' ');
			textCharacters.Text = chars;
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
				textCharacters.Text = " ";
			UpdateSelections();
			ValidateButtons();
			panelCharacters.Focus();
		}

		/// <summary>
		/// Handles the MouseEnter event of the fontCharacter control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void fontCharacter_MouseEnter(object sender, EventArgs e)
		{
			CheckBox character = (CheckBox)sender;		// Check box that sent the event.
			string charString = string.Empty;			// Character string.
			int charCode = 0;							// Character code.

			charString = character.Text;
			if (charString == "\n")
				charString = " ";
			if (charString == "\t")
				charString = " ";
			if (charString == "\r")
				charString = " ";
			charCode = (int)character.Text[0];
			labelCharacter.Text = "Character: '" + charString + "' (0x" + charCode.ToString("x").PadLeft(2, '0') + " Hex, " + charCode.ToString() + " Dec).";
			character.Focus();
		}

		/// <summary>
		/// Handles the Click event of the menuitemInvert control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemInvert_Click(object sender, EventArgs e)
		{
			CheckBox fontCharacter = null;		// Font character.
			string chars = string.Empty;		// Characters.

			for (int i = 1; i <=54; i++)
			{
				if (panelCharacters.Controls["checkBox" + i.ToString()] is CheckBox)
				{
					fontCharacter = (CheckBox)panelCharacters.Controls["checkBox" + i.ToString()];
					fontCharacter.CheckedChanged -= new EventHandler(fontCharacter_CheckedChanged);
					if (fontCharacter.Checked)
						fontCharacter.Checked = false;
					else
						fontCharacter.Checked = true;
					fontCharacter_CheckedChanged(fontCharacter, EventArgs.Empty);
					fontCharacter.CheckedChanged += new EventHandler(fontCharacter_CheckedChanged);
				}
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemSelectNone control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemSelectNone_Click(object sender, EventArgs e)
		{
			textCharacters.Text = " ";
			UpdateSelections();
		}

		/// <summary>
		/// Handles the Click event of the menuitemSelectAll control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemSelectAll_Click(object sender, EventArgs e)
		{
			textCharacters.Text = " ";
			for (int i = 1; i < 255; i++)
				textCharacters.Text += Convert.ToChar(i + _page).ToString();
			UpdateSelections();
		}

		/// <summary>
		/// Handles the Click event of the buttonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{
			if (textCharacters.Text.IndexOf(" ") < 0)
				textCharacters.Text += " ";
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			_charFont.Dispose();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			UpdateCharacterList();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public CharacterSelection()
		{
			InitializeComponent();
		}
		#endregion

		/// <summary>
		/// Handles the Scroll event of the scrollVertical control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ScrollEventArgs"/> instance containing the event data.</param>
		private void scrollVertical_Scroll(object sender, ScrollEventArgs e)
		{
			_page = scrollVertical.Value;
			UpdateCharacterList();
		}
	}
}