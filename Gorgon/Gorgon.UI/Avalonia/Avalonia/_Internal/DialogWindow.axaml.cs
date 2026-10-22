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
// Created: April 6, 2025 6:46:15 PM
//

using System;
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
    // The details text.

    // The confirmation result when clicking on a button.

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
        get;
        set
        {
            field = value;
            TextDetails.Text = field;//.Replace("\n", Environment.NewLine);
        }
    } = string.Empty;

    /// <summary>
    /// Property to set or return whether to show the OK button.
    /// </summary>
    public WindowDialogButtons Buttons
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
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
    public ConfirmationResult ConfirmationResult { get; private set; } = ConfirmationResult.None;

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
        if (Buttons == WindowDialogButtons.None)
        {
            ButtonOK.IsVisible = ButtonYes.IsVisible = ButtonNo.IsVisible = ButtonCancel.IsVisible = CheckDetail.IsVisible = CheckToAll.IsVisible = PanelDetails.IsVisible = false;
            return;
        }

        PanelDetails.IsVisible = false;

        ButtonOK.IsVisible = (Buttons & WindowDialogButtons.OK) == WindowDialogButtons.OK;
        ButtonCancel.IsVisible = (Buttons & WindowDialogButtons.Cancel) == WindowDialogButtons.Cancel;
        ButtonYes.IsVisible = (Buttons & WindowDialogButtons.Yes) == WindowDialogButtons.Yes;
        ButtonNo.IsVisible = (Buttons & WindowDialogButtons.No) == WindowDialogButtons.No;
        CheckDetail.IsVisible = (Buttons & WindowDialogButtons.Details) == WindowDialogButtons.Details;
        CheckToAll.IsVisible = (Buttons & WindowDialogButtons.ToAll) == WindowDialogButtons.ToAll;
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
        ConfirmationResult = ConfirmationResult.Cancel;
        Close(false);
    }

    /// <summary>
    /// Function called when the Yes button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void ButtonYes_Click(object sender, RoutedEventArgs e)
    {
        ConfirmationResult = ConfirmationResult.Yes;

        if (CheckToAll.IsChecked ?? false)
        {
            ConfirmationResult |= ConfirmationResult.ToAll;
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
        ConfirmationResult = ConfirmationResult.No;

        if (CheckToAll.IsChecked ?? false)
        {
            ConfirmationResult |= ConfirmationResult.ToAll;
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