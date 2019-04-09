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
// Created: March 23, 2019 9:42:34 PM
// 
#endregion

using System;
using System.ComponentModel;
using ComponentFactory.Krypton.Toolkit;
using DX = SharpDX;
using Gorgon.Editor.UI;
using Gorgon.UI;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The window for the manual rectangle input interface.
    /// </summary>
    internal partial class FormManualRectangleEdit
        : KryptonForm, IDataContext<IManualRectangleEditor>, IManualInputControl
    {
        #region Variables.
        // Flag to indicate that the value changed event for the numeric controls should not fire.
        private bool _noValueEvent;
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        /// <value>The data context.</value>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IManualRectangleEditor DataContext
        {
            get;
            private set;
        }

        /// <summary>Property to return the data context assigned to this view.</summary>
        IManualInputViewModel IManualInputControl.DataContext => DataContext;
        #endregion

        #region Methods.
        /// <summary>Handles the ValueChanged event of the NumericLeft control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericLeft_ValueChanged(object sender, EventArgs e)
        {
            if ((DataContext == null) || (_noValueEvent))
            {
                return;
            }

            var newRect = new DX.RectangleF
            {
                Left = (float)NumericLeft.Value,
                Top = (float)NumericTop.Value,
                Right = (float)NumericRight.Value,
                Bottom = (float)NumericBottom.Value
            };

            if (newRect.Equals(DataContext.Rectangle))
            {
                return;
            }

            DataContext.Rectangle = newRect;
        }

        /// <summary>
        /// Function to assign the values for the rectangle into the numeric inputs.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void SetRectangleInputs(IManualRectangleEditor dataContext)
        {
            try
            {
                _noValueEvent = true;

                NumericLeft.Value = (decimal)dataContext.Rectangle.Left;
                NumericTop.Value = (decimal)dataContext.Rectangle.Top;
                NumericRight.Value = (decimal)dataContext.Rectangle.Right;
                NumericBottom.Value = (decimal)dataContext.Rectangle.Bottom;
            }
            finally
            {
                _noValueEvent = false;
            }
        }

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IManualRectangleEditor.IsFixedSize):
					LabelRight.Enabled = LabelBottom.Enabled = 
                    NumericRight.Enabled = NumericBottom.Enabled = !DataContext.IsFixedSize;
                    break;                
                case nameof(IManualRectangleEditor.IsActive):
                    if (DataContext.IsActive)
                    {
                        Show(GorgonApplication.MainForm);
                    }
                    else
                    {
                        Hide();
                    }
                    break;
                case nameof(IManualRectangleEditor.Padding):
                case nameof(IManualRectangleEditor.FixedSize):
                case nameof(IManualRectangleEditor.Rectangle):
                    SetRectangleInputs(DataContext);
                    break;
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
            NumericLeft.Maximum = NumericRight.Maximum = NumericTop.Maximum = NumericBottom.Maximum = 16384;
            NumericLeft.Value = NumericTop.Value = NumericRight.Value = NumericBottom.Value = 0;
            LabelRight.Enabled = LabelBottom.Enabled = NumericRight.Enabled = NumericBottom.Enabled = true;
        }

        /// <summary>
        /// Function to initialize the view from the current data context.
        /// </summary>
        /// <param name="dataContext">The data context being assigned.</param>
        private void InitializeFromDataContext(IManualRectangleEditor dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            LabelRight.Enabled = LabelBottom.Enabled =
            NumericRight.Enabled = NumericBottom.Enabled = !dataContext.IsFixedSize;

            SetRectangleInputs(dataContext);
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.ResizeBegin"/> event.</summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);

            if (DataContext == null)
            {
                return;
            }

            DataContext.IsMoving = true;
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.ResizeEnd"/> event.</summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            if (DataContext == null)
            {
                return;
            }

            DataContext.IsMoving = false;
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            DataContext?.OnLoad();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IManualRectangleEditor dataContext)
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
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.FormManualRectInput"/> class.</summary>
        public FormManualRectangleEdit() => InitializeComponent();        
        #endregion
    }
}
