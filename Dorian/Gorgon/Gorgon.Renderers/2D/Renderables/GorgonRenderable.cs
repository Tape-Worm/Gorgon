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
// Created: Friday, February 17, 2012 6:11:50 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SlimMath;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// Renderable smoothing modes.
	/// </summary>
	public enum SmoothingMode
	{
		/// <summary>
		/// No smoothing.
		/// </summary>
		None = 0,
		/// <summary>
		/// Smooth both minified and magnified renderables.
		/// </summary>
		Smooth = 1,
		/// <summary>
		/// Smooth only minified renderables.
		/// </summary>
		SmoothMinify = 2,
		/// <summary>
		/// Smooth only magnified renderables.
		/// </summary>
		SmoothMagnify = 3,
		/// <summary>
		/// Custom smoothing set in the advanced texture sampler state.
		/// </summary>
		Custom = 32767
	}

	/// <summary>
	/// Renderable blending modes.
	/// </summary>
	public enum BlendingMode
	{
		/// <summary>
		/// No blending.
		/// </summary>
		None = 0,
		/// <summary>
		/// Modulated blending.
		/// </summary>
		Modulate = 1,
		/// <summary>
		/// Additive blending.
		/// </summary>
		Additive = 2,
		/// <summary>
		/// Inverted blending.
		/// </summary>
		Inverted = 3,
		/// <summary>
		/// Pre-multiplied alpha.
		/// </summary>
		PreMultiplied = 4,
		/// <summary>
		/// Custom blending set in the advanced blending state.
		/// </summary>
		Custom = 32767
	}

	/// <summary>
	/// A renderable object.
	/// </summary>
	/// <remarks>This is the base object for any object that can be drawn to a render target.</remarks>
	public abstract class GorgonRenderable
		: GorgonNamedObject, IRenderable
	{
		#region Classes.
		/// <summary>
		/// A texture sampler state.
		/// </summary>
		public class TextureSamplerState
		{
			#region Properties.
			/// <summary>
			/// Property to set or return the border color for areas outside of the texture.
			/// </summary>
			public virtual GorgonColor BorderColor
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the horizontal wrapping mode for areas outside of the texture.
			/// </summary>
			public virtual TextureAddressing HorizontalWrapping
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the vertical wrapping mode for areas outside of the texture.
			/// </summary>
			public virtual TextureAddressing VerticalWrapping
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the type of filtering for the texture.
			/// </summary>
			public virtual TextureFilter TextureFilter
			{
				get;
				set;
			}
			#endregion

			#region Constructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="TextureSamplerState"/> class.
			/// </summary>
			public TextureSamplerState()
			{
				BorderColor = new GorgonColor(0);
				HorizontalWrapping = TextureAddressing.Clamp;
				VerticalWrapping = TextureAddressing.Clamp;
				TextureFilter = TextureFilter.Point;
			}
			#endregion
		}

		/// <summary>
		/// A blending state.
		/// </summary>
		public class BlendState
		{
			#region Variables.
			private BlendType _sourceAlphaBlend = BlendType.One;												// Source alpha blend.
			private BlendType _destAlphaBlend = BlendType.Zero;													// Destination alpha blend.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to set or return the write mask to mask out specific channels of color (or alpha).
			/// </summary>
			public ColorWriteMaskFlags WriteMask
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the alpha blending operation.
			/// </summary>
			public BlendOperation AlphaOperation
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the blending operation.
			/// </summary>
			public BlendOperation BlendOperation
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the source alpha blending function.
			/// </summary>
			public BlendType SourceAlphaBlend
			{
				get
				{
					return _sourceAlphaBlend;
				}
				set
				{
#if DEBUG
				switch (value)
				{
					case BlendType.DestinationColor:
					case BlendType.InverseDestinationColor:
					case BlendType.InverseSecondarySourceColor:
					case BlendType.InverseSourceColor:
					case BlendType.SecondarySourceColor:
					case BlendType.SourceColor:
						throw new ArgumentException("Cannot use a color operation with an alpha blend function.");
				}
#endif
					_sourceAlphaBlend = value;
				}
			}

			/// <summary>
			/// Property to set or return the destination alpha blending function.
			/// </summary>
			public BlendType DestinationAlphaBlend
			{
				get
				{
					return _destAlphaBlend;
				}
				set
				{
#if DEBUG
				switch (value)
				{
					case BlendType.DestinationColor:
					case BlendType.InverseDestinationColor:
					case BlendType.InverseSecondarySourceColor:
					case BlendType.InverseSourceColor:
					case BlendType.SecondarySourceColor:
					case BlendType.SourceColor:
						throw new ArgumentException("Cannot use a color operation with an alpha blend function.");
				}
#endif
					_destAlphaBlend = value;
				}
			}

			/// <summary>
			/// Property to set or return the blend factor.
			/// </summary>
			/// <remarks>This is only valid when the <see cref="P:GorgonLibrary.Graphics.Renderers.GorgonRenderable.SourceBlend">SourceBlend</see> or the <see cref="P:GorgonLibrary.Graphics.Renderers.GorgonRenderable.DestinationBlend">DestinationBlend</see> are set to BlendFactor.</remarks>
			public GorgonColor BlendFactor
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the source blending function.
			/// </summary>
			public BlendType SourceBlend
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the destination blending function.
			/// </summary>
			public BlendType DestinationBlend
			{
				get;
				set;
			}
			#endregion

			#region Constructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="BlendState"/> class.
			/// </summary>
			public BlendState()
			{
				SourceBlend = BlendType.SourceAlpha;
				DestinationBlend = BlendType.InverseSourceAlpha;
				BlendFactor = new GorgonColor(0);
				BlendOperation = Graphics.BlendOperation.Add;
				AlphaOperation = Graphics.BlendOperation.Add;
				WriteMask = ColorWriteMaskFlags.All;
			}
			#endregion
		}

		/// <summary>
		/// Depth/Stencil buffer states for a renderable.
		/// </summary>
		public class DepthStencilStates
		{
			#region Classes.
			/// <summary>
			/// A stencil state.
			/// </summary>
			public class StencilState
			{
				#region Properties.
				/// <summary>
				/// Property to set or return the comparison operator.
				/// </summary>
				public ComparisonOperators ComparisonOperator
				{
					get;
					set;
				}

				/// <summary>
				/// Property to set or return the operation to perform if the depth test fails.
				/// </summary>
				public StencilOperations DepthFailOperation
				{
					get;
					set;
				}

				/// <summary>
				/// Property to set or return the operation to perform if the stencil test fails.
				/// </summary>
				public StencilOperations FailOperation
				{
					get;
					set;
				}

				/// <summary>
				/// Property to set or return the operation to perform if the stencil test passes.
				/// </summary>
				public StencilOperations PassOperation
				{
					get;
					set;
				}
				#endregion

				#region Constructor.
				/// <summary>
				/// Initializes a new instance of the <see cref="StencilState"/> class.
				/// </summary>
				internal StencilState()
				{
					ComparisonOperator = ComparisonOperators.Always;
					PassOperation = FailOperation = DepthFailOperation = StencilOperations.Keep;
				}
				#endregion
			}
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the stencil state for front facing polygons.
			/// </summary>
			public StencilState FrontFace
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the stencil state for back facing polygons.
			/// </summary>
			public StencilState BackFace
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to set or return the read mask for the stencil buffer and this renderable.
			/// </summary>
			public byte StencilReadMask
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the write mask for the stencil buffer and this renderable.
			/// </summary>
			public byte StencilWriteMask
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the depth bias for this renderable.
			/// </summary>
			/// <remarks>This value adds to a pixel when comparing depth.  This helps mitigate z-fighting between two objects sharing the same depth.</remarks>
			public int DepthBias
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the depth comparison function to use.
			/// </summary>
			public ComparisonOperators DepthComparison
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return whether this renderable can write to the depth buffer.
			/// </summary>
			/// <remarks>This value is only effective when <see cref="P:GorgonLibrary.Graphics.Renderers.Gorgon2D.IsDepthBufferEnabled">IsDepthBufferEnabled</see> is TRUE.
			/// <para>Note that the renderable will still take the depth buffer into account even when this is FALSE.  That is, it will read the depth buffer and mask 
			/// depth areas that are less than the current depth value.</para>
			/// </remarks>
			public bool IsDepthWriteEnabled
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the depth/stencil reference value for this renderable.
			/// </summary>
			public int DepthStencilReference
			{
				get;
				set;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="DepthStencilStates"/> class.
			/// </summary>
			public DepthStencilStates()
			{
				IsDepthWriteEnabled = true;
				DepthStencilReference = 0;
				DepthComparison = ComparisonOperators.Less;
				DepthBias = 0;
				StencilReadMask = 0xFF;
				StencilWriteMask = 0xFF;
				FrontFace = new StencilState();
				BackFace = new StencilState();
			}
			#endregion
		}
		#endregion

		#region Variables.
		private GorgonTexture2D _texture = null;															// Texture to use for the renderable.
		private DepthStencilStates _depthStencil = null;													// Depth stencil interface.
		private BlendState _blendState = null;																// Blending state.
		private TextureSamplerState _samplerState = null;													// Texture sampler state.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the renderable needs to adjust its dimensions.
		/// </summary>
		protected bool NeedsVertexUpdate
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the texture coordinates need updating.
		/// </summary>
		protected bool NeedsTextureUpdate
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the vertex buffer binding for this renderable.
		/// </summary>
		protected internal GorgonVertexBufferBinding VertexBufferBinding
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to set or return the index buffer for this renderable.
		/// </summary>
		protected internal virtual GorgonIndexBuffer IndexBuffer
		{
			get
			{
				return Gorgon2D.DefaultIndexBuffer;
			}
		}

		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		protected internal abstract PrimitiveType PrimitiveType
		{
			get;
		}

		/// <summary>
		/// Property to return a list of vertices to render.
		/// </summary>
		protected internal Gorgon2DVertex[] Vertices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		/// <remarks>This is only matters when the renderable uses an index buffer.</remarks>
		protected internal abstract int IndexCount
		{
			get;
		}

		/// <summary>
		/// Property to set or return the number of vertices to add to the base starting index.
		/// </summary>
		protected internal int BaseVertexCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the number of vertices for the renderable.
		/// </summary>
		protected internal int VertexCount
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to set or return depth/stencil buffer states for this renderable.
		/// </summary>
		public virtual DepthStencilStates DepthStencil
		{
			get
			{
				return _depthStencil;
			}
			set
			{
				if (value == null)
					return;

				_depthStencil = value;
			}
		}

		/// <summary>
		/// Property to set or return advanced blending states for this renderable.
		/// </summary>
		public virtual BlendState Blending
		{
			get
			{
				return _blendState;
			}
			set
			{
				if (value == null)
					return;

				_blendState = value;
			}
		}

		/// <summary>
		/// Property to set or return advanded texture sampler states for this renderable.
		/// </summary>
		public virtual TextureSamplerState TextureSampler
		{
			get
			{
				return _samplerState;
			}
			set
			{
				if (value == null)
					return;

				_samplerState = value;
			}
		}

		/// <summary>
		/// Property to set or return pre-defined smoothing states for the renderable.
		/// </summary>
		/// <remarks>These modes are pre-defined smoothing states, to get more control over the smoothing, use the <see cref="P:GorgonLibrary.Graphics.Renderers.GorgonRenderable.TextureSamplerState.TextureFilter.">TextureFilter</see> 
		/// property exposed by the <see cref="P:GorgonLibrary.Graphics.Renderers.GorgonRenderable.TextureSampler.">TextureSampler</see> property.</remarks>
		public virtual SmoothingMode SmoothingMode
		{
			get
			{
				switch(TextureSampler.TextureFilter)
				{
					case TextureFilter.Point:
						return SmoothingMode.None;
					case TextureFilter.Linear:
						return SmoothingMode.Smooth;
					case TextureFilter.MinLinear | TextureFilter.MipLinear:
						return SmoothingMode.SmoothMinify;
					case TextureFilter.MagLinear | TextureFilter.MipLinear:
						return SmoothingMode.SmoothMagnify;
					default:
						return SmoothingMode.Custom;
				}
			}
			set
			{
				switch (value)
				{
					case SmoothingMode.None:
						TextureSampler.TextureFilter = TextureFilter.Point;			
						break;
					case SmoothingMode.Smooth:
						TextureSampler.TextureFilter = TextureFilter.Linear;
						break;
					case SmoothingMode.SmoothMinify:
						TextureSampler.TextureFilter = TextureFilter.MinLinear | TextureFilter.MipLinear;
						break;
					case SmoothingMode.SmoothMagnify:
						TextureSampler.TextureFilter = TextureFilter.MagLinear | TextureFilter.MipLinear;
						break;
				}
			}		
		}
	
		/// <summary>
		/// Property to set or return a pre-defined blending states for the renderable.
		/// </summary>
		/// <remarks>These modes are pre-defined blending states, to get more control over the blending, use the <see cref="P:GorgonLibrary.Graphics.Renderers.GorgonRenderable.BlendingState.SourceBlend">SourceBlend</see> 
		/// or the <see cref="P:GorgonLibrary.Graphics.Renderers.GorgonRenderable.Blending.DestinationBlend">DestinationBlend</see> property which are exposed by the 
		/// <see cref="P:GorgonLibrary.Graphics.Renderers.GorgonRenderable.Blending">Blending</see> property.</remarks>
		public virtual BlendingMode BlendingMode
		{
			get
			{
				if ((Blending.SourceBlend == BlendType.One) && (Blending.DestinationBlend == BlendType.Zero))
					return Renderers.BlendingMode.None;

				if (Blending.SourceBlend == BlendType.SourceAlpha) 
				{	
					if (Blending.DestinationBlend == BlendType.InverseSourceAlpha)
						return Renderers.BlendingMode.Modulate;
					if (Blending.DestinationBlend == BlendType.One)
						return Renderers.BlendingMode.Additive;
				}

				if ((Blending.SourceBlend == BlendType.One) && (Blending.DestinationBlend == BlendType.InverseSourceAlpha))
					return Renderers.BlendingMode.PreMultiplied;

				if ((Blending.SourceBlend == BlendType.InverseDestinationColor) && (Blending.DestinationBlend == BlendType.InverseSourceColor))
					return Renderers.BlendingMode.Inverted;
				
				return Renderers.BlendingMode.Custom;
			}
			set
			{
				switch (value)
				{
					case Renderers.BlendingMode.Additive:
						Blending.SourceBlend = BlendType.SourceAlpha;
						Blending.DestinationBlend = BlendType.One;
						break;
					case Renderers.BlendingMode.Inverted:
						Blending.SourceBlend = BlendType.InverseDestinationColor;
						Blending.DestinationBlend = BlendType.InverseSourceColor;
						break;
					case Renderers.BlendingMode.Modulate:
						Blending.SourceBlend = BlendType.SourceAlpha;
						Blending.DestinationBlend = BlendType.InverseSourceAlpha;
						break;
					case Renderers.BlendingMode.PreMultiplied:
						Blending.SourceBlend = BlendType.One;
						Blending.DestinationBlend = BlendType.InverseSourceAlpha;
						break;
				}
			}
		}

		/// <summary>
		/// Property to set or return the culling mode.
		/// </summary>
		/// <remarks>Use this to make a renderable two-sided.</remarks>
		public virtual CullingMode CullingMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the range of alpha values to reject on this renderable.
		/// </summary>
		/// <remarks>The alpha testing tests to see if an alpha value is between or equal to the values and rejects the pixel if it is not.
		/// <para>This value will not take effect until <see cref="P:GorgonLibrary.Graphics.Renderers.Gorgon2D.IsAlphaTestEnabled">IsAlphaTestEnabled</see> is set to TRUE.</para>
		/// <para>Typically, performance is improved when alpha testing is turned on with a range of 0.  This will reject any pixels with an alpha of 0.</para>
		/// <para>Be aware that the default shaders implement alpha testing.  However, a custom shader will have to make use of the GorgonAlphaTest constant buffer 
		/// in order to take advantage of alpha testing.</para>
		/// </remarks>
		public virtual GorgonMinMaxF AlphaTestValues
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the opacity (Alpha channel) of the renderable object.
		/// </summary>
		/// <remarks>This will only return the alpha value for the first vertex of the renderable and consequently will set all the vertices to the same alpha value.</remarks>
		public virtual float Opacity
		{
			get
			{
				return Color.Alpha;
			}
			set
			{
				if (value != Color.Alpha)
					Color = new GorgonColor(Color.Red, Color.Green, Color.Blue, value);
			}
		}

		/// <summary>
		/// Property to set or return the color for a renderable object.
		/// </summary>
		/// <remarks>This will only return the color for the first vertex of the renderable and consequently will set all the vertices to the same color.</remarks>
		public virtual GorgonColor Color
		{
			get
			{
				return Vertices[0].Color;
			}
			set
			{
				GorgonColor current = Vertices[0].Color;

				if (value != current)
				{
					for (int i = 0; i < Vertices.Length; i++)
						Vertices[i].Color = value;
				}
			}
		}

		/// <summary>
		/// Property to set or return a texture for the renderable.
		/// </summary>
		public virtual GorgonTexture2D Texture
		{
			get
			{
				return _texture;
			}
			set
			{
				if (_texture != value)
				{
					_texture = value;

					NeedsTextureUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to return the Gorgon 2D interface that created this object.
		/// </summary>
		public Gorgon2D Gorgon2D
		{
			get;
			private set;
		}
		#endregion

		#region Methods.

		/// <summary>
		/// Function to set up any additional information for the renderable.
		/// </summary>
		protected abstract void InitializeCustomVertexInformation();

		/// <summary>
		/// Function to initialize the vertex 
		/// </summary>
		/// <param name="vertexCount">Number of vertices in this renderable object.</param>
		protected void InitializeVertices(int vertexCount)
		{
			VertexCount = vertexCount;

			Vertices = new Gorgon2DVertex[vertexCount];
			
			for (int i = 0; i < Vertices.Length; i++)
			{
				Vertices[i].Color = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f);
				Vertices[i].Position = new Vector4(0, 0, 0, 1.0f);
				Vertices[i].UV = Vector2.Zero;
			}

			InitializeCustomVertexInformation();
		}		

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <remarks>Please note that this doesn't draw the object to the target right away, but queues it up to be 
		/// drawn when <see cref="M:GorgonLibrary.Graphics.Renderers.Gorgon2D.Render">Render</see> is called.
		/// </remarks>
		public virtual void Draw()
		{
			Gorgon2D.AddRenderable(this);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderable"/> class.
		/// </summary>
		/// <param name="gorgon2D">Gorgon 2D interface.</param>
		/// <param name="name">The name.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="gorgon2D"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonRenderable(Gorgon2D gorgon2D, string name)
			: base(name)
		{			
			GorgonDebug.AssertNull<Gorgon2D>(gorgon2D, "gorgon2D");
						
			Gorgon2D = gorgon2D;
			CullingMode = Graphics.CullingMode.Back;
	
			VertexBufferBinding = gorgon2D.DefaultVertexBufferBinding;
			AlphaTestValues = GorgonMinMaxF.Empty;
			DepthStencil = new DepthStencilStates();
			Blending = new BlendState();
			TextureSampler = new TextureSamplerState();
			BaseVertexCount = 0;
		}
		#endregion

		#region IRenderable Members
		/// <summary>
		/// Property to set or return the vertex buffer binding for this renderable.
		/// </summary>
		GorgonVertexBufferBinding IRenderable.VertexBufferBinding
		{
			get
			{
				return VertexBufferBinding;
			}
		}

		/// <summary>
		/// Property to set or return the index buffer for this renderable.
		/// </summary>
		GorgonIndexBuffer IRenderable.IndexBuffer
		{
			get 
			{
				return IndexBuffer;				
			}
		}

		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		PrimitiveType IRenderable.PrimitiveType
		{
			get 
			{
				return PrimitiveType;
			}
		}

		/// <summary>
		/// Property to return a list of vertices to render.
		/// </summary>
		Gorgon2DVertex[] IRenderable.Vertices
		{
			get 
			{
				return Vertices;
			}
		}

		/// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		int IRenderable.IndexCount
		{
			get 
			{
				return IndexCount;
			}
		}

		/// <summary>
		/// Property to return the number of vertices to add to the base starting index.
		/// </summary>
		int IRenderable.BaseVertexCount
		{
			get
			{
				return BaseVertexCount;
			}
		}

		/// <summary>
		/// Property to return the number of vertices for the renderable.
		/// </summary>
		int IRenderable.VertexCount
		{
			get
			{
				return VertexCount;
			}
		}
		#endregion
	}
}
