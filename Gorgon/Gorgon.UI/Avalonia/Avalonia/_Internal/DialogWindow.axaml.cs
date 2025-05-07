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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: April 6, 2025 6:46:15 PM
//

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;

namespace Gorgon.UI.Avalonia;

/// <summary>
/// </summary>
internal partial class DialogWindow
    : Window
{
    // The dialog button visbility state.
    private WindowDialogButtons _buttonState;
    // The details text.
    private string _details = string.Empty;
    // The confirmation result when clicking on a button.
    private ConfirmationResult _confirmResult = ConfirmationResult.None;

    /// <summary>
    /// Property to set or return the message to place in the message label.
    /// </summary>
    public string Message
    {
        get => LabelMessage.Text ?? string.Empty;
        set => LabelMessage.Text = value;
    }

    /// <summary>
    /// Property to set or return the detail text for the detail area.
    /// </summary>
    public string Details
    {
        get => _details;
        set
        {
            _details = value;
            TextDetails.Text = _details;//.Replace("\n", Environment.NewLine);
        }
    }

    /// <summary>
    /// Property to set or return whether to show the OK button.
    /// </summary>
    public WindowDialogButtons Buttons
    {
        get => _buttonState;
        set
        {
            if (_buttonState == value)
            {
                return;
            }

            _buttonState = value;
            UpdateButtonVisibility();
        }
    }

    /// <summary>
    /// Property to return whether the 'To All' button was checked.
    /// </summary>
    public bool ToAll => CheckToAll.IsChecked ?? false;

    /// <summary>
    /// Property to set or return the icon for the dialog.
    /// </summary>
    public IImage? DialogIcon
    {
        get => ImageIcon.Source;
        set => ImageIcon.Source = value;
    }

    /// <summary>
    /// Property to return the result from a confirmation dialog.
    /// </summary>
    public ConfirmationResult ConfirmationResult => _confirmResult;

    /// <summary>
    /// Function called when the details button is checked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void CheckDetail_Click(object sender, RoutedEventArgs e) => PanelDetails.IsVisible = (CheckDetail.IsChecked ?? false);

    /// <summary>
    /// Function to update the button visibility state.
    /// </summary>
    private void UpdateButtonVisibility()
    {
        if (_buttonState == WindowDialogButtons.None)
        {
            ButtonOK.IsVisible = ButtonYes.IsVisible = ButtonNo.IsVisible = ButtonCancel.IsVisible = CheckDetail.IsVisible = CheckToAll.IsVisible = PanelDetails.IsVisible = false;
            return;
        }

        PanelDetails.IsVisible = false;

        ButtonOK.IsVisible = (_buttonState & WindowDialogButtons.OK) == WindowDialogButtons.OK;
        ButtonCancel.IsVisible = (_buttonState & WindowDialogButtons.Cancel) == WindowDialogButtons.Cancel;
        ButtonYes.IsVisible = (_buttonState & WindowDialogButtons.Yes) == WindowDialogButtons.Yes;
        ButtonNo.IsVisible = (_buttonState & WindowDialogButtons.No) == WindowDialogButtons.No;
        CheckDetail.IsVisible = (_buttonState & WindowDialogButtons.Details) == WindowDialogButtons.Details;
        CheckToAll.IsVisible = (_buttonState & WindowDialogButtons.ToAll) == WindowDialogButtons.ToAll;
    }

    /// <summary>
    /// Function called when the OK button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void ButtonOK_Click(object sender, RoutedEventArgs e) => Close(true);

    /// <summary>
    /// Function called when the Cancel button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
        _confirmResult = ConfirmationResult.Cancel;
        Close(false);
    }

    /// <summary>
    /// Function called when the Yes button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void ButtonYes_Click(object sender, RoutedEventArgs e)
    {
        _confirmResult = ConfirmationResult.Yes;

        if (CheckToAll.IsChecked ?? false)
        {
            _confirmResult |= ConfirmationResult.ToAll;
        }
        Close(true);
    }

    /// <summary>
    /// Function called when the No button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void ButtonNo_Click(object sender, RoutedEventArgs e)
    {
        _confirmResult = ConfirmationResult.No;

        if (CheckToAll.IsChecked ?? false)
        {
            _confirmResult |= ConfirmationResult.ToAll;
        }
        Close(true);
    }

    /// <summary>
    /// Function called when the window is loaded.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Screen? currentScreen = Screens.ScreenFromVisual(this);

        if (currentScreen is null)
        {
            return;
        }

        WindowStartupLocation = WindowStartupLocation.Manual;
        MaxWidth = (currentScreen.WorkingArea.Width / 2.0) / RenderScaling;
        MaxHeight = (currentScreen.WorkingArea.Height / 2.0) / RenderScaling;

        UpdateLayout();

        double xPos = (MaxWidth - (Width / 2));
        double yPos = (MaxHeight - (Height / 2));

        UpdateButtonVisibility();

        Position = new PixelPoint((int)(xPos * RenderScaling), (int)(yPos * RenderScaling));
    }

    /// <summary>Initializes a new instance of the <see cref="DialogWindow" /> class.</summary>
    public DialogWindow() => InitializeComponent();
}

/// <summary>
/// A list of buttons to enable or disable.
/// </summary>
[Flags]
internal enum WindowDialogButtons
{
    None = 0,
    OK = 1,
    Cancel = 2,
    Yes = 4,
    No = 8,
    Details = 16,
    ToAll = 32
}