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
// Created: April 19, 2019 12:31:15 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.UI;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Views
{
	/// <summary>
    /// The view used to alter settings for the application.
    /// </summary>
    internal partial class EditorSettingsPanel 
		: EditorBaseControl, IDataContext<IEditorSettingsVm>
    {
        #region Variables.
		// The lookup used to locate categories.
        private readonly Dictionary<string, string> _categoryLookup = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
		// The lookup used to locate panels.
        private readonly Dictionary<string, SettingsBaseControl> _panelLookup = new Dictionary<string, SettingsBaseControl>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEditorSettingsVm DataContext
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

        }

        /// <summary>
        /// Function to clear out the extra panels.
        /// </summary>
        /// <param name="clearPanelLookup"><b>true</b> to clear the panel look up, <b>false</b> to leave it alone.</param>
        private void ClearExtraPanels(bool clearPanelLookup)
        {
            var controls = new List<SettingsBaseControl>(SplitSettingsNav.Panel2.Controls.OfType<SettingsBaseControl>().Where(item => item != PluginsPanel_00000000000000000000000000000000));

            ListCategories.SelectedIndex = 0;

            // Clean out all other panels except our fixed ones.
            foreach (SettingsBaseControl control in controls)
            {
                string listItem = _categoryLookup.FirstOrDefault(item => string.Equals(item.Value, control.PanelID, StringComparison.OrdinalIgnoreCase)).Key;

                if (clearPanelLookup)
                {
                    string id = _panelLookup.Where(item => item.Value == control).Select(item => item.Key).FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        _panelLookup.Remove(id);
                    }

                    SplitSettingsNav.Panel2.Controls.Remove(control);
                }

                if ((listItem != null) && (ListCategories.Items.Contains(listItem)))
                {
                    ListCategories.Items.Remove(listItem);
                    _categoryLookup.Remove(listItem);
                }
            }
        }

        /// <summary>
        /// Function to unassign any events on the data context.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

		/// <summary>
        /// Function to reset the control back to its original state when no data context is available.
        /// </summary>
        private void ResetDataContext()
        {
            UnassignEvents();
            ClearExtraPanels(true);
        }

		/// <summary>
        /// Function to initialize the control based on the data context.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void InitializeFromDataContext(IEditorSettingsVm dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            int selectedIndex = 0;

            for (int i = 0; i < dataContext.Categories.Count; ++i)
            {
                ISettingsCategoryViewModel category = dataContext.Categories[i];

                string id = category.ID.ToString("N");
                if (!_panelLookup.ContainsKey(id))
                {
                    continue;
                }

                if (dataContext.CurrentCategory == category)
                {
                    selectedIndex = i;
                }

                ListCategories.Items.Add(category.Name);
                _categoryLookup[category.Name] = id;
            }

            ListCategories.SelectedIndex = selectedIndex;
        }

		/// <summary>
        /// Function to register the settings panels from plug ins.
        /// </summary>
        /// <param name="panels">The list of panels to evaluate.</param>
        /// <remarks>
        /// <para>
        /// This method must be called prior to calling <see cref="SetDataContext"/>.
        /// </para>
        /// </remarks>
        public void RegisterPluginSettingsPanels(IEnumerable<SettingsBaseControl> panels)
        {
            if (panels == null)
            {
                return;
            }

            SplitSettingsNav.SuspendLayout();

            try
            {
                ClearExtraPanels(true);

                foreach (SettingsBaseControl panel in panels)
                {
                    SplitSettingsNav.Panel2.Controls.Add(panel);

                    panel.Dock = DockStyle.Fill;
                    panel.SendToBack();
                    panel.Visible = false;
                }
            }
            finally
            {
                SplitSettingsNav.ResumeLayout(true);
            }
        }

        /// <summary>
        /// Function to assign a data context to the view as a view model.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>
        /// <para>
        /// Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.
        /// </para>
        /// </remarks>
        public void SetDataContext(IEditorSettingsVm dataContext)
        {
            InitializeFromDataContext(dataContext);
            DataContext = dataContext;
            PluginsPanel_00000000000000000000000000000000.SetDataContext(dataContext?.PluginsList);

            if (DataContext == null)
            {
                return;
            }

            DataContext.PropertyChanged += DataContext_PropertyChanged;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="EditorSettingsPanel"/> class.</summary>
        public EditorSettingsPanel()
        {
            InitializeComponent();

			// Register our fixed panel(s).
            _panelLookup[PluginsPanel_00000000000000000000000000000000.PanelID] = PluginsPanel_00000000000000000000000000000000;
        }
        #endregion
    }
}
