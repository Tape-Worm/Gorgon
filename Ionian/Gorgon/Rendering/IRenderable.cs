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
// Created: Saturday, January 06, 2007 1:13:42 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Interface for renderable objects.
	/// </summary>
	public interface IRenderable
		: IRenderableStates
	{
		#region Properties.
		/// <summary>
		/// Property to set or return whether we inherit smoothing from the layer.
		/// </summary>
		bool InheritSmoothing
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit blending from the layer.
		/// </summary>
		bool InheritBlending
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the alpha mask function from the layer.
		/// </summary>
		bool InheritAlphaMaskFunction
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the alpha mask value from the layer.
		/// </summary>
		bool InheritAlphaMaskValue
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the horizontal wrapping from the layer.
		/// </summary>
		bool InheritHorizontalWrapping
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the vertical wrapping from the layer.
		/// </summary>
		bool InheritVerticalWrapping
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil enabled flag from the layer.
		/// </summary>
		bool InheritStencilEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil reference from the layer.
		/// </summary>
		bool InheritStencilReference
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil mask from the layer.
		/// </summary>
		bool InheritStencilMask
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil pass operation from the layer.
		/// </summary>
		bool InheritStencilPassOperation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil failed operation from the layer.
		/// </summary>
		bool InheritStencilFailOperation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil z-failed operation from the layer.
		/// </summary>
		bool InheritStencilZFailOperation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil compare from the layer.
		/// </summary>
		bool InheritStencilCompare
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the AABB for the object.
		/// </summary>
		Drawing.RectangleF AABB
		{
			get;
		}

		/// <summary>
		/// Property to return the name of the object.
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// Property to return the children of this object.
		/// </summary>
		RenderableChildren Children
		{
			get;
		}

		/// <summary>
		/// Property to set or return the color.
		/// </summary>
		Drawing.Color Color
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the opacity.
		/// </summary>
		byte Opacity
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the image that this object is bound with.
		/// </summary>
		Image Image
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the parent of this object.
		/// </summary>
		IRenderable Parent
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw the sprite.
		/// </summary>
		/// <param name="flush">TRUE to flush the buffers after drawing, FALSE to only flush on state change.</param>
		void Draw(bool flush);

		/// <summary>
		/// Function to update children.
		/// </summary>
		void UpdateChildren();

		/// <summary>
		/// Function to set the parent for this object.
		/// </summary>
		/// <param name="parent">Parent of this object.</param>
		void SetParent(IRenderable parent);
		#endregion
	}
}
