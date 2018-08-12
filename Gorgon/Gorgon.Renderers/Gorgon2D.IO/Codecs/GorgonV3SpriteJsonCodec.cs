#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: August 11, 2018 3:43:13 PM
// 
#endregion

using System;
using System.Buffers;
using System.IO;
using System.Text;
using Gorgon.IO.Properties;
using Gorgon.Renderers;

namespace Gorgon.IO.Codecs
{
    /// <summary>
    /// A codec that can read and write a JSON formatted version of Gorgon v3 sprite data.
    /// </summary>
    public class GorgonV3SpriteJsonCodec
        : GorgonCodecCommon
    {
        #region Properties.
        /// <summary>
        /// Property to return whether or not the codec can decode sprite data.
        /// </summary>
        public override bool CanDecode => true;

        /// <summary>
        /// Property to return whether or not the codec can encode sprite data.
        /// </summary>
        public override bool CanEncode => true;

        /// <summary>
        /// Property to return the version of sprite data that the codec supports.
        /// </summary>
        public override Version Version
        {
            get;
        } = new Version(3, 0);
        #endregion

        #region Methods.
        /// <summary>
        /// Function to read the sprite data from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the sprite.</param>
        /// <param name="byteCount">The number of bytes to read from the stream.</param>
        /// <returns>A new <see cref="GorgonSprite"/>.</returns>
        protected override GorgonSprite OnReadFromStream(Stream stream, int byteCount)
        {
            StringWriter writer = null;
            StreamReader reader = null;
            char[] buffer = ArrayPool<char>.Shared.Rent(1024);

            try
            {
                writer = new StringWriter();
                reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
                long pos = stream.Position;

                while (pos < byteCount)
                {
                    int charsRead = reader.Read(buffer, 0, buffer.Length);
                    writer.Write(buffer, 0, charsRead);
                    pos = stream.Position;
                }

                return GorgonSprite.FromJson(Renderer, writer.ToString());
            }
            finally
            {
                reader?.Dispose();
                writer?.Dispose();
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Function to save the sprite data to a stream.
        /// </summary>
        /// <param name="sprite">The sprite to serialize into the stream.</param>
        /// <param name="stream">The stream that will contain the sprite.</param>
        protected override void OnSaveToStream(GorgonSprite sprite, Stream stream)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                writer.Write(sprite.ToJson());
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonV3SpriteJsonCodec"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used for resource handling.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
        public GorgonV3SpriteJsonCodec(Gorgon2D renderer)
            : base(renderer, Resources.GOR2DIO_V3_JSON_CODEC, Resources.GOR2DIO_V3_JSON_CODEC_DESCRIPTION)
        {
            
        }
        #endregion
    }
}
