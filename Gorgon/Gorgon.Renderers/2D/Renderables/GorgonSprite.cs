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
using System.Drawing;
using System.IO;
using System.Linq;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers.Properties;
using SlimMath;

namespace Gorgon.Renderers
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
		// SPRTDATA chunk.
		private const string SpriteDataChunk = "SPRTDATA";
		// RNDRDATA chunk.
		private const string RenderDataChunk = "RNDRDATA";
		// TXTRDATA chunk.
		private const string TextureDataChunk = "TXTRDATA";

		/// <summary>
		/// Header for the Gorgon sprite file.
		/// </summary>		
		public const string FileHeader = "GORSPR20";

        /// <summary>
        /// Default extension for sprite files.
        /// </summary>
        public const string DefaultExtension = ".gorSprite";
		#endregion

		#region Variables.
		private readonly Vector2[] _offsets;									// A list of vertex offsets.
		private Vector4 _corners = new Vector4(0);								// Corners for the sprite.
		private string _textureName = string.Empty;								// Name of the texture for the sprite.
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
				return PrimitiveType.TriangleList;
			}
		}

		/// <summary>
		/// Property to set or return whether this sprite is loaded from a version 1.x Gorgon sprite file.
		/// </summary>
		internal bool IsV1Sprite
		{
			get;
			set;
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
				{
					return;
				}

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
				{
					return;
				}

				_verticalFlip = value;
				NeedsTextureUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return a texture for the renderable.
		/// </summary>
        [AnimatedProperty]
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
			Vector4 corners = _corners;

			// Scale horizontally if necessary.
			// ReSharper disable CompareOfFloatsByEqualityOperator
			if (Scale.X != 1.0f)
			{
				corners.X *= Scale.X;
				corners.Z *= Scale.X;
			}

			// Scale vertically.
			if (Scale.Y != 1.0f)
			{
				corners.Y *= Scale.Y;
				corners.W *= Scale.Y;
			}

			// Calculate rotation if necessary.
			if (Angle != 0.0f)
			{
				float angle = Angle.ToRadians();						// Angle in radians.
				float cosVal = angle.Cos();							// Cached cosine.
				float sinVal = angle.Sin();							// Cached sine.

				Vertices[0].Position.X = (corners.X * cosVal - corners.Y * sinVal) + Position.X + _offsets[0].X;
				Vertices[0].Position.Y = (corners.X * sinVal + corners.Y * cosVal) + Position.Y + _offsets[0].Y;

				Vertices[1].Position.X = (corners.Z * cosVal - corners.Y * sinVal) + Position.X + _offsets[1].X;
				Vertices[1].Position.Y = (corners.Z * sinVal + corners.Y * cosVal) + Position.Y + _offsets[1].Y;

				Vertices[2].Position.X = (corners.X * cosVal - corners.W * sinVal) + Position.X + _offsets[2].X;
				Vertices[2].Position.Y = (corners.X * sinVal + corners.W * cosVal) + Position.Y + _offsets[2].Y;

				Vertices[3].Position.X = (corners.Z * cosVal - corners.W * sinVal) + Position.X + _offsets[3].X;
				Vertices[3].Position.Y = (corners.Z * sinVal + corners.W * cosVal) + Position.Y + _offsets[3].Y;

				Vertices[0].Angle = Vertices[1].Angle = Vertices[2].Angle = Vertices[3].Angle = angle;
			}
			else
			{
				Vertices[0].Position.X = corners.X + Position.X + _offsets[0].X;
				Vertices[0].Position.Y = corners.Y + Position.Y + _offsets[0].Y;
				Vertices[1].Position.X = corners.Z + Position.X + _offsets[1].X;
				Vertices[1].Position.Y = corners.Y + Position.Y + _offsets[1].Y;
				Vertices[2].Position.X = corners.X + Position.X + _offsets[2].X;
				Vertices[2].Position.Y = corners.W + Position.Y + _offsets[2].Y;
				Vertices[3].Position.X = corners.Z + Position.X + _offsets[3].X;
				Vertices[3].Position.Y = corners.W + Position.Y + _offsets[3].Y;
				Vertices[0].Angle = Vertices[1].Angle = Vertices[2].Angle = Vertices[3].Angle = 0;
			}

			// Apply depth to the sprite.
			Vertices[0].Position.Z = Depth;
			Vertices[1].Position.Z = Depth;
			Vertices[2].Position.Z = Depth;
			Vertices[3].Position.Z = Depth;

			// Update the collider boundaries.
			if (Collider != null)
			{
				Collider.UpdateFromCollisionObject();
			}
			// ReSharper restore CompareOfFloatsByEqualityOperator
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
			_corners.X = -Anchor.X;
			_corners.Y = -Anchor.Y;
			_corners.Z = Size.X - Anchor.X;
			_corners.W = Size.Y - Anchor.Y;
		}

		/// <summary>
		/// Function to set an offset for a corner.
		/// </summary>
		/// <param name="corner">Corner of the sprite to set.</param>
		/// <param name="offset">Offset for the corner.</param>
		public void SetCornerOffset(RectangleCorner corner, Vector2 offset)
		{
			var index = (int)corner;

			if (_offsets[index] == offset)
			{
				return;
			}

			_offsets[index] = offset;
			NeedsVertexOffsetUpdate = true;
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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the filePath parameter is empty.</exception>
		public void Save(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException(Resources.GOR2D_PARAMETER_MUST_NOT_BE_EMPTY, filePath);
			}

			using (FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				Save(stream);
			}
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <remarks>
		/// Please note that this doesn't draw the object to the target right away, but queues it up to be
		/// drawn when <see cref="Gorgon.Renderers.Gorgon2D.Render">Render</see> is called.
		/// </remarks>
		public override void Draw()
		{
			AddToRenderQueue();
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

			_offsets = new[] { 
				Vector2.Zero, 
				Vector2.Zero, 
				Vector2.Zero, 
				Vector2.Zero 
			};
		}
		#endregion

		#region IPersisted2DRenderable
		/// <summary>
		/// Function to read the renderable data from a stream.
		/// </summary>
		/// <param name="stream">Open file stream containing the renderable data.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream" /> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream parameter is not opened for reading data.</exception>
		/// <exception cref="GorgonException">Thrown when the data in the stream does not contain valid renderable data, or contains a newer version of the renderable than Gorgon can handle.</exception>
		void IPersistedRenderable.Load(Stream stream)
		{		
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanRead)
			{
				throw new IOException(Resources.GOR2D_STREAM_WRITE_ONLY);
			}

			// Check for the old sprite file format.
			long streamPosition = stream.Position;
			GorgonBinaryReader reader;

			using (reader = new GorgonBinaryReader(stream, true))
			{
				string headerVersion = reader.ReadString();
				stream.Position = streamPosition;

				// If the header matches with the old sprite header, then load that instead.
				if ((headerVersion.StartsWith("GORSPR", StringComparison.OrdinalIgnoreCase)) && (headerVersion.Length >= 7))
				{
					GorgonV1SpriteReader.LoadSprite(this, reader);
					return;
				}
			}

			IGorgonChunkFileReader spriteFile = new GorgonChunkFileReader(stream,
			                                                              new[]
			                                                              {
				                                                              FileHeader.ChunkID()
			                                                              });
			try
			{
				spriteFile.Open();

				// Get sprite data.
				reader = spriteFile.OpenChunk(SpriteDataChunk);

				Anchor = reader.ReadValue<Vector2>();
				Size = reader.ReadValue<Vector2>();
				HorizontalFlip = reader.ReadBoolean();
				VerticalFlip = reader.ReadBoolean();

				// Vertex colors.
				for (int i = 0; i < Vertices.Length; ++i)
				{
					Vertices[i].Color = new GorgonColor(reader.ReadInt32());
				}

				reader.ReadRange(_offsets);

				spriteFile.CloseChunk();

				reader = spriteFile.OpenChunk(RenderDataChunk);

				CullingMode = reader.ReadValue<CullingMode>();
				AlphaTestValues = reader.ReadValue<GorgonRangeF>();
				Blending.AlphaOperation = reader.ReadValue<BlendOperation>();
				Blending.BlendOperation = reader.ReadValue<BlendOperation>();
				Blending.BlendFactor = new GorgonColor(reader.ReadInt32());
				Blending.DestinationAlphaBlend = reader.ReadValue<BlendType>();
				Blending.DestinationBlend = reader.ReadValue<BlendType>();
				Blending.SourceAlphaBlend = reader.ReadValue<BlendType>();
				Blending.SourceBlend = reader.ReadValue<BlendType>();
				Blending.WriteMask = reader.ReadValue<ColorWriteMaskFlags>();
				DepthStencil.BackFace.ComparisonOperator = reader.ReadValue<ComparisonOperator>();
				DepthStencil.BackFace.DepthFailOperation = reader.ReadValue<StencilOperation>();
				DepthStencil.BackFace.FailOperation = reader.ReadValue<StencilOperation>();
				DepthStencil.BackFace.PassOperation = reader.ReadValue<StencilOperation>();
				DepthStencil.FrontFace.ComparisonOperator = reader.ReadValue<ComparisonOperator>();
				DepthStencil.FrontFace.DepthFailOperation = reader.ReadValue<StencilOperation>();
				DepthStencil.FrontFace.FailOperation = reader.ReadValue<StencilOperation>();
				DepthStencil.FrontFace.PassOperation = reader.ReadValue<StencilOperation>();
				DepthStencil.DepthBias = reader.ReadInt32();
				DepthStencil.DepthComparison = reader.ReadValue<ComparisonOperator>();
				DepthStencil.StencilReference = reader.ReadInt32();
				DepthStencil.IsDepthWriteEnabled = reader.ReadBoolean();
				DepthStencil.StencilReadMask = reader.ReadByte();
				DepthStencil.StencilReadMask = reader.ReadByte();

				spriteFile.CloseChunk();

				if (!spriteFile.Chunks.Contains(TextureDataChunk))
				{
					return;
				}

				reader = spriteFile.OpenChunk(TextureDataChunk);

				TextureSampler.BorderColor = new GorgonColor(reader.ReadInt32());
				TextureSampler.HorizontalWrapping = reader.ReadValue<TextureAddressing>();
				TextureSampler.VerticalWrapping = reader.ReadValue<TextureAddressing>();
				TextureSampler.TextureFilter = reader.ReadValue<TextureFilter>();
				DeferredTextureName = reader.ReadString();
				TextureRegion = new RectangleF(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

				spriteFile.CloseChunk();
			}
			finally
			{
				spriteFile.Close();
			}
		}

		/// <summary>
		/// Function to save the sprite data into a stream.
		/// </summary>
		/// <param name="stream">Stream that is used to write out the sprite data.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream parameter is not opened for writing data.</exception>
		public void Save(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanWrite)
			{
				throw new IOException(Resources.GOR2D_STREAM_READ_ONLY);
			}

			IGorgonChunkFileWriter spriteFile = new GorgonChunkFileWriter(stream, FileHeader.ChunkID());

			try
			{
				spriteFile.Open();

				GorgonBinaryWriter writer = spriteFile.OpenChunk(SpriteDataChunk);

				writer.WriteValue(Anchor);
				writer.WriteValue(Size);
				writer.Write(HorizontalFlip);
				writer.Write(VerticalFlip);

				// Write vertex colors.
				for (int i = 0; i < Vertices.Length; i++)
				{
					writer.Write(Vertices[i].Color.ToARGB());
				}

				// Write vertex offsets.
				writer.WriteRange(_offsets);

				spriteFile.CloseChunk();

				writer = spriteFile.OpenChunk(RenderDataChunk);

				writer.WriteValue(CullingMode);
				writer.WriteValue(AlphaTestValues);
				writer.WriteValue(Blending.AlphaOperation);
				writer.WriteValue(Blending.BlendOperation);
				writer.Write(Blending.BlendFactor.ToARGB());
				writer.WriteValue(Blending.DestinationAlphaBlend);
				writer.WriteValue(Blending.DestinationBlend);
				writer.WriteValue(Blending.SourceAlphaBlend);
				writer.WriteValue(Blending.SourceBlend);
				writer.WriteValue(Blending.WriteMask);
				writer.WriteValue(DepthStencil.BackFace.ComparisonOperator);
				writer.WriteValue(DepthStencil.BackFace.DepthFailOperation);
				writer.WriteValue(DepthStencil.BackFace.FailOperation);
				writer.WriteValue(DepthStencil.BackFace.PassOperation);
				writer.WriteValue(DepthStencil.FrontFace.ComparisonOperator);
				writer.WriteValue(DepthStencil.FrontFace.DepthFailOperation);
				writer.WriteValue(DepthStencil.FrontFace.FailOperation);
				writer.WriteValue(DepthStencil.FrontFace.PassOperation);
				writer.Write(DepthStencil.DepthBias);
				writer.WriteValue(DepthStencil.DepthComparison);
				writer.Write(DepthStencil.StencilReference);
				writer.Write(DepthStencil.IsDepthWriteEnabled);
				writer.Write(DepthStencil.StencilReadMask);
				writer.Write(DepthStencil.StencilWriteMask);

				spriteFile.CloseChunk();

				// Write texture information.
				if (string.IsNullOrWhiteSpace(DeferredTextureName))
				{
					return;
				}

				writer = spriteFile.OpenChunk(TextureDataChunk);

				writer.Write(TextureSampler.BorderColor.ToARGB());
				writer.WriteValue(TextureSampler.HorizontalWrapping);
				writer.WriteValue(TextureSampler.VerticalWrapping);
				writer.WriteValue(TextureSampler.TextureFilter);
				writer.Write(DeferredTextureName);
				writer.Write(TextureRegion.X);
				writer.Write(TextureRegion.Y);
				writer.Write(TextureRegion.Width);
				writer.Write(TextureRegion.Height);

				spriteFile.CloseChunk();
			}
			finally
			{
				spriteFile.Close();
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
				{
					value = string.Empty;
				}

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
		public void GetDeferredTexture()
		{
			if (string.IsNullOrEmpty(_textureName))
			{
				Texture = null;
				return;
			}

			Texture = (from texture in Gorgon2D.Graphics.GetTrackedObjectsOfType<GorgonTexture2D>()
							where (texture != null) && (string.Equals(texture.Name, _textureName, StringComparison.OrdinalIgnoreCase))
							select texture).FirstOrDefault();

			if ((IsV1Sprite) && (Texture != null))
			{
				// Convert the texture region to texel space.
				TextureRegion = new RectangleF(Texture.ToTexel(TextureOffset), Texture.ToTexel(TextureSize));
				IsV1Sprite = false;
			}

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
				{
					return;
				}

				if (value == null)
				{
					if (_collider != null)
					{
						_collider.CollisionObject = null;
					}

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
				return Vertices;
			}
		}
		#endregion
    }
}