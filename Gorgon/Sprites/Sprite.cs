#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Sunday, July 09, 2006 3:16:50 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Resources;
using Drawing = System.Drawing;
using SharpUtilities;
using SharpUtilities.Mathematics;
using GorgonLibrary.Internal;
using GorgonLibrary.Serialization;
using GorgonLibrary.Graphics.Animations;
using GorgonLibrary.Graphics.Shaders;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a sprite.
	/// </summary>
	public class Sprite
		: Renderable<VertexTypes.PositionDiffuse2DTexture1>, ISerializable
	{
		#region Variables.
		private float[] _spriteCorners;							// Sprite corners.
		private Vector2D[] _vertexOffsets;						// Relative offsets for vertices.
		private Vector2D _imagePosition;						// Position within the image to start copying from.
		private string _spritePath;								// Path to the loaded/saved sprite.
		private string _spriteImageName = string.Empty;			// Sprite image name.
		private string _spriteShaderName = string.Empty;		// Sprite shader name.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the image that this object is bound with.
		/// </summary>
		public override Image Image
		{
			get
			{
				if ((_spriteImageName != string.Empty) && (base.Image == null))
				{
					if (Gorgon.ImageManager.Contains(_spriteImageName))
						base.Image = Gorgon.ImageManager[_spriteImageName];
				}

				return base.Image;
			}
			set
			{
				base.Image = value;
				if (value != null)
					_spriteImageName = value.Name;
				else
					_spriteImageName = string.Empty;
			}
		}

		/// <summary>
		/// Property to set or return a shader effect for this object.
		/// </summary>
		/// <value></value>
		public override Shader Shader
		{
			get
			{
				if ((_spriteShaderName != string.Empty) && (base.Shader == null))
					base.Shader = Gorgon.Shaders[_spriteShaderName];
				return base.Shader;
			}
			set
			{
				base.Shader = value;
				if (value != null)
					_spriteShaderName = value.Name;
				else
					_spriteShaderName = string.Empty;
			}
		}

		/// <summary>
		/// Property to return the filename of the sprite.
		/// </summary>
		public string Filename
		{
			get
			{
				return _spritePath;
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
				return Drawing.Color.FromArgb(_vertices[0].Color);
			}
			set
			{
				int colorInt = value.ToArgb();		// Integer representation of the color.

				_vertices[3].Color = _vertices[2].Color = _vertices[1].Color = _vertices[0].Color = colorInt;
			}
		}

		/// <summary>
		/// Property to set or return the opacity.
		/// </summary>
		public override byte Opacity
		{
			get
			{
				return (byte)((_vertices[0].Color >> 24) & 0xFF);
			}
			set
			{
				_vertices[0].Color = ((int)value << 24) | (_vertices[0].Color & 0xFFFFFF);
				_vertices[1].Color = ((int)value << 24) | (_vertices[1].Color & 0xFFFFFF);
				_vertices[2].Color = ((int)value << 24) | (_vertices[2].Color & 0xFFFFFF);
				_vertices[3].Color = ((int)value << 24) | (_vertices[3].Color & 0xFFFFFF);
			}
		}

		/// <summary>
		/// Property to set or return the axis of the sprite.
		/// </summary>
		public override Vector2D Axis
		{
			get
			{
				return _axis;
			}
			set
			{
				_axis = value;
				_needAABBUpdate = true;
				_dimensionsChanged = true;
				_imageCoordinatesChanged = true;
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
				_imagePosition = value;
				_imageCoordinatesChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the region of the image that the sprite will use.
		/// </summary>
		public Drawing.RectangleF ImageRegion
		{
			get
			{
				return new Drawing.RectangleF(_imagePosition.X, _imagePosition.Y, _size.X, _size.Y);
			}
			set
			{
				_imagePosition.X = value.X;
				_imagePosition.Y = value.Y;
				_size.X = value.Width;
				_size.Y = value.Height;

				_imageCoordinatesChanged = true;
				_needAABBUpdate = true;
				_dimensionsChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the scale of the sprite.
		/// </summary>
		public override Vector2D Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_needAABBUpdate = true;
                _needParentUpdate = true;

				// Don't allow 0 scale.
				if ((value.X == 0.0f) && (value.Y == 0.0f))
					return;

				if (value.X != 0.0f)
					_scale.X = value.X;
				if (value.Y != 0.0f)
					_scale.Y = value.Y;

                if (_children.Count > 0)
                    ((IRenderable)this).UpdateChildren();
			}
		}

		/// <summary>
		/// Property to set or return a uniform scale across the X and Y axis.
		/// </summary>
		public override float UniformScale
		{
			get
			{
				return _scale.X;
			}
			set
			{
				_needAABBUpdate = true;

				if (value == 0.0f)
					return;

				// Set the uniform scale.
				_scale.X = _scale.Y = value;

                if (_children.Count > 0)
                    ((IRenderable)this).UpdateChildren();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the dimensions for an object.
		/// </summary>
		protected override void UpdateDimensions()
		{
			if ((MathUtility.EqualFloat(Size.X, 0.0f)) || (MathUtility.EqualFloat(Size.Y, 0.0f)))
				throw new SpriteSizeException(null);

			// Resize the sprite.
			_spriteCorners[0] = -_axis.X;
			_spriteCorners[1] = -_axis.Y;
			_spriteCorners[2] = _size.X - _axis.X;
			_spriteCorners[3] = _size.Y - _axis.Y;
			_dimensionsChanged = false;
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

            // Get applied transforms.
            GetParentTransform();

            posX1 = _spriteCorners[0];
            posX2 = _spriteCorners[2];
            posY1 = _spriteCorners[1];
            posY2 = _spriteCorners[3];

            // Scale horizontally if necessary.
			if (_finalScale.X != 1.0f)
			{
                posX1 *= _finalScale.X;
                posX2 *= _finalScale.X;
			}

			// Scale vertically.
			if (_finalScale.Y != 1.0f)
			{
                posY1 *= _finalScale.Y;
                posY2 *= _finalScale.Y;
			}

			// Calculate rotation if necessary.
			if (_finalRotation != 0.0f) 
			{
				float cosVal;		// Cached cosine.
				float sinVal;		// Cached sine.
				float angle;		// Angle in radians.

                angle = MathUtility.Radians(_finalRotation);
				cosVal = (float)Math.Cos(angle);
				sinVal = (float)Math.Sin(angle);

                _vertices[0].Position.X = (posX1 * cosVal - posY1 * sinVal);
                _vertices[0].Position.Y = (posX1 * sinVal + posY1 * cosVal);

                _vertices[1].Position.X = (posX2 * cosVal - posY1 * sinVal);
                _vertices[1].Position.Y = (posX2 * sinVal + posY1 * cosVal);

                _vertices[2].Position.X = (posX2 * cosVal - posY2 * sinVal);
                _vertices[2].Position.Y = (posX2 * sinVal + posY2 * cosVal);

                _vertices[3].Position.X = (posX1 * cosVal - posY2 * sinVal);
                _vertices[3].Position.Y = (posX1 * sinVal + posY2 * cosVal);
            }
			else
			{
				_vertices[0].Position.X = posX1;
				_vertices[0].Position.Y = posY1;
				_vertices[1].Position.X = posX2;
				_vertices[1].Position.Y = posY1;
				_vertices[2].Position.X = posX2;
				_vertices[2].Position.Y = posY2;
				_vertices[3].Position.X = posX1;
				_vertices[3].Position.Y = posY2;
			}

			// Translate.
			if (_finalPosition.X != 0.0f)
			{
                _vertices[0].Position.X += _finalPosition.X;
                _vertices[1].Position.X += _finalPosition.X;
                _vertices[2].Position.X += _finalPosition.X;
                _vertices[3].Position.X += _finalPosition.X;
			}

			if (_finalPosition.Y != 0.0f)
			{
                _vertices[0].Position.Y += _finalPosition.Y;
                _vertices[1].Position.Y += _finalPosition.Y;
                _vertices[2].Position.Y += _finalPosition.Y;
                _vertices[3].Position.Y += _finalPosition.Y;
			}

			// Adjust vertex offsets.
			_vertices[0].Position.X += _vertexOffsets[0].X;
			_vertices[0].Position.Y += _vertexOffsets[0].Y;
			_vertices[1].Position.X += _vertexOffsets[1].X;
			_vertices[1].Position.Y += _vertexOffsets[1].Y;
			_vertices[2].Position.X += _vertexOffsets[2].X;
			_vertices[2].Position.Y += _vertexOffsets[2].Y;
			_vertices[3].Position.X += _vertexOffsets[3].X;
			_vertices[3].Position.Y += _vertexOffsets[3].Y;
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
			tu = (_imagePosition.X + 0.5f) / Image.ActualWidth;
			tv = (_imagePosition.Y + 0.5f) / Image.ActualHeight;
			sizetu = (_imagePosition.X + _size.X) / Image.ActualWidth;
			sizetv = (_imagePosition.Y + _size.Y) / Image.ActualHeight;

			_vertices[0].TextureCoordinates = new Vector2D(tu, tv);
			_vertices[1].TextureCoordinates = new Vector2D(sizetu, tv);
			_vertices[2].TextureCoordinates = new Vector2D(sizetu, sizetv);
			_vertices[3].TextureCoordinates = new Vector2D(tu, sizetv);
			_imageCoordinatesChanged = false;
		}
		
		/// <summary>
		/// Function to save the sprite to a stream.
		/// </summary>
		/// <param name="stream">Stream to save the sprite into.</param>
		/// <param name="isXML">TRUE if this is an xml file, FALSE for binary.</param>
		/// <param name="fileImagePath">Substitute file image path.</param>
		public virtual void Save(Stream stream, bool isXML, string fileImagePath)
		{
			ISerializer spriteSerializer = null;		// Sprite serializer.
			string spriteImagePath = string.Empty;		// Path to the sprite image.
			string spritePath = string.Empty;			// Path for the sprite.

			try
			{
				// If we didn't specify a file path for the image, then use the actual image path.
				if ((fileImagePath == null) || (fileImagePath == string.Empty))
				{
					// Check for an image.
					if (Image != null)
					{
						if (Image.ImageType != ImageType.RenderTarget)
						{
							if (!Image.IsResource)
								spriteImagePath = Image.Filename;
							else
								spriteImagePath = Image.Name;
						}
						else
							spriteImagePath = "@RenderTarget";
					}
				}
				else
					spriteImagePath = fileImagePath;

				if (stream is FileStream)
					spritePath = ((FileStream)stream).Name;
				else
					spritePath = "@Memory.GorgonSprite";

				// Extract the directory name, if it's the same as the sprite, then just use the current directory.
				if ((spriteImagePath != string.Empty) && ((Path.GetDirectoryName(spriteImagePath) == Path.GetDirectoryName(spritePath)) || (Path.GetDirectoryName(spriteImagePath) == string.Empty)))
					spriteImagePath = @".\" + Path.GetFileName(spriteImagePath);

				// Serialize.
				if (isXML)
					spriteSerializer = new XMLSerializer(this, stream);				
				else
					spriteSerializer = new BinarySerializer(this, stream);

				// Don't close the file stream, leave that to the user.
				spriteSerializer.DontCloseStream = true;

				// Setup serializer.
				spriteSerializer.Parameters.AddParameter<string>("ImagePath", spriteImagePath);
				spriteSerializer.Serialize();

				_spritePath = spritePath;
			}
			catch (Exception ex)
			{
				throw new CannotSaveException(spritePath, GetType(), ex);
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
		/// <param name="fileImagePath">Substitute file image path.</param>
		public void Save(Stream stream, string fileImagePath)
		{
			Save(stream, false, fileImagePath);
		}

		/// <summary>
		/// Function to save the sprite to a stream.
		/// </summary>
		/// <param name="stream">Stream to save the sprite into.</param>
		/// <param name="isXML">TRUE if the file is XML, FALSE for binary.</param>
		public void Save(Stream stream, bool isXML)
		{
			Save(stream, isXML, string.Empty);
		}

		/// <summary>
		/// Function to save the binary sprite to a stream.
		/// </summary>
		/// <param name="stream">Stream to save the sprite into.</param>
		public void Save(Stream stream)
		{
			Save(stream, false, string.Empty);
		}

		/// <summary>
		/// Function to save the sprite to a file.
		/// </summary>
		/// <param name="fileName">Path and filename of the sprite.</param>
		/// <param name="isXML">TRUE if this is an xml file, FALSE for binary.</param>
		/// <param name="fileImagePath">Substitute file image path.</param>
		public virtual void Save(string fileName, bool isXML, string fileImagePath)
		{
			Stream stream = null;		// Stream to save into.

			if ((fileName == null) || (fileName.Trim().Length == 0))
				throw new InvalidFilenameException(null);

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
				Save(stream, isXML, fileImagePath);
			}
			catch (Exception ex)
			{
				throw new CannotSaveException(fileName, GetType(), ex);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to save the sprite to a file.
		/// </summary>
		/// <param name="fileName">Path and filename of the sprite.</param>
		/// <param name="isXML">TRUE to save as XML, FALSE to save as binary.</param>
		public void Save(string fileName, bool isXML)
		{
			Save(fileName, isXML, string.Empty);
		}

		/// <summary>
		/// Function to save the sprite to a file.
		/// </summary>
		/// <param name="fileName">Path and filename of the sprite.</param>
		/// <param name="fileImagePath">Substitute file image path.</param>
		public void Save(string fileName, string fileImagePath)
		{
			if (Path.GetExtension(fileName).ToLower() == ".xml")
				Save(fileName, true, fileImagePath);
			else
				Save(fileName, false, fileImagePath);
		}

		/// <summary>
		/// Function to save sprite to a file.
		/// </summary>
		/// <param name="fileName">Path and filename of the sprite.</param>
		public void Save(string fileName)
		{
			if (Path.GetExtension(fileName).ToLower() == ".xml")
				Save(fileName, true, string.Empty);
			else
				Save(fileName, false, string.Empty);
		}

		/// <summary>
		/// Function to save the sprite to a file system.
		/// </summary>
		/// <param name="fileSystem">File system to save the sprite into.</param>
		/// <param name="fileName">Path and filename of the sprite.</param>
		/// <param name="isXML">TRUE if this is an xml file, FALSE for binary.</param>
		public virtual void Save(FileSystem fileSystem, string fileName, bool isXML)
		{
			Save(fileSystem, fileName, isXML, string.Empty);
		}

		/// <summary>
		/// Function to save the sprite to a file system.
		/// </summary>
		/// <param name="fileSystem">File system to save the sprite into.</param>
		/// <param name="fileName">Path and filename of the sprite.</param>
		/// <param name="fileImagePath">Substitute file image path.</param>
		public virtual void Save(FileSystem fileSystem, string fileName, string fileImagePath)
		{
			if (Path.GetExtension(fileName).ToLower() == ".xml")
				Save(fileSystem, fileName, true, fileImagePath);
			else
				Save(fileSystem, fileName, false, fileImagePath);
		}

		/// <summary>
		/// Function to save the sprite to a file system.
		/// </summary>
		/// <param name="fileSystem">File system to save the sprite into.</param>
		/// <param name="fileName">Path and filename of the sprite.</param>
		public virtual void Save(FileSystem fileSystem, string fileName)
		{
			if (Path.GetExtension(fileName).ToLower() == ".xml")
				Save(fileSystem, fileName, true, string.Empty);
			else
				Save(fileSystem, fileName, false, string.Empty);
		}

		/// <summary>
		/// Function to save the sprite to a file system.
		/// </summary>
		/// <param name="fileSystem">File system to save the sprite into.</param>
		/// <param name="fileName">Path and filename of the sprite.</param>
		/// <param name="isXML">TRUE if this is an xml file, FALSE for binary.</param>
		/// <param name="fileImagePath">Substitute file image path.</param>
		public virtual void Save(FileSystem fileSystem, string fileName, bool isXML, string fileImagePath)
		{
			MemoryStream stream = null;		// Stream to save into.
			byte[] data = null;			// Binary data of the sprite.

			if ((fileName == null) || (fileName.Trim().Length == 0))
				throw new InvalidFilenameException(null);

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
				Save(stream, isXML, fileImagePath);
				stream.Position = 0;
				data = stream.ToArray();

				fileSystem.WriteFile(fileName, data);
			}
			catch (Exception ex)
			{
				throw new CannotSaveException(fileName, GetType(), ex);
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

			base.UpdateAABB();

			// Transform.
			UpdateTransform();

			for (int i = 0; i < 4;i++)
			{
				// Create bounding.			
				min.X = MathUtility.Min(min.X, _vertices[i].Position.X);
				min.Y = MathUtility.Min(min.Y, _vertices[i].Position.Y);
				max.X = MathUtility.Max(max.X, _vertices[i].Position.X);
				max.Y = MathUtility.Max(max.Y, _vertices[i].Position.Y);
			}

			_AABB.X = min.X;
			_AABB.Y = min.Y;
			_AABB.Width = max.X - min.X;
			_AABB.Height = max.Y - min.Y;

			_needAABBUpdate = false;
		}

		/// <summary>
		/// Function to set the position of a sprite vertex.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to change.</param>
		/// <param name="position">Position of the vertex.</param>
		public void SetSpriteVertexPosition(VertexLocations vertexPosition, Vector2D position)
		{
			SetSpriteVertexPosition(vertexPosition, position.X, position.Y);
		}

		/// <summary>
		/// Function to set the position of a sprite vertex.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to change.</param>
		/// <param name="x">Horizontal position of the vertex.</param>
		/// <param name="y">Vertical position of the vertex.</param>
		public void SetSpriteVertexPosition(VertexLocations vertexPosition, float x, float y)
		{
			if ((vertexPosition < VertexLocations.UpperLeft) || (vertexPosition > VertexLocations.LowerLeft))
				throw new SharpUtilities.Collections.IndexOutOfBoundsException("The sprite does not contain a vertex at the position " + ((int)vertexPosition).ToString(), SharpUtilities.Collections.CollectionErrorCodes.IndexOutOfBounds);

			_needAABBUpdate = true;
			_vertexOffsets[(int)vertexPosition].X = x;
			_vertexOffsets[(int)vertexPosition].Y = y;
			UpdateDimensions();
		}

		/// <summary>
		/// Function to set the color of a sprite vertex.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to retrieve.</param>
		public Vector2D GetSpriteVertexPosition(VertexLocations vertexPosition)
		{
			return _vertexOffsets[(int)vertexPosition];
		}

		/// <summary>
		/// Function to set the color of a sprite vertex.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to change.</param>
		/// <returns>Color of the sprite vertex.</returns>
		public Drawing.Color GetSpriteVertexColor(VertexLocations vertexPosition)
		{
			if ((vertexPosition < VertexLocations.UpperLeft) || (vertexPosition > VertexLocations.LowerLeft))
				throw new SharpUtilities.Collections.IndexOutOfBoundsException("The sprite does not contain a vertex at the position " + ((int)vertexPosition).ToString(), SharpUtilities.Collections.CollectionErrorCodes.IndexOutOfBounds);

			return Drawing.Color.FromArgb(_vertices[(int)vertexPosition].Color);
		}

		/// <summary>
		/// Function to set the color of a sprite vertex.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to change.</param>
		/// <param name="newColor">New color to set the vertex to.</param>
		public void SetSpriteVertexColor(VertexLocations vertexPosition, Drawing.Color newColor)
		{
			if ((vertexPosition < VertexLocations.UpperLeft) || (vertexPosition > VertexLocations.LowerLeft))
				throw new SharpUtilities.Collections.IndexOutOfBoundsException("The sprite does not contain a vertex at the position " + ((int)vertexPosition).ToString(), SharpUtilities.Collections.CollectionErrorCodes.IndexOutOfBounds);

			_vertices[(int)vertexPosition].Color = newColor.ToArgb();
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
			StateManager manager = Gorgon.StateManager;				// State manager.
			int start = manager.RenderData.VertexOffset;			// Vertex starting point.
			int count = manager.RenderData.VertexCount;				// Vertex count.
            IAnimatable child = null;								//Child object.

            // Draw the children.
            if (_children.Count > 0)
            {
                for (int i = 0; i < _children.Count; i++)
                {   
                    child = (IAnimatable)_children[i].Child;
                    if (child != null)
                        child.Draw(flush);
                }
            }

			// If we're at the end of the buffer, wrap around.
			if (((manager.RenderData.VerticesWritten + 4 >= count) || (Gorgon.Renderer.GetImage(0) != Image) || 
				(manager.StateChanged(this))) && (manager.RenderData.VerticesWritten !=0))
			{
				Gorgon.Renderer.Render();
				start = 0;
			}

            // Apply animations.
			if (_animations.Count > 0)
				((IAnimatable)this).ApplyAnimations();

			// Update the sprite dimensions and offsets if necessary.
			if (_dimensionsChanged)
				UpdateDimensions();
			if (_imageCoordinatesChanged)
				UpdateImageLayer();

			// Update the AABB.			
			if (_needAABBUpdate)
				UpdateAABB();
			else
				UpdateTransform();

			// Set the state for this sprite.
			manager.SetStates(this);

			// Set the currently active image.
			Gorgon.Renderer.SetImage(0, Image);

			// Write the data to the buffer.			
			manager.RenderData.VertexCache.WriteData(0, manager.RenderData.VertexOffset + manager.RenderData.VerticesWritten, _vertices.Length, _vertices);

			// Flush the current contents, this will slow things down greatly, so use sparingly.
			if (flush)
				Gorgon.Renderer.Render();
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

			Position = position;
			Size = size;
			
			Image = image;

			if (image != null)
				_spriteImageName = image.Name;
			else
				_spriteImageName = string.Empty;

			_scale = scale;			
			// Initialize the quad.
			_vertices = new VertexTypes.PositionDiffuse2DTexture1[4];
			_spriteCorners = new float[4];
			_vertexOffsets = new Vector2D[4];
			_axis = axis;
			_imagePosition = imageOffset;
			// Set sprite colors.
			Color = Drawing.Color.White;

			// Set to -1.0 Z.
			_vertices[0].Position.Z = -0.5f;
			_vertices[1].Position.Z = -0.5f;
			_vertices[2].Position.Z = -0.5f;
			_vertices[3].Position.Z = -0.5f;

			// Create animation list.
			_animations = new AnimationList(this);			

			UpdateDimensions();
			UpdateTransform();
			UpdateImageLayer();
			UpdateAABB();
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
				Size = new Vector2D(image.Width, image.Height);
			else
				Size = Vector2D.Zero;
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
		string ISerializable.Filename
		{
			get
			{
				return _spritePath;
			}
			set
			{
				if (value == null)
					value = string.Empty;

				_spritePath = value;
			}
		}

		/// <summary>
		/// Function to persist the data into the serializer stream.
		/// </summary>
		/// <param name="writer">Serializer that's calling this function.</param>
		void ISerializable.WriteData(ISerializer writer)
		{
			string spriteImagePath = string.Empty;		// Font image path.
			int[] vertexColor = new int[4];				// Vertex colors.

			if (writer.Parameters.Contains("ImagePath"))
				spriteImagePath = writer.Parameters.GetParameter<string>("ImagePath");

			writer.WriteComment("Gorgon Sprite - " + _objectName);
			writer.WriteComment("Written by Michael Winsor (Tape_Worm) for the Gorgon library.");
			writer.WriteComment("Use at your own peril.");

			// Root region.
			writer.WriteGroupBegin("GorgonSprite");

			// Write header.
			writer.WriteGroupBegin("Header");
			writer.Write<string>("HeaderValue", "GORSPR1");
			writer.WriteGroupEnd();

			// Write meta data.
			writer.WriteGroupBegin("MetaData");

			// Write name
			writer.Write<string>("Name", _objectName);

			writer.WriteGroupEnd();

			// Write sprite data.
			writer.WriteGroupBegin("Sprite");

			// Write image information.
			writer.WriteGroupBegin("Image");
			writer.Write<bool>("HasImage", Image != null);
			if ((spriteImagePath != string.Empty) || (Image != null))
			{
				// Check for render target.
				writer.Write<bool>("IsRenderTarget", Image.ImageType == ImageType.RenderTarget);
				if (Image.ImageType == ImageType.RenderTarget)
				{
					RenderImage renderImage = null;		// Render image.

					// Get the render image.
					renderImage = Image.RenderImage;

					if (renderImage == null)
						throw new InvalidResourceException("Sprite is bound to a render target, target does not exist.", null);
					writer.Write<int>("Format", (int)Image.Format);
					writer.Write<int>("SizeWidth", Image.Width);
					writer.Write<int>("SizeHeight", Image.Height);
					writer.Write<int>("TargetPriority", renderImage.Priority);
					writer.Write<bool>("HasDepthBuffer", renderImage.UseDepthBuffer);
					writer.Write<bool>("HasStencilBuffer", renderImage.UseStencilBuffer);
					writer.Write<string>("ImageName", renderImage.Name);
				}
				else
				{
					writer.Write<bool>("ImageIsResource", Image.IsResource);
					if (!Image.IsResource)
						writer.Write<string>("ImagePath", Path.GetDirectoryName(spriteImagePath) + @"\");
					writer.Write<string>("ImageName", Path.GetFileName(spriteImagePath));
				}				
			}
			writer.WriteGroupEnd();

			// Write inheritance settings.
			writer.WriteComment("These settings are used to determine if the sprite will inherit settings");
			writer.WriteComment("from its layer.");
			writer.WriteGroupBegin("InheritedSettings");
			writer.Write<bool>("AlphaMaskFunction", InheritAlphaMaskFunction);
			writer.Write<bool>("AlphaMaskValue", InheritAlphaMaskValue);
			writer.Write<bool>("BlendingMode", InheritBlending);
			writer.Write<bool>("HorizontalWrapping", InheritHorizontalWrapping);
			writer.Write<bool>("Smoothing", InheritSmoothing);
			writer.Write<bool>("StencilCompare", InheritStencilCompare);
			writer.Write<bool>("StencilEnabled", InheritStencilEnabled);
			writer.Write<bool>("StencilFailOperation", InheritStencilFailOperation);
			writer.Write<bool>("StencilMask", InheritStencilMask);
			writer.Write<bool>("StencilPassOperation", InheritStencilPassOperation);
			writer.Write<bool>("StencilReference", InheritStencilReference);
			writer.Write<bool>("StencilZFailOperation", InheritStencilZFailOperation);
			writer.Write<bool>("VerticalWrapping", InheritVerticalWrapping);
			writer.WriteGroupEnd();

			// Write dimensions.
			writer.WriteComment("Dimensions and offsets of the sprite.");
			writer.WriteGroupBegin("Dimensions");
			writer.Write<float>("Width", Width);
			writer.Write<float>("Height", Height);
			writer.Write<float>("OffsetX", ImageOffset.X);
			writer.Write<float>("OffsetY", ImageOffset.Y);
			writer.Write<float>("AxisX", Axis.X);
			writer.Write<float>("AxisY", Axis.Y);
			writer.Write<float>("Vertex1OffsetX", _vertexOffsets[0].X);
			writer.Write<float>("Vertex1OffsetY", _vertexOffsets[0].Y);
			writer.Write<float>("Vertex2OffsetX", _vertexOffsets[1].X);
			writer.Write<float>("Vertex2OffsetY", _vertexOffsets[1].Y);
			writer.Write<float>("Vertex3OffsetX", _vertexOffsets[2].X);
			writer.Write<float>("Vertex3OffsetY", _vertexOffsets[2].Y);
			writer.Write<float>("Vertex4OffsetX", _vertexOffsets[3].X);
			writer.Write<float>("Vertex4OffsetY", _vertexOffsets[3].Y);
			writer.WriteGroupEnd();

			// Write effects.
			writer.WriteComment("Effects on the sprite.");
			vertexColor[0] = GetSpriteVertexColor(VertexLocations.LowerLeft).ToArgb();
			vertexColor[1] = GetSpriteVertexColor(VertexLocations.LowerRight).ToArgb();
			vertexColor[2] = GetSpriteVertexColor(VertexLocations.UpperLeft).ToArgb();
			vertexColor[3] = GetSpriteVertexColor(VertexLocations.UpperRight).ToArgb();
			writer.WriteGroupBegin("Effects");
			writer.Write<int>("UpperLeftColor", vertexColor[2]);
			writer.Write<int>("UpperRightColor", vertexColor[3]);
			writer.Write<int>("LowerLeftColor", vertexColor[0]);
			writer.Write<int>("LowerRightColor", vertexColor[1]);
			// Write shader information.
			writer.Write<bool>("HasShader", Shader != null);
			if (Shader != null)
			{
				writer.Write<bool>("ShaderIsResource", Shader.IsResource);
				if (!Shader.IsResource)
					writer.Write<string>("ShaderPath", Path.GetDirectoryName(Shader.Filename) + @"\");
				writer.Write<string>("ShaderName", Path.GetFileName(Shader.Filename));
				writer.Write<bool>("HasTechnique", Shader.ActiveTechnique != null);
				if (Shader.ActiveTechnique != null)
					writer.Write<string>("Technique", Shader.ActiveTechnique.Name);				
			}

			if (!InheritAlphaMaskFunction)
				writer.Write<int>("AlphaMaskFunction", (int)AlphaMaskFunction);
			if (!InheritAlphaMaskValue)
				writer.Write<int>("AlphaMaskValue", AlphaMaskValue);
			if (!InheritBlending)
			{
				writer.Write<int>("DestinationBlend", (int)DestinationBlend);
				writer.Write<int>("SourceBlend", (int)SourceBlend);
				writer.Write<int>("BlendingMode", (int)BlendingMode);
			}
			if (!InheritHorizontalWrapping)
				writer.Write<int>("HorizontalWrapping", (int)HorizontalWrapMode);
			if (!InheritSmoothing)
				writer.Write<int>("Smoothing", (int)Smoothing);
			if (!InheritStencilCompare)
				writer.Write<int>("StencilCompare", (int)StencilCompare);
			if (!InheritStencilEnabled)
				writer.Write<bool>("StencilEnabled", StencilEnabled);
			if (!InheritStencilFailOperation)
				writer.Write<int>("StencilFailOperation", (int)StencilFailOperation);
			if (!InheritStencilMask)
				writer.Write<int>("StencilMask", StencilMask);
			if (!InheritStencilPassOperation)
				writer.Write<int>("StencilPassOperation", (int)StencilPassOperation);
			if (!InheritStencilReference)
				writer.Write<int>("StencilReference", StencilReference);
			if (!InheritStencilZFailOperation)
				writer.Write<int>("StencilZFailOperation", (int)StencilZFailOperation);
			if (!InheritVerticalWrapping)
				writer.Write<int>("VerticalWrapping", (int)VerticalWrapMode);
			writer.WriteGroupEnd();

            // Write animations.
            writer.WriteGroupBegin("Animations");
            writer.Write<int>("AnimationCount", _animations.Count);
            if (_animations.Count > 0)
            {
                // Write each animation.
                foreach (Animation animation in _animations)
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
		void ISerializable.ReadData(ISerializer reader)
		{
			string header = string.Empty;										// Header.
			string imagePath = string.Empty;									// Image path.
			string imageFileName = string.Empty;								// Image file name.
			string shaderPath = string.Empty;									// Path to the shader.
			string shaderFileName = string.Empty;								// File name of the shader.
			string technique = string.Empty;									// Active technique.
			bool hasImage = false;												// Flag to indicate whether an image is bound.
			bool hasShader = false;												// Flag to indicate whether a shader is bound.
			bool isImageResource = false;										// Flag to indicate that the image is a resource.
			bool isShaderResource = false;										// Flag to indicate that the shader is a resource.
			bool isRenderTarget = false;										// Flag to indicate that the image is a render target.
			int targetWidth = 0;												// Render target width.
			int targetHeight = 0;												// Render target height.
			int targetPriority = -1;											// Render target priority.
			bool targetUseDepth = false;										// Flag to indicate whether a render target uses a depth buffer or not.
			bool targetUseStencil = false;										// Flag to indicate whether a render target uses a stencil buffer or not.
			ImageBufferFormats targetFormat = ImageBufferFormats.BufferUnknown;	// Render target format.
			FileSystem fileSystem = null;										// File system.

			// Mark the sprite as changed.
			_imageCoordinatesChanged = true;
			_dimensionsChanged = true;
			_needAABBUpdate = true;

			// Retrieve the file system if it exists.
			if (reader.Parameters.Contains("FileSystem"))
				fileSystem = reader.Parameters.GetParameter<FileSystem>("FileSystem");

			header = reader.Read<string>("HeaderValue");

			if (header.ToLower() != "gorspr1")
				throw new InvalidResourceException("Unable to deserialize sprite.  This does not appear to be a Gorgon sprite.", null);

			// Get sprite data.
			_objectName = reader.Read<string>("Name");

			// Image data.
			hasImage = reader.Read<bool>("HasImage");
			if (hasImage)
			{
				isRenderTarget = reader.Read<bool>("IsRenderTarget");
				// If the image is a render target then get the settings.
				if (isRenderTarget)
				{
					targetFormat = (ImageBufferFormats)reader.Read<int>("Format");
					targetWidth = reader.Read<int>("SizeWidth");
					targetHeight = reader.Read<int>("SizeHeight");
					targetPriority = reader.Read<int>("TargetPriority");
					targetUseDepth = reader.Read<bool>("HasDepthBuffer");
					targetUseStencil = reader.Read<bool>("HasStencilBuffer");
				}
				else
				{
					isImageResource = reader.Read<bool>("ImageIsResource");
					if (!isImageResource)
						imagePath = reader.Read<string>("ImagePath");
				}
				imageFileName = reader.Read<string>("ImageName");
			}
           
			// Get inherited settings.
			InheritAlphaMaskFunction = reader.Read<bool>("AlphaMaskFunction");
			InheritAlphaMaskValue = reader.Read<bool>("AlphaMaskValue");
			InheritBlending = reader.Read<bool>("BlendingMode");
			InheritHorizontalWrapping = reader.Read<bool>("HorizontalWrapping");
			InheritSmoothing = reader.Read<bool>("Smoothing");
			InheritStencilCompare = reader.Read<bool>("StencilCompare");
			InheritStencilEnabled = reader.Read<bool>("StencilEnabled");
			InheritStencilFailOperation = reader.Read<bool>("StencilFailOperation");
			InheritStencilMask = reader.Read<bool>("StencilMask");
			InheritStencilPassOperation = reader.Read<bool>("StencilPassOperation");
			InheritStencilReference = reader.Read<bool>("StencilReference");
			InheritStencilZFailOperation = reader.Read<bool>("StencilZFailOperation");
			InheritVerticalWrapping = reader.Read<bool>("VerticalWrapping");

			// Get dimensions.
			_size.X = reader.Read<float>("Width");
			_size.Y = reader.Read<float>("Height");
			_imagePosition.X = reader.Read<float>("OffsetX");
			_imagePosition.Y = reader.Read<float>("OffsetY");			
			_axis.X = reader.Read<float>("AxisX");
			_axis.Y = reader.Read<float>("AxisY");
			_vertexOffsets[0].X = reader.Read<float>("Vertex1OffsetX");
			_vertexOffsets[0].Y = reader.Read<float>("Vertex1OffsetY");
			_vertexOffsets[1].X = reader.Read<float>("Vertex2OffsetX");
			_vertexOffsets[1].Y = reader.Read<float>("Vertex2OffsetY");
			_vertexOffsets[2].X = reader.Read<float>("Vertex3OffsetX");
			_vertexOffsets[2].Y = reader.Read<float>("Vertex3OffsetY");
			_vertexOffsets[3].X = reader.Read<float>("Vertex4OffsetX");
			_vertexOffsets[3].Y = reader.Read<float>("Vertex4OffsetY");

			// Effects.
			SetSpriteVertexColor(VertexLocations.UpperLeft, Drawing.Color.FromArgb(reader.Read<int>("UpperLeftColor")));
			SetSpriteVertexColor(VertexLocations.UpperRight, Drawing.Color.FromArgb(reader.Read<int>("UpperRightColor")));
			SetSpriteVertexColor(VertexLocations.LowerLeft, Drawing.Color.FromArgb(reader.Read<int>("LowerLeftColor")));
			SetSpriteVertexColor(VertexLocations.LowerRight, Drawing.Color.FromArgb(reader.Read<int>("LowerRightColor")));
			hasShader = reader.Read<bool>("HasShader");
			if (hasShader)
			{
				isShaderResource = reader.Read<bool>("ShaderIsResource");
				if (!isShaderResource)
					shaderPath = reader.Read<string>("ShaderPath");
				shaderFileName = reader.Read<string>("ShaderName");
				if (reader.Read<bool>("HasTechnique"))
					technique = reader.Read<string>("Technique");
			}

			if (!InheritAlphaMaskFunction)
				AlphaMaskFunction = (CompareFunctions)reader.Read<int>("AlphaMaskFunction");
			if (!InheritAlphaMaskValue)
				AlphaMaskValue = reader.Read<int>("AlphaMaskValue");
			if (!InheritBlending)
			{
				DestinationBlend = (AlphaBlendOperation)reader.Read<int>("DestinationBlend");
				SourceBlend = (AlphaBlendOperation)reader.Read<int>("SourceBlend");
				BlendingMode = (Blending)reader.Read<int>("BlendingMode");
			}
			if (!InheritHorizontalWrapping)
				HorizontalWrapMode = (ImageAddressing)reader.Read<int>("HorizontalWrapping");
			if (!InheritSmoothing)
				Smoothing = (Smoothing)reader.Read<int>("Smoothing");
			if (!InheritStencilCompare)
				StencilCompare = (CompareFunctions)reader.Read<int>("StencilCompare");
			if (!InheritStencilEnabled)
				StencilEnabled = reader.Read<bool>("StencilEnabled");
			if (!InheritStencilFailOperation)
				StencilFailOperation = (StencilOperations)reader.Read<int>("StencilFailOperation");
			if (!InheritStencilMask)
				StencilMask = reader.Read<int>("StencilMask");
			if (!InheritStencilPassOperation)
				StencilPassOperation = (StencilOperations)reader.Read<int>("StencilPassOperation");
			if (!InheritStencilReference)
				StencilReference = reader.Read<int>("StencilReference");
			if (!InheritStencilZFailOperation)
				StencilZFailOperation = (StencilOperations)reader.Read<int>("StencilZFailOperation");
			if (!InheritVerticalWrapping)
				VerticalWrapMode = (ImageAddressing)reader.Read<int>("VerticalWrapping");

			// Get shader.
			if (hasShader)
			{
				// Store the shader name.
				_spriteShaderName = Path.GetFileNameWithoutExtension(shaderFileName);

				// If the shader is already loaded, then return.
				if (Gorgon.Shaders.Contains(_spriteShaderName))
					Shader = Gorgon.Shaders[_spriteShaderName];
				else
				{
					if (shaderPath == @".\")
						shaderPath = Path.GetDirectoryName(_spritePath) + @"\";

					if (fileSystem == null)
					{
						// Load from disk if we're not a resource.
						if ((!reader.Parameters.Contains("ResourceManager")) && (!isShaderResource))
							Shader = Gorgon.Shaders.FromFile(shaderPath + shaderFileName);
						else
						{
							ResourceManager manager = null;		// Resource manager.

							// Load the image from a resource if this font is resource based.
							if (reader.Parameters.Contains("ResourceManager"))
							{
								manager = reader.Parameters.GetParameter<ResourceManager>("ResourceManager");
								Shader = Gorgon.Shaders.FromResource(_spriteShaderName, manager);
							}
							else
								Shader = Gorgon.Shaders.FromResource(_spriteShaderName);
						}
					}
					else
					{
						// Load from the active file system.
						if (fileSystem.FileExists(shaderPath + shaderFileName))
							Shader = Gorgon.Shaders.FromFileSystem(fileSystem, shaderPath + shaderFileName);
						else
						{
							// If we can't find the shader, then search for it in the index.
							// We can only load 1 instance of the particular shader, so it 
							// should be the first object we encounter with the name.
							FileSystemEntry shaderEntry = null;

							shaderEntry = fileSystem.FindFile(shaderFileName);
							if (shaderEntry != null)
								Shader = Gorgon.Shaders.FromFileSystem(fileSystem, shaderEntry.FullPath);
						}
					}
				}

				// Set the active technique if we can load the shader.
				if (Gorgon.Shaders.Contains(_spriteShaderName))
				{
					if ((Shader != null) && (technique != string.Empty) && (Shader.Techniques.Contains(technique)))
						Shader.ActiveTechnique = Shader.Techniques[technique];
				}
			}

			// Finally, grab the image.
			if (reader.Parameters.Contains("Image"))
				Image = reader.Parameters.GetParameter<Image>("Image");
			else
			{
				// Store the sprite image name.
				_spriteImageName = Path.GetFileNameWithoutExtension(imageFileName);

				// If this is a render target, then re-create it.
				if (isRenderTarget)
				{
					if (!Gorgon.RenderTargets.Contains(_spriteImageName))
						Image = Gorgon.RenderTargets.CreateRenderImage(_spriteImageName, targetWidth, targetHeight, targetFormat, targetUseDepth, targetUseStencil, targetPriority).Image;
					else
						Image = ((RenderImage)Gorgon.RenderTargets[_spriteImageName]).Image;
				}
				else
				{
					// If the image already exists, then use that.
					if (Gorgon.ImageManager.Contains(_spriteImageName))
						Image = Gorgon.ImageManager[_spriteImageName];
					else
					{
						// Adjust the image path.
						if (imagePath == @".\")
							imagePath = Path.GetDirectoryName(_spritePath) + @"\";

						if (fileSystem == null)
						{
							// Load from disk if we're not a resource.
							if ((!reader.Parameters.Contains("ResourceManager")) && (!isImageResource))
								Image = Gorgon.ImageManager.FromFile(imagePath + imageFileName);
							else
							{
								ResourceManager manager = null;		// Resource manager.

								// Load the image from a resource if this sprite or its image is resource based.
								if (reader.Parameters.Contains("ResourceManager"))
								{
									manager = reader.Parameters.GetParameter<ResourceManager>("ResourceManager");
									Image = Gorgon.ImageManager.FromResource(_spriteImageName, manager);
								}
								else
									Image = Gorgon.ImageManager.FromResource(_spriteImageName);
							}
						}
						else
						{
							// Load from the active file system.
							if (fileSystem.FileExists(imagePath + imageFileName))
								Image = Gorgon.ImageManager.FromFileSystem(fileSystem, imagePath + imageFileName);
							else
							{
								// If we can't find the image, then search for it in the index.
								// We can only load 1 instance of the particular image, so it 
								// should be the first object we encounter with the name.
								FileSystemEntry imageEntry = null;

								imageEntry = fileSystem.FindFile(imageFileName);
								if (imageEntry != null)
									Image = Gorgon.ImageManager.FromFileSystem(fileSystem, imageEntry.FullPath);
							}
						}
					}
				}
			}

            // Get animations.
            int animationCount = 0;             // Animation count.

            animationCount = reader.Read<int>("AnimationCount");
            if (animationCount > 0)
            {
                // Store the name.
                reader.Parameters.AddParameter<string>("ImageName", imageFileName);

                // Write each animation.
                for (int i = 0; i < animationCount; i++)
                {
                    Animation animation = new Animation("@EmptyAnimation", this, 0.0f);
                    ((ISerializable)animation).ReadData(reader);
                    _animations.Add(animation);
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
