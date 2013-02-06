#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, January 31, 2013 8:29:10 AM
// 

// This code was adapted from the SharpDX ToolKit by Alexandre Mutel.
#region SharpDX License.
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

// SharpDX is available from http://sharpdx.org
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using DX = SharpDX;
using WIC = SharpDX.WIC;
using GorgonLibrary.Native;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// A container for raw image data that can be sent to or read from a image.
    /// </summary>
    /// <remarks>This object will allow pixel manipulation of image data.  It will break a image into buffers, such as a series of buffers 
    /// for arrays (for 1D and 2D images only), mip-map levels and depth slices (for 3D images only).
	/// <para>The object takes its settings from an object that implements <see cref="GorgonLibrary.Graphics.IImageSettings">IImageSettings</see>.  All texture settings objects such as 
	/// <see cref="GorgonLibrary.Graphics.GorgonTexture1DSettings">GorgonTexture1DSettings</see>, <see cref="GorgonLibrary.Graphics.GorgonTexture2DSettings">GorgonTexture2DSettings</see> and 
	/// <see cref="GorgonLibrary.Graphics.GorgonTexture3DSettings">GorgonTexture3DSettings</see> implement the IImageSettings interface and can be used with this object.</para>
	/// </remarks>
    public unsafe class GorgonImageData
        : IDisposable, IEnumerable<GorgonImageData.ImageBuffer>, System.Collections.IEnumerable        
	{
		#region Classes.
		/// <summary>
		/// An image buffer containing data about the image.
		/// </summary>
		public class ImageBuffer
		{
			#region Properties.
			/// <summary>
			/// Property to return the format of the buffer.
			/// </summary>
			public BufferFormat Format
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the width for the current buffer.
			/// </summary>
			public int Width
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the height for the current buffer.
			/// </summary>
			/// <remarks>This is only valid for 2D and 3D images.</remarks>
			public int Height
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the depth for the current buffer.
			/// </summary>
			/// <remarks>This is only valid for 3D images.</remarks>
			public int Depth
			{
				get;
				private set;
			}
			
			/// <summary>
			/// Property to return the mip map level this buffer represents.
			/// </summary>
			public int MipLevel
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the array this buffer represents.
			/// </summary>
			/// <remarks>For 3D images, this will always be 0.</remarks>
			public int ArrayIndex
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the depth slice index.
			/// </summary>
			/// <remarks>For 1D or 2D images, this will always be 0.</remarks>
			public int SliceIndex
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the data stream for the image data.
			/// </summary>
			public GorgonDataStream Data
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return information about the pitch of the data for this buffer.
			/// </summary>
			public GorgonFormatPitch PitchInformation
			{
				get;
				private set;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="ImageBuffer" /> class.
			/// </summary>
			/// <param name="dataStart">The data start.</param>
			/// <param name="pitchInfo">The pitch info.</param>
			/// <param name="mipLevel">Mip map level.</param>
			/// <param name="arrayIndex">Array index.</param>
			/// <param name="sliceIndex">Slice index.</param>
			/// <param name="width">The width for the buffer.</param>
			/// <param name="height">The height for the buffer.</param>
			/// <param name="depth">The depth for the buffer.</param>
			/// <param name="format">Format of the buffer.</param>
			internal unsafe ImageBuffer(void *dataStart, GorgonFormatPitch pitchInfo, int mipLevel, int arrayIndex, int sliceIndex, int width, int height, int depth, BufferFormat format)
			{
				Data = new GorgonDataStream(dataStart, pitchInfo.SlicePitch);
				PitchInformation = pitchInfo;
				MipLevel = mipLevel;
				ArrayIndex = arrayIndex;
				SliceIndex = sliceIndex;
				Width = width;
				Height = height;
				Depth = depth;
				Format = format;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private DX.DataBox[] _dataBoxes = null;						// Data boxes for textures.
		private bool _disposed = false;                             // Flag to indicate whether the object was disposed.
        private GorgonDataStream _imageData = null;                 // Base image data buffer.
		private ImageBuffer[] _buffers = null;						// Buffers for access to the arrays, slices and mip maps.
		private Tuple<int, int>[] _mipOffsetSize = null;			// Offsets and sizes in the buffer list for mip maps.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the settings for the image.
        /// </summary>
        public IImageSettings Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the number of bytes, in total, that this image occupies.
        /// </summary>
        public int SizeInBytes
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to return the total number of buffers allocated.
		/// </summary>
		public int Count
		{
			get
			{
				return _buffers.Length;
			}
		}

		/// <summary>
		/// Property to return the buffer for the given array index, mip map level and depth slice.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the array index, mip level or depth slice parameters are larger than their respective boundaries, or less than 0.</exception>
		/// <remarks>To get the array length and/or the mip map count, use the <see cref="P:GorgonLibrary.Graphics.GorgonImageData{T}.Settings">Settings</see> property.
		/// <para>To get the depth slice count, use the <see cref="P:GorgonLibrary.Graphics.GorgonImageData{T}.GetDepthCount">GetDepthCount</see> method.</para>
		/// </remarks>
		public ImageBuffer this[int arrayIndex, int mipLevel, int depthSlice]
		{
			get
			{
				GorgonDebug.AssertParamRange(arrayIndex, 0, Settings.ArrayCount, "arrayIndex");
				GorgonDebug.AssertParamRange(mipLevel, 0, Settings.MipCount, "mipLevel");

				Tuple<int, int> offsetSize = _mipOffsetSize[mipLevel + (arrayIndex * Settings.MipCount)];

				GorgonDebug.AssertParamRange(depthSlice, 0, offsetSize.Item2, "depthSlice");

				return _buffers[offsetSize.Item1 + depthSlice];
			}
		}

		/// <summary>
		/// Property to return the buffer for the given mip map level and depth slice.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the array index or the depth slice parameters are larger than their respective boundaries, or less than 0.</exception>
		/// <remarks>To get the array length and/or the mip map count, use the <see cref="P:GorgonLibrary.Graphics.GorgonImageData{T}.Settings">Settings</see> property.
		/// </remarks>
		public ImageBuffer this[int arrayIndex, int mipLevel]
		{
			get
			{
				GorgonDebug.AssertParamRange(arrayIndex, 0, Settings.ArrayCount, "arrayIndex");
				GorgonDebug.AssertParamRange(mipLevel, 0, Settings.MipCount, "mipLevel");

				Tuple<int, int> offsetSize = _mipOffsetSize[mipLevel + (arrayIndex * Settings.MipCount)];

				return _buffers[offsetSize.Item1];
			}
		}

		/// <summary>
		/// Property to return the buffer for the given mip map level.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the mip map level parameter is larger than the number of mip levels, or less than 0.</exception>
		/// <remarks>To get the mip map count, use the <see cref="P:GorgonLibrary.Graphics.GorgonImageData{T}.Settings">Settings</see> property.
		/// </remarks>
		public ImageBuffer this[int mipLevel]
		{
			get
			{
				GorgonDebug.AssertParamRange(mipLevel, 0, Settings.MipCount, "mipLevel");

				Tuple<int, int> offsetSize = _mipOffsetSize[mipLevel];

				return _buffers[offsetSize.Item1];
			}
		}
		#endregion

        #region Methods.
		/// <summary>
		/// Function to initialize the image data.
		/// </summary>
		/// <param name="data">Pre-existing data to use.</param>
		private void Initialize(void* data)
        {
			int bufferIndex = 0;
			var formatInfo = GorgonBufferFormatInfo.GetInfo(Settings.Format);	// Format information.

            // Create a buffer large enough to hold our data.
            if (data == null)
            {
                _imageData = new GorgonDataStream(SizeInBytes);
				DirectAccess.ZeroMemory(_imageData.UnsafePointer, SizeInBytes);
            }
            else
            {
                _imageData = new GorgonDataStream(data, SizeInBytes);
            }

			// Create buffers.
			_buffers = new ImageBuffer[GetDepthSliceCount(Settings.Depth, Settings.MipCount) * Settings.ArrayCount];	// Allocate enough room for the array and mip levels.
			_mipOffsetSize = new Tuple<int, int>[Settings.MipCount * Settings.ArrayCount];								// Offsets for the mip maps.
			_dataBoxes = new DX.DataBox[Settings.ArrayCount * Settings.MipCount];										// Create the data boxes for textures.
			byte* imageData = (byte*)_imageData.UnsafePointer;															// Start at the beginning of our data block.
			
			// Enumerate array indices. (For 1D and 2D only, 3D will always be 1)
			for (int array = 0; array < Settings.ArrayCount; array++)
			{
                int mipWidth = Settings.Width;
                int mipHeight = Settings.Height;
                int mipDepth = Settings.Depth;
                
                // Enumerate mip map levels.
				for (int mip = 0; mip < Settings.MipCount; mip++)
				{
					int arrayIndex = mip + (array * Settings.MipCount);
					var pitchInformation = formatInfo.GetPitch(mipWidth, mipHeight, PitchFlags.None);

					// Get data box for texture upload.
					_dataBoxes[arrayIndex] = new DX.DataBox(new IntPtr(imageData), pitchInformation.RowPitch, pitchInformation.SlicePitch);

					// Calculate buffer offset by mip.
					_mipOffsetSize[arrayIndex] = new Tuple<int, int>(bufferIndex, mipDepth);

					// Enumerate depth slices.
					for (int depth = 0; depth < mipDepth; depth++)
					{
						// Get mip information.						
						_buffers[bufferIndex] = new ImageBuffer(imageData, pitchInformation, mip, array, depth, mipWidth, mipHeight, mipDepth, Settings.Format);

						imageData += pitchInformation.SlicePitch;
						bufferIndex++;
					}					

					if (mipWidth > 1)
					{
						mipWidth >>= 1;
					}
					if (mipHeight > 1)
					{
						mipHeight >>= 1;
					}
					if (mipDepth > 1)
					{
						mipDepth >>= 1;
					}
				}
			}
        }

		/// <summary>
		/// Function to sanitize the image settings.
		/// </summary>
		private void SanitizeSettings()
        {
			Settings.ArrayCount = 1.Max(Settings.ArrayCount);
			Settings.Width = 1.Max(Settings.Width);
			Settings.Height = 1.Max(Settings.Height);
			Settings.Depth = 1.Max(Settings.Depth);

			// Ensure mip values do not exceed more than what's available based on width, height and/or depth.
			if (Settings.MipCount > 1)
			{
				Settings.MipCount = Settings.MipCount.Min(GorgonImageData.GetMaxMipCount(Settings));
			}

			// Create mip levels if we didn't specify any.
			if (Settings.MipCount == 0)
			{
				Settings.MipCount = GorgonImageData.GetMaxMipCount(Settings);
			}
        }

		/// <summary>
		/// Function convert the data into Direct 3D data boxes.
		/// </summary>
		/// <returns>An array of data rectangles.</returns>
		internal DX.DataBox[] GetDataBoxes()
		{
			return _dataBoxes;
		}

        /// <summary>
        /// Function to create a new 1D image data object from a GDI+ Image.
        /// </summary>
        /// <param name="image">A GDI+ image object to use as the source image data.</param>
        /// <returns>
        /// A new 1D image data object containing a copy of the System.Drawing.Image data.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the image parameter is NULL (Nothing in VB.Net)
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when pixel format in the options cannot be converted or is not supported.</exception>
        /// <remarks>
        /// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> object from a <see cref="System.Drawing.Image">GDI+ Image</see>.
        /// The method will copy the image information and do a best fit conversion.
        /// </remarks>
        public static GorgonImageData Create1DFromGDIImage(Image image)
        {
            return Create1DFromGDIImage(image, new GorgonGDIOptions());
        }

        /// <summary>
        /// Function to create a new 1D image data object from a GDI+ Image.
        /// </summary>
        /// <param name="image">A GDI+ image object to use as the source image data.</param>
        /// <param name="options">Options for image conversion.</param>
        /// <returns>
        /// A new 1D image data object containing a copy of the System.Drawing.Image data.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the image parameter is NULL (Nothing in VB.Net)
        /// <para>-or-</para>
        /// <para>Thrown when the options parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when pixel format in the options cannot be converted or is not supported.</exception>
        /// <remarks>
        /// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> object from a <see cref="System.Drawing.Image">GDI+ Image</see>.
        /// The method will copy the image information and do a best fit conversion.
        /// <para>The <paramref name="options"/> parameter controls how the <paramref name="image" /> is converted.  Here is a list of available conversion options:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Setting</term><term>Description</term>
        /// </listheader>
        /// <item>
        /// <description>Width</description><description>The image will be resized to match the width specified.  Set to 0 to use the original image width.</description>
        /// </item>
        /// <item>
        /// <description>Height</description><description>This is ignored for 1D images.</description>
        /// </item>
        /// <item>
        /// <description>Depth</description><description>This is ignored for 1D images.</description>
        /// </item>
        /// <item>
        /// <description>Format</description><description>The image will be converted to the format specified.  Set to Unknown to map to the closest available format.</description>
        /// </item>
        /// <item>
        /// <description>MipCount</description><description>Gorgon will generate the requested number of mip-maps from the source image.  Set to 0 to generate a full mip-map chain, or set to 1 if no mip-maps are required.</description>
        /// </item>
        /// <item>
        /// <description>ArrayCount</description><description>This is ignored for this overload.</description>
        /// </item>
        /// <item>
        /// <description>Dither</description><description>Dithering to apply to images with a higher bit depth than the specified format.  The default value is None.</description>
        /// </item>
        /// <item>
        /// <description>Filter</description><description>Filtering to apply to images that are scaled to the width/height specified.  The default value is Point.</description>
        /// </item>
        /// <item>
        /// <description>UseClipping</description><description>Set to TRUE to clip the image instead of scaling when the width/height is smaller than the image width/height.  The default value is FALSE.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public static GorgonImageData Create1DFromGDIImage(Image image, GorgonGDIOptions options)
        {            
            
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            // Default these values to 1 for 1D images.
			options.ArrayCount = 1;
            options.Height = 1;
            options.Depth = 1;

            using (var wic = new GorgonWICImage())
            {
                return GorgonGDIImageConverter.Create1DImageDataFromImage(wic, image, options);
            }
        }

		/// <summary>
		/// Function to create a new 2D image data object from a GDI+ Image.
		/// </summary>
		/// <param name="image">A GDI+ image object to use as the source image data.</param>
		/// <returns>
		/// A new 2D image data object containing a copy of the System.Drawing.Image data.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the image parameter is NULL (Nothing in VB.Net)
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when pixel format in the options cannot be converted or is not supported.</exception>
		/// <remarks>
		/// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> object from a <see cref="System.Drawing.Image">GDI+ Image</see>.
		/// The method will copy the image information and do a best fit conversion.
		/// </remarks>
        public static GorgonImageData Create2DFromGDIImage(Image image)
        {
            return Create2DFromGDIImage(image, new GorgonGDIOptions());
        }

		/// <summary>
		/// Function to create a new 2D image data object from a GDI+ Image.
		/// </summary>
		/// <param name="image">A GDI+ image object to use as the source image data.</param>
		/// <param name="options">Options for image conversion.</param>
		/// <returns>
		/// A new 2D image data object containing a copy of the System.Drawing.Image data.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the image parameter is NULL (Nothing in VB.Net)
		/// <para>-or-</para>
		/// <para>Thrown when the options parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when pixel format in the options cannot be converted or is not supported.</exception>
		/// <remarks>
		/// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> object from a <see cref="System.Drawing.Image">GDI+ Image</see>.
		/// The method will copy the image information and do a best fit conversion.
		/// <para>The <paramref name="options"/> parameter controls how the <paramref name="image" /> is converted.  Here is a list of available conversion options:</para>
		/// <list type="table">
		/// <listheader>
		/// <term>Setting</term><term>Description</term>
		/// </listheader>
		/// <item>
		/// <description>Width</description><description>The image will be resized to match the width specified.  Set to 0 to use the original image width.</description>
		/// </item>
		/// <item>
		/// <description>Height</description><description>The image will be resized to match the height specified.  Set to 0 to use the original image height.</description>
		/// </item>
		/// <item>
		/// <description>Depth</description><description>This is ignored for 2D images.</description>
		/// </item>
		/// <item>
		/// <description>Format</description><description>The image will be converted to the format specified.  Set to Unknown to map to the closest available format.</description>
		/// </item>
		/// <item>
		/// <description>MipCount</description><description>Gorgon will generate the requested number of mip-maps from the source image.  Set to 0 to generate a full mip-map chain, or set to 1 if no mip-maps are required.</description>
		/// </item>
		/// <item>
		/// <description>ArrayCount</description><description>This is ignored for this overload.</description>
		/// </item>
		/// <item>
		/// <description>Dither</description><description>Dithering to apply to images with a higher bit depth than the specified format.  The default value is None.</description>
		/// </item>
		/// <item>
		/// <description>Filter</description><description>Filtering to apply to images that are scaled to the width/height specified.  The default value is Point.</description>
		/// </item>
		/// <item>
		/// <description>UseClipping</description><description>Set to TRUE to clip the image instead of scaling when the width/height is smaller than the image width/height.  The default value is FALSE.</description>
		/// </item>
		/// </list>
		/// </remarks>
		public static GorgonImageData Create2DFromGDIImage(Image image, GorgonGDIOptions options)
		{

			if (image == null)
			{
				throw new ArgumentNullException("image");
			}

			if (options == null)
			{
				throw new ArgumentNullException("options");
			}

			// Default these values to 1 for 2D images.
			options.ArrayCount = 1;
			options.Depth = 1;

			using (var wic = new GorgonWICImage())
			{
				return GorgonGDIImageConverter.Create2DImageDataFromImage(wic, image, options);
			}
		}

        /// <summary>
        /// Function to create a new 1D image data object from a list of <see cref="System.Drawing.Image">GDI+ Images</see>.
        /// </summary>
        /// <param name="images">A list of GDI+ image objects to use as the source image data.</param>
        /// <param name="options">Options for image conversion.</param>
        /// <returns>
        /// A new 1D image data object containing a copy of the System.Drawing.Image data.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown when the images parameter is NULL (Nothing in VB.Net) or empty.
		/// <para>-or-</para>
		/// <para>Thrown when the pixel format is unsupported or not the same across all images.</para>
		/// <para>-or-</para>
		/// <para>Thrown when an image in the list is NULL.</para>		
        /// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the images parameter does not contain enough elements to satisfy the array and mip count.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the options parameter is NULL.</exception>
        /// <remarks>
        /// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> object from a list of <see cref="System.Drawing.Image">GDI+ Images</see>.
        /// The method will copy the image information and do a best fit conversion.
        /// <para>This overload is used to create image arrays and/or mip-map chains from a list of images.  If the MipCount and ArrayCount are set to 1, then
        /// only the first image will be processed.</para>
        /// <para>The layout of the image list is processed in the following order (assuming the ArrayCount = 2, and the MipCount = 4):</para>
        /// <code>
        /// images[0]: Array Index 0, Mip Level 0
        /// images[1]: Array Index 0, Mip Level 1
        /// images[2]: Array Index 0, Mip Level 2
        /// images[3]: Array Index 0, Mip Level 3
        /// images[4]: Array Index 1, Mip Level 0
        /// images[5]: Array Index 1, Mip Level 1
        /// images[6]: Array Index 1, Mip Level 2
        /// images[7]: Array Index 1, Mip Level 3
        /// </code>
        /// <para>The <paramref name="options" /> parameter controls how the <paramref name="images" /> are converted.  Here is a list of available conversion options:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Setting</term><term>Description</term>
        /// </listheader>
        /// <item>
        /// <description>Width</description><description>The image will be resized to match the width specified.  Set to 0 to use the original image width.</description>
        /// </item>
        /// <item>
        /// <description>Height</description><description>This is ignored for 1D images.</description>
        /// </item>
        /// <item>
        /// <description>Depth</description><description>This is ignored for 1D images.</description>
        /// </item>
        /// <item>
        /// <description>Format</description><description>The image will be converted to the format specified.  Set to Unknown to map to the closest available format.</description>
        /// </item>
        /// <item>
        /// <description>MipCount</description><description>Gorgon will generate the requested number of mip-maps from the source image.  Set to 1 if no mip-maps are required.</description>
        /// </item>
        /// <item>
        /// <description>ArrayCount</description><description>Gorgon will generate the requested number of image arrays from the source image.  Set to 1 if no image arrays are required.</description>
        /// </item>
        /// <item>
        /// <description>Dither</description><description>Dithering to apply to images with a higher bit depth than the specified format.  The default value is None.</description>
        /// </item>
        /// <item>
        /// <description>Filter</description><description>Filtering to apply to images that are scaled to the width/height specified.  The default value is Point.</description>
        /// </item>
        /// <item>
        /// <description>UseClipping</description><description>Set to TRUE to clip the image instead of scaling when the width/height is smaller than the image width/height.  The default value is FALSE.</description>
        /// </item>
        /// </list>
        /// <para>The list of images must be large enough to accomodate the number of array indices and mip map levels (ArrayCount * MipCount), must not contain any NULL (Nothing in VB.Net) elements and all images must use
        /// the same pixel format.  If the list is larger than the requested mip/array count, then only the first elements up until ArrayCount * MipCount are used.  Unlike other overloads, this method will NOT auto-generate
        /// mip-maps and will only use the images provided.</para>
        /// <para>Images in the list to be used as mip-map levels do not need to be resized because the method will automatically resize based on mip-map level.</para>
        /// </remarks>
        public static GorgonImageData Create1DFromGDIImage(IList<Image> images, GorgonGDIOptions options)
        {
            if ((images == null) || (images.Count == 0))
            {
                throw new ArgumentException("The parameter must not be NULL or empty.", "images");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            
            using (var wic = new GorgonWICImage())
            {
                return GorgonGDIImageConverter.Create1DImageDataFromImages(wic, images, options);
            }
        }

		/// <summary>
		/// Function to create a new 2D image data object from a list of <see cref="System.Drawing.Image">GDI+ Images</see>.
		/// </summary>
		/// <param name="images">A list of GDI+ image objects to use as the source image data.</param>
		/// <param name="options">Options for image conversion.</param>
		/// <returns>
		/// A new 2D image data object containing a copy of the System.Drawing.Image data.
		/// </returns>
		/// <exception cref="System.ArgumentException">Thrown when the images parameter is NULL (Nothing in VB.Net) or empty.
		/// <para>-or-</para>
		/// <para>Thrown when the pixel format is unsupported or not the same across all images.</para>
		/// <para>-or-</para>
		/// <para>Thrown when an image in the list is NULL.</para>		
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the images parameter does not contain enough elements to satisfy the array and mip count.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the options parameter is NULL.</exception>
		/// <remarks>
		/// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> object from a list of <see cref="System.Drawing.Image">GDI+ Images</see>.
		/// The method will copy the image information and do a best fit conversion.
		/// <para>This overload is used to create image arrays and/or mip-map chains from a list of images.  If the MipCount and ArrayCount are set to 1, then
		/// only the first image will be processed.</para>
		/// <para>The layout of the image list is processed in the following order (assuming the ArrayCount = 2, and the MipCount = 4):</para>
		/// <code>
		/// images[0]: Array Index 0, Mip Level 0
		/// images[1]: Array Index 0, Mip Level 1
		/// images[2]: Array Index 0, Mip Level 2
		/// images[3]: Array Index 0, Mip Level 3
		/// images[4]: Array Index 1, Mip Level 0
		/// images[5]: Array Index 1, Mip Level 1
		/// images[6]: Array Index 1, Mip Level 2
		/// images[7]: Array Index 1, Mip Level 3
		/// </code>
		/// <para>The <paramref name="options" /> parameter controls how the <paramref name="images" /> are converted.  Here is a list of available conversion options:</para>
		/// <list type="table">
		/// <listheader>
		/// <term>Setting</term><term>Description</term>
		/// </listheader>
		/// <item>
		/// <description>Width</description><description>The image will be resized to match the width specified.  Set to 0 to use the original image width.</description>
		/// </item>
		/// <item>
		/// <description>Height</description><description>The image will be resized to match the height specified.  Set to 0 to use the original image height.</description>
		/// </item>
		/// <item>
		/// <description>Depth</description><description>This is ignored for 2D images.</description>
		/// </item>
		/// <item>
		/// <description>Format</description><description>The image will be converted to the format specified.  Set to Unknown to map to the closest available format.</description>
		/// </item>
		/// <item>
		/// <description>MipCount</description><description>Gorgon will generate the requested number of mip-maps from the source image.  Set to 1 if no mip-maps are required.</description>
		/// </item>
		/// <item>
		/// <description>ArrayCount</description><description>Gorgon will generate the requested number of image arrays from the source image.  Set to 1 if no image arrays are required.</description>
		/// </item>
		/// <item>
		/// <description>Dither</description><description>Dithering to apply to images with a higher bit depth than the specified format.  The default value is None.</description>
		/// </item>
		/// <item>
		/// <description>Filter</description><description>Filtering to apply to images that are scaled to the width/height specified.  The default value is Point.</description>
		/// </item>
		/// <item>
		/// <description>UseClipping</description><description>Set to TRUE to clip the image instead of scaling when the width/height is smaller than the image width/height.  The default value is FALSE.</description>
		/// </item>
		/// </list>
		/// <para>The list of images must be large enough to accomodate the number of array indices and mip map levels (ArrayCount * MipCount), must not contain any NULL (Nothing in VB.Net) elements and all images must use
		/// the same pixel format.  If the list is larger than the requested mip/array count, then only the first elements up until ArrayCount * MipCount are used.  Unlike other overloads, this method will NOT auto-generate
		/// mip-maps and will only use the images provided.</para>
		/// <para>Images in the list to be used as mip-map levels do not need to be resized because the method will automatically resize based on mip-map level.</para>
		/// </remarks>
		public static GorgonImageData Create2DFromGDIImage(IList<Image> images, GorgonGDIOptions options)
		{
			if ((images == null) || (images.Count == 0))
			{
				throw new ArgumentException("The parameter must not be NULL or empty.", "images");
			}

			if (options == null)
			{
				throw new ArgumentNullException("options");
			}

			using (var wic = new GorgonWICImage())
			{
				return GorgonGDIImageConverter.Create2DImageDataFromImages(wic, images, options);
			}
		}

		/// <summary>
		/// Function to create a new 3D image data object from a list of <see cref="System.Drawing.Image">GDI+ Images</see>.
		/// </summary>
		/// <param name="images">A list of GDI+ image objects to use as the source image data.</param>
		/// <returns>
		/// A new 3D image data object containing a copy of the System.Drawing.Image data.
		/// </returns>
		/// <exception cref="System.ArgumentException">Thrown when the images parameter is NULL (Nothing in VB.Net) or empty.
		/// <para>-or-</para>
		/// <para>Thrown when the pixel format is unsupported or not the same across all images.</para>
		/// <para>-or-</para>
		/// <para>Thrown when an image in the list is NULL.</para>		
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the images parameter does not contain enough elements to satisfy the depth and mip count.</exception>
		/// <remarks>
		/// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> object from a list of <see cref="System.Drawing.Image">GDI+ Images</see>.
		/// The method will copy the image information and do a best fit conversion.
		/// <para>This overload is used to create a 3D image from a list of images.  If the MipCount is set to 1, then
		/// only the first image will be processed IF there is only one image in the list.  If there is more than 1 image in the list, and the mip count is set to 1, then the element count 
		/// of the list will be taken as the depth size.</para>
		/// <para>The layout of the image list is processed in the following order (assuming the MipCount = 2, and the depth is = 4):</para>
		/// <code>
		/// images[0]: Mip Level 0, Depth slice 0.
		/// images[1]: Mip Level 0, Depth slice 1
		/// images[2]: Mip Level 0, Depth slice 2
		/// images[3]: Mip Level 0, Depth slice 3
		/// images[4]: Mip Level 1, Depth slice 4
		/// images[5]: Mip Level 1, Depth slice 5
		/// </code>
		/// <para>The depth is shrunk by a power of 2 for each mip level.  So, at mip level 0 we have 4 depth slices, and at mip level 1 we have 2.  If we had a third mip level, then 
		/// the depth would be 1 at that mip level.</para>
		/// <para>Note that unlike other image types, there is no array.  3D images do not support arrays and will ignore them.  Also note that a 3D image MUST have a width, height 
		/// and depth that is a power of 2 if mip maps are to be used.  If the image does not meet the criteria, then an exception will be thrown.
		/// </para>
		/// <para>The list of images must be large enough to accomodate the number of mip map levels and the depth at each mip level, must not contain any NULL (Nothing in VB.Net) elements and all images must use
		/// the same pixel format.  If the list is larger than the requested mip/array count, then only the first elements up until mip count and each depth for each mip level are used.  Unlike other overloads, 
		/// this method will NOT auto-generate mip-maps and will only use the images provided.</para>
		/// <para>Images in the list to be used as mip-map levels do not need to be resized because the method will automatically resize based on mip-map level.</para>
		/// </remarks>
        public static GorgonImageData Create3DFromGDIImage(IList<Image> images)
        {
            return Create3DFromGDIImage(images, new GorgonGDIOptions());
        }

		/// <summary>
		/// Function to create a new 3D image data object from a list of <see cref="System.Drawing.Image">GDI+ Images</see>.
		/// </summary>
		/// <param name="images">A list of GDI+ image objects to use as the source image data.</param>
		/// <param name="options">Options for image conversion.</param>
		/// <returns>
		/// A new 3D image data object containing a copy of the System.Drawing.Image data.
		/// </returns>
		/// <exception cref="System.ArgumentException">Thrown when the images parameter is NULL (Nothing in VB.Net) or empty.
		/// <para>-or-</para>
		/// <para>Thrown when the pixel format is unsupported or not the same across all images.</para>
		/// <para>-or-</para>
		/// <para>Thrown when an image in the list is NULL.</para>		
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the images parameter does not contain enough elements to satisfy the depth and mip count.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the options parameter is NULL.</exception>
		/// <remarks>
		/// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> object from a list of <see cref="System.Drawing.Image">GDI+ Images</see>.
		/// The method will copy the image information and do a best fit conversion.
		/// <para>This overload is used to create a 3D image from a list of images.  If the MipCount is set to 1, then
		/// only the first image will be processed IF there is only one image in the list.  If there is more than 1 image in the list, and the mip count is set to 1, then the element count 
		/// of the list will be taken as the depth size.</para>
		/// <para>The layout of the image list is processed in the following order (assuming the MipCount = 2, and the depth is = 4):</para>
		/// <code>
		/// images[0]: Mip Level 0, Depth slice 0.
		/// images[1]: Mip Level 0, Depth slice 1
		/// images[2]: Mip Level 0, Depth slice 2
		/// images[3]: Mip Level 0, Depth slice 3
		/// images[4]: Mip Level 1, Depth slice 4
		/// images[5]: Mip Level 1, Depth slice 5
		/// </code>
		/// <para>The depth is shrunk by a power of 2 for each mip level.  So, at mip level 0 we have 4 depth slices, and at mip level 1 we have 2.  If we had a third mip level, then 
		/// the depth would be 1 at that mip level.</para>
		/// <para>Note that unlike other image types, there is no array.  3D images do not support arrays and will ignore them.  Also note that a 3D image MUST have a width, height 
		/// and depth that is a power of 2 if mip maps are to be used.  If the image does not meet the criteria, then an exception will be thrown.
		/// </para>
		/// <para>The <paramref name="options" /> parameter controls how the <paramref name="images" /> are converted.  Here is a list of available conversion options:</para>
		/// <list type="table">
		/// <listheader>
		/// <term>Setting</term><term>Description</term>
		/// </listheader>
		/// <item>
		/// <description>Width</description><description>The image will be resized to match the width specified.  Set to 0 to use the original image width.</description>
		/// </item>
		/// <item>
		/// <description>Height</description><description>The image will be resized to match the height specified.  Set to 0 to use the original image height.</description>
		/// </item>
		/// <item>
		/// <description>Depth</description><description>This sets the depth for the image.  The default value is set to 1.  If there are no mip-maps (i.e. MipCount = 1), then the number of elements in the list will be used as the depth size.</description>
		/// </item>
		/// <item>
		/// <description>Format</description><description>The image will be converted to the format specified.  Set to Unknown to map to the closest available format.</description>
		/// </item>
		/// <item>
		/// <description>MipCount</description><description>Gorgon will generate the requested number of mip-maps from the source image.  Set to 1 if no mip-maps are required.</description>
		/// </item>
		/// <item>
		/// <description>ArrayCount</description><description>Image arrays are not available for 3D images.</description>
		/// </item>
		/// <item>
		/// <description>Dither</description><description>Dithering to apply to images with a higher bit depth than the specified format.  The default value is None.</description>
		/// </item>
		/// <item>
		/// <description>Filter</description><description>Filtering to apply to images that are scaled to the width/height specified.  The default value is Point.</description>
		/// </item>
		/// <item>
		/// <description>UseClipping</description><description>Set to TRUE to clip the image instead of scaling when the width/height is smaller than the image width/height.  The default value is FALSE.</description>
		/// </item>
		/// </list>
		/// <para>The list of images must be large enough to accomodate the number of mip map levels and the depth at each mip level, must not contain any NULL (Nothing in VB.Net) elements and all images must use
		/// the same pixel format.  If the list is larger than the requested mip/array count, then only the first elements up until mip count and each depth for each mip level are used.  Unlike other overloads, 
		/// this method will NOT auto-generate mip-maps and will only use the images provided.</para>
		/// <para>Images in the list to be used as mip-map levels do not need to be resized because the method will automatically resize based on mip-map level.</para>
		/// </remarks>
		public static GorgonImageData Create3DFromGDIImage(IList<Image> images, GorgonGDIOptions options)
		{
			if ((images == null) || (images.Count == 0))
			{
				throw new ArgumentException("The parameter must not be NULL or empty.", "images");
			}

			if (options == null)
			{
				throw new ArgumentNullException("options");
			}

			using (var wic = new GorgonWICImage())
			{
				return GorgonGDIImageConverter.Create3DImageDataFromImages(wic, images, options);
			}
		}

        /// <summary>
        /// Function to copy texture data into a buffer.
        /// </summary>
        /// <param name="stagingTexture">Texture to copy.</param>
        /// <param name="buffer">Buffer that will contain the data.</param>
        /// <param name="arrayIndex">Index of the array to copy.</param>
        /// <param name="mipLevel">Mip level to copy.</param>
        /// <param name="rowStride">Stride of a single row in the buffer.</param>
        /// <param name="height">Height of the buffer.</param>
        /// <param name="depthCount">Number of depth levels in the buffer.</param>
        private static void GetTextureData(GorgonTexture stagingTexture, void *buffer, int arrayIndex, int mipLevel, int rowStride, int height, int depthCount)
        {
            int sliceStride = rowStride * height;
            int resourceIndex = 0;
            
            // Get the resource index from the texture.
            switch (stagingTexture.Settings.ImageType)
            {
                case ImageType.Image1D:
                    resourceIndex = GorgonTexture1D.GetSubResourceIndex(mipLevel, arrayIndex, stagingTexture.Settings.MipCount, stagingTexture.Settings.ArrayCount);
                    break;
                case ImageType.Image2D:
                    resourceIndex = GorgonTexture2D.GetSubResourceIndex(mipLevel, arrayIndex, stagingTexture.Settings.MipCount, stagingTexture.Settings.ArrayCount);
                    break;
                case ImageType.Image3D:
                    resourceIndex = GorgonTexture3D.GetSubResourceIndex(mipLevel, stagingTexture.Settings.MipCount);   
                    sliceStride = (height * rowStride) * depthCount;        // Calculate how big the buffer should be with depth.
                    break;
            }

            // Copy the texture data into the buffer.
            try
            {                
                var textureLock = stagingTexture.Lock<ISubResourceData>(resourceIndex, BufferLockFlags.Read);

                // If the strides don't match, then the texture is using padding, so copy one scanline at a time for each depth index.
                if ((textureLock.RowPitch != rowStride) || (textureLock.SlicePitch != sliceStride))
                {
                    byte* destData = (byte*)buffer;
                    byte* sourceData = (byte*)textureLock.Data.UnsafePointer;

                    for (int depth = 0; depth < depthCount; depth++)
                    {
                        // Restart at the padded slice size.
                        byte* sourceStart = sourceData;

                        for (int row = 0; row < height; row++)
                        {
                            DirectAccess.MemoryCopy(destData, sourceStart, rowStride);
                            sourceStart += textureLock.RowPitch;
                            destData += rowStride;
                        }

                        sourceData += textureLock.SlicePitch;
                    }
                }
                else
                {
                    // Since we have the same row and slice stride, copy everything in one shot.
                    DirectAccess.MemoryCopy(buffer, textureLock.Data.UnsafePointer, sliceStride);
                }
            }
            finally
            {
                stagingTexture.Unlock(resourceIndex);
            }
        }

        /// <summary>
        /// Function to create an image data object from a texture.
        /// </summary>
        /// <param name="texture">Texture used to create the image data.</param>
        /// <param name="mipLevel">Mip level to copy.</param>
        /// <returns>A new image data object containing the data from the texture.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the texture parameter has a usage of Immutable.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="mipLevel"/> parameter is greater than or equal to the number of mip map levels.</exception>
        /// <remarks>This will create a system memory clone of the <see cref="GorgonLibrary.Graphics.GorgonTexture">texture</see>.  Only textures that do not have the Immutable usage can be converted, if an attempt to 
        /// convert a texture with a usage of Immutable is made, then an exception will be thrown.</remarks>
        public static GorgonImageData CreateFromTexture(GorgonTexture texture, int mipLevel)
        {
            return CreateFromTexture(texture, 0, mipLevel);
        }

        /// <summary>
        /// Function to create an image data object from a texture.
        /// </summary>
        /// <param name="texture">Texture used to create the image data.</param>
        /// <param name="arrayIndex">Index of the texture array to copy.</param>
        /// <param name="mipLevel">Mip level to copy.</param>
        /// <returns>A new image data object containing the data from the texture.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the texture parameter has a usage of Immutable.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="arrayIndex"/> or <paramref name="mipLevel"/> parameters are greater than or equal to the number of array indices or mip map levels.</exception>
        /// <remarks>This will create a system memory clone of the <see cref="GorgonLibrary.Graphics.GorgonTexture">texture</see>.  Only textures that do not have the Immutable usage can be converted, if an attempt to 
        /// convert a texture with a usage of Immutable is made, then an exception will be thrown.</remarks>
        public static GorgonImageData CreateFromTexture(GorgonTexture texture, int arrayIndex, int mipLevel)
        {
            GorgonImageData result = null;
            GorgonTexture staging = texture;

            if (texture == null)
            {
                throw new ArgumentException("texture");
            }

            if (texture.Settings.Usage == BufferUsage.Immutable)
            {
                throw new ArgumentException("The texture is immutable and cannot be read.", "texture");
            }

            // If the texture is a volume texture, then set the array index to 0.
            if ((arrayIndex < 0) || (texture.Settings.ImageType == ImageType.Image3D))
            {
                arrayIndex = 0;
            }

            if (mipLevel < 0)
            {
                mipLevel = 0;
            }

            if (arrayIndex >= texture.Settings.ArrayCount)
            {
                throw new ArgumentOutOfRangeException("arrayIndex", "The array index is larger than or equal to " + texture.Settings.ArrayCount.ToString());
            }

            if (mipLevel >= texture.Settings.MipCount)
            {
                throw new ArgumentOutOfRangeException("mipLevel", "The mip map level is larger than or equal to " + texture.Settings.ArrayCount.ToString());
            }

            if (texture.Settings.Usage != BufferUsage.Staging)
            {
                staging = texture.GetStagingTexture<GorgonTexture>();
            }

            try
            {
                // Build our structure.
                result = new GorgonImageData(texture.Settings);

                // Get the buffer for the array and mip level.
                var buffer = result[arrayIndex, mipLevel];

                // Copy the data from the texture.
                GetTextureData(staging, buffer.Data.UnsafePointer, arrayIndex, mipLevel, buffer.PitchInformation.RowPitch, buffer.Height, buffer.Depth);

                return result;
            }
            finally
            {
                if ((staging != null) && (staging != texture))
                {
                    staging.Dispose();
                }
                staging = null;
            }            
        }

        /// <summary>
        /// Function to create an image data object from a texture.
        /// </summary>
        /// <param name="texture">Texture used to create the image data.</param>
        /// <returns>A new image data object containing the data from the texture.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the texture parameter has a usage of Immutable.</exception>
        /// <remarks>This will create a system memory clone of the <see cref="GorgonLibrary.Graphics.GorgonTexture">texture</see>.  Only textures that do not have the Immutable usage can be converted, if an attempt to 
        /// convert a texture with a usage of Immutable is made, then an exception will be thrown.</remarks>
        public static GorgonImageData CreateFromTexture(GorgonTexture texture)
        {
            GorgonImageData result = null;
            GorgonTexture staging = texture;

            if (texture == null)
            {
                throw new ArgumentException("texture");
            }

            if (texture.Settings.Usage == BufferUsage.Immutable)
            {
                throw new ArgumentException("The texture is immutable and cannot be read.", "texture");
            }

            if (texture.Settings.Usage != BufferUsage.Staging)
            {
                staging = texture.GetStagingTexture<GorgonTexture>();
            }

            try
            {
                // Build our structure.
                result = new GorgonImageData(texture.Settings);

                for (int array = 0; array < texture.Settings.ArrayCount; array++)
                {    
                    for (int mipLevel = 0; mipLevel < texture.Settings.MipCount; mipLevel++)
                    {
                        // Get the buffer for the array and mip level.
                        var buffer = result[array, mipLevel];                        

                        // Copy the data from the texture.
                        GetTextureData(staging, buffer.Data.UnsafePointer, array, mipLevel, buffer.PitchInformation.RowPitch, buffer.Height, buffer.Depth);
                    }
                }

                return result;
            }
            finally
            {
                if ((staging != null) && (staging != texture))
                {
                    staging.Dispose();
                }
                staging = null;
            }
        }

		/// <summary>
		/// Function to return the size of a 1D image in bytes.
		/// </summary>
		/// <param name="width">Width of the 1D image.</param>
		/// <param name="format">Format of the 1D image.</param>
		/// <returns>The number of bytes for the 1D image.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> parameter is set to Unknown.</exception>
		public static int GetSizeInBytes(int width, BufferFormat format)
		{
			return GetSizeInBytes(width, format, 1, 1);
		}

		/// <summary>
		/// Function to return the size of a 1D image in bytes.
		/// </summary>
		/// <param name="width">Width of the 1D image.</param>
		/// <param name="format">Format of the 1D image.</param>
		/// <param name="mipCount">Number of mip-map levels in the 1D image.</param>
		/// <returns>The number of bytes for the 1D image.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> parameter is set to Unknown.</exception>
		public static int GetSizeInBytes(int width, BufferFormat format, int mipCount)
		{
			return GetSizeInBytes(width, format, 1, mipCount);
		}

		/// <summary>
		/// Function to return the size of a 1D image in bytes.
		/// </summary>
		/// <param name="width">Width of the 1D image.</param>
		/// <param name="format">Format of the 1D image.</param>
		/// <param name="arrayCount">Number of array indices.</param>
		/// <param name="mipCount">Number of mip-map levels in the 1D image.</param>
		/// <returns>The number of bytes for the 1D image.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> parameter is set to Unknown.</exception>
		public static int GetSizeInBytes(int width, BufferFormat format, int arrayCount, int mipCount)
		{
			return GetSizeInBytes(width, 1, format, arrayCount, mipCount);
		}

		/// <summary>
		/// Function to return the size of a 2D image in bytes.
		/// </summary>
		/// <param name="width">Width of the 2D image.</param>
		/// <param name="height">Height of the 2D image.</param>
		/// <param name="format">Format of the 2D image.</param>
		/// <returns>The number of bytes for the 2D image.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> parameter is set to Unknown.</exception>
		public static int GetSizeInBytes(int width, int height, BufferFormat format)
		{
			return GetSizeInBytes(width, height, format, 1, 1);
		}

		/// <summary>
		/// Function to return the size of a 2D image in bytes.
		/// </summary>
		/// <param name="width">Width of the 2D image.</param>
		/// <param name="height">Height of the 2D image.</param>
		/// <param name="format">Format of the 2D image.</param>
		/// <param name="mipCount">Number of mip-map levels in the 2D image.</param>
		/// <returns>The number of bytes for the 2D image.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> parameter is set to Unknown.</exception>
		public static int GetSizeInBytes(int width, int height, BufferFormat format, int mipCount)
		{
			return GetSizeInBytes(width, height, format, 1, mipCount);
		}

		/// <summary>
		/// Function to return the size of a 2D image in bytes.
		/// </summary>
		/// <param name="width">Width of the 2D image.</param>
		/// <param name="height">Height of the 2D image.</param>
		/// <param name="format">Format of the 2D image.</param>
		/// <param name="arrayCount">Number of array indices.</param>
		/// <param name="mipCount">Number of mip-map levels in the 2D image.</param>
		/// <returns>The number of bytes for the 2D image.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> parameter is set to Unknown.</exception>
		public static int GetSizeInBytes(int width, int height, BufferFormat format, int arrayCount, int mipCount)
		{
			int result = 0;

			arrayCount = 1.Max(arrayCount);

			for (int i = 0; i < arrayCount; i++)
			{
				result += GetSizeInBytes(width, height, 1, format, mipCount);
			}

			return result;
		}

		/// <summary>
		/// Function to return the size of a 3D image in bytes.
		/// </summary>
		/// <param name="width">Width of the 3D image.</param>
		/// <param name="height">Height of the 3D image.</param>
		/// <param name="depth">Depth of the 3D image.</param>
		/// <param name="format">Format of the 3D image.</param>
		/// <returns>The number of bytes for the 3D image.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> parameter is set to Unknown.</exception>
		public static int GetSizeInBytes(int width, int height, int depth, BufferFormat format)
		{
			return GetSizeInBytes(width, height, depth, format, 1);
		}

		/// <summary>
		/// Function to return the size of a 3D image in bytes.
		/// </summary>
		/// <param name="width">Width of the 3D image.</param>
		/// <param name="height">Height of the 3D image.</param>
		/// <param name="depth">Depth of the 3D image.</param>
		/// <param name="format">Format of the 3D image.</param>
		/// <param name="mipCount">Number of mip-map levels in the 3D image.</param>
		/// <returns>The number of bytes for the 3D image.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the value of the <paramref name="format"/> parameter is not supported.</exception>
		public static int GetSizeInBytes(int width, int height, int depth, BufferFormat format, int mipCount)
		{
			if (format == BufferFormat.Unknown)
			{
				throw new NotSupportedException("The buffer type 'Unknown' is not a valid format.");
			}

			width = 1.Max(width);
			height = 1.Max(height);
			depth = 1.Max(depth);
			mipCount = 1.Max(mipCount);
			var formatInfo = GorgonBufferFormatInfo.GetInfo(format);
			int result = 0;

			if (formatInfo.SizeInBytes == 0)
			{
				throw new ArgumentException("'" + format.ToString() + "' is not a supported format.", "format");
			}

			int mipWidth = width;
			int mipHeight = height;

			for (int mip = 0; mip < mipCount; mip++)
			{
				var pitchInfo = formatInfo.GetPitch(mipWidth, mipHeight, PitchFlags.None);
				result += pitchInfo.SlicePitch * depth;

				if (mipWidth > 1)
				{
					mipWidth >>= 1;
				}
				if (mipHeight > 1)
				{
					mipHeight >>= 1;
				}
				if (depth > 1)
				{
					depth >>= 1;
				}
			}

			return result;
		}

		/// <summary>
		/// Function to return the size, in bytes, of an image with the given settings.
		/// </summary>
		/// <param name="settings">Settings to describe the image.</param>
		/// <returns>The number of bytes for the image.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the format value of the <paramref name="settings"/> parameter is not supported.</exception>
		public static int GetSizeInBytes(IImageSettings settings)
		{
			if (settings.ImageType == ImageType.Image3D)
			{
				return GetSizeInBytes(settings.Width, settings.Height, settings.Depth, settings.Format, settings.MipCount);
			}
			else
			{
				return GetSizeInBytes(settings.Width, settings.Height, settings.Format, settings.ArrayCount, settings.MipCount);
			}
		}

		/// <summary>
		/// Function to return the maximum number of mip levels supported given the specified settings.
		/// </summary>
		/// <param name="settings">Settings to evaluate.</param>
		/// <returns>The number of possible mip-map levels in the image.</returns>
		public static int GetMaxMipCount(IImageSettings settings)
		{
			if (settings == null)
			{
				return 0;
			}

			return GetMaxMipCount(settings.Width, settings.Height, settings.Depth);
		}

		/// <summary>
		/// Function to return the maximum number of mip levels supported in a 2D image.
		/// </summary>
		/// <param name="width">Width of the proposed image.</param>
		/// <returns>The number of possible mip-map levels in the image.</returns>
		public static int GetMaxMipCount(int width)
		{
			return GetMaxMipCount(width, 1, 1);
		}

		/// <summary>
		/// Function to return the maximum number of mip levels supported in a 2D image.
		/// </summary>
		/// <param name="width">Width of the proposed image.</param>
		/// <param name="height">Height of the proposed image.</param>
		/// <returns>The number of possible mip-map levels in the image.</returns>
		public static int GetMaxMipCount(int width, int height)
		{
			return GetMaxMipCount(width, height, 1);
		}

		/// <summary>
		/// Function to return the maximum number of mip levels supported in a 3D image.
		/// </summary>
		/// <param name="width">Width of the proposed image.</param>
		/// <param name="height">Height of the proposed image.</param>
		/// <param name="depth">Depth of the proposed image.</param>
		/// <returns>The number of possible mip-map levels in the image.</returns>
		public static int GetMaxMipCount(int width, int height, int depth)
		{
			int result = 1;
			width = 1.Max(width);
			height = 1.Max(height);
			depth = 1.Max(depth);			

			while ((width > 1) || (height > 1) || (depth > 1))
			{
				if (width > 1)
				{
					width >>= 1;
				}
				if (height > 1)
				{
					height >>= 1;
				}
				if (depth > 1)
				{
					depth >>= 1;
				}

				result++;
			}

			return result;			
		}

		/// <summary>
		/// Function to return the number of depth slices for an image with the given number of mip maps.
		/// </summary>
		/// <param name="slices">Slices requested.</param>
		/// <param name="mipCount">Mip map count.</param>
		/// <returns>The number of depth slices.</returns>
		public static int GetDepthSliceCount(int slices, int mipCount)
		{
			if (mipCount < 2)
			{
				return slices;
			}

			int bufferCount = 0;
			int depth = slices;

			for (int i = 0; i < mipCount; i++)
			{
				bufferCount += depth;

				if (depth > 1)
				{
					depth >>= 1;
				}
			}

			return bufferCount;
		}

		/// <summary>
		/// Function to return the number of depth slices for a given mip map slice.
		/// </summary>
		/// <param name="mipLevel">The mip map level to look up.</param>
		/// <returns>The number of depth slices for the given mip map level.</returns>
		/// <remarks>For 1D and 2D images, the mip level will always return 1.</remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="mipLevel"/> parameter exceeds the number of mip maps for the image or is less than 0.</exception>
		public int GetDepthCount(int mipLevel)
		{
			GorgonDebug.AssertParamRange(mipLevel, 0, Settings.MipCount, "mipLevel");

			if (Settings.Depth <= 1)
			{
				return 1;
			}

			return _mipOffsetSize[mipLevel].Item2; 
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageData" /> class.
        /// </summary>
        /// <param name="settings">The settings to describe an image.</param>
        /// <param name="data">Pointer to pre-existing image data.</param>
        /// <param name="dataSize">Size of the data, in bytes.  This parameter is ignored if the data parameter is NULL.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the image format is unknown or is unsupported.</exception>
		/// <remarks>This overload takes a <paramref name="settings"/> value that implements the <see cref="GorgonLibrary.Graphics.IImageSettings">IImageSettings</see> interface.  
		/// The GorgonTexture[n]DSettings (where n = <see cref="GorgonLibrary.Graphics.GorgonTexture1DSettings">1</see>, <see cref="GorgonLibrary.Graphics.GorgonTexture2DSettings">2</see>, 
		/// or <see cref="GorgonLibrary.Graphics.GorgonTexture3DSettings">3</see>) types all implement IImageSettings.
		/// <para>If the <paramref name="data"/> pointer is NULL, then a buffer will be created, otherwise the buffer that the pointer is pointing 
        /// at must be large enough to accomodate the size of the image described in the settings parameter and will be validated against the 
        /// <paramref name="dataSize"/> parameter.</para>
        /// <para>If the user passes NULL to the data parameter, then the dataSize parameter is ignored.</para>
        /// <para>If the buffer is passed in via the pointer, then the user is responsible for freeing that memory 
        /// once they are done with it.</para></remarks>
        public GorgonImageData(IImageSettings settings, void *data, int dataSize)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (settings.Format == BufferFormat.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, "The image format is not known.");
            }

            Settings = settings;            
            SanitizeSettings();
			SizeInBytes = GetSizeInBytes(settings);

            // Validate the image size.
            if ((data != null) && (SizeInBytes > dataSize))
            {
                throw new ArgumentOutOfRangeException("There is a mismatch between the image size, and the buffer size.", "dataSize");
            }

            Initialize(data);
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageData" /> class.
        /// </summary>
        /// <param name="settings">The settings to describe an image.</param>
        /// <param name="data">Pointer to pre-existing image data.</param>
        /// <param name="dataSize">Size of the data, in bytes.  This parameter is ignored if the data parameter is NULL.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the image format is unknown or is unsupported.</exception>
		/// <remarks>This overload takes a <paramref name="settings"/> value that implements the <see cref="GorgonLibrary.Graphics.IImageSettings">IImageSettings</see> interface.  
		/// The GorgonTexture[n]DSettings (where n = <see cref="GorgonLibrary.Graphics.GorgonTexture1DSettings">1</see>, <see cref="GorgonLibrary.Graphics.GorgonTexture2DSettings">2</see>, 
		/// or <see cref="GorgonLibrary.Graphics.GorgonTexture3DSettings">3</see>) types all implement IImageSettings.
		/// <para>If the <paramref name="data"/> pointer is NULL, then a buffer will be created, otherwise the buffer that the pointer is pointing 
        /// at must be large enough to accomodate the size of the image described in the settings parameter and will be validated against the 
		/// <paramref name="dataSize"/> parameter.</para>
        /// <para>If the user passes NULL to the data parameter, then the dataSize parameter is ignored.</para>
        /// <para>If the buffer is passed in via the pointer, then the user is responsible for freeing that memory 
        /// once they are done with it.</para></remarks>
		public GorgonImageData(IImageSettings settings, IntPtr data, int dataSize)
			: this(settings, data == IntPtr.Zero ? null : data.ToPointer(), dataSize)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonImageData" /> class.
		/// </summary>
		/// <param name="settings">The settings to describe an image.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the image format is unknown or is unsupported.</exception>
		/// <remarks>This overload takes a <paramref name="settings"/> value that implements the <see cref="GorgonLibrary.Graphics.IImageSettings">IImageSettings</see> interface.  
		/// The GorgonTexture[n]DSettings (where n = <see cref="GorgonLibrary.Graphics.GorgonTexture1DSettings">1</see>, <see cref="GorgonLibrary.Graphics.GorgonTexture2DSettings">2</see>, 
		/// or <see cref="GorgonLibrary.Graphics.GorgonTexture3DSettings">3</see>) types all implement IImageSettings.</remarks>
		public GorgonImageData(IImageSettings settings)
			: this(settings, null, 0)
		{
		}
		#endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_imageData != null)
                    {
                        _imageData.Dispose();
                    }

					if (_buffers != null)
					{
						foreach (var buffer in _buffers)
						{
							if (buffer.Data != null)
							{
								buffer.Data.Dispose();
							}
						}
					}
                }

				_buffers = null;
                _imageData = null;
                _disposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

		#region IEnumerable<GorgonImageData.ImageBuffer> Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.Generic.IEnumerator{T}" /> object that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<GorgonImageData.ImageBuffer> GetEnumerator()
		{
			foreach (var item in _buffers)
			{
				yield return item;
			}
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _buffers.GetEnumerator();
		}
		#endregion
	}
}
