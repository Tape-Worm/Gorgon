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
// Created: Monday, September 03, 2007 9:54:19 PM
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
	/// Interface for preferences.
	/// </summary>
	public partial class formPreferences 
		: Form
	{
		#region Methods.
		/// <summary>
		/// Handles the Click event of the buttonBGColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonBGColor_Click(object sender, EventArgs e)
		{
			dialogColor.Color = pictureColor.BackColor;
			if (dialogColor.ShowDialog() == DialogResult.OK)
			{
				pictureColor.BackColor = dialogColor.Color;
				pictureColor.Refresh();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonPath control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonPath_Click(object sender, EventArgs e)
		{
			dialogEditorPath.FileName = textEditorPath.Text;
			if (dialogEditorPath.ShowDialog() == DialogResult.OK)
				textEditorPath.Text = dialogEditorPath.FileName;
		}

		/// <summary>
		/// Handles the KeyDown event of the formPreferences control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void formPreferences_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{			
				buttonOK_Click(this, EventArgs.Empty);
				DialogResult = DialogResult.OK;
			}

			if (e.KeyCode == Keys.Escape)
				Close();
		}

		/// <summary>
		/// Handles the TextChanged event of the textEditorName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textEditorName_TextChanged(object sender, EventArgs e)
		{
			ValidateForm();
		}

		/// <summary>
		/// Function to validate the form.
		/// </summary>
		private void ValidateForm()
		{
			if (textEditorName.Text != string.Empty)
				buttonPath.Enabled = true;
			else
				buttonPath.Enabled = false;
		}

		/// <summary>
		/// Handles the Click event of the buttonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{
			Settings.Root = "ImageManager";
			Settings.SetSetting("ImageEditorName", textEditorName.Text);

			if (textEditorName.Text != string.Empty)
				Settings.SetSetting("ImageEditorPath", textEditorPath.Text);
			else
				Settings.SetSetting("ImageEditorPath", string.Empty);
			Settings.Root = null;

			// Set misc. settings.
			Settings.SetSetting("ShowLogo", checkShowLogo.Checked.ToString());
			Settings.SetSetting("BGColor", pictureColor.BackColor.ToArgb().ToString());
		}

		/// <summary>
		/// Function to retrieve the settings for the application.
		/// </summary>
		public void GetSettings()
		{
			Settings.Root = "ImageManager";
			textEditorName.Text = Settings.GetSetting("ImageEditorName", string.Empty);

			if (textEditorName.Text != string.Empty)
				textEditorPath.Text = Settings.GetSetting("ImageEditorPath", string.Empty);

			ValidateForm();
			Settings.Root = null;

			// Get settings.
			checkShowLogo.Checked = (string.Compare(Settings.GetSetting("ShowLogo", "True"),"true",true) == 0);
			pictureColor.BackColor = Color.FromArgb(255, Color.FromArgb(Convert.ToInt32(Settings.GetSetting("BGColor", "-16777077"))));
			pictureColor.Refresh();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formPreferences()
		{
			InitializeComponent();
		}
		#endregion
	}
}