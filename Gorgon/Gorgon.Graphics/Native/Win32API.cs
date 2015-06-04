#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Sunday, July 24, 2011 10:15:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Graphics.Properties;

namespace Gorgon.Native
{
	// ReSharper disable InconsistentNaming
	/// <summary>
	/// Win 32 API function calls.
	/// </summary>
	[SuppressUnmanagedCodeSecurity]
	unsafe static class Win32API
	{
		#region Variables.
		private static IntPtr _lasthObj = IntPtr.Zero;				// Last active object handle.
		private static IntPtr _hdc = IntPtr.Zero;					// Current device context.
		private static IntPtr _hFont = IntPtr.Zero;					// Font handle.
		private static System.Drawing.Graphics _lastGraphics;		// Last used graphics interface.
		private static Font _tempFont;				// Temporary font.
		#endregion

		#region Win32 Methods.
		/// <summary>
		/// The SelectObject function selects an object into the specified device context (DC). The new object replaces the previous object of the same type.
		/// </summary>
		/// <param name="hDC">A handle to the DC.</param>
		/// <param name="hObj">A handle to the object to be selected.</param>
		/// <returns>If the selected object is not a region and the function succeeds, the return value is a handle to the object being replaced</returns>
		[DllImport("gdi32.dll", ExactSpelling = true)]
		private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObj);

		/// <summary>
		/// The DeleteDC function deletes the specified device context (DC).
		/// </summary>
		/// <param name="hObj">A handle to the device context.</param>
		/// <returns>If the function succeeds, the return value is nonzero.  If the function fails, the return value is zero.</returns>
		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
		private static extern int DeleteObject(IntPtr hObj);

