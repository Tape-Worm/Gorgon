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
// Created: August 18, 2018 7:52:17 PM
// 
#endregion

using System.Numerics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Animation
{
    /// <summary>
    /// A processor used to process the tracks for an animation.
    /// </summary>
    internal static class TrackKeyProcessor
    {
        /// <summary>
        /// Function to update the single floating point property of the object that the animation is being applied to.
        /// </summary>
        /// <param name="track">The track to evaluate.</param>
        /// <param name="time">The current time for the animation.</param>
        /// <param name="result">The result value to apply to the float object property.</param>
        /// <returns><b>true</b> if there's a value to update, <b>false</b> if not.</returns>
	    public static bool TryUpdateSingle(IGorgonAnimationTrack<GorgonKeySingle> track, float time, out float result)
        {
            switch (track.KeyFrames.Count)
            {
                case 0:
                    result = 0;
                    return false;
                case 1:
                    result = track.KeyFrames[0].Value;
                    return true;                
            }

            GorgonKeySingle firstKey = track.KeyFrames[0];
            GorgonKeySingle lastKey = track.KeyFrames[track.KeyFrames.Count - 1];

            if ((time < 0) || (time.EqualsEpsilon(firstKey.Time)))
            {
                result = firstKey.Value;
                return true;
            }

            if ((time > lastKey.Time) || (time.EqualsEpsilon(lastKey.Time)))
            {
                result = lastKey.Value;
                return true;
            }

            (GorgonKeySingle prev, GorgonKeySingle next, int prevKeyIndex, float deltaTime) = TweenKey.GetNearestKeys(track, time, lastKey.Time);

            switch (track.InterpolationMode)
            {
                case TrackInterpolationMode.Linear:
                    result = prev.Value.Lerp(next.Value, deltaTime);
                    break;
                case TrackInterpolationMode.Spline:
                    Vector4 val = track.SplineController.GetInterpolatedValue(prevKeyIndex, deltaTime);
                    result = val.X;
                    break;
                default:
                    result = prev.Value;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Function to update the vector2 property of the object that the animation is being applied to.
        /// </summary>
        /// <param name="track">The track to evaluate.</param>
        /// <param name="time">The current time for the animation.</param>
        /// <param name="result">The result value to apply to the vector2 object property.</param>
        /// <returns><b>true</b> if there's a value to update, <b>false</b> if not.</returns>
	    public static bool TryUpdateVector2(IGorgonAnimationTrack<GorgonKeyVector2> track, float time, out Vector2 result)
        {
            switch (track.KeyFrames.Count)
            {
                case 0:
                    result = Vector2.Zero;
                    return false;
                case 1:
                    result = track.KeyFrames[0].Value;
                    return true;
            }

            GorgonKeyVector2 firstKey = track.KeyFrames[0];
            GorgonKeyVector2 lastKey = track.KeyFrames[track.KeyFrames.Count - 1];

            if ((time < 0) || (time.EqualsEpsilon(firstKey.Time)))
            {
                result = firstKey.Value;
                return true;
            }

            if ((time > lastKey.Time) || (time.EqualsEpsilon(lastKey.Time)))
            {
                result = lastKey.Value;
                return true;
            }

            (GorgonKeyVector2 prev, GorgonKeyVector2 next, int prevKeyIndex, float deltaTime) = TweenKey.GetNearestKeys(track, time, lastKey.Time);

            switch (track.InterpolationMode)
            {
                case TrackInterpolationMode.Linear:
                    result = Vector2.Lerp(prev.Value, next.Value, deltaTime);
                    break;
                case TrackInterpolationMode.Spline:
                    Vector4 val = track.SplineController.GetInterpolatedValue(prevKeyIndex, deltaTime);
                    result = new Vector2(val.X, val.Y);
                    break;
                default:
                    result = prev.Value;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Function to update the vector3 property of the object that the animation is being applied to.
        /// </summary>
        /// <param name="track">The track to evaluate.</param>
        /// <param name="time">The current time for the animation.</param>
        /// <param name="result">The result value to apply to the vector3 object property.</param>
        /// <returns><b>true</b> if there's a value to update, <b>false</b> if not.</returns>
	    public static bool TryUpdateVector3(IGorgonAnimationTrack<GorgonKeyVector3> track, float time, out Vector3 result)
        {
            switch (track.KeyFrames.Count)
            {
                case 0:
                    result = Vector3.Zero;
                    return false;
                case 1:
                    result = track.KeyFrames[0].Value;
                    return true;
            }

            GorgonKeyVector3 firstKey = track.KeyFrames[0];
            GorgonKeyVector3 lastKey = track.KeyFrames[track.KeyFrames.Count - 1];

            if ((time < 0) || (time.EqualsEpsilon(firstKey.Time)))
            {
                result = firstKey.Value;
                return true;
            }

            if ((time > lastKey.Time) || (time.EqualsEpsilon(lastKey.Time)))
            {
                result = lastKey.Value;
                return true;
            }

            (GorgonKeyVector3 prev, GorgonKeyVector3 next, int prevKeyIndex, float deltaTime) = TweenKey.GetNearestKeys(track, time, lastKey.Time);

            switch (track.InterpolationMode)
            {
                case TrackInterpolationMode.Linear:
                    result = Vector3.Lerp(prev.Value, next.Value, deltaTime);
                    break;
                case TrackInterpolationMode.Spline:
                    Vector4 val = track.SplineController.GetInterpolatedValue(prevKeyIndex, deltaTime);
                    result = new Vector3(val.X, val.Y, val.Z);
                    break;
                default:
                    result = prev.Value;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Function to update the vector4 property of the object that the animation is being applied to.
        /// </summary>
        /// <param name="track">The track to evaluate.</param>
        /// <param name="time">The current time for the animation.</param>
        /// <param name="result">The result value to apply to the vector3 object property.</param>
        /// <returns><b>true</b> if there's a value to update, <b>false</b> if not.</returns>
	    public static bool TryUpdateVector4(IGorgonAnimationTrack<GorgonKeyVector4> track, float time, out Vector4 result)
        {
            switch (track.KeyFrames.Count)
            {
                case 0:
                    result = Vector4.Zero;
                    return false;
                case 1:
                    result = track.KeyFrames[0].Value;
                    return true;
            }

            GorgonKeyVector4 firstKey = track.KeyFrames[0];
            GorgonKeyVector4 lastKey = track.KeyFrames[track.KeyFrames.Count - 1];

            if ((time < 0) || (time.EqualsEpsilon(firstKey.Time)))
            {
                result = firstKey.Value;
                return true;
            }

            if ((time > lastKey.Time) || (time.EqualsEpsilon(lastKey.Time)))
            {
                result = lastKey.Value;
                return true;
            }

            (GorgonKeyVector4 prev, GorgonKeyVector4 next, int prevKeyIndex, float deltaTime) = TweenKey.GetNearestKeys(track, time, lastKey.Time);

            switch (track.InterpolationMode)
            {
                case TrackInterpolationMode.Linear:
                    result = Vector4.Lerp(prev.Value, next.Value, deltaTime);
                    break;
                case TrackInterpolationMode.Spline:
                    result = track.SplineController.GetInterpolatedValue(prevKeyIndex, deltaTime);
                    break;
                default:
                    result = prev.Value;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Function to update the color of the object that the animation is being applied to.
        /// </summary>
        /// <param name="track">The track to evaluate.</param>
        /// <param name="time">The current time for the animation.</param>
        /// <param name="result">The result value to apply to the object color.</param>
        /// <returns><b>true</b> if there's a value to update, <b>false</b> if not.</returns>
        public static bool TryUpdateColor(IGorgonAnimationTrack<GorgonKeyGorgonColor> track, float time, out GorgonColor result)
        {
            switch (track.KeyFrames.Count)
            {
                case 0:
                    result = GorgonColor.BlackTransparent;
                    return false;
                case 1:
                    result = track.KeyFrames[0].Value;
                    return true;
            }

            GorgonKeyGorgonColor firstKey = track.KeyFrames[0];
            GorgonKeyGorgonColor lastKey = track.KeyFrames[track.KeyFrames.Count - 1];

            if ((time < 0) || (time.EqualsEpsilon(firstKey.Time)))
            {
                result = firstKey.Value;
                return true;
            }

            if ((time > lastKey.Time) || (time.EqualsEpsilon(lastKey.Time)))
            {
                result = lastKey.Value;
                return true;
            }

            (GorgonKeyGorgonColor prev, GorgonKeyGorgonColor next, int prevKeyIndex, float deltaTime) = TweenKey.GetNearestKeys(track, time, lastKey.Time);

            switch (track.InterpolationMode)
            {
                case TrackInterpolationMode.Linear:
                    GorgonColor.Lerp(in prev.Value, in next.Value, deltaTime, out result);
                    break;
                case TrackInterpolationMode.Spline:
                    result = track.SplineController.GetInterpolatedValue(prevKeyIndex, deltaTime);
                    break;
                default:
                    result = prev.Value;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Function to update the rectangle boundaries of the object that the animation is being applied to.
        /// </summary>
        /// <param name="track">The track to evaluate.</param>
        /// <param name="time">The current time for the animation.</param>
        /// <param name="result">The result value to apply to the object bounds.</param>
        /// <returns><b>true</b> if there's a value to update, <b>false</b> if not.</returns>
        public static bool TryUpdateRectBounds(IGorgonAnimationTrack<GorgonKeyRectangle> track, float time, out DX.RectangleF result)
        {
            switch (track.KeyFrames.Count)
            {
                case 0:
                    result = DX.RectangleF.Empty;
                    return false;
                case 1:
                    result = track.KeyFrames[0].Value;
                    return true;
            }

            GorgonKeyRectangle firstKey = track.KeyFrames[0];
            GorgonKeyRectangle lastKey = track.KeyFrames[track.KeyFrames.Count - 1];

            if ((time < 0) || (time.EqualsEpsilon(firstKey.Time)))
            {
                result = firstKey.Value;
                return true;
            }

            if ((time > lastKey.Time) || (time.EqualsEpsilon(lastKey.Time)))
            {
                result = lastKey.Value;
                return true;
            }

            (GorgonKeyRectangle prev, GorgonKeyRectangle next, int prevKeyIndex, float deltaTime) = TweenKey.GetNearestKeys(track, time, lastKey.Time);

            switch (track.InterpolationMode)
            {
                case TrackInterpolationMode.Linear:
                    result = new DX.RectangleF(prev.Value.X.Lerp(next.Value.X, deltaTime),
                                               prev.Value.Y.Lerp(next.Value.Y, deltaTime),
                                               prev.Value.Width.Lerp(next.Value.Width, deltaTime),
                                               prev.Value.Height.Lerp(next.Value.Height, deltaTime));
                    break;
                case TrackInterpolationMode.Spline:
                    Vector4 splineResult = track.SplineController.GetInterpolatedValue(prevKeyIndex, deltaTime);
                    result = new DX.RectangleF(splineResult.X, splineResult.Y, splineResult.Z, splineResult.W);
                    break;
                default:
                    result = prev.Value;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Function to update the 2D texture values for the object that the animation is being applied to.
        /// </summary>
        /// <param name="track">The track to evaluate.</param>
        /// <param name="time">The current time for the animation.</param>
        /// <param name="texture">The texture to switch to.</param>
        /// <param name="texCoordinates">The texture coordinates to use.</param>
        /// <param name="textureArrayIndex">The current texture array index to use.</param>
        /// <returns><b>true</b> if there's a value to update, <b>false</b> if not.</returns>
        public static bool TryUpdateTexture2D(IGorgonAnimationTrack<GorgonKeyTexture2D> track, float time, out GorgonTexture2DView texture, out DX.RectangleF texCoordinates, out int textureArrayIndex)
        {
            switch (track.KeyFrames.Count)
            {
                case 0:
                    texture = null;
                    texCoordinates = DX.RectangleF.Empty;
                    textureArrayIndex = 0;
                    return false;
                case 1:
                    GorgonKeyTexture2D texKey = track.KeyFrames[0];
                    texture = texKey.Value;
                    texCoordinates = texKey.TextureCoordinates;
                    textureArrayIndex = texKey.TextureArrayIndex;
                    return true;
            }

            GorgonKeyTexture2D firstKey = track.KeyFrames[0];
            GorgonKeyTexture2D lastKey = track.KeyFrames[track.KeyFrames.Count - 1];

            if ((time < 0) || (time.EqualsEpsilon(firstKey.Time)))
            {
                texture = firstKey.Value;
                texCoordinates = firstKey.TextureCoordinates;
                textureArrayIndex = firstKey.TextureArrayIndex;
                return true;
            }

            if ((time > lastKey.Time) || (time.EqualsEpsilon(lastKey.Time)))
            {
                texture = lastKey.Value;
                texCoordinates = lastKey.TextureCoordinates;
                textureArrayIndex = lastKey.TextureArrayIndex;
                return true;
            }

            (GorgonKeyTexture2D prev, GorgonKeyTexture2D next, _, _) = TweenKey.GetNearestKeys(track, time, lastKey.Time);

            GorgonKeyTexture2D correctKey = ((time.EqualsEpsilon(next.Time)) || (time > next.Time)) ? next : prev;

            texture = correctKey.Value;
            texCoordinates = correctKey.TextureCoordinates;
            textureArrayIndex = correctKey.TextureArrayIndex;

            return true;
        }
    }
}
