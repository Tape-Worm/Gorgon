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
// Created: April 19, 2019 1:25:38 PM
// 
#endregion

using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Editor.UI.Views
{
    /// <summary>
    /// The base control used for settings panels.
    /// </summary>
    public partial class SettingsBaseControl 
		: EditorBaseControl
    {
        #region Variables.
		/// <summary>
        /// The panel that will contain the controls for the settings.
        /// </summary>
        protected Panel PanelBody;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the ID of the panel.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual string PanelID
        {
            get;
        }

        /// <summary>Gets or sets the text associated with this control.</summary>
        [Browsable(true), Category("Appearance"), Description("Sets the caption for the panel."), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Localizable(true)]
        public new string Text
        {
            get => base.Text;
            set => LabelCaption.Text =base.Text = value;
        }
        #endregion

        #region Methods.

        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.UI.Views.SettingsBaseControl"/> class.</summary>
        public SettingsBaseControl() => InitializeComponent();
        #endregion
    }
}
