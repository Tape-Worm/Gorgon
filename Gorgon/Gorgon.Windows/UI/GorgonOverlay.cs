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
// Created: December 5, 2020 3:41:43 PM
// 
#endregion

using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Collections;
using Gorgon.Graphics;
using Gorgon.Math;

namespace Gorgon.UI
{
    /// <summary>
    /// Functionality to display a translucent (like plexi-glass) panel on top of a control or form.
    /// </summary>
    public class GorgonOverlay
    {
        #region Variables.
        // The form used to display the overlay.
        private FormOverlay _overlayForm;        

        // The parent control for the overlay.
        private WeakReference<Control> _parent;

        // The form containing the parent control.
        private WeakReference<Form> _parentForm;

        // The amount of transparency.
        private int _transparencyPercent = 50;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether the overlay is active or not.
        /// </summary>
        public bool IsActive => _overlayForm != null;

        /// <summary>
        /// Property to set or return the amount of transparency.
        /// </summary>
        public int TransparencyPercent
        {
            get => _transparencyPercent;
            set => _transparencyPercent = value.Min(100).Max(1);
        }

        /// <summary>
        /// Property to set or return the color of the overlay.
        /// </summary>
        public GorgonColor OverlayColor
        {
            get;
            set;
        } = GorgonColor.Black;
        #endregion

        #region Methods.
        /// <summary>Handles the GotFocus event of the Parent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void Parent_GotFocus(object sender, EventArgs e)
        {
            if ((_overlayForm is null)
                || (_parentForm is null)
                || (_parent is null)
                || (!_parentForm.TryGetTarget(out Form parentForm))
                || (!_parent.TryGetTarget(out Control parent)))
            {
                return;
            }

            if (parentForm.ActiveControl is null)
            {
                return;
            }

            if (parent == parentForm)
            {
                parentForm.ActiveControl = null;
                return;
            }

            // For controls that are overlaid, we need to skip them when moving focus. This helps defeat using the keyboard (via Tab) to set control focus.
            parentForm.SelectNextControl(parentForm, (Control.ModifierKeys & Keys.Shift) != Keys.Shift, true, true, true);
        }

        /// <summary>Handles the HandleDestroyed event of the Parent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void Parent_HandleDestroyed(object sender, EventArgs e) => Hide();

        /// <summary>Handles the FormClosed event of the ParentForm control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosedEventArgs" /> instance containing the event data.</param>
        private void ParentForm_FormClosed(object sender, FormClosedEventArgs e) => Hide();

        /// <summary>Handles the Resize event of the ParentForm control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ParentForm_Layout(object sender, LayoutEventArgs e)
        {
            if ((_overlayForm is null) || (_parentForm is null) || (!_parentForm.TryGetTarget(out Form form)))
            {
                return;
            }

            if ((_overlayForm.Disposing) || (_overlayForm.IsDisposed))
            {
                return;
            }

            switch (form.WindowState)
            {
                case FormWindowState.Minimized:
                    _overlayForm.Visible = false;
                    break;
                default:
                    _overlayForm.Visible = true;
                    break;
            }
        }

        /// <summary>Handles the EnabledChanged event of the Parent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void Parent_EnabledChanged(object sender, EventArgs e)
        {
            if ((_overlayForm is null) || (_parent is null) || (!_parent.TryGetTarget(out Control parent)))
            {
                return;
            }

            if ((_overlayForm.Disposing) || (_overlayForm.IsDisposed))
            {
                return;
            }

            _overlayForm.Enabled = parent.Enabled;
        }

        /// <summary>Handles the VisibleChanged event of the Parent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void Parent_VisibleChanged(object sender, EventArgs e)
        {
            if ((_overlayForm is null) || (_parent is null) || (!_parent.TryGetTarget(out Control parent)))
            {
                return;
            }

            if ((_overlayForm.Disposing) || (_overlayForm.IsDisposed))
            {
                return;
            }

            _overlayForm.Visible = parent.Visible;
        }

        /// <summary>Handles the Resize event of the Parent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void Parent_Layout(object sender, LayoutEventArgs e)
        {
            if ((_overlayForm is null) || (_parent is null) || (!_parent.TryGetTarget(out Control parent)))
            {
                return;
            }

            if ((_overlayForm.Disposing) || (_overlayForm.IsDisposed))
            {
                return;
            }

            _overlayForm.Location = parent.PointToScreen(new Point(0, 0));
            _overlayForm.Size = parent.ClientSize;
        }

