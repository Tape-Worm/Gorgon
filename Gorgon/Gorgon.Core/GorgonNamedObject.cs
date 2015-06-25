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
// Created: Tuesday, June 14, 2011 10:16:27 PM
// 
#endregion

using System;
using Gorgon.Core.Properties;

namespace Gorgon.Core
{
	/// <summary>
	/// Abstract implementation of the <see cref="IGorgonNamedObject"/> interface. 
	/// </summary>
	public abstract class GorgonNamedObject
		: IGorgonNamedObject
    {
        #region Variables.
		// The name.
		private string _name;
        #endregion

        #region Methods.
        /// <summary>
		/// Serves as a hash method for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="GorgonNamedObject"/>.
		/// </returns>
		public override int GetHashCode()
        {
	        return string.IsNullOrWhiteSpace(Name) ? 0 : 281.GenerateHash(Name);
        }

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="GorgonNamedObject"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="GorgonNamedObject"/>.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GOR_TOSTR_NAMEDOBJECT, Name);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonNamedObject"/> class.
		/// </summary>
		/// <param name="name">The name of this object.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonNamedObject(string name)
		{
			if (name == null)
			{
			    throw new ArgumentNullException("name");
			}

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.GOR_ERR_PARAMETER_MUST_NOT_BE_EMPTY, "name");
            }

			_name = name;
		}
		#endregion

		#region IGorgonNamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		public virtual string Name
		{
			get
			{
			    return _name;
			}
			protected set
			{
			    _name = value;
			}
		}
		#endregion
	}
}
