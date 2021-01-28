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
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Graphics;
using Gorgon.Math;

namespace Gorgon.UI
{
    /// <summary>
    /// Functionality to display a translucent (like plexi-glass) panel on top of a control or form with a progress meter.
    /// </summary>
    public class GorgonProgressOverlay
    {
        #region Variables.
        // The overlay used to dim the background.
        private readonly Lazy<GorgonOverlay> _overlay = new Lazy<GorgonOverlay>(() => new GorgonOverlay(), true);

        // The form used to display the progress meter.
        private FormProgress _progressForm;

        // The form containing the parent control.
        private WeakReference<Form> _parentForm;

        // The parent control.
        private WeakReference<Control> _parent;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether the overlay is active or not.
        /// </summary>
        public bool IsActive => _progressForm != null;

        /// <summary>
        /// Property to set or return the amount of transparency.
        /// </summary>
        public int TransparencyPercent
        {
            get => _overlay.Value.TransparencyPercent;
            set => _overlay.Value.TransparencyPercent = value;
        }

        /// <summary>
        /// Property to set or return the color of the overlay.
        /// </summary>
        public GorgonColor OverlayColor
        {
            get => _overlay.Value.OverlayColor;
            set => _overlay.Value.OverlayColor = value;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the Resize event of the Parent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void Parent_Layout(object sender, LayoutEventArgs e)
        {
            if ((_progressForm == null) || (_parent == null) || (!IsActive))
            {
                return;
            }

            if ((_progressForm.IsDisposed) || (_progressForm.Disposing))
            {
                return;
            }

            _progressForm.Refresh();
        }

        /// <summary>Handles the Move event of the Parent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void Parent_Move(object sender, EventArgs e)
        {
            if ((_progressForm == null) || (_parent == null) || (!IsActive))
            {
                return;
            }

            if ((_progressForm.IsDisposed) || (_progressForm.Disposing))
            {
                return;
            }

            _progressForm.Refresh();
        }

        /// <summary>Handles the VisibleChanged event of the Parent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void Parent_VisibleChanged(object sender, EventArgs e)
        {
            if ((_progressForm == null) || (_parent == null) || (!_parent.TryGetTarget(out Control parent)) || (!IsActive))
            {
                return;
            }

            if ((_progressForm.IsDisposed) || (_progressForm.Disposing))
            {
                return;
            }

            _progressForm.Visible = parent.Visible;
        }

        /// <summary>Handles the FormClosed event of the ParentForm control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosedEventArgs" /> instance containing the event data.</param>
        private void ParentForm_FormClosed(object sender, FormClosedEventArgs e) => Hide();

        /// <summary>Handles the Resize event of the ParentForm control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ParentForm_Layout(object sender, LayoutEventArgs e)
        {
            if ((_progressForm == null) || (_parentForm == null) || (!_parentForm.TryGetTarget(out Form form)) || (!IsActive))
            {
                return;
            }

            if ((_progressForm.IsDisposed) || (_progressForm.Disposing))
            {
                return;
            }

            switch (form.WindowState)
            {
                case FormWindowState.Minimized:
                    _progressForm.Visible = false;
                    break;
                default:
                    _progressForm.Visible = true;
                    _progressForm.Refresh();
                    break;
            }
        }

        /// <summary>
        /// Function to update the progress percentage, and optionally, the message for the progress bar.
        /// </summary>
        /// <param name="value">The value to assign to the progress meter.</param>
        /// <param name="message">[Optional] The message to display above the progress bar.</param>
        /// <remarks>
        /// <para>
        /// When the <paramref name="value"/> parameter is set to <b>null</b>, then the progress value will not update. This is useful when only the message should be updated.
        /// </para>
        /// <para>
        /// <note type="important">
        /// This method is completely thread safe and can be called within a running thread.
        /// </note>
        /// </para>
        /// </remarks>
        public void UpdateProgress(float? value, string message = null)
        {
            if ((_progressForm == null) || (_progressForm.IsDisposed) || (_progressForm.Disposing))
            {
                return;
            }

            if (value != null)
            {
                _progressForm.Progress.CurrentValue = value.Value.Min(1.0f).Max(0);
            }

            if (message != null)
            {
                _progressForm.Progress.ProgressMessage = message;
            }
        }

        /// <summary>
        /// Function to show the overlay on top of a control/form.
        /// </summary>
        /// <param name="parentWindow">The control or form to use as the parent.</param>
        /// <param name="title">The title for the progress meter.</param>
        /// <param name="message">[Optional] A message to display above the progress bar.</param>
        /// <param name="meterStyle">[Optional] The type of bar to draw in the progress meter.</param>
        /// <param name="cancelAction">[Optional] The action to perform when cancelling.</param>
        /// <param name="initialValue">[Optional] The initial value for the progress meter.</param>
        /// <returns>The handle to the progress panel window.</returns>
        /// <remarks>
        /// <para>
        /// This will only show a single overlay at a time, and multiple overlays are not supported.
        /// </para>
        /// <para>
        /// If the <paramref name="cancelAction"/> parameter is supplied, a cancel button will appear on the progress window, otherwise it will be hidden.
        /// </para>
        /// <para>
        /// Passing <b>null</b> to the <paramref name="parentWindow"/> parameter is the same as calling <see cref="Hide"/>.
        /// </para>
        /// </remarks>
        public IWin32Window Show(IWin32Window parentWindow, string title, string message = null, Action cancelAction = null, ProgressBarStyle meterStyle = ProgressBarStyle.Marquee, float initialValue = 0)
        {
            Hide();

            if (!(parentWindow is Control parent))
            {
                return null;
            }

            void CancelEvent(object sender, EventArgs e) => cancelAction();

            _progressForm = new FormProgress();
            _progressForm.Progress.ProgressTitle = title;
            _progressForm.Progress.AllowCancellation = cancelAction != null;
            _progressForm.Progress.CurrentValue = initialValue.Max(0).Min(1.0f);
            _progressForm.Progress.MeterStyle = meterStyle;
            _progressForm.Progress.ProgressMessage = message;

            if (cancelAction != null)
            {
                _progressForm.Progress.OperationCancelled += CancelEvent;
            }
            
            _progressForm.Show(_overlay.Value.Show(parent));

            _parent = new WeakReference<Control>(parent);

            Form parentForm = parent.FindForm();
            if (parentForm != null)
            {
                _parentForm = new WeakReference<Form>(parentForm);
                parentForm.Move += Parent_Move;
                parentForm.FormClosed += ParentForm_FormClosed;
                parentForm.Layout += ParentForm_Layout;
            }

            parent.VisibleChanged += Parent_VisibleChanged;
            parent.Move += Parent_Move;
            parent.Layout += Parent_Layout;

            return _progressForm;
        }

        /// <summary>
        /// Function to hide an active overlay.
        /// </summary>
        public void Hide()
        {
            FormProgress progressForm = Interlocked.Exchange(ref _progressForm, null);
            WeakReference<Form> formRef = Interlocked.Exchange(ref _parentForm, null);
            WeakReference<Control> controlRef = Interlocked.Exchange(ref _parent, null);
            Control parent = null;
            Form parentForm = null;

            if (controlRef != null)
            {
                controlRef.TryGetTarget(out parent);
            }

            if (formRef != null)
            {
                formRef.TryGetTarget(out parentForm);
            }

            if (parentForm != null)
            {
                parentForm.Layout -= ParentForm_Layout;
                parentForm.FormClosed -= ParentForm_FormClosed;
                parentForm.Move -= Parent_Move;
            }

            if (parent != null)
            {
                parent.Layout -= Parent_Layout;
                parent.Move -= Parent_Move;
                parent.VisibleChanged -= Parent_VisibleChanged;
            }
                        
            progressForm?.Dispose();
            if (_overlay.IsValueCreated)
            {
                _overlay.Value.Hide();
            }           
        }
        #endregion
    }
}
