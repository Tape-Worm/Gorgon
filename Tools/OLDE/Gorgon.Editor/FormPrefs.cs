#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Wednesday, April 10, 2013 12:04:38 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GorgonLibrary.Editor.Controls;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.UI;
using KRBTabControl;

namespace GorgonLibrary.Editor
{
    /// <summary>
    /// Handles the preferences and plug-ins management for the application.
    /// </summary>
	partial class FormPreferences 
        : FlatForm
    {
        #region Variables.
        private IList<PreferencePanel> _prefPanels;               // A list of our loaded preference panels.
        #endregion

        #region Methods.
		/// <summary>
        /// Function to populate the values for tabs.
        /// </summary>
        private void PopulateTabValues()
        {
            foreach (PreferencePanel panel in _prefPanels.Where(item => item.CanAddAsTab()))
            {
                panel.LocalizeControls();

                var page = new TabPageEx(panel.Text)
                           {
                               BackColor = DarkFormsRenderer.WindowBackground,
                               ForeColor = Color.White,
                               Text = panel.Text,
                               Font = tabPrefs.Font,
                               IsClosable = false
                           };

                tabPrefs.TabPages.Add(page);

                page.Controls.Add(panel);

                if (panel.Height > page.ClientSize.Height)
                {
                    panel.Width = page.ClientSize.Width - (SystemInformation.VerticalScrollBarWidth + 2);
                    page.AutoScrollMinSize = new Size(panel.Width, panel.Height);                    
                }
                else
                {
                    panel.Width = page.ClientSize.Width;
                    panel.Height = page.ClientSize.Height;
                }

                panel.InitializeSettings();
            }
        }

        /// <summary>
        /// Handles the Click event of the buttonOK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                foreach (PreferencePanel panel in _prefPanels)
                {
                    if (!panel.ValidateSettings())
                    {
                        GorgonDialogs.ErrorBox(this, Resources.GOREDIT_DLG_SETTINGS_INVALID);
                        DialogResult = DialogResult.None;
                    }
                    else
                    {
                        panel.CommitSettings();
                    }
                }
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(this, ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

		/// <summary>
		/// Function to localize the text in the controls on the form.
		/// </summary>
	    private void LocalizeControls()
		{
			buttonOK.Text = Resources.GOREDIT_ACC_TEXT_OK;
			buttonCancel.Text = Resources.GOREDIT_ACC_TEXT_CANCEL;
			Text = Resources.GOREDIT_TEXT_PREFERENCES;
		}

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
				LocalizeControls();

                _prefPanels = new PreferencePanel[]
                              {
                                  new EditorPreferencePanel(),
                                  new PanelPlugIns(), 
                                  new PanelPlugInPreferences(ContentManagement.Current)
                              };
                
                PopulateTabValues();
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(this, ex);
                Close();
            }            
        }
	    #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="FormPreferences"/> class.
        /// </summary>
        public FormPreferences()
        {
            InitializeComponent();
        }
        #endregion
    }
}
