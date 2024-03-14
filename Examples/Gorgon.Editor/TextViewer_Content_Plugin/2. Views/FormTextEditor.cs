
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
// Created: August 5, 2020 1:03:04 AM
// 


namespace Gorgon.Examples;

/// <summary>
/// Our form used to modify the text
/// </summary>
/// <remarks>
/// This is just a standard control. No view model or MVVM support. It's used to support the text editor service
/// </remarks>
internal partial class FormTextEditor
    : Form
{

    /// <summary>
    /// Property to set or return the original text.
    /// </summary>
    public string OriginalText
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the text on the textbox.
    /// </summary>        
    public string ContentText
    {
        get => TextContent.Text;
        set => TextContent.Text = value ?? string.Empty;
    }



    /// <summary>
    /// Function to validate the OK button state.
    /// </summary>
    private void ValidateOk() => ButtonOK.Enabled = (!string.IsNullOrWhiteSpace(TextContent.Text)) && (!string.Equals(TextContent.Text, OriginalText, StringComparison.CurrentCulture));

    /// <summary>Handles the TextChanged event of the TextContent control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void TextContent_TextChanged(object sender, EventArgs e) => ValidateOk();



    /// <summary>Initializes a new instance of the <see cref="FormTextEditor"/> class.</summary>
    public FormTextEditor() => InitializeComponent();

}
