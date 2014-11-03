#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Monday, November 03, 2014 9:34:24 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.IO;

namespace GorgonLibrary.Graphics.Example
{
    /// <summary>
    /// Our useless image codec.
    /// </summary>
    /// <remarks>
    /// This codec will encode and decode image data as 1 channel/pixel.
    /// <para>
    /// To create a codec, we must inherit the GorgonImageCodec object and implement functionality to load and save image data to and from a stream.
    /// </para>
    /// </remarks>
    class UselessImageCodec
        : GorgonImageCodec
    {
        #region Variables.
        // Formats supported by the image.
        // We need to tell Gorgon which pixel formats this image codec stores its data as.  Otherwise, the image will not look right when it's loaded.
        private BufferFormat[] _supportFormats =
        {
            BufferFormat.R8G8B8A8_UIntNormal
        };
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the friendly description of the format.
        /// </summary>
        public override string CodecDescription
        {
            get
            {
                return "Useless image";
            }
        }

        /// <summary>
        /// Property to return the abbreviated name of the codec (e.g. PNG).
        /// </summary>
        public override string Codec
        {
            get
            {
                return "Useless";
            }
        }

        /// <summary>
        /// Property to return the data formats supported by the codec.
        /// </summary>
        public override IEnumerable<BufferFormat> SupportedFormats
        {
            get
            {
                return _supportFormats;
            }
        }

        /// <summary>
        /// Property to return whether the image codec supports a depth component for volume textures.
        /// </summary>
        /// <remarks>This useless format doesn't support volume textures.</remarks>
        public override bool SupportsDepth
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Property to return whether the image codec supports image arrays.
        /// </summary>
        /// <remarks>This useless format doesn't support texture arrays.</remarks>
        public override bool SupportsArray
        {
            get
            {
                return false;
            }
        }

        public override bool SupportsMipMaps
        {
            get { throw new NotImplementedException(); }
        }

        public override bool SupportsCubeMaps
        {
            get { throw new NotImplementedException(); }
        }

        public override bool SupportsBlockCompression
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<ImageType> SupportsImageType
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        #region Methods.

        #endregion

        #region Constructor/Destructor.

        #endregion

        protected override GorgonImageData LoadFromStream(GorgonDataStream stream, int size)
        {
            throw new NotImplementedException();
        }

        protected override void SaveToStream(GorgonImageData imageData, System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }

        public override bool IsReadable(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }

        public override IImageSettings GetMetaData(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
