﻿using Gorgon.Animation.Properties;
using Gorgon.Math;

namespace Gorgon.Animation;

/// <summary>
/// The concrete version of a <see cref="IGorgonTrackKeyBuilder{T}"/>
/// </summary>
/// <typeparam name="T">The type of key frame.</typeparam>
/// <seealso cref="GorgonAnimationBuilder"/>
/// <remarks>
/// Initializes a new instance of the <see cref="TrackKeyBuilder{T}"/> class
/// </remarks>
/// <param name="parent">The parent animation builder.</param>
internal class TrackKeyBuilder<T>(GorgonAnimationBuilder parent)
    : IGorgonTrackKeyBuilder<T>
    where T : class, IGorgonKeyFrame
{

    // The comparer used to sort the key frames by time.
    private readonly IComparer<T> _comparer = new KeyframeIndexComparer<T>();
    // The animation builder for the animation that contains the track being edited.
    private readonly GorgonAnimationBuilder _parent = parent;

    /// <summary>
    /// Property to return the interpolation mode for the track.
    /// </summary>
    public TrackInterpolationMode InterpolationMode
    {
        get;
        private set;
    } = TrackInterpolationMode.Linear;

    /// <summary>
    /// Property to return whether this track is initially disabled or enabled.
    /// </summary>
    public bool IsEnabled
    {
        get;
        private set;
    } = true;

    /// <summary>
    /// Property to return the list of keys for the builder.
    /// </summary>
    public List<T> Keys
    {
        get;
    } = [];

    /// <summary>
    /// Function to locate a key frame by its time code.
    /// </summary>
    /// <param name="frame">The frame to locate.</param>
    /// <returns>The index of the frame, -1 if not found or <b>null</b> if the frame instance already exists in the collection.</returns>
    private int? IndexOfFrame(T frame)
    {
        for (int i = 0; i < Keys.Count; ++i)
        {
            T key = Keys[i];

            if (key == frame)
            {
                return null;
            }

            if (key.Time.EqualsEpsilon(frame.Time))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Function to return the final list of keys as a list, sorted by time index.
    /// </summary>
    /// <returns>The sorted list as a read only collection.</returns>
    internal IReadOnlyList<T> GetSortedKeys()
    {
        T[] list = Keys
                   .Select(item => (T)item.Clone())
                   .ToArray();

        Array.Sort(list, _comparer);

        return list;
    }

    /// <summary>
    /// Function to clear the key list for the track being edited.
    /// </summary>
    /// <returns>The fluent interface for this builder.</returns>
    public IGorgonTrackKeyBuilder<T> Clear()
    {
        Keys.Clear();
        return this;
    }

    /// <summary>
    /// Function to set a key to the track.  
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="key"/> is already in the key list.</exception>
    /// <remarks>
    /// <para>
    /// This will add, or replace, using the <see cref="IGorgonKeyFrame.Time"/> index, a key frame on the track.
    /// </para>
    /// </remarks>
    public IGorgonTrackKeyBuilder<T> SetKey(T key)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        int? index = IndexOfFrame(key) ?? throw new ArgumentException(string.Format(Resources.GORANM_ERR_KEY_ALREADY_IN_TRACK, key.Time), nameof(key));

        if (index != -1)
        {
            // Replace the key at the specified time index.
            Keys[index.Value] = key;
        }
        else
        {
            Keys.Add(key);
        }

        return this;
    }

    /// <summary>
    /// Function to assign a set of keys to the track.
    /// </summary>
    /// <param name="keys">The list of keys to assign to the track.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="keys"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentException">Thrown if a key within the <paramref name="keys"/> parameter is already in the key list.</exception>
    /// <returns>The fluent interface for this builder.</returns>
    /// <remarks>
    /// <para>
    /// This will add, or replace, using the <see cref="IGorgonKeyFrame.Time"/> index, a key frame on the track.
    /// </para>
    /// </remarks>
    public IGorgonTrackKeyBuilder<T> SetKeys(IEnumerable<T> keys)
    {
        if (keys is null)
        {
            throw new ArgumentNullException(nameof(keys));
        }

        foreach (T key in keys)
        {
            int? index = IndexOfFrame(key);

            switch (index)
            {
                case null:
                    throw new ArgumentException(string.Format(Resources.GORANM_ERR_KEY_ALREADY_IN_TRACK, key.Time), nameof(key));
                case -1:
                    Keys.Add(key);
                    break;
                default:
                    Keys[index.Value] = key;
                    break;
            }
        }

        return this;
    }

    /// <summary>
    /// Function to delete a key at the specified time index.
    /// </summary>
    /// <param name="time">The time index for the key.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public IGorgonTrackKeyBuilder<T> DeleteKeyAtTime(float time)
    {
        for (int i = 0; i < Keys.Count; ++i)
        {
            if (!Keys[i].Time.EqualsEpsilon(time))
            {
                continue;
            }

            Keys.RemoveAt(i);
            break;
        }

        return this;
    }

    /// <summary>
    /// Function to delete a key at the specified time index.
    /// </summary>
    /// <param name="key">The key to delete.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is <b>null</b>.</exception>
    public IGorgonTrackKeyBuilder<T> DeleteKey(T key)
    {
        Keys.Remove(key);
        return this;
    }

    /// <summary>
    /// Function to end editing of the track.
    /// </summary>
    /// <returns>The <see cref="GorgonAnimationBuilder"/> for the animation containing the track being edited.</returns>
    /// <seealso cref="GorgonAnimationBuilder"/>
    public GorgonAnimationBuilder EndEdit() => _parent;

    /// <summary>Function to set the interpolation mode for this track.</summary>
    /// <param name="mode">The interpolation mode to assign to the track.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <remarks>Not all track types provide interpolation modes, in those cases, this value will be ignored.</remarks>
    public IGorgonTrackKeyBuilder<T> SetInterpolationMode(TrackInterpolationMode mode)
    {
        InterpolationMode = mode;
        return this;
    }

    public IGorgonTrackKeyBuilder<T> Enabled()
    {
        Enabled(true);
        return this;
    }

    /// <summary>Function to mark the track as enabled.</summary>
    /// <param name="isEnabled">
    ///   <b>true</b> if the track should be enabled initially, or <b>false</b> if not.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <remarks>Enabled tracks will play during the animation, disabled ones will not.</remarks>
    public IGorgonTrackKeyBuilder<T> Enabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        return this;
    }

    /// <summary>Function to mark the track as disabled.</summary>
    /// <returns>The fluent interface for this builder.</returns>
    /// <remarks>Enabled tracks will play during the animation, disabled ones will not.</remarks>
    public IGorgonTrackKeyBuilder<T> Disabled()
    {
        Enabled(false);
        return this;
    }
}
