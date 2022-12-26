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
using System.Threading;
using System.Windows.Forms;
using Gorgon.Graphics;

namespace Gorgon.UI;

/// <summary>
/// Functionality to display a translucent (like plexi-glass) panel on top of a control or form with a message and spinning icon to indicate that the system is busy.
/// </summary>
public class GorgonWaitOverlay
{
    #region Variables.
    // The overlay used to dim the background.
    private readonly Lazy<GorgonOverlay> _overlay = new(() => new GorgonOverlay(), true);

    // The form used to display the progress meter.
    private FormWait _waitForm;

    // The form containing the parent control.
    private WeakReference<Form> _parentForm;

    // The parent control.
    private WeakReference<Control> _parent;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return whether the overlay is active or not.
    /// </summary>
    public bool IsActive => _waitForm is not null;

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
        if ((_waitForm is null) || (_parent is null) || (!IsActive))
        {
            return;
        }

        if ((_waitForm.IsDisposed) || (_waitForm.Disposing))
        {
            return;
        }

        _waitForm.Refresh();
    }

    /// <summary>Handles the Move event of the Parent control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void Parent_Move(object sender, EventArgs e)
    {
        if ((_waitForm is null) || (_parent is null) || (!IsActive))
        {
            return;
        }

        if ((_waitForm.IsDisposed) || (_waitForm.Disposing))
        {
            return;
        }

        _waitForm.Refresh();
    }

    /// <summary>Handles the VisibleChanged event of the Parent control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void Parent_VisibleChanged(object sender, EventArgs e)
    {
        if ((_waitForm is null) || (_parent is null) || (!_parent.TryGetTarget(out Control parent)) || (!IsActive))
        {
            return;
        }

        if ((_waitForm.IsDisposed) || (_waitForm.Disposing))
        {
            return;
        }

        _waitForm.Visible = parent.Visible;
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
        if ((_waitForm is null) || (_parentForm is null) || (!_parentForm.TryGetTarget(out Form form)) || (!IsActive))
        {
            return;
        }

        if ((_waitForm.IsDisposed) || (_waitForm.Disposing))
        {
            return;
        }

        switch (form.WindowState)
        {
            case FormWindowState.Minimized:
                _waitForm.Visible = false;
                break;
            default:                    
                _waitForm.Visible = true;
                _waitForm.Refresh();
                break;
        }
    }

    /// <summary>
    /// Function to show the overlay on top of a control/form.
    /// </summary>
    /// <param name="parentWindow">The control or form to use as the parent.</param>
    /// <param name="title">[Optional] The title for the progress meter.</param>
    /// <param name="message">[Optional] A message to display above the progress bar.</param>
    /// <param name="image">[Optional] A custom image to display on the wait panel control.</param>
    /// <returns>The handle to the wait panel window.</returns>
    /// <remarks>
    /// <para>
    /// This will only show a single overlay at a time, and multiple overlays are not supported.
    /// </para>
    /// <para>
    /// When passing a <paramref name="image"/>, ensure that it is sized to 48x48, otherwise it may cause the panel to look incorrect.
    /// </para>
    /// <para>
    /// Passing <b>null</b> to the <paramref name="parentWindow"/> parameter is the same as calling <see cref="Hide"/>.
    /// </para>
    /// </remarks>
    public IWin32Window Show(IWin32Window parentWindow, string title = null, string message = null, Image image = null)
    {
        Hide();

        if (parentWindow is not Control parent)
        {
            return null;
        }

        _waitForm = new FormWait();
        if (title is not null)
        {
            _waitForm.Wait.WaitTitle = title;
        }

        if (message is not null)
        {
            _waitForm.Wait.WaitMessage = message;                    
        }

        if (image is not null)
        {
            _waitForm.Wait.WaitIcon = image;
        }

        _waitForm.Show(_overlay.Value.Show(parent));

        _parent = new WeakReference<Control>(parent);

        Form parentForm = parent.FindForm();
        if (parentForm is not null)
        {
            _parentForm = new WeakReference<Form>(parentForm);
            parentForm.Move += Parent_Move;
            parentForm.FormClosed += ParentForm_FormClosed;
            parentForm.Layout += ParentForm_Layout;
        }

        parent.VisibleChanged += Parent_VisibleChanged;
        parent.Move += Parent_Move;
        parent.Layout += Parent_Layout;

        return _waitForm;
    }

    /// <summary>
    /// Function to hide an active overlay.
    /// </summary>
    public void Hide()
    {
        FormWait waitForm = Interlocked.Exchange(ref _waitForm, null);
        WeakReference<Form> formRef = Interlocked.Exchange(ref _parentForm, null);
        WeakReference<Control> controlRef = Interlocked.Exchange(ref _parent, null);
        Control parent = null;
        Form parentForm = null;

        if (controlRef is not null)
        {
            controlRef.TryGetTarget(out parent);
        }

        if (formRef is not null)
        {
            formRef.TryGetTarget(out parentForm);
        }

        if (parentForm is not null)
        {
            parentForm.Layout -= ParentForm_Layout;
            parentForm.FormClosed -= ParentForm_FormClosed;
            parentForm.Move -= Parent_Move;   
        }

        if (parent is not null)
        {
            parent.Layout -= Parent_Layout;
            parent.Move -= Parent_Move;
            parent.VisibleChanged -= Parent_VisibleChanged;
        }
                    
        waitForm?.Dispose();

        if (_overlay.IsValueCreated)
        {
            _overlay.Value.Hide();
        }            
    }
    #endregion
}
