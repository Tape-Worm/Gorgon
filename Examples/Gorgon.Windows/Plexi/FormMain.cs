#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: December 5, 2020 3:34:58 PM
// 
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Graphics;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Examples
{
    /// <summary>
    /// Main example window.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This example shows how to use the overlay objects to create a "plexi-glass" appearance over the window, or any of its child controls. 
    /// Similar to how web or WPF applications show a loading screen where the background dims or lightens to indicate that the controls 
    /// behind are inaccessible. Often this is displayed with an accompanying control that the user can interact with.
    /// 
    /// Gorgon has 3 of these overlay object types:
    /// 1. GorgonOverlay - Displays a translucent layer on top of the form/control.
    /// 2. GorgonWaitOverlay - Displays the translucent layer on top of the form/control, plus a window that shows a customizable "please wait" 
    ///    dialog with an animated icon.
    /// 3. GorgonProgressOverlay - Displays the translucent layer on top of the form/control, plus a window that shows a customizable progress  
    ///    meter used to show a long running operation's progress.
    ///    
    /// The amount of translucency, and color of these overlay objects can be adjusted by the user for all panels, and each panel type has its 
    /// own set of customizable parameters to update the display as needed.
    /// </para>
    /// </remarks>
    public partial class FormMain
        : Form
    {
        #region Variables.
        // An overlay without any other windows or decoration, just sits on top of the control.
        private readonly GorgonOverlay _overlay = new GorgonOverlay
        {
            TransparencyPercent = 75
        };
        #endregion

        #region Methods.
        /// <summary>Handles the Click event of the ButtonOverlayForm control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private async void ButtonOverlayForm_Click(object sender, EventArgs e)
        {
            // This simulates a long running workload (10 seconds), and displays the progress overlay with a 
            // custom color, and translucency level. The progress meter also contains an optional cancel 
            // button which can be used to cancel a long running operation.

            bool isRunning = true;  // Normally you'd use a CancellationToken here, but that's overkill for this stupid example.

            var progress = new GorgonProgressOverlay
            {
                TransparencyPercent = 25,
                OverlayColor = GorgonColor.PurplePure
            };
            progress.Show(this, "Progress Meter", cancelAction: () => isRunning = false, meterStyle: ProgressBarStyle.Continuous);
            Text = "Plexi - Showing overlay for 10 seconds...";

            // Simulate work.
            await Task.Run(() =>
            {
                var timer = new GorgonTimerQpc();
                int count = 0;
                var wait = new SpinWait();                

                while ((isRunning) && (count < 10))
                {
                    // Wait for 1 second before updating the progress.
                    while ((isRunning) && (timer.Seconds < 1))
                    {
                        wait.SpinOnce();
                    }

                    // Stop if we've cancelled the operation.
                    if (!isRunning)
                    {
                        break;
                    }

                    ++count;

                    // This method will send data back to the progress meter, it is completely thread safe and can be called anywhere.
                    progress.UpdateProgress(count / 10.0f, $"{count} seconds.");

                    timer.Reset();
                }
            });
            progress.Hide();

            Text = "Plexi";
        }

        /// <summary>Handles the Click event of the ButtonOverlayTextbox control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ButtonOverlayTextbox_Click(object sender, EventArgs e)
        {
            // This method just displays an overlay on top of the panel containing our editable controls.
            // You'll note how we return a window handle back from the overlay (all overlay types will 
            // return a window handle). This is the handle to the top window in the z-order of the overlay. 
            // Applications that use the overlay and wish to display a messagebox or other window on top 
            // of the overlay should pass this handle as a parent of the window. Otherwise, in some cases, 
            // the window will get "stuck" behind the overlay.

            if (_overlay.IsActive)
            {
                _overlay.Hide();
                return;
            }
                                   
            IWin32Window overlay = _overlay.Show(PanelPlexi);

            // We pass back the overlay window to the dialog to ensure it gets placed on top.
            GorgonDialogs.InfoBox(overlay, "The panel is now locked.");
        }

        /// <summary>Handles the ValueChanged event of the DatePick control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private async void DatePick_ValueChanged(object sender, EventArgs e)
        {
            // Another simulatation of a long running workload (10 seconds) that displays the progress overlay 
            // with default values. Notice that by passing a null value to the progress title, the progress 
            // window adjusts to remove the space for the title.

            var progress = new GorgonProgressOverlay();
            progress.Show(this, null, message: "This is a really long time just to set a date right?");

            await Task.Delay(10000);

            progress.Hide();
        }

        /// <summary>Handles the ItemCheck event of the CheckList control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemCheckEventArgs" /> instance containing the event data.</param>
        private async void CheckList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // This shows a "please wait" panel with a spinning image (default) to indicate that the system 
            // is busy. It modifies the overlay to be 25% transparent, and gives it a blue tint.

            var wait = new GorgonWaitOverlay
            {
                TransparencyPercent = 25,
                OverlayColor = GorgonColor.BluePure
            };
            wait.Show(CheckList, string.Empty, "Really? This long? For a checkbox?");

            await Task.Delay(5000);

            wait.Hide();
        }

        /// <summary>Handles the Leave event of the TextPlexi control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private async void TextPlexi_Leave(object sender, EventArgs e)
        {
            // This shows a "please wait" panel with a spinning image (default) to indicate that the system 
            // is busy. It modifies the overlay to be 30% transparent, and gives it a white tint.

            if (string.Equals(LabelPlexi.Text, TextPlexi.Text, StringComparison.CurrentCulture))
            {
                return;
            }

            var wait = new GorgonWaitOverlay
            {
                TransparencyPercent = 30,
                OverlayColor = GorgonColor.White
            };

            wait.Show(LabelPlexi, "Updating...", "Sending text to label...");

            await Task.Delay(4000);

            LabelPlexi.Text = TextPlexi.Text;

            wait.Hide();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FormMain" /> class.</summary>
        public FormMain() => InitializeComponent();
        #endregion
    }
}
