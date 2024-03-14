
// 
// Gorgon
// Copyright (C) 2014 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Wednesday, February 26, 2014 6:19:08 PM
// 


namespace Gorgon.UI;

/// <summary>
/// Dialog for warning messages
/// </summary>
internal partial class WarningDialog
{

    private int _lastWidth;                 // Last used width.
    private string _warningDetails;         // Error details.
    private int _detailHeight;              // Height for details panel.



    /// <summary>
    /// Property to set or return whether to open the detail panel when the dialog is shown.
    /// </summary>
    public bool ShowDetailPanel
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return or set the additional details for the warning.
    /// </summary>
    public string WarningDetails
    {
        get => _warningDetails;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }

            _warningDetails = value;

            // Fix up line endings.				
            textWarningDetails.Text = value.Replace("\n", Environment.NewLine);

            ValidateFunctions();
        }
    }



    /// <summary>
    /// Handles the Click event of the detailsButton control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void DetailsButton_Click(object sender, EventArgs e)
    {
        if (checkDetail.Checked)
        {
            _lastWidth = Width;
            Height += _detailHeight;
            Width = MessageWidth + ((Width - ClientSize.Width) * 2);
            textWarningDetails.Visible = true;
        }
        else
        {
            textWarningDetails.Visible = false;
            Width = _lastWidth;
            Height -= _detailHeight;
        }

        Refresh();
        Invalidate();
    }

    /// <summary>
    /// OK button click event.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void OKButton_Click(object sender, EventArgs e) => Close();

    /// <summary>
    /// Function to validate the buttons.
    /// </summary>
    protected override void ValidateFunctions() => checkDetail.Enabled = !string.IsNullOrWhiteSpace(_warningDetails);

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs"></see> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        _detailHeight = textWarningDetails.Height + 10;
        MessageHeight -= _detailHeight;

        _lastWidth = Width;
        buttonOK.Focus();
        ValidateFunctions();

        if (!ShowDetailPanel)
        {
            return;
        }

        checkDetail.Checked = ShowDetailPanel;
        DetailsButton_Click(this, EventArgs.Empty);
    }

    /// <summary>
    /// Function to perform the actual drawing of the message.
    /// </summary>
    /// <param name="g">Graphics object to use.</param>
    protected override void DrawDialog(System.Drawing.Graphics g)
    {
        // Get size.
        float maxTextHeight = AdjustSize(g, 0);

        // Relocate buttons.
        buttonOK.Left = ClientSize.Width - buttonOK.Width - checkDetail.Left;

        // Adjust for detail window.
        if (checkDetail.Checked)
        {
            checkDetail.Top = textWarningDetails.Top - 6 - checkDetail.Height;
            buttonOK.Top = textWarningDetails.Top - 6 - buttonOK.Height;
        }
        else
        {
            checkDetail.Top = ClientSize.Height - 6 - checkDetail.Height;
            buttonOK.Top = ClientSize.Height - 6 - buttonOK.Height;
        }

        // Adjust the position of the details box.
        textWarningDetails.Top = ClientSize.Height - 8 - textWarningDetails.Height;
        textWarningDetails.Width = ClientSize.Width - (textWarningDetails.Left * 2);

        DrawMessage(g, maxTextHeight);
    }



    /// <summary>
    /// Constructor.
    /// </summary>
    public WarningDialog()
    {
        // Initial height = 134.
        // Expanded height = 334.
        InitializeComponent();
        ShowDetailPanel = false;
        _warningDetails = "";
    }

}