        /// <summary>Handles the Move event of the ParentForm control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ParentForm_Move(object sender, EventArgs e)
        {
            if ((_overlayForm is null) || (_parent is null) || (!_parent.TryGetTarget(out Control parent)))
            {
                return;
            }

            if ((_overlayForm.Disposing) || (_overlayForm.IsDisposed))
            {
                return;
            }

            _overlayForm.Location = parent.PointToScreen(new Point(0, 0));
        }

        /// <summary>Handles the Activated event of the OverlayForm control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void OverlayForm_Activated(object sender, EventArgs e)
        {
            if ((_parentForm is null) || (_parent is null) || (!_parentForm.TryGetTarget(out Form form)))
            {
                return;
            }

            form.Activate();
        }

        /// <summary>
        /// Function to show the overlay on top of a control/form.
        /// </summary>
        /// <param name="parentWindow">The control or form to use as the parent.</param>
        /// <returns>The handle for the overlay.</returns>
        /// <remarks>
        /// <para>
        /// This will only show a single overlay at a time, and multiple overlays are not supported.
        /// </para>
        /// <para>
        /// Passing <b>null</b> to the <paramref name="parentWindow"/> parameter is the same as calling <see cref="Hide"/>.
        /// </para>
        /// </remarks>
        public IWin32Window Show(IWin32Window parentWindow)
        {
            Hide();

            if (!(parentWindow is Control parent))
            {
                return null;
            }

            _overlayForm = new FormOverlay
            {
                Location = parent.PointToScreen(new Point(0, 0)),
                Size = parent.ClientSize,
                BackColor = OverlayColor
            };

            _parent = new WeakReference<Control>(parent);
            Form parentForm = parent.FindForm();

            if (parentForm != null)
            {
                _parentForm = new WeakReference<Form>(parentForm);
            }

            _overlayForm.Activated += OverlayForm_Activated;

            parent.Move += ParentForm_Move;
            parent.Layout += Parent_Layout;
            parent.VisibleChanged += Parent_VisibleChanged;
            parent.EnabledChanged += Parent_EnabledChanged;
            parent.HandleDestroyed += Parent_HandleDestroyed;
            parent.GotFocus += Parent_GotFocus;

            foreach (Control child in parent.Controls.OfType<Control>().Traverse(c => c.Controls.OfType<Control>()))
            {
                child.GotFocus += Parent_GotFocus;
            }

            if ((parentForm != null) && (parentForm != parent))
            {
                parentForm.FormClosed += ParentForm_FormClosed;
                parentForm.Move += ParentForm_Move;
                parentForm.Layout += ParentForm_Layout;
            }
                                    
            _overlayForm.Show(parentForm ?? parent);
            _overlayForm.Opacity = TransparencyPercent / 100.0;

            return _overlayForm;
        }

        /// <summary>
        /// Function to hide an active overlay.
        /// </summary>
        public void Hide()
        {
            FormOverlay overlayForm = Interlocked.Exchange(ref _overlayForm, null);
            WeakReference<Form> formRef = Interlocked.Exchange(ref _parentForm, null);
            WeakReference<Control> controlRef = Interlocked.Exchange(ref _parent, null);
            Form parentForm = null;
            Control parent = null;

            if (formRef != null)
            {
                formRef.TryGetTarget(out parentForm);
            }

            if (controlRef != null)
            {
                controlRef.TryGetTarget(out parent);
            }

            if (overlayForm != null)
            {
                overlayForm.Activated -= OverlayForm_Activated;
            }

            if (parentForm != null)
            {
                parentForm.Layout -= ParentForm_Layout;
                parentForm.Move -= ParentForm_Move;
                parentForm.FormClosed -= ParentForm_FormClosed;
            }

            if (parent != null)
            {
                foreach (Control child in parent.Controls.OfType<Control>().Traverse(c => c.Controls.OfType<Control>()))
                {
                    child.GotFocus -= Parent_GotFocus;
                }

                parent.GotFocus -= Parent_GotFocus;
                parent.HandleDestroyed -= Parent_HandleDestroyed;
                parent.EnabledChanged -= Parent_EnabledChanged;
                parent.VisibleChanged -= Parent_VisibleChanged;
                parent.Layout -= Parent_Layout;
                parent.Move -= ParentForm_Move;
            }

            overlayForm?.Dispose();
        }
        #endregion
    }
}
