#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, May 02, 2012 10:18:52 PM
// 
#endregion

using System;

namespace Gorgon.Configuration
{
	/// <summary>
	/// An attribute defining whether a property is to be used as an application setting.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class GorgonApplicationSettingAttribute
		: Attribute
	{
		#region Variables.
		// Default value;
		private object _default;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether there's a default setting or not.
		/// </summary>
		public bool HasDefault
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the section for the setting.
		/// </summary>
		public string Section
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the name of the setting.
		/// </summary>
		public string SettingName
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the default value for the property.
		/// </summary>
		public object DefaultValue
		{
			get
			{
				return _default;
			}
			set
			{
				_default = value;
				HasDefault = true;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonApplicationSettingAttribute"/> class.
		/// </summary>
		/// <param name="section">Section for the setting.</param>
		public GorgonApplicationSettingAttribute(string section)
		{
			Section = section;
			HasDefault = false;
		}
		#endregion
	}
}
