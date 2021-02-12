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
// Created: March 28, 2019 9:48:28 AM
// 
#endregion

using System;
using System.ComponentModel;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics;

namespace Gorgon.Examples
{
    /// <summary>
    /// A color selection control for the text.
    /// </summary>
    /// <remarks>
    /// This view is a sub panel of the main view. It shows up when input is required from the user before applying that input to the actual content.
    /// When the sub panel is activated it appears on the right hand side of the content view and can have an OK/Cancel button if the panel is modal 
    /// or it can apply its values to the content in real time if not.
    /// 
    /// In this example, we use this sub panel to provide us with a color picker editor that will adjust the color of the text content. 
    /// 
    /// To implement a sub panel, inherit from the EditorSubPanelCommon class and implement an IDataContext interface.
    /// </remarks>
    internal partial class TextColorView
        : EditorSubPanelCommon, IDataContext<ITextColor>
    {
        #region Properties.
        /// <summary>
        /// Property to return the data context for the view.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ITextColor DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the ColorChanged event of the Picker control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Gorgon.Editor.UI.Controls.ColorChangedEventArgs"/> instance containing the event data.</param>
        private void Picker_ColorChanged(object sender, ColorChangedEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            // When the color picker control is updated, we intercept its event and pass it back to the view model. This allows 
            // things like renderers to pick up the change and adjust the view for us.

            DataContext.SelectedColor = e.Color;

            // This method should be caused anytime there's a change. It will disable the control based on execution validation 
            // provided by a command object on the view model.
            ValidateOk();
        }

        /// <summary>
        /// Function to unassign the events from the data context.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext == null)
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
            // This event handler is used to capture the changes from the view model. We use this to modify the picker control 
            // color values based on what the view model property values. This allows us to synchronize our view with the view 
            // model.
            //
            // Note that some controls are jerks. Raising their value change events constantly. So the rule of thumb should be 
            // to turn off the events for the control prior to assigning the value, and turning it back on. In this case we 
            // don't need to do that, but for other control types, this may be required.

            switch (e.PropertyName)
            {
                case nameof(ITextColor.OriginalColor):
                    Picker.OriginalColor = DataContext.OriginalColor;
                    break;
                case nameof(ITextColor.SelectedColor):
                    Picker.SelectedColor = DataContext.SelectedColor;                    
                    break;
            }
        }

        /// <summary>
        /// Function to reset the values on the control for a null data context.
        /// </summary>
        private void ResetDataContext()
        {
            UnassignEvents();
            Picker.OriginalColor = Picker.SelectedColor = GorgonColor.Black;
        }
        
        /// <summary>
        /// Function to initialize the control with the data context.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        private void InitializeFromDataContext(ITextColor dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            Picker.OriginalColor = dataContext.OriginalColor;
            Picker.SelectedColor = dataContext.SelectedColor;
        }

        /// <summary>Function to submit the change.</summary>
        protected override void OnSubmit()
        {
            base.OnSubmit();

            // This handles the OK button click by executing a command on the view model. You'll note that we check to 
            // ensure that the command can actually be executed and that it's actually assigned to the property on the 
            // view model. This allows us to ensure that the operation doesn't cause any problems if state is not 
            // correct.

            if ((DataContext?.OkCommand == null) || (!DataContext.OkCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.OkCommand.Execute(null);
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

        // This method validates our OK button on the panel. We use the command to check whether the state of the view model
        // will allow us to continue or not. 
        //
        // Typically, and in the case of sub panels like this, we disable the button/control associated with the command when 
        // the CanExecute returns false (or the command is not assigned to the view model).

        /// <summary>
        /// Function to validate the state of the OK button.
        /// </summary>
        /// <returns><b>true</b> if the OK button is valid, <b>false</b> if not.</returns>
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

            DataContext?.OnLoad();
        }
        
        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(ITextColor dataContext)
        {
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
        /// <summary>Initializes a new instance of the <see cref="TextColorView"/> class.</summary>
        public TextColorView() => InitializeComponent();
        #endregion
    }
}
