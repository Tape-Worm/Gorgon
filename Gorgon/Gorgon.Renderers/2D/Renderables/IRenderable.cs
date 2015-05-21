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
// Created: Saturday, February 25, 2012 4:34:44 PM
// 
#endregion

using System.Drawing;
using Gorgon.Graphics;
using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// Defines a renderable object and its states.
	/// </summary>
	public interface IRenderable
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the texture region.
		/// </summary>
		/// <remarks>This texture value is in texel space (0..1).</remarks>
		RectangleF TextureRegion
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the coordinates in the texture to use as a starting point for drawing.
		/// </summary>
		/// <remarks>You can use this property to scroll the texture in the renderable.
		/// <para>This texture value is in texel space (0..1).</para>
		/// </remarks>        
		Vector2 TextureOffset
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the scaling of the texture width and height.
		/// </summary>
		/// <remarks>This texture value is in texel space (0..1).</remarks>        
		Vector2 TextureSize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the vertex buffer binding for this renderable.
		/// </summary>
		GorgonVertexBufferBinding VertexBufferBinding
		{
			get;
		}

		/// <summary>
		/// Property to set or return the index buffer for this renderable.
		/// </summary>
		GorgonIndexBuffer IndexBuffer
		{
			get;
		}

		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		PrimitiveType PrimitiveType
		{
			get;
		}

		/// <summary>
		/// Property to return a list of vertices to render.
		/// </summary>
		Gorgon2DVertex[] Vertices
		{
			get;
		}

	    /// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		/// <remarks>This only matters when the renderable uses an index buffer.</remarks>
		int IndexCount
		{
			get;
		}

		/// <summary>
		/// Property to return the number of vertices to add to the base starting index.
		/// </summary>
		int BaseVertexCount
		{
			get;
		}

		/// <summary>
		/// Property to return the number of vertices for the renderable.
		/// </summary>
		int VertexCount
		{
			get;
		}

		/// <summary>
		/// Property to set or return depth/stencil buffer states for this renderable.
		/// </summary>
		GorgonRenderable.DepthStencilStates DepthStencil
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return advanced blending states for this renderable.
		/// </summary>
		GorgonRenderable.BlendState Blending
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return advanded texture sampler states for this renderable.
		/// </summary>
		GorgonRenderable.TextureSamplerState TextureSampler
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return pre-defined smoothing states for the renderable.
		/// </summary>
		/// <remarks>These modes are pre-defined smoothing states, to get more control over the smoothing, use the <see cref="Gorgon.Renderers.GorgonRenderable.TextureSamplerState.TextureFilter">TextureFilter</see> 
		/// property exposed by the <see cref="Gorgon.Renderers.GorgonRenderable.TextureSampler">TextureSampler</see> property.</remarks>
		SmoothingMode SmoothingMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return a pre-defined blending states for the renderable.
		/// </summary>
		/// <remarks>These modes are pre-defined blending states, to get more control over the blending, use the <see cref="GorgonRenderable.BlendState.SourceBlend">SourceBlend</see> 
		/// or the <see cref="Gorgon.Renderers.GorgonRenderable.BlendState.DestinationBlend">DestinationBlend</see> property which are exposed by the 
		/// <see cref="Gorgon.Renderers.GorgonRenderable.Blending">Blending</see> property.</remarks>
		BlendingMode BlendingMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the culling mode.
		/// </summary>
		/// <remarks>Use this to make a renderable two-sided.</remarks>
		CullingMode CullingMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the range of alpha values to reject on this renderable.
		/// </summary>
		/// <remarks>The alpha testing tests to see if an alpha value is between or equal to the values and rejects the pixel if it is not.
		/// <para>This value will not take effect until <see cref="P:GorgonLibrary.Renderers.Gorgon2D.IsAlphaTestEnabled">IsAlphaTestEnabled</see> is set to TRUE.</para>
		/// <para>Typically, performance is improved when alpha testing is turned on with a range of 0.  This will reject any pixels with an alpha of 0.</para>
		/// <para>Be aware that the default shaders implement alpha testing.  However, a custom shader will have to make use of the GorgonAlphaTest constant buffer 
		/// in order to take advantage of alpha testing.</para>
		/// </remarks>
		GorgonRangeF AlphaTestValues
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the opacity (Alpha channel) of the renderable object.
		/// </summary>
		/// <remarks>This will only return the alpha value for the first vertex of the renderable and consequently will set all the vertices to the same alpha value.</remarks>
		float Opacity
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the color for a renderable object.
		/// </summary>
		/// <remarks>This will only return the color for the first vertex of the renderable and consequently will set all the vertices to the same color.</remarks>
		GorgonColor Color
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return a texture for the renderable.
		/// </summary>
		GorgonTexture2D Texture
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to force an update to the renderable object.
		/// </summary>
		/// <remarks>Take care when calling this method repeatedly.  It will have a significant performance impact.</remarks>
		void Refresh();

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <remarks>Please note that this doesn't draw the object to the target right away, but queues it up to be 
		/// drawn when <see cref="Gorgon2D.Render">Render</see> is called.
		/// </remarks>
		void Draw();
		#endregion
	}
}
