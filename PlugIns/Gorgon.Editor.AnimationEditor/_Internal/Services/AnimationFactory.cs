
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
// Created: June 24, 2020 2:54:40 PM
// 


using System.Buffers;
using System.Numerics;
using Gorgon.Animation;
using Gorgon.Graphics;
using Gorgon.Math;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A factory to build animations
/// </summary>
internal static class AnimationFactory
{

    // The builder used to create the animation.
    private static readonly GorgonAnimationBuilder _builder = new();



    /// <summary>
    /// Function to build animation tracks for single floating point value manipualtion.
    /// </summary>
    /// <param name="tracks">The tracks to update.</param>
    private static void BuildSingleTracks(IEnumerable<ITrack> tracks)
    {
        foreach (ITrack track in tracks)
        {
            IGorgonTrackKeyBuilder<GorgonKeySingle> keyBuilder = _builder.EditSingle(track.Name);

            // There is no interpolation for texture tracks (not yet at least).
            keyBuilder.SetInterpolationMode(track.InterpolationMode);

            foreach (IKeyFrame keyFrame in track.KeyFrames.Where(item => item is not null))
            {
                keyBuilder.SetKey(new GorgonKeySingle(keyFrame.Time, keyFrame.FloatValue.X));
            }

            keyBuilder.EndEdit();
        }
    }

    /// <summary>
    /// Function to build animation tracks for 2D vector value manipualtion.
    /// </summary>
    /// <param name="tracks">The tracks to update.</param>
    private static void BuildVector2Tracks(IEnumerable<ITrack> tracks)
    {
        foreach (ITrack track in tracks)
        {
            IGorgonTrackKeyBuilder<GorgonKeyVector2> keyBuilder = _builder.EditVector2(track.Name);

            // There is no interpolation for texture tracks (not yet at least).
            keyBuilder.SetInterpolationMode(track.InterpolationMode);

            foreach (IKeyFrame keyFrame in track.KeyFrames.Where(item => item is not null))
            {
                keyBuilder.SetKey(new GorgonKeyVector2(keyFrame.Time, new Vector2(keyFrame.FloatValue.X, keyFrame.FloatValue.Y)));
            }

            keyBuilder.EndEdit();
        }
    }

    /// <summary>
    /// Function to build animation tracks for 3D vector value manipualtion.
    /// </summary>
    /// <param name="tracks">The tracks to update.</param>
    private static void BuildVector3Tracks(IEnumerable<ITrack> tracks)
    {
        foreach (ITrack track in tracks)
        {
            IGorgonTrackKeyBuilder<GorgonKeyVector3> keyBuilder = _builder.EditVector3(track.Name);

            // There is no interpolation for texture tracks (not yet at least).
            keyBuilder.SetInterpolationMode(track.InterpolationMode);

            foreach (IKeyFrame keyFrame in track.KeyFrames.Where(item => item is not null))
            {
                keyBuilder.SetKey(new GorgonKeyVector3(keyFrame.Time, new Vector3(keyFrame.FloatValue.X, keyFrame.FloatValue.Y, keyFrame.FloatValue.Z)));
            }

            keyBuilder.EndEdit();
        }
    }

    /// <summary>
    /// Function to build animation tracks for 4D vector value manipualtion.
    /// </summary>
    /// <param name="tracks">The tracks to update.</param>
    private static void BuildVector4Tracks(IEnumerable<ITrack> tracks)
    {
        foreach (ITrack track in tracks)
        {
            IGorgonTrackKeyBuilder<GorgonKeyVector4> keyBuilder = _builder.EditVector4(track.Name);

            // There is no interpolation for texture tracks (not yet at least).
            keyBuilder.SetInterpolationMode(track.InterpolationMode);

            foreach (IKeyFrame keyFrame in track.KeyFrames.Where(item => item is not null))
            {
                keyBuilder.SetKey(new GorgonKeyVector4(keyFrame.Time, keyFrame.FloatValue));
            }

            keyBuilder.EndEdit();
        }
    }

    /// <summary>
    /// Function to build animation tracks for rectangle value manipualtion.
    /// </summary>
    /// <param name="tracks">The tracks to update.</param>
    private static void BuildRectangleTracks(IEnumerable<ITrack> tracks)
    {
        foreach (ITrack track in tracks)
        {
            IGorgonTrackKeyBuilder<GorgonKeyRectangle> keyBuilder = _builder.EditRectangle(track.Name);

            // There is no interpolation for texture tracks (not yet at least).
            keyBuilder.SetInterpolationMode(track.InterpolationMode);

            foreach (IKeyFrame keyFrame in track.KeyFrames.Where(item => item is not null))
            {
                keyBuilder.SetKey(new GorgonKeyRectangle(keyFrame.Time, new GorgonRectangleF
                {
                    Left = keyFrame.FloatValue.X,
                    Top = keyFrame.FloatValue.Y,
                    Right = keyFrame.FloatValue.Z,
                    Bottom = keyFrame.FloatValue.W,
                }));
            }

            keyBuilder.EndEdit();
        }
    }

    /// <summary>
    /// Function to build animation tracks for color value manipualtion.
    /// </summary>
    /// <param name="tracks">The tracks to update.</param>
    private static void BuildColorTracks(IEnumerable<ITrack> tracks)
    {
        foreach (ITrack track in tracks)
        {
            IGorgonTrackKeyBuilder<GorgonKeyGorgonColor> keyBuilder = _builder.EditColor(track.Name);

            // There is no interpolation for texture tracks (not yet at least).
            keyBuilder.SetInterpolationMode(track.InterpolationMode);

            foreach (IKeyFrame keyFrame in track.KeyFrames.Where(item => item is not null))
            {
                keyBuilder.SetKey(new GorgonKeyGorgonColor(keyFrame.Time, GorgonColor.FromVector4(keyFrame.FloatValue)));
            }

            keyBuilder.EndEdit();
        }
    }

