#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Monday, September 22, 2014 4:39:22 PM
// 
#endregion

using System;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.UI;

namespace Gorgon.Examples
{
	/// <summary>
	/// Application context.
	/// </summary>
	class AppContext
		: ApplicationContext
	{
		#region Variables.
		private readonly FormSplash _splash;			// Main splash screen.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to fade the splash screen in or out.
		/// </summary>
		/// <param name="fadeIn"><b>true</b> to fade in, <b>false</b> to fade out.</param>
		/// <param name="time">Time, in milliseconds, for the fade.</param>
		private void FadeSplashScreen(bool fadeIn, float time)
		{
			double startTime = GorgonTiming.MillisecondsSinceStart;
			double delta = 0;

			// Fade the splash screen in.
			while (delta <= 1)
			{
				delta = (GorgonTiming.MillisecondsSinceStart - startTime) / time;

				_splash.Opacity = fadeIn ? delta : 1.0f - delta;
			}
		}

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
			float startTime = GorgonTiming.SecondsSinceStart;

			try
			{
				_splash = new FormSplash();
				MainForm = new FormMain();

				_splash.Show();
				_splash.Refresh();

				// Fade in our splash screen.
				FadeSplashScreen(true, 500.0f);

				// Keep showing the splash screen.
				while ((GorgonTiming.SecondsSinceStart - startTime) < 3)
				{
					Thread.Sleep(1);
				}

				FadeSplashScreen(false, 250.0f);

				// Bring up our application form.
				MainForm.Show();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);

				if ((MainForm != null) && (!MainForm.IsDisposed))
				{
					MainForm.Dispose();
				}

				// Signal quit.
				Application.Exit();
			}
			finally
			{
				if (_splash != null)
				{
					_splash.Dispose();
				}
				_splash = null;
			}
		}
		#endregion
	}
}
