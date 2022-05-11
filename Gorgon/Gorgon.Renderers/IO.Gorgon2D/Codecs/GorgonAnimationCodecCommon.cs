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
// Created: August 25, 2018 2:43:42 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.IO.Properties;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.IO
{
    /// <summary>
    /// A base class containing common codec functionality.
    /// </summary>
    public abstract class GorgonAnimationCodecCommon
        : IGorgonAnimationCodec
    {
        #region Variables.
        /// <summary>
        /// The ID for the file header for the most current version of the animation format.
        /// </summary>
        public static readonly ulong CurrentFileHeader = "GORANM31".ChunkID();

        /// <summary>
        /// The highest currently supported version for animation serialization.
        /// </summary>
        public static readonly Version CurrentVersion = new(3, 1);
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the friendly description of the format.
        /// </summary>
        public string CodecDescription
        {
            get;
        }

        /// <summary>
        /// Property to return the abbreviated name of the codec (e.g. PNG).
        /// </summary>
        public string Codec
        {
            get;
        }

        /// <summary>
        /// Property to return the renderer used to create objects.
        /// </summary>
        public Gorgon2D Renderer
        {
            get;
        }

        /// <summary>
        /// Property to return whether or not the codec can decode animation data.
        /// </summary>
        public abstract bool CanDecode
        {
            get;
        }

        /// <summary>
        /// Property to return whether or not the codec can encode animation data.
        /// </summary>
        public abstract bool CanEncode
        {
            get;
        }

        /// <summary>
        /// Property to return the version of animation data that the codec supports.
        /// </summary>
        public abstract Version Version
        {
            get;
        }

        /// <summary>
        /// Property to return the graphics interface that built this object.
        /// </summary>
        public GorgonGraphics Graphics => Renderer.Graphics;

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        string IGorgonNamedObject.Name => Codec;

        /// <summary>
        /// Property to return the common file extensions for an animation.
        /// </summary>
        public IReadOnlyList<GorgonFileExtension> FileExtensions
        {
            get;
            protected set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to save the animation data to a stream.
        /// </summary>
        /// <param name="animation">The animation to serialize into the stream.</param>
        /// <param name="stream">The stream that will contain the animation.</param>
        protected abstract void OnSaveToStream(IGorgonAnimation animation, Stream stream);

        /// <summary>
        /// Function to read the animation data from a stream.
        /// </summary>
        /// <param name="name">The name of the animation.</param>
        /// <param name="stream">The stream containing the animation.</param>
        /// <param name="byteCount">The number of bytes to read from the stream.</param>
        /// <param name="textureOverrides">[Optional] Textures to use in a texture animation track.</param>
        /// <returns>A new <see cref="IGorgonAnimation"/>.</returns>
        /// <remarks>
        /// <para>
        /// Implementors should handle the <paramref name="textureOverrides"/> parameter by matching the textures by name, and, if the texture is not found in the override list, fall back to whatever scheme 
        /// is used to retrieve the texture for codec.
        /// </para>
        /// </remarks>
        protected abstract IGorgonAnimation OnReadFromStream(string name, Stream stream, int byteCount, IEnumerable<GorgonTexture2DView> textureOverrides = null);

        /// <summary>
        /// Function to determine if the data in a stream is readable by this codec.
        /// </summary>
        /// <param name="stream">The stream containing the data.</param>
        /// <returns><b>true</b> if the data can be read, or <b>false</b> if not.</returns>
        protected abstract bool OnIsReadable(Stream stream);


        /// <summary>
        /// Function to retrieve the names of the associated textures.
        /// </summary>
        /// <param name="stream">The stream containing the texture data.</param>
        /// <returns>The names of the texture associated with the animations, or an empty list if no textures were found.</returns>
        protected abstract IReadOnlyList<string> OnGetAssociatedTextureNames(Stream stream);

        /// <summary>
        /// Function to retrieve the names of the associated textures.
        /// </summary>
        /// <param name="stream">The stream containing the texture data.</param>
        /// <returns>The names of the texture associated with the animations, or an empty list if no textures were found.</returns>
        /// <exception cref="GorgonException">Thrown if the <paramref name="stream"/> is write only.</exception>
        /// <exception cref="EndOfStreamException">Thrown if the current <paramref name="stream"/> position, plus the size of the data exceeds the length of the stream.</exception>
        /// <exception cref="NotSupportedException">This method is not supported by this codec.</exception>
        public IReadOnlyList<string> GetAssociatedTextureNames(Stream stream)
        {
            if (!CanDecode)
            {
                throw new NotSupportedException();
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_STREAM_IS_WRITE_ONLY);
            }

            if (!stream.CanSeek)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_STREAM_UNSEEKABLE);
            }

            long pos = stream.Position;

            try
            {
                IReadOnlyList<string> result = OnGetAssociatedTextureNames(stream);

                return result ?? Array.Empty<string>();
            }
            finally
            {
                stream.Position = pos;
            }
        }

        /// <summary>
        /// Function to read the animation data from a stream.
        /// </summary>        
        /// <param name="stream">The stream containing the animation.</param>
        /// <param name="byteCount">[Optional] The number of bytes to read from the stream.</param>
        /// <param name="name">[Optional] The name of the animation.</param>
        /// <param name="textureOverrides">[Optional] Textures to use in a texture animation track.</param>
        /// <returns>A new <see cref="IGorgonAnimation"/>.</returns>
        /// <exception cref="GorgonException">Thrown if the <paramref name="stream"/> is write only.</exception>
        /// <exception cref="EndOfStreamException">Thrown if the current <paramref name="stream"/> position, plus the size of the data exceeds the length of the stream.</exception>
        /// <exception cref="NotSupportedException">This method is not supported by this codec.</exception>
        /// <remarks>
        /// <para>
        /// When passing in a list of <paramref name="textureOverrides"/>, the texture names should match the expected texture names in the key frame. For example, if the 
        /// <see cref="GorgonKeyTexture2D.TextureName"/> is <c>"WalkingFrames"</c>, then the <see cref="GorgonTexture2D.Name"/> should also be <c>"WalkingFrames"</c>. 
        /// </para>
        /// </remarks>
        public IGorgonAnimation FromStream(Stream stream, int? byteCount = null, string name = null, IEnumerable<GorgonTexture2DView> textureOverrides = null)
        {
            if (!CanDecode)
            {
                throw new NotSupportedException();
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_STREAM_IS_WRITE_ONLY);
            }

            if (byteCount is null)
            {
                byteCount = (int)stream.Length;
            }

            if ((stream.Position + byteCount.Value) > stream.Length)
            {
                throw new EndOfStreamException();
            }

            Stream externalStream = stream;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"GorgonAnimation_{Guid.NewGuid():N}";
            }

            try
            {
                if (!stream.CanSeek)
                {
                    externalStream = new DX.DataStream(byteCount ?? (int)stream.Length, true, true);
                    stream.CopyTo(externalStream, byteCount ?? (int)stream.Length);
                    externalStream.Position = 0;
                }

                return OnReadFromStream(name, externalStream, byteCount.Value, textureOverrides);
            }
            finally
            {
                if (externalStream != stream)
                {
                    externalStream?.Dispose();
                }
            }
        }

        /// <summary>
        /// Function to read the animation data from a file on the physical file system.
        /// </summary>
        /// <param name="filePath">The path to the file to read.</param>
        /// <param name="name">[Optional] The name of the animation.</param>
        /// <param name="textureOverrides">[Optional] Textures to use in a texture animation track.</param>
        /// <returns>A new <see cref="IGorgonAnimation"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
        /// <exception cref="NotSupportedException">This method is not supported by this codec.</exception>
        /// <remarks>
        /// <para>
        /// When passing in a list of <paramref name="textureOverrides"/>, the texture names should match the expected texture names in the key frame. For example, if the 
        /// <see cref="GorgonKeyTexture2D.TextureName"/> is <c>"WalkingFrames"</c>, then the <see cref="GorgonTexture2D.Name"/> should also be <c>"WalkingFrames"</c>. 
        /// </para>
        /// </remarks>
        public IGorgonAnimation FromFile(string filePath, string name = null, IEnumerable<GorgonTexture2DView> textureOverrides = null)
        {
            if (!CanDecode)
            {
                throw new NotSupportedException();
            }

            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentEmptyException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = filePath.FormatPath(Path.DirectorySeparatorChar);
            }

            using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return FromStream(stream, (int)stream.Length, name, textureOverrides);
        }

        /// <summary>
        /// Function to save the animation data to a stream.
        /// </summary>
        /// <param name="animation">The animation to serialize into the stream.</param>
        /// <param name="stream">The stream that will contain the animation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="animation"/>, or the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the stream is read only.</exception>
        /// <exception cref="NotSupportedException">This method is not supported by this codec.</exception>
        public void Save(IGorgonAnimation animation, Stream stream)
        {
            if (!CanEncode)
            {
                throw new NotSupportedException();
            }

            if (animation is null)
            {
                throw new ArgumentNullException(nameof(animation));
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new GorgonException(GorgonResult.CannotWrite, Resources.GOR2DIO_ERR_STREAM_IS_READ_ONLY);
            }

            OnSaveToStream(animation, stream);
        }

        /// <summary>
        /// Function to save the animation data to a file on a physical file system.
        /// </summary>
        /// <param name="animation">The animation to serialize into the file.</param>
        /// <param name="filePath">The path to the file to write.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath" /> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath" /> parameter is empty.</exception>
        /// <exception cref="NotSupportedException">This method is not supported by this codec.</exception>
        public void Save(IGorgonAnimation animation, string filePath)
        {
            if (!CanEncode)
            {
                throw new NotSupportedException();
            }

            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentEmptyException(nameof(filePath));
            }

            using FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            Save(animation, stream);
        }

        /// <summary>
        /// Function to determine if the data in a stream is readable by this codec.
        /// </summary>
        /// <param name="stream">The stream containing the data.</param>
        /// <returns><b>true</b> if the data can be read, or <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="stream"/> is write only.</exception>
        /// <exception cref="EndOfStreamException">Thrown if the current <paramref name="stream"/> position, plus the size of the data exceeds the length of the stream.</exception>
        /// <exception cref="NotSupportedException">This method is not supported by this codec.</exception>
        public bool IsReadable(Stream stream)
        {
            if (!CanDecode)
            {
                throw new NotSupportedException();
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_STREAM_IS_WRITE_ONLY);
            }

            if (!stream.CanSeek)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_STREAM_UNSEEKABLE);
            }

            long position = stream.Position;

            try
            {
                return OnIsReadable(stream);
            }
            finally
            {
                stream.Position = position;
            }
        }        
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonAnimationCodecCommon"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used for resource handling.</param>
        /// <param name="name">The codec name.</param>
        /// <param name="description">The friendly description for the codec.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/>, or the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        protected GorgonAnimationCodecCommon(Gorgon2D renderer, string name, string description)
        {
            Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentEmptyException(nameof(name));
            }

            FileExtensions = new[]
                             {
                                 new GorgonFileExtension(".gorAnim", Resources.GOR2DIO_ANIMTION_FILE_EXTENSION_DESC)
                             };
            Codec = name;
            CodecDescription = string.IsNullOrWhiteSpace(description) ? name : description;
        }
        #endregion
    }
}
