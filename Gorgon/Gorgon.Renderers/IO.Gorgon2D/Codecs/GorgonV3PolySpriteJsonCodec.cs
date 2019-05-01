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
using System.IO;
using System.Text;
using Gorgon.Core;
using Gorgon.IO.Properties;
using Gorgon.Renderers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Gorgon.IO
{
    /// <summary>
    /// A codec that can read and write a JSON formatted version of Gorgon v3 polygonal sprite data.
    /// </summary>
    public class GorgonV3PolySpriteJsonCodec
        : GorgonPolySpriteCodecCommon
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
        public override Version Version => CurrentVersion;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the stream as JSON.Net object.
        /// </summary>
        /// <param name="stream">The stream containing the data.</param>
        /// <returns>The data as a JSON.Net object.</returns>
        private static JsonReader GetJsonReader(Stream stream)
        {
            var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
            var jsonReader = new JsonTextReader(reader)
                             {
                                 CloseInput = true
                             };
            return jsonReader;
        }

        /// <summary>
        /// Function to determine if the jason object has the data we need.
        /// </summary>
        /// <param name="reader">The reader for the JSON data.</param>
        /// <returns><b>true</b> if the data is for a sprite, <b>false</b> if not.</returns>
        private bool IsReadableJObject(JsonReader reader)
        {
            // Find the header node.
            while (reader.Read())
            {
                if ((string.Equals(reader.Path, "header", StringComparison.Ordinal))
                    && (reader.TokenType == JsonToken.PropertyName))
                {
                    var id = (ulong?)reader.ReadAsDecimal();

                    if ((id == null) || (id != CurrentFileHeader))
                    {
                        return false;
                    }

                    if (!reader.Read())
                    {
                        return false;
                    }
                }

                // These must come right after each other.
                if ((!string.Equals(reader.Path, "version", StringComparison.Ordinal))
                    || (reader.TokenType != JsonToken.PropertyName))
                {
                    continue;
                }

                if (!reader.Read())
                {
                    return false;
                }

                return (Version.TryParse(reader.Value.ToString(), out Version version)) 
                       && (version.Equals(Version));
            }

            return false;
        }

        /// <summary>
        /// Function to read the sprite data from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the sprite.</param>
        /// <param name="byteCount">The number of bytes to read from the stream.</param>
        /// <returns>A new <see cref="GorgonPolySprite"/>.</returns>
        protected override GorgonPolySprite OnReadFromStream(Stream stream, int byteCount)
        {
            using (var wrappedStream = new GorgonStreamWrapper(stream, stream.Position, byteCount, false))
            {
                using (var reader = new StreamReader(wrappedStream, Encoding.UTF8, true, 80192, true))
                {
                    string jsonString = reader.ReadToEnd();
                    return FromJson(Renderer, jsonString);
                }
            }
        }

        /// <summary>
        /// Function to save the sprite data to a stream.
        /// </summary>
        /// <param name="sprite">The sprite to serialize into the stream.</param>
        /// <param name="stream">The stream that will contain the sprite.</param>
        protected override void OnSaveToStream(GorgonPolySprite sprite, Stream stream)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                writer.Write(sprite.ToJson());
            }
        }

        /// <summary>
        /// Function to determine if the data in a stream is readable by this codec.
        /// </summary>
        /// <param name="stream">The stream containing the data.</param>
        /// <returns><b>true</b> if the data can be read, or <b>false</b> if not.</returns>
        protected override bool OnIsReadable(Stream stream)
        {
            JsonReader reader = null;

            try
            {
                reader = GetJsonReader(stream);
                return IsReadableJObject(reader);
            }
            catch (JsonException)
            {
                return false;
            }
            finally
            {
                reader?.Close();
            }
        }

        /// <summary>
        /// Function to retrieve the name of the associated texture.
        /// </summary>
        /// <param name="stream">The stream containing the texture data.</param>
        /// <returns>The name of the texture associated with the sprite, or <b>null</b> if no texture was found.</returns>
        protected override string OnGetAssociatedTextureName(Stream stream)
        {
            using (JsonReader reader = GetJsonReader(stream))
            {
                if (!IsReadableJObject(reader))
                {
                    return null;
                }

                while (reader.Read())
                {
                    if ((!string.Equals(reader.Path, "Texture", StringComparison.Ordinal))
                        || (reader.TokenType != JsonToken.PropertyName))
                    {
                        continue;
                    }

                    while (reader.Read())
                    {
                        if ((!string.Equals(reader.Path, "Texture.name", StringComparison.Ordinal))
                            || (reader.TokenType != JsonToken.PropertyName))
                        {
                            continue;
                        }

                        return !reader.Read() ? null : reader.Value?.ToString();
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Function to convert a JSON string into a sprite object.
        /// </summary>
        /// <param name="renderer">The renderer for the sprite.</param>
        /// <param name="json">The JSON string containing the sprite data.</param>
        /// <returns>A new <see cref="GorgonPolySprite"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/>, or the <paramref name="json"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="json"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown if the JSON string does not contain sprite data, or there is a version mismatch.</exception>
        public static GorgonPolySprite FromJson(Gorgon2D renderer, string json)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentEmptyException(nameof(json));
            }

            // Set up serialization so we can convert our more complicated structures.
            var serializer = new JsonSerializer
                             {
                                 CheckAdditionalContent = false
                             };

            serializer.Converters.Add(new JsonVector2Converter());
            serializer.Converters.Add(new JsonVector3Converter());
            serializer.Converters.Add(new JsonSize2FConverter());
            serializer.Converters.Add(new JsonGorgonColorConverter());
            serializer.Converters.Add(new JsonRectangleFConverter());
            serializer.Converters.Add(new JsonSamplerConverter(renderer.Graphics));
            serializer.Converters.Add(new JsonTexture2DConverter(renderer.Graphics));
            serializer.Converters.Add(new VersionConverter());

            // Parse the string so we can extract our header/version for comparison.
            var jobj = JObject.Parse(json);
            ulong jsonID = jobj[GorgonSpriteExtensions.JsonHeaderProp].Value<ulong>();
            Version jsonVersion = jobj[GorgonSpriteExtensions.JsonVersionProp].ToObject<Version>(serializer);

            if (jsonID != CurrentFileHeader)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_JSON_NOT_SPRITE);
            }

            if (!jsonVersion.Equals(CurrentVersion))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR2DIO_ERR_SPRITE_VERSION_MISMATCH, CurrentVersion, jsonVersion));
            }

            GorgonPolySprite workingSpriteData = jobj.ToObject<GorgonPolySprite>(serializer);

            // We have to rebuild the sprite because it's only data at this point and we need to build up its vertex/index buffers before we can render it.
            var builder = new GorgonPolySpriteBuilder(renderer);
            builder.ResetTo(workingSpriteData);

            return builder.Build();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonV3PolySpriteJsonCodec"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used for resource handling.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
        public GorgonV3PolySpriteJsonCodec(Gorgon2D renderer)
            : base(renderer, Resources.GOR2DIO_V3_JSON_CODEC, Resources.GOR2DIO_V3_JSON_CODEC_DESCRIPTION)
        {
        }
        #endregion
    }
}
