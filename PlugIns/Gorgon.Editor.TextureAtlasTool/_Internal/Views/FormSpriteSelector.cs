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
// Created: May 9, 2019 7:50:58 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.UI;
using Gorgon.UI;

namespace Gorgon.Editor.TextureAtlasTool
{
    /// <summary>
    /// A dialog used for sprite selection.
    /// </summary>
    internal partial class FormSpriteSelector
        : KryptonForm, IDataContext<ISpriteFiles>
    {
        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISpriteFiles DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to validate the controls on the form.
        /// </summary>
        private void ValidateControls()
        {
            ButtonLabelMultiSprite.Visible = DataContext?.SelectedFiles.Count < 2;
            ButtonLoad.Enabled = DataContext?.ConfirmLoadCommand?.CanExecute(null) ?? false;
        }

        /// <summary>Handles the Search event of the ContentFileExplorer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GorgonSearchEventArgs"/> instance containing the event data.</param>
        private void ContentFileExplorer_Search(object sender, GorgonSearchEventArgs e)
        {
            if ((DataContext?.SearchCommand == null) || (!DataContext.SearchCommand.CanExecute(e.SearchText)))
            {
                return;
            }

            DataContext.SearchCommand.Execute(e.SearchText);
            ValidateControls();
        }

        /// <summary>Handles the Click event of the ButtonCancel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            if ((DataContext?.CancelCommand == null) || (!DataContext.CancelCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.CancelCommand.Execute(null);
        }

        /// <summary>Handles the Click event of the ButtonLoad control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            if ((DataContext?.ConfirmLoadCommand == null) || (!DataContext.ConfirmLoadCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ConfirmLoadCommand.Execute(null);
        }

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e) => ValidateControls();

        /// <summary>
        /// Function to unassign events from the data context.
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
        /// Function to reset the control back to its default state.
        /// </summary>
        private void ResetDataContext() => ContentFileExplorer.Entries = null;

        /// <summary>
        /// Function to initialize the form from the data context.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void InitializeFromDataContext(ISpriteFiles dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            ContentFileExplorer.Entries = dataContext.SpriteFileEntries;
        }

        /// <summary>Raises the <see cref="Form.FormClosing"/> event.</summary>
        /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            DataContext?.OnUnload();
        }

        /// <summary>Raises the Load event.</summary>
        /// <param name="e">An EventArgs containing event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DataContext?.OnLoad();

            ValidateControls();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(ISpriteFiles dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);
            DataContext = dataContext;

            if (DataContext == null)
            {
                return;
            }

            DataContext.PropertyChanged += DataContext_PropertyChanged;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FormSpriteSelector"/> class.</summary>
        public FormSpriteSelector() => InitializeComponent();
        #endregion
    }
}
