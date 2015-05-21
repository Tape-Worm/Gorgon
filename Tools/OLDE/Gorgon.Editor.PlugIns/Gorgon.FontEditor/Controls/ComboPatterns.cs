﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Monday, March 10, 2014 1:19:49 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;

namespace Gorgon.Editor.FontEditorPlugIn
{
	/// <summary>
	/// A combo box for displaying hatch patterns.
	/// </summary>
	class ComboPatterns
		: ComboBox
	{
		#region Variables.
		private SortedDictionary<string, HatchStyle> _patternList = new SortedDictionary<string, HatchStyle>(StringComparer.OrdinalIgnoreCase);		// List of patterns.
		private readonly StringBuilder _properName = new StringBuilder(128);													                    // Proper name buffer.
		#endregion

		#region Properties.
		/// <summary>
		/// N/A
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public new DrawMode DrawMode
		{
			get
			{
				return DrawMode.OwnerDrawVariable;
			}
			// ReSharper disable once ValueParameterNotUsed
			set
			{
			}
		}

		/// <summary>
		/// N/A
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public new bool Sorted
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Property to set or return the current style.
		/// </summary>
		[Browsable(false)]
		public HatchStyle Style
		{
			get
			{
				return SelectedIndex == -1 ? HatchStyle.BackwardDiagonal : _patternList[Items[SelectedIndex].ToString()];
			}
			set
			{
				string properName = GetProperName(value.ToString());

				if (!_patternList.ContainsKey(properName))
				{
					return;
				}

				SelectedIndex = Items.IndexOf(properName);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to separate an enumeration name at capital letters and numeric values with a space.
		/// </summary>
		/// <param name="enumName">Enumeration name.</param>
		/// <returns>The converted name.</returns>
		private string GetProperName(string enumName)
		{
			_properName.Length = 0;

			// Add spaces between the names.
			for (int i = 0; i < enumName.Length; ++i)
			{
				if ((i == 0)
				    || ((!char.IsUpper(enumName[i]))
				        && (!char.IsNumber(enumName[i])))
				    || ((char.IsNumber(enumName[i]))
				        && (char.IsNumber(enumName[i - 1]))))
				{
					_properName.Append(enumName[i]);
					continue;
				}

				_properName.Append(" ");
				_properName.Append(enumName[i]);
			}

			return _properName.ToString();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ComboBox.DrawItem"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.DrawItemEventArgs"/> that contains the event data.</param>
		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			base.OnDrawItem(e);

			TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;

			if ((e.Index < 0) || (e.Index >= Items.Count))
			{
				return;
			}

			if (GorgonFontEditorPlugIn.IsRightToLeft(this))
			{
				flags |= TextFormatFlags.RightToLeft;
			}

			e.DrawBackground();

			if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
			{
				e.DrawFocusRectangle();
			}

			string patternName = Items[e.Index].ToString();
		    HatchStyle style;

			if (!_patternList.TryGetValue(patternName, out style))
			{
				return;
			}

			e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			var textBounds = new Rectangle(26 + e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
			var patternBounds = new Rectangle(e.Bounds.Left + 2, e.Bounds.Top + 2, 22, e.Bounds.Height - 4);

			using (Brush brush = new HatchBrush(style, e.ForeColor, e.BackColor))
			{
				TextRenderer.DrawText(e.Graphics, patternName, Font, textBounds, e.ForeColor, e.BackColor, flags);
				e.Graphics.FillRectangle(brush, patternBounds);
				e.Graphics.DrawRectangle(Pens.Black, patternBounds);
			}
		}

		/// <summary>
		/// Function to refresh the font list.
		/// </summary>
		public void RefreshPatterns()
		{
			string[] patterns = Enum.GetNames(typeof(HatchStyle));
			var styles = (HatchStyle[])Enum.GetValues(typeof(HatchStyle));
			Items.Clear();
			_patternList.Clear();

			for (int i = 0; i < patterns.Length; ++i)
			{
				_patternList[GetProperName(patterns[i])] = styles[i];
			}

			foreach (KeyValuePair<string, HatchStyle> pattern in _patternList)
			{
				Items.Add(pattern.Key);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ComboPatterns"/> class.
		/// </summary>
		public ComboPatterns()
		{
			base.DrawMode = DrawMode.OwnerDrawVariable;
			base.Sorted = true;
		}
		#endregion
	}
}
