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
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.Native;
using GorgonLibrary.IO;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// A container for raw image data that can be sent to or read from a texture.
    /// </summary>
    /// <typeparam name="T">Type of texture settings to use.  Must implement the <see cref="GorgonLibrary.Graphics.ITextureSettings">ITextureSettings</see> interface.</typeparam>
    /// <remarks>This object will allow pixel manipulation of texture data.  It will break a texture into buffers, such as a series of buffers 
    /// for arrays, mip-map levels and depth slices.</remarks>
    public unsafe class GorgonImageData<T>
        : IDisposable
        where T : class, ITextureSettings
    {
        #region Variables.
        private bool _disposed = false;                             // Flag to indicate whether the object was disposed.
        private GorgonDataStream _imageData = null;                 // Image data.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the settings for the image.
        /// </summary>
        public T Settings
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
        #endregion

        #region Methods.
        /// <summary>
        /// Function to calculate the size of the image, in bytes.
        /// </summary>
        private void CalculateImageSize()
        {
            int width = Settings.Width;
            int height = Settings.Height;
            int depth = Settings.Depth;
            int arrayCount = Settings.ArrayCount;
            int mipCount = Settings.MipCount;
            int result = 0;
            var formatInfo = GorgonBufferFormatInfo.GetInfo(Settings.Format);

            if (formatInfo.SizeInBytes == 0)
            {
                throw new GorgonException(GorgonResult.InvalidFileFormat, "The format '" + Settings.Format.ToString() + "' is not valid.");
            }

            for (int array = 0; array < arrayCount; array++)
            {
                for (int mip = 0; mip < mipCount; mipCount++)
                {
                    // Get the row and slice pitch information.
                    var pitchInformation = formatInfo.GetPitch(width, height, PitchFlags.None);
                    result += pitchInformation.SlicePitch;

                    width >>= 1;
                    height >>= 1;
                    depth >>= 1;
                }
            }

            SizeInBytes = result;
        }

        /// <summary>
        /// Function to initialize the image data.
        /// </summary>
        /// <param name="data">Pre-existing data to use.</param>
        private void Initialize(void *data)
        {
            // First, calculate the size of the image.
            CalculateImageSize();

            if (_imageData != null)
            {
                _imageData.Dispose();
                _imageData = null;
            }

            // Create a buffer large enough to hold our data.
            if (data == null)
            {
                _imageData = new GorgonDataStream(SizeInBytes);
            }
            else
            {
                _imageData = new GorgonDataStream(data, SizeInBytes);
            }


        }

        /// <summary>
        /// Function to sanitize the texture settings.
        /// </summary>
        private void SanitizeSettings()
        {
            if (Settings.ArrayCount < 1)
            {
                Settings.ArrayCount = 1;
            }

            if (Settings.MipCount < 1)
            {
                Settings.MipCount = 1;
            }

            if (Settings.Width < 1)
            {
                Settings.Width = 1;
            }

            if (Settings.Height < 1)
            {
                Settings.Height = 1;
            }

            if (Settings.Depth < 1)
            {
                Settings.Depth = 1;
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageData{T}" /> class.
        /// </summary>
        /// <param name="settings">The settings to describe a texture.</param>
        /// <param name="data">Pointer to pre-existing image data.</param>
        /// <param name="dataSize">Size of the data, in bytes.  This parameter is ignored if the data parameter is NULL.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture format is unknown or is unsupported.</exception>
        /// <remarks>If the <paramref name="data"/> pointer is NULL, then a buffer will be created, otherwise the buffer that the pointer is pointing 
        /// at must be large enough to accomodate the size of the image described in the settings parameter and will be validated against the 
        /// <paramref name="dataSize"/> parameter.  
        /// <para>If the user passes NULL to the data parameter, then the dataSize parameter is ignored.</para>
        /// <para>If the buffer is passed in via the pointer, then the user is responsible for freeing that memory 
        /// once they are done with it.</para></remarks>
        public GorgonImageData(T settings, void *data, int dataSize)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (settings.Format == BufferFormat.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, "The texture format is not known.");
            }

            Settings = settings;            
            SanitizeSettings();
            CalculateImageSize();

            // Validate the image size.
            if ((data != null) && (SizeInBytes != dataSize))
            {
                throw new ArgumentOutOfRangeException("There is a mismatch between the image size, and the buffer size.", "dataSize");
            }

            Initialize(data);
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
                }

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
    }
}
