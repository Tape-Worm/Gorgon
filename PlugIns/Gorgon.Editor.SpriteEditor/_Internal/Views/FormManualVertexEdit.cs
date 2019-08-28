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
// Created: April 10, 2019 8:19:52 AM
// 
#endregion

using System;
using System.ComponentModel;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.UI;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Provides an interface to allow manual update of a sprite's vertex corners.
    /// </summary>
    internal partial class FormManualVertexEdit
        : KryptonForm, IDataContext<IManualVertexEditor>, IManualInputControl
    {
        #region Variables.

        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        public IManualVertexEditor DataContext
        {
            get;
            private set;
        }

        /// <summary>Property to return the data context assigned to this view.</summary>
        IManualInputViewModel IManualInputControl.DataContext => DataContext;
        #endregion

        #region Methods.
        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IManualVertexEditor.IsActive):
                    if (DataContext.IsActive)
                    {
                        Show(GorgonApplication.MainForm);
                    }
                    else
                    {
                        Hide();
                    }
                    break;
                case nameof(IManualVertexEditor.SelectedVertexIndex):
                case nameof(IManualVertexEditor.Offset):
                    GetOffsetFromDataContext(DataContext);
                    break;
            }
        }

        /// <summary>Handles the ValueChanged event of the NumericX or Y control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericOffset_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.Offset = new DX.Vector2((float)NumericX.Value, (float)NumericY.Value);
        }

        /// <summary>
        /// Function to retrieve the current vertex offset value from the data context.
        /// </summary>
        /// <param name="dataContext">The data context containing the offset value.</param>
        private void GetOffsetFromDataContext(IManualVertexEditor dataContext)
        {
            NumericX.ValueChanged -= NumericOffset_ValueChanged;
            NumericY.ValueChanged -= NumericOffset_ValueChanged;

            try
            {
                if ((dataContext.SelectedVertexIndex < 0) || (dataContext.SelectedVertexIndex > 3))
                {
                    tableLayoutPanel1.Enabled = false;
                    NumericX.Value = 0;
                    NumericY.Value = 0;
                }
                else
                {
                    tableLayoutPanel1.Enabled = true;
                    NumericX.Value = (decimal)dataContext.Offset.X;
                    NumericY.Value = (decimal)dataContext.Offset.Y;
                }
            }
            finally
            {
                NumericX.ValueChanged += NumericOffset_ValueChanged;
                NumericY.ValueChanged += NumericOffset_ValueChanged;
            }
        }

        /// <summary>
        /// Function to unassign events subscribed to the data context.
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
        /// Function to reset the state of the window without a data context.
        /// </summary>
        private void ResetDataContext()
        {
            UnassignEvents();

            NumericX.Value = 0;
            NumericY.Value = 0;
        }

        /// <summary>
        /// Function to initialize the window based on the current data context.
        /// </summary>
        /// <param name="dataContext"></param>
        private void InitializeFromDataContext(IManualVertexEditor dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            GetOffsetFromDataContext(dataContext);
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

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IManualVertexEditor dataContext)
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
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.FormManualVertexEdit"/> class.</summary>
        public FormManualVertexEdit() => InitializeComponent();
        #endregion
    }
}
