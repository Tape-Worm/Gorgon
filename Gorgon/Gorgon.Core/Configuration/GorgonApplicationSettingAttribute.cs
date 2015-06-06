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
using Gorgon.Core.Properties;

namespace Gorgon.Configuration
{
	/// <summary>
	/// An attribute defining whether a property is to be used as an application setting.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Apply this attribute to properties on a type inherited from <see cref="GorgonApplicationSettings"/> that represent application settings. If the setting property does not have this attribute applied, then 
	/// it will be skipped during serialization/deserialization.
	/// </para>
	/// <para>
	/// This attribute is only applicable to properties, and can be used on inherited types.
	/// </para>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class GorgonApplicationSettingAttribute
		: Attribute
	{
		#region Variables.
		// Default value;
		private object _default;
		// Flag to indicate that a default value is present.
		private bool _hasDefault;
		// The name of the section that the property value will be placed in.
		private readonly string _section;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether there's a default setting or not.
		/// </summary>
		/// <remarks>
		/// This will be set to <c>true</c> when the <see cref="DefaultValue"/> is assigned. Otherwise, it will return <c>false</c>.
		/// </remarks>
		public bool HasDefault
		{
			get
			{
				return _hasDefault;
			}
		}

		/// <summary>
		/// Property to return the section for the setting.
		/// </summary>
		/// <remarks>
		/// Sections are for formatting/namespace purposes in the XML file. For example, if the section name was called "Display", then any properties with the section name would appear under the "Display" section 
		/// element in the XML file.
		/// </remarks>
		public string Section
		{
			get
			{
				return _section;
			}
		}

		/// <summary>
		/// Property to set or return a new name for the setting.
		/// </summary>
		/// <remarks>
		/// This value is intended to provide an alias for a property name within the XML file. If it is not assigned, then the name of the property will be used instead when serializing/deserializing.
		/// </remarks>
		public string SettingName
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the default value for the property.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Assigns a default value to the setting. If it is not set, then the default value for the property type is used instead.
		/// </para>
		/// <para>
		/// This only affects value type, primitive, enum, <see cref="string"/> and <see cref="DateTime"/> property types. The default value for Collection types and array types are ignored.
		/// </para>
		/// </remarks>
		public object DefaultValue
		{
			get
			{
				return _default;
			}
			set
			{
				_default = value;
				_hasDefault = true;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonApplicationSettingAttribute"/> class.
		/// </summary>
		/// <param name="section">Section for the setting.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="section"/> parameter is <c>null</c> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="section"/> is empty.</exception>
		public GorgonApplicationSettingAttribute(string section)
		{
			if (section == null)
			{
				throw new ArgumentNullException("section");
			}

			if (string.IsNullOrWhiteSpace(section))
			{
				throw new ArgumentException(Resources.GOR_PARAMETER_MUST_NOT_BE_EMPTY, "section");
			}

			_section = section;
			_hasDefault = false;
		}
		#endregion
	}
}
