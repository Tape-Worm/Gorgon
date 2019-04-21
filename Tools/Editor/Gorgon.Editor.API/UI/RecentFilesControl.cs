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
// Created: December 20, 2018 12:38:53 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.ProjectData;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// A control for displaying the <see cref="RecentItem"/> items.
    /// </summary>
    public partial class RecentFilesControl 
        : UserControl
    {
        #region Variables.
        // The list of recent items to display.
        private ObservableCollection<RecentItem> _recentItems = new ObservableCollection<RecentItem>();
        #endregion

        #region Events.
        /// <summary>
        /// Event triggered when a recent item button is clicked.
        /// </summary>
        [Category("Behavior"), Description("Event triggered when the recent item button is clicked.")]
        public event EventHandler<RecentItemClickEventArgs> RecentItemClick;

        /// <summary>
        /// Event triggered when a recent item has its delete button clicked.
        /// </summary>
        [Category("Behavior"), Description("Event triggered when the delete button on the recent item button is clicked.")]
        public event EventHandler<RecentItemDeleteEventArgs> DeleteItem;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the list of recent items to display.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObservableCollection<RecentItem> RecentItems
        {
            get => _recentItems;
            set
            {
                if (_recentItems == value)
                {
                    return;
                }

                if (_recentItems != null)
                {
                    _recentItems.CollectionChanged -= RecentItems_CollectionChanged;
                }

                _recentItems = value ?? new ObservableCollection<RecentItem>();

                FillList();                

                if (_recentItems != null)
                {
                    _recentItems.CollectionChanged += RecentItems_CollectionChanged;
                }
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to clear all the items in the list.
        /// </summary>
        private void ClearItems()
        {
            while (PanelRecentItems.Controls.Count > 0)
            {
                var button = (RecentItemButton)PanelRecentItems.Controls[0];
                button.Click -= Button_Click;
                button.DeleteItem -= Button_DeleteItem;
                button.Dispose();
            }
        }

        /// <summary>Handles the DeleteItem event of the Button control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Button_DeleteItem(object sender, EventArgs e)
        {
            var button = (RecentItemButton)sender;

            var args = new RecentItemDeleteEventArgs(button.RecentItem);
            EventHandler<RecentItemDeleteEventArgs> handler = DeleteItem;
            handler?.Invoke(this, args);
        }

        /// <summary>Handles the Layout event of the PanelRecentItems control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LayoutEventArgs"/> instance containing the event data.</param>
        private void PanelRecentItems_Layout(object sender, LayoutEventArgs e)
        {
            if (PanelRecentItems.Controls.Count == 0)
            {
                return;
            }

            PanelRecentItems.Controls[0].Dock = DockStyle.None;
            PanelRecentItems.Controls[0].Width = PanelRecentItems.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - PanelRecentItems.Controls[0].Margin.Horizontal;

            for (int i = 1; i < PanelRecentItems.Controls.Count; ++i)
            {
                PanelRecentItems.Controls[i].Dock = DockStyle.Top;
            }
        }

        /// <summary>Handles the CollectionChanged event of the RecentItems control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void RecentItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddRecentItem(e.NewItems.OfType<RecentItem>().First());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveRecentItem(e.OldItems.OfType<RecentItem>().First());
                    break;
                default:
                    FillList();
                    break;
            }
        }

        /// <summary>
        /// Function to remove a recent item from the list.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        private void RemoveRecentItem(RecentItem item)
        {
            RecentItemButton theButton = PanelRecentItems.Controls.OfType<RecentItemButton>().FirstOrDefault(buttonItem => buttonItem.RecentItem == item);

            if (theButton == null)
            {
                return;
            }

            theButton.DeleteItem -= Button_DeleteItem;
            theButton.Click -= Button_Click;
            theButton.Dispose();
        }

        /// <summary>
        /// Function to add a recent item button to the list.
        /// </summary>
        /// <param name="item">The recent item to add.</param>
        private void AddRecentItem(RecentItem item)
        {
            if ((!IsHandleCreated) || (DesignMode))
            {
                return;
            }

            var button = new RecentItemButton
            {
                Name = item.FilePath
            };

            button.SuspendLayout();

            button.AutoSize = false;
            button.RecentItem = item;
            button.Click += Button_Click;
            button.DeleteItem += Button_DeleteItem;

            int index = 0;

            foreach (RecentItemButton otherButton in PanelRecentItems.Controls.OfType<RecentItemButton>())
            {
                if (otherButton.RecentItem.LastUsedDate >= item.LastUsedDate)
                {
                    index++;
                }
            }

            PanelRecentItems.Controls.Add(button);
            PanelRecentItems.Controls.SetChildIndex(button, index);

            button.ResumeLayout(true);
        }

        /// <summary>Handles the Click event of the Button control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Button_Click(object sender, EventArgs e)
        {
            var button = (RecentItemButton)sender;

            var args = new RecentItemClickEventArgs(button.RecentItem);
            EventHandler<RecentItemClickEventArgs> handler = RecentItemClick;
            handler?.Invoke(this, args);
        }

        /// <summary>
        /// Function to fill the list of recent items.
        /// </summary>
        private void FillList()
        {
            try
            {
                if ((!IsHandleCreated) || (DesignMode))
                {
                    return;
                }

                PanelRecentItems.SuspendLayout();

                ClearItems();

                foreach (RecentItem item in _recentItems.OrderBy(item => item.LastUsedDate))
                {
                    AddRecentItem(item);
                }
            }
            finally
            {
                PanelRecentItems.ResumeLayout();
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            FillList();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.UI.RecentFilesControl"/> class.</summary>
        public RecentFilesControl()
        {
            InitializeComponent();
        }
        #endregion
    }
}
