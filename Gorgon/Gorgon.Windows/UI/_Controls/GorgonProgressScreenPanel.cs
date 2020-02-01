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
// Created: August 31, 2018 9:23:08 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Gorgon.UI
{
    /// <summary>
    /// A progress screen panel.
    /// </summary>
    public class GorgonProgressScreenPanel
        : GorgonOverlayPanel
    {
        #region Variables.
        // The message panel to display.
        private GorgonProgressPanel _messagePanel;
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
            get => _messagePanel.ProgressBoxForeColor;
            set => _messagePanel.ProgressBoxForeColor = value;
        }

        /// <summary>Gets or sets the foreground color of progress message box.</summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the foreground color for the title text in the progress message box"),
         DefaultValue(typeof(Color), "DimGray"),
         RefreshProperties(RefreshProperties.Repaint)]
        public Color ProgressBoxTitleForeColor
        {
            get => _messagePanel.ProgressBoxTitleForeColor;
            set => _messagePanel.ProgressBoxTitleForeColor = value;
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
            get => _messagePanel.ProgressMessage;
            set => _messagePanel.ProgressMessage = value;
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
            get => _messagePanel.ProgressTitle;
            set => _messagePanel.ProgressTitle = value;
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
            get => _messagePanel.ProgressMessageFont;
            set => _messagePanel.ProgressMessageFont = value;
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
            get => _messagePanel.ProgressTitleFont;
            set => _messagePanel.ProgressTitleFont = value;
        }

        /// <summary>
        /// Property to set or return the current value for the progress.
        /// </summary>
        [Browsable(false)]
        public float CurrentValue
        {
            get => _messagePanel.CurrentValue;
            set => _messagePanel.CurrentValue = value;
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
            get => _messagePanel.MeterStyle;
            set => _messagePanel.MeterStyle = value;
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
            get => _messagePanel.AllowCancellation;
            set => _messagePanel.AllowCancellation = value;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the Click event of the ButtonCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnOperationCancelled(object sender, EventArgs e)
        {
            EventHandler handler = OperationCancelled;
            OperationCancelled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control" /> and its child controls and optionally releases the managed resources.</summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources. </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GorgonProgressPanel panel = Interlocked.Exchange(ref _messagePanel, null);

                if (panel == null)
                {
                    base.Dispose(disposing);
                    return;
                }

                panel.Resize -= MessagePanel_Resize;
                panel.OperationCancelled -= OnOperationCancelled;
                panel.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Handles the Resize event of the MessagePanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MessagePanel_Resize(object sender, EventArgs e)
        {
            if (!_messagePanel.IsHandleCreated)
            {
                _messagePanel.CreateControl();
            }
            _messagePanel.Location = new Point((ClientSize.Width / 2) - (_messagePanel.Width / 2), (ClientSize.Height / 2) - (_messagePanel.Height / 2));
        }

        /// <summary>
        /// Function called after the overlay is painted.
        /// </summary>
        /// <param name="e">The paint event arguments.</param>
        protected override void OnOverlayPainted(PaintEventArgs e)
        {
            base.OnOverlayPainted(e);

            MessagePanel_Resize(this, EventArgs.Empty);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonProgressScreenPanel"/> class.
        /// </summary>
        public GorgonProgressScreenPanel()
        {
            InitializeComponent();

            _messagePanel = new GorgonProgressPanel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            Controls.Add(_messagePanel);

            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {                
                _messagePanel.OperationCancelled += OnOperationCancelled;
            }

            _messagePanel.Resize += MessagePanel_Resize;
        }
        #endregion

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GorgonProgressScreenPanel
            // 
            this.Name = "GorgonProgressScreenPanel";
            this.ResumeLayout(false);

        }
    }
}
