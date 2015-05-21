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
// Created: Friday, July 15, 2011 6:22:48 AM
// 
#endregion

using Gorgon.Input.XInput;

namespace Gorgon.Input
{
	/// <summary>
	/// The entry point for the Xinput plug-in.
	/// </summary>
	public class GorgonXInputPlugIn
		: GorgonInputPlugIn
	{
		#region Methods.
		/// <summary>
		/// Function to perform the actual creation of the input factory object.
		/// </summary>
		/// <returns>
		/// The interface for the input factory.
		/// </returns>
		protected override GorgonInputFactory CreateFactory()
		{
			return new GorgonXInputFactory();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonXInputPlugIn"/> class.
		/// </summary>
		public GorgonXInputPlugIn()
			: base("Gorgon XBOX 360 controller input plug-in.")
		{
		}
		#endregion
	}
}
