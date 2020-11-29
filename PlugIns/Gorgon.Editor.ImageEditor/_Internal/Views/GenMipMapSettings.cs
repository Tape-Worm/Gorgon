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
using System.Windows.Forms;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The panel used to provide settings for image import resizing.
    /// </summary>
    internal partial class GenMipMapSettings
        : EditorSubPanelCommon, IDataContext<IMipMapSettings>
    {
        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        /// <value>The data context.</value>
        [Browsable(false)]
        public IMipMapSettings DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update whether mip maps are supported or not.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void UpdateMipSupport(IMipMapSettings dataContext)
        {
            if (dataContext == null)
            {
                LabelMipLevels.Enabled = NumericMipLevels.Enabled = false;
            }
            else
            {
                LabelMipLevels.Enabled = NumericMipLevels.Enabled = true;
            }
        }

        /// <summary>Handles the SelectedValueChanged event of the ComboImageFilter control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ComboImageFilter_SelectedValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.MipFilter = (ImageFilter)ComboMipFilter.SelectedItem;
        }

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IMipMapSettings.MipLevels):
                case nameof(IMipMapSettings.MaxMipLevels):
                    UpdateNumericUpDown(NumericMipLevels, DataContext.MaxMipLevels, DataContext.MipLevels);
                    break;
            }

            ValidateOk();
        }

        /// <summary>
        /// Function to update a numeric up/down control.
        /// </summary>
        /// <param name="control">The control to update.</param>
        /// <param name="maxValue">The maximum value for the control.</param>
        /// <param name="currentValue">The current value for the control.</param>
        /// <param name="steps">[Optional] The number of steps used to increment the value.</param>
        private void UpdateNumericUpDown(NumericUpDown control, int maxValue, int currentValue, int steps = 1)
        {
            control.Maximum = maxValue;
            control.Value = currentValue;
            control.Increment = steps;
        }

        /// <summary>Handles the ValueChanged event of the NumericMipLevels control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericMipLevels_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.MipLevels = (int)NumericMipLevels.Value;
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
            ComboMipFilter.Text = string.Empty;
            ComboMipFilter.Items.Clear();
            UpdateMipSupport(null);
        }

        /// <summary>
        /// Function to initialize the view from the current data context.
        /// </summary>
        /// <param name="dataContext">The data context being assigned.</param>
        private void InitializeFromDataContext(IMipMapSettings dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            ComboMipFilter.SelectedItem = dataContext.MipFilter;
            UpdateMipSupport(dataContext);
            UpdateNumericUpDown(NumericMipLevels, dataContext.MaxMipLevels, dataContext.MipLevels);
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
        /// <param name="e">An <see cref="System.EventArgs"/> that contains the event data.</param>
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
        public void SetDataContext(IMipMapSettings dataContext)
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
        /// <summary>Initializes a new instance of the <see cref="Gorgon.Editor.Views.GenMipMapSettings"/> class.</summary>
        public GenMipMapSettings()
        {
            InitializeComponent();

            // Populate the image filter drop down.
            var filters = (ImageFilter[])Enum.GetValues(typeof(ImageFilter));

            foreach (ImageFilter filter in filters)
            {
                ComboMipFilter.Items.Add(filter);
            }
        }
        #endregion
    }
}
