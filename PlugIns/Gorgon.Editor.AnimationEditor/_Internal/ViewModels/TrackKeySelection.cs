#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: July 14, 2020 3:00:30 PM
// 
#endregion

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A type that contains the track and keyframes that are selected for work.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="TrackKeySelection"/> class.</remarks>
/// <param name="trackIndex">Index of the track.</param>
/// <param name="track">The selected track.</param>
/// <param name="selectedKeys">The keys that were selected within the track.</param>
internal class TrackKeySelection(int trackIndex, ITrack track, IReadOnlyList<TrackKeySelection.KeySelection> selectedKeys)
{
    /// <summary>
    /// A keyframe selection.
    /// </summary>
    internal class KeySelection
    {
        /// <summary>
        /// Property to return the track that contains the key.
        /// </summary>
        public ITrack Track
        {
            get;
        }

        /// <summary>
        /// Property to return the index of the key within its track.
        /// </summary>
        public int KeyIndex
        {
            get;
        }

        /// <summary>
        /// Property to return the time, in seconds, for the key (regardless of whether the key actually exists or not).
        /// </summary>
        public float TimeIndex
        {
            get;
        }

        /// <summary>
        /// Property to set or return the selected keyframe.
        /// </summary>
        /// <remarks>
        /// This value may be <b>null</b> if no keyframe was assigned at the <see cref="KeyIndex"/>.
        /// </remarks>
        public IKeyFrame KeyFrame
        {
            get;
            set;
        }

        /// <summary>Initializes a new instance of the <see cref="KeySelection"/> class.</summary>
        /// <param name="track">The track.</param>
        /// <param name="keyIndex">Index of the key.</param>
        /// <param name="keyFrame">The keyframe to store.</param>
        /// <param name="fps">The FPS.</param>
        public KeySelection(ITrack track, int keyIndex, IKeyFrame keyFrame, float fps)
        {
            Track = track;
            KeyIndex = keyIndex;
            KeyFrame = keyFrame;
            TimeIndex = KeyFrame?.Time ?? keyIndex / fps;
        }

        /// <summary>Initializes a new instance of the <see cref="KeySelection"/> class.</summary>
        /// <param name="track">The track.</param>
        /// <param name="keyIndex">Index of the key.</param>
        /// <param name="fps">The FPS.</param>
        public KeySelection(ITrack track, int keyIndex, float fps)
            : this(track, keyIndex, track.KeyFrames[keyIndex], fps)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="KeySelection"/> class.</summary>
        /// <param name="copy">The selection to copy.</param>
        public KeySelection(KeySelection copy)
        {
            Track = copy.Track;
            KeyIndex = copy.KeyIndex;
            KeyFrame = copy.KeyFrame;
            TimeIndex = copy.TimeIndex;
        }
    }

    /// <summary>
    /// Property to return the index of the track.
    /// </summary>
    public int TrackIndex
    {
        get;
    } = trackIndex;

    /// <summary>
    /// Property to return the track that has selected keyframes.
    /// </summary>
    public ITrack Track
    {
        get;
    } = track;

    /// <summary>
    /// Property to return the list of keys that were selected within the track.
    /// </summary>
    public IReadOnlyList<KeySelection> SelectedKeys
    {
        get;
    } = selectedKeys ?? [];
}
