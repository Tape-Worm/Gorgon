#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Tuesday, April 15, 2014 10:14:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.Properties;
using Gorgon.UI;

namespace Gorgon.Editor
{
    /// <summary>
    /// Preferences for plug-ins.
    /// </summary>
    partial class PanelPlugInPreferences 
        : PreferencePanel
    {
        #region Classes.
        /// <summary>
        /// A list item for the plug-in.
        /// </summary>
        private class PlugInListItem
        {
            #region Properties.
            /// <summary>
            /// Property to return the plug-in for the list item.
            /// </summary>
            public EditorPlugIn PlugIn
            {
                get;
                private set;
            }

            /// <summary>
            /// Property to return the settings interface for the plug-in.
            /// </summary>
            public IPlugInSettingsUI SettingsUI
            {
                get;
                private set;
            }
            #endregion

            #region Methods.
            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.IsNullOrWhiteSpace(PlugIn.Description) ? PlugIn.Name : PlugIn.Description;
            }
            #endregion

            #region Constructor/Destructor.
            /// <summary>
            /// Initializes a new instance of the <see cref="PlugInListItem"/> class.
            /// </summary>
            /// <param name="plugIn">The plug in to assign.</param>
            public PlugInListItem(EditorPlugIn plugIn)
            {
                PlugIn = plugIn;
                SettingsUI = (IPlugInSettingsUI)plugIn;
            }
            #endregion
        }
        #endregion

        #region Variables.
        // Panels for the setting UI.
        private Dictionary<EditorPlugIn, PreferencePanel> _settingPanels = new Dictionary<EditorPlugIn, PreferencePanel>();
        #endregion

        #region Methods.
		/// <summary>
        /// Function to localize the controls on the panel.
        /// </summary>
        protected internal override void LocalizeControls()
        {
            Text = Resources.GOREDIT_TEXT_PLUGIN_PREFS;
            labelPlugIns.Text = string.Format("{0}:", Resources.GOREDIT_TEXT_PLUGINS);
        }

        /// <summary>
        /// Function to fill the plug-in list.
        /// </summary>
        private void FillList()
        {
            listPlugIns.Items.Clear();

            var plugIns = from plugIn in PlugIns.ContentPlugIns
                          where !PlugIns.IsDisabled(plugIn.Value)
                                && plugIn.Value is IPlugInSettingsUI
                          orderby plugIn.Key
                          select plugIn;

            listPlugIns.BeginUpdate();

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                foreach (KeyValuePair<string, ContentPlugIn> plugIn in plugIns)
                {
                    listPlugIns.Items.Add(new PlugInListItem(plugIn.Value));
                }

                if (listPlugIns.Items.Count > 0)
                {
                    listPlugIns.SelectedIndex = 0;
                }
            }
            finally
            {
                listPlugIns.EndUpdate();
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the listPlugIns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void listPlugIns_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (panelPlugInPrefs.Controls.Count > 0)
                {
                    panelPlugInPrefs.Controls.RemoveAt(0);
                }

                if (listPlugIns.SelectedIndex == -1)
                {
                    return;
                }

                var item = (PlugInListItem)listPlugIns.SelectedItem;
                PreferencePanel panel;

                if (!_settingPanels.TryGetValue(item.PlugIn, out panel))
                {
                    panel = item.SettingsUI.GetSettingsUI();
	                
					if (panel != null)
	                {
		                _settingPanels[item.PlugIn] = panel;
	                }
                }

                if (panel == null)
                {
                    return;
                }

                // Add the plug-in settings interface.
                panel.LocalizeControls();
                panel.BorderStyle = BorderStyle.FixedSingle;
                panel.BackColor = DarkFormsRenderer.DarkBackground;
                panel.ForeColor = DarkFormsRenderer.ForeColor;
                panel.Dock = DockStyle.Fill;
	            panel.Content = Content;
	            panel.PlugIn = item.PlugIn;
                panelPlugInPrefs.Controls.Add(panel);

                panel.InitializeSettings();
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                FillList();
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
        }

        /// <summary>
        /// Function to determine if this preference panel should be added as a tab.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the panel can be added as a tab, <c>false</c> if not.
        /// </returns>
        public override bool CanAddAsTab()
        {
            return PlugIns.ContentPlugIns.Any(item => !PlugIns.IsDisabled(item.Value) && item.Value is IPlugInSettingsUI);
        }

        /// <summary>
        /// Function to validate any settings on this panel.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the settings are valid, <c>false</c> if not.
        /// </returns>
        public override bool ValidateSettings()
        {
            return _settingPanels.All(panel => panel.Value.ValidateSettings());
        }

        /// <summary>
        /// Function to commit any settings.
        /// </summary>
        public override void CommitSettings()
        {
            foreach (KeyValuePair<EditorPlugIn, PreferencePanel> panel in _settingPanels)
            {
                try
                {
                    panel.Value.CommitSettings();

                    // If we have content open that uses the plug-in that's been updated, notify its UI (if it has one).
                    if ((ContentManagement.Current != null) 
                        && (ContentManagement.Current.PlugIn == panel.Key))
                    {
						ContentManagement.EditorSettingsUpdated();
                    }
                }
                catch (Exception ex)
                {
                    GorgonDialogs.ErrorBox(ParentForm, ex);
                }
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="PanelPlugInPreferences"/> class.
        /// </summary>
        public PanelPlugInPreferences()
        {
            InitializeComponent();
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="PanelPlugInPreferences"/> class.
		/// </summary>
		/// <param name="content">The currently open content.</param>
	    public PanelPlugInPreferences(ContentObject content)
			: this()
	    {
		    Content = content;
	    }
        #endregion
    }
}