    /// <summary>
    /// Function to build animation tracks for texture manipualtion.
    /// </summary>
    /// <param name="tracks">The tracks to update.</param>
    private static void BuildTextureTracks(IEnumerable<ITrack> tracks)
    {
        foreach (ITrack track in tracks)
        {
            IGorgonTrackKeyBuilder<GorgonKeyTexture2D> keyBuilder = _builder.Edit2DTexture(track.Name);

            // There is no interpolation for texture tracks (not yet at least).
            keyBuilder.SetInterpolationMode(TrackInterpolationMode.None);

            foreach (IKeyFrame keyFrame in track.KeyFrames.Where(item => item is not null))
            {
                keyBuilder.SetKey(new GorgonKeyTexture2D(keyFrame.Time, keyFrame.TextureValue.Texture, keyFrame.TextureValue.TextureCoordinates, keyFrame.TextureValue.ArrayIndex));
            }

            keyBuilder.EndEdit();
        }
    }

    /// <summary>
    /// Function to resize an animation and scale its keyframes to the new size.
    /// </summary>
    /// <param name="newKeyCount">The new number of keys.</param>
    /// <param name="fps">The number of frames per secon.</param>
    /// <param name="tracks">The track listing.</param>
    /// <param name="keyFrames">[Optional] The list of key frames to assign to the track.</param>
    public static void ResizeAnimation(int newKeyCount, float fps, IReadOnlyList<ITrack> tracks, Dictionary<ITrack, List<IKeyFrame>> keyFrames = null)
    {
        foreach (ITrack track in tracks)
        {
            IKeyFrame[] newKeys = ArrayPool<IKeyFrame>.Shared.Rent(newKeyCount);

            if (keyFrames is null)
            {
                for (int i = 0; i < track.KeyFrames.Count; ++i)
                {
                    IKeyFrame key = track.KeyFrames[i];

                    if (key is null)
                    {
                        continue;
                    }

                    int newIndex = (int)(key.Time * fps).Round();
                    newKeys[newIndex] = key;
                }
            }
            else
            {
                if (keyFrames.TryGetValue(track, out List<IKeyFrame> frames))
                {
                    for (int i = 0; i < frames.Count.Min(newKeyCount); ++i)
                    {
                        newKeys[i] = frames[i];
                    }
                }
            }

            SetKeyFramesArgs args = new()
            {
                KeyFrames = newKeys,
                MaxKeyFrameCount = newKeyCount
            };

            if ((track.SetKeyFramesCommand is not null) && (track.SetKeyFramesCommand.CanExecute(args)))
            {
                track.SetKeyFramesCommand.Execute(args);
            }

            ArrayPool<IKeyFrame>.Shared.Return(newKeys, true);
        }
    }

    /// <summary>
    /// Function to build the animation.
    /// </summary>
    /// <param name="name">The name for the animation.</param>
    /// <param name="isLooped"><b>true</b> to mark the animation as looping, <b>false</b> leave as a single run.</param>
    /// <param name="loopCount">The number of loops for a looped animation.</param>
    /// <param name="keyCount">The maximum number of keys in the animation.</param>
    /// <param name="fps">Frames per second for the animation.</param>
    /// <param name="tracks">The list of tracks used to build the animation.</param>
    /// <returns>The resulting animation.</returns>
    public static IGorgonAnimation CreateAnimation(string name, bool isLooped, int loopCount, int keyCount, float fps, IReadOnlyList<ITrack> tracks)
    {
        IGorgonAnimation result;

        _builder.Clear();

        if (tracks.Count != 0)
        {
            BuildSingleTracks(tracks.Where(item => (item is not null) && (item.KeyType == AnimationTrackKeyType.Single) && (item.KeyFrames.Any(item2 => item2 is not null))));
            BuildVector2Tracks(tracks.Where(item => (item is not null) && (item.KeyType == AnimationTrackKeyType.Vector2) && (item.KeyFrames.Any(item2 => item2 is not null))));
            BuildVector3Tracks(tracks.Where(item => (item is not null) && (item.KeyType == AnimationTrackKeyType.Vector3) && (item.KeyFrames.Any(item2 => item2 is not null))));
            BuildVector4Tracks(tracks.Where(item => (item is not null) && (item.KeyType == AnimationTrackKeyType.Vector4) && (item.KeyFrames.Any(item2 => item2 is not null))));
            BuildRectangleTracks(tracks.Where(item => (item is not null) && (item.KeyType == AnimationTrackKeyType.Rectangle) && (item.KeyFrames.Any(item2 => item2 is not null))));
            BuildColorTracks(tracks.Where(item => (item is not null) && (item.KeyType == AnimationTrackKeyType.Color) && (item.KeyFrames.Any(item2 => item2 is not null))));
            BuildTextureTracks(tracks.Where(item => (item is not null) && (item.KeyType == AnimationTrackKeyType.Texture2D) && (item.KeyFrames.Any(item2 => item2 is not null))));
        }

        result = _builder.Build(name, fps, keyCount / fps);
        result.IsLooped = isLooped;
        result.LoopCount = loopCount;

        return result;
    }

}
