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
// Created: Tuesday, January 1, 2013 7:24:03 PM
// 
#endregion

using System;

namespace Gorgon.Examples
{
	/// <summary>
	/// Our abstract class for our plug in.
	/// </summary>
	/// <remarks>This will be a simple plug in interface to write text with a specified (hard coded in the plug in DLL) color.  
	/// We could have just as easily used an interface here, but an abstract class will work for our needs.
	/// 
	/// As you can see, it's just a basic abstract class, there's no plug in details anywhere and there isn't any need for it
	/// because all of the plug in specific information is stored in the actual plug in assembly DLL.  This way, we could implement
	/// this class in our application for default behaviour, and override that behaviour in our plug in class.
	/// </remarks>
	public abstract class TextColorWriter
	{
		#region Properties
		/// <summary>
		/// We'll use this property to advertise the text color.
		/// </summary>
		public abstract ConsoleColor TextColor
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// This function will output the text passed into it with the specified color.
		/// </summary>
		/// <param name="text">Text to write.</param>
		public void WriteString(string text)
		{
			Console.ForegroundColor = TextColor;
			Console.WriteLine(text);
		}
		#endregion
	}
}