		/// <summary>
		/// The GetCharABCWidths function retrieves the widths, in logical units, of consecutive characters in a specified range from the current TrueType font. This function succeeds only with TrueType fonts.
		/// </summary>
		/// <param name="HDC">A handle to the device context.</param>
		/// <param name="uFirstChar">The first character in the group of consecutive characters from the current font.</param>
		/// <param name="uLastChar">The last character in the group of consecutive characters from the current font.</param>
		/// <param name="lpABC">A pointer to an array of ABC structures that receives the character widths, in logical units. This array must contain at least as many ABC structures as there are characters in the range specified by the uFirstChar and uLastChar parameters.</param>
		/// <returns><b>true</b> if successful, <b>false</b> if not.</returns>
		[DllImport("gdi32.dll", EntryPoint = "GetCharABCWidthsW", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetCharABCWidthsW(IntPtr HDC, uint uFirstChar, uint uLastChar, ABC* lpABC);

		/// <summary>
		/// The GetKerningPairs function retrieves the character-kerning pairs for the currently selected font for the specified device context.
		/// </summary>
		/// <param name="HDC">A handle to the device context.</param>
		/// <param name="numberOfPairs">The number of pairs in the keyPairs array. If the font has more than nNumPairs kerning pairs, the function returns an error.</param>
		/// <param name="kernPairs">A pointer to an array of KERNINGPAIR structures that receives the kerning pairs. The array must contain at least as many structures as specified by the nNumPairs parameter. If this parameter is NULL, the function returns the total number of kerning pairs for the font.</param>
		/// <returns>If the function succeeds, the return value is the number of kerning pairs returned.  If the function fails, the return value is zero.</returns>
		[DllImport("gdi32.dll", EntryPoint = "GetKerningPairsW", CharSet = CharSet.Unicode)]
		private static extern uint GetKerningPairsW(IntPtr HDC, uint numberOfPairs, KERNINGPAIR* kernPairs);

		/// <summary>
		/// The SetMapMode function sets the mapping mode of the specified device context. The mapping mode defines the unit of measure used to transform page-space units into device-space units, and also defines the orientation of the device's x and y axes.
		/// </summary>
		/// <param name="HDC">A handle to the device context.</param>
		/// <param name="fnMapMode">The new mapping mode.</param>
		/// <returns>If the function succeeds, the return value identifies the previous mapping mode.  If the function fails, the return value is zero.</returns>
		[DllImport("gdi32.dll", EntryPoint = "SetMapMode", CharSet = CharSet.Auto)]
		private static extern MapModes SetMapMode(IntPtr HDC, MapModes fnMapMode);

		/// <summary>
		/// Function to retrieve the nearest monitor to the window.
		/// </summary>
		/// <param name="hwnd">Handle to the window.</param>
		/// <param name="flags">Flags to pass in.</param>
		/// <returns></returns>
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		private static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorFlags flags);

		/// <summary>
		/// Function to enable or disable desktop composition.
		/// </summary>
		/// <param name="uCompositionAction">Composition action.</param>
		/// <returns></returns>
		[DllImport("Dwmapi.dll")]
		public static extern int DwmEnableComposition(int uCompositionAction);

		/// <summary>
		/// Function to determine if desktop composition is enabled or not.
		/// </summary>
		/// <param name="pfEnabled"></param>
		/// <returns></returns>
		[DllImport("dwmapi.dll")]
		public static extern int DwmIsCompositionEnabled([MarshalAs(UnmanagedType.Bool)] out bool pfEnabled);
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the monitor with the largest portion of the window inside of it.
		/// </summary>
		/// <param name="window">Window to locate.</param>
		/// <returns>The handle to the monitor.</returns>
		public static IntPtr GetMonitor(Control window)
		{
		    return MonitorFromWindow(window != null ? window.Handle : IntPtr.Zero, MonitorFlags.MONITOR_DEFAULTTOPRIMARY);
		}

	    /// <summary>
		/// Function to set the active font.
		/// </summary>
		/// <param name="graphics">Graphics interface to use.</param>
		/// <param name="font">Font to set.</param>
		public static void SetActiveFont(System.Drawing.Graphics graphics, Font font)
		{
			if ((_lasthObj != IntPtr.Zero) || (_hdc != IntPtr.Zero) || (_hFont != IntPtr.Zero))
			{
				return;
			}

			try
			{
				_lastGraphics = graphics;
				_tempFont = (Font)font.Clone();
				_hdc = graphics.GetHdc();
				_hFont = _tempFont.ToHfont();

				_lasthObj = SelectObject(_hdc, _hFont);
			}
			catch
			{
				// Restore everything in case of an exception.
				RestoreActiveObject();

				throw;
			}
		}

		/// <summary>
		/// Function to restore the last known active object.
		/// </summary>
		public static void RestoreActiveObject()
		{
			if (_lasthObj != IntPtr.Zero)
			{
				SelectObject(_hdc, _lasthObj);
			}

			if ((_hdc != IntPtr.Zero) && (_lastGraphics != null))
			{
				_lastGraphics.ReleaseHdc();
			}

			if (_hFont != IntPtr.Zero)
			{
				DeleteObject(_hFont);
			}

			if (_tempFont != null)
			{
				_tempFont.Dispose();
			}

			_tempFont = null;
			_hFont = IntPtr.Zero;
			_lasthObj = IntPtr.Zero;
			_hdc = IntPtr.Zero;
			_lastGraphics = null;
		}

		/// <summary>
		/// Function to get the kerning pairs for a font.
		/// </summary>
		/// <returns>A list of kerning pair values for the active font.</returns>
		public static KERNINGPAIR[] GetKerningPairs()
		{
			KERNINGPAIR[] pairs;

			if (_hdc == IntPtr.Zero)
			{
				throw new GorgonException(GorgonResult.CannotEnumerate, Resources.GORGFX_FONT_CANNOT_RETRIEVE_KERNING);
			}

			MapModes lastMode = SetMapMode(_hdc, MapModes.MM_TEXT);

			try
			{
				// Get the number of pairs.
				var size = (int)GetKerningPairsW(_hdc, 0, null);

				// If we have no pairs, then leave here.
				if (size == 0)
				{
					return new KERNINGPAIR[0];
				}

				pairs = new KERNINGPAIR[size];
				KERNINGPAIR* pairPtr = stackalloc KERNINGPAIR[size];

				if (GetKerningPairsW(_hdc, (uint)size, pairPtr) == 0)
				{
					throw new GorgonException(GorgonResult.CannotEnumerate, Resources.GORGFX_FONT_CANNOT_RETRIEVE_KERNING);
				}

				for (int i = 0; i < size; i++)
				{
					pairs[i] = pairPtr[i];
				}
			}
			finally
			{
				SetMapMode(_hdc, lastMode);
			}

			return pairs;
		}

		/// <summary>
		/// Function to get the ABC kerning widths for the active font object.
		/// </summary>
		/// <param name="firstCharacter">First character to return.</param>
		/// <param name="lastCharacter">Last character to return.</param>
		/// <returns>A list of font ABC values.</returns>
		public static IDictionary<char, ABC> GetCharABCWidths(char firstCharacter, char lastCharacter)
		{
			uint firstCharIndex = Convert.ToUInt32(firstCharacter);
			uint lastCharIndex = Convert.ToUInt32(lastCharacter);
			int size = (int)(lastCharIndex - firstCharIndex) + 1;
			var result = new SortedDictionary<char, ABC>();

			if (_hdc == IntPtr.Zero)
			{
				throw new GorgonException(GorgonResult.CannotEnumerate, Resources.GORGFX_FONT_CANNOT_RETRIEVE_ABC);
			}

			ABC* abcData = stackalloc ABC[size];

			if (!GetCharABCWidthsW(_hdc, firstCharIndex, lastCharIndex, abcData))
			{
				throw new GorgonException(GorgonResult.CannotEnumerate, Resources.GORGFX_FONT_CANNOT_RETRIEVE_ABC);
			}

			// Copy to our result.
			for (int i = 0; i < size; i++)
			{
				result.Add(Convert.ToChar(i + Convert.ToInt32(firstCharacter)), abcData[i]);
			}

			return result;
		}
		#endregion
	}
	// ReSharper restore InconsistentNaming
}
