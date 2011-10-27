#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Wednesday, September 26, 2007 9:19:45 PM
// 
#endregion

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Interface containing common renderable properties.
	/// </summary>
	public interface IRenderableStates
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
		/// Property to set or return the color of the border when the wrapping mode is set to Border.
		/// </summary>
		Color BorderColor
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
		/// Property to set or return whether to enable the depth buffer (if applicable) writing or not.
		/// </summary>
		bool DepthWriteEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the depth buffer (if applicable) testing comparison function.
		/// </summary>
		CompareFunctions DepthTestFunction
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return (if applicable) the depth buffer bias.
		/// </summary>
		float DepthBufferBias
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the culling mode.
		/// </summary>
		CullingMode CullingMode
		{
			get;
			set;
		}
	}
}
