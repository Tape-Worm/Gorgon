
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
// Created: August 17, 2018 2:02:22 PM
// 


using Gorgon.Animation.Properties;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;

namespace Gorgon.Animation;

/// <summary>
/// A builder used to create <see cref="IGorgonAnimation"/> objects
/// </summary>
/// <remarks>
/// <para>
/// This builder is used to create and configure an animation with key frames for the various animation tracks. 
/// </para>
/// <para>
/// Animations are composed of multiple tracks which represent the properties on the objects that are to be animated. Within these tracks, there are multiple key frames which indicate the value of 
/// the property at a specified time (measured in seconds). When the animation plays, the values in these key frames are interpolated between each frame to give the appearance of a smooth transition 
/// between the values
/// </para>
/// <para>
/// When building an animation using this builder type the tracks are edited via one of the several Edit methods on the builder. Each edit method corresponds to the type of data for the track (e.g. 
/// Vector2, float, etc...) and takes the registration <see cref="GorgonTrackRegistration.TrackName"/> of the track to indicate which track (and object property) is being edited
/// </para>
/// </remarks>
/// <seealso cref="GorgonTrackRegistration"/>
public class GorgonAnimationBuilder
    : IGorgonFluentBuilder<GorgonAnimationBuilder, IGorgonAnimation>
{

    // A list of builders for single floating point value tracks.
    private readonly Dictionary<string, TrackKeyBuilder<GorgonKeySingle>> _singleTracks = new(StringComparer.OrdinalIgnoreCase);
    // A list of builders for vector 2 tracks.
    private readonly Dictionary<string, TrackKeyBuilder<GorgonKeyVector2>> _vector2Tracks = new(StringComparer.OrdinalIgnoreCase);
    // A list of builders for vector 3 tracks.
    private readonly Dictionary<string, TrackKeyBuilder<GorgonKeyVector3>> _vector3Tracks = new(StringComparer.OrdinalIgnoreCase);
    // A list of builders for vector 4 tracks.
    private readonly Dictionary<string, TrackKeyBuilder<GorgonKeyVector4>> _vector4Tracks = new(StringComparer.OrdinalIgnoreCase);
    // A list of builders for quaternion tracks.
    private readonly Dictionary<string, TrackKeyBuilder<GorgonKeyQuaternion>> _quatTracks = new(StringComparer.OrdinalIgnoreCase);
    // A list of builders for rectangle tracks.
    private readonly Dictionary<string, TrackKeyBuilder<GorgonKeyRectangle>> _rectangleTracks = new(StringComparer.OrdinalIgnoreCase);
    // A list of builders for rectangle tracks.
    private readonly Dictionary<string, TrackKeyBuilder<GorgonKeyGorgonColor>> _colorTracks = new(StringComparer.OrdinalIgnoreCase);
    // A list of builders for texture tracks.
    private readonly Dictionary<string, TrackKeyBuilder<GorgonKeyTexture2D>> _textureTracks = new(StringComparer.OrdinalIgnoreCase);



    /// <summary>
    /// Function to edit a track that uses single floating point values for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to edit.</param>
    /// <returns>The builder used to update the requested animation track.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// If no track exists with the specified <paramref name="name"/>, then a new track is created.
    /// </para>
    /// </remarks>
    public IGorgonTrackKeyBuilder<GorgonKeySingle> EditSingle(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        if (!_singleTracks.TryGetValue(name, out TrackKeyBuilder<GorgonKeySingle> result))
        {
            result = _singleTracks[name] = new TrackKeyBuilder<GorgonKeySingle>(this);
        }

        return result;
    }

    /// <summary>
    /// Function to edit a track that uses 2D vectors for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to edit.</param>
    /// <returns>The builder used to update the requested animation track.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// If no track exists with the specified <paramref name="name"/>, then a new track is created.
    /// </para>
    /// </remarks>
    public IGorgonTrackKeyBuilder<GorgonKeyVector2> EditVector2(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        if (!_vector2Tracks.TryGetValue(name, out TrackKeyBuilder<GorgonKeyVector2> result))
        {
            result = _vector2Tracks[name] = new TrackKeyBuilder<GorgonKeyVector2>(this);
        }

        return result;
    }

    /// <summary>
    /// Function to edit a track that uses 3D vectors for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to edit.</param>
    /// <returns>The builder used to update the requested animation track.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// If no track exists with the specified <paramref name="name"/>, then a new track is created.
    /// </para>
    /// </remarks>
    public IGorgonTrackKeyBuilder<GorgonKeyVector3> EditVector3(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        if (!_vector3Tracks.TryGetValue(name, out TrackKeyBuilder<GorgonKeyVector3> result))
        {
            result = _vector3Tracks[name] = new TrackKeyBuilder<GorgonKeyVector3>(this);
        }

        return result;
    }

    /// <summary>
    /// Function to edit a track that uses 4D vectors for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to edit.</param>
    /// <returns>The builder used to update the requested animation track.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// If no track exists with the specified <paramref name="name"/>, then a new track is created.
    /// </para>
    /// </remarks>
    public IGorgonTrackKeyBuilder<GorgonKeyVector4> EditVector4(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        if (!_vector4Tracks.TryGetValue(name, out TrackKeyBuilder<GorgonKeyVector4> result))
        {
            result = _vector4Tracks[name] = new TrackKeyBuilder<GorgonKeyVector4>(this);
        }

        return result;
    }

    /// <summary>
    /// Function to edit a track that uses quaternions for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to edit.</param>
    /// <returns>The builder used to update the requested animation track.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// If no track exists with the specified <paramref name="name"/>, then a new track is created.
    /// </para>
    /// </remarks>
    public IGorgonTrackKeyBuilder<GorgonKeyQuaternion> EditQuaternion(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        if (!_quatTracks.TryGetValue(name, out TrackKeyBuilder<GorgonKeyQuaternion> result))
        {
            result = _quatTracks[name] = new TrackKeyBuilder<GorgonKeyQuaternion>(this);
        }

        return result;
    }

    /// <summary>
    /// Function to edit a track that uses SharpDX <c>RectangleF</c> values for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to edit.</param>
    /// <returns>The builder used to update the requested animation track.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// If no track exists with the specified <paramref name="name"/>, then a new track is created.
    /// </para>
    /// </remarks>
    public IGorgonTrackKeyBuilder<GorgonKeyRectangle> EditRectangle(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        if (!_rectangleTracks.TryGetValue(name, out TrackKeyBuilder<GorgonKeyRectangle> result))
        {
            result = _rectangleTracks[name] = new TrackKeyBuilder<GorgonKeyRectangle>(this);
        }

        return result;
    }

    /// <summary>
    /// Function to edit a track that uses <see cref="GorgonColor"/> values for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to edit.</param>
    /// <returns>The builder used to update the requested animation track.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// If no track exists with the specified <paramref name="name"/>, then a new track is created.
    /// </para>
    /// </remarks>
    public IGorgonTrackKeyBuilder<GorgonKeyGorgonColor> EditColor(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        if (!_colorTracks.TryGetValue(name, out TrackKeyBuilder<GorgonKeyGorgonColor> result))
        {
            result = _colorTracks[name] = new TrackKeyBuilder<GorgonKeyGorgonColor>(this);
        }

        return result;
    }

    /// <summary>
    /// Function to edit a track that updates a <see cref="GorgonTexture2DView"/>, texture coordinates, and/or texture array indices for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to edit.</param>
    /// <returns>The builder used to update the requested animation track.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// If no track exists with the specified <paramref name="name"/>, then a new track is created.
    /// </para>
    /// </remarks>
    public IGorgonTrackKeyBuilder<GorgonKeyTexture2D> Edit2DTexture(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        if (!_textureTracks.TryGetValue(name, out TrackKeyBuilder<GorgonKeyTexture2D> result))
        {
            result = _textureTracks[name] = new TrackKeyBuilder<GorgonKeyTexture2D>(this);
        }

        return result;
    }


    /// <summary>
    /// Function to delete a track that updates a single floating point value for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to delete.</param>
    /// <returns>The fluent builder for the animation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the track with the name specified does not exist.</exception>
    public GorgonAnimationBuilder DeleteSingle(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        return !_singleTracks.Remove(name)
            ? throw new KeyNotFoundException(string.Format(Resources.GORANM_TRACK_DOES_NOT_EXIST, name))
            : this;
    }


    /// <summary>
    /// Function to delete a track that updates a 2D vector value for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to delete.</param>
    /// <returns>The fluent builder for the animation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the track with the name specified does not exist.</exception>
    public GorgonAnimationBuilder DeleteVector2(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        return !_vector2Tracks.Remove(name)
            ? throw new KeyNotFoundException(string.Format(Resources.GORANM_TRACK_DOES_NOT_EXIST, name))
            : this;
    }

    /// <summary>
    /// Function to delete a track that updates a 3D vector value for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to delete.</param>
    /// <returns>The fluent builder for the animation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the track with the name specified does not exist.</exception>
    public GorgonAnimationBuilder DeleteVector3(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        return !_vector3Tracks.Remove(name)
            ? throw new KeyNotFoundException(string.Format(Resources.GORANM_TRACK_DOES_NOT_EXIST, name))
            : this;
    }

    /// <summary>
    /// Function to delete a track that updates a 4D vector value for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to delete.</param>
    /// <returns>The fluent builder for the animation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the track with the name specified does not exist.</exception>
    public GorgonAnimationBuilder DeleteVector4(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        return !_vector4Tracks.Remove(name)
            ? throw new KeyNotFoundException(string.Format(Resources.GORANM_TRACK_DOES_NOT_EXIST, name))
            : this;
    }

    /// <summary>
    /// Function to delete a track that updates a Quaternion value for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to delete.</param>
    /// <returns>The fluent builder for the animation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the track with the name specified does not exist.</exception>
    public GorgonAnimationBuilder DeleteQuaternion(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        return !_quatTracks.Remove(name)
            ? throw new KeyNotFoundException(string.Format(Resources.GORANM_TRACK_DOES_NOT_EXIST, name))
            : this;
    }

    /// <summary>
    /// Function to delete a track that updates a SharpDX <c>RectangleF</c> value for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to delete.</param>
    /// <returns>The fluent builder for the animation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the track with the name specified does not exist.</exception>
    public GorgonAnimationBuilder DeleteRectangle(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        return !_rectangleTracks.Remove(name)
            ? throw new KeyNotFoundException(string.Format(Resources.GORANM_TRACK_DOES_NOT_EXIST, name))
            : this;
    }

    /// <summary>
    /// Function to delete a track that updates a <see cref="GorgonColor"/> value for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to delete.</param>
    /// <returns>The fluent builder for the animation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the track with the name specified does not exist.</exception>
    public GorgonAnimationBuilder DeleteColor(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        return !_colorTracks.Remove(name)
            ? throw new KeyNotFoundException(string.Format(Resources.GORANM_TRACK_DOES_NOT_EXIST, name))
            : this;
    }

    /// <summary>
    /// Function to delete a track that updates a <see cref="GorgonTexture2DView"/>, texture coordinates, and/or texture array indices for its key frame values.
    /// </summary>
    /// <param name="name">The registration name of the track to delete.</param>
    /// <returns>The fluent builder for the animation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the track with the name specified does not exist.</exception>
    public GorgonAnimationBuilder Delete2DTexture(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        return !_textureTracks.Remove(name)
            ? throw new KeyNotFoundException(string.Format(Resources.GORANM_TRACK_DOES_NOT_EXIST, name))
            : this;
    }

    /// <summary>
    /// Function to return the newly built animation.
    /// </summary>
    /// <param name="name">The name of the animation.</param>
    /// <param name="fps">[Optional] The frames per second for the animation.</param>
    /// <param name="length">[Optional] The length of the animation, in seconds.</param>
    /// <returns>The object created or updated by this builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// When the <paramref name="fps"/> parameter is omitted, it will default to 60 frames per second. This parameter is used to adjust the key frame times to fit within the specified frame delays 
    /// for the frames per second. If this value is less than 1, the animation will be assumed to be 1 frame per second.
    /// </para>
    /// <para>
    /// When the <paramref name="length"/> parameter is omitted, the length of the animation will be calculated based on the key frame times present in the tracks. This may not be ideal for certain 
    /// cases, so supplying this value will force the animation length to be as long as the user requires. If 0, or less is supplied to this parameter the length will be reset to exactly 1 frame of 
    /// animation (one frame = 1 / <paramref name="fps"/>).
    /// </para>
    /// </remarks>
    public IGorgonAnimation Build(string name, float fps = 60, float? length = null)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        fps = fps.Max(1);

        float minFrameTime = 1.0f / fps;

        // Function to adjust the key time to fit within the frames per second provided.
        float AdjustKeyTime(float keyTime)
        {
            float newTime = ((keyTime * fps).Round()) / fps;

            return keyTime.EqualsEpsilon(newTime) ? keyTime : newTime;
        }

        Dictionary<string, IGorgonAnimationTrack<GorgonKeySingle>> singles = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, IGorgonAnimationTrack<GorgonKeyVector2>> vec2 = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, IGorgonAnimationTrack<GorgonKeyVector3>> vec3 = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, IGorgonAnimationTrack<GorgonKeyVector4>> vec4 = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, IGorgonAnimationTrack<GorgonKeyQuaternion>> quat = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, IGorgonAnimationTrack<GorgonKeyRectangle>> rect = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, IGorgonAnimationTrack<GorgonKeyGorgonColor>> color = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, IGorgonAnimationTrack<GorgonKeyTexture2D>> texture = new(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, TrackKeyBuilder<GorgonKeySingle>> builder in _singleTracks)
        {
            foreach (GorgonKeySingle keyValue in builder.Value.Keys)
            {
                keyValue.Time = AdjustKeyTime(keyValue.Time);
            }

            singles[builder.Key] = new SingleTrack(builder.Value.GetSortedKeys(), builder.Key)
            {
                IsEnabled = builder.Value.IsEnabled,
                InterpolationMode = builder.Value.InterpolationMode
            };
        }

        foreach (KeyValuePair<string, TrackKeyBuilder<GorgonKeyVector2>> builder in _vector2Tracks)
        {
            foreach (GorgonKeyVector2 keyValue in builder.Value.Keys)
            {
                keyValue.Time = AdjustKeyTime(keyValue.Time);
            }

            vec2[builder.Key] = new Vector2Track(builder.Value.GetSortedKeys(), builder.Key)
            {
                IsEnabled = builder.Value.IsEnabled,
                InterpolationMode = builder.Value.InterpolationMode
            };
        }

        foreach (KeyValuePair<string, TrackKeyBuilder<GorgonKeyVector3>> builder in _vector3Tracks)
        {
            foreach (GorgonKeyVector3 keyValue in builder.Value.Keys)
            {
                keyValue.Time = AdjustKeyTime(keyValue.Time);
            }

            vec3[builder.Key] = new Vector3Track(builder.Value.GetSortedKeys(), builder.Key)
            {
                IsEnabled = builder.Value.IsEnabled,
                InterpolationMode = builder.Value.InterpolationMode
            };
        }

        foreach (KeyValuePair<string, TrackKeyBuilder<GorgonKeyVector4>> builder in _vector4Tracks)
        {
            foreach (GorgonKeyVector4 keyValue in builder.Value.Keys)
            {
                keyValue.Time = AdjustKeyTime(keyValue.Time);
            }

            vec4[builder.Key] = new Vector4Track(builder.Value.GetSortedKeys(), builder.Key)
            {
                IsEnabled = builder.Value.IsEnabled,
                InterpolationMode = builder.Value.InterpolationMode
            };
        }

        foreach (KeyValuePair<string, TrackKeyBuilder<GorgonKeyQuaternion>> builder in _quatTracks)
        {
            foreach (GorgonKeyQuaternion keyValue in builder.Value.Keys)
            {
                keyValue.Time = AdjustKeyTime(keyValue.Time);
            }

            quat[builder.Key] = new QuaternionTrack(builder.Value.GetSortedKeys(), builder.Key)
            {
                IsEnabled = builder.Value.IsEnabled,
                InterpolationMode = builder.Value.InterpolationMode
            };
        }

        foreach (KeyValuePair<string, TrackKeyBuilder<GorgonKeyRectangle>> builder in _rectangleTracks)
        {
            foreach (GorgonKeyRectangle keyValue in builder.Value.Keys)
            {
                keyValue.Time = AdjustKeyTime(keyValue.Time);
            }

            rect[builder.Key] = new RectBoundsTrack(builder.Value.GetSortedKeys(), builder.Key)
            {
                IsEnabled = builder.Value.IsEnabled,
                InterpolationMode = builder.Value.InterpolationMode
            };
        }

        foreach (KeyValuePair<string, TrackKeyBuilder<GorgonKeyGorgonColor>> builder in _colorTracks)
        {
            foreach (GorgonKeyGorgonColor keyValue in builder.Value.Keys)
            {
                keyValue.Time = AdjustKeyTime(keyValue.Time);
            }

            color[builder.Key] = new ColorTrack(builder.Value.GetSortedKeys(), builder.Key)
            {
                IsEnabled = builder.Value.IsEnabled,
                InterpolationMode = builder.Value.InterpolationMode
            };
        }

        foreach (KeyValuePair<string, TrackKeyBuilder<GorgonKeyTexture2D>> builder in _textureTracks)
        {
            foreach (GorgonKeyTexture2D keyValue in builder.Value.Keys)
            {
                keyValue.Time = AdjustKeyTime(keyValue.Time);
            }

            texture[builder.Key] = new Texture2DViewTrack(builder.Value.GetSortedKeys(), builder.Key)
            {
                IsEnabled = builder.Value.IsEnabled,
                // Textures don't use interpolation.
                InterpolationMode = TrackInterpolationMode.None
            };
        }

        length ??= (_singleTracks.SelectMany(item => item.Value.Keys.Cast<IGorgonKeyFrame>()).DefaultIfEmpty()
                                 .Concat(_vector2Tracks.SelectMany(item => item.Value.Keys).DefaultIfEmpty())
                                 .Concat(_vector3Tracks.SelectMany(item => item.Value.Keys).DefaultIfEmpty())
                                 .Concat(_vector4Tracks.SelectMany(item => item.Value.Keys).DefaultIfEmpty())
                                 .Concat(_quatTracks.SelectMany(item => item.Value.Keys).DefaultIfEmpty())
                                 .Concat(_rectangleTracks.SelectMany(item => item.Value.Keys).DefaultIfEmpty())
                                 .Concat(_colorTracks.SelectMany(item => item.Value.Keys).DefaultIfEmpty())
                                 .Concat(_textureTracks.SelectMany(item => item.Value.Keys).DefaultIfEmpty())
                                 .Max(item => item?.Time ?? 0)) + minFrameTime;

        return new AnimationData(name, fps, length.Value.Max(minFrameTime))
        {
            SingleTracks = singles,
            ColorTracks = color,
            RectangleTracks = rect,
            Texture2DTracks = texture,
            Vector2Tracks = vec2,
            Vector3Tracks = vec3,
            Vector4Tracks = vec4,
            QuaternionTracks = quat
        };
    }

    /// <summary>
    /// Function to reset the builder to the specified object state.
    /// </summary>
    /// <param name="builderObject">[Optional] The specified object state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// Passing <b>null</b> to the <paramref name="builderObject"/> parameter will clear the builder settings.
    /// </para>
    /// </remarks>
    public GorgonAnimationBuilder ResetTo(IGorgonAnimation builderObject = null)
    {
        Clear();

        if (builderObject is null)
        {
            return this;
        }

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeySingle>> track in builderObject.SingleTracks)
        {
            TrackKeyBuilder<GorgonKeySingle> trackBuilder = new(this);
            trackBuilder.SetInterpolationMode(track.Value.InterpolationMode);
            trackBuilder.Enabled();
            trackBuilder.Keys.AddRange(track.Value.KeyFrames.Select(item => new GorgonKeySingle(item)));
            _singleTracks[track.Key] = trackBuilder;
        }

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyVector2>> track in builderObject.Vector2Tracks)
        {
            TrackKeyBuilder<GorgonKeyVector2> trackBuilder = new(this);
            trackBuilder.SetInterpolationMode(track.Value.InterpolationMode);
            trackBuilder.Enabled();
            trackBuilder.Keys.AddRange(track.Value.KeyFrames.Select(item => new GorgonKeyVector2(item)));
            _vector2Tracks[track.Key] = trackBuilder;
        }

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyVector3>> track in builderObject.Vector3Tracks)
        {
            TrackKeyBuilder<GorgonKeyVector3> trackBuilder = new(this);
            trackBuilder.SetInterpolationMode(track.Value.InterpolationMode);
            trackBuilder.Enabled();
            trackBuilder.Keys.AddRange(track.Value.KeyFrames.Select(item => new GorgonKeyVector3(item)));
            _vector3Tracks[track.Key] = trackBuilder;
        }

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyVector4>> track in builderObject.Vector4Tracks)
        {
            TrackKeyBuilder<GorgonKeyVector4> trackBuilder = new(this);
            trackBuilder.SetInterpolationMode(track.Value.InterpolationMode);
            trackBuilder.Enabled();
            trackBuilder.Keys.AddRange(track.Value.KeyFrames.Select(item => new GorgonKeyVector4(item)));
            _vector4Tracks[track.Key] = trackBuilder;
        }

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyQuaternion>> track in builderObject.QuaternionTracks)
        {
            TrackKeyBuilder<GorgonKeyQuaternion> trackBuilder = new(this);
            trackBuilder.SetInterpolationMode(track.Value.InterpolationMode);
            trackBuilder.Enabled();
            trackBuilder.Keys.AddRange(track.Value.KeyFrames.Select(item => new GorgonKeyQuaternion(item)));
            _quatTracks[track.Key] = trackBuilder;
        }

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyRectangle>> track in builderObject.RectangleTracks)
        {
            TrackKeyBuilder<GorgonKeyRectangle> trackBuilder = new(this);
            trackBuilder.SetInterpolationMode(track.Value.InterpolationMode);
            trackBuilder.Enabled();
            trackBuilder.Keys.AddRange(track.Value.KeyFrames.Select(item => new GorgonKeyRectangle(item)));
            _rectangleTracks[track.Key] = trackBuilder;
        }

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyGorgonColor>> track in builderObject.ColorTracks)
        {
            TrackKeyBuilder<GorgonKeyGorgonColor> trackBuilder = new(this);
            trackBuilder.SetInterpolationMode(track.Value.InterpolationMode);
            trackBuilder.Enabled();
            trackBuilder.Keys.AddRange(track.Value.KeyFrames.Select(item => new GorgonKeyGorgonColor(item)));
            _colorTracks[track.Key] = trackBuilder;
        }

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyTexture2D>> track in builderObject.Texture2DTracks)
        {
            TrackKeyBuilder<GorgonKeyTexture2D> trackBuilder = new(this);
            trackBuilder.SetInterpolationMode(track.Value.InterpolationMode);
            trackBuilder.Enabled();
            trackBuilder.Keys.AddRange(track.Value.KeyFrames.Select(item => new GorgonKeyTexture2D(item)));
            _textureTracks[track.Key] = trackBuilder;
        }

        return this;
    }

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonAnimationBuilder Clear()
    {
        _singleTracks.Clear();
        _vector2Tracks.Clear();
        _vector3Tracks.Clear();
        _vector4Tracks.Clear();
        _quatTracks.Clear();
        _rectangleTracks.Clear();
        _colorTracks.Clear();
        _textureTracks.Clear();

        return this;
    }

    /// <summary>
    /// Function to return the object.
    /// </summary>
    /// <returns>The object created or updated by this builder.</returns>
    /// <remarks>
    /// <para>
    /// This overload of the build method will assign an arbitrary name, and use a default of 60 frames per second.
    /// </para>
    /// <para>
    /// It is recommended that you use the <see cref="Build(string, float, float?)"/> method instead.
    /// </para>
    /// </remarks>
    IGorgonAnimation IGorgonFluentBuilder<GorgonAnimationBuilder, IGorgonAnimation>.Build()
    {
        float maxTime = _singleTracks.SelectMany(item => item.Value.Keys.Cast<IGorgonKeyFrame>())
            .Concat(_vector2Tracks.SelectMany(item => item.Value.Keys))
            .Concat(_vector3Tracks.SelectMany(item => item.Value.Keys))
            .Concat(_vector4Tracks.SelectMany(item => item.Value.Keys))
            .Concat(_quatTracks.SelectMany(item => item.Value.Keys))
            .Concat(_rectangleTracks.SelectMany(item => item.Value.Keys))
            .Concat(_colorTracks.SelectMany(item => item.Value.Keys))
            .Concat(_textureTracks.SelectMany(item => item.Value.Keys))
            .Max(item => item.Time);

        return Build($"Animation_{Guid.NewGuid():N}", maxTime.Max(0));
    }

}
