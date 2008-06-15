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
// Created: Monday, June 09, 2008 8:54:17 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary.Graphics;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Extras.GUI
{
	/// <summary>
	/// Object representing an element from a GUI skin.
	/// </summary>
	public class GUIElement
		: NamedObject
	{
		#region Variables.
		private Sprite _elementSprite = null;			// Sprite to represent the element.
		private GUISkin _owner = null;					// Skin that owns this element.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the owner skin.
		/// </summary>
		public GUISkin Owner
		{
			get
			{
				return _owner;
			}
		}

		/// <summary>
		/// Property to set or return the size of the element.
		/// </summary>
		public Drawing.Rectangle Dimensions
		{
			get
			{
				return Drawing.Rectangle.Round(_elementSprite.ImageRegion);
			}
			set
			{
				_elementSprite.ImageRegion = value;
			}
		}

		/// <summary>
		/// Property to set or return the element color.
		/// </summary>
		public Drawing.Color Color
		{
			get
			{
				return _elementSprite.Color;
			}
			set
			{
				_elementSprite.Color = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Property to set or return the 
		/// </summary>
		/// <param name="skin"></param>
		internal void SetOwner(GUISkin skin)
		{
			if ((_owner != null) && (skin != null))
				throw new ArgumentException("This element is already bound to a skin.");

			_owner = skin;

			if (_owner != null)
				_elementSprite.Image = _owner.SkinImage;
			else
				_elementSprite.Image = null;
		}

		/// <summary>
		/// Function to return a clone of the sprite used by the element.
		/// </summary>
		/// <returns>Clone of the element sprite.</returns>
		public Sprite GetSprite()
		{
			return _elementSprite.Clone() as Sprite;
		}

		/// <summary>
		/// Function to draw the element.
		/// </summary>
		/// <param name="dimensions">Dimensions of the element sprite.</param>
		public void Draw(Drawing.Rectangle dimensions)
		{
			_elementSprite.Position = dimensions.Location;
			_elementSprite.ScaledDimensions = dimensions.Size;
			_elementSprite.Draw();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GUIElement"/> class.
		/// </summary>
		/// <param name="name">The name of the element.</param>
		/// <param name="elementCoordinates">Coordinates of the element.</param>
		/// <param name="color">The color of the element.</param>
		public GUIElement(string name, Drawing.Rectangle elementCoordinates, Drawing.Color color)
			: base(name)
		{
			_elementSprite = new Sprite(name + ".Sprite");
			_elementSprite.ImageOffset = elementCoordinates.Location;
			_elementSprite.Size = elementCoordinates.Size;
			_elementSprite.Color = color;
		}
		#endregion
	}
}
