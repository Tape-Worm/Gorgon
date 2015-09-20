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
// Created: Saturday, September 19, 2015 9:27:59 PM
// 
#endregion

using System;

namespace Gorgon.Plugins
{
	/// <summary>
	/// An attribute to disallow a type from being picked up as a plug in.
	/// </summary>
	/// <remarks>
	/// Use this to exclude a type from the plug in enumeration process. This is handy in situations where a base plug in class is a concrete type (i.e. not abstract), and it is not beneficial to enumerate 
	/// this type as a plug in. 
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class ExcludeAsPluginAttribute
		: Attribute
	{
	}
}
