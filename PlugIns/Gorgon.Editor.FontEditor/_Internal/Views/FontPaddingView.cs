#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: September 3, 2021 9:37:58 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Threading;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics;

namespace Gorgon.Editor.FontEditor
{
    /// <summary>
    /// The view for changing padding between glyphs on a texture.
    /// </summary>
    internal partial class FontPaddingView
        : EditorSubPanelCommon, IDataContext<IFontPadding>
    {
        #region Variables.
        // The event hook counter.
        private int _eventHook;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the data context for the view.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IFontPadding DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the ValueChanged event of the NumericPadding control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void NumericPadding_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext is null)
            {
                return;
            }

            DataContext.Padding = (int)NumericPadding.Value;
            ValidateOk();
        }

        /// <summary>
        /// Function to unhook the events.
        /// </summary>
        private void UnhookEvents()
        {
            if (Interlocked.Exchange(ref _eventHook, 0) == 0)
            {
                return;
            }

            NumericPadding.ValueChanged -= NumericPadding_ValueChanged;
        }

        /// <summary>
        /// Function to hook events.
        /// </summary>
        private void HookEvents()
        {
            if (Interlocked.Exchange(ref _eventHook, 1) != 0)
            {
                return;
            }

            NumericPadding.ValueChanged += NumericPadding_ValueChanged;
        }

        /// <summary>
        /// Function to unassign the events from the data context.
        /// </summary>
        private void UnassignEvents()
        {
            UnhookEvents();

            if (DataContext is null)
            {
                return;
            }

            // Always unassign your view model events. Failure to do so can result in an event leak, causing the view to stay 
            // in memory for the lifetime of the application.

            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UnhookEvents();

            switch (e.PropertyName)
            {
                case nameof(IFontPadding.Padding):
                    NumericPadding.Value = DataContext.Padding;
                    break;
            }

            HookEvents();

            ValidateOk();
        }

        /// <summary>
        /// Function to reset the values on the control for a null data context.
        /// </summary>
        private void ResetDataContext() => UnassignEvents();

        /// <summary>
        /// Function to initialize the control with the data context.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        private void InitializeFromDataContext(IFontPadding dataContext)
        {
            if (dataContext is null)
            {
                ResetDataContext();
                return;
            }
        }

        /// <summary>
        /// Function to validate the state of the OK button.
        /// </summary>
        /// <returns><b>true</b> if the OK button is valid, <b>false</b> if not.</returns>
        protected override bool OnValidateOk() => (DataContext?.OkCommand is not null) && (DataContext.OkCommand.CanExecute(null));

        /// <summary>Function to cancel the change.</summary>
        protected override void OnCancel()
        {
            if (DataContext is not null)
            {
                DataContext.IsActive = false;
            }
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

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (IsDesignTime)
            {
                return;
            }

            DataContext?.Load();

            NumericPadding.Select();

            ValidateOk();
        }
        
        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IFontPadding dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);

            DataContext = dataContext;

            if (DataContext is null)
            {
                ValidateOk();
                return;
            }

            HookEvents();

            DataContext.PropertyChanged += DataContext_PropertyChanged;
            ValidateOk();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FontPaddingView"/> class.</summary>
        public FontPaddingView() => InitializeComponent();
        #endregion
    }
}
