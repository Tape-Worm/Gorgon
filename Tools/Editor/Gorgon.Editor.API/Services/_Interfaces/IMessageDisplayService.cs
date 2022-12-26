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
// Created: August 31, 2018 12:21:33 AM
// 
#endregion

using System;

namespace Gorgon.Editor.Services;

/// <summary>
/// Values that can be returned from a message.
/// </summary>
[Flags]
public enum MessageResponse
{
    /// <summary>
    /// No message.
    /// </summary>
    None = 0,
    /// <summary>
    /// User canceled.
    /// </summary>
    Cancel = 1,
    /// <summary>
    /// User confirmed.
    /// </summary>
    Yes = 2,
    /// <summary>
    /// User denied.
    /// </summary>
    No = 3,
    /// <summary>
    /// User confirmed that operation should be applied to all items.
    /// </summary>
    YesToAll = 4,
    /// <summary>
    /// User denied operation to all items.
    /// </summary>
    NoToAll = 5
}

/// <summary>
/// A display service for showing messages on the UI.
/// </summary>
public interface IMessageDisplayService
{
    #region Methods.
    /// <summary>
    /// Function to show an informational message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="caption">[Optional] A caption for the message.</param>
    void ShowInfo(string message, string caption = null);

    /// <summary>
    /// Function to show an error message based on an exception.
    /// </summary>
    /// <param name="ex">The exception to display.</param>
    /// <param name="message">[Optional] A custom message to display with the error.</param>
    /// <param name="caption">[Optional] A caption for the message.</param>
    void ShowError(Exception ex, string message = null, string caption = null);

    /// <summary>
    /// Function to show an error message with optional detail information.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="caption">[Optional] A caption for the message.</param>
    /// <param name="details">[Optional] Additional information for the error.</param>
    void ShowError(string message, string caption = null, string details = null);

    /// <summary>
    /// Function to show a warning message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="caption">[Optional] A caption for the message.</param>
    /// <param name="details">[Optional] Additional information for the error.</param>
    void ShowWarning(string message, string caption = null, string details = null);

    /// <summary>
    /// Function to show a confirmation message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="caption">[Optional] A caption for the message.</param>
    /// <param name="toAll">[Optional] <b>true</b> to allow an application to apply the result to all items, <b>false</b> to only allow an application to apply the result to a single item.</param>
    /// <param name="allowCancel">[Optional] <b>true</b> to allow cancellation support, <b>false</b> to only allow accept or deny functionality.</param>
    /// <returns>The response value for the message.</returns>
    MessageResponse ShowConfirmation(string message, string caption = null, bool toAll = false, bool allowCancel = false);
    #endregion
}
