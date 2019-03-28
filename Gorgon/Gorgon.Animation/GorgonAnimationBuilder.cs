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
// Created: August 17, 2018 2:02:22 PM
// 
#endregion

using System;
using System.Linq;
using Gorgon.Animation.Properties;
using Gorgon.Core;
using Gorgon.Math;

namespace Gorgon.Animation
{
    /// <summary>
    /// A builder used to create <see cref="IGorgonAnimation"/> objects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This builder is used to create and configure an animation with key frames for the various tracks. 
    /// </para>
    /// </remarks>
    public class GorgonAnimationBuilder
        :  IGorgonFluentBuilder<GorgonAnimationBuilder, IGorgonAnimation>
    {
        #region Variables.
        // The builder for the positions track.
        private readonly TrackKeyBuilder<GorgonKeyVector3> _trackPositions;
        // The builder for the scaling track.
        private readonly TrackKeyBuilder<GorgonKeyVector3> _trackScale;
        // The builder for the rotation track.
        private readonly TrackKeyBuilder<GorgonKeyVector3> _trackRotation;
        // The builder for the colors track.
        private readonly TrackKeyBuilder<GorgonKeyGorgonColor> _trackColors;
        // The builder for the rectangular boundaries track.
        private readonly TrackKeyBuilder<GorgonKeyRectangle> _trackRectBounds;
        // The builder for 2D texture values.
        private readonly TrackKeyBuilder<GorgonKeyTexture2D> _trackTexture2D;
        // The interpolation mode for the position track.
        private TrackInterpolationMode _positionMode = TrackInterpolationMode.Linear;
        // The interpolation mode for the scaling track.
        private TrackInterpolationMode _scaleMode = TrackInterpolationMode.Linear;
        // The interpolation mode for the rotation track.
        private TrackInterpolationMode _rotationMode = TrackInterpolationMode.Linear;
        // The interpolation mode for the color track.
        private TrackInterpolationMode _colorMode = TrackInterpolationMode.Linear;
        // The interpolation mode for the rectangle bounds track.
        private TrackInterpolationMode _rectBoundsMode = TrackInterpolationMode.Linear;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to edit the <see cref="IGorgonAnimation.PositionTrack">position track</see> of an animation.
        /// </summary>
        /// <returns>A <see cref="IGorgonTrackKeyBuilder{GorgonKeyVector3}"/> fluent interface for building track keys.</returns>
        public IGorgonTrackKeyBuilder<GorgonKeyVector3> EditPositions() => _trackPositions;

        /// <summary>
        /// Function to edit the <see cref="IGorgonAnimation.ScaleTrack">scaling track</see> of an animation.
        /// </summary>
        /// <returns>A <see cref="IGorgonTrackKeyBuilder{GorgonKeyVector3}"/> fluent interface for building track keys.</returns>
        public IGorgonTrackKeyBuilder<GorgonKeyVector3> EditScale() => _trackScale;

        /// <summary>
        /// Function to edit the <see cref="IGorgonAnimation.RotationTrack">rotation track</see> of an animation.
        /// </summary>
        /// <returns>A <see cref="IGorgonTrackKeyBuilder{GorgonKeyVector3}"/> fluent interface for building track keys.</returns>
        /// <remarks>
        /// <para>
        /// The values for the key frame use a 3 component vector representing the X, Y, and Z axis of rotation. Each axis is in degrees.
        /// </para>
        /// </remarks>
        public IGorgonTrackKeyBuilder<GorgonKeyVector3> EditRotation() => _trackRotation;

        /// <summary>
        /// Function to edit the <see cref="IGorgonAnimation.ColorTrack">color track</see> of an animation.
        /// </summary>
        /// <returns>A <see cref="IGorgonTrackKeyBuilder{GorgonKeyGorgonColor}"/> fluent interface for building track keys.</returns>
        public IGorgonTrackKeyBuilder<GorgonKeyGorgonColor> EditColors() => _trackColors;

        /// <summary>
        /// Function to edit the <see cref="IGorgonAnimation.RectBoundsTrack">rectangular boundaries track</see> of an animation.
        /// </summary>
        /// <returns>A <see cref="IGorgonTrackKeyBuilder{GorgonKeyRectangle}"/> fluent interface for building track keys.</returns>
        /// <remarks>
        /// <para>
        /// Some controllers will not use this track, while others may only use the width/height.
        /// </para>
        /// </remarks>
        public IGorgonTrackKeyBuilder<GorgonKeyRectangle> EditRectangularBounds() => _trackRectBounds;

        /// <summary>
        /// Function to edit the <see cref="IGorgonAnimation.Texture2DTrack">rectangular boundaries track</see> of an animation.
        /// </summary>
        /// <returns>A <see cref="IGorgonTrackKeyBuilder{GorgonKeyTexture2D}"/> fluent interface for building track keys.</returns>
        public IGorgonTrackKeyBuilder<GorgonKeyTexture2D> Edit2DTexture() => _trackTexture2D;

        /// <summary>
        /// Function to change the interpolation mode of the <see cref="IGorgonAnimation.PositionTrack"/>.
        /// </summary>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonAnimationBuilder PositionInterpolationMode(TrackInterpolationMode mode)
        {
            _positionMode = mode;
            return this;
        }

        /// <summary>
        /// Function to change the interpolation mode of the <see cref="IGorgonAnimation.RotationTrack"/>.
        /// </summary>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonAnimationBuilder RotationInterpolationMode(TrackInterpolationMode mode)
        {
            _rotationMode = mode;
            return this;
        }

        /// <summary>
        /// Function to change the interpolation mode of the <see cref="IGorgonAnimation.ScaleTrack"/>.
        /// </summary>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonAnimationBuilder ScaleInterpolationMode(TrackInterpolationMode mode)
        {
            _scaleMode = mode;
            return this;
        }

        /// <summary>
        /// Function to change the interpolation mode of the <see cref="IGorgonAnimation.ColorTrack"/>.
        /// </summary>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonAnimationBuilder ColorInterpolationMode(TrackInterpolationMode mode)
        {
            _colorMode = mode;
            return this;
        }

        /// <summary>
        /// Function to change the interpolation mode of the <see cref="IGorgonAnimation.RectBoundsTrack"/>.
        /// </summary>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonAnimationBuilder RectBoundsInterpolationMode(TrackInterpolationMode mode)
        {
            _rectBoundsMode = mode;
            return this;
        }

        /// <summary>
        /// Function to return the newly built animation.
        /// </summary>
        /// <param name="name">The name of the animation.</param>
        /// <param name="length">[Optional] The maximum length of the animation, in seconds.</param>
        /// <returns>The object created or updated by this builder.</returns>
        /// <remarks>
        /// <para>
        /// When the <paramref name="length"/> parameter is omitted, the length will be determined by evaluating all key frames in the animation and determining the highest time index. 
        /// </para>
        /// </remarks>
        public IGorgonAnimation Build(string name, float? length = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentEmptyException(nameof(name));
            }

            if (length == null)
            {
                length = _trackPositions.Keys.Cast<IGorgonKeyFrame>()
                                        .Concat(_trackColors.Keys)
                                        .Concat(_trackRectBounds.Keys)
                                        .Concat(_trackTexture2D.Keys)
                                        .Concat(_trackRotation.Keys)
                                        .Concat(_trackScale.Keys)
                                        .Max(item => item.Time);
            }

            return new AnimationData(name, length.Value)
                   {
                       PositionTrack = new Vector3Track(_trackPositions.GetSortedKeys(length.Value), Resources.GORANM_NAME_POSITION)
                                       {
                                           InterpolationMode = _positionMode
                                       },
                       ColorTrack = new ColorTrack(_trackColors.GetSortedKeys(length.Value))
                                    {
                                        InterpolationMode = _colorMode
                                    },
                       RectBoundsTrack = new RectBoundsTrack(_trackRectBounds.GetSortedKeys(length.Value))
                                         {
                                             InterpolationMode = _rectBoundsMode
                                         },
                       Texture2DTrack = new Texture2DViewTrack(_trackTexture2D.GetSortedKeys(length.Value)),
                       ScaleTrack = new Vector3Track(_trackScale.GetSortedKeys(length.Value), Resources.GORANM_NAME_SCALE)
                                    {
                                        InterpolationMode = _scaleMode
                                    },
                       RotationTrack = new Vector3Track(_trackRotation.GetSortedKeys(length.Value), Resources.GORANM_NAME_ROTATION)
                                       {
                                           InterpolationMode = _rotationMode
                                       }
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

            if (builderObject == null)
            {
                return this;
            }

            _positionMode = builderObject.PositionTrack.InterpolationMode;
            _colorMode = builderObject.ColorTrack.InterpolationMode;
            _rectBoundsMode = builderObject.RectBoundsTrack.InterpolationMode;
            _rotationMode = builderObject.RotationTrack.InterpolationMode;
            _scaleMode = builderObject.ScaleTrack.InterpolationMode;
            
            _trackPositions.Keys.AddRange(builderObject.PositionTrack.KeyFrames.Select(item => new GorgonKeyVector3(item.Time, item.Value)));
            _trackScale.Keys.AddRange(builderObject.ScaleTrack.KeyFrames.Select(item => new GorgonKeyVector3(item.Time, item.Value)));
            _trackRotation.Keys.AddRange(builderObject.RotationTrack.KeyFrames.Select(item => new GorgonKeyVector3(item.Time, item.Value)));
            _trackColors.Keys.AddRange(builderObject.ColorTrack.KeyFrames.Select(item => new GorgonKeyGorgonColor(item.Time, item.Value)));
            _trackRectBounds.Keys.AddRange(builderObject.RectBoundsTrack.KeyFrames.Select(item => new GorgonKeyRectangle(item.Time, item.Value)));
            _trackTexture2D.Keys.AddRange(builderObject.Texture2DTrack.KeyFrames.Select(item => new GorgonKeyTexture2D(item.Time,
                                                                                                                       item.Value,
                                                                                                                       item.TextureCoordinates,
                                                                                                                       item.TextureArrayIndex)));


            return this;
        }

        /// <summary>
        /// Function to clear the builder to a default state.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public GorgonAnimationBuilder Clear()
        {
            _positionMode = TrackInterpolationMode.Linear;
            _colorMode = TrackInterpolationMode.Linear;
            _rectBoundsMode = TrackInterpolationMode.Linear;
            _scaleMode = TrackInterpolationMode.Linear;
            _rotationMode = TrackInterpolationMode.Linear;

            _trackRectBounds.Clear();
            _trackPositions.Clear();
            _trackColors.Clear();
            _trackTexture2D.Clear();
            _trackScale.Clear();
            _trackRotation.Clear();

            return this;
        }

        /// <summary>
        /// Function to return the object.
        /// </summary>
        /// <returns>The object created or updated by this builder.</returns>
        /// <remarks>
        /// <para>
        /// This overload of the build method will assign an arbitrary name, and determine the length of time based on the highest time for a keyframe in the animation.
        /// </para>
        /// <para>
        /// It is recommended that you use the <see cref="Build(string, float?)"/> method instead.
        /// </para>
        /// </remarks>
        IGorgonAnimation IGorgonFluentBuilder<GorgonAnimationBuilder, IGorgonAnimation>.Build()
        {
            float maxTime = _trackPositions.Keys
                                           .Cast<IGorgonKeyFrame>()
                                           // TODO: .Concat other track types.
                                           .Max(item => item.Time);
            return Build($"Animation_{Guid.NewGuid():N}", maxTime.Max(0));
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonAnimationBuilder"/> class.
        /// </summary>
        public GorgonAnimationBuilder()
        {
            _trackPositions = new TrackKeyBuilder<GorgonKeyVector3>(this);
            _trackColors = new TrackKeyBuilder<GorgonKeyGorgonColor>(this);
            _trackRectBounds = new TrackKeyBuilder<GorgonKeyRectangle>(this);
            _trackTexture2D = new TrackKeyBuilder<GorgonKeyTexture2D>(this);
            _trackScale = new TrackKeyBuilder<GorgonKeyVector3>(this);
            _trackRotation =new TrackKeyBuilder<GorgonKeyVector3>(this);
        }
        #endregion
    }
}
