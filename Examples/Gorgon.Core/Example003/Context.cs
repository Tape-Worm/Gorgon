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
// Created: Tuesday, September 18, 2012 8:03:43 PM
// 
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Examples
{
	/// <summary>
	/// The application context.
	/// </summary>
	/// <remarks>We'll use this to display our splash screen, and then our main form and still use the idle loop.</remarks>
	internal class Context
		: ApplicationContext
	{
		#region Variables.
		// A timer for the splash screen.
		private readonly IGorgonTimer _timer;
		// A splash screen.
		private formSplash _splashScreen;
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the KeyDown event of the MainForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        /// <exception cref="NotSupportedException"></exception>
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Space)
			{
				return;
			}

			((formMain)MainForm).Clear();

			if (GorgonApplication.IdleMethod == Program.Idle)
			{
				GorgonApplication.IdleMethod = Program.NewIdle;
			}
			else
			{
				GorgonApplication.IdleMethod = Program.Idle;
			}
		}

		/// <summary>
		/// Calls <see cref="M:System.Windows.Forms.ApplicationContext.ExitThreadCore" />, which raises the <see cref="E:System.Windows.Forms.ApplicationContext.ThreadExit" /> event.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnMainFormClosed(object sender, EventArgs e)
		{
			base.OnMainFormClosed(sender, e);
			ExitThread();
		}

		/// <summary>
		/// Function to begin the execution of the application context.
		/// </summary>
		public void RunMe()
		{
			string annoyUser = "I'm going to delay you for a bit.";
			int counter = 0;

			try
			{
				_timer.Reset();
				_splashScreen.Show();
				_splashScreen.UpdateText("This is the splash screen.");

				// Fade in the splash screen about 10% every 7 milliseconds.
				while (_splashScreen.Opacity < 1)
				{
					if (!(_timer.Milliseconds > 7))
					{
						continue;
					}

					_timer.Reset();
					_splashScreen.Opacity += 0.01;
				}

				// Annoy the user.  They're asking for it.
				while (counter < 5)
				{
					while (_timer.Seconds > 1)
					{
						if (annoyUser.Length < 50)
							annoyUser += ".";
						else
							annoyUser = "I'm going to delay you for a bit.";

						_splashScreen.UpdateText(annoyUser);
						_timer.Reset();
						counter++;
					}
				}

				// Fade it out.
				while (_splashScreen.Opacity > 0.02)
				{
					if (!(_timer.Milliseconds > 5))
					{
						continue;
					}

					_timer.Reset();
					_splashScreen.Opacity -= 0.01;
				}

				// Resize the main form to 640 x 480.
				MainForm.KeyDown += MainForm_KeyDown;
			    MainForm.Deactivate += (sender, args) => GorgonApplication.Log.Print("Application is deactivated. Loops will pause.", LoggingLevel.All);
			    MainForm.Activated += (sender, args) => GorgonApplication.Log.Print("Application is activated. Loops will run.", LoggingLevel.All);
				MainForm.ClientSize = new Size(640, 480);
				MainForm.Show();
			}
			catch (Exception ex)
			{
				// If we get an error, then leave the application.
				GorgonDialogs.ErrorBox(MainForm, ex);

				MainForm?.Dispose();
				MainForm = null;
			}
			finally
			{
				// We don't need this any more.
				_splashScreen?.Dispose();
				_splashScreen = null;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Context" /> class.
		/// </summary>
		public Context()
		{
			// Create our timer object.
			if (GorgonTimerQpc.SupportsQpc())
			{
				_timer = new GorgonTimerQpc();
			}
			else
			{
				_timer = new GorgonTimerMultimedia();
			}

			// Create the splash screen and the main interface.
			_splashScreen = new formSplash();			
			MainForm = new formMain();			// Note that we're assign this to the inherited property 'MainForm'.
												// This how the application context knows which form controls the application.

			RunMe();
		}
		#endregion
	}
}
