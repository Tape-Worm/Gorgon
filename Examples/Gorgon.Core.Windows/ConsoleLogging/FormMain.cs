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
// Created: May 5, 2025 11:24:57 PM
//

using Gorgon.Core;
using Gorgon.Diagnostics;

namespace Gorgon.Examples;

/// <summary>
/// The primary interface for the example.
/// </summary>
public partial class FormMain : Form
{
    /// <summary>
    /// Property to set or return the active log interface.
    /// </summary>
    public IGorgonLog Log
    {
        get;
        set;
    } = GorgonLog.NullLog;

    /// <summary>
    /// An exception for our sample.
    /// </summary>
    private void Exception3() => throw new NullReferenceException("This is our root exception.");

    /// <summary>
    /// An exception for our sample.
    /// </summary>
    private void Exception2()
    {
        try
        {
            Exception3();
        }
        catch (Exception ex)
        {
            throw new ArgumentException("This is another exception.", ex);
        }
    }

    /// <summary>
    /// An exception for our sample.
    /// </summary>
    private void Exception1()
    {
        try
        {
            Exception2();
        }
        catch (Exception ex)
        {
            throw new GorgonException(GorgonResult.OutOfMemory, ex);
        }
    }

    /// <summary>
    /// An exception for our sample.
    /// </summary>
    private void TriggerException()
    {
        try
        {
            Exception1();
        }
        catch (Exception ex)
        {
            throw new InvalidCastException("The final exception.", ex);
        }
    }

    /// <summary>
    /// Function to clear and disable the custom text area.
    /// </summary>
    private void DisableCustomText()
    {
        TextLogMessage.Enabled = ButtonToLog.Enabled = false;
        TextLogMessage.Text = string.Empty;
    }

    /// <summary>
    /// Function to clear and enable the custom text area.
    /// </summary>
    private void EnableCustomText()
    {
        TextLogMessage.Enabled = true;
        ButtonToLog.Enabled = false;
        TextLogMessage.Text = string.Empty;
        TextLogMessage.Focus();
    }

    /// <summary>
    /// Function called when text is changed in the textbox.
    /// </summary>
    /// <param name="sender">Send of the event.</param>
    /// <param name="e">Event parameters.</param>
    private void TextLogMessage_TextChanged(object sender, EventArgs e) => ButtonToLog.Enabled = TextLogMessage.Text.Length > 0;

    /// <summary>
    /// Function called when the warning button is clicked.
    /// </summary>
    /// <param name="sender">Send of the event.</param>
    /// <param name="e">Event parameters.</param>
    private void ButtonWarning_Click(object sender, EventArgs e)
    {
        DisableCustomText();
        Log.PrintWarning("This is a warning message. We use this to let users know that things may not work the way they might expect.", LoggingLevel.Verbose);
    }

    /// <summary>
    /// Function called when the error button is clicked.
    /// </summary>
    /// <param name="sender">Send of the event.</param>
    /// <param name="e">Event parameters.</param>
    private void ButtonError_Click(object sender, EventArgs e)
    {
        DisableCustomText();
        Log.PrintError("This is an error message. We use this to let users know that something is not working.", LoggingLevel.Verbose);
    }

    /// <summary>
    /// Function called when the exception button is clicked.
    /// </summary>
    /// <param name="sender">Send of the event.</param>
    /// <param name="e">Event parameters.</param>
    private void ButtonException_Click(object sender, EventArgs e)
    {
        DisableCustomText();

        try
        {
            TriggerException();
        }
        catch (Exception ex)
        {
            Log.PrintError("We can also log exceptions with full stack traces, and inner exceptions.", LoggingLevel.Verbose);
            Log.PrintException(ex);
        }
    }

    /// <summary>
    /// Function called when the send to log button is clicked.
    /// </summary>
    /// <param name="sender">Send of the event.</param>
    /// <param name="e">Event parameters.</param>
    private void ButtonToLog_Click(object sender, EventArgs e)
    {
        string text = TextLogMessage.Text.Trim();

        Log.Print($"Custom text: \"{text}\"", LoggingLevel.Simple);
        TextLogMessage.Focus();
        TextLogMessage.SelectAll();
    }

    /// <summary>
    /// Function called when the custom button is clicked.
    /// </summary>
    /// <param name="sender">Send of the event.</param>
    /// <param name="e">Event parameters.</param>
    private void ButtonCustom_Click(object sender, EventArgs e) => EnableCustomText();

    /// <summary>
    /// Initializes a new instance of the <see cref="FormMain"/> class.
    /// </summary>
    public FormMain() => InitializeComponent();
}
