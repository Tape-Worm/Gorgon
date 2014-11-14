﻿#region MIT.
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
// Created: Monday, September 23, 2013 12:02:10 AM
// 
#endregion

using System;
using System.Windows.Forms;
using GorgonLibrary.Editor.Properties;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Splash screen form.
	/// </summary>
	partial class FormSplash : Form
	{
		#region Variables.
		private readonly Version _appVersion;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the version text.
		/// </summary>
		/// <param name="text">Text to display.</param>
		public void UpdateVersion(string text)
		{
			labelVersion.Text = string.Format(Resources.GOREDIT_VERSION, _appVersion, text);
			labelVersion.Refresh();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateVersion(Resources.GOREDIT_TEXT_INITIALIZING);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FormSplash"/> class.
		/// </summary>
		public FormSplash()
		{
			InitializeComponent();

			_appVersion = GetType().Assembly.GetName().Version;
		}
		#endregion
	}
}
