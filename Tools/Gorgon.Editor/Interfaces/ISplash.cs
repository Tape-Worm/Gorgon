#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Monday, February 23, 2015 8:31:13 PM
// 
#endregion

using System;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// The interface for the splash screen.
	/// </summary>
	public interface ISplash
		: IDisposable
	{
		/// <summary>
		/// Property to set or return the text shown in the information label.
		/// </summary>
		string InfoText
		{
			get;
			set;
		}

		/// <summary>
		/// Function to fade the splash screen in or out.
		/// </summary>
		/// <param name="fadeIn">TRUE to fade in, FALSE to fade out.</param>
		/// <param name="time">Time, in milliseconds, for the fade.</param>
		void Fade(bool fadeIn, float time);

		/// <summary>
		/// Function to show the splash screen.
		/// </summary>
		void Show();
	}
}