
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: July 4, 2020 12:40:55 AM
// 


using System.Numerics;
using Gorgon.Animation;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A processor for handling setting up key frames with relevant data
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="KeyProcessorService"/> class.</remarks>
/// <param name="textureCache">The texture cache.</param>
/// <param name="log">The log.</param>
internal class KeyProcessorService(ITextureCache textureCache, IGorgonLog log)
{

    // The texture cache used to retrieve textures for sprites.
    private readonly ITextureCache _textureCache = textureCache;
    // The log used for debug messages.
    private readonly IGorgonLog _log = log;
    // The list of files for a texture track.
    private readonly List<IContentFile> _textureFiles = [];
    // The arguments for the set keyframe command.
    private readonly SetKeyFramesArgs _setKeysArgs = new();
    // The synchronization lock for multiple threads.
    private readonly object _syncLock = new();



    /// <summary>
    /// Function to retrieve a list of files for each key frame in a texture track.
    /// </summary>
    /// <param name="tracks">The tracks in the animation.</param>
    /// <returns>The list of files.</returns>
    public IReadOnlyList<IContentFile> GetKeyframeTextureFiles(IReadOnlyList<ITrack> tracks)
    {

        ITrack track = tracks.FirstOrDefault(item => item.ID == GorgonSpriteAnimationController.TextureTrack.ID);

        lock (_syncLock)
        {
            _textureFiles.Clear();

            if (track is null)
            {
                return _textureFiles;
            }

            for (int i = 0; i < track.KeyFrames.Count; ++i)
            {
                IKeyFrame keyFrame = track.KeyFrames[i];

                if ((keyFrame is not null) && (!_textureFiles.Contains(keyFrame.TextureValue.TextureFile)))
                {
                    _textureFiles.Add(keyFrame.TextureValue.TextureFile);
                }
            }

            return _textureFiles;
        }
    }

    /// <summary>
    /// Function to copy a texture data to another texture key frame.
    /// </summary>
    /// <param name="textureValue">The texture data to copy.</param>
    /// <param name="destKeyFrame">The destination key frame that will recieve the data.</param>
    /// <returns>A new key frame if the <paramref name="destKeyFrame"/> is <b>null</b>, or <paramref name="destKeyFrame"/> with updated values.</returns>
    /// <remarks>
    /// <para>
    /// If the <paramref name="destKeyFrame"/> parameter is <b>null</b>, then a new <see cref="IKeyFrame"/> is created with the texture data populated. Otherwise, the texture data from the 
    /// <paramref name="srcKeyFrame"/> is copied into the existing <paramref name="destKeyFrame"/> and returned.
    /// </para>
    /// <para>
    /// If the <paramref name="destKeyFrame"/> parameter is supplied, 
    /// </para>
    /// </remarks>
    public async Task<IKeyFrame> CopyTextureToKeyFrameAsync(TextureValue textureValue, IKeyFrame destKeyFrame)
    {
        UnloadTextureKeyframe(destKeyFrame);

        if (textureValue.TextureFile is null)
        {
            _log.Print("WARNING: The source key frame does not have a texture file associated. The key frame will not be available in the animation.", LoggingLevel.Intermediate);
            return null;
        }

        textureValue = new TextureValue(await _textureCache.GetTextureAsync(textureValue.TextureFile),
                                                                            textureValue.TextureFile,
                                                                            textureValue.ArrayIndex,
                                                                            textureValue.TextureCoordinates);

        destKeyFrame.TextureValue = textureValue;

        return destKeyFrame;
    }

    /// <summary>
    /// Function to unload all textures from the key frames within a texture track.
    /// </summary>
    /// <param name="tracks">The tracks in the animation.</param>
    public void UnloadTextureKeyframes(IReadOnlyList<ITrack> tracks)
    {
        ITrack track = tracks.FirstOrDefault(item => item.ID == GorgonSpriteAnimationController.TextureTrack.ID);

        lock (_syncLock)
        {
            _textureFiles.Clear();

            if (track is null)
            {
                return;
            }

            for (int i = 0; i < track.KeyFrames.Count; ++i)
            {
                IKeyFrame keyFrame = track.KeyFrames[i];

                if (keyFrame?.TextureValue.Texture is not null)
                {
                    if ((_textureCache.ReturnTexture(keyFrame.TextureValue.Texture)) && (keyFrame.TextureValue.TextureFile is not null))
                    {
                        keyFrame.TextureValue.TextureFile.IsOpen = false;
                    }

                    keyFrame.TextureValue = new TextureValue(null, keyFrame.TextureValue.TextureFile, keyFrame.TextureValue.ArrayIndex, keyFrame.TextureValue.TextureCoordinates);
                }
            }
        }
    }

    /// <summary>
    /// Function to unload data from a keyframe.
    /// </summary>
    /// <param name="keyFrame">The key frame containing the data to update.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="keyFrame"/>, or the <paramref name="file"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="file"/> does not contain any sprite data.</exception>
    public void UnloadTextureKeyframe(IKeyFrame keyFrame)
    {
        if (keyFrame?.TextureValue.Texture is null)
        {
            return;
        }

        if ((keyFrame.TextureValue.Texture is not null)
            && (keyFrame.TextureValue.TextureFile is not null)
            && (_textureCache.ReturnTexture(keyFrame.TextureValue.Texture)))
        {
            keyFrame.TextureValue.TextureFile.IsOpen = false;
        }

        keyFrame.TextureValue = new TextureValue(null, keyFrame.TextureValue.TextureFile, keyFrame.TextureValue.ArrayIndex, keyFrame.TextureValue.TextureCoordinates);
    }

