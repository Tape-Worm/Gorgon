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
// Created: Saturday, July 02, 2011 12:10:08 PM
// 
#endregion

using Gorgon.Core;

namespace Gorgon.Input
{
	/// <summary>
	/// A specific property for the HID.
	/// </summary>
	public class GorgonCustomHIDProperty
		: GorgonNamedObject 
	{
		#region Variables.
		private object _value;			// Data for the property.
		#endregion

		#region Methods.
		/// <summary>
		/// Property to set the value for the property.
		/// </summary>
		/// <param name="value">Value of the property.</param>
		internal void SetValue(object value)
		{
			_value = value;
		}

		/// <summary>
		/// Function to return the value for the property.
		/// </summary>
		/// <returns>The value for the property.</returns>
		public object GetValue()
		{
			return _value;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCustomHIDProperty"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">Value to pass to the property.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonCustomHIDProperty(string name, object value)
			: base(name)
		{
			_value = value;
		}
		#endregion
	}
}
