// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: March 12, 2025 12:18:20 AM
//

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Gorgon.UI.WindowsForms;

/// <summary>
/// The internal form used for the dialog.
/// </summary>
internal partial class FormDialog
    : Form
{
    // The dialog button visbility state.
    // The details text.

    /// <summary>
    /// Property to set or return the message to place in the message label.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Message
    {
        get => LabelMessage.Text;
        set => LabelMessage.Text = value;
    }

    /// <summary>
    /// Property to set or return the detail text for the detail area.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Details
    {
        get;
        set
        {
            field = value;
            TextDetails.Text = field.Replace("\n", Environment.NewLine);
        }
    } = string.Empty;

    /// <summary>
    /// Property to set or return whether to show the OK button.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FormDialogButtons Buttons
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;

            if (IsHandleCreated)
            {
                UpdateButtonVisibility();
            }
        }
    }

    /// <summary>
    /// Property to return whether the 'To All' button was checked.
    /// </summary>
    public bool ToAll => CheckToAll.Checked;

    /// <summary>
    /// Property to set or return the icon for the dialog.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image? DialogIcon
    {
        get => PictureIcon.Image;
        set => PictureIcon.Image = value;
    }

    /// <summary>
    /// Function to update the button visibility state.
    /// </summary>
    private void UpdateButtonVisibility()
    {
        if (Buttons == FormDialogButtons.None)
        {
            ButtonOK.Visible = ButtonYes.Visible = ButtonNo.Visible = ButtonCancel.Visible = CheckDetail.Visible = CheckToAll.Visible = PanelDetails.Visible = false;
            return;
        }

        PanelDetails.Visible = false;

        ButtonOK.Visible = (Buttons & FormDialogButtons.OK) == FormDialogButtons.OK;
        ButtonCancel.Visible = (Buttons & FormDialogButtons.Cancel) == FormDialogButtons.Cancel;
        ButtonYes.Visible = (Buttons & FormDialogButtons.Yes) == FormDialogButtons.Yes;
        ButtonNo.Visible = (Buttons & FormDialogButtons.No) == FormDialogButtons.No;
        CheckDetail.Visible = (Buttons & FormDialogButtons.Details) == FormDialogButtons.Details;
        CheckToAll.Visible = (Buttons & FormDialogButtons.ToAll) == FormDialogButtons.ToAll;
    }

    /// <inheritdoc/>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        UpdateButtonVisibility();

        Screen current = Screen.FromControl(this);

        MaximumSize = new Size(current.WorkingArea.Width / 2, current.WorkingArea.Height - 256);

        int maxWidth = MaximumSize.Width - (LabelMessage.Left + LabelMessage.Margin.Left + LabelMessage.Margin.Right + (SystemInformation.Border3DSize.Width * 2));
        int maxHeight = MaximumSize.Height - (LabelMessage.Margin.Bottom + LabelMessage.Margin.Top + LabelMessage.Top + ButtonOK.Top + ButtonOK.Margin.Top + (SystemInformation.Border3DSize.Height * 2) + SystemInformation.CaptionHeight);

        LabelMessage.MaximumSize = new Size(maxWidth, maxHeight);

        if (Owner is not null)
        {
            CenterToParent();
        }
        else
        {
            CenterToScreen();
        }
    }

    /// <summary>
    /// Initializes a new instance of the windows form.
    /// </summary>
    public FormDialog() => InitializeComponent();

    /// <summary>
    /// Function called when the details check button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void CheckDetail_Click(object sender, EventArgs e) => PanelDetails.Visible = CheckDetail.Checked;
}

/// <summary>
/// A list of buttons to enable or disable.
/// </summary>
[Flags]
internal enum FormDialogButtons
{
    None = 0,
    OK = 1,
    Cancel = 2,
    Yes = 4,
    No = 8,
    Details = 16,
    ToAll = 32
}
