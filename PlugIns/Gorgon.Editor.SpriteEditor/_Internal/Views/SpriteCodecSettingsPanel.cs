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
// Created: April 20, 2019 2:19:56 PM
// 
#endregion

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The panel used to display settings for image codec support.
    /// </summary>
    internal partial class SpriteCodecSettingsPanel
        : SettingsBaseControl, IDataContext<IImportSettings>
    {
        #region Properties.
        /// <summary>Property to return the ID of the panel.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string PanelID => DataContext?.ID.ToString() ?? Guid.Empty.ToString();

        /// <summary>Property to return the data context assigned to this view.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IImportSettings DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to validate the buttons on the control.
        /// </summary>
        private void ValidateButtons()
        {
            if (DataContext is null)
            {
                ButtonAddCodec.Enabled = ButtonRemoveCodecs.Enabled = false;
                return;
            }

            ButtonAddCodec.Enabled = true;
            ButtonRemoveCodecs.Enabled = DataContext.UnloadPlugInAssembliesCommand?.CanExecute(null) ?? false;
        }

        /// <summary>
        /// Function to populate the list of codecs.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void FillList(IImportSettings dataContext)
        {
            ListCodecs.BeginUpdate();

            try
            {
                ListCodecs.Items.Clear();
                DataContext?.SelectedCodecs.Clear();

                if (dataContext is null)
                {
                    return;
                }

                foreach (CodecSetting setting in dataContext.CodecPlugInPaths)
                {
                    var item = new ListViewItem
                    {
                        Text = setting.Description,
                        Name = setting.Name,
                        Tag = setting
                    };

                    item.SubItems.Add(setting.PlugIn.PlugInPath);

                    ListCodecs.Items.Add(item);
                }
            }
            finally
            {
                ListCodecs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                ListCodecs.EndUpdate();
            }
        }


        /// <summary>Handles the ItemSelectionChanged event of the ListCodecs control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemSelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ListCodecs_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if ((e.Item is null) || (DataContext is null))
            {
                return;
            }

            if (e.Item.Tag is not CodecSetting setting)
            {
                return;
            }

            if (e.IsSelected)
            {
                DataContext.SelectedCodecs.Add(setting);
            }
            else
            {
                DataContext.SelectedCodecs.Remove(setting);
            }
        }

        /// <summary>Handles the Click event of the ButtonAddCodec control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonAddCodec_Click(object sender, EventArgs e)
        {
            if ((DataContext?.LoadPlugInAssemblyCommand is null) || (!DataContext.LoadPlugInAssemblyCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.LoadPlugInAssemblyCommand.Execute(null);
        }

        /// <summary>Handles the Click event of the ButtonRemoveCodecs control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonRemoveCodecs_Click(object sender, EventArgs e)
        {
            if ((DataContext?.UnloadPlugInAssembliesCommand is null) || (!DataContext.UnloadPlugInAssembliesCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.UnloadPlugInAssembliesCommand.Execute(null);
        }

        /// <summary>Handles the CollectionChanged event of the CodecPlugInPaths control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void CodecPlugInPaths_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ListCodecs.BeginUpdate();
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (CodecSetting setting in e.NewItems.OfType<CodecSetting>())
                        {
                            var item = new ListViewItem
                            {
                                Text = setting.Description,
                                Name = setting.Name,
                                Tag = setting
                            };

                            item.SubItems.Add(setting.PlugIn.PlugInPath);

                            ListCodecs.Items.Add(item);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (CodecSetting setting in e.OldItems.OfType<CodecSetting>())
                        {
                            ListViewItem listItem = ListCodecs.Items.OfType<ListViewItem>().FirstOrDefault(item => item.Tag == setting);

                            if (listItem is null)
                            {
                                continue;
                            }

                            ListCodecs.Items.Remove(listItem);
                        }
                        break;
                }
            }
            finally
            {
                ListCodecs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                ListCodecs.EndUpdate();
            }
        }

        /// <summary>Handles the CollectionChanged event of the SelectedCodecs control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void SelectedCodecs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (CodecSetting setting in e.NewItems.OfType<CodecSetting>())
                    {
                        if (ListCodecs.SelectedItems.OfType<ListViewItem>().Any(item => item.Tag == setting))
                        {
                            continue;
                        }

                        foreach (ListViewItem item in ListCodecs.Items.OfType<ListViewItem>().Where(item => item.Tag == setting))
                        {
                            item.Selected = true;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (CodecSetting setting in e.OldItems.OfType<CodecSetting>())
                    {
                        if (!ListCodecs.SelectedItems.OfType<ListViewItem>().Any(item => item.Tag == setting))
                        {
                            continue;
                        }

                        foreach (ListViewItem item in ListCodecs.Items.OfType<ListViewItem>().Where(item => item.Tag == setting))
                        {
                            item.Selected = false;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ListCodecs.SelectedItems.Clear();
                    break;
            }

            ValidateButtons();
        }

        /// <summary>
        /// Function to unassign events from the data context.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext is null)
            {
                return;
            }

            DataContext.SelectedCodecs.CollectionChanged -= SelectedCodecs_CollectionChanged;
            DataContext.CodecPlugInPaths.CollectionChanged -= CodecPlugInPaths_CollectionChanged;
        }

        /// <summary>
        /// Function to restore the control to its default state.
        /// </summary>
        private void ResetDataContext() => FillList(null);

        /// <summary>
        /// Function to initialize the control from the specified data context.
        /// </summary>
        /// <param name="dataContext">The data context to apply.</param>
        private void InitializeFromDataContext(IImportSettings dataContext)
        {
            if (dataContext is null)
            {
                ResetDataContext();
                return;
            }

            FillList(dataContext);
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ValidateButtons();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IImportSettings dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);
            DataContext = dataContext;

            if (DataContext is null)
            {
                return;
            }

            DataContext.CodecPlugInPaths.CollectionChanged += CodecPlugInPaths_CollectionChanged;
            DataContext.SelectedCodecs.CollectionChanged += SelectedCodecs_CollectionChanged;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="SpriteCodecSettingsPanel"/> class.</summary>
        public SpriteCodecSettingsPanel() => InitializeComponent();
        #endregion
    }
}
