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
// Created: January 16, 2019 8:16:45 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Gorgon.UI
{
    /// <summary>
    /// A control used to provide a visual means of specifying an anchor setting.
    /// </summary>
    [ToolboxItem(true), Designer(typeof(GorgonAlignmentPickerDesigner))]
    public partial class GorgonAlignmentPicker
        : UserControl
    {
        #region Variables.
        // The current alignment.
        private Alignment _alignment = Alignment.Center;
        #endregion

        #region Events.
        /// <summary>
        /// Event triggered when the alignment value changes.
        /// </summary>
        [Description("Event fired with the alignment property changes."), Category("Alignment")]
        public event EventHandler AlignmentChanged;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the currently selected alignment.
        /// </summary>
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Category("Behavior"), Description("The currently selected alignment."), DefaultValue(typeof(Alignment), nameof(Alignment.Center))]
        public Alignment Alignment
        {
            get => _alignment;
            set
            {
                if (_alignment == value)
                {
                    return;
                }

                _alignment = value;
                CheckButton();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the button state based on the current alignment.
        /// </summary>
        private void CheckButton()
        {
            switch (_alignment)
            {
                case Alignment.CenterLeft:
                    radioMiddleLeft.Checked = true;
                    break;
                case Alignment.CenterRight:
                    radioMiddleRight.Checked = true;
                    break;
                case Alignment.UpperCenter:
                    radioTopCenter.Checked = true;
                    break;
                case Alignment.LowerCenter:
                    radioBottomCenter.Checked = true;
                    break;
                case Alignment.UpperLeft:
                    radioTopLeft.Checked = true;
                    break;
                case Alignment.UpperRight:
                    radioTopRight.Checked = true;
                    break;
                case Alignment.LowerLeft:
                    radioBottomLeft.Checked = true;
                    break;
                case Alignment.LowerRight:
                    radioBottomRight.Checked = true;
                    break;
                case Alignment.Center:
                    radioCenter.Checked = true;
                    break;
                default:
                    radioMiddleLeft.Checked =
                    radioMiddleRight.Checked =
                    radioTopCenter.Checked =
                    radioBottomCenter.Checked =
                    radioTopLeft.Checked =
                    radioTopRight.Checked =
                    radioBottomLeft.Checked =
                    radioBottomRight.Checked =
                    radioCenter.Checked = false;
                    break;
            }
        }

        /// <summary>Handles the Click event of the Radio control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Radio_Click(object sender, EventArgs e)
        {
            var radio = (RadioButton)sender;
            _alignment = (Alignment)radio.Tag;

            EventHandler handler = AlignmentChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Performs the work of setting the specified bounds of this control.</summary>
        /// <param name="x">The new <see cref="P:System.Windows.Forms.Control.Left"/> property value of the control.</param>
        /// <param name="y">The new <see cref="P:System.Windows.Forms.Control.Top"/> property value of the control.</param>
        /// <param name="width">The new <see cref="P:System.Windows.Forms.Control.Width"/> property value of the control.</param>
        /// <param name="height">The new <see cref="P:System.Windows.Forms.Control.Height"/> property value of the control.</param>
        /// <param name="specified">A bitwise combination of the <see cref="System.Windows.Forms.BoundsSpecified"/> values.</param>
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) => base.SetBoundsCore(x, y, 105, 105, specified);
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="GorgonAlignmentPicker"/> class.</summary>
        public GorgonAlignmentPicker()
        {
            InitializeComponent();

            radioMiddleLeft.Tag = Alignment.CenterLeft;
            radioMiddleRight.Tag = Alignment.CenterRight;
            radioTopCenter.Tag = Alignment.UpperCenter;
            radioBottomCenter.Tag = Alignment.LowerCenter;
            radioTopLeft.Tag = Alignment.UpperLeft;
            radioTopRight.Tag = Alignment.UpperRight;
            radioBottomLeft.Tag = Alignment.LowerLeft;
            radioBottomRight.Tag = Alignment.LowerRight;
            radioCenter.Tag = Alignment.Center;
        }
        #endregion
    }

    /// <summary>
    /// A designer for overriding design elements.
    /// </summary>
    internal class GorgonAlignmentPickerDesigner
        : ControlDesigner
    {
        /// <summary>Initializes a new instance of the <see cref="Gorgon.UI.GorgonAlignmentPickerDesigner"/> class.</summary>
        internal GorgonAlignmentPickerDesigner() => AutoResizeHandles = true;

        /// <summary>Gets the selection rules that indicate the movement capabilities of a component.</summary>
        /// <value>The selection rules.</value>
        public override SelectionRules SelectionRules => SelectionRules.None;
    }
}
