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
	[AttributeUsage(AttributeTargets.Property, Inherited=true, AllowMultiple=false)]
	public sealed class ApplicationSettingAttribute
		: Attribute
	{
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
			private set;
		}

		/// <summary>
		/// Property to return the default value for the property.
		/// </summary>
		public object DefaultValue
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the type of the property.
		/// </summary>
		public Type PropertyType
		{
			get;
			private set;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationSettingAttribute"/> class.
		/// </summary>
		/// <param name="settingName">Name of the setting.</param>
		/// <param name="defaultValue">The default value for the property.</param>
		/// <param name="propertyType">Type of property.</param>		
		/// <param name="section">Section for the setting.</param>
		public ApplicationSettingAttribute(string settingName, object defaultValue, Type propertyType, string section)
		{
			SettingName = settingName;
			DefaultValue = defaultValue;
			PropertyType = propertyType;
			Section = section;
			HasDefault = true;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationSettingAttribute"/> class.
		/// </summary>
		/// <param name="settingName">Name of the setting.</param>
		/// <param name="defaultValue">The default value for the property.</param>
		/// <param name="propertyType">Type of property.</param>		
		public ApplicationSettingAttribute(string settingName, object defaultValue, Type propertyType)
			: this(settingName, defaultValue, propertyType, string.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationSettingAttribute"/> class.
		/// </summary>
		/// <param name="settingName">Name of the setting.</param>
		/// <param name="propertyType">Type of property.</param>		
		/// <param name="section">Section for the setting.</param>
		public ApplicationSettingAttribute(string settingName, Type propertyType, string section)
			: this(settingName, null, propertyType, section)
		{
			HasDefault = false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationSettingAttribute"/> class.
		/// </summary>
		/// <param name="settingName">Name of the setting.</param>
		/// <param name="propertyType">Type of property.</param>		
		public ApplicationSettingAttribute(string settingName, Type propertyType)
			: this(settingName, null, propertyType, string.Empty)
		{
			HasDefault = false;
		}
		#endregion
	}
}
