#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Sunday, July 09, 2006 3:16:50 AM
//
// Code snippets submitted by:
//   - Cycor (GIF animation code based upon original code)
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Resources;
using System.Reflection;
using Drawing = System.Drawing;
using GorgonLibrary.Internal;
using GorgonLibrary.Serialization;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a sprite.
	/// </summary>
	public class Sprite
		: Renderable, ISerializable
	{
		#region Variables.
		private float[] _spriteCorners;									// Sprite corners.
		private Vector2D[] _vertexOffsets;								// Relative offsets for vertices.
		private Vector2D _imagePosition;								// Position within the image to start copying from.
		private string _spritePath;										// Path to the loaded/saved sprite.
		private bool _flipHorizontal = false;							// Flag to flip horizontally.
		private bool _flipVertical = false;								// Flag to flip vertically.
		private string _deferredImage = string.Empty;					// Name of the deferred image to bind.
		private BoundingCircle _boundCircle = BoundingCircle.Empty;		// Bounding circle.
		private bool _isResource = false;								// Flag to indicate that this object is an embedded resource.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the bounding circle of the sprite.
		/// </summary>
		public BoundingCircle BoundingCircle
		{
			get
			{
				if (this.IsAABBUpdated)
					UpdateAABB();
				return _boundCircle;
			}
		}

		/// <summary>
		/// Property to set or return the image that this object is bound with.
		/// </summary>
		public override Image Image
		{
			get
			{
				if ((_deferredImage != string.Empty) && (base.Image == null))
				{
					if (ImageCache.Images.Contains(_deferredImage))
						Image = ImageCache.Images[_deferredImage];
				}

				return base.Image;
			}
			set
			{
				_deferredImage = string.Empty;
				base.Image = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite is flipped horizontally or not.
		/// </summary>
		public bool HorizontalFlip
		{
			get
			{
				return _flipHorizontal;
			}
			set
			{
				_flipHorizontal = value;
				Refresh();
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite is flipped vertically or not.
		/// </summary>
		public bool VerticalFlip
		{
			get
			{
				return _flipVertical;
			}
			set
			{
				_flipVertical = value;
				Refresh();
			}
		}

		/// <summary>
		/// Property to return the primitive style for the object.
		/// </summary>
		public override PrimitiveStyle PrimitiveStyle
		{
			get
			{
				return PrimitiveStyle.TriangleList;
			}
		}

		/// <summary>
		/// Property to return whether to use an index buffer or not.
		/// </summary>
		public override bool UseIndices
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite color.
		/// </summary>
		public override Drawing.Color Color
		{
			get
			{
				return Drawing.Color.FromArgb(Vertices[0].ColorValue);
			}
			set
			{
				int colorInt = value.ToArgb();		// Integer representation of the color.

				Vertices[3].ColorValue = Vertices[2].ColorValue = Vertices[1].ColorValue = Vertices[0].ColorValue = colorInt;
			}
		}

		/// <summary>
		/// Property to set or return the opacity.
		/// </summary>
		public override int Opacity
		{
			get
			{
				return ((Vertices[0].ColorValue >> 24) & 0xFF);
			}
			set
			{
				value &= 0xFF;
				Vertices[0].ColorValue = (value << 24) | (Vertices[0].ColorValue & 0xFFFFFF);
				Vertices[1].ColorValue = (value << 24) | (Vertices[1].ColorValue & 0xFFFFFF);
				Vertices[2].ColorValue = (value << 24) | (Vertices[2].ColorValue & 0xFFFFFF);
				Vertices[3].ColorValue = (value << 24) | (Vertices[3].ColorValue & 0xFFFFFF);
			}
		}

		/// <summary>
		/// Property to set or return the axis of the sprite.
		/// </summary>
		public override Vector2D Axis
		{
			get
			{
				return base.Axis;
			}
			set
			{
				base.Axis = value;
				IsAABBUpdated = true;
				IsSizeUpdated = true;
				IsImageUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the offset within the source image to start drawing from.
		/// </summary>
		public override Vector2D ImageOffset
		{
			get
			{
				return _imagePosition;
			}
			set
			{
				// If we haven't changed, then do nothing.
				if (value == _imagePosition)
					return;

				_imagePosition = value;
				IsImageUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the region of the image that the sprite will use.
		/// </summary>
		public Drawing.RectangleF ImageRegion
		{
			get
			{
				return new Drawing.RectangleF(_imagePosition.X, _imagePosition.Y, Size.X, Size.Y);
			}
			set
			{
				_imagePosition.X = value.X;
				_imagePosition.Y = value.Y;
				SetSize(value.Width, value.Height);

				IsImageUpdated = true;
				IsAABBUpdated = true;
				IsSizeUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the scale of the sprite.
		/// </summary>
		public override Vector2D Scale
		{
			get
			{
				return base.Scale;
			}
			set
			{
				IsAABBUpdated = true;
				NotifyParent();

				base.Scale = value;

				if (Children.Count > 0)
                    ((Renderable)this).UpdateChildren();
			}
		}

		/// <summary>
		/// Property to set or return a uniform scale across the X and Y axis.
		/// </summary>
		public override float UniformScale
		{
			get
			{
				return Scale.X;
			}
			set
			{
				if (value == 0.0f)
					return;

				// Set the uniform scale.
				Scale = new Vector2D(value, value);
			}
		}

		/// <summary>
		/// Property to set or return the name of the sprite.
		/// </summary>
		public new string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return a renderable from a stream.
		/// </summary>
		/// <param name="name">Renderable name or path.</param>
		/// <param name="fileSystem">A file system that contains the renderable.</param>
		/// <param name="resources">A resource manager that is used to load the file(s).</param>
		/// <param name="stream">Stream that contains the object.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The object contained within the stream.</returns>
		private static Sprite SpriteFromStream(string name, FileSystem fileSystem, ResourceManager resources, Stream stream, bool isXML, Image alternateImage)
		{
			Serializer spriteSerializer = null;			// Sprite serializer.
			Sprite newSprite = null;					// New sprite object.
			string imageName = string.Empty;			// Image name.
			string spritePath = string.Empty;			// Path to the sprite.

            try
            {
                // Get the filename if this is a file stream.
                if (stream is FileStream)
                    spritePath = ((FileStream)stream).Name;
                else
                    spritePath = name;

                // Create the sprite object.
                newSprite = new Sprite(name);
                newSprite._spritePath = spritePath;
				newSprite._isResource = (resources != null);

                // Open the file for reading.
                if (isXML)
                    spriteSerializer = new XMLSerializer(newSprite, stream);
                else
                    spriteSerializer = new BinarySerializer(newSprite, stream);

                if (resources != null)
                    spriteSerializer.Parameters["ResourceManager"] = resources;

                // Don't close the underlying stream.
                spriteSerializer.DontCloseStream = true;

                // Set the image parameters.
                if (alternateImage != null)
                    spriteSerializer.Parameters["Image"] = alternateImage;

                spriteSerializer.Deserialize();
            }
			finally
			{
				if (spriteSerializer != null)
					spriteSerializer.Dispose();
				spriteSerializer = null;
			}

			return newSprite;
		}

		/// <summary>
		/// Function to update the dimensions for an object.
		/// </summary>
		protected override void UpdateDimensions()
		{
			// Resize the sprite.
			_spriteCorners[0] = -Axis.X;
			_spriteCorners[1] = -Axis.Y;
			if (Size.X >= 0)
				_spriteCorners[2] = Size.X - Axis.X;
			else
				_spriteCorners[2] = -Size.X - Axis.X;

			if (Size.Y >= 0)
				_spriteCorners[3] = Size.Y - Axis.Y;
			else
				_spriteCorners[3] = -Size.Y - Axis.Y;

			IsSizeUpdated = false;
		}

		/// <summary>
		/// Function to update the position of an object.
		/// </summary>
		protected override void UpdateTransform()
		{
			float posX1;		// Horizontal position 1.
			float posX2;		// Horizontal position 2.
			float posY1;		// Vertical position 1.
			float posY2;		// Vertical position 2.			

            posX1 = _spriteCorners[0];
            posX2 = _spriteCorners[2];
            posY1 = _spriteCorners[1];
            posY2 = _spriteCorners[3];

            // Scale horizontally if necessary.
			if (FinalScale.X != 1.0f)
			{
				posX1 *= FinalScale.X;
				posX2 *= FinalScale.X;
			}

			// Scale vertically.
			if (FinalScale.Y != 1.0f)
			{
				posY1 *= FinalScale.Y;
				posY2 *= FinalScale.Y;
			}

			// Calculate rotation if necessary.
			if (FinalRotation != 0.0f) 
			{
				float cosVal;		// Cached cosine.
				float sinVal;		// Cached sine.
				float angle;		// Angle in radians.

                angle = MathUtility.Radians(FinalRotation);
				cosVal = (float)Math.Cos(angle);
				sinVal = (float)Math.Sin(angle);

                Vertices[0].Position.X = (posX1 * cosVal - posY1 * sinVal);
                Vertices[0].Position.Y = (posX1 * sinVal + posY1 * cosVal);

                Vertices[1].Position.X = (posX2 * cosVal - posY1 * sinVal);
                Vertices[1].Position.Y = (posX2 * sinVal + posY1 * cosVal);

                Vertices[2].Position.X = (posX2 * cosVal - posY2 * sinVal);
                Vertices[2].Position.Y = (posX2 * sinVal + posY2 * cosVal);

                Vertices[3].Position.X = (posX1 * cosVal - posY2 * sinVal);
                Vertices[3].Position.Y = (posX1 * sinVal + posY2 * cosVal);
            }
			else
			{
				Vertices[0].Position.X = posX1;
				Vertices[0].Position.Y = posY1;
				Vertices[1].Position.X = posX2;
				Vertices[1].Position.Y = posY1;
                Vertices[2].Position.X = posX2;
				Vertices[2].Position.Y = posY2;
                Vertices[3].Position.X = posX1;
				Vertices[3].Position.Y = posY2;
			}

			// Translate.
			if (FinalPosition.X != 0.0f)
			{
                Vertices[0].Position.X += FinalPosition.X;
                Vertices[1].Position.X += FinalPosition.X;
                Vertices[2].Position.X += FinalPosition.X;
                Vertices[3].Position.X += FinalPosition.X;
			}

			if (FinalPosition.Y != 0.0f)
			{
                Vertices[0].Position.Y += FinalPosition.Y;
                Vertices[1].Position.Y += FinalPosition.Y;
                Vertices[2].Position.Y += FinalPosition.Y;
                Vertices[3].Position.Y += FinalPosition.Y;
			}

			// Adjust vertex offsets.
            Vertices[0].Position.Z = -Depth;
			Vertices[1].Position.Z = -Depth;
			Vertices[2].Position.Z = -Depth;
			Vertices[3].Position.Z = -Depth;

			Vertices[0].Position.X += _vertexOffsets[0].X;
            Vertices[0].Position.Y += _vertexOffsets[0].Y;
            Vertices[1].Position.X += _vertexOffsets[1].X;
            Vertices[1].Position.Y += _vertexOffsets[1].Y;
            Vertices[2].Position.X += _vertexOffsets[2].X;
            Vertices[2].Position.Y += _vertexOffsets[2].Y;
            Vertices[3].Position.X += _vertexOffsets[3].X;
            Vertices[3].Position.Y += _vertexOffsets[3].Y;
		}

		/// <summary>
		/// Function to update the source image positioning.
		/// </summary>
		protected override void UpdateImageLayer()
		{
			float tu;		// Starting horizontal position.
			float tv;		// Starting vertical position.
			float sizetu;	// Width.
			float sizetv;	// Height.

			if (Image == null)
				return;

			// Initialize texture coordinates.
			if (!_flipHorizontal)
				tu = (_imagePosition.X + 0.5f) / Image.ActualWidth;
			else
				tu = (_imagePosition.X + Size.X - 0.5f) / Image.ActualWidth;
			if (!_flipVertical)
				tv = (_imagePosition.Y + 0.5f) / Image.ActualHeight;
			else
				tv = (_imagePosition.Y + Size.Y - 0.5f) / Image.ActualHeight;

			if (!_flipHorizontal)
				sizetu = (_imagePosition.X + Size.X) / Image.ActualWidth;
			else
				sizetu = (_imagePosition.X) / Image.ActualWidth;

			if (!_flipVertical)
				sizetv = (_imagePosition.Y + Size.Y) / Image.ActualHeight;
			else
				sizetv = (_imagePosition.Y) / Image.ActualHeight;

			Vertices[0].TextureCoordinates = new Vector2D(tu, tv);
			Vertices[1].TextureCoordinates = new Vector2D(sizetu, tv);
			Vertices[2].TextureCoordinates = new Vector2D(sizetu, sizetv);
			Vertices[3].TextureCoordinates = new Vector2D(tu, sizetv);
			IsImageUpdated = false;
		}

		/// <summary>
		/// Function to return the number of vertices for this object.
		/// </summary>
		/// <returns>
		/// An array of vertices used for this renderable.
		/// </returns>
		protected internal override BatchVertex[] GetVertices()
		{
			BatchVertex[] result = new BatchVertex[Vertices.Length];

			if (IsSizeUpdated)
				UpdateDimensions();
			if (IsImageUpdated)
				UpdateImageLayer();

			// Update the AABB.			
			if (IsAABBUpdated)
				UpdateAABB();
			else
				UpdateTransform();

			for (int i = 0; i < Vertices.Length; i++)
			{
				result[i].Vertex = Vertices[i];
				result[i].Image = Image;
			}

			return result;
		}

		/// <summary>
		/// Function to return a sprite from a stream.
		/// </summary>
		/// <param name="stream">Stream that contains the sprite.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The sprite contained within the stream.</returns>
		public static Sprite FromStream(Stream stream, bool isXML, Image alternateImage)
		{
			return SpriteFromStream("@SpriteObject.", null, null, stream, isXML, alternateImage);
		}

		/// <summary>
		/// Function to return a sprite from a stream.
		/// </summary>
		/// <param name="stream">Stream that contains the sprite.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>		
		/// <returns>The sprite contained within the stream.</returns>
		public static Sprite FromStream(Stream stream, bool isXML)
		{
			return SpriteFromStream("@SpriteObject.", null, null, stream, isXML, null);
		}

		/// <summary>
		/// Function to return a binary sprite from a stream.
		/// </summary>
		/// <param name="stream">Stream that contains the sprite.</param>
		/// <returns>The sprite contained within the stream.</returns>
		public static Sprite FromStream(Stream stream)
		{
			return SpriteFromStream("@SpriteObject.", null, null, stream, false, null);
		}

		/// <summary>
		/// Function to return a sprite from a file system.
		/// </summary>
		/// <param name="fileSystem">File system to use.</param>
		/// <param name="path">Path and filename of the sprite.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The sprite contained within the file system.</returns>
		public static Sprite FromFileSystem(FileSystem fileSystem, string path, Image alternateImage)
		{
			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");

			MemoryStream stream = null;		// Stream that contains the sprite data.

			try
			{
				// Extract the object data from the file system.
				stream = new MemoryStream(fileSystem.ReadFile(path));

				// Open the file for reading.
				if (string.Compare(fileSystem[path].Extension, ".xml", true) == 0)
					return SpriteFromStream(path, fileSystem, null, stream, true, alternateImage);
				else
					return SpriteFromStream(path, fileSystem, null, stream, false, alternateImage);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to return a sprite from a file system.
		/// </summary>
		/// <param name="fileSystem">File system to use.</param>
		/// <param name="path">Path and filename of the sprite.</param>
		/// <returns>The sprite contained within the file system.</returns>
		public static Sprite FromFileSystem(FileSystem fileSystem, string path)
		{
			return FromFileSystem(fileSystem, path, null);
		}

		/// <summary>
		/// Function to load a sprite.
		/// </summary>
		/// <param name="spritePath">Filename/path to the sprite.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The sprite stored on the disk.</returns>
		public static Sprite FromFile(string spritePath, Image alternateImage)
		{
			Stream spriteStream = null;				// Stream for the sprite.

			try
			{
				// Open the file for reading.
				spriteStream = File.OpenRead(spritePath);

				// Open the file for reading.
				if (string.Compare(Path.GetExtension(spritePath), ".xml", true) == 0)
					return SpriteFromStream(spritePath, null, null, spriteStream, true, alternateImage);
				else
					return SpriteFromStream(spritePath, null, null, spriteStream, false, alternateImage);
			}
			finally
			{
				if (spriteStream != null)
					spriteStream.Dispose();
				spriteStream = null;
			}
		}

		/// <summary>
		/// Function to load a sprite.
		/// </summary>
		/// <param name="spritePath">Filename/path to the sprite.</param>
		/// <returns>The sprite stored on the disk.</returns>
		public static Sprite FromFile(string spritePath)
		{
			return FromFile(spritePath, null);
		}

		/// <summary>
		/// Function to load a sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="isXML">TRUE if the file is in XML format, FALSE if binary.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public static Sprite FromResource(string spriteName, ResourceManager resourceManager, bool isXML, Image alternateImage)
		{
			MemoryStream stream = null;						// Memory stream.
			Assembly assembly = null;						// Assembly that holds the resource manager.
			object resourceData = null;						// Resource data.

			try
			{
				// Default to the calling application resource manager.
				if (resourceManager == null)
				{
					// Extract the resource manager from the calling assembly.
					assembly = Assembly.GetEntryAssembly();
					resourceManager = new ResourceManager(assembly.GetName().Name + ".Properties.Resources", assembly);
				}

				// Get object from memory stream.
				resourceData = resourceManager.GetObject(spriteName);

				// If this is a text file, then convert to a byte array.
				if (resourceData is string)
					stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(resourceData.ToString()));
				else
					stream = new MemoryStream((byte[])resourceManager.GetObject(spriteName));

				return SpriteFromStream("@SpriteResource.", null, resourceManager, stream, isXML, alternateImage);
			}
			finally
			{
				if (stream != null)
					stream.Close();

				stream = null;
			}
		}

		/// <summary>
		/// Function to load a sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="isXML">TRUE if the file is in XML format, FALSE if binary.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public static Sprite FromResource(string spriteName, ResourceManager resourceManager, bool isXML)
		{
			return FromResource(spriteName, resourceManager, isXML, null);
		}

		/// <summary>
		/// Function to load a binary sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public static Sprite FromResource(string spriteName, ResourceManager resourceManager, Image alternateImage)
		{
			return FromResource(spriteName, resourceManager, false, alternateImage);
		}

		/// <summary>
		/// Function to load a binary sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public static Sprite FromResource(string spriteName, ResourceManager resourceManager)
		{
			return FromResource(spriteName, resourceManager, false, null);
		}

		/// <summary>
		/// Function to load a sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="isXML">TRUE if the file is in XML format, FALSE if binary.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public static Sprite FromResource(string spriteName, bool isXML, Image alternateImage)
		{
			return FromResource(spriteName, null, isXML, alternateImage);
		}

		/// <summary>
		/// Function to load a sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="isXML">TRUE if the file is in XML format, FALSE if binary.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public static Sprite FromResource(string spriteName, bool isXML)
		{
			return FromResource(spriteName, null, isXML, null);
		}

		/// <summary>
		/// Function to load a binary sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public static Sprite FromResource(string spriteName, Image alternateImage)
		{
			return FromResource(spriteName, null, false, alternateImage);
		}

		/// <summary>
		/// Function to load a binary sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public static Sprite FromResource(string spriteName)
		{
			return FromResource(spriteName, null, false, null);
		}

		private const int PropertyTagFrameDelay = 0x5100; // Frame delay interval tag in a GIF file
		private const int PropertyTagLoopCount = 0x5101; // Loop count tag in a GIF file

		/// <summary>
		/// Returns a sprite, complete with animation, from an animated GIF (Graphics Interchange Format) file.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="filePath">Filename/path to the GIF animation.</param>
		/// <param name="looped">TRUE if the animation should be looped; FALSE if not.</param>
		/// <returns>The sprite that was loaded from the GIF file.</returns>
		public static Sprite FromAnimatedGIF(string spriteName, string filePath, bool looped)
		{
			// Are we even valid?
			if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
				throw new FileNotFoundException("File not found: " + filePath);

			// Create Sprite
			Sprite gifSprite = new Sprite(spriteName);

			// Load Gif File
			using (System.Drawing.Image gifImg = System.Drawing.Image.FromFile(filePath))
			{
				Drawing.Imaging.FrameDimension dimension = new Drawing.Imaging.FrameDimension(gifImg.FrameDimensionsList[0]);
				int interval = BitConverter.ToInt32(gifImg.GetPropertyItem(PropertyTagFrameDelay).Value, 0) * 10;
				int loops = BitConverter.ToInt16(gifImg.GetPropertyItem(PropertyTagLoopCount).Value, 0);
				int frames = gifImg.GetFrameCount(Drawing.Imaging.FrameDimension.Time);

				// Create Animation
				Animation anim = new Animation("GifAnimation", frames * interval);

				anim.Looped = looped;
				anim.Loops = loops;

				KeyImage imageKeyFrame;
				gifSprite.Animations.Add(anim);

				// Add Frames to animation
				for (int i = 0; i < frames; i++)
				{
					gifImg.SelectActiveFrame(dimension, i);
					GorgonLibrary.Graphics.Image image = GorgonLibrary.Graphics.Image.FromBitmap(spriteName + ".frame" + i, gifImg);
					imageKeyFrame = new KeyImage(i * interval, image);
					imageKeyFrame.ImageOffset = new Vector2D(0, 0);
					imageKeyFrame.ImageSize = new Vector2D(image.Width, image.Height);
					anim.Tracks["Image"].AddKey(imageKeyFrame);
				}
			}

			// return sprite
			return gifSprite;
		}

		/// <summary>
		/// Function to save the sprite to a stream.
		/// </summary>
		/// <param name="stream">Stream to save the sprite into.</param>
		/// <param name="isXML">TRUE if this is an xml file, FALSE for binary.</param>
		public virtual void Save(Stream stream, bool isXML)
		{
			Serializer spriteSerializer = null;			// Sprite serializer.
			string spritePath = string.Empty;			// Path for the sprite.

			try
			{				
				if (stream is FileStream)
					spritePath = ((FileStream)stream).Name;
				else
					spritePath = "@Memory.GorgonSprite";

				// Serialize.
				if (isXML)
					spriteSerializer = new XMLSerializer(this, stream);				
				else
					spriteSerializer = new BinarySerializer(this, stream);

				// Don't close the file stream, leave that to the user.
				spriteSerializer.DontCloseStream = true;

				// Setup serializer.
				spriteSerializer.Serialize();

				_spritePath = spritePath;
			}
			finally
			{
				if (spriteSerializer != null)
					spriteSerializer.Dispose();
				spriteSerializer = null;
			}
		}

		/// <summary>
		/// Function to save the binary sprite to a stream.
		/// </summary>
		/// <param name="stream">Stream to save the sprite into.</param>
		public void Save(Stream stream)
		{
			Save(stream, false);
		}

		/// <summary>
		/// Function to save the sprite to a file.
		/// </summary>
		/// <param name="fileName">Path and filename of the sprite.</param>
		/// <param name="isXML">TRUE if this is an xml file, FALSE for binary.</param>
		public virtual void Save(string fileName, bool isXML)
		{
			Stream stream = null;		// Stream to save into.

			if ((fileName == null) || (fileName == string.Empty))
				throw new ArgumentNullException("fileName");

			try
			{
				// Attach extension.
				if (Path.GetExtension(fileName) == "")
				{
					if (!isXML)
						fileName += ".gorSprite";
					else
						fileName += ".xml";
				}

				// Open the file stream.
				stream = File.Open(fileName, FileMode.Create);

				// Serialize.
				Save(stream, isXML);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to save sprite to a file.
		/// </summary>
		/// <param name="fileName">Path and filename of the sprite.</param>
		public void Save(string fileName)
		{
			if (string.Compare(Path.GetExtension(fileName), ".xml", true) == 0)
				Save(fileName, true);
			else
				Save(fileName, false);
		}

		/// <summary>
		/// Function to save the sprite to a file system.
		/// </summary>
		/// <param name="fileSystem">File system to save the sprite into.</param>
		/// <param name="fileName">Path and filename of the sprite.</param>
		public virtual void Save(FileSystem fileSystem, string fileName)
		{
			if (string.Compare(Path.GetExtension(fileName), ".xml", true) == 0)
				Save(fileSystem, fileName, true);
			else
				Save(fileSystem, fileName, false);
		}

		/// <summary>
		/// Function to save the sprite to a file system.
		/// </summary>
		/// <param name="fileSystem">File system to save the sprite into.</param>
		/// <param name="fileName">Path and filename of the sprite.</param>
		/// <param name="isXML">TRUE if this is an xml file, FALSE for binary.</param>
		public virtual void Save(FileSystem fileSystem, string fileName, bool isXML)
		{
			MemoryStream stream = null;		// Stream to save into.
			byte[] data = null;				// Binary data of the sprite.

			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");
			if ((fileName == null) || (fileName == string.Empty))
				throw new ArgumentNullException("fileName");

			try
			{
				// Attach extension.
				if (Path.GetExtension(fileName) == "")
				{
					if (!isXML)
						fileName += ".gorSprite";
					else
						fileName += ".xml";
				}

				// Send the data to a memory stream and get the binary data to send to the file
				// system.
				stream = new MemoryStream();
				Save(stream, isXML);
				stream.Position = 0;
				data = stream.ToArray();

				fileSystem.WriteFile(fileName, data);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to update AABB.
		/// </summary>
		public override void UpdateAABB()
		{
			Vector2D max = new Vector2D(float.MinValue, float.MinValue);	// Max boundary for the AABB.
			Vector2D min = new Vector2D(float.MaxValue, float.MaxValue);	// Min boundary for the AABB.
			float xradius = 0.0f;											// Bounding circle radius. 
			float yradius = 0.0f;											// Bounding circle radius.

			base.UpdateAABB();

			// Update dimensions if needed.
			if (IsSizeUpdated)
				UpdateDimensions();

			// Transform.
			UpdateTransform();

			for (int i = 0; i < 4;i++)
			{
				// Create bounding.			
				min.X = MathUtility.Min(min.X, Vertices[i].Position.X);
				min.Y = MathUtility.Min(min.Y, Vertices[i].Position.Y);
				max.X = MathUtility.Max(max.X, Vertices[i].Position.X);
				max.Y = MathUtility.Max(max.Y, Vertices[i].Position.Y);
			}

			SetAABB(min, max);
			_boundCircle.Center = new Vector2D(min.X + (max.X - min.X) / 2.0f, min.Y + (max.Y - min.Y) / 2.0f);
			xradius = MathUtility.Abs(max.X - min.X) / 2.0f;
			yradius = MathUtility.Abs(max.Y - min.Y) / 2.0f;
			if (xradius > yradius)
				_boundCircle.Radius = xradius;
			else
				_boundCircle.Radius = yradius;
			IsAABBUpdated = false;
		}

		/// <summary>
		/// Function to set the position of a sprite vertex.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to change.</param>
		/// <param name="position">Position of the vertex.</param>
		public void SetSpriteVertexOffset(VertexLocations vertexPosition, Vector2D position)
		{
			SetSpriteVertexOffset(vertexPosition, position.X, position.Y);
		}

		/// <summary>
		/// Function to set the offset of a sprite vertex.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to change.</param>
		/// <param name="x">Horizontal position of the vertex.</param>
		/// <param name="y">Vertical position of the vertex.</param>
		public void SetSpriteVertexOffset(VertexLocations vertexPosition, float x, float y)
		{
			if ((vertexPosition < VertexLocations.UpperLeft) || (vertexPosition > VertexLocations.LowerLeft))
				throw new ArgumentOutOfRangeException("vertexPosition", "The sprite does not contain a vertex at the position " + ((int)vertexPosition).ToString());

			IsAABBUpdated = true;
			_vertexOffsets[(int)vertexPosition].X = x;
			_vertexOffsets[(int)vertexPosition].Y = y;
			UpdateDimensions();
		}

		/// <summary>
		/// Function to get the offset of a sprite vertex.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to retrieve.</param>
		/// <returns>The offset of the sprite vertex.</returns>
		public Vector2D GetSpriteVertexOffset(VertexLocations vertexPosition)
		{
			return _vertexOffsets[(int)vertexPosition];
		}

		/// <summary>
		/// Function to retrieve the final sprite vertex position.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to retrieve.</param>
		/// <returns>The final transformed location of the sprite vertex.</returns>
		public Vector2D GetSpriteVertexPosition(VertexLocations vertexPosition)
		{
			return Vertices[(int)vertexPosition].Position;
		}

		/// <summary>
		/// Function to get the color of a sprite vertex.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to change.</param>
		/// <returns>Color of the sprite vertex.</returns>
		public Drawing.Color GetSpriteVertexColor(VertexLocations vertexPosition)
		{
			if ((vertexPosition < VertexLocations.UpperLeft) || (vertexPosition > VertexLocations.LowerLeft))
				throw new ArgumentOutOfRangeException("vertexPosition", "The sprite does not contain a vertex at the position " + ((int)vertexPosition).ToString());

			return Drawing.Color.FromArgb(Vertices[(int)vertexPosition].ColorValue);
		}

		/// <summary>
		/// Function to set the color of a sprite vertex.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to change.</param>
		/// <param name="newColor">New color to set the vertex to.</param>
		public void SetSpriteVertexColor(VertexLocations vertexPosition, Drawing.Color newColor)
		{
			if ((vertexPosition < VertexLocations.UpperLeft) || (vertexPosition > VertexLocations.LowerLeft))
				throw new ArgumentOutOfRangeException("vertexPosition", "The sprite does not contain a vertex at the position " + ((int)vertexPosition).ToString());

			Vertices[(int)vertexPosition].ColorValue = newColor.ToArgb();
		}

		/// <summary>
		/// Function to set the offset within the source image to start drawing from.
		/// </summary>
		/// <param name="x">Horizontal position.</param>
		/// <param name="y">Vertical position.</param>
		public void SetImageOffset(float x, float y)
		{
			ImageOffset = new Vector2D(x, y);
		}

		/// <summary>
		/// Function to set the region of the image to capture.
		/// </summary>
		/// <param name="x">Horizontal position.</param>
		/// <param name="y">Vertical position.</param>
		/// <param name="width">Width of the region.</param>
		/// <param name="height">Height of the region.</param>
		public void SetImageRegion(float x, float y, float width, float height)
		{
			ImageRegion = new Drawing.RectangleF(x, y, width, height);
		}

		/// <summary>
		/// Function to draw the sprite.
		/// </summary>
		/// <param name="flush">TRUE to flush the buffers after drawing, FALSE to only flush on state change.</param>
		public override void Draw(bool flush)
		{
			BeginRendering(flush);

			// Update the sprite dimensions and offsets if necessary.
			if (IsSizeUpdated)
				UpdateDimensions();
			if (IsImageUpdated)
				UpdateImageLayer();

			// Update the AABB.			
			if (IsAABBUpdated)
				UpdateAABB();
			else
				UpdateTransform();

			WriteVertexData(0, Vertices.Length);

			EndRendering(flush);
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public override Renderable Clone()
		{
			Sprite clone = new Sprite(Name + ".Clone", Image, ImageOffset, Size, Axis, Position, Rotation, Scale);		// Create clone.

			// Copy properties.
			for (int i = 0; i < _spriteCorners.Length; i++)
				clone._spriteCorners[i] = _spriteCorners[i];

			for (int i = 0; i < _vertexOffsets.Length; i++)
				clone._vertexOffsets[i] = _vertexOffsets[i];

			for (int i = 0; i < Vertices.Length; i++)
				clone.Vertices[i] = Vertices[i];

			clone._imagePosition = _imagePosition;
			clone._spritePath = string.Empty;
			clone.SetParent(Parent);
			clone.Size = Size;
			clone.Position = Position;
			clone.Rotation = Rotation;
			clone.Scale = Scale;
			clone.Axis = Axis;
			clone.SetAABB(AABB);
			clone.ParentPosition = ParentPosition;
			clone.ParentRotation = ParentRotation;
			clone.ParentScale = ParentScale;
			clone.HorizontalFlip = HorizontalFlip;
			clone.VerticalFlip = VerticalFlip;
			clone.BorderColor = BorderColor;

			if (!InheritSmoothing)
				clone.Smoothing = Smoothing;
			if (!InheritBlending)
			{
				clone.BlendingMode = BlendingMode;
				clone.SourceBlend = SourceBlend;
				clone.DestinationBlend = DestinationBlend;
			}
			clone.Depth = Depth;
			if (!InheritDepthBias)
				clone.DepthBufferBias = DepthBufferBias;
			if (!InheritDepthTestFunction)
				clone.DepthTestFunction = DepthTestFunction;
			if (!InheritDepthWriteEnabled)
				clone.DepthWriteEnabled = DepthWriteEnabled;
			if (!InheritAlphaMaskFunction)
				clone.AlphaMaskFunction = AlphaMaskFunction;
			if (!InheritAlphaMaskValue)
				clone.AlphaMaskValue = AlphaMaskValue;
			if (!InheritHorizontalWrapping)
				clone.HorizontalWrapMode = HorizontalWrapMode;
			if (!InheritVerticalWrapping)
				clone.VerticalWrapMode = VerticalWrapMode;
			if (!InheritStencilPassOperation)
				clone.StencilPassOperation = StencilPassOperation;
			if (!InheritStencilFailOperation)
				clone.StencilFailOperation = StencilFailOperation;
			if (!InheritStencilZFailOperation)
				clone.StencilZFailOperation = StencilZFailOperation;
			if (!InheritStencilCompare)
				clone.StencilCompare = StencilCompare;
			if (!InheritStencilEnabled)
				clone.StencilEnabled = StencilEnabled;
			if (!InheritStencilReference)
				clone.StencilReference = StencilReference;
			if (!InheritStencilMask)
				clone.StencilMask = StencilMask;

			// Clone the animations.
			Animations.CopyTo(clone);

			clone.InheritScale = InheritScale;
			clone.InheritRotation = InheritRotation;
			clone.Refresh();

			return clone;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Image to use.</param>
		/// <param name="imageOffset">Offset within the image for the sprite to start at.</param>
		/// <param name="size">Width and height of the sprite.</param>
		/// <param name="axis">Pivot axis of the sprite.</param>
		/// <param name="position">Initial position of the sprite.</param>
		/// <param name="rotation">Initial rotation of the sprite.</param>		
		/// <param name="scale">Initial scale of the sprite.</param>
		public Sprite(string spriteName, Image image, Vector2D imageOffset, Vector2D size, Vector2D axis, Vector2D position, float rotation, Vector2D scale)
			: base(spriteName)
		{
			// A new sprite has no path.
			_spritePath = string.Empty;

			base.Position = position;
			base.Size = size;

			Image = image;

			Scale = scale;			
			// Initialize the quad.
			InitializeVertices(4);
			_spriteCorners = new float[4];
			_vertexOffsets = new Vector2D[4];
			Axis = axis;
			_imagePosition = imageOffset;
			// Set sprite colors.
			Vertices[3].ColorValue = Vertices[2].ColorValue = Vertices[1].ColorValue = Vertices[0].ColorValue = Drawing.Color.White.ToArgb();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Image to use.</param>
		/// <param name="imageOffset">Offset within the image for the sprite to start at.</param>
		/// <param name="size">Width and height of the sprite.</param>
		/// <param name="axis">Pivot axis of the sprite.</param>
		/// <param name="position">Initial position of the sprite.</param>
		/// <param name="rotation">Initial rotation of the sprite.</param>		
		public Sprite(string spriteName, Image image, Vector2D imageOffset, Vector2D size, Vector2D axis, Vector2D position, float rotation)
			: this(spriteName, image, imageOffset, size, axis, position, rotation, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Image to use.</param>
		/// <param name="imageOffset">Offset within the image for the sprite to start at.</param>
		/// <param name="size">Width and height of the sprite.</param>
		/// <param name="axis">Pivot axis of the sprite.</param>
		/// <param name="position">Initial position of the sprite.</param>
		public Sprite(string spriteName, Image image, Vector2D imageOffset, Vector2D size, Vector2D axis, Vector2D position)
			: this(spriteName, image, imageOffset, size, axis, position, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Image to use.</param>
		/// <param name="imageOffset">Offset within the image for the sprite to start at.</param>
		/// <param name="size">Width and height of the sprite.</param>
		/// <param name="axis">Pivot axis of the sprite.</param>		
		public Sprite(string spriteName, Image image, Vector2D imageOffset, Vector2D size, Vector2D axis)
			: this(spriteName, image, imageOffset, size, axis, Vector2D.Zero, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Image to use.</param>
		/// <param name="imageOffset">Offset within the image for the sprite to start at.</param>
		/// <param name="size">Width and height of the sprite.</param>
		public Sprite(string spriteName, Image image, Vector2D imageOffset, Vector2D size)
			: this(spriteName, image, imageOffset, size, Vector2D.Zero, Vector2D.Zero, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Image to use.</param>
		/// <param name="size">Width and height of the sprite.</param>
		public Sprite(string spriteName, Image image, Vector2D size)
			: this(spriteName, image, Vector2D.Zero, size, Vector2D.Zero, Vector2D.Zero, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Image to use.</param>
		public Sprite(string spriteName, Image image)
			: this(spriteName, image, Vector2D.Zero, Vector2D.Unit, Vector2D.Zero, Vector2D.Zero, 0, Vector2D.Unit)
		{
			// Re-evaluate the size.
			if (image != null)
				base.Size = new Vector2D(image.Width, image.Height);
			else
				base.Size = Vector2D.Zero;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="size">Width and height of the sprite.</param>
		public Sprite(string spriteName, Vector2D size)
			: this(spriteName, (Image)null, Vector2D.Zero, size, Vector2D.Zero, Vector2D.Zero, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		public Sprite(string spriteName)
			: this(spriteName, (Image)null, Vector2D.Zero, Vector2D.Unit, Vector2D.Zero, Vector2D.Zero, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Image to use.</param>
		/// <param name="imageX">Horizontal position in the image to map to the upper-left corner.</param>
		/// <param name="imageY">Vertical position in the image to map to the upper-left corner.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		/// <param name="axisX">Horizontal position of the rotation axis.</param>
		/// <param name="axisY">Vertical position of the rotation axis.</param>
		/// <param name="positionX">Horizontal position of the sprite.</param>
		/// <param name="positionY">Vertical position of the sprite.</param>
		/// <param name="rotation">Initial rotation of the sprite.</param>		
		/// <param name="scaleX">Initial horizontal scale of the sprite.</param>
		/// <param name="scaleY">Initial vertical scale of the sprite.</param>
		public Sprite(string spriteName, Image image, float imageX, float imageY, float width, float height, float axisX, float axisY, float positionX, float positionY, float rotation, float scaleX, float scaleY)
			: this(spriteName, image, new Vector2D(imageX, imageY), new Vector2D(width, height), new Vector2D(axisX, axisY), new Vector2D(positionX, positionY), rotation, new Vector2D(scaleX, scaleY))
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Image to use.</param>
		/// <param name="imageX">Horizontal position in the image to map to the upper-left corner.</param>
		/// <param name="imageY">Vertical position in the image to map to the upper-left corner.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		/// <param name="axisX">Horizontal position of the rotation axis. (Default: center of sprite).</param>
		/// <param name="axisY">Vertical position of the rotation axis. (Default: center of sprite).</param>
		/// <param name="positionX">Horizontal position of the sprite.</param>
		/// <param name="positionY">Vertical position of the sprite.</param>
		/// <param name="rotation">Initial rotation of the sprite.</param>		
		public Sprite(string spriteName, Image image, float imageX, float imageY, float width, float height, float axisX, float axisY, float positionX, float positionY, float rotation)
			: this(spriteName, image, new Vector2D(imageX, imageY), new Vector2D(width, height), new Vector2D(axisX, axisY), new Vector2D(positionX, positionY), rotation, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Image to use.</param>
		/// <param name="imageX">Horizontal position in the image to map to the upper-left corner.</param>
		/// <param name="imageY">Vertical position in the image to map to the upper-left corner.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		/// <param name="axisX">Horizontal position of the rotation axis. (Default: center of sprite).</param>
		/// <param name="axisY">Vertical position of the rotation axis. (Default: center of sprite).</param>
		/// <param name="positionX">Horizontal position of the sprite.</param>
		/// <param name="positionY">Vertical position of the sprite.</param>
		public Sprite(string spriteName, Image image, float imageX, float imageY, float width, float height, float axisX, float axisY, float positionX, float positionY)
			: this(spriteName, image, new Vector2D(imageX, imageY), new Vector2D(width, height), new Vector2D(axisX, axisY), new Vector2D(positionX, positionY), 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Image to use.</param>
		/// <param name="imageX">Horizontal position in the image to map to the upper-left corner.</param>
		/// <param name="imageY">Vertical position in the image to map to the upper-left corner.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		/// <param name="axisX">Horizontal position of the rotation axis. (Default: center of sprite).</param>
		/// <param name="axisY">Vertical position of the rotation axis. (Default: center of sprite).</param>
		public Sprite(string spriteName, Image image, float imageX, float imageY, float width, float height, float axisX, float axisY)
			: this(spriteName, image, new Vector2D(imageX, imageY), new Vector2D(width, height), new Vector2D(axisX, axisY), Vector2D.Zero, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Image to use.</param>
		/// <param name="imageX">Horizontal position in the image to map to the upper-left corner.</param>
		/// <param name="imageY">Vertical position in the image to map to the upper-left corner.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		public Sprite(string spriteName, Image image, float imageX, float imageY, float width, float height)
			: this(spriteName, image, new Vector2D(imageX, imageY), new Vector2D(width, height), Vector2D.Zero, Vector2D.Zero, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Image to use.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		public Sprite(string spriteName, Image image, float width, float height)
			: this(spriteName, image, Vector2D.Zero, new Vector2D(width, height), Vector2D.Zero, Vector2D.Zero, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Render target image to use.</param>
		/// <param name="imageOffset">Offset within the image for the sprite to start at.</param>
		/// <param name="size">Width and height of the sprite.</param>
		/// <param name="axis">Pivot axis of the sprite.</param>
		/// <param name="position">Initial position of the sprite.</param>
		/// <param name="rotation">Initial rotation of the sprite.</param>		
		/// <param name="scale">Initial scale of the sprite.</param>
		public Sprite(string spriteName, RenderImage image, Vector2D imageOffset, Vector2D size, Vector2D axis, Vector2D position, float rotation, Vector2D scale)
			: this(spriteName, image.Image, imageOffset, size, axis, position, rotation, scale)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Render target image to use.</param>
		/// <param name="imageOffset">Offset within the image for the sprite to start at.</param>
		/// <param name="size">Width and height of the sprite.</param>
		/// <param name="axis">Pivot axis of the sprite.</param>
		/// <param name="position">Initial position of the sprite.</param>
		/// <param name="rotation">Initial rotation of the sprite.</param>		
		public Sprite(string spriteName, RenderImage image, Vector2D imageOffset, Vector2D size, Vector2D axis, Vector2D position, float rotation)
			: this(spriteName, image.Image, imageOffset, size, axis, position, rotation, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Render target image to use.</param>
		/// <param name="imageOffset">Offset within the image for the sprite to start at.</param>
		/// <param name="size">Width and height of the sprite.</param>
		/// <param name="axis">Pivot axis of the sprite.</param>
		/// <param name="position">Initial position of the sprite.</param>
		public Sprite(string spriteName, RenderImage image, Vector2D imageOffset, Vector2D size, Vector2D axis, Vector2D position)
			: this(spriteName, image.Image, imageOffset, size, axis, position, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Render target image to use.</param>
		/// <param name="imageOffset">Offset within the image for the sprite to start at.</param>
		/// <param name="size">Width and height of the sprite.</param>
		/// <param name="axis">Pivot axis of the sprite.</param>
		public Sprite(string spriteName, RenderImage image, Vector2D imageOffset, Vector2D size, Vector2D axis)
			: this(spriteName, image.Image, imageOffset, size, axis, Vector2D.Zero, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Render target image to use.</param>
		/// <param name="imageOffset">Offset within the image for the sprite to start at.</param>
		/// <param name="size">Width and height of the sprite.</param>
		public Sprite(string spriteName, RenderImage image, Vector2D imageOffset, Vector2D size)
			: this(spriteName, image.Image, imageOffset, size, Vector2D.Zero, Vector2D.Zero, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Render target image to use.</param>
		/// <param name="size">Width and height of the sprite.</param>
		public Sprite(string spriteName, RenderImage image, Vector2D size)
			: this(spriteName, image.Image, Vector2D.Zero, size, Vector2D.Zero, Vector2D.Zero, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Render target image to use.</param>
		public Sprite(string spriteName, RenderImage image)
			: this(spriteName, image.Image, Vector2D.Zero, new Vector2D(image.Width, image.Height), Vector2D.Zero, Vector2D.Zero, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Render target image to use.</param>
		/// <param name="imageX">Horizontal position in the image to map to the upper-left corner.</param>
		/// <param name="imageY">Vertical position in the image to map to the upper-left corner.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		/// <param name="axisX">Horizontal position of the rotation axis.</param>
		/// <param name="axisY">Vertical position of the rotation axis.</param>
		/// <param name="positionX">Horizontal position of the sprite.</param>
		/// <param name="positionY">Vertical position of the sprite.</param>
		/// <param name="rotation">Initial rotation of the sprite.</param>		
		/// <param name="scaleX">Initial horizontal scale of the sprite.</param>
		/// <param name="scaleY">Initial vertical scale of the sprite.</param>
		public Sprite(string spriteName, RenderImage image, float imageX, float imageY, float width, float height, float axisX, float axisY, float positionX, float positionY, float rotation, float scaleX, float scaleY)
			: this(spriteName, image.Image, new Vector2D(imageX, imageY), new Vector2D(width, height), new Vector2D(axisX, axisY), new Vector2D(positionX, positionY), rotation, new Vector2D(scaleX, scaleY))
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Render target image to use.</param>
		/// <param name="imageX">Horizontal position in the image to map to the upper-left corner.</param>
		/// <param name="imageY">Vertical position in the image to map to the upper-left corner.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		/// <param name="axisX">Horizontal position of the rotation axis. (Default: center of sprite).</param>
		/// <param name="axisY">Vertical position of the rotation axis. (Default: center of sprite).</param>
		/// <param name="positionX">Horizontal position of the sprite.</param>
		/// <param name="positionY">Vertical position of the sprite.</param>
		/// <param name="rotation">Initial rotation of the sprite.</param>		
		public Sprite(string spriteName, RenderImage image, float imageX, float imageY, float width, float height, float axisX, float axisY, float positionX, float positionY, float rotation)
			: this(spriteName, image.Image, new Vector2D(imageX, imageY), new Vector2D(width, height), new Vector2D(axisX, axisY), new Vector2D(positionX, positionY), rotation, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Render target image to use.</param>
		/// <param name="imageX">Horizontal position in the image to map to the upper-left corner.</param>
		/// <param name="imageY">Vertical position in the image to map to the upper-left corner.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		/// <param name="axisX">Horizontal position of the rotation axis. (Default: center of sprite).</param>
		/// <param name="axisY">Vertical position of the rotation axis. (Default: center of sprite).</param>
		/// <param name="positionX">Horizontal position of the sprite.</param>
		/// <param name="positionY">Vertical position of the sprite.</param>
		public Sprite(string spriteName, RenderImage image, float imageX, float imageY, float width, float height, float axisX, float axisY, float positionX, float positionY)
			: this(spriteName, image.Image, new Vector2D(imageX, imageY), new Vector2D(width, height), new Vector2D(axisX, axisY), new Vector2D(positionX, positionY), 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Render target image to use.</param>
		/// <param name="imageX">Horizontal position in the image to map to the upper-left corner.</param>
		/// <param name="imageY">Vertical position in the image to map to the upper-left corner.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		/// <param name="axisX">Horizontal position of the rotation axis. (Default: center of sprite).</param>
		/// <param name="axisY">Vertical position of the rotation axis. (Default: center of sprite).</param>
		public Sprite(string spriteName, RenderImage image, float imageX, float imageY, float width, float height, float axisX, float axisY)
			: this(spriteName, image.Image, new Vector2D(imageX, imageY), new Vector2D(width, height), new Vector2D(axisX, axisY), Vector2D.Zero, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Render target image to use.</param>
		/// <param name="imageX">Horizontal position in the image to map to the upper-left corner.</param>
		/// <param name="imageY">Vertical position in the image to map to the upper-left corner.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		public Sprite(string spriteName, RenderImage image, float imageX, float imageY, float width, float height)
			: this(spriteName, image.Image, new Vector2D(imageX, imageY), new Vector2D(width, height), Vector2D.Zero, Vector2D.Zero, 0, Vector2D.Unit)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="image">Render target image to use.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		public Sprite(string spriteName, RenderImage image, float width, float height)
			: this(spriteName, image.Image, Vector2D.Zero, new Vector2D(width, height), Vector2D.Zero, Vector2D.Zero, 0, Vector2D.Unit)
		{
		}
		#endregion

	    #region ISerializable Members.
		/// <summary>
		/// Property to set or return the filename of the serializable object.
		/// </summary>
		public string Filename
		{
			get
			{
				return _spritePath;
			}
		}

		/// <summary>
		/// Property to return whether this object is an embedded resource.
		/// </summary>
		public bool IsResource
		{
			get
			{
				return _isResource;
			}
		}

		/// <summary>
		/// Function to persist the data into the serializer stream.
		/// </summary>
		/// <param name="writer">Serializer that's calling this function.</param>
		void ISerializable.WriteData(Serializer writer)
		{
			int[] vertexColor = new int[4];				// Vertex colors.

			writer.WriteComment("Gorgon Sprite - " + Name);
			writer.WriteComment("Written by Michael Winsor (Tape_Worm) for the Gorgon library.");
			writer.WriteComment("Use at your own peril.");

			// Root region.
			writer.WriteGroupBegin("GorgonSprite");

			// Write header.
			writer.WriteGroupBegin("Header");
			writer.Write("HeaderValue", "GORSPR1.1");
			writer.WriteGroupEnd();

			// Write meta data.
			writer.WriteGroupBegin("MetaData");

			// Write name
			writer.Write("Name", Name);

			writer.WriteGroupEnd();

			// Write sprite data.
			writer.WriteGroupBegin("Sprite");

			// Write image information.
			writer.WriteGroupBegin("Image");
			writer.Write("HasImage", Image != null);
			if (Image != null)
			{
				writer.Write("IsRenderTarget", Image.ImageType == ImageType.RenderTarget);

				// Write out render target parameters.
				if (Image.ImageType == ImageType.RenderTarget)
				{
					writer.Write("ImageName", Image.RenderImage.Name);
					writer.Write("Format", (int)Image.Format);
					writer.Write("SizeWidth", Image.Width);
					writer.Write("SizeHeight", Image.Height);
					writer.Write("HasDepthBuffer", Image.RenderImage.UseDepthBuffer);
					writer.Write("HasStencilBuffer", Image.RenderImage.UseStencilBuffer);
				}
				else
					writer.Write("ImageName", Image.Name);
			}
			writer.WriteGroupEnd();

			// Write inheritance settings.
			writer.WriteComment("These settings are used to determine if the sprite will inherit settings");
			writer.WriteComment("from its layer.");
			writer.WriteGroupBegin("InheritedSettings");
			writer.Write("AlphaMaskFunction", InheritAlphaMaskFunction);
			writer.Write("AlphaMaskValue", InheritAlphaMaskValue);
			writer.Write("BlendingMode", InheritBlending);
			writer.Write("HorizontalWrapping", InheritHorizontalWrapping);
			writer.Write("Smoothing", InheritSmoothing);
			writer.Write("StencilCompare", InheritStencilCompare);
			writer.Write("StencilEnabled", InheritStencilEnabled);
			writer.Write("StencilFailOperation", InheritStencilFailOperation);
			writer.Write("StencilMask", InheritStencilMask);
			writer.Write("StencilPassOperation", InheritStencilPassOperation);
			writer.Write("StencilReference", InheritStencilReference);
			writer.Write("StencilZFailOperation", InheritStencilZFailOperation);
			writer.Write("VerticalWrapping", InheritVerticalWrapping);
			writer.Write("DepthBias", InheritDepthBias);
			writer.Write("DepthFunction", InheritDepthTestFunction);
			writer.Write("DepthWrite", InheritDepthWriteEnabled);
			writer.WriteGroupEnd();

			// Write dimensions.
			writer.WriteComment("Dimensions and offsets of the sprite.");
			writer.WriteGroupBegin("Dimensions");
			writer.Write("Width", Width);
			writer.Write("Height", Height);
			writer.Write("OffsetX", ImageOffset.X);
			writer.Write("OffsetY", ImageOffset.Y);
			writer.Write("AxisX", Axis.X);
			writer.Write("AxisY", Axis.Y);
			writer.Write("Vertex1OffsetX", _vertexOffsets[0].X);
			writer.Write("Vertex1OffsetY", _vertexOffsets[0].Y);
			writer.Write("Vertex2OffsetX", _vertexOffsets[1].X);
			writer.Write("Vertex2OffsetY", _vertexOffsets[1].Y);
			writer.Write("Vertex3OffsetX", _vertexOffsets[2].X);
			writer.Write("Vertex3OffsetY", _vertexOffsets[2].Y);
			writer.Write("Vertex4OffsetX", _vertexOffsets[3].X);
			writer.Write("Vertex4OffsetY", _vertexOffsets[3].Y);
			writer.WriteGroupEnd();

			// Write effects.
			writer.WriteComment("Effects on the sprite.");
			vertexColor[0] = GetSpriteVertexColor(VertexLocations.LowerLeft).ToArgb();
			vertexColor[1] = GetSpriteVertexColor(VertexLocations.LowerRight).ToArgb();
			vertexColor[2] = GetSpriteVertexColor(VertexLocations.UpperLeft).ToArgb();
			vertexColor[3] = GetSpriteVertexColor(VertexLocations.UpperRight).ToArgb();
			writer.WriteGroupBegin("Effects");
			writer.Write("UpperLeftColor", vertexColor[2]);
			writer.Write("UpperRightColor", vertexColor[3]);
			writer.Write("LowerLeftColor", vertexColor[0]);
			writer.Write("LowerRightColor", vertexColor[1]);

			if (!InheritAlphaMaskFunction)
				writer.Write("AlphaMaskFunction", (int)AlphaMaskFunction);
			if (!InheritAlphaMaskValue)
				writer.Write("AlphaMaskValue", AlphaMaskValue);
			if (!InheritBlending)
			{
				writer.Write("DestinationBlend", (int)DestinationBlend);
				writer.Write("SourceBlend", (int)SourceBlend);
				writer.Write("BlendingMode", (int)BlendingMode);
			}
			if (!InheritHorizontalWrapping)
				writer.Write("HorizontalWrapping", (int)HorizontalWrapMode);
			if (!InheritSmoothing)
				writer.Write("Smoothing", (int)Smoothing);
			if (!InheritStencilCompare)
				writer.Write("StencilCompare", (int)StencilCompare);
			if (!InheritStencilEnabled)
				writer.Write("StencilEnabled", StencilEnabled);
			if (!InheritStencilFailOperation)
				writer.Write("StencilFailOperation", (int)StencilFailOperation);
			if (!InheritStencilMask)
				writer.Write("StencilMask", StencilMask);
			if (!InheritStencilPassOperation)
				writer.Write("StencilPassOperation", (int)StencilPassOperation);
			if (!InheritStencilReference)
				writer.Write("StencilReference", StencilReference);
			if (!InheritStencilZFailOperation)
				writer.Write("StencilZFailOperation", (int)StencilZFailOperation);
			if (!InheritVerticalWrapping)
				writer.Write("VerticalWrapping", (int)VerticalWrapMode);
			if (!InheritDepthBias)
				writer.Write("DepthBias", DepthBufferBias);
			if (!InheritDepthTestFunction)
				writer.Write("DepthCompare", (int)DepthTestFunction);
			if (!InheritDepthWriteEnabled)
				writer.Write("DepthWrite", DepthWriteEnabled);

			writer.Write("BorderColor", BorderColor.ToArgb());

			// Write flipped flags.
			writer.Write("HorizontallyFlipped", _flipHorizontal);
			writer.Write("VerticallyFlipped", _flipVertical);
			writer.WriteGroupEnd();

            // Write animations.
            writer.WriteGroupBegin("Animations");
            writer.Write("AnimationCount", Animations.Count);
            if (Animations.Count > 0)
            {
                // Write each animation.
                foreach (Animation animation in Animations)
                    ((ISerializable)animation).WriteData(writer);
            }

            writer.WriteGroupEnd();
            
            // Sprite.
			writer.WriteGroupEnd();
			// GorgonSprite.
			writer.WriteGroupEnd();
		}

		/// <summary>
		/// Function to retrieve data from the serializer stream.
		/// </summary>
		/// <param name="reader">Serializer that's calling this function.</param>
		void ISerializable.ReadData(Serializer reader)
		{
			string header = string.Empty;										// Header.
			bool hasImage = false;												// Flag to indicate whether an image is bound.
			bool hasShader = false;												// Flag to indicate whether a shader is bound.
			Version spriteVersion = new Version(1, 0);							// Sprite versioning.

			// Mark the sprite as changed.
			IsImageUpdated = true;
			IsSizeUpdated = true;
			IsAABBUpdated = true;
			_deferredImage = string.Empty;

			header = reader.ReadString("HeaderValue");

			switch (header.ToLower())
			{
				case "gorspr1":
					spriteVersion = new Version(1, 0);
					break;
				case "gorspr1.1":
					spriteVersion = new Version(1, 1);
					break;
				default:
					throw new GorgonException(GorgonErrors.CannotLoad, "Sprite file type is unknown or corrupted.");
			}

			// Get sprite data.
			Name = reader.ReadString("Name");

			// We have an alternate image, so use that instead.
			if (reader.Parameters.ContainsKey("Image"))
				Image = reader.Parameters["Image"] as Image;

			// Image data.
			hasImage = reader.ReadBool("HasImage");
			if (hasImage)
			{
				string imageName = string.Empty;		// Image name.
				bool isRenderTarget = false;			// Flag to indicate that the image is a render target.

				isRenderTarget = reader.ReadBool("IsRenderTarget");
				imageName = reader.ReadString("ImageName");

				// If the image is a render target then get the settings.
				if (isRenderTarget)
				{
					int targetWidth = 0;												// Render target width.
					int targetHeight = 0;												// Render target height.
					bool targetUseDepth = false;										// Flag to indicate whether a render target uses a depth buffer or not.
					bool targetUseStencil = false;										// Flag to indicate whether a render target uses a stencil buffer or not.
					ImageBufferFormats targetFormat = ImageBufferFormats.BufferUnknown;	// Render target format.

					targetFormat = (ImageBufferFormats)reader.ReadInt32("Format");
					targetWidth = reader.ReadInt32("SizeWidth");
					targetHeight = reader.ReadInt32("SizeHeight");
					targetUseDepth = reader.ReadBool("HasDepthBuffer");
					targetUseStencil = reader.ReadBool("HasStencilBuffer");

					// If this target already exists, then use it, otherwise, create it.
					if (Image == null)
					{
						if (RenderTargetCache.Targets.Contains(imageName))
						{
							if (RenderTargetCache.Targets[imageName] is RenderImage)
								Image = ((RenderImage)RenderTargetCache.Targets[imageName]).Image;
						}
						else
						{
							// Create render target.
							RenderImage target = new RenderImage(imageName, targetWidth, targetHeight, targetFormat, targetUseDepth, targetUseStencil);
							Image = target.Image;
						}
					}
				}
				else
				{
					// If an image is loaded with this name, use it.
					if ((Image == null) && (ImageCache.Images.Contains(imageName)))
						Image = ImageCache.Images[imageName];
					else
						_deferredImage = imageName;
				}
			}
          
			// Get inherited settings.
			InheritAlphaMaskFunction = reader.ReadBool("AlphaMaskFunction");
			InheritAlphaMaskValue = reader.ReadBool("AlphaMaskValue");
			InheritBlending = reader.ReadBool("BlendingMode");
			InheritHorizontalWrapping = reader.ReadBool("HorizontalWrapping");
			InheritSmoothing = reader.ReadBool("Smoothing");
			InheritStencilCompare = reader.ReadBool("StencilCompare");
			InheritStencilEnabled = reader.ReadBool("StencilEnabled");
			InheritStencilFailOperation = reader.ReadBool("StencilFailOperation");
			InheritStencilMask = reader.ReadBool("StencilMask");
			InheritStencilPassOperation = reader.ReadBool("StencilPassOperation");
			InheritStencilReference = reader.ReadBool("StencilReference");
			InheritStencilZFailOperation = reader.ReadBool("StencilZFailOperation");
			InheritVerticalWrapping = reader.ReadBool("VerticalWrapping");
			// Get version 1.1 fields.
			if (spriteVersion == new Version(1, 1))
			{
				InheritDepthBias = reader.ReadBool("DepthBias");
				InheritDepthTestFunction = reader.ReadBool("DepthFunction");
				InheritDepthWriteEnabled = reader.ReadBool("DepthWrite");
			}


			// Get dimensions.
			Size = new Vector2D(reader.ReadSingle("Width"), reader.ReadSingle("Height"));
			_imagePosition.X = reader.ReadSingle("OffsetX");
			_imagePosition.Y = reader.ReadSingle("OffsetY");
			Axis = new Vector2D(reader.ReadSingle("AxisX"), reader.ReadSingle("AxisY"));
			_vertexOffsets[0].X = reader.ReadSingle("Vertex1OffsetX");
			_vertexOffsets[0].Y = reader.ReadSingle("Vertex1OffsetY");
			_vertexOffsets[1].X = reader.ReadSingle("Vertex2OffsetX");
			_vertexOffsets[1].Y = reader.ReadSingle("Vertex2OffsetY");
			_vertexOffsets[2].X = reader.ReadSingle("Vertex3OffsetX");
			_vertexOffsets[2].Y = reader.ReadSingle("Vertex3OffsetY");
			_vertexOffsets[3].X = reader.ReadSingle("Vertex4OffsetX");
			_vertexOffsets[3].Y = reader.ReadSingle("Vertex4OffsetY");

			// Effects.
			SetSpriteVertexColor(VertexLocations.UpperLeft, Drawing.Color.FromArgb(reader.ReadInt32("UpperLeftColor")));
			SetSpriteVertexColor(VertexLocations.UpperRight, Drawing.Color.FromArgb(reader.ReadInt32("UpperRightColor")));
			SetSpriteVertexColor(VertexLocations.LowerLeft, Drawing.Color.FromArgb(reader.ReadInt32("LowerLeftColor")));
			SetSpriteVertexColor(VertexLocations.LowerRight, Drawing.Color.FromArgb(reader.ReadInt32("LowerRightColor")));

			// This is for compatibility - v1.0 files had shaders attached to the sprites.  In reality this was kind of pointless.
			if (spriteVersion < new Version(1, 1))
			{
				hasShader = reader.ReadBool("HasShader");
				if (hasShader)
				{
					reader.ReadString("ShaderName");
					reader.ReadBool("ShaderIsCompiled");		// For now, we'll ignore this.
					if (reader.ReadBool("HasTechnique"))
						reader.ReadString("Technique");
				}
			}

			if (!InheritAlphaMaskFunction)
				AlphaMaskFunction = (CompareFunctions)reader.ReadInt32("AlphaMaskFunction");
			if (!InheritAlphaMaskValue)
				AlphaMaskValue = reader.ReadInt32("AlphaMaskValue");
			if (!InheritBlending)
			{
				AlphaBlendOperation sourceBlend;		// Source blending mode.  I'm so stupid, I should have put this last, it always overrides the dest/source settings.
				AlphaBlendOperation destBlend;			// Destination blending mode.  I'm so stupid, I should have put this last, it always overrides the dest/source settings.

				destBlend = (AlphaBlendOperation)reader.ReadInt32("DestinationBlend");
				sourceBlend = (AlphaBlendOperation)reader.ReadInt32("SourceBlend");
				BlendingMode = (BlendingModes)reader.ReadInt32("BlendingMode");

				// Set the blending states.
				DestinationBlend = destBlend;
				SourceBlend = sourceBlend;
			}
			if (!InheritHorizontalWrapping)
				HorizontalWrapMode = (ImageAddressing)reader.ReadInt32("HorizontalWrapping");
			if (!InheritSmoothing)
				Smoothing = (Smoothing)reader.ReadInt32("Smoothing");
			if (!InheritStencilCompare)
				StencilCompare = (CompareFunctions)reader.ReadInt32("StencilCompare");
			if (!InheritStencilEnabled)
				StencilEnabled = reader.ReadBool("StencilEnabled");
			if (!InheritStencilFailOperation)
				StencilFailOperation = (StencilOperations)reader.ReadInt32("StencilFailOperation");
			if (!InheritStencilMask)
				StencilMask = reader.ReadInt32("StencilMask");
			if (!InheritStencilPassOperation)
				StencilPassOperation = (StencilOperations)reader.ReadInt32("StencilPassOperation");
			if (!InheritStencilReference)
				StencilReference = reader.ReadInt32("StencilReference");
			if (!InheritStencilZFailOperation)
				StencilZFailOperation = (StencilOperations)reader.ReadInt32("StencilZFailOperation");
			if (!InheritVerticalWrapping)
				VerticalWrapMode = (ImageAddressing)reader.ReadInt32("VerticalWrapping");

			// Get version 1.1 fields.
			if (spriteVersion == new Version(1, 1))
			{
				if (!InheritDepthBias)
					DepthBufferBias = reader.ReadSingle("DepthBias");
				if (!InheritDepthTestFunction)
					DepthTestFunction = (CompareFunctions)reader.ReadInt32("DepthCompare");
				if (!InheritDepthWriteEnabled)
					DepthWriteEnabled = reader.ReadBool("DepthWrite");

				BorderColor = Drawing.Color.FromArgb(reader.ReadInt32("BorderColor"));
			}

			// Set flipping flags.
			_flipHorizontal = reader.ReadBool("HorizontallyFlipped");
			_flipVertical = reader.ReadBool("VerticallyFlipped");

            // Get animations.			
			int animationCount = 0;             // Animation count.
			
            animationCount = reader.ReadInt32("AnimationCount");
            if (animationCount > 0)
            {
                // Read each animation.
                for (int i = 0; i < animationCount; i++)
                {					
                    Animation animation = new Animation("@EmptyAnimation", 0.0f);
					animation.SetOwner(this);
					if (spriteVersion == new Version(1, 1))
						((ISerializable)animation).ReadData(reader);
					else
						animation.ReadVersion1Animation(reader);		// Import the old animation format.
					Animations.Add(animation);
                }
            }

			// Perform updates.
			UpdateDimensions();
			UpdateTransform();
			UpdateImageLayer();
			UpdateAABB();
		}
		#endregion
	}
}
