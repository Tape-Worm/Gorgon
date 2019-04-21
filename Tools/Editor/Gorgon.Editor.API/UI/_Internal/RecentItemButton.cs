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
// Created: December 20, 2018 12:47:27 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Editor.ProjectData;
using System.IO;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// A button for the <see cref="RecentFilesControl"/>
    /// </summary>
    internal partial class RecentItemButton 
        : UserControl
    {
        #region Variables.
        // The recent item object for this button.
        private RecentItem _recentItem;
        // Flag to indicate that the mouse is over this item.
        private bool _isOver;
        // Flag to indicate that a mouse button is down.
        private bool _isMouseDown;
        #endregion

        #region Events.
        /// <summary>
        /// Event fired to determine if the button should be deleted or not.
        /// </summary>
        public event EventHandler DeleteItem;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the recent item object for this button.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RecentItem RecentItem
        {
            get => _recentItem;
            set
            {
                if (_recentItem == value)
                {
                    return;
                }

                _recentItem = value;
                UpdateRecentItem();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if the mouse is over the control or not.
        /// </summary>
        private void CheckCursorState()
        {
            Point p = Cursor.Position;
            p = PointToClient(p);

            if (DisplayRectangle.Contains(p))
            {
                _isOver = true;
            }
            else
            {
                _isOver = false;
            }

            UpdateBackColor();
        }

        /// <summary>Handles the MouseDown event of the RecentItemButton control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void RecentItemButton_MouseDown(object sender, MouseEventArgs e)
        {
            _isOver = true;
            _isMouseDown = true;
            UpdateBackColor();
        }

        /// <summary>Handles the MouseUp event of the RecentItemButton control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void RecentItemButton_MouseUp(object sender, MouseEventArgs e)
        {
            _isOver = true;
            _isMouseDown = false;
            UpdateBackColor();

            if (e.Button == MouseButtons.Left)
            {
                OnClick(EventArgs.Empty);
            }            
        }

        /// <summary>Handles the Click event of the ButtonDelete control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            EventHandler handler = DeleteItem;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Handles the MouseMove event of the RecentItemButton control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void RecentItemButton_MouseMove(object sender, MouseEventArgs e) => CheckCursorState();

        /// <summary>Handles the MouseEnter event of the RecentItemButton control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RecentItemButton_MouseEnter(object sender, EventArgs e) => CheckCursorState();

        /// <summary>Handles the MouseLeave event of the RecentItemButton control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RecentItemButton_MouseLeave(object sender, EventArgs e) => CheckCursorState();

        /// <summary>
        /// Function to update the control with the contents of the recent item object.
        /// </summary>
        private void UpdateRecentItem()
        {
            if (_recentItem == null)
            {
                LabelTitle.Text = 
                LabelPath.Text =
                LabelDateTime.Text = string.Empty;
                return;
            }

            var dir = new DirectoryInfo(_recentItem.FilePath);
            LabelTitle.Text = dir.Name;
            LabelPath.Text = dir.FullName;
            LabelDateTime.Text = _recentItem.LastUsedDate.ToString();
        }

        /// <summary>
        /// Function to update the background color.
        /// </summary>
        private void UpdateBackColor()
        {
            if (_isOver)
            {
                ButtonDelete.Visible = true;
                if (_isMouseDown)
                {
                    BackColor = Color.FromKnownColor(KnownColor.DarkOrange);
                }
                else
                {
                    BackColor = Color.FromKnownColor(KnownColor.SteelBlue);
                }
            }
            else
            {
                ButtonDelete.Visible = false;
                BackColor = Color.FromArgb(64, 64, 64);
            }

            PanelFileDate.BackColor = Color.FromArgb((int)(BackColor.R * 0.75f), (int)(BackColor.G * 0.75f), (int)(BackColor.B * 0.75f));
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            UpdateRecentItem();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.UI._Internal.RecentItemButton"/> class.</summary>
        public RecentItemButton()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponent();
        }
        #endregion
    }
}
