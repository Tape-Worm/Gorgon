#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: August 31, 2018 8:17:26 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Math;

namespace Gorgon.UI
{
    // ReSharper disable ValueParameterNotUsed
    /// <summary>
    /// A translucent overlay panel used to indicate that a background operation is on going.
    /// </summary>
    [ToolboxItem(true), ToolboxBitmap(typeof(GorgonOverlayPanel), "Resources.GorgonSelectablePanel.bmp")]
    public class GorgonOverlayPanel
        : UserControl
    {
        #region Variables.
        // The opacity level.
        private double _opacity = 50.0;
        // The color of the overlay.
        private Color _overlayColor = Color.Black;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether or not this control is in the designer.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDesignTime
        {
            get;
        }

        /// <summary>
        /// Property to set or return the level of translucency for the control.
        /// </summary>
        /// <remarks>
        /// This value is a percentage, from 0 to 100.
        /// </remarks>
        [Browsable(true), Category("Appearance"), Description("Sets the level of translucency for the control. The value is a percentage from 0 to 100."),
        DefaultValue(50.0), RefreshProperties(RefreshProperties.Repaint)]
        public double Opacity
        {
            get => _opacity;
            set
            {
                if (_opacity.EqualsEpsilon(value))
                {
                    return;
                }

                _opacity = value.Min(100).Max(0);
                Invalidate();
            }
        }

        /// <summary>
        /// Indicates the border style for the control.
        /// </summary>
        /// <value>The border style.</value>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new BorderStyle BorderStyle
        {
            get => BorderStyle.None;
            // ReSharper disable once ValueParameterNotUsed
            set => base.BorderStyle = BorderStyle.None;
        }

        /// <summary>Gets or sets the foreground color of the control.</summary>
        /// <returns>The foreground <see cref="T:System.Drawing.Color" /> of the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultForeColor" /> property.</returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color ForeColor
        {
            get => Color.Transparent;
            set => base.ForeColor = Color.Transparent;
        }

        /// <summary>Gets or sets the foreground color of the control.</summary>
        /// <returns>The foreground <see cref="T:System.Drawing.Color" /> of the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultForeColor" /> property.</returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                // Intentionally left blank.
            }
        }

        /// <summary>Gets or sets the background color for the control.</summary>
        /// <returns>A <see cref="T:System.Drawing.Color" /> that represents the background color of the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultBackColor" /> property.</returns>
        [Browsable(true), Category("Appearance"),
         Description("Sets the background color for the control."),
         DefaultValue(typeof(Color), "Black"), RefreshProperties(RefreshProperties.Repaint)]
        public Color OverlayColor
        {
            get => _overlayColor;
            set
            {
                if (_overlayColor == value)
                {
                    return;
                }

                _overlayColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets padding within the control.
        /// </summary>
        /// <value>The padding.</value>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Padding Padding
        {
            get => Padding.Empty;
            set => base.Padding = Padding.Empty;
        }

        /// <summary>Gets or sets the size that is the lower limit that <see cref="M:System.Windows.Forms.Control.GetPreferredSize(System.Drawing.Size)" /> can specify.</summary>
        /// <returns>An ordered pair of type <see cref="T:System.Drawing.Size" /> representing the width and height of a rectangle.</returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Size MinimumSize
        {
            get => Size.Empty;
            set => base.MinimumSize = Size.Empty;
        }

        /// <summary>Gets or sets the size that is the upper limit that <see cref="M:System.Windows.Forms.Control.GetPreferredSize(System.Drawing.Size)" /> can specify.</summary>
        /// <returns>An ordered pair of type <see cref="T:System.Drawing.Size" /> representing the width and height of a rectangle.</returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Size MaximumSize
        {
            get => Size.Empty;
            set => base.MaximumSize = Size.Empty;
        }

        /// <summary>Indicates the automatic sizing behavior of the control.</summary>
        /// <returns>One of the <see cref="T:System.Windows.Forms.AutoSizeMode" /> values.</returns>
        /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The specified value when setting this property is not a valid <see cref="T:System.Windows.Forms.AutoSizeMode" /> values.</exception>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override DockStyle Dock
        {
            get => DockStyle.None;
            set => base.Dock = DockStyle.None;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the control resizes based on its contents.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool AutoSize
        {
            get => false;
            set => base.AutoSize = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control causes validation to be performed on any controls that require validation when it receives focus.
        /// </summary>
        /// <value><c>true</c> if [causes validation]; otherwise, <c>false</c>.</value>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool CausesValidation
        {
            get => false;
            set => base.CausesValidation = false;
        }

        /// <summary>
        /// Gets or sets the size of the auto-scroll margin.
        /// </summary>
        /// <value>The automatic scroll margin.</value>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Size AutoScrollMargin
        {
            get => Size.Empty;
            set => base.AutoScrollMargin = Size.Empty;
        }

        /// <summary>
        /// Gets or sets the minimum size of the auto-scroll.
        /// </summary>
        /// <value>The minimum size of the automatic scroll.</value>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Size AutoScrollMinSize
        {
            get => Size.Empty;
            set => base.AutoScrollMargin = Size.Empty;
        }

        /// <summary>Gets or sets a value indicating whether the container enables the user to scroll to any controls placed outside of its visible boundaries.</summary>
        /// <returns>
        /// <see langword="true" /> if the container enables auto-scrolling; otherwise, <see langword="false" />. The default value is <see langword="false" />. </returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool AutoScroll
        {
            get => false;
            set => base.AutoScroll = value;
        }

        /// <summary>Gets or sets the edges of the container to which a control is bound and determines how a control is resized with its parent. </summary>
        /// <returns>A bitwise combination of the <see cref="T:System.Windows.Forms.AnchorStyles" /> values. The default is <see langword="Top" /> and <see langword="Left" />.</returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override AnchorStyles Anchor
        {
            get => base.Anchor;
            set => base.Anchor = value;
        }

        /// <summary>Gets or sets the <see cref="T:System.Windows.Forms.ContextMenuStrip" /> associated with this control.</summary>
        /// <returns>The <see cref="T:System.Windows.Forms.ContextMenuStrip" /> for this control, or <see langword="null" /> if there is no <see cref="T:System.Windows.Forms.ContextMenuStrip" />. The default is <see langword="null" />.</returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ContextMenuStrip ContextMenuStrip
        {
            get => null;
            set => base.ContextMenuStrip = null;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control can respond to user interaction.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool Enabled
        {
            get => true;
            set => base.Enabled = true;
        }

        /// <summary>Gets or sets the font of the text displayed by the control.</summary>
        /// <returns>The <see cref="T:System.Drawing.Font" /> to apply to the text displayed by the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultFont" /> property.</returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Font Font
        {
            get => base.Font;
#pragma warning disable CA2245 // Do not assign a property to itself.
            set => base.Font = base.Font;
#pragma warning restore CA2245 // Do not assign a property to itself.
        }

        /// <summary>Gets or sets a value indicating whether control's elements are aligned to support locales using right-to-left fonts.</summary>
        /// <returns>One of the <see cref="T:System.Windows.Forms.RightToLeft" /> values. The default is <see cref="F:System.Windows.Forms.RightToLeft.Inherit" />.</returns>
        /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The assigned value is not one of the <see cref="T:System.Windows.Forms.RightToLeft" /> values. </exception>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override RightToLeft RightToLeft
        {
            get => RightToLeft.No;
            set => base.RightToLeft = RightToLeft.No;
        }

        /// <summary>Gets or sets a value indicating whether the control can accept data that the user drags onto it.</summary>
        /// <returns>
        /// <see langword="true" /> if drag-and-drop operations are allowed in the control; otherwise, <see langword="false" />. The default is <see langword="false" />.</returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool AllowDrop
        {
            get => false;
            set => base.AllowDrop = false;
        }

        /// <summary>Gets or sets the background image layout as defined in the <see cref="T:System.Windows.Forms.ImageLayout" /> enumeration.</summary>
        /// <returns>One of the values of <see cref="T:System.Windows.Forms.ImageLayout" /> (<see cref="F:System.Windows.Forms.ImageLayout.Center" /> , <see cref="F:System.Windows.Forms.ImageLayout.None" />, <see cref="F:System.Windows.Forms.ImageLayout.Stretch" />, <see cref="F:System.Windows.Forms.ImageLayout.Tile" />, or <see cref="F:System.Windows.Forms.ImageLayout.Zoom" />). <see cref="F:System.Windows.Forms.ImageLayout.Tile" /> is the default value.</returns>
        /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The specified enumeration value does not exist. </exception>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ImageLayout BackgroundImageLayout
        {
            get => ImageLayout.None;
            set => base.BackgroundImageLayout = ImageLayout.None;
        }

        /// <summary>Gets or sets the background image displayed in the control.</summary>
        /// <returns>An <see cref="T:System.Drawing.Image" /> that represents the image to display in the background of the control.</returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Image BackgroundImage
        {
            get => null;
            set => base.BackgroundImage = null;
        }

        /// <summary>
        /// Property to set or return whether or not the overlay will take focus away or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default, the overlay will take focus away from other controls on the same form so that the UI is locked down while the overlay is active. In most cases, this is the desired behavior. But, 
        /// for cases where a control is sitting on top of the overlay, and the control requires focus, this may be disruptive. To counter this, setting this value to <b>false</b> will ensure that it does 
        /// not take focus from the other controls.
        /// </para>
        /// </remarks>
        [Browsable(true), Category("Behavior"), Description("Sets whether or the overlay will take focus from other controls."), DefaultValue(true)]
        public bool AllowStealFocus
        {
            get;
            set;
        } = true;
        #endregion

        #region Methods.
        /// <summary>
        /// Initializes the component.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            OverlayColor = Color.Black;
            BorderStyle = BorderStyle.None;
            TabStop = true;
            Dock = DockStyle.None;
            Anchor = AnchorStyles.None;

            ResumeLayout(false);
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.GotFocus"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            if (AllowStealFocus)
            {
                return;
            }

            // If we can't steal focus, then we shouldn't accept focus either, so move to the next control.
            int thisIndex = Parent.Controls.GetChildIndex(this);

            // We're at the top, so nothing to do.
            if (thisIndex < 1)
            {
                return;
            }

            --thisIndex;

            for (int i = thisIndex; i >= 0; --i)
            {
                if (!Parent.Controls[i].CanSelect)
                {
                    if (Parent.Controls[i].SelectNextControl(Parent.Controls[i], true, true, true, true))
                    {
                        return;
                    }
                    continue;
                }

                Parent.Controls[i]?.Select();
            }
        }

        /// <summary>Processes a command key.</summary>
        /// <param name="msg">A <see cref="T:System.Windows.Forms.Message" />, passed by reference, that represents the window message to process. </param>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys" /> values that represents the key to process. </param>
        /// <returns>
        /// <see langword="true" /> if the character was processed by the control; otherwise, <see langword="false" />.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) =>
            // Attempt to mitigate Tab switching while this control is live.
            ((keyData & Keys.Tab) == Keys.Tab) && (AllowStealFocus) || base.ProcessCmdKey(ref msg, keyData);

        /// <summary>
        /// Function called after the siblings for this control are rendered.
        /// </summary>
        /// <param name="e">The paint event arguments.</param>
        protected virtual void OnOverlayPainted(PaintEventArgs e)
        {

        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.ParentChanged" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if ((Parent == null) || (IsDesignTime))
            {
                return;
            }

            Location = new Point(0, 0);
            Size = Parent.ClientSize;

            // This check needs to be done because forms with a Krypton Ribbon have their dock altered to always keep the ribbon exposed even when the panel is a sibling with a lower Z index.
            // This gets around that. Otherwise we'll have a ribbon sitting on top of our panel, and that defeats the purpose of the panel.
            if (Parent is Form)
            {
                base.Dock = DockStyle.None;
                base.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            }
            else
            {
                base.Anchor = AnchorStyles.None;
                base.Dock = DockStyle.Fill;
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data. </param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Form parentForm = FindForm();

            if ((Parent == null) || ((!IsDesignTime) && (parentForm == null)))
            {
                base.OnPaint(e);
                return;
            }

            int zIndex = Parent.Controls.GetChildIndex(this);

            if ((parentForm != null) && (AllowStealFocus))
            {
                parentForm.ActiveControl = this;
            }

            using (var backingImage = new Bitmap(Parent.Width, Parent.Height))
            {
                IEnumerable<Control> siblings = (from sibling in Parent.Controls.Cast<Control>()
                                                 where (sibling.Enabled) && (sibling.Visible)
                                                 let siblingZ = Parent.Controls.GetChildIndex(sibling)
                                                 where (siblingZ > zIndex) && (sibling.Bounds.IntersectsWith(Bounds))
                                                 orderby siblingZ descending
                                                 select sibling);

                foreach (Control control in siblings)
                {
                    control.Refresh();
                    control.DrawToBitmap(backingImage, control.Bounds);
                }

                int alphaValue = (int)(_opacity / 100.0 * 255.0);

                e.Graphics.DrawImage(backingImage, -Left, -Top);
                using (var alphaBrush = new SolidBrush(Color.FromArgb(alphaValue, _overlayColor)))
                {
                    e.Graphics.FillRectangle(alphaBrush, ClientRectangle);
                }

                // Function called when the siblings for this control are painted.
                OnOverlayPainted(e);
            }
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonOverlayPanel"/> class.
        /// </summary>
        public GorgonOverlayPanel()
        {
            IsDesignTime = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ContainerControl, false);
            InitializeComponent();
        }
        #endregion
        // ReSharper restore ValueParameterNotUsed
    }
}
