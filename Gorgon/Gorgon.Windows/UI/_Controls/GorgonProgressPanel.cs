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
// Created: September 15, 2018 12:53:46 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Math;

namespace Gorgon.UI
{
    /// <summary>
    /// The progress meter panel.
    /// </summary>
    public partial class GorgonProgressPanel
        : UserControl
    {
        #region Variables.
        // The synchronization context to use.
        private readonly SynchronizationContext _syncContext;
        // The current progress percentage.
        private float _progress;
        // Flag to indicate that the cancel event is assigned.
        private int _cancelEvent;
        // Flag to indicate that cancellation is allowed.
        private bool _allowCancel;
        #endregion

        #region Events.
        /// <summary>
        /// Event triggered when an operation is cancelled.
        /// </summary>
        [Description("Triggered when an operation is cancelled via this panel."), Category("Behavior")]
        public event EventHandler OperationCancelled;
        #endregion

        #region Properties.
        /// <summary>Gets or sets the foreground color of progress message box.</summary>
        [Browsable(true),
        Category("Appearance"),
        Description("Sets the foreground color for the text in the progress message box"),
        DefaultValue(typeof(Color), "Black"),
         RefreshProperties(RefreshProperties.Repaint)]
        public Color ProgressBoxForeColor
        {
            get => LabelProgressMessage.ForeColor;
            set => LabelProgressMessage.ForeColor = value;
        }

        /// <summary>Gets or sets the foreground color of progress message box.</summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the foreground color for the title text in the progress message box"),
         DefaultValue(typeof(Color), "DimGray"),
         RefreshProperties(RefreshProperties.Repaint)]
        public Color ProgressBoxTitleForeColor
        {
            get => LabelTitle.ForeColor;
            set => LabelTitle.ForeColor = value;
        }

        /// <summary>
        /// Property to set or return the message to display in the progress box.
        /// </summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the message to display in the progress message box."),
        RefreshProperties(RefreshProperties.Repaint)]
        public string ProgressMessage
        {
            get => LabelProgressMessage.Text;
            set
            {
                if (string.Equals(value, LabelProgressMessage.Text, StringComparison.CurrentCulture))
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(value))
                {

                }

                LabelProgressMessage.Visible = !string.IsNullOrEmpty(value);
                LabelProgressMessage.Text = value;
            }
        }

        /// <summary>
        /// Property to set or return the title to display in the progress box.
        /// </summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the title to display in the progress message box."),
         RefreshProperties(RefreshProperties.Repaint)]
        public string ProgressTitle
        {
            get => LabelTitle.Text;
            set
            {
                if (string.Equals(value, LabelTitle.Text, StringComparison.CurrentCulture))
                {
                    return;
                }

                LabelTitle.Visible = !string.IsNullOrEmpty(value);
                LabelTitle.Text = value;
            }
        }

        /// <summary>
        /// Property to set or return the font to use for the progress message.
        /// </summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the font to use in the progress message box."),
         RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(Font), "Segoe UI, 9pt")]
        public Font ProgressMessageFont
        {
            get => LabelProgressMessage.Font;
            set => LabelProgressMessage.Font = value;
        }

        /// <summary>
        /// Property to set or return the font to use for the progress message title.
        /// </summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the font to use in the progress message box title."),
         RefreshProperties(RefreshProperties.Repaint),
         DefaultValue(typeof(Font), "Segoe UI, 14pt")]
        public Font ProgressTitleFont
        {
            get => LabelTitle.Font;
            set => LabelTitle.Font = value;
        }

        /// <summary>
        /// Property to set or return the current value for the progress.
        /// </summary>
        [Browsable(false)]
        public float CurrentValue
        {
            get => _progress;
            set => SetProgressValue(value);
        }

        /// <summary>
        /// Property to set or return the style of the progress meter.
        /// </summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the style for the progress meter."),
         DefaultValue(typeof(ProgressBarStyle), nameof(ProgressBarStyle.Marquee)),
         RefreshProperties(RefreshProperties.Repaint)]
        public ProgressBarStyle MeterStyle
        {
            get => ProgressMeter.Style;
            set => ProgressMeter.Style = value;
        }

        /// <summary>
        /// Property to set or return whether cancellation of an operation is supported
        /// </summary>
        [Browsable(true),
         Category("Behavior"),
         Description("Sets whether or not the operation can be cancelled through this panel."),
         DefaultValue(false),
         RefreshProperties(RefreshProperties.Repaint)]
        public bool AllowCancellation
        {
            get => _allowCancel;
            set
            {
                if (_allowCancel == value)
                {
                    return;
                }

                UnassignCancelEvent();
                ButtonCancel.Visible = _allowCancel = value;

                if (value)
                {
                    AssignCancelEvent();
                }
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to unassign the cancel event.
        /// </summary>
        private void UnassignCancelEvent()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            if (!AllowCancellation)
            {
                return;
            }

            ButtonCancel.Click -= ButtonCancel_Click;
            ButtonCancel.Enabled = false;

            Interlocked.Exchange(ref _cancelEvent, 0);
        }

        /// <summary>
        /// Function to assign the cancel event.
        /// </summary>
        private void AssignCancelEvent()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            if ((!AllowCancellation) || (Interlocked.Exchange(ref _cancelEvent, 1) == 1))
            {
                return;
            }

            ButtonCancel.Click += ButtonCancel_Click;
            ButtonCancel.Enabled = true;
        }

        /// <summary>
        /// Handles the Click event of the ButtonCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            UnassignCancelEvent();            
            EventHandler handler = OperationCancelled;
            OperationCancelled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to set the progress meter value in a thread safe context.
        /// </summary>
        /// <param name="value">The value to set.</param>
        private void SetProgressValue(float value)
        {
            _progress = value.Min(1.0f).Max(0);

            if ((_syncContext != null) && (InvokeRequired))
            {
                _syncContext.Post(progValue => SetProgressValue((float)progValue), _progress);
                return;
            }

            int progressValue = (int)(_progress * 100.0f);

            // This awful block of code is to handle a very stupid bug that's been a part of the progress bar since Aero was introduced:
            // https://derekwill.com/2014/06/24/combating-the-lag-of-the-winforms-progressbar/
            // Effectively, this disables animation so that we can get accurate feedback. 
            if (progressValue == 100)
            {
                ProgressMeter.Maximum = 101;
                ProgressMeter.Value = 101;
                ProgressMeter.Value = 100;
                ProgressMeter.Maximum = 100;
            }
            else if (progressValue > 0)
            {
                ProgressMeter.Value = progressValue + 1;
                ProgressMeter.Value = progressValue - 1;
            }
            else
            {
                ProgressMeter.Value = 1;
                ProgressMeter.Value = 0;
            }
            ProgressMeter.Update();
        }

        /// <summary>Raises the <see cref="Control.GotFocus"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            ButtonCancel.Select();
        }

        /// <summary>Raises the <see cref="Control.KeyUp"/> event.</summary>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if ((e.KeyCode == Keys.Escape) && (AllowCancellation))
            {
                ButtonCancel.PerformClick();
            }
        }

        /// <summary>Raises the <see cref="UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ButtonCancel.Select();
        }                

        /// <summary>
        /// Function to cancel the operation.
        /// </summary>
        public void Cancel() 
        {
            if (!AllowCancellation)
            {
                return;
            }

            ButtonCancel.PerformClick();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonProgressPanel"/> class.
        /// </summary>
        public GorgonProgressPanel()
        {
            InitializeComponent();

            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                _syncContext = SynchronizationContext.Current;
                AssignCancelEvent();
            }
        }
        #endregion
    }
}
