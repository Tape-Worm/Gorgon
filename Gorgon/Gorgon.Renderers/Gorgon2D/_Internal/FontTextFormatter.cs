#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: June 11, 2018 8:50:14 PM
// 
#endregion

using System.Text;
using Gorgon.Graphics.Fonts;
using Gorgon.Math;

namespace Gorgon.Renderers
{
	/// <summary>
	/// The text formatter used to format a string for rendering.
	/// </summary>
	internal static class FontTextFormatter
	{
		/// <summary>
		/// Function to format a string for rendering using a <see cref="GorgonFont"/>.
		/// </summary>
		/// <param name="formattedText">The string builder that will contain the formatted text.</param>
		/// <param name="textToRender">The text to render.</param>
		/// <param name="tabSpacing">The number of spaces used to replace a tab control character.</param>
		/// <remarks>
		/// <para>
		/// This method will format the string so that all control characters such as carriage return, and tabs are converted into spaces. 
		/// </para>
		/// <para>
		/// If the <paramref name="tabSpacing"/> parameter is changed from its default of 4, then that will be the number of space substituted for the tab control character.
		/// </para>
		/// </remarks>
		public static void FormatStringForRendering(this StringBuilder formattedText, string textToRender, int tabSpacing)
		{
		    formattedText.Length = 0;

			if (string.IsNullOrEmpty(textToRender))
			{
			    return;
			}

			tabSpacing = tabSpacing.Max(1);
		    formattedText.Append(textToRender);

			// Strip all carriage returns.
			formattedText.Replace("\r", string.Empty);
			// Convert tabs to spaces.
		    formattedText.Replace("\t", new string(' ', tabSpacing));
		}
	}
}
