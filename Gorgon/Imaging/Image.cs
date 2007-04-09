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
// Created: Friday, May 19, 2006 10:18:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.IO;
using Drawing = System.Drawing;
using SharpUtilities;
using SharpUtilities.Utility;
using DX = Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;
using GorgonLibrary.Internal;
using GorgonLibrary.Serialization;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing an image.
	/// </summary>
	public class Image 
		: NamedObject, IDeviceStateObject, ISerializable
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
			private IntPtr _data;					// Pointer to the data for the image.
			private int _pitch;						// Total width of the image in bytes.
			private int _bytesPerPixel;				// Number of bytes in a single pixel.
			private bool _readOnly;					// Flag to indicate that the data is read-only or read-write.
			private DX.GraphicsStream _lockStream;	// Stream used by the lock.
			#endregion

			#region Properties.
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
			/// Property to return the pointer to the image data.
			/// </summary>
			public IntPtr Data
			{
				get
				{
					return _data;
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
					long result = 0;		// Result.

					if (Gorgon.Screen.DeviceIsInLostState)
						return 0;

					if (_lockStream == null)
						return 0;

					unsafe
					{
						int position = (y * _region.Width) + x;						// Linear position.						

						// Get each byte.
						switch (_bytesPerPixel)
						{
							case 1:
								// Get a single byte.
								byte* ptr = ((byte *)_data.ToPointer()) + position;		// Offset into the data.
								result = (long)(*ptr);
								break;
							case 2:
								// Get a single short.
								short* sptr = ((short *)_data.ToPointer()) + position;	// Offset into the data.
								result = (long)(*sptr);
								break;
							case 4:
								// Get a single integer.
								int* iptr = ((int *)_data.ToPointer()) + position;		// Offset into the data.
								result = (long)(*iptr);
								break;
							default:
								// Get a single long.
								long* lptr = ((long *)_data.ToPointer()) + position;		// Offset into the data.
								result = *lptr;
								break;
						}						
					}

					return result;
				}
				set
				{
					if (Gorgon.Screen.DeviceIsInLostState)
						return;

					if (_lockStream == null)
						return;

					unsafe
					{
						int position = (y * _region.Width) + x; // Linear position.						

						// Get each byte.
						switch (_bytesPerPixel)
						{
							case 1:
								// Get a single byte.
								byte* ptr = ((byte*)_data.ToPointer()) + position;		// Offset into the data.
								*ptr = (byte)(value & 0xFF);
								break;
							case 2:
								// Get a single short.
								short* sptr = ((short*)_data.ToPointer()) + position;	// Offset into the data.
								*sptr = (short)(value & 0xFFFF);
								break;
							case 4:
								// Get a single integer.
								int* iptr = ((int*)_data.ToPointer()) + position;		// Offset into the data.
								*iptr = (int)(value & 0xFFFFFFFF);
								break;
							default:
								// Get a single long.
								long* lptr = ((long*)_data.ToPointer()) + position;		// Offset into the data.
								*lptr = value;
								break;
						}
					}
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to unlock the image.
			/// </summary>
			public void Unlock()
			{
				if (Gorgon.Screen.DeviceIsInLostState)
					return;

				if (_lockStream == null)
					return;

				try
				{
					if (_image.D3DTexture == null)
						throw new InvalidResourceException("D3D texture was not generated.", null);

					_lockStream.Dispose();					
					_image.D3DTexture.UnlockRectangle(0);
				}
				finally
				{
					// Clear data.
					_lockStream = null;
					_data = IntPtr.Zero;
					_pitch = 0;
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
				if (Gorgon.Screen.DeviceIsInLostState)
					return;

				// If locked, then unlock and re-lock.
				if (_lockStream != null)
					Unlock();

				try
				{
					D3D.LockFlags flags = D3D.LockFlags.None;		// Lock flags.

					if (_image.ImageType == ImageType.RenderTarget)
						throw new Exception("Cannot lock a rendering image.");

					if (_image.D3DTexture == null)
						throw new Exception("No D3D texture allocated.");

					if ((_image.Pool == D3D.Pool.Default) && (_image.ImageType != ImageType.Dynamic))
						throw new Exception("This image is not dynamic, and cannot be locked.");
					
					if (_readOnly)
						flags |= D3D.LockFlags.ReadOnly;
					if (discard)
						flags |= D3D.LockFlags.Discard;

					// Lock the image.
					if (discard)
					{
						_region = new Drawing.Rectangle(0, 0, _image.ActualWidth, _image.ActualHeight);
						_lockStream = _image.D3DTexture.LockRectangle(0, flags, out _pitch);
					}
					else
						_lockStream = _image.D3DTexture.LockRectangle(0, _region, flags, out _pitch);	
				
					// Get pointer.
					_data = _lockStream.InternalData;
					_bytesPerPixel = _pitch / _image.ActualWidth;
					_image._locks.Add(this);
				}
				catch (Exception ex)
				{
					if (_image != null)
						throw new CannotLockException(_image.GetType(), ex);
					else
						throw new CannotLockException(GetType(), ex);
				}
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
			}

			/// <summary>
			/// Destructor.
			/// </summary>
			~ImageLockBox()
			{
				Dispose(false);
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
					if (_lockStream != null)
						Unlock();
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
		private string _filename;					// Filename of the image.				
		private D3D.Texture _d3dImage = null;		// Direct 3D image.
		private int _width;							// Width of the image.		
		private int _height;						// Height of the image.		
		private ImageBufferFormats _format;			// Format of the image.		
		private int _actualWidth;					// Actual width of the image.		
		private int _actualHeight;					// Actual height of the image.
		private ImageType _imageType;				// Type of image this object represents.
		private Drawing.Color _fillColor;			// Color used to fill a texture.
		private bool _isResource = false;			// Image is a resource object.
		private List<ImageLockBox> _locks;			// Image locks.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the memory pool for the image.
		/// </summary>
		private D3D.Pool Pool
		{
			get
			{
				// Set up for dynamic images.
				if ((_imageType == ImageType.Dynamic) || (_imageType == ImageType.RenderTarget))
					return D3D.Pool.Default;

				return D3D.Pool.Managed;
			}
		}

		/// <summary>
		/// Property to return the usage flags for the image.
		/// </summary>
		private D3D.Usage Usage
		{
			get
			{
				D3D.Usage flags = D3D.Usage.None;		// Usage flags.

				// Set up for dynamic images.
				if ((_imageType == ImageType.Dynamic) && (Gorgon.Driver.SupportDynamicTextures))
					flags |= D3D.Usage.Dynamic;

				// Force flags and pool for render targets.
				if (_imageType == ImageType.RenderTarget)
					flags = D3D.Usage.RenderTarget;

				return flags;
			}
		}		

		/// <summary>
		/// Property to return the Direct 3D image.
		/// </summary>
		internal D3D.Texture D3DTexture
		{
			get
			{
				return _d3dImage;
			}
			set
			{
				D3D.SurfaceDescription surfaceDesc;			// Surface information.

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
					_filename = string.Empty;
					_isResource = false;
				}
			}
		}

		/// <summary>
		/// Property to return the render image that uses this image.
		/// </summary>
		internal RenderImage RenderImage
		{
			get
			{
				if (_imageType != ImageType.RenderTarget)
					return null;

				// Find the target.
				foreach (RenderTarget target in Gorgon.RenderTargets)
				{
					if ((target is RenderImage) && (((RenderImage)target).Image == this))
						return (RenderImage)target;
				}

				return null;
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
		/// Property to return the filename for the image.
		/// </summary>
		public string Filename
		{
			get
			{
				return _filename;
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
		private DX.Vector4 ClearTextureCallback(DX.Vector2 textureCoordinate, DX.Vector2 textureSize)
		{
			return new DX.Vector4(_fillColor.R, _fillColor.G, _fillColor.B, _fillColor.A);
		}

		/// <summary>
		/// Function to return the best D3D format from the given Gorgon format.
		/// </summary>
		/// <param name="format">Gorgon format to convert.</param>
		/// <returns>The best matching supported format for the image.</returns>
		private ImageBufferFormats ValidateFormat(ImageBufferFormats format)
		{
			D3D.TextureRequirements requires;		// Texture requirements.

			// Get the straight conversion.
			requires = new D3D.TextureRequirements();
			requires.Format = Converter.Convert(format);
			// Height and width don't matter.
			requires.Width = 0;
			requires.Height = 0;

			D3D.TextureLoader.CheckTextureRequirements(Gorgon.Screen.Device, 0, Pool, out requires);

			// Return the best matching format.
			return Converter.ConvertD3DImageFormat(requires.Format);
		}

		/// <summary>
		/// Function to create an empty image.
		/// </summary>
		private void Initialize()
		{
			try
			{
				D3D.SurfaceDescription surfaceDesc;			// Surface description.

				// Destroy the previous image object.
				if (_d3dImage != null)
					_d3dImage.Dispose();
				_d3dImage = null;

				Gorgon.Log.Print("Image", "Creating image \"{0}\": {1}x{2}, Format: {3}.", LoggingLevel.Simple, Name, _width, _height, _format.ToString());

				// Start out with the same size.
				_actualWidth = _width;
				_actualHeight = _height;

				// If the device only supports square images, then make square.
				if (!Gorgon.Driver.SupportNonSquareTexture)
				{
					if (_actualWidth > _actualHeight)
						_actualHeight = _actualWidth;
					else
						_actualWidth = _actualHeight;
				}

				// Resize to the power of two.
				if ((!Gorgon.Driver.SupportNonPowerOfTwoTexture) && (!Gorgon.Driver.SupportNonPowerOfTwoTextureConditional))
				{
					Drawing.Size newSize;		// New image size.

					newSize = ResizePowerOf2(_actualWidth, _actualHeight);
					_actualWidth = newSize.Width;
					_actualHeight = newSize.Height;
				}

				// Ensure the image is the correct size.
				if ((_actualWidth > Gorgon.Driver.MaximumTextureWidth) || (_actualHeight > Gorgon.Driver.MaximumTextureHeight) || (_actualWidth < 0) || (_actualHeight < 0))
					throw new ImageSizeException(Name, _actualWidth, _actualHeight, Gorgon.Driver.MaximumTextureWidth, Gorgon.Driver.MaximumTextureHeight, null);

				// If we specified unknown for the buffer format, assume 32 bit.
				if (_format == ImageBufferFormats.BufferUnknown)
				{
					_format = ValidateFormat(ImageBufferFormats.BufferRGB888A8);

					if (_format == ImageBufferFormats.BufferUnknown)
						throw new FormatNotSupportedException("Unable to find a suitable image format.", null);
				}
				else
				{
					// Throw an exception if the format requested is not supported.
					if (!SupportsFormat(_format))
						throw new FormatNotSupportedException(_format, null);
				}

				// Create the image.
				_d3dImage = new D3D.Texture(Gorgon.Screen.Device,
					_actualWidth, _actualHeight, 1,
					Usage,
					Converter.Convert(_format),
					Pool);

#if DEBUG
				Clear(Drawing.Color.Pink);
#else
				Clear(Drawing.Color.Black);
#endif
				surfaceDesc = _d3dImage.GetLevelDescription(0);

				_format = Converter.ConvertD3DImageFormat(surfaceDesc.Format);
				_filename = string.Empty;
				_isResource = false;
				Gorgon.Log.Print("Image", "Image \"{0}\" created: {1}x{2} (actual texture size: {4}x{5}), Format: {3}.", LoggingLevel.Simple, Name, _width, _height, _format.ToString(), _actualWidth, _actualHeight);
			}
			catch (SharpException sEx)
			{
				// Pass the error on if it's a sharp exception.
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotCreateException(_objectName, GetType(), ex);
			}
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

			try
			{
				D3D.ImageInformation imageInfo;		// Image information.
				

				imageInfo = D3D.TextureLoader.ImageInformationFromFile(path);

				imageSize.Width = imageInfo.Width;
				imageSize.Height = imageInfo.Height;

				// If the device only supports square images, then make square.
				if ((adjust) && (Gorgon.Driver != default(Driver)))
				{
					if (!Gorgon.Driver.SupportNonSquareTexture)
					{
						if (imageSize.Width > imageSize.Height)
							imageSize.Height = imageSize.Width;
						else
							imageSize.Width = imageSize.Height;
					}

					// Resize to the power of two.
					if ((!Gorgon.Driver.SupportNonPowerOfTwoTexture) && (!Gorgon.Driver.SupportNonPowerOfTwoTextureConditional))
						imageSize = ResizePowerOf2(imageSize.Width, imageSize.Height);
				}
			}
			catch(Exception ex)
			{
				throw new CannotGetImageInformationException("Cannot retrieve image information for '" + path + "'.", ex);
			}

			return imageSize;
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
			DX.GraphicsStream imageData = null;		// Image data.
			D3D.Texture newImage = null;			// New texture.

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
				D3D.SurfaceDescription surfaceDesc;			// Surface description.

				// Ensure the image is the correct size.
				if ((width > Gorgon.Driver.MaximumTextureWidth) || (height > Gorgon.Driver.MaximumTextureHeight) || (width < 0) || (height < 0))
					throw new ImageSizeException(Name, width, height, Gorgon.Driver.MaximumTextureWidth, Gorgon.Driver.MaximumTextureHeight, null);

				// Start out with the same size.
				_actualWidth = width;
				_actualHeight = height;

				// If the device only supports square images, then make square.
				if (!Gorgon.Driver.SupportNonSquareTexture)
				{
					if (_actualWidth > _actualHeight)
						_actualHeight = _actualWidth;
					else
						_actualWidth = _actualHeight;
				}

				// Resize to the power of two.
				if ((!Gorgon.Driver.SupportNonPowerOfTwoTexture) && (!Gorgon.Driver.SupportNonPowerOfTwoTextureConditional))
				{
					Drawing.Size newSize;		// New image size.

					newSize = ResizePowerOf2(_actualWidth, _actualHeight);
					_actualWidth = newSize.Width;
					_actualHeight = newSize.Height;
				}

				// If we specified unknown for the buffer format, assume 32 bit.
				if (format == ImageBufferFormats.BufferUnknown)
				{
					format = ValidateFormat(ImageBufferFormats.BufferRGB888A8);

					if (format == ImageBufferFormats.BufferUnknown)
						throw new FormatNotSupportedException("Unable to find a suitable image format.", null);
				}
				else
				{
					// Throw an exception if the format requested is not supported.
					if (!SupportsFormat(format))
						throw new FormatNotSupportedException(format, null);
				}
				
				imageData = D3D.TextureLoader.SaveToStream(D3D.ImageFileFormat.Bmp, _d3dImage);
				newImage = D3D.TextureLoader.FromStream(Gorgon.Screen.Device, imageData, _actualWidth, _actualHeight, 1, Usage, Converter.Convert(format), Pool, D3D.Filter.None, D3D.Filter.None, 0);

				// Clean up the previous image.
				_d3dImage.Dispose();
				_d3dImage = newImage;

				// Get image information.
				surfaceDesc = _d3dImage.GetLevelDescription(0);

				_width = width;
				_height = height;
				_format = Converter.ConvertD3DImageFormat(surfaceDesc.Format);
				_isResource = false;
			}
			catch(Exception ex)
			{
				// Destroy the new image.
				if (newImage != null)
					newImage.Dispose();

				throw new CannotUpdateException(_objectName, GetType(), ex);
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
			try
			{
				if (_d3dImage == null)
					return;

				_fillColor = color;
				D3D.TextureLoader.FillTexture(_d3dImage, ClearTextureCallback);
			}
			finally
			{
			}			
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
			Stream memoryImage = null;			// Stream containing the image.

			// Ensure the image is the correct size.
			if ((width > Gorgon.Driver.MaximumTextureWidth) || (height > Gorgon.Driver.MaximumTextureHeight) || (width < 0) || (height < 0))
				throw new ImageSizeException(Name, width, height, Gorgon.Driver.MaximumTextureWidth, Gorgon.Driver.MaximumTextureHeight, null);

			try
			{
				D3D.SurfaceDescription surfaceInfo;	// Surface description.					

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
				if (!Gorgon.Driver.SupportNonSquareTexture)
				{
					if (_actualWidth > _actualHeight)
						_actualHeight = _actualWidth;
					else
						_actualWidth = _actualHeight;
				}

				// Resize to the power of two.
				if ((!Gorgon.Driver.SupportNonPowerOfTwoTexture) && (!Gorgon.Driver.SupportNonPowerOfTwoTextureConditional))
				{
					Drawing.Size newSize;		// New image size.

					newSize = ResizePowerOf2(_actualWidth, _actualHeight);
					_actualWidth = newSize.Width;
					_actualHeight = newSize.Height;
				}

				// Copy the texture data to a stream.
				memoryImage = D3D.TextureLoader.SaveToStream(D3D.ImageFileFormat.Png, image.D3DTexture);
				_d3dImage = D3D.TextureLoader.FromStream(Gorgon.Screen.Device, memoryImage, _actualWidth, _actualHeight, 1, Usage, Converter.Convert(format), Pool, D3D.Filter.None, D3D.Filter.None, colorKey.ToArgb());
				surfaceInfo = _d3dImage.GetLevelDescription(0);
				_actualWidth = surfaceInfo.Width;
				_actualHeight = surfaceInfo.Height;
				_width = width;
				_height = height;
				_format = Converter.ConvertD3DImageFormat(surfaceInfo.Format);
				_filename = string.Empty;
				_isResource = false;
			}
			catch(Exception ex)
			{
				throw new CannotLoadException(_objectName, GetType(), ex);
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
			Copy(image, width, height, format, Drawing.Color.FromArgb(0,0,0,0));
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
			Copy(image, 0, 0, format, Drawing.Color.FromArgb(0,0,0,0));
		}

		/// <summary>
		/// Function to copy the contents of another image into this image.
		/// </summary>
		/// <param name="image">Image to copy.</param>
		public void Copy(Image image)
		{
			Copy(image, 0, 0, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0,0,0,0));
		}

		/// <summary>
		/// Function to copy the contents of a gdi image into this image.
		/// </summary>
		/// <param name="gdiImage">GDI+ image to copy.</param>
		/// <param name="format">Format for the image.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		public void Copy(Drawing.Bitmap gdiImage, ImageBufferFormats format, Drawing.Color colorKey)
		{
			Copy(gdiImage, 0, 0, format, colorKey);
		}

		/// <summary>
		/// Function to copy the contents of a gdi image into this image.
		/// </summary>
		/// <param name="gdiImage">GDI+ image to copy.</param>
		/// <param name="format">Format for the image.</param>
		public void Copy(Drawing.Bitmap gdiImage, ImageBufferFormats format)
		{
			Copy(gdiImage, 0, 0, format, Drawing.Color.FromArgb(0,0,0,0));
		}

		/// <summary>
		/// Function to copy the contents of a gdi image into this image.
		/// </summary>
		/// <param name="gdiImage">GDI+ image to copy.</param>
		/// <param name="width">New width of the image.  0 to keep current.</param>
		/// <param name="height">New height of the image.  0 to keep current.</param>
		/// <param name="format">Format for the image.</param>
		public void Copy(Drawing.Bitmap gdiImage, int width, int height, ImageBufferFormats format)
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
		public void Copy(Drawing.Bitmap gdiImage, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			Stream memoryImage = null;			// Stream containing the image.

			// Ensure the image is the correct size.
			if ((width > Gorgon.Driver.MaximumTextureWidth) || (height > Gorgon.Driver.MaximumTextureHeight) || (width < 0) || (height < 0))
				throw new ImageSizeException(Name, width, height, Gorgon.Driver.MaximumTextureWidth, Gorgon.Driver.MaximumTextureHeight, null);

			try
			{
				D3D.SurfaceDescription surfaceInfo;	// Surface description.			

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
				if (!Gorgon.Driver.SupportNonSquareTexture)
				{
					if (_actualWidth > _actualHeight)
						_actualHeight = _actualWidth;
					else
						_actualWidth = _actualHeight;
				}

				// Resize to the power of two.
				if ((!Gorgon.Driver.SupportNonPowerOfTwoTexture) && (!Gorgon.Driver.SupportNonPowerOfTwoTextureConditional))
				{
					Drawing.Size newSize;		// New image size.

					newSize = ResizePowerOf2(_actualWidth, _actualHeight);
					_actualWidth = newSize.Width;
					_actualHeight = newSize.Height;
				}

				// Load the image.
				memoryImage = new MemoryStream();
				gdiImage.Save(memoryImage, Drawing.Imaging.ImageFormat.Png);
				memoryImage.Position = 0;
				_d3dImage = D3D.TextureLoader.FromStream(Gorgon.Screen.Device, memoryImage, _actualWidth, _actualHeight, 1, Usage, Converter.Convert(format), Pool, D3D.Filter.None, D3D.Filter.None, colorKey.ToArgb());
				surfaceInfo = _d3dImage.GetLevelDescription(0);
				_actualWidth = surfaceInfo.Width;
				_actualHeight = surfaceInfo.Height;
				_width = width;
				_height = height;
				_format = Converter.ConvertD3DImageFormat(surfaceInfo.Format);
				_filename = string.Empty;
				_isResource = false;
			}
			catch(Exception ex)
			{
				throw new CannotLoadException(_objectName, GetType(), ex);
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
		public void Copy(Drawing.Bitmap gdiImage, Drawing.Color colorKey)
		{
			Copy(gdiImage, 0, 0, ImageBufferFormats.BufferUnknown, colorKey);
		}

		/// <summary>
		/// Function to copy the contents of a gdi image into this image.
		/// </summary>
		/// <param name="gdiImage">GDI+ image to copy.</param>
		public void Copy(Drawing.Bitmap gdiImage)
		{
			Copy(gdiImage, 0, 0, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0,0,0,0));
		}

		/// <summary>
		/// Function to return whether the current device supports a specified image format.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <returns>TRUE if supported, FALSE if not.</returns>
		public bool SupportsFormat(ImageBufferFormats format)
		{
			return (ValidateFormat(format) == format);
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
			ImageSerializer serializer = null;		// Serializer object.
			string filename = string.Empty;			// Filename.

			try
			{
				if (stream is FileStream)
					filename = ((FileStream)stream).Name;
				else
					filename = _objectName;

				// Open the serializer.
				serializer = new ImageSerializer(this, stream);
				serializer.DontCloseStream = true;

				serializer.Parameters.AddParameter<D3D.ImageFileFormat>("FileFormat", Converter.Convert(fileFormat));

				serializer.Serialize();
			}
			catch(Exception ex)
			{
				throw new CannotSaveException(filename, GetType(), ex);
			}
		}

		/// <summary>
		/// Function to save an image.
		/// </summary>
		/// <param name="fileSystem">File system to save image within.</param>
		/// <param name="filename">Filename of the image to save.</param>
		/// <param name="fileFormat">Format to use for the file.</param>
		public void Save(FileSystem fileSystem, string filename, ImageFileFormat fileFormat)
		{
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
			catch
			{
				throw;
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
			catch
			{
				throw;
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
			return new ImageLockBox(this, new Drawing.Rectangle(0,0,_actualWidth,_actualHeight));
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
		/// <param name="isResource">TRUE if the image is a resource, FALSE if not.</param>
		/// <param name="createTexture">TRUE to create an empty texture, FALSE to bypass.</param>
		internal Image(string name, ImageType imageType, int width, int height, ImageBufferFormats format, bool isResource, bool createTexture)
			: base(name)
		{
			_actualHeight = -1;
			_actualWidth = -1;
			_locks = new List<ImageLockBox>();
			_filename = string.Empty;
			_isResource = isResource;

			// Determine format from the video mode.
			if (format == ImageBufferFormats.BufferUnknown)
			{
				switch (Gorgon.VideoMode.Format)
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
				Gorgon.DeviceStateList.Add(this);

			// If we pass in a width & height, then initialize.
			if (createTexture)
				Initialize();
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
				// Remove any open locks.
				foreach (ImageLockBox box in _locks)
					box.Dispose();
				_locks.Clear();

				if (_d3dImage != null)
					_d3dImage.Dispose();
				if (Gorgon.DeviceStateList.Contains(this))
					Gorgon.DeviceStateList.Remove(this);
				Gorgon.Log.Print("Image", "Destroying image \"{0}\".", LoggingLevel.Simple, Name);
			}

			_d3dImage = null;
		}

		/// <summary>
		/// Function to perform clean up, used by the image manager.
		/// </summary>
		/// <remarks>Users should call ImageManager.Remove() to destroy an image.</remarks>
		internal void Dispose()
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

			if (Pool != D3D.Pool.Managed)
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
            if (Pool != D3D.Pool.Managed)
				Refresh();
		}

		/// <summary>
		/// Function to force the loss of the objects data.
		/// </summary>
		public void ForceRelease()
		{
			if (_d3dImage != null)
				_d3dImage.Dispose();
			_d3dImage = null;
		}
		#endregion

		#region ISerializable Members
		/// <summary>
		/// Property to set or return the filename of the serializable object.
		/// </summary>
		string ISerializable.Filename
		{
			get
			{
				return _filename;
			}
			set
			{
				if (value == null)
					value = string.Empty;

				_filename = value;
			}
		}

		/// <summary>
		/// Function to persist the data into the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		void ISerializable.WriteData(ISerializer serializer)
		{
			DX.GraphicsStream dxStream = null;		// Graphics stream.
			byte[] data = null;						// Data in the stream.
			D3D.ImageFileFormat fileFormat;			// Image file format.

			if (!serializer.Parameters.Contains("FileFormat"))
				throw new InvalidResourceException(_objectName, GetType(), null);

			// Get the file format.
			fileFormat = serializer.Parameters.GetParameter<D3D.ImageFileFormat>("FileFormat");

			// Save to a graphics stream.
			dxStream = D3D.TextureLoader.SaveToStream(fileFormat, _d3dImage);
			dxStream.Position = 0;

			// Transfer to our stream.
			data = new byte[dxStream.Length];
			dxStream.Read(data, 0, (int)dxStream.Length);
			serializer.Stream.Write(data, 0, data.Length);
		}

		/// <summary>
		/// Function to retrieve data from the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		void ISerializable.ReadData(ISerializer serializer)
		{
			D3D.ImageInformation imageInfo;		// Image information.
			D3D.SurfaceDescription surfaceInfo;	// Surface description.
			int size;							// Size of image in bytes.
			int width;							// Alternate width.
			int height;							// Alternate height.
			ImageBufferFormats format;			// Alternate image format.
			Drawing.Color colorkey;				// Color key.
			
			if ((!serializer.Parameters.Contains("byteSize")) || (!serializer.Parameters.Contains("Width")) ||
				(!serializer.Parameters.Contains("Height")) || (!serializer.Parameters.Contains("Format")) ||
				(!serializer.Parameters.Contains("ColorKey")))
				throw new InvalidResourceException(_objectName, GetType(), null);

			// Get image parameters.
			size = serializer.Parameters.GetParameter<int>("byteSize");
			width = serializer.Parameters.GetParameter<int>("Width");
			height = serializer.Parameters.GetParameter<int>("Height");
			format = serializer.Parameters.GetParameter<ImageBufferFormats>("Format");
			colorkey = serializer.Parameters.GetParameter<Drawing.Color>("ColorKey");
			
			// Read in the image.
			if (_d3dImage != null)
				_d3dImage.Dispose();
			_d3dImage = null;

			// Ensure the image is the correct size.
			if (width > Gorgon.Driver.MaximumTextureWidth)
				width = Gorgon.Driver.MaximumTextureWidth;
			if (height > Gorgon.Driver.MaximumTextureHeight)
				height = Gorgon.Driver.MaximumTextureHeight;

			// Set to defaults.
			if (width < 0)
				width = 0;
			if (height < 0)
				height = 0;
				
			// If we specified unknown for the buffer format, assume 32 bit.
			if (format == ImageBufferFormats.BufferUnknown)
			{
				format = ValidateFormat(ImageBufferFormats.BufferRGB888A8);

				if (format == ImageBufferFormats.BufferUnknown)
					throw new FormatNotSupportedException("Unable to find a suitable image format.", null);
			}
			else
			{
				// Throw an exception if the format requested is not supported.
				if (!SupportsFormat(format))
					throw new FormatNotSupportedException(format, null);
			}

			// Load the image.
			imageInfo = new D3D.ImageInformation();		
			if (size > 0)
				_d3dImage = D3D.TextureLoader.FromStream(Gorgon.Screen.Device, serializer.Stream, size, width, height, 1, Usage, Converter.Convert(format), Pool, D3D.Filter.None, D3D.Filter.None, colorkey.ToArgb(), ref imageInfo);
			else
				_d3dImage = D3D.TextureLoader.FromStream(Gorgon.Screen.Device, serializer.Stream, width, height, 1, Usage, Converter.Convert(format), Pool, D3D.Filter.None, D3D.Filter.None, colorkey.ToArgb(), ref imageInfo);

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
