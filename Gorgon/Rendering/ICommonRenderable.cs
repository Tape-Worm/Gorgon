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
// Created: Wednesday, September 26, 2007 9:19:45 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Interface containing common renderable properties.
	/// </summary>
	public interface ICommonRenderable
	{
		/// <summary>
		/// Property to set or return the horizontal wrapping mode to use.
		/// </summary>
		ImageAddressing HorizontalWrapMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the vertical wrapping mode to use.
		/// </summary>
		ImageAddressing VerticalWrapMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the type of smoothing for the sprites.
		/// </summary>
		Smoothing Smoothing
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the function used for alpha masking.
		/// </summary>
		CompareFunctions AlphaMaskFunction
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the alpha value used for alpha masking.
		/// </summary>
		int AlphaMaskValue
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the blending mode.
		/// </summary>
		BlendingModes BlendingMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the source blending operation.
		/// </summary>
		AlphaBlendOperation SourceBlend
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the destination blending operation.
		/// </summary>
		AlphaBlendOperation DestinationBlend
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to enable the use of the stencil buffer or not.
		/// </summary>
		bool StencilEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the reference value for the stencil buffer.
		/// </summary>
		int StencilReference
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the mask value for the stencil buffer.
		/// </summary>
		int StencilMask
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the operation for passing stencil values.
		/// </summary>
		StencilOperations StencilPassOperation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the operation for the failing stencil values.
		/// </summary>
		StencilOperations StencilFailOperation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the stencil operation for the failing depth values.
		/// </summary>
		StencilOperations StencilZFailOperation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the stencil comparison function.
		/// </summary>
		CompareFunctions StencilCompare
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether to use an index buffer or not.
		/// </summary>
		bool UseIndices
		{
			get;
		}

		/// <summary>
		/// Property to return the primitive style for the object.
		/// </summary>
		PrimitiveStyle PrimitiveStyle
		{
			get;
		}
				
		/// <summary>
		/// Property to set or return the wrapping mode to use.
		/// </summary>
		ImageAddressing WrapMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return a shader effect for this object.
		/// </summary>
		Shader Shader
		{
			get;
			set;
		}
	}
}
