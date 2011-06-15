#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Wednesday, April 30, 2008 1:52:49 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Drawing = System.Drawing;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A key frame dealing with images on renderable objects.
	/// </summary>
	/// <remarks>This class can only be used on a <see cref="GorgonLibrary.Graphics.Renderable"/> object.</remarks>
	public class KeyImage
		: KeyFrame
	{
		#region Variables.
		private string _deferredImage = string.Empty;		// Deferred image name.
		private Image _image = null;						// Image for the key.
		private Vector2D _imageOffset = Vector2D.Zero;		// Image offset.
		private Vector2D _imageSize = Vector2D.Zero;		// Image size.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type of data stored in the key.
		/// </summary>
		/// <value></value>
		public override Type DataType
		{
			get
			{
				return typeof(Image);
			}
		}

		/// <summary>
		/// Property to set or return the track that owns this key.
		/// </summary>
		/// <value></value>
		public override Track Owner
		{
			get
			{
				return base.Owner;
			}
			set
			{
				base.Owner = value;

				// Only apply to renderable objects.
				if ((Owner != null) && (!(Owner.Owner.Owner is Renderable)))
					throw new InvalidOperationException("The KeyImage type can only apply to Renderable objects.");
			}
		}

		/// <summary>
		/// Property to set or return the image for the key.
		/// </summary>
		public Image Image
		{
			get
			{
				if ((_image == null) && (_deferredImage != string.Empty))
				{
					if (ImageCache.Images.Contains(_deferredImage))
					{
						_image = ImageCache.Images[_deferredImage];
						_deferredImage = string.Empty;
					}
				}

				return _image;
			}
			set
			{
				_image = value;
				_deferredImage = string.Empty;
			}
		}

		/// <summary>
		/// Property to set or return the image offset for the key.
		/// </summary>
		public Vector2D ImageOffset
		{
			get
			{
				return _imageOffset;
			}
			set
			{
				_imageOffset = value;
			}
		}

		/// <summary>
		/// Property to set or return the image size for the key.
		/// </summary>
		public Vector2D ImageSize
		{
			get
			{
				return _imageSize;
			}
			set
			{
				_imageSize = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the data within the key frame.
		/// </summary>
		/// <param name="keyData">Interpolated key data used to help calculate data between keys.</param>
		protected internal override void UpdateKeyData(Track.NearestKeys keyData)
		{
		}

		/// <summary>
		/// Function to perform an update of the bound property.
		/// </summary>
		public override void Update()
		{
			Renderable renderObject = null;		// Renderable object.

			if (Owner == null)
				return;

			// Only apply to renderable objects.
			renderObject = Owner.Owner.Owner as Renderable;

			if (renderObject != null)
			{
				if (renderObject.Image != Image)
					renderObject.Image = Image;
				if (renderObject.ImageOffset != _imageOffset)
					renderObject.ImageOffset = _imageOffset;
				if (renderObject.Size != _imageSize)
					renderObject.Size = _imageSize;
			}
		}

		/// <summary>
		/// Function to retrieve data from the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		public override void ReadData(Serializer serializer)
		{
			string typeName = string.Empty;		// Type name of the key.

			typeName = serializer.ReadString("Type");
			if (string.Compare(typeName, "KeyImage", true) != 0)
				throw new GorgonException(GorgonErrors.CannotReadData, "Got an unexpected key type: " + typeName + ", expected: KeyImage");

			Time = serializer.ReadSingle("Time");
			_image = null;
			_deferredImage = serializer.ReadString("ImageName");
			_imageOffset = new Vector2D(serializer.ReadSingle("ImageOffsetX"), serializer.ReadSingle("ImageOffsetY"));
			_imageSize = new Vector2D(serializer.ReadSingle("ImageSizeX"), serializer.ReadSingle("ImageSizeY"));
		}

		/// <summary>
		/// Function to persist the data into the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		public override void WriteData(Serializer serializer)
		{
			serializer.WriteGroupBegin("KeyImage");
			serializer.Write("Type", "KeyImage");
			serializer.Write("Time", Time);
			if (_image != null)
				serializer.Write("ImageName", _image.Name);
			else
				serializer.Write("ImageName", string.Empty);
			serializer.Write("ImageOffsetX", _imageOffset.X);
			serializer.Write("ImageOffsetY", _imageOffset.Y);
			serializer.Write("ImageSizeX", _imageSize.X);
			serializer.Write("ImageSizeY", _imageSize.Y);
			serializer.WriteGroupEnd();
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public override KeyFrame Clone()
		{
			KeyImage clone = null;			// Cloned key.

			clone = new KeyImage(Time, _image);
			clone.ImageOffset = _imageOffset;
			clone.ImageSize = _imageSize;

			return clone;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="KeyImage"/> class.
		/// </summary>
		/// <param name="time">The time (in milliseconds) at which this keyframe exists within the track.</param>
		/// <param name="deferred">The name of the deferred image.</param>
		internal KeyImage(float time, string deferred)
			: base(time)
		{
			_image = null;
			_deferredImage = deferred;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="time">The time (in milliseconds) at which this keyframe exists within the track.</param>
		/// <param name="image">Image to bind to the key.</param>
		/// <param name="offset">The starting offset into the image to use when drawing.</param>
		/// <param name="size">The width and height of the area to use when drawing.</param>
		public KeyImage(float time, Image image, Vector2D offset, Vector2D size)
			: base(time)
		{
			_image = image;
			ImageOffset = offset;
			ImageSize = size;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KeyImage"/> class.
		/// </summary>
		/// <param name="time">The time (in milliseconds) at which this keyframe exists within the track.</param>
		/// <param name="image">Image to bind to the key.</param>
		/// <param name="x">The horizontal offset into the image to start at when drawing.</param>
		/// <param name="y">The vertical offset into the image to start at when drawing.</param>
		/// <param name="width">The width of the area to use when drawing.</param>
		/// <param name="height">The height of the area to use when drawing.</param>
		public KeyImage(float time, Image image, float x, float y, float width, float height)
			: this(time, image, new Vector2D(x, y), new Vector2D(width, height))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KeyImage"/> class.
		/// </summary>
		/// <param name="time">The time (in milliseconds) at which this keyframe exists within the track.</param>
		/// <param name="image">Image to bind to the key.</param>
		/// <param name="imageRegion">The region of the image to use when drawing.</param>
		public KeyImage(float time, Image image, Drawing.RectangleF imageRegion)
			: this(time, image, imageRegion.Location, imageRegion.Size)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KeyImage"/> class.
		/// </summary>
		/// <param name="time">The time (in milliseconds) at which this keyframe exists within the track.</param>
		/// <param name="image">Image to bind to the key.</param>
		public KeyImage(float time, Image image)
			: base(time)
		{
			_image = image;
			if (_image != null)
				ImageSize = new Vector2D(image.Width, image.Height);
		}
		#endregion
	}
}