    /// <summary>
    /// Function to restore a texture keyframe texture from the texture cache after it's been unloaded.
    /// </summary>
    /// <param name="keyFrame">The key frame to restore.</param>
    /// <returns>A task for asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// If the keyframe is not a texture keyframe, then nothing will be done for this key frame.
    /// </para>
    /// </remarks>
    public async Task RestoreTextureAsync(IKeyFrame keyFrame)
    {
        if (keyFrame.TextureValue.TextureFile is null)
        {
            return;
        }

        // We've already got texture data.
        if (keyFrame.TextureValue.Texture is not null)
        {
            return;
        }

        GorgonTexture2DView texture = await _textureCache.GetTextureAsync(keyFrame.TextureValue.TextureFile);
        keyFrame.TextureValue = new TextureValue(texture, keyFrame.TextureValue.TextureFile, keyFrame.TextureValue.ArrayIndex, keyFrame.TextureValue.TextureCoordinates);
        keyFrame.TextureValue.TextureFile.IsOpen = true;
    }

    /// <summary>
    /// Function to assign key frames to the track.
    /// </summary>
    /// <param name="track">The track that will hold the keyframes.</param>
    /// <param name="keyFrames">The keyframes to assign.</param>
    /// <param name="maxKeyCount">The maximum number of keys for the track.</param>
    public void AssignKeyFrames(ITrack track, IReadOnlyList<IKeyFrame> keyFrames, int maxKeyCount)
    {
        lock (_syncLock)
        {
            _setKeysArgs.MaxKeyFrameCount = maxKeyCount;
            _setKeysArgs.KeyFrames = keyFrames;

            if ((track?.SetKeyFramesCommand is null) || (!track.SetKeyFramesCommand.CanExecute(_setKeysArgs)))
            {
                return;
            }

            track.SetKeyFramesCommand.Execute(_setKeysArgs);
        }
    }

    /// <summary>
    /// Function to retrieve the interpolated floating point values for a track at a specified time.
    /// </summary>
    /// <param name="track">The track to evaluate.</param>
    /// <param name="time">The time, in seconds, for the keyframe.</param>
    /// <param name="animation">The currently active animation being edited.</param>
    /// <param name="workingSprite">The working sprite to update.</param>
    /// <returns>The floating point values at the specified time.</returns>
    public Vector4? GetTrackFloatValues(ITrack track, float time, IGorgonAnimation animation, GorgonSprite workingSprite)
    {
        if ((animation is null) || (workingSprite is null))
        {
            return null;
        }

        switch (track.KeyType)
        {
            case AnimationTrackKeyType.Single:
                if (!animation.SingleTracks.TryGetValue(track.Name, out IGorgonAnimationTrack<GorgonKeySingle> singleTrack))
                {
                    return workingSprite.GetFloatValues(track.SpriteProperty);
                }

                GorgonKeySingle singleKey = singleTrack?.GetValueAtTime(time);

                if (singleKey is null)
                {
                    return workingSprite.GetFloatValues(track.SpriteProperty);
                }

                return new Vector4(singleKey.Value, 0, 0, 0);
            case AnimationTrackKeyType.Vector2:
                if (!animation.Vector2Tracks.TryGetValue(track.Name, out IGorgonAnimationTrack<GorgonKeyVector2> v2Track))
                {
                    return workingSprite.GetFloatValues(track.SpriteProperty);
                }

                GorgonKeyVector2 v2Key = v2Track.GetValueAtTime(time);

                if (v2Key is null)
                {
                    return workingSprite.GetFloatValues(track.SpriteProperty);
                }

                return new Vector4(v2Key.Value.X, v2Key.Value.Y, 0, 0);
            case AnimationTrackKeyType.Vector3:
                if (!animation.Vector3Tracks.TryGetValue(track.Name, out IGorgonAnimationTrack<GorgonKeyVector3> v3Track))
                {
                    return workingSprite.GetFloatValues(track.SpriteProperty);
                }

                GorgonKeyVector3 v3Key = v3Track.GetValueAtTime(time);

                if (v3Key is null)
                {
                    return workingSprite.GetFloatValues(track.SpriteProperty);
                }

                return new Vector4(v3Key.Value.X, v3Key.Value.Y, v3Key.Value.Z, 0);
            case AnimationTrackKeyType.Vector4:
                if (!animation.Vector4Tracks.TryGetValue(track.Name, out IGorgonAnimationTrack<GorgonKeyVector4> v4Track))
                {
                    return workingSprite.GetFloatValues(track.SpriteProperty);
                }

                GorgonKeyVector4 v4Key = v4Track.GetValueAtTime(time);

                if (v4Key is null)
                {
                    return workingSprite.GetFloatValues(track.SpriteProperty);
                }

                return v4Key.Value;
            case AnimationTrackKeyType.Rectangle:
                if (!animation.RectangleTracks.TryGetValue(track.Name, out IGorgonAnimationTrack<GorgonKeyRectangle> rectTrack))
                {
                    return workingSprite.GetFloatValues(track.SpriteProperty);
                }

                GorgonKeyRectangle rectKey = rectTrack.GetValueAtTime(time);

                if (rectKey is null)
                {
                    return workingSprite.GetFloatValues(track.SpriteProperty);
                }

                return new Vector4(rectKey.Value.Left, rectKey.Value.Top, rectKey.Value.Width, rectKey.Value.Height);
            case AnimationTrackKeyType.Color:
                if (!animation.ColorTracks.TryGetValue(track.Name, out IGorgonAnimationTrack<GorgonKeyGorgonColor> colorTrack))
                {
                    return workingSprite.GetFloatValues(track.SpriteProperty);
                }

                GorgonKeyGorgonColor colorKey = colorTrack.GetValueAtTime(time);

                if (colorKey is null)
                {
                    return workingSprite.GetFloatValues(track.SpriteProperty);
                }

                return colorKey.Value;
            default:
                return null;
        }
    }


}
