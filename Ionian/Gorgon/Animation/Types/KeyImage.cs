#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Wednesday, April 30, 2008 1:52:49 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
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
				if (renderObject.Image != _image)
					renderObject.Image = _image;
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
				throw new AnimationTypeMismatchException("serialized key type", string.Empty, "KeyImage", typeName);

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
		/// <param name="image">Image to bind to the key.</param>
		public KeyImage(float time, Image image)
			: base(time)
		{
			_image = image;
		}
		#endregion
	}
}
