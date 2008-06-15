#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Monday, June 09, 2008 8:53:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Extras.GUI
{
	/// <summary>
	/// Object representing a custom GUI skin.
	/// </summary>
	public class GUISkin
	{
		#region Variables.
		private Image _skinImage = null;						// Image to use for the skin.
		private GUIElementCollection _elements = null;			// Elements for the skin.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the image used for the skin.
		/// </summary>
		public Image SkinImage
		{
			get
			{
				return _skinImage;
			}
			set
			{
				_skinImage = value;
			}
		}

		/// <summary>
		/// Property to return the list of elements for the skin.
		/// </summary>
		public GUIElementCollection Elements
		{
			get
			{
				return _elements;
			}
		}
		#endregion

		#region Methods.

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GUISkin"/> class.
		/// </summary>
		/// <param name="skinImage">The skin image.</param>
		public GUISkin(Image skinImage)
		{
			SkinImage = skinImage;
			_elements = new GUIElementCollection(this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GUISkin"/> class.
		/// </summary>
		public GUISkin()
			: this(null)
		{
		}
		#endregion
	}
}
