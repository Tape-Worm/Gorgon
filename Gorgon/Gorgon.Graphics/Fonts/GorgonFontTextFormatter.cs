#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: February 21, 2017 12:50:31 PM
// 
#endregion

using System.Text;
using Gorgon.Math;

namespace Gorgon.Graphics
{
	/// <summary>
	/// The text formatter used to format a string for rendering.
	/// </summary>
	public static class GorgonFontTextFormatter
	{
		/// <summary>
		/// Function to format a string for rendering using a <see cref="GorgonFont"/>.
		/// </summary>
		/// <param name="renderText">The text to render.</param>
		/// <param name="tabSpacing">[Optional] The number of spaces used to replace a tab control character.</param>
		/// <returns>The formatted text.</returns>
		/// <remarks>
		/// <para>
		/// This method will format the string so that all control characters such as carriage return, and tabs are converted into spaces. 
		/// </para>
		/// <para>
		/// If the <paramref name="tabSpacing"/> parameter is changed from its default of 4, then that will be the number of space substituted for the tab control character.
		/// </para>
		/// </remarks>
		public static string FormatStringForRendering(this string renderText, int tabSpacing = 4)
		{
			if (string.IsNullOrEmpty(renderText))
			{
				return string.Empty;
			}

			tabSpacing = tabSpacing.Min(1);
			var newString = new StringBuilder(renderText);

			// Strip all carriage returns.
			newString.Replace("\r", string.Empty);

			// Convert tabs to spaces.
			newString.Replace("\t", new string(' ', tabSpacing));

			return newString.ToString();
		}

		/// <summary>
		/// Function to break a string into an array of strings based on the newline control characters present in the text.
		/// </summary>
		/// <param name="renderText">The text to evaluate.</param>
		/// <returns>The array of strings representing a single line per newline control character.</returns>
		public static string[] GetLines(this string renderText)
		{
			return string.IsNullOrEmpty(renderText) ? new string[0] : renderText.Split('\n');
		}
	}
}
