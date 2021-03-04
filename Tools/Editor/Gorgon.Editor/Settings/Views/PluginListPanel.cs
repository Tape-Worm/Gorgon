#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: April 19, 2019 12:51:39 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// General settings for the application.
    /// </summary>
    internal partial class PlugInListPanel
        : SettingsBaseControl, IDataContext<ISettingsPlugInsList>
    {
        #region Properties.
        /// <summary>Property to return the ID of the panel.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string PanelID => DataContext?.ID.ToString() ?? Guid.Empty.ToString();

        /// <summary>Property to return the data context assigned to this view.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISettingsPlugInsList DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISettingsPlugInsList.Current):
                    TextStatus.Text = DataContext.Current?.DisabledReason ?? string.Empty;
                    break;
            }
        }

        /// <summary>Handles the SelectedIndexChanged event of the ListPlugIns control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ListPlugIns_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = ListPlugIns.SelectedIndices.Count > 0 ? ListPlugIns.SelectedIndices[0] : -1;
            if ((DataContext?.SelectPlugInCommand is null) || (!DataContext.SelectPlugInCommand.CanExecute(selectedIndex)))
            {
                return;
            }

            DataContext.SelectPlugInCommand.Execute(selectedIndex);
        }

        /// <summary>
        /// Function to fill the list with plug in information.
        /// </summary>
        /// <param name="dataContext">The data context containing the information.</param>
        private void FillPlugInList(ISettingsPlugInsList dataContext)
        {
            ListPlugIns.BeginUpdate();

            try
            {
                ListPlugIns.Items.Clear();
                ListViewItem selected = null;

                foreach (ISettingsPlugInListItem item in dataContext.PlugIns)
                {
                    var listItem = new ListViewItem()
                    {
                        Name = item.Name,
                        Text = item.Name
                    };

                    listItem.SubItems.Add(item.Type.GetDescription());
                    listItem.SubItems.Add(item.State);
                    listItem.SubItems.Add(item.Path);

                    if (!string.Equals(item.State, Resources.GOREDIT_PLUGIN_STATE_LOADED, StringComparison.CurrentCulture))
                    {
                        listItem.ForeColor = Color.DarkRed;
                    }

                    if ((dataContext.Current == item) && (selected is null))
                    {
                        selected = listItem;
                    }

                    ListPlugIns.Items.Add(listItem);
                }

                if (ListPlugIns.Items.Count == 0)
                {
                    return;
                }

                ListPlugIns.Select();

                if (selected is null)
                {
                    selected = ListPlugIns.Items[0];
                }

                selected.Selected = true;
                ListPlugIns.SelectedIndices.Add(selected.Index);

                if ((dataContext?.SelectPlugInCommand is null) || (!dataContext.SelectPlugInCommand.CanExecute(selected.Index)))
                {
                    return;
                }

                dataContext.SelectPlugInCommand.Execute(selected.Index);
            }
            finally
            {
                ListPlugIns.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                ListPlugIns.EndUpdate();
            }
        }

        /// <summary>
        /// Function to unassign events on the data context.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext is null)
            {
                return;
            }

            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        /// <summary>
        /// Function to reset the control back to its original state.
        /// </summary>
        private void ResetDataContext()
        {
            ListPlugIns.Items.Clear();
            TextStatus.Text = string.Empty;
        }

        /// <summary>
        /// Function to initialize the control from the data context.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void InitializeFromDataContext(ISettingsPlugInsList dataContext)
        {
            if (dataContext is null)
            {
                ResetDataContext();
                return;
            }

            FillPlugInList(dataContext);
            TextStatus.Text = dataContext.Current?.DisabledReason ?? string.Empty;
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(ISettingsPlugInsList dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);
            DataContext = dataContext;

            if (DataContext is null)
            {
                return;
            }

            DataContext.PropertyChanged += DataContext_PropertyChanged;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="PlugInListPanel"/> class.</summary>
        public PlugInListPanel() => InitializeComponent();
        #endregion
    }
}
