#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: August 26, 2018 5:37:32 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.UI;

namespace Gorgon.Editor
{
    /// <summary>
    /// Bootstrap functionality for the application.
    /// </summary>
    public class Boot
        : ApplicationContext
    {
        #region Variables.
        // Splash screen.
        private FormSplash _splash;
        // The main application form.
        private FormMain _mainForm;
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.ApplicationContext" /> and optionally releases the managed resources.</summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources. </param>
        protected override void Dispose(bool disposing)
        {
            FormMain mainForm = Interlocked.Exchange(ref _mainForm, null);
            FormSplash splash = Interlocked.Exchange(ref _splash, null);
            mainForm?.Dispose();
            splash?.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Function to show the splash screen for our application boot up procedure.
        /// </summary>
        private async Task ShowSplashAsync()
        {
            _splash = new FormSplash();
            _splash.Show();
            await _splash.FadeAsync(true, 16);
        }

        /// <summary>
        /// Function to hide the splash screen.
        /// </summary>
        private async Task HideSplashAsync()
        {
            if (_splash == null)
            {
                return;
            }

            await _splash.FadeAsync(false, 16);

            _splash.Dispose();
            _splash = null;
        }

        /// <summary>
        /// Function to perform the boot strapping operation.
        /// </summary>
        /// <returns>The main application window.</returns>
        public async void BootStrap()
        {
            try
            {
                // Get our initial context.
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

                Program.Log.Print("Booting application...", LoggingLevel.All);

                Cursor.Current = Cursors.WaitCursor;

                await ShowSplashAsync();

                

                await HideSplashAsync();

                MainForm = _mainForm = new FormMain();
                _mainForm.Show();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        #endregion
    }
}
