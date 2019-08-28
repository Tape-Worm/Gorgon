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
// Created: Monday, September 23, 2013 12:02:10 AM
// 
#endregion

using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor
{
    /// <summary>
    /// Splash screen form.
    /// </summary>
    partial class FormSplash
        : Form
    {
        #region Variables.
        // The application version number.
        private readonly Version _appVersion;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the text shown in the information label.
        /// </summary>
        public string InfoText
        {
            get => labelInfo.Text;
            set
            {
                labelInfo.Text = value;

                // Print any text to the log.
                Program.Log.Print("AppStart: {0}", LoggingLevel.Intermediate, value);

                labelInfo.Refresh();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the version text.
        /// </summary>
        private void UpdateVersion()
        {
            labelVersionNumber.Text = _appVersion.ToString();
            labelVersionNumber.Refresh();
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

            UpdateVersion();

            InfoText = Resources.GOREDIT_TEXT_INITIALIZING;
        }

        /// <summary>
        /// Function to fade the splash screen in or out.
        /// </summary>
        /// <param name="fadeIn"><b>true</b> to fade in, <b>false</b> to fade out.</param>
        /// <param name="time">Time, in milliseconds, for the fade.</param>
        public async Task FadeAsync(bool fadeIn, int time)
        {
            Refresh();

            // Fade the splash screen in.
            while (((fadeIn) && (Opacity < 1))
                || ((!fadeIn) && (Opacity > 0)))
            {
                await Task.Delay(time);

                if (fadeIn)
                {
                    Opacity += 0.05;
                }
                else
                {
                    Opacity -= 0.05f;
                }
            }
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
