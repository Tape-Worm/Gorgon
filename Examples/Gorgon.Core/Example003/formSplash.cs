#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, September 18, 2012 8:07:22 PM
// 
#endregion

using System.Windows.Forms;

namespace Gorgon.Examples
{
    public partial class FormSplash : Form
    {
        /// <summary>
        /// Function to update the text label.
        /// </summary>
        /// <param name="text">Text to display.</param>
        public void UpdateText(string text)
        {
            labelText.Text = text;
            labelText.Refresh();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormSplash" /> class.
        /// </summary>
        public FormSplash() => InitializeComponent();
    }
}
