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
// Created: August 25, 2018 2:43:32 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.IO.Properties;
using Gorgon.Renderers;
using Newtonsoft.Json;

namespace Gorgon.IO
{
    /// <summary>
    /// A codec used to read/write animations as a JSON formatted string value.
    /// </summary>
    public class GorgonV31AnimationJsonCodec
        : GorgonAnimationCodecCommon
    {
        #region Constants.

        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether or not the codec can decode animation data.
        /// </summary>
        public override bool CanDecode => true;

        /// <summary>
        /// Property to return whether or not the codec can encode animation data.
        /// </summary>
        public override bool CanEncode => true;

        /// <summary>
        /// Property to return the version of animation data that the codec supports.
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
                    ulong? id = (ulong?)reader.ReadAsDecimal();

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

                return !reader.Read()
                    ? false
                    : (Version.TryParse(reader.Value.ToString(), out Version version))
                       && (version.Equals(Version));
            }

            return false;
        }

        /// <summary>
        /// Function to read a track containing rectangle data.
        /// </summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="converter">The converter used to convert the data from JSON.</param>
        /// <param name="interpolation">The interpolation mode for the track.</param>
        /// <returns>A list of rectangle key frames.</returns>
        private static List<GorgonKeyRectangle> ReadRects(JsonReader reader, JsonRectKeyConverter converter, out TrackInterpolationMode interpolation)
        {
            List<GorgonKeyRectangle> ReadKeys()
            {
                var keys = new List<GorgonKeyRectangle>();
                Type type = typeof(GorgonKeyRectangle);

                while ((reader.Read()) && (reader.TokenType != JsonToken.EndArray))
                {
                    if (reader.TokenType != JsonToken.StartObject)
                    {
                        continue;
                    }

                    keys.Add(converter.ReadJson(reader, type, null, false, null));
                }

                return keys;
            }

            List<GorgonKeyRectangle> bounds = null;
            interpolation = TrackInterpolationMode.None;

            while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                string propName = reader.Value.ToString().ToUpperInvariant();

                switch (propName)
                {
                    case "INTERPOLATIONMODE":
                        interpolation = (TrackInterpolationMode)(reader.ReadAsInt32() ?? 0);
                        break;
                    case "KEYFRAMES":
                        bounds = ReadKeys();
                        break;
                }
            }

            return bounds;
        }

        /// <summary>
        /// Function to read a track containing <see cref="GorgonColor"/> data.
        /// </summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="converter">The converter used to convert the data from JSON.</param>
        /// <param name="interpolation">The interpolation mode for the track.</param>
        /// <returns>A list of <see cref="GorgonColor"/> key frames.</returns>
        private static List<GorgonKeyGorgonColor> ReadColors(JsonReader reader, JsonGorgonColorKeyConverter converter, out TrackInterpolationMode interpolation)
        {
            List<GorgonKeyGorgonColor> ReadKeys()
            {
                var keys = new List<GorgonKeyGorgonColor>();
                Type type = typeof(GorgonKeyGorgonColor);

                while ((reader.Read()) && (reader.TokenType != JsonToken.EndArray))
                {
                    if (reader.TokenType != JsonToken.StartObject)
                    {
                        continue;
                    }

                    keys.Add(converter.ReadJson(reader, type, null, false, null));
                }

                return keys;
            }

            List<GorgonKeyGorgonColor> colors = null;
            interpolation = TrackInterpolationMode.None;

            while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                string propName = reader.Value.ToString().ToUpperInvariant();

                switch (propName)
                {
                    case "INTERPOLATIONMODE":
                        interpolation = (TrackInterpolationMode)(reader.ReadAsInt32() ?? 0);
                        break;
                    case "KEYFRAMES":
                        colors = ReadKeys();
                        break;
                }
            }

            return colors;
        }

        /// <summary>
        /// Function to read a track containing texture data.
        /// </summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="converter">The converter used to convert the data from JSON.</param>
        /// <returns>A list of texture key frames.</returns>
        private static List<GorgonKeyTexture2D> ReadTextures(JsonReader reader, JsonTextureKeyConverter converter)
        {
            List<GorgonKeyTexture2D> ReadKeys()
            {
                var keys = new List<GorgonKeyTexture2D>();
                Type type = typeof(GorgonKeyTexture2D);

                while ((reader.Read()) && (reader.TokenType != JsonToken.EndArray))
                {
                    if (reader.TokenType != JsonToken.StartObject)
                    {
                        continue;
                    }

                    GorgonKeyTexture2D key = converter.ReadJson(reader, type, null, false, null);

                    if (key != null)
                    {
                        keys.Add(key);
                    }
                }

                return keys;
            }

            List<GorgonKeyTexture2D> textures = null;

            while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                string propName = reader.Value.ToString();

                if (string.Equals(propName, "KEYFRAMES", StringComparison.InvariantCultureIgnoreCase))
                {
                    textures = ReadKeys();
                }
            }

            return textures;
        }

        /// <summary>
        /// Function to read key data for a track.
        /// </summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="converter">The converter used to convert the data from JSON.</param>
        /// <param name="interpolation">The interpolation mode for the track.</param>
        /// <returns>A list of 3D vector key frames.</returns>
        private static List<Tk> ReadKeyData<Tk, Tc>(JsonReader reader, Tc converter, out TrackInterpolationMode interpolation)
               where Tk : class, IGorgonKeyFrame
               where Tc : JsonConverter<Tk>
        {
            List<Tk> ReadKeys()
            {
                var keys = new List<Tk>();
                Type type = typeof(Tk);

                while ((reader.Read()) && (reader.TokenType != JsonToken.EndArray))
                {
                    if (reader.TokenType != JsonToken.StartObject)
                    {
                        continue;
                    }

                    keys.Add(converter.ReadJson(reader, type, null, false, null));
                }

                return keys;
            }

            List<Tk> positions = null;
            interpolation = TrackInterpolationMode.None;

            while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                string propName = reader.Value.ToString().ToUpperInvariant();

                switch (propName)
                {
                    case "INTERPOLATIONMODE":
                        interpolation = (TrackInterpolationMode)(reader.ReadAsInt32() ?? 0);
                        break;
                    case "KEYFRAMES":
                        positions = ReadKeys();
                        break;
                }
            }

            return positions;
        }

        /// <summary>
        /// Function to read in a track containing single precision floating point key values.
        /// </summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="converter">The converter for key frame data.</param>
        /// <param name="track">The track list to update.</param>
        private void ReadTrack<Tk, Tc>(JsonTextReader reader, Tc converter, Dictionary<string, (List<Tk> keys, TrackInterpolationMode interpolation)> track)
            where Tk : class, IGorgonKeyFrame
            where Tc : JsonConverter<Tk>
        {
            if ((!reader.Read()) || (reader.TokenType != JsonToken.StartObject))
            {                
                return;
            }

            while ((reader.Read()) && (reader.TokenType != JsonToken.EndObject))
            {
                string trackName = reader.Value.ToString();
                List<Tk> keys = ReadKeyData<Tk, Tc>(reader, converter, out TrackInterpolationMode interpolation);
                track[trackName] = (keys, interpolation);
            }
        }

        /// <summary>
        /// Function to convert a JSON formatted string into a <see cref="IGorgonAnimation"/> object.
        /// </summary>
        /// <param name="renderer">The renderer for the animation.</param>
        /// <param name="json">The JSON string containing the animation data.</param>
        /// <returns>A new <see cref="IGorgonAnimation"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/>, or the <paramref name="json"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="json"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown if the JSON string does not contain animation data, or there is a version mismatch.</exception>
        public IGorgonAnimation FromJson(Gorgon2D renderer, string json)
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

            string animName = string.Empty;
            float animLength = 0;
            int loopCount = 0;
            bool isLooped = false;

            var colorConvert = new JsonGorgonColorKeyConverter();
            var textureConvert = new JsonTextureKeyConverter(renderer.Graphics);
            var singleConverter = new JsonSingleKeyConverter();
            var vec2Converter = new JsonVector2KeyConverter();
            var vec3Converter = new JsonVector3KeyConverter();
            var vec4Converter = new JsonVector4KeyConverter();
            var rectConverter = new JsonRectKeyConverter();

            var vec4 = new Dictionary<string, (List<GorgonKeyVector4>, TrackInterpolationMode)>(StringComparer.OrdinalIgnoreCase);
            var vec3 = new Dictionary<string, (List<GorgonKeyVector3>, TrackInterpolationMode)>(StringComparer.OrdinalIgnoreCase);
            var vec2 = new Dictionary<string, (List<GorgonKeyVector2>, TrackInterpolationMode)>(StringComparer.OrdinalIgnoreCase);
            var singles = new Dictionary<string, (List<GorgonKeySingle>, TrackInterpolationMode)>(StringComparer.OrdinalIgnoreCase);
            var colors = new Dictionary<string, (List<GorgonKeyGorgonColor>, TrackInterpolationMode)>(StringComparer.OrdinalIgnoreCase);
            var rects = new Dictionary<string, (List<GorgonKeyRectangle>, TrackInterpolationMode)>(StringComparer.OrdinalIgnoreCase);
            var textures = new Dictionary<string, (List<GorgonKeyTexture2D>, TrackInterpolationMode)>(StringComparer.OrdinalIgnoreCase);

            using (var baseReader = new StringReader(json))
            using (var reader = new JsonTextReader(baseReader))
            {
                if (!IsReadableJObject(reader))
                {
                    throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_JSON_NOT_ANIM);
                }

                while (reader.Read())
                {
                    if (reader.TokenType != JsonToken.PropertyName)
                    {
                        continue;
                    }

                    string propName = reader.Value.ToString().ToUpperInvariant();

                    switch (propName)
                    {
                        case "SINGLETRACKS":
                            ReadTrack(reader, singleConverter, singles);
                            break;
                        case "VECTOR2TRACKS":
                            ReadTrack(reader, vec2Converter, vec2);
                            break;
                        case "VECTOR3TRACKS":
                            ReadTrack(reader, vec3Converter, vec3);
                            break;
                        case "VECTOR4TRACKS":
                            ReadTrack(reader, vec4Converter, vec4);
                            break;
                        case "RECTTRACKS":
                            ReadTrack(reader, rectConverter, rects);
                            break;
                        case "COLORTRACKS":
                            ReadTrack(reader, colorConvert, colors);
                            break;
                        case "TEXTURES":
                            ReadTrack(reader, textureConvert, textures);
                            break;
                        case "NAME":
                            animName = reader.ReadAsString();
                            break;
                        case "LENGTH":
                            animLength = (float)(reader.ReadAsDecimal() ?? 0);
                            break;
                        case "ISLOOPED":
                            isLooped = (reader.ReadAsBoolean() ?? false);
                            break;
                        case "LOOPCOUNT":
                            loopCount = (reader.ReadAsInt32() ?? 0);
                            break;
                    }
                }
            }

            // There's no name, so it's a broken JSON.
            if (string.IsNullOrWhiteSpace(animName))
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_JSON_NOT_ANIM);
            }
            
            var builder = new GorgonAnimationBuilder();

            if (singles.Count > 0)
            {
                foreach (KeyValuePair<string, (List<GorgonKeySingle> keys, TrackInterpolationMode interpolation)> track in singles)
                {
                    builder.EditSingle(track.Key)
                           .SetInterpolationMode(track.Value.interpolation)
                           .SetKeys(track.Value.keys)
                           .EndEdit();
                }
            }

            if (vec2.Count > 0)
            {
                foreach (KeyValuePair<string, (List<GorgonKeyVector2> keys, TrackInterpolationMode interpolation)> track in vec2)
                {
                    builder.EditVector2(track.Key)
                           .SetInterpolationMode(track.Value.interpolation)
                           .SetKeys(track.Value.keys)
                           .EndEdit();
                }
            }

            if (vec3.Count > 0)
            {
                foreach (KeyValuePair<string, (List<GorgonKeyVector3> keys, TrackInterpolationMode interpolation)> track in vec3)
                {
                    builder.EditVector3(track.Key)
                           .SetInterpolationMode(track.Value.interpolation)
                           .SetKeys(track.Value.keys)
                           .EndEdit();
                }
            }

            if (vec4.Count > 0)
            {
                foreach (KeyValuePair<string, (List<GorgonKeyVector4> keys, TrackInterpolationMode interpolation)> track in vec4)
                {
                    builder.EditVector4(track.Key)
                           .SetInterpolationMode(track.Value.interpolation)
                           .SetKeys(track.Value.keys)
                           .EndEdit();
                }
            }

            if (rects.Count > 0)
            {
                foreach (KeyValuePair<string, (List<GorgonKeyRectangle> keys, TrackInterpolationMode interpolation)> track in rects)
                {
                    builder.EditRectangle(track.Key)
                           .SetInterpolationMode(track.Value.interpolation)
                           .SetKeys(track.Value.keys)
                           .EndEdit();
                }
            }

            if (colors.Count > 0)
            {
                foreach (KeyValuePair<string, (List<GorgonKeyGorgonColor> keys, TrackInterpolationMode interpolation)> track in colors)
                {
                    builder.EditColor(track.Key)
                           .SetInterpolationMode(track.Value.interpolation)
                           .SetKeys(track.Value.keys)
                           .EndEdit();
                }
            }

            if (textures.Count > 0)
            {
                foreach (KeyValuePair<string, (List<GorgonKeyTexture2D> keys, TrackInterpolationMode interpolation)> track in textures)
                {
                    builder.Edit2DTexture(track.Key)
                           .SetInterpolationMode(track.Value.interpolation)
                           .SetKeys(track.Value.keys)
                           .EndEdit();
                }
            }

            IGorgonAnimation result = builder.Build(animName, animLength);

            result.LoopCount = loopCount;
            result.IsLooped = isLooped;
            result.Speed = 1.0f;

            return result;
        }

        /// <summary>
        /// Function to save the animation data to a stream.
        /// </summary>
        /// <param name="animation">The animation to serialize into the stream.</param>
        /// <param name="stream">The stream that will contain the animation.</param>
        protected override void OnSaveToStream(IGorgonAnimation animation, Stream stream)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                writer.Write(animation.ToJson());
            }
        }

        /// <summary>
        /// Function to read the animation data from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the animation.</param>
        /// <param name="byteCount">The number of bytes to read from the stream.</param>
        /// <returns>A new <see cref="IGorgonAnimation"/>.</returns>
        protected override IGorgonAnimation OnReadFromStream(Stream stream, int byteCount)
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
        /// Function to determine if the data in a stream is readable by this codec.
        /// </summary>
        /// <param name="stream">The stream containing the data.</param>
        /// <returns><b>true</b> if the data can be read, or <b>false</b> if not.</returns>
        protected override bool OnIsReadable(Stream stream)
        {
            JsonReader reader = null;

            long currentPosition = stream.Position;
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
                stream.Position = currentPosition;
                reader?.Close();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonV3AnimationJsonCodec"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used for resource handling.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> is <b>null</b>.</exception>
        public GorgonV31AnimationJsonCodec(Gorgon2D renderer)
            : base(renderer, Resources.GOR2DIO_V3_1_ANIM_JSON_CODEC, Resources.GOR2DIO_V3_1_ANIM_JSON_CODEC_DESCRIPTION)
        {
        }
        #endregion
    }
}
