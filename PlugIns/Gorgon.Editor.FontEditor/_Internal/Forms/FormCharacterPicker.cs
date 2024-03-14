#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Sunday, March 10, 2013 10:57:24 PM
// 
#endregion

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// Dialog to pick characters for conversion into bitmap glyphs.
/// </summary>
internal partial class FormCharacterPicker 
        : Form
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the current font.
        /// </summary>
        public Font CurrentFont
        {
            get => CharPicker.CurrentFont;
            set => CharPicker.CurrentFont = value;
        }

        /// <summary>
        /// Property to set or return the list of characters.
        /// </summary>
        public IEnumerable<char> Characters
        {
            get => CharPicker.Characters;
            set => CharPicker.Characters = value;
        }
    #endregion

    #region Methods.
    /// <summary>Handles the OkClicked event of the CharPicker control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void CharPicker_OkClicked(object sender, EventArgs e) => DialogResult = DialogResult.OK;

    /// <summary>Handles the CancelClicked event of the CharPicker control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void CharPicker_CancelClicked(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="FormCharacterPicker"/> class.
        /// </summary>
        public FormCharacterPicker() =>  InitializeComponent();
        #endregion        
}
