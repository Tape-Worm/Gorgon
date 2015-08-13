#region MIT
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
// Created: Wednesday, August 12, 2015 7:36:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Input
{
	/// <summary>
	/// A joystick interface.
	/// </summary>
	public interface IGorgonJoystick
	{
		/// <summary>
		/// Property to return whether the joystick is connected or not.
		/// </summary> 
		/// <remarks>
		/// <para>
		/// Joysticks may be registered with the system, and appear in the enumeration list provided by <see cref="IGorgonInputService.EnumerateJoysticks"/>, but they may not be connected to the system 
		/// at the time of enumeration. Thus, we have this property to ensure that we know when a joystick is connected to the system or not. 
		/// </para>
		/// <para>
		/// This property will update itself when a joystick is connected or disconnected.
		/// </para>
		/// </remarks>
		bool IsConnected
		{
			get;
		}
	}
}
