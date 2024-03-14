
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: April 19, 2019 12:31:15 PM
// 


using System.ComponentModel;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Views;

/// <summary>
/// The view used to alter settings for the application
/// </summary>
internal partial class EditorSettingsPanel
    : EditorBaseControl, IDataContext<IEditorSettings>
{

    // The lookup used to locate categories.
    private readonly Dictionary<string, string> _categoryLookup = new(StringComparer.CurrentCultureIgnoreCase);
    // The lookup used to locate panels.
    private readonly Dictionary<string, SettingsBaseControl> _panelLookup = new(StringComparer.OrdinalIgnoreCase);



    /// <summary>Property to return the data context assigned to this view.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IEditorSettings ViewModel
    {
        get;
        private set;
    }



    /// <summary>Handles the SelectedIndexChanged event of the ListCategories control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ListCategories_SelectedIndexChanged(object sender, EventArgs e)
    {
        if ((ListCategories.SelectedIndex < 0) || (ListCategories.SelectedIndex >= ListCategories.Items.Count))
        {
            ListCategories.SelectedIndex = 0;
            return;
        }

        if (!_categoryLookup.TryGetValue(ListCategories.Items[ListCategories.SelectedIndex].ToString(), out string id))
        {
            return;
        }

        if ((ViewModel?.SetCategoryCommand is null) || (!ViewModel.SetCategoryCommand.CanExecute(id)))
        {
            return;
        }

        ViewModel.SetCategoryCommand.Execute(id);
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IEditorSettings.CurrentCategory):
                if (!_panelLookup.TryGetValue(ViewModel.CurrentCategory.ID.ToString(), out SettingsBaseControl value))
                {
                    break;
                }

                foreach (SettingsBaseControl control in SplitSettingsNav.Panel2.Controls.OfType<SettingsBaseControl>().Where(item => item.Visible && item != value))
                {
                    control.SendToBack();
                    control.Visible = false;
                }

                value.Visible = true;
                value.BringToFront();
                break;
        }
    }

    /// <summary>
    /// Function to register the settings panels from plug ins.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void RegisterPlugInSettingsPanels(IEditorSettings dataContext)
    {
        SplitSettingsNav.SuspendLayout();

        try
        {
            ClearExtraPanels(true);

            if (dataContext is null)
            {
                return;
            }

            foreach (ISettingsCategory category in dataContext.Categories)
            {
                if (!ViewFactory.IsRegistered(category))
                {
                    continue;
                }

                SettingsBaseControl panel = ViewFactory.CreateView<SettingsBaseControl>(category);

                if (panel is null)
                {
                    continue;
                }

                ViewFactory.AssignViewModel(category, panel);

                SplitSettingsNav.Panel2.Controls.Add(panel);

                panel.Dock = DockStyle.Fill;
                panel.SendToBack();
                panel.Visible = false;

                category.Load();

                _panelLookup[panel.PanelID] = panel;
            }
        }
        finally
        {
            SplitSettingsNav.ResumeLayout(true);
        }
    }

    /// <summary>
    /// Function to clear out the extra panels.
    /// </summary>
    /// <param name="clearPanelLookup"><b>true</b> to clear the panel look up, <b>false</b> to leave it alone.</param>
    private void ClearExtraPanels(bool clearPanelLookup)
    {
        var controls = new List<SettingsBaseControl>(SplitSettingsNav.Panel2.Controls.OfType<SettingsBaseControl>().Where(item => item != PlugInList));

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

            if ((listItem is not null) && (ListCategories.Items.Contains(listItem)))
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
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged -= DataContext_PropertyChanged;
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
    private void InitializeFromDataContext(IEditorSettings dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        RegisterPlugInSettingsPanels(dataContext);

        int selectedIndex = 0;

        for (int i = 0; i < dataContext.Categories.Count; ++i)
        {
            ISettingsCategory category = dataContext.Categories[i];

            string id = category.ID.ToString();
            if ((!_panelLookup.ContainsKey(id)) || (_categoryLookup.ContainsKey(category.Name)))
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
    /// Function to assign a data context to the view as a view model.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>
    /// <para>
    /// Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.
    /// </para>
    /// </remarks>
    public void SetDataContext(IEditorSettings dataContext)
    {
        PlugInList.SetDataContext(dataContext?.PlugInsList);

        InitializeFromDataContext(dataContext);
        ViewModel = dataContext;

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged += DataContext_PropertyChanged;
    }



    /// <summary>Initializes a new instance of the <see cref="EditorSettingsPanel"/> class.</summary>
    public EditorSettingsPanel()
    {
        InitializeComponent();

        // Register our fixed panel(s).
        _panelLookup[PlugInList.PanelID] = PlugInList;
        _categoryLookup[ListCategories.Items[0].ToString()] = PlugInList.PanelID;
    }

}
