#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: September 3, 2021 8:09:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Editor.UI.Views;
using Gorgon.UI;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// A control that allows the user to select specific characters for font generation.
/// </summary>
public partial class CharacterPicker 
        : EditorBaseControl
{
        #region Variables.
        // Page for characters.
        private int _page;
        // Character ranges for the font.
        private IDictionary<string, GorgonRange> _characterRanges;
        // GDI+ graphics.
        private System.Drawing.Graphics _graphics;
        // Font handle.
        private IntPtr _hFont = IntPtr.Zero;
        // Device context.
        private IntPtr _hDc = IntPtr.Zero;
        // Previous object.
        private IntPtr _prevHObj = IntPtr.Zero;
        // The current font.
        private Font _current;
        // The list of selected characters.
        private IEnumerable<char> _characters;
        #endregion

        #region Events.
        // Events for handling button clicks.
        private EventHandler _okClickedEvent;
        private EventHandler _cancelClickedEvent;
        // Event for the character property change event.
        private EventHandler _characterChangedEvent;

        /// <summary>
    /// Event triggered when the characters property is updated.
    /// </summary>
        [Category("Property Changed"), Description("Event triggered when the characters property is updated.")]
        public event EventHandler CharactersChanged
        {
            add
            {
                if (value is null)
                {
                    _characterChangedEvent = null;
                    return;
                }

                _characterChangedEvent += value;
            }
            remove
            {
                if (value is null)
                {
                    return;
                }

                _characterChangedEvent -= value;
            }
        }

        /// <summary>
        /// Event triggered when the OK button is clicked.
        /// </summary>        
        [Category("Behavior"), Description("Event triggered when the OK button is clicked.")]
        public event EventHandler OkClicked
        {
            add
            {
                if (value is null)
                {
                    _okClickedEvent = null;
                    return;
                }

                _okClickedEvent += value;
            }
            remove
            {
                if (value is null)
                {                    
                    return;
                }

                _okClickedEvent -= value;
            }
        }

        /// <summary>
        /// Event triggered when the OK button is clicked.
        /// </summary>        
        [Category("Behavior"), Description("Event triggered when the Cancel button is clicked.")]
        public event EventHandler CancelClicked
        {
            add
            {
                if (value is null)
                {
                    _cancelClickedEvent = null;
                    return;
                }

                _cancelClickedEvent += value;
            }
            remove
            {
                if (value is null)
                {
                    return;
                }

                _cancelClickedEvent -= value;
            }
        }
        #endregion

        #region Properties.
        /// <summary>
    /// Property to set or return whether the OK/Cancel buttons are visible.
    /// </summary>
    [Category("Behavior"), Description("Shows or hides the OK and Cancel buttons."), 
        Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(true)]
        public bool ShowOkCancel
        {
            get => PanelButtons.Visible;
            set => PanelButtons.Visible = value;
        }

        /// <summary>
        /// Property to set or return the current font.
        /// </summary>
        [Browsable(false)]
        public Font CurrentFont
        {
            get => _current;
            set
            {
                if (_current == value)
                {
                    return;
                }

                _current = value;
                CurrentFontChanged();
            }
        }

        /// <summary>
        /// Property to set or return the list of characters.
        /// </summary>
        [Browsable(false)]
        public IEnumerable<char> Characters
        {
            get => _characters;
            set
            {
                _characters = value;

                if (!IsDesignTime)
                {
                    _characterChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the Click event of the buttonSelectAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ButtonSelectAll_Click(object sender, EventArgs e)
        {
            try
            {
                var item = (KeyValuePair<string, GorgonRange>)ListCharacterRanges.SelectedItems[0].Tag;

                string newString = string.Empty;

                SelectFont();

                for (int index = item.Value.Minimum; index <= item.Value.Maximum; index++)
                {
                    char c = Convert.ToChar(index);
                    if (IsCharacterSupported(c))
                    {
                        newString += Convert.ToChar(index);
                    }
                }

                TextSelectedCharacters.Text = " " + newString;
                UpdateSelections();
                UpdateCharacters();
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(this, ex);
            }
            finally
            {
                DeselectFont();
                ValidateButtons();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the listRanges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ListCharacterRanges_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _page = 0;
                ScrollVertical.Value = 0;
                if (ListCharacterRanges.SelectedItems.Count > 0)
                {
                    ButtonSelectAll.Text = $"{Resources.GORFNT_TEXT_SELECT_ALL_OF} {ListCharacterRanges.SelectedItems[0].SubItems[1].Text}";
                }

                GetCharacterSets();
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(this, ex);
            }
            finally
            {
                ValidateButtons();
            }
        }

        /// <summary>
        /// Function to validate the buttons.
        /// </summary>
        private void ValidateButtons()
        {
            ButtonOK.Enabled = TextSelectedCharacters.Text != string.Empty;
            ButtonSelectAll.Enabled = ListCharacterRanges.SelectedItems.Count > 0;
        }

        /// <summary>
        /// Handles the Scroll event of the scrollVertical control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ScrollEventArgs"/> instance containing the event data.</param>
        private void ScrollVertical_Scroll(object sender, ScrollEventArgs e)
        {
            _page = ScrollVertical.Value * 8;
            GetCharacterSets();
        }

        /// <summary>
        /// Handles the MouseEnter event of the fontControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void FontControl_MouseEnter(object sender, EventArgs e)
        {
            var check = (CheckBox)sender;

            string name = Win32API.GetCodePointName(check.Text[0]);

            if (string.IsNullOrEmpty(name))
            {
                name = $"'{check.Text}'";
            }

            TipChar.SetToolTip(check, $"U+0x{Convert.ToUInt16(check.Text[0]).FormatHex()} ({Convert.ToUInt16(check.Text[0])}, {name})");
        }

        /// <summary>
        /// Handles the CheckedChanged event of the fontControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void FontControl_CheckedChanged(object sender, EventArgs e)
        {
            string chars = TextSelectedCharacters.Text;     // Characters.

            // Go through each character.
            var fontControl = (CheckBox)sender;
            int pos = chars.IndexOf(fontControl.Text, StringComparison.Ordinal);

            if (pos > -1)
            {
                if (!fontControl.Checked)
                {
                    chars = chars.Remove(pos, 1);
                }
            }
            else
            {
                if (fontControl.Checked)
                {
                    chars += fontControl.Text;
                }
            }

            if ((chars.Length < 1)
                || (chars.IndexOf(" ", StringComparison.Ordinal) < 0))
            {
                chars += " ";
            }

            chars = chars.Replace('\0', ' ');

            TextSelectedCharacters.Text = chars;
            UpdateCharacters();
        }

        /// <summary>
        /// Function to update the selections.
        /// </summary>
        private void UpdateSelections()
        {
            // Go through each character.            
            for (int i = 1; i <= 48; i++)
            {
                var fontControl = (CheckBox)panelCharacters.Controls["checkBox" + i];       // Font character.

                if (!fontControl.Enabled)
                {
                    continue;
                }

                fontControl.CheckedChanged -= FontControl_CheckedChanged;

                fontControl.Checked = TextSelectedCharacters.Text.IndexOf(fontControl.Text, StringComparison.Ordinal) > -1;

                fontControl.CheckedChanged += FontControl_CheckedChanged;
            }
        }

        /// <summary>
        /// Handles the KeyDown event of the textCharacters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        private void TextCharacters_KeyDown(object sender, KeyEventArgs e)
        {
            if ((TextSelectedCharacters.Text != string.Empty) && (e.KeyCode == Keys.Enter))
            {
                e.Handled = true;
                UpdateSelections();
            }            
            ValidateButtons();
        }

        /// <summary>
        /// Handles the Leave event of the textCharacters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void TextCharacters_Leave(object sender, EventArgs e)
        {
            if (TextSelectedCharacters.Text == string.Empty)
            {
                TextSelectedCharacters.Text = " ";
            }

            UpdateSelections();
            UpdateCharacters();
            ValidateButtons();
            panelCharacters.Focus();
        }

        /// <summary>
    /// Function to update the final character list.
    /// </summary>
        private void UpdateCharacters()
        {
            if (TextSelectedCharacters.Text.IndexOf(" ", StringComparison.Ordinal) < 0)
            {
                TextSelectedCharacters.Text += " ";
            }

            Characters = TextSelectedCharacters.Text;
        }

        /// <summary>Handles the Click event of the ButtonCancel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ButtonCancel_Click(object sender, EventArgs e) => _cancelClickedEvent?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Handles the Click event of the buttonOK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ButtonOK_Click(object sender, EventArgs e)
        {
            UpdateCharacters();
            _okClickedEvent?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to determine if a character is supported by the font.
        /// </summary>
        /// <param name="c">Character to check.</param>
        /// <returns>TRUE if supported, FALSE if not.</returns>
        private bool IsCharacterSupported(char c)
        {
            if ((char.IsWhiteSpace(c)) || (char.IsControl(c)))
            {
                return false;
            }

            if (!Win32API.IsGlyphSupported(c, _hDc))
            {
                return false;
            }

            int charValue = Convert.ToInt32(c);

            return _characterRanges.Any(item => ((charValue >= item.Value.Minimum) && (charValue <= item.Value.Maximum)));
        }

        /// <summary>
        /// Function to retrieve a list of character sets.
        /// </summary>
        private void GetCharacterSets()
        {
            if (CurrentFont is null)
            {
                return;
            }

            if (ListCharacterRanges.SelectedIndices.Count == 0)
            {
                return;
            }

            ListViewItem charSet = ListCharacterRanges.SelectedItems[0];
            GorgonRange range = ((KeyValuePair<string, GorgonRange>)charSet.Tag).Value;
            int lineCount = ((int)System.Math.Ceiling(range.Range / 8.0f)) - 1;
            if (lineCount > 6)
            {
                ScrollVertical.Enabled = true;
                ScrollVertical.Minimum = 0;
                ScrollVertical.Maximum = lineCount;
                ScrollVertical.SmallChange = 1;
                ScrollVertical.LargeChange = 6;
            }
            else
            {
                ScrollVertical.Enabled = false;
            }

            try
            {
                SelectFont();

                // Set the font.
                for (int i = 1; i <= 48; i++)
                {
                    string checkBoxName = "CheckBox" + i;
                    var fontControl = (CheckBox)panelCharacters.Controls[checkBoxName];                 // Font character.            

                    fontControl.MouseEnter -= FontControl_MouseEnter;
                    fontControl.CheckedChanged -= FontControl_CheckedChanged;

                    // If the range has less characters than our character list, then disable the rest of the check boxes.
                    if (i > range.Range)
                    {
                        fontControl.Enabled = false;
                        fontControl.Font = Font;
                        fontControl.BackColor = Color.DimGray;
                        fontControl.Text = string.Empty;
                        fontControl.Checked = false;
                        continue;
                    }

                    char c = Convert.ToChar(i + range.Minimum + _page);


                    if ((!IsCharacterSupported(c)) || (Convert.ToInt32(c) > range.Maximum))
                    {
                        fontControl.Enabled = false;
                        fontControl.Font = Font;
                        fontControl.BackColor = Color.DimGray;
                        fontControl.Text = string.Empty;
                        fontControl.Checked = false;
                        continue;
                    }

                    fontControl.Enabled = true;
                    fontControl.BackColor = Color.White;

                    fontControl.Font = CurrentFont;
                    fontControl.Text = c.ToString(CultureInfo.CurrentUICulture);
                    fontControl.Checked = TextSelectedCharacters.Text.IndexOf(fontControl.Text, StringComparison.Ordinal) != -1;

                    fontControl.CheckedChanged += FontControl_CheckedChanged;
                    fontControl.MouseEnter += FontControl_MouseEnter;
                }
            }
            finally
            {
                DeselectFont();
                ValidateButtons();
            }
        }

        /// <summary>
        /// Function to fill the unicode ranges based on the font.
        /// </summary>
        private void FillRanges()
        {
            ListCharacterRanges.Items.Clear();
            if (_characterRanges.Count <= 0)
            {
                return;
            }

            ListCharacterRanges.BeginUpdate();
            IOrderedEnumerable<KeyValuePair<string, GorgonRange>> sortedRanges = _characterRanges.OrderBy(item => item.Value.Minimum);
            foreach (KeyValuePair<string, GorgonRange> range in sortedRanges)
            {
                var item = new ListViewItem(((ushort)range.Value.Minimum).FormatHex() + ".." + ((ushort)range.Value.Maximum).FormatHex());
                item.SubItems.Add(range.Key);
                item.Tag = range;
                ListCharacterRanges.Items.Add(item);
            }
            ListCharacterRanges.EndUpdate();
            ListCharacterRanges.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            ListCharacterRanges.SelectedIndices.Add(0);
        }

        /// <summary>
        /// Function to select the font object into the device context.
        /// </summary>
        private void SelectFont()
        {
            if (IsDesignTime)
            {
                return;
            }

            if (_hDc == IntPtr.Zero)
            {
                _hDc = _graphics.GetHdc();
            }

            if (_hFont == IntPtr.Zero)
            {
                _hFont = CurrentFont.ToHfont();
            }

            if (_prevHObj == IntPtr.Zero)
            {
                _prevHObj = Win32API.SelectObject(_hDc, _hFont);
            }
        }

        /// <summary>
        /// Function to deselect a font from the device context.
        /// </summary>
        private void DeselectFont()
        {
            if ((IsDesignTime) || (_hDc == IntPtr.Zero) || (_prevHObj == IntPtr.Zero))
            {
                return;
            }

            Win32API.SelectObject(_hDc, _prevHObj);
            _prevHObj = IntPtr.Zero;

            _graphics.ReleaseHdc();
            _hDc = IntPtr.Zero;
        }

        /// <summary>
    /// Function to handle a font change.
    /// </summary>
        private void CurrentFontChanged()
        {
            if (CurrentFont is null)
            {
                _characterRanges?.Clear();
                TextSelectedCharacters.Text = "No font selected.";
                ListCharacterRanges.Items.Clear();
                return;
            }

            try
            {
                _graphics = System.Drawing.Graphics.FromHwnd(Handle);

                SelectFont();

                TextSelectedCharacters.Text = string.Join(string.Empty, Characters);
                _characterRanges = Win32API.GetUnicodeRanges(_hDc);
                FillRanges();
                GetCharacterSets();
            }
            finally
            {
                DeselectFont();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (IsDesignTime)
            {
                return;
            }

            CurrentFontChanged();
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="CharacterPicker" /> class.</summary>
        public CharacterPicker()
        {
            InitializeComponent();

            if (IsDesignTime)
            {
                return;
            }

            // For some reason the scroll bar is not scaling with DPI.
            if (DeviceDpi != 96)
            {
                ScrollVertical.Width = (int)(ScrollVertical.Width * (DeviceDpi / 96.0));
            }
        }
    #endregion
}
