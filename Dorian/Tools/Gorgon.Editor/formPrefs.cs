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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using KRBTabControl;
using GorgonLibrary.IO;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
    /// <summary>
    /// Handles the preferences and plug-ins management for the application.
    /// </summary>
    public partial class formPrefs 
        : ZuneForm
    {
        #region Variables.
        private List<PreferencePanel> _prefPanels = null;               // A list of our loaded preference panels.
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>
        /// Function to populate the values for tabs.
        /// </summary>
        private void PopulateTabValues()
        {
            for (int i = 0; i < _prefPanels.Count; i++)
            {
                PreferencePanel panel = _prefPanels[i];
                TabPageEx page = new TabPageEx(panel.Text);

                page.BackColor = DarkFormsRenderer.WindowBackground;
                page.ForeColor = Color.White;
                page.Text = panel.Text;
                page.Font = tabPrefs.Font;
                page.IsClosable = false;                

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
            try
            {
                for (int i = 0; i < _prefPanels.Count; i++)
                {
                    PreferencePanel panel = _prefPanels[i];

                    if (!panel.ValidateSettings())
                    {
                        DialogResult = System.Windows.Forms.DialogResult.None;
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
                _prefPanels = new List<PreferencePanel>();

                // Add our default panel.
                _prefPanels.Add(new EditorPreferencePanel());

                // TODO: Get preference panels from plug-ins.
                
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
        /// Initializes a new instance of the <see cref="formPrefs"/> class.
        /// </summary>
        public formPrefs()
        {
            InitializeComponent();
        }
        #endregion
    }
}
