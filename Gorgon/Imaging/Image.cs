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
// Created: Friday, May 19, 2006 10:18:50 PM
//
// Code snippets submitted by:
//   - Cycor (ImageLockBox.Write)
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.IO;
using System.Reflection;
using Drawing = System.Drawing;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;
using GorgonLibrary.Serialization;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing an image.
	/// </summary>
	public class Image
		: NamedObject, IDeviceStateObject, ISerializable, IDisposable
	{
		#region Classes.
		/// <summary>
		/// Value type containing information about a locked region.
		/// </summary>
		public class ImageLockBox
			: IDisposable
		{
			#region Variables.
			private Image _image = null;			// Image that owns this lock box.			
			private Drawing.Rectangle _region;		// Region that is locked.
			private int _pitch;						// Total width of the image in bytes.
			private int _bytesPerPixel;				// Number of bytes in a single pixel.
			private bool _readOnly;					// Flag to indicate that the data is read-only or read-write.
			private DX.DataStream _lockStream;		// Stream used by the lock.
			private bool _disposed = false;			// Flag to indicate whether this is disposed already or not.
			private D3D9.Surface _rtSurface = null;	// A temporary render target surface to lock.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to set or return the position in the locked data.
			/// </summary>
			public long LockPosition
			{
				get
				{
					if (_lockStream == null)
						return 0;
					return _lockStream.Position;
				}
				set
				{
					if (_lockStream == null)
						return;

					_lockStream.Position = value;
				}
			}

			/// <summary>
			/// Property to return the image that owns this lock box.
			/// </summary>
			public Image Owner
			{
				get
				{
					return _image;
				}
			}

			/// <summary>
			/// Property to return the locked region dimensions.
			/// </summary>
			public Drawing.Rectangle Region
			{
				get
				{
					return _region;
				}
				set
				{
					_region = value;
					if (_lockStream != null)
					{
						Unlock();
						Lock(false);
					}
				}
			}

			/// <summary>
			/// Property to return the image format.
			/// </summary>
			public ImageBufferFormats Format
			{
				get
				{
					return _image.Format;
				}
			}

			/// <summary>
			/// Property to return the total width in bytes of the image.
			/// </summary>
			public int Pitch
			{
				get
				{
					return _pitch;
				}
			}

			/// <summary>
			/// Property to return the number of bytes per pixel.
			/// </summary>
			public int BytesPerPixel
			{
				get
				{
					return _bytesPerPixel;
				}
			}

			/// <summary>
			/// Property to return whether the data is read-only or read-write.
			/// </summary>
			public bool ReadOnly
			{
				get
				{
					return _readOnly;
				}
				set
				{
					_readOnly = value;
					if (_lockStream != null)
					{
						Unlock();
						Lock(false);
					}
				}
			}

			/// <summary>
			/// Property to return the pitch for the locked region.
			/// </summary>
			/// <remarks>Use this to get the pitch value for the width of the region, this differs from <see cref="P:GorgonLibrary.Graphics.Image.ImageLockBox">Pitch</see> in that the Pitch property takes the entire width of the image into account, whereas this property only takes the width of the locked region.</remarks>
			public int RegionPitch
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to set or return image data.
			/// </summary>
			/// <remarks>This method does NOT perform any formatting on the data.</remarks>
			/// <param name="x">Horizontal positioning.</param>
			/// <param name="y">Vertical positioning.</param>
			/// <returns>Data at the position.</returns>
			public long this[int x, int y]
			{
				get
				{
					if (_lockStream == null)
						return 0;

					long lastPosition = _lockStream.Position;
					long value = 0;

					_lockStream.Position = (y * _pitch) + (x * _bytesPerPixel); // Linear position.

					// Get each byte.
					switch (_bytesPerPixel)
					{
						case 1:
							value = _lockStream.Read<byte>();
							break;
						case 2:
							value = _lockStream.Read<short>();
							break;
						case 4:
							value = _lockStream.Read<int>();
							break;
						default:
							value = _lockStream.Read<long>();
							break;
					}

					_lockStream.Position = lastPosition;

					return value;
				}
				set
				{
					if (_lockStream == null)
						return;

					long lastPosition = _lockStream.Position;
					_lockStream.Position = (y * _pitch) + (x * _bytesPerPixel); // Linear position.

					// Get each byte.
					switch (_bytesPerPixel)
					{
						case 1:
							_lockStream.Write<byte>((byte)(value & 0xFF));
							break;
						case 2:
							_lockStream.Write<short>((short)(value & 0xFFFF));
							break;
						case 4:
							_lockStream.Write<int>((int)(value & 0xFFFFFFFF));
							break;
						default:
							_lockStream.Write<long>(value);
							break;
					}

					_lockStream.Position = lastPosition;
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to unlock the image.
			/// </summary>
			public void Unlock()
			{
				// Paril, r349
				// FIXME
				// Commented this out because it seems to hinder some
				// activities in DeviceLost events.
				// Needs more testing to see if it is actually required.
				//if (Gorgon.Screen.DeviceNotReset)
				//	return;

				if (_lockStream == null)
					return;

				if (_image.D3DTexture == null)
					return;

				try
				{
					_lockStream.Dispose();
					if ((_rtSurface != null) && (_image.ImageType == ImageType.RenderTarget))
					{
						_rtSurface.UnlockRectangle();

						// Copy back to the render target.
						Gorgon.Screen.Device.UpdateSurface(_rtSurface, _image.RenderImage.SurfaceBuffer);
					}
					else
						_image.D3DTexture.UnlockRectangle(0);
				}
				finally
				{
					// Clear data.
					_lockStream = null;
					_pitch = 0;
					RegionPitch = 0;
					_bytesPerPixel = 0;

					// Remove ourselves from the list.
					if (_image._locks.Contains(this))
						_image._locks.Remove(this);
				}
			}

			/// <summary>
			/// Function to lock an image.
			/// </summary>
			/// <param name="discard">TRUE to discard the data, FALSE to leave alone.</param>
			public void Lock(bool discard)
			{
				// Paril, r349
				// FIXME
				// Commented this out because it seems to hinder some
				// activities in DeviceLost events.
				// Needs more testing to see if it is actually required.
				//if (Gorgon.Screen.DeviceNotReset)
				//	return;
				
				DX.DataRectangle lockData = null;  // Lock data.

				// If locked, then unlock and re-lock.
				if (_lockStream != null)
					Unlock();

                if (_image.D3DTexture == null)
					throw new GorgonException(GorgonErrors.CannotLock, "No backing store for this image.");

                try
				{
					D3D9.LockFlags flags = D3D9.LockFlags.None;		// Lock flags.

					if (_readOnly)
						flags |= D3D9.LockFlags.ReadOnly;
					if (discard)
						flags |= D3D9.LockFlags.Discard;

					// Lock the image.
					if (_image.ImageType != ImageType.RenderTarget)
					{
						if (discard)
						{
							_region = new Drawing.Rectangle(0, 0, _image.ActualWidth, _image.ActualHeight);
							lockData = _image.D3DTexture.LockRectangle(0, flags);
						}
						else
							lockData = _image.D3DTexture.LockRectangle(0, _region, flags);
					}
					else
					{						
						Gorgon.Screen.Device.GetRenderTargetData(_image.RenderImage.SurfaceBuffer, _rtSurface);

						if (discard)
						{
							_region = new Drawing.Rectangle(0, 0, _image.ActualWidth, _image.ActualHeight);
							lockData = _rtSurface.LockRectangle(flags);
						}
						else
							lockData = _rtSurface.LockRectangle(_region, flags);
					}

					// Get locked data.				
					_pitch = lockData.Pitch;
					_lockStream = lockData.Data;

					_bytesPerPixel = _pitch / _image.ActualWidth;
					RegionPitch = _region.Width * _bytesPerPixel;
					_image._locks.Add(this);
				}
				catch (Exception ex)
				{
					throw GorgonException.Repackage(GorgonErrors.CannotLock, ex);
				}
			}

			/// <summary>
			/// Function to read data directly from the lock stream.
			/// </summary>
			/// <param name="buffer">Buffer to fill with data from the lock stream.</param>
			/// <remarks>This function depends on where the stream pointer is positioned.  You can adjust the position in the lock stream by using the <see cref="P:GorgonLibrary.Graphics.ImageLockBox.LockPosition">LockPosition</see> property.
			/// <para>Unlike <see cref="M:GorgonLibrary.Graphics.Image.ImageLockBox.ReadRegion">ReadRegion</see>, this function will NOT respect the region specified in the lock.  The user will be responsible for keeping the data within the region.</para>
			/// </remarks>
			public void Read(byte[] buffer)
			{
				if (buffer == null)
					throw new ArgumentNullException("buffer");

				Read(buffer, 0, buffer.Length);
			}

			/// <summary>
			/// Function to read data directly from the lock stream.
			/// </summary>
			/// <param name="buffer">Buffer to fill with data from the lock stream.</param>
			/// <param name="length">Number of bytes to read.</param>
			/// <remarks>This function depends on where the stream pointer is positioned.  You can adjust the position in the lock stream by using the <see cref="P:GorgonLibrary.Graphics.ImageLockBox.LockPosition">LockPosition</see> property.
			/// <para>Unlike <see cref="M:GorgonLibrary.Graphics.Image.ImageLockBox.ReadRegion">ReadRegion</see>, this function will NOT respect the region specified in the lock.  The user will be responsible for keeping the data within the region.</para>
			/// </remarks>
			public void Read(byte[] buffer, int length)
			{
				if (buffer == null)
					throw new ArgumentNullException("buffer");

				Read(buffer, 0, length);
			}

			/// <summary>
			/// Function to read data directly from the lock stream.
			/// </summary>
			/// <param name="buffer">Buffer to fill with data from the lock stream.</param>
			/// <param name="offset">Offset into the <paramref name="buffer"/> to start writing at.</param>
			/// <param name="length">Number of bytes to read.</param>
			/// <remarks>This function depends on where the stream pointer is positioned.  You can adjust the position in the lock stream by using the <see cref="P:GorgonLibrary.Graphics.ImageLockBox.LockPosition">LockPosition</see> property.
			/// <para>Unlike <see cref="M:GorgonLibrary.Graphics.Image.ImageLockBox.ReadRegion">ReadRegion</see>, this function will NOT respect the region specified in the lock.  The user will be responsible for keeping the data within the region.</para>
			/// </remarks>
			public void Read(byte[] buffer, int offset, int length)
			{
				if (buffer == null)
					throw new ArgumentNullException("buffer");

				_lockStream.Read(buffer, offset, length);
			}

			/// <summary>
			/// Function to write data directly to the lock stream.
			/// </summary>
			/// <param name="buffer">Buffer containing data to write.</param>
			/// <remarks>This function depends on where the stream pointer is positioned.  You can adjust the position in the lock stream by using the <see cref="P:GorgonLibrary.Graphics.ImageLockBox.LockPosition">LockPosition</see> property.
			/// <para>Unlike <see cref="M:GorgonLibrary.Graphics.Image.ImageLockBox.ReadRegion">ReadRegion</see>, this function will NOT respect the region specified in the lock.  The user will be responsible for keeping the data within the region.</para>
			/// </remarks>
			public void Write(byte[] buffer)
			{
				if (buffer == null)
					throw new ArgumentNullException("buffer");

				Write(buffer, 0, buffer.Length);
			}

			/// <summary>
			/// Function to write data directly to the lock stream.
			/// </summary>
			/// <param name="buffer">Buffer containing data to write.</param>
			/// <param name="length">Number of bytes to write.</param>
			/// <remarks>This function depends on where the stream pointer is positioned.  You can adjust the position in the lock stream by using the <see cref="P:GorgonLibrary.Graphics.ImageLockBox.LockPosition">LockPosition</see> property.
			/// <para>Unlike <see cref="M:GorgonLibrary.Graphics.Image.ImageLockBox.ReadRegion">ReadRegion</see>, this function will NOT respect the region specified in the lock.  The user will be responsible for keeping the data within the region.</para>
			/// </remarks>
			public void Write(byte[] buffer, int length)
			{
				if (buffer == null)
					throw new ArgumentNullException("buffer");

				Write(buffer, 0, length);
			}
			
			/// <summary>
			/// Function to write data directly to the lock stream.
			/// </summary>
			/// <param name="buffer">Buffer containing data to write.</param>
			/// <param name="offset">Offset into the <paramref name="buffer"/> to start reading from.</param>
			/// <param name="length">Number of bytes to write.</param>
			/// <remarks>This function depends on where the stream pointer is positioned.  You can adjust the position in the lock stream by using the <see cref="P:GorgonLibrary.Graphics.ImageLockBox.LockPosition">LockPosition</see> property.
			/// <para>Unlike <see cref="M:GorgonLibrary.Graphics.Image.ImageLockBox.ReadRegion">ReadRegion</see>, this function will NOT respect the region specified in the lock.  The user will be responsible for keeping the data within the region.</para>
			/// </remarks>
			public void Write(byte[] buffer, int offset, int length)
			{
				if (buffer == null)
					throw new ArgumentNullException("buffer");

				_lockStream.Write(buffer, offset, length);
			}

			/// <summary>
			/// Function to write data into the locked region.
			/// </summary>
			/// <param name="buffer">The buffer containing the image data to write to the region.</param>
			/// <remarks>Ensure that the buffer size is at least equal to that of the locked region (in bytes).</remarks>
			public void WriteRegion(byte[] buffer)
			{
				int offset = 0;
				long lastPosition = _lockStream.Position;

				if (buffer == null)
					throw new ArgumentNullException("buffer");

				if (buffer.Length < RegionPitch * _region.Height)
					throw new ArgumentException("The buffer is too small for the region.", "buffer");

				// If we've locked the entire image, read it all in one shot.
				if ((_region.Width == _image.ActualWidth) && (_region.Height == _image.ActualHeight) && (_region.X == 0) && (_region.Y == 0))
				{
					if (buffer.Length > RegionPitch * _region.Height)
						throw new ArgumentException("The buffer is too large for the locked region.", "buffer");

					_lockStream.Position = 0;
					_lockStream.Write(buffer, 0, buffer.Length);
				}
				else
				{
					for (int y = 0; y < _region.Height; y++)
					{
						_lockStream.Position = (y * _pitch);
						_lockStream.Write(buffer, offset, RegionPitch);
						offset += RegionPitch;
					}
				}

				_lockStream.Position = lastPosition;
			}

			/// <summary>
			/// Function to read data from the locked region on the image.
			/// </summary>
			/// <returns>The image data in the locked region.</returns>
			public byte[] ReadRegion()
			{
				int offset = 0;
				long lastPosition = _lockStream.Position;
				byte[] result = new byte[RegionPitch * _region.Height];

				// If we've locked the entire image, read it all in one shot.
				if ((_region.Width == _image.ActualWidth) && (_region.Height == _image.ActualHeight) && (_region.X == 0) && (_region.Y == 0))
				{
					_lockStream.Position = 0;
					_lockStream.Read(result, 0, result.Length);
				}
				else
				{
					for (int y = 0; y < _region.Height; y++)
					{
						_lockStream.Position = (y * _pitch);
						_lockStream.Read(result, offset, RegionPitch);
						offset += RegionPitch;
					}
				}

				_lockStream.Position = lastPosition;

				return result;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="owner">Image that owns this lock box.</param>
			/// <param name="region">Locked region.</param>
			internal ImageLockBox(Image owner, Drawing.Rectangle region)
			{
				_image = owner;
				_region = region;

				// Create the render target work surface.
				if (owner.ImageType == ImageType.RenderTarget)
					_rtSurface = D3D9.Surface.CreateOffscreenPlain(Gorgon.Screen.Device, _image.ActualWidth, _image.ActualHeight, Converter.Convert(_image.Format), D3D9.Pool.SystemMemory);

				// Auto-lock.
				Lock(false);
			}
			#endregion

			#region IDisposable Members
			/// <summary>
			/// Function to perform clean up.
			/// </summary>
			/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
			protected virtual void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (!_disposed)
					{
						if (_lockStream != null)
							Unlock();

						if (_rtSurface != null)
							_rtSurface.Dispose();
					}

					_rtSurface = null;
					_disposed = true;
				}
			}

			/// <summary>
			/// Function to perform clean up.
			/// </summary>
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			#endregion
		}
		#endregion

		#region Variables.
		private string _filename;					                    // Filename of the image.				
		private D3D9.Texture _d3dImage = null;		                    // Direct 3D image.
		private int _width;							                    // Width of the image.		
		private int _height;						                    // Height of the image.		
		private ImageBufferFormats _format;			                    // Format of the image.		
		private int _actualWidth;					                    // Actual width of the image.		
		private int _actualHeight;					                    // Actual height of the image.
		private ImageType _imageType;				                    // Type of image this object represents.
		private Drawing.Color _fillColor;			                    // Color used to fill a texture.
		private bool _isResource;					                    // Image is a resource object.
		private List<ImageLockBox> _locks;			                    // Image locks.
		private bool _clearOnInit;					                    // Flag to clear the image upon initialization.
		private Sprite _blitter = null;				                    // Blitter sprite.
		private bool _disposed = false;				                    // Flag to indicate whether an object is disposed already or not.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the memory pool for the image.
		/// </summary>
		private D3D9.Pool Pool
		{
			get
			{
				// Set up for dynamic images.
				if ((_imageType == ImageType.Dynamic) || (_imageType == ImageType.RenderTarget))
					return D3D9.Pool.Default;

				return D3D9.Pool.Managed;
			}
		}

		/// <summary>
		/// Property to return the usage flags for the image.
		/// </summary>
		private D3D9.Usage Usage
		{
			get
			{
				D3D9.Usage flags = D3D9.Usage.None;		// Usage flags.

				// Set up for dynamic images.
				if ((_imageType == ImageType.Dynamic) && (Gorgon.CurrentDriver.SupportDynamicTextures))
					flags |= D3D9.Usage.Dynamic;

				// Force flags and pool for render targets.
				if (_imageType == ImageType.RenderTarget)
					flags = D3D9.Usage.RenderTarget;

				return flags;
			}
		}

		/// <summary>
		/// Property to return the Direct 3D image.
		/// </summary>
		internal D3D9.Texture D3DTexture
		{
			get
			{
				return _d3dImage;
			}
			set
			{
				D3D9.SurfaceDescription surfaceDesc;			// Surface information.

				if (_d3dImage != null)
					_d3dImage.Dispose();
				_d3dImage = value;

				// Get texture information.
				if (value != null)
				{
					surfaceDesc = _d3dImage.GetLevelDescription(0);

					_width = _actualWidth = surfaceDesc.Width;
					_height = _actualHeight = surfaceDesc.Height;
					_format = Converter.ConvertD3DImageFormat(surfaceDesc.Format);
				}
			}
		}

        /// <summary>
		/// Property to return the render image that uses this image.
		/// </summary>
		public RenderImage RenderImage
		{
			get
			{
				if (_imageType != ImageType.RenderTarget)
					return null;

				// Find the target.
				foreach (RenderTarget target in RenderTargetCache.Targets)
				{
					RenderImage renderImage = target as RenderImage;		// Try to convert to a render image.

					if ((renderImage != null) && (renderImage.Image == this))
						return renderImage;
				}

				return null;
			}
		}

		/// <summary>
		/// Property to set or return whether the buffer should be cleared on creation/reset or not.
		/// </summary>
		/// <remarks>Setting this to TRUE will cause a significant delay on device reset or image creation.</remarks>
		public bool ClearBufferOnCreateOrReset
		{
			get
			{
				return _clearOnInit;
			}
			set
			{
				_clearOnInit = true;
			}
		}

		/// <summary>
		/// Property to return whether this image was loaded from a resource object or not.
		/// </summary>
		public bool IsResource
		{
			get
			{
				return _isResource;
			}
		}

		/// <summary>
		/// Property to return the type of image this object represents.
		/// </summary>
		public ImageType ImageType
		{
			get
			{
				return _imageType;
			}
		}

		/// <summary>
		/// Property to set or return the width of the image.
		/// </summary>
		public int Width
		{
			get
			{
				return _width;
			}
			set
			{
				SetDimensions(value, _height, _format, true);
			}
		}

		/// <summary>
		/// Property to set or return the height of the image.
		/// </summary>
		public int Height
		{
			get
			{
				return _height;
			}
			set
			{
				SetDimensions(_width, value, _format, true);
			}
		}

		/// <summary>
		/// Property to set or return the format of the image.
		/// </summary>
		public ImageBufferFormats Format
		{
			get
			{
				return _format;
			}
			set
			{
				SetDimensions(_width, _height, value, true);
			}
		}

		/// <summary>
		/// Property to return the actual width of the image.
		/// </summary>
		public int ActualWidth
		{
			get
			{
				return _actualWidth;
			}
		}

		/// <summary>
		/// Property to return the actual height of the image.
		/// </summary>
		public int ActualHeight
		{
			get
			{
				return _actualHeight;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when a texture is filled.
		/// </summary>
		/// <param name="textureCoordinate">Texture coordinate.</param>
		/// <param name="textureSize">Size of the texel.</param>
		private DX.Color4 ClearTextureCallback(DX.Vector2 textureCoordinate, DX.Vector2 textureSize)
		{
			return new DX.Color4(_fillColor);
		}

		/// <summary>
		/// Function to create an empty image.
		/// </summary>
		private void Initialize()
		{
			if (Gorgon.Screen == null)
				throw new GorgonException(GorgonErrors.NoDevice);

			D3D9.SurfaceDescription surfaceDesc = default(D3D9.SurfaceDescription);			// Surface description.

			// Destroy the previous image object.
			if (_d3dImage != null)
				_d3dImage.Dispose();
			_d3dImage = null;

			Gorgon.Log.Print("Image", "Creating image \"{0}\": {1}x{2}, Format: {3}.", LoggingLevel.Simple, Name, _width, _height, _format.ToString());

			// Start out with the same size.
			_actualWidth = _width;
			_actualHeight = _height;

			// If the device only supports square images, then make square.
			if (!Gorgon.CurrentDriver.SupportNonSquareTexture)
			{
				if (_actualWidth > _actualHeight)
					_actualHeight = _actualWidth;
				else
					_actualWidth = _actualHeight;
			}

			// Resize to the power of two.
			if ((!Gorgon.CurrentDriver.SupportNonPowerOfTwoTexture) && (!Gorgon.CurrentDriver.SupportNonPowerOfTwoTextureConditional))
			{
				Drawing.Size newSize;		// New image size.

				newSize = ResizePowerOf2(_actualWidth, _actualHeight);
				_actualWidth = newSize.Width;
				_actualHeight = newSize.Height;
			}

			// Ensure the image is the correct size.
			if ((_actualWidth > Gorgon.CurrentDriver.MaximumTextureWidth) || (_actualHeight > Gorgon.CurrentDriver.MaximumTextureHeight) || (_actualWidth < 0) || (_actualHeight < 0))
				throw new GorgonException(GorgonErrors.CannotCreate, "The image size of " + _actualWidth.ToString() + "x" + _actualHeight.ToString() + " is too large/small for the video card.");

			// If we specified unknown for the buffer format, assume 32 bit.
			if (_format == ImageBufferFormats.BufferUnknown)
			{
				_format = ValidateFormat(ImageBufferFormats.BufferRGB888A8, _imageType);

				if (!SupportsFormat(_format, _imageType))
					throw new GorgonException(GorgonErrors.InvalidFormat);
			}
			else
			{
				// Throw an exception if the format requested is not supported.
				if (!SupportsFormat(_format, _imageType))
					throw new GorgonException(GorgonErrors.InvalidFormat, _format.ToString() + " is not a valid format.");
			}

			// Create the image.
			try
			{
				_d3dImage = new D3D9.Texture(Gorgon.Screen.Device,
					_actualWidth, _actualHeight, 1,
					Usage,
					Converter.Convert(_format),
					Pool);

				surfaceDesc = _d3dImage.GetLevelDescription(0);
			}
			catch(Exception ex)
			{
				throw GorgonException.Repackage(GorgonErrors.CannotCreate, "Unable to create the D3D texture object.", ex);
			}

			_format = Converter.ConvertD3DImageFormat(surfaceDesc.Format);
			_filename = string.Empty;
			_isResource = false;
			Gorgon.Log.Print("Image", "Image \"{0}\" created: {1}x{2} (actual texture size: {4}x{5}), Format: {3}.", LoggingLevel.Simple, Name, _width, _height, _format.ToString(), _actualWidth, _actualHeight);

			// Clear the buffer.
			if (_clearOnInit)
				Clear(Drawing.Color.FromArgb(255, 0, 255));
		}

		/// <summary>
		/// Function to return an image from a stream.
		/// </summary>
		/// <param name="name">Image name or path.</param>
		/// <param name="fileSystem">A file system that contains the image.</param>
		/// <param name="resources">A resource manager that is used to load the file(s).</param>
		/// <param name="stream">Stream that contains the object.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>
		/// <param name="width">Width of the image.</param>
		/// <param name="height">Height of the image.</param>
		/// <param name="bytes">Size in bytes of the image.</param>
		/// <param name="format">Format of the image buffer.</param>
		/// <param name="colorKey">Color key to use for transparency.</param>
		/// <returns>The object contained within the stream.</returns>
		private static Image ImageFromStream(string name, FileSystem fileSystem, ResourceManager resources, Stream stream, bool isXML, int width, int height, int bytes, ImageBufferFormats format, Drawing.Color colorKey)
		{
			Image newImage = null;						// New image.
			ImageSerializer serializer = null;			// Serializer.
			string imagePath = string.Empty;			// Path to the image.

			try
			{
				// Get the path.
				imagePath = name;

				// Get the image name.
				name = Path.GetFileNameWithoutExtension(name);

				// If the image already exists, then return it.
				if (ImageCache.Images.Contains(name))
					return ImageCache.Images[name];

				// Create the Image.
				newImage = new Image(name, ImageType.Normal, 1, 1, ImageBufferFormats.BufferUnknown, false);
				newImage._isResource = (resources != null);
				newImage._filename = imagePath;

				// Create image serializer.
				serializer = new ImageSerializer(newImage, stream);

				// Add parameters.
				serializer.Parameters["byteSize"] = bytes;
				serializer.Parameters["Width"] = width;
				serializer.Parameters["Height"] = height;
				serializer.Parameters["Format"] = format;
				serializer.Parameters["ColorKey"] = colorKey;
				serializer.Deserialize();

				ImageCache.Images.Add(newImage);
				return newImage;
			}
			catch
			{
				if (newImage != null)
					newImage.Dispose();
				newImage = null;
				throw;
			}
			finally
			{
				if (serializer != null)
					serializer.Dispose();
				serializer = null;
			}
		}

		/// <summary>
		/// Function to return the best D3D format from the given Gorgon format.
		/// </summary>
		/// <param name="format">Gorgon format to convert.</param>
		/// <param name="type">Type of image.</param>
		/// <returns>The best matching supported format for the image.</returns>
		public static ImageBufferFormats ValidateFormat(ImageBufferFormats format, ImageType type)
		{
			D3D9.TextureRequirements requires;		// Texture requirements.
			D3D9.Pool pool;							// Resource pool.

			if (Gorgon.Screen == null)
				throw new GorgonException(GorgonErrors.NoDevice);

			if ((type == ImageType.RenderTarget) || (type == ImageType.Dynamic))
				pool = D3D9.Pool.Default;
			else
				pool = D3D9.Pool.Managed;

			// Get the requirements.
			requires = D3D9.Texture.CheckRequirements(Gorgon.Screen.Device, 0, 0, 0, D3D9.Usage.None, Converter.Convert(format), pool);

			// Return the best matching format.
			return Converter.ConvertD3DImageFormat(requires.Format);
		}

		/// <summary>
		/// Function to load a Image from the disk.
		/// </summary>
		/// <param name="filename">Name and path of the file to load.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromFile(string filename)
		{
			return FromFile(filename, 0, 0, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to load a Image from the disk.
		/// </summary>
		/// <param name="filename">Name and path of the file to load.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromFile(string filename, ImageBufferFormats format)
		{
			return FromFile(filename, 0, 0, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to load a Image from the disk.
		/// </summary>
		/// <param name="filename">Name and path of the file to load.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <param name="colorKey">Color to use as transparent.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromFile(string filename, ImageBufferFormats format, Drawing.Color colorKey)
		{
			return FromFile(filename, 0, 0, format, colorKey);
		}

		/// <summary>
		/// Function to load a Image from disk.
		/// </summary>
		/// <param name="filename">Filename of the Image to load.</param>
		/// <param name="width">Requested width of the Image.</param>
		/// <param name="height">Requested height of the Image.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromFile(string filename, int width, int height, ImageBufferFormats format)
		{
			return FromFile(filename, width, height, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to load a Image from disk.
		/// </summary>
		/// <param name="filename">Filename of the Image to load.</param>
		/// <param name="width">Requested width of the Image.</param>
		/// <param name="height">Requested height of the Image.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <param name="colorKey">Color to use as transparent.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromFile(string filename, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			Stream stream = null;			// File stream.

			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			try
			{
				// Open the stream.
				stream = File.OpenRead(filename);
				return ImageFromStream(filename, null, null, stream, false, width, height, -1, format, colorKey);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to load a Image from a file system.
		/// </summary>
		/// <param name="fileSystem">File system that contains the image.</param>
		/// <param name="filename">Filename of the Image to load.</param>
		/// <param name="width">Requested width of the Image.</param>
		/// <param name="height">Requested height of the Image.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <param name="colorKey">Color to use as transparent.</param>
		/// <returns>The image contained within the file system.</returns>
		public static Image FromFileSystem(FileSystem fileSystem, string filename, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			Stream stream = null;			// File stream.

			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");

			try
			{
				// Open the stream.
				stream = new MemoryStream(fileSystem.ReadFile(filename));

				return ImageFromStream(filename, fileSystem, null, stream, false, width, height, -1, format, colorKey);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to load a Image from a file system.
		/// </summary>
		/// <param name="fileSystem">File system that contains the image.</param>
		/// <param name="filename">Filename of the Image to load.</param>
		/// <param name="width">Requested width of the Image.</param>
		/// <param name="height">Requested height of the Image.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <returns>The image contained within the file system.</returns>
		public static Image FromFileSystem(FileSystem fileSystem, string filename, int width, int height, ImageBufferFormats format)
		{
			return FromFileSystem(fileSystem, filename, width, height, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load a Image from a file system.
		/// </summary>
		/// <param name="fileSystem">File system that contains the image.</param>
		/// <param name="filename">Filename of the Image to load.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <param name="colorKey">Color to use as transparent.</param>
		/// <returns>The image contained within the file system.</returns>
		public static Image FromFileSystem(FileSystem fileSystem, string filename, ImageBufferFormats format, Drawing.Color colorKey)
		{
			return FromFileSystem(fileSystem, filename, format, colorKey);
		}

		/// <summary>
		/// Function to load a Image from a file system.
		/// </summary>
		/// <param name="fileSystem">File system that contains the image.</param>
		/// <param name="filename">Filename of the Image to load.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <returns>The image contained within the file system.</returns>
		public static Image FromFileSystem(FileSystem fileSystem, string filename, ImageBufferFormats format)
		{
			return FromFileSystem(fileSystem, filename, 0, 0, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load a Image from a file system.
		/// </summary>
		/// <param name="fileSystem">File system that contains the image.</param>
		/// <param name="filename">Filename of the Image to load.</param>
		/// <returns>The image contained within the file system.</returns>
		public static Image FromFileSystem(FileSystem fileSystem, string filename)
		{
			return FromFileSystem(fileSystem, filename, 0, 0, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromResource(string name, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			return FromResource(name, null, width, height, format, colorKey);
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromResource(string name, int width, int height, ImageBufferFormats format)
		{
			return FromResource(name, null, width, height, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromResource(string name, ImageBufferFormats format, Drawing.Color colorKey)
		{
			return FromResource(name, null, 0, 0, format, colorKey);
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromResource(string name, ImageBufferFormats format)
		{
			return FromResource(name, null, 0, 0, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromResource(string name)
		{
			return FromResource(name, null, 0, 0, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromResource(string name, ResourceManager resourceManager, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			Stream memStream = null;			// Memory stream for the resource.
			Assembly assembly = null;			// Assembly object.
			Drawing.Bitmap imageData = null;	// Image data.

			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			try
			{
				// Default to the calling application resource manager.
				if (resourceManager == null)
				{
					// Extract the resource manager from the calling assembly.
					assembly = Assembly.GetEntryAssembly();
					resourceManager = new ResourceManager(assembly.GetName().Name + ".Properties.Resources", assembly);
				}

				// Get the resource memory stream.
				imageData = (Drawing.Bitmap)resourceManager.GetObject(name);

				// Inherit the width and height from the bitmap.
				if (width == 0)
					width = imageData.Width;
				if (height == 0)
					height = imageData.Height;

				memStream = new MemoryStream();
				imageData.Save(memStream, Drawing.Imaging.ImageFormat.Png);
				memStream.Position = 0;
				return ImageFromStream(name, null, resourceManager, memStream, false, width, height, -1, format, colorKey);
			}
			finally
			{
				if (memStream != null)
					memStream.Dispose();
				memStream = null;
			}
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromResource(string name, ResourceManager resourceManager, int width, int height, ImageBufferFormats format)
		{
			return FromResource(name, resourceManager, width, height, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromResource(string name, ResourceManager resourceManager, ImageBufferFormats format)
		{
			return FromResource(name, resourceManager, 0, 0, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromResource(string name, ResourceManager resourceManager, ImageBufferFormats format, Drawing.Color colorKey)
		{
			return FromResource(name, resourceManager, 0, 0, format, colorKey);
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromResource(string name, ResourceManager resourceManager)
		{
			return FromResource(name, resourceManager, 0, 0, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to create a texture from a GDI+ image.
		/// </summary>
		/// <param name="name">Name of the image.</param>
		/// <param name="image">Image to retrieve data from.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromBitmap(string name, Drawing.Image image, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			Image newImage = null;			// New Image.

			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (image == null)
				throw new ArgumentNullException("image");

			try
			{
				if (!ImageCache.Images.Contains(name))
					newImage = new Image(name, width, height, format);
				else
					newImage = ImageCache.Images[name];

				// Copy the GDI image into our image.
				newImage.Copy(image, width, height, format, colorKey);
			}
			catch
			{
				if (newImage != null)
					newImage.Dispose();
				throw;
			}

			return newImage;
		}

		/// <summary>
		/// Function to create a texture from a GDI+ image.
		/// </summary>
		/// <param name="name">Name of the image.</param>
		/// <param name="image">Image to retrieve data from.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromBitmap(string name, Drawing.Image image, int width, int height, ImageBufferFormats format)
		{
			return FromBitmap(name, image, width, height, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to create a texture from a GDI+ image.
		/// </summary>
		/// <param name="name">Name of the image.</param>
		/// <param name="image">Image to retrieve data from.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color used for transparency.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromBitmap(string name, Drawing.Image image, ImageBufferFormats format, Drawing.Color colorKey)
		{
			if (image == null)
				throw new ArgumentNullException("image");

			return FromBitmap(name, image, image.Width, image.Height, ImageBufferFormats.BufferUnknown, colorKey);
		}

		/// <summary>
		/// Function to create a texture from a GDI+ image.
		/// </summary>
		/// <param name="name">Name of the image.</param>
		/// <param name="image">Image to retrieve data from.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromBitmap(string name, Drawing.Image image, ImageBufferFormats format)
		{
			if (image == null)
				throw new ArgumentNullException("image");

			return FromBitmap(name, image, image.Width, image.Height, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to create a texture from a GDI+ image.
		/// </summary>
		/// <param name="name">Name of the image.</param>
		/// <param name="image">Image to retrieve data from.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromBitmap(string name, Drawing.Image image)
		{
			if (image == null)
				throw new ArgumentNullException("image");

			return FromBitmap(name, image, image.Width, image.Height, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>		
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>		
		/// <param name="bytes">Number of bytes for the image.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromStream(string name, Stream stream, int bytes, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			if (stream == null)
				throw new ArgumentNullException("stream");

			return ImageFromStream(name, null, null, stream, false, width, height, bytes, format, colorKey);
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>		
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromStream(string name, Stream stream, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			if (stream == null)
				throw new ArgumentNullException("stream");

			return ImageFromStream(name, null, null, stream, false, width, height, -1, format, colorKey);
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>		
		/// <param name="bytes">Number of bytes for the image.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromStream(string name, Stream stream, int bytes, int width, int height, ImageBufferFormats format)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			if (stream == null)
				throw new ArgumentNullException("stream");

			return ImageFromStream(name, null, null, stream, false, width, height, bytes, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>		
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromStream(string name, Stream stream, int width, int height, ImageBufferFormats format)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			if (stream == null)
				throw new ArgumentNullException("stream");

			return ImageFromStream(name, null, null, stream, false, width, height, -1, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>
		/// <param name="bytes">Number of bytes for the image.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromStream(string name, Stream stream, int bytes, int width, int height)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			if (stream == null)
				throw new ArgumentNullException("stream");

			return ImageFromStream(name, null, null, stream, false, width, height, bytes, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>
		/// <param name="bytes">Number of bytes for the image data.</param>
		/// <param name="format">Format of the image.</param>		
		/// <returns>The loaded image.</returns>
		public static Image FromStream(string name, Stream stream, int bytes, ImageBufferFormats format)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			if (stream == null)
				throw new ArgumentNullException("stream");

			return ImageFromStream(name, null, null, stream, false, 0, 0, bytes, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromStream(string name, Stream stream, int width, int height)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			if (stream == null)
				throw new ArgumentNullException("stream");

			return ImageFromStream(name, null, null, stream, false, width, height, -1, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>		
		/// <param name="bytes">Number of bytes for the image.</param>
		/// <returns>The loaded image.</returns>
		public static Image FromStream(string name, Stream stream, int bytes)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			if (stream == null)
				throw new ArgumentNullException("stream");

			return ImageFromStream(name, null, null, stream, false, 0, 0, bytes, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to resize the dimensions into a power of two if required.
		/// </summary>
		/// <param name="width">Width of the image.</param>
		/// <param name="height">Height of the image.</param>
		/// <returns>Adjusted size.</returns>
		public static Drawing.Size ResizePowerOf2(int width, int height)
		{
			double power = 0;		// Exponent power.

			// Do width.
			power = Math.Log((double)width, 2);
			while ((power % 1) != 0)
			{
				width++;
				power = Math.Log((double)width, 2);
			}

			// Do height.
			power = Math.Log((double)height, 2);
			while ((power % 1) != 0)
			{
				height++;
				power = Math.Log((double)height, 2);
			}

			return new Drawing.Size(width, height);
		}

		/// <summary>
		/// Function to return the dimensions of an image.
		/// </summary>
		/// <param name="path">Path to the image.</param>
		/// <param name="adjust">TRUE to return adjusted sizes if the device requires it, FALSE to return file image size.</param>
		/// <returns>Image width and height.</returns>
		public static Drawing.Size GetImageDimensions(string path, bool adjust)
		{
			Drawing.Size imageSize = Drawing.Size.Empty;	// Image size.
			D3D9.ImageInformation imageInfo;				// Image information.
							
			imageInfo = D3D9.ImageInformation.FromFile(path);

			imageSize.Width = imageInfo.Width;
			imageSize.Height = imageInfo.Height;

			// If the device only supports square images, then make square.
			if ((adjust) && (Gorgon.CurrentDriver != default(Driver)))
			{
				if (!Gorgon.CurrentDriver.SupportNonSquareTexture)
				{
					if (imageSize.Width > imageSize.Height)
						imageSize.Height = imageSize.Width;
					else
						imageSize.Width = imageSize.Height;
				}

				// Resize to the power of two.
				if ((!Gorgon.CurrentDriver.SupportNonPowerOfTwoTexture) && (!Gorgon.CurrentDriver.SupportNonPowerOfTwoTextureConditional))
					imageSize = ResizePowerOf2(imageSize.Width, imageSize.Height);
			}

			return imageSize;
		}

		/// <summary>
		/// Function to blit this image to the current render target.
		/// </summary>
		/// <param name="x">Horizontal position.</param>
		/// <param name="y">Vertical position.</param>
		/// <param name="width">Width of the blit area.</param>
		/// <param name="height">Height of the blit area.</param>
        /// <param name="color">Color to modulate with the image.</param>
		/// <param name="mode">Mode to define how to handle dimensions that are smaller/larger than the image.</param>
        /// <remarks>Mode is used to either crop or scale the image when the dimensions are larger or smaller than the source image.  It can be one of:
        /// <para>
        /// <list type="table">
        /// <item><term>Scale</term><description>Scale the image up or down to match the width and height parameters.</description></item>
        /// <item><term>Crop</term><description>Clip the image if it's larger than the width and height passed.  If the image is smaller, then the image will be handled according to the
        /// <see cref="GorgonLibrary.Graphics.RenderTarget.HorizontalWrapMode">Gorgon.CurrentRenderTarget.HorizontalWrapMode</see> or the
        /// <see cref="GorgonLibrary.Graphics.RenderTarget.VerticalWrapMode">Gorgon.CurrentRenderTarget.VerticalWrapMode</see>.</description></item></list>
        /// </para></remarks>
		public void Blit(float x, float y, float width, float height, Drawing.Color color, BlitterSizeMode mode)
		{
			// If the blitter object doesn't already exist, then create it.
			if (_blitter == null)
				_blitter = new Sprite("ImageBlitter", this);
            if (color != _blitter.Color)
                _blitter.Color = color;
			
			// Perform scale.
			if (mode == BlitterSizeMode.Scale)
			{
				// Calculate the new scale.
				Vector2D newScale = new Vector2D(width / (float)Width, height / (float)Height);

				// Adjust scale if necessary.
				if (newScale != _blitter.Scale)
					_blitter.Scale = newScale;
			}
			else
			{
				if (_blitter.UniformScale != 1.0f)
					_blitter.UniformScale = 1.0f;

				// Crop.
				if (width != _blitter.Width)
					_blitter.Width = width;
				if (height != _blitter.Height)
					_blitter.Height = height;
			}

			// Set the position.
			if ((_blitter.Position.X != x) || (_blitter.Position.Y != y))
				_blitter.Position = new Vector2D(x, y);

			// Inherit the target states.
			if (_blitter.Smoothing != Gorgon.CurrentRenderTarget.Smoothing)
				_blitter.Smoothing = Gorgon.CurrentRenderTarget.Smoothing;
			if (_blitter.SourceBlend != Gorgon.CurrentRenderTarget.SourceBlend)
				_blitter.SourceBlend = Gorgon.CurrentRenderTarget.SourceBlend;
			if (_blitter.DestinationBlend != Gorgon.CurrentRenderTarget.DestinationBlend)
				_blitter.DestinationBlend = Gorgon.CurrentRenderTarget.DestinationBlend;
            if (_blitter.SourceBlendAlpha != Gorgon.CurrentRenderTarget.SourceBlendAlpha)
                _blitter.SourceBlendAlpha = Gorgon.CurrentRenderTarget.SourceBlendAlpha;
            if (_blitter.DestinationBlendAlpha != Gorgon.CurrentRenderTarget.DestinationBlendAlpha)
                _blitter.DestinationBlendAlpha = Gorgon.CurrentRenderTarget.DestinationBlendAlpha;
			if (_blitter.HorizontalWrapMode != Gorgon.CurrentRenderTarget.HorizontalWrapMode)
				_blitter.HorizontalWrapMode = Gorgon.CurrentRenderTarget.HorizontalWrapMode;
			if (_blitter.VerticalWrapMode != Gorgon.CurrentRenderTarget.VerticalWrapMode)
				_blitter.VerticalWrapMode = Gorgon.CurrentRenderTarget.VerticalWrapMode;
			if (_blitter.AlphaMaskFunction != Gorgon.CurrentRenderTarget.AlphaMaskFunction)
				_blitter.AlphaMaskFunction = Gorgon.CurrentRenderTarget.AlphaMaskFunction;
			if (_blitter.AlphaMaskValue != Gorgon.CurrentRenderTarget.AlphaMaskValue)
				_blitter.AlphaMaskValue = Gorgon.CurrentRenderTarget.AlphaMaskValue;
			if (_blitter.StencilCompare != Gorgon.CurrentRenderTarget.StencilCompare)
				_blitter.StencilCompare = Gorgon.CurrentRenderTarget.StencilCompare;
			if (_blitter.StencilEnabled != Gorgon.CurrentRenderTarget.StencilEnabled)
				_blitter.StencilEnabled = Gorgon.CurrentRenderTarget.StencilEnabled;
			if (_blitter.StencilFailOperation != Gorgon.CurrentRenderTarget.StencilFailOperation)
				_blitter.StencilFailOperation = Gorgon.CurrentRenderTarget.StencilFailOperation;
			if (_blitter.StencilMask != Gorgon.CurrentRenderTarget.StencilMask)
				_blitter.StencilMask = Gorgon.CurrentRenderTarget.StencilMask;
			if (_blitter.StencilPassOperation != Gorgon.CurrentRenderTarget.StencilPassOperation)
				_blitter.StencilPassOperation = Gorgon.CurrentRenderTarget.StencilPassOperation;
			if (_blitter.StencilReference != Gorgon.CurrentRenderTarget.StencilReference)
				_blitter.StencilReference = Gorgon.CurrentRenderTarget.StencilReference;
			if (_blitter.StencilZFailOperation != Gorgon.CurrentRenderTarget.StencilZFailOperation)
				_blitter.StencilZFailOperation = Gorgon.CurrentRenderTarget.StencilZFailOperation;
			_blitter.Draw();
		}

		/// <summary>
		/// Function to draw this render image to the current render target.
		/// </summary>
		/// <param name="x">Left position of the blit.</param>
		/// <param name="y">Top position of the blit.</param>
		/// <param name="width">Width to blit with.</param>
		/// <param name="height">Height to blit with.</param>
		public void Blit(float x, float y, float width, float height)
		{
            Blit(x, y, width, height, Drawing.Color.White, BlitterSizeMode.Scale);
		}

		/// <summary>
		/// Function to draw this render image to the current render target.
		/// </summary>
		/// <param name="x">Left position of the blit.</param>
		/// <param name="y">Top position of the blit.</param>
		public void Blit(float x, float y)
		{
            Blit(x, y, Width, Height, Drawing.Color.White, BlitterSizeMode.Crop);
		}

		/// <summary>
		/// Function to draw this render image to the current render target.
		/// </summary>
		public void Blit()
		{
            Blit(0, 0, Width, Height, Drawing.Color.White, BlitterSizeMode.Crop);
		}
		
		/// <summary>
		/// Function to update the image dimensions and format.
		/// </summary>
		/// <param name="width">New width to use.</param>
		/// <param name="height">New height to use.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="preserve">TRUE to preserve the image, FALSE to destroy it.</param>
		public void SetDimensions(int width, int height, ImageBufferFormats format, bool preserve)
		{
			DX.DataStream imageData = null;		// Image data.
			D3D9.Texture newImage = null;		// New texture.

			// If we don't need to preserve the data, then re-create.
			if (!preserve)
			{
				_width = width;
				_height = height;
				_format = format;
				Initialize();
				return;
			}

			try
			{
				D3D9.SurfaceDescription surfaceDesc;			// Surface description.

				// Start out with the same size.
				_actualWidth = width;
				_actualHeight = height;

				// If the device only supports square images, then make square.
				if (!Gorgon.CurrentDriver.SupportNonSquareTexture)
				{
					if (_actualWidth > _actualHeight)
						_actualHeight = _actualWidth;
					else
						_actualWidth = _actualHeight;
				}

				// Resize to the power of two.
				if ((!Gorgon.CurrentDriver.SupportNonPowerOfTwoTexture) && (!Gorgon.CurrentDriver.SupportNonPowerOfTwoTextureConditional))
				{
					Drawing.Size newSize;		// New image size.

					newSize = ResizePowerOf2(_actualWidth, _actualHeight);
					_actualWidth = newSize.Width;
					_actualHeight = newSize.Height;
				}

				// Ensure the image is the correct size.
				if ((_actualWidth > Gorgon.CurrentDriver.MaximumTextureWidth) || (_actualHeight > Gorgon.CurrentDriver.MaximumTextureHeight) || (_actualWidth < 0) || (_actualHeight < 0))
					throw new GorgonException(GorgonErrors.CannotCreate, "The image size of " + _actualWidth.ToString() + "x" + _actualHeight.ToString() + " is too large/small for the video card.");

				// If we specified unknown for the buffer format, assume 32 bit.
				if (format == ImageBufferFormats.BufferUnknown)
				{
					format = ValidateFormat(ImageBufferFormats.BufferRGB888A8, _imageType);

					if (!SupportsFormat(format, _imageType))
						throw new GorgonException(GorgonErrors.InvalidFormat);
				}
				else
				{
					// Throw an exception if the format requested is not supported.
					if (!SupportsFormat(format, _imageType))
						throw new GorgonException(GorgonErrors.InvalidFormat, format.ToString() + " is not a valid format.");
				}

				// Reload the image data within the new format.				
				try
				{
					imageData = D3D9.Texture.ToStream(_d3dImage, D3D9.ImageFileFormat.Png);					
					newImage = D3D9.Texture.FromStream(Gorgon.Screen.Device, imageData, (int)imageData.Length, _actualWidth, _actualHeight, 1, Usage, Converter.Convert(format), Pool, D3D9.Filter.None, D3D9.Filter.None, 0);
				}
				catch (Exception ex)
				{
					throw GorgonException.Repackage(GorgonErrors.CannotUpdate, "Cannot update the D3D texture object.", ex);
				}

				// Clean up the previous image.
				_d3dImage.Dispose();
				_d3dImage = newImage;

				// Get image information.
				surfaceDesc = _d3dImage.GetLevelDescription(0);

				_width = width;
				_height = height;
				_format = Converter.ConvertD3DImageFormat(surfaceDesc.Format);
			}
			finally
			{
				if (imageData != null)
					imageData.Close();

				newImage = null;
			}
		}

		/// <summary>
		/// Function to update the image dimensions and format.
		/// </summary>
		/// <param name="width">New width to use.</param>
		/// <param name="height">New height to use.</param>
		/// <param name="format">Format to use.</param>
		public void SetDimensions(int width, int height, ImageBufferFormats format)
		{
			SetDimensions(width, height, format, false);
		}

		/// <summary>
		/// Function to update the image dimensions and format.
		/// </summary>
		/// <param name="width">New width to use.</param>
		/// <param name="height">New height to use.</param>
		public void SetDimensions(int width, int height)
		{
			SetDimensions(width, height, _format);
		}

		/// <summary>
		/// Function to clear the image with a specified color.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		public void Clear(Drawing.Color color)
		{
			if (_d3dImage == null)
				return;

			_fillColor = color;
			_d3dImage.Fill(ClearTextureCallback);
		}

		/// <summary>
		/// Function to fill this image from a shader.
		/// </summary>
		/// <param name="shader">Shader to use.</param>
		public void FillFromShader(ImageShader shader)
		{
			if (shader == null)
				throw new ArgumentNullException("shader");
			
			_d3dImage.Fill(shader.D3DShader);
		}

		/// <summary>
		/// Function to copy the contents of another image into this image.
		/// </summary>
		/// <param name="image">Image to copy.</param>
		/// <param name="width">New width of the image.  0 to keep current.</param>
		/// <param name="height">New height of the image.  0 to keep current.</param>
		/// <param name="format">Format for the image.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		public void Copy(Image image, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			if (image == null)
				throw new ArgumentNullException("image");

			Stream memoryImage = null;			// Stream containing the image.

			try
			{
				D3D9.SurfaceDescription surfaceInfo;	// Surface description.					

				if (_d3dImage != null)
					_d3dImage.Dispose();
				_d3dImage = null;

				// Keep the size if needed.
				if (width == 0)
					width = image.Width;
				if (height == 0)
					height = image.Height;

				// Start out with the same size.
				_actualWidth = width;
				_actualHeight = height;

				// If the device only supports square images, then make square.
				if (!Gorgon.CurrentDriver.SupportNonSquareTexture)
				{
					if (_actualWidth > _actualHeight)
						_actualHeight = _actualWidth;
					else
						_actualWidth = _actualHeight;
				}

				// Resize to the power of two.
				if ((!Gorgon.CurrentDriver.SupportNonPowerOfTwoTexture) && (!Gorgon.CurrentDriver.SupportNonPowerOfTwoTextureConditional))
				{
					Drawing.Size newSize;		// New image size.

					newSize = ResizePowerOf2(_actualWidth, _actualHeight);
					_actualWidth = newSize.Width;
					_actualHeight = newSize.Height;
				}

				// Ensure the image is the correct size.
				if ((_actualWidth > Gorgon.CurrentDriver.MaximumTextureWidth) || (_actualHeight > Gorgon.CurrentDriver.MaximumTextureHeight) || (_actualWidth < 0) || (_actualHeight < 0))
					throw new GorgonException(GorgonErrors.CannotCreate, "The image size of " + _actualWidth.ToString() + "x" + _actualHeight.ToString() + " is too large/small for the video card.");

				// If we specified unknown for the buffer format, assume 32 bit.
				if (format == ImageBufferFormats.BufferUnknown)
				{
					format = ValidateFormat(ImageBufferFormats.BufferRGB888A8, _imageType);

					if (!SupportsFormat(format, _imageType))
						throw new GorgonException(GorgonErrors.InvalidFormat);
				}
				else
				{
					// Throw an exception if the format requested is not supported.
					if (!SupportsFormat(format, _imageType))
						throw new GorgonException(GorgonErrors.InvalidFormat, format.ToString() + " is not a valid format.");
				}

				// Copy the texture data to a stream.
				memoryImage = D3D9.Texture.ToStream(image.D3DTexture, D3D9.ImageFileFormat.Png);

				try
				{
					_d3dImage = D3D9.Texture.FromStream(Gorgon.Screen.Device, memoryImage, (int)memoryImage.Length, _actualWidth, _actualHeight, 1, Usage, Converter.Convert(format), Pool, D3D9.Filter.None, D3D9.Filter.None, colorKey.ToArgb());
				}
				catch (Exception ex)
				{
					throw GorgonException.Repackage(GorgonErrors.CannotCreate, "Error while trying to create the D3D texture.",ex);
				}
				surfaceInfo = _d3dImage.GetLevelDescription(0);
				_actualWidth = surfaceInfo.Width;
				_actualHeight = surfaceInfo.Height;
				_width = width;
				_height = height;
				_format = Converter.ConvertD3DImageFormat(surfaceInfo.Format);
			}
			finally
			{
				if (memoryImage != null)
					memoryImage.Dispose();
				memoryImage = null;
			}
		}

		/// <summary>
		/// Function to copy the contents of another image into this image.
		/// </summary>
		/// <param name="image">Image to copy.</param>
		/// <param name="width">New width of the image.  0 to keep current.</param>
		/// <param name="height">New height of the image.  0 to keep current.</param>
		/// <param name="format">Format for the image.</param>
		public void Copy(Image image, int width, int height, ImageBufferFormats format)
		{
			Copy(image, width, height, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to copy the contents of another image into this image.
		/// </summary>
		/// <param name="image">Image to copy.</param>
		/// <param name="format">Format for the image.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		public void Copy(Image image, ImageBufferFormats format, Drawing.Color colorKey)
		{
			Copy(image, 0, 0, format, colorKey);
		}

		/// <summary>
		/// Function to copy the contents of another image into this image.
		/// </summary>
		/// <param name="image">Image to copy.</param>		
		/// <param name="colorKey">Color to use for transparency.</param>
		public void Copy(Image image, Drawing.Color colorKey)
		{
			Copy(image, 0, 0, ImageBufferFormats.BufferUnknown, colorKey);
		}

		/// <summary>
		/// Function to copy the contents of another image into this image.
		/// </summary>
		/// <param name="image">Image to copy.</param>
		/// <param name="format">Format for the image.</param>		
		public void Copy(Image image, ImageBufferFormats format)
		{
			Copy(image, 0, 0, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to copy the contents of another image into this image.
		/// </summary>
		/// <param name="image">Image to copy.</param>
		public void Copy(Image image)
		{
			Copy(image, 0, 0, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to copy the contents of a gdi image into this image.
		/// </summary>
		/// <param name="gdiImage">GDI+ image to copy.</param>
		/// <param name="format">Format for the image.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		public void Copy(Drawing.Image gdiImage, ImageBufferFormats format, Drawing.Color colorKey)
		{
			Copy(gdiImage, 0, 0, format, colorKey);
		}

		/// <summary>
		/// Function to copy the contents of a gdi image into this image.
		/// </summary>
		/// <param name="gdiImage">GDI+ image to copy.</param>
		/// <param name="format">Format for the image.</param>
		public void Copy(Drawing.Image gdiImage, ImageBufferFormats format)
		{
			Copy(gdiImage, 0, 0, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to copy the contents of a gdi image into this image.
		/// </summary>
		/// <param name="gdiImage">GDI+ image to copy.</param>
		/// <param name="width">New width of the image.  0 to keep current.</param>
		/// <param name="height">New height of the image.  0 to keep current.</param>
		/// <param name="format">Format for the image.</param>
		public void Copy(Drawing.Image gdiImage, int width, int height, ImageBufferFormats format)
		{
			Copy(gdiImage, width, height, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to copy the contents of a gdi image into this image.
		/// </summary>
		/// <param name="gdiImage">GDI+ image to copy.</param>
		/// <param name="width">New width of the image.  0 to keep current.</param>
		/// <param name="height">New height of the image.  0 to keep current.</param>
		/// <param name="format">Format for the image.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		public void Copy(Drawing.Image gdiImage, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			if (gdiImage == null)
				throw new ArgumentNullException("gdiImage");
			
			Stream memoryImage = null;			// Stream containing the image.

			try
			{
				D3D9.SurfaceDescription surfaceInfo;	// Surface description.			

				if (_d3dImage != null)
					_d3dImage.Dispose();
				_d3dImage = null;

				// Keep the size if needed.
				if (width == 0)
					width = gdiImage.Width;
				if (height == 0)
					height = gdiImage.Height;

				// Start out with the same size.
				_actualWidth = width;
				_actualHeight = height;

				// If the device only supports square images, then make square.
				if (!Gorgon.CurrentDriver.SupportNonSquareTexture)
				{
					if (_actualWidth > _actualHeight)
						_actualHeight = _actualWidth;
					else
						_actualWidth = _actualHeight;
				}

				// Resize to the power of two.
				if ((!Gorgon.CurrentDriver.SupportNonPowerOfTwoTexture) && (!Gorgon.CurrentDriver.SupportNonPowerOfTwoTextureConditional))
				{
					Drawing.Size newSize;		// New image size.

					newSize = ResizePowerOf2(_actualWidth, _actualHeight);
					_actualWidth = newSize.Width;
					_actualHeight = newSize.Height;
				}

				// Ensure the image is the correct size.
				if ((_actualWidth > Gorgon.CurrentDriver.MaximumTextureWidth) || (_actualHeight > Gorgon.CurrentDriver.MaximumTextureHeight) || (_actualWidth < 0) || (_actualHeight < 0))
					throw new GorgonException(GorgonErrors.CannotCreate, "The image size of " + _actualWidth.ToString() + "x" + _actualHeight.ToString() + " is too large/small for the video card.");

				// Load the image.
				memoryImage = new MemoryStream();
				gdiImage.Save(memoryImage, Drawing.Imaging.ImageFormat.Png);
				memoryImage.Position = 0;

				try
				{
					_d3dImage = D3D9.Texture.FromStream(Gorgon.Screen.Device, memoryImage, (int)memoryImage.Length, _actualWidth, _actualHeight, 1, Usage, Converter.Convert(format), Pool, D3D9.Filter.None, D3D9.Filter.None, colorKey.ToArgb());
				}
				catch (Exception ex)
				{
					throw GorgonException.Repackage(GorgonErrors.CannotLoad, "Could not load the D3D texture from the stream.", ex);
				}
				surfaceInfo = _d3dImage.GetLevelDescription(0);
				_actualWidth = surfaceInfo.Width;
				_actualHeight = surfaceInfo.Height;
				_width = width;
				_height = height;
				_format = Converter.ConvertD3DImageFormat(surfaceInfo.Format);
			}
			finally
			{
				if (memoryImage != null)
					memoryImage.Dispose();
				memoryImage = null;
			}
		}

		/// <summary>
		/// Function to copy the contents of a gdi image into this image.
		/// </summary>
		/// <param name="gdiImage">GDI+ image to copy.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		public void Copy(Drawing.Image gdiImage, Drawing.Color colorKey)
		{
			Copy(gdiImage, 0, 0, ImageBufferFormats.BufferUnknown, colorKey);
		}

		/// <summary>
		/// Function to copy the contents of a gdi image into this image.
		/// </summary>
		/// <param name="gdiImage">GDI+ image to copy.</param>
		public void Copy(Drawing.Image gdiImage)
		{
			Copy(gdiImage, 0, 0, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to return whether the current device supports a specified image format.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <param name="type">Type of image.</param>
		/// <returns>TRUE if supported, FALSE if not.</returns>
		public static bool SupportsFormat(ImageBufferFormats format, ImageType type)
		{
			// Check to see if the device can handle the image format.
			if (!Gorgon.CurrentDriver.ValidImageFormat(format, type))
				return false;

			return (ValidateFormat(format, type) == format);
		}

		/// <summary>
		/// Function to convert this image into a GDI+ image.
		/// </summary>
		/// <returns>The image wrapped in GDI+ Image object.</returns>
		public Drawing.Image SaveBitmap()
		{
			MemoryStream stream = null;			// Stream to contain our data.

			try
			{
				stream = new MemoryStream();
				Save(stream, ImageFileFormat.PNG);
				stream.Position = 0;

				return Drawing.Image.FromStream(stream);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to (re)initialize the buffer.
		/// This will destroy all data contained within the buffer.
		/// </summary>
		public void Refresh()
		{
			// Recreate the image.
			Initialize();
		}

		/// <summary>
		/// Function to save an image to a stream.
		/// </summary>
		/// <param name="stream">Stream to save into.</param>
		/// <param name="fileFormat">File format of the image.</param>
		public void Save(Stream stream, ImageFileFormat fileFormat)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			ImageSerializer serializer = null;		// Serializer object.
			string filename = string.Empty;			// Filename.

			if (stream is FileStream)
				filename = ((FileStream)stream).Name;
			else
				filename = Name;

			// Open the serializer.
			serializer = new ImageSerializer(this, stream);
			serializer.DontCloseStream = true;

			serializer.Parameters["FileFormat"] = fileFormat;

			serializer.Serialize();
		}

		/// <summary>
		/// Function to save an image.
		/// </summary>
		/// <param name="fileSystem">File system to save image within.</param>
		/// <param name="filename">Filename of the image to save.</param>
		/// <param name="fileFormat">Format to use for the file.</param>
		public void Save(FileSystem fileSystem, string filename, ImageFileFormat fileFormat)
		{
			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");
			
			MemoryStream stream = null;			// File stream.
			byte[] data = null;						// Binary image data.

			try
			{
				// Create memory stream and save data into it.
				stream = new MemoryStream();
				Save(stream, fileFormat);

				// Get byte data.
				stream.Position = 0;
				data = stream.ToArray();

				// Write to the file system.
				fileSystem.WriteFile(filename, data);

				_filename = filename;
				_isResource = false;
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to save an image.
		/// </summary>
		/// <param name="filename">Filename of the image to save.</param>
		/// <param name="fileFormat">Format to use for the file.</param>
		public void Save(string filename, ImageFileFormat fileFormat)
		{
			Stream filestream = null;			// File stream.

			try
			{
				filestream = File.Open(filename, FileMode.Create, FileAccess.Write);
				Save(filestream, fileFormat);

				_filename = filename;
				_isResource = false;
			}
			finally
			{
				if (filestream != null)
					filestream.Dispose();
				filestream = null;
			}
		}

		/// <summary>
		/// Function to save an image to a file system.
		/// </summary>
		/// <param name="fileSystem">File system to save the image into.</param>
		/// <param name="filename">Filename of the image to save.</param>
		public void Save(FileSystem fileSystem, string filename)
		{
			ImageFileFormat format = ImageFileFormat.PNG;		// File format.

			if (Path.GetExtension(filename) == string.Empty)
				filename += ".png";

			// Try to determine the file format.
			switch (Path.GetExtension(filename).ToLower())
			{
				case ".jpg":
				case ".jpeg":
					format = ImageFileFormat.JPEG;
					break;
				case ".bmp":
					format = ImageFileFormat.BMP;
					break;
				case ".tga":
					format = ImageFileFormat.TGA;
					break;
				case ".dib":
					format = ImageFileFormat.DIB;
					break;
				case ".pfm":
					format = ImageFileFormat.PFM;
					break;
				case ".ppm":
					format = ImageFileFormat.PPM;
					break;
				case ".dds":
					format = ImageFileFormat.DDS;
					break;
			}

			// Save.
			Save(fileSystem, filename, format);
		}

		/// <summary>
		/// Function to save an image.
		/// </summary>
		/// <param name="filename">Filename of the image to save.</param>
		public void Save(string filename)
		{
			ImageFileFormat format = ImageFileFormat.PNG;		// File format.

			if (Path.GetExtension(filename) == string.Empty)
				filename += ".png";

			// Try to determine the file format.
			switch (Path.GetExtension(filename).ToLower())
			{
				case ".jpg":
				case ".jpeg":
					format = ImageFileFormat.JPEG;
					break;
				case ".bmp":
					format = ImageFileFormat.BMP;
					break;
				case ".tga":
					format = ImageFileFormat.TGA;
					break;
				case ".dib":
					format = ImageFileFormat.DIB;
					break;
				case ".pfm":
					format = ImageFileFormat.PFM;
					break;
				case ".ppm":
					format = ImageFileFormat.PPM;
					break;
				case ".dds":
					format = ImageFileFormat.DDS;
					break;
			}

			// Save.
			Save(filename, format);
		}

		/// <summary>
		/// Function to get access to the image data.
		/// </summary>
		/// <param name="lockRectangle">Sub section of the image to retrieve.</param>
		/// <returns>An image lock box containing data about the locked region.</returns>
		public ImageLockBox GetImageData(Drawing.Rectangle lockRectangle)
		{
			return new ImageLockBox(this, lockRectangle);
		}

		/// <summary>
		/// Function to get access to the image data.
		/// </summary>
		/// <returns>An image lock box containing data about the locked region.</returns>
		public ImageLockBox GetImageData()
		{
			return new ImageLockBox(this, new Drawing.Rectangle(0, 0, _actualWidth, _actualHeight));
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the Image.</param>
		/// <param name="imageType">Type of image.</param>
		/// <param name="width">Width of the Image.</param>
		/// <param name="height">Height of the Image.</param>
		/// <param name="format">Format of the Image.</param>
		/// <param name="createTexture">TRUE to create an empty texture, FALSE to bypass.</param>
		internal Image(string name, ImageType imageType, int width, int height, ImageBufferFormats format, bool createTexture)
			: base(name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (Gorgon.Screen == null)
				throw new GorgonException(GorgonErrors.NoDevice);

			_actualHeight = -1;
			_actualWidth = -1;
			_locks = new List<ImageLockBox>();
			_filename = string.Empty;
			_isResource = false;

			// Determine format from the video mode.
			if (format == ImageBufferFormats.BufferUnknown)
			{
				switch (Gorgon.CurrentVideoMode.Format)
				{
					case BackBufferFormats.BufferRGB888:
						format = ImageBufferFormats.BufferRGB888X8;
						break;
					case BackBufferFormats.BufferRGB565:
						format = ImageBufferFormats.BufferRGB565;
						break;
					case BackBufferFormats.BufferRGB555:
						format = ImageBufferFormats.BufferRGB555X1;
						break;
					case BackBufferFormats.BufferRGB101010A2:
						format = ImageBufferFormats.BufferRGB101010A2;
						break;
					case BackBufferFormats.BufferP8:
						format = ImageBufferFormats.BufferP8;
						break;
				}
			}

			_imageType = imageType;
			_width = width;
			_height = height;
			_format = format;

			// If this is a regular image, then add it to the state list.
			if (_imageType != ImageType.RenderTarget)
				DeviceStateList.Add(this);

			if (createTexture)
			{
				Initialize();

				// Don't clear again.
				_clearOnInit = false;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Image"/> class.
		/// </summary>
		/// <param name="name">Name of the Image.</param>
		/// <param name="width">Width of the Image.</param>
		/// <param name="height">Height of the Image.</param>
		/// <param name="format">Format of the Image.</param>
		/// <param name="dynamic">TRUE if a dynamic image, FALSE if not.</param>
		public Image(string name, int width, int height, ImageBufferFormats format, bool dynamic)
			: this(name, dynamic ? ImageType.Dynamic : ImageType.Normal, width, height, format, true)
		{
			if (ImageCache.Images.Contains(name))
				throw new ArgumentException("'" + name + "' is already loaded.", "name");

			try
			{
				// Register with the factory.
				ImageCache.Images.Add(this);
			}
			catch
			{
				// Force clean-up if we have a failure.
				Dispose();
				throw;
			}
		}

		/// <summary>
		/// Initializes the <see cref="Image"/> class.
		/// </summary>
		/// <param name="name">Name of the Image.</param>
		/// <param name="width">Width of the Image.</param>
		/// <param name="height">Height of the Image.</param>
		/// <param name="format">Format of the Image.</param>
		public Image(string name, int width, int height, ImageBufferFormats format)
			: this(name, width, height, format, false)
		{
		}

		/// <summary>
		/// Initializes the <see cref="Image"/> class.
		/// </summary>
		/// <param name="name">Name of the Image.</param>
		/// <param name="width">Width of the Image.</param>
		/// <param name="height">Height of the Image.</param>
		public Image(string name, int width, int height)
			: this(name, width, height, ImageBufferFormats.BufferUnknown, false)
		{
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to perform clean up of all objects.  FALSE to clean up only unmanaged objects.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!_disposed)
				{
					// Remove any open locks.
					foreach (ImageLockBox box in _locks)
						box.Dispose();
					_locks.Clear();

					if (_d3dImage != null)
						_d3dImage.Dispose();
					if (DeviceStateList.Contains(this))
						DeviceStateList.Remove(this);

					// Deregister from the factory.
					if (ImageCache.Images.Contains(Name))
						ImageCache.Images.Remove(Name);

					Gorgon.Log.Print("Image", "Destroying image \"{0}\".", LoggingLevel.Simple, Name);
				}

				_disposed = true;
			}
			_d3dImage = null;
		}

		/// <summary>
		/// Function to perform clean up, used by the image manager.
		/// </summary>
		/// <remarks>Users should call ImageManager.Remove() to destroy an image.</remarks>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region IDeviceStateObject Members
		/// <summary>
		/// Function called when the device is in a lost state.
		/// </summary>
		public void DeviceLost()
		{
			// Remove any open locks.
			foreach (ImageLockBox box in _locks)
				box.Unlock();

			if (Pool != D3D9.Pool.Managed)
			{
				if (_d3dImage != null)
					_d3dImage.Dispose();
				_d3dImage = null;
			}
		}

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public void DeviceReset()
		{
			if (Pool != D3D9.Pool.Managed)
				Refresh();
		}

		/// <summary>
		/// Function to force the loss of the objects data.
		/// </summary>
		public void ForceRelease()
		{
			// Remove any open locks.
			foreach (ImageLockBox box in _locks)
				box.Unlock();

			if (_d3dImage != null)
				_d3dImage.Dispose();
			_d3dImage = null;
		}
		#endregion

		#region ISerializable Members
		/// <summary>
		/// Property to set or return the filename of the serializable object.
		/// </summary>
		public string Filename
		{
			get
			{
				return _filename;
			}
		}

		/// <summary>
		/// Function to persist the data into the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		void ISerializable.WriteData(Serializer serializer)
		{
			DX.DataStream dxStream = null;		// Graphics stream.
			byte[] data = null;					// Data in the stream.
			D3D9.ImageFileFormat fileFormat;	// Image file format.

			if (!serializer.Parameters.Contains("FileFormat"))
				throw new GorgonException(GorgonErrors.InvalidFormat, "The file format was not supplied as a serialization parameter.");

			try
			{
				// Get the file format.
				fileFormat = Converter.Convert((ImageFileFormat)serializer.Parameters["FileFormat"]);

				// Save to a graphics stream.
				dxStream = D3D9.Texture.ToStream(_d3dImage, fileFormat);
				dxStream.Position = 0;

				// Transfer to our stream.
				data = new byte[dxStream.Length];
				dxStream.Read(data, 0, (int)dxStream.Length);
				serializer.Write(string.Empty, data, 0, data.Length);
			}
			finally
			{
				if (dxStream != null)
					dxStream.Close();

				dxStream = null;
			}
		}

		/// <summary>
		/// Function to retrieve data from the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		void ISerializable.ReadData(Serializer serializer)
		{
			D3D9.ImageInformation imageInfo;		// Image information.
			D3D9.SurfaceDescription surfaceInfo;	// Surface description.
			int size;								// Size of image in bytes.
			int width;								// Alternate width.
			int height;								// Alternate height.
			ImageBufferFormats format;				// Alternate image format.
			Drawing.Color colorkey;					// Color key.
			byte[] imageData = null;				// Image data.

			if ((!serializer.Parameters.Contains("byteSize")) || (!serializer.Parameters.Contains("Width")) ||
				(!serializer.Parameters.Contains("Height")) || (!serializer.Parameters.Contains("Format")) ||
				(!serializer.Parameters.Contains("ColorKey")))
				throw new GorgonException(GorgonErrors.InvalidFormat, "Some serialization parameters are missing for this image.");

			// Get image parameters.
			size = (int)serializer.Parameters["byteSize"];
			width = (int)serializer.Parameters["Width"];
			height = (int)serializer.Parameters["Height"];
			format = (ImageBufferFormats)serializer.Parameters["Format"];
			colorkey = (Drawing.Color)serializer.Parameters["ColorKey"];

			// Read in the image.
			if (_d3dImage != null)
				_d3dImage.Dispose();
			_d3dImage = null;

			// Ensure the image is the correct size.
			if (width > Gorgon.CurrentDriver.MaximumTextureWidth)
				width = Gorgon.CurrentDriver.MaximumTextureWidth;
			if (height > Gorgon.CurrentDriver.MaximumTextureHeight)
				height = Gorgon.CurrentDriver.MaximumTextureHeight;

			// Set to defaults.
			if (width < 0)
				width = 0;
			if (height < 0)
				height = 0;

			// If we specified unknown for the buffer format, assume 32 bit.
			if (format == ImageBufferFormats.BufferUnknown)
			{
				format = ValidateFormat(ImageBufferFormats.BufferRGB888A8, ImageType.Normal);

				if (!SupportsFormat(format, ImageType.Normal))
					throw new GorgonException(GorgonErrors.InvalidFormat);					
			}
			else
			{
				// Throw an exception if the format requested is not supported.
				if (!SupportsFormat(format, ImageType.Normal))
					throw new GorgonException(GorgonErrors.InvalidFormat, format.ToString() + " is not a valid format.");
			}
			

			// Load the image.			
			if (size > 0)
				imageData = serializer.ReadBytes(string.Empty, size);				
			else
			{
				// Copy the entire stream.
				imageData = serializer.ReadBytes(string.Empty);
				size = imageData.Length;
			}

			// Load the image.
			imageInfo = D3D9.ImageInformation.FromMemory(imageData);

			// If we don't do this, the function will resize our image to the nearest power of two, even if
			// we can support non pow2 textures.
			if (width <= 0)
				width = imageInfo.Width;
			if (height <= 0)
				height = imageInfo.Height;
			_d3dImage = D3D9.Texture.FromMemory(Gorgon.Screen.Device, imageData, width, height, 1, Usage, Converter.Convert(format), Pool, D3D9.Filter.None, D3D9.Filter.None, colorkey.ToArgb());

			surfaceInfo = _d3dImage.GetLevelDescription(0);
			_actualWidth = surfaceInfo.Width;
			_actualHeight = surfaceInfo.Height;
			_width = imageInfo.Width;
			_height = imageInfo.Height;
			_format = Converter.ConvertD3DImageFormat(surfaceInfo.Format);
			_imageType = ImageType.Normal;
		}
		#endregion
	}
}
