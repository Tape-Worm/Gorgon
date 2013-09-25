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
using System.Drawing;
using System.Windows.Forms;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Default content.
	/// </summary>
	partial class DefaultContentPanel 
		: ContentPanel
	{
		#region Variables.
		private readonly DefaultContent _content;			// Content for the panel.
		#endregion

		#region Methods.
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
			panelOptions.Top = ClientSize.Height - 33;
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
			panelOptions.Top = ClientSize.Height - 4;
		}

		/// <summary>
		/// Handles the MouseEnter event of the panelOptions control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelOptions_MouseEnter(object sender, EventArgs e)
		{
			panelOptions.BackColor = panelOptions.Height != 33 ? Color.SteelBlue : DarkFormsRenderer.DisabledColor;
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

		/// <summary>
		/// Handles the Click event of the checkPulse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void checkPulse_Click(object sender, EventArgs e)
		{
			Program.Settings.StartPageAnimationPulseRate =
				_content.AlphaDelta = checkPulse.Checked ? (float)numericPulseRate.Value : 0;
			numericPulseRate.Enabled = checkPulse.Checked;
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericPulseRate control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericPulseRate_ValueChanged(object sender, EventArgs e)
		{
			Program.Settings.StartPageAnimationPulseRate = _content.AlphaDelta = (float)numericPulseRate.Value;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			checkPulse.Checked = _content.AlphaDelta > 0.0f;
			numericPulseRate.Value = (decimal)_content.AlphaDelta;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultContentPanel"/> class.
		/// </summary>
		public DefaultContentPanel()
		{
			InitializeComponent();
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultContentPanel"/> class.
		/// </summary>
		/// <param name="content">The content accessed by the panel.</param>
		public DefaultContentPanel(DefaultContent content)
			: base(content)
		{
			InitializeComponent();

			_content = content;
		}
		#endregion
	}
}
