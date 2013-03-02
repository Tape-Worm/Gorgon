#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Monday, April 30, 2012 9:20:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.UI;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Application context for the editor.
	/// </summary>
	class AppContext
		: ApplicationContext
	{
		#region Variables.
		private formSplash _splash = null;			// Main splash screen.
		#endregion

		#region Methods.
		/// <summary>
		/// Calls <see cref="M:System.Windows.Forms.ApplicationContext.ExitThreadCore"/>, which raises the <see cref="E:System.Windows.Forms.ApplicationContext.ThreadExit"/> event.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnMainFormClosed(object sender, EventArgs e)
		{
			base.OnMainFormClosed(sender, e);
			ExitThread();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="AppContext"/> class.
		/// </summary>
		public AppContext()
		{
			try
			{
				GorgonTimer timer = null;		

				_splash = new formSplash();
				MainForm = new formMain();

				_splash.Show();
				_splash.Refresh();

				timer = new GorgonTimer();

				// Fade the splash screen in.
				while (_splash.Opacity < 1)
				{
					if (timer.Milliseconds > 7)
					{
						timer.Reset();
						_splash.Opacity += 0.01;
					}
				}

				// If we've got time left, keep the pretty logo up.
				timer.Reset();

				_splash.UpdateVersion("Building font cache...");
				// Get our cached fonts.
				Program.UpdateCachedFonts();

				_splash.UpdateVersion("Initializing graphics...");
				Program.InitializeGraphics(MainForm);

				_splash.UpdateVersion(string.Empty);

				while (timer.Milliseconds < 3000)
					System.Threading.Thread.Sleep(1);

				// Fade it out.
				while (_splash.Opacity > 0.02)
				{
					if (timer.Milliseconds > 5)
					{
						timer.Reset();
						_splash.Opacity -= 0.01;
					}
				}

				// Bring up our application form.
				MainForm.Show();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
				if (MainForm != null)
					MainForm.Close();
			}
			finally
			{
				if (_splash != null)
					_splash.Dispose();
				_splash = null;
			}
		}
		#endregion
	}
}
