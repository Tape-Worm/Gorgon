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
// Created: March 26, 2019 3:57:12 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.IO;

namespace Gorgon.Editor.UI.Forms
{
    /// <summary>
    /// A form used to input a name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Plug in developers can use this to present a dialog that will prompt the user for a name.
    /// </para>
    /// <para>
    /// An ideal place to use this form would be in the <see cref="ContentPlugIn"/>.<see cref="ContentPlugIn.GetDefaultContentAsync(string, System.Collections.Generic.HashSet{string})"/> method so that users 
    /// could be given a chance to name their objects prior to creation.
    /// </para>
    /// </remarks>
    /// <seealso cref="ContentPlugIn"/>
    public partial class FormName
        : Form
    {
        #region Variables.
        // The object type to be displayed in the textbox cue.
        private string _objectType = string.Empty;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the name of the object.
        /// </summary>
        [Category("Appearance"), Description("The name of the object to be named."), DefaultValue("")]
        public string ObjectName
        {
            get => TextName.Text;
            set => TextName.Text = value?.FormatFileName().Trim() ?? string.Empty;
        }

        /// <summary>
        /// Property to set or return the type of object to be displayed in the textbox cue.
        /// </summary>
        [Category("Appearance"), Description("The type of the object to be displayed in the cue."), DefaultValue("")]
        public string ObjectType
        {
            get => _objectType;
            set
            {
                _objectType = value ?? string.Empty;

                if (string.IsNullOrWhiteSpace(_objectType))
                {
                    TextName.CueText = string.Empty;
                    return;
                }

                TextName.CueText = string.Format(Resources.GOREDIT_TEXT_NAME_CUE, _objectType);
            }
        }
        #endregion

        #region Methods.
        /// <summary>Handles the Leave event of the TextName control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextName_Leave(object sender, EventArgs e)
        {
            PanelLocateText.BackColor = BackColor;
            TextName.BackColor = BackColor;
            TextName.ForeColor = ForeColor;

            if (!string.IsNullOrWhiteSpace(TextName.Text))
            {
                TextName.Text = TextName.Text.Trim().FormatFileName();
            }
        }

        /// <summary>Handles the MouseEnter event of the TextName control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextName_MouseEnter(object sender, EventArgs e)
        {
            if (TextName.Focused)
            {
                return;
            }

            TextName.BackColor = Color.FromKnownColor(KnownColor.SteelBlue);
        }

        /// <summary>Handles the MouseLeave event of the TextName control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextName_MouseLeave(object sender, EventArgs e)
        {
            if (TextName.Focused)
            {
                return;
            }

            PanelLocateText.BackColor = BackColor;
            TextName.BackColor = BackColor;
            TextName.ForeColor = ForeColor;
        }

        /// <summary>Handles the Enter event of the TextName control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextName_Enter(object sender, EventArgs e)
        {
            TextName.Parent.BackColor = TextName.BackColor = Color.White;
            TextName.ForeColor = BackColor;
            TextName.SelectAll();
            PanelLocateText.BackColor = Color.Black;
        }

        /// <summary>Handles the TextChanged event of the TextName control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextName_TextChanged(object sender, EventArgs e) => ButtonOK.Enabled = !string.IsNullOrWhiteSpace(ObjectName);

        /// <summary>Handles the <see cref="UserControl.Load"/> event.</summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            TextName.Select();
            TextName.SelectAll();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FormName"/> class.</summary>
        public FormName() => InitializeComponent();
        #endregion
    }
}
