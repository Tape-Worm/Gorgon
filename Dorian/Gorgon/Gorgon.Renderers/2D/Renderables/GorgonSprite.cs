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
// Created: Thursday, February 16, 2012 9:19:30 AM
// 
#endregion

using System;
using System.IO;
using System.Linq;
using System.Text;
using GorgonLibrary.Animation;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using SlimMath;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// The corners of a rectangle.
	/// </summary>
	public enum RectangleCorner
	{
		/// <summary>
		/// Upper left hand corner of the rectangle.
		/// </summary>
		/// <remarks>This equates to vertex #0 in a sprite.</remarks>
		UpperLeft = 0,
		/// <summary>
		/// Upper right hand corner of the sprite.
		/// </summary>
		/// <remarks>This equates to vertex #1 in a sprite.</remarks>
		UpperRight = 1,
		/// <summary>
		/// Lower left hand corner of the sprite.
		/// </summary>
		/// <remarks>This equates to vertex #2 in a sprite.</remarks>
		LowerLeft = 2,
		/// <summary>
		/// Lower right hand corner of the sprite.
		/// </summary>
		/// <remarks>This equates to vertex #3 in a sprite.</remarks>
		LowerRight = 3
	}

	/// <summary>
	/// A sprite object.
	/// </summary>
	public class GorgonSprite
		: GorgonMoveable, IDeferredTextureLoad, I2DCollisionObject, IPersistedRenderable
	{
		#region Constants.
		/// <summary>
		/// Header for the Gorgon sprite file.
		/// </summary>		
		public const string FileHeader = "GORSPR20";
		#endregion

		#region Variables.
		private readonly float[] _corners = new float[4];						// Corners for the sprite.
		private string _textureName = string.Empty;								// Name of the texture for the sprite.
		private readonly Vector2[] _offsets;									// A list of vertex offsets.
		private bool _horizontalFlip;											// Flag to indicate that the sprite is flipped horizontally.
		private bool _verticalFlip;												// Flag to indicate that the sprite is flipped vertically.
		private Gorgon2DCollider _collider;										// Collider for the sprite.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the vertices need to be updated due to an offset.
		/// </summary>
		protected bool NeedsVertexOffsetUpdate
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		protected internal override int IndexCount
		{
			get
			{
				return 6;
			}
		}

		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		protected internal override PrimitiveType PrimitiveType
		{
			get
			{
				return Graphics.PrimitiveType.TriangleList;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite texture is flipped horizontally.
		/// </summary>
		/// <remarks>This only flips the texture region mapped to the sprite.  It does not affect the positioning or axis of the sprite.</remarks>
		public bool HorizontalFlip
		{
			get
			{
				return _horizontalFlip;
			}
			set
			{
				if (value == _horizontalFlip)
					return;

				_horizontalFlip = value;
				NeedsTextureUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite texture is flipped vertically.
		/// </summary>
		/// <remarks>This only flips the texture region mapped to the sprite.  It does not affect the positioning or axis of the sprite.</remarks>
		public bool VerticalFlip
		{
			get
			{
				return _verticalFlip;
			}
			set
			{
				if (value == _verticalFlip)
					return;

				_verticalFlip = value;
				NeedsTextureUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return a texture for the renderable.
		/// </summary>
        [AnimatedProperty()]
		public override GorgonTexture2D Texture
		{
			get
			{
				return base.Texture;
			}
			set
			{
				if (value == base.Texture)
				{
					return;
				}

				base.Texture = value;

				// Assign the texture name.
				_textureName = Texture != null ? Texture.Name : string.Empty;

				NeedsTextureUpdate = true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to transform the vertices.
		/// </summary>
		protected override void TransformVertices()
		{
			bool changed = false;	// Flag to indicate that the object changed.
			float posX1;			// Horizontal position 1.
			float posX2;			// Horizontal position 2.
			float posY1;			// Vertical position 1.
			float posY2;			// Vertical position 2.			

			posX1 = _corners[0];
			posX2 = _corners[2];
			posY1 = _corners[1];
			posY2 = _corners[3];

			// Scale horizontally if necessary.
			if (Scale.X != 1.0f)
			{
				posX1 *= Scale.X;
				posX2 *= Scale.X;
				changed = true;
			}

			// Scale vertically.
			if (Scale.Y != 1.0f)
			{
				posY1 *= Scale.Y;
				posY2 *= Scale.Y;
				changed = true;
			}

			// Calculate rotation if necessary.
			if (Angle != 0.0f)
			{
				float angle = Angle.Radians();						// Angle in radians.
				float cosVal = angle.Cos();							// Cached cosine.
				float sinVal = angle.Sin();							// Cached sine.

				Vertices[0].Position.X = (posX1 * cosVal - posY1 * sinVal);
				Vertices[0].Position.Y = (posX1 * sinVal + posY1 * cosVal);

				Vertices[1].Position.X = (posX2 * cosVal - posY1 * sinVal);
				Vertices[1].Position.Y = (posX2 * sinVal + posY1 * cosVal);

				Vertices[2].Position.X = (posX1 * cosVal - posY2 * sinVal);
				Vertices[2].Position.Y = (posX1 * sinVal + posY2 * cosVal);

				Vertices[3].Position.X = (posX2 * cosVal - posY2 * sinVal);
				Vertices[3].Position.Y = (posX2 * sinVal + posY2 * cosVal);

				changed = true;
			}
			else
			{
				Vertices[0].Position.X = posX1;
				Vertices[0].Position.Y = posY1;
				Vertices[1].Position.X = posX2;
				Vertices[1].Position.Y = posY1;
				Vertices[2].Position.X = posX1;
				Vertices[2].Position.Y = posY2;
				Vertices[3].Position.X = posX2;
				Vertices[3].Position.Y = posY2;
			}

			// Translate.
			if (Position.X != 0.0f)
			{
				Vertices[0].Position.X += Position.X;
				Vertices[1].Position.X += Position.X;
				Vertices[2].Position.X += Position.X;
				Vertices[3].Position.X += Position.X;
				changed = true;
			}

			if (Position.Y != 0.0f)
			{
				Vertices[0].Position.Y += Position.Y;
				Vertices[1].Position.Y += Position.Y;
				Vertices[2].Position.Y += Position.Y;
				Vertices[3].Position.Y += Position.Y;
				changed = true;
			}

			// Apply depth to the sprite.
			if (Depth != 0.0f)
			{
				Vertices[0].Position.Z = Depth;
				Vertices[1].Position.Z = Depth;
				Vertices[2].Position.Z = Depth;
				Vertices[3].Position.Z = Depth;
			}

			for (int i = 0; i < Vertices.Length; i++)
			{
				if (Vertices[i].Position.X != _offsets[i].X)
				{
					Vertices[i].Position.X += _offsets[i].X;
					changed = true;
				}

				if (Vertices[i].Position.Y != _offsets[i].Y)
				{
					Vertices[i].Position.Y += _offsets[i].Y;
					changed = true;
				}
			}

			// Update the collider boundaries.
			if ((Collider != null) && (changed))
				Collider.UpdateFromCollisionObject();
		}

		/// <summary>
		/// Function to update the texture coordinates.
		/// </summary>
		protected override void UpdateTextureCoordinates()
		{
			// Calculate texture coordinates.
			Vector2 rightBottom = Vector2.Zero;
			Vector2 leftTop = Vector2.Zero;

			if (Texture == null)
			{
				Vertices[0].UV = Vertices[1].UV = Vertices[2].UV = Vertices[3].UV = Vector2.Zero;
				return;
			}

			if (!_horizontalFlip)
			{
				leftTop.X = TextureRegion.Left;
				rightBottom.X = TextureRegion.Right;
			}
			else
			{
				leftTop.X = TextureRegion.Right;
				rightBottom.X = TextureRegion.Left;
			}

			if (!_verticalFlip)
			{
				leftTop.Y = TextureRegion.Top;
				rightBottom.Y = TextureRegion.Bottom;
			}
			else
			{
				leftTop.Y = TextureRegion.Bottom;
				rightBottom.Y = TextureRegion.Top;
			}

			Vertices[0].UV = leftTop;
			Vertices[1].UV = new Vector2(rightBottom.X, leftTop.Y);
			Vertices[2].UV = new Vector2(leftTop.X, rightBottom.Y);
			Vertices[3].UV = rightBottom;
		}

		/// <summary>
		/// Function to update the vertices for the renderable.
		/// </summary>
		protected override void UpdateVertices()
		{
			_corners[0] = -Anchor.X;
			_corners[1] = -Anchor.Y;
			_corners[2] = Size.X - Anchor.X;
			_corners[3] = Size.Y - Anchor.Y;
		}

		/// <summary>
		/// Function to set an offset for a corner.
		/// </summary>
		/// <param name="corner">Corner of the sprite to set.</param>
		/// <param name="offset">Offset for the corner.</param>
		public void SetCornerOffset(RectangleCorner corner, Vector2 offset)
		{
			var index = (int)corner;

			if (_offsets[index] != offset)
			{
				_offsets[index] = offset;
				NeedsVertexOffsetUpdate = true;
			}
		}

		/// <summary>
		/// Function to retrieve an offset for a corner.
		/// </summary>
		/// <param name="corner">Corner of the sprite to retrieve the offset from.</param>
		/// <returns>The offset of the corner.</returns>
		public Vector2 GetCornerOffset(RectangleCorner corner)
		{
			return _offsets[(int)corner];
		}

		/// <summary>
		/// Function to set the color for a specific corner on the sprite.
		/// </summary>
		/// <param name="corner">Corner of the sprite to set.</param>
		/// <param name="color">Color to set.</param>
		public void SetCornerColor(RectangleCorner corner, GorgonColor color)
		{
			Vertices[(int)corner].Color = color;
		}

		/// <summary>
		/// Function to retrieve the color for a specific corner on the sprite.
		/// </summary>
		/// <param name="corner">Corner of the sprite to retrieve the color from.</param>
		/// <returns>The color on the specified corner of the sprite.</returns>
		public GorgonColor GetCornerColor(RectangleCorner corner)
		{
			return Vertices[(int)corner].Color;
		}

		/// <summary>
		/// Function to save the sprite into memory.
		/// </summary>
		/// <returns>A byte array containing the sprite data.</returns>
		public byte[] Save()
		{
			using (var stream = new MemoryStream())
			{
				Save(stream);

				return stream.ToArray();
			}			
		}

		/// <summary>
		/// Function to save the sprite to a file.
		/// </summary>
		/// <param name="filePath">Path to the file to write the sprite information into.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the filePath parameter is empty.</exception>
		public void Save(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("The parameter must not be empty.", filePath);
			}

			using (FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				Save(stream);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSprite" /> class.
		/// </summary>
		/// <param name="gorgon2D">The interface that owns this object.</param>
		/// <param name="name">Name of the sprite.</param>
		internal GorgonSprite(Gorgon2D gorgon2D, string name)
			: base(gorgon2D, name)
		{
			InitializeVertices(4);

			Size = Vector2.Zero;
			Color = GorgonColor.White;
			Angle = 0;
			Scale = new Vector2(1.0f);
			Position = Vector2.Zero;
			Texture = null;
			TextureRegion = System.Drawing.RectangleF.Empty;
			Anchor = Vector2.Zero;

			_offsets = new[] { 
				Vector2.Zero, 
				Vector2.Zero, 
				Vector2.Zero, 
				Vector2.Zero, 
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSprite"/> class.
		/// </summary>
		/// <param name="gorgon2D">The interface that owns this object.</param>
		/// <param name="name">Name of the sprite.</param>
		/// <param name="settings">Settings for the sprite.</param>
		internal GorgonSprite(Gorgon2D gorgon2D, string name, GorgonSpriteSettings settings)
			: this(gorgon2D, name)
		{
			Size = settings.Size;
			Color = settings.Color;
			Angle = settings.InitialAngle;
			// Ensure scale is not set to 0.
            if (settings.InitialScale.X == 0.0f)
            {
                settings.InitialScale = new Vector2(1.0f, settings.InitialScale.Y);
            }
            if (settings.InitialScale.Y == 0.0f)
            {
                settings.InitialScale = new Vector2(settings.InitialScale.X, 1.0f);
            }
			Scale = settings.InitialScale;
			Position = settings.InitialPosition;
			Texture = settings.Texture;
			TextureRegion = settings.TextureRegion;
			Anchor = settings.Anchor;
		}
		#endregion

		#region IPersisted2DRenderable
		/// <summary>
		/// Function to read the renderable data from a stream.
		/// </summary>
		/// <param name="stream">Open file stream containing the renderable data.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream" /> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream parameter is not opened for reading data.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the data in the stream does not contain valid renderable data, or contains a newer version of the renderable than Gorgon can handle.</exception>
		void IPersistedRenderable.Load(Stream stream)
		{		
			string currentHeader = string.Empty;			// Current header from the file.

			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanRead)
			{
				throw new IOException("Stream is not open for reading.");
			}

            if (stream.Length <= 0)
            {
                throw new ArgumentOutOfRangeException("stream", "The parameter length must be greater than 0.");
            }

            // Read the sprite in.
            using (var chunk = new GorgonChunkReader(stream))
            {
                if (!chunk.HasChunk(FileHeader))
                {
                    // Check to see if it's an older version of the Gorgon sprite data.
                    using (var oldReader = new GorgonBinaryReader(stream, true))
                    {
                        GorgonV1SpriteReader.LoadSprite(this, oldReader);
                    }
                    return;
                }
                else
                {
                    // Read in the file header.
					chunk.Begin(FileHeader);
                }
                
                chunk.Begin("SPRTDATA");
                Anchor = chunk.Read<Vector2>();
                Size = chunk.Read<Vector2>();
                HorizontalFlip = chunk.ReadBoolean();
                VerticalFlip = chunk.ReadBoolean();

                // Read vertex colors.
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i].Color = chunk.Read<GorgonColor>();
                }

                // Write vertex offsets.
                chunk.ReadRange<Vector2>(_offsets);
                chunk.End();

                // Read rendering information.
                chunk.Begin("RNDRDATA");
                CullingMode = chunk.Read<CullingMode>();
                AlphaTestValues = chunk.Read<GorgonRangeF>();
                Blending.AlphaOperation = chunk.Read<BlendOperation>();
                Blending.BlendOperation = chunk.Read<BlendOperation>();
                Blending.BlendFactor = chunk.Read<GorgonColor>();
                Blending.DestinationAlphaBlend = chunk.Read<BlendType>();
                Blending.DestinationBlend = chunk.Read<BlendType>();
                Blending.SourceAlphaBlend = chunk.Read<BlendType>();
                Blending.SourceBlend = chunk.Read<BlendType>();
                Blending.WriteMask = chunk.Read<ColorWriteMaskFlags>();
                DepthStencil.BackFace.ComparisonOperator = chunk.Read<ComparisonOperators>();
                DepthStencil.BackFace.DepthFailOperation = chunk.Read<StencilOperations>();
                DepthStencil.BackFace.FailOperation = chunk.Read<StencilOperations>();
                DepthStencil.BackFace.PassOperation = chunk.Read<StencilOperations>();
                DepthStencil.FrontFace.ComparisonOperator = chunk.Read<ComparisonOperators>();
                DepthStencil.FrontFace.DepthFailOperation = chunk.Read<StencilOperations>();
                DepthStencil.FrontFace.FailOperation = chunk.Read<StencilOperations>();
                DepthStencil.FrontFace.PassOperation = chunk.Read<StencilOperations>();
                DepthStencil.DepthBias = chunk.ReadInt32();
                DepthStencil.DepthComparison = chunk.Read<ComparisonOperators>();
                DepthStencil.DepthStencilReference = chunk.ReadInt32();
                DepthStencil.IsDepthWriteEnabled = chunk.ReadBoolean();
                DepthStencil.StencilReadMask = chunk.ReadByte();
                DepthStencil.StencilReadMask = chunk.ReadByte();
                chunk.End();

                // Read collider information.                    
                if (chunk.HasChunk("COLLIDER"))
                {
                    Type colliderType = null;
                    Gorgon2DCollider collider = null;

                    chunk.Begin("COLLIDER");
                    colliderType = Type.GetType(chunk.ReadString());
                    collider = Activator.CreateInstance(colliderType) as Gorgon2DCollider;
                    collider.ReadFromChunk(chunk);
                    chunk.End();
                }

                // Read texture information.
                chunk.Begin("TXTRDATA");
                TextureSampler.BorderColor = chunk.Read<GorgonColor>();
                TextureSampler.HorizontalWrapping = chunk.Read<TextureAddressing>();
                TextureSampler.VerticalWrapping = chunk.Read<TextureAddressing>();
                TextureSampler.TextureFilter = chunk.Read<TextureFilter>();
                DeferredTextureName = chunk.ReadString();
                TextureRegion = chunk.ReadRectangleF();
            }
		}

		/// <summary>
		/// Function to save the sprite data into a stream.
		/// </summary>
		/// <param name="stream">Stream that is used to write out the sprite data.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream parameter is not opened for writing data.</exception>
		public void Save(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanWrite)
			{
				throw new IOException("Stream is not open for writing.");
			}

            // Chunk the file.            
            using (var chunk = new GorgonChunkWriter(stream))
            {
                // Write anchor information.
				chunk.Begin(FileHeader);

				chunk.Begin("SPRTDATA");
				chunk.Write<Vector2>(Anchor);
				chunk.Write<Vector2>(Size);
				chunk.WriteBoolean(HorizontalFlip);
				chunk.WriteBoolean(VerticalFlip);
				
                // Write vertex colors.
				for (int i = 0; i < Vertices.Length; i++)
				{
					chunk.Write<GorgonColor>(Vertices[i].Color);
	    		}

                // Write vertex offsets.
                chunk.WriteRange<Vector2>(_offsets);
				chunk.End();

                // Write rendering information.
				chunk.Begin("RNDRDATA");                
                chunk.Write<CullingMode>(CullingMode);
                chunk.Write<GorgonRangeF>(AlphaTestValues);
                chunk.Write<BlendOperation>(Blending.AlphaOperation);
                chunk.Write<BlendOperation>(Blending.BlendOperation);
                chunk.Write<GorgonColor>(Blending.BlendFactor);
                chunk.Write<BlendType>(Blending.DestinationAlphaBlend);
                chunk.Write<BlendType>(Blending.DestinationBlend);
                chunk.Write<BlendType>(Blending.SourceAlphaBlend);
                chunk.Write<BlendType>(Blending.SourceBlend);
                chunk.Write<ColorWriteMaskFlags>(Blending.WriteMask);
				chunk.Write<ComparisonOperators>(DepthStencil.BackFace.ComparisonOperator);
				chunk.Write<StencilOperations>(DepthStencil.BackFace.DepthFailOperation);
				chunk.Write<StencilOperations>(DepthStencil.BackFace.FailOperation);
				chunk.Write<StencilOperations>(DepthStencil.BackFace.PassOperation);
				chunk.Write<ComparisonOperators>(DepthStencil.FrontFace.ComparisonOperator);
				chunk.Write<StencilOperations>(DepthStencil.FrontFace.DepthFailOperation);
				chunk.Write<StencilOperations>(DepthStencil.FrontFace.FailOperation);
				chunk.Write<StencilOperations>(DepthStencil.FrontFace.PassOperation);
				chunk.WriteInt32(DepthStencil.DepthBias);
				chunk.Write<ComparisonOperators>(DepthStencil.DepthComparison);
				chunk.WriteInt32(DepthStencil.DepthStencilReference);
				chunk.WriteBoolean(DepthStencil.IsDepthWriteEnabled);
				chunk.WriteByte(DepthStencil.StencilReadMask);
				chunk.WriteByte(DepthStencil.StencilWriteMask);
                chunk.End();

                // Write collider information.                    
                if (_collider != null)
                {
                    chunk.Begin("COLLIDER");
                    _collider.WriteToChunk(chunk);
                    chunk.End();
                }

                // Write texture information.
                if (!string.IsNullOrWhiteSpace(DeferredTextureName))
                {
                    chunk.Begin("TXTRDATA");
                    chunk.Write<GorgonColor>(TextureSampler.BorderColor);
                    chunk.Write<TextureAddressing>(TextureSampler.HorizontalWrapping);
                    chunk.Write<TextureAddressing>(TextureSampler.VerticalWrapping);
                    chunk.Write<TextureFilter>(TextureSampler.TextureFilter);
                    chunk.WriteString(DeferredTextureName);
                    chunk.WriteRectangle(TextureRegion);
                    chunk.End();
                }
            }
		}
		#endregion

		#region IDeferredTextureLoad Members
		/// <summary>
		/// Property to return the name of the texture bound to this image.
		/// </summary>
		/// <remarks>This is used to defer the texture assignment until it the texture with the specified name is loaded.</remarks>
		public string DeferredTextureName
		{
			get
			{
				if (Texture != null)
				{
					return Texture.Name;
				}

				// Check to see if the texture is loaded.
				if (!string.IsNullOrWhiteSpace(_textureName))
				{
					GetDeferredTexture();
				}

				return _textureName;
			}
			set
			{
				if (value == null)
					value = string.Empty;

				if (string.Equals(_textureName, value, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				_textureName = value;
				GetDeferredTexture();
			}
		}

		/// <summary>
		/// Function to assign a deferred texture.
		/// </summary>
		/// <remarks>
		/// Call this method to assign a texture that's been deferred.  If a sprite is created/loaded before its texture has been loaded, then the 
		/// sprite will just appear with the color assigned to it and no image.  To counteract this we can assign the <see cref="P:GorgonLibrary.Renderers.GorgonSprite.DeferredTextureName">DeferredTextureName</see> 
		/// property to the name of the texture.  Once the texture with the right name is loaded, call this method to get the sprite to update its texture value from the deferred name.
		/// <para>If loading a sprite from a data source, then this method will be called upon load.  If the texture is not bound successfully (i.e. Texture == null), then it will set the deferred name 
		/// to the texture name stored in the sprite data.</para>
		/// <para>If there are multiple textures with the same name, then the first texture will be chosen.</para>
		/// </remarks>
		public virtual void GetDeferredTexture()
		{
			if (string.IsNullOrEmpty(_textureName))
			{
				Texture = null;
				return;
			}

			Texture = (from texture in Gorgon2D.Graphics.GetTrackedObjectsOfType<GorgonTexture2D>()
							where (texture != null) && (string.Equals(texture.Name, _textureName, StringComparison.OrdinalIgnoreCase))
							select texture).FirstOrDefault();
			NeedsTextureUpdate = true;
		}
		#endregion

		#region I2DCollisionObject Members
		/// <summary>
		/// Property to set or return the collider that is assigned to the object.
		/// </summary>
		public Gorgon2DCollider Collider
		{
			get
			{
				return _collider;
			}
			set
			{
				if (value == _collider)
					return;

				if (value == null)
				{
					if (_collider != null)
						_collider.CollisionObject = null;

					return;
				}

				// Force a transform to get the latest vertices.
				value.CollisionObject = this;
				_collider = value;
				TransformVertices();
			}
		}

		/// <summary>
		/// Property to return the number of vertices to process.
		/// </summary>
		int I2DCollisionObject.VertexCount
		{
			get
			{
				// Always 4 for a sprite.
				return 4;
			}
		}

		/// <summary>
		/// Property to return a list of vertices to render.
		/// </summary>
		Gorgon2DVertex[] I2DCollisionObject.Vertices
		{
			get
			{
				return this.Vertices;
			}
		}
		#endregion
    }
}