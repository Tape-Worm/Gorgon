#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: March 6, 2020 1:30:22 PM
// 
#endregion

using System;
using System.ComponentModel;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The settings for the emboss effect.
    /// </summary>
    internal partial class FxEmbossSettings 
        : EditorSubPanelCommon, IDataContext<IFxEmboss>
    {
        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        public IFxEmboss DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the ValueChanged event of the NumericEmbossAmount control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericEmbossAmount_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext is null)
            {
                return;
            }

            int currentValue = (int)NumericEmbossAmount.Value;

            if (currentValue == DataContext.Amount)
            {
                return;
            }

            DataContext.Amount = currentValue;
        }

        /// <summary>Function to submit the change.</summary>
        protected override void OnSubmit()
        {
            base.OnSubmit();

            if ((DataContext?.OkCommand is null) || (!DataContext.OkCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.OkCommand.Execute(null);
        }

        /// <summary>Function to cancel the change.</summary>
        protected override void OnCancel()
        {
            base.OnCancel();

            if ((DataContext?.CancelCommand is null) || (!DataContext.CancelCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.CancelCommand.Execute(null);
        }

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFxSharpen.Amount):
                    NumericEmbossAmount.Value = DataContext.Amount;
                    break;
            }
        }

        /// <summary>
        /// Function to unassign the events from the data context.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext is null)
            {
                return;
            }

            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        /// <summary>
        /// Function to initialize the control from the data context.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void InitializeFromDataContext(IFxEmboss dataContext)
        {
            if (dataContext is null)
            {
                NumericEmbossAmount.ValueChanged -= NumericEmbossAmount_ValueChanged;
                NumericEmbossAmount.Value = 1;
                NumericEmbossAmount.ValueChanged += NumericEmbossAmount_ValueChanged;
                return;
            }

            NumericEmbossAmount.Value = dataContext.Amount;
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IFxEmboss dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);

            DataContext = dataContext;

            if (dataContext is null)
            {
                return;
            }

            DataContext.PropertyChanged += DataContext_PropertyChanged;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FxEmbossSettings"/> class.</summary>
        public FxEmbossSettings() => InitializeComponent();
        #endregion
    }
}
