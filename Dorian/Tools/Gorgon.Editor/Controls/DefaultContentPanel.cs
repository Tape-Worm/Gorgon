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
// Created: Tuesday, March 5, 2013 8:51:06 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Default content.
	/// </summary>
	partial class DefaultContentPanel : UserControl
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultContentPanel"/> class.
		/// </summary>
		public DefaultContentPanel()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Handles the MouseMove event of the panel1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panel1_MouseMove(object sender, MouseEventArgs e)
		{
			panelOptions.Height = 30;
		}

		/// <summary>
		/// Handles the MouseDown event of the panelOptions control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelOptions_MouseDown(object sender, MouseEventArgs e)
		{
			checkPulse.Visible = true;
			numericPulseRate.Visible = true;
			labelClosePanel.Visible = true;
			panelOptions.Height = 33;
			panelOptions.Top = this.ClientSize.Height - 33;
			panelOptions.BackColor = DarkFormsRenderer.DisabledColor;
		}

		/// <summary>
		/// Handles the MouseEnter event of the labelClosePanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelClosePanel_MouseEnter(object sender, EventArgs e)
		{
			labelClosePanel.ForeColor = Color.White;
		}

		/// <summary>
		/// Handles the MouseLeave event of the labelClosePanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelClosePanel_MouseLeave(object sender, EventArgs e)
		{
			labelClosePanel.ForeColor = Color.Silver;
		}

		/// <summary>
		/// Handles the MouseMove event of the labelClosePanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void labelClosePanel_MouseMove(object sender, MouseEventArgs e)
		{
			labelClosePanel.ForeColor = Color.White;
		}

		/// <summary>
		/// Handles the MouseDown event of the labelClosePanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void labelClosePanel_MouseDown(object sender, MouseEventArgs e)
		{
			checkPulse.Visible = false;
			numericPulseRate.Visible = false;
			labelClosePanel.Visible = false;
			panelOptions.Height = 4;
			panelOptions.Top = this.ClientSize.Height - 4;
		}

		/// <summary>
		/// Handles the MouseEnter event of the panelOptions control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelOptions_MouseEnter(object sender, EventArgs e)
		{
			if (panelOptions.Height != 33)
			{
				panelOptions.BackColor = Color.SteelBlue;
			}
			else
			{
				panelOptions.BackColor = DarkFormsRenderer.DisabledColor;
			}
		}

		/// <summary>
		/// Handles the MouseLeave event of the panelOptions control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelOptions_MouseLeave(object sender, EventArgs e)
		{
			panelOptions.BackColor = DarkFormsRenderer.DisabledColor;
		}		
	}
}
