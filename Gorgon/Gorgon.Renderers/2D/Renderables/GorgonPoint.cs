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
// Created: Saturday, February 25, 2012 4:11:13 PM
// 
#endregion

using System;
using System.Drawing;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Math;
using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// A renderable object for drawing a point on the screen.
	/// </summary>
	public class GorgonPoint
		: GorgonNamedObject, IRenderable, IMoveable
	{
		#region Variables.
		private GorgonRenderable.BlendState _blendState;						// Blending state.
		private GorgonRenderable.DepthStencilStates _depthState;				// Depth/stencil state.
		private readonly GorgonRenderable.TextureSamplerState _samplerState;	// Sampler state.
		private readonly Gorgon2DVertex[] _vertices;							// List of vertices.
		private Vector2 _pointSize = new Vector2(1);							// Point size.
	    private bool _isUnitPoint = true;                                       // Flag to indicate that the point size is set to 1.0.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the interface that created this point.
		/// </summary>
		public Gorgon2D Gorgon2D
		{
			get;
		}

		/// <summary>
		/// Property to set or return the size of the point.
		/// </summary>
		/// <remarks>This value cannot be less than 1.</remarks>
		[AnimatedProperty]
		public Vector2 PointThickness
		{
			get
			{
				return _pointSize;
			}
			set
			{
			    if (_pointSize == value)
			    {
			        return;
			    }

			    if (value.X < 1)
			    {
			        value.X = 1;
			    }

			    if (value.Y < 1)
			    {
			        value.Y = 1;
			    }

			    _pointSize = value;
			    _isUnitPoint = (_pointSize.X.EqualsEpsilon(1.0f)) && (_pointSize.Y.EqualsEpsilon(1.0f));
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to transform the vertices.
		/// </summary>
		private void TransformVertices()
		{
			_vertices[0].Position.X = Position.X;
			_vertices[0].Position.Y = Position.Y;
			_vertices[1].Position.X = Position.X + _pointSize.X;
			_vertices[1].Position.Y = Position.Y;
			_vertices[2].Position.X = Position.X;
			_vertices[2].Position.Y = Position.Y + _pointSize.Y;
			_vertices[3].Position.X = Position.X + _pointSize.X;
			_vertices[3].Position.Y = Position.Y + _pointSize.Y;

		    _vertices[3].Position.Z = _vertices[2].Position.Z = _vertices[1].Position.Z = _vertices[0].Position.Z = Depth;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPoint"/> class.
		/// </summary>
		/// <param name="gorgon2D">Gorgon interface that owns this renderable.</param>
		/// <param name="name">The name of the point.</param>
		internal GorgonPoint(Gorgon2D gorgon2D, string name)
			: base(name)
		{
			CullingMode = CullingMode.Back;
			Gorgon2D = gorgon2D;
			_depthState = new GorgonRenderable.DepthStencilStates();
			_blendState = new GorgonRenderable.BlendState();
			_samplerState = new GorgonRenderable.TextureSamplerState();
			_vertices = new []
			{
				new Gorgon2DVertex
				{
					Position = new Vector4(0, 0, 0, 1.0f)
				},
				new Gorgon2DVertex
				{
					Position = new Vector4(0, 0, 0, 1.0f)
				},
				new Gorgon2DVertex
				{
					Position = new Vector4(0, 0, 0, 1.0f)
				},
				new Gorgon2DVertex
				{
					Position = new Vector4(0, 0, 0, 1.0f)
				}
			};
		}
		#endregion

		#region IRenderable Members
		/// <summary>
		/// Property to set or return the vertex buffer binding for this renderable.
		/// </summary>
		GorgonVertexBufferBinding IRenderable.VertexBufferBinding => Gorgon2D.DefaultVertexBufferBinding;

		/// <summary>
		/// Property to set or return the index buffer for this renderable.
		/// </summary>
		GorgonIndexBuffer IRenderable.IndexBuffer => _isUnitPoint ? null : Gorgon2D.DefaultIndexBuffer;

		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		PrimitiveType IRenderable.PrimitiveType => _isUnitPoint ? PrimitiveType.PointList : PrimitiveType.TriangleList;

		/// <summary>
		/// Property to return a list of vertices to render.
		/// </summary>
		Gorgon2DVertex[] IRenderable.Vertices => _vertices;

		/// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		int IRenderable.IndexCount => _isUnitPoint ? 0 : 6;

		/// <summary>
		/// Property to set or return the number of vertices to add to the base starting index.
		/// </summary>
		int IRenderable.BaseVertexCount => _isUnitPoint ? 1 : 0;

		/// <summary>
		/// Property to return the number of vertices for the renderable.
		/// </summary>
		int IRenderable.VertexCount => _isUnitPoint ? 1 : 4;

		/// <summary>
		/// Property to set or return depth/stencil buffer states for this renderable.
		/// </summary>
		public GorgonRenderable.DepthStencilStates DepthStencil
		{
			get 
			{
				return _depthState;
			}
			set
			{
			    if (value == null)
			    {
			        return;
			    }

			    _depthState = value;
			}
		}

		/// <summary>
		/// Property to set or return advanced blending states for this renderable.
		/// </summary>
		public GorgonRenderable.BlendState Blending
		{
			get 
			{
				return _blendState;
			}
			set
			{
			    if (value == null)
			    {
			        return;
			    }

			    _blendState = value;
			}
		}

		/// <summary>
		/// Property to set or return a pre-defined blending states for the renderable.
		/// </summary>
		public BlendingMode BlendingMode
		{
			get
			{
			    if ((Blending.SourceBlend == BlendType.One)
			        && (Blending.DestinationBlend == BlendType.Zero))
			    {
			        return BlendingMode.None;
			    }

			    if (Blending.SourceBlend == BlendType.SourceAlpha)
				{
				    if (Blending.DestinationBlend == BlendType.InverseSourceAlpha)
				    {
				        return BlendingMode.Modulate;
				    }
				    if (Blending.DestinationBlend == BlendType.One)
				    {
				        return BlendingMode.Additive;
				    }
				}

			    if ((Blending.SourceBlend == BlendType.One)
			        && (Blending.DestinationBlend == BlendType.InverseSourceAlpha))
			    {
			        return BlendingMode.PreMultiplied;
			    }

			    if ((Blending.SourceBlend == BlendType.InverseDestinationColor)
			        && (Blending.DestinationBlend == BlendType.InverseSourceColor))
			    {
			        return BlendingMode.Inverted;
			    }

			    return BlendingMode.Custom;
			}
			set
			{
				switch (value)
				{
					case BlendingMode.Additive:
						Blending.SourceBlend = BlendType.SourceAlpha;
						Blending.DestinationBlend = BlendType.One;
						break;
					case BlendingMode.Inverted:
						Blending.SourceBlend = BlendType.InverseDestinationColor;
						Blending.DestinationBlend = BlendType.InverseSourceColor;
						break;
					case BlendingMode.Modulate:
						Blending.SourceBlend = BlendType.SourceAlpha;
						Blending.DestinationBlend = BlendType.InverseSourceAlpha;
						break;
					case BlendingMode.PreMultiplied:
						Blending.SourceBlend = BlendType.One;
						Blending.DestinationBlend = BlendType.InverseSourceAlpha;
						break;
					case BlendingMode.None:
						Blending.SourceBlend = BlendType.One;
						Blending.DestinationBlend = BlendType.Zero;
						break;
				}
			}
		}

		/// <summary>
		/// Property to set or return the culling mode.
		/// </summary>
		public CullingMode CullingMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the range of alpha values to reject on this renderable.
		/// </summary>
		public GorgonRangeF AlphaTestValues
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the opacity (Alpha channel) of the renderable object.
		/// </summary>
		[AnimatedProperty]
		public float Opacity
		{
			get
			{
				return _vertices[0].Color.Alpha;
			}
			set
			{
                GorgonColor.SetAlpha(ref _vertices[3].Color, value, out _vertices[3].Color);
                GorgonColor.SetAlpha(ref _vertices[2].Color, value, out _vertices[2].Color);
                GorgonColor.SetAlpha(ref _vertices[1].Color, value, out _vertices[1].Color);
                GorgonColor.SetAlpha(ref _vertices[0].Color, value, out _vertices[0].Color);
			}
		}

		/// <summary>
		/// Property to set or return the color for a renderable object.
		/// </summary>
		[AnimatedProperty]
		public GorgonColor Color
		{
			get
			{
				return _vertices[0].Color;
			}
			set
			{
				_vertices[3].Color = _vertices[2].Color = _vertices[1].Color = _vertices[0].Color = value;
			}
		}

		/// <summary>
		/// Property to set or return advanded texture sampler states for this renderable.
		/// </summary>
		GorgonRenderable.TextureSamplerState IRenderable.TextureSampler
		{
			get
			{
				return _samplerState;
			}
			set
			{				
			}
		}

		/// <summary>
		/// Property to set or return pre-defined smoothing states for the renderable.
		/// </summary>
		SmoothingMode IRenderable.SmoothingMode
		{
			get
			{
				return SmoothingMode.None;
			}
			set
			{				
			}
		}

		/// <summary>
		/// Property to set or return a texture for the renderable.
		/// </summary>
		GorgonTexture2D IRenderable.Texture
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the texture region.
		/// </summary>
		RectangleF IRenderable.TextureRegion
		{
			get
			{
				return RectangleF.Empty;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the coordinates in the texture to use as a starting point for drawing.
		/// </summary>
		Vector2 IRenderable.TextureOffset
		{
			get
			{
				return Vector2.Zero;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the scaling of the texture width and height.
		/// </summary>
		Vector2 IRenderable.TextureSize
		{
			get
			{
				return Vector2.Zero;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Function to force an update to the renderable object.
		/// </summary>
		/// <remarks>
		/// Take care when calling this method repeatedly.  It will have a significant performance impact.
		/// </remarks>
		public void Refresh()
		{
			// Do nothing since we don't care about redundant calls.
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <remarks>Please note that this doesn't draw the object to the target right away, but queues it up to be 
		/// drawn when <see cref="Gorgon.Renderers.Gorgon2D.Render">Render</see> is called.
		/// </remarks>
		public void Draw()
		{
			TransformVertices();
			Gorgon2D.AddRenderable(this);
		}
		#endregion

		#region IMoveable Members
		/// <summary>
		/// Property to set or return the angle of rotation (in degrees) for a renderable.
		/// </summary>
		float IMoveable.Angle
		{
			get
			{
				return 0;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the scale of the renderable.
		/// </summary>
		Vector2 IMoveable.Scale
		{
			get
			{
				return new Vector2(1);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the anchor point of the renderable.
		/// </summary>
		Vector2 IMoveable.Anchor
		{
			get
			{
				return Vector2.Zero;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the size of the renderable.
		/// </summary>
		Vector2 IMoveable.Size
		{
			get
			{
				return PointThickness;
			}
			set
			{
				PointThickness = value;
			}
		}

		/// <summary>
		/// Property to set or return the depth buffer depth for the point.
		/// </summary>
		[AnimatedProperty]
		public float Depth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the position of the point.
		/// </summary>
		[AnimatedProperty]
		public Vector2 Position
		{
			get;
			set;
		}
		#endregion
	}
}
