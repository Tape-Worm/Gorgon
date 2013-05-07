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
// Created: Sunday, May 06, 2012 11:15:54 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Listbox for font styles.
	/// </summary>
	class listBoxFontStyle
		: CheckedListBox
	{
		#region Variables.
		private StringFormat _format = null;					// String format.
		private bool _disposed = false;							// Flag to indicate that we've disposed the object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the editor service to use.
		/// </summary>
		public IWindowsFormsEditorService Service
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the font style.
		/// </summary>
		public FontStyle FontStyle
		{
			get
			{
				var result = System.Drawing.FontStyle.Regular;

				if (CheckedItems.Count == 0)
					return FontStyle.Regular;

				foreach (var item in CheckedItems)
				{
					switch (item.ToString().ToLower())
					{
						case "bold":
							result |= System.Drawing.FontStyle.Bold;
							break;
						case "italic":
							result |= System.Drawing.FontStyle.Italic;
							break;
						case "underline":
							result |= System.Drawing.FontStyle.Underline;
							break;
						case "strikeout":
							result |= System.Drawing.FontStyle.Strikeout;
							break;
					}
				}

				return result;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control"/> and its child controls and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_format != null)
						_format.Dispose();
				}

				_format = null;
				_disposed = true;
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.CheckedListBox.ItemCheck"/> event.
		/// </summary>
		/// <param name="ice">An <see cref="T:System.Windows.Forms.ItemCheckEventArgs"/> that contains the event data.</param>
		protected override void OnItemCheck(ItemCheckEventArgs ice)
		{
			base.OnItemCheck(ice);

			if (Items.Count < 2)
				ice.NewValue = CheckState.Checked;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="listBoxFontStyle"/> class.
		/// </summary>
		public listBoxFontStyle()
		{
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="listBoxFontStyle"/> class.
		/// </summary>
		/// <param name="fontFamily">Font family.</param>
		/// <param name="currentStyles">Currently selected styles.</param>
		public listBoxFontStyle(string fontFamily, FontStyle currentStyles)
			: this()
		{
			FontStyle[] fontStyles = ((FontStyle[])Enum.GetValues(typeof(FontStyle))).Where(item => item != FontStyle.Regular).ToArray();
			Font cachedFont = GorgonFontEditorPlugIn.CachedFonts[fontFamily.ToLower()];

			Items.Clear();

			for (int i = 0; i < fontStyles.Length; i++)
			{
				if (cachedFont.FontFamily.IsStyleAvailable(fontStyles[i]))
					Items.Add(fontStyles[i], (currentStyles & fontStyles[i]) == fontStyles[i]);
			}

			CheckOnClick = true;
			BackColor = Color.FromArgb(255, 58, 58, 58);
			ForeColor = Color.White;
		}
		#endregion
	}
}
