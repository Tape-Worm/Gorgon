#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Saturday, June 16, 2007 3:27:46 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Flobbster.Windows.Forms;

namespace GorgonLibrary.Graphics.Tools.PropBag
{
	/// <summary>
	/// Interface for the an object that has a property bag.
	/// </summary>
	public interface IPropertyBagObject
	{
		#region Properties.
		/// <summary>
		/// Property to return the property bag for the object.
		/// </summary>
		PropertyBag PropertyBag
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set a value for the property.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		void SetValue(object sender, PropertySpecEventArgs e);

		/// <summary>
		/// Function to set a value for the property.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		void GetValue(object sender, PropertySpecEventArgs e);
		#endregion
	}
}
