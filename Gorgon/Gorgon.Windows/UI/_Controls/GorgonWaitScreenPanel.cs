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

using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Gorgon.UI
{
    /// <summary>
    /// A wait screen panel.
    /// </summary>
    public class GorgonWaitScreenPanel
        : GorgonOverlayPanel
    {
        #region Variables.
        // The message panel to display.
        private GorgonWaitMessagePanel _messagePanel;
        #endregion

        #region Properties.
        /// <summary>
        /// Indicates the border style for the for wait message box.
        /// </summary>
        [Browsable(true), Category("Appearance"), Description("Set whether there is a border on the wait message box."),
        DefaultValue(true), RefreshProperties(RefreshProperties.Repaint)]
        public bool WaitBoxHasBorder
        {
            get => _messagePanel.WaitBoxHasBorder;
            set => _messagePanel.WaitBoxHasBorder = value;
        }

        /// <summary>Gets or sets the foreground color of wait message box.</summary>
        [Browsable(true),
        Category("Appearance"),
        Description("Sets the foreground color for the text in the wait message box"),
        DefaultValue(typeof(Color), "Black"),
         RefreshProperties(RefreshProperties.Repaint)]
        public Color WaitBoxForeColor
        {
            get => _messagePanel.WaitBoxForeColor;
            set => _messagePanel.WaitBoxForeColor = value;
        }

        /// <summary>Gets or sets the foreground color of wait message box.</summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the foreground color for the title text in the wait message box"),
         DefaultValue(typeof(Color), "DimGray"),
         RefreshProperties(RefreshProperties.Repaint)]
        public Color WaitBoxTitleForeColor
        {
            get => _messagePanel.WaitBoxTitleForeColor;
            set => _messagePanel.WaitBoxTitleForeColor = value;
        }

        /// <summary>
        /// Property to set or return the message to display in the wait box.
        /// </summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the message to display in the wait message box."),
        RefreshProperties(RefreshProperties.Repaint)]
        public string WaitMessage
        {
            get => _messagePanel.WaitMessage;
            set => _messagePanel.WaitMessage = value;
        }

        /// <summary>
        /// Property to set or return the title to display in the wait box.
        /// </summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the title to display in the wait message box."),
         RefreshProperties(RefreshProperties.Repaint)]
        public string WaitTitle
        {
            get => _messagePanel.WaitTitle;
            set => _messagePanel.WaitTitle = value;
        }

        /// <summary>
        /// Property to set or return the font to use for the wait message.
        /// </summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the font to use in the wait message box."),
         RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(Font), "Segoe UI, 9pt")]
        public Font WaitMessageFont
        {
            get => _messagePanel.WaitMessageFont;
            set => _messagePanel.WaitMessageFont = value;
        }

        /// <summary>
        /// Property to set or return the font to use for the wait message title.
        /// </summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the font to use in the wait message box title."),
         RefreshProperties(RefreshProperties.Repaint),
         DefaultValue(typeof(Font), "Segoe UI, 12pt")]
        public Font WaitTitleFont
        {
            get => _messagePanel.WaitTitleFont;
            set => _messagePanel.WaitTitleFont = value;
        }

        /// <summary>
        /// Property to set or return the icon used for the wait message.
        /// </summary>
        /// <remarks>
        /// Setting this value to <b>null</b> will reset the image to the original image for the control.
        /// </remarks>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the image to use in the wait message box."),
         RefreshProperties(RefreshProperties.Repaint),
         DefaultValue(typeof(Image), null)]
        public Image WaitIcon
        {
            get => _messagePanel.WaitIcon;
            set => _messagePanel.WaitIcon = value;
        }
        #endregion

        #region Methods.
        /// <summary>Releases the unmanaged resources used by the <see cref="System.Windows.Forms.Control" /> and its child controls and optionally releases the managed resources.</summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources. </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GorgonWaitMessagePanel panel = Interlocked.Exchange(ref _messagePanel, null);
                panel?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Function called after the overlay is painted.
        /// </summary>
        /// <param name="e">The paint event arguments.</param>
        protected override void OnOverlayPainted(PaintEventArgs e)
        {
            base.OnOverlayPainted(e);

            if (!_messagePanel.IsHandleCreated)
            {
                _messagePanel.CreateControl();
            }
            _messagePanel.Location = new Point((ClientSize.Width / 2) - (_messagePanel.Width / 2), (ClientSize.Height / 2) - (_messagePanel.Height / 2));
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonWaitScreenPanel"/> class.
        /// </summary>
        public GorgonWaitScreenPanel()
        {
            _messagePanel = new GorgonWaitMessagePanel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = true
            };

            Controls.Add(_messagePanel);
        }
        #endregion
    }
}
