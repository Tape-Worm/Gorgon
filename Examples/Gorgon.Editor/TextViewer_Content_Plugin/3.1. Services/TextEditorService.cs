
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 5, 2020 1:18:16 AM
// 


using Gorgon.UI;

namespace Gorgon.Examples;

/// <summary>
/// A service used to present a text editor to the user
/// </summary>
internal class TextEditorService
{
    // This is an example of a service. It allows us a means of providing functionality to the view model without violating the 
    // rule that the view model shouldn't know anything about the view.
    //
    // By providing this service we can wrap up this functionality in an agnostic way. For example, this particular implementation 
    // uses a form to allow the user to input text. But, what if we wanted another means of input? By wrapping this in a service 
    // we can define any means we'd like to retrieve the input. All the view model cares about is that we get text back from some 
    // external means. How we get that text is irrelevant.

    /// <summary>
    /// Function to retrieve the text from the service.
    /// </summary>
    /// <param name="currentText">The current text.</param>
    /// <returns>The new text, or <b>null</b> if cancelled.</returns>
    public string GetText(string currentText)
    {
        using FormTextEditor textEditor = new();
        textEditor.ContentText = textEditor.OriginalText = currentText;

        return textEditor.ShowDialog(GorgonApplication.MainForm) == DialogResult.Cancel ? null : textEditor.ContentText;
    }
}
