using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Animation.Properties;
using Gorgon.Math;

namespace Gorgon.Animation._Internal
{
    /// <summary>
    /// The concrete version of a <see cref="IGorgonTrackKeyBuilder{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of key frame.</typeparam>
    internal class TrackKeyBuilder<T>
        : IGorgonTrackKeyBuilder<T>
        where T : class, IGorgonKeyFrame
    {
        #region Variables.
        // The comparer used to sort the key frames by time.
        private readonly IComparer<T> _comparer;
        // The animation builder for the animation that contains the track being edited.
        private readonly GorgonAnimationBuilder _parent;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of keys for the builder.
        /// </summary>
        public List<T> Keys
        {
            get;
        } = new List<T>();

        /// <summary>
        /// Property to return the current track interpolation mode.
        /// </summary>
        public TrackInterpolationMode Mode 
        { 
            get; 
            private set; 
        } = TrackInterpolationMode.Linear;
        #endregion

        #region Methods.
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
        /// <param name="maxAnimLength">The maximum animation length.</param>
        /// <returns>The sorted list as a read only collection.</returns>
        public IReadOnlyList<T> GetSortedKeys(float maxAnimLength)
        {
            T[] list = Keys
                       .Where(item => item.Time <= maxAnimLength)
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
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            int? index = IndexOfFrame(key);

            // Do not allow the same key to added again.
            if (index == null)
            {
                throw new ArgumentException(string.Format(Resources.GORANM_ERR_KEY_ALREADY_IN_TRACK, key.Time), nameof(key));
            }

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
            if (keys == null)
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
            Mode = mode;
            return this;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackKeyBuilder{T}"/> class.
        /// </summary>
        /// <param name="parent">The parent animation builder.</param>
        public TrackKeyBuilder(GorgonAnimationBuilder parent)
        {
            _parent = parent;
            _comparer = new KeyframeIndexComparer<T>();
        }
        #endregion
    }
}