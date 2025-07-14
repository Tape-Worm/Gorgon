// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Saturday, June 18, 2011 4:20:00 PM
// 

using Gorgon.UI.Win32.Properties;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Gorgon.UI.Win32;

/// <summary>
/// Standard messagebox dialogs.
/// </summary>
public static class GorgonDialogs
{
    /// <summary>
    /// Function to display the enhanced error dialog.
    /// </summary>
    /// <param name="ownerHandle">The handle of the owner window for this dialog.</param>
    /// <param name="message">The error message to display.</param>
    /// <param name="caption">The caption for the error box.</param>
    /// <param name="exception">The exception that was thrown.</param>
    /// <remarks>
    /// <para>
    /// If the <paramref name="message"/> parameter is <b>null</b> or an empty string, then the <see cref="Exception.Message"/> property is used to display the error message.
    /// </para>
    /// </remarks>
    public static void Error(nint ownerHandle, Exception exception, string message = "", string caption = "") 
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            message = exception.Message ?? Resources.GOR_EXCEPT_NO_MSG;
        }

        Error(ownerHandle, message, caption);
    }

    /// <summary>
    /// Function to display the enhanced error dialog.
    /// </summary>
    /// <param name="ownerHandle">The handle of the owner window for this dialog.</param>
    /// <param name="message">The error message to display.</param>
    /// <param name="caption">[Optional] The caption for the error box.</param>
    public static void Error(nint ownerHandle, string message, string caption = "") => PInvoke.MessageBox(new HWND(ownerHandle), message, string.IsNullOrWhiteSpace(caption) ? Resources.GOR_TEXT_ERROR : caption, MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONERROR);

    /// <summary>
    /// Function to display the enhanced information dialog.
    /// </summary>
    /// <param name="ownerHandle">The handle of the owner window for this dialog.</param>
    /// <param name="message">The informational message to display.</param>
    /// <param name="caption">[Optional] The caption for the dialog.</param>
    public static void Information(nint ownerHandle, string message, string caption = "") => PInvoke.MessageBox(new HWND(ownerHandle), message, string.IsNullOrWhiteSpace(caption) ? Resources.GOR_TEXT_INFO : caption, MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONINFORMATION);

    /// <summary>
    /// Function to display the enhanced warning dialog.
    /// </summary>
    /// <param name="ownerHandle">The handle of the owner window for this dialog.</param>
    /// <param name="message">The warning message to display.</param>
    /// <param name="caption">[Optional] The caption for the warning box.</param>
    public static void Warning(nint ownerHandle, string message, string caption = "") => PInvoke.MessageBox(new HWND(ownerHandle), message, string.IsNullOrWhiteSpace(caption) ? Resources.GOR_TEXT_WARNING : caption, MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONWARNING);

    /// <summary>
    /// Function to display a confirmation dialog.
    /// </summary>
    /// <param name="ownerHandle">The handle of the owner window for this dialog.</param>
    /// <param name="message">The confirmation message to display.</param>
    /// <param name="caption">[Optional] The caption for the dialog.</param>
    /// <param name="allowCancel">[Optional] <b>true</b> to show a Cancel button, <b>false</b> to hide.</param>
    /// <returns>
    /// A flag from the <see cref="ConfirmationResult"/> enumeration. 
    /// </returns>
    /// <remarks>
    /// <para>
    /// This will display a confirmation dialog with an optional Cancel button, and an optional "To all" checkbox.
    /// </para>
    /// <para>
    /// If the "Yes" button is clicked, this method will return the <see cref="ConfirmationResult.Yes"/> flag, or if the "No" button is clicked, this method will return the <see cref="ConfirmationResult.No"/> 
    /// flag.
    /// </para>
    /// <para>
    /// Setting the <paramref name="allowCancel"/> parameter to <b>true</b> will display a cancel button and will allow the method to return the <see cref="ConfirmationResult.Cancel"/> flag. This flag is 
    /// exclusive and is not combined with the <see cref="ConfirmationResult.ToAll"/> flag under any circumstances.
    /// </para>
    /// </remarks>
    public static ConfirmationResult Confirm(nint ownerHandle, string message, string caption = "", bool allowCancel = false)
    {
        MESSAGEBOX_STYLE style = MESSAGEBOX_STYLE.MB_ICONQUESTION;

        if (allowCancel)
        {
            style |= MESSAGEBOX_STYLE.MB_YESNOCANCEL;
        }
        else
        {
            style |= MESSAGEBOX_STYLE.MB_YESNO;
        }

        MESSAGEBOX_RESULT result = PInvoke.MessageBox(new HWND(ownerHandle), message, string.IsNullOrWhiteSpace(caption) ? Resources.GOR_TEXT_CONFIRM : caption, style);

        return result switch
        {
            MESSAGEBOX_RESULT.IDYES => ConfirmationResult.Yes,
            MESSAGEBOX_RESULT.IDNO => ConfirmationResult.No,
            _ => ConfirmationResult.Cancel,
        };
    }
}