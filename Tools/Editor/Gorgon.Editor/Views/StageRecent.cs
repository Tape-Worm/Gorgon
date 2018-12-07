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
// Created: August 26, 2018 11:23:41 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;
using Gorgon.Math;
using Gorgon.UI;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// The control to display recent documents.
    /// </summary>
    internal partial class StageRecent 
        : EditorBaseControl, IDataContext<IRecentVm>
    {
        #region Classes.
        /// <summary>
        /// A comparer used to sort the list view.
        /// </summary>
        private class ListSorter
            : IComparer<ListViewItem>, IComparer
        {
            /// <summary>
            /// Property to set or return the column to sort.
            /// </summary>
            public ColumnHeader SortColumn
            {
                get;
                set;
            }

            /// <summary>
            /// Property to set or return the order of sorting.
            /// </summary>
            public SortOrder Order
            {
                get;
                set;
            }

            /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero
            /// <paramref name="x" /> is less than <paramref name="y" />.Zero
            /// <paramref name="x" /> equals <paramref name="y" />.Greater than zero
            /// <paramref name="x" /> is greater than <paramref name="y" />.
            /// </returns>
            public int Compare(ListViewItem x, ListViewItem y)
            {
                if ((x == null) || (y == null))
                {
                    return 0;
                }

                var xItem = (RecentItem)x.Tag;
                var yItem = (RecentItem)y.Tag;

                if ((xItem == null) || (yItem == null))
                {
                    return 0;
                }

#pragma warning disable IDE0046 // Convert to conditional expression
                if (SortColumn.DisplayIndex != 0)
                {
                    return Order == SortOrder.Ascending
                        ? DateTime.Compare(xItem.LastUsedDate, yItem.LastUsedDate)
                        : DateTime.Compare(xItem.LastUsedDate, yItem.LastUsedDate) * -1;
                }
#pragma warning restore IDE0046 // Convert to conditional expression

                return Order == SortOrder.Ascending
                    ? string.Compare(xItem.FilePath, yItem.FilePath, StringComparison.CurrentCultureIgnoreCase)
                    : string.Compare(xItem.FilePath, yItem.FilePath, StringComparison.CurrentCultureIgnoreCase) * -1;
            }

            /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero
            /// <paramref name="x" /> is less than <paramref name="y" />. Zero
            /// <paramref name="x" /> equals <paramref name="y" />. Greater than zero
            /// <paramref name="x" /> is greater than <paramref name="y" />.
            /// </returns>
            public int Compare(object x, object y) => Compare((ListViewItem)x, (ListViewItem)y);
        }
        #endregion

        #region Variables.
        // The list item sorter
        private ListSorter _sorter;
        // The font used for sub items in the list
        private Font _subItemFont;
        // The font used for items in the list
        private Font _itemFont;
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>        
        public IRecentVm DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangedEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangingEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {

        }


        /// <summary>Handles the ItemActivate event of the ListFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void ListFiles_ItemActivate(object sender, EventArgs e)
        {
            if ((DataContext?.OpenProjectCommand == null) || (ListFiles.SelectedItems.Count != 1))
            {
                return;
            }

            // Locate the item we've activated.
            ListViewItem selected = ListFiles.SelectedItems[0];

            if (selected == null)
            {
                return;
            }

            RecentItem recentItem = DataContext.Files.FirstOrDefault(item => string.Equals(item.FilePath, selected.Name, StringComparison.OrdinalIgnoreCase));

            if ((recentItem == null) || (!DataContext.OpenProjectCommand.CanExecute(recentItem)))
            {
                return;
            }

            DataContext.OpenProjectCommand.Execute(recentItem);
        }

        /// <summary>Handles the ColumnClick event of the ListFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [ColumnClickEventArgs] instance containing the event data.</param>
        private void ListFiles_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            int columnIndex = e.Column.Max(0).Min(ListFiles.Columns.Count - 1);
            ColumnHeader sortColumn = ListFiles.Columns[columnIndex];

            _sorter.SortColumn = sortColumn;
            _sorter.Order = _sorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            try
            {
                ListFiles.BeginUpdate();
                ListFiles.Sort();
            }
            finally
            {
                ListFiles.EndUpdate();
                ListFiles.SetSortIcon(sortColumn.Index, _sorter.Order);
            }
        }

        /// <summary>
        /// Function unassign any data context events.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.Files.CollectionChanged -= Files_CollectionChanged;
            DataContext.PropertyChanging -= DataContext_PropertyChanging;
            DataContext.PropertyChanged -= DataContext_PropertyChanged;

            DataContext.OnUnload();
            DataContext = null;
        }

        /// <summary>
        /// Function to reset the control back to its original state when no data context is assigned.
        /// </summary>
        private void ResetDataContext() => ListFiles.Items.Clear();

        /// <summary>
        /// Function to populate the list of files.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void PopulateFileList(IRecentVm dataContext)
        {
            try
            {
                ListFiles.BeginUpdate();

                ListFiles.Items.Clear();

                if (dataContext.Files.Count == 0)
                {
                    return;
                }

                foreach (RecentItem file in dataContext.Files)
                {
                    var item = new ListViewItem(file.FilePath)
                    {
                        Name = file.FilePath,
                        UseItemStyleForSubItems = false,
                        Font = _itemFont,
                        Tag = file
                    };
                    ListViewItem.ListViewSubItem subItem = item.SubItems.Add(file.LastUsedDate.ToString(CultureInfo.CurrentCulture));
                    subItem.Font = _subItemFont;
                    subItem.ForeColor = Color.FromKnownColor(KnownColor.Gray);
                    ListFiles.Items.Add(item);
                }

                ListFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                ListFiles.Sort();
            }
            finally
            {
                ListFiles.EndUpdate();
                ListFiles.SetSortIcon(_sorter.SortColumn.Index, _sorter.Order);
            }
        }

        /// <summary>Handles the CollectionChanged event of the Files control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [System.Collections.Specialized.NotifyCollectionChangedEventArgs] instance containing the event data.</param>
        private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (RecentItem recentItem in e.NewItems.OfType<RecentItem>())
                    {
                        ListViewItem listItem = null;

                        if (ListFiles.Items.ContainsKey(recentItem.FilePath))
                        {
                            listItem = ListFiles.Items[recentItem.FilePath];
                        }                        

                        if (listItem != null)
                        {
                            listItem.SubItems[1].Text = recentItem.LastUsedDate.ToString(CultureInfo.CurrentCulture);
                            continue;
                        }

                        listItem = new ListViewItem(recentItem.FilePath)
                        {
                            Name = recentItem.FilePath,
                            UseItemStyleForSubItems = false,
                            Font = _itemFont,
                            Tag = recentItem
                        };
                        ListViewItem.ListViewSubItem subItem = listItem.SubItems.Add(recentItem.LastUsedDate.ToString(CultureInfo.CurrentCulture));
                        subItem.Font = _subItemFont;
                        subItem.ForeColor = Color.FromKnownColor(KnownColor.Gray);
                        ListFiles.Items.Add(listItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (RecentItem recentItem in e.OldItems.OfType<RecentItem>())
                    {
                        if (!ListFiles.Items.ContainsKey(recentItem.FilePath))
                        {
                            continue;
                        }

                        ListFiles.Items.RemoveByKey(recentItem.FilePath);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    PopulateFileList(DataContext);
                    break;
            }

            // TODO: Re-sort the list.
            ListFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        /// <summary>
        /// Function to initialize the control from a data context.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        private void InitializeFromDataContext(IRecentVm dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            PopulateFileList(dataContext);
        }

        /// <summary>Raises the <a href="http://msdn.microsoft.com/en-us/library/system.windows.forms.usercontrol.load.aspx" target="_blank">Load</a> event.</summary>
        /// <param name="e">An <a href="http://msdn.microsoft.com/en-us/library/system.eventargs.aspx" target="_blank">EventArgs</a> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DataContext?.OnLoad();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IRecentVm dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);
            DataContext = dataContext;

            if ((IsDesignTime) || (DataContext == null))
            {
                return;
            }

            DataContext.PropertyChanging += DataContext_PropertyChanging;
            DataContext.PropertyChanged += DataContext_PropertyChanged;
            DataContext.Files.CollectionChanged += Files_CollectionChanged;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="StageRecent"/> class.
        /// </summary>
        public StageRecent()
        {
            InitializeComponent();
            _sorter = new ListSorter
            {
                Order = SortOrder.Descending,
                SortColumn = ColumnDate
            };
            ListFiles.ListViewItemSorter = _sorter;
            _itemFont = new Font(Font.FontFamily, 10.0f, FontStyle.Regular, Font.Unit);
            _subItemFont = new Font(Font.FontFamily, 10.0f, FontStyle.Regular, Font.Unit);
        }
        #endregion
    }
}
