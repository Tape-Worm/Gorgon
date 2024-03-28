
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 25, 2018 2:43:32 PM
// 

using System.Numerics;
using System.Text;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.IO.Properties;
using Gorgon.Renderers;

namespace Gorgon.IO;

/// <summary>
/// A codec used to import sprite animations from version 1 of Gorgon
/// </summary>
public class GorgonV1AnimationCodec
    : GorgonAnimationCodecCommon
{
    /// <summary>
    /// Property to return whether or not the codec can decode animation data.
    /// </summary>
    public override bool CanDecode => true;

    /// <summary>
    /// Property to return whether or not the codec can encode animation data.
    /// </summary>
    public override bool CanEncode => false;

    /// <summary>
    /// Property to return the version of animation data that the codec supports.
    /// </summary>
    public override Version Version
    {
        get;
    } = new Version(1, 2);

    /// <summary>
    /// Function to save the animation data to a stream.
    /// </summary>
    /// <param name="animation">The animation to serialize into the stream.</param>
    /// <param name="stream">The stream that will contain the animation.</param>
    protected override void OnSaveToStream(IGorgonAnimation animation, Stream stream) => throw new NotSupportedException();

    /// <summary>
    /// Function to skip the unnecessary parts of the file and move to the animation section.
    /// </summary>
    /// <param name="reader">The binary reader for the stream.</param>
    /// <param name="version">The version of the sprite.</param>
    private static void SkipToAnimationSection(BinaryReader reader, Version version)
    {
        // We don't need the sprite name.
        reader.ReadString();

        // Find out if we have an image.
        if (reader.ReadBoolean())
        {
            bool isRenderTarget = reader.ReadBoolean();
            reader.ReadString();

            // We won't be supporting reading render targets from sprites in this version.
            if (isRenderTarget)
            {
                // Skip the target data.
                for (int i = 0; i < 3; ++i)
                {
                    reader.ReadInt32();
                }

                reader.ReadBoolean();
                reader.ReadBoolean();
            }
        }

        // We don't use "inherited" values anymore.  But we need them because
        // the file doesn't include inherited data.
        bool InheritAlphaMaskFunction = reader.ReadBoolean();
        bool InheritAlphaMaskValue = reader.ReadBoolean();
        bool InheritBlending = reader.ReadBoolean();
        bool InheritHorizontalWrapping = reader.ReadBoolean();
        bool InheritSmoothing = reader.ReadBoolean();
        bool InheritStencilCompare = reader.ReadBoolean();
        bool InheritStencilEnabled = reader.ReadBoolean();
        bool InheritStencilFailOperation = reader.ReadBoolean();
        bool InheritStencilMask = reader.ReadBoolean();
        bool InheritStencilPassOperation = reader.ReadBoolean();
        bool InheritStencilReference = reader.ReadBoolean();
        bool InheritStencilZFailOperation = reader.ReadBoolean();
        bool InheritVerticalWrapping = reader.ReadBoolean();
        bool InheritDepthBias = true;
        bool InheritDepthTestFunction = true;
        bool InheritDepthWriteEnabled = true;

        // Get version 1.1 fields.
        if ((version.Major == 1) && (version.Minor >= 1))
        {
            InheritDepthBias = reader.ReadBoolean();
            InheritDepthTestFunction = reader.ReadBoolean();
            InheritDepthWriteEnabled = reader.ReadBoolean();
        }

        // Get the size of the sprite.
        for (int i = 0; i < 14; ++i)
        {
            reader.ReadSingle(); // 0
        }

        // Get vertex colors.
        for (int i = 0; i < 4; ++i)
        {
            reader.ReadInt32();
        }

        // Skip shader information.  Version 1.0 had shader information attached to the sprite.
        if ((version.Major == 1) && (version.Minor < 1))
        {
            if (reader.ReadBoolean())
            {
                reader.ReadString();
                reader.ReadBoolean();
                if (reader.ReadBoolean())
                {
                    reader.ReadString();
                }
            }
        }

        // We no longer have an alpha mask function.
        if (!InheritAlphaMaskFunction)
        {
            reader.ReadInt32();
        }

        if (!InheritAlphaMaskValue)
        {
            // Direct 3D 9 used a value from 0..255 for alpha masking, we use
            // a scalar value so convert to a scalar.
            reader.ReadInt32();
        }

        // Set the blending mode.
        if (!InheritBlending)
        {
            // Skip the blending mode.  We don't use it per-sprite.
            for (int i = 0; i < 3; ++i)
            {
                reader.ReadInt32();
            }
        }

        // Get alpha blending mode.
        if ((version.Major == 1) && (version.Minor >= 2))
        {
            // Skip the blending mode information.  We don't use it per-sprite.
            reader.ReadInt32();
            reader.ReadInt32();
        }

        // Get horizontal wrapping mode.
        if (!InheritHorizontalWrapping)
        {
            reader.ReadInt32();
        }

        // Get smoothing mode.
        if (!InheritSmoothing)
        {
            reader.ReadInt32();
        }

        // Get stencil stuff.
        if (!InheritStencilCompare)
        {
            // We don't use depth/stencil info per sprite anymore.
            reader.ReadInt32();
        }

        if (!InheritStencilEnabled)
        {
            // We don't enable stencil in the same way anymore, so skip this value.
            reader.ReadBoolean();
        }

        if (!InheritStencilFailOperation)
        {
            // We don't use depth/stencil info per sprite anymore.
            reader.ReadInt32();
        }

        if (!InheritStencilMask)
        {
            // We don't use depth/stencil info per sprite anymore.
            reader.ReadInt32();
        }

        if (!InheritStencilPassOperation)
        {
            // We don't use depth/stencil info per sprite anymore.
            reader.ReadInt32();
        }

        if (!InheritStencilReference)
        {
            // We don't use depth/stencil info per sprite anymore.
            reader.ReadInt32();
        }

        if (!InheritStencilZFailOperation)
        {
            // We don't use depth/stencil info per sprite anymore.
            reader.ReadInt32();
        }

        // Get vertical wrapping mode.
        if (!InheritVerticalWrapping)
        {
            reader.ReadInt32();
        }

        // Get depth info.
        if ((version.Major == 1) && (version.Minor >= 1))
        {
            if (!InheritDepthBias)
            {
                // Depth bias values are quite different on D3D9 than they are on D3D11, so skip this.
                reader.ReadSingle();
            }

            if (!InheritDepthTestFunction)
            {
                // We don't use depth/stencil info per sprite anymore.
                reader.ReadInt32();
            }

            if (!InheritDepthWriteEnabled)
            {
                // We don't use depth/stencil info per sprite anymore.
                reader.ReadBoolean();
            }

            reader.ReadInt32();
        }

        // Get flipped flags.
        reader.ReadBoolean();
        reader.ReadBoolean();
    }

    /// <summary>
    /// Function to read 2D vector data.
    /// </summary>
    /// <param name="reader">The reader containing the track data.</param>
    /// <param name="keyCount">The number of keys to read.</param>
    /// <returns>A list of track times and vector2 values.</returns>
    private static IReadOnlyList<(float time, Vector2 value)> ReadVec2(BinaryReader reader, int keyCount)
    {
        (float, Vector2)[] keys = new (float, Vector2)[keyCount];

        for (int i = 0; i < keyCount; ++i)
        {
            // Skip this, we don't need it.
            reader.ReadString();

            keys[i] = (reader.ReadSingle() / 1000.0f, reader.ReadValue<Vector2>());
        }

        return keys;
    }

    /// <summary>
    /// Function to read the key frame data for floating point value information in the track.
    /// </summary>
    /// <param name="reader">The reader containing the track data.</param>
    /// <param name="keyCount">The number of keys to read.</param>
    /// <returns>A list of track times and float values.</returns>
    private static IReadOnlyList<(float time, float value)> ReadFloat(BinaryReader reader, int keyCount)
    {
        (float, float)[] keys = new (float, float)[keyCount];

        for (int i = 0; i < keyCount; ++i)
        {
            // Skip this, we don't need it.
            reader.ReadString();

            keys[i] = (reader.ReadSingle() / 1000.0f, reader.ReadSingle());
        }

        return keys;
    }

    /// <summary>
    /// Function to read the key frame data for int32 data in the track.
    /// </summary>
    /// <param name="reader">The reader containing the track data.</param>
    /// <param name="keyCount">The number of keys to read.</param>
    /// <returns>A list of track times and int32 values.</returns>
    private static IReadOnlyList<(float time, int value)> ReadInt32(BinaryReader reader, int keyCount)
    {
        (float, int)[] keys = new (float, int)[keyCount];

        for (int i = 0; i < keyCount; ++i)
        {
            // Skip this, we don't need it.
            reader.ReadString();

            keys[i] = (reader.ReadSingle() / 1000.0f, reader.ReadInt32());
        }

        return keys;
    }

    /// <summary>
    /// Function to read the key frame data for rectangle information in the track.
    /// </summary>
    /// <param name="reader">The reader containing the track data.</param>
    /// <param name="keyCount">The number of keys to read.</param>
    /// <returns>A list of track keys.</returns>
    private IReadOnlyList<(float time, GorgonTexture2DView texture, GorgonRectangleF uv, string textureName)> ReadTexture(BinaryReader reader, int keyCount)
    {
        List<(float, GorgonTexture2DView, GorgonRectangleF, string)> keys = new(keyCount);

        for (int i = 0; i < keyCount; ++i)
        {
            // Skip this, we don't need it.
            reader.ReadString();

            float time = reader.ReadSingle() / 1000.0f;
            string name = reader.ReadString();
            Vector2 uvOffset = reader.ReadValue<Vector2>();
            Vector2 uvSize = reader.ReadValue<Vector2>();

            GorgonTexture2D texture = Graphics.LocateResourcesByName<GorgonTexture2D>(name).FirstOrDefault();
            GorgonTexture2DView view = texture?.GetShaderResourceView();

            if ((view is null) && (string.IsNullOrWhiteSpace(name)))
            {
                Graphics.Log.Print("The animation has texture keys, but no applicable textures were found, and no name for the texture was given.", LoggingLevel.Verbose);
                continue;
            }

            GorgonRectangleF uv;

            if (view is not null)
            {
                uv = new GorgonRectangleF(uvOffset.X / texture.Width,
                                       uvOffset.Y / texture.Height,
                                       uvSize.X / texture.Width,
                                       uvSize.Y / texture.Height);

                keys.Add((time, view, uv, name));
            }
            else
            {
                Graphics.Log.Print($"The animation has texture keys, but the texture '{name}' was not found. A deferred texture key will be returned, but note that the texture coordinates will be incorrect until manually updated.",
                                   LoggingLevel.Verbose);
                uv = new GorgonRectangleF(uvOffset.X,
                                       uvOffset.Y,
                                       uvSize.X,
                                       uvSize.Y);

                keys.Add((time, null, uv, name));
            }
        }

        return keys;
    }

    /// <summary>
    /// Function to read version 1.1 - 1.2 animation data.
    /// </summary>
    /// <param name="reader">The reader for the stream that contains animation data.</param>
    /// <param name="builder">The animation builder.</param>
    /// <param name="count">The number of animations.</param>
    /// <returns>The animations in the sprite data.</returns>>
    private IReadOnlyList<IGorgonAnimation> ReadLatestVersion(BinaryReader reader, GorgonAnimationBuilder builder, int count)
    {
        IGorgonAnimation[] result = new IGorgonAnimation[count];

        for (int i = 0; i < count; ++i)
        {
            string header = reader.ReadString();

            if (!string.Equals(header, "GORANM11", StringComparison.OrdinalIgnoreCase))
            {
                if (header.Length > 8)
                {
                    header = header[..8];
                }

                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR2DIO_ERR_VERSION_MISMATCH, header));
            }

            string name = reader.ReadString();
            float length = reader.ReadSingle() / 1000.0f; // Old version stored length as milliseconds.
            bool looped = reader.ReadBoolean();

            // We have no need for these values.
            reader.ReadBoolean();
            reader.ReadInt32();

            int trackCount = reader.ReadInt32();

            for (int t = 0; t < trackCount; ++t)
            {
                string trackName = reader.ReadString();
                int keyCount = reader.ReadInt32();

                if (keyCount == 0)
                {
                    continue;
                }

                TrackInterpolationMode interpMode = TrackInterpolationMode.None;

                // We only use the following tracks, everything else can be skipped.
                switch (trackName.ToUpperInvariant())
                {
                    case "POSITION":
                        interpMode = (TrackInterpolationMode)reader.ReadInt32();
                        IReadOnlyList<(float time, Vector2 position)> positions = ReadVec2(reader, keyCount);
                        if (positions.Count > 0)
                        {
                            builder.EditVector2("Position")
                                   .SetInterpolationMode(interpMode)
                                   .SetKeys(positions.Select(item => new GorgonKeyVector2(item.time, item.position)))
                                   .EndEdit();
                        }
                        break;
                    case "SCALE":
                        interpMode = (TrackInterpolationMode)reader.ReadInt32();
                        IReadOnlyList<(float time, Vector2 scale)> scales = ReadVec2(reader, keyCount);
                        if (scales.Count > 0)
                        {
                            builder.EditVector2("Scale")
                                   .SetInterpolationMode(interpMode)
                                   .SetKeys(scales.Select(item => new GorgonKeyVector2(item.time, item.scale)))
                                   .EndEdit();
                        }
                        break;
                    case "SIZE":
                        interpMode = (TrackInterpolationMode)reader.ReadInt32();
                        IReadOnlyList<(float time, Vector2 sizes)> bounds = ReadVec2(reader, keyCount);
                        if (bounds.Count > 0)
                        {
                            builder.EditVector2("Size")
                                   .SetInterpolationMode(interpMode)
                                   .SetKeys(bounds.Select(item => new GorgonKeyVector2(item.time, item.sizes)))
                                   .EndEdit();
                        }
                        break;
                    case "ROTATION":
                        interpMode = (TrackInterpolationMode)reader.ReadInt32();
                        IReadOnlyList<(float time, float angle)> angles = ReadFloat(reader, keyCount);
                        if (angles.Count > 0)
                        {
                            builder.EditSingle("Angle")
                                   .SetInterpolationMode(interpMode)
                                   .SetKeys(angles.Select(item => new GorgonKeySingle(item.time, item.angle)))
                                   .EndEdit();
                        }
                        break;
                    case "COLOR":
                        interpMode = (TrackInterpolationMode)reader.ReadInt32();
                        IReadOnlyList<(float time, int argb)> colors = ReadInt32(reader, keyCount);
                        if (colors.Count > 0)
                        {
                            builder.EditColor("Color")
                                   .SetInterpolationMode(interpMode)
                                   .SetKeys(colors.Select(item => new GorgonKeyGorgonColor(item.time, GorgonColor.FromARGB(item.argb))))
                                   .EndEdit();
                        }
                        break;
                    case "OPACITY":
                        interpMode = (TrackInterpolationMode)reader.ReadInt32();
                        IReadOnlyList<(float time, int argb)> opacities = ReadInt32(reader, keyCount);
                        if (opacities.Count > 0)
                        {
                            builder.EditSingle("Opacity")
                                   .SetInterpolationMode(interpMode)
                                   .SetKeys(opacities.Select(item => new GorgonKeySingle(item.time, item.argb / 255.0f)))
                                   .EndEdit();
                        }
                        break;
                    case "IMAGE":
                        reader.ReadInt32(); // We don't support interpolation.
                        IReadOnlyList<(float time, GorgonTexture2DView texture, GorgonRectangleF uv, string name)> textures = ReadTexture(reader, keyCount);
                        if (textures.Count > 0)
                        {
                            builder.Edit2DTexture("Texture")
                                   .SetKeys(textures.Select(item => item.texture is null
                                                                        ? new GorgonKeyTexture2D(item.time, item.name, item.uv, 0)
                                                                        : new GorgonKeyTexture2D(item.time, item.texture, item.uv, 0)))
                                   .EndEdit();
                        }
                        break;
                    case "ALPHAMASKVALUE":
                        // We don't support this, so leave.
                        ReadInt32(reader, keyCount);
                        break;
                    case "SCALEDWIDTH":
                    case "SCALEDHEIGHT":
                        interpMode = (TrackInterpolationMode)reader.ReadInt32();
                        IReadOnlyList<(float time, Vector2 sizes)> scaledSize = ReadVec2(reader, keyCount);
                        if (scaledSize.Count > 0)
                        {
                            builder.EditVector2("Size")
                                   .SetInterpolationMode(interpMode)
                                   .SetKeys(scaledSize.Select(item => new GorgonKeyVector2(item.time, item.sizes)))
                                   .EndEdit();
                        }
                        break;
                    case "WIDTH":
                    case "HEIGHT":
                        ReadFloat(reader, keyCount);
                        break;
                    case "AXIS":
                    case "SCALEDDIMENSIONS":
                    case "IMAGEOFFSET":
                        // We don't support this, so leave.
                        ReadVec2(reader, keyCount);
                        break;
                    case "UNIFORMSCALE":
                        interpMode = (TrackInterpolationMode)reader.ReadInt32();
                        IReadOnlyList<(float time, float scale)> uniScales = ReadFloat(reader, keyCount);
                        if (uniScales.Count > 0)
                        {
                            builder.EditVector2("Scale")
                                   .SetKeys(uniScales.Select(item => new GorgonKeyVector2(item.time, new Vector2(item.scale))))
                                   .EndEdit();
                        }
                        break;
                    default:
                        continue;
                }

            }

            result[i] = builder.Build(name, length);
            result[i].IsLooped = looped;
        }

        return result;
    }

    /// <summary>
    /// Function to read the animation data from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the animation.</param>
    /// <param name="firstOnly"><b>true</b> if we should only load the first animation, <b>false</b> to load all.</param>
    /// <returns>A new <see cref="IGorgonAnimation"/>.</returns>
    private IReadOnlyList<IGorgonAnimation> OnReadMultipleFromStream(Stream stream, bool firstOnly)
    {
        GorgonAnimationBuilder builder = new();

        using BinaryReader reader = new(stream, Encoding.UTF8, true);
        string headerVersion = reader.ReadString();
        if ((!headerVersion.StartsWith("GORSPR", StringComparison.OrdinalIgnoreCase))
            || (headerVersion.Length < 7)
            || (headerVersion.Length > 9))
        {
            throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_INVALID_HEADER);
        }

        Version version = headerVersion.ToUpperInvariant() switch
        {
            "GORSPR1" => new Version(1, 0),
            "GORSPR1.1" => new Version(1, 1),
            "GORSPR1.2" => new Version(1, 2),
            _ => throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR2DIO_ERR_VERSION_MISMATCH, headerVersion)),
        };
        SkipToAnimationSection(reader, version);

        // If we have less than 1 int value left to the stream, it's likely that we don't have animations.
        // So get out.
        if ((stream.Length - stream.Position) < sizeof(int))
        {
            throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_NO_ANIMATIONS_IN_FILE);
        }

        int animationCount = reader.ReadInt32();

        if (animationCount == 0)
        {
            throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_NO_ANIMATIONS_IN_FILE);
        }

        IReadOnlyList<IGorgonAnimation> animations;

        if (firstOnly)
        {
            animationCount = 1;
        }

        if ((version.Major == 1) && (version.Minor >= 1))
        {
            animations = ReadLatestVersion(reader, builder, animationCount);
        }
        else
        {
            animations = ReadV10AnimationData(reader, builder, animationCount);
        }

        return (animations is null) || (animations.Count == 0)
            ? throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_NO_ANIMATIONS_IN_FILE)
            : animations;
    }

    /// <summary>
    /// Function to read version 1.0 animation data.
    /// </summary>
    /// <param name="reader">The reader for the stream that contains animation data.</param>
    /// <param name="builder">The animation builder.</param>
    /// <param name="count">The number of animations.</param>
    /// <returns>The animations in the sprite data.</returns>>
    private IReadOnlyList<IGorgonAnimation> ReadV10AnimationData(BinaryReader reader, GorgonAnimationBuilder builder, int count)
    {
        IGorgonAnimation[] result = new IGorgonAnimation[count];

        for (int i = 0; i < count; ++i)
        {
            string name = reader.ReadString();
            float length = reader.ReadSingle() / 1000.0f; // Old version stored length as milliseconds.
            bool looped = reader.ReadBoolean();

            // We have no need for this value.
            reader.ReadBoolean();

            int transformKeyCount = reader.ReadInt32();
            bool interpSet = false;
            TrackInterpolationMode interpolationMode = TrackInterpolationMode.None;

            for (int j = 0; j < transformKeyCount; ++j)
            {
                float time = reader.ReadSingle() / 1000.0f; // Stored as milliseconds.                    

                if (!interpSet)
                {
                    // The old animations tied scale, position, rotation and size keys together, and interpolation was per key. We'll just use 
                    // the first interpolation value going forward.
                    interpolationMode = (TrackInterpolationMode)reader.ReadInt32();
                    interpSet = true;
                }

                Vector2 position = reader.ReadValue<Vector2>();
                Vector2 scale = reader.ReadValue<Vector2>();
                float angle = reader.ReadSingle();
                // Skip the anchor, we're not animating that anymore.
                reader.ReadValue<Vector2>();
                Vector2 size = reader.ReadValue<Vector2>();
                // This is an "image offset"... but honestly, I have no idea what it's used for.
                reader.ReadValue<Vector2>();

                builder.EditVector2("Position")
                       .SetKey(new GorgonKeyVector2(time, position))
                       .SetInterpolationMode(interpolationMode)
                       .EndEdit()
                       .EditVector2("Scale")
                       .SetKey(new GorgonKeyVector2(time, scale))
                       .SetInterpolationMode(interpolationMode)
                       .EndEdit()
                       .EditSingle("Angle")
                       .SetKey(new GorgonKeySingle(time, angle))
                       .SetInterpolationMode(interpolationMode)
                       .EndEdit()
                       .EditVector2("Size")
                       .SetKey(new GorgonKeyVector2(time, size))
                       .SetInterpolationMode(interpolationMode)
                       .EndEdit();
            }

            interpSet = false;
            int colorKeyCount = reader.ReadInt32();
            interpolationMode = TrackInterpolationMode.None;

            for (int j = 0; j < colorKeyCount; ++j)
            {
                float time = reader.ReadSingle() / 1000.0f;

                if (!interpSet)
                {
                    // The old animations tied scale, position, rotation and size keys together, and interpolation was per key. We'll just use 
                    // the first interpolation value going forward.
                    interpolationMode = (TrackInterpolationMode)reader.ReadInt32();
                    interpSet = true;
                }

                GorgonColor color = GorgonColor.FromARGB(reader.ReadInt32());
                // We don't use alpha mask value.
                reader.ReadInt32();

                builder.EditColor("Color")
                       .SetKey(new GorgonKeyGorgonColor(time, color))
                       .SetInterpolationMode(interpolationMode)
                       .EndEdit();
            }

            int textureKeyCount = reader.ReadInt32();
            for (int j = 0; j < textureKeyCount; ++j)
            {
                float time = reader.ReadSingle() / 1000.0f;
                reader.ReadInt32(); // We don't use interpolation on texture tracks.

                string imageName = reader.ReadString();
                GorgonTexture2DView view = null;
                Vector2 imageOffset = Vector2.Zero;
                Vector2 imageSize = Vector2.One;
                GorgonRectangleF texCoords = new(imageOffset.X, imageOffset.Y, imageSize.X, imageSize.Y);

                if (!string.IsNullOrWhiteSpace(imageName))
                {
                    imageOffset = reader.ReadValue<Vector2>();
                    imageSize = reader.ReadValue<Vector2>();

                    // Attempt to locate the image.
                    GorgonTexture2D texture = Renderer.Graphics.LocateResourcesByName<GorgonTexture2D>(imageName).FirstOrDefault();
                    view = texture?.GetShaderResourceView();
                }

                // We cannot locate the texture, so do not add the key.
                if (view is null)
                {
                    Graphics.Log.Print($"The animation has texture keys and the texture '{imageName}' is required, but was not found in the loaded resources. The texture coordinates will most likely be incorrect for this frame.", LoggingLevel.Verbose);
                }
                else
                {
                    texCoords = new GorgonRectangleF(imageOffset.X / view.Width, imageOffset.Y / view.Height, imageSize.X / view.Width, imageSize.Y / view.Height);
                }

                builder.Edit2DTexture("Texture")
                       .SetKey((view is null) && (!string.IsNullOrWhiteSpace(imageName)) ? new GorgonKeyTexture2D(time, imageName, texCoords, 0) : new GorgonKeyTexture2D(time, view, texCoords, 0))
                       .EndEdit();
            }

            result[i] = builder.Build(name, length);
            result[i].IsLooped = looped;
        }

        return result;
    }

    /// <summary>Function to read the animation data from a stream.</summary>
    /// <param name="name">The name of the animation.</param>
    /// <param name="stream">The stream containing the animation.</param>
    /// <param name="byteCount">The number of bytes to read from the stream.</param>
    /// <param name="textureOverrides">[Optional] Textures to use in a texture animation track.</param>
    /// <returns>A new <see cref="IGorgonAnimation" />.</returns>
    /// <remarks>
    /// Implementors should handle the <paramref name="textureOverrides" /> parameter by matching the textures by name, and, if the texture is not found in the override list, fall back to whatever scheme
    /// is used to retrieve the texture for codec.
    /// </remarks>
    protected override IGorgonAnimation OnReadFromStream(string name, Stream stream, int byteCount, IEnumerable<GorgonTexture2DView> textureOverrides)
    {
        if ((textureOverrides is not null) && (textureOverrides.Any()))
        {
            Graphics.Log.PrintWarning("The texture overrides parameter is not supported for version 1.x files. Textures will not be overridden.", LoggingLevel.Intermediate);
        }

        return OnReadMultipleFromStream(stream, true)[0];
    }

    /// <summary>Function to retrieve the names of the associated textures.</summary>
    /// <param name="stream">The stream containing the texture data.</param>
    /// <returns>The names of the texture associated with the animations, or an empty list if no textures were found.</returns>
    /// <remarks>This implementation does not do anything.</remarks>
    protected override IReadOnlyList<string> OnGetAssociatedTextureNames(Stream stream) => [];

    /// <summary>
    /// Function to determine the number of animations in the sprite.
    /// </summary>
    /// <param name="stream">The stream containing the sprite data.</param>
    /// <returns>The number of animations in the sprite.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="stream"/> is write only.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the current <paramref name="stream"/> position, plus the size of the data exceeds the length of the stream.</exception>
    public int GetAnimationCount(Stream stream)
    {
        long currentPos = stream.Position;

        try
        {
            BinaryReader reader = new(stream, Encoding.UTF8, true);

            // If we don't have at least 10 bytes, then this file is not valid.
            if ((stream.Length - stream.Position) < 16)
            {
                return 0;
            }

            string headerVersion = reader.ReadString();
            if ((!headerVersion.StartsWith("GORSPR", StringComparison.OrdinalIgnoreCase))
                || (headerVersion.Length < 7)
                || (headerVersion.Length > 9))
            {
                return 0;
            }

            Version version;

            // Get the version information.
            switch (headerVersion.ToUpperInvariant())
            {
                case "GORSPR1":
                    version = new Version(1, 0);
                    break;
                case "GORSPR1.1":
                    version = new Version(1, 1);
                    break;
                case "GORSPR1.2":
                    version = new Version(1, 2);
                    break;
                default:
                    return 0;
            }

            try
            {
                SkipToAnimationSection(reader, version);
            }
            catch (EndOfStreamException)
            {
                return 0;
            }

            if (stream.Length - stream.Position < sizeof(int))
            {
                return 0;
            }

            int count = reader.ReadInt32();
            if (version.Minor == 0)
            {
                return count;
            }

            string header = reader.ReadString();

            return !string.Equals(header, "GORANM11", StringComparison.OrdinalIgnoreCase) ? 0 : count;
        }
        finally
        {
            stream.Position = currentPos;
        }
    }

    /// <summary>
    /// Function to determine if the data in a stream is readable by this codec.
    /// </summary>
    /// <param name="stream">The stream containing the data.</param>
    /// <returns><b>true</b> if the data can be read, or <b>false</b> if not.</returns>
    protected override bool OnIsReadable(Stream stream)
    {
        using BinaryReader reader = new(stream, Encoding.UTF8, true);
        // If we don't have at least 10 bytes, then this file is not valid.
        if ((stream.Length - stream.Position) < 16)
        {
            return false;
        }

        string headerVersion = reader.ReadString();
        if ((!headerVersion.StartsWith("GORSPR", StringComparison.OrdinalIgnoreCase))
            || (headerVersion.Length < 7)
            || (headerVersion.Length > 9))
        {
            return false;
        }

        Version version;

        // Get the version information.
        switch (headerVersion.ToUpperInvariant())
        {
            case "GORSPR1":
                version = new Version(1, 0);
                break;
            case "GORSPR1.1":
                version = new Version(1, 1);
                break;
            case "GORSPR1.2":
                version = new Version(1, 2);
                break;
            default:
                return false;
        }

        try
        {
            SkipToAnimationSection(reader, version);
        }
        catch (EndOfStreamException)
        {
            return false;
        }

        if (stream.Length - stream.Position < sizeof(int))
        {
            return false;
        }

        int animCount = reader.ReadInt32();
        if (version.Minor == 0)
        {
            return animCount > 0;
        }

        string header = reader.ReadString();

        return (animCount > 0) && (string.Equals(header, "GORANM11", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Function to read all animations from a file if it contains more than one.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
    /// <returns>A list of <see cref="IGorgonAnimation"/> objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
    public IReadOnlyList<IGorgonAnimation> AllFromFile(string filePath)
    {
        using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return AllFromStream(stream);
    }

    /// <summary>
    /// Function to read all animations from a stream if it contains more than one.
    /// </summary>
    /// <param name="stream">The stream containing the animation.</param>
    /// <param name="byteCount">[Optional] The number of bytes to read from the stream.</param>
    /// <returns>A list of <see cref="IGorgonAnimation"/> objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="stream"/> is write only.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the current <paramref name="stream"/> position, plus the size of the data exceeds the length of the stream.</exception>
    public IReadOnlyList<IGorgonAnimation> AllFromStream(Stream stream, int? byteCount = null)
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

        byteCount ??= (int)stream.Length;

        return (stream.Position + byteCount.Value) > stream.Length
            ? throw new EndOfStreamException()
            : OnReadMultipleFromStream(stream, false);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonV1AnimationCodec"/> class.
    /// </summary>
    /// <param name="renderer">The renderer used for resource handling.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> is <b>null</b>.</exception>
    public GorgonV1AnimationCodec(Gorgon2D renderer)
        : base(renderer, Resources.GOR2DIO_V1_ANIM_CODEC, Resources.GOR2DIO_V1_ANIM_CODEC_DESCRIPTION) => FileExtensions =
                         [
                             new GorgonFileExtension(".gorSprite", Resources.GOR2DIO_SPRITE_FILE_EXTENSION_DESC),
                         ];

}
