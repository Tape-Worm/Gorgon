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
// Created: January 15, 2019 9:04:41 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The panel used to provide settings for setting image alpha.
    /// </summary>
    internal partial class SetAlphaSettings
        : EditorSubPanelCommon, IDataContext<IAlphaSettings>
    {
        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        /// <value>The data context.</value>
        [Browsable(false)]
        public IAlphaSettings DataContext
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
            switch (e.PropertyName)
            {
                case nameof(IAlphaSettings.AlphaValue):
                    NumericAlphaValue.Value = DataContext.AlphaValue;
                    ImageAlpha.Refresh();
                    break;
                case nameof(IAlphaSettings.UpdateRange):
                    NumericMinAlpha.Value = DataContext.UpdateRange.Minimum;
                    NumericMaxAlpha.Value = DataContext.UpdateRange.Maximum;
                    break;
            }

            ValidateOk();
        }

        /// <summary>Handles the ValueChanged event of the NumericMinAlpha control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericMinAlpha_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.UpdateRange = new GorgonRange((int)NumericMinAlpha.Value, DataContext.UpdateRange.Maximum);
        }

        /// <summary>Handles the ValueChanged event of the NumericMaxAlpha control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericMaxAlpha_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.UpdateRange = new GorgonRange(DataContext.UpdateRange.Minimum, (int)NumericMaxAlpha.Value);
        }

        /// <summary>Handles the ValueChanged event of the NumericMipLevels control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericAlphaValue_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.AlphaValue = (int)NumericAlphaValue.Value;
        }


        /// <summary>Handles the Paint event of the ImageAlpha control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void ImageAlpha_Paint(object sender, PaintEventArgs e)
        {
            using (var brush = new SolidBrush(Color.FromArgb((int)NumericAlphaValue.Value, 255, 255, 255)))
            {
                e.Graphics.FillRectangle(brush, ImageAlpha.ClientRectangle);
            }
        }

        /// <summary>
        /// Function to unassign the events assigned to the datacontext.
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
        /// Function called when the view should be reset by a <b>null</b> data context.
        /// </summary>
        private void ResetDataContext()
        {
            NumericAlphaValue.Value = 255;
            NumericMinAlpha.Value = 0;
            NumericMaxAlpha.Value = 255;
        }

        /// <summary>
        /// Function to initialize the view from the current data context.
        /// </summary>
        /// <param name="dataContext">The data context being assigned.</param>
        private void InitializeFromDataContext(IAlphaSettings dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            NumericAlphaValue.Value = dataContext.AlphaValue;
            NumericMinAlpha.Value = dataContext.UpdateRange.Minimum;
            NumericMaxAlpha.Value = dataContext.UpdateRange.Maximum;
        }

        /// <summary>Function to cancel the change.</summary>
        protected override void OnCancel()
        {
            base.OnCancel();

            if ((DataContext?.CancelCommand == null) || (!DataContext.CancelCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.CancelCommand.Execute(null);
        }

        /// <summary>Function to submit the change.</summary>
        protected override void OnSubmit()
        {
            base.OnSubmit();

            if ((DataContext?.OkCommand == null) || (!DataContext.OkCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.OkCommand.Execute(null);
        }

        /// <summary>Function called to validate the OK button.</summary>
        /// <returns>
        ///   <b>true</b> if the OK button is valid, <b>false</b> if not.</returns>
        protected override bool OnValidateOk() => (DataContext?.OkCommand != null) && (DataContext.OkCommand.CanExecute(null));

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (IsDesignTime)
            {
                return;
            }

            ValidateOk();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IAlphaSettings dataContext)
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
        /// <summary>Initializes a new instance of the <see cref="SetAlphaSettings"/> class.</summary>
        public SetAlphaSettings() => InitializeComponent();
        #endregion
    }
}
