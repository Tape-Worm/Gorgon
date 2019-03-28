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
// Created: April 4, 2019 8:58:41 AM
// 
#endregion

using System;
using System.ComponentModel;
using DX = SharpDX;
using Gorgon.Editor.UI.Controls;
using Gorgon.Editor.UI;
using Gorgon.Math;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// A view used to manually update the sprite anchor value.
    /// </summary>
    internal partial class SpriteAnchor 
        : EditorSubPanelCommon, IDataContext<ISpriteAnchorEdit>
    {
        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        public ISpriteAnchorEdit DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.        
        /// <summary>Handles the ValueChanged event of the NumericHorizontal control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericHorizontal_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.AnchorPosition = new DX.Vector2((float)NumericHorizontal.Value, DataContext.AnchorPosition.Y);
            ValidateOk();
        }

        /// <summary>Handles the ValueChanged event of the NumericVertical control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericVertical_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.AnchorPosition = new DX.Vector2(DataContext.AnchorPosition.X, (float)NumericVertical.Value);
            ValidateOk();
        }

        /// <summary>Handles the AlignmentChanged event of the Alignment control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Alignment_AlignmentChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            decimal w = (decimal)DataContext.Bounds.Width;
            decimal h = (decimal)DataContext.Bounds.Height;
            decimal mx = (int)(w  * 0.5M);
            decimal my = (int)(h * 0.5M);

            switch (Alignment.Alignment)
            {
                case Gorgon.UI.Alignment.UpperLeft:
                    NumericHorizontal.Value = 0;
                    NumericVertical.Value = 0;
                    break;
                case Gorgon.UI.Alignment.UpperCenter:
                    NumericHorizontal.Value = mx;
                    NumericVertical.Value = 0;
                    break;
                case Gorgon.UI.Alignment.UpperRight:
                    NumericHorizontal.Value = w;
                    NumericVertical.Value = 0;
                    break;
                case Gorgon.UI.Alignment.CenterLeft:
                    NumericHorizontal.Value = 0;
                    NumericVertical.Value = my;
                    break;
                case Gorgon.UI.Alignment.Center:
                    NumericHorizontal.Value = mx;
                    NumericVertical.Value = my;
                    break;
                case Gorgon.UI.Alignment.CenterRight:
                    NumericHorizontal.Value = w;
                    NumericVertical.Value = my;
                    break;
                case Gorgon.UI.Alignment.LowerLeft:
                    NumericHorizontal.Value = 0;
                    NumericVertical.Value = h;
                    break;
                case Gorgon.UI.Alignment.LowerCenter:
                    NumericHorizontal.Value = mx;
                    NumericVertical.Value = h;
                    break;
                case Gorgon.UI.Alignment.LowerRight:
                    NumericHorizontal.Value = w;
                    NumericVertical.Value = h;
                    break;
            }
        }

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteAnchorEdit.Bounds):
                    NumericVertical.Minimum = NumericHorizontal.Minimum = 0;
                    NumericHorizontal.Maximum = (decimal)DataContext.Bounds.Width;
                    NumericVertical.Maximum = (decimal)DataContext.Bounds.Height;

                    SetAlignment(DataContext);
                    break;
                case nameof(ISpriteAnchorEdit.AnchorPosition):
                    NumericHorizontal.Value = (decimal)DataContext.AnchorPosition.X;
                    NumericVertical.Value = (decimal)DataContext.AnchorPosition.Y;

                    SetAlignment(DataContext);
                    break;
            }

            ValidateOk();
        }

        /// <summary>
        /// Function to set up the alignment on the alignment control.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void SetAlignment(ISpriteAnchorEdit dataContext)
        {
            Alignment.AlignmentChanged -= Alignment_AlignmentChanged;

            try
            {
                Alignment.Alignment = Gorgon.UI.Alignment.None;

                if (dataContext == null)
                {
                    return;
                }

                float w = dataContext.Bounds.Width;
                float h = dataContext.Bounds.Height;
                float mx = (int)(w * 0.5f);
                float my = (int)(h * 0.5f);

                if ((dataContext.AnchorPosition.X.EqualsEpsilon(0)) && (dataContext.AnchorPosition.Y.EqualsEpsilon(0)))
                {
                    Alignment.Alignment = Gorgon.UI.Alignment.UpperLeft;
                }

                if ((dataContext.AnchorPosition.X.EqualsEpsilon(mx)) && (dataContext.AnchorPosition.Y.EqualsEpsilon(0)))
                {
                    Alignment.Alignment = Gorgon.UI.Alignment.UpperCenter;
                }

                if ((dataContext.AnchorPosition.X.EqualsEpsilon(w)) && (dataContext.AnchorPosition.Y.EqualsEpsilon(0)))
                {
                    Alignment.Alignment = Gorgon.UI.Alignment.UpperRight;
                }

                if ((dataContext.AnchorPosition.X.EqualsEpsilon(0)) && (dataContext.AnchorPosition.Y.EqualsEpsilon(my)))
                {
                    Alignment.Alignment = Gorgon.UI.Alignment.CenterLeft;
                }

                if ((dataContext.AnchorPosition.X.EqualsEpsilon(mx)) && (dataContext.AnchorPosition.Y.EqualsEpsilon(my)))
                {
                    Alignment.Alignment = Gorgon.UI.Alignment.Center;
                }

                if ((dataContext.AnchorPosition.X.EqualsEpsilon(w)) && (dataContext.AnchorPosition.Y.EqualsEpsilon(my)))
                {
                    Alignment.Alignment = Gorgon.UI.Alignment.CenterRight;
                }

                if ((dataContext.AnchorPosition.X.EqualsEpsilon(0)) && (dataContext.AnchorPosition.Y.EqualsEpsilon(h)))
                {
                    Alignment.Alignment = Gorgon.UI.Alignment.LowerLeft;
                }

                if ((dataContext.AnchorPosition.X.EqualsEpsilon(mx)) && (dataContext.AnchorPosition.Y.EqualsEpsilon(h)))
                {
                    Alignment.Alignment = Gorgon.UI.Alignment.LowerCenter;
                }

                if ((dataContext.AnchorPosition.X.EqualsEpsilon(w)) && (dataContext.AnchorPosition.Y.EqualsEpsilon(h)))
                {
                    Alignment.Alignment = Gorgon.UI.Alignment.LowerRight;
                }
            }
            finally
            {
                Alignment.AlignmentChanged += Alignment_AlignmentChanged;
            }
        }

        /// <summary>
        /// Function to unassign any events.
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
        /// Function to reset the control when a data context is removed.
        /// </summary>
        private void ResetDataContext()
        {
            if (DataContext == null)
            {
                return;
            }

            UnassignEvents();
            Alignment.Alignment = Gorgon.UI.Alignment.UpperLeft;
            NumericHorizontal.Value = NumericVertical.Value = 0M;
            NumericVertical.Minimum = NumericHorizontal.Minimum = 0;
            NumericVertical.Maximum = NumericHorizontal.Maximum = int.MaxValue;
        }

        /// <summary>
        /// Function to initialize the control from the data context.
        /// </summary>
        /// <param name="dataContext">The data context being assigned.</param>
        private void InitializeDataContext(ISpriteAnchorEdit dataContext)
        {
            ResetDataContext();

            NumericHorizontal.Value = (decimal)dataContext.AnchorPosition.X;
            NumericVertical.Value = (decimal)dataContext.AnchorPosition.Y;

            NumericHorizontal.Minimum = 0;
            NumericHorizontal.Maximum = (decimal)dataContext.Bounds.Width - 1;

            NumericVertical.Minimum = 0;
            NumericVertical.Maximum = (decimal)dataContext.Bounds.Height - 1;
            
            SetAlignment(dataContext);

            ValidateOk();
        }

        /// <summary>
        /// Function to submit the change.
        /// </summary>
        protected override void OnSubmit()
        {
            base.OnSubmit();

            if ((DataContext?.OkCommand == null) || (!DataContext.OkCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.OkCommand.Execute(null);
        }

        /// <summary>
        /// Function to cancel the change.
        /// </summary>
        protected override void OnCancel()
        {
            base.OnCancel();

            if ((DataContext?.CancelCommand == null) || (!DataContext.CancelCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.CancelCommand.Execute(null);
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (IsDesignTime)
            {
                return;
            }

            DataContext?.OnLoad();
        }

        /// <summary>Function called to validate the OK button.</summary>
        /// <returns>
        ///   <b>true</b> if the OK button is valid, <b>false</b> if not.</returns>
        protected override bool OnValidateOk() => (DataContext?.OkCommand != null) && (DataContext.OkCommand.CanExecute(null));

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(ISpriteAnchorEdit dataContext)
        {
            InitializeDataContext(dataContext);

            DataContext = dataContext;

            if (DataContext == null)
            {
                return;
            }

            DataContext.PropertyChanged += DataContext_PropertyChanged;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteAnchor"/> class.</summary>
        public SpriteAnchor() => InitializeComponent();
        #endregion
    }
}
