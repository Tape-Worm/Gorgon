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
// Created: Sunday, March 10, 2013 11:07:01 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using GorgonLibrary.Native;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Win32 functions.
	/// </summary>
	[System.Security.SuppressUnmanagedCodeSecurity()]
	static class Win32API
	{
		#region Variables.
		private static IDictionary<string, GorgonMinMax> _ranges = null;		// List of ranges.
		private static IDictionary<int, string> _codePointNames = null;			// List of code point names.
		#endregion

		#region Methods.
		/// <summary>
		/// Gets the ranges supported by a font.
		/// </summary>
		/// <param name="hdc">Device context.</param>
		/// <param name="lpgs">Font ranges.</param>
		/// <returns></returns>
		[DllImport("gdi32.dll")]
		private static extern uint GetFontUnicodeRanges(IntPtr hdc, IntPtr lpgs);

		/// <summary>
		/// Selects an object into the device context.
		/// </summary>
		/// <param name="hDC">Device context.</param>
		/// <param name="hObject">Object to select.</param>
		/// <returns>The previous object.</returns>
		[DllImport("gdi32.dll")]
		public extern static IntPtr SelectObject(IntPtr hDC, IntPtr hObject);		

		/// <summary>
		/// Convert a virtual key to ascii.
		/// </summary>
		/// <param name="uVirtKey"></param>
		/// <param name="uScanCode"></param>
		/// <param name="lpbKeyState"></param>
		/// <param name="lpChar"></param>
		/// <param name="uFlags"></param>
		/// <returns></returns>
		[DllImport("user32.dll", CharSet=CharSet.Unicode)]
		private extern static int ToUnicode(int uVirtKey, int uScanCode , byte[] lpbKeyState, [Out] char[] lpChar, int cchBuff, int uFlags);

		/// <summary>
		/// Retrieves the current keyboard state.
		/// </summary>
		/// <param name="uVirtKey"></param>
		/// <param name="uScanCode"></param>
		/// <param name="lpbKeyState"></param>
		/// <param name="lpChar"></param>
		/// <param name="uFlags"></param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		private extern static int GetKeyboardState(byte[] lpbKeyState);

		/// <summary>
		/// Function to retrieve indices for a glyph.
		/// </summary>
		/// <param name="hdc"></param>
		/// <param name="lpsz"></param>
		/// <param name="c"></param>
		/// <param name="pgi"></param>
		/// <param name="fl"></param>
		/// <returns></returns>
		[DllImport("gdi32.dll", EntryPoint = "GetGlyphIndicesW")]
		private static extern uint GetGlyphIndices([In] IntPtr hdc, [In] [MarshalAs(UnmanagedType.LPTStr)] string lpsz, int c, [Out] ushort[] pgi, uint fl);
		
		/// <summary>
		/// Function to build a list of unicode ranges.
		/// </summary>
		private static void BuildUnicodeRangeList()
		{
			IList<string> rangeLines = Properties.Resources.UnicodeBlocks.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			IList<string> items = null;
			_ranges = new SortedDictionary<string, GorgonMinMax>();

			// Break out the lines.
			foreach (var line in rangeLines)
			{
				items = line.Split(new char[] { ';' });

				// Get range.
				if ((!string.IsNullOrEmpty(items[0])) && (!string.IsNullOrEmpty(items[1])))
				{
					var range = new GorgonMinMax(int.Parse(items[0].Substring(0, items[0].IndexOf('.')), System.Globalization.NumberStyles.HexNumber), 
						int.Parse(items[0].Substring(items[0].LastIndexOf('.') + 1), System.Globalization.NumberStyles.HexNumber));

					// Combine the first 2 latin categories into the one category.
					if (range.Maximum <= 0xff) 
					{
						if (!_ranges.ContainsKey("Latin + Latin Supplement"))
							_ranges.Add("Latin + Latin Supplement", new GorgonMinMax(0, 0xFF));
					}
					else
						_ranges.Add(items[1].Trim(), range);
				}
			}
		}

		/// <summary>
		/// Function to determine if a character is supported.
		/// </summary>
		/// <param name="c">The character to check.</param>
		/// <param name="hDc">Device context.</param>
		/// <returns>TRUE if supported, FALSE if not.</returns>
		public static bool IsGlyphSupported(char c, IntPtr hDc)
		{
			var indices = new ushort[1];

			GetGlyphIndices(hDc, c.ToString(), 1, indices, 0x01);

			return indices[0] != 0xffff;	
		}

		/// <summary>
		/// Function to retrieve a character for a key stroke.
		/// </summary>
		/// <param name="key">Key to look up.</param>
		/// <returns>The character for the key.</returns>
		public static char GetKeyCharacter(System.Windows.Forms.Keys key)
		{
			char result = ' ';

			var keys = new byte[255];
			GetKeyboardState(keys);
			var characters = new char[1];

			if (ToUnicode((int)key, 0, keys, characters, characters.Length, 1) != 0)
				result = characters[0];

			return result;
		}

		/// <summary>
		/// Function to retrieve 
		/// </summary>
		/// <param name="character">Character for the code point.</param>
		/// <returns>The name of the code point if it exists.</returns>
		public static string GetCodePointName(char character)
		{
			if (_codePointNames == null)
			{
				IList<string> lines = Properties.Resources.UnicodeData.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

				_codePointNames = new Dictionary<int, string>();

				foreach (var line in lines)
				{
					IList<string> fields = line.Split(new char[] { ';' });
					_codePointNames.Add(Int32.Parse(fields[0], System.Globalization.NumberStyles.HexNumber), string.IsNullOrEmpty(fields[10]) ? fields[1] : fields[10]);
				}
			}

			if (_codePointNames.ContainsKey(Convert.ToInt32(character)))
				return _codePointNames[Convert.ToInt32(character)];
			else
				return string.Empty;
		}

		/// <summary>
		/// Function to retrieve the ranges available to a specific font.
		/// </summary>
		/// <param name="font">Font to look up.</param>
		/// <param name="hDc">Device context.</param>
		/// <returns>A list of ranges.</returns>
		public static IDictionary<string, GorgonMinMax> GetUnicodeRanges(Font font, IntPtr hDc)
		{
			Dictionary<string, GorgonMinMax> result = null;

			if (_ranges == null)
				BuildUnicodeRangeList();

			uint size = 0;
			int itemCount = 0;
			size = GetFontUnicodeRanges(hDc, IntPtr.Zero);

			using (var stream = new GorgonDataStream((int)size))
			{							
				GetFontUnicodeRanges(hDc, stream.BasePointer);

				// Skip the first 12 bytes.
				stream.Read<Int64>();
				stream.Read<Int32>();

				itemCount = stream.Read<int>();
				result = new Dictionary<string, GorgonMinMax>(itemCount);

				for (int i = 0; i < itemCount; i++)
				{
					GorgonMinMax value = default(GorgonMinMax);

					value.Minimum = stream.Read<ushort>();
					value.Maximum = stream.Read<ushort>() + value.Minimum - 1;

					var rangeName = (from unicodeRange in _ranges
									where unicodeRange.Value.Contains(value.Minimum) && unicodeRange.Value.Contains(value.Maximum)
									select unicodeRange).SingleOrDefault();

					if ((!string.IsNullOrEmpty(rangeName.Key)) && (!result.ContainsKey(rangeName.Key)))
						result.Add(rangeName.Key, rangeName.Value);
				}
			}

			return result;
		}
		#endregion
	}
}
