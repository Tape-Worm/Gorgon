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
using Gorgon.Math;
using Gorgon.Windows.Properties;

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
        private ProgressPanel _messagePanel;
        // The synchronization context to use.
        private SynchronizationContext _syncContext;
        #endregion

        #region Events.
        /// <summary>
        /// Event triggered when an operation is cancelled.
        /// </summary>
        [Description("Triggered when an operation is cancelled via this panel."), Category("Behavior")]
        public EventHandler OperationCancelled;
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
            get => _messagePanel.LabelProgressMessage.ForeColor;
            set => _messagePanel.LabelProgressMessage.ForeColor = value;
        }

        /// <summary>Gets or sets the foreground color of progress message box.</summary>
        [Browsable(true), 
         Category("Appearance"),
         Description("Sets the foreground color for the title text in the progress message box"),
         DefaultValue(typeof(Color), "DimGray"),
         RefreshProperties(RefreshProperties.Repaint)]
        public Color ProgressBoxTitleForeColor
        {
            get => _messagePanel.LabelTitle.ForeColor;
            set => _messagePanel.LabelTitle.ForeColor = value;
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
            get => _messagePanel.LabelProgressMessage.Text;
            set
            {
                if (string.Equals(value, _messagePanel.LabelProgressMessage.Text, StringComparison.CurrentCulture))
                {
                    return;
                }

                _messagePanel.LabelProgressMessage.Text = value;

                if (string.IsNullOrEmpty(_messagePanel.LabelProgressMessage.Text))
                {
                    _messagePanel.TableProgress.RowStyles[1].SizeType = SizeType.Absolute;
                    _messagePanel.TableProgress.RowStyles[1].Height = 0;
                }
                else
                {
                    _messagePanel.TableProgress.RowStyles[1].SizeType = SizeType.AutoSize;
                }

                Invalidate();
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
            get => _messagePanel.LabelTitle.Text;
            set
            {
                if (string.Equals(value, _messagePanel.LabelTitle.Text, StringComparison.CurrentCulture))
                {
                    return;
                }

                _messagePanel.LabelTitle.Text = value;

                if (string.IsNullOrEmpty(_messagePanel.LabelTitle.Text))
                {
                    _messagePanel.LabelProgressMessage.Anchor = AnchorStyles.Left;
                    _messagePanel.TableProgress.RowStyles[0].SizeType = SizeType.Absolute;
                    _messagePanel.TableProgress.RowStyles[0].Height = 0;
                }
                else
                {
                    _messagePanel.LabelProgressMessage.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    _messagePanel.TableProgress.RowStyles[0].SizeType = SizeType.AutoSize;
                }
                
                Invalidate();
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
            get => _messagePanel.LabelProgressMessage.Font;
            set
            {
                _messagePanel.LabelProgressMessage.Font = value;
                Invalidate();
            }
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
            get => _messagePanel.LabelTitle.Font;
            set
            {
                _messagePanel.LabelTitle.Font = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Property to set or return the current value for the progress.
        /// </summary>
        [Browsable(false)]
        public float CurrentValue
        {
            get => _messagePanel.ProgressMeter.Value / 100.0f;
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
            get => _messagePanel.ProgressMeter.Style;
            set
            {
                _messagePanel.ProgressMeter.Style = value;
                Invalidate();
            }
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
            get => _messagePanel.ButtonCancel.Visible;
            set
            {
                _messagePanel.ButtonCancel.Visible = value;
                Invalidate();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the Click event of the ButtonCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            EventHandler handler = OperationCancelled;
            OperationCancelled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to set the progress meter value in a thread safe context.
        /// </summary>
        /// <param name="value">The value to set.</param>
        private void SetProgressValue(float value)
        {
            value = value.Min(1.0f).Max(0);

            if ((_syncContext != null) && (InvokeRequired))
            {
                _syncContext.Post(progValue => SetProgressValue((float)progValue), value);
                return;
            }

            _messagePanel.ProgressMeter.Value = (int)(value * 100.0f);
        }

        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control" /> and its child controls and optionally releases the managed resources.</summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources. </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ProgressPanel panel = Interlocked.Exchange(ref _messagePanel, null);

                if (panel == null)
                {
                    base.Dispose(disposing);
                    return;
                }

                panel.ButtonCancel.Click -= ButtonCancel_Click;
                panel.Dispose();
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
        /// Initializes a new instance of the <see cref="GorgonProgressScreenPanel"/> class.
        /// </summary>
        public GorgonProgressScreenPanel()
        {
            _messagePanel = new ProgressPanel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = true,
                LabelProgressMessage =
                                {
                                    ForeColor = Color.Black,
                                    Text = Resources.GOR_TEXT_PROGRESS
                                },
                LabelTitle =
                                {
                                    ForeColor = Color.DimGray,
                                    Text = Resources.GOR_TEXT_WAIT_TITLE
                                }
            };

            Controls.Add(_messagePanel);

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                _syncContext = SynchronizationContext.Current;
                _messagePanel.ButtonCancel.Click += ButtonCancel_Click;
            }
        }
        #endregion
    }
}
