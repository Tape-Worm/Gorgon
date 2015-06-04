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
// Created: Thursday, April 11, 2013 9:10:55 AM
// 
#endregion

using System.ComponentModel;
using System.Windows.Forms;
using Gorgon.Design;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor
{
    /// <summary>
    /// Base panel used for preferences in the editor.
    /// </summary>
    public partial class PreferencePanel : UserControl
    {
        #region Variables.

        #endregion

		#region Properties.
		/// <summary>
		/// Property to return the plug-in associated with the preferences.
		/// </summary>
	    protected internal EditorPlugIn PlugIn
	    {
		    get;
		    internal set;
	    }

		/// <summary>
		/// Property to return the currently open content.
		/// </summary>
	    protected internal ContentObject Content
	    {
		    get;
		    internal set;
	    }

        /// <summary>
        /// Property to set or return the text caption for this panel.
        /// </summary>
        [Browsable(true)]
        [LocalCategory(typeof(APIResources), "PROP_CATEGORY_DESIGN")]
        [LocalDescription(typeof(APIResources), "PROP_TEXT_PREF_PANEL_DESC")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                base.Text = value;
            }
        }

        /// <summary>
        /// Property to return whether to refresh the tree after the preferences are committed.
        /// </summary>
        [Browsable(false)]
        public bool RefreshTree
        {
            get;
            protected set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to localize the text on the controls for the panel.
        /// </summary>
        /// <remarks>Override this method to supply localized text for any controls on the panel.</remarks>
        protected internal virtual void LocalizeControls()
        {
            Text = APIResources.GOREDIT_TEXT_PREFERENCES;
        }

        /// <summary>
        /// Function to determine if this preference panel should be added as a tab.
        /// </summary>
        /// <returns><b>true</b> if the panel can be added as a tab, <b>false</b> if not.</returns>
        public virtual bool CanAddAsTab()
        {
            return true;
        }

        /// <summary>
        /// Function to validate any settings on this panel.
        /// </summary>
        /// <returns><b>true</b> if the settings are valid, <b>false</b> if not.</returns>
        public virtual bool ValidateSettings()
        {
            return true;
        }

        /// <summary>
        /// Function to commit any settings.
        /// </summary>
        public virtual void CommitSettings()
        {
        }

        /// <summary>
        /// Function to read the current settings into their respective controls.
        /// </summary>
        public virtual void InitializeSettings()
        {
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="PreferencePanel"/> class.
        /// </summary>
        public PreferencePanel()
        {
            InitializeComponent();
	        toolHelp.ToolTipTitle = APIResources.GOREDIT_TEXT_HELP;
        }
        #endregion
    }
}
